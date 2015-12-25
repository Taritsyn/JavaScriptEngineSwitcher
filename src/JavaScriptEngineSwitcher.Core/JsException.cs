namespace JavaScriptEngineSwitcher.Core
{
	using System;

	/// <summary>
	/// The exception that is thrown during the work of JavaScript engine
	/// </summary>
	public class JsException : Exception
	{
		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		public string EngineName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a version of original JavaScript engine
		/// </summary>
		public string EngineVersion
		{
			get;
			private set;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public JsException(string message)
			: this(message, string.Empty, string.Empty)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsException(string message, Exception innerException)
			: this(message, string.Empty, string.Empty, innerException)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JavaScript engine</param>
		/// <param name="engineVersion">Version of original JavaScript engine</param>
		public JsException(string message, string engineName, string engineVersion)
			: this(message, engineName, engineVersion, null)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JavaScript engine</param>
		/// <param name="engineVersion">Version of original JavaScript engine</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsException(string message, string engineName, string engineVersion, Exception innerException)
			: base(message, innerException)
		{
			EngineName = engineName;
			EngineVersion = engineVersion;
		}
	}
}