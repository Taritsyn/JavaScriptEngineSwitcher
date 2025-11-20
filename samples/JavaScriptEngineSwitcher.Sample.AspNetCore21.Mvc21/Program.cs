using System.IO;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace JavaScriptEngineSwitcher.Sample.AspNetCore21.Mvc21
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseWebRoot(Path.Combine(
					Directory.GetCurrentDirectory(),
					"../JavaScriptEngineSwitcher.Sample.AspNetCore.ClientSideAssets/wwwroot"
				))
				.UseStartup<Startup>()
				;
	}
}