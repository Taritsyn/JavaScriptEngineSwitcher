using System;
using System.IO;
using System.Reflection;

using Jurassic;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Represents a embedded JS-resource
	/// </summary>
	internal sealed class ResourceScriptSource : ScriptSource
	{
		/// <summary>
		/// The case-sensitive resource name
		/// </summary>
		private readonly string _resourceName;

		/// <summary>
		/// The assembly, which contains the embedded resource
		/// </summary>
		private readonly Assembly _assembly;

		/// <summary>
		/// Gets a path to the embedded JS-resource
		/// </summary>
		public override string Path
		{
			get { return _resourceName; }
		}


		/// <summary>
		/// Constructs a instance of <see cref="ResourceScriptSource"/>
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name</param>
		/// <param name="assembly">The assembly, which contains the embedded resource</param>
		public ResourceScriptSource(string resourceName, Assembly assembly)
		{
			if (resourceName == null)
			{
				throw new ArgumentNullException(
					"resourceName", string.Format(CoreStrings.Common_ArgumentIsNull, "resourceName"));
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(
					"assembly", string.Format(CoreStrings.Common_ArgumentIsNull, "assembly"));
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			_resourceName = resourceName;
			_assembly = assembly;
		}


		/// <summary>
		/// Gets a reader that can be used to read the source code from the embedded JS-resource
		/// </summary>
		/// <returns>The reader that can be used to read the source code from the embedded
		/// JS-resource, positioned at the start of the source code</returns>
		public override TextReader GetReader()
		{
			Stream stream = _assembly.GetManifestResourceStream(_resourceName);

			if (stream == null)
			{
				throw new NullReferenceException(
					string.Format(CoreStrings.Resources_ResourceIsNull, _resourceName));
			}

			return new StreamReader(stream);
		}
	}
}