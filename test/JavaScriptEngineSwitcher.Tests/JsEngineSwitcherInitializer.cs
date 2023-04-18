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
#if NET471 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.NiL;
using JavaScriptEngineSwitcher.Node;
#endif
#if NETFRAMEWORK || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.V8;
#endif
using JavaScriptEngineSwitcher.Vroom;
#if NET471 || NETCOREAPP3_1_OR_GREATER
using JavaScriptEngineSwitcher.Yantra;
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
#if NET471 || NETCOREAPP3_1_OR_GREATER
					.AddNiL()
					.AddNode()
#endif
#if NETFRAMEWORK || NETCOREAPP3_1_OR_GREATER
					.AddV8()
#endif
					.AddVroom()
#if NET471 || NETCOREAPP3_1_OR_GREATER
					.AddYantra()
#endif
					;
			}
		}
	}
}