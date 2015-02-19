namespace JavaScriptEngineSwitcher.Jurassic
{
	using System;
	using System.Configuration;

	using Core;
	using Configuration;

	/// <summary>
	/// JavaScript engine switcher extensions
	/// </summary>
	public static class JsEngineSwitcherExtensions
	{
		/// <summary>
		/// Configuration settings of Jurassic JavaScript engine
		/// </summary>
		private static readonly Lazy<JurassicConfiguration> _jurassicConfig =
			new Lazy<JurassicConfiguration>(() => (JurassicConfiguration)ConfigurationManager.GetSection("jsEngineSwitcher/jurassic"));

		/// <summary>
		/// Gets a Jurassic JavaScript engine configuration settings
		/// </summary>
		/// <param name="switcher">JavaScript engine switcher</param>>
		/// <returns>Configuration settings of Jurassic JavaScript engine</returns>
		public static JurassicConfiguration GetJurassicConfiguration(this JsEngineSwitcher switcher)
		{
			return _jurassicConfig.Value;
		}
	}
}