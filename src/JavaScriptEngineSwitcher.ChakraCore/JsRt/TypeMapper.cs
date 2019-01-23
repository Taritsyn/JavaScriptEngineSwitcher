using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Extensions;
using JavaScriptEngineSwitcher.Core.Utilities;

using JavaScriptEngineSwitcher.ChakraCore.Helpers;
using JavaScriptEngineSwitcher.ChakraCore.JsRt.Embedding;
using JavaScriptEngineSwitcher.ChakraCore.Resources;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Type mapper
	/// </summary>
	internal sealed class TypeMapper : IDisposable
	{
		/// <summary>
		/// Storage for lazy-initialized embedded objects
		/// </summary>
		private ConcurrentDictionary<EmbeddedObjectKey, Lazy<EmbeddedObject>> _lazyEmbeddedObjects;

		/// <summary>
		/// Callback for finalization of embedded object
		/// </summary>
		private JsFinalizeCallback _embeddedObjectFinalizeCallback;

		/// <summary>
		/// Synchronizer of embedded object storage's initialization
		/// </summary>
		private readonly object _embeddedObjectStorageInitializationSynchronizer = new object();

		/// <summary>
		/// Flag indicating whether the embedded object storage is initialized
		/// </summary>
		private bool _embeddedObjectStorageInitialized;

		/// <summary>
		/// Storage for lazy-initialized embedded types
		/// </summary>
		private ConcurrentDictionary<string, Lazy<EmbeddedType>> _lazyEmbeddedTypes;

		/// <summary>
		/// Callback for finalization of embedded type
		/// </summary>
		private JsFinalizeCallback _embeddedTypeFinalizeCallback;

		/// <summary>
		/// Synchronizer of embedded type storage's initialization
		/// </summary>
		private readonly object _embeddedTypeStorageInitializationSynchronizer = new object();

		/// <summary>
		/// Flag indicating whether the embedded type storage is initialized
		/// </summary>
		private bool _embeddedTypeStorageInitialized;

		/// <summary>
		/// Flag indicating whether this object is disposed
		/// </summary>
		private readonly InterlockedStatedFlag _disposedFlag = new InterlockedStatedFlag();


		/// <summary>
		/// Constructs an instance of type mapper
		/// </summary>
		public TypeMapper()
		{ }


		/// <summary>
		/// Creates a JavaScript value from an host object if the it does not already exist
		/// </summary>
		/// <param name="obj">Instance of host type</param>
		/// <returns>JavaScript value created from an host object</returns>
		public JsValue GetOrCreateScriptObject(object obj)
		{
			if (!_embeddedObjectStorageInitialized)
			{
				lock (_embeddedObjectStorageInitializationSynchronizer)
				{
					if (!_embeddedObjectStorageInitialized)
					{
						_lazyEmbeddedObjects = new ConcurrentDictionary<EmbeddedObjectKey, Lazy<EmbeddedObject>>();
						_embeddedObjectFinalizeCallback = EmbeddedObjectFinalizeCallback;

						_embeddedObjectStorageInitialized = true;
					}
				}
			}

			var embeddedObjectKey = new EmbeddedObjectKey(obj);
			EmbeddedObject embeddedObject = _lazyEmbeddedObjects.GetOrAdd(
				embeddedObjectKey,
				key => new Lazy<EmbeddedObject>(() => CreateEmbeddedObjectOrFunction(obj))
			).Value;

			return embeddedObject.ScriptValue;
		}

		/// <summary>
		/// Creates a JavaScript value from an host type if the it does not already exist
		/// </summary>
		/// <param name="type">Host type</param>
		/// <returns>JavaScript value created from an host type</returns>
		public JsValue GetOrCreateScriptType(Type type)
		{
			if (!_embeddedTypeStorageInitialized)
			{
				lock (_embeddedTypeStorageInitializationSynchronizer)
				{
					if (!_embeddedTypeStorageInitialized)
					{
						_lazyEmbeddedTypes = new ConcurrentDictionary<string, Lazy<EmbeddedType>>();
						_embeddedTypeFinalizeCallback = EmbeddedTypeFinalizeCallback;

						_embeddedTypeStorageInitialized = true;
					}
				}
			}

			string embeddedTypeKey = type.AssemblyQualifiedName;
			EmbeddedType embeddedType = _lazyEmbeddedTypes.GetOrAdd(
				embeddedTypeKey,
				key => new Lazy<EmbeddedType>(() => CreateEmbeddedType(type))
			).Value;

			return embeddedType.ScriptValue;
		}

		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		public JsValue MapToScriptType(object value)
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
					return GetOrCreateScriptObject(value);
			}
		}

		/// <summary>
		/// Makes a mapping of array items from the host type to a script type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		public JsValue[] MapToScriptType(object[] args)
		{
			return args.Select(MapToScriptType).ToArray();
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		public object MapToHostType(JsValue value)
		{
			JsValueType valueType = value.ValueType;
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
					result = value.ToBoolean();
					break;
				case JsValueType.Number:
					result = NumericHelpers.CastDoubleValueToCorrectType(value.ToDouble());
					break;
				case JsValueType.String:
					result = value.ToString();
					break;
				case JsValueType.Object:
				case JsValueType.Function:
				case JsValueType.Error:
				case JsValueType.Array:
				case JsValueType.Symbol:
				case JsValueType.ArrayBuffer:
				case JsValueType.TypedArray:
				case JsValueType.DataView:
					result = value.HasExternalData ?
						GCHandle.FromIntPtr(value.ExternalData).Target : value.ConvertToObject();
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
		public object[] MapToHostType(JsValue[] args)
		{
			return args.Select(MapToHostType).ToArray();
		}

		private EmbeddedObject CreateEmbeddedObjectOrFunction(object obj)
		{
			var del = obj as Delegate;
			EmbeddedObject embeddedObject = del != null ?
				CreateEmbeddedFunction(del) : CreateEmbeddedObject(obj);

			return embeddedObject;
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private EmbeddedObject CreateEmbeddedObject(object obj)
		{
			GCHandle objHandle = GCHandle.Alloc(obj);
			IntPtr objPtr = GCHandle.ToIntPtr(objHandle);
			JsValue objValue = JsValue.CreateExternalObject(objPtr, _embeddedObjectFinalizeCallback);

			var embeddedObject = new EmbeddedObject(obj, objValue);

			ProjectFields(embeddedObject);
			ProjectProperties(embeddedObject);
			ProjectMethods(embeddedObject);
			FreezeObject(objValue);

			return embeddedObject;
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private EmbeddedObject CreateEmbeddedFunction(Delegate del)
		{
			JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
			{
				object[] processedArgs = GetHostItemMemberArguments(args);
#if NET40
				MethodInfo method = del.Method;
#else
				MethodInfo method = del.GetMethodInfo();
#endif
				ParameterInfo[] parameters = method.GetParameters();

				ReflectionHelpers.FixArgumentTypes(ref processedArgs, parameters);

				object result;

				try
				{
					result = del.DynamicInvoke(processedArgs);
				}
				catch (Exception e)
				{
					JsValue undefinedValue = JsValue.Undefined;
					JsValue errorValue = JsErrorHelpers.CreateError(
						string.Format(Strings.Runtime_HostDelegateInvocationFailed, e.Message));
					JsErrorHelpers.SetException(errorValue);

					return undefinedValue;
				}

				JsValue resultValue = MapToScriptType(result);

				return resultValue;
			};

			GCHandle delHandle = GCHandle.Alloc(del);
			IntPtr delPtr = GCHandle.ToIntPtr(delHandle);
			JsValue prototypeValue = JsValue.CreateExternalObject(delPtr, _embeddedObjectFinalizeCallback);

			JsValue functionValue = JsValue.CreateFunction(nativeFunction);
			functionValue.Prototype = prototypeValue;

			var embeddedObject = new EmbeddedObject(del, functionValue,
				new List<JsNativeFunction> { nativeFunction });

			return embeddedObject;
		}

		private void EmbeddedObjectFinalizeCallback(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
			{
				return;
			}

			GCHandle objHandle = GCHandle.FromIntPtr(ptr);
			object obj = objHandle.Target;
			var lazyEmbeddedObjects = _lazyEmbeddedObjects;

			if (obj != null && lazyEmbeddedObjects != null)
			{
				var embeddedObjectKey = new EmbeddedObjectKey(obj);
				Lazy<EmbeddedObject> lazyEmbeddedObject;

				if (lazyEmbeddedObjects.TryRemove(embeddedObjectKey, out lazyEmbeddedObject))
				{
					lazyEmbeddedObject.Value?.Dispose();
				}
			}

			objHandle.Free();
		}

		private EmbeddedType CreateEmbeddedType(Type type)
		{
#if NET40
			Type typeInfo = type;
#else
			TypeInfo typeInfo = type.GetTypeInfo();
#endif
			string typeName = type.FullName;
			BindingFlags defaultBindingFlags = ReflectionHelpers.GetDefaultBindingFlags(true);
			ConstructorInfo[] constructors = type.GetConstructors(defaultBindingFlags);

			JsNativeFunction nativeConstructorFunction = (callee, isConstructCall, args, argCount, callbackData) =>
			{
				object result;
				JsValue resultValue;
				object[] processedArgs = GetHostItemMemberArguments(args);

				if (processedArgs.Length == 0 && typeInfo.IsValueType)
				{
					result = Activator.CreateInstance(type);
					resultValue = MapToScriptType(result);

					return resultValue;
				}

				if (constructors.Length == 0)
				{
					JsValue undefinedValue = JsValue.Undefined;
					JsValue errorValue = JsErrorHelpers.CreateError(
						string.Format(Strings.Runtime_HostTypeConstructorNotFound, typeName));
					JsErrorHelpers.SetException(errorValue);

					return undefinedValue;
				}

				var bestFitConstructor = (ConstructorInfo)ReflectionHelpers.GetBestFitMethod(
					constructors, processedArgs);
				if (bestFitConstructor == null)
				{
					JsValue undefinedValue = JsValue.Undefined;
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
					JsValue undefinedValue = JsValue.Undefined;
					JsValue errorValue = JsErrorHelpers.CreateError(
						string.Format(Strings.Runtime_HostTypeConstructorInvocationFailed, typeName, e.Message));
					JsErrorHelpers.SetException(errorValue);

					return undefinedValue;
				}

				resultValue = MapToScriptType(result);

				return resultValue;
			};

			string embeddedTypeKey = type.AssemblyQualifiedName;
			GCHandle embeddedTypeKeyHandle = GCHandle.Alloc(embeddedTypeKey);
			IntPtr embeddedTypeKeyPtr = GCHandle.ToIntPtr(embeddedTypeKeyHandle);
			JsValue prototypeValue = JsValue.CreateExternalObject(embeddedTypeKeyPtr,
				_embeddedTypeFinalizeCallback);

			JsValue typeValue = JsValue.CreateFunction(nativeConstructorFunction);
			typeValue.Prototype = prototypeValue;

			var embeddedType = new EmbeddedType(type, typeValue,
				new List<JsNativeFunction> { nativeConstructorFunction });

			ProjectFields(embeddedType);
			ProjectProperties(embeddedType);
			ProjectMethods(embeddedType);
			FreezeObject(typeValue);

			return embeddedType;
		}

		private void EmbeddedTypeFinalizeCallback(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
			{
				return;
			}

			GCHandle embeddedTypeKeyHandle = GCHandle.FromIntPtr(ptr);
			var embeddedTypeKey = (string)embeddedTypeKeyHandle.Target;
			var lazyEmbeddedTypes = _lazyEmbeddedTypes;

			if (!string.IsNullOrEmpty(embeddedTypeKey) && lazyEmbeddedTypes != null)
			{
				Lazy<EmbeddedType> lazyEmbeddedType;

				if (lazyEmbeddedTypes.TryRemove(embeddedTypeKey, out lazyEmbeddedType))
				{
					lazyEmbeddedType.Value?.Dispose();
				}
			}

			embeddedTypeKeyHandle.Free();
		}

		private void ProjectFields(EmbeddedItem externalItem)
		{
			Type type = externalItem.HostType;
			object obj = externalItem.HostObject;
			JsValue typeValue = externalItem.ScriptValue;
			bool instance = externalItem.IsInstance;
			IList<JsNativeFunction> nativeFunctions = externalItem.NativeFunctions;

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
					if (instance && obj == null)
					{
						JsValue undefinedValue = JsValue.Undefined;
						JsValue errorValue = JsErrorHelpers.CreateTypeError(
							string.Format(Strings.Runtime_InvalidThisContextForHostObjectField, fieldName));
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					object result;

					try
					{
						result = field.GetValue(obj);
					}
					catch (Exception e)
					{
						string errorMessage = instance ?
							string.Format(Strings.Runtime_HostObjectFieldGettingFailed, fieldName, e.Message)
							:
							string.Format(Strings.Runtime_HostTypeFieldGettingFailed, fieldName, typeName, e.Message)
							;

						JsValue undefinedValue = JsValue.Undefined;
						JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					JsValue resultValue = MapToScriptType(result);

					return resultValue;
				};
				nativeFunctions.Add(nativeGetFunction);

				JsValue getMethodValue = JsValue.CreateFunction(nativeGetFunction);
				descriptorValue.SetProperty("get", getMethodValue, true);

				JsNativeFunction nativeSetFunction = (callee, isConstructCall, args, argCount, callbackData) =>
				{
					if (instance && obj == null)
					{
						JsValue undefinedValue = JsValue.Undefined;
						JsValue errorValue = JsErrorHelpers.CreateTypeError(
							string.Format(Strings.Runtime_InvalidThisContextForHostObjectField, fieldName));
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					object value = MapToHostType(args[1]);
					ReflectionHelpers.FixFieldValueType(ref value, field);

					try
					{
						field.SetValue(obj, value);
					}
					catch (Exception e)
					{
						string errorMessage = instance ?
							string.Format(Strings.Runtime_HostObjectFieldSettingFailed, fieldName, e.Message)
							:
							string.Format(Strings.Runtime_HostTypeFieldSettingFailed, fieldName, typeName, e.Message)
							;

						JsValue undefinedValue = JsValue.Undefined;
						JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					return JsValue.Undefined;
				};
				nativeFunctions.Add(nativeSetFunction);

				JsValue setMethodValue = JsValue.CreateFunction(nativeSetFunction);
				descriptorValue.SetProperty("set", setMethodValue, true);

				typeValue.DefineProperty(fieldName, descriptorValue);
			}
		}

		private void ProjectProperties(EmbeddedItem externalItem)
		{
			Type type = externalItem.HostType;
			object obj = externalItem.HostObject;
			JsValue typeValue = externalItem.ScriptValue;
			IList<JsNativeFunction> nativeFunctions = externalItem.NativeFunctions;
			bool instance = externalItem.IsInstance;

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
					JsNativeFunction nativeGetFunction = (callee, isConstructCall, args, argCount, callbackData) =>
					{
						if (instance && obj == null)
						{
							JsValue undefinedValue = JsValue.Undefined;
							JsValue errorValue = JsErrorHelpers.CreateTypeError(
								string.Format(Strings.Runtime_InvalidThisContextForHostObjectProperty, propertyName));
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						object result;

						try
						{
							result = property.GetValue(obj, new object[0]);
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

							JsValue undefinedValue = JsValue.Undefined;
							JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						JsValue resultValue = MapToScriptType(result);

						return resultValue;
					};
					nativeFunctions.Add(nativeGetFunction);

					JsValue getMethodValue = JsValue.CreateFunction(nativeGetFunction);
					descriptorValue.SetProperty("get", getMethodValue, true);
				}

				if (property.GetSetMethod() != null)
				{
					JsNativeFunction nativeSetFunction = (callee, isConstructCall, args, argCount, callbackData) =>
					{
						if (instance && obj == null)
						{
							JsValue undefinedValue = JsValue.Undefined;
							JsValue errorValue = JsErrorHelpers.CreateTypeError(
								string.Format(Strings.Runtime_InvalidThisContextForHostObjectProperty, propertyName));
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						object value = MapToHostType(args[1]);
						ReflectionHelpers.FixPropertyValueType(ref value, property);

						try
						{
							property.SetValue(obj, value, new object[0]);
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

							JsValue undefinedValue = JsValue.Undefined;
							JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
							JsErrorHelpers.SetException(errorValue);

							return undefinedValue;
						}

						return JsValue.Undefined;
					};
					nativeFunctions.Add(nativeSetFunction);

					JsValue setMethodValue = JsValue.CreateFunction(nativeSetFunction);
					descriptorValue.SetProperty("set", setMethodValue, true);
				}

				typeValue.DefineProperty(propertyName, descriptorValue);
			}
		}

		private void ProjectMethods(EmbeddedItem externalItem)
		{
			Type type = externalItem.HostType;
			object obj = externalItem.HostObject;
			JsValue typeValue = externalItem.ScriptValue;
			IList<JsNativeFunction> nativeFunctions = externalItem.NativeFunctions;
			bool instance = externalItem.IsInstance;

			string typeName = type.FullName;
			BindingFlags defaultBindingFlags = ReflectionHelpers.GetDefaultBindingFlags(instance);
			IEnumerable<MethodInfo> methods = type.GetMethods(defaultBindingFlags)
				.Where(ReflectionHelpers.IsFullyFledgedMethod);
			IEnumerable<IGrouping<string, MethodInfo>> methodGroups = methods.GroupBy(m => m.Name);

			foreach (IGrouping<string, MethodInfo> methodGroup in methodGroups)
			{
				string methodName = methodGroup.Key;
				MethodInfo[] methodCandidates = methodGroup.ToArray();

				JsNativeFunction nativeFunction = (callee, isConstructCall, args, argCount, callbackData) =>
				{
					if (instance && obj == null)
					{
						JsValue undefinedValue = JsValue.Undefined;
						JsValue errorValue = JsErrorHelpers.CreateTypeError(
							string.Format(Strings.Runtime_InvalidThisContextForHostObjectMethod, methodName));
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					object[] processedArgs = GetHostItemMemberArguments(args);

					var bestFitMethod = (MethodInfo)ReflectionHelpers.GetBestFitMethod(
						methodCandidates, processedArgs);
					if (bestFitMethod == null)
					{
						JsValue undefinedValue = JsValue.Undefined;
						JsValue errorValue = JsErrorHelpers.CreateReferenceError(
							string.Format(Strings.Runtime_SuitableMethodOfHostObjectNotFound, methodName));
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					ReflectionHelpers.FixArgumentTypes(ref processedArgs, bestFitMethod.GetParameters());

					object result;

					try
					{
						result = bestFitMethod.Invoke(obj, processedArgs);
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

						JsValue undefinedValue = JsValue.Undefined;
						JsValue errorValue = JsErrorHelpers.CreateError(errorMessage);
						JsErrorHelpers.SetException(errorValue);

						return undefinedValue;
					}

					JsValue resultValue = MapToScriptType(result);

					return resultValue;
				};
				nativeFunctions.Add(nativeFunction);

				JsValue methodValue = JsValue.CreateFunction(nativeFunction);
				typeValue.SetProperty(methodName, methodValue, true);
			}
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private object[] GetHostItemMemberArguments(JsValue[] args)
		{
			object[] processedArgs = args.Length > 1 ?
				MapToHostType(args.Skip(1).ToArray()) : new object[0];

			return processedArgs;
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private void FreezeObject(JsValue objValue)
		{
			JsValue freezeMethodValue = JsValue.GlobalObject
				.GetProperty("Object")
				.GetProperty("freeze")
				;
			freezeMethodValue.CallFunction(objValue);
		}

		#region IDisposable implementation

		/// <summary>
		/// Disposes a type mapper
		/// </summary>
		public void Dispose()
		{
			if (_disposedFlag.Set())
			{
				var lazyEmbeddedObjects = _lazyEmbeddedObjects;
				if (lazyEmbeddedObjects != null)
				{
					if (lazyEmbeddedObjects.Count > 0)
					{
						foreach (EmbeddedObjectKey key in lazyEmbeddedObjects.Keys)
						{
							Lazy<EmbeddedObject> lazyEmbeddedObject;

							if (lazyEmbeddedObjects.TryGetValue(key, out lazyEmbeddedObject))
							{
								lazyEmbeddedObject.Value?.Dispose();
							}
						}

						lazyEmbeddedObjects.Clear();
					}

					_lazyEmbeddedObjects = null;
				}

				_embeddedObjectFinalizeCallback = null;

				var lazyEmbeddedTypes = _lazyEmbeddedTypes;
				if (lazyEmbeddedTypes != null)
				{
					if (lazyEmbeddedTypes.Count > 0)
					{
						foreach (string key in lazyEmbeddedTypes.Keys)
						{
							Lazy<EmbeddedType> lazyEmbeddedType;

							if (lazyEmbeddedTypes.TryGetValue(key, out lazyEmbeddedType))
							{
								lazyEmbeddedType.Value?.Dispose();
							}
						}

						lazyEmbeddedTypes.Clear();
					}

					_lazyEmbeddedTypes = null;
				}

				_embeddedTypeFinalizeCallback = null;
			}
		}

		#endregion
	}
}