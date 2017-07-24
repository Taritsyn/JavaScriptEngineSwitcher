namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// Defines a interface of JS engine switcher
	/// </summary>
	public interface IJsEngineSwitcher
	{
		/// <summary>
		/// Gets or sets a name of default JS engine
		/// </summary>
		string DefaultEngineName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a collection of JS engine factories
		/// </summary>
		JsEngineFactoryCollection EngineFactories
		{
			get;
		}


		/// <summary>
		/// Creates a instance of JS engine
		/// </summary>
		/// <param name="name">JS engine name</param>
		/// <returns>JS engine</returns>
		IJsEngine CreateEngine(string name);

		/// <summary>
		/// Creates a instance of default JS engine
		/// </summary>
		/// <returns>JS engine</returns>
		IJsEngine CreateDefaultEngine();
	}
}