namespace JavaScriptEngineSwitcher.Vroom.Constants
{
	/// <summary>
	/// DLL names
	/// </summary>
	internal static class DllName
	{
		public const string Universal = "VroomJsNative";
		public const string ForWindows = Universal + ".dll";
		public const string ForUnix = "lib" + Universal + ".so";
	}
}