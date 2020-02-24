using System;

namespace JavaScriptEngineSwitcher.Node
{
	/// <summary>
	/// Settings of the Node JS engine
	/// </summary>
	public sealed class NodeSettings
	{
		/// <summary>
		/// Gets or sets a interval to wait before the script execution times out
		/// </summary>
		public TimeSpan TimeoutInterval
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to use the Node.js built-in library
		/// </summary>
		public bool UseBuiltinLibrary
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the Node settings
		/// </summary>
		public NodeSettings()
		{
			TimeoutInterval = TimeSpan.Zero;
			UseBuiltinLibrary = false;
		}
	}
}