#if NETSTANDARD1_6 || NET451
using System.Runtime.InteropServices;
#elif NET40
using System;
using System.Linq;
#else
#error No implementation for this target
#endif

namespace JavaScriptEngineSwitcher.Vroom.Utilities
{
	internal static class Utils
	{
#if NET40
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
#if NETSTANDARD1_6 || NET451
			bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#elif NET40
			bool isWindows = _windowsPlatformIDs.Contains(Environment.OSVersion.Platform);
#else
#error No implementation for this target
#endif

			return isWindows;
		}
	}
}