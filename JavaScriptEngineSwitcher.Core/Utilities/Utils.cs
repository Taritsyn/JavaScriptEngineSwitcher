namespace JavaScriptEngineSwitcher.Core.Utilities
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Text;

	using Resources;

	public static class Utils
	{
		/// <summary>
		/// Gets a content of the embedded resource as string
		/// </summary>
		/// <param name="resourceName">Resource name</param>
		/// <param name="type">Type from assembly that containing an embedded resource</param>
		/// <returns>Сontent of the embedded resource as string</returns>
		public static string GetResourceAsString(string resourceName, Type type)
		{
			Assembly assembly = type.Assembly;

			return GetResourceAsString(resourceName, assembly);
		}

		/// <summary>
		/// Gets a content of the embedded resource as string
		/// </summary>
		/// <param name="resourceName">Resource name</param>
		/// <param name="assembly">Assembly that containing an embedded resource</param>
		/// <returns>Сontent of the embedded resource as string</returns>
		public static string GetResourceAsString(string resourceName, Assembly assembly)
		{
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					throw new NullReferenceException(
						string.Format(Strings.Resources_ResourceIsNull, resourceName));
				}

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		/// <summary>
		/// Gets text content of the specified file
		/// </summary>
		/// <param name="path">File path</param>
		/// <param name="encoding">Content encoding</param>
		/// <returns>Text content</returns>
		public static string GetFileTextContent(string path, Encoding encoding = null)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(
					string.Format(Strings.Common_FileNotExist, path), path);
			}

			string content;

			using (var file = new StreamReader(path, encoding ?? Encoding.UTF8))
			{
				content = file.ReadToEnd();
			}

			return content;
		}

		/// <summary>
		/// Creates instance by specified full type name
		/// </summary>
		/// <param name="fullTypeName">Full type name</param>
		/// <typeparam name="T">Target type</typeparam>
		/// <returns>Instance of type</returns>
		internal static T CreateInstanceByFullTypeName<T>(string fullTypeName) where T : class
		{
			if (string.IsNullOrWhiteSpace(fullTypeName))
			{
				throw new ArgumentNullException(Strings.Common_ValueIsEmpty);
			}

			string typeName;
			string assemblyName;
			Assembly assembly;
			int commaPosition = fullTypeName.IndexOf(',');

			if (commaPosition != -1)
			{
				typeName = fullTypeName.Substring(0, commaPosition).Trim();
				if (string.IsNullOrEmpty(typeName))
				{
					throw new EmptyValueException(Strings.Common_TypeNameIsEmpty);
				}

				assemblyName = fullTypeName.Substring(commaPosition + 1,
					fullTypeName.Length - (commaPosition + 1)).Trim();
				if (string.IsNullOrEmpty(assemblyName))
				{
					throw new EmptyValueException(Strings.Common_AssemblyNameIsEmpty);
				}

				assembly = Assembly.Load(assemblyName);
			}
			else
			{
				typeName = fullTypeName;
				assembly = typeof(Utils).Assembly;
				assemblyName = assembly.FullName;
			}

			object instance = assembly.CreateInstance(typeName);
			if (instance == null)
			{
				throw new NullReferenceException(string.Format(Strings.Common_InstanceCreationFailed,
					typeName, assemblyName));
			}

			return (T)instance;
		}
	}
}