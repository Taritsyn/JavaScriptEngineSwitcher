using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if NET45 || NET471 || NETSTANDARD || NETCOREAPP2_1
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

using JavaScriptEngineSwitcher.Core;
#if NET40
using JavaScriptEngineSwitcher.Core.Extensions;
#endif
using JavaScriptEngineSwitcher.Core.Utilities;

//using JavaScriptEngineSwitcher.ChakraCore.JsRt;

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
		private readonly Queue<ScriptTask> _taskQueue = new Queue<ScriptTask>();

		/// <summary>
		/// Synchronizer of script task queue
		/// </summary>
		private readonly object _taskQueueSynchronizer = new object();

		/// <summary>
		/// Delegate that restore a JS engine after interruption
		/// </summary>
		private Action _restoreEngineCallback;

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
		/// or 0 to use the default maximum stack size specified in the header for the executable.</param>
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
					try
					{
						task.Run();
					}
					catch (JsInterruptedException e)
					{
						task.Exception = e;
						_restoreEngineCallback?.Invoke();
					}
					catch (Exception e)
					{
						task.Exception = e;
					}

					task.WaitHandle.Set();
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
		/// Runs a specified delegate on the thread with modified stack size,
		/// and returns its result as an <see cref="System.Object"/>.
		/// Blocks until the invocation of delegate is completed.
		/// </summary>
		/// <param name="del">Delegate to invocation</param>
		/// <returns>Result of the delegate invocation</returns>
		private T InnnerInvoke<T>(Func<T> del)
		{
			if (Thread.CurrentThread == _thread)
			{
				return del();
			}

			ScriptTask<T> task;

			using (var waitHandle = new ManualResetEvent(false))
			{
				task = new ScriptTask<T>(del, waitHandle);
				EnqueueTask(task);

				waitHandle.WaitOne();
			}

			Exception exception = task.Exception;
			if (exception != null)
			{
#if NET45 || NET471 || NETSTANDARD || NETCOREAPP2_1
				ExceptionDispatchInfo.Capture(exception).Throw();
#elif NET40
				exception.PreserveStackTrace();
				throw exception;
#else
#error No implementation for this target
#endif
			}

			return task.Result;
		}

		/// <summary>
		/// Sets a delegate that restore a JS engine after interruption
		/// </summary>
		/// <param name="restoreEngineCallback">Delegate that restore a JS engine after interruption</param>
		public void SetRestoreEngineCallback(Action restoreEngineCallback)
		{
			_restoreEngineCallback = restoreEngineCallback;
		}

		/// <summary>
		/// Runs a specified delegate on the thread with modified stack size,
		/// and returns its result as an object.
		/// Blocks until the invocation of delegate is completed.
		/// </summary>
		/// <param name="func">Delegate to invocation</param>
		/// <returns>Result of the delegate invocation</returns>
		public object Invoke(Func<object> func)
		{
			VerifyNotDisposed();

			if (func == null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			return InnnerInvoke(func);
		}

		/// <summary>
		/// Runs a specified delegate on the thread with modified stack size,
		/// and returns its result as an <typeparamref name="T" />.
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

			return InnnerInvoke(func);
		}

		///// <summary>
		///// Runs a specified delegate on the thread with modified stack size.
		///// Blocks until the invocation of delegate is completed.
		///// </summary>
		///// <param name="action">Delegate to invocation</param>
		//public void Invoke(Action action)
		//{
		//	VerifyNotDisposed();

		//	if (action == null)
		//	{
		//		throw new ArgumentNullException(nameof(action));
		//	}

		//	InnnerInvoke(() =>
		//	{
		//		action();
		//		return null;
		//	});
		//}

		#region IDisposable implementation

		/// <summary>
		/// Destroys object
		/// </summary>
		public void Dispose()
		{
			if (_disposedFlag.Set())
			{
				EnqueueTask(null);

				_restoreEngineCallback = null;

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
			}
		}

		#endregion

		#region Internal types

		/// <summary>
		/// Represents a script task, that must be executed on separate thread
		/// </summary>
		private abstract class ScriptTask
		{

			/// <summary>
			/// Gets a event to signal when the invocation of delegate has completed
			/// </summary>
			public ManualResetEvent WaitHandle
			{
				get;
				private set;
			}

			/// <summary>
			/// Gets or sets a exception, that occurred during the invocation of delegate.
			/// If no exception has occurred, this will be null.
			/// </summary>
			public Exception Exception
			{
				get;
				set;
			}


			/// <summary>
			/// Constructs an instance of script task
			/// </summary>
			/// <param name="waitHandle">Event to signal when the invocation of delegate has completed</param>
			protected ScriptTask(ManualResetEvent waitHandle)
			{
				WaitHandle = waitHandle;
			}


			public abstract void Run();
		}

		private sealed class ScriptTask<T> : ScriptTask
		{
			/// <summary>
			/// Delegate to invocation
			/// </summary>
			private Func<T> _delegate;

			/// <summary>
			/// Gets or sets a result of the delegate invocation
			/// </summary>
			public T Result
			{
				get;
				private set;
			}


			/// <summary>
			/// Constructs an instance of script task
			/// </summary>
			/// <param name="del">Delegate to invocation</param>
			/// <param name="waitHandle">Event to signal when the invocation of delegate has completed</param>
			public ScriptTask(Func<T> del, ManualResetEvent waitHandle)
				: base(waitHandle)
			{
				_delegate = del;
			}


			public override void Run()
			{
				Result = _delegate();
			}
		}

		#endregion
	}
}