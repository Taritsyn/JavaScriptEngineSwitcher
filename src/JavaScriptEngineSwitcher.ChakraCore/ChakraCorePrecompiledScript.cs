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
		/// Gets a source code of the script
		/// </summary>
		public string Code
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a attribute mask for parsing the script
		/// </summary>
		public JsParseScriptAttributes ParseAttributes
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
		/// Gets a callback to load the source code of the serialized script
		/// </summary>
		public JsSerializedLoadScriptCallback LoadScriptSourceCodeCallback
		{
			get;
			private set;
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
			Code = code;
			ParseAttributes = parseAttributes;
			CachedBytes = cachedBytes;
			DocumentName = documentName;
			LoadScriptSourceCodeCallback = LoadScriptSourceCode;
		}


		/// <summary>
		/// Loads a source code of the serialized script
		/// </summary>
		/// <param name="sourceContext">A cookie identifying the script that can be used
		/// by debuggable script contexts</param>
		/// <param name="value">The script returned</param>
		/// <param name="parseAttributes">Attribute mask for parsing the script</param>
		/// <returns>true if the operation succeeded, false otherwise</returns>
		private bool LoadScriptSourceCode(JsSourceContext sourceContext,
			out JsValue value, out JsParseScriptAttributes parseAttributes)
		{
			bool result;
			parseAttributes = ParseAttributes;
			Encoding encoding = parseAttributes.HasFlag(JsParseScriptAttributes.ArrayBufferIsUtf16Encoded) ?
				Encoding.Unicode : Encoding.UTF8;

			try
			{
				value = JsValue.CreateExternalArrayBuffer(Code, encoding);
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