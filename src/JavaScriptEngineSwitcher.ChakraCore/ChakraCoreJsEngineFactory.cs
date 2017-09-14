using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// ChakraCore JS engine factory
	/// </summary>
	public sealed class ChakraCoreJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the ChakraCore JS engine
		/// </summary>
		private readonly ChakraCoreSettings _settings;


		/// <summary>
		/// Constructs an instance of the ChakraCore JS engine factory
		/// </summary>
		public ChakraCoreJsEngineFactory()
			: this(new ChakraCoreSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the ChakraCore JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the ChakraCore JS engine</param>
		public ChakraCoreJsEngineFactory(ChakraCoreSettings settings)
		{
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		public string EngineName
		{
			get { return ChakraCoreJsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the ChakraCore JS engine
		/// </summary>
		/// <returns>Instance of the ChakraCore JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new ChakraCoreJsEngine(_settings);
		}

		#endregion
	}
}