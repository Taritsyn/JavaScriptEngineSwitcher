using System;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// A finalizer callback
	/// </summary>
	/// <param name="data">The external data that was passed in when creating the object being finalized</param>
	internal delegate void JsFinalizeCallback(IntPtr data);
}