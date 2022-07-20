using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Jering.Javascript.NodeJS;

namespace JavaScriptEngineSwitcher.Node
{
	/// <summary>
	/// Default Node JS service
	/// </summary>
	/// <remarks>
	/// Wrapper around the <see cref="StaticNodeJSService"/> class.
	/// </remarks>
	public sealed class DefaultNodeJsService : INodeJSService
	{
		/// <summary>
		/// Instance of default Node JS service
		/// </summary>
		private static readonly DefaultNodeJsService _instance = new DefaultNodeJsService();

		/// <summary>
		/// Gets a instance of default Node JS service
		/// </summary>
		public static INodeJSService Instance
		{
			get { return _instance; }
		}


		/// <summary>
		/// Private constructor for implementation Singleton pattern
		/// </summary>
		private DefaultNodeJsService()
		{ }


		#region INodeJSService implementation

		public Task<T> InvokeFromFileAsync<T>(string modulePath, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromFileAsync<T>(modulePath, exportName, args, cancellationToken);
		}

		public Task InvokeFromFileAsync(string modulePath, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromFileAsync(modulePath, exportName, args, cancellationToken);
		}

		public Task<T> InvokeFromStringAsync<T>(string moduleString, string cacheIdentifier = null, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStringAsync<T>(moduleString, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task InvokeFromStringAsync(string moduleString, string cacheIdentifier = null, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStringAsync(moduleString, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task<T> InvokeFromStringAsync<T>(Func<string> moduleFactory, string cacheIdentifier, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStringAsync<T>(moduleFactory, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task InvokeFromStringAsync(Func<string> moduleFactory, string cacheIdentifier, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStringAsync(moduleFactory, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task<T> InvokeFromStreamAsync<T>(Stream moduleStream, string cacheIdentifier = null, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStreamAsync<T>(moduleStream, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task InvokeFromStreamAsync(Stream moduleStream, string cacheIdentifier = null, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStreamAsync(moduleStream, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task<T> InvokeFromStreamAsync<T>(Func<Stream> moduleFactory, string cacheIdentifier, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStreamAsync<T>(moduleFactory, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task InvokeFromStreamAsync(Func<Stream> moduleFactory, string cacheIdentifier, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.InvokeFromStreamAsync(moduleFactory, cacheIdentifier, exportName, args, cancellationToken);
		}

		public Task<(bool, T)> TryInvokeFromCacheAsync<T>(string moduleCacheIdentifier, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.TryInvokeFromCacheAsync<T>(moduleCacheIdentifier, exportName, args, cancellationToken);
		}

		public Task<bool> TryInvokeFromCacheAsync(string moduleCacheIdentifier, string exportName = null, object[] args = null, CancellationToken cancellationToken = default)
		{
			return StaticNodeJSService.TryInvokeFromCacheAsync(moduleCacheIdentifier, exportName, args, cancellationToken);
		}

		public void MoveToNewProcess()
		{
			throw new NotSupportedException();
		}

		#region IDisposable implementation

		public void Dispose()
		{
			throw new NotSupportedException();
		}

		#endregion

		#endregion
	}
}