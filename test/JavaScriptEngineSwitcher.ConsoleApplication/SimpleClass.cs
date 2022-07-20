using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaScriptEngineSwitcher.ConsoleApplication
{
	public class SimpleClass
	{
		public string Get(byte d)
		{
			return d.ToString();
		}

		public double Sum(int a, int b, int c)
		{
			return a + b + c;
		}

		public double Sum(int a, int b, double c)
		{
			return a + b + c;
		}

		public double Sum(int a, double b, double c)
		{
			return a + b + c;
		}

		public double Sum(double a, double b, double c)
		{
			return a + b + c;
		}

		public double Sum(int a, byte b, byte c)
		{
			return a + b + c;
		}
	}
}
