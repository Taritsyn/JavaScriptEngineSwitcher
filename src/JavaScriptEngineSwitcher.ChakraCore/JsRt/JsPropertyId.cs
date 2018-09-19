using System;
#if NET45 || NET471 || NETSTANDARD
using System.Buffers;
using System.Runtime.InteropServices;
#endif
using System.Text;
#if NET40

using JavaScriptEngineSwitcher.Core.Polyfills.System.Runtime.InteropServices;
#endif

#if NET40
using JavaScriptEngineSwitcher.ChakraCore.Polyfills.System.Buffers;
#endif
using JavaScriptEngineSwitcher.ChakraCore.Helpers;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The property identifier
	/// </summary>
	/// <remarks>
	/// Property identifiers are used to refer to properties of JavaScript objects instead of using
	/// strings.
	/// </remarks>
	internal struct JsPropertyId : IEquatable<JsPropertyId>
	{
		/// <summary>
		/// The id
		/// </summary>
		private readonly IntPtr _id;

		/// <summary>
		/// Gets a invalid ID
		/// </summary>
		public static JsPropertyId Invalid
		{
			get { return new JsPropertyId(IntPtr.Zero); }
		}

		/// <summary>
		/// Gets a name associated with the property ID
		/// </summary>
		/// <remarks>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		public string Name
		{
			get
			{
				byte[] buffer = null;
				UIntPtr bufferSize = UIntPtr.Zero;
				UIntPtr length;

				JsErrorCode errorCode = NativeMethods.JsCopyPropertyId(this, buffer, bufferSize, out length);
				JsErrorHelpers.ThrowIfError(errorCode);

				string name;
				var byteArrayPool = ArrayPool<byte>.Shared;
				bufferSize = length;
				buffer = byteArrayPool.Rent((int)bufferSize);

				try
				{
					errorCode = NativeMethods.JsCopyPropertyId(this, buffer, bufferSize, out length);
					JsErrorHelpers.ThrowIfError(errorCode);

					name = Encoding.UTF8.GetString(buffer, 0, (int)bufferSize);
				}
				finally
				{
					byteArrayPool.Return(buffer);
				}

				return name;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsPropertyId"/> struct
		/// </summary>
		/// <param name="id">The ID</param>
		internal JsPropertyId(IntPtr id)
		{
			_id = id;
		}


		/// <summary>
		/// Gets a property ID associated with the name
		/// </summary>
		/// <remarks>
		/// <para>
		/// Property IDs are specific to a context and cannot be used across contexts.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the property ID to get or create.
		/// The name may consist of only digits.</param>
		/// <returns>The property ID in this runtime for the given name</returns>
		public static JsPropertyId FromString(string name)
		{
			string processedName;
			int byteCount;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				processedName = EncodingHelpers.UnicodeToUtf8(name, out byteCount);
			}
			else
			{
				processedName = name;
				byteCount = Encoding.UTF8.GetByteCount(processedName);
			}

			JsPropertyId id;
			UIntPtr length = new UIntPtr((uint)byteCount);

			JsErrorCode errorCode = NativeMethods.JsCreatePropertyId(processedName, length, out id);
			JsErrorHelpers.ThrowIfError(errorCode);

			return id;
		}

		/// <summary>
		/// The equality operator for property IDs
		/// </summary>
		/// <param name="left">The first property ID to compare</param>
		/// <param name="right">The second property ID to compare</param>
		/// <returns>Whether the two property IDs are the same</returns>
		public static bool operator ==(JsPropertyId left, JsPropertyId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// The inequality operator for property IDs
		/// </summary>
		/// <param name="left">The first property ID to compare</param>
		/// <param name="right">The second property ID to compare</param>
		/// <returns>Whether the two property IDs are not the same</returns>
		public static bool operator !=(JsPropertyId left, JsPropertyId right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Checks for equality between property IDs
		/// </summary>
		/// <param name="obj">The other property ID to compare</param>
		/// <returns>Whether the two property IDs are the same</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is JsPropertyId && Equals((JsPropertyId)obj);
		}

		/// <summary>
		/// The hash code
		/// </summary>
		/// <returns>The hash code of the property ID</returns>
		public override int GetHashCode()
		{
			return _id.ToInt32();
		}

		/// <summary>
		/// Converts the property ID to a string
		/// </summary>
		/// <returns>The name of the property ID</returns>
		public override string ToString()
		{
			return Name;
		}

		#region IEquatable<T> implementation

		/// <summary>
		/// Checks for equality between property IDs
		/// </summary>
		/// <param name="other">The other property ID to compare</param>
		/// <returns>Whether the two property IDs are the same</returns>
		public bool Equals(JsPropertyId other)
		{
			return _id == other._id;
		}

		#endregion
	}
}