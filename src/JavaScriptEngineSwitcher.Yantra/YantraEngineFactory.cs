using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Yantra
{
	/// <summary>
	/// Yantra JS engine factory
	/// </summary>
	public sealed class YantraJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the Yantra JS engine
		/// </summary>
		private readonly YantraSettings _settings;


		/// <summary>
		/// Constructs an instance of the Yantra JS engine factory
		/// </summary>
		public YantraJsEngineFactory()
			: this(new YantraSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Yantra JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the Yantra JS engine</param>
		public YantraJsEngineFactory(YantraSettings settings)
		{
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <inheritdoc/>
		public string EngineName
		{
			get { return YantraJsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the Yantra JS engine
		/// </summary>
		/// <returns>Instance of the Yantra JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new YantraJsEngine(_settings);
		}

		#endregion
	}
}