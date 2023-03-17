using System;

using IOriginalEngine = Tenray.Topaz.ITopazEngine;

namespace JavaScriptEngineSwitcher.Topaz
{
	/// <summary>
	/// Settings of the Topaz JS engine
	/// </summary>
	public sealed class TopazSettings
	{
		/// <summary>
		/// Gets or sets a delegate invoked to initialize a built-in Javascript objects
		/// </summary>
		/// <remarks>
		/// When this property is set to <c>null</c>, the global scope remains empty.
		/// </remarks>
		public Action<IOriginalEngine> BuiltinObjectsInitializer;


		/// <summary>
		/// Constructs an instance of the Topaz settings
		/// </summary>
		public TopazSettings()
		{
			BuiltinObjectsInitializer = DefaultBuiltinObjectsInitializer.Initialize;
		}
	}
}