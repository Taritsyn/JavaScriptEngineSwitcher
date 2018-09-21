#if NET40
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Threading;

using JavaScriptEngineSwitcher.ChakraCore.Polyfills.System.Threading;

namespace JavaScriptEngineSwitcher.ChakraCore.Polyfills.System.Buffers
{
	/// <summary>
	/// Provides a resource pool that enables reusing instances of type <see cref="T:T[]"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Renting and returning buffers with an <see cref="ArrayPool{T}"/> can increase performance
	/// in situations where arrays are created and destroyed frequently, resulting in significant
	/// memory pressure on the garbage collector.
	/// </para>
	/// <para>
	/// This class is thread-safe. All members may be used by multiple threads concurrently.
	/// </para>
	/// </remarks>
	internal abstract class ArrayPool<T>
	{
		/// <summary>
		/// The lazily-initialized shared pool instance
		/// </summary>
		private static ArrayPool<T> s_sharedInstance = null;

		/// <summary>
		/// Retrieves a shared <see cref="ArrayPool{T}"/> instance
		/// </summary>
		/// <remarks>
		/// The shared pool provides a default implementation of <see cref="ArrayPool{T}"/>
		/// that's intended for general applicability. It maintains arrays of multiple sizes, and
		/// may hand back a larger array than was actually requested, but will never hand back a smaller
		/// array than was requested. Renting a buffer from it with <see cref="Rent"/> will result in an
		/// existing buffer being taken from the pool if an appropriate buffer is available or in a new
		/// buffer being allocated if one is not available.
		/// </remarks>
		public static ArrayPool<T> Shared
		{
			[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
			get
			{
				return Volatile.Read(ref s_sharedInstance) ?? EnsureSharedCreated();
			}
		}


		/// <summary>
		/// Ensures that <see cref="s_sharedInstance"/> has been initialized to a pool and returns it
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static ArrayPool<T> EnsureSharedCreated()
		{
			Interlocked.CompareExchange(ref s_sharedInstance, new DefaultArrayPool<T>(), null);
			return s_sharedInstance;
		}

		/// <summary>
		/// Retrieves a buffer that is at least the requested length
		/// </summary>
		/// <remarks>
		/// This buffer is loaned to the caller and should be returned to the same pool via
		/// <see cref="Return"/> so that it may be reused in subsequent usage of <see cref="Rent"/>.
		/// It is not a fatal error to not return a rented buffer, but failure to do so may lead to
		/// decreased application performance, as the pool may need to create a new buffer to replace
		/// the one lost.
		/// </remarks>
		/// <param name="minimumLength">The minimum length of the array needed</param>
		/// <returns>An <see cref="T:T[]"/> that is at least <paramref name="minimumLength"/> in length</returns>
		public abstract T[] Rent(int minimumLength);

		/// <summary>
		/// Returns to the pool an array that was previously obtained via <see cref="Rent"/> on the same
		/// <see cref="ArrayPool{T}"/> instance
		/// </summary>
		/// <remarks>
		/// Once a buffer has been returned to the pool, the caller gives up all ownership of the buffer
		/// and must not use it. The reference returned from a given call to <see cref="Rent"/> must only be
		/// returned via <see cref="Return"/> once. The default <see cref="ArrayPool{T}"/>
		/// may hold onto the returned buffer in order to rent it again, or it may release the returned buffer
		/// if it's determined that the pool already has enough buffers stored.
		/// </remarks>
		/// <param name="array">The buffer previously obtained from <see cref="Rent"/> to return to the pool</param>
		public abstract void Return(T[] array);
	}
}
#endif