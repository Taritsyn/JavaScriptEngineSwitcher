#if NET40
using System;

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.Core.Polyfills.System.Runtime.InteropServices
{
	public static class RuntimeInformation
	{
		/// <summary>
		/// Operating system platform
		/// </summary>
		private static OSPlatform _osPlatform;

		/// <summary>
		/// Operating system architecture
		/// </summary>
		private static Architecture _osArch;

		public static Architecture OSArchitecture
		{
			get { return _osArch; }
		}


		/// <summary>
		/// Static constructor
		/// </summary>
		static RuntimeInformation()
		{
			PlatformID platform = Environment.OSVersion.Platform;

			if (platform == PlatformID.Win32NT || platform == PlatformID.Win32S
				|| platform == PlatformID.Win32Windows || platform == PlatformID.WinCE)
			{
				_osPlatform = OSPlatform.Windows;
			}
			else if (platform == PlatformID.MacOSX)
			{
				_osPlatform = OSPlatform.OSX;
			}
			else if (platform == PlatformID.Unix)
			{
				string unixName = Utils.ReadProcessOutput("uname") ?? string.Empty;
				if (unixName.Contains("Darwin"))
				{
					_osPlatform = OSPlatform.OSX;
				}
				else
				{
					_osPlatform = OSPlatform.Linux;
				}
			}
			else
			{
				_osPlatform = OSPlatform.Create("UNKNOWN");
			}

			_osArch = Environment.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
		}


		public static bool IsOSPlatform(OSPlatform osPlatform)
		{
			return osPlatform == _osPlatform;
		}
	}
}
#endif