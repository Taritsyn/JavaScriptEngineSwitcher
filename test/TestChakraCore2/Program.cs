using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using JavaScriptEngineSwitcher.ChakraCore;

namespace TestChakraCore2
{
	class Program
	{
		/// <summary>
		/// Name of the file containing library for transliteration of Russian
		/// </summary>
		private const string LibraryFileName = "russian-translit.js";

		/// <summary>
		/// Name of transliterate function
		/// </summary>
		private const string FunctionName = "transliterate";

		/// <summary>
		/// Number of transliterated items
		/// </summary>
		private const int ItemCount = 7;

		/// <summary>
		/// Code of library for transliteration of Russian
		/// </summary>
		private static string _libraryCode;

		/// <summary>
		/// List of transliteration types
		/// </summary>
		private static string[] _inputTypes;

		/// <summary>
		/// List of input strings
		/// </summary>
		private static string[] _inputStrings;


		static void Main(string[] args)
		{
			PopulateTestData();
			bool withPrecompilation = Convert.ToBoolean(args[0]);

			Console.WriteLine("withPrecompilation = {0}", withPrecompilation);
			Console.WriteLine();

			Func<IJsEngine> createJsEngine = () => new ChakraCoreJsEngine();
			TransliterateStrings(createJsEngine, withPrecompilation);

			Console.ReadLine();
		}


		/// <summary>
		/// Populates a test data
		/// </summary>
		public static void PopulateTestData()
		{
			_libraryCode = Utils.GetResourceAsString(
				$"Resources.{LibraryFileName}", typeof(Program));
			_inputTypes = new string[ItemCount]
			{
				"basic", "letters-numbers", "gost-16876-71", "gost-7-79-2000", "police", "foreign-passport",
				"yandex-friendly-url"
			};
			_inputStrings = new string[ItemCount]
			{
				"SOLID — мнемонический акроним, введённый Майклом Фэзерсом для первых пяти принципов, названных " +
				"Робертом Мартином в начале 2000-х, которые означали пять основных принципов объектно-ориентированного " +
				"программирования и проектирования.",

				"Принцип единственной ответственности (The Single Responsibility Principle). " +
				"Каждый класс выполняет лишь одну задачу.",

				"Принцип открытости/закрытости (The Open Closed Principle). " +
				"«программные сущности … должны быть открыты для расширения, но закрыты для модификации.»",

				"Принцип подстановки Барбары Лисков (The Liskov Substitution Principle). " +
				"«объекты в программе должны быть заменяемыми на экземпляры их подтипов без изменения правильности выполнения программы.»",

				"Принцип разделения интерфейса (The Interface Segregation Principle). " +
				"«много интерфейсов, специально предназначенных для клиентов, лучше, чем один интерфейс общего назначения.»",

				"Принцип инверсии зависимостей (The Dependency Inversion Principle). " +
				"«Зависимость на Абстракциях. Нет зависимости на что-то конкретное.»",

				"SOLID (объектно-ориентированное программирование)"
			};
		}

		/// <summary>
		/// Transliterates a strings
		/// </summary>
		/// <param name="createJsEngine">Delegate for create an instance of the JS engine</param>
		/// <param name="withPrecompilation">Flag for whether to allow execution of JS code with pre-compilation</param>
		private static void TransliterateStrings(Func<IJsEngine> createJsEngine, bool withPrecompilation)
		{
			// Arrange
			string[] outputStrings = new string[ItemCount];
			IPrecompiledScript precompiledCode = null;

			// Act
			using (var jsEngine = createJsEngine())
			{
				if (withPrecompilation)
				{
					if (!jsEngine.SupportsScriptPrecompilation)
					{
						throw new NotSupportedException($"{jsEngine.Name} does not support precompilation.");
					}

					precompiledCode = jsEngine.Precompile(_libraryCode, LibraryFileName);
					jsEngine.Execute(precompiledCode);
				}
				else
				{
					jsEngine.Execute(_libraryCode, LibraryFileName);
				}

				outputStrings[0] = jsEngine.CallFunction<string>(FunctionName, _inputStrings[0], _inputTypes[0]);
			}

			for (int itemIndex = 1; itemIndex < ItemCount; itemIndex++)
			{
				using (var jsEngine = createJsEngine())
				{
					if (withPrecompilation)
					{
						jsEngine.Execute(precompiledCode);
					}
					else
					{
						jsEngine.Execute(_libraryCode, LibraryFileName);
					}
					outputStrings[itemIndex] = jsEngine.CallFunction<string>(FunctionName, _inputStrings[itemIndex],
						_inputTypes[itemIndex]);
				}
			}

			// Assert
			for (int itemIndex = 0; itemIndex < ItemCount; itemIndex++)
			{
				Console.WriteLine(outputStrings[itemIndex]);
				Console.WriteLine();
			}
		}
	}
}