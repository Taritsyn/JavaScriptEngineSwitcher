#if NETCOREAPP1_0
using System.Globalization;

namespace JavaScriptEngineSwitcher.Tests.Interop.Drawing
{
	/// <summary>
	/// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane
	/// </summary>
	public struct Point
	{
		private int _x;
		private int _y;

		/// <summary>
		/// Represents a <see cref="Point"/> that has X and Y values set to zero
		/// </summary>
		public static readonly Point Empty;

		/// <summary>
		/// Gets or sets the x-coordinate of this <see cref="Point"/>
		/// </summary>
		public int X
		{
			get { return _x; }
			set { _x = value; }
		}

		/// <summary>
		/// Gets or sets the y-coordinate of this <see cref="Point"/>
		/// </summary>
		public int Y
		{
			get { return _y; }
			set { _y = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Point"/> is empty
		/// </summary>
		/// <returns>true if both X and Y are 0; otherwise, false</returns>
		public bool IsEmpty
		{
			get
			{
				if (_x == 0 && _y == 0)
				{
					return true;
				}

				return false;
			}
		}


		/// <summary>
		/// Constructs an instance of the <see cref="Point"/> class with the specified coordinates
		/// </summary>
		/// <param name="x">The horizontal position of the point</param>
		/// <param name="y">The vertical position of the point</param>
		public Point(int x, int y)
		{
			_x = x;
			_y = y;
		}


		/// <summary>
		/// Converts this <see cref="Point"/> to a human-readable string
		/// </summary>
		/// <returns>A string that represents this <see cref="Point"/></returns>
		public override string ToString()
		{
			return "{X=" + _x.ToString(CultureInfo.CurrentCulture) +
				",Y=" + _y.ToString(CultureInfo.CurrentCulture) + "}"
				;
		}
	}
}
#endif