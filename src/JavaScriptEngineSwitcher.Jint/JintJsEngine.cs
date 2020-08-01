using System;
using System.Text;
using System.Threading;

using Jint;
using IOriginalPrimitiveInstance = Jint.Native.IPrimitiveInstance;
using OriginalEngine = Jint.Engine;
using OriginalJavaScriptException = Jint.Runtime.JavaScriptException;
using OriginalMemoryLimitExceededException = Jint.Runtime.MemoryLimitExceededException;
using OriginalObjectInstance = Jint.Native.Object.ObjectInstance;
using OriginalParsedScript = Esprima.Ast.Script;
using OriginalParser = Esprima.JavaScriptParser;
using OriginalParserException = Esprima.ParserException;
using OriginalParserOptions = Esprima.ParserOptions;
using OriginalRecursionDepthOverflowException = Jint.Runtime.RecursionDepthOverflowException;
using OriginalRuntimeException = Jint.Runtime.JintException;
using OriginalStatementsCountOverflowException = Jint.Runtime.StatementsCountOverflowException;
using OriginalTypeReference = Jint.Runtime.Interop.TypeReference;
using OriginalTypes = Jint.Runtime.Types;
using OriginalValue = Jint.Native.JsValue;

using AdvancedStringBuilder;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperInterruptedException = JavaScriptEngineSwitcher.Core.JsInterruptedException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperTimeoutException = JavaScriptEngineSwitcher.Core.JsTimeoutException;
using WrapperUsageException = JavaScriptEngineSwitcher.Core.JsUsageException;

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
		private const string EngineVersion = "3.0.0 Beta 1828";

		/// <summary>
		/// Jint JS engine
		/// </summary>
		private OriginalEngine _jsEngine;

		/// <summary>
		/// Token source for canceling of script execution
		/// </summary>
		private CancellationTokenSource _cancellationTokenSource;

		/// <summary>
		/// Constraint for canceling of script execution
		/// </summary>
		private CustomCancellationConstraint _cancellationConstraint;

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
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationConstraint = new CustomCancellationConstraint(_cancellationTokenSource.Token);

			JintSettings jintSettings = settings ?? new JintSettings();

			try
			{
				_jsEngine = new OriginalEngine(options => {
					options
						.AllowDebuggerStatement(jintSettings.AllowDebuggerStatement)
						.Constraint(_cancellationConstraint)
						.DebugMode(jintSettings.EnableDebugging)
						.LimitMemory(jintSettings.MemoryLimit)
						.LimitRecursion(jintSettings.MaxRecursionDepth)
						.LocalTimeZone(jintSettings.LocalTimeZone ?? TimeZoneInfo.Local)
						.MaxStatements(jintSettings.MaxStatements)
						.Strict(jintSettings.StrictMode)
						.TimeoutInterval(jintSettings.TimeoutInterval)
						;

					if (jintSettings.RegexTimeoutInterval.HasValue)
					{
						options.RegexTimeoutInterval(jintSettings.RegexTimeoutInterval.Value);
					}

					options.AddObjectConverter(new UndefinedConverter());
				});
			}
			catch (Exception e)
			{
				throw JsErrorHelpers.WrapEngineLoadException(e, EngineName, EngineVersion, true);
			}
		}


		/// <summary>
		/// Creates a Esprima .NET parser options with document name and special settings for the Jint
		/// </summary>
		/// <param name="documentName">Document name</param>
		/// <returns>Esprima .NET parser options with document name and special settings for the Jint</returns>
		private static OriginalParserOptions CreateParserOptions(string documentName)
		{
			var parserOptions = new OriginalParserOptions(documentName)
			{
				AdaptRegexp = true,
				Tolerant = true,
				Loc = true
			};

			return parserOptions;
		}

		#region Mapping

		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private OriginalValue MapToScriptType(object value)
		{
			return OriginalValue.FromObject(_jsEngine, value);
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private object MapToHostType(OriginalValue value)
		{
			switch (value.Type)
			{
				case OriginalTypes.Undefined:
					return Undefined.Value;

				case OriginalTypes.Object:
					if (!(value is IOriginalPrimitiveInstance))
					{
						return value;
					}
					else
					{
						break;
					}
			}

			return value.ToObject();
		}

		private static WrapperCompilationException WrapParserException(OriginalParserException originalParserException)
		{
			string description = originalParserException.Description;
			string type = JsErrorType.Syntax;
			string documentName = originalParserException.SourceText;
			int lineNumber = originalParserException.LineNumber;
			int columnNumber = originalParserException.Column;
			string message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName, lineNumber,
				columnNumber);

			var wrapperCompilationException = new WrapperCompilationException(message, EngineName, EngineVersion,
				originalParserException)
			{
				Description = description,
				Type = type,
				DocumentName = documentName,
				LineNumber = lineNumber,
				ColumnNumber = columnNumber
			};

			return wrapperCompilationException;
		}

		private WrapperRuntimeException WrapRuntimeException(OriginalRuntimeException originalRuntimeException)
		{
			WrapperRuntimeException wrapperRuntimeException;
			string message = originalRuntimeException.Message;
			if (string.IsNullOrWhiteSpace(message))
			{
				message = "An unknown error occurred";
			}
			string description = message;
			string type = string.Empty;
			string documentName = string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			string callStack = string.Empty;

			if (originalRuntimeException is OriginalJavaScriptException)
			{
				var originalJavaScriptException = (OriginalJavaScriptException)originalRuntimeException;
				documentName = originalJavaScriptException.Location.Source;
				lineNumber = originalJavaScriptException.LineNumber;
				columnNumber = originalJavaScriptException.Column + 1;

				OriginalValue errorValue = originalJavaScriptException.Error;
				if (errorValue.IsObject())
				{
					OriginalObjectInstance errorObject = errorValue.AsObject();

					OriginalValue namePropertyValue = errorObject.Get("name");
					if (namePropertyValue.IsString())
					{
						type = namePropertyValue.AsString();
					}
				}

				if (string.IsNullOrEmpty(type))
				{
					type = JsErrorType.Common;
				}

				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName, lineNumber,
					columnNumber);

				wrapperRuntimeException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalJavaScriptException);
			}
			else if (originalRuntimeException is OriginalMemoryLimitExceededException)
			{
				type = JsErrorType.Common;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty);

				wrapperRuntimeException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalRuntimeException);
			}
			else if (originalRuntimeException is OriginalRecursionDepthOverflowException)
			{
				var originalRecursionException = (OriginalRecursionDepthOverflowException)originalRuntimeException;
				string[] callChainItems = originalRecursionException.CallChain
					.Split(new string[] { "->" }, StringSplitOptions.None)
					;

				if (callChainItems.Length > 0)
				{
					var stringBuilderPool = StringBuilderPool.Shared;
					StringBuilder stackBuilder = stringBuilderPool.Rent();

					for (int chainItemIndex = callChainItems.Length - 1; chainItemIndex >= 0; chainItemIndex--)
					{
						string chainItem = callChainItems[chainItemIndex];
						if (chainItem == "anonymous function")
						{
							chainItem = "Anonymous function";
						}

						JsErrorHelpers.WriteErrorLocationLine(stackBuilder, chainItem, string.Empty, 0, 0);
						if (chainItemIndex > 0)
						{
							stackBuilder.AppendLine();
						}
					}

					callStack = stackBuilder.ToString();
					stringBuilderPool.Return(stackBuilder);
				}

				type = JsErrorType.Range;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, callStack);

				wrapperRuntimeException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalRecursionException);
			}
			else if (originalRuntimeException is OriginalStatementsCountOverflowException)
			{
				type = JsErrorType.Range;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty);

				wrapperRuntimeException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalRuntimeException);
			}
			else if (originalRuntimeException is ScriptExecutionCanceledException)
			{
				_cancellationTokenSource = new CancellationTokenSource();
				_cancellationConstraint.Reset(_cancellationTokenSource.Token);

				type = JsErrorType.Common;
				message = CoreStrings.Runtime_ScriptInterrupted;
				description = message;

				wrapperRuntimeException = new WrapperInterruptedException(message,
					EngineName, EngineVersion, originalRuntimeException);
			}
			else
			{
				type = JsErrorType.Common;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty);

				wrapperRuntimeException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalRuntimeException);
			}

			wrapperRuntimeException.Description = description;
			wrapperRuntimeException.Type = type;
			wrapperRuntimeException.DocumentName = documentName;
			wrapperRuntimeException.LineNumber = lineNumber;
			wrapperRuntimeException.ColumnNumber = columnNumber;
			wrapperRuntimeException.CallStack = callStack;

			return wrapperRuntimeException;
		}

		private static WrapperTimeoutException WrapTimeoutException(TimeoutException originalTimeoutException)
		{
			string message = CoreStrings.Runtime_ScriptTimeoutExceeded;
			string description = message;

			var wrapperTimeoutException = new WrapperTimeoutException(message, EngineName, EngineVersion,
				originalTimeoutException)
			{
				Description = description
			};

			return wrapperTimeoutException;
		}

		#endregion

		#region JsEngineBase overrides

		protected override IPrecompiledScript InnerPrecompile(string code)
		{
			return InnerPrecompile(code, null);
		}

		protected override IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			OriginalParsedScript parsedScript;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);
			OriginalParserOptions parserOptions = CreateParserOptions(uniqueDocumentName);

			try
			{
				var parser = new OriginalParser(code, parserOptions);
				parsedScript = parser.ParseScript();
			}
			catch (OriginalParserException e)
			{
				throw WrapParserException(e);
			}

			return new JintPrecompiledScript(parsedScript);
		}

		protected override object InnerEvaluate(string expression)
		{
			return InnerEvaluate(expression, null);
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			object result;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);
			OriginalParserOptions parserOptions = CreateParserOptions(uniqueDocumentName);

			lock (_executionSynchronizer)
			{
				OriginalValue resultValue;

				try
				{
					resultValue = _jsEngine.Execute(expression, parserOptions).GetCompletionValue();
				}
				catch (OriginalParserException e)
				{
					throw WrapParserException(e);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
				}
				catch (TimeoutException e)
				{
					throw WrapTimeoutException(e);
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
			OriginalParserOptions parserOptions = CreateParserOptions(uniqueDocumentName);

			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Execute(code, parserOptions);
				}
				catch (OriginalParserException e)
				{
					throw WrapParserException(e);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
				}
				catch (TimeoutException e)
				{
					throw WrapTimeoutException(e);
				}
			}
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			var jintPrecompiledScript = precompiledScript as JintPrecompiledScript;
			if (jintPrecompiledScript == null)
			{
				throw new WrapperUsageException(
					string.Format(CoreStrings.Usage_CannotConvertPrecompiledScriptToInternalType,
						typeof(JintPrecompiledScript).FullName),
					Name, Version
				);
			}

			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Execute(jintPrecompiledScript.ParsedScript);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
				}
				catch (TimeoutException e)
				{
					throw WrapTimeoutException(e);
				}
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;

			lock (_executionSynchronizer)
			{
				OriginalValue functionValue;

				try
				{
					functionValue = _jsEngine.GetValue(functionName);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
				}

				OriginalValue resultValue;

				try
				{
					resultValue = _jsEngine.Invoke(functionValue, args);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
				}
				catch (TimeoutException e)
				{
					throw WrapTimeoutException(e);
				}
				catch (ArgumentException e) when (e.Message == "Can only invoke functions")
				{
					throw new WrapperRuntimeException(
						string.Format(CoreStrings.Runtime_FunctionNotExist, functionName));
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
					OriginalValue variableValue = _jsEngine.GetValue(variableName);
					result = !variableValue.IsUndefined();
				}
				catch (OriginalJavaScriptException)
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
				OriginalValue variableValue;

				try
				{
					variableValue = _jsEngine.GetValue(variableName);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
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
				OriginalValue processedValue = MapToScriptType(value);

				try
				{
					_jsEngine.SetValue(variableName, processedValue);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
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
				OriginalValue processedValue = MapToScriptType(value);

				try
				{
					_jsEngine.SetValue(itemName, processedValue);
				}
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
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
				catch (OriginalRuntimeException e)
				{
					throw WrapRuntimeException(e);
				}
			}
		}

		protected override void InnerInterrupt()
		{
			_cancellationTokenSource.Cancel();
		}

		protected override void InnerCollectGarbage()
		{
			throw new NotSupportedException();
		}

		#region IJsEngine implementation

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
		/// Gets a value that indicates if the JS engine supports script pre-compilation
		/// </summary>
		public override bool SupportsScriptPrecompilation
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports script interruption
		/// </summary>
		public override bool SupportsScriptInterruption
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports garbage collection
		/// </summary>
		public override bool SupportsGarbageCollection
		{
			get { return false; }
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
					_cancellationConstraint = null;

					if (_cancellationTokenSource != null)
					{
						_cancellationTokenSource.Dispose();
						_cancellationTokenSource = null;
					}
				}
			}
		}

		#endregion

		#endregion
	}
}