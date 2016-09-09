#if NETCOREAPP1_0
using System;
using System.Text;

namespace JavaScriptEngineSwitcher.Tests.Interop.Drawing
{
	/// <summary>
	/// Represents an ARGB (alpha, red, green, blue) color
	/// </summary>
	public struct Color
	{
		private readonly string _name;
		private readonly long _value;
		private readonly short _knownColor;
		private readonly short _state;

		private static short StateKnownColorValid = 1;
		private static short StateARGBValueValid = 2;
		private static short StateValueMask = StateARGBValueValid;
		private static short StateNameValid = 8;
		private static long NotDefinedValue = 0L;
		public static readonly Color Empty = new Color();

		/// <summary>
		/// Gets a value indicating whether this <see cref="Color"/> structure is a predefined color.
		/// Predefined colors are represented by the elements of the <see cref="KnownColor"/> enumeration.
		/// </summary>
		/// <returns>true if this <see cref="Color"/> was created from a predefined color by using either
		/// the <code>FromName</code> method or the <code>FromKnownColor</code> method; otherwise, false.</returns>
		public bool IsKnownColor
		{
			get
			{
				return ((uint)_state & (uint)StateKnownColorValid) > 0U;
			}
		}

		/// <summary>
		/// Gets a name of this <see cref="Color"/>
		/// </summary>
		/// <returns>The name of this <see cref="Color"/></returns>
		public string Name
		{
			get
			{
				if ((_state & StateNameValid) != 0)
				{
					return _name;
				}

				if (IsKnownColor)
				{
					var knownColor = (KnownColor)_knownColor;
					return KnownColorTable.KnownColorToName(knownColor) ?? knownColor.ToString();
				}

				return Convert.ToString(_value, 16);
			}
		}

		private long Value
		{
			get
			{
				if ((_state & StateValueMask) != 0)
				{
					return _value;
				}

				if (IsKnownColor)
				{
					var knownColor = (KnownColor)_knownColor;
					return KnownColorTable.KnownColorToArgb(knownColor);
				}

				return NotDefinedValue;
			}
		}

		/// <summary>
		/// Gets a system-defined color that has an ARGB value of #FFFF4500
		/// </summary>
		/// <returns>A <see cref="Color"/> representing a system-defined color</returns>
		public static Color OrangeRed
		{
			get
			{
				return new Color(KnownColor.OrangeRed);
			}
		}

		/// <summary>
		/// Gets a red component value of this <see cref="Color"/> structure
		/// </summary>
		/// <returns>The red component value of this <see cref="Color"/></returns>
		public byte R
		{
			get
			{
				return (byte)((ulong)(Value >> 16) & byte.MaxValue);
			}
		}

		/// <summary>
		/// Gets a green component value of this <see cref="Color"/> structure
		/// </summary>
		/// <returns>The green component value of this <see cref="Color"/></returns>
		public byte G
		{
			get
			{
				return (byte)((ulong)(Value >> 8) & byte.MaxValue);
			}
		}

		/// <summary>
		/// Gets a blue component value of this <see cref="Color"/> structure
		/// </summary>
		/// <returns>The blue component value of this <see cref="Color"/></returns>
		public byte B
		{
			get
			{
				return (byte)((ulong)Value & byte.MaxValue);
			}
		}

		/// <summary>
		/// Gets a alpha component value of this <see cref="Color"/> structure
		/// </summary>
		/// <returns>The alpha component value of this <see cref="Color"/></returns>
		public byte A
		{
			get
			{
				return (byte)((ulong)(Value >> 24) & byte.MaxValue);
			}
		}


		internal Color(KnownColor knownColor)
		{
			_value = 0L;
			_state = StateKnownColorValid;
			_name = null;
			_knownColor = (short)knownColor;
		}

		private Color(long value, short state, string name, KnownColor knownColor)
		{
			_value = value;
			_state = state;
			_name = name;
			_knownColor = (short)knownColor;
		}


		private static void CheckByte(int value, string name)
		{
			if (value < 0 || value > byte.MaxValue)
			{
				throw new ArgumentException(string.Format(
					"Value of '{1}' is not valid for '{0}'. '{0}' should be greater than or equal to {2} and less than or equal to {3}.",
					name, value, 0, (int)byte.MaxValue
				));
			}
		}

		private static long MakeArgb(byte alpha, byte red, byte green, byte blue)
		{
			return (uint)(red << 16 | green << 8 | blue | alpha << 24) & (long)uint.MaxValue;
		}

		/// <summary>
		/// Creates a <see cref="Color"/> structure from the specified 8-bit color values (red, green, and blue).
		/// The alpha value is implicitly 255 (fully opaque).
		/// Although this method allows a 32-bit value to be passed for each color component, the value
		/// of each component is limited to 8 bits.
		/// </summary>
		/// <param name="red">The red component value for the new <see cref="Color"/>.
		/// Valid values are 0 through 255.</param>
		/// <param name="green">The green component value for the new <see cref="Color"/>.
		/// Valid values are 0 through 255.</param>
		/// <param name="blue">The blue component value for the new <see cref="Color"/>.
		/// Valid values are 0 through 255.</param>
		/// <returns>The <see cref="Color"/> that this method creates</returns>
		/// <exception cref="ArgumentException"><paramref name="red"/>, <paramref name="green"/>, or
		/// <paramref name="blue"/> is less than 0 or greater than 255.</exception>
		public static Color FromArgb(int red, int green, int blue)
		{
			return FromArgb(byte.MaxValue, red, green, blue);
		}

		/// <summary>
		/// Creates a <see cref="Color"/> structure from the four ARGB component (alpha, red, green, and blue) values.
		/// Although this method allows a 32-bit value to be passed for each component, the value of
		/// each component is limited to 8 bits.
		/// </summary>
		/// <param name="alpha">The alpha component. Valid values are 0 through 255.</param>
		/// <param name="red">The red component. Valid values are 0 through 255.</param>
		/// <param name="green">The green component. Valid values are 0 through 255.</param>
		/// <param name="blue">The blue component. Valid values are 0 through 255.</param>
		/// <returns>The <see cref="Color"/> that this method creates</returns>
		/// <exception cref="ArgumentException"><paramref name="alpha"/>, <paramref name="red"/>,
		/// <paramref name="green"/>, or <paramref name="blue"/> is less than 0 or greater than 255.</exception>
		public static Color FromArgb(int alpha, int red, int green, int blue)
		{
			CheckByte(alpha, "alpha");
			CheckByte(red, "red");
			CheckByte(green, "green");
			CheckByte(blue, "blue");

			return new Color(MakeArgb((byte)alpha, (byte)red, (byte)green, (byte)blue),
				StateARGBValueValid, null, 0);
		}

		/// <summary>
		/// Gets a hue-saturation-brightness (HSB) brightness value for this <see cref="Color"/> structure
		/// </summary>
		/// <returns>The brightness of this <see cref="Color"/>.
		/// The brightness ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.</returns>
		public float GetBrightness()
		{
			double num1 = R / (double)byte.MaxValue;
			float num2 = G / (float)byte.MaxValue;
			float num3 = B / (float)byte.MaxValue;
			float num4 = (float)num1;
			float num5 = (float)num1;
			if (num2 > num4)
			{
				num4 = num2;
			}
			if (num3 > num4)
			{
				num4 = num3;
			}
			if (num2 < num5)
			{
				num5 = num2;
			}
			if (num3 < num5)
			{
				num5 = num3;
			}

			return (float)((num4 + num5) / 2.0);
		}

		/// <summary>
		/// Gets a hue-saturation-brightness (HSB) hue value, in degrees, for this <see cref="Color"/> structure
		/// </summary>
		/// <returns>The hue, in degrees, of this <see cref="Color"/>.
		/// The hue is measured in degrees, ranging from 0.0 through 360.0, in HSB color space.</returns>
		public float GetHue()
		{
			if (R == G && G == B)
			{
				return 0.0f;
			}
			float num1 = R / (float)byte.MaxValue;
			float num2 = G / (float)byte.MaxValue;
			float num3 = B / (float)byte.MaxValue;
			float num4 = 0.0f;
			float num5 = num1;
			float num6 = num1;
			if (num2 > num5)
			{
				num5 = num2;
			}
			if (num3 > num5)
			{
				num5 = num3;
			}
			if (num2 < num6)
			{
				num6 = num2;
			}
			if (num3 < num6)
			{
				num6 = num3;
			}
			float num7 = num5 - num6;
			if (num1 == num5)
			{
				num4 = (num2 - num3)/num7;
			}
			else if (num2 == num5)
			{
				num4 = (float)(2.0 + (num3 - num1) / num7);
			}
			else if (num3 == num5)
			{
				num4 = (float)(4.0 + (num1 - num2) / (double)num7);
			}
			float num8 = num4 * 60f;
			if (num8 < 0.0)
			{
				num8 += 360f;
			}

			return num8;
		}

		/// <summary>
		/// Gets a hue-saturation-brightness (HSB) saturation value for this <see cref="Color"/> structure
		/// </summary>
		/// <returns>The saturation of this <see cref="Color"/>.
		/// The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
		public float GetSaturation()
		{
			double num1 = R / (double)byte.MaxValue;
			float num2 = G / (float)byte.MaxValue;
			float num3 = B / (float)byte.MaxValue;
			float num4 = 0.0f;
			float num5 = (float)num1;
			float num6 = (float)num1;
			if (num2 > num5)
			{
				num5 = num2;
			}
			if (num3 > num5)
			{
				num5 = num3;
			}
			if (num2 < num6)
			{
				num6 = num2;
			}
			if (num3 < num6)
			{
				num6 = num3;
			}
			if (num5 != num6)
			{
				num4 = (num5 + num6) / 2.0 > 0.5 ?
					(float)((num5 -num6) / (2.0 - num5 - num6))
					:
					(num5 - num6) / (num5 + num6)
					;
			}

			return num4;
		}

		/// <summary>
		/// Converts this <see cref="Color"/> structure to a human-readable string
		/// </summary>
		/// <returns>A string that is the name of this <see cref="Color"/>, if the <see cref="Color"/>
		/// is created from a predefined color by using either the <code>FromName</code> method
		/// or the <code>FromKnownColor</code> method;
		/// otherwise, a string that consists of the ARGB component names and their values.
		/// </returns>
		public override string ToString()
		{
			var sb = new StringBuilder(32);
			sb.Append(GetType().Name);
			sb.Append(" [");
			if ((_state & StateNameValid) != 0)
			{
				sb.Append(Name);
			}
			else if ((_state & StateKnownColorValid) != 0)
			{
				sb.Append(Name);
			}
			else if ((_state & StateValueMask) != 0)
			{
				sb.Append("A=");
				sb.Append(A);
				sb.Append(", R=");
				sb.Append(R);
				sb.Append(", G=");
				sb.Append(G);
				sb.Append(", B=");
				sb.Append(B);
			}
			else
			{
				sb.Append("Empty");
			}
			sb.Append("]");

			return sb.ToString();
		}
	}
}
#endif