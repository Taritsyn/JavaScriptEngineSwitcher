using System;
using System.IO;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
#if NET48 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.NiL;
#endif
using JavaScriptEngineSwitcher.Node;
#if NET48 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.V8;
#endif
using JavaScriptEngineSwitcher.Vroom;
#if NET48 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.Yantra;
#endif

namespace JavaScriptEngineSwitcher.Benchmarks
{
	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Declared)]
	public class JsExecutionHeavyBenchmark
	{
		/// <summary>
		/// Name of the file containing library for template rendering
		/// </summary>
		private const string LibraryFileName = "bundle.min.js";

		/// <summary>
		/// Name of template rendering function
		/// </summary>
		private const string FunctionName = "renderTemplate";

		/// <summary>
		/// Code of library for template rendering
		/// </summary>
		private static string _libraryCode;

		/// <summary>
		/// List of items
		/// </summary>
		private static ContentItem[] _contentItems = new[] {
			new ContentItem("hello-world"),
			new ContentItem("contacts"),
			new ContentItem("js-engines"),
			new ContentItem("web-browser-family-tree")
		};


		/// <summary>
		/// Static constructor
		/// </summary>
		static JsExecutionHeavyBenchmark()
		{
			PopulateTestData();
		}


		public static string GetAbsoluteDirectoryPath(string directoryPath)
		{
			string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
			string absoluteDirectoryPath = Path.GetFullPath(Path.Combine(baseDirectoryPath, directoryPath));
#if NETCOREAPP

			if (!Directory.Exists(absoluteDirectoryPath))
			{
				absoluteDirectoryPath = Path.GetFullPath(
					Path.Combine(baseDirectoryPath, "../../../../", directoryPath));
			}
#endif

			return absoluteDirectoryPath;
		}

		/// <summary>
		/// Populates a test data
		/// </summary>
		public static void PopulateTestData()
		{
			string filesDirectoryPath = GetAbsoluteDirectoryPath("Files/template-rendering");
			string librariesDirectoryPath = Path.Combine(filesDirectoryPath, "lib");
			string contentDirectoryPath = Path.Combine(filesDirectoryPath, "content");

			_libraryCode = File.ReadAllText(Path.Combine(librariesDirectoryPath, LibraryFileName));

			foreach (ContentItem item in _contentItems)
			{
				string itemDirectoryPath = Path.Combine(contentDirectoryPath, item.Name);

				item.TemplateCode = File.ReadAllText(Path.Combine(itemDirectoryPath, "template.handlebars"));
				item.SerializedData = File.ReadAllText(Path.Combine(itemDirectoryPath, "data.json"));
				item.TargetOutput = File.ReadAllText(Path.Combine(itemDirectoryPath, "target-output.html"));
			}
		}

		/// <summary>
		/// Render a templates
		/// </summary>
		/// <param name="createJsEngine">Delegate for create an instance of the JS engine</param>
		/// <param name="withPrecompilation">Flag for whether to allow execution of JS code with pre-compilation</param>
		private static void RenderTemplates(Func<IJsEngine> createJsEngine, bool withPrecompilation)
		{
			// Arrange
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

				_contentItems[0].Output = jsEngine.CallFunction<string>(FunctionName, _contentItems[0].TemplateCode,
					_contentItems[0].SerializedData);
			}

			for (int itemIndex = 1; itemIndex < _contentItems.Length; itemIndex++)
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
					_contentItems[itemIndex].Output = jsEngine.CallFunction<string>(FunctionName,
						_contentItems[itemIndex].TemplateCode, _contentItems[itemIndex].SerializedData);
				}
			}

			// Assert
			foreach (ContentItem item in _contentItems)
			{
				Assert.Equal(item.TargetOutput, item.Output, true);
			}
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void ChakraCore(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new ChakraCoreJsEngine();
			RenderTemplates(createJsEngine, withPrecompilation);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void Jint(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new JintJsEngine();
			RenderTemplates(createJsEngine, withPrecompilation);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void Jurassic(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new JurassicJsEngine();
			RenderTemplates(createJsEngine, withPrecompilation);
		}
#if NET48

		[Benchmark]
		public void MsieClassic()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.Classic,
				UseJson2Library = true
			});
			RenderTemplates(createJsEngine, false);
		}

		[Benchmark]
		public void MsieChakraActiveScript()
		{
			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraActiveScript
			});
			RenderTemplates(createJsEngine, false);
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
			RenderTemplates(createJsEngine, withPrecompilation);
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
			RenderTemplates(createJsEngine, withPrecompilation);
		}
#if NET48 || NETCOREAPP3_1_OR_GREATER

		[Benchmark]
		public void NiL()
		{
			Func<IJsEngine> createJsEngine = () => new NiLJsEngine();
			RenderTemplates(createJsEngine, false);
		}
#endif

		[Benchmark]
		public void Node()
		{
			Func<IJsEngine> createJsEngine = () => new NodeJsEngine();
			RenderTemplates(createJsEngine, false);
		}
#if NET48 || NETCOREAPP3_1_OR_GREATER

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void V8(bool withPrecompilation)
		{
			Func<IJsEngine> createJsEngine = () => new V8JsEngine();
			RenderTemplates(createJsEngine, withPrecompilation);
		}
#endif

		[Benchmark]
		public void Vroom()
		{
			Func<IJsEngine> createJsEngine = () => new VroomJsEngine();
			RenderTemplates(createJsEngine, false);
		}
#if NET48 || NETCOREAPP3_1_OR_GREATER

		[Benchmark]
		public void Yantra()
		{
			Func<IJsEngine> createJsEngine = () => new YantraJsEngine();
			RenderTemplates(createJsEngine, false);
		}
#endif

		#region Internal types

		private sealed class ContentItem
		{
			public string Name
			{
				get;
				set;
			}

			public string TemplateCode
			{
				get;
				set;
			}

			public string SerializedData
			{
				get;
				set;
			}

			public string TargetOutput
			{
				get;
				set;
			}

			public string Output
			{
				get;
				set;
			}


			public ContentItem(string name)
			{
				Name = name;
			}
		}

		#endregion
	}
}