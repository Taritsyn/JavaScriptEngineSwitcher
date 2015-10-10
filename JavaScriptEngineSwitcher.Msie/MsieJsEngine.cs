using System.Configuration;

namespace JavaScriptEngineSwitcher.Msie
{
	using System;

	using Core;
	using Core.Utilities;
	using CoreStrings = Core.Resources.Strings;
	using Creek.Scripting;
	using Microsoft.ClearScript.Windows;
	using Microsoft.ClearScript;
	using System.Collections.Generic;

	/// <summary>
	/// Adapter for MSIE JavaScript engine
	/// </summary>
	public sealed class MsieJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JavaScript engine
		/// </summary>
		private const string ENGINE_NAME = "MSIE JavaScript engine";

		/// <summary>
		/// Version of original JavaScript engine
		/// </summary>
		private readonly string _engineVersion;

		/// <summary>
		/// MSIE JS engine
		/// </summary>
		private JScriptEngine _jsEngine;

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
			get { return _engineVersion; }
		}


		/// <summary>
		/// Constructs a instance of adapter for MSIE JavaScript engine
		/// </summary>
		public MsieJsEngine()
		{
			_jsEngine = new JScriptEngine(WindowsScriptEngineFlags.EnableJITDebugging);
			_jsEngine.Add("host", new HostFunctions());
			_jsEngine.Add("xhost", new ExtendedHostFunctions());
		}

		#region JsEngineBase implementation

		protected override object InnerEvaluate(string expression)
		{
			object result;

			try
			{
				result = _jsEngine.Evaluate(expression);
			}
			catch (Exception e)
			{
				throw e;
			}

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return (T)Convert.ChangeType(result, typeof(T));
		}

		protected override void InnerExecute(string code)
		{
			try
			{
				_jsEngine.Execute(code);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;

			try
			{
				result = _jsEngine.InvokeFunction(functionName, args);
			}
			catch (Exception e)
			{
				throw e;
			}

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return (T)Convert.ChangeType(result, typeof(T));
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			try
			{
				result = ((IDictionary<string, object>)_jsEngine.Script).ContainsKey(variableName);
			}
			catch (Exception e)
			{
				throw e;
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			try
			{
				result = Evaluate(variableName);
			}
			catch (Exception e)
			{
				throw e;
			}

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return (T)Convert.ChangeType(result, typeof(T));
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = value;

			try
			{
				_jsEngine.Add(variableName, processedValue);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				_jsEngine.Execute("delete " + variableName);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		#endregion

		#region IDisposable implementation

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
			}
		}

		#endregion
	}
}