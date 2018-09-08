using System.Runtime.InteropServices;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Default callback for finalization of external buffer
	/// </summary>
	internal static class DefaultExternalBufferFinalizeCallback
	{
		/// <summary>
		/// Gets a instance of default callback for finalization of external buffer
		/// </summary>
		public static readonly JsObjectFinalizeCallback Instance = Marshal.FreeHGlobal;
	}
}