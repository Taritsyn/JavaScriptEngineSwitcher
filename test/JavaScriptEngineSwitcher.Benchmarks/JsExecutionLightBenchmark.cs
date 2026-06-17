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
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Vroom;
using JavaScriptEngineSwitcher.Yantra;

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
			_inputTypes =
			[
				"basic", "letters-numbers", "gost-16876-71", "gost-7-79-2000", "police", "foreign-passport",
				"yandex-friendly-url"
			];
			_inputStrings =
			[
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
			];
			_targetOutputStrings =
			[
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
			];
		}

		/// <summary>
		/// Transliterates a strings
		/// </summary>
		/// <param name="jsEngineFactory">JS engine factory</param>
		/// <param name="precompileScript">Flag for whether to allow execution of JS code with pre-compilation</param>
		private static void TransliterateStrings(IJsEngineFactory jsEngineFactory, bool precompileScript)
		{
			// Arrange
			string[] outputStrings = new string[ItemCount];
			IPrecompiledScript precompiledCode = null;

			// Act
			using (var jsEngine = jsEngineFactory.CreateEngine())
			{
				if (precompileScript)
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
				using (var jsEngine = jsEngineFactory.CreateEngine())
				{
					if (precompileScript)
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
		public void ChakraCore(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new ChakraCoreJsEngineFactory();
			TransliterateStrings(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		[Arguments(false, false)]
		[Arguments(true, false)]
		[Arguments(true, true)]
		public void Jint(bool precompileScript, bool compileRegex)
		{
			IJsEngineFactory jsEngineFactory = new JintJsEngineFactory(new JintSettings
			{
				CompileRegex = compileRegex
			});
			TransliterateStrings(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void Jurassic(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new JurassicJsEngineFactory();
			TransliterateStrings(jsEngineFactory, precompileScript);
		}
#if NET462

		[Benchmark]
		public void MsieClassic()
		{
			IJsEngineFactory jsEngineFactory = new MsieJsEngineFactory(new MsieSettings
			{
				EngineMode = JsEngineMode.Classic
			});
			TransliterateStrings(jsEngineFactory, false);
		}

		[Benchmark]
		public void MsieChakraActiveScript()
		{
			IJsEngineFactory jsEngineFactory = new MsieJsEngineFactory(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraActiveScript
			});
			TransliterateStrings(jsEngineFactory, false);
		}
#endif
		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void MsieChakraIeJsRt(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new MsieJsEngineFactory(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraIeJsRt
			});
			TransliterateStrings(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void MsieChakraEdgeJsRt(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new MsieJsEngineFactory(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraEdgeJsRt
			});
			TransliterateStrings(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		public void NiL()
		{
			IJsEngineFactory jsEngineFactory = new NiLJsEngineFactory();
			TransliterateStrings(jsEngineFactory, false);
		}

		[Benchmark]
		public void Node()
		{
			IJsEngineFactory jsEngineFactory = new NodeJsEngineFactory();
			TransliterateStrings(jsEngineFactory, false);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void V8(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new V8JsEngineFactory();
			TransliterateStrings(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		public void Vroom()
		{
			IJsEngineFactory jsEngineFactory = new VroomJsEngineFactory();
			TransliterateStrings(jsEngineFactory, false);
		}

		[Benchmark]
		public void Yantra()
		{
			IJsEngineFactory jsEngineFactory = new YantraJsEngineFactory();
			TransliterateStrings(jsEngineFactory, false);
		}
	}
}