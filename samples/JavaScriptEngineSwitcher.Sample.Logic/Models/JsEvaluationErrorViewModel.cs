namespace JavaScriptEngineSwitcher.Sample.Logic.Models
{
	/// <summary>
	/// JS evaluation error view model
	/// </summary>
	public sealed class JsEvaluationErrorViewModel
	{
		/// <summary>
		/// Gets or sets a name of JS engine
		/// </summary>
		public string EngineName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a version of original JS engine
		/// </summary>
		public string EngineVersion
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a full name of JS engine
		/// </summary>
		public string EngineFullName
		{
			get
			{
				string engineFullName = EngineName + " ";
				if (EngineVersion.Contains(".") || EngineVersion.Contains(","))
				{
					engineFullName += "version ";
					if (EngineVersion.Contains(","))
					{
						engineFullName += "of ";
					}
				}
				engineFullName += EngineVersion;

				return engineFullName;
			}
		}

		/// <summary>
		/// Gets or sets a message
		/// </summary>
		public string Message
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
		/// Constructs an instance of JS evaluation error view model
		/// </summary>
		public JsEvaluationErrorViewModel()
		{
			EngineName = string.Empty;
			EngineVersion = string.Empty;
			Message = string.Empty;
			LineNumber = 0;
			ColumnNumber = 0;
			SourceFragment = string.Empty;
		}
	}
}