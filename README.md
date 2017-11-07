JavaScript Engine Switcher [![NuGet version](http://img.shields.io/nuget/v/JavaScriptEngineSwitcher.Core.svg)](https://www.nuget.org/packages/JavaScriptEngineSwitcher.Core/)  [![Download count](https://img.shields.io/nuget/dt/JavaScriptEngineSwitcher.Core.svg)](https://www.nuget.org/packages/JavaScriptEngineSwitcher.Core/)
==========================

JavaScript Engine Switcher determines unified interface for access to the basic features of popular JavaScript engines ([MSIE JavaScript Engine for .Net](http://github.com/Taritsyn/MsieJavaScriptEngine), [Microsoft ClearScript.V8](http://github.com/Microsoft/ClearScript), [Jurassic](http://github.com/paulbartrum/jurassic), [Jint](http://github.com/sebastienros/jint), [ChakraCore](http://github.com/Microsoft/ChakraCore) and [VroomJs](http://github.com/pauldotknopf/vroomjs-core)).
This library allows you to quickly and easily switch to using of another JavaScript engine.

The supported .NET types are as follows:

 * `JavaScriptEngineSwitcher.Core.Undefined`
 * `System.Boolean`
 * `System.Int32`
 * `System.Double`
 * `System.String`

## Installation
This library can be installed through NuGet:

 * [JS Engine Switcher: Core](http://nuget.org/packages/JavaScriptEngineSwitcher.Core) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)
 * [JS Engine Switcher: MS Dependency Injection](http://nuget.org/packages/JavaScriptEngineSwitcher.Extensions.MsDependencyInjection) (supports .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)
 * [JS Engine Switcher: MSIE](http://nuget.org/packages/JavaScriptEngineSwitcher.Msie) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)
 * [JS Engine Switcher: V8](http://nuget.org/packages/JavaScriptEngineSwitcher.V8) (supports .NET Framework 4.0 Client and .NET Framework 4.5)
   * [Windows (x86)](http://nuget.org/packages/JavaScriptEngineSwitcher.V8.Native.win-x86)
   * [Windows (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.V8.Native.win-x64)
 * [JS Engine Switcher: Jurassic](http://nuget.org/packages/JavaScriptEngineSwitcher.Jurassic) (supports .NET Framework 4.0 Client, .NET Framework 4.5 and .NET Standard 2.0)
 * [JS Engine Switcher: Jint](http://nuget.org/packages/JavaScriptEngineSwitcher.Jint) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)
 * [JS Engine Switcher: ChakraCore](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)
   * [Windows (x86)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.win-x86)
   * [Windows (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.win-x64)
   * [Windows (ARM)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm)
   * [Linux (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.linux-x64)
   * [OS X (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64)
 * [JS Engine Switcher: Vroom](http://nuget.org/packages/JavaScriptEngineSwitcher.Vroom) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Standard 1.6 and .NET Standard 2.0)

If you have used the JavaScript Engine Switcher version 1.X, then I recommend to first read [“How to upgrade applications to version 2.X”](https://github.com/Taritsyn/JavaScriptEngineSwitcher/wiki/How-to-upgrade-applications-to-version-2.X) section of the documentation.

## Documentation
Documentation is located on the [wiki](https://github.com/Taritsyn/JavaScriptEngineSwitcher/wiki) of this Repo.

## Release History
See the [changelog](CHANGELOG.md).

## License
[Apache License Version 2.0](http://github.com/Taritsyn/JavaScriptEngineSwitcher/blob/master/LICENSE)

## Who's Using JavaScript Engine Switcher
If you use the JavaScript Engine Switcher in some project, please send me a message so I can include it in this list:

 * [Bundle Transformer](https://github.com/Taritsyn/BundleTransformer) by Andrey Taritsyn
 * [Cruncher](https://github.com/JimBobSquarePants/Cruncher) by James South
 * [GFMParserSample.Net](https://github.com/mad4-red/GFMParserSample.Net)
 * [JSPool](http://dan.cx/projects/jspool) by Daniel Lo Nigro
 * [ReactJS.NET](http://reactjs.net/) by Daniel Lo Nigro
 * [Sitecore JavaScript Presentation Module](https://github.com/asmagin/sitecore-js-presentation) by Alex Smagin
 * [T1.Scripts](http://nuget.org/packages/T1.Scripts)
 * [Wyam](http://wyam.io/)
 * [zxcvbn.net](https://github.com/darcythomas/zxcvbn.net) by Darcy Thomas