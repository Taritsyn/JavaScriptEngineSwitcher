using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.NiL
{
	/// <summary>
	/// NiL JS engine factory
	/// </summary>
	public sealed class NiLJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the NiL JS engine
		/// </summary>
		private readonly NiLSettings _settings;


		/// <summary>
		/// Constructs an instance of the NiL JS engine factory
		/// </summary>
		public NiLJsEngineFactory()
			: this(new NiLSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the NiL JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the NiL JS engine</param>
		public NiLJsEngineFactory(NiLSettings settings)
		{
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		public string EngineName
		{
			get { return NiLJsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the NiL JS engine
		/// </summary>
		/// <returns>Instance of the NiL JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new NiLJsEngine(_settings);
		}

		#endregion
	}
}