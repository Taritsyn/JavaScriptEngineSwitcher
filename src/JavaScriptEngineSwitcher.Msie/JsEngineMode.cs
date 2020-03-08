namespace JavaScriptEngineSwitcher.Msie
{
	/// <summary>
	/// MSIE JS engine modes
	/// </summary>
	public enum JsEngineMode
	{
		/// <summary>
		/// Automatically selects the most modern JS engine from available on the machine
		/// </summary>
		Auto = 0,

		/// <summary>
		/// Classic MSIE JS engine (supports ECMAScript 3 with possibility of using
		/// the ECMAScript 5 Polyfill and the JSON2 library).
		/// Requires Internet Explorer 6 or higher on the machine.
		/// Not supported in version for .NET Core.
		/// </summary>
		Classic,

		/// <summary>
		/// ActiveScript version of Chakra JS engine (supports ECMAScript 5).
		/// Requires Internet Explorer 9 or higher on the machine.
		/// Not supported in version for .NET Core.
		/// </summary>
		ChakraActiveScript,

		/// <summary>
		/// “IE” JsRT version of Chakra JS engine (supports ECMAScript 5).
		/// Requires Internet Explorer 11 or Microsoft Edge Legacy on the machine.
		/// </summary>
		ChakraIeJsRt,

		/// <summary>
		/// “Edge” JsRT version of Chakra JS engine (supports ECMAScript 5).
		/// Requires Microsoft Edge Legacy on the machine.
		/// </summary>
		ChakraEdgeJsRt
	}
}