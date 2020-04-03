using OriginalParsedScript = Esprima.Ast.Script;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the Jint JS engine
	/// </summary>
	internal sealed class JintPrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Gets a parsed script
		/// </summary>
		public OriginalParsedScript ParsedScript
		{
			get;
			private set;
		}


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="parsedScript">The parsed script</param>
		public JintPrecompiledScript(OriginalParsedScript parsedScript)
		{
			ParsedScript = parsedScript;
		}


		#region IPrecompiledScript implementation

		/// <summary>
		/// Gets a name of JS engine for which the pre-compiled script was created
		/// </summary>
		public string EngineName
		{
			get { return JintJsEngine.EngineName; }
		}

		#endregion
	}
}