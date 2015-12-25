namespace JavaScriptEngineSwitcher.Core
{
	using System;
	using System.Configuration;

	using Configuration;
	using Resources;
	using Utilities;

	/// <summary>
	/// JavaScript engine switcher
	/// </summary>
	public sealed class JsEngineSwitcher
	{
		/// <summary>
		/// Instance of JavaScript engine switcher
		/// </summary>
		private static readonly Lazy<JsEngineSwitcher> _instance =
			new Lazy<JsEngineSwitcher>(() => new JsEngineSwitcher());

		/// <summary>
		/// Configuration settings of core
		/// </summary>
		private readonly Lazy<CoreConfiguration> _coreConfig =
			new Lazy<CoreConfiguration>(() =>
				(CoreConfiguration)ConfigurationManager.GetSection("jsEngineSwitcher/core"));

		/// <summary>
		/// Gets a instance of JavaScript engine switcher
		/// </summary>
		public static JsEngineSwitcher Current
		{
			get { return _instance.Value; }
		}


		/// <summary>
		/// Private constructor for implementation Singleton pattern
		/// </summary>
		private JsEngineSwitcher()
		{ }


		/// <summary>
		/// Creates a instance of JavaScript engine
		/// </summary>
		/// <param name="name">JavaScript engine name</param>
		/// <returns>JavaScript engine</returns>
		public IJsEngine CreateJsEngineInstance(string name)
		{
			IJsEngine jsEngine;
			JsEngineRegistrationList jsEngineRegistrationList = _coreConfig.Value.Engines;
			JsEngineRegistration jsEngineRegistration = jsEngineRegistrationList[name];

			if (jsEngineRegistration != null)
			{
				jsEngine = Utils.CreateInstanceByFullTypeName<IJsEngine>(jsEngineRegistration.Type);
			}
			else
			{
				throw new JsEngineNotFoundException(
					string.Format(Strings.Configuration_JsEngineNotRegistered, name));
			}

			return jsEngine;
		}

		/// <summary>
		/// Creates a instance of default JavaScript engine based on the settings
		/// that specified in configuration files (App.config or Web.config)
		/// </summary>
		/// <returns>JavaScript engine</returns>
		public IJsEngine CreateDefaultJsEngineInstance()
		{
			string defaultJsEngineName = _coreConfig.Value.DefaultEngine;
			if (string.IsNullOrWhiteSpace(defaultJsEngineName))
			{
				throw new ConfigurationErrorsException(Strings.Configuration_DefaultJsEngineNotSpecified);
			}

			IJsEngine jsEngine = CreateJsEngineInstance(defaultJsEngineName);

			return jsEngine;
		}
	}
}