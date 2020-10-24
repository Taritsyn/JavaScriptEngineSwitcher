using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

using OriginalCacheKind = Microsoft.ClearScript.V8.V8CacheKind;
using OriginalEngine = Microsoft.ClearScript.V8.V8ScriptEngine;
using OriginalEngineFlags = Microsoft.ClearScript.V8.V8ScriptEngineFlags;
using OriginalException = Microsoft.ClearScript.ScriptEngineException;
using OriginalInterruptedException = Microsoft.ClearScript.ScriptInterruptedException;
using OriginalRuntimeConstraints = Microsoft.ClearScript.V8.V8RuntimeConstraints;
using OriginalScript = Microsoft.ClearScript.V8.V8Script;
using OriginalUndefined = Microsoft.ClearScript.Undefined;

using AdvancedStringBuilder;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperEngineLoadException = JavaScriptEngineSwitcher.Core.JsEngineLoadException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperFatalException = JavaScriptEngineSwitcher.Core.JsFatalException;
using WrapperInterruptedException = JavaScriptEngineSwitcher.Core.JsInterruptedException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;
using WrapperUsageException = JavaScriptEngineSwitcher.Core.JsUsageException;

using JavaScriptEngineSwitcher.V8.Constants;
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
		private const string EngineVersion = "8.6.395.17";

		/// <summary>
		/// V8 JS engine
		/// </summary>
		private OriginalEngine _jsEngine;

		/// <summary>
		/// Regular expression for working with the error message with type
		/// </summary>
		private static readonly Regex _errorMessageWithTypeRegex =
			new Regex(@"^(?<type>" + CommonRegExps.JsFullNamePattern + @"):\s+(?<description>[\s\S]+?)$");

		/// <summary>
		/// Regular expression for working with the library load error message
		/// </summary>
		private static readonly Regex _libraryLoadErrorMessage =
			new Regex(@"^Cannot load ClearScript V8 library. " +
				"Load failure information for (?<assemblyFileName>" + CommonRegExps.DocumentNamePattern + "):");


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

			var constraints = new OriginalRuntimeConstraints
			{
				MaxNewSpaceSize = v8Settings.MaxNewSpaceSize,
				MaxOldSpaceSize = v8Settings.MaxOldSpaceSize,
			};

			OriginalEngineFlags flags = OriginalEngineFlags.None;
			if (v8Settings.AwaitDebuggerAndPauseOnStart)
			{
				flags |= OriginalEngineFlags.AwaitDebuggerAndPauseOnStart;
			}
			if (v8Settings.EnableDebugging)
			{
				flags |= OriginalEngineFlags.EnableDebugging;
			}
			if (v8Settings.EnableRemoteDebugging)
			{
				flags |= OriginalEngineFlags.EnableRemoteDebugging;
			}
			if (v8Settings.DisableGlobalMembers)
			{
				flags |= OriginalEngineFlags.DisableGlobalMembers;
			}

			int debugPort = v8Settings.DebugPort;

			try
			{
				_jsEngine = new OriginalEngine(constraints, flags, debugPort)
				{
					MaxRuntimeHeapSize = v8Settings.MaxHeapSize,
					RuntimeHeapSizeSampleInterval = v8Settings.HeapSizeSampleInterval,
					MaxRuntimeStackUsage = v8Settings.MaxStackUsage
				};
			}
			catch (DllNotFoundException e)
			{
				throw WrapDllNotFoundException(e);
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

		private static WrapperException WrapScriptEngineException(OriginalException originalException)
		{
			WrapperException wrapperException;
			string message = originalException.Message;
			string messageWithErrorLocation = originalException.ErrorDetails;
			string description = message;
			string type = string.Empty;
			string documentName = string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			string callStack = string.Empty;
			string sourceFragment = string.Empty;

			if (originalException.IsFatal)
			{
				if (message == "The V8 runtime has exceeded its memory limit")
				{
					wrapperException = new WrapperRuntimeException(message, EngineName, EngineVersion,
						originalException);
				}
				else
				{
					wrapperException = new WrapperFatalException(message, EngineName, EngineVersion,
						originalException);
				}
			}
			else
			{
				Match messageWithTypeMatch = _errorMessageWithTypeRegex.Match(message);
				if (messageWithTypeMatch.Success)
				{
					GroupCollection messageWithTypeGroups = messageWithTypeMatch.Groups;
					type = messageWithTypeGroups["type"].Value;
					description = messageWithTypeGroups["description"].Value;
					var errorLocationItems = new ErrorLocationItem[0];

					if (message.Length < messageWithErrorLocation.Length)
					{
						string errorLocation = messageWithErrorLocation
							.TrimStart(message)
							.TrimStart(new char[] { '\n', '\r' })
							;

						errorLocationItems = JsErrorHelpers.ParseErrorLocation(errorLocation);
						if (errorLocationItems.Length > 0)
						{
							ErrorLocationItem firstErrorLocationItem = errorLocationItems[0];

							documentName = firstErrorLocationItem.DocumentName;
							lineNumber = firstErrorLocationItem.LineNumber;
							columnNumber = firstErrorLocationItem.ColumnNumber;
							string sourceLine = firstErrorLocationItem.SourceFragment;
							sourceFragment = TextHelpers.GetTextFragmentFromLine(sourceLine, columnNumber);

							firstErrorLocationItem.SourceFragment = sourceFragment;
						}
					}

					WrapperScriptException wrapperScriptException;
					if (type == JsErrorType.Syntax)
					{
						message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, documentName,
							lineNumber, columnNumber, sourceFragment);

						wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
							originalException);
					}
					else
					{
						callStack = JsErrorHelpers.StringifyErrorLocationItems(errorLocationItems, true);
						string callStackWithSourceFragment = JsErrorHelpers.StringifyErrorLocationItems(
							errorLocationItems);
						message = JsErrorHelpers.GenerateScriptErrorMessage(type, description,
							callStackWithSourceFragment);

						wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
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
					wrapperException = new WrapperException(message, EngineName, EngineVersion,
						originalException);
				}
			}

			wrapperException.Description = description;

			return wrapperException;
		}

		private static WrapperInterruptedException WrapScriptInterruptedException(
			OriginalInterruptedException originalInterruptedException)
		{
			string message = CoreStrings.Runtime_ScriptInterrupted;
			string description = message;

			var wrapperInterruptedException = new WrapperInterruptedException(message, EngineName, EngineVersion,
				originalInterruptedException)
			{
				Description = description
			}
			;

			return wrapperInterruptedException;
		}

		private static WrapperEngineLoadException WrapDllNotFoundException(
			DllNotFoundException originalDllNotFoundException)
		{
			string originalMessage = originalDllNotFoundException.Message;
			string description;
			string message;

			if (originalMessage.ContainsQuotedValue(DllName.Universal))
			{
				Architecture osArchitecture = RuntimeInformation.OSArchitecture;

				var stringBuilderPool = StringBuilderPool.Shared;
				StringBuilder descriptionBuilder = stringBuilderPool.Rent();
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					descriptionBuilder.AppendFormat(CoreStrings.Engine_AssemblyNotFound, DllName.ForWindows);
					descriptionBuilder.Append(" ");
					if (osArchitecture == Architecture.X64 || osArchitecture == Architecture.X86)
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NuGetPackageInstallationRequired,
							Utils.Is64BitProcess() ?
								"JavaScriptEngineSwitcher.V8.Native.win-x64"
								:
								"JavaScriptEngineSwitcher.V8.Native.win-x86"
						);
					}
					else
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NoNuGetPackageForProcessorArchitecture,
							"JavaScriptEngineSwitcher.V8.Native.win*",
							osArchitecture.ToString().ToLowerInvariant()
						);
					}
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					descriptionBuilder.AppendFormat(CoreStrings.Engine_AssemblyNotFound, DllName.ForLinux);
					descriptionBuilder.Append(" ");
					if (osArchitecture == Architecture.X64)
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NuGetPackageInstallationRequired,
							"JavaScriptEngineSwitcher.V8.Native.linux-x64");
					}
					else
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NoNuGetPackageForProcessorArchitecture,
							"JavaScriptEngineSwitcher.V8.Native.linux-*",
							osArchitecture.ToString().ToLowerInvariant()
						);
					}
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					descriptionBuilder.AppendFormat(CoreStrings.Engine_AssemblyNotFound, DllName.ForOsx);
					descriptionBuilder.Append(" ");
					if (osArchitecture == Architecture.X64)
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NuGetPackageInstallationRequired,
							"JavaScriptEngineSwitcher.V8.Native.osx-x64");
					}
					else
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NoNuGetPackageForProcessorArchitecture,
							"JavaScriptEngineSwitcher.V8.Native.osx-*",
							osArchitecture.ToString().ToLowerInvariant()
						);
					}
				}
				else
				{
					descriptionBuilder.Append(CoreStrings.Engine_OperatingSystemNotSupported);
				}

				description = descriptionBuilder.ToString();
				stringBuilderPool.Return(descriptionBuilder);

				message = JsErrorHelpers.GenerateEngineLoadErrorMessage(description, EngineName);
			}
			else
			{
				description = originalMessage;
				message = JsErrorHelpers.GenerateEngineLoadErrorMessage(description, EngineName, true);
			}

			var wrapperEngineLoadException = new WrapperEngineLoadException(message, EngineName, EngineVersion,
				originalDllNotFoundException)
			{
				Description = description
			};

			return wrapperEngineLoadException;
		}

		#endregion

		#region JsEngineBase overrides

		protected override IPrecompiledScript InnerPrecompile(string code)
		{
			return Precompile(code, null);
		}

		protected override IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			var cacheKind = OriginalCacheKind.Code;
			byte[] cachedBytes;

			try
			{
				using (OriginalScript generalScript = _jsEngine.Compile(documentName, code, cacheKind,
					out cachedBytes))
				{ }
			}
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}

			return new V8PrecompiledScript(code, cacheKind, cachedBytes, documentName);
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
				result = _jsEngine.Evaluate(documentName, false, expression);
			}
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
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
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
			}
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			var v8PrecompiledScript = precompiledScript as V8PrecompiledScript;
			if (v8PrecompiledScript == null)
			{
				throw new WrapperUsageException(
					string.Format(CoreStrings.Usage_CannotConvertPrecompiledScriptToInternalType,
						typeof(V8PrecompiledScript).FullName),
					Name, Version
				);
			}

			bool cacheAccepted;

			try
			{
				using (OriginalScript script = _jsEngine.Compile(v8PrecompiledScript.DocumentName,
					v8PrecompiledScript.Code, v8PrecompiledScript.CacheKind, v8PrecompiledScript.CachedBytes,
					out cacheAccepted))
				{
					if (!cacheAccepted)
					{
						throw new WrapperUsageException(Strings.Usage_PrecompiledScriptNotAccepted, Name, Version);
					}

					_jsEngine.Execute(script);
				}
			}
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
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
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
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
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
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
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
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
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			try
			{
				_jsEngine.AddHostType(itemName, type);
			}
			catch (OriginalException e)
			{
				throw WrapScriptEngineException(e);
			}
			catch (OriginalInterruptedException e)
			{
				throw WrapScriptInterruptedException(e);
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