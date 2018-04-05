using System;
using System.Collections.Generic;
using System.IO;

using JavaScriptEngineSwitcher.Core.Resources;

namespace JavaScriptEngineSwitcher.Core.Utilities
{
	/// <summary>
	/// Unique document name manager
	/// </summary>
	public sealed class UniqueDocumentNameManager
	{
		/// <summary>
		/// Default document name
		/// </summary>
		private readonly string _defaultName;

		/// <summary>
		/// Storage of unique names
		/// </summary>
		private readonly Dictionary<string, uint> _storage = new Dictionary<string, uint>();

		/// <summary>
		/// Synchronizer of unique name storage
		/// </summary>
		private readonly object _storageSynchronizer = new object();


		/// <summary>
		/// Constructs an instance of the unique document name manager
		/// </summary>
		/// <param name="defaultName">Default document name</param>
		public UniqueDocumentNameManager(string defaultName)
		{
			if (defaultName == null)
			{
				throw new ArgumentNullException(
					nameof(defaultName),
					string.Format(Strings.Common_ArgumentIsNull, nameof(defaultName))
				);
			}

			if (string.IsNullOrWhiteSpace(defaultName))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentIsEmpty, nameof(defaultName)),
					nameof(defaultName)
				);
			}

			_defaultName = defaultName;
		}


		/// <summary>
		/// Gets a unique document name
		/// </summary>
		/// <param name="name">Document name</param>
		/// <returns>Unique document name</returns>
		public string GetUniqueName(string name)
		{
			string appropriateName = !string.IsNullOrWhiteSpace(name) ?
				Path.GetFileNameWithoutExtension(name) : _defaultName;
			string extension = Path.GetExtension(name);

			lock (_storageSynchronizer)
			{
				uint count;
				_storage.TryGetValue(appropriateName, out count);
				_storage[appropriateName] = ++count;

				string uniqueName = count > 1 ?
					string.Concat(appropriateName, " [", count, "]", extension)
					:
					string.Concat(appropriateName, extension)
					;

				return uniqueName;
			}
		}
	}
}