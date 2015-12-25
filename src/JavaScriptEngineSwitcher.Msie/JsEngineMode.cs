namespace JavaScriptEngineSwitcher.Msie
{
	/// <summary>
	/// JavaScript engine modes
	/// </summary>
	public enum JsEngineMode
	{
		/// <summary>
		/// Automatically selects the most modern JavaScript engine from available on the machine
		/// </summary>
		Auto = 0,

		/// <summary>
		/// Classic MSIE JavaScript engine (supports ECMAScript 3 with
		/// possibility of using the ECMAScript 5 Polyfill and the JSON2 library).
		/// Requires Internet Explorer 6 or higher on the machine.
		/// </summary>
		Classic,

		/// <summary>
		/// ActiveScript version of Chakra JavaScript engine (supports ECMAScript 3
		/// with possibility of using the ECMAScript 5 Polyfill and the JSON2 library).
		/// Requires Internet Explorer 9 or higher on the machine.
		/// </summary>
		ChakraActiveScript,

		/// <summary>
		/// “IE” JsRT version of Chakra JavaScript engine (supports ECMAScript 5).
		/// Requires Internet Explorer 11 or Microsoft Edge on the machine.
		/// </summary>
		ChakraIeJsRt,

		/// <summary>
		/// “Edge” JsRT version of Chakra JavaScript engine (supports ECMAScript 5).
		/// Requires Microsoft Edge on the machine.
		/// </summary>
		ChakraEdgeJsRt
	}
}