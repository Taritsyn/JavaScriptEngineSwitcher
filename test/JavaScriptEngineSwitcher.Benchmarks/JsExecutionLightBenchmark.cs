using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;
using JavaScriptEngineSwitcher.Node;
#if NET461 || NETCOREAPP3_1 || NET5_0
using JavaScriptEngineSwitcher.V8;
#endif
using JavaScriptEngineSwitcher.Vroom;

namespace JavaScriptEngineSwitcher.Benchmarks
{
	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Declared)]
	public class JsExecutionLightBenchmark
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

		/// <summary>
		/// List of target output strings
		/// </summary>
		private static string[] _targetOutputStrings;


		/// <summary>
		/// Static constructor
		/// </summary>
		static JsExecutionLightBenchmark()
		{
			PopulateTestData();
		}


		/// <summary>
		/// Populates a test data
		/// </summary>
		public static void PopulateTestData()
		{
			_libraryCode = Utils.GetResourceAsString(
				$"Resources.{LibraryFileName}", typeof(JsExecutionLightBenchmark));
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
			_targetOutputStrings = new string[ItemCount]
			{
				"SOLID — mnemonicheskij akronim, vvedjonnyj Majklom Fjezersom dlja pervyh pjati principov, nazvannyh " +
				"Robertom Martinom v nachale 2000-h, kotorye oznachali pjat' osnovnyh principov ob#ektno-orientirovannogo " +
				"programmirovanija i proektirovanija.",

				"Princip edinstvennoj otvetstvennosti (The Single Responsibility Principle). " +
				"Ka#dyj klass vypolnjaet li6' odnu zada4u.",

				"Princip otkrytosti/zakrytosti (The Open Closed Principle). " +
				"«programmnye sushhnosti … dolzhny byt' otkryty dlja rasshirenija, no zakryty dlja modifikacii.»",

				"Princip podstanovki Barbary Liskov (The Liskov Substitution Principle). " +
				"«ob\"ekty v programme dolzhny byt' zamenyaemymi na e'kzemplyary ix podtipov bez izmeneniya pravil'nosti " +
				"vypolneniya programmy.»",

				"Printsip razdeleniia interfeisa (The Interface Segregation Principle). " +
				"«mnogo interfeisov, spetsialno prednaznachennykh dlia klientov, luchshe, chem odin interfeis obshchego " +
				"naznacheniia.»",

				"Printcip inversii zavisimostei (The Dependency Inversion Principle). " +
				"«Zavisimost na Abstraktciiakh. Net zavisimosti na chto-to konkretnoe.»",

				"solid-obektno-orientirovannoe-programmirovanie"
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
				Assert.Equal(_targetOutputStrings[itemIndex], outputStrings[itemIndex]);
			}
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void ChakraCore(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new ChakraCoreJsEngine();
			TransliterateStrings(createJsEngine, withPrecompilation);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void Jint(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new JintJsEngine();
			TransliterateStrings(createJsEngine, withPrecompilation);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void Jurassic(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new JurassicJsEngine();
			TransliterateStrings(createJsEngine, withPrecompilation);
		}
#if NET461

		[Benchmark]
		public void MsieClassic()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.Classic
			});
			TransliterateStrings(createJsEngine, false);
		}

		[Benchmark]
		public void MsieChakraActiveScript()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraActiveScript
			});
			TransliterateStrings(createJsEngine, false);
		}
#endif
		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void MsieChakraIeJsRt(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraIeJsRt
			});
			TransliterateStrings(createJsEngine, withPrecompilation);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void MsieChakraEdgeJsRt(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraEdgeJsRt
			});
			TransliterateStrings(createJsEngine, withPrecompilation);
		}

		[Benchmark]
		public void NiL()
		{
			Func<IJsEngine> createJsEngine = () => new NiLJsEngine();
			TransliterateStrings(createJsEngine, false);
		}

		[Benchmark]
		public void Node()
		{
			Func<IJsEngine> createJsEngine = () => new NodeJsEngine();
			TransliterateStrings(createJsEngine, false);
		}
#if NET461 || NETCOREAPP3_1 || NET5_0

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void V8(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new V8JsEngine();
			TransliterateStrings(createJsEngine, withPrecompilation);
		}
#endif

		[Benchmark]
		public void Vroom()
		{
			Func<IJsEngine> createJsEngine = () => new VroomJsEngine();
			TransliterateStrings(createJsEngine, false);
		}
	}
}