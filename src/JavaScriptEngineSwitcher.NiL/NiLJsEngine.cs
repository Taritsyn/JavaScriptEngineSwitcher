using System;
using System.Text.RegularExpressions;

using NiL.JS.Extensions;

using OriginalArguments = NiL.JS.Core.Arguments;
using OriginalCodeCoordinates = NiL.JS.Core.CodeCoordinates;
using OriginalContext = NiL.JS.Core.Context;
using OriginalDebuggerCallback = NiL.JS.Core.DebuggerCallback;
using OriginalError = NiL.JS.BaseLibrary.Error;
using OriginalException = NiL.JS.Core.JSException;
using OriginalFunction = NiL.JS.BaseLibrary.Function;
using OriginalValue = NiL.JS.Core.JSValue;
using OriginalValueType = NiL.JS.Core.JSValueType;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;

using JavaScriptEngineSwitcher.NiL.Helpers;

namespace JavaScriptEngineSwitcher.NiL
{
	/// <summary>
	/// Adapter for the NiL JS engine
	/// </summary>
	public sealed class NiLJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "NiLJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "2.5.1677";

		/// <summary>
		/// Regular expression for working with the syntax error message
		/// </summary>
		private static readonly Regex _syntaxErrorMessageRegex =
			new Regex(@"^(?<description>[\s\S]+?) (?:at )?\((?<lineNumber>\d+):(?<columnNumber>\d+)\)$");

		/// <summary>
		/// NiL JS context
		/// </summary>
		private OriginalContext _jsContext;

		/// <summary>
		/// Debugger callback
		/// </summary>
		private OriginalDebuggerCallback _debuggerCallback;

		/// <summary>
		/// Synchronizer
		/// </summary>
		private readonly object _synchronizer = new object();


		/// <summary>
		/// Constructs an instance of adapter for the NiL JS engine
		/// </summary>
		public NiLJsEngine()
			: this(new NiLSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the NiL JS engine
		/// </summary>
		/// <param name="settings">Settings of the NiL JS engine</param>
		public NiLJsEngine(NiLSettings settings)
		{
			NiLSettings niLSettings = settings ?? new NiLSettings();
			_debuggerCallback = niLSettings.DebuggerCallback;

			try
			{
				_jsContext = new OriginalContext(niLSettings.StrictMode);
				_jsContext.Debugging = niLSettings.EnableDebugging;
				if (_debuggerCallback != null)
				{
					_jsContext.DebuggerCallback += _debuggerCallback;
				}
				_jsContext.GlobalContext.CurrentTimeZone = niLSettings.LocalTimeZone;
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
		private static OriginalValue MapToScriptType(object value)
		{
			if (value == null)
			{
				return OriginalValue.Null;
			}

			if (value is Undefined)
			{
				return OriginalValue.Undefined;
			}

			return OriginalContext.CurrentGlobalContext.ProxyValue(value);
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToHostType(OriginalValue value)
		{
			if (value.IsNull)
			{
				return null;
			}

			OriginalValueType valueType = value.ValueType;
			object result;

			switch (valueType)
			{
				case OriginalValueType.NotExists:
				case OriginalValueType.NotExistsInObject:
				case OriginalValueType.Undefined:
					result = Undefined.Value;
					break;

				case OriginalValueType.Boolean:
				case OriginalValueType.Integer:
				case OriginalValueType.Double:
				case OriginalValueType.String:
				case OriginalValueType.Symbol:
				case OriginalValueType.Object:
				case OriginalValueType.Function:
				case OriginalValueType.Date:
				case OriginalValueType.Property:
				case OriginalValueType.SpreadOperatorResult:
					result = value.Value;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			return result;
		}

		private static WrapperException WrapJsException(OriginalException originalException)
		{
			WrapperException wrapperException;
			string message = originalException.Message;
			string description = message;
			string type = string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			string sourceFragment = string.Empty;

			var errorValue = originalException.Error?.Value as OriginalError;
			if (errorValue != null)
			{
				message = errorValue.message.As<string>();
				description = message;
				type = errorValue.name.As<string>();
			}

			if (!string.IsNullOrEmpty(type))
			{
				WrapperScriptException wrapperScriptException;
				if (type == JsErrorType.Syntax)
				{
					Match messageMatch = _syntaxErrorMessageRegex.Match(message);
					if (messageMatch.Success)
					{
						GroupCollection messageGroups = messageMatch.Groups;
						description = messageGroups["description"].Value;
						lineNumber = int.Parse(messageGroups["lineNumber"].Value);
						columnNumber = int.Parse(messageGroups["columnNumber"].Value);
					}
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty,
						lineNumber, columnNumber);

					wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
						originalException);
				}
				else
				{
					string sourceCode = originalException.SourceCode;
					OriginalCodeCoordinates codeCoordinates = originalException.CodeCoordinates;
					if (codeCoordinates != null)
					{
						lineNumber = codeCoordinates.Line;
						columnNumber = codeCoordinates.Column;
					}

					sourceFragment = TextHelpers.GetTextFragment(sourceCode, lineNumber, columnNumber);
					string callStack = string.Empty;
					ErrorLocationItem[] callStackItems = NiLJsErrorHelpers.ParseErrorLocation(
						originalException.StackTrace);
					if (callStackItems.Length > 0)
					{
						NiLJsErrorHelpers.FixErrorLocationItems(callStackItems);

						ErrorLocationItem firstCallStackItem = callStackItems[0];
						firstCallStackItem.SourceFragment = sourceFragment;

						callStack = JsErrorHelpers.StringifyErrorLocationItems(callStackItems, true);
						string callStackWithSourceFragment = JsErrorHelpers.StringifyErrorLocationItems(
							callStackItems);
						message = JsErrorHelpers.GenerateScriptErrorMessage(type, description,
							callStackWithSourceFragment);
					}
					else
					{
						message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty,
							lineNumber, columnNumber, sourceFragment);
					}

					var wrapperRuntimeException = new WrapperRuntimeException(message, EngineName, EngineVersion,
						originalException);
					wrapperRuntimeException.CallStack = callStack;

					wrapperScriptException = wrapperRuntimeException;
				}
				wrapperScriptException.Type = type;
				wrapperScriptException.LineNumber = lineNumber;
				wrapperScriptException.ColumnNumber = columnNumber;
				wrapperScriptException.SourceFragment = sourceFragment;

				wrapperException = wrapperScriptException;
			}
			else
			{
				wrapperException = new WrapperException(message, EngineName, EngineVersion, originalException);
			}

			wrapperException.Description = description;

			return wrapperException;
		}

		#endregion

		#region JsEngineBase overrides

		protected override IPrecompiledScript InnerPrecompile(string code)
		{
			throw new NotSupportedException();
		}

		protected override IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			throw new NotSupportedException();
		}

		protected override object InnerEvaluate(string expression)
		{
			return InnerEvaluate(expression, null);
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			OriginalValue resultValue;

			try
			{
				lock (_synchronizer)
				{
					resultValue = _jsContext.Eval(expression, true);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}

			object result = MapToHostType(resultValue);

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
			try
			{
				lock (_synchronizer)
				{
					_jsContext.Eval(code, true);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			throw new NotSupportedException();
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			OriginalValue resultValue;
			var processedArgs = new OriginalArguments();

			int argumentCount = args.Length;
			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					processedArgs.Add(MapToScriptType(args[argumentIndex]));
				}
			}

			try
			{
				lock (_synchronizer)
				{
					OriginalValue functionValue = _jsContext.GetVariable(functionName);
					var function = functionValue.As<OriginalFunction>();
					if (function == null)
					{
						throw new WrapperRuntimeException(
							string.Format(CoreStrings.Runtime_FunctionNotExist, functionName));
					}

					resultValue = function.Call(processedArgs);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			catch (WrapperRuntimeException)
			{
				throw;
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

			lock (_synchronizer)
			{
				try
				{
					OriginalValue variableValue = _jsContext.GetVariable(variableName);
					OriginalValueType valueType = variableValue.ValueType;

					result = valueType != OriginalValueType.NotExists && valueType != OriginalValueType.Undefined;
				}
				catch (OriginalException)
				{
					result = false;
				}
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			try
			{
				lock (_synchronizer)
				{
					OriginalValue variableValue = _jsContext.GetVariable(variableName);
					if (variableValue.ValueType == OriginalValueType.NotExists)
					{
						throw new WrapperRuntimeException(
							string.Format(CoreStrings.Runtime_VariableNotExist, variableName),
							EngineName, EngineVersion
						);
					}

					result = MapToHostType(variableValue);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
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
			OriginalValue processedValue = MapToScriptType(value);

			try
			{
				lock (_synchronizer)
				{
					OriginalValue variableValue = _jsContext.GetVariable(variableName);
					if (variableValue.ValueType == OriginalValueType.NotExists)
					{
						variableValue = _jsContext.DefineVariable(variableName, true);
					}
					variableValue.Assign(processedValue);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				lock (_synchronizer)
				{
					_jsContext.DeleteVariable(variableName);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			OriginalValue processedValue = _jsContext.GlobalContext.ProxyValue(value);

			try
			{
				lock (_synchronizer)
				{
					OriginalValue variableValue = _jsContext.GetVariable(itemName);
					if (variableValue.ValueType == OriginalValueType.NotExists)
					{
						variableValue = _jsContext.DefineVariable(itemName, true);
					}
					variableValue.Assign(processedValue);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			try
			{
				lock (_synchronizer)
				{
					OriginalValue processedValue = _jsContext.GlobalContext.GetConstructor(type);

					OriginalValue variableValue = _jsContext.GetVariable(itemName);
					if (variableValue.ValueType == OriginalValueType.NotExists)
					{
						variableValue = _jsContext.DefineVariable(itemName, true);
					}
					variableValue.Assign(processedValue);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerInterrupt()
		{
			throw new NotSupportedException();
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
			get { return false; }
		}

		public override bool SupportsScriptInterruption
		{
			get { return false; }
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
				lock (_synchronizer)
				{
					if (_jsContext != null)
					{
						if (_debuggerCallback != null)
						{
							_jsContext.DebuggerCallback -= _debuggerCallback;
							_debuggerCallback = null;
						}

						_jsContext = null;
					}
				}
			}
		}

		#endregion

		#endregion
	}
}