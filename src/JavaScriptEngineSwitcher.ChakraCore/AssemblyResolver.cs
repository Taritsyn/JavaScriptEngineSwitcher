#if !NETSTANDARD
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using JavaScriptEngineSwitcher.ChakraCore.Resources;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// Assembly resolver
	/// </summary>
	internal static class AssemblyResolver
	{
		/// <summary>
		/// Name of the ChakraCore assembly
		/// </summary>
		private const string ASSEMBLY_NAME = "ChakraCore";

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);


		/// <summary>
		/// Initialize a assembly resolver
		/// </summary>
		public static void Initialize()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			string baseDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
			if (string.IsNullOrEmpty(baseDirectoryPath))
			{
				// `PrivateBinPath` property is empty in test scenarios, so
				// need to use the `BaseDirectory` property
				baseDirectoryPath = currentDomain.BaseDirectory;
			}

			if (!PathHelpers.ContainsDirectory(baseDirectoryPath, "bin"))
			{
				return;
			}

			string platform;
			if (IsArmProcessorArchitecture())
			{
				platform = "arm";
			}
			else
			{
				platform = Utils.Is64BitProcess() ? "x64" : "x86";
			}

			string assemblyFileName = ASSEMBLY_NAME + ".dll";
			string assemblyDirectoryPath = Path.Combine(baseDirectoryPath, platform);
			string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
			bool assemblyFileExists = File.Exists(assemblyFilePath);

			if (!assemblyFileExists)
			{
				return;
			}

			if (!SetDllDirectory(assemblyDirectoryPath))
			{
				throw new InvalidOperationException(
					string.Format(Strings.Engines_AddingDirectoryToDllSearchPathFailed, assemblyDirectoryPath));
			}
		}

		/// <summary>
		/// Determines whether the current processor architecture is a ARM
		/// </summary>
		/// <returns>true if the processor architecture is ARM; otherwise, false</returns>
		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private static bool IsArmProcessorArchitecture()
		{
			PortableExecutableKinds peKind;
			ImageFileMachine machine;

			typeof(object).Module.GetPEKind(out peKind, out machine);
			bool isArm = (int)machine == 452 /* ImageFileMachine.ARM */;

			return isArm;
		}
	}
}
#endif