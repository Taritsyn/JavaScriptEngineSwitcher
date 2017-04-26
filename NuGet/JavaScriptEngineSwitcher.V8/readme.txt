

   --------------------------------------------------------------------------------
                     README file for JS Engine Switcher: V8 v2.4.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2017 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://clearscript.codeplex.com) version of April 19,
   2017).

   This package does not contain the native ClearScript and V8 assemblies.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.V8.Native.win-x86
    * JavaScriptEngineSwitcher.V8.Native.win-x64

   =============
   RELEASE NOTES
   =============
   1. Microsoft ClearScript.V8 was updated to version of April 19, 2017;
   2. Now the `Evaluate` and `Execute` methods of `V8ScriptEngine` class are called
      with the `discard` parameter equal to `false`.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher