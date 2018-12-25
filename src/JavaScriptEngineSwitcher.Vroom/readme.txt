

   --------------------------------------------------------------------------------
                   README file for JS Engine Switcher: Vroom v3.0.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2018 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Vroom contains adapter `VroomJsEngine` (wrapper for the
   VroomJs (http://github.com/pauldotknopf/vroomjs-core) version 1.2.3 with support
   of V8 version 3.17.16.2).

   For correct working of the VroomJs on Windows require the Visual C++
   Redistributable for Visual Studio 2012 and the Microsoft Visual C++ 2015
   Redistributable.

   =============
   RELEASE NOTES
   =============
   1. Added a ability to interrupt execution of the script;
   2. Added support of .NET Framework 4.7.1 and .NET Standard 2.0.

   ====================
   POST-INSTALL ACTIONS
   ====================
   If in your system does not assemblies `msvcr110.dll` and `msvcp140.dll`, then
   download and install the Visual C++ Redistributable Packages for Visual Studio
   2012 (https://www.microsoft.com/en-us/download/details.aspx?id=30679) and 2015
   (https://www.microsoft.com/en-us/download/details.aspx?id=53840).

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher