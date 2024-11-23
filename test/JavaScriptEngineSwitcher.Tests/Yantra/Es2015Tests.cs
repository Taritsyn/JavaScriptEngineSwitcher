using Xunit;

namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class Es2015Tests : Es2015TestsBase
	{
		protected override string EngineName
		{
			get { return "YantraJsEngine"; }
		}


		#region Promises

		[Fact]
		public override void SupportsPromises()
		{ }

		#endregion
	}
}