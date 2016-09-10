using System;
using System.IO;
using System.Runtime.InteropServices;
#if NETSTANDARD1_3

using Microsoft.Extensions.PlatformAbstractions;
#endif

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
			bool is64BitProcess = Utils.Is64BitProcess();
			string baseDirectoryPath;

#if NETSTANDARD1_3
			baseDirectoryPath = PlatformServices.Default.Application.ApplicationBasePath;
#else
			AppDomain currentDomain = AppDomain.CurrentDomain;
			baseDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
			if (string.IsNullOrEmpty(baseDirectoryPath))
			{
				// `PrivateBinPath` property is empty in test scenarios, so
				// need to use the `BaseDirectory` property
				baseDirectoryPath = currentDomain.BaseDirectory;
			}
#endif

			if (!PathHelpers.ContainsDirectory(baseDirectoryPath, "bin"))
			{
				return;
			}

			string platform = is64BitProcess ? "x64" : "x86";
			string assemblyFileName = ASSEMBLY_NAME + ".dll";
			string assemblyDirectoryPath = Path.Combine(baseDirectoryPath, platform);
			string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
			bool assemblyFileExists = File.Exists(assemblyFilePath);

			if (!assemblyFileExists)
			{
				string projectDirectoryPath = PathHelpers.RemoveDirectoryFromPath(baseDirectoryPath, "bin");
				string solutionDirectoryPath = Path.GetFullPath(Path.Combine(projectDirectoryPath, "../../"));
				assemblyDirectoryPath = Path.GetFullPath(
					Path.Combine(solutionDirectoryPath, "lib/ChakraCore/", platform));
				assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
				assemblyFileExists = File.Exists(assemblyFilePath);
			}

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
	}
}