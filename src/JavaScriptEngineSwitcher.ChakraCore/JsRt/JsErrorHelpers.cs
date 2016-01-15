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

					case JsErrorCode.AlreadyDebuggingContext:
						throw new JsUsageException(error, "Context is already in debug mode.");

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

					case JsErrorCode.AlreadyProfilingContext:
						throw new JsUsageException(error, "Already profiling this context.");

					case JsErrorCode.IdleNotEnabled:
						throw new JsUsageException(error, "Idle is not enabled.");

					case JsErrorCode.OutOfMemory:
						throw new JsEngineException(error, "Out of memory.");

					case JsErrorCode.ScriptException:
						{
							JsValue errorObject;
							JsErrorCode innerError = NativeMethods.JsGetAndClearException(out errorObject);

							if (innerError != JsErrorCode.NoError)
							{
								throw new JsFatalException(innerError);
							}

							throw new JsScriptException(error, errorObject, "Script threw an exception.");
						}

					case JsErrorCode.ScriptCompile:
						{
							JsValue errorObject;
							JsErrorCode innerError = NativeMethods.JsGetAndClearException(out errorObject);

							if (innerError != JsErrorCode.NoError)
							{
								throw new JsFatalException(innerError);
							}

							throw new JsScriptException(error, errorObject, "Compile error.");
						}

					case JsErrorCode.ScriptTerminated:
						throw new JsScriptException(error, JsValue.Invalid, "Script was terminated.");

					case JsErrorCode.ScriptEvalDisabled:
						throw new JsScriptException(error, JsValue.Invalid, "Eval of strings is disabled in this runtime.");

					case JsErrorCode.Fatal:
						throw new JsFatalException(error);

					default:
						throw new JsFatalException(error);
				}
			}
		}
	}
}