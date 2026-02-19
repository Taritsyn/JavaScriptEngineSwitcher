using System;
using System.Reflection;

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.Yantra.Helpers
{
	/// <summary>
	/// Reflection helpers
	/// </summary>
	internal static class ReflectionHelpers
	{
		public static void FixArgumentTypes(ref object[] argValues, ParameterInfo[] parameters)
		{
			int argCount = argValues.Length;
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
	}
}