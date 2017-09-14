using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// V8 JS engine factory
	/// </summary>
	public sealed class V8JsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the V8 JS engine
		/// </summary>
		private readonly V8Settings _settings;


		/// <summary>
		/// Constructs an instance of the V8 JS engine factory
		/// </summary>
		public V8JsEngineFactory()
			: this(new V8Settings())
		{ }

		/// <summary>
		/// Constructs an instance of the V8 JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the V8 JS engine</param>
		public V8JsEngineFactory(V8Settings settings)
		{
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		public string EngineName
		{
			get { return V8JsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the V8 JS engine
		/// </summary>
		/// <returns>Instance of the V8 JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new V8JsEngine(_settings);
		}

		#endregion
	}
}