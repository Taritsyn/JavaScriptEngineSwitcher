using System.Configuration;
using System.Web.Mvc;

using JavaScriptEngineSwitcher.Sample.Logic.Services;

namespace JavaScriptEngineSwitcher.Sample.AspNet4.Mvc4.Controllers
{
	public class HomeController : Controller
	{
		private readonly FileContentService _fileContentService;
		private readonly JsEvaluationService _jsEvaluationService;


		public HomeController()
			: this(
				new FileContentService(ConfigurationManager.AppSettings["jsengineswitcher:Samples:TextContentDirectoryPath"]),
				new JsEvaluationService()
			)
		{ }

		public HomeController(FileContentService fileContentService, JsEvaluationService jsEvaluationService)
		{
			_fileContentService = fileContentService;
			_jsEvaluationService = jsEvaluationService;
		}


		public ActionResult Index()
		{
			ViewBag.Body = _fileContentService.GetFileContent("index.html");

			return View();
		}

		[HttpGet]
		public ActionResult Demo()
		{
			var model = _jsEvaluationService.GetInitializationData();

			return View(model);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Demo(FormCollection collection)
		{
			var model = _jsEvaluationService.GetInitializationData();

			TryUpdateModel(model, new[] { "EngineName", "Expression" }, collection);

			if (ModelState.IsValid)
			{
				model = _jsEvaluationService.Evaluate(model);

				ModelState.Clear();
			}

			return View(model);
		}

		public ActionResult Contact()
		{
			ViewBag.Body = _fileContentService.GetFileContent("contact.html");

			return View();
		}
	}
}