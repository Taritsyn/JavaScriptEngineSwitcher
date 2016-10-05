using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.Vroom;
#if !NETCOREAPP1_0
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
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
				JsEngineSwitcher.Instance.EngineFactories
					.AddChakraCore()
					.AddMsie()
					.AddVroom()
#if !NETCOREAPP1_0
					.AddJint()
					.AddJurassic()
					.AddV8()
#endif
					;
			}
		}
	}
}