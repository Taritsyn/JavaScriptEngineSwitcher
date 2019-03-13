namespace JavaScriptEngineSwitcher.Tests.Msie
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "MsieJsEngine"; }
		}


		#region Embedding of objects

		#region Delegates

#if !NETCOREAPP
		public override void EmbeddedInstanceOfDelegateHasFunctionPrototype()
		{ }
#endif

		#endregion

		#endregion
	}
}