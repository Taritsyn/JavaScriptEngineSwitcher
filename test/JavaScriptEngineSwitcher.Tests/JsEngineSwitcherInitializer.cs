#if NETCOREAPP
using System.Text;

#endif
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
#if !NETCOREAPP1_0
using JavaScriptEngineSwitcher.Jint;
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
#if NETCOREAPP
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#endif
				JsEngineSwitcher.Current.EngineFactories
					.AddChakraCore()
#if !NETCOREAPP1_0
					.AddJint()
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