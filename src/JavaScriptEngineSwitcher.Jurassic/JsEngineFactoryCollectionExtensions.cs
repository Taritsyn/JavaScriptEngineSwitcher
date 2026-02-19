using System;

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
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJurassic(this JsEngineFactoryCollection source)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.AddJurassic(new JurassicSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="JurassicJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="configure">The delegate to configure the provided <see cref="JurassicSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJurassic(this JsEngineFactoryCollection source,
			Action<JurassicSettings> configure)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (configure is null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new JurassicSettings();
			configure(settings);

			return source.AddJurassic(settings);
		}

		/// <summary>
		/// Adds a instance of <see cref="JurassicJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="settings">Settings of the Jurassic JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJurassic(this JsEngineFactoryCollection source,
			JurassicSettings settings)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new JurassicJsEngineFactory(settings));

			return source;
		}
	}
}