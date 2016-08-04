using System;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The script exception
	/// </summary>
	internal sealed class JsScriptException : JsException
	{
		/// <summary>
		/// The error
		/// </summary>
		private readonly JsValue _error;

		/// <summary>
		/// Gets a JavaScript object representing the script error
		/// </summary>
		public JsValue Error
		{
			get
			{
				return _error;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsScriptException"/> class
		/// </summary>
		/// <param name="code">The error code returned</param>
		/// <param name="error">The JavaScript error object</param>
		public JsScriptException(JsErrorCode code, JsValue error)
			: this(code, error, "JavaScript Exception")
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsScriptException"/> class
		/// </summary>
		/// <param name="code">The error code returned</param>
		/// <param name="error">The JavaScript error object</param>
		/// <param name="message">The error message</param>
		public JsScriptException(JsErrorCode code, JsValue error, string message)
			: base(code, message)
		{
			_error = error;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsScriptException"/> class
		/// </summary>
		/// <param name="message">The error message</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		private JsScriptException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}
}