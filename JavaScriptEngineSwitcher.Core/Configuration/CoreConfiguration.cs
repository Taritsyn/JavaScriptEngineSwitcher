namespace JavaScriptEngineSwitcher.Core.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Configuration settings of core
	/// </summary>
	public sealed class CoreConfiguration : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets a name of default JavaScript engine
		/// </summary>
		[ConfigurationProperty("defaultEngine", DefaultValue = "")]
		public string DefaultEngine
		{
			get { return (string)this["defaultEngine"]; }
			set { this["defaultEngine"] = value; }
		}

		/// <summary>
		/// Gets a list of registered JavaScript engines
		/// </summary>
		[ConfigurationProperty("engines", IsRequired = true)]
		public JsEngineRegistrationList Engines
		{
			get { return (JsEngineRegistrationList)this["engines"]; }
		}
	}
}