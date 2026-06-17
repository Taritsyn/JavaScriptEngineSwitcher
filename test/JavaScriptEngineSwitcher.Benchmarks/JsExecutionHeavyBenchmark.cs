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
using JavaScriptEngineSwitcher.NiL;
using JavaScriptEngineSwitcher.Node;
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Vroom;
using JavaScriptEngineSwitcher.Yantra;

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
		private static ContentItem[] _contentItems = [
			new ContentItem("hello-world"),
			new ContentItem("contacts"),
			new ContentItem("js-engines"),
			new ContentItem("web-browser-family-tree")
		];


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
		/// <param name="jsEngineFactory">JS engine factory</param>
		/// <param name="precompileScript">Flag for whether to allow execution of JS code with pre-compilation</param>
		private static void RenderTemplates(IJsEngineFactory jsEngineFactory, bool precompileScript)
		{
			// Arrange
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

				_contentItems[0].Output = jsEngine.CallFunction<string>(FunctionName, _contentItems[0].TemplateCode,
					_contentItems[0].SerializedData);
			}

			for (int itemIndex = 1; itemIndex < _contentItems.Length; itemIndex++)
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
		public void ChakraCore(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new ChakraCoreJsEngineFactory();
			RenderTemplates(jsEngineFactory, precompileScript);
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
			RenderTemplates(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void Jurassic(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new JurassicJsEngineFactory();
			RenderTemplates(jsEngineFactory, precompileScript);
		}
#if NET462

		[Benchmark]
		public void MsieClassic()
		{
			IJsEngineFactory jsEngineFactory = new MsieJsEngineFactory(new MsieSettings
			{
				EngineMode = JsEngineMode.Classic,
				UseEcmaScript5Polyfill = true,
				UseJson2Library = true
			});
			RenderTemplates(jsEngineFactory, false);
		}

		[Benchmark]
		public void MsieChakraActiveScript()
		{
			IJsEngineFactory jsEngineFactory = new MsieJsEngineFactory(new MsieSettings
			{
				EngineMode = JsEngineMode.ChakraActiveScript
			});
			RenderTemplates(jsEngineFactory, false);
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
			RenderTemplates(jsEngineFactory, precompileScript);
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
			RenderTemplates(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		public void NiL()
		{
			IJsEngineFactory jsEngineFactory = new NiLJsEngineFactory();
			RenderTemplates(jsEngineFactory, false);
		}

		[Benchmark]
		public void Node()
		{
			IJsEngineFactory jsEngineFactory = new NodeJsEngineFactory();
			RenderTemplates(jsEngineFactory, false);
		}

		[Benchmark]
		[Arguments(false)]
		[Arguments(true)]
		public void V8(bool precompileScript)
		{
			IJsEngineFactory jsEngineFactory = new V8JsEngineFactory();
			RenderTemplates(jsEngineFactory, precompileScript);
		}

		[Benchmark]
		public void Vroom()
		{
			IJsEngineFactory jsEngineFactory = new VroomJsEngineFactory();
			RenderTemplates(jsEngineFactory, false);
		}

		[Benchmark]
		public void Yantra()
		{
			IJsEngineFactory jsEngineFactory = new YantraJsEngineFactory();
			RenderTemplates(jsEngineFactory, false);
		}

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