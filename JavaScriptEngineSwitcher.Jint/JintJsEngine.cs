using IOriginalCallable = Jint.Native.ICallable;
using OriginalJsEngine = Jint.Engine;
using OriginalJsException = Jint.Runtime.JavaScriptException;
using OriginalJsValue = Jint.Native.JsValue;
using OriginalObjectInstance = Jint.Native.Object.ObjectInstance;
using OriginalParserException = Jint.Parser.ParserException;
using OriginalRecursionDepthOverflowException = Jint.Runtime.RecursionDepthOverflowException;
using OriginalStatementsCountOverflowException = Jint.Runtime.StatementsCountOverflowException;

namespace JavaScriptEngineSwitcher.Jint
{
	using System;

	using Core;
	using Core.Utilities;
	using CoreStrings = Core.Resources.Strings;

	using Configuration;
	using Resources;

	/// <summary>
	/// Adapter for Jint JavaScript engine
	/// </summary>
	public sealed class JintJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JavaScript engine
		/// </summary>
		private const string ENGINE_NAME = "Jint JavaScript engine";

		/// <summary>
		/// Version of original JavaScript engine
		/// </summary>
		private const string ENGINE_VERSION = "Mar 29, 2015";

		/// <summary>
		/// Jint JS engine
		/// </summary>
		private OriginalJsEngine _jsEngine;

		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		public override string Name
		{
			get { return ENGINE_NAME; }
		}

		/// <summary>
		/// Gets a version of original JavaScript engine
		/// </summary>
		public override string Version
		{
			get { return ENGINE_VERSION; }
		}


		/// <summary>
		/// Constructs instance of adapter for Jint
		/// </summary>
		public JintJsEngine()
			: this(JsEngineSwitcher.Current.GetJintConfiguration())
		{ }

		/// <summary>
		/// Constructs instance of adapter for Jint
		/// </summary>
		/// <param name="jintConfig">Configuration settings of Jint JavaScript engine</param>
		public JintJsEngine(JintConfiguration jintConfig)
		{
			try
			{
				_jsEngine = new OriginalJsEngine(config => config
					.AllowDebuggerStatement(jintConfig.EnableDebugging)
					.LimitRecursion(jintConfig.MaxRecursionDepth)
					.MaxStatements(jintConfig.MaxStatements)
					.Strict(jintConfig.StrictMode)
					.TimeoutInterval(TimeSpan.FromMilliseconds(jintConfig.Timeout))
				);
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						ENGINE_NAME, e.Message), ENGINE_NAME, ENGINE_VERSION, e);
			}
		}

		#region JsEngineBase implementation

		/// <summary>
		/// Executes a mapping from the host type to a Jint type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private OriginalJsValue MapToJintType(object value)
		{
			if (value is Undefined)
			{
				return OriginalJsValue.Undefined;
			}

			return OriginalJsValue.FromObject(_jsEngine, value);
		}

		/// <summary>
		/// Executes a mapping from the Jint type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private object MapToHostType(OriginalJsValue value)
		{
			if (value.IsUndefined())
			{
				return Undefined.Value;
			}

			return value.ToObject();
		}

		private JsRuntimeException ConvertParserExceptionToJsRuntimeException(
			OriginalParserException jsParserException)
		{
			string message = jsParserException.Description;
			if (string.IsNullOrWhiteSpace(message))
			{
				message = jsParserException.Message;
			}

			var jsRuntimeException = new JsRuntimeException(message, ENGINE_NAME, ENGINE_VERSION,
				jsParserException)
			{
				Category = "ParserError",
				LineNumber = jsParserException.LineNumber,
				ColumnNumber = jsParserException.Column,
				Source = jsParserException.Source,
				HelpLink = jsParserException.HelpLink
			};

			return jsRuntimeException;
		}

		private JsRuntimeException ConvertJavaScriptExceptionToJsRuntimeException(
			OriginalJsException jsException)
		{
			string category = string.Empty;
			OriginalJsValue errorValue = jsException.Error;

			if (errorValue.IsObject())
			{
				OriginalObjectInstance errorObject = errorValue.AsObject();
				OriginalJsValue categoryPropertyValue = errorObject.Get("name");

				if (categoryPropertyValue.IsString())
				{
					category = categoryPropertyValue.AsString();
				}
			}

			var jsRuntimeException = new JsRuntimeException(jsException.Message, ENGINE_NAME, ENGINE_VERSION,
				jsException)
			{
				Category = category,
				Source = jsException.Source,
				HelpLink = jsException.HelpLink
			};

			return jsRuntimeException;
		}

		private JsRuntimeException ConvertRecursionDepthOverflowExceptionToJsRuntimeException(
			OriginalRecursionDepthOverflowException jsRecursionException)
		{
			string message = string.Format(Strings.Runtime_RecursionDepthOverflow,
				jsRecursionException.CallChain);

			var jsRuntimeException = new JsRuntimeException(message,
				ENGINE_NAME, ENGINE_VERSION, jsRecursionException)
			{
				Category = "RecursionDepthOverflowError",
				Source = jsRecursionException.Source,
				HelpLink = jsRecursionException.HelpLink
			};

			return jsRuntimeException;
		}

		private JsRuntimeException ConvertStatementsCountOverflowExceptionToJsRuntimeException(
			OriginalStatementsCountOverflowException jsStatementsException)
		{
			var jsRuntimeException = new JsRuntimeException(Strings.Runtime_StatementsCountOverflow,
				ENGINE_NAME, ENGINE_VERSION)
			{
				Category = "StatementsCountOverflowError",
				Source = jsStatementsException.Source,
				HelpLink = jsStatementsException.HelpLink
			};

			return jsRuntimeException;
		}

		private JsRuntimeException ConvertTimeoutExceptionToJsRuntimeException(
			TimeoutException jsTimeoutException)
		{
			var jsRuntimeException = new JsRuntimeException(Strings.Runtime_ExecutionTimeout,
				ENGINE_NAME, ENGINE_VERSION)
			{
				Category = "TimeoutError",
				Source = jsTimeoutException.Source,
				HelpLink = jsTimeoutException.HelpLink
			};

			return jsRuntimeException;
		}

		protected override object InnerEvaluate(string expression)
		{
			OriginalJsValue resultValue;

			try
			{
				resultValue = _jsEngine.Execute(expression).GetCompletionValue();
			}
			catch (OriginalParserException e)
			{
				throw ConvertParserExceptionToJsRuntimeException(e);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavaScriptExceptionToJsRuntimeException(e);
			}
			catch (OriginalRecursionDepthOverflowException e)
			{
				throw ConvertRecursionDepthOverflowExceptionToJsRuntimeException(e);
			}
			catch (OriginalStatementsCountOverflowException e)
			{
				throw ConvertStatementsCountOverflowExceptionToJsRuntimeException(e);
			}
			catch (TimeoutException e)
			{
				throw ConvertTimeoutExceptionToJsRuntimeException(e);
			}

			object result = MapToHostType(resultValue);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			try
			{
				_jsEngine.Execute(code);
			}
			catch (OriginalParserException e)
			{
				throw ConvertParserExceptionToJsRuntimeException(e);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavaScriptExceptionToJsRuntimeException(e);
			}
			catch (OriginalRecursionDepthOverflowException e)
			{
				throw ConvertRecursionDepthOverflowExceptionToJsRuntimeException(e);
			}
			catch (OriginalStatementsCountOverflowException e)
			{
				throw ConvertStatementsCountOverflowExceptionToJsRuntimeException(e);
			}
			catch (TimeoutException e)
			{
				throw ConvertTimeoutExceptionToJsRuntimeException(e);
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			OriginalJsValue functionValue;

			try
			{
				functionValue = _jsEngine.GetValue(functionName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavaScriptExceptionToJsRuntimeException(e);
			}

			var callable = functionValue.TryCast<IOriginalCallable>();
			if (callable == null)
			{
				throw new JsRuntimeException(
					string.Format(CoreStrings.Runtime_FunctionNotExist, functionName));
			}

			int argumentCount = args.Length;
			var processedArgs = new OriginalJsValue[argumentCount];

			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					processedArgs[argumentIndex] = MapToJintType(args[argumentIndex]);
				}
			}

			OriginalJsValue resultValue;

			try
			{
				resultValue = callable.Call(functionValue, processedArgs);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavaScriptExceptionToJsRuntimeException(e);
			}
			catch (OriginalRecursionDepthOverflowException e)
			{
				throw ConvertRecursionDepthOverflowExceptionToJsRuntimeException(e);
			}
			catch (OriginalStatementsCountOverflowException e)
			{
				throw ConvertStatementsCountOverflowExceptionToJsRuntimeException(e);
			}
			catch (TimeoutException e)
			{
				throw ConvertTimeoutExceptionToJsRuntimeException(e);
			}

			object result = MapToHostType(resultValue);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			try
			{
				OriginalJsValue variableValue = _jsEngine.GetValue(variableName);
				result = !variableValue.IsUndefined();
			}
			catch (OriginalJsException)
			{
				result = false;
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			OriginalJsValue variableValue;

			try
			{
				variableValue = _jsEngine.GetValue(variableName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavaScriptExceptionToJsRuntimeException(e);
			}

			object result = MapToHostType(variableValue);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			OriginalJsValue processedValue = MapToJintType(value);

			try
			{
				_jsEngine.SetValue(variableName, processedValue);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavaScriptExceptionToJsRuntimeException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InnerSetVariableValue(variableName, Undefined.Value);
		}

		#endregion

		#region IDisposable implementation

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				_jsEngine = null;
			}
		}

		#endregion
	}
}