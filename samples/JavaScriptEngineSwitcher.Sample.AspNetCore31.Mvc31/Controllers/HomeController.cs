using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using JavaScriptEngineSwitcher.Sample.AspNetCore31.Mvc31.Models;
using JavaScriptEngineSwitcher.Sample.Logic.Models;
using JavaScriptEngineSwitcher.Sample.Logic.Services;

namespace JavaScriptEngineSwitcher.Sample.AspNetCore31.Mvc31.Controllers
{
	public class HomeController : Controller
	{
		private readonly FileContentService _fileContentService;
		private readonly JsEvaluationService _jsEvaluationService;


		public HomeController(
			IConfigurationRoot configuration,
			IWebHostEnvironment hostingEnvironment,
			JsEvaluationService jsEvaluationService)
		{
			string textContentDirectoryPath = configuration
				.GetSection("jsengineswitcher")
				.GetSection("Samples")["TextContentDirectoryPath"]
				;

			_fileContentService = new FileContentService(textContentDirectoryPath, hostingEnvironment);
			_jsEvaluationService = jsEvaluationService;
		}


		[ResponseCache(CacheProfileName = "CacheCompressedContent5Minutes")]
		public IActionResult Index()
		{
			ViewBag.Body = new HtmlString(_fileContentService.GetFileContent("index.html"));

			return View();
		}

		[HttpGet]
		[ResponseCache(CacheProfileName = "CacheCompressedContent5Minutes")]
		public IActionResult Demo()
		{
			var model = _jsEvaluationService.GetInitializationData();

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Demo(JsEvaluationViewModel editedModel)
		{
			var model = _jsEvaluationService.GetInitializationData();
			await TryUpdateModelAsync(model, string.Empty, m => m.EngineName, m=> m.Expression);

			if (ModelState.IsValid)
			{
				model = _jsEvaluationService.Evaluate(model);

				ModelState.Clear();
			}

			return View(model);
		}

		[ResponseCache(CacheProfileName = "CacheCompressedContent5Minutes")]
		public IActionResult Contact()
		{
			ViewBag.Body = new HtmlString(_fileContentService.GetFileContent("contact.html"));

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}