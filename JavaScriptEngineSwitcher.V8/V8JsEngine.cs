namespace JavaScriptEngineSwitcher.V8
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using System.Reflection;
	using System.Web.Script.Serialization;

	using Core;
	using Core.Constants;
	using CoreStrings = Core.Resources.Strings;
	using Configuration;
	using Resources;

	/// <summary>
	/// Adapter for Noesis Javascript .NET
	/// </summary>
	public sealed class V8JsEngine : JsEngineBase
	{
		/// <summary>
		/// Full name of JS engine assembly
		/// </summary>
		private const string JS_ENGINE_ASSEMBLY_FULL_NAME
			= "Noesis.Javascript, Version=0.0.0.0, Culture=neutral, PublicKeyToken=ae36d046c7f89f85";

		/// <summary>
		/// Name of JS context type
		/// </summary>
		private const string JS_CONTEXT_TYPE_NAME = "Noesis.Javascript.JavascriptContext";

		/// <summary>
		/// Name of JS exception type
		/// </summary>
		private const string JS_EXCEPTION_TYPE_NAME = "Noesis.Javascript.JavascriptException";

		/// <summary>
		/// Assembly, which contains JS Engine
		/// </summary>
		private static Assembly _jsEngineAssembly;

		/// <summary>
		/// Synchronizer of assembly loading
		/// </summary>
		private static readonly object _assemblyLoadingSynchronizer = new object();

		/// <summary>
		/// Flag that assembly is loaded
		/// </summary>
		private bool _assemblyLoaded;

		/// <summary>
		/// Type of JS context
		/// </summary>
		private Type _jsContextType;

		/// <summary>
		/// Type of JS exception
		/// </summary>
		private Type _jsExceptionType;

		/// <summary>
		/// Instance of JS engine
		/// </summary>
		private object _jsEngineObject;

		/// <summary>
		/// Information about the method <code>Run</code>
		/// </summary>
		private MethodInfo _runMethodInfo;

		/// <summary>
		/// Information about the method <code>GetParameter</code>
		/// </summary>
		private MethodInfo _getParameterMethodInfo;

		/// <summary>
		/// Information about the method <code>SetParameter</code>
		/// </summary>
		private MethodInfo _setParameterMethodInfo;

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

		/// <summary>
		/// JS-serializer
		/// </summary>
		private readonly JavaScriptSerializer _jsSerializer;

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;


		/// <summary>
		/// Constructs instance of adapter for Noesis Javascript .NET
		/// </summary>
		public V8JsEngine() : this(JsEngineSwitcher.Current.GetV8Configuration())
		{ }

		/// <summary>
		/// Constructs instance of adapter for Noesis Javascript .NET
		/// </summary>
		/// <param name="v8Config">Configuration settings of V8 JavaScript engine</param>
		public V8JsEngine(V8Configuration v8Config)
		{
			try
			{
				LoadAssembly(v8Config.NoesisJavascriptAssembliesDirectoryPath);

				_jsContextType = _jsEngineAssembly.GetType(JS_CONTEXT_TYPE_NAME);
				_jsExceptionType = _jsEngineAssembly.GetType(JS_EXCEPTION_TYPE_NAME);
				_jsEngineObject = _jsEngineAssembly.CreateInstance(JS_CONTEXT_TYPE_NAME);

				_runMethodInfo = _jsContextType.GetMethod("Run", new[] {typeof(string)});
				_getParameterMethodInfo = _jsContextType.GetMethod("GetParameter", new[] {typeof(string)});
				_setParameterMethodInfo = _jsContextType.GetMethod("SetParameter",
					new[] {typeof(string), typeof(object)});
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						"V8 JavaScript engine", e.Message), e);
			}

			_jsSerializer = new JavaScriptSerializer();
		}


		/// <summary>
		/// Loads assembly, which contains JS Engine
		/// </summary>
		/// <param name="noesisJavascriptAssembliesDirectoryPath">Path to directory that contains 
		/// the Noesis Javascript .NET assemblies</param>
		private void LoadAssembly(string noesisJavascriptAssembliesDirectoryPath)
		{
			lock (_assemblyLoadingSynchronizer)
			{
				if (!_assemblyLoaded)
				{
					string assemblyDirectoryPath;
					if (!string.IsNullOrWhiteSpace(noesisJavascriptAssembliesDirectoryPath))
					{
						assemblyDirectoryPath = noesisJavascriptAssembliesDirectoryPath;
						if (!Directory.Exists(assemblyDirectoryPath))
						{
							throw new ConfigurationErrorsException(
								string.Format(Strings.Engines_NoesisJavascriptAssembliesDirectoryNotFound,
									assemblyDirectoryPath));
						}
					}
					else
					{
						string binDirectoryPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
						assemblyDirectoryPath = Path.Combine(binDirectoryPath, @"../Noesis.Javascript/");
					}

					string platform = (IntPtr.Size == 4) ? "x86" : "x64";
					string assemblyPath = Path.Combine(assemblyDirectoryPath, 
						string.Format("{0}/Noesis.Javascript.dll", platform));

					Assembly assembly;
					if (File.Exists(assemblyPath))
					{
						assembly = Assembly.LoadFile(assemblyPath);
					}
					else
					{
						assembly = Assembly.Load(JS_ENGINE_ASSEMBLY_FULL_NAME);
					}

					_jsEngineAssembly = assembly;
					_assemblyLoaded = true;
				}
			}
		}

		/// <summary>
		/// Runs JS-code
		/// </summary>
		/// <param name="sourceCode">JS-code</param>
		private void Run(string sourceCode)
		{
			_runMethodInfo.Invoke(_jsEngineObject, new object[] { sourceCode });
		}

		/// <summary>
		/// Gets a value of parameter
		/// </summary>
		/// <param name="name">Name of parameter</param>
		/// <returns>Value of parameter</returns>
		private object GetParameter(string name)
		{
			var value = _getParameterMethodInfo.Invoke(_jsEngineObject, new object[] { name });

			return value;
		}

		/// <summary>
		/// Sets a value of parameter
		/// </summary>
		/// <param name="name">Name of parameter</param>
		/// <param name="value">Value of parameter</param>
		private void SetParameter(string name, object value)
		{
			_setParameterMethodInfo.Invoke(_jsEngineObject, new[] { name, value });
		}

		private bool IsJavascriptException(Exception exception)
		{
			bool result = (_jsExceptionType == exception.GetType());

			return result;
		}

		private static JsRuntimeException ConvertJavascriptExceptionToJsRuntimeException(
			Exception jsException)
		{
			dynamic dynamicJsException = jsException;

			var jsRuntimeException = new JsRuntimeException(jsException.Message, jsException)
			{
				EngineName = EngineName.V8JsEngine,
				LineNumber = (int)dynamicJsException.Line,
				ColumnNumber = (int)dynamicJsException.StartColumn + 1,
				Source = jsException.Source,
				HelpLink = jsException.HelpLink
			};

			return jsRuntimeException;
		}

		/// <summary>
		/// Converts a given value to the specified type
		/// </summary>
		/// <typeparam name="T">The type to which value will be converted</typeparam>
		/// <param name="value">The value to convert</param>
		/// <returns>The value that has been converted to the target type</returns>
		private T ConvertToType<T>(object value)
		{
			return (T)ConvertToType(value, typeof(T));
		}

		/// <summary>
		/// Converts a specified value to the specified type
		/// </summary>
		/// <param name="value">The value to convert</param>
		/// <param name="targetType">The type to convert the value to</param>
		/// <returns>The value that has been converted to the target type</returns>
		private object ConvertToType(object value, Type targetType)
		{
			object result = _jsSerializer.ConvertToType(value, targetType);

			return result;
		}

		protected override object InnerEvaluate(string expression)
		{
			const string resultingParameterName = "result";
			string processedExpression = expression.TrimEnd();
			if (processedExpression.EndsWith(";"))
			{
				processedExpression = processedExpression.TrimEnd(';');
			}

			object result;

			lock(_executionSynchronizer)
			{
				try
				{
					Run(string.Format("var {0} = {1};", resultingParameterName, processedExpression));
					result = GetParameter(resultingParameterName);
				}
				catch (TargetInvocationException e)
				{
					Exception innerException = e.InnerException;
					if (IsJavascriptException(innerException))
					{
						throw ConvertJavascriptExceptionToJsRuntimeException(innerException);
					}

					throw innerException;
				}
			}

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			return ConvertToType<T>(InnerEvaluate(expression));
		}

		protected override void InnerExecute(string code)
		{
			try
			{
				Run(code);
			}
			catch (TargetInvocationException e)
			{
				Exception innerException = e.InnerException;
				if (IsJavascriptException(innerException))
				{
					throw ConvertJavascriptExceptionToJsRuntimeException(innerException);
				}

				throw innerException;
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;
			const string resultingParameterName = "result";
			int argumentCount = args.Length;

			if (argumentCount > 0)
			{
				var parameters = new List<string>();

				lock (_executionSynchronizer)
				{
					try
					{
						for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
						{
							string parameterName = string.Format("param{0}", argumentIndex + 1);
							object argument = args[argumentIndex];

							SetParameter(parameterName, argument);
							parameters.Add(parameterName);
						}

						Run(string.Format("var {0} = {1}({2});", resultingParameterName, 
							functionName, string.Join(", ", parameters)));
						result = GetParameter(resultingParameterName);
					}
					catch (TargetInvocationException e)
					{
						Exception innerException = e.InnerException;
						if (IsJavascriptException(innerException))
						{
							throw ConvertJavascriptExceptionToJsRuntimeException(innerException);
						}

						throw innerException;
					}
				}
			}
			else
			{
				lock (_executionSynchronizer)
				{
					try
					{
						Run(string.Format("var {0} = {1}();", resultingParameterName, functionName));
						result = GetParameter(resultingParameterName);
					}
					catch (TargetInvocationException e)
					{
						Exception innerException = e.InnerException;
						if (IsJavascriptException(innerException))
						{
							throw ConvertJavascriptExceptionToJsRuntimeException(innerException);
						}

						throw innerException;
					}
				}
			}


			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			return ConvertToType<T>(InnerCallFunction(functionName, args));
		}

		protected override bool InnerHasVariable(string variableName)
		{
			string expression = string.Format("(typeof {0} !== 'undefined');", variableName);
			var result = InnerEvaluate<bool>(expression);

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			try
			{
				result = GetParameter(variableName);
			}
			catch (TargetInvocationException e)
			{
				Exception innerException = e.InnerException;
				if (IsJavascriptException(innerException))
				{
					throw ConvertJavascriptExceptionToJsRuntimeException(innerException);
				}

				throw innerException;
			}

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			return ConvertToType<T>(InnerGetVariableValue(variableName));
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			try
			{
				SetParameter(variableName, value);
			}
			catch (TargetInvocationException e)
			{
				Exception innerException = e.InnerException;
				if (IsJavascriptException(innerException))
				{
					throw ConvertJavascriptExceptionToJsRuntimeException(innerException);
				}

				throw innerException;
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			string code = string.Format(@"if (typeof {0} !== 'undefined') {{
	{0} = undefined;
}}", variableName);

			InnerExecute(code);
		}

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				_runMethodInfo = null;
				_getParameterMethodInfo = null;
				_setParameterMethodInfo = null;

				if (_jsEngineObject != null && _jsContextType != null)
				{
					var disposeMethodInfo = _jsContextType.GetMethod("Dispose");
					disposeMethodInfo.Invoke(_jsEngineObject, new object[0]);

					_jsEngineObject = null;
					_jsContextType = null;
					_jsExceptionType = null;
				}
			}
		}
	}
}