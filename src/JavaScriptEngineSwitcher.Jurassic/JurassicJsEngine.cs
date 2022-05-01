using System;
using System.IO;
using System.Reflection;
using System.Text;

using OriginalCompatibilityMode = Jurassic.CompatibilityMode;
using OriginalCompiledScript = Jurassic.CompiledScript;
using OriginalConcatenatedString = Jurassic.ConcatenatedString;
using OriginalEngine = Jurassic.ScriptEngine;
using OriginalErrorInstance = Jurassic.Library.ErrorInstance;
using OriginalJavaScriptException = Jurassic.JavaScriptException;
using OriginalNull = Jurassic.Null;
using OriginalStringScriptSource = Jurassic.StringScriptSource;
using OriginalSyntaxException = Jurassic.Compiler.SyntaxErrorException;
using OriginalTypeConverter = Jurassic.TypeConverter;
using OriginalUndefined = Jurassic.Undefined;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;
using WrapperUsageException = JavaScriptEngineSwitcher.Core.JsUsageException;

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
		private const string EngineVersion = "Apr 30, 2022";

		/// <summary>
		/// Jurassic JS engine
		/// </summary>
		private OriginalEngine _jsEngine;

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
		/// Constructs an instance of adapter for the Jurassic JS engine
		/// </summary>
		public JurassicJsEngine()
			: this(new JurassicSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the Jurassic JS engine
		/// </summary>
		/// <param name="settings">Settings of the Jurassic JS engine</param>
		public JurassicJsEngine(JurassicSettings settings)
		{
			JurassicSettings jurassicSettings = settings ?? new JurassicSettings();

			try
			{
				_jsEngine = new OriginalEngine
				{
					CompatibilityMode = OriginalCompatibilityMode.Latest,
					DisableClrCollectionsExposingByValue = !jurassicSettings.EnableHostCollectionsEmbeddingByValue,
					EnableExposedClrTypes = true,
					EnableILAnalysis = jurassicSettings.EnableIlAnalysis,
					ForceStrictMode = jurassicSettings.StrictMode
				};
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

		private static WrapperCompilationException WrapSyntaxException(
			OriginalSyntaxException originalSyntaxException)
		{
			string description = originalSyntaxException.Message;
			string type = JsErrorType.Syntax;
			string documentName = originalSyntaxException.SourcePath;
			int lineNumber = originalSyntaxException.LineNumber;
			string message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName,
				lineNumber, 0);

			var wrapperCompilationException = new WrapperCompilationException(message, EngineName, EngineVersion,
				originalSyntaxException)
			{
				Description = description,
				Type = type,
				DocumentName = documentName,
				LineNumber = lineNumber
			};

			return wrapperCompilationException;
		}

		private WrapperException WrapJavaScriptException(
			OriginalJavaScriptException originalJavaScriptException)
		{
			WrapperException wrapperException;
			string message = originalJavaScriptException.Message;
			string messageWithCallStack = string.Empty;
			string description = originalJavaScriptException.ErrorMessage;
			string type = originalJavaScriptException.ErrorType.ToString();
			string documentName = originalJavaScriptException.SourcePath ?? string.Empty;
			int lineNumber = originalJavaScriptException.LineNumber;
			string callStack = string.Empty;

			object errorObject = originalJavaScriptException.GetErrorObject(_jsEngine);
			var errorValue = errorObject as OriginalErrorInstance;
			if (errorValue != null)
			{
				messageWithCallStack = errorValue.Stack;
			}

			if (!string.IsNullOrEmpty(type))
			{
				WrapperScriptException wrapperScriptException;
				if (type == JsErrorType.Syntax)
				{
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName,
						lineNumber, 0);

					wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
						originalJavaScriptException);
				}
				else
				{
					if (message.Length < messageWithCallStack.Length)
					{
						string rawCallStack = messageWithCallStack
							.TrimStart(message)
							.TrimStart(new char[] { '\n', '\r' })
							;
						ErrorLocationItem[] callStackItems = JsErrorHelpers.ParseErrorLocation(rawCallStack);

						if (callStackItems.Length > 0)
						{
							FixCallStackItems(callStackItems);
							callStack = JsErrorHelpers.StringifyErrorLocationItems(callStackItems);

							if (string.IsNullOrWhiteSpace(documentName))
							{
								documentName = callStackItems[0].DocumentName;
							}
						}
					}

					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, callStack);

					wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
						originalJavaScriptException)
					{
						CallStack = callStack
					};
				}
				wrapperScriptException.Type = type;
				wrapperScriptException.DocumentName = documentName;
				wrapperScriptException.LineNumber = lineNumber;

				wrapperException = wrapperScriptException;
			}
			else
			{
				wrapperException = new WrapperException(message, EngineName, EngineVersion,
					originalJavaScriptException);
			}

			wrapperException.Description = description;

			return wrapperException;
		}

		/// <summary>
		/// Fixes a function name in call stack items
		/// </summary>
		/// <param name="callStackItems">An array of <see cref="ErrorLocationItem"/> instances</param>
		private static void FixCallStackItems(ErrorLocationItem[] callStackItems)
		{
			foreach (ErrorLocationItem callStackItem in callStackItems)
			{
				string functionName = callStackItem.FunctionName;
				if (functionName.Length > 0)
				{
					if (functionName == "anonymous")
					{
						callStackItem.FunctionName = "Anonymous function";
					}
				}
				else
				{
					callStackItem.FunctionName = "Global code";
				}
			}
		}

		#endregion

		#region JsEngineBase overrides

		protected override IPrecompiledScript InnerPrecompile(string code)
		{
			return InnerPrecompile(code, null);
		}

		protected override IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			OriginalCompiledScript compiledScript;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new OriginalStringScriptSource(code, uniqueDocumentName);
					compiledScript = OriginalCompiledScript.Compile(source);
				}
				catch (OriginalSyntaxException e)
				{
					throw WrapSyntaxException(e);
				}
			}

			return new JurassicPrecompiledScript(compiledScript);
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
				try
				{
					var source = new OriginalStringScriptSource(expression, uniqueDocumentName);
					result = _jsEngine.Evaluate(source);
				}
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
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
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new OriginalStringScriptSource(code, uniqueDocumentName);
					_jsEngine.Execute(source);
				}
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
				}
			}
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			var jurassicPrecompiledScript = precompiledScript as JurassicPrecompiledScript;
			if (jurassicPrecompiledScript == null)
			{
				throw new WrapperUsageException(
					string.Format(CoreStrings.Usage_CannotConvertPrecompiledScriptToInternalType,
						typeof(JurassicPrecompiledScript).FullName),
					Name, Version
				);
			}

			lock (_executionSynchronizer)
			{
				try
				{
					jurassicPrecompiledScript.CompiledScript.Execute(_jsEngine);
				}
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
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
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
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
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
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
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
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
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
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
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
				}
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
			get { return false; }
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports garbage collection
		/// </summary>
		public override bool SupportsGarbageCollection
		{
			get { return false; }
		}


		public override IPrecompiledScript PrecompileFile(string path, Encoding encoding = null)
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

			OriginalCompiledScript compiledScript;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(path);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new FileScriptSource(uniqueDocumentName, path, encoding);
					compiledScript = OriginalCompiledScript.Compile(source);
				}
				catch (OriginalSyntaxException e)
				{
					throw WrapSyntaxException(e);
				}
				catch (FileNotFoundException)
				{
					throw;
				}
			}

			return new JurassicPrecompiledScript(compiledScript);
		}

		public override IPrecompiledScript PrecompileResource(string resourceName, Type type)
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

#if NET40
			Assembly assembly = type.Assembly;
#else
			Assembly assembly = type.GetTypeInfo().Assembly;
#endif
			string nameSpace = type.Namespace;
			string resourceFullName = nameSpace != null ? nameSpace + "." + resourceName : resourceName;

			OriginalCompiledScript compiledScript;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(resourceFullName);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new ResourceScriptSource(uniqueDocumentName, resourceFullName, assembly);
					compiledScript = OriginalCompiledScript.Compile(source);
				}
				catch (OriginalSyntaxException e)
				{
					throw WrapSyntaxException(e);
				}
				catch (NullReferenceException)
				{
					throw;
				}
			}

			return new JurassicPrecompiledScript(compiledScript);
		}

		public override IPrecompiledScript PrecompileResource(string resourceName, Assembly assembly)
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

			OriginalCompiledScript compiledScript;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(resourceName);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new ResourceScriptSource(uniqueDocumentName, resourceName, assembly);
					compiledScript = OriginalCompiledScript.Compile(source);
				}
				catch (OriginalSyntaxException e)
				{
					throw WrapSyntaxException(e);
				}
				catch (NullReferenceException)
				{
					throw;
				}
			}

			return new JurassicPrecompiledScript(compiledScript);
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

			string uniqueDocumentName = _documentNameManager.GetUniqueName(path);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new FileScriptSource(uniqueDocumentName, path, encoding);
					_jsEngine.Execute(source);
				}
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
				}
				catch (FileNotFoundException)
				{
					throw;
				}
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

#if NET40
			Assembly assembly = type.Assembly;
#else
			Assembly assembly = type.GetTypeInfo().Assembly;
#endif
			string nameSpace = type.Namespace;
			string resourceFullName = nameSpace != null ? nameSpace + "." + resourceName : resourceName;
			string uniqueDocumentName = _documentNameManager.GetUniqueName(resourceFullName);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new ResourceScriptSource(uniqueDocumentName, resourceFullName, assembly);
					_jsEngine.Execute(source);
				}
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
				}
				catch (NullReferenceException)
				{
					throw;
				}
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

			string uniqueDocumentName = _documentNameManager.GetUniqueName(resourceName);

			lock (_executionSynchronizer)
			{
				try
				{
					var source = new ResourceScriptSource(uniqueDocumentName, resourceName, assembly);
					_jsEngine.Execute(source);
				}
				catch (OriginalJavaScriptException e)
				{
					throw WrapJavaScriptException(e);
				}
				catch (NullReferenceException)
				{
					throw;
				}
			}
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
				}
			}
		}

		#endregion

		#endregion
	}
}