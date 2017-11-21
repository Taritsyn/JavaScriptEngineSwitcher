

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: V8 v3.0.0 Alpha 7

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2017 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://github.com/Microsoft/ClearScript) version
   5.5.0).

   This package does not contain the native ClearScript and V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.V8.Native.win-x86
    * JavaScriptEngineSwitcher.V8.Native.win-x64

   =============
   RELEASE NOTES
   =============
   In configuration settings of the V8 JS engine was changed types of `MaxHeapSize`
   and `MaxStackUsage` properties from `ulong` to `UIntPtr`.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher