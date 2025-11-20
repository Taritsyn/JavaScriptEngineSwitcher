using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JavaScriptEngineSwitcher.Sample.AspNetCore31.Mvc31
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseWebRoot(Path.Combine(
						Directory.GetCurrentDirectory(),
						"../JavaScriptEngineSwitcher.Sample.AspNetCore.ClientSideAssets/wwwroot"
					));
					webBuilder.UseStartup<Startup>();
				})
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				})
				;
	}
}