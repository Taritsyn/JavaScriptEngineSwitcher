using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="JintJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddJint(this JsEngineFactoryCollection source)
		{
			return source.AddJint(new JintSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="JintJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="settings">Settings of the Jint JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddJint(this JsEngineFactoryCollection source, JintSettings settings)
		{
			source.Add(new JintJsEngineFactory(settings));

			return source;
		}
	}
}