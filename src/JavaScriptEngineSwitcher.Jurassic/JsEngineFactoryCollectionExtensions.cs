using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="JurassicJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddJurassic(this JsEngineFactoryCollection source)
		{
			return source.AddJurassic(new JurassicSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="JurassicJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="settings">Settings of the Jurassic JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddJurassic(this JsEngineFactoryCollection source,
			JurassicSettings settings)
		{
			source.Add(new JurassicJsEngineFactory(settings));

			return source;
		}
	}
}