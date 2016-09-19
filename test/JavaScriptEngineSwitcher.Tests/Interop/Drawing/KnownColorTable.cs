#if NETCOREAPP1_0
namespace JavaScriptEngineSwitcher.Tests.Interop.Drawing
{
	internal static class KnownColorTable
	{
		public static int KnownColorToArgb(KnownColor color)
		{
			if (color == KnownColor.OrangeRed)
			{
				return -1286;
			}

			return 0;
		}

		public static string KnownColorToName(KnownColor color)
		{
			if (color == KnownColor.OrangeRed)
			{
				return "OrangeRed";
			}

			return null;
		}
	}
}
#endif