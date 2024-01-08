

   --------------------------------------------------------------------------------
                README file for JS Engine Switcher: ChakraCore v3.23.7

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2024 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.ChakraCore contains adapter `ChakraCoreJsEngine`
   (wrapper for the ChakraCore (https://github.com/chakra-core/ChakraCore)).
   Project was based on the code of Chakra-Samples
   (https://github.com/Microsoft/Chakra-Samples) and jsrt-dotnet
   (https://github.com/robpaveza/jsrt-dotnet).

   This package does not contain the native implementations of ChakraCore.
   Therefore, you need to choose and install the most appropriate package(s) for
   your platform. The following packages are available:

    * JavaScriptEngineSwitcher.ChakraCore.Native.win-x86
    * JavaScriptEngineSwitcher.ChakraCore.Native.win-x64
    * JavaScriptEngineSwitcher.ChakraCore.Native.win-arm
    * JavaScriptEngineSwitcher.ChakraCore.Native.win-arm64
    * JavaScriptEngineSwitcher.ChakraCore.Native.linux-x64
    * JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64

   =============
   RELEASE NOTES
   =============
   Fixed a error that occurred in the `ReflectionHelpers.IsAllowedProperty` method
   when running on .NET Core 1.0.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher