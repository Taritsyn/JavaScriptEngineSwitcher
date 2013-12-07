using OriginalJsEngine = Jurassic.ScriptEngine;
using OriginalConcatenatedString = Jurassic.ConcatenatedString;
using OriginalNull = Jurassic.Null;
using OriginalUndefined = Jurassic.Undefined;
using OriginalCompatibilityMode = Jurassic.CompatibilityMode;
using OriginJsException = Jurassic.JavaScriptException;

namespace JavaScriptEngineSwitcher.Jurassic
{
	using System;

	using Core;
	using Core.Constants;
	using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// Adapter for Jurassic JavaScript engine
	/// </summary>
	public sealed class JurassicJsEngine : JsEngineBase
	{
		/// <summary>
		/// Jurassic JS engine
		/// </summary>
		private OriginalJsEngine _jsEngine;


		/// <summary>
		/// Constructs instance of adapter for Jurassic
		/// </summary>
		public JurassicJsEngine()
		{
			try
			{
				_jsEngine = new OriginalJsEngine
				{
					CompatibilityMode = OriginalCompatibilityMode.Latest
				};
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						"Jurassic JavaScript engine", e.Message), e);
			}
		}


		private static object FixJurassicTypes(object value)
		{
			var result = value;
			if (value is OriginalConcatenatedString)
			{
				result = result.ToString();
			}
			else if (value is OriginalNull || value is OriginalUndefined)
			{
				result = null;
			}

			return result;
		}

		private static JsRuntimeException ConvertJavascriptExceptionToJsRuntimeException(
			OriginJsException jsException)
		{
			var jsRuntimeException = new JsRuntimeException(jsException.Message, jsException)
			{
				EngineName = EngineName.JurassicJsEngine,
				Category = jsException.Name,
				LineNumber = jsException.LineNumber,
				ColumnNumber = 0,
				SourceFragment = jsException.SourcePath,
				Source = jsException.Source,
				HelpLink = jsException.HelpLink
			};

			return jsRuntimeException;
		}

		protected override object InnerEvaluate(string expression)
		{
			object result;

			try
			{
				result = _jsEngine.Evaluate(expression);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			result = FixJurassicTypes(result);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			T result;

			try
			{
				result = _jsEngine.Evaluate<T>(expression);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override void InnerExecute(string code)
		{
			try
			{
				_jsEngine.Execute(code);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;

			try
			{
				result = _jsEngine.CallGlobalFunction(functionName, args);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			result = FixJurassicTypes(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			T result;

			try
			{
				result = _jsEngine.CallGlobalFunction<T>(functionName, args);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override bool InnerHasVariable(string variableName)
		{
			string expression = string.Format("(typeof {0} !== 'undefined');", variableName);
			var result = InnerEvaluate<bool>(expression);

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			try
			{
				result = _jsEngine.GetGlobalValue(variableName);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			result = FixJurassicTypes(result);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			T result;

			try
			{
				result = _jsEngine.GetGlobalValue<T>(variableName);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			try
			{
				_jsEngine.SetGlobalValue(variableName, value);
			}
			catch (OriginJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			string code = string.Format(@"if (typeof {0} !== 'undefined') {{
	{0} = undefined;
}}", variableName);

			InnerExecute(code);
		}

		public override void Dispose()
		{
			_jsEngine = null;
		}
	}
}