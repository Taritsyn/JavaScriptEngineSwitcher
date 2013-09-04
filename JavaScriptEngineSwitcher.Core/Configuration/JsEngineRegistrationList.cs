namespace JavaScriptEngineSwitcher.Core.Configuration
{
	using System.Configuration;

	/// <summary>
	/// List of registered JavaScript engines
	/// </summary>
	public sealed class JsEngineRegistrationList : ConfigurationElementCollection
	{
		/// <summary>
		/// Creates a new JavaScript engine registration
		/// </summary>
		/// <returns>JavaScript engine registration</returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new JsEngineRegistration();
		}

		/// <summary>
		/// Gets a key of the specified JavaScript engine registration
		/// </summary>
		/// <param name="element">JavaScript engine registration</param>
		/// <returns>Key</returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((JsEngineRegistration)element).Name;
		}

		/// <summary>
		/// Gets a JavaScript engine registration by JavaScript engine name
		/// </summary>
		/// <param name="name">JavaScript engine name</param>
		/// <returns>JavaScript engine registration</returns>
		public new JsEngineRegistration this[string name]
		{
			get { return (JsEngineRegistration)BaseGet(name); }
		}
	}
}