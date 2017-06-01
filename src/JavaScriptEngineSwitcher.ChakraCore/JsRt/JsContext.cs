using System;

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The script context
	/// </summary>
	/// <remarks>
	/// <para>
	/// Each script context contains its own global object, distinct from the global object in
	/// other script contexts.
	/// </para>
	/// <para>
	/// Many Chakra hosting APIs require an "active" script context, which can be set using
	/// Current. Chakra hosting APIs that require a current context to be set will note
	/// that explicitly in their documentation.
	/// </para>
	/// </remarks>
	internal struct JsContext
	{
		/// <summary>
		/// The reference
		/// </summary>
		private readonly IntPtr _reference;

		/// <summary>
		/// Gets a invalid context
		/// </summary>
		public static JsContext Invalid
		{
			get { return new JsContext(IntPtr.Zero); }
		}

		/// <summary>
		/// Gets or sets a current script context on the thread
		/// </summary>
		public static JsContext Current
		{
			get
			{
				JsContext reference;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetCurrentContext(out reference));
				return reference;
			}
			set
			{
				JsErrorHelpers.ThrowIfError(NativeMethods.JsSetCurrentContext(value));
			}
		}

		/// <summary>
		/// Gets a value indicating whether the runtime of the current context is in an exception state
		/// </summary>
		/// <remarks>
		/// <para>
		/// If a call into the runtime results in an exception (either as the result of running a
		/// script or due to something like a conversion failure), the runtime is placed into an
		/// "exception state." All calls into any context created by the runtime (except for the
		/// exception APIs) will fail with <c>InExceptionState</c> until the exception is
		/// cleared.
		/// </para>
		/// <para>
		/// If the runtime of the current context is in the exception state when a callback returns
		/// into the engine, the engine will automatically rethrow the exception.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		public static bool HasException
		{
			get
			{
				bool hasException;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsHasException(out hasException));

				return hasException;
			}
		}

		/// <summary>
		/// Gets a runtime that the context belongs to
		/// </summary>
		public JsRuntime Runtime
		{
			get
			{
				JsRuntime handle;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetRuntime(this, out handle));

				return handle;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the context is a valid context or not
		/// </summary>
		public bool IsValid
		{
			get { return _reference != IntPtr.Zero; }
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsContext"/> struct
		/// </summary>
		/// <param name="reference">The reference</param>
		internal JsContext(IntPtr reference)
		{
			_reference = reference;
		}


		/// <summary>
		/// Tells the runtime to do any idle processing it need to do
		/// </summary>
		/// <remarks>
		/// <para>
		/// If idle processing has been enabled for the current runtime, calling <c>Idle</c> will
		/// inform the current runtime that the host is idle and that the runtime can perform
		/// memory cleanup tasks.
		/// </para>
		/// <para>
		/// <c>Idle</c> will also return the number of system ticks until there will be more idle work
		/// for the runtime to do. Calling <c>Idle</c> before this number of ticks has passed will do
		/// no work.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <returns>
		/// The next system tick when there will be more idle work to do. Returns the
		/// maximum number of ticks if there no upcoming idle work to do.
		/// </returns>
		public static uint Idle()
		{
			uint ticks;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsIdle(out ticks));

			return ticks;
		}

		/// <summary>
		/// Parses a script and returns a <c>Function</c> representing the script
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="script">The script to parse</param>
		/// <param name="sourceContext">The cookie identifying the script that can be used
		/// by script contexts that have debugging enabled</param>
		/// <param name="sourceName">The location the script came from</param>
		/// <returns>The <c>Function</c> representing the script code</returns>
		public static JsValue ParseScript(string script, JsSourceContext sourceContext, string sourceName)
		{
			JsValue result;
			JsErrorCode errorCode;

			if (Utils.IsWindows())
			{
				errorCode = NativeMethods.JsParseScript(script, sourceContext, sourceName, out result);
				JsErrorHelpers.ThrowIfError(errorCode);
			}
			else
			{
				JsValue scriptValue = JsValue.FromString(script);
				scriptValue.AddRef();

				JsValue sourceUrlValue = JsValue.FromString(sourceName);
				sourceUrlValue.AddRef();

				try
				{
					errorCode = NativeMethods.JsParse(scriptValue, sourceContext, sourceUrlValue,
						JsParseScriptAttributes.None, out result);
					JsErrorHelpers.ThrowIfError(errorCode);
				}
				finally
				{
					scriptValue.Release();
					sourceUrlValue.Release();
				}
			}

			return result;
		}

		/// <summary>
		/// Parses a script and returns a <c>Function</c> representing the script
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="script">The script to parse</param>
		/// <returns>The <c>Function</c> representing the script code</returns>
		public static JsValue ParseScript(string script)
		{
			return ParseScript(script, JsSourceContext.None, string.Empty);
		}

		/// <summary>
		/// Executes a script
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="script">The script to run</param>
		/// <param name="sourceContext">The cookie identifying the script that can be used
		/// by script contexts that have debugging enabled</param>
		/// <param name="sourceName">The location the script came from</param>
		/// <returns>The result of the script, if any</returns>
		public static JsValue RunScript(string script, JsSourceContext sourceContext, string sourceName)
		{
			JsValue result;
			JsErrorCode errorCode;

			if (Utils.IsWindows())
			{
				errorCode = NativeMethods.JsRunScript(script, sourceContext, sourceName, out result);
				JsErrorHelpers.ThrowIfError(errorCode);
			}
			else
			{
				JsValue scriptValue = JsValue.FromString(script);
				scriptValue.AddRef();

				JsValue sourceUrlValue = JsValue.FromString(sourceName);
				sourceUrlValue.AddRef();

				try
				{
					errorCode = NativeMethods.JsRun(scriptValue, sourceContext, sourceUrlValue,
						JsParseScriptAttributes.None, out result);
					JsErrorHelpers.ThrowIfError(errorCode);
				}
				finally
				{
					scriptValue.Release();
					sourceUrlValue.Release();
				}
			}

			return result;
		}

		/// <summary>
		/// Executes a script
		/// </summary>
		/// <remarks>
		/// Requires an active script context
		/// </remarks>
		/// <param name="script">The script to run</param>
		/// <returns>The result of the script, if any</returns>
		public static JsValue RunScript(string script)
		{
			return RunScript(script, JsSourceContext.None, string.Empty);
		}

		/// <summary>
		/// Serializes a parsed script to a buffer than can be reused
		/// </summary>
		/// <remarks>
		/// <para>
		/// SerializeScript parses a script and then stores the parsed form of the script in a
		/// runtime-independent format. The serialized script then can be deserialized in any
		/// runtime without requiring the script to be re-parsed.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <param name="script">The script to serialize</param>
		/// <param name="buffer">The buffer to put the serialized script into. Can be null</param>
		/// <returns>The size of the buffer, in bytes, required to hold the serialized script</returns>
		public static ulong SerializeScript(string script, byte[] buffer)
		{
			var bufferSize = (ulong)buffer.Length;
			JsErrorCode errorCode;

			if (Utils.IsWindows())
			{
				errorCode = NativeMethods.JsSerializeScript(script, buffer, ref bufferSize);
				JsErrorHelpers.ThrowIfError(errorCode);
			}
			else
			{
				JsValue scriptValue = JsValue.FromString(script);
				scriptValue.AddRef();

				JsValue bufferValue;

				try
				{
					errorCode = NativeMethods.JsSerialize(scriptValue, out bufferValue,
						JsParseScriptAttributes.None);
					JsErrorHelpers.ThrowIfError(errorCode);
				}
				finally
				{
					scriptValue.Release();
				}

				JsValue lengthValue = bufferValue.GetProperty("length");
				bufferSize = Convert.ToUInt64(lengthValue.ConvertToNumber().ToDouble());
			}

			return bufferSize;
		}

		/// <summary>
		/// Returns a exception that caused the runtime of the current context to be in the
		/// exception state and resets the exception state for that runtime
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the runtime of the current context is not in an exception state, this API will throw
		/// <see cref="JsErrorCode.InvalidArgument"/>. If the runtime is disabled, this will return
		/// an exception indicating that the script was terminated, but it will not clear the exception
		/// (the exception will be cleared if the runtime is re-enabled using
		/// <c>JsEnableRuntimeExecution</c>).
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <returns>The exception for the runtime of the current context</returns>
		public static JsValue GetAndClearException()
		{
			JsValue exception;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsGetAndClearException(out exception));

			return exception;
		}

		/// <summary>
		/// Returns a metadata relating to the exception that caused the runtime of the current context
		/// to be in the exception state and resets the exception state for that runtime. The metadata
		/// includes a reference to the exception itself.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the runtime of the current context is not in an exception state, this API will throw
		/// <see cref="JsErrorCode.InvalidArgument"/>. If the runtime is disabled, this will return
		/// an exception indicating that the script was terminated, but it will not clear the exception
		/// (the exception will be cleared if the runtime is re-enabled using
		/// <c>JsEnableRuntimeExecution</c>).
		/// </para>
		/// <para>
		/// The metadata value is a javascript object with the following properties: <c>exception</c>, the
		/// thrown exception object; <c>line</c>, the 0 indexed line number where the exception was thrown;
		/// <c>column</c>, the 0 indexed column number where the exception was thrown; <c>length</c>, the
		/// source-length of the cause of the exception; <c>source</c>, a string containing the line of
		/// source code where the exception was thrown; and <c>url</c>, a string containing the name of
		/// the script file containing the code that threw the exception.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <returns>The exception metadata for the runtime of the current context</returns>
		public static JsValue JsGetAndClearExceptionWithMetadata()
		{
			JsValue metadata;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsGetAndClearExceptionWithMetadata(out metadata));

			return metadata;
		}

		/// <summary>
		/// Sets a runtime of the current context to an exception state
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the runtime of the current context is already in an exception state, this API will
		/// throw <c>JsErrorInExceptionState</c>.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <param name="exception">The JavaScript exception to set for the runtime of the current context</param>
		public static void SetException(JsValue exception)
		{
			JsErrorHelpers.ThrowIfError(NativeMethods.JsSetException(exception));
		}

		/// <summary>
		/// Adds a reference to a script context
		/// </summary>
		/// <remarks>
		/// Calling AddRef ensures that the context will not be freed until Release is called.
		/// </remarks>
		/// <returns>The object's new reference count</returns>
		public uint AddRef()
		{
			uint count;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsContextAddRef(this, out count));

			return count;
		}

		/// <summary>
		/// Releases a reference to a script context
		/// </summary>
		/// <remarks>
		/// Removes a reference to a context that was created by AddRef.
		/// </remarks>
		/// <returns>The object's new reference count</returns>
		public uint Release()
		{
			uint count;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsContextRelease(this, out count));

			return count;
		}
	}
}