#if NETSTANDARD1_3
using System;
using System.Reflection;

namespace JavaScriptEngineSwitcher.Core.Polyfills.System.Reflection
{
	internal static class TypeInfoExtensions
	{
		public static bool IsInstanceOfType(this TypeInfo source, object o)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (o == null)
			{
				return false;
			}

			return source.IsAssignableFrom(o.GetType().GetTypeInfo());
		}
	}
}
#endif