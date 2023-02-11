namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Called by the runtime to load the source code of the serialized script.
	/// The caller must keep the script buffer valid until the <c>JsSerializedScriptUnloadCallback</c>.
	/// </summary>
	/// <param name="sourceContext">The context passed to <c>Js[Parse|Run]SerializedScriptWithCallback</c></param>
	/// <param name="scriptBuffer">The script returned</param>
	/// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise</returns>
	internal delegate bool JsSerializedScriptLoadSourceCallback(JsSourceContext sourceContext, out string scriptBuffer);
}