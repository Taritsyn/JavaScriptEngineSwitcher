using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Node
{
	/// <summary>
	/// Node JS engine factory
	/// </summary>
	public sealed class NodeJsEngineFactory : IJsEngineFactory
	{
		/// <summary>
		/// The services available in the application
		/// </summary>
		private IServiceCollection _services;

		/// <summary>
		/// Node JS service
		/// </summary>
		private INodeJSService _jsService;

		/// <summary>
		/// Settings of the Node JS engine
		/// </summary>
		private readonly NodeSettings _settings;

		/// <summary>
		/// Synchronizer of Node JS service creation
		/// </summary>
		private readonly object _creationSynchronizer = new object();


		/// <summary>
		/// Constructs an instance of the Node JS engine factory
		/// </summary>
		public NodeJsEngineFactory()
			: this(new NodeSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Node JS engine factory
		/// </summary>
		/// <param name="service">Node JS service</param>
		public NodeJsEngineFactory(INodeJSService service)
			: this(service, new NodeSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Node JS engine factory
		/// </summary>
		/// <param name="services">The services available in the application</param>
		public NodeJsEngineFactory(IServiceCollection services)
			: this(services, new NodeSettings())
		{ }

		/// <summary>
		/// Constructs an instance of the Node JS engine factory
		/// </summary>
		/// <param name="settings">Settings of the Node JS engine</param>
		public NodeJsEngineFactory(NodeSettings settings)
		{
			_settings = settings;
		}

		/// <summary>
		/// Constructs an instance of the Node JS engine factory
		/// </summary>
		/// <param name="service">Node JS service</param>
		/// <param name="settings">Settings of the Node JS engine</param>
		public NodeJsEngineFactory(INodeJSService service, NodeSettings settings)
		{
			_jsService = service;
			_settings = settings;
		}

		/// <summary>
		/// Constructs an instance of the Node JS engine factory
		/// </summary>
		/// <param name="services">The services available in the application</param>
		/// <param name="settings">Settings of the Node JS engine</param>
		public NodeJsEngineFactory(IServiceCollection services, NodeSettings settings)
		{
			_services = services;
			_settings = settings;
		}


		#region IJsEngineFactory implementation

		/// <inheritdoc/>
		public string EngineName
		{
			get { return NodeJsEngine.EngineName; }
		}


		/// <summary>
		/// Creates a instance of the Node JS engine
		/// </summary>
		/// <returns>Instance of the Node JS engine</returns>
		public IJsEngine CreateEngine()
		{
			if (_services != null && _jsService == null)
			{
				lock (_creationSynchronizer)
				{
					if (_jsService == null)
					{
						ServiceProvider serviceProvider = _services.BuildServiceProvider();
						_jsService = serviceProvider.GetRequiredService<INodeJSService>();
					}
				}
			}

			IJsEngine engine = _jsService != null ?
				new NodeJsEngine(_jsService, _settings) : new NodeJsEngine(_settings);

			return engine;
		}

		#endregion
	}
}