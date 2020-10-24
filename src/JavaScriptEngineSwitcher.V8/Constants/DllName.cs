namespace JavaScriptEngineSwitcher.V8.Constants
{
	/// <summary>
	/// DLL names
	/// </summary>
	internal static class DllName
	{
		public const string Universal = "ClearScriptV8";
		public const string ForWindows = Universal + ".dll";
		public const string ForLinux = Universal + ".so";
		public const string ForOsx = Universal + ".dylib";
	}
}