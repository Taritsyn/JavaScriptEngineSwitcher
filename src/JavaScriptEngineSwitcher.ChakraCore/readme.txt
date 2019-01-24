

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: ChakraCore v3.0.2

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2019 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.ChakraCore contains adapter `ChakraCoreJsEngine`
   (wrapper for the ChakraCore (http://github.com/Microsoft/ChakraCore)).
   Project was based on the code of Chakra-Samples
   (http://github.com/Microsoft/Chakra-Samples) and jsrt-dotnet
   (http://github.com/robpaveza/jsrt-dotnet).

   This package does not contain the native implementations of ChakraCore.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.ChakraCore.Native.win-x86
    * JavaScriptEngineSwitcher.ChakraCore.Native.win-x64
    * JavaScriptEngineSwitcher.ChakraCore.Native.win-arm
    * JavaScriptEngineSwitcher.ChakraCore.Native.linux-x64
    * JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64

   =============
   RELEASE NOTES
   =============
  1. Reduced a memory consumption in cases, where not used the embedding of objects
     and types;
  2. Fixed a wrong implementation of destruction of the embedded delegates;
  3. Accelerated a conversion of script types to host types.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher