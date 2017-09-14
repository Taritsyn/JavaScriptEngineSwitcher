namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Settings of the Jint JS engine
	/// </summary>
	public sealed class JintSettings
	{
		/// <summary>
		/// Gets or sets a flag for whether to allow the <code>debugger</code> statement
		/// to be called in a script
		/// </summary>
		public bool AllowDebuggerStatement
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to enable debug mode
		/// </summary>
		public bool EnableDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum allowed depth of recursion:
		///	   -1 - recursion without limits;
		///     N - one scope function can be called no more than N times.
		/// </summary>
		public int MaxRecursionDepth
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum number of statements
		/// </summary>
		public int MaxStatements
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
		/// Gets or sets a number of milliseconds to wait before the script execution times out
		/// </summary>
		public int Timeout
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the Jint settings
		/// </summary>
		public JintSettings()
		{
			AllowDebuggerStatement = false;
			EnableDebugging = false;
			MaxRecursionDepth = -1;
			MaxStatements = 0;
			StrictMode = false;
			Timeout = 0;
		}
	}
}