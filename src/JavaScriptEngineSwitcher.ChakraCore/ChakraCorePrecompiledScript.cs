using System.Text;

using JavaScriptEngineSwitcher.Core;

using JavaScriptEngineSwitcher.ChakraCore.JsRt;
using OriginalException = JavaScriptEngineSwitcher.ChakraCore.JsRt.JsException;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the ChakraCore JS engine
	/// </summary>
	internal sealed class ChakraCorePrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Source code of the script
		/// </summary>
		private readonly string _code;

		/// <summary>
		/// Attribute mask for parsing the script
		/// </summary>
		private readonly JsParseScriptAttributes _parseAttributes;

		/// <summary>
		/// Cached data for accelerated recompilation
		/// </summary>
		private readonly byte[] _cachedBytes;

		/// <summary>
		/// Document name
		/// </summary>
		private readonly string _documentName;

		/// <summary>
		/// Callback to load the source code of the serialized script
		/// </summary>
		private readonly JsSerializedLoadScriptCallback _loadScriptSourceCodeCallback;

		/// <summary>
		/// Source code of the script as an array of bytes
		/// </summary>
		private byte[] _codeBytes;

		/// <summary>
		/// Synchronizer of the script source code loading
		/// </summary>
		private readonly object _scriptLoadingSynchronizer = new object();

		/// <summary>
		/// Gets a source code of the script
		/// </summary>
		public string Code
		{
			get { return _code; }
		}

		/// <summary>
		/// Gets a attribute mask for parsing the script
		/// </summary>
		public JsParseScriptAttributes ParseAttributes
		{
			get { return _parseAttributes; }
		}

		/// <summary>
		/// Gets a cached data for accelerated recompilation
		/// </summary>
		public byte[] CachedBytes
		{
			get { return _cachedBytes; }
		}

		/// <summary>
		/// Gets a document name
		/// </summary>
		public string DocumentName
		{
			get { return _documentName; }
		}

		/// <summary>
		/// Gets a callback to load the source code of the serialized script
		/// </summary>
		public JsSerializedLoadScriptCallback LoadScriptSourceCodeCallback
		{
			get { return _loadScriptSourceCodeCallback; }
		}


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="code">The source code of the script</param>
		/// <param name="parseAttributes">Attribute mask for parsing the script</param>
		/// <param name="cachedBytes">Cached data for accelerated recompilation</param>
		/// <param name="documentName">Document name</param>
		public ChakraCorePrecompiledScript(string code, JsParseScriptAttributes parseAttributes, byte[] cachedBytes,
			string documentName)
		{
			_code = code;
			_parseAttributes = parseAttributes;
			_cachedBytes = cachedBytes;
			_documentName = documentName;
			_loadScriptSourceCodeCallback = LoadScriptSourceCode;
		}


		/// <summary>
		/// Loads a source code of the serialized script
		/// </summary>
		/// <param name="sourceContext">A cookie identifying the script that can be used
		/// by debuggable script contexts</param>
		/// <param name="value">The script returned</param>
		/// <param name="parseAttributes">Attribute mask for parsing the script</param>
		/// <returns>true if the operation succeeded, false otherwise</returns>
		private bool LoadScriptSourceCode(JsSourceContext sourceContext, out JsValue value,
			out JsParseScriptAttributes parseAttributes)
		{
			if (_codeBytes == null)
			{
				lock (_scriptLoadingSynchronizer)
				{
					if (_codeBytes == null)
					{
						Encoding encoding = _parseAttributes.HasFlag(JsParseScriptAttributes.ArrayBufferIsUtf16Encoded) ?
							Encoding.Unicode : Encoding.UTF8;
						_codeBytes = encoding.GetBytes(_code);
					}
				}
			}

			bool result;
			parseAttributes = _parseAttributes;

			try
			{
				value = JsValue.CreateExternalArrayBuffer(_codeBytes);
				result = true;
			}
			catch (OriginalException)
			{
				value = JsValue.Invalid;
				result = false;
			}

			return result;
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