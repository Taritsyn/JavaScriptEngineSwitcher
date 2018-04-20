using System;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.NiL
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="NiLJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNiL(this JsEngineFactoryCollection source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.AddNiL(new NiLSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="NiLJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="configure">The delegate to configure the provided <see cref="NiLSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNiL(this JsEngineFactoryCollection source,
			Action<NiLSettings> configure)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new NiLSettings();
			configure(settings);

			return source.AddNiL(settings);
		}

		/// <summary>
		/// Adds a instance of <see cref="NiLJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="settings">Settings of the NiL JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNiL(this JsEngineFactoryCollection source, NiLSettings settings)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new NiLJsEngineFactory(settings));

			return source;
		}
	}
}