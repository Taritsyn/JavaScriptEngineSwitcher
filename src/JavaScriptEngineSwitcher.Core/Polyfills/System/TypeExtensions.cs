#if NET40
using System;

using JavaScriptEngineSwitcher.Core.Polyfills.System.Reflection;

namespace JavaScriptEngineSwitcher.Core.Polyfills.System
{
	/// <summary>
	/// Type extensions
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Returns the <see cref="TypeInfo"/> representation of the specified type
		/// </summary>
		/// <param name="source">The type to convert</param>
		/// <returns>The converted object</returns>
		public static TypeInfo GetTypeInfo(this Type source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return new TypeInfo(source);
		}
	}
}
#endif