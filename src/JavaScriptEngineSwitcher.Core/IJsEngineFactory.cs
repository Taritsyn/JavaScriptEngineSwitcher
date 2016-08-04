namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// Defines a interface of JS engine factory
	/// </summary>
	public interface IJsEngineFactory
	{
		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		string EngineName { get; }


		/// <summary>
		/// Creates a instance of JS engine
		/// </summary>
		/// <returns>Instance of JS engine</returns>
		IJsEngine CreateEngine();
	}
}