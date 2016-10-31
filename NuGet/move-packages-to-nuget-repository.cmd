set packages_directory="C:\Projects\JavaScriptEngineSwitcher\NuGet"
set repository_directory="C:\NuGet Repository"

move %packages_directory%\JavaScriptEngineSwitcher.Core\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.Extensions.MsDependencyInjection\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.ChakraCore\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.debian-x64\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.win-x64\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.ChakraCore.Native.win-x86\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.Jint\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.Jurassic\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.Msie\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.V8\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.V8.Native.win-x64\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.V8.Native.win-x86\*.nupkg %repository_directory%
move %packages_directory%\JavaScriptEngineSwitcher.Vroom\*.nupkg %repository_directory%