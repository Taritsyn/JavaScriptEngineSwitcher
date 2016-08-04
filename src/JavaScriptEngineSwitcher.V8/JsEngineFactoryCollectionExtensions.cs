using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="V8JsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddV8(this JsEngineFactoryCollection source)
		{
			return source.AddV8(new V8Settings());
		}

		/// <summary>
		/// Adds a instance of <see cref="V8JsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="settings">Settings of the V8 JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddV8(this JsEngineFactoryCollection source,
			V8Settings settings)
		{
			source.Add(new V8JsEngineFactory(settings));

			return source;
		}
	}
}