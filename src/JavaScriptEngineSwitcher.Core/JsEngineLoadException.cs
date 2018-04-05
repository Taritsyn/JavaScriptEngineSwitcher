using System;
#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

using JavaScriptEngineSwitcher.Core.Constants;

namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// The exception that is thrown when a loading of JS engine is failed
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public sealed class JsEngineLoadException : JsEngineException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public JsEngineLoadException(string message)
			: base(message)
		{
			Category = JsErrorCategory.EngineLoad;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// with a specified error message and a reference to the inner exception
		/// that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsEngineLoadException(string message, Exception innerException)
			: base(message, innerException)
		{
			Category = JsErrorCategory.EngineLoad;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JS engine</param>
		/// <param name="engineVersion">Version of original JS engine</param>
		public JsEngineLoadException(string message, string engineName, string engineVersion)
			: base(message, engineName, engineVersion)
		{
			Category = JsErrorCategory.EngineLoad;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="engineName">Name of JS engine</param>
		/// <param name="engineVersion">Version of original JS engine</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsEngineLoadException(string message, string engineName, string engineVersion,
			Exception innerException)
			: base(message, engineName, engineVersion, innerException)
		{
			Category = JsErrorCategory.EngineLoad;
		}
#if !NETSTANDARD1_3

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineLoadException"/> class with serialized data
		/// </summary>
		/// <param name="info">The object that holds the serialized data</param>
		/// <param name="context">The contextual information about the source or destination</param>
		private JsEngineLoadException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}