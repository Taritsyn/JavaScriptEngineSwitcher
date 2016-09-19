using System;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The callback called before collection
	/// </summary>
	/// <param name="callbackState">The state passed to SetBeforeCollectCallback</param>
	internal delegate void JsBeforeCollectCallback(IntPtr callbackState);
}