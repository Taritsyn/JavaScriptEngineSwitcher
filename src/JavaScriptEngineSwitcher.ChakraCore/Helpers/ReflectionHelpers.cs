using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.ChakraCore.Helpers
{
	/// <summary>
	/// Reflection helpers
	/// </summary>
	internal static class ReflectionHelpers
	{
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

		public static void FixFieldValueType(ref object value, FieldInfo field)
		{
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

			for (int argIndex = 0; argIndex < argCount; argIndex++)
			{
				object argValue = argValues[argIndex];
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
			MethodWithMetadata[] methodCandidates = methods
				.Select(m => new MethodWithMetadata
				{
					Method = m,
					ParameterTypes = m.GetParameters()
						.Select(p => p.ParameterType)
						.ToArray()
				})
				.ToArray()
				;
			int argCount = argValues.Length;
			MethodWithMetadata[] sameArityMethods = methodCandidates
				.Where(m => m.ParameterTypes.Length == argCount)
				.ToArray()
				;

			int sameArityMethodCount = sameArityMethods.Length;
			if (sameArityMethodCount == 0)
			{
				return null;
			}

			Type[] argTypes = argValues
				.Select(a => a.GetType())
				.ToArray()
				;
			var compatibleMethods = new List<MethodWithMetadata>();

			for (int methodIndex = 0; methodIndex < sameArityMethodCount; methodIndex++)
			{
				MethodWithMetadata method = sameArityMethods[methodIndex];
				ushort compatibilityScore;

				if (CompareParameterTypes(argValues, argTypes, method.ParameterTypes, out compatibilityScore))
				{
					method.CompatibilityScore = compatibilityScore;
					compatibleMethods.Add(method);
				}
			}

			int compatibleMethodCount = compatibleMethods.Count;
			if (compatibleMethodCount > 0)
			{
				if (compatibleMethodCount == 1)
				{
					return compatibleMethods[0].Method;
				}

				MethodWithMetadata bestFitMethod = compatibleMethods
					.OrderByDescending(m => m.CompatibilityScore)
					.First()
					;

				return bestFitMethod.Method;
			}

			return null;
		}

		private static bool CompareParameterTypes(object[] argValues, Type[] argTypes, Type[] parameterTypes,
			out ushort compatibilityScore)
		{
			int argValueCount = argValues.Length;
			int argTypeCount = argTypes.Length;
			int parameterCount = parameterTypes.Length;
			compatibilityScore = 0;

			if (argValueCount != argTypeCount || argTypeCount != parameterCount)
			{
				return false;
			}

			for (int argIndex = 0; argIndex < argValueCount; argIndex++)
			{
				object argValue = argValues[argIndex];
				Type argType = argTypes[argIndex];
				Type parameterType = parameterTypes[argIndex];

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

					continue;
				}
			}

			return true;
		}


		private sealed class MethodWithMetadata
		{
			public MethodBase Method
			{
				get;
				set;
			}

			public Type[] ParameterTypes
			{
				get;
				set;
			}

			/// TODO: In future will need to change type to <code>double</code>
			public ushort CompatibilityScore
			{
				get;
				set;
			}
		}
	}
}