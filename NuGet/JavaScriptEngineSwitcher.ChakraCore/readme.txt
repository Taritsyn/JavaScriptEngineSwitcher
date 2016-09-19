

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: ChakraCore v2.0.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2016 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.ChakraCore contains adapter `ChakraCoreJsEngine`
   (wrapper for the ChakraCore (http://github.com/Microsoft/ChakraCore) version
   1.3). Project was based on the code of Chakra-Samples
   (http://github.com/Microsoft/Chakra-Samples) and jsrt-dotnet
   (http://github.com/robpaveza/jsrt-dotnet).

   For correct working of the ChakraCore require assemblies `msvcp120.dll` and
   `msvcr120.dll` from the Visual C++ Redistributable Packages for Visual Studio
   2013.

   =============
   RELEASE NOTES
   =============
   1. Added support of .NET Core 1.0.1 and .NET Framework 4.5.1;
   2. ChakraCore was updated to version 1.3;
   3. Added the ability to change configuration settings of the ChakraCore JS
      engine: `DisableBackgroundWork` (default `false`),
      `DisableNativeCodeGeneration` (default `false`), `DisableEval` (default
      `false`) and `EnableExperimentalFeatures` (default `false`).

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