using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Yantra
{
	/// <summary>
	/// Yantra JS engine factory
	/// </summary>
	public sealed class YantraJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Constructs an instance of the Yantra JS engine factory
		/// </summary>
		public YantraJsEngineFactory()
		{ }


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
			return new YantraJsEngine();
		}

		#endregion
	}
}