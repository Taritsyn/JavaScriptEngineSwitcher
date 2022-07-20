using System;
using System.Collections.Generic;
using System.Text;

namespace JavaScriptEngineSwitcher.ConsoleApplication
{
    public static class AnimalManager
    {
		public static string GetInfo(IAnimal animal)
		{
			return animal.Cry();
		}
    }
}
