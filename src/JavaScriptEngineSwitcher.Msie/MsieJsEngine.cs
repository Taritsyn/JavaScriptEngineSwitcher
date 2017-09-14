using System;
using System.IO;
using System.Reflection;
using System.Text;

using OriginalJsEngine = MsieJavaScriptEngine.MsieJsEngine;
using OriginalJsEngineLoadException = MsieJavaScriptEngine.JsEngineLoadException;
using OriginalJsEngineMode = MsieJavaScriptEngine.JsEngineMode;
using OriginalJsException = MsieJavaScriptEngine.JsException;
using OriginalJsRuntimeException = MsieJavaScriptEngine.JsRuntimeException;
using OriginalJsScriptInterruptedException = MsieJavaScriptEngine.JsScriptInterruptedException;
using OriginalJsEngineSettings = MsieJavaScriptEngine.JsEngineSettings;
using OriginalTypeConverter = MsieJavaScriptEngine.Utilities.TypeConverter;
using OriginalUndefined = MsieJavaScriptEngine.Undefined;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.Msie
{
	/// <summary>
	/// Adapter for the MSIE JS engine
	/// </summary>
	public sealed class MsieJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "MsieJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private readonly string _engineVersion;

		/// <summary>
		/// MSIE JS engine
		/// </summary>
		private OriginalJsEngine _jsEngine;


		/// <summary>
		/// Constructs an instance of adapter for the MSIE JS engine
		/// </summary>
		public MsieJsEngine()
			: this(new MsieSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the MSIE JS engine
		/// </summary>
		/// <param name="settings">Settings of the MSIE JS engine</param>
		public MsieJsEngine(MsieSettings settings)
		{
			MsieSettings msieSettings = settings ?? new MsieSettings();

			try
			{
				_jsEngine = new OriginalJsEngine(new OriginalJsEngineSettings
				{
					EnableDebugging = msieSettings.EnableDebugging,
					EngineMode = Utils.GetEnumFromOtherEnum<JsEngineMode, OriginalJsEngineMode>(
						msieSettings.EngineMode),
					UseEcmaScript5Polyfill = msieSettings.UseEcmaScript5Polyfill,
					UseJson2Library = msieSettings.UseJson2Library
				});
				_engineVersion = _jsEngine.Mode;
			}
			catch (OriginalJsEngineLoadException e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						EngineName, e.Message), EngineName, e.EngineMode, e);
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						EngineName, e.Message), EngineName, _engineVersion, e);
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
				return OriginalUndefined.Value;
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

		private JsException ConvertScriptExceptionToHostException(
			OriginalJsException scriptException)
		{
			JsException hostException;

			if (scriptException is OriginalJsRuntimeException)
			{
				var scriptRuntimeException = (OriginalJsRuntimeException)scriptException;
				hostException = new JsRuntimeException(scriptRuntimeException.Message,
					EngineName, _engineVersion, scriptRuntimeException)
				{
					ErrorCode = scriptRuntimeException.ErrorCode,
					Category = scriptRuntimeException.Category,
					LineNumber = scriptRuntimeException.LineNumber,
					ColumnNumber = scriptRuntimeException.ColumnNumber,
					SourceFragment = scriptRuntimeException.SourceFragment
				};
			}
			else if (scriptException is OriginalJsScriptInterruptedException)
			{
				var scriptInterruptedException = (OriginalJsScriptInterruptedException)scriptException;
				hostException = new JsScriptInterruptedException(CoreStrings.Runtime_ScriptInterrupted,
					EngineName, _engineVersion, scriptInterruptedException);
			}
			else
			{
				hostException = new JsException(scriptException.Message,
					EngineName, _engineVersion, scriptException);
			}

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
				result = _jsEngine.Evaluate(expression, documentName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
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

			return OriginalTypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			InnerExecute(code, null);
		}

		protected override void InnerExecute(string code, string documentName)
		{
			try
			{
				_jsEngine.Execute(code, documentName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
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
				result = _jsEngine.CallFunction(functionName, processedArgs);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return OriginalTypeConverter.ConvertToType<T>(result);
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			try
			{
				result = _jsEngine.HasVariable(variableName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			try
			{
				result = _jsEngine.GetVariableValue(variableName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return OriginalTypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToScriptType(value);

			try
			{
				_jsEngine.SetVariableValue(variableName, processedValue);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				_jsEngine.RemoveVariable(variableName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			object processedValue = MapToScriptType(value);

			try
			{
				_jsEngine.EmbedHostObject(itemName, processedValue);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			try
			{
				_jsEngine.EmbedHostType(itemName, type);
			}
			catch (OriginalJsException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}
		}

		protected override void InnerInterrupt()
		{
			_jsEngine.Interrupt();
		}

		protected override void InnerCollectGarbage()
		{
			_jsEngine.CollectGarbage();
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
			get { return _engineVersion; }
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


		public override void ExecuteFile(string path, Encoding encoding = null)
		{
			VerifyNotDisposed();

			if (path == null)
			{
				throw new ArgumentNullException(
					"path", string.Format(CoreStrings.Common_ArgumentIsNull, "path"));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "path"), "path");
			}

			if (!File.Exists(path))
			{
				throw new FileNotFoundException(
					string.Format(CoreStrings.Common_FileNotExist, path), path);
			}

			try
			{
				_jsEngine.ExecuteFile(path, encoding);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}
		}

		public override void ExecuteResource(string resourceName, Type type)
		{
			VerifyNotDisposed();

			if (resourceName == null)
			{
				throw new ArgumentNullException(
					"resourceName", string.Format(CoreStrings.Common_ArgumentIsNull, "resourceName"));
			}

			if (type == null)
			{
				throw new ArgumentNullException(
					"type", string.Format(CoreStrings.Common_ArgumentIsNull, "type"));
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			try
			{
				_jsEngine.ExecuteResource(resourceName, type);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertScriptExceptionToHostException(e);
			}
		}

		public override void ExecuteResource(string resourceName, Assembly assembly)
		{
			VerifyNotDisposed();

			if (resourceName == null)
			{
				throw new ArgumentNullException(
					"resourceName", string.Format(CoreStrings.Common_ArgumentIsNull, "resourceName"));
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(
					"assembly", string.Format(CoreStrings.Common_ArgumentIsNull, "assembly"));
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			try
			{
				_jsEngine.ExecuteResource(resourceName, assembly);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertScriptExceptionToHostException(e);
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

		#endregion
	}
}