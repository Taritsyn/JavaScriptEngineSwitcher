namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Settings of the V8 JS engine
	/// </summary>
	public sealed class V8Settings
	{
		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a TCP/IP-based debugging)
		/// </summary>
		public bool EnableDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a TCP/IP port on which to listen for a debugger connection
		/// </summary>
		public int DebugPort
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable global members
		/// </summary>
		public bool DisableGlobalMembers
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size of the new object heap in mebibytes
		/// </summary>
		public int MaxNewSpaceSize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size of the old object heap in mebibytes
		/// </summary>
		public int MaxOldSpaceSize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size of the executable code heap in mebibytes
		/// </summary>
		public int MaxExecutableSize
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs instance of the V8 settings
		/// </summary>
		public V8Settings()
		{
			EnableDebugging = false;
			DebugPort = 9222;
			DisableGlobalMembers = false;
			MaxNewSpaceSize = 0;
			MaxOldSpaceSize = 0;
			MaxExecutableSize = 0;
		}
	}
}