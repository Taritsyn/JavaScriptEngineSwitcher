using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
#if !NETCOREAPP1_0
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Msie;
#endif
#if NET40
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
#if !NETCOREAPP1_0
							.AddJint()
							.AddMsie()
#endif
#if NET40
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