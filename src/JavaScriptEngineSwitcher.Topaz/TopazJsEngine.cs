using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using OriginalAssignmentWithoutDefinitionBehavior = Tenray.Topaz.Options.AssignmentWithoutDefinitionBehavior;
using OriginalEngine = Tenray.Topaz.TopazEngine;
using OriginalEngineOptions = Tenray.Topaz.Options.TopazEngineOptions;
using OriginalEngineSetup = Tenray.Topaz.TopazEngineSetup;
using OriginalException = Tenray.Topaz.TopazException;
using OriginalParserException = Esprima.ParserException;
using OriginalUndefined = Tenray.Topaz.Undefined;
using OriginalVarScopeBehavior = Tenray.Topaz.Options.VarScopeBehavior;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Constants;
using JavaScriptEngineSwitcher.Core.Helpers;
using JavaScriptEngineSwitcher.Core.Utilities;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;
using WrapperCompilationException = JavaScriptEngineSwitcher.Core.JsCompilationException;
using WrapperException = JavaScriptEngineSwitcher.Core.JsException;
using WrapperInterruptedException = JavaScriptEngineSwitcher.Core.JsInterruptedException;
using WrapperRuntimeException = JavaScriptEngineSwitcher.Core.JsRuntimeException;
using WrapperScriptException = JavaScriptEngineSwitcher.Core.JsScriptException;

namespace JavaScriptEngineSwitcher.Topaz
{
	/// <summary>
	/// Adapter for the Topaz JS engine
	/// </summary>
	public sealed class TopazJsEngine : JsEngineBase
	{
		/// <summary>
		/// Name of JS engine
		/// </summary>
		public const string EngineName = "TopazJsEngine";

		/// <summary>
		/// Version of original JS engine
		/// </summary>
		private const string EngineVersion = "1.3.0";

		/// <summary>
		/// Regular expression for working with the error message with type
		/// </summary>
		private static readonly Regex _errorMessageWithTypeRegex =
			new Regex(@"^(?<type>" + CommonRegExps.JsFullNamePattern + @"):\s+(?<description>[\s\S]+?)$");

		/// <summary>
		/// Topaz JS engine
		/// </summary>
		private OriginalEngine _jsEngine;

		/// <summary>
		/// Token source for canceling of script execution
		/// </summary>
		private CancellationTokenSource _cancellationTokenSource;


		/// <summary>
		/// Constructs an instance of adapter for the Topaz JS engine
		/// </summary>
		public TopazJsEngine()
			: this(new TopazSettings())
		{ }

		/// <summary>
		/// Constructs an instance of adapter for the Topaz JS engine
		/// </summary>
		/// <param name="settings">Settings of the Topaz JS engine</param>
		public TopazJsEngine(TopazSettings settings)
		{
			_cancellationTokenSource = new CancellationTokenSource();

			TopazSettings topazSettings = settings ?? new TopazSettings();

			try
			{
				_jsEngine = new OriginalEngine(new OriginalEngineSetup
				{
					Options = new OriginalEngineOptions
					{
						AllowNullReferenceMemberAccess = false,
						AllowUndefinedReferenceAccess = true,
						AllowUndefinedReferenceMemberAccess = false,
						AssignmentWithoutDefinitionBehavior = OriginalAssignmentWithoutDefinitionBehavior.DefineAsVarInGlobalScope,
						DefineSpecialArgumentsObjectOnEachFunctionCall = true,
						NoUndefined = false,
						VarScopeBehavior = OriginalVarScopeBehavior.FunctionScope,
						UseThreadSafeJsObjects = true
					}
				});
				topazSettings.BuiltinObjectsInitializer?.Invoke(_jsEngine);
			}
			catch (Exception e)
			{
				throw JsErrorHelpers.WrapEngineLoadException(e, EngineName, EngineVersion, true);
			}
		}


		#region Mapping

		/// <summary>
		/// Makes a mapping of value from the host type to a script type
		/// </summary>
		/// <param name="value">The source value</param>
		/// <returns>The mapped value</returns>
		private static object MapToScriptType(object value)
		{
			if (value is Undefined)
			{
				return OriginalUndefined.Value;
			}

			return value;
		}

		/// <summary>
		/// Makes a mapping of array items from the host type to a script type
		/// </summary>
		/// <param name="args">The source array</param>
		/// <returns>The mapped array</returns>
		private static object[] MapToScriptType(object[] args)
		{
			return args.Select(MapToScriptType).ToArray();
		}

		/// <summary>
		/// Makes a mapping of value from the script type to a host type
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

		private static WrapperCompilationException WrapParserException(OriginalParserException originalParserException)
		{
			string description = originalParserException.Description;
			string type = JsErrorType.Syntax;
			int lineNumber = originalParserException.LineNumber;
			int columnNumber = originalParserException.Column;
			string message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty, lineNumber,
				columnNumber);

			var wrapperCompilationException = new WrapperCompilationException(message, EngineName, EngineVersion,
				originalParserException)
			{
				Description = description,
				Type = type,
				LineNumber = lineNumber,
				ColumnNumber = columnNumber
			};

			return wrapperCompilationException;
		}

		private static WrapperException WrapTopazException(OriginalException originalException)
		{
			WrapperScriptException wrapperScriptException;
			string message = originalException.Message;
			string description = message;
			string type = string.Empty;

			Match messageWithTypeMatch = _errorMessageWithTypeRegex.Match(message);
			if (messageWithTypeMatch.Success)
			{
				GroupCollection messageWithTypeGroups = messageWithTypeMatch.Groups;
				type = messageWithTypeGroups["type"].Value;
				description = messageWithTypeGroups["description"].Value;
			}
			else
			{
				type = JsErrorType.Common;
			}

			message = JsErrorHelpers.GenerateScriptErrorMessage(type, description, string.Empty);

			if (type == JsErrorType.Syntax)
			{
				wrapperScriptException = new WrapperCompilationException(message, EngineName, EngineVersion,
					originalException);
			}
			else
			{
				wrapperScriptException = new WrapperRuntimeException(message, EngineName, EngineVersion,
					originalException);
			}

			wrapperScriptException.Description = description;
			wrapperScriptException.Type = type;

			return wrapperScriptException;
		}

		private static WrapperInterruptedException WrapOperationCanceledException(
			OperationCanceledException originalOperationCanceledException)
		{
			string message = CoreStrings.Runtime_ScriptInterrupted;
			string description = message;

			var wrapperInterruptedException = new WrapperInterruptedException(message, EngineName, EngineVersion,
				originalOperationCanceledException)
			{
				Description = description
			};

			return wrapperInterruptedException;
		}

		private static WrapperException WrapTargetInvocationException(TargetInvocationException originalTargetInvocationException)
		{
			Exception innerException = originalTargetInvocationException.InnerException;
			if (innerException == null)
			{
				return null;
			}

			var wrapperException = innerException as WrapperException;
			if (wrapperException == null)
			{
				wrapperException = new WrapperException(innerException.Message, EngineName, EngineVersion,
					innerException)
				{
					Description = innerException.Message
				};
			}

			return wrapperException;
		}

		#endregion

		#region JsEngineBase overrides

		protected override IPrecompiledScript InnerPrecompile(string code)
		{
			throw new NotSupportedException();
		}

		protected override IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			throw new NotSupportedException();
		}

		protected override object InnerEvaluate(string expression)
		{
			return InnerEvaluate(expression, null);
		}

		protected override object InnerEvaluate(string expression, string documentName)
		{
			object result;

			try
			{
				result = _jsEngine.ExecuteExpression(expression, _cancellationTokenSource.Token);
			}
			catch (OriginalParserException e)
			{
				throw WrapParserException(e);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}
			catch (OperationCanceledException e)
			{
				throw WrapOperationCanceledException(e);
			}
			catch (TargetInvocationException e)
			{
				var wrapperException = WrapTargetInvocationException(e);
				if (wrapperException != null)
				{
					throw wrapperException;
				}

				throw;
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerEvaluate<T>(string expression)
		{
			return InnerEvaluate<T>(expression, null);
		}

		protected override T InnerEvaluate<T>(string expression, string documentName)
		{
			object result = InnerEvaluate(expression, documentName);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerExecute(string code)
		{
			InnerExecute(code, null);
		}

		protected override void InnerExecute(string code, string documentName)
		{
			try
			{
				_jsEngine.ExecuteScript(code, _cancellationTokenSource.Token);
			}
			catch (OriginalParserException e)
			{
				throw WrapParserException(e);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}
			catch (OperationCanceledException e)
			{
				throw WrapOperationCanceledException(e);
			}
			catch (TargetInvocationException e)
			{
				var wrapperException = WrapTargetInvocationException(e);
				if (wrapperException != null)
				{
					throw wrapperException;
				}

				throw;
			}
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			throw new NotSupportedException();
		}

		protected override object InnerCallFunction(string functionName, params object[] args)
		{
			object result;
			object[] processedArgs = MapToScriptType(args);

			try
			{
				result = _jsEngine.InvokeFunction(functionName, _cancellationTokenSource.Token, processedArgs);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}
			catch (OperationCanceledException e)
			{
				throw WrapOperationCanceledException(e);
			}
			catch (TargetInvocationException e)
			{
				var wrapperException = WrapTargetInvocationException(e);
				if (wrapperException != null)
				{
					throw wrapperException;
				}

				throw;
			}

			result = MapToHostType(result);

			return result;
		}

		protected override T InnerCallFunction<T>(string functionName, params object[] args)
		{
			object result = InnerCallFunction(functionName, args);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override bool InnerHasVariable(string variableName)
		{
			bool result;

			try
			{
				object variableValue = _jsEngine.GetValue(variableName);
				result = variableValue != OriginalUndefined.Value;
			}
			catch (OriginalException)
			{
				result = false;
			}

			return result;
		}

		protected override object InnerGetVariableValue(string variableName)
		{
			object variableValue;

			try
			{
				variableValue = _jsEngine.GetValue(variableName);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}

			object result = MapToHostType(variableValue);

			return result;
		}

		protected override T InnerGetVariableValue<T>(string variableName)
		{
			object result = InnerGetVariableValue(variableName);

			return TypeConverter.ConvertToType<T>(result);
		}

		protected override void InnerSetVariableValue(string variableName, object value)
		{
			object processedValue = MapToScriptType(value);

			try
			{
				_jsEngine.SetValue(variableName, processedValue);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}
		}

		protected override void InnerRemoveVariable(string variableName)
		{
			try
			{
				_jsEngine.SetValue(variableName, OriginalUndefined.Value);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}
		}

		protected override void InnerEmbedHostObject(string itemName, object value)
		{
			try
			{
				_jsEngine.SetValue(itemName, value);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}
		}

		protected override void InnerEmbedHostType(string itemName, Type type)
		{
			try
			{
				_jsEngine.AddType(type, itemName);
			}
			catch (OriginalException e)
			{
				throw WrapTopazException(e);
			}
		}

		protected override void InnerInterrupt()
		{
			_cancellationTokenSource.Cancel();
		}

		protected override void InnerCollectGarbage()
		{
			throw new NotSupportedException();
		}

		#region IJsEngine implementation

		public override string Name
		{
			get { return EngineName; }
		}

		public override string Version
		{
			get { return EngineVersion; }
		}

		public override bool SupportsScriptPrecompilation
		{
			get { return false; }
		}

		public override bool SupportsScriptInterruption
		{
			get { return true; }
		}

		public override bool SupportsGarbageCollection
		{
			get { return false; }
		}

		#endregion

		#region IDisposable implementation

		public override void Dispose()
		{
			if (_disposedFlag.Set())
			{
				_jsEngine = null;

				if (_cancellationTokenSource != null)
				{
					_cancellationTokenSource.Dispose();
					_cancellationTokenSource = null;
				}
			}
		}

		#endregion

		#endregion
	}
}