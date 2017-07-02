using System;
using System.Reflection;
#if NET45
using System.Runtime.ExceptionServices;
#endif
using System.Text.RegularExpressions;

using Microsoft.ClearScript.V8;
using OriginalJsException = Microsoft.ClearScript.ScriptEngineException;
using OriginalUndefined = Microsoft.ClearScript.Undefined;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

using JavaScriptEngineSwitcher.V8.Resources;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Adapter for the V8 JS engine (Microsoft ClearScript.V8)
	/// </summary>
	public sealed class V8JsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "V8JsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "5.5.372.40";

		/// <summary>
		/// V8 JS engine
		/// </summary>
		private V8ScriptEngine _jsEngine;

		/// <summary>
		/// ClearScript <code>undefined</code> value
		/// </summary>
		private static OriginalUndefined _originalUndefinedValue;

		/// <summary>
		/// Information about <code>InvokeMethod</code> method of <see cref="V8ScriptItem"/> class
		/// </summary>
		private static MethodInfo _v8ScriptItemInvokeMethodInfo;

		/// <summary>
		/// Regular expression for working with the string representation of error
		/// </summary>
		private static readonly Regex _errorStringRegex =
			new Regex(@"[ ]{3,5}at (?:[A-Za-z_\$][0-9A-Za-z_\$]* )?" +
				@"\(?[^\s*?""<>|][^\t\n\r*?""<>|]*?:(?<lineNumber>\d+):(?<columnNumber>\d+)\)? -> ");

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

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
			get { return true; }
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
		/// Constructs a instance of adapter for the V8 JS engine (Microsoft ClearScript.V8)
		/// </summary>
		public V8JsEngine()
			: this(new V8Settings())
		{ }

		/// <summary>
		/// Constructs a instance of adapter for the V8 JS engine (Microsoft ClearScript.V8)
		/// </summary>
		/// <param name="settings">Settings of the V8 JS engine</param>
		public V8JsEngine(V8Settings settings)
		{
			V8Settings v8Settings = settings ?? new V8Settings();

			var constraints = new V8RuntimeConstraints
			{
				MaxNewSpaceSize = v8Settings.MaxNewSpaceSize,
				MaxOldSpaceSize = v8Settings.MaxOldSpaceSize,
				MaxExecutableSize = v8Settings.MaxExecutableSize
			};

			V8ScriptEngineFlags flags = V8ScriptEngineFlags.None;
			if (v8Settings.EnableDebugging)
			{
				flags |= V8ScriptEngineFlags.EnableDebugging;
			}
			if (v8Settings.DisableGlobalMembers)
			{
				flags |= V8ScriptEngineFlags.DisableGlobalMembers;
			}

			int debugPort = v8Settings.DebugPort;

			try
			{
				_jsEngine = new V8ScriptEngine(constraints, flags, debugPort);
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						EngineName, e.Message), EngineName, EngineVersion, e);
			}
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
					EngineName, EngineVersion);
			}
		}

		/// <summary>
		/// Loads a `InvokeMethod` method information of `Microsoft.ClearScript.V8.V8ScriptItem` type
		/// </summary>
		private static void LoadWinScriptItemInvokeMethodInfo()
		{
			const string typeName = "Microsoft.ClearScript.V8.V8ScriptItem";
			const string methodName = "InvokeMethod";

			Assembly clearScriptAssembly = typeof(V8ScriptEngine).Assembly;
			Type v8ScriptItemType = clearScriptAssembly.GetType(typeName);
			MethodInfo v8ScriptItemInvokeMethodInfo = null;

			if (v8ScriptItemType != null)
			{
				v8ScriptItemInvokeMethodInfo = v8ScriptItemType.GetMethod(methodName,
					BindingFlags.Instance | BindingFlags.Public);
			}

			if (v8ScriptItemInvokeMethodInfo != null)
			{
				_v8ScriptItemInvokeMethodInfo = v8ScriptItemInvokeMethodInfo;
			}
			else
			{
				throw new JsEngineLoadException(
					string.Format(Strings.Runtime_MethodInfoNotLoaded, typeName, methodName),
						EngineName, EngineVersion);
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

			var jsRuntimeException = new JsRuntimeException(errorDetails, EngineName, EngineVersion,
				scriptEngineException)
			{
				LineNumber = lineNumber,
				ColumnNumber = columnNumber
			};

			return jsRuntimeException;
		}

		#region JsEngineBase implementation

		protected override object InnerEvaluate(string expression)
		{
			return InnerEvaluate(expression, null);
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			object result;

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsEngine.Evaluate(documentName, false, expression);
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
			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Execute(documentName, false, code);
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
					result = _v8ScriptItemInvokeMethodInfo.Invoke(obj, new object[] { functionName, processedArgs });
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
#if NET45

						ExceptionDispatchInfo.Capture(innerException).Throw();
#elif NET40

						innerException.PreserveStackTrace();
						throw innerException;
#else
#error No implementation for this target
#endif
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

			return TypeConverter.ConvertToType<T>(result);
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

			return TypeConverter.ConvertToType<T>(result);
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

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			object processedValue = MapToClearScriptType(value);

			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.AddHostObject(itemName, processedValue);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.AddHostType(itemName, type);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}
		}

		protected override void InnerCollectGarbage()
		{
			lock (_executionSynchronizer)
			{
				_jsEngine.CollectGarbage(true);
			}
		}

		#endregion

		#region IDisposable implementation

		public override void Dispose()
		{
			if (_disposedFlag.Set())
			{
				if (_jsEngine != null)
				{
					_jsEngine.Dispose();
					_jsEngine = null;
				}
			}
		}

		#endregion
	}
}