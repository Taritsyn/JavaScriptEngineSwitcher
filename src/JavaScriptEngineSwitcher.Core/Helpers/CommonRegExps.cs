namespace JavaScriptEngineSwitcher.Core.Helpers
{
	/// <summary>
	/// Common regular expressions
	/// </summary>
	public static class CommonRegExps
	{
		/// <summary>
		/// Pattern for working with JS names
		/// </summary>
		public static readonly string JsNamePattern = @"[$_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}]" +
			@"[$_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\u200C\u200D\p{Mn}\p{Mc}\p{Nd}\p{Pc}]*";

		/// <summary>
		/// Pattern for working with JS full names
		/// </summary>
		public static readonly string JsFullNamePattern = JsNamePattern + @"(?:\." + JsNamePattern + @")*";

		/// <summary>
		/// Pattern for working with document names
		/// </summary>
		public static readonly string DocumentNamePattern = @"[^\s*?""<>|][^\t\n\r*?""<>|]*?";
	}
}