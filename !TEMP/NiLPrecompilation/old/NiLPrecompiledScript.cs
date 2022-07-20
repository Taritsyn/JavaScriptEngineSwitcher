using JavaScriptEngineSwitcher.Core;

using OriginalScript = NiL.JS.Script;

namespace JavaScriptEngineSwitcher.NiL
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the NiL JS engine
	/// </summary>
	internal sealed class NiLPrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Gets a parsed script
		/// </summary>
		public OriginalScript ParsedScript
		{
			get;
			private set;
		}


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="parsedScript">The parsed script</param>
		public NiLPrecompiledScript(OriginalScript parsedScript)
		{
			ParsedScript = parsedScript;
		}


		#region IPrecompiledScript implementation

		/// <summary>
		/// Gets a name of JS engine for which the pre-compiled script was created
		/// </summary>
		public string EngineName
		{
			get { return NiLJsEngine.EngineName; }
		}

		#endregion
	}
}