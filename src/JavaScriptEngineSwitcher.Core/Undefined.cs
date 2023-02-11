namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// Represents an JS <c>undefined</c> type
	/// </summary>
	public sealed class Undefined
	{
		/// <summary>
		/// Gets a one and only <c>undefined</c> instance
		/// </summary>
		public static readonly Undefined Value = new Undefined();


		private Undefined()
		{ }


		/// <summary>
		/// Returns a string that represents the current object
		/// </summary>
		/// <returns>A string that represents the current object</returns>
		public override string ToString()
		{
			return "undefined";
		}
	}
}