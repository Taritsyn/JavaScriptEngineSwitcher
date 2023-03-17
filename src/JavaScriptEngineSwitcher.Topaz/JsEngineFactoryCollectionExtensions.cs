using System;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Topaz
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="TopazJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddTopaz(this JsEngineFactoryCollection source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.AddTopaz(new TopazSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="TopazJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="configure">The delegate to configure the provided <see cref="TopazSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddTopaz(this JsEngineFactoryCollection source,
			Action<TopazSettings> configure)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new TopazSettings();
			configure(settings);

			return source.AddTopaz(settings);
		}

		/// <summary>
		/// Adds a instance of <see cref="TopazJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="settings">Settings of the Topaz JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddTopaz(this JsEngineFactoryCollection source,
			TopazSettings settings)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new TopazJsEngineFactory(settings));

			return source;
		}
	}
}