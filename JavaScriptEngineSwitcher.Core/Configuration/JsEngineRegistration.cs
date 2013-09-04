namespace JavaScriptEngineSwitcher.Core.Configuration
{
	using System.Configuration;

	/// <summary>
	/// JavaScript engine registration
	/// </summary>
	public sealed class JsEngineRegistration : ConfigurationElement
	{
		/// <summary>
		/// Gets or sets a JavaScript engine name
		/// </summary>
		[ConfigurationProperty("name", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}

		/// <summary>
		/// Gets or sets a JavaScript engine .NET-type name
		/// </summary>
		[ConfigurationProperty("type", IsRequired = true)]
		public string Type
		{
			get { return (string)this["type"]; }
			set { this["type"] = value; }
		}
	}
}