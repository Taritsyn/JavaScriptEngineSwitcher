using System;
using System.IO;
using System.Text;

using OriginalScriptSource = Jurassic.ScriptSource;

using CoreStrings = JavaScriptEngineSwitcher.Core.Resources.Strings;

namespace JavaScriptEngineSwitcher.Jurassic
{
	/// <summary>
	/// Represents a JS-file
	/// </summary>
	internal sealed class FileScriptSource : OriginalScriptSource
	{
		/// <summary>
		/// The document name
		/// </summary>
		private readonly string _documentName;

		/// <summary>
		/// The path to the JS-file
		/// </summary>
		private readonly string _path;

		/// <summary>
		/// The text encoding
		/// </summary>
		private readonly Encoding _encoding;


		/// <summary>
		/// Constructs an instance of <see cref="FileScriptSource"/>
		/// </summary>
		/// <param name="documentName">The document name</param>
		/// <param name="path">The path to the JS-file</param>
		/// <param name="encoding">The text encoding</param>
		public FileScriptSource(string documentName, string path, Encoding encoding = null)
		{
			if (documentName == null)
			{
				throw new ArgumentNullException(
					"documentName", string.Format(CoreStrings.Common_ArgumentIsNull, "documentName"));
			}

			if (path == null)
			{
				throw new ArgumentNullException(
					"path", string.Format(CoreStrings.Common_ArgumentIsNull, "path"));
			}

			if (string.IsNullOrWhiteSpace(documentName))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "documentName"), "documentName");
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					string.Format(CoreStrings.Common_ArgumentIsEmpty, "path"), "path");
			}

			_documentName = documentName;
			_path = path;
			_encoding = encoding ?? Encoding.UTF8;
		}


		#region Jurassic.ScriptSource overrides

		/// <summary>
		/// Gets a document name
		/// </summary>
		public override string Path
		{
			get { return _documentName; }
		}


		/// <summary>
		/// Gets a reader that can be used to read the source code from JS-file
		/// </summary>
		/// <returns>A reader that can be used to read the source code from JS-file,
		/// positioned at the start of the source code</returns>
		public override TextReader GetReader()
		{
			return new StreamReader(_path, _encoding, true);
		}

		#endregion
	}
}