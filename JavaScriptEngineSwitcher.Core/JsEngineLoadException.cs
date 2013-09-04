namespace JavaScriptEngineSwitcher.Core
{
	using System;

	/// <summary>
	/// The exception that is thrown when a loading of JavaScript engine is failed
	/// </summary>
	public sealed class JsEngineLoadException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the JavaScriptEngineSwitcher.Core.JsEngineLoadException class 
		/// with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public JsEngineLoadException(string message)
			: base(message)
		{ }

		/// <summary>
		/// Initializes a new instance of the JavaScriptEngineSwitcher.Core.JsEngineLoadException class 
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsEngineLoadException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}
}