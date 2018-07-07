namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Error helpers
	/// </summary>
	internal static class JsErrorHelpers
	{
		/// <summary>
		/// Throws if a native method returns an error code
		/// </summary>
		/// <param name="error">The error</param>
		public static void ThrowIfError(JsErrorCode error)
		{
			if (error != JsErrorCode.NoError)
			{
				switch (error)
				{
					#region Usage

					case JsErrorCode.InvalidArgument:
						throw new JsUsageException(error, "Invalid argument.");

					case JsErrorCode.NullArgument:
						throw new JsUsageException(error, "Null argument.");

					case JsErrorCode.NoCurrentContext:
						throw new JsUsageException(error, "No current context.");

					case JsErrorCode.InExceptionState:
						throw new JsUsageException(error, "Runtime is in exception state.");

					case JsErrorCode.NotImplemented:
						throw new JsUsageException(error, "Method is not implemented.");

					case JsErrorCode.WrongThread:
						throw new JsUsageException(error, "Runtime is active on another thread.");

					case JsErrorCode.RuntimeInUse:
						throw new JsUsageException(error, "Runtime is in use.");

					case JsErrorCode.BadSerializedScript:
						throw new JsUsageException(error, "Bad serialized script.");

					case JsErrorCode.InDisabledState:
						throw new JsUsageException(error, "Runtime is disabled.");

					case JsErrorCode.CannotDisableExecution:
						throw new JsUsageException(error, "Cannot disable execution.");

					case JsErrorCode.HeapEnumInProgress:
						throw new JsUsageException(error, "Heap enumeration is in progress.");

					case JsErrorCode.ArgumentNotObject:
						throw new JsUsageException(error, "Argument is not an object.");

					case JsErrorCode.InProfileCallback:
						throw new JsUsageException(error, "In a profile callback.");

					case JsErrorCode.InThreadServiceCallback:
						throw new JsUsageException(error, "In a thread service callback.");

					case JsErrorCode.CannotSerializeDebugScript:
						throw new JsUsageException(error, "Cannot serialize a debug script.");

					case JsErrorCode.AlreadyDebuggingContext:
						throw new JsUsageException(error, "Context is already in debug mode.");

					case JsErrorCode.AlreadyProfilingContext:
						throw new JsUsageException(error, "Already profiling this context.");

					case JsErrorCode.IdleNotEnabled:
						throw new JsUsageException(error, "Idle is not enabled.");

					case JsErrorCode.CannotSetProjectionEnqueueCallback:
						throw new JsUsageException(error, "Cannot set projection enqueue callback.");

					case JsErrorCode.CannotStartProjection:
						throw new JsUsageException(error, "Cannot start projection.");

					case JsErrorCode.InObjectBeforeCollectCallback:
						throw new JsUsageException(error, "In object before collect callback.");

					case JsErrorCode.ObjectNotInspectable:
						throw new JsUsageException(error, "Object not inspectable.");

					case JsErrorCode.PropertyNotSymbol:
						throw new JsUsageException(error, "Property not symbol.");

					case JsErrorCode.PropertyNotString:
						throw new JsUsageException(error, "Property not string.");

					case JsErrorCode.InvalidContext:
						throw new JsUsageException(error, "Invalid context.");

					case JsErrorCode.InvalidModuleHostInfoKind:
						throw new JsUsageException(error, "Invalid module host info kind.");

					case JsErrorCode.ModuleParsed:
						throw new JsUsageException(error, "Module parsed.");

					case JsErrorCode.NoWeakRefRequired:
						throw new JsUsageException(error, "No weak reference is required, the value will never be collected.");

					case JsErrorCode.PromisePending:
						throw new JsUsageException(error, "The `Promise` object is still in the pending state.");

					case JsErrorCode.ModuleNotEvaluated:
						throw new JsUsageException(error, "Module was not yet evaluated when `JsGetModuleNamespace` was called.");

					#endregion

					#region Engine

					case JsErrorCode.OutOfMemory:
						throw new JsEngineException(error, "Out of memory.");

					case JsErrorCode.BadFPUState:
						throw new JsEngineException(error, "Bad the Floating Point Unit state.");

					#endregion

					#region Script

					case JsErrorCode.ScriptException:
					case JsErrorCode.ScriptCompile:
						{
							JsValue errorMetadata;
							JsErrorCode innerError = NativeMethods.JsGetAndClearExceptionWithMetadata(out errorMetadata);

							if (innerError != JsErrorCode.NoError)
							{
								throw new JsFatalException(innerError);
							}

							string message = error == JsErrorCode.ScriptCompile ?
								"Compile error." : "Script threw an exception.";

							throw new JsScriptException(error, errorMetadata, message);
						}

					case JsErrorCode.ScriptTerminated:
						throw new JsScriptException(error, JsValue.Invalid, "Script was terminated.");

					case JsErrorCode.ScriptEvalDisabled:
						throw new JsScriptException(error, JsValue.Invalid, "Eval of strings is disabled in this runtime.");

					#endregion

					#region Fatal

					case JsErrorCode.Fatal:
						throw new JsFatalException(error, "Fatal error.");

					case JsErrorCode.WrongRuntime:
						throw new JsFatalException(error, "Wrong runtime.");

					#endregion

					default:
						throw new JsFatalException(error);
				}
			}
		}

		/// <summary>
		/// Creates a new JavaScript error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">The message that describes the error</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateError(string message)
		{
			JsValue messageValue = JsValue.FromString(message);
			JsValue errorValue = JsValue.CreateError(messageValue);

			return errorValue;
		}

		/// <summary>
		/// Creates a new JavaScript RangeError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">The message that describes the error</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateRangeError(string message)
		{
			JsValue messageValue = JsValue.FromString(message);
			JsValue errorValue = JsValue.CreateRangeError(messageValue);

			return errorValue;
		}

		/// <summary>
		/// Creates a new JavaScript ReferenceError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">The message that describes the error</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateReferenceError(string message)
		{
			JsValue messageValue = JsValue.FromString(message);
			JsValue errorValue = JsValue.CreateReferenceError(messageValue);

			return errorValue;
		}

		/// <summary>
		/// Creates a new JavaScript SyntaxError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">The message that describes the error</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateSyntaxError(string message)
		{
			JsValue messageValue = JsValue.FromString(message);
			JsValue errorValue = JsValue.CreateSyntaxError(messageValue);

			return errorValue;
		}

		/// <summary>
		/// Creates a new JavaScript TypeError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">The message that describes the error</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateTypeError(string message)
		{
			JsValue messageValue = JsValue.FromString(message);
			JsValue errorValue = JsValue.CreateTypeError(messageValue);

			return errorValue;
		}

		/// <summary>
		/// Creates a new JavaScript URIError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">The message that describes the error</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateUriError(string message)
		{
			JsValue messageValue = JsValue.FromString(message);
			JsValue errorValue = JsValue.CreateUriError(messageValue);

			return errorValue;
		}

		/// <summary>
		/// Sets a exception
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="exception">The error object</param>
		public static void SetException(JsValue exception)
		{
			JsErrorCode innerError = NativeMethods.JsSetException(exception);
			if (innerError != JsErrorCode.NoError)
			{
				throw new JsFatalException(innerError);
			}
		}
	}
}