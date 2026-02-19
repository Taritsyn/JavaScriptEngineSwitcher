using System;
using System.Reflection;
using System.Runtime.CompilerServices;
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


		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		protected void VerifyNotDisposed()
		{
			if (_disposedFlag.IsSet())
			{
				throw new ObjectDisposedException(ToString());
			}
		}

		protected virtual IPrecompiledScript InnerPrecompile(string code)
		{
			throw new NotImplementedException();
		}

		protected virtual IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			throw new NotImplementedException();
		}

		protected abstract object InnerEvaluate(string expression);

		protected abstract object InnerEvaluate(string expression, string documentName);

		protected abstract T InnerEvaluate<T>(string expression);

		protected abstract T InnerEvaluate<T>(string expression, string documentName);

		protected abstract void InnerExecute(string code);

		protected abstract void InnerExecute(string code, string documentName);

		protected virtual void InnerExecute(IPrecompiledScript precompiledScript)
		{
			throw new NotImplementedException();
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

		protected virtual void InnerInterrupt()
		{
			throw new NotImplementedException();
		}

		protected virtual void InnerCollectGarbage()
		{
			throw new NotImplementedException();
		}

		#region IJsEngine implementation

		/// <inheritdoc/>
		public abstract string Name
		{
			get;
		}

		/// <inheritdoc/>
		public abstract string Version
		{
			get;
		}

		/// <inheritdoc/>
		public virtual bool SupportsScriptPrecompilation
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <inheritdoc/>
		public virtual bool SupportsScriptInterruption
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <inheritdoc/>
		public virtual bool SupportsGarbageCollection
		{
			get
			{
				throw new NotImplementedException();
			}
		}


		/// <inheritdoc/>
		public virtual IPrecompiledScript Precompile(string code)
		{
			VerifyNotDisposed();

			if (code is null)
			{
				throw new ArgumentNullException(
					nameof(code),
					string.Format(Strings.Common_ArgumentIsNull, nameof(code))
				);
			}

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(code)),
					nameof(code)
				);
			}

			return InnerPrecompile(code);
		}

		/// <inheritdoc/>
		public virtual IPrecompiledScript Precompile(string code, string documentName)
		{
			VerifyNotDisposed();

			if (code is null)
			{
				throw new ArgumentNullException(
					nameof(code),
					string.Format(Strings.Common_ArgumentIsNull, nameof(code))
				);
			}

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(code)),
					nameof(code)
				);
			}

			if (!string.IsNullOrWhiteSpace(documentName)
				&& !ValidationHelpers.CheckDocumentNameFormat(documentName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidDocumentNameFormat, documentName),
					nameof(documentName)
				);
			}

			return InnerPrecompile(code, documentName);
		}

		/// <inheritdoc/>
		public virtual IPrecompiledScript PrecompileFile(string path, Encoding encoding = null)
		{
			VerifyNotDisposed();

			if (path is null)
			{
				throw new ArgumentNullException(
					nameof(path),
					string.Format(Strings.Common_ArgumentIsNull, nameof(path))
				);
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(path)),
					nameof(path)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(path))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidFileNameFormat, path),
					nameof(path)
				);
			}

			string code = Utils.GetFileTextContent(path, encoding);
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new JsUsageException(
					string.Format(Strings.Usage_CannotPrecompileEmptyFile, path),
					Name, Version
				);
			}

			return InnerPrecompile(code, path);
		}

		/// <inheritdoc/>
		public virtual IPrecompiledScript PrecompileResource(string resourceName, Type type)
		{
			VerifyNotDisposed();

			if (resourceName is null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (type is null)
			{
				throw new ArgumentNullException(
					nameof(type),
					string.Format(Strings.Common_ArgumentIsNull, nameof(type))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidResourceNameFormat, resourceName),
					nameof(resourceName)
				);
			}

#if NET40
			Assembly assembly = type.Assembly;
#else
			Assembly assembly = type.GetTypeInfo().Assembly;
#endif
			string nameSpace = type.Namespace;
			string resourceFullName = nameSpace is not null ? nameSpace + "." + resourceName : resourceName;

			string code = Utils.GetResourceAsString(resourceFullName, assembly);
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new JsUsageException(
					string.Format(Strings.Usage_CannotPrecompileEmptyResource, resourceFullName),
					Name, Version
				);
			}

			return InnerPrecompile(code, resourceName);
		}

		/// <inheritdoc/>
		public virtual IPrecompiledScript PrecompileResource(string resourceName, Assembly assembly)
		{
			VerifyNotDisposed();

			if (resourceName is null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(
					nameof(assembly),
					string.Format(Strings.Common_ArgumentIsNull, nameof(assembly))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidResourceNameFormat, resourceName),
					nameof(resourceName)
				);
			}

			string code = Utils.GetResourceAsString(resourceName, assembly);
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new JsUsageException(
					string.Format(Strings.Usage_CannotPrecompileEmptyResource, resourceName),
					Name, Version
				);
			}

			return InnerPrecompile(code, resourceName);
		}

		/// <inheritdoc/>
		public virtual object Evaluate(string expression)
		{
			VerifyNotDisposed();

			if (expression is null)
			{
				throw new ArgumentNullException(
					nameof(expression),
					string.Format(Strings.Common_ArgumentIsNull, nameof(expression))
				);
			}

			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(expression)),
					nameof(expression)
				);
			}

			return InnerEvaluate(expression);
		}

		/// <inheritdoc/>
		public virtual object Evaluate(string expression, string documentName)
		{
			VerifyNotDisposed();

			if (expression is null)
			{
				throw new ArgumentNullException(
					nameof(expression),
					string.Format(Strings.Common_ArgumentIsNull, nameof(expression))
				);
			}

			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(expression)),
					nameof(expression)
				);
			}

			if (!string.IsNullOrWhiteSpace(documentName)
				&& !ValidationHelpers.CheckDocumentNameFormat(documentName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidDocumentNameFormat, documentName),
					nameof(documentName)
				);
			}

			return InnerEvaluate(expression, documentName);
		}

		/// <inheritdoc/>
		public virtual T Evaluate<T>(string expression)
		{
			VerifyNotDisposed();

			if (expression is null)
			{
				throw new ArgumentNullException(
					nameof(expression),
					string.Format(Strings.Common_ArgumentIsNull, nameof(expression))
				);
			}

			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(expression)),
					nameof(expression)
				);
			}

			Type returnValueType = typeof(T);
			if (!ValidationHelpers.IsSupportedType(returnValueType))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_ReturnValueTypeNotSupported, returnValueType.FullName),
					nameof(T)
				);
			}

			return InnerEvaluate<T>(expression);
		}

		/// <inheritdoc/>
		public virtual T Evaluate<T>(string expression, string documentName)
		{
			VerifyNotDisposed();

			if (expression is null)
			{
				throw new ArgumentNullException(
					nameof(expression),
					string.Format(Strings.Common_ArgumentIsNull, nameof(expression))
				);
			}

			if (string.IsNullOrWhiteSpace(expression))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(expression)),
					nameof(expression)
				);
			}

			if (!string.IsNullOrWhiteSpace(documentName)
				&& !ValidationHelpers.CheckDocumentNameFormat(documentName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidDocumentNameFormat, documentName),
					nameof(documentName)
				);
			}

			Type returnValueType = typeof(T);
			if (!ValidationHelpers.IsSupportedType(returnValueType))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_ReturnValueTypeNotSupported, returnValueType.FullName),
					nameof(T)
				);
			}

			return InnerEvaluate<T>(expression, documentName);
		}

		/// <inheritdoc/>
		public virtual void Execute(string code)
		{
			VerifyNotDisposed();

			if (code is null)
			{
				throw new ArgumentNullException(
					nameof(code),
					string.Format(Strings.Common_ArgumentIsNull, nameof(code))
				);
			}

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(code)),
					nameof(code)
				);
			}

			InnerExecute(code);
		}

		/// <inheritdoc/>
		public virtual void Execute(string code, string documentName)
		{
			VerifyNotDisposed();

			if (code is null)
			{
				throw new ArgumentNullException(
					nameof(code),
					string.Format(Strings.Common_ArgumentIsNull, nameof(code))
				);
			}

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(code)),
					nameof(code)
				);
			}

			if (!string.IsNullOrWhiteSpace(documentName)
				&& !ValidationHelpers.CheckDocumentNameFormat(documentName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidDocumentNameFormat, documentName),
					nameof(documentName)
				);
			}

			InnerExecute(code, documentName);
		}

		/// <inheritdoc/>
		public virtual void Execute(IPrecompiledScript precompiledScript)
		{
			VerifyNotDisposed();

			if (precompiledScript is null)
			{
				throw new ArgumentNullException(
					nameof(precompiledScript),
					string.Format(Strings.Common_ArgumentIsNull, nameof(precompiledScript))
				);
			}

			if (precompiledScript.EngineName != Name)
			{
				throw new JsUsageException(
					string.Format(Strings.Usage_CannotExecutePrecompiledScriptForAnotherJsEngine,
						precompiledScript.EngineName),
					Name, Version
				);
			}

			InnerExecute(precompiledScript);
		}

		/// <inheritdoc/>
		public virtual void ExecuteFile(string path, Encoding encoding = null)
		{
			VerifyNotDisposed();

			if (path is null)
			{
				throw new ArgumentNullException(
					nameof(path),
					string.Format(Strings.Common_ArgumentIsNull, nameof(path))
				);
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(path)),
					nameof(path)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(path))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidFileNameFormat, path),
					nameof(path)
				);
			}

			string code = Utils.GetFileTextContent(path, encoding);
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new JsUsageException(
					string.Format(Strings.Usage_CannotExecuteEmptyFile, path),
					Name, Version
				);
			}

			InnerExecute(code, path);
		}

		/// <inheritdoc/>
		public virtual void ExecuteResource(string resourceName, Type type)
		{
			VerifyNotDisposed();

			if (resourceName is null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (type is null)
			{
				throw new ArgumentNullException(
					nameof(type),
					string.Format(Strings.Common_ArgumentIsNull, nameof(type))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidResourceNameFormat, resourceName),
					nameof(resourceName)
				);
			}

#if NET40
			Assembly assembly = type.Assembly;
#else
			Assembly assembly = type.GetTypeInfo().Assembly;
#endif
			string nameSpace = type.Namespace;
			string resourceFullName = nameSpace is not null ? nameSpace + "." + resourceName : resourceName;

			string code = Utils.GetResourceAsString(resourceFullName, assembly);
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new JsUsageException(
					string.Format(Strings.Usage_CannotExecuteEmptyResource, resourceFullName),
					Name, Version
				);
			}

			InnerExecute(code, resourceName);
		}

		/// <inheritdoc/>
		public virtual void ExecuteResource(string resourceName, Assembly assembly)
		{
			VerifyNotDisposed();

			if (resourceName is null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(
					nameof(assembly),
					string.Format(Strings.Common_ArgumentIsNull, nameof(assembly))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (!ValidationHelpers.CheckDocumentNameFormat(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidResourceNameFormat, resourceName),
					nameof(resourceName)
				);
			}

			string code = Utils.GetResourceAsString(resourceName, assembly);
			if (string.IsNullOrWhiteSpace(code))
			{
				throw new JsUsageException(
					string.Format(Strings.Usage_CannotExecuteEmptyResource, resourceName),
					Name, Version
				);
			}

			InnerExecute(code, resourceName);
		}

		/// <inheritdoc/>
		public virtual object CallFunction(string functionName, params object[] args)
		{
			VerifyNotDisposed();

			if (functionName is null)
			{
				throw new ArgumentNullException(
					nameof(functionName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(functionName))
				);
			}

			if (string.IsNullOrWhiteSpace(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(functionName)),
					nameof(functionName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidFunctionNameFormat, nameof(functionName)),
					nameof(functionName)
				);
			}

			int argumentCount = args.Length;
			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					object argument = args[argumentIndex];

					if (argument is not null)
					{
						Type argType = argument.GetType();

						if (!ValidationHelpers.IsSupportedType(argType))
						{
							throw new ArgumentException(
								string.Format(Strings.Usage_FunctionParameterTypeNotSupported,
									functionName, argType.FullName),
								nameof(args)
							);
						}
					}
				}
			}

			return InnerCallFunction(functionName, args);
		}

		/// <inheritdoc/>
		public virtual T CallFunction<T>(string functionName, params object[] args)
		{
			VerifyNotDisposed();

			if (functionName is null)
			{
				throw new ArgumentNullException(
					nameof(functionName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(functionName))
				);
			}

			if (string.IsNullOrWhiteSpace(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(functionName)),
					nameof(functionName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(functionName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidFunctionNameFormat, functionName),
					nameof(functionName)
				);
			}

			int argumentCount = args.Length;
			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					object argument = args[argumentIndex];

					if (argument is not null)
					{
						Type argType = argument.GetType();

						if (!ValidationHelpers.IsSupportedType(argType))
						{
							throw new ArgumentException(
								string.Format(Strings.Usage_FunctionParameterTypeNotSupported,
									functionName, argType.FullName),
								nameof(args)
							);
						}
					}
				}
			}

			Type returnValueType = typeof(T);
			if (!ValidationHelpers.IsSupportedType(returnValueType))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_ReturnValueTypeNotSupported, returnValueType.FullName),
					nameof(T)
				);
			}

			return InnerCallFunction<T>(functionName, args);
		}

		/// <inheritdoc/>
		public virtual bool HasVariable(string variableName)
		{
			VerifyNotDisposed();

			if (variableName is null)
			{
				throw new ArgumentNullException(
					nameof(variableName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(variableName))
				);
			}

			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(variableName)),
					nameof(variableName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidVariableNameFormat, variableName),
					nameof(variableName)
				);
			}

			return InnerHasVariable(variableName);
		}

		/// <inheritdoc/>
		public virtual object GetVariableValue(string variableName)
		{
			VerifyNotDisposed();

			if (variableName is null)
			{
				throw new ArgumentNullException(
					nameof(variableName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(variableName))
				);
			}

			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(variableName)),
					nameof(variableName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidVariableNameFormat, variableName),
					nameof(variableName)
				);
			}

			return InnerGetVariableValue(variableName);
		}

		/// <inheritdoc/>
		public virtual T GetVariableValue<T>(string variableName)
		{
			VerifyNotDisposed();

			if (variableName is null)
			{
				throw new ArgumentNullException(
					nameof(variableName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(variableName))
				);
			}

			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(variableName)),
					nameof(variableName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidVariableNameFormat, variableName),
					nameof(variableName)
				);
			}

			Type returnValueType = typeof(T);
			if (!ValidationHelpers.IsSupportedType(returnValueType))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_ReturnValueTypeNotSupported, returnValueType.FullName),
					nameof(T)
				);
			}

			return InnerGetVariableValue<T>(variableName);
		}

		/// <inheritdoc/>
		public virtual void SetVariableValue(string variableName, object value)
		{
			VerifyNotDisposed();

			if (variableName is null)
			{
				throw new ArgumentNullException(
					nameof(variableName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(variableName))
				);
			}

			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(variableName)),
					nameof(variableName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidVariableNameFormat, variableName),
					nameof(variableName)
				);
			}

			if (value is not null)
			{
				Type variableType = value.GetType();

				if (!ValidationHelpers.IsSupportedType(variableType))
				{
					throw new ArgumentException(
						string.Format(Strings.Usage_VariableTypeNotSupported,
							variableName, variableType.FullName),
						nameof(value)
					);
				}
			}

			InnerSetVariableValue(variableName, value);
		}

		/// <inheritdoc/>
		public virtual void RemoveVariable(string variableName)
		{
			VerifyNotDisposed();

			if (variableName is null)
			{
				throw new ArgumentNullException(
					nameof(variableName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(variableName))
				);
			}

			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(variableName)),
					nameof(variableName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(variableName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidVariableNameFormat, variableName),
					nameof(variableName)
				);
			}

			InnerRemoveVariable(variableName);
		}

		/// <inheritdoc/>
		public virtual void EmbedHostObject(string itemName, object value)
		{
			VerifyNotDisposed();

			if (itemName is null)
			{
				throw new ArgumentNullException(
					nameof(itemName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(itemName))
				);
			}

			if (value is null)
			{
				throw new ArgumentNullException(
					nameof(value),
					string.Format(Strings.Common_ArgumentIsNull, nameof(value))
				);
			}

			if (string.IsNullOrWhiteSpace(itemName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(itemName)),
					nameof(itemName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(itemName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidScriptItemNameFormat, itemName),
					nameof(itemName)
				);
			}

			Type itemType = value.GetType();

			if (ValidationHelpers.IsPrimitiveType(itemType)
				|| itemType == typeof(Undefined)
				|| itemType == typeof(DateTime))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_EmbeddedHostObjectTypeNotSupported, itemName, itemType.FullName),
					nameof(value)
				);
			}

			InnerEmbedHostObject(itemName, value);
		}

		/// <inheritdoc/>
		public virtual void EmbedHostType(string itemName, Type type)
		{
			VerifyNotDisposed();

			if (itemName is null)
			{
				throw new ArgumentNullException(
					nameof(itemName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(itemName))
				);
			}

			if (type is null)
			{
				throw new ArgumentNullException(
					nameof(type),
					string.Format(Strings.Common_ArgumentIsNull, nameof(type))
				);
			}

			if (string.IsNullOrWhiteSpace(itemName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(itemName)),
					nameof(itemName)
				);
			}

			if (!ValidationHelpers.CheckNameFormat(itemName))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_InvalidScriptItemNameFormat, itemName),
					nameof(itemName)
				);
			}

			if (ValidationHelpers.IsPrimitiveType(type) || type == typeof(Undefined))
			{
				throw new ArgumentException(
					string.Format(Strings.Usage_EmbeddedHostTypeNotSupported, type.FullName),
					nameof(type)
				);
			}

			InnerEmbedHostType(itemName, type);
		}

		/// <inheritdoc/>
		public virtual void Interrupt()
		{
			VerifyNotDisposed();

			InnerInterrupt();
		}

		/// <inheritdoc/>
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