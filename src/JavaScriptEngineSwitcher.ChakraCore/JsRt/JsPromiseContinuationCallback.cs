using System;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The promise continuation callback
	/// </summary>
	/// <remarks>
	/// The host can specify a promise continuation callback in <c>JsSetPromiseContinuationCallback</c>. If
	/// a script creates a task to be run later, then the promise continuation callback will be called with
	/// the task and the task should be put in a FIFO queue, to be run when the current script is
	/// done executing.
	/// </remarks>
	/// <param name="task">The task, represented as a JavaScript function</param>
	/// <param name="callbackState">The data argument to be passed to the callback</param>
	internal delegate void JsPromiseContinuationCallback(JsValue task, IntPtr callbackState);
}