using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;
#if !NETCOREAPP1_0
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.V8;
#endif

namespace JavaScriptEngineSwitcher.Tests
{
	internal static class JsEngineSwitcherInitializer
	{
		private static readonly object _synchronizer = new object();
		private static bool _initialized;


		public static void Initialize()
		{
			if (!_initialized)
			{
				lock (_synchronizer)
				{
					if (!_initialized)
					{
						JsEngineSwitcher.Instance.EngineFactories
							.AddChakraCore()
							.AddMsie()
#if !NETCOREAPP1_0
							.AddJint()
							.AddJurassic()
							.AddV8()
#endif
							;

						_initialized = true;
					}
				}
			}
		}
	}
}