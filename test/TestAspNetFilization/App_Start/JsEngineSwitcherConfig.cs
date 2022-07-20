using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;

namespace TestAspNetFilization
{
	public class JsEngineSwitcherConfig
	{
		public static void Configure(IJsEngineSwitcher engineSwitcher)
		{
			engineSwitcher.EngineFactories
				.AddChakraCore()
				.AddMsie(new MsieSettings
				{
					EngineMode = JsEngineMode.ChakraActiveScript
				})
				;
			engineSwitcher.DefaultEngineName = MsieJsEngine.EngineName;
		}
	}
}