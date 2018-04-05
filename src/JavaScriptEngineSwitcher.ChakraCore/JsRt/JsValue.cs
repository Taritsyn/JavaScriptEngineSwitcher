using System;
using System.Runtime.InteropServices;
using System.Text;
#if NET40

using JavaScriptEngineSwitcher.Core.Polyfills.System.Runtime.InteropServices;
#endif

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The JavaScript value
	/// </summary>
	/// <remarks>
	/// The JavaScript value is one of the following types of values: Undefined, Null, Boolean,
	/// String, Number, or Object.
	/// </remarks>
	internal struct JsValue
	{
		/// <summary>
		/// The reference
		/// </summary>
		private readonly IntPtr _reference;

		/// <summary>
		/// Gets a invalid value
		/// </summary>
		public static JsValue Invalid
		{
			get { return new JsValue(IntPtr.Zero); }
		}

		/// <summary>
		/// Gets a value of <c>undefined</c> in the current script context
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public static JsValue Undefined
		{
			get
			{
				JsValue value;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetUndefinedValue(out value));

				return value;
			}
		}

		/// <summary>
		/// Gets a value of <c>null</c> in the current script context
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public static JsValue Null
		{
			get
			{
				JsValue value;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetNullValue(out value));

				return value;
			}
		}

		/// <summary>
		/// Gets a value of <c>true</c> in the current script context
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public static JsValue True
		{
			get
			{
				JsValue value;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetTrueValue(out value));

				return value;
			}
		}

		/// <summary>
		/// Gets a value of <c>false</c> in the current script context
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public static JsValue False
		{
			get
			{
				JsValue value;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetFalseValue(out value));

				return value;
			}
		}

		/// <summary>
		/// Gets a global object in the current script context
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public static JsValue GlobalObject
		{
			get
			{
				JsValue value;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetGlobalObject(out value));

				return value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the value is valid
		/// </summary>
		public bool IsValid
		{
			get { return _reference != IntPtr.Zero; }
		}

		/// <summary>
		/// Gets a JavaScript type of the value
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The type of the value</returns>
		public JsValueType ValueType
		{
			get
			{
				JsValueType type;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetValueType(this, out type));

				return type;
			}
		}

		/// <summary>
		/// Gets a length of a <c>String</c> value
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The length of the string</returns>
		public int StringLength
		{
			get
			{
				int length;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetStringLength(this, out length));

				return length;
			}
		}

		/// <summary>
		/// Gets or sets a prototype of an object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public JsValue Prototype
		{
			get
			{
				JsValue prototypeReference;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetPrototype(this, out prototypeReference));

				return prototypeReference;
			}
			set
			{
				JsErrorHelpers.ThrowIfError(NativeMethods.JsSetPrototype(this, value));
			}
		}

		/// <summary>
		/// Gets a value indicating whether an object is extensible or not
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public bool IsExtensionAllowed
		{
			get
			{
				bool allowed;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetExtensionAllowed(this, out allowed));

				return allowed;
			}
		}

		/// <summary>
		/// Gets a value indicating whether an object is an external object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public bool HasExternalData
		{
			get
			{
				bool hasExternalData;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsHasExternalData(this, out hasExternalData));

				return hasExternalData;
			}
		}

		/// <summary>
		/// Gets or sets a data in an external object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		public IntPtr ExternalData
		{
			get
			{
				IntPtr data;
				JsErrorHelpers.ThrowIfError(NativeMethods.JsGetExternalData(this, out data));

				return data;
			}

			set
			{
				JsErrorHelpers.ThrowIfError(NativeMethods.JsSetExternalData(this, value));
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="JsValue"/> struct
		/// </summary>
		/// <param name="reference">The reference</param>
		private JsValue(IntPtr reference)
		{
			_reference = reference;
		}


		/// <summary>
		/// Creates a <c>Boolean</c> value from a <c>bool</c> value
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="value">The value to be converted</param>
		/// <returns>The converted value</returns>
		public static JsValue FromBoolean(bool value)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsBoolToBoolean(value, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a <c>Number</c> value from a <c>double</c> value
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="value">The value to be converted</param>
		/// <returns>The new <c>Number</c> value</returns>
		public static JsValue FromDouble(double value)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsDoubleToNumber(value, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a <c>Number</c> value from a <c>int</c> value
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="value">The value to be converted</param>
		/// <returns>The new <c>Number</c> value</returns>
		public static JsValue FromInt32(int value)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsIntToNumber(value, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a <c>String</c> value from a string pointer
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="value">The string  to convert to a <c>String</c> value</param>
		/// <returns>The new <c>String</c> value</returns>
		public static JsValue FromString(string value)
		{
			JsValue reference;
			JsErrorCode errorCode;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var stringLength = new UIntPtr((uint)value.Length);
				errorCode = NativeMethods.JsPointerToString(value, stringLength, out reference);
			}
			else
			{
				var byteCount = new UIntPtr((uint)Encoding.GetEncoding(0).GetByteCount(value));
				errorCode = NativeMethods.JsCreateString(value, byteCount, out reference);
			}

			JsErrorHelpers.ThrowIfError(errorCode);

			return reference;
		}

		/// <summary>
		/// Creates a new <c>Object</c>
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The new <c>Object</c></returns>
		public static JsValue CreateObject()
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateObject(out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new <c>Object</c> that stores some external data
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="data">External data that the object will represent. May be null</param>
		/// <param name="finalizer">The callback for when the object is finalized. May be null.</param>
		/// <returns>The new <c>Object</c></returns>
		public static JsValue CreateExternalObject(IntPtr data, JsObjectFinalizeCallback finalizer)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateExternalObject(data, finalizer, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript function
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="function">The method to call when the function is invoked</param>
		/// <returns>The new function object</returns>
		public static JsValue CreateFunction(JsNativeFunction function)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateFunction(function, IntPtr.Zero, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript function
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="function">The method to call when the function is invoked</param>
		/// <param name="callbackData">Data to be provided to all function callbacks</param>
		/// <returns>The new function object</returns>
		public static JsValue CreateFunction(JsNativeFunction function, IntPtr callbackData)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateFunction(function, callbackData, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a JavaScript array object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="length">The initial length of the array</param>
		/// <returns>The new array object</returns>
		public static JsValue CreateArray(uint length)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateArray(length, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">Message for the error object</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateError(JsValue message)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateError(message, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript RangeError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">Message for the error object</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateRangeError(JsValue message)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateRangeError(message, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript ReferenceError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">Message for the error object</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateReferenceError(JsValue message)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateReferenceError(message, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript SyntaxError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">Message for the error object</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateSyntaxError(JsValue message)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateSyntaxError(message, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript TypeError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">Message for the error object</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateTypeError(JsValue message)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateTypeError(message, out reference));

			return reference;
		}

		/// <summary>
		/// Creates a new JavaScript URIError error object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="message">Message for the error object</param>
		/// <returns>The new error object</returns>
		public static JsValue CreateUriError(JsValue message)
		{
			JsValue reference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsCreateURIError(message, out reference));

			return reference;
		}

		/// <summary>
		/// Adds a reference to the object
		/// </summary>
		/// <remarks>
		/// This only needs to be called on objects that are not going to be stored somewhere on
		/// the stack. Calling AddRef ensures that the JavaScript object the value refers to will not be freed
		/// until Release is called
		/// </remarks>
		/// <returns>The object's new reference count</returns>
		public uint AddRef()
		{
			uint count;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsAddRef(this, out count));

			return count;
		}

		/// <summary>
		/// Releases a reference to the object
		/// </summary>
		/// <remarks>
		/// Removes a reference that was created by AddRef.
		/// </remarks>
		/// <returns>The object's new reference count</returns>
		public uint Release()
		{
			uint count;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsRelease(this, out count));

			return count;
		}

		/// <summary>
		/// Retrieves a <c>bool</c> value of a <c>Boolean</c> value
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The converted value</returns>
		public bool ToBoolean()
		{
			bool value;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsBooleanToBool(this, out value));

			return value;
		}

		/// <summary>
		/// Retrieves a <c>double</c> value of a <c>Number</c> value
		/// </summary>
		/// <remarks>
		/// <para>
		/// This function retrieves the value of a Number value. It will fail with
		/// <c>InvalidArgument</c> if the type of the value is not <c>Number</c>.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <returns>The <c>double</c> value</returns>
		public double ToDouble()
		{
			double value;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsNumberToDouble(this, out value));

			return value;
		}

		/// <summary>
		/// Retrieves a <c>int</c> value of a <c>Number</c> value
		/// </summary>
		/// <remarks>
		/// <para>
		/// This function retrieves the value of a Number value. It will fail with
		/// <c>InvalidArgument</c> if the type of the value is not <c>Number</c>.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <returns>The <c>int</c> value</returns>
		public int ToInt32()
		{
			int value;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsNumberToInt(this, out value));

			return value;
		}

		/// <summary>
		/// Retrieves a string pointer of a <c>String</c> value
		/// </summary>
		/// <remarks>
		/// <para>
		/// This function retrieves the string pointer of a <c>String</c> value. It will fail with
		/// <c>InvalidArgument</c> if the type of the value is not <c>String</c>.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <returns>The string</returns>
		public new string ToString()
		{
			string result;
			JsErrorCode errorCode;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				IntPtr ptr;
				UIntPtr stringLength;

				errorCode = NativeMethods.JsStringToPointer(this, out ptr, out stringLength);
				JsErrorHelpers.ThrowIfError(errorCode);

				result = Marshal.PtrToStringUni(ptr, (int)stringLength);
			}
			else
			{
				byte[] buffer = null;
				UIntPtr bufferSize = UIntPtr.Zero;
				UIntPtr length;

				errorCode = NativeMethods.JsCopyString(this, buffer, bufferSize, out length);
				JsErrorHelpers.ThrowIfError(errorCode);

				buffer = new byte[(int)length];
				bufferSize = new UIntPtr((uint)length);

				errorCode = NativeMethods.JsCopyString(this, buffer, bufferSize, out length);
				JsErrorHelpers.ThrowIfError(errorCode);

				result = Encoding.GetEncoding(0).GetString(buffer);
			}

			return result;
		}

		/// <summary>
		/// Converts a value to <c>Boolean</c> using regular JavaScript semantics
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The converted value</returns>
		public JsValue ConvertToBoolean()
		{
			JsValue booleanReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsConvertValueToBoolean(this, out booleanReference));

			return booleanReference;
		}

		/// <summary>
		/// Converts a value to <c>Number</c> using regular JavaScript semantics
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The converted value</returns>
		public JsValue ConvertToNumber()
		{
			JsValue numberReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsConvertValueToNumber(this, out numberReference));

			return numberReference;
		}

		/// <summary>
		/// Converts a value to <c>String</c> using regular JavaScript semantics
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The converted value</returns>
		public JsValue ConvertToString()
		{
			JsValue stringReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsConvertValueToString(this, out stringReference));

			return stringReference;
		}

		/// <summary>
		/// Converts a value to <c>Object</c> using regular JavaScript semantics
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>The converted value</returns>
		public JsValue ConvertToObject()
		{
			JsValue objectReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsConvertValueToObject(this, out objectReference));

			return objectReference;
		}

		/// <summary>
		/// Sets an object to not be extensible
		/// </summary>
		/// <remarks>
		/// Requires an active script context
		/// </remarks>
		public void PreventExtension()
		{
			JsErrorHelpers.ThrowIfError(NativeMethods.JsPreventExtension(this));
		}

		/// <summary>
		/// Gets a property descriptor for an object's own property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="propertyId">The ID of the property</param>
		/// <returns>The property descriptor</returns>
		public JsValue GetOwnPropertyDescriptor(JsPropertyId propertyId)
		{
			JsValue descriptorReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsGetOwnPropertyDescriptor(this, propertyId, out descriptorReference));

			return descriptorReference;
		}

		/// <summary>
		/// Gets a list of all properties on the object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <returns>An array of property names</returns>
		public JsValue GetOwnPropertyNames()
		{
			JsValue propertyNamesReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsGetOwnPropertyNames(this, out propertyNamesReference));

			return propertyNamesReference;
		}

		/// <summary>
		/// Determines whether an object has a property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="propertyId">The ID of the property</param>
		/// <returns>Whether the object (or a prototype) has the property</returns>
		public bool HasProperty(JsPropertyId propertyId)
		{
			bool hasProperty;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsHasProperty(this, propertyId, out hasProperty));

			return hasProperty;
		}

		/// <summary>
		/// Determines whether an object has a non-inherited property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="propertyId">The ID of the property</param>
		/// <returns>Whether the object has the non-inherited property</returns>
		public bool HasOwnProperty(JsPropertyId propertyId)
		{
			bool hasOwnProperty;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsHasOwnProperty(this, propertyId, out hasOwnProperty));

			return hasOwnProperty;
		}

		/// <summary>
		/// Gets an object's property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="id">The ID of the property</param>
		/// <returns>The value of the property</returns>
		public JsValue GetProperty(JsPropertyId id)
		{
			JsValue propertyReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsGetProperty(this, id, out propertyReference));

			return propertyReference;
		}

		/// <summary>
		/// Sets an object's property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="id">The ID of the property</param>
		/// <param name="value">The new value of the property</param>
		/// <param name="useStrictRules">The property set should follow strict mode rules</param>
		public void SetProperty(JsPropertyId id, JsValue value, bool useStrictRules)
		{
			JsErrorHelpers.ThrowIfError(NativeMethods.JsSetProperty(this, id, value, useStrictRules));
		}

		/// <summary>
		/// Deletes an object's property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="propertyId">The ID of the property</param>
		/// <param name="useStrictRules">The property set should follow strict mode rules</param>
		/// <returns>Whether the property was deleted</returns>
		public JsValue DeleteProperty(JsPropertyId propertyId, bool useStrictRules)
		{
			JsValue returnReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsDeleteProperty(this, propertyId, useStrictRules, out returnReference));

			return returnReference;
		}

		/// <summary>
		/// Defines a new object's own property from a property descriptor
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="propertyId">The ID of the property</param>
		/// <param name="propertyDescriptor">The property descriptor</param>
		/// <returns>Whether the property was defined</returns>
		public bool DefineProperty(JsPropertyId propertyId, JsValue propertyDescriptor)
		{
			bool result;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsDefineProperty(this, propertyId, propertyDescriptor, out result));

			return result;
		}

		/// <summary>
		/// Test if an object has a value at the specified index
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="index">The index to test</param>
		/// <returns>Whether the object has an value at the specified index</returns>
		public bool HasIndexedProperty(JsValue index)
		{
			bool hasProperty;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsHasIndexedProperty(this, index, out hasProperty));

			return hasProperty;
		}

		/// <summary>
		/// Retrieves a value at the specified index of an object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="index">The index to retrieve</param>
		/// <returns>The retrieved value</returns>
		public JsValue GetIndexedProperty(JsValue index)
		{
			JsValue propertyReference;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsGetIndexedProperty(this, index, out propertyReference));

			return propertyReference;
		}

		/// <summary>
		/// Sets a value at the specified index of an object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="index">The index to set</param>
		/// <param name="value">The value to set</param>
		public void SetIndexedProperty(JsValue index, JsValue value)
		{
			JsErrorHelpers.ThrowIfError(NativeMethods.JsSetIndexedProperty(this, index, value));
		}

		/// <summary>
		/// Deletes d value at the specified index of an object
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="index">The index to delete</param>
		public void DeleteIndexedProperty(JsValue index)
		{
			JsErrorHelpers.ThrowIfError(NativeMethods.JsDeleteIndexedProperty(this, index));
		}

		/// <summary>
		/// Compare two JavaScript values for equality
		/// </summary>
		/// <remarks>
		/// <para>
		/// This function is equivalent to the "==" operator in JavaScript.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <param name="other">The object to compare</param>
		/// <returns>Whether the values are equal</returns>
		public bool Equals(JsValue other)
		{
			bool equals;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsEquals(this, other, out equals));

			return equals;
		}

		/// <summary>
		/// Compare two JavaScript values for strict equality
		/// </summary>
		/// <remarks>
		/// <para>
		/// This function is equivalent to the "===" operator in JavaScript.
		/// </para>
		/// <para>
		/// Requires an active script context.
		/// </para>
		/// </remarks>
		/// <param name="other">The object to compare</param>
		/// <returns>Whether the values are strictly equal</returns>
		public bool StrictEquals(JsValue other)
		{
			bool equals;
			JsErrorHelpers.ThrowIfError(NativeMethods.JsStrictEquals(this, other, out equals));

			return equals;
		}

		/// <summary>
		/// Invokes a function
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="arguments">The arguments to the call</param>
		/// <returns>The <c>Value</c> returned from the function invocation, if any</returns>
		public JsValue CallFunction(params JsValue[] arguments)
		{
			JsValue returnReference;

			if (arguments.Length > ushort.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(arguments));
			}

			JsErrorHelpers.ThrowIfError(NativeMethods.JsCallFunction(this, arguments, (ushort)arguments.Length, out returnReference));

			return returnReference;
		}

		/// <summary>
		/// Invokes a function as a constructor
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="arguments">The arguments to the call</param>
		/// <returns>The <c>Value</c> returned from the function invocation</returns>
		public JsValue ConstructObject(params JsValue[] arguments)
		{
			JsValue returnReference;

			if (arguments.Length > ushort.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(arguments));
			}

			JsErrorHelpers.ThrowIfError(NativeMethods.JsConstructObject(this, arguments, (ushort)arguments.Length, out returnReference));

			return returnReference;
		}
	}
}