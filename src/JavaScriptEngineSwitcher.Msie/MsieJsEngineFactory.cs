using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Msie
{
	/// <summary>
	/// MSIE JS engine factory
	/// </summary>
	public sealed class MsieJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the MSIE JS engine
		/// </summary>
		private readonly MsieSettings _settings;

		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		public string EngineName
		{
			get { return "MsieJsEngine"; }
		}


		/// <summary>
		/// Constructs an instance of the MSIE JS engine factory
		/// </summary>
		public MsieJsEngineFactory()
			: this(new MsieSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the MSIE JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the MSIE JS engine</param>
		public MsieJsEngineFactory(MsieSettings settings)
		{
			_settings = settings;
		}


		/// <summary>
		/// Creates a instance of the MSIE JS engine
		/// </summary>
		/// <returns>Instance of the MSIE JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new MsieJsEngine(_settings);
		}
	}
}