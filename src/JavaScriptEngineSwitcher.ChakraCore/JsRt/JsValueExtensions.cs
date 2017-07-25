namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// Extensions for the JavaScript value
	/// </summary>
	internal static class JsValueExtensions
	{
		/// <summary>
		/// Gets a property descriptor for an object's own property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="source">The JavaScript value</param>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>The property descriptor</returns>
		public static JsValue GetOwnPropertyDescriptor(this JsValue source, string propertyName)
		{
			JsPropertyId propertyId = JsPropertyId.FromString(propertyName);
			JsValue resultValue = source.GetOwnPropertyDescriptor(propertyId);

			return resultValue;
		}

		/// <summary>
		/// Determines whether an object has a property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="source">The JavaScript value</param>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>Whether the object (or a prototype) has the property</returns>
		public static bool HasProperty(this JsValue source, string propertyName)
		{
			JsPropertyId propertyId = JsPropertyId.FromString(propertyName);
			bool result = source.HasProperty(propertyId);

			return result;
		}

		/// <summary>
		/// Determines whether an object has a non-inherited property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="source">The JavaScript value</param>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>Whether the object has the non-inherited property</returns>
		public static bool HasOwnProperty(this JsValue source, string propertyName)
		{
			JsPropertyId propertyId = JsPropertyId.FromString(propertyName);
			bool result = source.HasOwnProperty(propertyId);

			return result;
		}

		/// <summary>
		/// Gets an object's property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="source">The JavaScript value</param>
		/// <param name="name">The name of the property</param>
		/// <returns>The value of the property</returns>
		public static JsValue GetProperty(this JsValue source, string name)
		{
			JsPropertyId id = JsPropertyId.FromString(name);
			JsValue resultValue = source.GetProperty(id);

			return resultValue;
		}

		/// <summary>
		/// Sets an object's property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="source">The JavaScript value</param>
		/// <param name="name">The name of the property</param>
		/// <param name="value">The new value of the property</param>
		/// <param name="useStrictRules">The property set should follow strict mode rules</param>
		public static void SetProperty(this JsValue source, string name, JsValue value, bool useStrictRules)
		{
			JsPropertyId id = JsPropertyId.FromString(name);
			source.SetProperty(id, value, useStrictRules);
		}

		/// <summary>
		/// Deletes an object's property
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="source">The JavaScript value</param>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="useStrictRules">The property set should follow strict mode rules</param>
		/// <returns>Whether the property was deleted</returns>
		public static JsValue DeleteProperty(this JsValue source, string propertyName, bool useStrictRules)
		{
			JsPropertyId propertyId = JsPropertyId.FromString(propertyName);
			JsValue resultValue = source.DeleteProperty(propertyId, useStrictRules);

			return resultValue;
		}

		/// <summary>
		/// Defines a new object's own property from a property descriptor
		/// </summary>
		/// <remarks>
		/// Requires an active script context.
		/// </remarks>
		/// <param name="source">The JavaScript value</param>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="propertyDescriptor">The property descriptor</param>
		/// <returns>Whether the property was defined</returns>
		public static bool DefineProperty(this JsValue source, string propertyName, JsValue propertyDescriptor)
		{
			JsPropertyId propertyId = JsPropertyId.FromString(propertyName);
			bool result = source.DefineProperty(propertyId, propertyDescriptor);

			return result;
		}
	}
}