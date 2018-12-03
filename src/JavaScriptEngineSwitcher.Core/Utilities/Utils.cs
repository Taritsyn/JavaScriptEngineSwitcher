using System;
#if NET40
using System.Diagnostics;
#endif
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using JavaScriptEngineSwitcher.Core.Resources;

namespace JavaScriptEngineSwitcher.Core.Utilities
{
	public static class Utils
	{
		/// <summary>
		/// Flag indicating whether the current runtime is Mono
		/// </summary>
		private static readonly bool _isMonoRuntime;


		/// <summary>
		/// Static constructor
		/// </summary>
		static Utils()
		{
			_isMonoRuntime = Type.GetType("Mono.Runtime") != null;
		}


		/// <summary>
		/// Determines whether the current runtime is Mono
		/// </summary>
		/// <returns>true if the runtime is Mono; otherwise, false</returns>
		public static bool IsMonoRuntime()
		{
			return _isMonoRuntime;
		}

		/// <summary>
		/// Determines whether the current process is a 64-bit process
		/// </summary>
		/// <returns>true if the process is 64-bit; otherwise, false</returns>
		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		public static bool Is64BitProcess()
		{
#if NETSTANDARD1_3
			bool is64Bit = IntPtr.Size == 8;
#else
			bool is64Bit = Environment.Is64BitProcess;
#endif

			return is64Bit;
		}

		/// <summary>
		/// Gets a content of the embedded resource as string
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name without the namespace of the specified type</param>
		/// <param name="type">The type, that determines the assembly and whose namespace is used to scope
		/// the resource name</param>
		/// <returns>Сontent of the embedded resource as string</returns>
		public static string GetResourceAsString(string resourceName, Type type)
		{
			if (resourceName == null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (type == null)
			{
				throw new ArgumentNullException(
					nameof(type),
					string.Format(Strings.Common_ArgumentIsNull, nameof(type))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

#if NET40
			Assembly assembly = type.Assembly;
#else
			Assembly assembly = type.GetTypeInfo().Assembly;
#endif
			string nameSpace = type.Namespace;
			string resourceFullName = nameSpace != null ? nameSpace + "." + resourceName : resourceName;

			return InnerGetResourceAsString(resourceFullName, assembly);
		}

		/// <summary>
		/// Gets a content of the embedded resource as string
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name</param>
		/// <param name="assembly">The assembly, which contains the embedded resource</param>
		/// <returns>Сontent of the embedded resource as string</returns>
		public static string GetResourceAsString(string resourceName, Assembly assembly)
		{
			if (resourceName == null)
			{
				throw new ArgumentNullException(
					nameof(resourceName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(resourceName))
				);
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(
					nameof(assembly),
					string.Format(Strings.Common_ArgumentIsNull, nameof(assembly))
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			return InnerGetResourceAsString(resourceName, assembly);
		}

		private static string InnerGetResourceAsString(string resourceName, Assembly assembly)
		{
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					throw new NullReferenceException(
						string.Format(Strings.Common_ResourceIsNull, resourceName)
					);
				}

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		/// <summary>
		/// Gets a text content of the specified file
		/// </summary>
		/// <param name="path">File path</param>
		/// <param name="encoding">Content encoding</param>
		/// <returns>Text content</returns>
		public static string GetFileTextContent(string path, Encoding encoding = null)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(
					string.Format(Strings.Common_FileNotExist, path),
					path
				);
			}

			string content;

			using (var stream = File.OpenRead(path))
			using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
			{
				content = reader.ReadToEnd();
			}

			return content;
		}

		/// <summary>
		/// Converts a value of source enumeration type to value of destination enumeration type
		/// </summary>
		/// <typeparam name="TSource">Source enumeration type</typeparam>
		/// <typeparam name="TDest">Destination enumeration type</typeparam>
		/// <param name="value">Value of source enumeration type</param>
		/// <returns>Value of destination enumeration type</returns>
		public static TDest GetEnumFromOtherEnum<TSource, TDest>(TSource value)
		{
			string name = value.ToString();
			var destEnumValues = (TDest[])Enum.GetValues(typeof(TDest));

			foreach (var destEnum in destEnumValues)
			{
				if (string.Equals(destEnum.ToString(), name, StringComparison.OrdinalIgnoreCase))
				{
					return destEnum;
				}
			}

			throw new InvalidCastException(
				string.Format(Strings.Common_EnumValueConversionFailed,
					name, typeof(TSource), typeof(TDest))
			);
		}
	}
}