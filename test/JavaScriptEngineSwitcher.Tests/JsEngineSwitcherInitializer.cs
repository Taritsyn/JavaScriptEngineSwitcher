#if NETCOREAPP
using System.Text;

#endif
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
#if NIL_JS
using JavaScriptEngineSwitcher.NiL;
#endif
using JavaScriptEngineSwitcher.Node;
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Vroom;
using JavaScriptEngineSwitcher.Yantra;

namespace JavaScriptEngineSwitcher.Tests
{
	internal static class JsEngineSwitcherInitializer
	{
		private static InterlockedStatedFlag _initializedFlag = new InterlockedStatedFlag();


		public static void Initialize()
		{
			if (_initializedFlag.Set())
			{
#if NETCOREAPP
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#endif
				JsEngineSwitcher.Current.EngineFactories
					.AddChakraCore()
					.AddJint()
					.AddJurassic()
					.AddMsie(new MsieSettings
					{
						EngineMode = JsEngineMode.ChakraIeJsRt
					})
#if NIL_JS
					.AddNiL()
#endif
					.AddNode()
					.AddV8()
					.AddVroom()
					.AddYantra()
					;
			}
		}
	}
}