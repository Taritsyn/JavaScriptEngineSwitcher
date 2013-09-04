namespace JavaScriptEngineSwitcher.Core
{
	using System;
	using System.Text;
	using System.Reflection;

	/// <summary>
	/// Interface of JavaScript engine
	/// </summary>
	public interface IJsEngine : IDisposable
	{
		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <param name="expression">JS-expression</param>
		/// <returns>Result of the expression</returns>
		object Evaluate(string expression);

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <typeparam name="T">Type of result</typeparam>
		/// <param name="expression">JS-expression</param>
		/// <returns>Result of the expression</returns>
		T Evaluate<T>(string expression);

		/// <summary>
		/// Executes a code
		/// </summary>
		/// <param name="code">Code</param>
		void Execute(string code);

		/// <summary>
		/// Executes a code from JS-file
		/// </summary>
		/// <param name="path">Path to the JS-file</param>
		/// <param name="encoding">Text encoding</param>
		void ExecuteFile(string path, Encoding encoding = null);

		/// <summary>
		/// Executes a code from embedded JS-resource
		/// </summary>
		/// <param name="resourceName">JS-resource name</param>
		/// <param name="type">Type from assembly that containing an embedded resource</param>
		void ExecuteResource(string resourceName, Type type);

		/// <summary>
		/// Executes a code from embedded JS-resource
		/// </summary>
		/// <param name="resourceName">JS-resource name</param>
		/// <param name="assembly">Assembly that containing an embedded resource</param>
		void ExecuteResource(string resourceName, Assembly assembly);

		/// <summary>
		/// Calls a function
		/// </summary>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution</returns>
		object CallFunction(string functionName, params object[] args);

		/// <summary>
		/// Calls a function
		/// </summary>
		/// <typeparam name="T">Type of function result</typeparam>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution</returns>
		T CallFunction<T>(string functionName, params object[] args);

		/// <summary>
		/// Сhecks for the existence of a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Result of check (true - exists; false - not exists</returns>
		bool HasVariable(string variableName);

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		object GetVariableValue(string variableName);

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <typeparam name="T">Type of variable</typeparam>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		T GetVariableValue<T>(string variableName);

		/// <summary>
		/// Sets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <param name="value">Value of variable</param>
		void SetVariableValue(string variableName, object value);

		/// <summary>
		/// Removes a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		void RemoveVariable(string variableName);
	}
}