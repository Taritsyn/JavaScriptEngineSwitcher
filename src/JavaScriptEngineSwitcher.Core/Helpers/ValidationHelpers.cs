using System;
using System.Linq;
using System.Text.RegularExpressions;

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.Core.Helpers
{
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
		/// List of primitive type codes
		/// </summary>
		private static readonly TypeCode[] _primitiveTypeCodes =
		{
			TypeCode.Boolean,
			TypeCode.SByte, TypeCode.Byte,
			TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
			TypeCode.Single, TypeCode.Double, TypeCode.Decimal,
			TypeCode.Char, TypeCode.String
		};

		/// <summary>
		/// Regular expression for working with JS names
		/// </summary>
		private static readonly Regex _jsNameRegex = new Regex(@"^[$_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}]" +
			@"[$_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\u200C\u200D\p{Mn}\p{Mc}\p{Nd}\p{Pc}]*$");


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
		/// Checks whether .NET type is primitive
		/// </summary>
		/// <param name="type">.NET type</param>
		/// <returns>Result of check (true - is primitive; false - is not primitive)</returns>
		public static bool IsPrimitiveType(Type type)
		{
			TypeCode typeCode = type.GetTypeCode();
			bool result = _primitiveTypeCodes.Contains(typeCode);

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