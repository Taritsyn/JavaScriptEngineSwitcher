namespace JavaScriptEngineSwitcher.Tests.Interop
{
	public static class BundleTable
	{
		private static bool _enableOptimizations = true;

		public static object SyncRoot = new object();

		public static bool EnableOptimizations
		{
			get
			{
				return _enableOptimizations;
			}
			set
			{
				_enableOptimizations = value;
			}
		}
	}
}