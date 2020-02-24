using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;
using JavaScriptEngineSwitcher.Node;
using JavaScriptEngineSwitcher.Sample.Logic.Services;
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Vroom;

namespace JavaScriptEngineSwitcher.Sample.AspNetCore31.Mvc31
{
	public class Startup
	{
		/// <summary>
		/// Gets or sets a instance of hosting environment
		/// </summary>
		public IWebHostEnvironment HostingEnvironment
		{
			get;
			set;
		}

		public IConfigurationRoot Configuration
		{
			get;
			set;
		}


		public Startup(IWebHostEnvironment env)
		{
			HostingEnvironment = env;

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(Configuration);

			// Add Jering Node.js service to the services container.
			services.AddNodeJS();

			// Add JavaScriptEngineSwitcher services to the services container.
			services.AddJsEngineSwitcher(options =>
				options.DefaultEngineName = ChakraCoreJsEngine.EngineName
			)
				.AddChakraCore()
				.AddJint()
				.AddJurassic()
				.AddMsie(options =>
				{
					options.EngineMode = JsEngineMode.ChakraIeJsRt;
				})
				.AddNiL()
				.AddNode(services)
				.AddV8()
				.AddVroom()
				;

			services.Configure<MvcOptions>(options =>
			{
				options.CacheProfiles.Add("CacheCompressedContent5Minutes",
					new CacheProfile
					{
						NoStore = HostingEnvironment.IsDevelopment(),
						Duration = 300,
						Location = ResponseCacheLocation.Client,
						VaryByHeader = "Accept-Encoding"
					}
				);
			});

			// Add framework services.
			services.AddControllersWithViews();

			// Add JavaScriptEngineSwitcher sample services to the services container.
			services.AddSingleton<JsEvaluationService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}