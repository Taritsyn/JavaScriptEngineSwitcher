using System;
using System.IO;
using System.Reflection;
using System.Text;

using OriginalCompilationException = MsieJavaScriptEngine.JsCompilationException;
using OriginalEngine = MsieJavaScriptEngine.MsieJsEngine;
using OriginalEngineException = MsieJavaScriptEngine.JsEngineException;
using OriginalEngineLoadException = MsieJavaScriptEngine.JsEngineLoadException;
using OriginalEngineMode = MsieJavaScriptEngine.JsEngineMode;
using OriginalEngineSettings = MsieJavaScriptEngine.JsEngineSettings;
using OriginalException = MsieJavaScriptEngine.JsException;
using OriginalFatalException = MsieJavaScriptEngine.JsFatalException;
using OriginalInterruptedException = MsieJavaScriptEngine.JsInterruptedException;
using OriginalRuntimeException = MsieJavaScriptEngine.JsRuntimeException;
using OriginalScriptException = MsieJavaScriptEngine.JsScriptException;
using OriginalTypeConverter = MsieJavaScriptEngine.Utilities.TypeConverter;
using OriginalUndefined = MsieJavaScriptEngine.Undefined;
using OriginalUsageException = MsieJavaScriptEngine.JsUsageException;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperEngineException = JavaScriptEngineSwitcher.Core.JsEngineException;
using WrapperEngineLoadException = JavaScriptEngineSwitcher.Core.JsEngineLoadException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperFatalException = JavaScriptEngineSwitcher.Core.JsFatalException;
using WrapperInterruptedException = JavaScriptEngineSwitcher.Core.JsInterruptedException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;
using WrapperUsageException = JavaScriptEngineSwitcher.Core.JsUsageException;

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
		private OriginalEngine _jsEngine;


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
				_jsEngine = new OriginalEngine(new OriginalEngineSettings
				{
					EnableDebugging = msieSettings.EnableDebugging,
					EngineMode = Utils.GetEnumFromOtherEnum<JsEngineMode, OriginalEngineMode>(
						msieSettings.EngineMode),
					UseEcmaScript5Polyfill = msieSettings.UseEcmaScript5Polyfill,
					UseJson2Library = msieSettings.UseJson2Library
				});
				_engineVersion = _jsEngine.Mode;
			}
			catch (OriginalUsageException e)
			{
				throw JsErrorHelpers.WrapEngineLoadException(e, EngineName, _engineVersion);
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
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

		private WrapperException WrapJsException(OriginalException originalException)
		{
			WrapperException wrapperException;

			var originalScriptException = originalException as OriginalScriptException;
			if (originalScriptException != null)
			{
				WrapperScriptException wrapperScriptException;

				var originalRuntimeException = originalScriptException as OriginalRuntimeException;
				if (originalRuntimeException != null)
				{
					WrapperRuntimeException wrapperRuntimeException;
					if (originalRuntimeException is OriginalInterruptedException)
					{
						wrapperRuntimeException = new WrapperInterruptedException(originalScriptException.Message,
							EngineName, _engineVersion, originalRuntimeException);
					}
					else
					{
						wrapperRuntimeException = new WrapperRuntimeException(originalScriptException.Message,
							EngineName, _engineVersion, originalRuntimeException);
					}
					wrapperRuntimeException.CallStack = originalRuntimeException.CallStack;

					wrapperScriptException = wrapperRuntimeException;
				}
				else if (originalScriptException is OriginalCompilationException)
				{
					wrapperScriptException = new WrapperCompilationException(originalScriptException.Message,
						EngineName, _engineVersion, originalScriptException);
				}
				else
				{
					wrapperScriptException = new WrapperScriptException(originalScriptException.Message,
						EngineName, _engineVersion, originalScriptException);
				}

				wrapperScriptException.Type = originalScriptException.Type;
				wrapperScriptException.DocumentName = originalScriptException.DocumentName;
				wrapperScriptException.LineNumber = originalScriptException.LineNumber;
				wrapperScriptException.ColumnNumber = originalScriptException.ColumnNumber;
				wrapperScriptException.SourceFragment = originalScriptException.SourceFragment;

				wrapperException = wrapperScriptException;
			}
			else
			{
				if (originalException is OriginalUsageException)
				{
					wrapperException = new WrapperUsageException(originalException.Message,
						EngineName, _engineVersion, originalException);
				}
				else if (originalException is OriginalEngineException)
				{
					if (originalException is OriginalEngineLoadException)
					{
						wrapperException = new WrapperEngineLoadException(originalException.Message,
							EngineName, _engineVersion, originalException);
					}
					else
					{
						wrapperException = new WrapperEngineException(originalException.Message,
							EngineName, _engineVersion, originalException);
					}
				}
				else if (originalException is OriginalFatalException)
				{
					wrapperException = new WrapperFatalException(originalException.Message,
						EngineName, _engineVersion, originalException);
				}
				else
				{
					wrapperException = new WrapperException(originalException.Message,
						EngineName, _engineVersion, originalException);
				}
			}

			wrapperException.Description = originalException.Description;

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
			object result;

			try
			{
				result = _jsEngine.Evaluate(expression, documentName);
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
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
			catch (OriginalException e)
			{
				throw WrapJsException(e);
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
			catch (OriginalException e)
			{
				throw WrapJsException(e);
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
			catch (OriginalException e)
			{
				throw WrapJsException(e);
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
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				_jsEngine.RemoveVariable(variableName);
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			object processedValue = MapToScriptType(value);

			try
			{
				_jsEngine.EmbedHostObject(itemName, processedValue);
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
				_jsEngine.EmbedHostType(itemName, type);
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
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
		/// Gets a value that indicates if the JS engine supports script pre-сompilation
		/// </summary>
		public override bool SupportsScriptPrecompilation
		{
			get { return false; }
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
					nameof(path),
					string.Format(CoreStrings.Common_ArgumentIsNull, nameof(path))
				);
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, nameof(path)),
					nameof(path)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(path))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Usage_InvalidFileNameFormat, path),
					nameof(path)
				);
			}

			try
			{
				_jsEngine.ExecuteFile(path, encoding);
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			catch (FileNotFoundException)
			{
				throw;
			}
		}

		public override void ExecuteResource(string resourceName, Type type)
		{
			VerifyNotDisposed();

			if (resourceName == null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(CoreStrings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (type == null)
			{
				throw new ArgumentNullException(
					nameof(type),
					string.Format(CoreStrings.Common_ArgumentIsNull, nameof(type))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Usage_InvalidResourceNameFormat, resourceName),
					nameof(resourceName)
				);
			}

			try
			{
				_jsEngine.ExecuteResource(resourceName, type);
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			catch (NullReferenceException)
			{
				throw;
			}
		}

		public override void ExecuteResource(string resourceName, Assembly assembly)
		{
			VerifyNotDisposed();

			if (resourceName == null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(CoreStrings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(
					nameof(assembly),
					string.Format(CoreStrings.Common_ArgumentIsNull, nameof(assembly))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Usage_InvalidResourceNameFormat, resourceName),
					nameof(resourceName)
				);
			}

			try
			{
				_jsEngine.ExecuteResource(resourceName, assembly);
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			catch (NullReferenceException)
			{
				throw;
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