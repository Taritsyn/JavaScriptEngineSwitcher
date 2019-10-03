using OriginalProgram = Esprima.Ast.Program;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the Jint JS engine
	/// </summary>
	internal sealed class JintPrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Gets a program
		/// </summary>
		public OriginalProgram Program
		{
			get;
			private set;
		}


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="program">The program</param>
		public JintPrecompiledScript(OriginalProgram program)
		{
			Program = program;
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