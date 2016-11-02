

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: ChakraCore v2.1.0

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
   1. Fixed a errors, that occurred during marshaling of Unicode strings in
      Unix-like operating systems;
   2. Native assemblies for Windows have been moved to separate packages:
      JavaScriptEngineSwitcher.ChakraCore.Native.win-x86 and
      JavaScriptEngineSwitcher.ChakraCore.Native.win-x64;
   3. Added a packages, that contains a native assemblies for Windows (ARM),
      Debian-based Linux (x64) and OS X (x64);
   4. ChakraCore was updated to version of October 29, 2016;
   5. New version of the ChakraCore for Windows requires `msvcp140.dll` assembly
      from the Visual C++ Redistributable for Visual Studio 2015.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher