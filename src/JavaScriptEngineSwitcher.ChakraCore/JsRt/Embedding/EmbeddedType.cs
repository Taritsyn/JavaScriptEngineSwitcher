using System;
using System.Collections.Generic;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt.Embedding
{
	/// <summary>
	/// Embedded type
	/// </summary>
	internal sealed class EmbeddedType : EmbeddedItem
	{
		/// <summary>
		/// Constructs an instance of the embedded type
		/// </summary>
		/// <param name="hostType">Host type</param>
		/// <param name="scriptValue">JavaScript value created from an host type</param>
		public EmbeddedType(Type hostType, JsValue scriptValue)
			: base(hostType, null, scriptValue, new List<JsNativeFunction>())
		{ }

		/// <summary>
		/// Constructs an instance of the embedded type
		/// </summary>
		/// <param name="hostType">Host type</param>
		/// <param name="scriptValue">JavaScript value created from an host type</param>
		/// <param name="nativeFunctions">List of native functions, that used to access to members of type</param>
		public EmbeddedType(Type hostType, JsValue scriptValue, IList<JsNativeFunction> nativeFunctions)
			: base(hostType, null, scriptValue, nativeFunctions)
		{ }

		#region EmbeddedItem overrides

		/// <summary>
		/// Gets a value that indicates if the host item is an instance
		/// </summary>
		public override bool IsInstance
		{
			get { return false; }
		}

		#endregion
	}
}