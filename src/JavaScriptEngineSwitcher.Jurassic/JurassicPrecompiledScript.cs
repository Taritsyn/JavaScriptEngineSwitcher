using OriginalCompiledScript = Jurassic.CompiledScript;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the Jurassic JS engine
	/// </summary>
	internal sealed class JurassicPrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Gets a compiled script
		/// </summary>
		public OriginalCompiledScript CompiledScript
		{
			get;
			private set;
		}


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="compiledScript">The compiled script</param>
		public JurassicPrecompiledScript(OriginalCompiledScript compiledScript)
		{
			CompiledScript = compiledScript;
		}


		#region IPrecompiledScript implementation

		/// <summary>
		/// Gets a name of JS engine for which the pre-compiled script was created
		/// </summary>
		public string EngineName
		{
			get { return JurassicJsEngine.EngineName; }
		}

		#endregion
	}
}