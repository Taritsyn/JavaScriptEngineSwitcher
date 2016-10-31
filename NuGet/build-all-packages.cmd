set packages_directory="C:\Projects\JavaScriptEngineSwitcher\NuGet"

cd %packages_directory%\JavaScriptEngineSwitcher.Core
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Extensions.MsDependencyInjection
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.ChakraCore
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.debian-x64
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.win-x64
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.win-x86
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Jint
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Jurassic
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Msie
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.V8
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.V8.Native.win-x64
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.V8.Native.win-x86
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Vroom
call build-package.cmd