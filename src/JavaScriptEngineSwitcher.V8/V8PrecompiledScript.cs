using OriginalCacheKind = Microsoft.ClearScript.V8.V8CacheKind;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the V8 JS engine
	/// </summary>
	internal sealed class V8PrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Gets a source code of the script
		/// </summary>
		public string Code
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a kind of cache data to be generated
		/// </summary>
		public OriginalCacheKind CacheKind
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a cached data for accelerated recompilation
		/// </summary>
		public byte[] CachedBytes
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a document name
		/// </summary>
		public string DocumentName
		{
			get;
			private set;
		}


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="code">The source code of the script</param>
		/// <param name="cacheKind">The kind of cache data to be generated</param>
		/// <param name="cachedBytes">Cached data for accelerated recompilation</param>
		/// <param name="documentName">Document name</param>
		public V8PrecompiledScript(string code, OriginalCacheKind cacheKind, byte[] cachedBytes,
			string documentName)
		{
			Code = code;
			CacheKind = cacheKind;
			CachedBytes = cachedBytes;
			DocumentName = documentName;
		}


		#region IPrecompiledScript implementation

		/// <inheritdoc/>
		public string EngineName
		{
			get { return V8JsEngine.EngineName; }
		}

		#endregion
	}
}