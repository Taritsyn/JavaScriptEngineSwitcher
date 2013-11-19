namespace JavaScriptEngineSwitcher.V8
{
	using System;
	using System.Collections.Generic;
	using System.Web.Script.Serialization;

	using Noesis.Javascript;

	using Core;
	using Core.Constants;
	using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// Adapter for Noesis Javascript .NET
	/// </summary>
	public sealed class V8JsEngine : JsEngineBase
	{
		/// <summary>
		/// Full name of JavaScript engine
		/// </summary>
		private const string JS_ENGINE_FULL_NAME = "V8 JavaScript engine";

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

		/// <summary>
		/// JS-context
		/// </summary>
		private readonly JavascriptContext _jsContext;

		/// <summary>
		/// JS-serializer
		/// </summary>
		private readonly JavaScriptSerializer _jsSerializer;

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;


		static V8JsEngine()
		{
			try
			{
				AssemblyResolver.Initialize();
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						JS_ENGINE_FULL_NAME, e.Message), e);
			}
		}

		/// <summary>
		/// Constructs instance of adapter for Noesis Javascript .NET
		/// </summary>
		public V8JsEngine()
		{
			try
			{
				_jsContext = new JavascriptContext();
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						JS_ENGINE_FULL_NAME, e.Message), e);
			}

			_jsSerializer = new JavaScriptSerializer();
		}

		private static JsRuntimeException ConvertJavascriptExceptionToJsRuntimeException(
			JavascriptException jsException)
		{
			var jsRuntimeException = new JsRuntimeException(jsException.Message, jsException)
			{
				EngineName = EngineName.V8JsEngine,
				LineNumber = jsException.Line,
				ColumnNumber = jsException.StartColumn + 1,
				Source = jsException.Source,
				HelpLink = jsException.HelpLink
			};

			return jsRuntimeException;
		}

		/// <summary>
		/// Converts a given value to the specified type
		/// </summary>
		/// <typeparam name="T">The type to which value will be converted</typeparam>
		/// <param name="value">The value to convert</param>
		/// <returns>The value that has been converted to the target type</returns>
		private T ConvertToType<T>(object value)
		{
			return (T)ConvertToType(value, typeof(T));
		}

		/// <summary>
		/// Converts a specified value to the specified type
		/// </summary>
		/// <param name="value">The value to convert</param>
		/// <param name="targetType">The type to convert the value to</param>
		/// <returns>The value that has been converted to the target type</returns>
		private object ConvertToType(object value, Type targetType)
		{
			object result = _jsSerializer.ConvertToType(value, targetType);

			return result;
		}

		protected override object InnerEvaluate(string expression)
		{
			const string resultingParameterName = "result";
			string processedExpression = expression.TrimEnd();
			if (processedExpression.EndsWith(";"))
			{
				processedExpression = processedExpression.TrimEnd(';');
			}

			object result;

			lock(_executionSynchronizer)
			{
				try
				{
					_jsContext.Run(string.Format("var {0} = {1};", resultingParameterName, processedExpression));
					result = _jsContext.GetParameter(resultingParameterName);
				}
				catch (JavascriptException e)
				{
					throw ConvertJavascriptExceptionToJsRuntimeException(e);
				}
			}

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			return ConvertToType<T>(InnerEvaluate(expression));
		}

		protected override void InnerExecute(string code)
		{
			lock (_executionSynchronizer)
			{
				try
				{
					_jsContext.Run(code);
				}
				catch (JavascriptException e)
				{
					throw ConvertJavascriptExceptionToJsRuntimeException(e);
				}
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;
			const string resultingParameterName = "result";
			int argumentCount = args.Length;

			if (argumentCount > 0)
			{
				var parameters = new List<string>();

				lock (_executionSynchronizer)
				{
					try
					{
						for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
						{
							string parameterName = string.Format("param{0}", argumentIndex + 1);
							object argument = args[argumentIndex];

							_jsContext.SetParameter(parameterName, argument);
							parameters.Add(parameterName);
						}

						_jsContext.Run(string.Format("var {0} = {1}({2});", resultingParameterName, 
							functionName, string.Join(", ", parameters)));
						result = _jsContext.GetParameter(resultingParameterName);
					}
					catch (JavascriptException e)
					{
						throw ConvertJavascriptExceptionToJsRuntimeException(e);
					}
				}
			}
			else
			{
				lock (_executionSynchronizer)
				{
					try
					{
						_jsContext.Run(string.Format("var {0} = {1}();", resultingParameterName, functionName));
						result = _jsContext.GetParameter(resultingParameterName);
					}
					catch (JavascriptException e)
					{
						throw ConvertJavascriptExceptionToJsRuntimeException(e);
					}
				}
			}


			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			return ConvertToType<T>(InnerCallFunction(functionName, args));
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

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsContext.GetParameter(variableName);
				}
				catch (JavascriptException e)
				{
					throw ConvertJavascriptExceptionToJsRuntimeException(e);
				}
			}

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			return ConvertToType<T>(InnerGetVariableValue(variableName));
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			lock (_executionSynchronizer)
			{
				try
				{
					_jsContext.SetParameter(variableName, value);
				}
				catch (JavascriptException e)
				{
					throw ConvertJavascriptExceptionToJsRuntimeException(e);
				}
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
			if (!_disposed)
			{
				_disposed = true;

				if (_jsContext != null)
				{
					_jsContext.Dispose();
				}
			}
		}
	}
}