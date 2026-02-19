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
		/// Adds a default instance of the JS engine switcher to the <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <returns>Instance of the <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			var options = new JsEngineSwitcherOptions();

			IJsEngineSwitcher engineSwitcher = CreateJsEngineSwitcher(options);
			ApplyOptionsToJsEngineSwitcher(engineSwitcher, options);
			RegisterJsEngineSwitcher(services, engineSwitcher, options);

			return engineSwitcher.EngineFactories;
		}

		/// <summary>
		/// Adds a specified instance of the JS engine switcher to the <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="engineSwitcher">Instance of the JS engine switcher</param>
		/// <returns>Instance of the <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			IJsEngineSwitcher engineSwitcher)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (engineSwitcher is null)
			{
				throw new ArgumentNullException(nameof(engineSwitcher));
			}

			var options = new JsEngineSwitcherOptions();

			IJsEngineSwitcher currentEngineSwitcher = GetJsEngineSwitcher(engineSwitcher, options);
			ApplyOptionsToJsEngineSwitcher(currentEngineSwitcher, options);
			RegisterJsEngineSwitcher(services, currentEngineSwitcher, options);

			return currentEngineSwitcher.EngineFactories;
		}

		/// <summary>
		/// Adds a default instance of the JS engine switcher to the <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="configure">The <see cref="JsEngineSwitcherOptions"/> which need to be configured</param>
		/// <returns>Instance of the <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			Action<JsEngineSwitcherOptions> configure)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (configure is null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var options = new JsEngineSwitcherOptions();
			configure(options);

			IJsEngineSwitcher engineSwitcher = CreateJsEngineSwitcher(options);
			ApplyOptionsToJsEngineSwitcher(engineSwitcher, options);
			RegisterJsEngineSwitcher(services, engineSwitcher, options);

			return engineSwitcher.EngineFactories;
		}

		/// <summary>
		/// Adds a specified instance of the JS engine switcher to <see cref="IServiceCollection"/>
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="engineSwitcher">Instance of the JS engine switcher</param>
		/// <param name="configure">The <see cref="JsEngineSwitcherOptions"/> which need to be configured</param>
		/// <returns>Instance of the <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddJsEngineSwitcher(this IServiceCollection services,
			IJsEngineSwitcher engineSwitcher, Action<JsEngineSwitcherOptions> configure)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (engineSwitcher is null)
			{
				throw new ArgumentNullException(nameof(engineSwitcher));
			}

			if (configure is null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			var options = new JsEngineSwitcherOptions();
			configure(options);

			IJsEngineSwitcher currentEngineSwitcher = GetJsEngineSwitcher(engineSwitcher, options);
			ApplyOptionsToJsEngineSwitcher(currentEngineSwitcher, options);
			RegisterJsEngineSwitcher(services, currentEngineSwitcher, options);

			return currentEngineSwitcher.EngineFactories;
		}

		#region Helper methods

		/// <summary>
		/// Creates an instance of the JS engine switcher
		/// </summary>
		/// <param name="options">Options of the JS engine switcher</param>
		/// <returns>Instance of the JS engine switcher</returns>
		private static IJsEngineSwitcher CreateJsEngineSwitcher(JsEngineSwitcherOptions options)
		{
			IJsEngineSwitcher engineSwitcher = options.AllowCurrentProperty ?
				JsEngineSwitcher.Current
				:
				new JsEngineSwitcher()
				;

			return engineSwitcher;
		}

		/// <summary>
		/// Gets a instance of the JS engine switcher
		/// </summary>
		/// <param name="engineSwitcher">Instance of the JS engine switcher</param>
		/// <param name="options">Options of the JS engine switcher</param>
		/// <returns>Current instance of the JS engine switcher</returns>
		private static IJsEngineSwitcher GetJsEngineSwitcher(IJsEngineSwitcher engineSwitcher, JsEngineSwitcherOptions options)
		{
			IJsEngineSwitcher currentEngineSwitcher = options.AllowCurrentProperty ?
				JsEngineSwitcher.Current
				:
				engineSwitcher
				;

			return currentEngineSwitcher;
		}

		/// <summary>
		/// Applies a options to the JS engine switcher
		/// </summary>
		/// <param name="engineSwitcher">Instance of the JS engine switcher</param>
		/// <param name="options">Options of the JS engine switcher</param>
		private static void ApplyOptionsToJsEngineSwitcher(IJsEngineSwitcher engineSwitcher, JsEngineSwitcherOptions options)
		{
			JsEngineSwitcher.AllowCurrentProperty = options.AllowCurrentProperty;
			engineSwitcher.DefaultEngineName = options.DefaultEngineName;
		}

		/// <summary>
		/// Registers a instance of the JS engine switcher
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="engineSwitcher">Instance of the JS engine switcher</param>
		/// <param name="options">Options of the JS engine switcher</param>
		private static void RegisterJsEngineSwitcher(IServiceCollection services, IJsEngineSwitcher engineSwitcher,
			JsEngineSwitcherOptions options)
		{
			// Register the current instance of JS engine switcher as a service
			services.AddSingleton(engineSwitcher);

			// Set the current instance of JS engine switcher
			if (options.AllowCurrentProperty)
			{
				JsEngineSwitcher.Current = engineSwitcher;
			}
		}

		#endregion
	}
}