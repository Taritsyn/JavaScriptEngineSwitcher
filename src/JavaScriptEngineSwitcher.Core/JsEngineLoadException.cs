namespace JavaScriptEngineSwitcher.Core
{
	using System;

	/// <summary>
	/// The exception that is thrown when a loading of JavaScript engine is failed
	/// </summary>
	public sealed class JsEngineLoadException : JsException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public JsEngineLoadException(string message)
			: this(message, null)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsEngineLoadException(string message, Exception innerException)
			: this(message, string.Empty, string.Empty, innerException)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JavaScript engine</param>
		/// <param name="engineVersion">Version of original JavaScript engine</param>
		public JsEngineLoadException(string message, string engineName, string engineVersion)
			: this(message, engineName, engineVersion, null)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JavaScript engine</param>
		/// <param name="engineVersion">Version of original JavaScript engine</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsEngineLoadException(string message, string engineName, string engineVersion,
			Exception innerException)
			: base(message, engineName, engineVersion, innerException)
		{ }
	}
}