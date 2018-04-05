namespace JavaScriptEngineSwitcher.Core.Helpers
{
	/// <summary>
	/// Script error location item
	/// </summary>
	public sealed class ErrorLocationItem
	{
		/// <summary>
		/// Gets or sets a function name
		/// </summary>
		public string FunctionName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a document name
		/// </summary>
		public string DocumentName
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
		/// Constructs an instance of the script error location item
		/// </summary>
		public ErrorLocationItem()
		{
			FunctionName = string.Empty;
			DocumentName = string.Empty;
			LineNumber = 0;
			ColumnNumber = 0;
			SourceFragment = string.Empty;
		}
	}
}