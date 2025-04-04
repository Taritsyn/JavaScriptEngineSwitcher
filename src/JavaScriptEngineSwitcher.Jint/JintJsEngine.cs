using System;
using System.Threading;

using Jint;
using IOriginalPrimitive = Jint.Native.IJsPrimitive;
using OriginalCancellationConstraint = Jint.Constraints.CancellationConstraint;
using OriginalDebuggerEventHandler = Jint.Runtime.Debugger.DebugHandler.DebugEventHandler;
using OriginalDebuggerStatementHandlingMode = Jint.Runtime.Debugger.DebuggerStatementHandling;
using OriginalEngine = Jint.Engine;
using OriginalErrorPosition = Acornima.Position;
using OriginalExecutionCanceledException = Jint.Runtime.ExecutionCanceledException;
using OriginalException = Jint.JintException;
using OriginalJavaScriptException = Jint.Runtime.JavaScriptException;
using OriginalMemoryLimitExceededException = Jint.Runtime.MemoryLimitExceededException;
using OriginalObjectInstance = Jint.Native.Object.ObjectInstance;
using OriginalParsedScript = Jint.Prepared<Acornima.Ast.Script>;
using OriginalRecursionDepthOverflowException = Jint.Runtime.RecursionDepthOverflowException;
using OriginalScriptPreparationException = Jint.ScriptPreparationException;
using OriginalStatementsCountOverflowException = Jint.Runtime.StatementsCountOverflowException;
using OriginalTypeReference = Jint.Runtime.Interop.TypeReference;
using OriginalTypes = Jint.Runtime.Types;
using OriginalValue = Jint.Native.JsValue;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperInterruptedException = JavaScriptEngineSwitcher.Core.JsInterruptedException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;
using WrapperTimeoutException = JavaScriptEngineSwitcher.Core.JsTimeoutException;
using WrapperUsageException = JavaScriptEngineSwitcher.Core.JsUsageException;

using JavaScriptEngineSwitcher.Jint.Helpers;

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
		private const string EngineVersion = "4.2.2";

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
		private OriginalCancellationConstraint _cancellationConstraint;

		/// <summary>
		/// Debugger break callback
		/// </summary>
		private OriginalDebuggerEventHandler _debuggerBreakCallback;

		/// <summary>
		/// Debugger step callback
		/// </summary>
		private OriginalDebuggerEventHandler _debuggerStepCallback;

		/// <summary>
		/// Flag for whether to allow run the script in strict mode
		/// </summary>
		private bool _strictMode;

		/// <summary>
		/// Synchronizer of script execution
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

			JintSettings jintSettings = settings ?? new JintSettings();
			_debuggerBreakCallback = jintSettings.DebuggerBreakCallback;
			_debuggerStepCallback = jintSettings.DebuggerStepCallback;
			var debuggerStatementHandlingMode = Utils.GetEnumFromOtherEnum<JsDebuggerStatementHandlingMode, OriginalDebuggerStatementHandlingMode>(
				jintSettings.DebuggerStatementHandlingMode);

			try
			{
				_jsEngine = new OriginalEngine(options => {
					options.Interop.AllowGetType = true;
					options.Interop.AllowSystemReflection = true;

					options
						.CancellationToken(_cancellationTokenSource.Token)
						.DebuggerStatementHandling(debuggerStatementHandlingMode)
						.DebugMode(jintSettings.EnableDebugging)
						.DisableStringCompilation(jintSettings.DisableEval)
						.LimitMemory(jintSettings.MemoryLimit)
						.LimitRecursion(jintSettings.MaxRecursionDepth)
						.LocalTimeZone(jintSettings.LocalTimeZone ?? TimeZoneInfo.Local)
						.MaxArraySize(jintSettings.MaxArraySize)
						.MaxJsonParseDepth(jintSettings.MaxJsonParseDepth)
						.MaxStatements(jintSettings.MaxStatements)
						.Strict(jintSettings.StrictMode)
						.TimeoutInterval(jintSettings.TimeoutInterval)
						;

					if (jintSettings.RegexTimeoutInterval.HasValue)
					{
						options.RegexTimeoutInterval(jintSettings.RegexTimeoutInterval.Value);
					}

					options.AddObjectConverter(new UndefinedConverter());
					options.SetTypeResolver(jintSettings.AllowReflection ?
						CustomTypeResolvers.AllowingReflection : CustomTypeResolvers.DisallowingReflection);
				});
				_cancellationConstraint = _jsEngine.Constraints.Find<OriginalCancellationConstraint>();
				if (_debuggerBreakCallback != null)
				{
					_jsEngine.Debugger.Break += _debuggerBreakCallback;
				}
				if (_debuggerStepCallback != null)
				{
					_jsEngine.Debugger.Step += _debuggerStepCallback;
				}
				_strictMode = settings.StrictMode;
			}
			catch (Exception e)
			{
				throw JsErrorHelpers.WrapEngineLoadException(e, EngineName, EngineVersion, true);
			}
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
					if (!(value is IOriginalPrimitive))
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

		private WrapperScriptException WrapException(OriginalException originalException)
		{
			WrapperScriptException wrapperScriptException;
			string message = originalException.Message;
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

			if (originalException is OriginalJavaScriptException)
			{
				var originalJavaScriptException = (OriginalJavaScriptException)originalException;
				documentName = originalJavaScriptException.Location.SourceFile;
				OriginalErrorPosition errorPosition = originalJavaScriptException.Location.Start;
				lineNumber = errorPosition.Line;
				columnNumber = errorPosition.Column + 1;

				ErrorLocationItem[] callStackItems = JintJsErrorHelpers.ParseErrorLocation(
					originalJavaScriptException.JavaScriptStackTrace);
				if (callStackItems.Length > 0)
				{
					JintJsErrorHelpers.FixErrorLocationItems(callStackItems);
					callStack = JsErrorHelpers.StringifyErrorLocationItems(callStackItems, true);
				}

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

				if (type == JsErrorType.Syntax)
				{
					description = JintJsErrorHelpers.GetDescriptionFromSyntaxErrorMessage(message);
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName, lineNumber,
						columnNumber);
					wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
						originalJavaScriptException);
				}
				else
				{
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, callStack);
					wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
						originalJavaScriptException)
					{
						CallStack = callStack
					};
				}
			}
			else if (originalException is OriginalScriptPreparationException)
			{
				ErrorLocationItem errorLocation;
				JintJsErrorHelpers.ParseScriptPreparationErrorMessage(message, out description, out errorLocation);

				type = JsErrorType.Syntax;
				documentName = errorLocation.DocumentName;
				lineNumber = errorLocation.LineNumber;
				columnNumber = errorLocation.ColumnNumber;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName, lineNumber,
					columnNumber);
				wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
					originalException);
			}
			else if (originalException is OriginalMemoryLimitExceededException)
			{
				type = JsErrorType.Common;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty);

				wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalException);
			}
			else if (originalException is OriginalRecursionDepthOverflowException)
			{
				var originalRecursionException = (OriginalRecursionDepthOverflowException)originalException;
				callStack = JintJsErrorHelpers.ConvertCallChainToStack(originalRecursionException.CallChain);
				type = JsErrorType.Range;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, callStack);

				wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalRecursionException)
				{
					CallStack = callStack
				};
			}
			else if (originalException is OriginalStatementsCountOverflowException)
			{
				type = JsErrorType.Range;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty);

				wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalException);
			}
			else if (originalException is OriginalExecutionCanceledException)
			{
				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = new CancellationTokenSource();

				_cancellationConstraint.Reset(_cancellationTokenSource.Token);

				type = JsErrorType.Common;
				message = CoreStrings.Runtime_ScriptInterrupted;
				description = message;

				wrapperScriptException = new WrapperInterruptedException(message,
					EngineName, EngineVersion, originalException);
			}
			else
			{
				type = JsErrorType.Common;
				message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty);

				wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalException);
			}

			wrapperScriptException.Description = description;
			wrapperScriptException.Type = type;
			wrapperScriptException.DocumentName = documentName;
			wrapperScriptException.LineNumber = lineNumber;
			wrapperScriptException.ColumnNumber = columnNumber;

			return wrapperScriptException;
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

			try
			{
				parsedScript = OriginalEngine.PrepareScript(code, uniqueDocumentName, _strictMode);
			}
			catch (OriginalException e)
			{
				throw WrapException(e);
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

			lock (_executionSynchronizer)
			{
				OriginalValue resultValue;

				try
				{
					resultValue = _jsEngine.Evaluate(expression, uniqueDocumentName);
				}
				catch (OriginalException e)
				{
					throw WrapException(e);
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

			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Execute(code, uniqueDocumentName);
				}
				catch (OriginalException e)
				{
					throw WrapException(e);
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
				catch (OriginalException e)
				{
					throw WrapException(e);
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
				catch (OriginalException e)
				{
					throw WrapException(e);
				}

				OriginalValue resultValue;

				try
				{
					resultValue = _jsEngine.Invoke(functionValue, args);
				}
				catch (OriginalException e)
				{
					throw WrapException(e);
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
				catch (OriginalException e)
				{
					throw WrapException(e);
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
				catch (OriginalException e)
				{
					throw WrapException(e);
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
				catch (OriginalException e)
				{
					throw WrapException(e);
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
				catch (OriginalException e)
				{
					throw WrapException(e);
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

		public override string Name
		{
			get { return EngineName; }
		}

		public override string Version
		{
			get { return EngineVersion; }
		}

		public override bool SupportsScriptPrecompilation
		{
			get { return true; }
		}

		public override bool SupportsScriptInterruption
		{
			get { return true; }
		}

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
					if (_jsEngine != null)
					{
						if (_debuggerStepCallback != null)
						{
							_jsEngine.Debugger.Step -= _debuggerStepCallback;
						}

						if (_debuggerBreakCallback != null)
						{
							_jsEngine.Debugger.Break -= _debuggerBreakCallback;
						}

						_jsEngine.Dispose();
						_jsEngine = null;
					}

					_debuggerStepCallback = null;
					_debuggerBreakCallback = null;
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