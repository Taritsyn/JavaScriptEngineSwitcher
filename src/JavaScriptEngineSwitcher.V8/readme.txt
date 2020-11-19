

   --------------------------------------------------------------------------------
                     README file for JS Engine Switcher: V8 v3.9.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2020 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://github.com/Microsoft/ClearScript) version
   7.0).

   This package does not contain the native ClearScript.V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * Microsoft.ClearScript.V8.Native.win-x86
    * Microsoft.ClearScript.V8.Native.win-x64
    * Microsoft.ClearScript.V8.Native.linux-x64
    * Microsoft.ClearScript.V8.Native.osx-x64

   =============
   RELEASE NOTES
   =============
   1. Microsoft ClearScript.V8 was updated to version 7.0;
   2. Added support of .NET Framework 4.7.1 and .NET 5.0;
   3. Added support of Linux (x64) and OS X (x64);
   4. Own versions of the ClearScript's assemblies are no longer build, because the
      official NuGet package is now used. Therefore, you should also replace in
      your projects the `JavaScriptEngineSwitcher.V8.Native.*` packages by the
      `Microsoft.ClearScript.V8.Native.*` packages.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher