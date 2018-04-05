namespace JavaScriptEngineSwitcher.ChakraCore.Constants
{
	/// <summary>
	/// DLL names
	/// </summary>
	internal static class DllName
	{
		public const string Universal = "ChakraCore";
		public const string ForWindows = Universal + ".dll";
		public const string ForLinux = "lib" + Universal + ".so";
		public const string ForOsx = "lib" + Universal + ".dylib";
	}
}
