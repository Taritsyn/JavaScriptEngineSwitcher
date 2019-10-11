using IOriginalObjectConverter = Jint.Runtime.Interop.IObjectConverter;
using OriginalEngine = Jint.Engine;
using OriginalValue = Jint.Native.JsValue;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Converts a <see cref="Undefined"/> instance to a <see cref="OriginalValue"/> instance
	/// </summary>
	internal sealed class UndefinedConverter : IOriginalObjectConverter
	{
		public bool TryConvert(OriginalEngine engine, object value, out OriginalValue result)
		{
			if (value is Undefined)
			{
				result = OriginalValue.Undefined;
				return true;
			}

			result = OriginalValue.Null;
			return false;
		}
	}
}