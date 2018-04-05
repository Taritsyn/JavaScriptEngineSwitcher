#if NET40
using System;

using JavaScriptEngineSwitcher.Core.Resources;

namespace JavaScriptEngineSwitcher.Core.Polyfills.System.Runtime.InteropServices
{
	public struct OSPlatform : IEquatable<OSPlatform>
	{
		private readonly string _osPlatform;

		public static OSPlatform Linux { get; } = new OSPlatform("LINUX");

		public static OSPlatform OSX { get; } = new OSPlatform("OSX");

		public static OSPlatform Windows { get; } = new OSPlatform("WINDOWS");


		private OSPlatform(string osPlatform)
		{
			if (osPlatform == null)
			{
				throw new ArgumentNullException(nameof(osPlatform));
			}

			if (osPlatform.Length == 0)
			{
				throw new ArgumentException(Strings.Common_ArgumentIsEmpty, nameof(osPlatform));
			}

			_osPlatform = osPlatform;
		}


		public static OSPlatform Create(string osPlatform)
		{
			return new OSPlatform(osPlatform);
		}

		public bool Equals(OSPlatform other)
		{
			return Equals(other._osPlatform);
		}

		internal bool Equals(string other)
		{
			return string.Equals(_osPlatform, other, StringComparison.Ordinal);
		}

		public override bool Equals(object obj)
		{
			return obj is OSPlatform && Equals((OSPlatform)obj);
		}

		public override int GetHashCode()
		{
			return _osPlatform == null ? 0 : _osPlatform.GetHashCode();
		}

		public override string ToString()
		{
			return _osPlatform ?? string.Empty;
		}

		public static bool operator ==(OSPlatform left, OSPlatform right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(OSPlatform left, OSPlatform right)
		{
			return !(left == right);
		}
	}
}
#endif