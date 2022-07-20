using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;

namespace TestAspNetFilization.Controllers
{
	public class HomeController : Controller
	{
		private static IJsEngine _firstEngine;
		private static IJsEngine _secondEngine;
		private static IJsEngine _thirdEngine;


		static HomeController()
		{
			IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;

			_firstEngine = engineSwitcher.CreateDefaultEngine();
			_secondEngine = engineSwitcher.CreateDefaultEngine();
			_thirdEngine = engineSwitcher.CreateDefaultEngine();
		}

		public ActionResult Index()
		{
			_firstEngine.Execute(@"function declensionOfNumerals(number, titles) {
	var result,
		titleIndex,
		cases = [2, 0, 1, 1, 1, 2],
		caseIndex
		;

	if (number % 100 > 4 && number % 100 < 20) {
		titleIndex = 2;
	}
	else {
		caseIndex = number % 10 < 5 ? number % 10 : 5;
		titleIndex = cases[caseIndex];
	}

	result = titles[titleIndex];

	return result;
}

function declinationOfSeconds(number) {
	return declensionOfNumerals(number, ['секунда', 'секунды', 'секунд']);
}");
			ViewBag.Title = _secondEngine.Evaluate<string>("1 + 2 * 8 / 77");
			_thirdEngine.SetVariableValue("qwerty", "onlime");

			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}