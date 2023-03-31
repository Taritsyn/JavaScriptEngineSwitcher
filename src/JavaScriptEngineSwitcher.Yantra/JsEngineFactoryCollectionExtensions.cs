using System;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Yantra
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="YantraJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddYantra(this JsEngineFactoryCollection source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.AddYantra(new YantraSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="YantraJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="configure">The delegate to configure the provided <see cref="YantraSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddYantra(this JsEngineFactoryCollection source,
			Action<YantraSettings> configure)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new YantraSettings();
			configure(settings);

			return source.AddYantra(settings);
		}

		/// <summary>
		/// Adds a instance of <see cref="YantraJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="settings">Settings of the Yantra JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddYantra(this JsEngineFactoryCollection source, YantraSettings settings)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new YantraJsEngineFactory(settings));

			return source;
		}
	}
}