

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: V8 v2.4.13

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2018 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://github.com/Microsoft/ClearScript) version
   5.4.10).

   This package does not contain the native ClearScript and V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.V8.Native.win-x86
    * JavaScriptEngineSwitcher.V8.Native.win-x64

   =============
   RELEASE NOTES
   =============
   1. Microsoft ClearScript.V8 was updated to version 5.4.10;
   2. Improved implementation of the `CallFunction` method;
   3. Removed unnecessary locks from the `V8JsEngine` class;
   4. In configuration settings of the V8 JS engine was added 3 new properties:
      `HeapSizeSampleInterval` (default `TimeSpan.Zero`), `MaxHeapSize` (default
      `UIntPtr.Zero`) and `MaxStackUsage` (default `UIntPtr.Zero`).

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher