namespace JavaScriptEngineSwitcher.Msie.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Configuration settings of MSIE JavaScript engine
	/// </summary>
	public sealed class MsieConfiguration : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets a JavaScript engine mode
		/// </summary>
		[ConfigurationProperty("engineMode", DefaultValue = JsEngineMode.Auto)]
		public JsEngineMode EngineMode
		{
			get { return (JsEngineMode)this["engineMode"]; }
			set { this["engineMode"] = value; }
		}

		/// <summary>
		/// Gets or sets a flag for whether to use the ECMAScript 5 Polyfill
		/// </summary>
		[ConfigurationProperty("useEcmaScript5Polyfill", DefaultValue = false)]
		public bool UseEcmaScript5Polyfill
		{
			get { return (bool)this["useEcmaScript5Polyfill"]; }
			set { this["useEcmaScript5Polyfill"] = value; }
		}

		/// <summary>
		/// Gets or sets a flag for whether to use the JSON2 library
		/// </summary>
		[ConfigurationProperty("useJson2Library", DefaultValue = false)]
		public bool UseJson2Library
		{
			get { return (bool)this["useJson2Library"]; }
			set { this["useJson2Library"] = value; }
		}
	}
}