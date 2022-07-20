using System;
using System.IO;
using System.Text;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;

namespace TemplateRendering
{
	public class Program
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
			//new ContentItem("hello-world"),
			//new ContentItem("js-engines"),
			//new ContentItem("contacts"),
			new ContentItem("web-browser-family-tree")
		};


		/// <summary>
		/// Static constructor
		/// </summary>
		static Program()
		{
			PopulateTestData();
		}


		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;

			Func<IJsEngine> createJsEngine = () => new MsieJsEngine(new MsieSettings
			{
				EngineMode = JsEngineMode.Classic,
				UseJson2Library = true
			});
			//Func<IJsEngine> createJsEngine = () => new NiLJsEngine();
			RenderTemplates(createJsEngine, false);
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

			_libraryCode = File.ReadAllText(Path.Combine(librariesDirectoryPath, "bundle.min.js"));

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
				Console.WriteLine(item.Output);
				Console.WriteLine();
				Console.WriteLine();
			}
		}


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
	}
}