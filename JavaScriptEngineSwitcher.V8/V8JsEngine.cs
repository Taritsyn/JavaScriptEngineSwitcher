namespace JavaScriptEngineSwitcher.V8
{
	using System;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Web.Script.Serialization;

	using Microsoft.ClearScript.V8;
	using OriginalUndefined = Microsoft.ClearScript.Undefined;
	using OriginalJsException = Microsoft.ClearScript.ScriptEngineException;

	using Core;
	using CoreStrings = Core.Resources.Strings;

	using Resources;

	/// <summary>
	/// Adapter for Microsoft ClearScript.V8
	/// </summary>
	public sealed class V8JsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JavaScript engine
		/// </summary>
		private const string ENGINE_NAME = "V8 JavaScript engine";

		/// <summary>
		/// Version of original JavaScript engine
		/// </summary>
		private const string ENGINE_VERSION = "3.24.17";

		/// <summary>
		/// JS-engine
		/// </summary>
		private V8ScriptEngine _jsEngine;

		/// <summary>
		/// JS-serializer
		/// </summary>
		private JavaScriptSerializer _jsSerializer;

		/// <summary>
		/// ClearScript <code>undefined</code> value
		/// </summary>
		private static OriginalUndefined _originalUndefinedValue;

		/// <summary>
		/// Information about `InvokeMethod` method of `Microsoft.ClearScript.Windows.WindowsScriptItem` type
		/// </summary>
		private static MethodInfo _winScriptItemInvokeMethodInfo;
		
		/// <summary>
		/// Regular expression for working with the string representation of error
		/// </summary>
		private static readonly Regex _errorStringRegex =
			new Regex(@"at (?:[A-Za-z_\$][0-9A-Za-z_\$]* )?" + 
				@"\(?Script Document(?:\s*\[\d+\])?:(?<lineNumber>\d+):(?<columnNumber>\d+)\)?");

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

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
		/// Static constructor
		/// </summary>
		static V8JsEngine()
		{
			AssemblyResolver.Initialize();
			LoadUndefinedValue();
			LoadWinScriptItemInvokeMethodInfo();
		}

		/// <summary>
		/// Constructs instance of adapter for Microsoft ClearScript.V8
		/// </summary>
		public V8JsEngine()
		{
			try
			{
				_jsEngine = new V8ScriptEngine();
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						ENGINE_NAME, e.Message), ENGINE_NAME, ENGINE_VERSION, e);
			}

			_jsSerializer = new JavaScriptSerializer();
		}


		/// <summary>
		/// Loads a ClearScript <code>undefined</code> value
		/// </summary>
		private static void LoadUndefinedValue()
		{
			FieldInfo undefinedValueFieldInfo = typeof(OriginalUndefined).GetField("Value",
				BindingFlags.NonPublic | BindingFlags.Static);
			OriginalUndefined originalUndefinedValue = null;

			if (undefinedValueFieldInfo != null)
			{
				originalUndefinedValue = undefinedValueFieldInfo.GetValue(null) as OriginalUndefined;
			}

			if (originalUndefinedValue != null)
			{
				_originalUndefinedValue = originalUndefinedValue;
			}
			else
			{
				throw new JsEngineLoadException(Strings.Engines_ClearScriptUndefinedValueNotLoaded,
					ENGINE_NAME, ENGINE_VERSION);
			}
		}

		/// <summary>
		/// Loads a `InvokeMethod` method information of `Microsoft.ClearScript.Windows.WindowsScriptItem` type
		/// </summary>
		private static void LoadWinScriptItemInvokeMethodInfo()
		{
			const string typeName = "Microsoft.ClearScript.V8.V8ScriptItem";
			const string methodName = "InvokeMethod";

			Assembly clearScriptAssembly = typeof(V8ScriptEngine).Assembly;
			Type winScriptItemType = clearScriptAssembly.GetType(typeName);
			MethodInfo winScriptItemInvokeMethodInfo = null;

			if (winScriptItemType != null)
			{
				winScriptItemInvokeMethodInfo = winScriptItemType.GetMethod(methodName,
					BindingFlags.Instance | BindingFlags.Public);
			}

			if (winScriptItemInvokeMethodInfo != null)
			{
				_winScriptItemInvokeMethodInfo = winScriptItemInvokeMethodInfo;
			}
			else
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_MethodInfoNotLoaded, typeName, methodName),
						ENGINE_NAME, ENGINE_VERSION);
			}
		}

		/// <summary>
		/// Executes a mapping from the host type to a ClearScript type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToClearScriptType(object value)
		{
			if (value is Undefined)
			{
				return _originalUndefinedValue;
			}

			return value;
		}

		/// <summary>
		/// Executes a mapping from the ClearScript type to a host type
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

		private JsRuntimeException ConvertScriptEngineExceptionToJsRuntimeException(
			OriginalJsException scriptEngineException)
		{
			string errorDetails = scriptEngineException.ErrorDetails;
			int lineNumber = 0;
			int columnNumber = 0;

			Match errorStringMatch = _errorStringRegex.Match(errorDetails);
			if (errorStringMatch.Success)
			{
				GroupCollection errorStringGroups = errorStringMatch.Groups;

				lineNumber = int.Parse(errorStringGroups["lineNumber"].Value);
				columnNumber = int.Parse(errorStringGroups["columnNumber"].Value);
			}

			var jsRuntimeException = new JsRuntimeException(errorDetails, ENGINE_NAME, ENGINE_VERSION,
				scriptEngineException)
			{
				LineNumber = lineNumber,
				ColumnNumber = columnNumber,
				Source = scriptEngineException.Source,
				HelpLink = scriptEngineException.HelpLink
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

		#region JsEngineBase implementation

		protected override object InnerEvaluate(string expression)
		{
			object result;

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsEngine.Evaluate(expression);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Execute(code);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
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
					processedArgs[argumentIndex] = MapToClearScriptType(args[argumentIndex]);
				}
			}

			lock (_executionSynchronizer)
			{
				try
				{
					object obj = _jsEngine.Script;
					result = _winScriptItemInvokeMethodInfo.Invoke(obj, new object[] {functionName, processedArgs});
				}
				catch (TargetInvocationException e)
				{
					Exception innerException = e.InnerException;
					if (innerException != null)
					{
						var scriptEngineException = e.InnerException as OriginalJsException;
						if (scriptEngineException != null)
						{
							throw ConvertScriptEngineExceptionToJsRuntimeException(scriptEngineException);
						}

						throw innerException;
					}

					throw;
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return ConvertToType<T>(result);
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
					result = _jsEngine.Script[variableName];
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToClearScriptType(value);

			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Script[variableName] = processedValue;
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
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

				if (_jsEngine != null)
				{
					_jsEngine.Dispose();
					_jsEngine = null;
				}

				_jsSerializer = null;
			}
		}

		#endregion
	}
}