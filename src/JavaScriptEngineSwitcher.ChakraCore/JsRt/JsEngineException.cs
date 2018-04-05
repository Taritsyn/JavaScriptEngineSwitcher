#if !NETSTANDARD1_3
using System;
using System.Runtime.Serialization;

#endif
namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The exception that occurred in the workings of the JavaScript engine itself
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public sealed class JsEngineException : JsException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineException"/> class
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		public JsEngineException(JsErrorCode errorCode)
			: base(errorCode)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		/// <param name="message">The error message</param>
		public JsEngineException(JsErrorCode errorCode, string message)
			: base(errorCode, message)
		{ }
#if !NETSTANDARD1_3

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineException"/> class with serialized data
		/// </summary>
		/// <param name="info">The object that holds the serialized data</param>
		/// <param name="context">The contextual information about the source or destination</param>
		private JsEngineException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}