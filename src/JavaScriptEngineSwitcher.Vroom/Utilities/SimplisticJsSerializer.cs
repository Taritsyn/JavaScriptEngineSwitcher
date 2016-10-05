using System;
using System.Globalization;
using System.Text;

using JavaScriptEngineSwitcher.Core;

using JavaScriptEngineSwitcher.Vroom.Resources;

namespace JavaScriptEngineSwitcher.Vroom.Utilities
{
	/// <summary>
	/// Simplistic JavaScript serializer
	/// </summary>
	internal static class SimplisticJsSerializer
	{
		private static bool JsEncodeAmpersand
		{
			get { return true; }
		}


		/// <summary>
		/// Converts a value to JavaScript string
		/// </summary>
		/// <param name="value">The value to serialize</param>
		/// <returns>The serialized JavaScript string</returns>
		public static string Serialize(object value)
		{
			if (value == null)
			{
				return "null";
			}

			if (value is Undefined)
			{
				return "undefined";
			}

			string serializedValue;
			Type type = value.GetType();
			TypeCode typeCode = Type.GetTypeCode(type);

			switch (typeCode)
			{
				case TypeCode.Boolean:
					serializedValue = SerializeBoolean((bool)value);
					break;
				case TypeCode.Int32:
					var convertible = value as IConvertible;
					serializedValue = (convertible != null) ?
						convertible.ToString(CultureInfo.InvariantCulture) : value.ToString();
					break;
				case TypeCode.Double:
					serializedValue = ((double)value).ToString("r", CultureInfo.InvariantCulture);
					break;
				case TypeCode.String:
					serializedValue = SerializeString((string)value);
					break;
				default:
					throw new NotSupportedException(string.Format(Strings.Common_CannotSerializeType, type));
			}

			return serializedValue;
		}

		private static string SerializeBoolean(bool value)
		{
			string serializedValue = value ? "true" : "false";

			return serializedValue;
		}

		private static string SerializeString(string value)
		{
			string serializedValue = '"' + JsStringEncode(value) + '"';

			return serializedValue;
		}

		private static string JsStringEncode(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			StringBuilder sb = null;
			int charCount = value.Length;

			int startIndex = 0;
			int count = 0;

			for (int charIndex = 0; charIndex < charCount; ++charIndex)
			{
				char charValue = value[charIndex];

				if (CharRequiresJsEncoding(charValue))
				{
					if (sb == null)
					{
						sb = new StringBuilder(value.Length + 5);
					}

					if (count > 0)
					{
						sb.Append(value, startIndex, count);
					}

					startIndex = charIndex + 1;
					count = 0;

					switch (charValue)
					{
						case '\b':
							sb.Append("\\b");
							break;
						case '\t':
							sb.Append("\\t");
							break;
						case '\n':
							sb.Append("\\n");
							break;
						case '\f':
							sb.Append("\\f");
							break;
						case '\r':
							sb.Append("\\r");
							break;
						case '"':
							sb.Append("\\\"");
							break;
						case '\\':
							sb.Append("\\\\");
							break;
						default:
							AppendCharAsJsUnicode(sb, charValue);
							break;
					}
				}
				else
				{
					++count;
				}
			}

			if (sb == null)
			{
				return value;
			}

			if (count > 0)
			{
				sb.Append(value, startIndex, count);
			}

			return sb.ToString();
		}

		private static bool CharRequiresJsEncoding(char charValue)
		{
			if (charValue >= 32 && charValue != 34 && (charValue != 92 && charValue != 39)
				&& (charValue != 60 && charValue != 62 && (charValue != 38 || !JsEncodeAmpersand))
				&& (charValue != 133 && charValue != 8232))
			{
				return (charValue == 8233);
			}

			return true;
		}

		private static void AppendCharAsJsUnicode(StringBuilder sb, char charValue)
		{
			sb.Append("\\u");
			sb.Append(((int)charValue).ToString("x4", CultureInfo.InvariantCulture));
		}
	}
}