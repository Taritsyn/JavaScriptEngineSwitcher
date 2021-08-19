using System;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Settings of the Jurassic JS engine
	/// </summary>
	public sealed class JurassicSettings
	{
		/// <summary>
		/// Gets or sets a flag for whether to enable conversion of host collections,
		/// that are passed or returned to script code, to script arrays
		/// </summary>
		/// <remarks>
		/// <para>This property does not allow the embedding of host collections by
		/// using a <see cref="JavaScriptEngineSwitcher.Core.IJsEngine.EmbedHostObject"/>
		/// method, it only affects the internal mechanisms of the Jurassic library.</para>
		/// </remarks>
		public bool EnableHostCollectionsEmbeddingByValue
		{
			get;
			set;
		}
#if !NETSTANDARD2_0

		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a generation of debug information)
		/// </summary>
		[Obsolete("Since the Jurassic version 3.2.1, debugging is no longer supported.")]
		public bool EnableDebugging
		{
			get;
			set;
		}
#endif

		/// <summary>
		/// Gets or sets a flag for whether to disassemble any generated IL
		/// and store it in the associated function
		/// </summary>
		public bool EnableIlAnalysis
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to allow run the script in strict mode
		/// </summary>
		public bool StrictMode
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the Jurassic settings
		/// </summary>
		public JurassicSettings()
		{
			EnableHostCollectionsEmbeddingByValue = false;
			EnableIlAnalysis = false;
			StrictMode = false;
		}
	}
}