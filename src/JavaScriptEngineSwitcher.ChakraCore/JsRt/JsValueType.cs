namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The JavaScript type of a JavaScript value
	/// </summary>
	internal enum JsValueType
	{
		/// <summary>
		/// The value is the <c>undefined</c> value
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// The value is the <c>null</c> value
		/// </summary>
		Null = 1,

		/// <summary>
		/// The value is a JavaScript <c>Number</c> value
		/// </summary>
		Number = 2,

		/// <summary>
		/// The value is a JavaScript <c>String</c> value
		/// </summary>
		String = 3,

		/// <summary>
		/// The value is a JavaScript <c>Boolean</c> value
		/// </summary>
		Boolean = 4,

		/// <summary>
		/// The value is a JavaScript <c>Object</c> value
		/// </summary>
		Object = 5,

		/// <summary>
		/// The value is a JavaScript <c>Function</c> object value
		/// </summary>
		Function = 6,

		/// <summary>
		/// The value is a JavaScript <c>Error</c> object value
		/// </summary>
		Error = 7,

		/// <summary>
		/// The value is a JavaScript <c>Array</c> object value
		/// </summary>
		Array = 8,

		/// <summary>
		/// The value is a JavaScript <c>Symbol</c> object value
		/// </summary>
		Symbol = 9,

		/// <summary>
		/// The value is a JavaScript <c>ArrayBuffer</c> object value
		/// </summary>
		ArrayBuffer = 10,

		/// <summary>
		/// The value is a JavaScript typed array object value
		/// </summary>
		TypedArray = 11,

		/// <summary>
		/// The value is a JavaScript <c>DataView</c> object value
		/// </summary>
		DataView = 12
	}
}