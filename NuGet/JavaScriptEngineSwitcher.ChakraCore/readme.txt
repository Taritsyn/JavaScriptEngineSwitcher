

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: ChakraCore v2.2.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2016 Andrey Taritsyn - http://www.taritsyn.ru


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
   1. Added support of .NET Core 1.0.3;
   2. Downgraded .NET Framework version from 4.5.1 to 4.5;
   3. Attempt to prevent occurrence of the access violation exception in the
      `CallFunction` method;
   4. Fixed a error “Out of stack space”.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher