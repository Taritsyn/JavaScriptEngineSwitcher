#if !NETCOREAPP
namespace JavaScriptEngineSwitcher.Tests.V8
{
	public class InteropTests : InteropTestsBase
	{
		protected override string EngineName
		{
			get { return "V8JsEngine"; }
		}


		#region Embedding of objects

		#region Delegates

		public override void EmbeddedInstanceOfDelegateHasFunctionPrototype()
		{ }

		#endregion

		#endregion
	}
}
#endif