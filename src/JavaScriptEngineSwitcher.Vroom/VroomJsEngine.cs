using System;
using System.Collections.Generic;
#if NET45 || NETSTANDARD
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
#endif
using System.Text;
using System.Text.RegularExpressions;

using OriginalAssemblyLoader = VroomJs.AssemblyLoader;
using OriginalContext = VroomJs.JsContext;
using OriginalEngine = VroomJs.JsEngine;
using OriginalException = VroomJs.JsException;
using OriginalInteropException = VroomJs.JsInteropException;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Helpers;
#if NET40
using JavaScriptEngineSwitcher.Core.Polyfills.System.Runtime.InteropServices;
#endif
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperEngineLoadException = JavaScriptEngineSwitcher.Core.JsEngineLoadException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperInterruptedException = JavaScriptEngineSwitcher.Core.JsInterruptedException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;

using JavaScriptEngineSwitcher.Vroom.Constants;
using JavaScriptEngineSwitcher.Vroom.Resources;
using JavaScriptEngineSwitcher.Vroom.Utilities;

namespace JavaScriptEngineSwitcher.Vroom
{
	/// <summary>
	/// Adapter for the Vroom JS engine (cross-platform bridge to the V8 JS engine)
	/// </summary>
	public sealed class VroomJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "VroomJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "3.17.16.2";

		/// <summary>
		/// Regular expression for working with the script error message
		/// </summary>
		private static readonly Regex _scriptErrorMessageRegex =
			new Regex(@"^" + CommonRegExps.DocumentNamePattern + ": " +
				@"Uncaught " +
				@"(?:" + CommonRegExps.JsFullNamePattern + @": )?" +
				@"(?<description>[\s\S]+?) " +
				@"at line: \d+ column: \d+\.$");

		/// <summary>
		/// Vroom JS engine
		/// </summary>
		private OriginalEngine _jsEngine;

		/// <summary>
		/// JS context
		/// </summary>
		private OriginalContext _jsContext;

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

		/// <summary>
		/// List of host items
		/// </summary>
		private readonly Dictionary<string, object> _hostItems = new Dictionary<string, object>();

		/// <summary>
		/// Synchronizer of JS engine initialization
		/// </summary>
		private static readonly object _initializationSynchronizer = new object();

		/// <summary>
		/// Flag indicating whether the JS engine is initialized
		/// </summary>
		private static bool _initialized;


		/// <summary>
		/// Unique document name manager
		/// </summary>
		private readonly UniqueDocumentNameManager _documentNameManager =
			new UniqueDocumentNameManager(DefaultDocumentName);


		/// <summary>
		/// Constructs an instance of adapter for the Vroom JS engine
		/// (cross-platform bridge to the V8 JS engine)
		/// </summary>
		public VroomJsEngine()
			: this(new VroomSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the Vroom JS engine
		/// (cross-platform bridge to the V8 JS engine)
		/// </summary>
		/// <param name="settings">Settings of the Vroom JS engine</param>
		public VroomJsEngine(VroomSettings settings)
		{
			Initialize();

			VroomSettings vroomSettings = settings ?? new VroomSettings();

			try
			{
				_jsEngine = new OriginalEngine(vroomSettings.MaxYoungSpaceSize, vroomSettings.MaxOldSpaceSize);
				_jsContext = _jsEngine.CreateContext();
			}
			catch (TypeInitializationException e)
			{
				Exception innerException = e.InnerException;
				if (innerException != null)
				{
					var typeLoadException = innerException as TypeLoadException;
					if (typeLoadException != null)
					{
						throw WrapTypeLoadException(typeLoadException);
					}
					else
					{
						throw JsErrorHelpers.WrapUnknownEngineLoadException(innerException,
							EngineName, EngineVersion);
					}
				}

				throw JsErrorHelpers.WrapUnknownEngineLoadException(e, EngineName, EngineVersion);
			}
			catch (Exception e)
			{
				throw JsErrorHelpers.WrapUnknownEngineLoadException(e, EngineName, EngineVersion);
			}
			finally
			{
				if (_jsContext == null)
				{
					Dispose();
				}
			}
		}


		/// <summary>
		/// Initializes a JS engine
		/// </summary>
		private static void Initialize()
		{
			if (_initialized)
			{
				return;
			}

			lock (_initializationSynchronizer)
			{
				if (_initialized)
				{
					return;
				}

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					try
					{
						OriginalAssemblyLoader.EnsureLoaded();
					}
					catch (Exception e)
					{
						throw WrapAssemblyLoaderException(e);
					}
				}

				_initialized = true;
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
				return null;
			}

			return value;
		}

		private static WrapperException WrapJsException(OriginalException originalException)
		{
			WrapperException wrapperException;
			string message = originalException.Message;
			string description = message;
			string type = originalException.Type;
			string documentName = originalException.Resource;
			int lineNumber = originalException.Line;
			int columnNumber = originalException.Column;

			if (originalException is OriginalInteropException)
			{
				wrapperException = new WrapperException(message, EngineName, EngineVersion, originalException);
			}
			else if (type == null && message.Equals(":  at line: 0 column: 1.", StringComparison.Ordinal))
			{
				wrapperException = new WrapperInterruptedException(CoreStrings.Runtime_ScriptInterrupted,
					EngineName, EngineVersion, originalException);
			}
			else
			{
				Match scriptErrorMessageMatch = _scriptErrorMessageRegex.Match(message);
				if (scriptErrorMessageMatch.Success)
				{
					WrapperScriptException wrapperScriptException;
					description = scriptErrorMessageMatch.Groups["description"].Value;
					message = JsErrorHelpers.GenerateErrorMessage(type, description, documentName,
						lineNumber, columnNumber);

					if (type == JsErrorType.Syntax)
					{
						wrapperScriptException = new WrapperCompilationException(message,
							EngineName, EngineVersion, originalException);
					}
					else
					{
						wrapperScriptException = new WrapperRuntimeException(message,
							EngineName, EngineVersion, originalException);
					}
					wrapperScriptException.Type = type;
					wrapperScriptException.DocumentName = documentName;
					wrapperScriptException.LineNumber = lineNumber;
					wrapperScriptException.ColumnNumber = columnNumber;

					wrapperException = wrapperScriptException;

				}
				else
				{
					wrapperException = new WrapperException(message, EngineName, EngineVersion,
						originalException);
				}
			}

			wrapperException.Description = description;

			return wrapperException;
		}

		private static WrapperEngineLoadException WrapAssemblyLoaderException(
			Exception originalAssemblyLoaderException)
		{
			string originalMessage = originalAssemblyLoaderException.Message;
			string jsEngineNotLoadedPart = string.Format(CoreStrings.Engine_JsEngineNotLoaded, EngineName);
			string message;
			Architecture osArchitecture = RuntimeInformation.OSArchitecture;

			if ((osArchitecture == Architecture.X64 || osArchitecture == Architecture.X86)
				&& originalMessage.StartsWith("Couldn't load native assembly at "))
			{
				message = jsEngineNotLoadedPart + " " + Strings.Engine_VcRedist2012And2015InstallationRequired;
			}
			else
			{
				message = jsEngineNotLoadedPart + " " +
					string.Format(CoreStrings.Common_SeeOriginalErrorMessage, originalMessage);
			}

			return new WrapperEngineLoadException(message, EngineName, EngineVersion, originalAssemblyLoaderException);
		}

		private static WrapperEngineLoadException WrapTypeLoadException(
			TypeLoadException originalTypeLoadException)
		{
			string originalMessage = originalTypeLoadException.Message;
			string jsEngineNotLoadedPart = string.Format(CoreStrings.Engine_JsEngineNotLoaded, EngineName);
			string message;
			bool isMonoRuntime = Utils.IsMonoRuntime();

			if (originalTypeLoadException is DllNotFoundException
				&& ((isMonoRuntime && originalMessage == DllName.Universal)
					|| originalMessage.ContainsQuotedValue(DllName.Universal)))
			{
				const string buildInstructionsUrl = "https://github.com/pauldotknopf/vroomjs-core#maclinux";

				StringBuilder messageBuilder = StringBuilderPool.GetBuilder();
				messageBuilder.Append(jsEngineNotLoadedPart);
				messageBuilder.Append(" ");

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					Architecture osArchitecture = RuntimeInformation.OSArchitecture;

					if (osArchitecture == Architecture.X64 || osArchitecture == Architecture.X86)
					{
						messageBuilder.AppendFormat(CoreStrings.Common_SeeOriginalErrorMessage, originalMessage);
					}
					else
					{
						messageBuilder.AppendFormat(CoreStrings.Engine_ProcessorArchitectureNotSupported,
							osArchitecture.ToString().ToLowerInvariant());
					}
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
					|| RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					messageBuilder.AppendFormat(CoreStrings.Engine_AssemblyNotFound, DllName.ForUnix);
					messageBuilder.Append(" ");
					messageBuilder.AppendFormat(Strings.Engine_BuildNativeAssembly, DllName.ForUnix,
						buildInstructionsUrl);
				}
				else
				{
					messageBuilder.Append(CoreStrings.Engine_OperatingSystemNotSupported);
				}

				message = messageBuilder.ToString();
				StringBuilderPool.ReleaseBuilder(messageBuilder);
			}
			else
			{
				message = jsEngineNotLoadedPart + " " +
					string.Format(CoreStrings.Common_SeeOriginalErrorMessage, originalMessage);
			}

			return new WrapperEngineLoadException(message, EngineName, EngineVersion, originalTypeLoadException);
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
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsContext.Execute(expression, uniqueDocumentName);
				}
				catch (OriginalException e)
				{
					throw WrapJsException(e);
				}
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
					_jsContext.Execute(code, uniqueDocumentName);
				}
				catch (OriginalException e)
				{
					throw WrapJsException(e);
				}
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			string functionCallExpression;
			int argumentCount = args.Length;

			if (argumentCount > 0)
			{
				var functionCallBuilder = StringBuilderPool.GetBuilder();
				functionCallBuilder.Append(functionName);
				functionCallBuilder.Append("(");

				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					object value = args[argumentIndex];
					string serializedValue = SimplisticJsSerializer.Serialize(value);

					if (argumentIndex > 0)
					{
						functionCallBuilder.Append(", ");
					}
					functionCallBuilder.Append(serializedValue);
				}

				functionCallBuilder.Append(");");

				functionCallExpression = functionCallBuilder.ToString();
				StringBuilderPool.ReleaseBuilder(functionCallBuilder);
			}
			else
			{
				functionCallExpression = string.Format("{0}();", functionName);
			}

			object result = Evaluate(functionCallExpression);

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
					result = _jsContext.GetVariable(variableName);
				}
				catch (OriginalException e)
				{
					throw WrapJsException(e);
				}
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
			object processedValue = MapToScriptType(value);

			lock (_executionSynchronizer)
			{
				try
				{
					_jsContext.SetVariable(variableName, processedValue);
				}
				catch (OriginalException e)
				{
					throw WrapJsException(e);
				}
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			string expression = string.Format(@"if (typeof {0} !== 'undefined') {{
	{0} = undefined;
}}", variableName);

			lock (_executionSynchronizer)
			{
				try
				{
					_jsContext.Execute(expression);

					if (_hostItems.ContainsKey(variableName))
					{
						_hostItems.Remove(variableName);
					}
				}
				catch (OriginalException e)
				{
					throw WrapJsException(e);
				}
			}
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			object processedValue = MapToScriptType(value);
			InnerEmbedHostItem(itemName, processedValue);
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			InnerEmbedHostItem(itemName, type);
		}

		private void InnerEmbedHostItem(string itemName, object value)
		{
			lock (_executionSynchronizer)
			{
				object oldValue = null;
				if (_hostItems.ContainsKey(itemName))
				{
					oldValue = _hostItems[itemName];
				}
				_hostItems[itemName] = value;

				try
				{
					var delegateValue = value as Delegate;
					if (delegateValue != null)
					{
						_jsContext.SetFunction(itemName, delegateValue);
					}
					else
					{
						_jsContext.SetVariable(itemName, value);
					}
				}
				catch (OriginalException e)
				{
					if (oldValue != null)
					{
						_hostItems[itemName] = oldValue;
					}
					else
					{
						_hostItems.Remove(itemName);
					}

					throw WrapJsException(e);
				}
			}
		}

		protected override void InnerInterrupt()
		{
			_jsEngine.TerminateExecution();
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
			get { return true; }
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports garbage collection
		/// </summary>
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

					if (_jsEngine != null)
					{
						_jsEngine.Dispose();
						_jsEngine = null;
					}

					_hostItems?.Clear();
				}
			}
		}

		#endregion

		#endregion
	}
}