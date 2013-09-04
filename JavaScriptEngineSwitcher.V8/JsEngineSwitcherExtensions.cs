namespace JavaScriptEngineSwitcher.V8
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
		/// Configuration settings of V8 JavaScript engine
		/// </summary>
		private static readonly Lazy<V8Configuration> _v8Config =
			new Lazy<V8Configuration>(() => (V8Configuration)ConfigurationManager.GetSection("jsEngineSwitcher/v8"));

		/// <summary>
		/// Gets a V8 JavaScript engine configuration settings
		/// </summary>
		/// <param name="switcher">JavaScript engine switcher</param>>
		/// <returns>Configuration settings of V8 JavaScript engine</returns>
		public static V8Configuration GetV8Configuration(this JsEngineSwitcher switcher)
		{
			return _v8Config.Value;
		}
	}
}