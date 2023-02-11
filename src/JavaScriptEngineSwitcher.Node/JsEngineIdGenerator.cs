using System;
using System.Threading;

namespace JavaScriptEngineSwitcher.Node
{
	internal static class JsEngineIdGenerator
	{
		// Base32 encoding - in ascii sort order for easy text based sorting
		private static readonly char[] _encode32Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV".ToCharArray();

		// Seed the `_lastId` for this application instance with
		// the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001
		// for a roughly increasing `_lastId` over restarts
		private static long _lastId = DateTime.UtcNow.Ticks;


		public static string GetNextId() => GenerateId(Interlocked.Increment(ref _lastId));

		private static unsafe string GenerateId(long id)
		{
			char[] encode32Chars = _encode32Chars;

			// stackalloc to allocate array on stack rather than heap
			char* buffer = stackalloc char[13];

			buffer[12] = encode32Chars[id & 31];
			buffer[11] = encode32Chars[(id >> 5) & 31];
			buffer[10] = encode32Chars[(id >> 10) & 31];
			buffer[9] = encode32Chars[(id >> 15) & 31];
			buffer[8] = encode32Chars[(id >> 20) & 31];
			buffer[7] = encode32Chars[(id >> 25) & 31];
			buffer[6] = encode32Chars[(id >> 30) & 31];
			buffer[5] = encode32Chars[(id >> 35) & 31];
			buffer[4] = encode32Chars[(id >> 40) & 31];
			buffer[3] = encode32Chars[(id >> 45) & 31];
			buffer[2] = encode32Chars[(id >> 50) & 31];
			buffer[1] = encode32Chars[(id >> 55) & 31];
			buffer[0] = encode32Chars[(id >> 60) & 31];

			// string `ctor` overload that takes `char*`
			return new string(buffer, 0, 13);
		}
	}
}