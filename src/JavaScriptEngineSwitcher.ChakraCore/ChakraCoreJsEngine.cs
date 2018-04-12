using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Extensions;
#if NET40
using JavaScriptEngineSwitcher.Core.Polyfills.System;
using JavaScriptEngineSwitcher.Core.Polyfills.System.Reflection;
using JavaScriptEngineSwitcher.Core.Polyfills.System.Runtime.InteropServices;
#endif
using JavaScriptEngineSwitcher.Core.Utilities;

using ErrorLocationItem = JavaScriptEngineSwitcher.Core.Helpers.ErrorLocationItem;
using CoreErrorHelpers = JavaScriptEngineSwitcher.Core.Helpers.JsErrorHelpers;
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

using JavaScriptEngineSwitcher.ChakraCore.Constants;
using JavaScriptEngineSwitcher.ChakraCore.Helpers;
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
		private const string EngineVersion = "1.8.3";

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
		/// Set of external objects
		/// </summary>
		private readonly HashSet<object> _externalObjects = new HashSet<object>();

		/// <summary>
		/// Callback for finalization of external object
		/// </summary>
		private JsObjectFinalizeCallback _externalObjectFinalizeCallback;

		/// <summary>
		/// Callback for continuation of promise
		/// </summary>
		private JsPromiseContinuationCallback _promiseContinuationCallback;

		/// <summary>
		/// List of native function callbacks
		/// </summary>
		private readonly HashSet<JsNativeFunction> _nativeFunctions = new HashSet<JsNativeFunction>();

		/// <summary>
		/// Script dispatcher
		/// </summary>
		private readonly ScriptDispatcher _dispatcher;

		/// <summary>
		/// Unique document name manager
		/// </summary>
		private readonly UniqueDocumentNameManager _documentNameManager =
			new UniqueDocumentNameManager(DefaultDocumentName);
#if !NETSTANDARD

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
#if !NETSTANDARD
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

			_dispatcher = new ScriptDispatcher(chakraCoreSettings.MaxStackSize);
			_externalObjectFinalizeCallback = ExternalObjectFinalizeCallback;
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

#if !NETSTANDARD

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

		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private JsValue MapToScriptType(object value)
		{
			if (value == null)
			{
				return JsValue.Null;
			}

			if (value is Undefined)
			{
				return JsValue.Undefined;
			}

			TypeCode typeCode = value.GetType().GetTypeCode();

			switch (typeCode)
			{
				case TypeCode.Boolean:
					return (bool)value ? JsValue.True : JsValue.False;

				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
					return JsValue.FromInt32(Convert.ToInt32(value));

				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					return JsValue.FromDouble(Convert.ToDouble(value));

				case TypeCode.Char:
				case TypeCode.String:
					return JsValue.FromString((string)value);

				default:
					return FromObject(value);
			}
		}

		/// <summary>
		/// Makes a mapping of array items from the host type to a script type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		private JsValue[] MapToScriptType(object[] args)
		{
			return args.Select(MapToScriptType).ToArray();
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private object MapToHostType(JsValue value)
		{
			JsValueType valueType = value.ValueType;
			JsValue processedValue;
			object result;

			switch (valueType)
			{
				case JsValueType.Null:
					result = null;
					break;
				case JsValueType.Undefined:
					result = Undefined.Value;
					break;
				case JsValueType.Boolean:
					processedValue = value.ConvertToBoolean();
					result = processedValue.ToBoolean();
					break;
				case JsValueType.Number:
					processedValue = value.ConvertToNumber();
					result = NumericHelpers.CastDoubleValueToCorrectType(processedValue.ToDouble());
					break;
				case JsValueType.String:
					processedValue = value.ConvertToString();
					result = processedValue.ToString();
					break;
				case JsValueType.Object:
				case JsValueType.Function:
				case JsValueType.Error:
				case JsValueType.Array:
				case JsValueType.Symbol:
				case JsValueType.ArrayBuffer:
				case JsValueType.TypedArray:
				case JsValueType.DataView:
					result = ToObject(value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return result;
		}

		/// <summary>
		/// Makes a mapping of array items from the script type to a host type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		private object[] MapToHostType(JsValue[] args)
		{
			return args.Select(MapToHostType).ToArray();
		}

		private JsValue FromObject(object value)
		{
			var del = value as Delegate;
			JsValue objValue = del != null ? CreateFunctionFromDelegate(del) : CreateExternalObjectFromObject(value);

			return objValue;
		}

		private object ToObject(JsValue value)
		{
			object result = value.HasExternalData ?
				GCHandle.FromIntPtr(value.ExternalData).Target : value.ConvertToObject();

			return result;
		}

		private JsValue CreateExternalObjectFromObject(object value)
		{
			GCHandle handle = GCHandle.Alloc(value);
			_externalObjects.Add(value);

			JsValue objValue = JsValue.CreateExternalObject(
				GCHandle.ToIntPtr(handle), _externalObjectFinalizeCallback);
			Type type = value.GetType();

			ProjectFields(objValue, type, true);
			ProjectProperties(objValue, type, true);
			ProjectMethods(objValue, type, true);
			FreezeObject(objValue);

			return objValue;
		}

		private void ExternalObjectFinalizeCallback(IntPtr data)
		{
			if (data == IntPtr.Zero)
			{
				return;
			}

			GCHandle handle = GCHandle.FromIntPtr(data);
			object obj = handle.Target;

			if (obj == null)
			{
				return;
			}

			if (_externalObjects != null)
			{
				_externalObjects.Remove(obj);
			}
		}

		private JsValue CreateObjectFromType(Type type)
		{
			JsValue typeValue = CreateConstructor(type);

			ProjectFields(typeValue, type, false);
			ProjectProperties(typeValue, type, false);
			ProjectMethods(typeValue, type, false);
			FreezeObject(typeValue);

			return typeValue;
		}

		private void FreezeObject(JsValue objValue)
		{
			JsValue freezeMethodValue = JsValue.GlobalObject
				.GetProperty("Object")
				.GetProperty("freeze")
				;
			freezeMethodValue.CallFunction(objValue);
		}

		private JsValue CreateFunctionFromDelegate(Delegate value)
		{
			JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
			{
				object[] processedArgs = MapToHostType(args.Skip(1).ToArray());
				ParameterInfo[] parameters = value.GetMethodInfo().GetParameters();
				JsValue undefinedValue = JsValue.Undefined;

				ReflectionHelpers.FixArgumentTypes(ref processedArgs, parameters);

				object result;

				try
				{
					result = value.DynamicInvoke(processedArgs);
				}
				catch (Exception e)
				{
					JsValue errorValue = JsErrorHelpers.CreateError(
						string.Format(Strings.Runtime_HostDelegateInvocationFailed, e.Message));
					JsErrorHelpers.SetException(errorValue);

					return undefinedValue;
				}

				JsValue resultValue = MapToScriptType(result);

				return resultValue;
			};
			_nativeFunctions.Add(nativeFunction);

			JsValue functionValue = JsValue.CreateFunction(nativeFunction);

			return functionValue;
		}

		private JsValue CreateConstructor(Type type)
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			string typeName = type.FullName;
			BindingFlags defaultBindingFlags = ReflectionHelpers.GetDefaultBindingFlags(true);
			ConstructorInfo[] constructors = type.GetConstructors(defaultBindingFlags);

			JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
			{
				JsValue resultValue;
				JsValue undefinedValue = JsValue.Undefined;

				object[] processedArgs = MapToHostType(args.Skip(1).ToArray());
				object result;

				if (processedArgs.Length == 0 && typeInfo.IsValueType)
				{
					result = Activator.CreateInstance(type);
					resultValue = MapToScriptType(result);

					return resultValue;
				}

				if (constructors.Length == 0)
				{
					JsValue errorValue = JsErrorHelpers.CreateError(
						string.Format(Strings.Runtime_HostTypeConstructorNotFound, typeName));
					JsErrorHelpers.SetException(errorValue);

					return undefinedValue;
				}

				var bestFitConstructor = (ConstructorInfo)ReflectionHelpers.GetBestFitMethod(
					constructors, processedArgs);
				if (bestFitConstructor == null)
				{
					JsValue errorValue = JsErrorHelpers.CreateReferenceError(
						string.Format(Strings.Runtime_SuitableConstructorOfHostTypeNotFound, typeName));
					JsErrorHelpers.SetException(errorValue);

					return undefinedValue;
				}

				ReflectionHelpers.FixArgumentTypes(ref processedArgs, bestFitConstructor.GetParameters());

				try
				{
					result = bestFitConstructor.Invoke(processedArgs);
				}
				catch (Exception e)
				{
					JsValue errorValue = JsErrorHelpers.CreateError(
						string.Format(Strings.Runtime_HostTypeConstructorInvocationFailed, typeName, e.Message));
					JsErrorHelpers.SetException(errorValue);

					return undefinedValue;
				}

				resultValue = MapToScriptType(result);

				return resultValue;
			};
			_nativeFunctions.Add(nativeFunction);

			JsValue constructorValue = JsValue.CreateFunction(nativeFunction);

			return constructorValue;
		}

		private void ProjectFields(JsValue target, Type type, bool instance)
		{
			string typeName = type.FullName;
			BindingFlags defaultBindingFlags = ReflectionHelpers.GetDefaultBindingFlags(instance);
			FieldInfo[] fields = type.GetFields(defaultBindingFlags);

			foreach (FieldInfo field in fields)
			{
				string fieldName = field.Name;

				JsValue descriptorValue = JsValue.CreateObject();
				descriptorValue.SetProperty("enumerable", JsValue.True, true);

				JsNativeFunction nativeGetFunction = (callee, isConstructCall, args, argCount, callbackData) =>
				{
					JsValue thisValue = args[0];
					JsValue undefinedValue = JsValue.Undefined;

					object thisObj = null;

					if (instance)
					{
						if (!thisValue.HasExternalData)
						{
							JsValue errorValue = JsErrorHelpers.CreateTypeError(
								string.Format(Strings.Runtime_InvalidThisContextForHostObjectField, fieldName));
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						thisObj = MapToHostType(thisValue);
					}

					object result;

					try
					{
						result = field.GetValue(thisObj);
					}
					catch (Exception e)
					{
						string errorMessage = instance ?
							string.Format(Strings.Runtime_HostObjectFieldGettingFailed, fieldName, e.Message)
							:
							string.Format(Strings.Runtime_HostTypeFieldGettingFailed, fieldName, typeName, e.Message)
							;

						JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					JsValue resultValue = MapToScriptType(result);

					return resultValue;
				};
				_nativeFunctions.Add(nativeGetFunction);

				JsValue getMethodValue = JsValue.CreateFunction(nativeGetFunction);
				descriptorValue.SetProperty("get", getMethodValue, true);

				JsNativeFunction nativeSetFunction = (callee, isConstructCall, args, argCount, callbackData) =>
				{
					JsValue thisValue = args[0];
					JsValue undefinedValue = JsValue.Undefined;

					object thisObj = null;

					if (instance)
					{
						if (!thisValue.HasExternalData)
						{
							JsValue errorValue = JsErrorHelpers.CreateTypeError(
								string.Format(Strings.Runtime_InvalidThisContextForHostObjectField, fieldName));
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						thisObj = MapToHostType(thisValue);
					}

					object value = MapToHostType(args.Skip(1).First());
					ReflectionHelpers.FixFieldValueType(ref value, field);

					try
					{
						field.SetValue(thisObj, value);
					}
					catch (Exception e)
					{
						string errorMessage = instance ?
							string.Format(Strings.Runtime_HostObjectFieldSettingFailed, fieldName, e.Message)
							:
							string.Format(Strings.Runtime_HostTypeFieldSettingFailed, fieldName, typeName, e.Message)
							;

						JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					return undefinedValue;
				};
				_nativeFunctions.Add(nativeSetFunction);

				JsValue setMethodValue = JsValue.CreateFunction(nativeSetFunction);
				descriptorValue.SetProperty("set", setMethodValue, true);

				target.DefineProperty(fieldName, descriptorValue);
			}
		}

		private void ProjectProperties(JsValue target, Type type, bool instance)
		{
			string typeName = type.FullName;
			BindingFlags defaultBindingFlags = ReflectionHelpers.GetDefaultBindingFlags(instance);
			PropertyInfo[] properties = type.GetProperties(defaultBindingFlags);

			foreach (PropertyInfo property in properties)
			{
				string propertyName = property.Name;

				JsValue descriptorValue = JsValue.CreateObject();
				descriptorValue.SetProperty("enumerable", JsValue.True, true);

				if (property.GetGetMethod() != null)
				{
					JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
					{
						JsValue thisValue = args[0];
						JsValue undefinedValue = JsValue.Undefined;

						object thisObj = null;

						if (instance)
						{
							if (!thisValue.HasExternalData)
							{
								JsValue errorValue = JsErrorHelpers.CreateTypeError(
									string.Format(Strings.Runtime_InvalidThisContextForHostObjectProperty, propertyName));
								JsErrorHelpers.SetException(errorValue);

								return undefinedValue;
							}

							thisObj = MapToHostType(thisValue);
						}

						object result;

						try
						{
							result = property.GetValue(thisObj, new object[0]);
						}
						catch (Exception e)
						{
							string errorMessage = instance ?
								string.Format(
									Strings.Runtime_HostObjectPropertyGettingFailed, propertyName, e.Message)
								:
								string.Format(
									Strings.Runtime_HostTypePropertyGettingFailed, propertyName, typeName, e.Message)
								;

							JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						JsValue resultValue = MapToScriptType(result);

						return resultValue;
					};
					_nativeFunctions.Add(nativeFunction);

					JsValue getMethodValue = JsValue.CreateFunction(nativeFunction);
					descriptorValue.SetProperty("get", getMethodValue, true);
				}

				if (property.GetSetMethod() != null)
				{
					JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
					{
						JsValue thisValue = args[0];
						JsValue undefinedValue = JsValue.Undefined;

						object thisObj = null;

						if (instance)
						{
							if (!thisValue.HasExternalData)
							{
								JsValue errorValue = JsErrorHelpers.CreateTypeError(
									string.Format(Strings.Runtime_InvalidThisContextForHostObjectProperty, propertyName));
								JsErrorHelpers.SetException(errorValue);

								return undefinedValue;
							}

							thisObj = MapToHostType(thisValue);
						}

						object value = MapToHostType(args.Skip(1).First());
						ReflectionHelpers.FixPropertyValueType(ref value, property);

						try
						{
							property.SetValue(thisObj, value, new object[0]);
						}
						catch (Exception e)
						{
							string errorMessage = instance ?
								string.Format(
									Strings.Runtime_HostObjectPropertySettingFailed, propertyName, e.Message)
								:
								string.Format(
									Strings.Runtime_HostTypePropertySettingFailed, propertyName, typeName, e.Message)
								;

							JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						return undefinedValue;
					};
					_nativeFunctions.Add(nativeFunction);

					JsValue setMethodValue = JsValue.CreateFunction(nativeFunction);
					descriptorValue.SetProperty("set", setMethodValue, true);
				}

				target.DefineProperty(propertyName, descriptorValue);
			}
		}

		private void ProjectMethods(JsValue target, Type type, bool instance)
		{
			string typeName = type.FullName;
			BindingFlags defaultBindingFlags = ReflectionHelpers.GetDefaultBindingFlags(instance);
			MethodInfo[] methods = type.GetMethods(defaultBindingFlags);
			IEnumerable<IGrouping<string, MethodInfo>> methodGroups = methods.GroupBy(m => m.Name);

			foreach (IGrouping<string, MethodInfo> methodGroup in methodGroups)
			{
				string methodName = methodGroup.Key;
				MethodInfo[] methodCandidates = methodGroup.ToArray();

				JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
				{
					JsValue thisValue = args[0];
					JsValue undefinedValue = JsValue.Undefined;

					object thisObj = null;

					if (instance)
					{
						if (!thisValue.HasExternalData)
						{
							JsValue errorValue = JsErrorHelpers.CreateTypeError(
								string.Format(Strings.Runtime_InvalidThisContextForHostObjectMethod, methodName));
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						thisObj = MapToHostType(thisValue);
					}

					object[] processedArgs = MapToHostType(args.Skip(1).ToArray());

					var bestFitMethod = (MethodInfo)ReflectionHelpers.GetBestFitMethod(
						methodCandidates, processedArgs);
					if (bestFitMethod == null)
					{
						JsValue errorValue = JsErrorHelpers.CreateReferenceError(
							string.Format(Strings.Runtime_SuitableMethodOfHostObjectNotFound, methodName));
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					ReflectionHelpers.FixArgumentTypes(ref processedArgs, bestFitMethod.GetParameters());

					object result;

					try
					{
						result = bestFitMethod.Invoke(thisObj, processedArgs);
					}
					catch (Exception e)
					{
						string errorMessage = instance ?
							string.Format(
								Strings.Runtime_HostObjectMethodInvocationFailed, methodName, e.Message)
							:
							string.Format(
								Strings.Runtime_HostTypeMethodInvocationFailed, methodName, typeName, e.Message)
							;

						JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					JsValue resultValue = MapToScriptType(result);

					return resultValue;
				};
				_nativeFunctions.Add(nativeFunction);

				JsValue methodValue = JsValue.CreateFunction(nativeFunction);
				target.SetProperty(methodName, methodValue, true);
			}
		}

		private static WrapperException WrapJsException(OriginalException originalException)
		{
			WrapperException wrapperException;
			JsErrorCode errorCode = originalException.ErrorCode;
			string description = originalException.Message;
			string message = description;
			string type = string.Empty;
			string documentName = string.Empty;
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
						JsValue messagePropertyValue = errorValue.GetProperty("message");
						description = messagePropertyValue.ConvertToString().ToString();

						JsValue namePropertyValue = errorValue.GetProperty("name");
						type = namePropertyValue.ValueType == JsValueType.String ?
							namePropertyValue.ConvertToString().ToString() : string.Empty;

						JsPropertyId urlPropertyId = JsPropertyId.FromString("url");
						if (metadataValue.HasProperty(urlPropertyId))
						{
							JsValue urlPropertyValue = metadataValue.GetProperty(urlPropertyId);
							documentName = urlPropertyValue.ConvertToString().ToString();
						}

						JsPropertyId linePropertyId = JsPropertyId.FromString("line");
						if (metadataValue.HasProperty(linePropertyId))
						{
							JsValue linePropertyValue = metadataValue.GetProperty(linePropertyId);
							lineNumber = linePropertyValue.ConvertToNumber().ToInt32() + 1;
						}

						JsPropertyId columnPropertyId = JsPropertyId.FromString("column");
						if (metadataValue.HasProperty(columnPropertyId))
						{
							JsValue columnPropertyValue = metadataValue.GetProperty(columnPropertyId);
							columnNumber = columnPropertyValue.ConvertToNumber().ToInt32() + 1;
						}

						string sourceLine = string.Empty;
						JsPropertyId sourcePropertyId = JsPropertyId.FromString("source");
						if (metadataValue.HasProperty(sourcePropertyId))
						{
							JsValue sourcePropertyValue = metadataValue.GetProperty(sourcePropertyId);
							sourceLine = sourcePropertyValue.ConvertToString().ToString();
							sourceFragment = CoreErrorHelpers.GetSourceFragment(sourceLine, columnNumber);
						}

						JsPropertyId stackPropertyId = JsPropertyId.FromString("stack");
						if (errorValue.HasProperty(stackPropertyId))
						{
							JsPropertyId descriptionPropertyId = JsPropertyId.FromString("description");
							if (errorValue.HasProperty(descriptionPropertyId))
							{
								JsValue descriptionPropertyValue = errorValue.GetProperty(descriptionPropertyId);
								if (descriptionPropertyValue.ValueType == JsValueType.String
									|| descriptionPropertyValue.StringLength > 0)
								{
									description = descriptionPropertyValue.ConvertToString().ToString();
								}
							}

							JsValue stackPropertyValue = errorValue.GetProperty(stackPropertyId);
							string messageWithTypeAndCallStack = stackPropertyValue.ValueType == JsValueType.String ?
								stackPropertyValue.ConvertToString().ToString() : string.Empty;
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
					"https://github.com/Taritsyn/JavaScriptEngineSwitcher/wiki/JS-Engine-Switcher:-ChakraCore#{0}";
				Architecture osArchitecture = RuntimeInformation.OSArchitecture;

				StringBuilder descriptionBuilder = StringBuilderPool.GetBuilder();
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
							"JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm");
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
				StringBuilderPool.ReleaseBuilder(descriptionBuilder);

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
						JsValue resultValue = JsContext.RunScript(expression, _jsSourceContext++,
							uniqueDocumentName);

						return MapToHostType(resultValue);
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
						JsContext.RunScript(code, _jsSourceContext++, uniqueDocumentName);
					}
					catch (OriginalException e)
					{
						throw WrapJsException(e);
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
							JsValue[] processedArgs = MapToScriptType(args);

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

						return MapToHostType(resultValue);
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

						return MapToHostType(variableValue);
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
						JsValue inputValue = MapToScriptType(value);
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
						JsValue processedValue = MapToScriptType(value);
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
						JsValue typeValue = CreateObjectFromType(type);
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
				if (_dispatcher != null)
				{
					_dispatcher.Invoke(() =>
					{
						if (_jsContext.IsValid)
						{
							_jsContext.Release();
						}
						_jsRuntime.Dispose();
					});
					_dispatcher.Dispose();
				}

				if (disposing)
				{
					_externalObjects?.Clear();
					_nativeFunctions?.Clear();

					_promiseContinuationCallback = null;
					_externalObjectFinalizeCallback = null;
				}
			}
		}

		#endregion

		#endregion
	}
}