namespace JavaScriptEngineSwitcher.Msie
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
		/// Configuration settings of MSIE JavaScript engine
		/// </summary>
		private static readonly Lazy<MsieConfiguration> _msieConfig =
			new Lazy<MsieConfiguration>(() => (MsieConfiguration)ConfigurationManager.GetSection("jsEngineSwitcher/msie"));

		/// <summary>
		/// Gets a MSIE JavaScript engine configuration settings
		/// </summary>
		/// <param name="switcher">JavaScript engine switcher</param>>
		/// <returns>Configuration settings of MSIE JavaScript engine</returns>
		public static MsieConfiguration GetMsieConfiguration(this JsEngineSwitcher switcher)
		{
			return _msieConfig.Value;
		}
	}
}