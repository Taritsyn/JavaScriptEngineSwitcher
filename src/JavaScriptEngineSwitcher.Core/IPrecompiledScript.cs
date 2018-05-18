namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the JS engine
	/// </summary>
	public interface IPrecompiledScript
	{
		/// <summary>
		/// Gets a name of JS engine for which the pre-compiled script was created
		/// </summary>
		string EngineName { get; }
	}
}