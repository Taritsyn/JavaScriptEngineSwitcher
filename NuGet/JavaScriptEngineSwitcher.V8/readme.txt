

   --------------------------------------------------------------------------------
                     README file for JS Engine Switcher: V8 v2.2.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2016 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://clearscript.codeplex.com) version 5.4.8).

   This package does not contain the native ClearScript and V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.V8.Native.win-x86
    * JavaScriptEngineSwitcher.V8.Native.win-x64

   =============
   RELEASE NOTES
   =============
   1. Downgraded .NET Framework version from 4.5.1 to 4.5;
   2. Now the Microsoft ClearScript.V8 requires `msvcp140.dll` assembly from the
      Visual C++ Redistributable for Visual Studio 2015.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher