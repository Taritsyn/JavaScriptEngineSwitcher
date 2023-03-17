namespace JavaScriptEngineSwitcher.Tests.Interop.Logging
{
	public class DefaultLogger
	{
		public static object SyncRoot = new object();
		public static ILogger Current = new NullLogger();
	}
}