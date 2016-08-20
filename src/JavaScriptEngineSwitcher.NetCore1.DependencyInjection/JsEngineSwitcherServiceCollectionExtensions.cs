using System;

using Microsoft.Extensions.DependencyInjection;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.NetCore1.DependencyInjection
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
				throw new ArgumentNullException("services");
			}

			JsEngineSwitcher engineSwitcher = JsEngineSwitcher.Instance;
			services.AddSingleton(engineSwitcher);

			return engineSwitcher.EngineFactories;
		}

		/// <summary>
		/// Adds a JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="configure">The <see cref="JsEngineSwitcher"/> which need to be configured</param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			Action<JsEngineSwitcher> configure)
		{
			if (services == null)
			{
				throw new ArgumentNullException("services");
			}

			if (configure == null)
			{
				throw new ArgumentNullException("configure");
			}

			JsEngineSwitcher engineSwitcher = JsEngineSwitcher.Instance;
			configure(engineSwitcher);

			services.AddSingleton(engineSwitcher);

			return engineSwitcher.EngineFactories;
		}
	}
}