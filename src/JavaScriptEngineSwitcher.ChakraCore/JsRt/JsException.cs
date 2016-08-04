using System;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The exception returned from the Chakra engine
	/// </summary>
	internal class JsException : Exception
	{
		/// <summary>
		/// The error code
		/// </summary>
		private readonly JsErrorCode _code;

		/// <summary>
		/// Gets a error code
		/// </summary>
		public JsErrorCode ErrorCode
		{
			get { return _code; }
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// </summary>
		/// <param name="code">The error code returned</param>
		public JsException(JsErrorCode code)
			: this(code, "A fatal exception has occurred in a JavaScript runtime")
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// </summary>
		/// <param name="code">The error code returned</param>
		/// <param name="message">The error message</param>
		public JsException(JsErrorCode code, string message)
			: base(message)
		{
			_code = code;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// </summary>
		/// <param name="message">The error message</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		protected JsException(string message, Exception innerException)
			: base(message, innerException)
		{
			if (message != null)
			{
				_code = (JsErrorCode) HResult;
			}
		}
	}
}