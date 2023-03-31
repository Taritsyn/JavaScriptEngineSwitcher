using OriginalDebugger = YantraJS.Debugger.JSDebugger;
using OriginalV8Debugger = YantraJS.Core.Debugger.V8Debugger;

namespace JavaScriptEngineSwitcher.Yantra
{
	/// <summary>
	/// Settings of the Yantra JS engine
	/// </summary>
	public sealed class YantraSettings
	{
		/// <summary>
		/// Gets or sets a JS debugging console callback
		/// </summary>
		public YantraJsConsoleCallback ConsoleCallback
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets an instance of JS debugger (for example, the <see cref="OriginalV8Debugger"/>)
		/// </summary>
		public OriginalDebugger Debugger
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the Yantra settings
		/// </summary>
		public YantraSettings()
		{
			ConsoleCallback = null;
			Debugger = null;
		}
	}
}