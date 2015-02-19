namespace JavaScriptEngineSwitcher.Jint.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Configuration settings of Jint JavaScript engine
	/// </summary>
	public sealed class JintConfiguration : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a <code>debugger</code> statement to be called in a script)
		/// </summary>
		[ConfigurationProperty("enableDebugging", DefaultValue = false)]
		public bool EnableDebugging
		{
			get { return (bool)this["enableDebugging"]; }
			set { this["enableDebugging"] = value; }
		}

		/// <summary>
		/// Gets or sets a maximum allowed depth of recursion:
		///	   -1 - recursion without limits;
		///     N - one scope function can be called no more than N times.
		/// </summary>
		[ConfigurationProperty("maxRecursionDepth", DefaultValue = 20678)]
		[IntegerValidator(MinValue = -1, MaxValue = int.MaxValue, ExcludeRange = false)]
		public int MaxRecursionDepth
		{
			get { return (int)this["maxRecursionDepth"]; }
			set { this["maxRecursionDepth"] = value; }
		}

		/// <summary>
		/// Gets or sets a maximum number of statements
		/// </summary>
		[ConfigurationProperty("maxStatements", DefaultValue = 0)]
		[IntegerValidator(MinValue = 0, MaxValue = int.MaxValue, ExcludeRange = false)]
		public int MaxStatements
		{
			get { return (int)this["maxStatements"]; }
			set { this["maxStatements"] = value; }
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

		/// <summary>
		/// Gets or sets a number of milliseconds to wait before the script execution times out
		/// </summary>
		[ConfigurationProperty("timeout", DefaultValue = 0)]
		public int Timeout
		{
			get { return (int)this["timeout"]; }
			set { this["timeout"] = value; }
		}
	}
}