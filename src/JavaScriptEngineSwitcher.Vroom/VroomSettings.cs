namespace JavaScriptEngineSwitcher.Vroom
{
	/// <summary>
	/// Settings of the Vroom JS engine
	/// </summary>
	public sealed class VroomSettings
	{
		/// <summary>
		/// Gets or sets a maximum size of the young object heap
		/// </summary>
		public int MaxYoungSpaceSize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size of the old object heap
		/// </summary>
		public int MaxOldSpaceSize
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs instance of the Vroom settings
		/// </summary>
		public VroomSettings()
		{
			MaxYoungSpaceSize = -1;
			MaxOldSpaceSize = -1;
		}
	}
}