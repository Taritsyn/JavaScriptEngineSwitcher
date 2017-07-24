using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class TestsBase
	{
		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		protected abstract string EngineName { get; }


		static TestsBase()
		{
			JsEngineSwitcherInitializer.Initialize();
		}


		public IJsEngine CreateJsEngine()
		{
			var jsEngine = JsEngineSwitcher.Current.CreateEngine(EngineName);

			return jsEngine;
		}
	}
}