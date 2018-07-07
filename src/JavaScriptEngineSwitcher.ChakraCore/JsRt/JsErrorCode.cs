namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The error code returned from a Chakra hosting API
	/// </summary>
	public enum JsErrorCode : uint
	{
		/// <summary>
		/// Success error code
		/// </summary>
		NoError = 0,

		#region Usage

		/// <summary>
		/// Category of errors that relates to incorrect usage of the API itself
		/// </summary>
		CategoryUsage = 0x10000,

		/// <summary>
		/// An argument to a hosting API was invalid
		/// </summary>
		InvalidArgument,

		/// <summary>
		/// An argument to a hosting API was null in a context where null is not allowed
		/// </summary>
		NullArgument,

		/// <summary>
		/// The hosting API requires that a context be current, but there is no current context
		/// </summary>
		NoCurrentContext,

		/// <summary>
		/// The engine is in an exception state and no APIs can be called until the exception is
		/// cleared
		/// </summary>
		InExceptionState,

		/// <summary>
		/// A hosting API is not yet implemented
		/// </summary>
		NotImplemented,

		/// <summary>
		/// A hosting API was called on the wrong thread
		/// </summary>
		WrongThread,

		/// <summary>
		/// A runtime that is still in use cannot be disposed
		/// </summary>
		RuntimeInUse,

		/// <summary>
		/// A bad serialized script was used, or the serialized script was serialized by a
		/// different version of the Chakra engine
		/// </summary>
		BadSerializedScript,

		/// <summary>
		/// The runtime is in a disabled state
		/// </summary>
		InDisabledState,

		/// <summary>
		/// Runtime does not support reliable script interruption
		/// </summary>
		CannotDisableExecution,

		/// <summary>
		/// A heap enumeration is currently underway in the script context
		/// </summary>
		HeapEnumInProgress,

		/// <summary>
		/// A hosting API that operates on object values was called with a non-object value
		/// </summary>
		ArgumentNotObject,

		/// <summary>
		/// A script context is in the middle of a profile callback
		/// </summary>
		InProfileCallback,

		/// <summary>
		/// A thread service callback is currently underway
		/// </summary>
		InThreadServiceCallback,

		/// <summary>
		/// Scripts cannot be serialized in debug contexts
		/// </summary>
		CannotSerializeDebugScript,

		/// <summary>
		/// The context cannot be put into a debug state because it is already in a debug state
		/// </summary>
		AlreadyDebuggingContext,

		/// <summary>
		/// The context cannot start profiling because it is already profiling
		/// </summary>
		AlreadyProfilingContext,

		/// <summary>
		/// Idle notification given when the host did not enable idle processing
		/// </summary>
		IdleNotEnabled,

		/// <summary>
		/// The context did not accept the enqueue callback
		/// </summary>
		CannotSetProjectionEnqueueCallback,

		/// <summary>
		/// Failed to start projection
		/// </summary>
		CannotStartProjection,

		/// <summary>
		/// The operation is not supported in an object before collect callback
		/// </summary>
		InObjectBeforeCollectCallback,

		/// <summary>
		/// Object cannot be unwrapped to IInspectable pointer
		/// </summary>
		ObjectNotInspectable,

		/// <summary>
		/// A hosting API that operates on symbol property ids but was called with a non-symbol property id.
		/// The error code is returned by JsGetSymbolFromPropertyId if the function is called with non-symbol property id.
		/// </summary>
		PropertyNotSymbol,

		/// <summary>
		/// A hosting API that operates on string property ids but was called with a non-string property id.
		/// The error code is returned by existing JsGetPropertyNamefromId if the function is called with non-string property id.
		/// </summary>
		PropertyNotString,

		/// <summary>
		/// Module evaluation is called in wrong context
		/// </summary>
		InvalidContext,

		/// <summary>
		/// Module evaluation is called in wrong context
		/// </summary>
		InvalidModuleHostInfoKind,

		/// <summary>
		/// Module was parsed already when JsParseModuleSource is called
		/// </summary>
		ModuleParsed,

		/// <summary>
		/// Argument passed to JsCreateWeakReference is a primitive that is not managed by the GC.
		/// No weak reference is required, the value will never be collected.
		/// </summary>
		NoWeakRefRequired,

		/// <summary>
		/// The <c>Promise</c> object is still in the pending state
		/// </summary>
		PromisePending,

		/// <summary>
		/// Module was not yet evaluated when <c>JsGetModuleNamespace</c> was called
		/// </summary>
		ModuleNotEvaluated,

		#endregion

		#region Engine

		/// <summary>
		/// Category of errors that relates to errors occurring within the engine itself
		/// </summary>
		CategoryEngine = 0x20000,

		/// <summary>
		/// The Chakra engine has run out of memory
		/// </summary>
		OutOfMemory,

		/// <summary>
		/// The Chakra engine failed to set the Floating Point Unit state
		/// </summary>
		BadFPUState,

		#endregion

		#region Script

		/// <summary>
		/// Category of errors that relates to errors in a script
		/// </summary>
		CategoryScript = 0x30000,

		/// <summary>
		/// A JavaScript exception occurred while running a script
		/// </summary>
		ScriptException,

		/// <summary>
		/// JavaScript failed to compile
		/// </summary>
		ScriptCompile,

		/// <summary>
		/// A script was terminated due to a request to suspend a runtime
		/// </summary>
		ScriptTerminated,

		/// <summary>
		/// A script was terminated because it tried to use <c>eval</c> or <c>function</c> and eval
		/// was disabled
		/// </summary>
		ScriptEvalDisabled,

		#endregion

		#region Fatal

		/// <summary>
		/// Category of errors that are fatal and signify failure of the engine
		/// </summary>
		CategoryFatal = 0x40000,

		/// <summary>
		/// A fatal error in the engine has occurred
		/// </summary>
		Fatal,

		/// <summary>
		/// A hosting API was called with object created on different javascript runtime
		/// </summary>
		WrongRuntime,

		#endregion

		#region Diagnostic

		/// <summary>
		/// Category of errors that are related to failures during diagnostic operations
		/// </summary>
		CategoryDiagError = 0x50000,

		/// <summary>
		/// The object for which the debugging API was called was not found
		/// </summary>
		DiagAlreadyInDebugMode,

		/// <summary>
		/// The debugging API can only be called when VM is in debug mode
		/// </summary>
		DiagNotInDebugMode,

		/// <summary>
		/// The debugging API can only be called when VM is at a break
		/// </summary>
		DiagNotAtBreak,

		/// <summary>
		/// Debugging API was called with an invalid handle.
		/// </summary>
		DiagInvalidHandle,

		/// <summary>
		/// The object for which the debugging API was called was not found
		/// </summary>
		DiagObjectNotFound,

		/// <summary>
		/// VM was unable to perform the request action
		/// </summary>
		DiagUnableToPerformAction

		#endregion
	}
}