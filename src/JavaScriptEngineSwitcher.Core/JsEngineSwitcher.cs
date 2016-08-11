using System;

using JavaScriptEngineSwitcher.Core.Resources;

namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// JS engine switcher
	/// </summary>
	public sealed class JsEngineSwitcher
	{
		/// <summary>
		/// Instance of JS engine switcher
		/// </summary>
		private static readonly Lazy<JsEngineSwitcher> _instance =
			new Lazy<JsEngineSwitcher>(() => new JsEngineSwitcher());

		/// <summary>
		/// Gets a instance of JS engine switcher
		/// </summary>
		public static JsEngineSwitcher Instance
		{
			get { return _instance.Value; }
		}

		/// <summary>
		/// Gets or sets a name of default JS engine
		/// </summary>
		public string DefaultEngineName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a collection of JS engine factories
		/// </summary>
		public JsEngineFactoryCollection EngineFactories
		{
			get;
			private set;
		}


		/// <summary>
		/// Private constructor for implementation Singleton pattern
		/// </summary>
		private JsEngineSwitcher()
		{
			DefaultEngineName = string.Empty;
			EngineFactories = new JsEngineFactoryCollection();
		}


		/// <summary>
		/// Creates a instance of JS engine
		/// </summary>
		/// <param name="name">JS engine name</param>
		/// <returns>JS engine</returns>
		public IJsEngine CreateEngine(string name)
		{
			IJsEngine engine;
			IJsEngineFactory engineFactory = EngineFactories.Get(name);

			if (engineFactory != null)
			{
				engine = engineFactory.CreateEngine();
			}
			else
			{
				throw new JsEngineNotFoundException(
					string.Format(Strings.Configuration_JsEngineFactoryNotFound, name));
			}

			return engine;
		}

		/// <summary>
		/// Creates a instance of default JS engine
		/// </summary>
		/// <returns>JS engine</returns>
		public IJsEngine CreateDefaultEngine()
		{
			string defaultJsEngineName = DefaultEngineName;
			if (string.IsNullOrWhiteSpace(defaultJsEngineName))
			{
				throw new EmptyValueException(Strings.Configuration_DefaultJsEngineNameNotSpecified);
			}

			IJsEngine jsEngine = CreateEngine(defaultJsEngineName);

			return jsEngine;
		}
	}
}