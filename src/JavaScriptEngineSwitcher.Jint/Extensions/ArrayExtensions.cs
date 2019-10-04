/* .NET Object Deep Copy
 * https://github.com/Burtsev-Alexey/net-object-deep-copy
 *
 * Copyright (c) 2014, Burtsev Alexey
 * Source code is released under the MIT license.
 */

using System;

namespace JavaScriptEngineSwitcher.Jint.Extensions
{
	internal static class ArrayExtensions
	{
		public static void ForEach(this Array array, Action<Array, int[]> action)
		{
			if (array.LongLength == 0) return;
			ArrayTraverse walker = new ArrayTraverse(array);
			do action(array, walker.Position);
			while (walker.Step());
		}

		private class ArrayTraverse
		{
			public int[] Position;
			private int[] maxLengths;

			public ArrayTraverse(Array array)
			{
				maxLengths = new int[array.Rank];
				for (int i = 0; i < array.Rank; ++i)
				{
					maxLengths[i] = array.GetLength(i) - 1;
				}
				Position = new int[array.Rank];
			}

			public bool Step()
			{
				for (int i = 0; i < Position.Length; ++i)
				{
					if (Position[i] < maxLengths[i])
					{
						Position[i]++;
						for (int j = 0; j < i; j++)
						{
							Position[j] = 0;
						}
						return true;
					}
				}
				return false;
			}
		}
	}
}