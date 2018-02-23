namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Settings of the Jurassic JS engine
	/// </summary>
	public sealed class JurassicSettings
	{
		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a generation of debug information)
		/// </summary>
		public bool EnableDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disassemble any generated IL
		/// and store it in the associated function
		/// </summary>
		public bool EnableIlAnalysis
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to allow run the script in strict mode
		/// </summary>
		public bool StrictMode
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the Jurassic settings
		/// </summary>
		public JurassicSettings()
		{
			EnableDebugging = false;
			EnableIlAnalysis = false;
			StrictMode = false;
		}
	}
}