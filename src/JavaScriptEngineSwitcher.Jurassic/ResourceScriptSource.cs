using System;
using System.IO;
using System.Reflection;

using OriginalScriptSource = Jurassic.ScriptSource;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Represents a embedded JS-resource
	/// </summary>
	internal sealed class ResourceScriptSource : OriginalScriptSource
	{
		/// <summary>
		/// The document name
		/// </summary>
		private readonly string _documentName;

		/// <summary>
		/// The case-sensitive resource name
		/// </summary>
		private readonly string _resourceName;

		/// <summary>
		/// The assembly, which contains the embedded resource
		/// </summary>
		private readonly Assembly _assembly;

		/// <summary>
		/// Gets a document name
		/// </summary>
		public override string Path
		{
			get { return _documentName; }
		}


		/// <summary>
		/// Constructs a instance of <see cref="ResourceScriptSource"/>
		/// </summary>
		/// <param name="documentName">The document name</param>
		/// <param name="resourceName">The case-sensitive resource name</param>
		/// <param name="assembly">The assembly, which contains the embedded resource</param>
		public ResourceScriptSource(string documentName, string resourceName, Assembly assembly)
		{
			if (documentName == null)
			{
				throw new ArgumentNullException(
					"documentName", string.Format(CoreStrings.Common_ArgumentIsNull, "documentName"));
			}

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

			if (string.IsNullOrWhiteSpace(documentName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "documentName"), "documentName");
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "resourceName"), "resourceName");
			}

			_documentName = documentName;
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