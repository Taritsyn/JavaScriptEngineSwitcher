#if NET40
using System.Threading;

namespace JavaScriptEngineSwitcher.ChakraCore.Polyfills.System.Threading
{
	/// <summary>
	/// Contains methods for performing volatile memory operations
	/// </summary>
	internal static class Volatile
	{
		/// <summary>
		/// Reads the object reference from the specified field. On systems that require it, inserts
		/// a memory barrier that prevents the processor from reordering memory operations as follows:
		/// If a read or write appears after this method in the code, the processor cannot move it
		/// before this method.
		/// </summary>
		/// <typeparam name="T">The type of field to read. This must be a reference type, not a value type.</typeparam>
		/// <param name="location">The field to read</param>
		/// <returns>The reference to T that was read. This reference is the latest written by any
		/// processor in the computer, regardless of the number of processors or the state of processor
		/// cache.</returns>
		public static T Read<T>(ref T location) where T : class
		{
			T obj = location;
			Thread.MemoryBarrier();

			return obj;
		}
	}
}
#endif