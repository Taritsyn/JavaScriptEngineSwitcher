namespace JavaScriptEngineSwitcher.Tests.Interop
{
	public static class BundleTable
	{
		private static bool _enableOptimizations = true;

		public static readonly Lock SyncRoot = new Lock();

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