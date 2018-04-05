#if !NETSTANDARD1_3
using System;
using System.Runtime.Serialization;

#endif
namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The script exception
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public sealed class JsScriptException : JsException
	{
		/// <summary>
		/// The error metadata
		/// </summary>
#if !NETSTANDARD1_3
		[NonSerialized]
#endif
		private readonly JsValue _metadata;

		/// <summary>
		/// Gets a JavaScript object representing the error metadata
		/// </summary>
		internal JsValue Metadata
		{
			get { return _metadata; }
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsScriptException"/> class
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		public JsScriptException(JsErrorCode errorCode)
			: this(errorCode, "JavaScript Exception")
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsScriptException"/> class
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		/// <param name="message">The error message</param>
		public JsScriptException(JsErrorCode errorCode, string message)
			: this(errorCode, JsValue.Invalid, message)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsScriptException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		/// <param name="metadata">The JavaScript error metadata</param>
		/// <param name="message">The error message</param>
		internal JsScriptException(JsErrorCode errorCode, JsValue metadata, string message)
			: base(errorCode, message)
		{
			_metadata = metadata;
		}
#if !NETSTANDARD1_3

		/// <summary>
		/// Initializes a new instance of the <see cref="JsScriptException"/> class with serialized data
		/// </summary>
		/// <param name="info">The object that holds the serialized data</param>
		/// <param name="context">The contextual information about the source or destination</param>
		private JsScriptException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}