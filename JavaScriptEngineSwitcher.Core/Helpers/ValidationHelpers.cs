namespace JavaScriptEngineSwitcher.Core.Helpers
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	
	/// <summary>
	/// Validation helpers
	/// </summary>
	public static class ValidationHelpers
	{
		/// <summary>
		/// List of supported types
		/// </summary>
		private static readonly Type[] _supportedTypes =
		{
			typeof(Undefined), typeof(Boolean), typeof(Int32), typeof(Double), typeof(String)		 		
		};

		/// <summary>
		/// Regular expression for working with JS-names
		/// </summary>
		private static readonly Regex _jsNameRegex = new Regex(@"^[A-Za-z_\$][0-9A-Za-z_\$]*$");
		

		/// <summary>
		/// Checks whether supports a .NET type
		/// </summary>
		/// <param name="type">.NET type</param>
		/// <returns>Result of check (true - is supported; false - is not supported)</returns>
		public static bool IsSupportedType(Type type)
		{
			bool result = _supportedTypes.Contains(type);

			return result;
		}

		/// <summary>
		/// Checks a format of the name
		/// </summary>
		/// <param name="name">The name</param>
		/// <returns>Result of check (true - correct format; false - wrong format)</returns>
		public static bool CheckNameFormat(string name)
		{
			return _jsNameRegex.IsMatch(name);
		}
	}
}