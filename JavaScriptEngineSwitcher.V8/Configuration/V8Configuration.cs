namespace JavaScriptEngineSwitcher.V8.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Configuration settings of V8 JavaScript engine
	/// </summary>
	public sealed class V8Configuration : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a TCP/IP-based debugging)
		/// </summary>
		[ConfigurationProperty("enableDebugging", DefaultValue = false)]
		public bool EnableDebugging
		{
			get { return (bool)this["enableDebugging"]; }
			set { this["enableDebugging"] = value; }
		}

		/// <summary>
		/// Gets or sets a TCP/IP port on which to listen for a debugger connection
		/// </summary>
		[ConfigurationProperty("debugPort", DefaultValue = 9222)]
		[IntegerValidator(MinValue = 0, MaxValue = 65535, ExcludeRange = false)]
		public int DebugPort
		{
			get { return (int)this["debugPort"]; }
			set { this["debugPort"] = value; }
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable global members
		/// </summary>
		[ConfigurationProperty("disableGlobalMembers", DefaultValue = false)]
		public bool DisableGlobalMembers
		{
			get { return (bool)this["disableGlobalMembers"]; }
			set { this["disableGlobalMembers"] = value; }
		}

		/// <summary>
		/// Gets or sets a maximum size of the new object heap in mebibytes
		/// </summary>
		[ConfigurationProperty("maxNewSpaceSize", DefaultValue = 0)]
		[IntegerValidator(MinValue = 0, MaxValue = int.MaxValue, ExcludeRange = false)]
		public int MaxNewSpaceSize
		{
			get { return (int)this["maxNewSpaceSize"]; }
			set { this["maxNewSpaceSize"] = value; }
		}

		/// <summary>
		/// Gets or sets a maximum size of the old object heap in mebibytes
		/// </summary>
		[ConfigurationProperty("maxOldSpaceSize", DefaultValue = 0)]
		[IntegerValidator(MinValue = 0, MaxValue = int.MaxValue, ExcludeRange = false)]
		public int MaxOldSpaceSize
		{
			get { return (int)this["maxOldSpaceSize"]; }
			set { this["maxOldSpaceSize"] = value; }
		}

		/// <summary>
		/// Gets or sets a maximum size of the executable code heap in mebibytes
		/// </summary>
		[ConfigurationProperty("maxExecutableSize", DefaultValue = 0)]
		[IntegerValidator(MinValue = 0, MaxValue = int.MaxValue, ExcludeRange = false)]
		public int MaxExecutableSize
		{
			get { return (int)this["maxExecutableSize"]; }
			set { this["maxExecutableSize"] = value; }
		}
	}
}