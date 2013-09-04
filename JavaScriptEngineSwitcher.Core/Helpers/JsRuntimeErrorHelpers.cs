namespace JavaScriptEngineSwitcher.Core.Helpers
{
	using System;
	using System.Text;

	using Resources;
	using Utilities;

	/// <summary>
	/// JavaScript runtime error helpers
	/// </summary>
	public static class JsRuntimeErrorHelpers
	{
		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="jsRuntimeException">JavaScript runtime exception</param>
		/// <returns>Detailed error message</returns>
		public static string Format(JsRuntimeException jsRuntimeException)
		{
			var errorMessage = new StringBuilder();
			errorMessage.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_Message,
				jsRuntimeException.Message);
			errorMessage.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_EngineName,
				jsRuntimeException.EngineName);
			if (!string.IsNullOrWhiteSpace(jsRuntimeException.ErrorCode))
			{
				errorMessage.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_ErrorCode,
					jsRuntimeException.ErrorCode);
			}
			if (!string.IsNullOrWhiteSpace(jsRuntimeException.Category))
			{
				errorMessage.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_Category,
					jsRuntimeException.Category);
			}
			if (jsRuntimeException.LineNumber > 0)
			{
				errorMessage.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_LineNumber,
					jsRuntimeException.LineNumber.ToString());
			}
			if (jsRuntimeException.ColumnNumber > 0)
			{
				errorMessage.AppendFormatLine("{0}: {1}", Strings.ErrorDetails_ColumnNumber,
					jsRuntimeException.ColumnNumber.ToString());	
			}
			if (!string.IsNullOrWhiteSpace(jsRuntimeException.SourceFragment))
			{
				errorMessage.AppendFormatLine("{1}:{0}{0}{2}", Environment.NewLine,
					Strings.ErrorDetails_SourceFragment,
					jsRuntimeException.SourceFragment);
			}

			return errorMessage.ToString();
		}
	}
}