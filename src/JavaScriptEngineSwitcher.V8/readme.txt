

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: V8 v3.9.0 Preview

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2020 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://github.com/Microsoft/ClearScript) version
   7.0 RC3).

   This package does not contain the native ClearScript.V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.V8.Native.win-x86
    * JavaScriptEngineSwitcher.V8.Native.win-x64
    * JavaScriptEngineSwitcher.V8.Native.linux-x64
    * JavaScriptEngineSwitcher.V8.Native.osx-x64

   =============
   RELEASE NOTES
   =============
   1. Cross-platform is implemented by using an unofficial experimental version of
      the Microsoft ClearScript.V8 (https://github.com/Taritsyn/ClearScript-Experimental)
      library, which is not targeted at any particular operating system or processor
      architecture, and can work with various native assemblies;
   2. Added a packages, that contains a native assemblies for Linux (x64) and OS X (x64).

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher