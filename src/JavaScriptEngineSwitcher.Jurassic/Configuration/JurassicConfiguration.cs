namespace JavaScriptEngineSwitcher.Jurassic.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Configuration settings of Jurassic JavaScript engine
	/// </summary>
	public sealed class JurassicConfiguration : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a generation of debug information)
		/// </summary>
		[ConfigurationProperty("enableDebugging", DefaultValue = false)]
		public bool EnableDebugging
		{
			get { return (bool)this["enableDebugging"]; }
			set { this["enableDebugging"] = value; }
		}

		/// <summary>
		/// Gets or sets a flag for whether to disassemble any generated IL
		/// and store it in the associated function
		/// </summary>
		[ConfigurationProperty("enableIlAnalysis", DefaultValue = false)]
		public bool EnableIlAnalysis
		{
			get { return (bool)this["enableIlAnalysis"]; }
			set { this["enableIlAnalysis"] = value; }
		}

		/// <summary>
		/// Gets or sets a flag for whether to allow run the script in strict mode
		/// </summary>
		[ConfigurationProperty("strictMode", DefaultValue = false)]
		public bool StrictMode
		{
			get { return (bool)this["strictMode"]; }
			set { this["strictMode"] = value; }
		}
	}
}