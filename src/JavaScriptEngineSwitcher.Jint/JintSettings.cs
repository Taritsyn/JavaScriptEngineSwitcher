using System;

using OriginalDebuggerEventHandler = Jint.Runtime.Debugger.DebugHandler.DebugEventHandler;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Settings of the Jint JS engine
	/// </summary>
	public sealed class JintSettings
	{
		/// <summary>
		/// Gets or sets a flag for whether to allow the usage of reflection API in the script code
		/// </summary>
		/// <remarks>
		/// This affects <see cref="Object.GetType"/> and <see cref="Exception.GetType"/>.
		/// </remarks>
		public bool AllowReflection
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to allow the <c>debugger</c> statement
		/// to be called in a script
		/// </summary>
		[Obsolete("Use a `DebuggerStatementHandlingMode` property")]
		public bool AllowDebuggerStatement
		{
			get { return DebuggerStatementHandlingMode == JsDebuggerStatementHandlingMode.Clr; }
			set { DebuggerStatementHandlingMode = value ? JsDebuggerStatementHandlingMode.Clr : JsDebuggerStatementHandlingMode.Ignore; }
		}

		/// <summary>
		/// Gets or sets a debugger break callback
		/// </summary>
		public OriginalDebuggerEventHandler DebuggerBreakCallback
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a handling mode for script <c>debugger</c> statements
		/// </summary>
		public JsDebuggerStatementHandlingMode DebuggerStatementHandlingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a debugger step callback
		/// </summary>
		public OriginalDebuggerEventHandler DebuggerStepCallback
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable calls of <c>eval</c> function with custom code
		/// and <c>Function</c> constructors taking function code as string
		/// </summary>
		public bool DisableEval
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
		/// Gets or sets a local time zone for the <c>Date</c> objects in the script
		/// </summary>
		public TimeZoneInfo LocalTimeZone
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size for JavaScript array
		/// </summary>
		public uint MaxArraySize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum allowed depth of recursion:
		///    <c>-1</c> - recursion without limits;
		///     <c>N</c> - one scope function can be called no more than <c>N</c> times.
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
		/// Gets or sets a timeout interval for regular expressions
		/// </summary>
		/// <remarks>
		/// If the value of this property is <c>null</c>, then the value of regular expression
		/// timeout interval are taken from the <see cref="TimeoutInterval"/> property.
		/// </remarks>
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
			AllowReflection = false;
			DebuggerBreakCallback = null;
			DebuggerStatementHandlingMode = JsDebuggerStatementHandlingMode.Ignore;
			DebuggerStepCallback = null;
			DisableEval = false;
			EnableDebugging = false;
			LocalTimeZone = TimeZoneInfo.Local;
			MaxArraySize = uint.MaxValue;
			MaxRecursionDepth = -1;
			MaxStatements = 0;
			MemoryLimit = 0;
			RegexTimeoutInterval = null;
			StrictMode = false;
			TimeoutInterval = TimeSpan.Zero;
		}
	}
}