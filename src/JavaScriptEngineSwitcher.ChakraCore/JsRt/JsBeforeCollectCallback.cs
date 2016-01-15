namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	using System;

	/// <summary>
	/// The callback called before collection
	/// </summary>
	/// <param name="callbackState">The state passed to SetBeforeCollectCallback</param>
	internal delegate void JsBeforeCollectCallback(IntPtr callbackState);
}