using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.V8;

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
							.AddJint()
							.AddJurassic()
							.AddMsie()
							.AddV8()
							;

						_initialized = true;
					}
				}
			}
		}
	}
}