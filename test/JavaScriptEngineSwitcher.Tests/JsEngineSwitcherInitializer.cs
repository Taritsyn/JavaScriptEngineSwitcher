using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using JavaScriptEngineSwitcher.Jint;
#if !NETCOREAPP1_0
using JavaScriptEngineSwitcher.Jurassic;
#endif
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;
#if !NETCOREAPP
using JavaScriptEngineSwitcher.V8;
#endif
using JavaScriptEngineSwitcher.Vroom;

namespace JavaScriptEngineSwitcher.Tests
{
	internal static class JsEngineSwitcherInitializer
	{
		private static InterlockedStatedFlag _initializedFlag = new InterlockedStatedFlag();


		public static void Initialize()
		{
			if (_initializedFlag.Set())
			{
				JsEngineSwitcher.Current.EngineFactories
					.AddChakraCore()
					.AddJint()
#if !NETCOREAPP1_0
					.AddJurassic()
#endif
					.AddMsie(new MsieSettings
					{
						EngineMode = JsEngineMode.ChakraIeJsRt
					})
					.AddNiL()
#if !NETCOREAPP
					.AddV8()
#endif
					.AddVroom()
					;
			}
		}
	}
}