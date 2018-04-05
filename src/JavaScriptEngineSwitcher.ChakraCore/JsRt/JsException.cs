using System;
#if !NETSTANDARD1_3
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The exception returned from the Chakra engine
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public class JsException : Exception
	{
		/// <summary>
		/// The error code
		/// </summary>
		private readonly JsErrorCode _errorCode;

		/// <summary>
		/// Gets a error code
		/// </summary>
		public JsErrorCode ErrorCode
		{
			get { return _errorCode; }
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		public JsException(JsErrorCode errorCode)
			: this(errorCode, "A fatal exception has occurred in a JavaScript runtime")
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="errorCode">The error code returned</param>
		/// <param name="message">The error message</param>
		public JsException(JsErrorCode errorCode, string message)
			: base(message)
		{
			_errorCode = errorCode;
		}
#if !NETSTANDARD1_3

		/// <summary>
		/// Initializes a new instance of the <see cref="JsException"/> class with serialized data
		/// </summary>
		/// <param name="info">The object that holds the serialized data</param>
		/// <param name="context">The contextual information about the source or destination</param>
		protected JsException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				_errorCode = (JsErrorCode)info.GetUInt32("ErrorCode");
			}
		}


		#region Exception overrides

		/// <summary>
		/// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> to populate with data</param>
		/// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization</param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			base.GetObjectData(info, context);
			info.AddValue("ErrorCode", (uint)_errorCode);
		}

		#endregion
#endif
	}
}