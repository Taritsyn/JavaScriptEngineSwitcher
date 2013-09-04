namespace JavaScriptEngineSwitcher.Core
{
	using System;
	using System.Reflection;
	using System.Text;

	using Helpers;
	using Resources;
	using Utilities;

	/// <summary>
	/// Base class of JavaScript engine
	/// </summary>
	public abstract class JsEngineBase : IJsEngine
	{
		public object Evaluate(string expression)
		{
			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "expression"), "expression");
			}

			return InnerEvaluate(expression);
		}

		public T Evaluate<T>(string expression)
		{
			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "expression"), "expression");
			}

			Type returnValueType = typeof(T);
			if (!ValidationHelpers.IsSupportedType(returnValueType))
			{
				throw new NotSupportedTypeException(
					string.Format(Strings.Runtime_ReturnValueTypeNotSupported, returnValueType.FullName));
			}

			return InnerEvaluate<T>(expression);
		}

		public void Execute(string code)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "code"), "code");
			}

			InnerExecute(code);
		}

		public void ExecuteFile(string path, Encoding encoding = null)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "path"), "path");
			}

			string code = Utils.GetFileTextContent(path, encoding);
			Execute(code);
		}

		public void ExecuteResource(string resourceName, Type type)
		{
			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			if (type == null)
			{
				throw new ArgumentNullException(
					"type", string.Format(Strings.Common_ArgumentIsNull, "type"));
			}

			string code = Utils.GetResourceAsString(resourceName, type);
			Execute(code);
		}

		public void ExecuteResource(string resourceName, Assembly assembly)
		{
			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(
					"assembly", string.Format(Strings.Common_ArgumentIsNull, "assembly"));
			}

			string code = Utils.GetResourceAsString(resourceName, assembly);
			Execute(code);
		}

		public object CallFunction(string functionName, params object[] args)
		{
			if (string.IsNullOrWhiteSpace(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "functionName"), "functionName");
			}

			if (args == null)
			{
				throw new ArgumentNullException("args", Strings.Common_ValueIsNull);
			}

			if (!ValidationHelpers.CheckNameFormat(functionName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidFunctionNameFormat, functionName));
			}

			if (!ValidationHelpers.CheckNameAllowability(functionName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_FunctionNameIsForbidden, functionName));
			}

			int argumentCount = args.Length;
			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					object argument = args[argumentIndex];

					if (argument != null)
					{
						Type argType = argument.GetType();

						if (!ValidationHelpers.IsSupportedType(argType))
						{
							throw new NotSupportedTypeException(
								string.Format(Strings.Runtime_FunctionParameterTypeNotSupported,
											functionName, argType.FullName));
						}
					}
				}
			}

			return InnerCallFunction(functionName, args);
		}

		public T CallFunction<T>(string functionName, params object[] args)
		{
			if (string.IsNullOrWhiteSpace(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "functionName"), "functionName");
			}

			if (args == null)
			{
				throw new ArgumentNullException("args", Strings.Common_ValueIsNull);
			}

			Type returnValueType = typeof(T);
			if (!ValidationHelpers.IsSupportedType(returnValueType))
			{
				throw new NotSupportedTypeException(
					string.Format(Strings.Runtime_ReturnValueTypeNotSupported, returnValueType.FullName));
			}

			if (!ValidationHelpers.CheckNameFormat(functionName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidFunctionNameFormat, functionName));
			}

			if (!ValidationHelpers.CheckNameAllowability(functionName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_FunctionNameIsForbidden, functionName));
			}

			int argumentCount = args.Length;
			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					object argument = args[argumentIndex];

					if (argument != null)
					{
						Type argType = argument.GetType();

						if (!ValidationHelpers.IsSupportedType(argType))
						{
							throw new NotSupportedTypeException(
								string.Format(Strings.Runtime_FunctionParameterTypeNotSupported,
											functionName, argType.FullName));
						}
					}
				}
			}

			return InnerCallFunction<T>(functionName, args);
		}

		public bool HasVariable(string variableName)
		{
			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "variableName"), "variableName");
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidVariableNameFormat, variableName));
			}

			if (!ValidationHelpers.CheckNameAllowability(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_VariableNameIsForbidden, variableName));
			}

			return InnerHasVariable(variableName);
		}

		public object GetVariableValue(string variableName)
		{
			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "variableName"), "variableName");
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidVariableNameFormat, variableName));
			}

			if (!ValidationHelpers.CheckNameAllowability(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_VariableNameIsForbidden, variableName));
			}

			return InnerGetVariableValue(variableName);
		}

		public T GetVariableValue<T>(string variableName)
		{
			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "variableName"), "variableName");
			}

			Type returnValueType = typeof(T);
			if (!ValidationHelpers.IsSupportedType(returnValueType))
			{
				throw new NotSupportedTypeException(
					string.Format(Strings.Runtime_ReturnValueTypeNotSupported, returnValueType.FullName));
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidVariableNameFormat, variableName));
			}

			if (!ValidationHelpers.CheckNameAllowability(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_VariableNameIsForbidden, variableName));
			}

			return InnerGetVariableValue<T>(variableName);
		}

		public void SetVariableValue(string variableName, object value)
		{
			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "variableName"), "variableName");
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidVariableNameFormat, variableName));
			}

			if (!ValidationHelpers.CheckNameAllowability(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_VariableNameIsForbidden, variableName));
			}

			if (value != null)
			{
				Type variableType = value.GetType();

				if (!ValidationHelpers.IsSupportedType(variableType))
				{
					throw new NotSupportedTypeException(
						string.Format(Strings.Runtime_VariableTypeNotSupported,
									variableName, variableType.FullName));
				}
			}

			InnerSetVariableValue(variableName, value);
		}

		public void RemoveVariable(string variableName)
		{
			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "variableName"), "variableName");
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidVariableNameFormat, variableName));
			}

			if (!ValidationHelpers.CheckNameAllowability(variableName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_VariableNameIsForbidden, variableName));
			}

			InnerRemoveVariable(variableName);
		}

		public abstract void Dispose();

		protected abstract object InnerEvaluate(string expression);

		protected abstract T InnerEvaluate<T>(string expression);

		protected abstract void InnerExecute(string code);

		protected abstract object InnerCallFunction(string functionName, params object[] args);

		protected abstract T InnerCallFunction<T>(string functionName, params object[] args);

		protected abstract bool InnerHasVariable(string variableName);

		protected abstract object InnerGetVariableValue(string variableName);

		protected abstract T InnerGetVariableValue<T>(string variableName);

		protected abstract void InnerSetVariableValue(string variableName, object value);

		protected abstract void InnerRemoveVariable(string variableName);
	}
}