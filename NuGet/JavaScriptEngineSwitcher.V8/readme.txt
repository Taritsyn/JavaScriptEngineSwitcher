

   --------------------------------------------------------------------------------
                     README file for JS Engine Switcher: V8 v2.0.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2016 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for the
   Microsoft ClearScript.V8 (http://clearscript.codeplex.com) version 5.4.7 with
   support of V8 version 5.3.332.45. For correct working of the Microsoft
   ClearScript.V8 require assemblies `msvcp120.dll` and `msvcr120.dll` from the
   Visual C++ Redistributable Packages for Visual Studio 2013.

   =============
   RELEASE NOTES
   =============
   1. Removed dependency on `System.Configuration.dll` (no longer supported
      configuration by using the `Web.config` and `App.config` files);
   2. Added support of .NET Framework 4.5.1;
   3. Microsoft ClearScript.V8 was updated to version 5.4.7 (support of V8 version
      5.3.332.45);
   4. In configuration settings of the V8 JS engine was changed type of `DebugPort`
      property from `int` to `ushort`.

   ====================
   POST-INSTALL ACTIONS
   ====================
   If in your system does not assemblies `msvcp120.dll` and `msvcr120.dll`, then
   download and install the Visual C++ Redistributable Packages for Visual Studio
   2013 (http://www.microsoft.com/en-us/download/details.aspx?id=40784).

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher