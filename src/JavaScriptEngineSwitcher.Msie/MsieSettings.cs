namespace JavaScriptEngineSwitcher.Msie
{
	/// <summary>
	/// Settings of the MSIE JS engine
	/// </summary>
	public sealed class MsieSettings
	{
		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// </summary>
		public bool EnableDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a JS engine mode
		/// </summary>
		public JsEngineMode EngineMode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to use the ECMAScript 5 Polyfill
		/// </summary>
		public bool UseEcmaScript5Polyfill
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to use the JSON2 library
		/// </summary>
		public bool UseJson2Library
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the MSIE settings
		/// </summary>
		public MsieSettings()
		{
			EnableDebugging = false;
			EngineMode = JsEngineMode.Auto;
			UseEcmaScript5Polyfill = false;
			UseJson2Library = false;
		}
	}
}