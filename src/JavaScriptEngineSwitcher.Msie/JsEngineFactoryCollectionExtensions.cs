using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Msie
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="MsieJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddMsie(this JsEngineFactoryCollection source)
		{
			return source.AddMsie(new MsieSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="MsieJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="settings">Settings of the MSIE JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddMsie(this JsEngineFactoryCollection source,
			MsieSettings settings)
		{
			source.Add(new MsieJsEngineFactory(settings));

			return source;
		}
	}
}