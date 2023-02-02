namespace JavaScriptEngineSwitcher.Tests.Vroom
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "VroomJsEngine"; }
		}


		#region Object methods

		public override void SupportsObjectCreateMethod()
		{ }

		#endregion
	}
}