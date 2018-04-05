using System;

using Microsoft.Extensions.DependencyInjection;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Extensions.MsDependencyInjection
{
	/// <summary>
	/// Extension methods for adding the JS engine switcher in an <see cref="IServiceCollection" />
	/// </summary>
	public static class JsEngineSwitcherServiceCollectionExtensions
	{
		/// <summary>
		/// Adds a JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;
			services.AddSingleton(engineSwitcher);

			return engineSwitcher.EngineFactories;
		}

		/// <summary>
		/// Adds a JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="configure">The <see cref="IJsEngineSwitcher"/> which need to be configured</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			Action<IJsEngineSwitcher> configure)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;
			configure(engineSwitcher);

			services.AddSingleton(engineSwitcher);

			return engineSwitcher.EngineFactories;
		}
	}
}