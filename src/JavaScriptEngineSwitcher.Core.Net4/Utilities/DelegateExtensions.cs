#if NET40
using System;
using System.Reflection;

namespace JavaScriptEngineSwitcher.Core.Utilities
{
	/// <summary>
	/// Delegate extensions
	/// </summary>
	public static class DelegateExtensions
	{
		/// <summary>
		/// Gets an object that represents the method represented by the specified delegate
		/// </summary>
		/// <param name="source">The delegate to examine</param>
		/// <returns>An object that represents the method</returns>
		public static MethodInfo GetMethodInfo(this Delegate source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.Method;
		}
	}
}
#endif