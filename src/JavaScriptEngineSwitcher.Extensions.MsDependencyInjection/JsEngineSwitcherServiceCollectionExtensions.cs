using System;

using Microsoft.Extensions.DependencyInjection;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Extensions.MsDependencyInjection
{
	/// <summary>
	/// Extension methods for adding the JS engine switcher in an <see cref="IServiceCollection"/>
	/// </summary>
	public static class JsEngineSwitcherServiceCollectionExtensions
	{
		/// <summary>
		/// Adds a default instance of JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services)
		{
			return AddJsEngineSwitcher(services, (IJsEngineSwitcher)null);
		}

		/// <summary>
		/// Adds a specified instance of JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="engineSwitcher">Instance of JS engine switcher</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			IJsEngineSwitcher engineSwitcher)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			// Set the current instance of JS engine switcher
			JsEngineSwitcher.Current = engineSwitcher;

			// Get the current instance of JS engine switcher
			IJsEngineSwitcher currentEngineSwitcher = JsEngineSwitcher.Current;

			// Register the current instance of JS engine switcher as a service
			services.AddSingleton(currentEngineSwitcher);

			return currentEngineSwitcher.EngineFactories;
		}

		/// <summary>
		/// Adds a default instance of JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="configure">The <see cref="IJsEngineSwitcher"/> which need to be configured</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			Action<IJsEngineSwitcher> configure)
		{
			return AddJsEngineSwitcher(services, null, configure);
		}

		/// <summary>
		/// Adds a specified instance of JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="engineSwitcher">Instance of JS engine switcher</param>
		/// <param name="configure">The <see cref="IJsEngineSwitcher"/> which need to be configured</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			IJsEngineSwitcher engineSwitcher, Action<IJsEngineSwitcher> configure)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			// Set the current instance of JS engine switcher
			JsEngineSwitcher.Current = engineSwitcher;

			// Get and configure the current instance of JS engine switcher
			IJsEngineSwitcher currentEngineSwitcher = JsEngineSwitcher.Current;
			configure(currentEngineSwitcher);

			// Register the current instance of JS engine switcher as a service
			services.AddSingleton(currentEngineSwitcher);

			return currentEngineSwitcher.EngineFactories;
		}
	}
}