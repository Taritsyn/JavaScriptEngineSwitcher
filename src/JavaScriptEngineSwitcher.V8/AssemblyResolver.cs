using System;
using System.IO;
using System.Reflection;

using JavaScriptEngineSwitcher.Core.Helpers;

using JavaScriptEngineSwitcher.V8.Resources;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Assembly resolver
	/// </summary>
	internal static class AssemblyResolver
	{
		/// <summary>
		/// Name of the ClearScriptV8 assembly
		/// </summary>
		private const string ASSEMBLY_NAME = "ClearScriptV8";


		/// <summary>
		/// Initialize a assembly resolver
		/// </summary>
		public static void Initialize()
		{
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveHandler;
		}

		private static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
		{
			if (args.Name.StartsWith(ASSEMBLY_NAME, StringComparison.OrdinalIgnoreCase))
			{
				var currentDomain = (AppDomain)sender;
				string baseDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
				if (string.IsNullOrEmpty(baseDirectoryPath))
				{
					// `PrivateBinPath` property is empty in test scenarios, so
					// need to use the `BaseDirectory` property
					baseDirectoryPath = currentDomain.BaseDirectory;
				}

				string platformName;
				int platformBitness;
				if (Environment.Is64BitProcess)
				{
					platformName = "x64";
					platformBitness = 64;
				}
				else
				{
					platformName = "x86";
					platformBitness = 32;
				}

				string assemblyDirectoryPath = Path.Combine(baseDirectoryPath, platformName);
				string assemblyFileName = string.Format("{0}-{1}.dll", ASSEMBLY_NAME, platformBitness);
				string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
				bool assemblyFileExists = File.Exists(assemblyFilePath);

				if (!assemblyFileExists)
				{
					string projectDirectoryPath = PathHelpers.RemoveDirectoryFromPath(baseDirectoryPath, "bin");
					string solutionDirectoryPath = Path.GetFullPath(Path.Combine(projectDirectoryPath, "../../"));
					assemblyDirectoryPath = Path.GetFullPath(
						Path.Combine(solutionDirectoryPath, "Binaries/ClearScript/", platformName));
					assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
					assemblyFileExists = File.Exists(assemblyFilePath);
				}


				if (!assemblyFileExists)
				{
					throw new FileNotFoundException(
						string.Format(Strings.Engines_ClearScriptV8AssemblyFileNotFound, assemblyFileName));
				}

				return Assembly.LoadFile(assemblyFilePath);
			}

			return null;
		}
	}
}