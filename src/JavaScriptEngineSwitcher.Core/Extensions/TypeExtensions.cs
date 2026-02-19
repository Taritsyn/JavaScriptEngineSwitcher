using System;
#if NETSTANDARD1_3
using System.Reflection;
#endif

namespace JavaScriptEngineSwitcher.Core.Extensions
{
	/// <summary>
	/// Type extensions
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Gets a underlying type code of the specified <see cref="Type"/>
		/// </summary>
		/// <param name="source">The type whose underlying type code to get</param>
		/// <returns>The code of the underlying type</returns>
		public static TypeCode GetTypeCode(this Type source)
		{
			TypeCode typeCode;

#if NETSTANDARD1_3
			if (source is null)
			{
				typeCode = TypeCode.Empty;
			}
			else if (source == typeof(bool))
			{
				typeCode = TypeCode.Boolean;
			}
			else if (source == typeof(char))
			{
				typeCode = TypeCode.Char;
			}
			else if (source == typeof(sbyte))
			{
				typeCode = TypeCode.SByte;
			}
			else if (source == typeof(byte))
			{
				typeCode = TypeCode.Byte;
			}
			else if (source == typeof(short))
			{
				typeCode = TypeCode.Int16;
			}
			else if (source == typeof(ushort))
			{
				typeCode = TypeCode.UInt16;
			}
			else if (source == typeof(int))
			{
				typeCode = TypeCode.Int32;
			}
			else if (source == typeof(uint))
			{
				typeCode = TypeCode.UInt32;
			}
			else if (source == typeof(long))
			{
				typeCode = TypeCode.Int64;
			}
			else if (source == typeof(ulong))
			{
				typeCode = TypeCode.UInt64;
			}
			else if (source == typeof(float))
			{
				typeCode = TypeCode.Single;
			}
			else if (source == typeof(double))
			{
				typeCode = TypeCode.Double;
			}
			else if (source == typeof(decimal))
			{
				typeCode = TypeCode.Decimal;
			}
			else if (source == typeof(DateTime))
			{
				typeCode = TypeCode.DateTime;
			}
			else if (source == typeof(string))
			{
				typeCode = TypeCode.String;
			}
			else if (source.GetTypeInfo().IsEnum)
			{
				typeCode = GetTypeCode(Enum.GetUnderlyingType(source));
			}
			else
			{
				typeCode = TypeCode.Object;
			}
#else
			typeCode = Type.GetTypeCode(source);
#endif

			return typeCode;
		}
	}
}