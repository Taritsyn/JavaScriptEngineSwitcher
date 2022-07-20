using System;
using System.Text;

using JavaScriptEngineSwitcher.Jurassic;

namespace TestJurassic
{
	class Program
	{
		static void Main(string[] args)
		{
			var fileManager = new FakeFileManager();
			var base64Encoder = new Base64Encoder();

			var engine = new JurassicJsEngine();
			engine.EmbedHostObject("fileManager", fileManager);
			engine.EmbedHostObject("base64Encoder", base64Encoder);

			string result = engine.Evaluate<string>(
				"base64Encoder.EncodeToBase64(fileManager.ReadBinaryFile('0.gif'))");
			Console.WriteLine("result = {0}", result);
		}

		/// <summary>
		/// Fake file manager
		/// </summary>
		internal sealed class FakeFileManager
		{
			/// <summary>
			/// Opens a binary file, reads all content of the file, and then closes the file
			/// </summary>
			/// <param name="path">The file to open for reading</param>
			/// <returns>The byte array containing all content of the file</returns>
			public byte[] ReadBinaryFile(string path)
			{
				if (path == null)
				{
					throw new ArgumentNullException("path");
				}

				var bytes = new byte[] { 71, 73, 70, 56, 57, 97, 1, 0 };

				return bytes;
			}
		}

		/// <summary>
		/// Base64 encoder
		/// </summary>
		internal sealed class Base64Encoder
		{
			/// <summary>
			/// Encodes a text content to Base64
			/// </summary>
			/// <param name="value">Text content</param>
			/// <returns>Base64 encoded content</returns>
			public string EncodeToBase64(string value)
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				byte[] bytes = Encoding.GetEncoding(0).GetBytes(value);
				string encodedValue = Convert.ToBase64String(bytes);

				return encodedValue;
			}

			/// <summary>
			/// Encodes a binary content to Base64
			/// </summary>
			/// <param name="value">Binary content</param>
			/// <returns>Base64 encoded content</returns>
			public string EncodeToBase64(byte[] value)
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				string encodedValue = Convert.ToBase64String(value);

				return encodedValue;
			}
		}
	}
}