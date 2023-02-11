using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if NET45_OR_GREATER || NETSTANDARD
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

#if NET40
using JavaScriptEngineSwitcher.Core.Extensions;
#endif
using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// Provides services for managing the queue of script tasks on the thread with modified stack size
	/// </summary>
	internal sealed class ScriptDispatcher : IDisposable
	{
		/// <summary>
		/// The thread with modified stack size
		/// </summary>
		private Thread _thread;

		/// <summary>
		/// Event to signal when the new script task entered to the queue
		/// </summary>
		private AutoResetEvent _waitHandle = new AutoResetEvent(false);

		/// <summary>
		/// Queue of script tasks
		/// </summary>
		private Queue<ScriptTask> _taskQueue = new Queue<ScriptTask>();

		/// <summary>
		/// Synchronizer of script task queue
		/// </summary>
		private readonly object _taskQueueSynchronizer = new object();

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private InterlockedStatedFlag _disposedFlag = new InterlockedStatedFlag();


#if NETSTANDARD1_3
		/// <summary>
		/// Constructs an instance of script dispatcher
		/// </summary>
		public ScriptDispatcher()
		{
			_thread = new Thread(StartThread)
#else
		/// <summary>
		/// Constructs an instance of script dispatcher
		/// </summary>
		/// <param name="maxStackSize">The maximum stack size, in bytes, to be used by the thread,
		/// or <c>0</c> to use the default maximum stack size specified in the header for the executable.</param>
		public ScriptDispatcher(int maxStackSize)
		{
			_thread = new Thread(StartThread, maxStackSize)
#endif
			{
				IsBackground = true
			};
			_thread.Start();
		}


		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private void VerifyNotDisposed()
		{
			if (_disposedFlag.IsSet())
			{
				throw new ObjectDisposedException(ToString());
			}
		}

		/// <summary>
		/// Starts a thread with modified stack size.
		/// Loops forever, processing script tasks from the queue.
		/// </summary>
		private void StartThread()
		{
			while (true)
			{
				ScriptTask task = null;

				lock (_taskQueueSynchronizer)
				{
					if (_taskQueue.Count > 0)
					{
						task = _taskQueue.Dequeue();
						if (task == null)
						{
							_taskQueue.Clear();
							return;
						}
					}
				}

				if (task != null)
				{
					task.Run();
				}
				else
				{
					_waitHandle.WaitOne();
				}
			}
		}

		/// <summary>
		/// Adds a script task to the end of the queue
		/// </summary>
		/// <param name="task">Script task</param>
		private void EnqueueTask(ScriptTask task)
		{
			lock (_taskQueueSynchronizer)
			{
				_taskQueue.Enqueue(task);
			}
			_waitHandle.Set();
		}

		/// <summary>
		/// Executes a script task
		/// </summary>
		/// <param name="task">Script task</param>
		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private void ExecuteTask(ScriptTask task)
		{
			EnqueueTask(task);
			task.Wait();

			Exception exception = task.Exception;
			if (exception != null)
			{
#if NET45_OR_GREATER || NETSTANDARD
				ExceptionDispatchInfo.Capture(exception).Throw();
#elif NET40
				exception.PreserveStackTrace();
				throw exception;
#else
#error No implementation for this target
#endif
			}
		}

		/// <summary>
		/// Runs a specified delegate on the thread with modified stack size,
		/// and returns its result as an <typeparamref name="T"/>.
		/// Blocks until the invocation of delegate is completed.
		/// </summary>
		/// <typeparam name="T">The type of the return value of the method,
		/// that specified delegate encapsulates</typeparam>
		/// <param name="func">Delegate to invocation</param>
		/// <returns>Result of the delegate invocation</returns>
		public T Invoke<T>(Func<T> func)
		{
			VerifyNotDisposed();

			if (func == null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			if (Thread.CurrentThread == _thread)
			{
				return func();
			}

			using (var task = new ScriptTaskWithResult<T>(func))
			{
				ExecuteTask(task);
				return task.Result;
			}
		}

		/// <summary>
		/// Runs a specified delegate on the thread with modified stack size.
		/// Blocks until the invocation of delegate is completed.
		/// </summary>
		/// <param name="action">Delegate to invocation</param>
		public void Invoke(Action action)
		{
			VerifyNotDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (Thread.CurrentThread == _thread)
			{
				action();
				return;
			}

			using (var task = new ScriptTaskWithoutResult(action))
			{
				ExecuteTask(task);
			}
		}

		#region IDisposable implementation

		/// <summary>
		/// Destroys a script dispatcher
		/// </summary>
		public void Dispose()
		{
			if (_disposedFlag.Set())
			{
				EnqueueTask(null);

				if (_thread != null)
				{
					_thread.Join();
					_thread = null;
				}

				if (_waitHandle != null)
				{
					_waitHandle.Dispose();
					_waitHandle = null;
				}

				_taskQueue = null;
			}
		}

		#endregion

		#region Internal types

		/// <summary>
		/// Represents a script task, that must be executed on separate thread
		/// </summary>
		private abstract class ScriptTask : IDisposable
		{
			/// <summary>
			/// Event to signal when the invocation of delegate has completed
			/// </summary>
			protected ManualResetEvent _waitHandle = new ManualResetEvent(false);

			/// <summary>
			/// Exception, that occurred during the invocation of delegate
			/// </summary>
			protected Exception _exception;

			/// <summary>
			/// Flag that object is destroyed
			/// </summary>
			protected StatedFlag _disposedFlag = new StatedFlag();

			/// <summary>
			/// Gets a exception, that occurred during the invocation of delegate.
			/// If no exception has occurred, this will be <c>null</c>.
			/// </summary>
			public Exception Exception
			{
				get { return _exception; }
			}


			[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
			protected void VerifyNotDisposed()
			{
				if (_disposedFlag.IsSet())
				{
					throw new ObjectDisposedException(ToString());
				}
			}

			/// <summary>
			/// Runs a script task
			/// </summary>
			public abstract void Run();

			/// <summary>
			/// Waits for the script task to complete execution
			/// </summary>
			public void Wait()
			{
				VerifyNotDisposed();

				_waitHandle.WaitOne();
			}

			#region IDisposable implementation

			/// <summary>
			/// Destroys a script task
			/// </summary>
			public virtual void Dispose()
			{
				if (_waitHandle != null)
				{
					_waitHandle.Dispose();
					_waitHandle = null;
				}

				_exception = null;
			}

			#endregion
		}

		/// <summary>
		/// Represents a script task with result, that must be executed on separate thread
		/// </summary>
		private sealed class ScriptTaskWithResult<TResult> : ScriptTask
		{
			/// <summary>
			/// Delegate to invocation
			/// </summary>
			private Func<TResult> _func;

			/// <summary>
			/// Result of the delegate invocation
			/// </summary>
			private TResult _result;

			/// <summary>
			/// Gets a result of the delegate invocation
			/// </summary>
			public TResult Result
			{
				get { return _result; }
			}


			/// <summary>
			/// Constructs an instance of script task with result
			/// </summary>
			/// <param name="func">Delegate to invocation</param>
			public ScriptTaskWithResult(Func<TResult> func)
			{
				_func = func;
			}


			/// <summary>
			/// Runs a script task
			/// </summary>
			public override void Run()
			{
				VerifyNotDisposed();

				try
				{
					_result = _func();
				}
				catch (Exception e)
				{
					_exception = e;
				}

				_waitHandle.Set();
			}

			/// <summary>
			/// Destroys a script task
			/// </summary>
			public override void Dispose()
			{
				if (_disposedFlag.Set())
				{
					base.Dispose();

					_result = default(TResult);
					_func = null;
				}
			}
		}

		/// <summary>
		/// Represents a script task without result, that must be executed on separate thread
		/// </summary>
		private sealed class ScriptTaskWithoutResult : ScriptTask
		{
			/// <summary>
			/// Delegate to invocation
			/// </summary>
			private Action _action;


			/// <summary>
			/// Constructs an instance of script task without result
			/// </summary>
			/// <param name="action">Delegate to invocation</param>
			public ScriptTaskWithoutResult(Action action)
			{
				_action = action;
			}


			/// <summary>
			/// Runs a script task
			/// </summary>
			public override void Run()
			{
				VerifyNotDisposed();

				try
				{
					_action();
				}
				catch (Exception e)
				{
					_exception = e;
				}

				_waitHandle.Set();
			}

			/// <summary>
			/// Destroys a script task
			/// </summary>
			public override void Dispose()
			{
				if (_disposedFlag.Set())
				{
					base.Dispose();

					_action = null;
				}
			}
		}

		#endregion
	}
}