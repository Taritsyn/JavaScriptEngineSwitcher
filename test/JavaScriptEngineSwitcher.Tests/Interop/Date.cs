using System;

namespace JavaScriptEngineSwitcher.Tests.Interop
{
	public struct Date
	{
		private static readonly int[] _cumulativeDays = { 0, 31, 59, 90, 120, 151, 181,
			212, 243, 273, 304, 334 };

		public int Year;
		public int Month;
		public int Day;

		public static Date Today
		{
			get
			{
				DateTime currentDateTime = DateTime.Today;
				var currentDate = new Date(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day);

				return currentDate;
			}
		}


		public Date(int year, int month, int day)
		{
			Year = year;
			Month = month;
			Day = day;
		}


		public static bool IsLeapYear(int year)
		{
			return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
		}

		public int GetDayOfYear()
		{
			return _cumulativeDays[Month - 1] +
				Day +
				(Month > 2 && IsLeapYear(Year) ? 1 : 0)
				;
		}

		public Date AddDays(double value)
		{
			var dateTime = new DateTime(Year, Month, Day);
			DateTime newDateTime = dateTime.AddDays(value);

			return new Date(newDateTime.Year, newDateTime.Month, newDateTime.Day);
		}
	}
}