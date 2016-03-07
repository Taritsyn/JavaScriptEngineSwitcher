namespace JavaScriptEngineSwitcher.ChakraCore
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;

	using OriginalJsException = JsRt.JsException;

	using Core;
	using Core.Utilities;
	using CoreStrings = Core.Resources.Strings;

	using Helpers;
	using JsRt;
	using Resources;

	/// <summary>
	/// Adapter for ChakraCore JavaScript engine
	/// </summary>
	public sealed class ChakraCoreJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JavaScript engine
		/// </summary>
		private const string ENGINE_NAME = "ChakraCore JavaScript engine";

		/// <summary>
		/// Version of original JavaScript engine
		/// </summary>
		private const string ENGINE_VERSION = "Mar 6, 2016";

		/// <summary>
		/// Instance of JavaScript runtime
		/// </summary>
		private JsRuntime _jsRuntime;

		/// <summary>
		/// Instance of JavaScript context
		/// </summary>
		private readonly JsContext _jsContext;

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

		/// <summary>
		/// List of external objects
		/// </summary>
		private readonly ISet<object> _externalObjects;

		/// <summary>
		/// Callback for finalization of external object
		/// </summary>
		private JsObjectFinalizeCallback _externalObjectFinalizeCallback;

		/// <summary>
		/// List of native function callbacks
		/// </summary>
		private readonly ISet<JsNativeFunction> _nativeFunctions;

		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		public override string Name
		{
			get { return ENGINE_NAME; }
		}

		/// <summary>
		/// Gets a version of original JavaScript engine
		/// </summary>
		public override string Version
		{
			get { return ENGINE_VERSION; }
		}


		/// <summary>
		/// Static constructor
		/// </summary>
		static ChakraCoreJsEngine()
		{
			AssemblyResolver.Initialize();
		}

		/// <summary>
		/// Constructs a instance of adapter for ChakraCore JavaScript engine
		/// </summary>
		public ChakraCoreJsEngine()
		{
			try
			{
				_jsRuntime = JsRuntime.Create(JsRuntimeAttributes.AllowScriptInterrupt, JsRuntimeVersion.VersionEdge, null);
				_jsContext = _jsRuntime.CreateContext();
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						ENGINE_NAME, e.Message), ENGINE_NAME, ENGINE_VERSION, e);
			}

			_externalObjects = new HashSet<object>();
			_externalObjectFinalizeCallback = ExternalObjectFinalizeCallback;
			_nativeFunctions = new HashSet<JsNativeFunction>();
		}

		/// <summary>
		/// Destructs instance of adapter for ChakraCore JavaScript engine
		/// </summary>
		~ChakraCoreJsEngine()
		{
			Dispose(false);
		}


		private void InvokeScript(Action action)
		{
			lock (_executionSynchronizer)
			using (new JsScope(_jsContext))
			{
				try
				{
					action();
				}
				catch (OriginalJsException e)
				{
					throw ConvertJsExceptionToJsRuntimeException(e);
				}
			}
		}

		private T InvokeScript<T>(Func<T> func)
		{
			lock (_executionSynchronizer)
			using (new JsScope(_jsContext))
			{
				try
				{
					return func();
				}
				catch (OriginalJsException e)
				{
					throw ConvertJsExceptionToJsRuntimeException(e);
				}
			}
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
				lock (_executionSynchronizer)
				{
					_jsRuntime.Dispose();

					_externalObjects.Clear();
					_nativeFunctions.Clear();
					_externalObjectFinalizeCallback = null;
				}
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

			TypeCode typeCode = Type.GetTypeCode(value.GetType());

			switch (typeCode)
			{
				case TypeCode.Boolean:
					return JsValue.FromBoolean((bool)value);

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

			lock (_executionSynchronizer)
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
				JsValue undefinedValue = JsValue.Undefined;

				ReflectionHelpers.FixArgumentTypes(ref processedArgs, value.Method.GetParameters());

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
			string typeName = type.FullName;
			BindingFlags defaultBindingFlags = ReflectionHelpers.GetDefaultBindingFlags(true);
			ConstructorInfo[] constructors = type.GetConstructors(defaultBindingFlags);

			JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
			{
				JsValue resultValue;
				JsValue undefinedValue = JsValue.Undefined;

				object[] processedArgs = MapToHostType(args.Skip(1).ToArray());
				object result;

				if (processedArgs.Length == 0 && type.IsValueType)
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

		private static JsRuntimeException ConvertJsExceptionToJsRuntimeException(
			OriginalJsException jsException)
		{
			string message = jsException.Message;
			string category = string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			string sourceFragment = string.Empty;

			var jsScriptException = jsException as JsScriptException;
			if (jsScriptException != null)
			{
				category = "Script error";
				JsValue errorValue = jsScriptException.Error;

				JsValue messagePropertyValue = errorValue.GetProperty("message");
				string scriptMessage = messagePropertyValue.ConvertToString().ToString();
				if (!string.IsNullOrWhiteSpace(scriptMessage))
				{
					message = string.Format("{0}: {1}", message.TrimEnd('.'), scriptMessage);
				}

				JsPropertyId linePropertyId = JsPropertyId.FromString("line");
				if (errorValue.HasProperty(linePropertyId))
				{
					JsValue linePropertyValue = errorValue.GetProperty(linePropertyId);
					lineNumber = (int)linePropertyValue.ConvertToNumber().ToDouble() + 1;
				}

				JsPropertyId columnPropertyId = JsPropertyId.FromString("column");
				if (errorValue.HasProperty(columnPropertyId))
				{
					JsValue columnPropertyValue = errorValue.GetProperty(columnPropertyId);
					columnNumber = (int)columnPropertyValue.ConvertToNumber().ToDouble() + 1;
				}

				JsPropertyId sourcePropertyId = JsPropertyId.FromString("source");
				if (errorValue.HasProperty(sourcePropertyId))
				{
					JsValue sourcePropertyValue = errorValue.GetProperty(sourcePropertyId);
					sourceFragment = sourcePropertyValue.ConvertToString().ToString();
				}
			}
			else if (jsException is JsUsageException)
			{
				category = "Usage error";
			}
			else if (jsException is JsEngineException)
			{
				category = "Engine error";
			}
			else if (jsException is JsFatalException)
			{
				category = "Fatal error";
			}

			var jsEngineException = new JsRuntimeException(message, ENGINE_NAME, ENGINE_VERSION)
			{
				ErrorCode = ((uint)jsException.ErrorCode).ToString(CultureInfo.InvariantCulture),
				Category = category,
				LineNumber = lineNumber,
				ColumnNumber = columnNumber,
				SourceFragment = sourceFragment,
				HelpLink = jsException.HelpLink
			};

			return jsEngineException;
		}

		#endregion

		#region JsEngineBase implementation

		protected override object InnerEvaluate(string expression)
		{
			object result = InvokeScript(() =>
			{
				JsValue resultValue = JsContext.RunScript(expression);

				return MapToHostType(resultValue);
			});

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			InvokeScript(() => JsContext.RunScript(code));
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result = InvokeScript(() =>
			{
				JsValue globalObj = JsValue.GlobalObject;
				JsPropertyId functionId = JsPropertyId.FromString(functionName);

				bool functionExist = globalObj.HasProperty(functionId);
				if (!functionExist)
				{
					throw new JsRuntimeException(
						string.Format(CoreStrings.Runtime_FunctionNotExist, functionName));
				}

				var processedArgs = MapToScriptType(args);
				var allProcessedArgs = new[] { globalObj }.Concat(processedArgs).ToArray();

				JsValue functionValue = globalObj.GetProperty(functionId);
				JsValue resultValue = functionValue.CallFunction(allProcessedArgs);

				return MapToHostType(resultValue);
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
			bool result = InvokeScript(() =>
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
			});

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result = InvokeScript(() =>
			{
				JsValue variableValue = JsValue.GlobalObject.GetProperty(variableName);

				return MapToHostType(variableValue);
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
			InvokeScript(() =>
			{
				JsValue inputValue = MapToScriptType(value);
				JsValue.GlobalObject.SetProperty(variableName, inputValue, true);
			});
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InvokeScript(() =>
			{
				JsValue globalObj = JsValue.GlobalObject;
				JsPropertyId variableId = JsPropertyId.FromString(variableName);

				if (globalObj.HasProperty(variableId))
				{
					globalObj.SetProperty(variableId, JsValue.Undefined, true);
				}
			});
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			InvokeScript(() =>
			{
				JsValue processedValue = MapToScriptType(value);
				JsValue.GlobalObject.SetProperty(itemName, processedValue, true);
			});
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			InvokeScript(() =>
			{
				JsValue typeValue = CreateObjectFromType(type);
				JsValue.GlobalObject.SetProperty(itemName, typeValue, true);
			});
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

		#endregion
	}
}