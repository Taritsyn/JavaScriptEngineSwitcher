namespace JavaScriptEngineSwitcher.Tests.Vroom
{
	public class CommonTests : CommonTestsBase
	{
		protected override string EngineName
		{
			get { return "VroomJsEngine"; }
		}


		#region Evaluation of code

		public override void EvaluationOfExpressionWithUndefinedResultIsCorrect()
		{ }

		#endregion

		#region Calling of functions

		public override void CallingOfFunctionWithUndefinedResultIsCorrect()
		{ }

		#endregion

		#region Getting, setting and removing variables

		public override void SettingAndGettingVariableWithUndefinedValueIsCorrect()
		{ }

		#endregion
	}
}