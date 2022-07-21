

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: V8 v3.19.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2022 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://github.com/Microsoft/ClearScript) version
   7.3.1).

   This package does not contain the native ClearScript.V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * Microsoft.ClearScript.V8.Native.win-x86
    * Microsoft.ClearScript.V8.Native.win-x64
    * Microsoft.ClearScript.V8.Native.win-arm64
    * Microsoft.ClearScript.V8.Native.linux-x64
    * Microsoft.ClearScript.V8.Native.linux-arm
    * Microsoft.ClearScript.V8.Native.linux-arm64
    * Microsoft.ClearScript.V8.Native.osx-x64
    * Microsoft.ClearScript.V8.Native.osx-arm64

   =============
   RELEASE NOTES
   =============
   Fixed a error #102 “Resources should conform to correct ICU standard for
   naming”. Special thanks to Tim Heuer.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher