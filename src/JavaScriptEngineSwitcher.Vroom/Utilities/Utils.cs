#if NETSTANDARD1_6
using System.Runtime.InteropServices;
#else
using System;
using System.Linq;
#endif

namespace JavaScriptEngineSwitcher.Vroom.Utilities
{
	internal static class Utils
	{
#if !NETSTANDARD1_6
		/// <summary>
		/// List of Windows platform identifiers
		/// </summary>
		private static readonly PlatformID[] _windowsPlatformIDs =
		{
			PlatformID.Win32NT,
			PlatformID.Win32S,
			PlatformID.Win32Windows,
			PlatformID.WinCE
		};
#endif

		/// <summary>
		/// Determines whether the current operating system is Windows
		/// </summary>
		/// <returns>true if the operating system is Windows; otherwise, false</returns>
		public static bool IsWindows()
		{
#if NETSTANDARD1_6
			bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
			bool isWindows = _windowsPlatformIDs.Contains(Environment.OSVersion.Platform);
#endif

			return isWindows;
		}
	}
}