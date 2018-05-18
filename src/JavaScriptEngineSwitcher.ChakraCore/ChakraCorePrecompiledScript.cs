using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the ChakraCore JS engine
	/// </summary>
	internal sealed class ChakraCorePrecompiledScript : IPrecompiledScript
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
		/// <param name="cachedBytes">Cached data for accelerated recompilation</param>
		/// <param name="documentName">Document name</param>
		public ChakraCorePrecompiledScript(string code, byte[] cachedBytes, string documentName)
		{
			Code = code;
			CachedBytes = cachedBytes;
			DocumentName = documentName;
		}


		#region IPrecompiledScript implementation

		/// <summary>
		/// Gets a name of JS engine for which the pre-compiled script was created
		/// </summary>
		public string EngineName
		{
			get { return ChakraCoreJsEngine.EngineName; }
		}

		#endregion
	}
}