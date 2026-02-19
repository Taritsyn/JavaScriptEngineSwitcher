using System;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="ChakraCoreJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddChakraCore(this JsEngineFactoryCollection source)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.AddChakraCore(new ChakraCoreSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="ChakraCoreJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="configure">The delegate to configure the provided <see cref="ChakraCoreSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddChakraCore(this JsEngineFactoryCollection source,
			Action<ChakraCoreSettings> configure)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (configure is null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new ChakraCoreSettings();
			configure(settings);

			return source.AddChakraCore(settings);
		}

		/// <summary>
		/// Adds a instance of <see cref="ChakraCoreJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <param name="settings">Settings of the ChakraCore JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddChakraCore(this JsEngineFactoryCollection source, ChakraCoreSettings settings)
		{
			if (source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new ChakraCoreJsEngineFactory(settings));

			return source;
		}
	}
}