using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Jint JS engine factory
	/// </summary>
	public sealed class JintJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the Jint JS engine
		/// </summary>
		private readonly JintSettings _settings;


		/// <summary>
		/// Constructs an instance of the Jint JS engine factory
		/// </summary>
		public JintJsEngineFactory()
			: this(new JintSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Jint JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the Jint JS engine</param>
		public JintJsEngineFactory(JintSettings settings)
		{
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <summary>
		/// Gets a name of JS engine
		/// </summary>
		public string EngineName
		{
			get { return JintJsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the Jint JS engine
		/// </summary>
		/// <returns>Instance of the Jint JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new JintJsEngine(_settings);
		}

		#endregion
	}
}