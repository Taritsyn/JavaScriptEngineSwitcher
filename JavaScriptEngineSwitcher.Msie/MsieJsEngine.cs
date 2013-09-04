namespace JavaScriptEngineSwitcher.Msie
{
	using System;

	using MsieJavaScriptEngine.ActiveScript;
	using OriginalJsEngine = MsieJavaScriptEngine.MsieJsEngine;

	using Core;
	using Core.Constants;
	using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// Adapter for MSIE JavaScript engine
	/// </summary>
	public sealed class MsieJsEngine : JsEngineBase
	{
		/// <summary>
		/// MSIE JS engine
		/// </summary>
		private readonly OriginalJsEngine _jsEngine;

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;


		/// <summary>
		/// Constructs instance of adapter for MSIE JavaScript engine
		/// </summary>
		public MsieJsEngine()
		{
			try
			{
				_jsEngine = new OriginalJsEngine(true, true);
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						"MSIE JavaScript engine", e.Message), e);
			}
		}


		private static JsRuntimeException ConvertActiveScriptExceptionToJsRuntimeException(
			ActiveScriptException activeScriptException)
		{
			var jsRuntimeException = new JsRuntimeException(activeScriptException.Message, 
				activeScriptException)
			{
				EngineName = EngineName.MsieJsEngine, 
				ErrorCode = activeScriptException.ErrorCode.ToString(),
				Category = activeScriptException.Subcategory,
				LineNumber = (int)activeScriptException.LineNumber,
				ColumnNumber = activeScriptException.ColumnNumber,
				SourceFragment = activeScriptException.SourceError,
				Source = activeScriptException.Source,
				HelpLink = activeScriptException.HelpLink
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
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			T result;

			try
			{
				result = _jsEngine.Evaluate<T>(expression);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override void InnerExecute(string code)
		{
			try
			{
				_jsEngine.Execute(code);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;

			try
			{
				result = _jsEngine.CallFunction(functionName, args);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			T result;

			try
			{
				result = _jsEngine.CallFunction<T>(functionName, args);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			try
			{
				result = _jsEngine.HasVariable(variableName);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			try
			{
				result = _jsEngine.GetVariableValue(variableName);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			T result;

			try
			{
				result = _jsEngine.GetVariableValue<T>(variableName);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			try
			{
				_jsEngine.SetVariableValue(variableName, value);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				_jsEngine.RemoveVariable(variableName);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}
		}

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				if (_jsEngine != null)
				{
					_jsEngine.Dispose();
				}
			}
		}
	}
}