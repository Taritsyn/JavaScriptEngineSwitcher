using System;
using System.Text;

using IOriginalCallable = Jint.Native.ICallable;
using OriginalJsEngine = Jint.Engine;
using OriginalJsException = Jint.Runtime.JavaScriptException;
using OriginalJsValue = Jint.Native.JsValue;
using OriginalObjectInstance = Jint.Native.Object.ObjectInstance;
using OriginalParserException = Jint.Parser.ParserException;
using OriginalParserOptions = Jint.Parser.ParserOptions;
using OriginalRecursionDepthOverflowException = Jint.Runtime.RecursionDepthOverflowException;
using OriginalStatementsCountOverflowException = Jint.Runtime.StatementsCountOverflowException;
using OriginalTypeReference = Jint.Runtime.Interop.TypeReference;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

using JavaScriptEngineSwitcher.Jint.Resources;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Adapter for the Jint JS engine
	/// </summary>
	public sealed class JintJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "JintJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "2.11.58";

		/// <summary>
		/// Jint JS engine
		/// </summary>
		private OriginalJsEngine _jsEngine;

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

		/// <summary>
		/// Unique document name manager
		/// </summary>
		private readonly UniqueDocumentNameManager _documentNameManager =
			new UniqueDocumentNameManager(DefaultDocumentName);

		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		public override string Name
		{
			get { return EngineName; }
		}

		/// <summary>
		/// Gets a version of original JS engine
		/// </summary>
		public override string Version
		{
			get { return EngineVersion; }
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports garbage collection
		/// </summary>
		public override bool SupportsGarbageCollection
		{
			get { return false; }
		}


		/// <summary>
		/// Constructs an instance of adapter for the Jint JS engine
		/// </summary>
		public JintJsEngine()
			: this(new JintSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the Jint JS engine
		/// </summary>
		/// <param name="settings">Settings of the Jint JS engine</param>
		public JintJsEngine(JintSettings settings)
		{
			JintSettings jintSettings = settings ?? new JintSettings();

			try
			{
				_jsEngine = new OriginalJsEngine(c => c
					.AllowDebuggerStatement(jintSettings.AllowDebuggerStatement)
					.DebugMode(jintSettings.EnableDebugging)
					.LimitRecursion(jintSettings.MaxRecursionDepth)
					.MaxStatements(jintSettings.MaxStatements)
					.Strict(jintSettings.StrictMode)
					.TimeoutInterval(TimeSpan.FromMilliseconds(jintSettings.Timeout))
				);
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						EngineName, e.Message), EngineName, EngineVersion, e);
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
			const string category = "ParserError";
			string description = jsParserException.Description;
			int lineNumber = jsParserException.LineNumber;
			int columnNumber = jsParserException.Column;
			string message = !string.IsNullOrWhiteSpace(description) ?
				GenerateErrorMessageWithLocation(category, description, jsParserException.Source,
					lineNumber, columnNumber)
				:
				jsParserException.Message
				;

			var jsRuntimeException = new JsRuntimeException(message, EngineName, EngineVersion,
				jsParserException)
			{
				Category = category,
				LineNumber = lineNumber,
				ColumnNumber = columnNumber
			};

			return jsRuntimeException;
		}

		private JsRuntimeException ConvertJavaScriptExceptionToJsRuntimeException(
			OriginalJsException jsException)
		{
			string category = string.Empty;
			int lineNumber = jsException.LineNumber;
			int columnNumber = jsException.Column + 1;
			string message = jsException.Message;
			OriginalJsValue errorValue = jsException.Error;

			if (errorValue.IsObject())
			{
				OriginalObjectInstance errorObject = errorValue.AsObject();
				OriginalJsValue categoryPropertyValue = errorObject.Get("name");

				if (categoryPropertyValue.IsString())
				{
					category = categoryPropertyValue.AsString();
				}

				message = GenerateErrorMessageWithLocation(category, message,
					jsException.Location.Source, lineNumber, columnNumber);
			}

			var jsRuntimeException = new JsRuntimeException(message, EngineName, EngineVersion,
				jsException)
			{
				Category = category,
				LineNumber = lineNumber,
				ColumnNumber = columnNumber
			};

			return jsRuntimeException;
		}

		private static string GenerateErrorMessageWithLocation(string category, string message,
			string documentName, int lineNumber, int columnNumber)
		{
			var messageBuilder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(category))
			{
				messageBuilder.AppendFormat("{0}: ", category);
			}
			messageBuilder.Append(message);
			if (!string.IsNullOrWhiteSpace(documentName))
			{
				messageBuilder.AppendLine();
				messageBuilder.AppendFormat("    at {0}", documentName);
				if (lineNumber > 0)
				{
					messageBuilder.AppendFormat(":{0}", lineNumber);
					if (columnNumber > 0)
					{
						messageBuilder.AppendFormat(":{0}", columnNumber);
					}
				}
			}

			string errorMessage = messageBuilder.ToString();
			messageBuilder.Clear();

			return errorMessage;
		}

		private JsRuntimeException ConvertRecursionDepthOverflowExceptionToJsRuntimeException(
			OriginalRecursionDepthOverflowException jsRecursionException)
		{
			string message = string.Format(Strings.Runtime_RecursionDepthOverflow,
				jsRecursionException.CallChain);

			var jsRuntimeException = new JsRuntimeException(message, EngineName, EngineVersion,
				jsRecursionException)
			{
				Category = "RecursionDepthOverflowError"
			};

			return jsRuntimeException;
		}

		private JsRuntimeException ConvertStatementsCountOverflowExceptionToJsRuntimeException(
			OriginalStatementsCountOverflowException jsStatementsException)
		{
			var jsRuntimeException = new JsRuntimeException(Strings.Runtime_StatementsCountOverflow,
				EngineName, EngineVersion, jsStatementsException)
			{
				Category = "StatementsCountOverflowError"
			};

			return jsRuntimeException;
		}

		private JsRuntimeException ConvertTimeoutExceptionToJsRuntimeException(
			TimeoutException jsTimeoutException)
		{
			var jsRuntimeException = new JsRuntimeException(Strings.Runtime_ExecutionTimeout,
				EngineName, EngineVersion)
			{
				Category = "TimeoutError",
				Source = jsTimeoutException.Source,
				HelpLink = jsTimeoutException.HelpLink
			};

			return jsRuntimeException;
		}

		protected override object InnerEvaluate(string expression)
		{
			return InnerEvaluate(expression, null);
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			object result;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			lock (_executionSynchronizer)
			{
				OriginalJsValue resultValue;

				try
				{
					var parserOptions = new OriginalParserOptions
					{
						Source = uniqueDocumentName
					};
					resultValue = _jsEngine.Execute(expression, parserOptions).GetCompletionValue();
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

				result = MapToHostType(resultValue);
			}

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			return InnerEvaluate<T>(expression, null);
		}

		protected override T InnerEvaluate<T>(string expression, string documentName)
		{
			object result = InnerEvaluate(expression, documentName);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			InnerExecute(code, null);
		}

		protected override void InnerExecute(string code, string documentName)
		{
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			lock (_executionSynchronizer)
			{
				try
				{
					var parserOptions = new OriginalParserOptions
					{
						Source = uniqueDocumentName
					};
					_jsEngine.Execute(code, parserOptions);
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
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;

			lock (_executionSynchronizer)
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

				result = MapToHostType(resultValue);
			}

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

			lock (_executionSynchronizer)
			{
				try
				{
					OriginalJsValue variableValue = _jsEngine.GetValue(variableName);
					result = !variableValue.IsUndefined();
				}
				catch (OriginalJsException)
				{
					result = false;
				}
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			lock (_executionSynchronizer)
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

				result = MapToHostType(variableValue);
			}

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			lock (_executionSynchronizer)
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
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InnerSetVariableValue(variableName, Undefined.Value);
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			lock (_executionSynchronizer)
			{
				OriginalJsValue processedValue = MapToJintType(value);

				try
				{
					_jsEngine.SetValue(itemName, processedValue);
				}
				catch (OriginalJsException e)
				{
					throw ConvertJavaScriptExceptionToJsRuntimeException(e);
				}
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			lock (_executionSynchronizer)
			{
				OriginalTypeReference typeReference = OriginalTypeReference.CreateTypeReference(_jsEngine, type);

				try
				{
					_jsEngine.SetValue(itemName, typeReference);
				}
				catch (OriginalJsException e)
				{
					throw ConvertJavaScriptExceptionToJsRuntimeException(e);
				}
			}
		}

		protected override void InnerCollectGarbage()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDisposable implementation

		public override void Dispose()
		{
			if (_disposedFlag.Set())
			{
				lock (_executionSynchronizer)
				{
					_jsEngine = null;
				}
			}
		}

		#endregion
	}
}