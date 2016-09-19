using System;

namespace JavaScriptEngineSwitcher.Tests.Interop
{
	public struct Temperature
	{
		double _celsius;

		public double Celsius
		{
			get
			{
				return _celsius;
			}
			set
			{
				_celsius = value;
			}
		}

		public double Kelvin
		{
			get
			{
				return _celsius + 273.15;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}

				_celsius = value - 273.15;
			}
		}

		public double Fahrenheit
		{
			get
			{
				return 9 * _celsius / 5 + 32;
			}
			set
			{
				_celsius = 5 * (value - 32) / 9;
			}
		}

		public Temperature(double degree, TemperatureUnits units)
		{
			_celsius = 0;

			switch (units)
			{
				case TemperatureUnits.Celsius:
					Celsius = degree;
					break;
				case TemperatureUnits.Kelvin:
					Kelvin = degree;
					break;
				case TemperatureUnits.Fahrenheit:
					Fahrenheit = degree;
					break;
				default:
					throw new NotSupportedException();
			}
		}

		public override string ToString()
		{
			return ToString(TemperatureUnits.Celsius);
		}

		public string ToString(TemperatureUnits units)
		{
			string formattedValue;

			switch (units)
			{
				case TemperatureUnits.Celsius:
					formattedValue = String.Format("{0}\u00B0 C", Celsius);
					break;
				case TemperatureUnits.Kelvin:
					formattedValue = String.Format("{0}\u00B0 K", Kelvin);
					break;
				case TemperatureUnits.Fahrenheit:
					formattedValue = String.Format("{0}\u00B0 F", Fahrenheit);
					break;
				default:
					formattedValue = string.Empty;
					break;
			}

			return formattedValue;
		}
	}

	public enum TemperatureUnits
	{
		Celsius,
		Kelvin,
		Fahrenheit
	}
}