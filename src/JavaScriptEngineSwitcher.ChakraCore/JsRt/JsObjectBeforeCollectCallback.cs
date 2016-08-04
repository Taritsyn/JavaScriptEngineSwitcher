using System;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The callback called before collecting an object
	/// </summary>
	/// <remarks>
	/// Use <c>JsSetObjectBeforeCollectCallback</c> to register this callback
	/// </remarks>
	/// <param name="reference">The object to be collected</param>
	/// <param name="callbackState">The state passed to <c>JsSetObjectBeforeCollectCallback</c></param>
	internal delegate void JsObjectBeforeCollectCallback(JsValue reference, IntPtr callbackState);
}