namespace JavaScriptEngineSwitcher.Tests.Interop.Logging
{
	public class DefaultLogger
	{
		public static readonly Lock SyncRoot = new Lock();

		public static ILogger Current = new NullLogger();
	}
}