using Xunit;

using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.ChakraCore
{
	public class EvalTests : EvalTestsBase
	{
		protected override string EngineName
		{
			get { return "ChakraCoreJsEngine"; }
		}


		private IJsEngine CreateJsEngine(bool disableEval)
		{
			var jsEngine = new ChakraCoreJsEngine(new ChakraCoreSettings
			{
				DisableEval = disableEval
			});

			return jsEngine;
		}


		public override void UsageOfEvalFunction()
		{
			// Arrange
			int TestDisableEvalSetting(bool disableEval)
			{
				using (var jsEngine = CreateJsEngine(disableEval: disableEval))
				{
					return jsEngine.Evaluate<int>("eval('2*2');");
				}
			}

			// Act and Assert
			Assert.Equal(4, TestDisableEvalSetting(false));

			JsRuntimeException exception = Assert.Throws<JsRuntimeException>(() => TestDisableEvalSetting(true));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Eval of strings is disabled in this runtime.", exception.Description);
		}

		public override void UsageOfFunctionConstructor()
		{
			// Arrange
			int TestDisableEvalSetting(bool disableEval)
			{
				using (var jsEngine = CreateJsEngine(disableEval: disableEval))
				{
					return jsEngine.Evaluate<int>("new Function('return 2*2;')();");
				}
			}

			// Act and Assert
			Assert.Equal(4, TestDisableEvalSetting(false));

			JsRuntimeException exception = Assert.Throws<JsRuntimeException>(() => TestDisableEvalSetting(true));
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Eval of strings is disabled in this runtime.", exception.Description);
		}
	}
}