using System;
using System.Reflection;
using System.Text;

using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Resources;
using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// Base class of JS engine
	/// </summary>
	public abstract class JsEngineBase : IJsEngine
	{
		/// <summary>
		/// Default document name
		/// </summary>
		protected const string DefaultDocumentName = "Script Document";

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		protected InterlockedStatedFlag _disposedFlag = new InterlockedStatedFlag();


		protected void VerifyNotDisposed()
		{
			if (_disposedFlag.IsSet())
			{
				throw new ObjectDisposedException(ToString());
			}
		}

		protected abstract object InnerEvaluate(string expression);

		protected virtual object InnerEvaluate(string expression, string documentName)
		{
			return InnerEvaluate(expression);
		}

		protected abstract T InnerEvaluate<T>(string expression);

		protected virtual T InnerEvaluate<T>(string expression, string documentName)
		{
			return InnerEvaluate<T>(expression);
		}

		protected abstract void InnerExecute(string code);

		protected virtual void InnerExecute(string code, string documentName)
		{
			InnerExecute(code);
		}

		protected abstract object InnerCallFunction(string functionName, params object[] args);

		protected abstract T InnerCallFunction<T>(string functionName, params object[] args);

		protected abstract bool InnerHasVariable(string variableName);

		protected abstract object InnerGetVariableValue(string variableName);

		protected abstract T InnerGetVariableValue<T>(string variableName);

		protected abstract void InnerSetVariableValue(string variableName, object value);

		protected abstract void InnerRemoveVariable(string variableName);

		protected virtual void InnerEmbedHostObject(string itemName, object value)
		{
			throw new NotImplementedException();
		}

		protected virtual void InnerEmbedHostType(string itemName, Type type)
		{
			throw new NotImplementedException();
		}

		protected virtual void InnerCollectGarbage()
		{
			throw new NotImplementedException();
		}

		#region IJsEngine implementation

		public abstract string Name
		{
			get;
		}

		public abstract string Version
		{
			get;
		}

		public virtual bool SupportsGarbageCollection
		{
			get
			{
				throw new NotImplementedException();
			}
		}


		public virtual object Evaluate(string expression)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "expression"), "expression");
			}

			return InnerEvaluate(expression);
		}

		public virtual object Evaluate(string expression, string documentName)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "expression"), "expression");
			}

			return InnerEvaluate(expression, documentName);
		}

		public virtual T Evaluate<T>(string expression)
		{
			VerifyNotDisposed();

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

		public virtual T Evaluate<T>(string expression, string documentName)
		{
			VerifyNotDisposed();

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

			return InnerEvaluate<T>(expression, documentName);
		}

		public virtual void Execute(string code)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "code"), "code");
			}

			InnerExecute(code);
		}

		public virtual void Execute(string code, string documentName)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "code"), "code");
			}

			InnerExecute(code, documentName);
		}

		public virtual void ExecuteFile(string path, Encoding encoding = null)
		{
			VerifyNotDisposed();

			if (path == null)
			{
				throw new ArgumentNullException(
					"path", string.Format(Strings.Common_ArgumentIsNull, "path"));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "path"), "path");
			}

			string code = Utils.GetFileTextContent(path, encoding);
			Execute(code, path);
		}

		public virtual void ExecuteResource(string resourceName, Type type)
		{
			VerifyNotDisposed();

			if (resourceName == null)
			{
				throw new ArgumentNullException(
					"resourceName", string.Format(Strings.Common_ArgumentIsNull, "resourceName"));
			}

			if (type == null)
			{
				throw new ArgumentNullException(
					"type", string.Format(Strings.Common_ArgumentIsNull, "type"));
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			Assembly assembly = type.GetTypeInfo().Assembly;
			string nameSpace = type.Namespace;
			string resourceFullName = nameSpace != null ? nameSpace + "." + resourceName : resourceName;

			string code = Utils.GetResourceAsString(resourceFullName, assembly);
			Execute(code, resourceName);
		}

		public virtual void ExecuteResource(string resourceName, Assembly assembly)
		{
			VerifyNotDisposed();

			if (resourceName == null)
			{
				throw new ArgumentNullException(
					"resourceName", string.Format(Strings.Common_ArgumentIsNull, "resourceName"));
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(
					"assembly", string.Format(Strings.Common_ArgumentIsNull, "assembly"));
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			string code = Utils.GetResourceAsString(resourceName, assembly);
			Execute(code, resourceName);
		}

		public virtual object CallFunction(string functionName, params object[] args)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "functionName"), "functionName");
			}

			if (!ValidationHelpers.CheckNameFormat(functionName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidFunctionNameFormat, functionName));
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

		public virtual T CallFunction<T>(string functionName, params object[] args)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "functionName"), "functionName");
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

		public virtual bool HasVariable(string variableName)
		{
			VerifyNotDisposed();

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

			return InnerHasVariable(variableName);
		}

		public virtual object GetVariableValue(string variableName)
		{
			VerifyNotDisposed();

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

			return InnerGetVariableValue(variableName);
		}

		public virtual T GetVariableValue<T>(string variableName)
		{
			VerifyNotDisposed();

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

			return InnerGetVariableValue<T>(variableName);
		}

		public virtual void SetVariableValue(string variableName, object value)
		{
			VerifyNotDisposed();

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

		public virtual void RemoveVariable(string variableName)
		{
			VerifyNotDisposed();

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

			InnerRemoveVariable(variableName);
		}

		public virtual void EmbedHostObject(string itemName, object value)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(itemName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "itemName"), "itemName");
			}

			if (!ValidationHelpers.CheckNameFormat(itemName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidScriptItemNameFormat, itemName));
			}

			if (value != null)
			{
				Type itemType = value.GetType();

				if (ValidationHelpers.IsPrimitiveType(itemType)
					|| itemType == typeof(Undefined)
					|| itemType == typeof(DateTime))
				{
					throw new NotSupportedTypeException(
						string.Format(Strings.Runtime_EmbeddedHostObjectTypeNotSupported, itemName, itemType.FullName));
				}
			}
			else
			{
				throw new ArgumentNullException("value", string.Format(Strings.Common_ArgumentIsNull, "value"));
			}

			InnerEmbedHostObject(itemName, value);
		}

		public virtual void EmbedHostType(string itemName, Type type)
		{
			VerifyNotDisposed();

			if (string.IsNullOrWhiteSpace(itemName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "itemName"), "itemName");
			}

			if (!ValidationHelpers.CheckNameFormat(itemName))
			{
				throw new FormatException(
					string.Format(Strings.Runtime_InvalidScriptItemNameFormat, itemName));
			}

			if (type != null)
			{
				if (ValidationHelpers.IsPrimitiveType(type)
					|| type == typeof(Undefined))
				{
					throw new NotSupportedTypeException(
						string.Format(Strings.Runtime_EmbeddedHostTypeNotSupported, type.FullName));
				}
			}
			else
			{
				throw new ArgumentNullException("type", string.Format(Strings.Common_ArgumentIsNull, "type"));
			}

			InnerEmbedHostType(itemName, type);
		}

		public virtual void CollectGarbage()
		{
			VerifyNotDisposed();

			InnerCollectGarbage();
		}

		#endregion

		#region IDisposable implementation

		public abstract void Dispose();

		#endregion
	}
}