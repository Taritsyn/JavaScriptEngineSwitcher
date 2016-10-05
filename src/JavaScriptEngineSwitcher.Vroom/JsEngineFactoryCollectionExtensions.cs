using System;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Vroom
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="VroomJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddVroom(this JsEngineFactoryCollection source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.AddVroom(new VroomSettings());
		}

		/// <summary>
		/// Adds a instance of <see cref="VroomJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="configure">The delegate to configure the provided <see cref="VroomSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddVroom(this JsEngineFactoryCollection source,
			Action<VroomSettings> configure)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (configure == null)
			{
				throw new ArgumentNullException("configure");
			}

			var settings = new VroomSettings();
			configure(settings);

			return source.AddVroom(settings);
		}

		/// <summary>
		/// Adds a instance of <see cref="VroomJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="settings">Settings of the Vroom JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddVroom(this JsEngineFactoryCollection source,
			VroomSettings settings)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			source.Add(new VroomJsEngineFactory(settings));

			return source;
		}
	}
}