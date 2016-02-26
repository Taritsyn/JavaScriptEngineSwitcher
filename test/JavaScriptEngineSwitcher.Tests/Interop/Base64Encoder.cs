namespace JavaScriptEngineSwitcher.Tests.Interop
{
	using System;
	using System.Text;

	public static class Base64Encoder
	{
		public const int DATA_URI_MAX = 32768;


		public static string Encode(string value)
		{
			return Convert.ToBase64String(Encoding.Default.GetBytes(value));
		}
	}
}