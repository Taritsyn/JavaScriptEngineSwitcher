using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Mvc;

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

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var configuration = new ConfigurationBuilder()
	.SetBasePath(env.ContentRootPath)
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables()
	.Build()
	;

#region Configure services

IServiceCollection services = builder.Services;

services.AddSingleton(configuration);

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
			NoStore = builder.Environment.IsDevelopment(),
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

#endregion

#region Configure the HTTP request pipeline

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}"
);

#endregion

app.Run();