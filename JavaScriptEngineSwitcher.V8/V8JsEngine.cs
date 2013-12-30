namespace JavaScriptEngineSwitcher.V8
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Script.Serialization;

	using Microsoft.ClearScript.V8;
	using OriginalUndefined = Microsoft.ClearScript.Undefined;
	using OriginalJsException = Microsoft.ClearScript.ScriptEngineException;

	using Core;
	using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// Adapter for Microsoft ClearScript.V8
	/// </summary>
	public sealed class V8JsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of directory, that contains the Microsoft ClearScript.V8 assemblies
		/// </summary>
		private const string ASSEMBLY_DIRECTORY_NAME = "ClearScript.V8";

		/// <summary>
		/// Synchronizer of code execution
		/// </summary>
		private readonly object _executionSynchronizer = new object();

		/// <summary>
		/// JS-engine
		/// </summary>
		private V8ScriptEngine _jsEngine;

		/// <summary>
		/// JS-serializer
		/// </summary>
		private JavaScriptSerializer _jsSerializer;

		/// <summary>
		/// ClearScript <code>undefined</code> value
		/// </summary>
		private static readonly OriginalUndefined _originalUndefinedValue;

		/// <summary>
		/// Regular expression for working with the string representation of error
		/// </summary>
		private static readonly Regex _errorStringRegex =
			new Regex(@"at Script Document:(?<lineNumber>\d+):(?<columnNumber>\d+)");

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;

		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		public override string Name
		{
			get { return "V8 JavaScript engine"; }
		}

		/// <summary>
		/// Gets a version of original JavaScript engine
		/// </summary>
		public override string Version
		{
			get { return "3.23.13"; }
		}


		/// <summary>
		/// Static constructor
		/// </summary>
		static V8JsEngine()
		{
			// Sets a path under the base directory where the assembly resolver 
			// should probe for private assemblies
			var currentDomain = AppDomain.CurrentDomain;

			string binDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
			if (string.IsNullOrEmpty(binDirectoryPath))
			{
				// `PrivateBinPath` property is empty in test scenarios, so
				// need to use the `BaseDirectory` property
				binDirectoryPath = currentDomain.BaseDirectory;
			}

			string assemblyDirectoryPath = Path.Combine(binDirectoryPath, ASSEMBLY_DIRECTORY_NAME);
			if (!Directory.Exists(assemblyDirectoryPath) && HttpContext.Current != null)
			{
				// Fix for WebMatrix
				string applicationRootPath = HttpContext.Current.Server.MapPath("~");
				assemblyDirectoryPath = Path.Combine(applicationRootPath, ASSEMBLY_DIRECTORY_NAME);
			}

			currentDomain.AppendPrivatePath(assemblyDirectoryPath);

			// Gets a ClearScript <code>undefined</code> value
			FieldInfo undefinedValueFieldInfo = typeof(OriginalUndefined).GetField("Value",
				BindingFlags.NonPublic | BindingFlags.Static);
			if (undefinedValueFieldInfo != null)
			{
				_originalUndefinedValue = (OriginalUndefined)undefinedValueFieldInfo.GetValue(null);
			}
		}

		/// <summary>
		/// Constructs instance of adapter for Microsoft ClearScript.V8
		/// </summary>
		public V8JsEngine()
		{
			try
			{
				_jsEngine = new V8ScriptEngine();
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						Name, e.Message), e);
			}
			_jsSerializer = new JavaScriptSerializer();
		}


		/// <summary>
		/// Executes a mapping from the host type to a ClearScript type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToClearScriptType(object value)
		{
			if (value is Undefined)
			{
				return _originalUndefinedValue;
			}

			return value;
		}

		/// <summary>
		/// Executes a mapping from the ClearScript type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToHostType(object value)
		{
			if (value is OriginalUndefined)
			{
				return Undefined.Value;
			}

			return value;
		}

		private JsRuntimeException ConvertScriptEngineExceptionToJsRuntimeException(
			OriginalJsException scriptEngineException)
		{
			string errorDetails = scriptEngineException.ErrorDetails;
			int lineNumber = 0;
			int columnNumber = 0;

			Match errorStringMatch = _errorStringRegex.Match(errorDetails);
			if (errorStringMatch.Success)
			{
				GroupCollection errorStringGroups = errorStringMatch.Groups;

				lineNumber = int.Parse(errorStringGroups["lineNumber"].Value);
				columnNumber = int.Parse(errorStringGroups["columnNumber"].Value);
			}

			var jsRuntimeException = new JsRuntimeException(errorDetails)
			{
				EngineName = Name,
				EngineVersion = Version,
				LineNumber = lineNumber,
				ColumnNumber = columnNumber,
				Source = scriptEngineException.Source,
				HelpLink = scriptEngineException.HelpLink
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
			object result;

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsEngine.Evaluate(expression);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Execute(code);
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			const string resultingParameterName = "result";
			object result;
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
							object argument = MapToClearScriptType(args[argumentIndex]);

							_jsEngine.Script[parameterName] = argument;
							parameters.Add(parameterName);
						}

						_jsEngine.Execute(string.Format("var {0} = {1}({2});", resultingParameterName, 
							functionName, string.Join(", ", parameters)));
						result = _jsEngine.Script[resultingParameterName];
					}
					catch (OriginalJsException e)
					{
						throw ConvertScriptEngineExceptionToJsRuntimeException(e);
					}
				}
			}
			else
			{
				lock (_executionSynchronizer)
				{
					try
					{
						_jsEngine.Execute(string.Format("var {0} = {1}();", resultingParameterName, functionName));
						result = _jsEngine.Script[resultingParameterName];
					}
					catch (OriginalJsException e)
					{
						throw ConvertScriptEngineExceptionToJsRuntimeException(e);
					}
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return ConvertToType<T>(result);
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

			lock (_executionSynchronizer)
			{
				try
				{
					result = _jsEngine.Script[variableName];
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToClearScriptType(value);

			lock (_executionSynchronizer)
			{
				try
				{
					_jsEngine.Script[variableName] = processedValue;
				}
				catch (OriginalJsException e)
				{
					throw ConvertScriptEngineExceptionToJsRuntimeException(e);
				}
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InnerSetVariableValue(variableName, Undefined.Value);
		}

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				if (_jsEngine != null)
				{
					_jsEngine.Dispose();
					_jsEngine = null;
				}

				_jsSerializer = null;
			}
		}
	}
}