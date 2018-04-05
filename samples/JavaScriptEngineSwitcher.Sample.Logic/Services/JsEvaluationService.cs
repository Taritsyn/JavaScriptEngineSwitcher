using System.Collections.Generic;
using System.Linq;
#if NET451 || NETSTANDARD
using Microsoft.AspNetCore.Mvc.Rendering;
#elif NET40
using System.Web.Mvc;
#else
#error No implementation for this target
#endif

using JavaScriptEngineSwitcher.Core;

using JavaScriptEngineSwitcher.Sample.Logic.Models;

namespace JavaScriptEngineSwitcher.Sample.Logic.Services
{
	public class JsEvaluationService
	{
		private static readonly Dictionary<string, string> _engineDisplayNameMappings;

		private readonly IJsEngineSwitcher _engineSwitcher;


		static JsEvaluationService()
		{
			_engineDisplayNameMappings = new Dictionary<string, string>
			{
				{ "ChakraCoreJsEngine", "ChakraCore" },
				{ "JintJsEngine", "Jint" },
				{ "JurassicJsEngine", "Jurassic" },
				{ "MsieJsEngine", "MSIE" },
				{ "V8JsEngine", "V8" }
			};
		}

#if NET40
		public JsEvaluationService()
			: this(JsEngineSwitcher.Current)
		{ }

#endif
		public JsEvaluationService(IJsEngineSwitcher engineSwitcher)
		{
			_engineSwitcher = engineSwitcher;
		}


		public JsEvaluationViewModel GetInitializationData()
		{
			var model = new JsEvaluationViewModel
			{
				EngineName = _engineSwitcher.DefaultEngineName,
				AvailableEngineList = _engineSwitcher.EngineFactories
					.Select(e => new SelectListItem
					{
						Value = e.EngineName,
						Text = GetEngineDisplayName(e.EngineName)
					}),
				Expression = string.Empty,
				Result = null
			};

			return model;
		}

		public JsEvaluationViewModel Evaluate(JsEvaluationViewModel model)
		{
			IJsEngine engine = null;
			var result = new JsEvaluationResultViewModel();

			try
			{
				engine = _engineSwitcher.CreateEngine(model.EngineName);
				result.Value = engine.Evaluate<string>(model.Expression);
			}
			catch (JsScriptException e)
			{
				var error = GetJsEvaluationErrorFromException(e);
				error.LineNumber = e.LineNumber;
				error.ColumnNumber = e.ColumnNumber;
				error.SourceFragment = e.SourceFragment;

				result.Errors.Add(error);
			}
			catch (JsException e)
			{
				var error = GetJsEvaluationErrorFromException(e);
				result.Errors.Add(error);
			}
			finally
			{
				if (engine != null)
				{
					engine.Dispose();
				}
			}

			model.Result = result;

			return model;
		}

		private static JsEvaluationErrorViewModel GetJsEvaluationErrorFromException(JsException jsException)
		{
			var jsError = new JsEvaluationErrorViewModel
			{
				EngineName = GetEngineDisplayName(jsException.EngineName),
				EngineVersion = jsException.EngineVersion,
				Message = jsException.Message
			};

			return jsError;
		}

		private static string GetEngineDisplayName(string engineName)
		{
			string displayName = engineName;
			const string postfix = "JsEngine";

			if (_engineDisplayNameMappings.ContainsKey(engineName))
			{
				displayName = _engineDisplayNameMappings[engineName];
			}
			else
			{
				if (engineName.EndsWith(postfix))
				{
					int displayNameLength = engineName.Length - postfix.Length;
					if (displayNameLength > 0)
					{
						displayName = engineName.Substring(0, displayNameLength);
					}
				}
			}

			return displayName;
		}
	}
}