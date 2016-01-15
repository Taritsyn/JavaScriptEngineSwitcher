namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Called by the runtime to load the source code of the serialized script.
	/// The caller must keep the script buffer valid until the JsSerializedScriptUnloadCallback.
	/// </summary>
	/// <param name="sourceContext">The context passed to Js[Parse|Run]SerializedScriptWithCallback</param>
	/// <param name="scriptBuffer">The script returned</param>
	/// <returns>true if the operation succeeded, false otherwise</returns>
	internal delegate bool JsSerializedScriptLoadSourceCallback(JsSourceContext sourceContext, out string scriptBuffer);
}