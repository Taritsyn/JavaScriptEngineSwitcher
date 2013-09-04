namespace JavaScriptEngineSwitcher.V8.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Configuration settings of V8 JavaScript engine
	/// </summary>
	public sealed class V8Configuration : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets a path to directory that contains the Noesis Javascript .NET assemblies
		/// </summary>
		[ConfigurationProperty("noesisJavascriptAssembliesDirectoryPath", DefaultValue = "")]
		public string NoesisJavascriptAssembliesDirectoryPath
		{
			get { return (string)this["noesisJavascriptAssembliesDirectoryPath"]; }
			set { this["noesisJavascriptAssembliesDirectoryPath"] = value; }
		}
	}
}