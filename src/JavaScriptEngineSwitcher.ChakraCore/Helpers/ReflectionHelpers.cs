namespace JavaScriptEngineSwitcher.ChakraCore.Helpers
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Core.Utilities;

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
			int argCount = argValues.Length;
			var methodCandidates = methods
				.Select(m => new
				{
					Method = m,
					ParameterTypes = m.GetParameters()
						.Select(p => p.ParameterType)
						.ToArray()
				})
				.ToArray()
				;

			var methodsWithSameArity = methodCandidates
				.Where(m => m.ParameterTypes.Length == argCount)
				.ToArray()
				;
			if (methodsWithSameArity.Length == 0)
			{
				return null;
			}

			Type[] argTypes = argValues
				.Select(a => a.GetType())
				.ToArray()
				;
			var weaklyCompatibleMethods = methodsWithSameArity
				.Where(m => CompareParameterTypes(argValues, argTypes, m.ParameterTypes, false))
				.ToArray()
				;

			int weaklyCompatibleMethodCount = weaklyCompatibleMethods.Length;
			if (weaklyCompatibleMethodCount > 0)
			{
				if (weaklyCompatibleMethodCount == 1)
				{
					return weaklyCompatibleMethods[0].Method;
				}

				var strictlyCompatibleMethods = weaklyCompatibleMethods
					.Where(m => CompareParameterTypes(argValues, argTypes, m.ParameterTypes, true))
					.ToArray()
					;
				if (strictlyCompatibleMethods.Length > 0)
				{
					return strictlyCompatibleMethods[0].Method;
				}

				return weaklyCompatibleMethods[0].Method;
			}

			return null;
		}

		private static bool CompareParameterTypes(object[] argValues, Type[] argTypes, Type[] parameterTypes,
			bool strictСompliance)
		{
			int argValueCount = argValues.Length;
			int argTypeCount = argTypes.Length;
			int parameterCount = parameterTypes.Length;

			if (argValueCount != argTypeCount || argTypeCount != parameterCount)
			{
				return false;
			}

			for (int argIndex = 0; argIndex < argValueCount; argIndex++)
			{
				object argValue = argValues[argIndex];
				Type argType = argTypes[argIndex];
				Type parameterType = parameterTypes[argIndex];

				if (argType != parameterType)
				{
					if (!strictСompliance
						&& NumericHelpers.IsNumericType(argType) && NumericHelpers.IsNumericType(parameterType))
					{
						object convertedArgValue;

						if (!TypeConverter.TryConvertToType(argValue, parameterType, out convertedArgValue))
						{
							return false;
						}

						if (argValue != convertedArgValue)
						{
							return false;
						}

						continue;
					}

					return false;
				}
			}

			return true;
		}
	}
}