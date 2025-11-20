using System.IO;

using Microsoft.AspNetCore.Hosting;

namespace JavaScriptEngineSwitcher.Sample.AspNetCore1.Mvc1
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string currentDirectory = Directory.GetCurrentDirectory();
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(currentDirectory)
				.UseWebRoot(Path.Combine(
					currentDirectory,
					"../JavaScriptEngineSwitcher.Sample.AspNetCore.ClientSideAssets/wwwroot"
				))
				.UseIISIntegration()
				.UseStartup<Startup>()
				.Build()
				;

			host.Run();
		}
	}
}