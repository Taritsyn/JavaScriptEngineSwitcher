#if NETCOREAPP
using System.Text;

#endif
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
#if !NET452
using JavaScriptEngineSwitcher.Jint;
#endif
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;
#if !NET452
using JavaScriptEngineSwitcher.Node;
#endif
#if NETFULL || NETCOREAPP3_1 || NET5_0
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
#if !NET452
					.AddJint()
#endif
					.AddJurassic()
					.AddMsie(new MsieSettings
					{
						EngineMode = JsEngineMode.ChakraIeJsRt
					})
					.AddNiL()
#if !NET452
					.AddNode()
#endif
#if NETFULL || NETCOREAPP3_1 || NET5_0
					.AddV8()
#endif
					.AddVroom()
					;
			}
		}
	}
}