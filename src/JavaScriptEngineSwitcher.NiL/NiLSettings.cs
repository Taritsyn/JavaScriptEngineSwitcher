using System;

using OriginalDebuggerCallback = NiL.JS.Core.DebuggerCallback;

namespace JavaScriptEngineSwitcher.NiL
{
	/// <summary>
	/// Settings of the NiL JS engine
	/// </summary>
	public sealed class NiLSettings
	{
		/// <summary>
		/// Gets or sets a debugger callback
		/// </summary>
		public OriginalDebuggerCallback DebuggerCallback
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
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
		/// Gets or sets a flag for whether to allow run the script in strict mode
		/// </summary>
		public bool StrictMode
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the NiL settings
		/// </summary>
		public NiLSettings()
		{
			DebuggerCallback = null;
			EnableDebugging = false;
			LocalTimeZone = TimeZoneInfo.Local;
			StrictMode = false;
		}
	}
}