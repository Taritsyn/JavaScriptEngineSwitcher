using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Jurassic JS engine factory
	/// </summary>
	public sealed class JurassicJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the Jurassic JS engine
		/// </summary>
		private readonly JurassicSettings _settings;

		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		public string EngineName
		{
			get { return "JurassicJsEngine"; }
		}


		/// <summary>
		/// Constructs an instance of the Jurassic JS engine factory
		/// </summary>
		public JurassicJsEngineFactory()
			: this(new JurassicSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Jurassic JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the Jurassic JS engine</param>
		public JurassicJsEngineFactory(JurassicSettings settings)
		{
			_settings = settings;
		}


		/// <summary>
		/// Creates a instance of the Jurassic JS engine
		/// </summary>
		/// <returns>Instance of the Jurassic JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new JurassicJsEngine(_settings);
		}
	}
}