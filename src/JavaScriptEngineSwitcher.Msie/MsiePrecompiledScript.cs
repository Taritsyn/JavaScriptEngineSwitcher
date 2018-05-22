using OriginalPrecompiledScript = MsieJavaScriptEngine.PrecompiledScript;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Msie
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the MSIE JS engine
	/// </summary>
	internal sealed class MsiePrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Gets a original pre-compiled script
		/// </summary>
		public OriginalPrecompiledScript PrecompiledScript
		{
			get;
			private set;
		}


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="precompiledScript">The original pre-compiled script</param>
		public MsiePrecompiledScript(OriginalPrecompiledScript precompiledScript)
		{
			PrecompiledScript = precompiledScript;
		}


		#region IPrecompiledScript implementation

		/// <summary>
		/// Gets a name of JS engine for which the pre-compiled script was created
		/// </summary>
		public string EngineName
		{
			get { return MsieJsEngine.EngineName; }
		}

		#endregion
	}
}