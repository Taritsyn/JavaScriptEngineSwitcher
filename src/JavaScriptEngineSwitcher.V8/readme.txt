

   --------------------------------------------------------------------------------
                     README file for JS Engine Switcher: V8 v3.0.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2018 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://github.com/Microsoft/ClearScript) version
   5.5.4).

   This package does not contain the native ClearScript and V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.V8.Native.win-x86
    * JavaScriptEngineSwitcher.V8.Native.win-x64

   =============
   RELEASE NOTES
   =============
   1. Microsoft ClearScript.V8 was updated to version 5.5.4;
   2. Now requires .NET Framework 4.5 or higher;
   3. Added a ability to interrupt execution of the script;
   4. Added a ability to pre-compile scripts;
   5. In configuration settings of the V8 JS engine became obsolete the
      `MaxExecutableSize` property and added two new properties:
      `AwaitDebuggerAndPauseOnStart` (default `false`) and `EnableRemoteDebugging`
      (default `false`).

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher