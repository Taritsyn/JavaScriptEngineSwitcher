using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// Defines a interface of JS engine
	/// </summary>
	public interface IJsEngine : IDisposable
	{
		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Gets a version of original JS engine
		/// </summary>
		string Version
		{
			get;
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports script interruption
		/// </summary>
		bool SupportsScriptInterruption
		{
			get;
		}

		/// <summary>
		/// Gets a value that indicates if the JS engine supports garbage collection
		/// </summary>
		bool SupportsGarbageCollection
		{
			get;
		}


		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <param name="expression">JS expression</param>
		/// <returns>Result of the expression</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		object Evaluate(string expression);

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <param name="expression">JS expression</param>
		/// <param name="documentName">Document name</param>
		/// <returns>Result of the expression</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		object Evaluate(string expression, string documentName);

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <typeparam name="T">Type of result</typeparam>
		/// <param name="expression">JS expression</param>
		/// <returns>Result of the expression</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		T Evaluate<T>(string expression);

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <typeparam name="T">Type of result</typeparam>
		/// <param name="expression">JS expression</param>
		/// <param name="documentName">Document name</param>
		/// <returns>Result of the expression</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		T Evaluate<T>(string expression, string documentName);

		/// <summary>
		/// Executes a code
		/// </summary>
		/// <param name="code">JS code</param>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		void Execute(string code);

		/// <summary>
		/// Executes a code
		/// </summary>
		/// <param name="code">JS code</param>
		/// <param name="documentName">Document name</param>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		void Execute(string code, string documentName);

		/// <summary>
		/// Executes a code from JS file
		/// </summary>
		/// <param name="path">Path to the JS file</param>
		/// <param name="encoding">Text encoding</param>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="FileNotFoundException"/>
		/// <exception cref="JsUsageException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		void ExecuteFile(string path, Encoding encoding = null);

		/// <summary>
		/// Executes a code from embedded JS resource
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name without the namespace of the specified type</param>
		/// <param name="type">The type, that determines the assembly and whose namespace is used to scope
		/// the resource name</param>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="JsUsageException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		void ExecuteResource(string resourceName, Type type);

		/// <summary>
		/// Executes a code from embedded JS resource
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name</param>
		/// <param name="assembly">The assembly, which contains the embedded resource</param>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="JsUsageException"/>
		/// <exception cref="JsCompilationException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		void ExecuteResource(string resourceName, Assembly assembly);

		/// <summary>
		/// Calls a function
		/// </summary>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		object CallFunction(string functionName, params object[] args);

		/// <summary>
		/// Calls a function
		/// </summary>
		/// <typeparam name="T">Type of function result</typeparam>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsInterruptedException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		T CallFunction<T>(string functionName, params object[] args);

		/// <summary>
		/// Сhecks for the existence of a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Result of check (true - exists; false - not exists</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		bool HasVariable(string variableName);

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		object GetVariableValue(string variableName);

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <typeparam name="T">Type of variable</typeparam>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		T GetVariableValue<T>(string variableName);

		/// <summary>
		/// Sets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <param name="value">Value of variable</param>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		void SetVariableValue(string variableName, object value);

		/// <summary>
		/// Removes a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsRuntimeException"/>
		/// <exception cref="JsException"/>
		void RemoveVariable(string variableName);

		/// <summary>
		/// Embeds a host object to script code
		/// </summary>
		/// <param name="itemName">The name for the new global variable or function that will represent the object</param>
		/// <param name="value">The object to expose</param>
		/// <remarks>Allows to embed instances of simple classes (or structures) and delegates.</remarks>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsException"/>
		void EmbedHostObject(string itemName, object value);

		/// <summary>
		/// Embeds a host type to script code
		/// </summary>
		/// <param name="itemName">The name for the new global variable that will represent the type</param>
		/// <param name="type">The type to expose</param>
		/// <remarks>
		/// Host types are exposed to script code in the form of objects whose properties and
		/// methods are bound to the type's static members.
		/// </remarks>
		/// <exception cref="ObjectDisposedException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="JsException"/>
		void EmbedHostType(string itemName, Type type);

		/// <summary>
		/// Interrupts script execution and causes the JS engine to throw an exception
		/// </summary>
		void Interrupt();

		/// <summary>
		/// Performs a full garbage collection
		/// </summary>
		void CollectGarbage();
	}
}