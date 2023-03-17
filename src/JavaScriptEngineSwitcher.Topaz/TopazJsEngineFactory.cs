using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Topaz
{
	/// <summary>
	/// Topaz JS engine factory
	/// </summary>
	public sealed class TopazJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the Topaz JS engine
		/// </summary>
		private readonly TopazSettings _settings;


		/// <summary>
		/// Constructs an instance of the Topaz JS engine factory
		/// </summary>
		public TopazJsEngineFactory()
			: this(new TopazSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Topaz JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the Topaz JS engine</param>
		public TopazJsEngineFactory(TopazSettings settings)
		{
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <inheritdoc/>
		public string EngineName
		{
			get { return TopazJsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the Topaz JS engine
		/// </summary>
		/// <returns>Instance of the Topaz JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new TopazJsEngine(_settings);
		}

		#endregion
	}
}