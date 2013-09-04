namespace JavaScriptEngineSwitcher.Core
{
	using System;

	/// <summary>
	/// The exception that is thrown during a execution of code by JavaScript engine
	/// </summary>
	public sealed class JsRuntimeException : Exception
	{
		/// <summary>
		/// Gets or sets a name of JavaScript engine
		/// </summary>
		public string EngineName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a error code
		/// </summary>
		public string ErrorCode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a error category
		/// </summary>
		public string Category
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a line number
		/// </summary>
		public int LineNumber
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a column number
		/// </summary>
		public int ColumnNumber
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a source fragment
		/// </summary>
		public string SourceFragment
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the JavaScriptEngineSwitcher.Core.JsRuntimeException class 
		/// with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public JsRuntimeException(string message)
			: this(message, null)
		{ }

		/// <summary>
		/// Initializes a new instance of the JavaScriptEngineSwitcher.Core.JsRuntimeException class 
		/// with a specified error message and a reference to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsRuntimeException(string message, Exception innerException)
			: base(message, innerException)
		{
			EngineName = string.Empty;
			ErrorCode = string.Empty;
			Category = string.Empty;
			LineNumber = 0;
			ColumnNumber = 0;
			SourceFragment = string.Empty;
		}
	}
}