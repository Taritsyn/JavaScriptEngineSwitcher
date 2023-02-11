using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Vroom
{
	/// <summary>
	/// Vroom JS engine factory
	/// </summary>
	public sealed class VroomJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// Settings of the Vroom JS engine
		/// </summary>
		private readonly VroomSettings _settings;


		/// <summary>
		/// Constructs an instance of the Vroom JS engine factory
		/// </summary>
		public VroomJsEngineFactory()
			: this(new VroomSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Vroom JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the Vroom JS engine</param>
		public VroomJsEngineFactory(VroomSettings settings)
		{
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <inheritdoc/>
		public string EngineName
		{
			get { return VroomJsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the Vroom JS engine
		/// </summary>
		/// <returns>Instance of the Vroom JS engine</returns>
		public IJsEngine CreateEngine()
		{
			return new VroomJsEngine(_settings);
		}

		#endregion
	}
}