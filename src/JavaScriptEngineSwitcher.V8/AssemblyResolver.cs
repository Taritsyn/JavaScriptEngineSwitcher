using System;
using System.IO;
using System.Reflection;

using OriginalEngine = Microsoft.ClearScript.V8.V8ScriptEngine;

using JavaScriptEngineSwitcher.V8.Constants;
using JavaScriptEngineSwitcher.V8.Resources;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Assembly resolver
	/// </summary>
	internal static class AssemblyResolver
	{
		/// <summary>
		/// Initialize a assembly resolver
		/// </summary>
		public static void Initialize()
		{
			var currentDomain = AppDomain.CurrentDomain;
#if NETFULL
			string baseDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
			if (string.IsNullOrEmpty(baseDirectoryPath))
			{
				// `PrivateBinPath` property is empty in test scenarios, so
				// need to use the `BaseDirectory` property
				baseDirectoryPath = currentDomain.BaseDirectory;
			}
#else
			string baseDirectoryPath = currentDomain.BaseDirectory;
#endif

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

			string assemblyName = DllName.ClearScriptV8Universal + "-" + platformBitness.ToString();
			string assemblyDirectoryPath = Path.Combine(baseDirectoryPath, platformName);
			string assemblyFileName = assemblyName + ".dll";
			string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
			bool assemblyFileExists = File.Exists(assemblyFilePath);

			if (assemblyFileExists)
			{
				if (!SetDeploymentDir(platformName))
				{
					throw new InvalidOperationException(
						string.Format(Strings.Engines_SettingDeploymentDirectoryToV8ProxyFailed, platformName));
				}
			}
		}

		/// <summary>
		/// Sets a deployment directory name to V8 proxy
		/// </summary>
		/// <param name="directoryName">Deployment directory name</param>
		/// <returns>Result of operation (true - success; false - fail)</returns>
		private static bool SetDeploymentDir(string directoryName)
		{
			Assembly clearScriptAssembly = typeof(OriginalEngine).Assembly;
			Type v8ProxyType = clearScriptAssembly.GetType("Microsoft.ClearScript.V8.V8Proxy");

			bool success = true;

			try
			{
				FieldInfo deploymentDirNameFieldInfo = v8ProxyType.GetField("deploymentDirName",
					BindingFlags.NonPublic | BindingFlags.Static);
				deploymentDirNameFieldInfo.SetValue(null, directoryName);
			}
			catch
			{
				success = false;
			}

			return success;
		}
	}
}