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
	}
}