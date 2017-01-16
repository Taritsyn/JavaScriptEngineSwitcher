namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Called by the runtime to load the source code of the serialized script
	/// </summary>
	/// <param name="sourceContext">The context passed to Js[Parse|Run]Serialized</param>
	/// <param name="value">The result of the compiled script</param>
	/// <param name="parseAttributes">Attribute mask for parsing the script</param>
	/// <returns>true if the operation succeeded, false otherwise</returns>
	internal delegate bool JsSerializedLoadScriptCallback(JsSourceContext sourceContext,
		out JsValue value, out JsParseScriptAttributes parseAttributes);
}