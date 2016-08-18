using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// ChakraCore JS engine factory
	/// </summary>
	public sealed class ChakraCoreJsEngineFactory : IJsEngineFactory
	{
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
			return new ChakraCoreJsEngine();
		}
	}
}