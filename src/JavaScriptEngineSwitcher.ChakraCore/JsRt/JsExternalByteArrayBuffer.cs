using System;
#if NET45 || NET471 || NETSTANDARD || NETCOREAPP2_1
using System.Buffers;
#endif
using System.Runtime.InteropServices;
using System.Text;
#if NET40

using PolyfillsForOldDotNet.System.Buffers;
#endif

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Wrapper for the Javascript ArrayBuffer object
	/// </summary>
	internal sealed class JsExternalByteArrayBuffer : IDisposable
	{
		/// <summary>
		/// The Javascript ArrayBuffer object
		/// </summary>
		private readonly JsValue _value;

		/// <summary>
		/// Buffer containing byte array
		/// </summary>
		private byte[] _buffer;

		/// <summary>
		/// Handle for the buffer
		/// </summary>
		private readonly GCHandle _bufferHandle;

		/// <summary>
		/// Flag indicating that the buffer was received from the pool
		/// </summary>
		private readonly bool _usePool;

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
		/// <param name="buffer">Buffer containing byte array</param>
		/// <param name="bufferHandle">Handle for the buffer</param>
		/// <param name="usePool">Flag indicating that the buffer was received from the pool</param>
		private JsExternalByteArrayBuffer(JsValue value, byte[] buffer, GCHandle bufferHandle, bool usePool)
		{
			_value = value;
			_buffer = buffer;
			_bufferHandle = bufferHandle;
			_usePool = usePool;
		}


		/// <summary>
		/// Creates a new wrapper for the Javascript ArrayBuffer from a string value
		/// </summary>
		/// <param name="value">String value</param>
		/// <param name="encoding">Character encoding</param>
		/// <returns>Instance of wrapper for the Javascript ArrayBuffer</returns>
		public static JsExternalByteArrayBuffer FromString(string value, Encoding encoding)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (encoding == null)
			{
				throw new ArgumentNullException(nameof(encoding));
			}

			int valueLength = value.Length;
			var byteArrayPool = ArrayPool<byte>.Shared;
			int bufferLength = encoding.GetByteCount(value);
			byte[] buffer = byteArrayPool.Rent(bufferLength + 1);

			encoding.GetBytes(value, 0, value.Length, buffer, 0);
			buffer[bufferLength] = 0;

			GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr bufferPtr = bufferHandle.AddrOfPinnedObject();
			JsValue bufferValue;

			try
			{
				JsErrorCode errorCode = NativeMethods.JsCreateExternalArrayBuffer(bufferPtr, (uint)bufferLength,
					null, IntPtr.Zero, out bufferValue);
				JsErrorHelpers.ThrowIfError(errorCode);

				bufferValue.AddRef();
			}
			catch
			{
				bufferHandle.Free();
				byteArrayPool.Return(buffer);

				throw;
			}

			return new JsExternalByteArrayBuffer(bufferValue, buffer, bufferHandle, true);
		}

		/// <summary>
		/// Creates a new wrapper for the Javascript ArrayBuffer from a byte array
		/// </summary>
		/// <param name="value">Byte array</param>
		/// <returns>Instance of wrapper for the Javascript ArrayBuffer</returns>
		public static JsExternalByteArrayBuffer FromBytes(byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			GCHandle bufferHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
			IntPtr bufferPtr = bufferHandle.AddrOfPinnedObject();
			JsValue bufferValue;

			try
			{
				JsErrorCode errorCode = NativeMethods.JsCreateExternalArrayBuffer(bufferPtr, (uint)value.Length,
					null, IntPtr.Zero, out bufferValue);
				JsErrorHelpers.ThrowIfError(errorCode);

				bufferValue.AddRef();
			}
			catch
			{
				bufferHandle.Free();

				throw;
			}

			return new JsExternalByteArrayBuffer(bufferValue, value, bufferHandle, false);
		}

		#region IDisposable implementation

		/// <summary>
		/// Free a resources used by the wrapper
		/// </summary>
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				_value.Release();
				_bufferHandle.Free();
				if (_usePool)
				{
					ArrayPool<byte>.Shared.Return(_buffer);
				}
				_buffer = null;
			}
		}

		#endregion
	}
}