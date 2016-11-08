using System;
#if !NETSTANDARD1_3
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif

namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// The exception that is thrown during a execution of code by JS engine
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public sealed class JsRuntimeException : JsException
	{
		/// <summary>
		/// Error code
		/// </summary>
		private string _errorCode = string.Empty;

		/// <summary>
		/// Error category
		/// </summary>
		private string _category = string.Empty;

		/// <summary>
		/// Line number
		/// </summary>
		private int _lineNumber;

		/// <summary>
		/// Column number
		/// </summary>
		private int _columnNumber;

		/// <summary>
		/// Source fragment
		/// </summary>
		private string _sourceFragment = string.Empty;

		/// <summary>
		/// Gets or sets a error code
		/// </summary>
		public string ErrorCode
		{
			get { return _errorCode; }
			set { _errorCode = value; }
		}

		/// <summary>
		/// Gets or sets a error category
		/// </summary>
		public string Category
		{
			get { return _category; }
			set { _category = value; }
		}

		/// <summary>
		/// Gets or sets a line number
		/// </summary>
		public int LineNumber
		{
			get { return _lineNumber; }
			set { _lineNumber = value; }
		}

		/// <summary>
		/// Gets or sets a column number
		/// </summary>
		public int ColumnNumber
		{
			get { return _columnNumber; }
			set { _columnNumber = value; }
		}

		/// <summary>
		/// Gets or sets a source fragment
		/// </summary>
		public string SourceFragment
		{
			get { return _sourceFragment; }
			set { _sourceFragment = value; }
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsRuntimeException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public JsRuntimeException(string message)
			: base(message)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsRuntimeException"/> class
		/// with a specified error message and a reference to the inner exception
		/// that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsRuntimeException(string message, Exception innerException)
			: base(message, innerException)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsRuntimeException"/> class
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JS engine</param>
		/// <param name="engineVersion">Version of original JS engine</param>
		public JsRuntimeException(string message, string engineName, string engineVersion)
			: base(message, engineName, engineVersion)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsRuntimeException"/> class
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JS engine</param>
		/// <param name="engineVersion">Version of original JS engine</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsRuntimeException(string message, string engineName, string engineVersion,
			Exception innerException)
			: base(message, engineName, engineVersion, innerException)
		{ }
#if !NETSTANDARD1_3

		/// <summary>
		/// Initializes a new instance of the <see cref="JsRuntimeException"/> class with serialized data
		/// </summary>
		/// <param name="info">The object that holds the serialized data</param>
		/// <param name="context">The contextual information about the source or destination</param>
		private JsRuntimeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				_errorCode = info.GetString("ErrorCode");
				_category = info.GetString("Category");
				_lineNumber = info.GetInt32("LineNumber");
				_columnNumber = info.GetInt32("ColumnNumber");
				_sourceFragment = info.GetString("SourceFragment");
			}
		}


		#region JsException overrides

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
				throw new ArgumentNullException("info");
			}

			base.GetObjectData(info, context);
			info.AddValue("ErrorCode", _errorCode);
			info.AddValue("Category", _category);
			info.AddValue("LineNumber", _lineNumber);
			info.AddValue("ColumnNumber", _columnNumber);
			info.AddValue("SourceFragment", _sourceFragment);
		}

		#endregion
#endif
	}
}