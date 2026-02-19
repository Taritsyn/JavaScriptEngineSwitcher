using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Jering.Javascript.NodeJS;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;
using WrapperTimeoutException = JavaScriptEngineSwitcher.Core.JsTimeoutException;
using WrapperUsageException = JavaScriptEngineSwitcher.Core.JsUsageException;

using JavaScriptEngineSwitcher.Node.Helpers;

namespace JavaScriptEngineSwitcher.Node
{
	/// <summary>
	/// Adapter for the Node JS engine
	/// </summary>
	public sealed class NodeJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of resource, which contains a JS engine helpers
		/// </summary>
		private const string ENGINE_HELPERS_RESOURCE_NAME = "JavaScriptEngineSwitcher.Node.Resources.engine-helpers.js";

		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "NodeJsEngine";

		/// <summary>
		/// Node JS service
		/// </summary>
		private INodeJSService _jsService;

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private string _engineVersion = "0.0.0";

		/// <summary>
		/// JS engine identifier
		/// </summary>
		private string _engineId;

		/// <summary>
		/// Number of milliseconds to wait before the script execution times out
		/// </summary>
		private int _executionTimeout = -1;

		/// <summary>
		/// Unique document name manager
		/// </summary>
		private UniqueDocumentNameManager _documentNameManager = new UniqueDocumentNameManager(DefaultDocumentName);

		/// <summary>
		/// Regular expression for working with the timeout error message
		/// </summary>
		private static readonly Regex _timeoutErrorMessage = new Regex(@"^(?:Script execution|The Node invocation) " +
			@"timed out after \d+ms");

		/// <summary>
		/// Regular expression for working with the error details
		/// </summary>
		private static readonly Regex _errorDetailsRegex = new Regex(@"^(?<description>[^\r\n]+)(?:\r\n|\n|\r)" +
			@"(?:" +
				@"(?<documentName>" + CommonRegExps.DocumentNamePattern + @")?:(?<lineNumber>\d+)(?:\r\n|\n|\r)" +
				@"(?<sourceLine>[^\r\n]+)(?:\r\n|\n|\r)" +
				@"(?<pointer>[ \t]*\^)(?:\r\n|\n|\r){2}" +
			@")?");

		/// <summary>
		/// Regular expression for working with the error message with type
		/// </summary>
		private static readonly Regex _errorMessageWithTypeRegex =
			new Regex(@"^(?<type>" + CommonRegExps.JsFullNamePattern + @"): (?<description>[^\r\n]+)");


		/// <summary>
		/// Constructs an instance of adapter for the Node JS engine
		/// </summary>
		public NodeJsEngine()
			: this(DefaultNodeJsService.Instance, new NodeSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the Node JS engine
		/// </summary>
		/// <param name="nodeJsService">Node JS service</param>
		public NodeJsEngine(INodeJSService nodeJsService)
			: this(nodeJsService, new NodeSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the Node JS engine
		/// </summary>
		/// <param name="settings">Settings of the Node JS engine</param>
		public NodeJsEngine(NodeSettings settings)
			: this(DefaultNodeJsService.Instance, settings)
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the Node JS engine
		/// </summary>
		/// <param name="service">Node JS service</param>
		/// <param name="settings">Settings of the Node JS engine</param>
		public NodeJsEngine(INodeJSService service, NodeSettings settings)
		{
			if (service is null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			_jsService = service;

			try
			{
				Task<string> versionTask = _jsService.InvokeFromStringAsync<string>(
					@"module.exports = (callback) => {
	let version = process.versions.node;
	callback(null , version);
};"
				);
				_engineVersion = versionTask.ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				throw JsErrorHelpers.WrapEngineLoadException(e, EngineName, _engineVersion, true);
			}

			NodeSettings nodeSettings = settings ?? new NodeSettings();
			_executionTimeout = (int)nodeSettings.TimeoutInterval.TotalMilliseconds;
			_engineId = JsEngineIdGenerator.GetNextId();

			InvokeEngineHelper("addContext", new object[] { _engineId, nodeSettings.UseBuiltinLibrary });
		}


		private void InvokeEngineHelper(string exportName = null, object[] args = null)
		{
			Task<bool> cachedTask = _jsService.TryInvokeFromCacheAsync(ENGINE_HELPERS_RESOURCE_NAME,
				exportName, args);
			bool success = cachedTask.ConfigureAwait(false).GetAwaiter().GetResult();

			if (!success)
			{
				Type type = typeof(NodeJsEngine);
				Assembly assembly = type.Assembly;

				using (Stream resourceStream = assembly.GetManifestResourceStream(ENGINE_HELPERS_RESOURCE_NAME))
				{
					Task task = _jsService.InvokeFromStreamAsync(resourceStream, ENGINE_HELPERS_RESOURCE_NAME,
						exportName, args);
					task.ConfigureAwait(false).GetAwaiter().GetResult();
				}
			}
		}

		private T InvokeEngineHelper<T>(string exportName = null, object[] args = null)
		{
			Task<(bool, T)> cachedTask = _jsService.TryInvokeFromCacheAsync<T>(ENGINE_HELPERS_RESOURCE_NAME,
				exportName, args);
			(bool success, T result) = cachedTask.ConfigureAwait(false).GetAwaiter().GetResult();

			if (success)
			{
				return result;
			}
			else
			{
				Type type = typeof(NodeJsEngine);
				Assembly assembly = type.Assembly;

				using (Stream resourceStream = assembly.GetManifestResourceStream(ENGINE_HELPERS_RESOURCE_NAME))
				{
					Task<T> task = _jsService.InvokeFromStreamAsync<T>(resourceStream, ENGINE_HELPERS_RESOURCE_NAME,
						exportName, args);

					return task.ConfigureAwait(false).GetAwaiter().GetResult();
				}
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

		/// <summary>
		/// Makes a mapping of array items from the host type to a script type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		public static object[] MapToScriptType(object[] args)
		{
			return args.Select(arg => MapToScriptType(arg)).ToArray();
		}

		#region Mapping

		private WrapperException WrapInvocationException(InvocationException originalException)
		{
			WrapperException wrapperException;
			string message = originalException.Message;
			string description = string.Empty;

			if (_timeoutErrorMessage.IsMatch(message))
			{
				message = CoreStrings.Runtime_ScriptTimeoutExceeded;
				description = message;

				var wrapperTimeoutException = new WrapperTimeoutException(message, EngineName, _engineVersion,
					originalException)
				{
					Description = description
				};

				return wrapperTimeoutException;
			}

			string documentName = string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			string sourceLine = string.Empty;

			Match detailsMatch = _errorDetailsRegex.Match(message);
			int detailsLength = 0;

			if (detailsMatch.Success)
			{
				GroupCollection detailsGroups = detailsMatch.Groups;
				description = detailsGroups["description"].Value;
				documentName = detailsGroups["documentName"].Success ?
					detailsGroups["documentName"].Value : string.Empty;
				lineNumber = detailsGroups["lineNumber"].Success ?
					int.Parse(detailsGroups["lineNumber"].Value) : 0;
				columnNumber = NodeJsErrorHelpers.GetColumnCountFromLine(detailsGroups["pointer"].Value);
				sourceLine = detailsGroups["sourceLine"].Value;

				detailsLength = detailsMatch.Length;
			}

			message = detailsLength > 0 ? message.Substring(detailsLength) : message;

			Match messageWithTypeMatch = _errorMessageWithTypeRegex.Match(message);
			if (messageWithTypeMatch.Success)
			{
				GroupCollection messageWithTypeGroups = messageWithTypeMatch.Groups;
				string type = messageWithTypeGroups["type"].Value;
				description = messageWithTypeGroups["description"].Value;
				string sourceFragment = TextHelpers.GetTextFragmentFromLine(sourceLine, columnNumber);

				WrapperScriptException wrapperScriptException;
				if (type == JsErrorType.Syntax)
				{
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName,
						lineNumber, columnNumber, sourceFragment);

					wrapperScriptException = new WrapperCompilationException(message, EngineName, _engineVersion,
						originalException);
				}
				else if (type == "UsageError")
				{
					wrapperException = new WrapperUsageException(description, EngineName, _engineVersion,
						originalException);
					wrapperException.Description = description;

					return wrapperException;
				}
				else
				{
					var errorLocationItems = new ErrorLocationItem[0];
					int messageLength = message.Length;
					int messageWithTypeLength = messageWithTypeMatch.Length;

					if (messageWithTypeLength < messageLength)
					{
						string errorLocation = message.Substring(messageWithTypeLength);
						errorLocationItems = NodeJsErrorHelpers.ParseErrorLocation(errorLocation);
						errorLocationItems = NodeJsErrorHelpers.FilterErrorLocationItems(errorLocationItems);

						if (errorLocationItems.Length > 0)
						{
							ErrorLocationItem firstErrorLocationItem = errorLocationItems[0];
							documentName = firstErrorLocationItem.DocumentName;
							lineNumber = firstErrorLocationItem.LineNumber;
							columnNumber = firstErrorLocationItem.ColumnNumber;

							firstErrorLocationItem.SourceFragment = sourceFragment;
						}
					}

					string callStack = JsErrorHelpers.StringifyErrorLocationItems(errorLocationItems, true);
					string callStackWithSourceFragment = JsErrorHelpers.StringifyErrorLocationItems(
						errorLocationItems);
					message = JsErrorHelpers.GenerateScriptErrorMessage(type, description,
						callStackWithSourceFragment);

					wrapperScriptException = new WrapperRuntimeException(message, EngineName, _engineVersion,
						originalException)
					{
						CallStack = callStack
					};
				}

				wrapperScriptException.Type = type;
				wrapperScriptException.DocumentName = documentName;
				wrapperScriptException.LineNumber = lineNumber;
				wrapperScriptException.ColumnNumber = columnNumber;
				wrapperScriptException.SourceFragment = sourceFragment;

				wrapperException = wrapperScriptException;
			}
			else
			{
				wrapperException = new WrapperException(message, EngineName, _engineVersion,
					originalException);
			}

			wrapperException.Description = description;

			return wrapperException;
		}

		#endregion

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
			throw new NotSupportedException();
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			throw new NotSupportedException();
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			return InnerEvaluate<T>(expression, null);
		}

		protected override T InnerEvaluate<T>(string expression, string documentName)
		{
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);
			T result;

			try
			{
				result = InvokeEngineHelper<T>("evaluate", new object[] { _engineId, expression, uniqueDocumentName,
					_executionTimeout });
			}
			catch (InvocationException e)
			{
				throw WrapInvocationException(e);
			}

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
				InvokeEngineHelper("execute", new object[] { _engineId, code, uniqueDocumentName, _executionTimeout });
			}
			catch (InvocationException e)
			{
				throw WrapInvocationException(e);
			}
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			throw new NotSupportedException();
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			throw new NotSupportedException();
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			T result;
			object[] processedArgs = MapToScriptType(args);

			try
			{
				result = InvokeEngineHelper<T>("callFunction", new object[] { _engineId, functionName, processedArgs,
					_executionTimeout });
			}
			catch (InvocationException e)
			{
				throw WrapInvocationException(e);
			}

			return result;
		}

		protected override bool InnerHasVariable(string variableName)
		{
			return InvokeEngineHelper<bool>("hasVariable", new [] { _engineId, variableName });
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			throw new NotSupportedException();
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			T result;

			try
			{
				result = InvokeEngineHelper<T>("getVariableValue", new[] { _engineId, variableName });
			}
			catch (InvocationException e)
			{
				throw WrapInvocationException(e);
			}

			return result;
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToScriptType(value);

			try
			{
				InvokeEngineHelper("setVariableValue", new[] { _engineId, variableName, processedValue });
			}
			catch (InvocationException e)
			{
				throw WrapInvocationException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				InvokeEngineHelper("removeVariable", new[] { _engineId, variableName });
			}
			catch (InvocationException e)
			{
				throw WrapInvocationException(e);
			}
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			throw new NotSupportedException();
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			throw new NotSupportedException();
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
			get { return _engineVersion; }
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
				InvokeEngineHelper("removeContext", new[] { _engineId });
				_jsService = null;
			}
		}

		#endregion

		#endregion
	}
}