#if !NETSTANDARD1_3
using System;
using System.Runtime.Serialization;

#endif
namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The fatal exception occurred
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public sealed class JsFatalException : JsException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsFatalException"/> class
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		public JsFatalException(JsErrorCode errorCode)
			: base(errorCode)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsFatalException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		/// <param name="message">The error message</param>
		public JsFatalException(JsErrorCode errorCode, string message)
			: base(errorCode, message)
		{ }
#if !NETSTANDARD1_3

		/// <summary>
		/// Initializes a new instance of the <see cref="JsFatalException"/> class with serialized data
		/// </summary>
		/// <param name="info">The object that holds the serialized data</param>
		/// <param name="context">The contextual information about the source or destination</param>
		private JsFatalException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}