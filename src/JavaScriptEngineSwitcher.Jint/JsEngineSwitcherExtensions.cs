namespace JavaScriptEngineSwitcher.Jint
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
		/// Configuration settings of Jint JavaScript engine
		/// </summary>
		private static readonly Lazy<JintConfiguration> _jintConfig =
			new Lazy<JintConfiguration>(() => (JintConfiguration)ConfigurationManager.GetSection("jsEngineSwitcher/jint"));

		/// <summary>
		/// Gets a Jint JavaScript engine configuration settings
		/// </summary>
		/// <param name="switcher">JavaScript engine switcher</param>>
		/// <returns>Configuration settings of Jint JavaScript engine</returns>
		public static JintConfiguration GetJintConfiguration(this JsEngineSwitcher switcher)
		{
			return _jintConfig.Value;
		}
	}
}