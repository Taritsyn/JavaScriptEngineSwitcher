namespace JavaScriptEngineSwitcher.Msie
{
	using System;

	using OriginalJsEngine = MsieJavaScriptEngine.MsieJsEngine;
	using OriginalJsEngineLoadException = MsieJavaScriptEngine.JsEngineLoadException;
	using OriginalJsEngineMode = MsieJavaScriptEngine.JsEngineMode;
	using OriginalJsRuntimeException = MsieJavaScriptEngine.JsRuntimeException;
	using OriginalJsEngineSettings = MsieJavaScriptEngine.JsEngineSettings;
	using OriginalTypeConverter = MsieJavaScriptEngine.Utilities.TypeConverter;
	using OriginalUndefined = MsieJavaScriptEngine.Undefined;

	using Core;
	using Core.Utilities;
	using CoreStrings = Core.Resources.Strings;

	using Configuration;

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
			get { return _engineVersion; }
		}


		/// <summary>
		/// Constructs a instance of adapter for MSIE JavaScript engine
		/// </summary>
		public MsieJsEngine()
			: this(JsEngineSwitcher.Current.GetMsieConfiguration())
		{ }

		/// <summary>
		/// Constructs a instance of adapter for MSIE JavaScript engine
		/// </summary>
		/// <param name="config">Configuration settings of MSIE JavaScript engine</param>
		public MsieJsEngine(MsieConfiguration config)
		{
			MsieConfiguration msieConfig = config ?? new MsieConfiguration();

			try
			{
				_jsEngine = new OriginalJsEngine(new OriginalJsEngineSettings
				{
					EngineMode = Utils.GetEnumFromOtherEnum<JsEngineMode, OriginalJsEngineMode>(
						msieConfig.EngineMode),
					UseEcmaScript5Polyfill = msieConfig.UseEcmaScript5Polyfill,
					UseJson2Library = msieConfig.UseJson2Library
				});
				_engineVersion = _jsEngine.Mode;
			}
			catch (OriginalJsEngineLoadException e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						ENGINE_NAME, e.Message), ENGINE_NAME, e.EngineMode, e);
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						ENGINE_NAME, e.Message), ENGINE_NAME, _engineVersion, e);
			}
		}


		/// <summary>
		/// Executes a mapping from the host type to a MSIE type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToMsieType(object value)
		{
			if (value is Undefined)
			{
				return OriginalUndefined.Value;
			}

			return value;
		}

		/// <summary>
		/// Executes a mapping from the MSIE type to a host type
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

		private JsRuntimeException ConvertMsieJsRuntimeExceptionToJsRuntimeException(
			OriginalJsRuntimeException msieJsRuntimeException)
		{
			var jsRuntimeException = new JsRuntimeException(msieJsRuntimeException.Message,
				ENGINE_NAME, _engineVersion, msieJsRuntimeException)
			{
				ErrorCode = msieJsRuntimeException.ErrorCode,
				Category = msieJsRuntimeException.Category,
				LineNumber = msieJsRuntimeException.LineNumber,
				ColumnNumber = msieJsRuntimeException.ColumnNumber,
				SourceFragment = msieJsRuntimeException.SourceFragment,
				Source = msieJsRuntimeException.Source,
				HelpLink = msieJsRuntimeException.HelpLink
			};

			return jsRuntimeException;
		}

		#region JsEngineBase implementation

		protected override object InnerEvaluate(string expression)
		{
			object result;

			try
			{
				result = _jsEngine.Evaluate(expression);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertMsieJsRuntimeExceptionToJsRuntimeException(e);
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return OriginalTypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			try
			{
				_jsEngine.Execute(code);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertMsieJsRuntimeExceptionToJsRuntimeException(e);
			}
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;
			int argumentCount = args.Length;
			var processedArgs = new object[argumentCount];

			if (argumentCount > 0)
			{
				for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					processedArgs[argumentIndex] = MapToMsieType(args[argumentIndex]);
				}
			}

			try
			{
				result = _jsEngine.CallFunction(functionName, processedArgs);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertMsieJsRuntimeExceptionToJsRuntimeException(e);
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return OriginalTypeConverter.ConvertToType<T>(result);
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			try
			{
				result = _jsEngine.HasVariable(variableName);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertMsieJsRuntimeExceptionToJsRuntimeException(e);
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result;

			try
			{
				result = _jsEngine.GetVariableValue(variableName);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertMsieJsRuntimeExceptionToJsRuntimeException(e);
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return OriginalTypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToMsieType(value);

			try
			{
				_jsEngine.SetVariableValue(variableName, processedValue);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertMsieJsRuntimeExceptionToJsRuntimeException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				_jsEngine.RemoveVariable(variableName);
			}
			catch (OriginalJsRuntimeException e)
			{
				throw ConvertMsieJsRuntimeExceptionToJsRuntimeException(e);
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