using System;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.ClearScript.V8;
using OriginalScriptEngineException = Microsoft.ClearScript.ScriptEngineException;
using OriginalScriptInterruptedException = Microsoft.ClearScript.ScriptInterruptedException;
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
		private const string EngineVersion = "6.3.292.48";

		/// <summary>
		/// V8 JS engine
		/// </summary>
		private V8ScriptEngine _jsEngine;

		/// <summary>
		/// ClearScript <code>undefined</code> value
		/// </summary>
		private static OriginalUndefined _originalUndefinedValue;

		/// <summary>
		/// Regular expression for working with the string representation of error
		/// </summary>
		private static readonly Regex _errorStringRegex =
			new Regex(@"[ ]{3,5}at (?:[A-Za-z_\$][0-9A-Za-z_\$]* )?" +
				@"\(?[^\s*?""<>|][^\t\n\r*?""<>|]*?:(?<lineNumber>\d+):(?<columnNumber>\d+)\)? -> ");


		/// <summary>
		/// Static constructor
		/// </summary>
		static V8JsEngine()
		{
			AssemblyResolver.Initialize();
			LoadUndefinedValue();
		}

		/// <summary>
		/// Constructs an instance of adapter for the V8 JS engine (Microsoft ClearScript.V8)
		/// </summary>
		public V8JsEngine()
			: this(new V8Settings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the V8 JS engine (Microsoft ClearScript.V8)
		/// </summary>
		/// <param name="settings">Settings of the V8 JS engine</param>
		public V8JsEngine(V8Settings settings)
		{
			V8Settings v8Settings = settings ?? new V8Settings();

			var constraints = new V8RuntimeConstraints
			{
				MaxNewSpaceSize = v8Settings.MaxNewSpaceSize,
				MaxOldSpaceSize = v8Settings.MaxOldSpaceSize,
			};

			V8ScriptEngineFlags flags = V8ScriptEngineFlags.None;
			if (v8Settings.AwaitDebuggerAndPauseOnStart)
			{
				flags |= V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart;
			}
			if (v8Settings.EnableDebugging)
			{
				flags |= V8ScriptEngineFlags.EnableDebugging;
			}
			if (v8Settings.EnableRemoteDebugging)
			{
				flags |= V8ScriptEngineFlags.EnableRemoteDebugging;
			}
			if (v8Settings.DisableGlobalMembers)
			{
				flags |= V8ScriptEngineFlags.DisableGlobalMembers;
			}

			int debugPort = v8Settings.DebugPort;

			try
			{
				_jsEngine = new V8ScriptEngine(constraints, flags, debugPort);
				_jsEngine.MaxRuntimeHeapSize = v8Settings.MaxHeapSize;
				_jsEngine.RuntimeHeapSizeSampleInterval = v8Settings.HeapSizeSampleInterval;
				_jsEngine.MaxRuntimeStackUsage = v8Settings.MaxStackUsage;
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

		#region Mapping

		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToScriptType(object value)
		{
			if (value is Undefined)
			{
				return _originalUndefinedValue;
			}

			return value;
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
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

		private JsRuntimeException ConvertScriptEngineExceptionToHostException(
			OriginalScriptEngineException scriptEngineException)
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

			var hostException = new JsRuntimeException(errorDetails, EngineName, EngineVersion,
				scriptEngineException)
			{
				LineNumber = lineNumber,
				ColumnNumber = columnNumber
			};

			return hostException;
		}

		private JsScriptInterruptedException ConvertScriptInterruptedExceptionToHostException(
			OriginalScriptInterruptedException scriptInterruptedException)
		{
			var hostException = new JsScriptInterruptedException(CoreStrings.Runtime_ScriptInterrupted,
				EngineName, EngineVersion, scriptInterruptedException);

			return hostException;
		}

		#endregion

		#region JsEngineBase overrides

		protected override object InnerEvaluate(string expression)
		{
			return InnerEvaluate(expression, null);
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			object result;

			try
			{
				result = _jsEngine.Evaluate(documentName, false, expression);
			}
			catch (OriginalScriptEngineException e)
			{
				throw ConvertScriptEngineExceptionToHostException(e);
			}
			catch (OriginalScriptInterruptedException e)
			{
				throw ConvertScriptInterruptedExceptionToHostException(e);
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
			try
			{
				_jsEngine.Execute(documentName, false, code);
			}
			catch (OriginalScriptEngineException e)
			{
				throw ConvertScriptEngineExceptionToHostException(e);
			}
			catch (OriginalScriptInterruptedException e)
			{
				throw ConvertScriptInterruptedExceptionToHostException(e);
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
					processedArgs[argumentIndex] = MapToScriptType(args[argumentIndex]);
				}
			}

			try
			{
				result = _jsEngine.Invoke(functionName, processedArgs);
			}
			catch (OriginalScriptEngineException e)
			{
				throw ConvertScriptEngineExceptionToHostException(e);
			}
			catch (OriginalScriptInterruptedException e)
			{
				throw ConvertScriptInterruptedExceptionToHostException(e);
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

			try
			{
				result = _jsEngine.Script[variableName];
			}
			catch (OriginalScriptEngineException e)
			{
				throw ConvertScriptEngineExceptionToHostException(e);
			}
			catch (OriginalScriptInterruptedException e)
			{
				throw ConvertScriptInterruptedExceptionToHostException(e);
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
			object processedValue = MapToScriptType(value);

			try
			{
				_jsEngine.Script[variableName] = processedValue;
			}
			catch (OriginalScriptEngineException e)
			{
				throw ConvertScriptEngineExceptionToHostException(e);
			}
			catch (OriginalScriptInterruptedException e)
			{
				throw ConvertScriptInterruptedExceptionToHostException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InnerSetVariableValue(variableName, Undefined.Value);
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			object processedValue = MapToScriptType(value);

			try
			{
				_jsEngine.AddHostObject(itemName, processedValue);
			}
			catch (OriginalScriptEngineException e)
			{
				throw ConvertScriptEngineExceptionToHostException(e);
			}
			catch (OriginalScriptInterruptedException e)
			{
				throw ConvertScriptInterruptedExceptionToHostException(e);
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			try
			{
				_jsEngine.AddHostType(itemName, type);
			}
			catch (OriginalScriptEngineException e)
			{
				throw ConvertScriptEngineExceptionToHostException(e);
			}
			catch (OriginalScriptInterruptedException e)
			{
				throw ConvertScriptInterruptedExceptionToHostException(e);
			}
		}

		protected override void InnerInterrupt()
		{
			_jsEngine.Interrupt();
		}

		protected override void InnerCollectGarbage()
		{
				_jsEngine.CollectGarbage(true);
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
			get { return true; }
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

		#endregion
	}
}