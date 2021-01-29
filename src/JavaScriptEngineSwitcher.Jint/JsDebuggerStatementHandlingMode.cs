namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Handling mode for script <code>debugger</code> statements
	/// </summary>
	public enum JsDebuggerStatementHandlingMode
	{
		/// <summary>
		/// No action will be taken when encountering a <code>debugger</code> statement
		/// </summary>
		Ignore,

		/// <summary>
		/// <code>debugger</code> statements will trigger debugging through <see cref="System.Diagnostics.Debugger" />
		/// </summary>
		Clr,

		/// <summary>
		/// <code>debugger</code> statements will trigger a break in Jint's DebugHandler.
		/// See the <see cref="JintSettings.DebuggerBreakCallback" /> configuration property.
		/// </summary>
		Script
	}
}