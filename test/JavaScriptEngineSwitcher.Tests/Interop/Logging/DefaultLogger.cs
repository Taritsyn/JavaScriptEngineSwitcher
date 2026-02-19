namespace JavaScriptEngineSwitcher.Tests.Interop.Logging
{
	public class DefaultLogger
	{
		public static readonly object SyncRoot = new object();
		public static ILogger Current = new NullLogger();
	}
}