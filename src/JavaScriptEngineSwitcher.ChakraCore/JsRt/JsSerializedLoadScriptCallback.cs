namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Called by the runtime to load the source code of the serialized script
	/// </summary>
	/// <param name="sourceContext">A cookie identifying the script that can be used
	/// by debuggable script contexts</param>
	/// <param name="value">The script returned</param>
	/// <param name="parseAttributes">Attribute mask for parsing the script</param>
	/// <returns>true if the operation succeeded, false otherwise</returns>
	internal delegate bool JsSerializedLoadScriptCallback(JsSourceContext sourceContext,
		out JsValue value, out JsParseScriptAttributes parseAttributes);
}