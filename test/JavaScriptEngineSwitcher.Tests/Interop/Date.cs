namespace JavaScriptEngineSwitcher.Tests.Interop
{
	using System;

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
				Date currentDate = new Date(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day);

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
	}
}