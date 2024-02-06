using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Extensions.MsDependencyInjection
{
	/// <summary>
	/// Options of the JS engine switcher
	/// </summary>
	public sealed class JsEngineSwitcherOptions
	{
		/// <summary>
		/// Gets or sets a flag for whether to allow usage of the <see cref="JsEngineSwitcher.Current"/> property
		/// </summary>
		/// <remarks>
		/// Required to ensure the usage of an instance of the JS engine switcher that is registered by using
		/// the <c>IServiceCollection</c> interface.
		/// </remarks>
		public bool AllowCurrentProperty
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a name of default JS engine
		/// </summary>
		public string DefaultEngineName
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the JS engine switcher options
		/// </summary>
		public JsEngineSwitcherOptions()
		{
			AllowCurrentProperty = true;
			DefaultEngineName = string.Empty;
		}
	}
}