using System;

using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Node
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			source.Add(new NodeJsEngineFactory());

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="service">Node JS service</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			INodeJSService service)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			source.Add(new NodeJsEngineFactory(service));

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="services">The services available in the application</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			IServiceCollection services)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			source.Add(new NodeJsEngineFactory(services));

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="configure">The delegate to configure the provided <see cref="NodeSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			Action<NodeSettings> configure)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new NodeSettings();
			configure(settings);

			source.Add(new NodeJsEngineFactory(settings));

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="settings">Settings of the Node JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			NodeSettings settings)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new NodeJsEngineFactory(settings));

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="service">Node JS service</param>
		/// <param name="configure">The delegate to configure the provided <see cref="NodeSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			INodeJSService service, Action<NodeSettings> configure)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new NodeSettings();
			configure(settings);

			source.Add(new NodeJsEngineFactory(service, settings));

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="service">Node JS service</param>
		/// <param name="settings">Settings of the Node JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			INodeJSService service, NodeSettings settings)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new NodeJsEngineFactory(service, settings));

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="services">The services available in the application</param>
		/// <param name="configure">The delegate to configure the provided <see cref="NodeSettings"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			IServiceCollection services, Action<NodeSettings> configure)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var settings = new NodeSettings();
			configure(settings);

			source.Add(new NodeJsEngineFactory(services, settings));

			return source;
		}

		/// <summary>
		/// Adds a instance of <see cref="NodeJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <param name="services">The services available in the application</param>
		/// <param name="settings">Settings of the Node JS engine</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddNode(this JsEngineFactoryCollection source,
			IServiceCollection services, NodeSettings settings)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			source.Add(new NodeJsEngineFactory(services, settings));

			return source;
		}
	}
}