using System;

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
		/// Gets or sets a local time zone for the <code>Date</code> objects in the script
		/// </summary>
		public TimeZoneInfo LocalTimeZone
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum allowed depth of recursion:
		///    -1 - recursion without limits;
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
		/// Gets or sets a current memory limit for a engine in bytes
		/// </summary>
		public long MemoryLimit
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a timeout interval for regular expressions.
		/// If the value of this property is null, then the value of regular expression
		/// timeout interval are taken from the <see cref="TimeoutInterval"/> property.
		/// </summary>
		public TimeSpan? RegexTimeoutInterval
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
		[Obsolete("Use a `TimeoutInterval` property")]
		public int Timeout
		{
			get { return TimeoutInterval.Milliseconds; }
			set { TimeoutInterval = TimeSpan.FromMilliseconds(value); }
		}

		/// <summary>
		/// Gets or sets a interval to wait before the script execution times out
		/// </summary>
		public TimeSpan TimeoutInterval
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
			LocalTimeZone = TimeZoneInfo.Local;
			MaxRecursionDepth = -1;
			MaxStatements = 0;
			MemoryLimit = 0;
			RegexTimeoutInterval = null;
			StrictMode = false;
			TimeoutInterval = TimeSpan.Zero;
		}
	}
}