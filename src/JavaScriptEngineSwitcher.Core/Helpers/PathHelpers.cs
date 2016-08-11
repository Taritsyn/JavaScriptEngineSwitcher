using System;
using System.Linq;
using System.Text;

using JavaScriptEngineSwitcher.Core.Resources;

namespace JavaScriptEngineSwitcher.Core.Helpers
{
	/// <summary>
	/// Path helpers
	/// </summary>
	public static class PathHelpers
	{
		/// <summary>
		/// Converts a back slashes to forward slashes
		/// </summary>
		/// <param name="path">Path with back slashes</param>
		/// <returns>Path with forward slashes</returns>
		public static string ProcessBackSlashes(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path",
					string.Format(Strings.Common_ArgumentIsNull, "path"));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}

			string result = path.Replace('\\', '/');

			return result;
		}

		/// <summary>
		/// Determines whether the path contains the specified directory
		/// </summary>
		/// <param name="path">Path</param>
		/// <param name="directoryName">Name of directory</param>
		/// <returns>true if the path contains an directory with the specified name; otherwise, false</returns>
		public static bool ContainsDirectory(string path, string directoryName)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path",
					string.Format(Strings.Common_ArgumentIsNull, "path"));
			}

			if (directoryName == null)
			{
				throw new ArgumentNullException("directoryName",
					string.Format(Strings.Common_ArgumentIsNull, "directoryName"));
			}

			if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(directoryName))
			{
				return false;
			}

			string processedPath = ProcessBackSlashes(path);
			string[] pathParts = processedPath.Split('/');
			bool result = pathParts.Contains(directoryName, StringComparer.OrdinalIgnoreCase);

			return result;
		}

		/// <summary>
		/// Removes a directory from path
		/// </summary>
		/// <param name="path">Path</param>
		/// <param name="directoryName">Name of directory</param>
		/// <returns>Path without specified directory</returns>
		public static string RemoveDirectoryFromPath(string path, string directoryName)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path",
					string.Format(Strings.Common_ArgumentIsNull, "path"));
			}

			if (directoryName == null)
			{
				throw new ArgumentNullException("directoryName",
					string.Format(Strings.Common_ArgumentIsNull, "directoryName"));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			string processedPath = ProcessBackSlashes(path);
			string[] pathParts = processedPath.Split('/');
			int pathPartCount = pathParts.Length;
			string newPath = string.Empty;

			if (pathPartCount > 0)
			{
				var sb = new StringBuilder();

				for (int pathPartIndex = 0; pathPartIndex < pathPartCount; pathPartIndex++)
				{
					if (pathParts[pathPartIndex].Equals(directoryName, StringComparison.OrdinalIgnoreCase))
					{
						break;
					}

					sb.Append(pathParts[pathPartIndex]);
					sb.Append("/");
				}

				newPath = sb.ToString();
				sb.Clear();
			}

			return newPath;
		}
	}
}