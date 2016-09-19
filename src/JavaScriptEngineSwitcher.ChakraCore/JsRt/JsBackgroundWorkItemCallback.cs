using System;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The background work item callback
	/// </summary>
	/// <remarks>
	/// This is passed to the host's thread service (if provided) to allow the host to
	/// invoke the work item callback on the background thread of its choice.
	/// </remarks>
	/// <param name="callbackData">Data argument passed to the thread service</param>
	internal delegate void JsBackgroundWorkItemCallback(IntPtr callbackData);
}