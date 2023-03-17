using IOriginalEngine = Tenray.Topaz.ITopazEngine;
using OriginalArray = Tenray.Topaz.API.JsArray;
using OriginalGlobalThis = Tenray.Topaz.API.GlobalThis;
using OriginalJSONObject = Tenray.Topaz.API.JSONObject;
using OriginalConcurrentObject = Tenray.Topaz.API.ConcurrentJsObject;
using OriginalObject = Tenray.Topaz.API.JsObject;
using OriginalUndefined = Tenray.Topaz.Undefined;
using OriginalVariableKind = Tenray.Topaz.VariableKind;

namespace JavaScriptEngineSwitcher.Topaz
{
	/// <summary>
	/// Default built-in objects initializer
	/// </summary>
	public static class DefaultBuiltinObjectsInitializer
	{
		/// <summary>
		/// Initializes a built-in objects in the global scope
		/// </summary>
		/// <param name="engine">Original JS engine</param>
		public static void Initialize(IOriginalEngine engine)
		{
			// Constants
			engine.SetValueAndKind("Infinity", double.PositiveInfinity, OriginalVariableKind.Const);
			engine.SetValueAndKind("NaN", double.NaN, OriginalVariableKind.Const);
			engine.SetValueAndKind("undefined", OriginalUndefined.Value, OriginalVariableKind.Const);

			// Constructors
			engine.AddType(engine.IsThreadSafe ? typeof(OriginalConcurrentObject) :typeof(OriginalObject), "Object");
			engine.AddType(typeof(OriginalArray), "Array");

			// Objects
			engine.SetValue("globalThis", new OriginalGlobalThis(engine.GlobalScope));
			engine.SetValue("JSON", new OriginalJSONObject());
		}
	}
}