using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.Vroom;
#if !NETCOREAPP1_0
using JavaScriptEngineSwitcher.Jurassic;
#endif
#if !NETCOREAPP
using JavaScriptEngineSwitcher.V8;
#endif

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
					.AddMsie(new MsieSettings
					{
						EngineMode = JsEngineMode.ChakraIeJsRt
					})
					.AddVroom()
#if !NETCOREAPP1_0
					.AddJurassic()
#endif
#if !NETCOREAPP
					.AddV8()
#endif
					;
			}
		}
	}
}