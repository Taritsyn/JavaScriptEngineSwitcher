

   ----------------------------------------------------------------------
       README file for JavaScript Engine Switcher for .Net: V8 0.9.3

   ----------------------------------------------------------------------

          Copyright 2013 Andrey Taritsyn - http://www.taritsyn.ru
		  
		  
   ===========
   DESCRIPTION
   ===========   
   JavaScriptEngineSwitcher.V8 contains adapter `V8JsEngine` (wrapper for 
   the Noesis Javascript Engine for .Net 
   (http://javascriptdotnet.codeplex.com) version 0.7.0). For correct 
   working of the Noesis Javascript Engine require assemblies 
   `msvcp100.dll` and `msvcr100.dll` from the Microsoft Visual C++ 2010.
   
   =============
   RELEASE NOTES
   =============
   1. Changed a mechanism for loading the Noesis Javascript .NET 
      assemblies for different processor architectures;
   2. Deleted a `/configuration/jsEngineSwitcher/v8` configuration 
      section.
   
   ====================
   POST-INSTALL ACTIONS
   ====================
   If your Web.config file is contains the 
   `/configuration/jsEngineSwitcher/v8` configuration element, then you
   should remove it.
   
   If in your system does not assemblies `msvcp100.dll` and 
   `msvcr100.dll`, then download and install the Microsoft Visual C++
   2010 Redistributable Package 
   (x86 - http://www.microsoft.com/en-us/download/details.aspx?id=5555,
   x64 - http://www.microsoft.com/en-us/download/details.aspx?id=14632).
   
   If you want to register the `Noesis.Javascript.x86.dll` and 
   `Noesis.Javascript.x64.dll` assemblies in GAC, then before registering 
   you should rename them to `Noesis.Javascript.dll`.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub - 
   http://github.com/Taritsyn/JavaScriptEngineSwitcher