using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.V8;

namespace JavaScriptEngineSwitcher.Sample.AspNet4.Mvc4
{
	public class JsEngineSwitcherConfig
	{
		public static void Configure(JsEngineSwitcher engineSwitcher)
		{
			engineSwitcher.EngineFactories
				.AddChakraCore()
				.AddJint()
				.AddJurassic()
				.AddMsie(new MsieSettings
				{
					UseEcmaScript5Polyfill = true,
					UseJson2Library = true
				})
				.AddV8()
				;
			engineSwitcher.DefaultEngineName = ChakraCoreJsEngine.EngineName;
		}
	}
}