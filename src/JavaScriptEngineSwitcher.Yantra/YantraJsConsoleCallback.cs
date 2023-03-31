namespace JavaScriptEngineSwitcher.Yantra
{
	/// <summary>
	/// The JS debugging console callback
	/// </summary>
	/// <param name="type">Type of message</param>
	/// <param name="args">A array of objects to output</param>
	public delegate void YantraJsConsoleCallback(string type, object[] args);
}