using System.Collections.Generic;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt.Embedding
{
	/// <summary>
	/// Embedded object
	/// </summary>
	internal sealed class EmbeddedObject : EmbeddedItem
	{
		/// <summary>
		/// Constructs an instance of the embedded object
		/// </summary>
		/// <param name="hostObject">Instance of host type</param>
		/// <param name="scriptValue">JavaScript value created from an host object</param>
		public EmbeddedObject(object hostObject, JsValue scriptValue)
			: base(hostObject.GetType(), hostObject, scriptValue, new List<JsNativeFunction>())
		{ }

		/// <summary>
		/// Constructs an instance of the embedded object
		/// </summary>
		/// <param name="hostObject">Instance of host type</param>
		/// <param name="scriptValue">JavaScript value created from an host object</param>
		/// <param name="nativeFunctions">List of native functions, that used to access to members of host object</param>
		public EmbeddedObject(object hostObject, JsValue scriptValue,
			IList<JsNativeFunction> nativeFunctions)
			: base(hostObject.GetType(), hostObject, scriptValue, nativeFunctions)
		{ }

		#region EmbeddedItem overrides

		/// <summary>
		/// Gets a value that indicates if the host item is an instance
		/// </summary>
		public override bool IsInstance
		{
			get { return true; }
		}

		#endregion
	}
}