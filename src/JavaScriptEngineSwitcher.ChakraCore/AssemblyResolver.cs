using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.ChakraCore.Resources;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// Assembly resolver
	/// </summary>
	internal static class AssemblyResolver
	{
		/// <summary>
		/// Name of directory, that contains the ChakraCore assemblies
		/// </summary>
		private const string ASSEMBLY_DIRECTORY_NAME = "ChakraCore";

		/// <summary>
		/// Regular expression for working with the `bin` directory path
		/// </summary>
		private static readonly Regex _binDirectoryRegex = new Regex(@"\\bin\\?$", RegexOptions.IgnoreCase);


		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

		/// <summary>
		/// Initialize a assembly resolver
		/// </summary>
		public static void Initialize()
		{
			var currentDomain = AppDomain.CurrentDomain;
			string platform = Environment.Is64BitProcess ? "x64" : "x86";

			string binDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
			if (string.IsNullOrEmpty(binDirectoryPath))
			{
				// `PrivateBinPath` property is empty in test scenarios, so
				// need to use the `BaseDirectory` property
				binDirectoryPath = currentDomain.BaseDirectory;
			}

			string assemblyDirectoryPath = Path.Combine(binDirectoryPath, ASSEMBLY_DIRECTORY_NAME, platform);

			if (!Directory.Exists(assemblyDirectoryPath))
			{
				if (_binDirectoryRegex.IsMatch(binDirectoryPath))
				{
					string applicationRootPath = _binDirectoryRegex.Replace(binDirectoryPath, string.Empty);
					assemblyDirectoryPath = Path.Combine(applicationRootPath, ASSEMBLY_DIRECTORY_NAME, platform);

					if (!Directory.Exists(assemblyDirectoryPath))
					{
						throw new DirectoryNotFoundException(
							string.Format(Strings.Engines_ChakraCoreAssemblyDirectoryNotFound, assemblyDirectoryPath));
					}
				}
				else
				{
					throw new DirectoryNotFoundException(
						string.Format(Strings.Engines_ChakraCoreAssemblyDirectoryNotFound, assemblyDirectoryPath));
				}
			}

			if (!SetDllDirectory(assemblyDirectoryPath))
			{
				throw new InvalidOperationException(
					string.Format(Strings.Engines_AddingDirectoryToDllSearchPathFailed, assemblyDirectoryPath));
			}
		}
	}
}