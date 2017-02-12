

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: ChakraCore v2.3.2

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2017 Andrey Taritsyn - http://www.taritsyn.ru


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
    * JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm
    * JavaScriptEngineSwitcher.ChakraCore.Native.debian-x64
    * JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64

   =============
   RELEASE NOTES
   =============
   Fixed a error causing a crash during finalization.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher