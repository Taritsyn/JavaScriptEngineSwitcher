using System;
#if NET45 || NET471 || NETSTANDARD || NETCOREAPP2_1
using System.Buffers;
#endif
using System.Text;
#if NET40

using PolyfillsForOldDotNet.System.Buffers;
#endif

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Wrapper for the Javascript ArrayBuffer object that is created from byte array stored in the pool.
	/// When this wrapper is disposed, byte array will be returned to the pool.
	/// </summary>
	internal sealed class JsPooledArrayBuffer : IDisposable
	{
		/// <summary>
		/// The Javascript ArrayBuffer object
		/// </summary>
		private readonly JsValue _value;

		/// <summary>
		/// Byte array returned from the pool
		/// </summary>
		private byte[] _pooledBytes;

		/// <summary>
		/// Flag indicating whether this object is disposed
		/// </summary>
		private bool _disposed;

		/// <summary>
		/// Gets a Javascript ArrayBuffer object
		/// </summary>
		public JsValue Value
		{
			get { return _value; }
		}


		/// <summary>
		/// Constructs an instance of wrapper for the Javascript ArrayBuffer
		/// </summary>
		/// <param name="value">The Javascript ArrayBuffer object</param>
		/// <param name="pooledBytes">Byte array returned from the pool</param>
		private JsPooledArrayBuffer(JsValue value, byte[] pooledBytes)
		{
			_value = value;
			_pooledBytes = pooledBytes;
			_disposed = false;
		}


		/// <summary>
		/// Creates a new wrapper for the Javascript ArrayBuffer
		/// </summary>
		/// <param name="value">String value</param>
		/// <param name="encoding">Character encoding</param>
		/// <returns>Instance of wrapper for the Javascript ArrayBuffer</returns>
		public static JsPooledArrayBuffer Create(string value, Encoding encoding)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (encoding == null)
			{
				throw new ArgumentNullException(nameof(encoding));
			}

			var byteArrayPool = ArrayPool<byte>.Shared;
			int bufferLength = encoding.GetByteCount(value);
			byte[] buffer = byteArrayPool.Rent(bufferLength);

			JsValue bufferValue;
			JsErrorCode errorCode;

			try
			{
				unsafe
				{
					fixed (char* pValue = value)
					fixed (byte* pBuffer = buffer)
					{
						encoding.GetBytes(pValue, value.Length, pBuffer, bufferLength);
						errorCode = NativeMethods.JsCreateExternalArrayBuffer((IntPtr)pBuffer, (uint)bufferLength,
							null, IntPtr.Zero, out bufferValue);
					}
				}

				JsErrorHelpers.ThrowIfError(errorCode);
				bufferValue.AddRef();
			}
			catch
			{
				byteArrayPool.Return(buffer);
				throw;
			}

			return new JsPooledArrayBuffer(bufferValue, buffer);
		}

		#region IDisposable implementation

		/// <summary>
		/// Returns a rented byte array to the pool
		/// </summary>
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				_value.Release();

				if (_pooledBytes != null)
				{
					ArrayPool<byte>.Shared.Return(_pooledBytes);
					_pooledBytes = null;
				}
			}
		}

		#endregion
	}
}