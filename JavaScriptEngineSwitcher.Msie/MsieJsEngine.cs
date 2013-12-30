namespace JavaScriptEngineSwitcher.Msie
{
	using System;
	using System.Globalization;

	using MsieJavaScriptEngine.ActiveScript;
	using OriginalJsEngine = MsieJavaScriptEngine.MsieJsEngine;
	using OriginalUndefined = MsieJavaScriptEngine.Undefined;

	using Core;
	using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// Adapter for MSIE JavaScript engine
	/// </summary>
	public sealed class MsieJsEngine : JsEngineBase
	{
		/// <summary>
		/// MSIE JS engine
		/// </summary>
		private OriginalJsEngine _jsEngine;

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;

		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		public override string Name
		{
			get { return "MSIE JavaScript engine"; }
		}

		/// <summary>
		/// Gets a version of original JavaScript engine
		/// </summary>
		public override string Version
		{
			get { return string.Empty; }
		}


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
						Name, e.Message), e);
			}
		}


		/// <summary>
		/// Executes a mapping from the host type to a MSIE type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToMsieType(object value)
		{
			if (value is Undefined)
			{
				return OriginalUndefined.Value;
			}

			return value;
		}

		/// <summary>
		/// Executes a mapping from the MSIE type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToHostType(object value)
		{
			if (value is OriginalUndefined)
			{
				return Undefined.Value;
			}

			return value;
		}

		private JsRuntimeException ConvertActiveScriptExceptionToJsRuntimeException(
			ActiveScriptException activeScriptException)
		{
			var jsRuntimeException = new JsRuntimeException(activeScriptException.Message, 
				activeScriptException)
			{
				EngineName = Name,
				EngineVersion = Version,
				ErrorCode = activeScriptException.ErrorCode.ToString(CultureInfo.InvariantCulture),
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

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return _jsEngine.ConvertToType<T>(result);
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
			int argumentCount = args.Length;
			var processedArgs = new object[argumentCount];

			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					processedArgs[argumentIndex] = MapToMsieType(args[argumentIndex]);
				}
			}

			try
			{
				result = _jsEngine.CallFunction(functionName, processedArgs);
			}
			catch (ActiveScriptException e)
			{
				throw ConvertActiveScriptExceptionToJsRuntimeException(e);
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return _jsEngine.ConvertToType<T>(result);
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

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return _jsEngine.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToMsieType(value);

			try
			{
				_jsEngine.SetVariableValue(variableName, processedValue);
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
					_jsEngine = null;
				}
			}
		}
	}
}