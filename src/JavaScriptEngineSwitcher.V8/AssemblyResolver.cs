using System;
using System.IO;
using System.Reflection;

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
				string binDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
				if (string.IsNullOrEmpty(binDirectoryPath))
				{
					// `PrivateBinPath` property is empty in test scenarios, so
					// need to use the `BaseDirectory` property
					binDirectoryPath = currentDomain.BaseDirectory;
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

				string assemblyDirectoryPath = Path.Combine(binDirectoryPath, platformName);
				string assemblyFileName = string.Format("{0}-{1}.dll", ASSEMBLY_NAME, platformBitness);
				string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);

				if (!Directory.Exists(assemblyDirectoryPath))
				{
					throw new DirectoryNotFoundException(
						string.Format(Strings.Engines_ClearScriptV8AssembliesDirectoryNotFound, assemblyDirectoryPath));
				}

				if (!File.Exists(assemblyFilePath))
				{
					throw new FileNotFoundException(
						string.Format(Strings.Engines_ClearScriptV8AssemblyFileNotFound, assemblyFilePath));
				}

				return Assembly.LoadFile(assemblyFilePath);
			}

			return null;
		}
	}
}