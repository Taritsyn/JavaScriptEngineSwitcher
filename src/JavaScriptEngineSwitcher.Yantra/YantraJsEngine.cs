using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using YantraJS.Core;

using OriginalArguments = YantraJS.Core.Arguments;
using OriginalClrMemberNamingConvention = YantraJS.Core.Clr.ClrMemberNamingConvention;
using OriginalClrProxy = YantraJS.Core.Clr.ClrProxy;
using OriginalClrType = YantraJS.Core.Clr.ClrType;
using OriginalContext = YantraJS.Core.JSContext;
using OriginalException = YantraJS.Core.JSException;
using OriginalFunction = YantraJS.Core.JSFunction;
using OriginalTypeConverter = YantraJS.Utils.TypeConverter;
using OriginalUndefined = YantraJS.Core.JSUndefined;
using OriginalValue = YantraJS.Core.JSValue;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;

using JavaScriptEngineSwitcher.Yantra.Helpers;

namespace JavaScriptEngineSwitcher.Yantra
{
	/// <summary>
	/// Adapter for the Yantra JS engine
	/// </summary>
	public sealed class YantraJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "YantraJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "1.2.121";

		/// <summary>
		/// Regular expression for working with the error message
		/// </summary>
		private static readonly Regex _errorMessageRegex =
			new Regex(@"^(?<type>" + CommonRegExps.JsFullNamePattern + @"):\s+(?<description>[\s\S]+?)" +
				@"(?: at (?<lineNumber>\d+), (?<columnNumber>\d+))?$");

		/// <summary>
		/// Yantra JS context
		/// </summary>
		private OriginalContext _jsContext;

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
		/// Constructs an instance of adapter for the Yantra JS engine
		/// </summary>
		public YantraJsEngine()
		{
			_jsContext = new OriginalContext()
			{
				ClrMemberNamingConvention = OriginalClrMemberNamingConvention.Declared
			};
		}


		#region Mapping

		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static OriginalValue MapToScriptType(object value)
		{
			if (value is Undefined)
			{
				return OriginalUndefined.Value;
			}

			return OriginalTypeConverter.FromBasic(value);
		}

		/// <summary>
		/// Makes a mapping of array items from the host type to a script type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		private static OriginalValue[] MapToScriptType(object[] args)
		{
			return args.Select(MapToScriptType).ToArray();
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToHostType(OriginalValue value)
		{
			object result;

			if (value.IsNull)
			{
				result = null;
			}
			else if (value.IsUndefined)
			{
				result = Undefined.Value;
			}
			else if (value.IsBoolean)
			{
				result = value.BooleanValue;
			}
			else if (value.IsNumber)
			{
				result = value.DoubleValue;
			}
			else if (value.IsString)
			{
				result = value.ToString();
			}
			else
			{
				result = value;
			}

			return result;
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
		/// </summary>
		/// <typeparam name="T">The type to convert the value to</typeparam>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static T MapToHostType<T>(OriginalValue value)
		{
			if (value.IsNull)
			{
				return TypeConverter.ConvertToType<T>(null);
			}

			Type targetType = typeof(T);

			if (targetType == typeof(Undefined))
			{
				if (value.IsUndefined)
				{
					return (T)(object)Undefined.Value;
				}
				else
				{
					throw new InvalidOperationException(
						string.Format(CoreStrings.Common_CannotConvertObjectToType, value.GetType(), targetType)
					);
				}
			}

			T result;

			if (!value.ConvertTo<T>(out result))
			{
				if (targetType == typeof(string))
				{
					result = (T)(object)value.ToString();
				}
				else
				{
					throw new InvalidOperationException(
						string.Format(CoreStrings.Common_CannotConvertObjectToType, value.GetType(), targetType)
					);
				}
			}

			return result;
		}

		private static OriginalFunction CreateEmbeddedFunction(Delegate del)
		{
			var originalFunction = new OriginalFunction((in OriginalArguments args) =>
			{
				MethodInfo method = del.GetMethodInfo();
				ParameterInfo[] parameters = method.GetParameters();
				object[] processedArgs = GetHostDelegateArguments(args.ToArray(), parameters.Length);

				ReflectionHelpers.FixArgumentTypes(ref processedArgs, parameters);

				object result;

				try
				{
					result = del.DynamicInvoke(processedArgs);
				}
				catch (Exception e) when ((e is TargetInvocationException || e is WrapperException)
					&& e.InnerException != null)
				{
					OriginalException originalException = OriginalException.From(e.InnerException);
					throw originalException;
				}
				catch (Exception e)
				{
					OriginalException originalException = OriginalException.From(e);
					throw originalException;
				}

				OriginalValue resultValue = MapToScriptType(result);

				return resultValue;
			});

			return originalFunction;
		}

		private static object[] GetHostDelegateArguments(OriginalValue[] args, int maxArgCount)
		{
			if (args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}

			int argCount = args.Length;
			int processedArgCount = argCount > maxArgCount ? maxArgCount : argCount;
			object[] processedArgs;

			if (processedArgCount > 0)
			{
				processedArgs = args
					.Take(processedArgCount)
					.Select(MapToHostType)
					.ToArray()
					;
			}
			else
			{
				processedArgs = new object[0];
			}

			return processedArgs;
		}

		private WrapperException WrapJsException(OriginalException originalException)
		{
			WrapperException wrapperException;
			string message = originalException.Message;
			string description = message;
			string type = string.Empty;
			string documentName = string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			ErrorLocationItem[] callStackItems = null;

			OriginalValue errorValue = originalException.Error;
			if (errorValue != null)
			{
				message = errorValue.ToString();
				Match messageMatch = _errorMessageRegex.Match(message);

				if (messageMatch.Success)
				{
					GroupCollection messageGroups = messageMatch.Groups;
					type = messageGroups["type"].Value;
					description = messageGroups["description"].Value;
					lineNumber = messageGroups["lineNumber"].Success ?
						int.Parse(messageGroups["lineNumber"].Value) : 0;
					columnNumber = messageGroups["columnNumber"].Success ?
						int.Parse(messageGroups["columnNumber"].Value) : 0;
				}

				string rawCallStack = errorValue["stack"].AsStringOrDefault();

				callStackItems = YantraJsErrorHelpers.ParseErrorLocation(rawCallStack);
				callStackItems = YantraJsErrorHelpers.FilterErrorLocationItems(callStackItems);
				YantraJsErrorHelpers.FixErrorLocationItems(callStackItems);

				if (callStackItems.Length > 0)
				{
					ErrorLocationItem firstCallStackItem = callStackItems[0];

					documentName = firstCallStackItem.DocumentName;
					if (lineNumber == 0 && columnNumber == 0)
					{
						lineNumber = firstCallStackItem.LineNumber;
						columnNumber = firstCallStackItem.ColumnNumber;
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(type))
			{
				WrapperScriptException wrapperScriptException;
				if (type == JsErrorType.Syntax)
				{
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName,
						lineNumber, columnNumber);

					wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
						originalException);
				}
				else
				{
					string callStack = callStackItems != null ?
						JsErrorHelpers.StringifyErrorLocationItems(callStackItems) : string.Empty;
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, callStack);

					var wrapperRuntimeException = new WrapperRuntimeException(message, EngineName, EngineVersion,
						originalException);
					wrapperRuntimeException.CallStack = callStack;

					wrapperScriptException = wrapperRuntimeException;
				}
				wrapperScriptException.Type = type;
				wrapperScriptException.DocumentName = documentName;
				wrapperScriptException.LineNumber = lineNumber;
				wrapperScriptException.ColumnNumber = columnNumber;

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

		/// <summary>
		/// Evaluates an expression without converting its result to a host type
		/// </summary>
		/// <param name="expression">JS expression</param>
		/// <param name="documentName">Document name</param>
		/// <returns>Result of the expression not converted to a host type</returns>
		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private OriginalValue InnerEvaluateWithoutResultConversion(string expression, string documentName)
		{
			OriginalValue resultValue;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			try
			{
				lock (_executionSynchronizer)
				{
					resultValue = _jsContext.Eval(expression, uniqueDocumentName);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			catch (Exception e) when ((e is TargetInvocationException || e is WrapperException)
				&& e.InnerException != null)
			{
				OriginalException originalException = OriginalException.From(e.InnerException);
				throw originalException;
			}

			return resultValue;
		}

		/// <summary>
		/// Calls a function without converting its result to a host type
		/// </summary>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution not converted to a host type</returns>
		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private OriginalValue InnerCallFunctionWithoutResultConversion(string functionName, params object[] args)
		{
			OriginalValue resultValue;
			OriginalValue[] processedArgs = MapToScriptType(args);

			try
			{
				lock (_executionSynchronizer)
				{
					resultValue = _jsContext.InvokeMethod(functionName, new OriginalArguments(_jsContext, processedArgs));
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			catch (Exception e) when ((e is TargetInvocationException || e is WrapperException)
				&& e.InnerException != null)
			{
				OriginalException originalException = OriginalException.From(e.InnerException);
				throw originalException;
			}

			return resultValue;
		}

		/// <summary>
		/// Gets a value of variable without converting it to a host type
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable not converted to a host type</returns>
		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private OriginalValue InnerGetVariableValueWithoutResultConversion(string variableName)
		{
			OriginalValue variableValue;

			try
			{
				lock (_executionSynchronizer)
				{
					variableValue = _jsContext[variableName];
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}

			return variableValue;
		}

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
			OriginalValue resultValue = InnerEvaluateWithoutResultConversion(expression, documentName);
			object result = MapToHostType(resultValue);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			return InnerEvaluate<T>(expression, null);
		}

		protected override T InnerEvaluate<T>(string expression, string documentName)
		{
			OriginalValue resultValue = InnerEvaluateWithoutResultConversion(expression, documentName);
			T result = MapToHostType<T>(resultValue);

			return result;
		}

		protected override void InnerExecute(string code)
		{
			InnerExecute(code, null);
		}

		protected override void InnerExecute(string code, string documentName)
		{
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			try
			{
				lock (_executionSynchronizer)
				{
					_jsContext.Execute(code, uniqueDocumentName);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			catch (Exception e) when ((e is TargetInvocationException || e is WrapperException)
				&& e.InnerException != null)
			{
				OriginalException originalException = OriginalException.From(e.InnerException);
				throw originalException;
			}
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			throw new NotSupportedException();
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			OriginalValue resultValue = InnerCallFunctionWithoutResultConversion(functionName, args);
			object result = MapToHostType(resultValue);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			OriginalValue resultValue = InnerCallFunctionWithoutResultConversion(functionName, args);
			T result = MapToHostType<T>(resultValue);

			return result;
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			try
			{
				OriginalValue variableValue;

				lock (_executionSynchronizer)
				{
					variableValue = _jsContext[variableName];
				}

				result = !variableValue.IsUndefined;
			}
			catch (OriginalException)
			{
				result = false;
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			OriginalValue variableValue = InnerGetVariableValueWithoutResultConversion(variableName);
			object result = MapToHostType(variableValue);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			OriginalValue variableValue = InnerGetVariableValueWithoutResultConversion(variableName);
			T result = MapToHostType<T>(variableValue);

			return result;
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			OriginalValue processedValue = MapToScriptType(value);

			try
			{
				lock (_executionSynchronizer)
				{
					_jsContext[variableName] = processedValue;
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
				lock (_executionSynchronizer)
				{
					_jsContext.Delete(variableName);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			OriginalValue processedValue;
			if (value is Delegate)
			{
				processedValue = CreateEmbeddedFunction((Delegate)value);
			}
			else
			{
				processedValue = OriginalClrProxy.From(value);
			}

			try
			{
				lock (_executionSynchronizer)
				{
					_jsContext[itemName] = processedValue;

				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			OriginalValue processedValue = OriginalClrType.From(type);

			try
			{
				lock (_executionSynchronizer)
				{
					_jsContext[itemName] = processedValue;
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
				lock (_executionSynchronizer)
				{
					if (_jsContext != null)
					{
						_jsContext.Dispose();
						_jsContext = null;
					}
				}
			}
		}

		#endregion

		#endregion
	}
}