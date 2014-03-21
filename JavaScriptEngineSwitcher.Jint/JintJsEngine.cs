using OriginalJsEngine = Jint.Engine;
using OriginalNull = Jint.Native.Null;
using OriginalUndefined = Jint.Native.Undefined;
using OriginalJsException = Jint.Runtime.JavaScriptException;
using OriginalJsValue = Jint.Native.JsValue;

namespace JavaScriptEngineSwitcher.Jint
{
	using System;

	using Core;
	using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// Adapter for Jint JavaScript engine
	/// </summary>
	public sealed class JintJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JavaScript engine
		/// </summary>
		private const string ENGINE_NAME = "Jint JavaScript engine";

		/// <summary>
		/// Version of original JavaScript engine
		/// </summary>
		private const string ENGINE_VERSION = "20 Feb 2014";

		/// <summary>
		/// Jint JS engine
		/// </summary>
		private OriginalJsEngine _jsEngine;

		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		public override string Name
		{
			get { return ENGINE_NAME; }
		}

		/// <summary>
		/// Gets a version of original JavaScript engine
		/// </summary>
		public override string Version
		{
			get { return ENGINE_VERSION; }
		}

		/// <summary>
		/// Constructs instance of adapter for Jurassic
		/// </summary>
		public JintJsEngine()
		{
			try
			{
				_jsEngine = new OriginalJsEngine();
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						ENGINE_NAME, e.Message), ENGINE_NAME, ENGINE_VERSION, e);
			}
		}

		/// <summary>
		/// Executes a mapping from the host type to a Jurassic type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToJintType(object value)
		{
			if (value == null)
			{
				return OriginalNull.Instance;
			}

			if (value is Undefined)
			{
				return OriginalUndefined.Instance;
			}

			return value;
		}

		/// <summary>
		/// Executes a mapping from the Jint type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToHostType(OriginalJsValue value)
		{
			if (value.IsNull())
			{
				return null;
			}

			if (value.IsUndefined())
			{
				return Undefined.Value;
			}

			return value.ToObject();
		}

		private JsRuntimeException ConvertJavascriptExceptionToJsRuntimeException(
			OriginalJsException jsException)
		{
			var jsRuntimeException = new JsRuntimeException(jsException.Message, ENGINE_NAME, ENGINE_VERSION,
				jsException)
			{
				Category = jsException.Message,
				Source = jsException.Source,
				HelpLink = jsException.HelpLink,
			};

			return jsRuntimeException;
		}

		#region JsEngineBase implementation

		protected override object InnerEvaluate(string expression)
		{
			OriginalJsValue result;

			try
			{
				result = _jsEngine.Execute(expression).GetCompletionValue();
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			return MapToHostType(result);
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return (T) Convert.ChangeType(result, typeof(T));
		}

		protected override void InnerExecute(string code)
		{
			try
			{
				_jsEngine.Execute(code);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			int argumentCount = args.Length;
			var processedArgs = new object[argumentCount];

			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					processedArgs[argumentIndex] = MapToJintType(args[argumentIndex]);
				}
			}

			OriginalJsValue result;

			try
			{
				result = _jsEngine.Invoke(functionName, processedArgs);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			return MapToHostType(result);
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			var result = InnerCallFunction(functionName, args);

			return (T)Convert.ChangeType(result, typeof(T));
		}

		protected override bool InnerHasVariable(string variableName)
		{
			// Slightly messy but there doesn't seem to be a better way :/
			try
			{
				var value = _jsEngine.GetValue(variableName);
				return value != OriginalUndefined.Instance;
			}
			catch (OriginalJsException)
			{
				return false;
			}
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			OriginalJsValue result;

			try
			{
				result = _jsEngine.GetValue(variableName);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}

			return MapToHostType(result);
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return (T)Convert.ChangeType(result, typeof(T));
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToJintType(value);

			try
			{
				_jsEngine.SetValue(variableName, processedValue);
			}
			catch (OriginalJsException e)
			{
				throw ConvertJavascriptExceptionToJsRuntimeException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InnerSetVariableValue(variableName, Undefined.Value);
		}

		#endregion

		#region IDisposable implementation

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_jsEngine = null;
			}
		}

		#endregion
	}
}