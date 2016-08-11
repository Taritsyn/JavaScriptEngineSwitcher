using System.Reflection;

namespace JavaScriptEngineSwitcher.Core.Utilities
{
	public static class TypeInfoExtensions
	{
		public static bool IsInstanceOfType(this TypeInfo source, object o)
		{
			if (o == null)
			{
				return false;
			}

			return source.IsAssignableFrom(o.GetType().GetTypeInfo());
		}
	}
}