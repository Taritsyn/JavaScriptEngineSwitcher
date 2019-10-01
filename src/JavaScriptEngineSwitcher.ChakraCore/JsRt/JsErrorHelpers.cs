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
		/// <param name="errorCode">The error code</param>
		public static void ThrowIfError(JsErrorCode errorCode)
		{
			if (errorCode != JsErrorCode.NoError)
			{
				switch (errorCode)
				{
					#region Usage

					case JsErrorCode.InvalidArgument:
						throw new JsUsageException(errorCode, "Invalid argument.");

					case JsErrorCode.NullArgument:
						throw new JsUsageException(errorCode, "Null argument.");

					case JsErrorCode.NoCurrentContext:
						throw new JsUsageException(errorCode, "No current context.");

					case JsErrorCode.InExceptionState:
						throw new JsUsageException(errorCode, "Runtime is in exception state.");

					case JsErrorCode.NotImplemented:
						throw new JsUsageException(errorCode, "Method is not implemented.");

					case JsErrorCode.WrongThread:
						throw new JsUsageException(errorCode, "Runtime is active on another thread.");

					case JsErrorCode.RuntimeInUse:
						throw new JsUsageException(errorCode, "Runtime is in use.");

					case JsErrorCode.BadSerializedScript:
						throw new JsUsageException(errorCode, "Bad serialized script.");

					case JsErrorCode.InDisabledState:
						throw new JsUsageException(errorCode, "Runtime is disabled.");

					case JsErrorCode.CannotDisableExecution:
						throw new JsUsageException(errorCode, "Cannot disable execution.");

					case JsErrorCode.HeapEnumInProgress:
						throw new JsUsageException(errorCode, "Heap enumeration is in progress.");

					case JsErrorCode.ArgumentNotObject:
						throw new JsUsageException(errorCode, "Argument is not an object.");

					case JsErrorCode.InProfileCallback:
						throw new JsUsageException(errorCode, "In a profile callback.");

					case JsErrorCode.InThreadServiceCallback:
						throw new JsUsageException(errorCode, "In a thread service callback.");

					case JsErrorCode.CannotSerializeDebugScript:
						throw new JsUsageException(errorCode, "Cannot serialize a debug script.");

					case JsErrorCode.AlreadyDebuggingContext:
						throw new JsUsageException(errorCode, "Context is already in debug mode.");

					case JsErrorCode.AlreadyProfilingContext:
						throw new JsUsageException(errorCode, "Already profiling this context.");

					case JsErrorCode.IdleNotEnabled:
						throw new JsUsageException(errorCode, "Idle is not enabled.");

					case JsErrorCode.CannotSetProjectionEnqueueCallback:
						throw new JsUsageException(errorCode, "Cannot set projection enqueue callback.");

					case JsErrorCode.CannotStartProjection:
						throw new JsUsageException(errorCode, "Cannot start projection.");

					case JsErrorCode.InObjectBeforeCollectCallback:
						throw new JsUsageException(errorCode, "In object before collect callback.");

					case JsErrorCode.ObjectNotInspectable:
						throw new JsUsageException(errorCode, "Object not inspectable.");

					case JsErrorCode.PropertyNotSymbol:
						throw new JsUsageException(errorCode, "Property not symbol.");

					case JsErrorCode.PropertyNotString:
						throw new JsUsageException(errorCode, "Property not string.");

					case JsErrorCode.InvalidContext:
						throw new JsUsageException(errorCode, "Invalid context.");

					case JsErrorCode.InvalidModuleHostInfoKind:
						throw new JsUsageException(errorCode, "Invalid module host info kind.");

					case JsErrorCode.ModuleParsed:
						throw new JsUsageException(errorCode, "Module parsed.");

					case JsErrorCode.NoWeakRefRequired:
						throw new JsUsageException(errorCode, "No weak reference is required, the value will never be collected.");

					case JsErrorCode.PromisePending:
						throw new JsUsageException(errorCode, "The `Promise` object is still in the pending state.");

					case JsErrorCode.ModuleNotEvaluated:
						throw new JsUsageException(errorCode, "Module was not yet evaluated when `JsGetModuleNamespace` was called.");

					#endregion

					#region Engine

					case JsErrorCode.OutOfMemory:
						throw new JsEngineException(errorCode, "Out of memory.");

					case JsErrorCode.BadFPUState:
						throw new JsEngineException(errorCode, "Bad the Floating Point Unit state.");

					#endregion

					#region Script

					case JsErrorCode.ScriptException:
					case JsErrorCode.ScriptCompile:
						{
							JsValue errorMetadata;
							JsErrorCode innerErrorCode = NativeMethods.JsGetAndClearExceptionWithMetadata(out errorMetadata);

							if (innerErrorCode != JsErrorCode.NoError)
							{
								throw new JsFatalException(innerErrorCode);
							}

							string message = errorCode == JsErrorCode.ScriptCompile ?
								"Compile error." : "Script threw an exception.";

							throw new JsScriptException(errorCode, errorMetadata, message);
						}

					case JsErrorCode.ScriptTerminated:
						throw new JsScriptException(errorCode, JsValue.Invalid, "Script was terminated.");

					case JsErrorCode.ScriptEvalDisabled:
						throw new JsScriptException(errorCode, JsValue.Invalid, "Eval of strings is disabled in this runtime.");

					#endregion

					#region Fatal

					case JsErrorCode.Fatal:
						throw new JsFatalException(errorCode, "Fatal error.");

					case JsErrorCode.WrongRuntime:
						throw new JsFatalException(errorCode, "Wrong runtime.");

					#endregion

					default:
						throw new JsFatalException(errorCode);
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
	}
}