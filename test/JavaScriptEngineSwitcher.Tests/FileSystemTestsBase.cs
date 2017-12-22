#if NET452 || NETCOREAPP
using Microsoft.Extensions.PlatformAbstractions;
#elif NET40
using System;
#else
#error No implementation for this target
#endif
using System.IO;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class FileSystemTestsBase : TestsBase
	{
		protected string _baseDirectoryPath;


		protected FileSystemTestsBase()
		{
#if NET452 || NETCOREAPP
			var appEnv = PlatformServices.Default.Application;
			_baseDirectoryPath = Path.Combine(appEnv.ApplicationBasePath, "../../../");
#elif NET40
			_baseDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../");
#else
#error No implementation for this target
#endif
		}
	}
}