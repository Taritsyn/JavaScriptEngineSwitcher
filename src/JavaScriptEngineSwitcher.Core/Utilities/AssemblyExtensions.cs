#if NETSTANDARD1_3
using System;
using System.IO;
using System.Reflection;

using JavaScriptEngineSwitcher.Core.Resources;

namespace JavaScriptEngineSwitcher.Core.Utilities
{
	/// <summary>
	/// Assembly extensions
	/// </summary>
	public static class AssemblyExtensions
	{
		/// <summary>
		/// Loads the specified manifest resource, scoped by the namespace of the specified type, from this assembly.
		/// </summary>
		/// <param name="source">This assembly.</param>
		/// <param name="type">The type whose namespace is used to scope the manifest resource name.</param>
		/// <param name="name">The case-sensitive name of the manifest resource being requested.</param>
		/// <returns>The manifest resource; or null if no resources were specified during compilation
		/// or if the resource is not visible to the caller.</returns>
		/// <exception cref="ArgumentNullException">The name parameter is null.</exception>
		/// <exception cref="ArgumentException">The name parameter is an empty string ("").</exception>
		/// <exception cref="FileLoadException">A file that was found could not be loaded.</exception>
		/// <exception cref="FileNotFoundException">name was not found.</exception>
		/// <exception cref="BadImageFormatException">name is not a valid assembly.</exception>
		/// <exception cref="NotImplementedException">Resource length is greater than System.Int64.MaxValue.</exception>
		public static Stream GetManifestResourceStream(this Assembly source, Type type, string name)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, "name"), "name");
			}

			Assembly assembly = type.GetTypeInfo().Assembly;
			string nameSpace = type.Namespace;
			string fullName = nameSpace != null ? nameSpace + "." + name : name;

			return assembly.GetManifestResourceStream(fullName);
		}
	}
}
#endif