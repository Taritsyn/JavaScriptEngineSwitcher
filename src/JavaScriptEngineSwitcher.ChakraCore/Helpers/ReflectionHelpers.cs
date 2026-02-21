using System;
#if NET45_OR_GREATER || NETSTANDARD || NET10_0_OR_GREATER
using System.Buffers;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NET40

using PolyfillsForOldDotNet.System.Buffers;
#endif

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.ChakraCore.Helpers
{
	/// <summary>
	/// Reflection helpers
	/// </summary>
	internal static class ReflectionHelpers
	{
		private static readonly PropertyInfo[] _disallowedProperties =
		{
			typeof(Delegate).GetProperty("Method"),
			typeof(Exception).GetProperty("TargetSite")
		};

		private static readonly MethodInfo[] _disallowedMethods =
		{
			typeof(object).GetMethod("GetType"),
			typeof(Exception).GetMethod("GetType")
		};


		public static BindingFlags GetDefaultBindingFlags(bool instance)
		{
			BindingFlags bindingFlags = BindingFlags.Public;
			if (instance)
			{
				bindingFlags |= BindingFlags.Instance;
			}
			else
			{
				bindingFlags |= BindingFlags.Static;
			}

			return bindingFlags;
		}

		public static bool IsAllowedProperty(PropertyInfo property)
		{
			bool isAllowed = !_disallowedProperties.Contains(property, MemberComparer<PropertyInfo>.Instance);

			return isAllowed;
		}

		public static bool IsAllowedMethod(MethodInfo method)
		{
			bool isAllowed = !_disallowedMethods.Contains(method, MemberComparer<MethodInfo>.Instance);

			return isAllowed;
		}

		public static bool IsFullyFledgedMethod(MethodInfo method)
		{
			if (!method.Attributes.HasFlag(MethodAttributes.SpecialName))
			{
				return true;
			}

			string name = method.Name;
			bool isFullyFledged = !(name.StartsWith("get_", StringComparison.Ordinal)
				|| name.StartsWith("set_", StringComparison.Ordinal));

			return isFullyFledged;
		}

		public static void FixFieldValueType(ref object value, FieldInfo field)
		{
			if (value is null)
			{
				return;
			}

			Type valueType = value.GetType();
			Type fieldType = field.FieldType;

			if (valueType != fieldType)
			{
				object convertedValue;

				if (TypeConverter.TryConvertToType(value, fieldType, out convertedValue))
				{
					value = convertedValue;
				}
			}
		}

		public static void FixPropertyValueType(ref object value, PropertyInfo property)
		{
			if (value is null)
			{
				return;
			}

			Type valueType = value.GetType();
			Type propertyType = property.PropertyType;

			if (valueType != propertyType)
			{
				object convertedValue;

				if (TypeConverter.TryConvertToType(value, propertyType, out convertedValue))
				{
					value = convertedValue;
				}
			}
		}

		public static void FixArgumentTypes(ref object[] argValues, ParameterInfo[] parameters)
		{
			int argCount = argValues.Length;
			if (argCount == 0)
			{
				return;
			}

			int parameterCount = parameters.Length;

			for (int argIndex = 0; argIndex < argCount; argIndex++)
			{
				if (argIndex >= parameterCount)
				{
					break;
				}

				object argValue = argValues[argIndex];
				if (argValue is null)
				{
					continue;
				}

				Type argType = argValue.GetType();

				ParameterInfo parameter = parameters[argIndex];
				Type parameterType = parameter.ParameterType;

				if (argType != parameterType)
				{
					object convertedArgValue;

					if (TypeConverter.TryConvertToType(argValue, parameterType, out convertedArgValue))
					{
						argValues[argIndex] = convertedArgValue;
					}
				}
			}
		}

		public static MethodBase GetBestFitMethod(MethodBase[] methods, object[] argValues)
		{
			int methodCount = methods.Length;
			if (methodCount == 0)
			{
				return null;
			}

			if (methodCount == 1)
			{
				MethodBase method = methods[0];
				ParameterInfo[] parameters = method.GetParameters();

				MethodBase bestFitMethod = null;
				if (CompareParameterTypes(argValues, parameters, out _))
				{
					bestFitMethod = method;
				}

				return bestFitMethod;
			}

			MethodWithMetadata[] compatibleMethods = null;
			int compatibleMethodCount = 0;

			var methodArrayPool = ArrayPool<MethodWithMetadata>.Shared;
			MethodWithMetadata[] buffer = methodArrayPool.Rent(methodCount);

			try
			{
				for (int methodIndex = 0; methodIndex < methodCount; methodIndex++)
				{
					MethodBase method = methods[methodIndex];
					ParameterInfo[] parameters = method.GetParameters();
					ushort compatibilityScore;

					if (CompareParameterTypes(argValues, parameters, out compatibilityScore))
					{
						compatibleMethodCount++;

						int compatibleMethodIndex = compatibleMethodCount - 1;
						buffer[compatibleMethodIndex] = new MethodWithMetadata
						{
							Method = method,
							CompatibilityScore = compatibilityScore
						};
					}
				}

				if (compatibleMethodCount > 0)
				{
					if (compatibleMethodCount == 1)
					{
						return buffer[0].Method;
					}

					compatibleMethods = new MethodWithMetadata[compatibleMethodCount];
					Array.Copy(buffer, compatibleMethods, compatibleMethodCount);
				}
			}
			finally
			{
				bool clearArray = compatibleMethodCount > 0;
				methodArrayPool.Return(buffer, clearArray);
			}

			if (compatibleMethods is not null)
			{
				MethodWithMetadata bestFitMethod = compatibleMethods
					.OrderByDescending(m => m.CompatibilityScore)
					.First()
					;

				return bestFitMethod.Method;
			}

			return null;
		}

		private static bool CompareParameterTypes(object[] argValues, ParameterInfo[] parameters,
			out ushort compatibilityScore)
		{
			int argCount = argValues.Length;
			int parameterCount = parameters.Length;
			compatibilityScore = 0;

			if (argCount != parameterCount)
			{
				return false;
			}
			else if (argCount == 0)
			{
				compatibilityScore = ushort.MaxValue;
				return true;
			}

			for (int argIndex = 0; argIndex < argCount; argIndex++)
			{
				object argValue = argValues[argIndex];
				Type argType = argValue is not null ? argValue.GetType() : typeof(object);
				ParameterInfo parameter = parameters[argIndex];
				Type parameterType = parameter.ParameterType;

				if (argType == parameterType)
				{
					compatibilityScore++;
				}
				else
				{
					// TODO: It is necessary to calculate the compatibility score based on length
					// of inheritance and interface implementation chains.
					object convertedArgValue;

					if (!TypeConverter.TryConvertToType(argValue, parameterType, out convertedArgValue))
					{
						return false;
					}
				}
			}

			return true;
		}


		private sealed class MemberComparer<T> : EqualityComparer<T>
			where T : MemberInfo
		{
			public static MemberComparer<T> Instance { get; } = new MemberComparer<T>();


			private MemberComparer()
			{ }


			#region MemberComparer overrides

			public override bool Equals(T x, T y)
			{
				if (x is null && y is null)
				{
					return true;
				}
				else if (x is null || y is null)
				{
					return false;
				}

				return x.Module == y.Module
#if !NETSTANDARD1_3
					&& x.MetadataToken == y.MetadataToken
#else
					&& x.DeclaringType == y.DeclaringType
					&& x.Name == y.Name
#endif
					;
			}

			public override int GetHashCode(T obj)
			{
				return obj is not null ? obj.GetHashCode() : 0;
			}

			#endregion
		}

		private sealed class MethodWithMetadata
		{
			public MethodBase Method
			{
				get;
				set;
			}

			/// TODO: In future will need to change type to <c>double</c>
			public ushort CompatibilityScore
			{
				get;
				set;
			}
		}
	}
}