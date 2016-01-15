namespace JavaScriptEngineSwitcher.ChakraCore
{
	using System;
	using System.Globalization;
	using System.Linq;

	using OriginalJsException = JsRt.JsException;

	using Core;
	using Core.Utilities;
	using CoreStrings = Core.Resources.Strings;

	using JsRt;

	/// <summary>
	/// Adapter for ChakraCore JavaScript engine
	/// </summary>
	public sealed class ChakraCoreJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JavaScript engine
		/// </summary>
		private const string ENGINE_NAME = "ChakraCore JavaScript engine";

		/// <summary>
		/// Version of original JavaScript engine
		/// </summary>
		private const string ENGINE_VERSION = "1.1";

		/// <summary>
		/// Instance of JavaScript runtime
		/// </summary>
		private JsRuntime _jsRuntime;

		/// <summary>
		/// Instance of JavaScript context
		/// </summary>
		private readonly JsContext _jsContext;

		/// <summary>
		/// Run synchronizer
		/// </summary>
		private readonly object _runSynchronizer = new object();

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;

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
		/// Static constructor
		/// </summary>
		static ChakraCoreJsEngine()
		{
			AssemblyResolver.Initialize();
		}

		/// <summary>
		/// Constructs a instance of adapter for ChakraCore JavaScript engine
		/// </summary>
		public ChakraCoreJsEngine()
		{
			try
			{
				_jsRuntime = JsRuntime.Create(JsRuntimeAttributes.AllowScriptInterrupt, JsRuntimeVersion.VersionEdge, null);
				_jsContext = _jsRuntime.CreateContext();
			}
			catch (Exception e)
			{
				throw new JsEngineLoadException(
					string.Format(CoreStrings.Runtime_JsEngineNotLoaded,
						ENGINE_NAME, e.Message), ENGINE_NAME, ENGINE_VERSION, e);
			}
		}

		/// <summary>
		/// Destructs instance of adapter for ChakraCore JavaScript engine
		/// </summary>
		~ChakraCoreJsEngine()
		{
			Dispose(false);
		}


		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static JsValue MapToScriptType(object value)
		{
			if (value == null)
			{
				return JsValue.Null;
			}

			if (value is Undefined)
			{
				return JsValue.Undefined;
			}

			TypeCode typeCode = Type.GetTypeCode(value.GetType());

			switch (typeCode)
			{
				case TypeCode.Boolean:
					return JsValue.FromBoolean((bool)value);
				case TypeCode.Int32:
					return JsValue.FromInt32((int)value);
				case TypeCode.Double:
					return JsValue.FromDouble((double)value);
				case TypeCode.String:
					return JsValue.FromString((string)value);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Makes a mapping of array items from the host type to a script type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		private static JsValue[] MapToScriptType(object[] args)
		{
			return args.Select(MapToScriptType).ToArray();
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToHostType(JsValue value)
		{
			JsValueType valueType = value.ValueType;
			JsValue processedValue;
			object result;

			switch (valueType)
			{
				case JsValueType.Null:
					result = null;
					break;
				case JsValueType.Undefined:
					result = Undefined.Value;
					break;
				case JsValueType.Boolean:
					processedValue = value.ConvertToBoolean();
					result = processedValue.ToBoolean();
					break;
				case JsValueType.Number:
					processedValue = value.ConvertToNumber();
					result = processedValue.ToDouble();
					break;
				case JsValueType.String:
					processedValue = value.ConvertToString();
					result = processedValue.ToString();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return result;
		}

		/// <summary>
		/// Makes a mapping of array items from the script type to a host type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		private static object[] MapToHostType(JsValue[] args)
		{
			return args.Select(MapToHostType).ToArray();
		}

		private static JsRuntimeException ConvertJsExceptionToJsRuntimeException(
			OriginalJsException jsException)
		{
			string message = jsException.Message;
			string category = string.Empty;
			int lineNumber = 0;
			int columnNumber = 0;
			string sourceFragment = string.Empty;

			var jsScriptException = jsException as JsScriptException;
			if (jsScriptException != null)
			{
				category = "Script error";
				JsValue errorValue = jsScriptException.Error;

				JsPropertyId messagePropertyId = JsPropertyId.FromString("message");
				JsValue messagePropertyValue = errorValue.GetProperty(messagePropertyId);
				string scriptMessage = messagePropertyValue.ConvertToString().ToString();
				if (!string.IsNullOrWhiteSpace(scriptMessage))
				{
					message = string.Format("{0}: {1}", message.TrimEnd('.'), scriptMessage);
				}

				JsPropertyId linePropertyId = JsPropertyId.FromString("line");
				if (errorValue.HasProperty(linePropertyId))
				{
					JsValue linePropertyValue = errorValue.GetProperty(linePropertyId);
					lineNumber = (int)linePropertyValue.ConvertToNumber().ToDouble() + 1;
				}

				JsPropertyId columnPropertyId = JsPropertyId.FromString("column");
				if (errorValue.HasProperty(columnPropertyId))
				{
					JsValue columnPropertyValue = errorValue.GetProperty(columnPropertyId);
					columnNumber = (int)columnPropertyValue.ConvertToNumber().ToDouble() + 1;
				}

				JsPropertyId sourcePropertyId = JsPropertyId.FromString("source");
				if (errorValue.HasProperty(sourcePropertyId))
				{
					JsValue sourcePropertyValue = errorValue.GetProperty(sourcePropertyId);
					sourceFragment = sourcePropertyValue.ConvertToString().ToString();
				}
			}
			else if (jsException is JsUsageException)
			{
				category = "Usage error";
			}
			else if (jsException is JsEngineException)
			{
				category = "Engine error";
			}
			else if (jsException is JsFatalException)
			{
				category = "Fatal error";
			}

			var jsEngineException = new JsRuntimeException(message, ENGINE_NAME, ENGINE_VERSION)
			{
				ErrorCode = ((uint)jsException.ErrorCode).ToString(CultureInfo.InvariantCulture),
				Category = category,
				LineNumber = lineNumber,
				ColumnNumber = columnNumber,
				SourceFragment = sourceFragment,
				HelpLink = jsException.HelpLink
			};

			return jsEngineException;
		}

		private void InvokeScript(Action action)
		{
			lock (_runSynchronizer)
			using (new JsScope(_jsContext))
			{
				try
				{
					action();
				}
				catch (OriginalJsException e)
				{
					throw ConvertJsExceptionToJsRuntimeException(e);
				}
			}
		}

		private T InvokeScript<T>(Func<T> func)
		{
			lock (_runSynchronizer)
			using (new JsScope(_jsContext))
			{
				try
				{
					return func();
				}
				catch (OriginalJsException e)
				{
					throw ConvertJsExceptionToJsRuntimeException(e);
				}
			}
		}

		/// <summary>
		/// Destroys object
		/// </summary>
		/// <param name="disposing">Flag, allowing destruction of
		/// managed objects contained in fields of class</param>
		private void Dispose(bool disposing)
		{
			lock (_runSynchronizer)
			{
				if (!_disposed)
				{
					_disposed = true;

					_jsRuntime.Dispose();
				}
			}
		}

		#region JsEngineBase implementation

		protected override object InnerEvaluate(string expression)
		{
			object result = InvokeScript(() =>
			{
				JsValue resultValue = JsContext.RunScript(expression);

				return MapToHostType(resultValue);
			});

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			object result = InnerEvaluate(expression);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			InvokeScript(() => JsContext.RunScript(code));
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result = InvokeScript(() =>
			{
				JsValue globalObj = JsValue.GlobalObject;
				JsPropertyId functionId = JsPropertyId.FromString(functionName);

				bool functionExist = globalObj.HasProperty(functionId);
				if (!functionExist)
				{
					throw new JsRuntimeException(
						string.Format(CoreStrings.Runtime_FunctionNotExist, functionName));
				}

				var processedArgs = MapToScriptType(args);
				var allProcessedArgs = new[] { globalObj }.Concat(processedArgs).ToArray();

				JsValue functionValue = globalObj.GetProperty(functionId);
				JsValue resultValue = functionValue.CallFunction(allProcessedArgs);

				return MapToHostType(resultValue);
			});

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result = InvokeScript(() =>
			{
				JsValue globalObj = JsValue.GlobalObject;
				JsPropertyId variableId = JsPropertyId.FromString(variableName);
				bool variableExist = globalObj.HasProperty(variableId);

				if (variableExist)
				{
					JsValue variableValue = globalObj.GetProperty(variableId);
					variableExist = (variableValue.ValueType != JsValueType.Undefined);
				}

				return variableExist;
			});

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object result = InvokeScript(() =>
			{
				JsPropertyId variableId = JsPropertyId.FromString(variableName);
				JsValue variableValue = JsValue.GlobalObject.GetProperty(variableId);

				return MapToHostType(variableValue);
			});

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			InvokeScript(() =>
			{
				JsPropertyId variableId = JsPropertyId.FromString(variableName);
				JsValue inputValue = MapToScriptType(value);

				JsValue.GlobalObject.SetProperty(variableId, inputValue, true);
			});
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			InvokeScript(() =>
			{
				JsValue globalObj = JsValue.GlobalObject;
				JsPropertyId variableId = JsPropertyId.FromString(variableName);

				if (globalObj.HasProperty(variableId))
				{
					globalObj.SetProperty(variableId, JsValue.Undefined, true);
				}
			});
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Destroys object
		/// </summary>
		public override void Dispose()
		{
			Dispose(true /* disposing */);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}