using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.Vroom;

namespace JavaScriptEngineSwitcher.Sample.AspNet4.Mvc4
{
	public class JsEngineSwitcherConfig
	{
		public static void Configure(IJsEngineSwitcher engineSwitcher)
		{
			engineSwitcher.EngineFactories
				.AddChakraCore()
				.AddJurassic()
				.AddMsie(new MsieSettings
				{
					EngineMode = JsEngineMode.ChakraIeJsRt
				})
				.AddVroom()
				;
			engineSwitcher.DefaultEngineName = ChakraCoreJsEngine.EngineName;
		}
	}
}