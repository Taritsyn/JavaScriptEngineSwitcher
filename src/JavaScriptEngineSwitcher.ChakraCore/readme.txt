

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: ChakraCore v3.0.0

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2018 Andrey Taritsyn - http://www.taritsyn.ru


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
   1. ChakraCore was updated to version 1.11.4;
   2. No longer used the old ChakraCore API for Windows (Internet Explorer-like
      API);
   3. Added a ability to interrupt execution of the script;
   4. Added a ability to pre-compile scripts;
   5. In configuration settings of the ChakraCore JS engine was added one new
      property - `MaxStackSize` (default `492` or `984` KB);
   6. Added support of .NET Framework 4.7.1, .NET Standard 2.0 and .NET Core App
      2.1.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher