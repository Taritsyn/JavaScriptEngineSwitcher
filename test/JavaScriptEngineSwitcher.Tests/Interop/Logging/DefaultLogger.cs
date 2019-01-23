namespace JavaScriptEngineSwitcher.Tests.Interop.Logging
{
	public class DefaultLogger
	{
		public static ILogger Current = new NullLogger();
	}
}