using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
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
				.AddJint()
				.AddJurassic()
				.AddMsie(new MsieSettings
				{
					EngineMode = JsEngineMode.ChakraIeJsRt,
					UseEcmaScript5Polyfill = true,
					UseJson2Library = true
				})
				.AddVroom()
				;
			engineSwitcher.DefaultEngineName = ChakraCoreJsEngine.EngineName;
		}
	}
}