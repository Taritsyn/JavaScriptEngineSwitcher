#if NETCOREAPP
using System.Text;

#endif
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Core.Utilities;
#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1
using JavaScriptEngineSwitcher.Jint;
#endif
#if !NETCOREAPP1_0
using JavaScriptEngineSwitcher.Jurassic;
#endif
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.NiL;
#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1
using JavaScriptEngineSwitcher.Node;
#endif
#if NETFULL || NETCOREAPP3_1
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
#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1
					.AddJint()
#endif
#if !NETCOREAPP1_0
					.AddJurassic()
#endif
					.AddMsie(new MsieSettings
					{
						EngineMode = JsEngineMode.ChakraIeJsRt
					})
					.AddNiL()
#if NET471 || NETCOREAPP2_1 || NETCOREAPP3_1
					.AddNode()
#endif
#if NETFULL || NETCOREAPP3_1
					.AddV8()
#endif
					.AddVroom()
					;
			}
		}
	}
}