using System;
using System.Linq;
#if NET45 || NET471 || NETSTANDARD || NETCOREAPP2_1
using System.Runtime.InteropServices;
#endif
using System.Text;

using AdvancedStringBuilder;
#if NET40
using PolyfillsForOldDotNet.System.Runtime.InteropServices;
#endif

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Utilities;

using ErrorLocationItem = JavaScriptEngineSwitcher.Core.Helpers.ErrorLocationItem;
using CoreErrorHelpers = JavaScriptEngineSwitcher.Core.Helpers.JsErrorHelpers;
using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using TextHelpers = JavaScriptEngineSwitcher.Core.Helpers.TextHelpers;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperEngineException = JavaScriptEngineSwitcher.Core.JsEngineException;
using WrapperEngineLoadException = JavaScriptEngineSwitcher.Core.JsEngineLoadException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperFatalException = JavaScriptEngineSwitcher.Core.JsFatalException;
using WrapperInterruptedException = JavaScriptEngineSwitcher.Core.JsInterruptedException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;
using WrapperUsageException = JavaScriptEngineSwitcher.Core.JsUsageException;

using JavaScriptEngineSwitcher.ChakraCore.Constants;
using JavaScriptEngineSwitcher.ChakraCore.JsRt;
using JavaScriptEngineSwitcher.ChakraCore.Resources;

using OriginalEngineException = JavaScriptEngineSwitcher.ChakraCore.JsRt.JsEngineException;
using OriginalException = JavaScriptEngineSwitcher.ChakraCore.JsRt.JsException;
using OriginalFatalException = JavaScriptEngineSwitcher.ChakraCore.JsRt.JsFatalException;
using OriginalScriptException = JavaScriptEngineSwitcher.ChakraCore.JsRt.JsScriptException;
using OriginalUsageException = JavaScriptEngineSwitcher.ChakraCore.JsRt.JsUsageException;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// Adapter for the ChakraCore JS engine
	/// </summary>
	public sealed class ChakraCoreJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "ChakraCoreJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "1.11.8";

		/// <summary>
		/// Instance of JS runtime
		/// </summary>
		private JsRuntime _jsRuntime;

		/// <summary>
		/// Instance of JS context
		/// </summary>
		private JsContext _jsContext;

		/// <summary>
		/// JS source context
		/// </summary>
		private JsSourceContext _jsSourceContext = JsSourceContext.FromIntPtr(IntPtr.Zero);

		/// <summary>
		/// Type mapper
		/// </summary>
		private TypeMapper _typeMapper = new TypeMapper();

		/// <summary>
		/// Callback for continuation of promise
		/// </summary>
		private JsPromiseContinuationCallback _promiseContinuationCallback;

		/// <summary>
		/// Script dispatcher
		/// </summary>
		private ScriptDispatcher _dispatcher;

		/// <summary>
		/// Unique document name manager
		/// </summary>
		private readonly UniqueDocumentNameManager _documentNameManager =
			new UniqueDocumentNameManager(DefaultDocumentName);
#if NETFULL

		/// <summary>
		/// Synchronizer of JS engine initialization
		/// </summary>
		private static readonly object _initializationSynchronizer = new object();

		/// <summary>
		/// Flag indicating whether the JS engine is initialized
		/// </summary>
		private static bool _initialized;
#endif


		/// <summary>
		/// Constructs an instance of adapter for the ChakraCore JS engine
		/// </summary>
		public ChakraCoreJsEngine()
			: this(new ChakraCoreSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the ChakraCore JS engine
		/// </summary>
		/// <param name="settings">Settings of the ChakraCore JS engine</param>
		public ChakraCoreJsEngine(ChakraCoreSettings settings)
		{
#if NETFULL
			Initialize();

#endif
			ChakraCoreSettings chakraCoreSettings = settings ?? new ChakraCoreSettings();

			JsRuntimeAttributes attributes = JsRuntimeAttributes.AllowScriptInterrupt;
			if (chakraCoreSettings.DisableBackgroundWork)
			{
				attributes |= JsRuntimeAttributes.DisableBackgroundWork;
			}
			if (chakraCoreSettings.DisableEval)
			{
				attributes |= JsRuntimeAttributes.DisableEval;
			}
			if (chakraCoreSettings.DisableExecutablePageAllocation)
			{
				attributes |= JsRuntimeAttributes.DisableExecutablePageAllocation;
			}
			if (chakraCoreSettings.DisableFatalOnOOM)
			{
				attributes |= JsRuntimeAttributes.DisableFatalOnOOM;
			}
			if (chakraCoreSettings.DisableNativeCodeGeneration)
			{
				attributes |= JsRuntimeAttributes.DisableNativeCodeGeneration;
			}
			if (chakraCoreSettings.EnableExperimentalFeatures)
			{
				attributes |= JsRuntimeAttributes.EnableExperimentalFeatures;
			}

#if NETSTANDARD1_3
			_dispatcher = new ScriptDispatcher();
#else
			_dispatcher = new ScriptDispatcher(chakraCoreSettings.MaxStackSize);
#endif
			_promiseContinuationCallback = PromiseContinuationCallback;

			try
			{
				_dispatcher.Invoke(() =>
				{
					_jsRuntime = JsRuntime.Create(attributes, null);
					_jsRuntime.MemoryLimit = settings.MemoryLimit;

					_jsContext = _jsRuntime.CreateContext();
					if (_jsContext.IsValid)
					{
						_jsContext.AddRef();
					}
				});
			}
			catch (DllNotFoundException e)
			{
				throw WrapDllNotFoundException(e);
			}
			catch (Exception e)
			{
				throw CoreErrorHelpers.WrapEngineLoadException(e, EngineName, EngineVersion, true);
			}
			finally
			{
				if (!_jsContext.IsValid)
				{
					Dispose();
				}
			}
		}

		/// <summary>
		/// Destructs an instance of adapter for the ChakraCore JS engine
		/// </summary>
		~ChakraCoreJsEngine()
		{
			Dispose(false);
		}

#if NETFULL

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
						AssemblyResolver.Initialize();
					}
					catch (InvalidOperationException e)
					{
						throw CoreErrorHelpers.WrapEngineLoadException(e, EngineName, EngineVersion);
					}
				}

				_initialized = true;
			}
		}
#endif

		/// <summary>
		/// Adds a reference to the value
		/// </summary>
		/// <param name="value">The value</param>
		private static void AddReferenceToValue(JsValue value)
		{
			if (CanHaveReferences(value))
			{
				value.AddRef();
			}
		}

		/// <summary>
		/// Removes a reference to the value
		/// </summary>
		/// <param name="value">The value</param>
		private static void RemoveReferenceToValue(JsValue value)
		{
			if (CanHaveReferences(value))
			{
				value.Release();
			}
		}

		/// <summary>
		/// Checks whether the value can have references
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns>Result of check (true - may have; false - may not have)</returns>
		private static bool CanHaveReferences(JsValue value)
		{
			JsValueType valueType = value.ValueType;

			switch (valueType)
			{
				case JsValueType.Null:
				case JsValueType.Undefined:
				case JsValueType.Boolean:
					return false;
				default:
					return true;
			}
		}

		/// <summary>
		/// Creates a instance of JS scope
		/// </summary>
		/// <returns>Instance of JS scope</returns>
		private JsScope CreateJsScope()
		{
			if (_jsRuntime.Disabled)
			{
				_jsRuntime.Disabled = false;
			}

			var jsScope = new JsScope(_jsContext);

			JsRuntime.SetPromiseContinuationCallback(_promiseContinuationCallback, IntPtr.Zero);

			return jsScope;
		}

		/// <summary>
		/// The promise continuation callback
		/// </summary>
		/// <param name="task">The task, represented as a JavaScript function</param>
		/// <param name="callbackState">The data argument to be passed to the callback</param>
		private static void PromiseContinuationCallback(JsValue task, IntPtr callbackState)
		{
			task.AddRef();

			try
			{
				task.CallFunction(JsValue.GlobalObject);
			}
			finally
			{
				task.Release();
			}
		}

		#region Mapping

		private static WrapperException WrapJsException(OriginalException originalException,
			string defaultDocumentName = null)
		{
			WrapperException wrapperException;
			JsErrorCode errorCode = originalException.ErrorCode;
			string description = originalException.Message;
			string message = description;
			string type = string.Empty;
			string documentName = defaultDocumentName ?? string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			string callStack = string.Empty;
			string sourceFragment = string.Empty;

			var originalScriptException = originalException as OriginalScriptException;
			if (originalScriptException != null)
			{
				JsValue metadataValue = originalScriptException.Metadata;

				if (metadataValue.IsValid)
				{
					JsValue errorValue = metadataValue.GetProperty("exception");
					JsValueType errorValueType = errorValue.ValueType;

					if (errorValueType == JsValueType.Error
						|| errorValueType == JsValueType.Object)
					{
						JsPropertyId innerErrorPropertyId = JsPropertyId.FromString("innerException");
						if (errorValue.HasProperty(innerErrorPropertyId))
						{
							JsValue innerErrorValue = errorValue.GetProperty(innerErrorPropertyId);
							JsPropertyId metadataPropertyId = JsPropertyId.FromString("metadata");

							if (innerErrorValue.HasProperty(metadataPropertyId))
							{
								errorValue = innerErrorValue;
								metadataValue = innerErrorValue.GetProperty(metadataPropertyId);
							}
						}

						JsValue messagePropertyValue = errorValue.GetProperty("message");
						string localDescription = messagePropertyValue.ConvertToString().ToString();
						if (!string.IsNullOrWhiteSpace(localDescription))
						{
							description = localDescription;
						}

						JsValue namePropertyValue = errorValue.GetProperty("name");
						type = namePropertyValue.ValueType == JsValueType.String ?
							namePropertyValue.ToString() : string.Empty;

						JsPropertyId descriptionPropertyId = JsPropertyId.FromString("description");
						if (errorValue.HasProperty(descriptionPropertyId))
						{
							JsValue descriptionPropertyValue = errorValue.GetProperty(descriptionPropertyId);
							localDescription = descriptionPropertyValue.ConvertToString().ToString();
							if (!string.IsNullOrWhiteSpace(localDescription))
							{
								description = localDescription;
							}
						}

						if (type == JsErrorType.Syntax)
						{
							errorCode = JsErrorCode.ScriptCompile;
						}
						else
						{
							JsPropertyId numberPropertyId = JsPropertyId.FromString("number");
							if (errorValue.HasProperty(numberPropertyId))
							{
								JsValue numberPropertyValue = errorValue.GetProperty(numberPropertyId);
								int errorNumber = numberPropertyValue.ValueType == JsValueType.Number ?
									numberPropertyValue.ToInt32() : 0;
								errorCode = (JsErrorCode)errorNumber;
							}
						}

						JsPropertyId urlPropertyId = JsPropertyId.FromString("url");
						if (metadataValue.HasProperty(urlPropertyId))
						{
							JsValue urlPropertyValue = metadataValue.GetProperty(urlPropertyId);
							string url = urlPropertyValue.ValueType == JsValueType.String ?
								urlPropertyValue.ToString() : string.Empty;
							if (url != "undefined")
							{
								documentName = url;
							}
						}

						JsPropertyId linePropertyId = JsPropertyId.FromString("line");
						if (metadataValue.HasProperty(linePropertyId))
						{
							JsValue linePropertyValue = metadataValue.GetProperty(linePropertyId);
							lineNumber = linePropertyValue.ValueType == JsValueType.Number ?
								linePropertyValue.ToInt32() + 1 : 0;
						}

						JsPropertyId columnPropertyId = JsPropertyId.FromString("column");
						if (metadataValue.HasProperty(columnPropertyId))
						{
							JsValue columnPropertyValue = metadataValue.GetProperty(columnPropertyId);
							columnNumber = columnPropertyValue.ValueType == JsValueType.Number ?
								columnPropertyValue.ToInt32() + 1 : 0;
						}

						string sourceLine = string.Empty;
						JsPropertyId sourcePropertyId = JsPropertyId.FromString("source");
						if (metadataValue.HasProperty(sourcePropertyId))
						{
							JsValue sourcePropertyValue = metadataValue.GetProperty(sourcePropertyId);
							sourceLine = sourcePropertyValue.ValueType == JsValueType.String ?
								sourcePropertyValue.ToString() : string.Empty;
							sourceFragment = TextHelpers.GetTextFragmentFromLine(sourceLine, columnNumber);
						}

						JsPropertyId stackPropertyId = JsPropertyId.FromString("stack");
						if (errorValue.HasProperty(stackPropertyId))
						{
							JsValue stackPropertyValue = errorValue.GetProperty(stackPropertyId);
							string messageWithTypeAndCallStack = stackPropertyValue.ValueType == JsValueType.String ?
								stackPropertyValue.ToString() : string.Empty;
							string messageWithType = errorValue.ConvertToString().ToString();
							string rawCallStack = messageWithTypeAndCallStack
								.TrimStart(messageWithType)
								.TrimStart("Error")
								.TrimStart(new char[] { '\n', '\r' })
								;
							string callStackWithSourceFragment = string.Empty;

							ErrorLocationItem[] callStackItems = CoreErrorHelpers.ParseErrorLocation(rawCallStack);
							if (callStackItems.Length > 0)
							{
								ErrorLocationItem firstCallStackItem = callStackItems[0];
								firstCallStackItem.SourceFragment = sourceFragment;

								documentName = firstCallStackItem.DocumentName;
								lineNumber = firstCallStackItem.LineNumber;
								columnNumber = firstCallStackItem.ColumnNumber;
								callStack = CoreErrorHelpers.StringifyErrorLocationItems(callStackItems, true);
								callStackWithSourceFragment = CoreErrorHelpers.StringifyErrorLocationItems(callStackItems);
							}

							message = CoreErrorHelpers.GenerateScriptErrorMessage(type, description,
								callStackWithSourceFragment);
						}
						else
						{
							message = CoreErrorHelpers.GenerateScriptErrorMessage(type, description, documentName,
								lineNumber, columnNumber, sourceFragment);
						}
					}
					else if (errorValueType == JsValueType.String)
					{
						message = errorValue.ToString();
						description = message;
					}
					else
					{
						message = errorValue.ConvertToString().ToString();
						description = message;
					}
				}

				WrapperScriptException wrapperScriptException;
				if (errorCode == JsErrorCode.ScriptCompile)
				{
					wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
						originalScriptException);
				}
				else if (errorCode == JsErrorCode.ScriptTerminated)
				{
					message = CoreStrings.Runtime_ScriptInterrupted;
					description = message;

					wrapperScriptException = new WrapperInterruptedException(message,
						EngineName, EngineVersion, originalScriptException)
					{
						CallStack = callStack
					};
				}
				else
				{
					wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
						originalScriptException)
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
				if (originalException is OriginalUsageException)
				{
					wrapperException = new WrapperUsageException(message, EngineName, EngineVersion,
						originalException);
				}
				else if (originalException is OriginalEngineException)
				{
					wrapperException = new WrapperEngineException(message, EngineName, EngineVersion,
						originalException);
				}
				else if (originalException is OriginalFatalException)
				{
					wrapperException = new WrapperFatalException(message, EngineName, EngineVersion,
						originalException);
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

		private static WrapperEngineLoadException WrapDllNotFoundException(
			DllNotFoundException originalDllNotFoundException)
		{
			string originalMessage = originalDllNotFoundException.Message;
			string description;
			string message;
			bool isMonoRuntime = Utils.IsMonoRuntime();

			if ((isMonoRuntime && originalMessage == DllName.Universal)
				|| originalMessage.ContainsQuotedValue(DllName.Universal))
			{
				const string buildInstructionsUrl =
					"https://github.com/Microsoft/ChakraCore/wiki/Building-ChakraCore#{0}";
				const string manualInstallationInstructionsUrl =
					"https://github.com/Taritsyn/JavaScriptEngineSwitcher/wiki/ChakraCore#{0}";
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
								"JavaScriptEngineSwitcher.ChakraCore.Native.win-x64"
								:
								"JavaScriptEngineSwitcher.ChakraCore.Native.win-x86"
						);
						descriptionBuilder.Append(" ");
						descriptionBuilder.Append(Strings.Engine_VcRedist2017InstallationRequired);
					}
					else if (osArchitecture == Architecture.Arm)
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NuGetPackageInstallationRequired,
							"JavaScriptEngineSwitcher.ChakraCore.Native.win-arm");
					}
					else
					{
						descriptionBuilder.AppendFormat(CoreStrings.Engine_NoNuGetPackageForProcessorArchitecture,
							"JavaScriptEngineSwitcher.ChakraCore.Native.win*",
							osArchitecture.ToString().ToLowerInvariant()
						);
						descriptionBuilder.Append(" ");
						descriptionBuilder.AppendFormat(Strings.Engine_BuildNativeAssemblyForCurrentProcessorArchitecture,
							DllName.ForWindows,
							string.Format(buildInstructionsUrl, "windows")
						);
					}
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					descriptionBuilder.AppendFormat(CoreStrings.Engine_AssemblyNotFound, DllName.ForLinux);
					descriptionBuilder.Append(" ");
					if (isMonoRuntime)
					{
						descriptionBuilder.AppendFormat(Strings.Engine_ManualInstallationUnderMonoRequired,
							"JavaScriptEngineSwitcher.ChakraCore.Native.linux-*",
							string.Format(manualInstallationInstructionsUrl, "linux")
						);
					}
					else
					{
						if (osArchitecture == Architecture.X64)
						{
							descriptionBuilder.AppendFormat(CoreStrings.Engine_NuGetPackageInstallationRequired,
								"JavaScriptEngineSwitcher.ChakraCore.Native.linux-x64");
						}
						else
						{
							descriptionBuilder.AppendFormat(CoreStrings.Engine_NoNuGetPackageForProcessorArchitecture,
								"JavaScriptEngineSwitcher.ChakraCore.Native.linux-*",
								osArchitecture.ToString().ToLowerInvariant()
							);
							descriptionBuilder.Append(" ");
							descriptionBuilder.AppendFormat(Strings.Engine_BuildNativeAssemblyForCurrentProcessorArchitecture,
								DllName.ForLinux,
								string.Format(buildInstructionsUrl, "linux")
							);
						}
					}
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					descriptionBuilder.AppendFormat(CoreStrings.Engine_AssemblyNotFound, DllName.ForOsx);
					descriptionBuilder.Append(" ");
					if (isMonoRuntime)
					{
						descriptionBuilder.AppendFormat(Strings.Engine_ManualInstallationUnderMonoRequired,
							"JavaScriptEngineSwitcher.ChakraCore.Native.osx-*",
							string.Format(manualInstallationInstructionsUrl, "os-x")
						);
					}
					else
					{
						if (osArchitecture == Architecture.X64)
						{
							descriptionBuilder.AppendFormat(CoreStrings.Engine_NuGetPackageInstallationRequired,
								"JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64");
						}
						else
						{
							descriptionBuilder.AppendFormat(CoreStrings.Engine_NoNuGetPackageForProcessorArchitecture,
								"JavaScriptEngineSwitcher.ChakraCore.Native.osx-*",
								osArchitecture.ToString().ToLowerInvariant()
							);
							descriptionBuilder.Append(" ");
							descriptionBuilder.AppendFormat(Strings.Engine_BuildNativeAssemblyForCurrentProcessorArchitecture,
								DllName.ForOsx,
								string.Format(buildInstructionsUrl, "os-x")
							);
						}
					}
				}
				else
				{
					descriptionBuilder.Append(CoreStrings.Engine_OperatingSystemNotSupported);
				}

				description = descriptionBuilder.ToString();
				stringBuilderPool.Return(descriptionBuilder);

				message = CoreErrorHelpers.GenerateEngineLoadErrorMessage(description, EngineName);
			}
			else
			{
				description = originalMessage;
				message = CoreErrorHelpers.GenerateEngineLoadErrorMessage(description, EngineName, true);
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
			return InnerPrecompile(code, null);
		}

		protected override IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			IPrecompiledScript precompiledScript = _dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsParseScriptAttributes parseAttributes = JsParseScriptAttributes.None;
						byte[] cachedBytes = JsContext.SerializeScript(code, ref parseAttributes);

						return new ChakraCorePrecompiledScript(code, parseAttributes, cachedBytes, uniqueDocumentName);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e, uniqueDocumentName);
					}
				}
			});

			return precompiledScript;
		}

		protected override object InnerEvaluate(string expression)
		{
			return InnerEvaluate(expression, null);
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			string uniqueDocumentName = _documentNameManager.GetUniqueName(documentName);

			object result = _dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsParseScriptAttributes parseAttributes = JsParseScriptAttributes.None;
						JsValue resultValue = JsContext.RunScript(expression, _jsSourceContext++,
							uniqueDocumentName, ref parseAttributes);

						return _typeMapper.MapToHostType(resultValue);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});

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

			_dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsParseScriptAttributes parseAttributes = JsParseScriptAttributes.None;
						JsContext.RunScript(code, _jsSourceContext++, uniqueDocumentName, ref parseAttributes);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			var chakraCorePrecompiledScript = precompiledScript as ChakraCorePrecompiledScript;
			if (chakraCorePrecompiledScript == null)
			{
				throw new WrapperUsageException(
					string.Format(CoreStrings.Usage_CannotConvertPrecompiledScriptToInternalType,
						typeof(ChakraCorePrecompiledScript).FullName),
					Name, Version
				);
			}

			_dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsContext.RunSerializedScript(chakraCorePrecompiledScript.Code,
							chakraCorePrecompiledScript.CachedBytes,
							chakraCorePrecompiledScript.LoadScriptSourceCodeCallback, _jsSourceContext++,
							chakraCorePrecompiledScript.DocumentName);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
					finally
					{
						GC.KeepAlive(chakraCorePrecompiledScript);
					}
				}
			});
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result = _dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsValue globalObj = JsValue.GlobalObject;
						JsPropertyId functionId = JsPropertyId.FromString(functionName);

						bool functionExist = globalObj.HasProperty(functionId);
						if (!functionExist)
						{
							throw new WrapperRuntimeException(
								string.Format(CoreStrings.Runtime_FunctionNotExist, functionName),
								EngineName, EngineVersion
							);
						}

						JsValue resultValue;
						JsValue functionValue = globalObj.GetProperty(functionId);

						if (args.Length > 0)
						{
							JsValue[] processedArgs = _typeMapper.MapToScriptType(args);

							foreach (JsValue processedArg in processedArgs)
							{
								AddReferenceToValue(processedArg);
							}

							JsValue[] allProcessedArgs = new[] { globalObj }
								.Concat(processedArgs)
								.ToArray()
								;

							try
							{
								resultValue = functionValue.CallFunction(allProcessedArgs);
							}
							finally
							{
								foreach (JsValue processedArg in processedArgs)
								{
									RemoveReferenceToValue(processedArg);
								}
							}
						}
						else
						{
							resultValue = functionValue.CallFunction(globalObj);
						}

						return _typeMapper.MapToHostType(resultValue);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result = _dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsValue globalObj = JsValue.GlobalObject;
						JsPropertyId variableId = JsPropertyId.FromString(variableName);
						bool variableExist = globalObj.HasProperty(variableId);

						if (variableExist)
						{
							JsValue variableValue = globalObj.GetProperty(variableId);
							variableExist = variableValue.ValueType != JsValueType.Undefined;
						}

						return variableExist;
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result = _dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsValue variableValue = JsValue.GlobalObject.GetProperty(variableName);

						return _typeMapper.MapToHostType(variableValue);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			_dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsValue inputValue = _typeMapper.MapToScriptType(value);
						AddReferenceToValue(inputValue);

						try
						{
							JsValue.GlobalObject.SetProperty(variableName, inputValue, true);
						}
						finally
						{
							RemoveReferenceToValue(inputValue);
						}
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			_dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsValue globalObj = JsValue.GlobalObject;
						JsPropertyId variableId = JsPropertyId.FromString(variableName);

						if (globalObj.HasProperty(variableId))
						{
							globalObj.SetProperty(variableId, JsValue.Undefined, true);
						}
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			_dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsValue processedValue = _typeMapper.GetOrCreateScriptObject(value);
						JsValue.GlobalObject.SetProperty(itemName, processedValue, true);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			_dispatcher.Invoke(() =>
			{
				using (CreateJsScope())
				{
					try
					{
						JsValue typeValue = _typeMapper.GetOrCreateScriptType(type);
						JsValue.GlobalObject.SetProperty(itemName, typeValue, true);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
					}
				}
			});
		}

		protected override void InnerInterrupt()
		{
			_jsRuntime.Disabled = true;
		}

		protected override void InnerCollectGarbage()
		{
			_jsRuntime.CollectGarbage();
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

		/// <summary>
		/// Destroys object
		/// </summary>
		public override void Dispose()
		{
			Dispose(true /* disposing */);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Destroys object
		/// </summary>
		/// <param name="disposing">Flag, allowing destruction of
		/// managed objects contained in fields of class</param>
		private void Dispose(bool disposing)
		{
			if (_disposedFlag.Set())
			{
				if (disposing)
				{
					if (_dispatcher != null)
					{
						_dispatcher.Invoke(DisposeUnmanagedResources);

						_dispatcher.Dispose();
						_dispatcher = null;
					}

					if (_typeMapper != null)
					{
						_typeMapper.Dispose();
						_typeMapper = null;
					}

					_promiseContinuationCallback = null;
				}
				else
				{
					DisposeUnmanagedResources();
				}
			}
		}

		private void DisposeUnmanagedResources()
		{
			if (_jsContext.IsValid)
			{
				_jsContext.Release();
			}
			_jsRuntime.Dispose();
		}

		#endregion

		#endregion
	}
}