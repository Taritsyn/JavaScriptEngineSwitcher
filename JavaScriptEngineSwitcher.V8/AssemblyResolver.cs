namespace JavaScriptEngineSwitcher.V8
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Web;

	using Resources;

	/// <summary>
	/// Assembly resolver
	/// </summary>
	internal static class AssemblyResolver
	{
		/// <summary>
		/// Name of directory, that contains the Noesis Javascript .NET assemblies
		/// </summary>
		private const string ASSEMBLY_DIRECTORY_NAME = "Noesis.Javascript";
		
		/// <summary>
		/// Name of the Noesis Javascript .NET assembly
		/// </summary>
		private const string ASSEMBLY_NAME = "Noesis.Javascript";


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
				string platform = Environment.Is64BitProcess ? "x64" : "x86";

				string binDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
				if (string.IsNullOrEmpty(binDirectoryPath))
				{
					// `PrivateBinPath` property is empty in test scenarios, so 
					// need to use the `BaseDirectory` property
					binDirectoryPath = currentDomain.BaseDirectory;
				}

				string assemblyDirectoryPath = Path.Combine(binDirectoryPath, ASSEMBLY_DIRECTORY_NAME);
				string assemblyFileName = string.Format("{0}.{1}.dll", ASSEMBLY_NAME, platform);
				string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);

				if (!Directory.Exists(assemblyDirectoryPath))
				{
					if (HttpContext.Current != null)
					{
						// Fix for WebMatrix
						string applicationRootPath = HttpContext.Current.Server.MapPath("~");
						assemblyDirectoryPath = Path.Combine(applicationRootPath, ASSEMBLY_DIRECTORY_NAME);

						if (!Directory.Exists(assemblyDirectoryPath))
						{
							throw new DirectoryNotFoundException(
								string.Format(Strings.Engines_NoesisJavascriptAssembliesDirectoryNotFound, assemblyDirectoryPath));
						}

						assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
					}
					else
					{
						throw new DirectoryNotFoundException(
							string.Format(Strings.Engines_NoesisJavascriptAssembliesDirectoryNotFound, assemblyDirectoryPath));
					}
				}

				if (!File.Exists(assemblyFilePath))
				{
					throw new FileNotFoundException(
						string.Format(Strings.Engines_NoesisJavascriptAssemblyFileNotFound, assemblyFilePath));
				}

				return Assembly.LoadFile(assemblyFilePath);
			}

			return null;
		}
	}
}