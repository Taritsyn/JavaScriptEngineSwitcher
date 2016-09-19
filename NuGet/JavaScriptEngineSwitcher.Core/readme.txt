

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: Core v2.0.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2016 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScript Engine Switcher determines unified interface for access to the basic
   features of popular JavaScript engines (MSIE JavaScript Engine for .Net,
   Microsoft ClearScript.V8, Jurassic, Jint and ChakraCore). This library allows
   you to quickly and easily switch to using of another JavaScript engine.

   =============
   RELEASE NOTES
   =============
   1. Removed dependency on `System.Configuration.dll` (no longer supported
      configuration by using the `Web.config` and `App.config` files);
   2. Added support of .NET Core 1.0.1 and .NET Framework 4.5.1;
   3. In `IJsEngine` interface was added `SupportsGarbageCollection` property and
      `CollectGarbage` method;
   4. `JsRuntimeErrorHelpers` class was renamed to `JsErrorHelpers` class.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher