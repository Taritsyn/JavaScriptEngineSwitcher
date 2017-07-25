using System;
using System.IO;
using System.Reflection;
using System.Text;

using OriginalCompatibilityMode = Jurassic.CompatibilityMode;
using OriginalConcatenatedString = Jurassic.ConcatenatedString;
using OriginalErrorInstance = Jurassic.Library.ErrorInstance;
using OriginalJsEngine = Jurassic.ScriptEngine;
using OriginalJsException = Jurassic.JavaScriptException;
using OriginalNull = Jurassic.Null;
using OriginalStringScriptSource = Jurassic.StringScriptSource;
using OriginalTypeConverter = Jurassic.TypeConverter;
using OriginalUndefined = Jurassic.Undefined;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Adapter for the Jurassic JS engine
	/// </summary>
	public sealed class JurassicJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "JurassicJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "Jul 13, 2017";

		/// <summary>
		/// Jurassic JS engine
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
		/// Constructs a instance of adapter for the Jurassic JS engine
		/// </summary>
		public JurassicJsEngine()
			: this(new JurassicSettings())
		{ }

		/// <summary>
		/// Constructs a instance of adapter for the Jurassic JS engine
		/// </summary>
		/// <param name="settings">Settings of the Jurassic JS engine</param>
		public JurassicJsEngine(JurassicSettings settings)
		{
			JurassicSettings jurassicSettings = settings ?? new JurassicSettings();

			try
			{
				_jsEngine = new OriginalJsEngine
				{
					EnableDebugging = jurassicSettings.EnableDebugging,
					CompatibilityMode = OriginalCompatibilityMode.Latest,
					EnableExposedClrTypes = true,
					EnableILAnalysis = jurassicSettings.EnableIlAnalysis,
					ForceStrictMode = jurassicSettings.StrictMode
				};
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						EngineName, e.Message), EngineName, EngineVersion, e);
			}
		}


		private string GetUniqueDocumentName(string documentName, bool isFile)
		{
			string uniqueDocumentName;

			if (!_jsEngine.EnableDebugging)
			{
				uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);
			}
			else
			{
				uniqueDocumentName = isFile ? documentName : null;
			}

			return uniqueDocumentName;
		}

		#region Mapping

		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToScriptType(object value)
		{
			if (value == null)
			{
				return OriginalNull.Value;
			}

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
			if (value is OriginalConcatenatedString)
			{
				return value.ToString();
			}

			if (value is OriginalNull)
			{
				return null;
			}

			if (value is OriginalUndefined)
			{
				return Undefined.Value;
			}

			return value;
		}

		private JsRuntimeException ConvertScriptExceptionToHostException(
			OriginalJsException scriptException)
		{
			var errorValue = scriptException.ErrorObject as OriginalErrorInstance;
			string message = scriptException.Message;
			if (errorValue != null)
			{
				message = !string.IsNullOrEmpty(errorValue.Stack) ? errorValue.Stack : errorValue.Message;
			}

			var hostException = new JsRuntimeException(message, EngineName, EngineVersion,
				scriptException)
			{
				Category = scriptException.Name,
				LineNumber = scriptException.LineNumber,
				ColumnNumber = 0,
				SourceFragment = string.Empty
			};

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
			string uniqueDocumentName = GetUniqueDocumentName(documentName, false);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new OriginalStringScriptSource(expression, uniqueDocumentName);
					result = _jsEngine.Evaluate(source);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
				catch (NotImplementedException e)
				{
					throw new JsRuntimeException(e.Message, e);
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

			return OriginalTypeConverter.ConvertTo<T>(_jsEngine, result);
		}

		protected override void InnerExecute(string code)
		{
			InnerExecute(code, null);
		}

		protected override void InnerExecute(string code, string documentName)
		{
			string uniqueDocumentName = GetUniqueDocumentName(documentName, false);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new OriginalStringScriptSource(code, uniqueDocumentName);
					_jsEngine.Execute(source);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
				catch (NotImplementedException e)
				{
					throw new JsRuntimeException(e.Message, e);
				}
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			int argumentCount = args.Length;
			var processedArgs = new object[argumentCount];

			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					processedArgs[argumentIndex] = MapToScriptType(args[argumentIndex]);
				}
			}

			object result;

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsEngine.CallGlobalFunction(functionName, processedArgs);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
				catch (NotImplementedException e)
				{
					throw new JsRuntimeException(e.Message, e);
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return OriginalTypeConverter.ConvertTo<T>(_jsEngine, result);
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			lock (_executionSynchronizer)
			{
				result = _jsEngine.HasGlobalValue(variableName);
				if (result)
				{
					object value = _jsEngine.GetGlobalValue(variableName);
					result = value.ToString() != "undefined";
				}
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsEngine.GetGlobalValue(variableName);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return OriginalTypeConverter.ConvertTo<T>(_jsEngine, result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToScriptType(value);

			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.SetGlobalValue(variableName, processedValue);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InnerSetVariableValue(variableName, Undefined.Value);
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			object processedValue = MapToScriptType(value);

			lock (_executionSynchronizer)
			{
				try
				{
					var delegateValue = processedValue as Delegate;
					if (delegateValue != null)
					{
						_jsEngine.SetGlobalFunction(itemName, delegateValue);
					}
					else
					{
						_jsEngine.SetGlobalValue(itemName, processedValue);
					}
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.SetGlobalValue(itemName, type);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
			}
		}

		protected override void InnerInterrupt()
		{
			throw new NotImplementedException();
		}

		protected override void InnerCollectGarbage()
		{
			throw new NotImplementedException();
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
			get { return false; }
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports garbage collection
		/// </summary>
		public override bool SupportsGarbageCollection
		{
			get { return false; }
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

			string uniqueDocumentName = GetUniqueDocumentName(path, true);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new FileScriptSource(uniqueDocumentName, path, encoding);
					_jsEngine.Execute(source);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
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

			Assembly assembly = type.GetTypeInfo().Assembly;
			string nameSpace = type.Namespace;
			string resourceFullName = nameSpace != null ? nameSpace + "." + resourceName : resourceName;
			string uniqueDocumentName = GetUniqueDocumentName(resourceFullName, false);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new ResourceScriptSource(uniqueDocumentName, resourceFullName, assembly);
					_jsEngine.Execute(source);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
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

			string uniqueDocumentName = GetUniqueDocumentName(resourceName, false);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new ResourceScriptSource(uniqueDocumentName, resourceName, assembly);
					_jsEngine.Execute(source);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptExceptionToHostException(e);
				}
			}
		}

		#endregion

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