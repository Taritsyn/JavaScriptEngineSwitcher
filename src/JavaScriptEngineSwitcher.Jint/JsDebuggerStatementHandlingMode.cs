namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Handling mode for script <c>debugger</c> statements
	/// </summary>
	public enum JsDebuggerStatementHandlingMode
	{
		/// <summary>
		/// No action will be taken when encountering a <c>debugger</c> statement
		/// </summary>
		Ignore,

		/// <summary>
		/// <c>debugger</c> statements will trigger debugging through <see cref="System.Diagnostics.Debugger"/>
		/// </summary>
		Clr,

		/// <summary>
		/// <c>debugger</c> statements will trigger a break in Jint's <c>DebugHandler</c>
		/// </summary>
		/// <remarks>
		/// See the <see cref="JintSettings.DebuggerBreakCallback"/> configuration property.
		/// </remarks>
		Script
	}
}