JavaScript Engine Switcher [![NuGet version](http://img.shields.io/nuget/v/JavaScriptEngineSwitcher.Core.svg)](https://www.nuget.org/packages/JavaScriptEngineSwitcher.Core/)  [![Download count](https://img.shields.io/nuget/dt/JavaScriptEngineSwitcher.Core.svg)](https://www.nuget.org/packages/JavaScriptEngineSwitcher.Core/)
==========================

JavaScript Engine Switcher determines unified interface for access to the basic features of popular JavaScript engines ([ChakraCore](https://github.com/Microsoft/ChakraCore), [Jering.Javascript.NodeJS](https://github.com/JeringTech/Javascript.NodeJS), [Jint](https://github.com/sebastienros/jint), [Jurassic](https://github.com/paulbartrum/jurassic), [MSIE JavaScript Engine for .NET](https://github.com/Taritsyn/MsieJavaScriptEngine), [NiL.JS](https://github.com/nilproject/NiL.JS), [Microsoft ClearScript.V8](https://github.com/Microsoft/ClearScript) and [VroomJs](https://github.com/pauldotknopf/vroomjs-core)).
This library allows you to quickly and easily switch to using of another JavaScript engine.

The supported .NET types are as follows:

 * `JavaScriptEngineSwitcher.Core.Undefined`
 * `System.Boolean`
 * `System.Int32`
 * `System.Double`
 * `System.String`

## Installation
This library can be installed through NuGet:

### Core
 * [JS Engine Switcher: Core](http://nuget.org/packages/JavaScriptEngineSwitcher.Core) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)
 * [JS Engine Switcher: MS Dependency Injection](http://nuget.org/packages/JavaScriptEngineSwitcher.Extensions.MsDependencyInjection) (supports .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)

### JS engines
 * [JS Engine Switcher: ChakraCore](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Framework 4.7.1, .NET Standard 1.3, .NET Standard 2.0, .NET Core App 2.1 and .NET Standard 2.1)
   * [Windows (x86)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.win-x86)
   * [Windows (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.win-x64)
   * [Windows (ARM)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.win-arm)
   * [Windows (ARM64)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.win-arm64)
   * [Linux (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.linux-x64)
   * [OS X (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64)
 * [JS Engine Switcher: Jint](http://nuget.org/packages/JavaScriptEngineSwitcher.Jint) (supports .NET Framework 4.6.1, .NET Standard 2.0 and .NET Standard 2.1)
 * [JS Engine Switcher: Jurassic](http://nuget.org/packages/JavaScriptEngineSwitcher.Jurassic) (supports .NET Framework 4.0 Client, .NET Framework 4.5 and .NET Standard 2.0)
 * [JS Engine Switcher: MSIE](http://nuget.org/packages/JavaScriptEngineSwitcher.Msie) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Standard 1.3 and .NET Standard 2.0)
 * [JS Engine Switcher: NiL](http://nuget.org/packages/JavaScriptEngineSwitcher.NiL) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Framework 4.6.1, .NET Standard 1.3, .NET Standard 1.6 and .NET Standard 2.0)
 * [JS Engine Switcher: Node](http://nuget.org/packages/JavaScriptEngineSwitcher.Node) (supports .NET Framework 4.6.1 and .NET Standard 2.0)
 * [JS Engine Switcher: V8](http://nuget.org/packages/JavaScriptEngineSwitcher.V8) (supports .NET Framework 4.5 and .NET Core App 3.1)
   * [Windows (x86)](http://nuget.org/packages/JavaScriptEngineSwitcher.V8.Native.win-x86)
   * [Windows (x64)](http://nuget.org/packages/JavaScriptEngineSwitcher.V8.Native.win-x64)
 * [JS Engine Switcher: Vroom](http://nuget.org/packages/JavaScriptEngineSwitcher.Vroom) (supports .NET Framework 4.0 Client, .NET Framework 4.5, .NET Framework 4.7.1, .NET Standard 1.6 and .NET Standard 2.0)

If you have used the JavaScript Engine Switcher version 2.X, then I recommend to first read [“How to upgrade applications to version 3.X”](https://github.com/Taritsyn/JavaScriptEngineSwitcher/wiki/How-to-upgrade-applications-to-version-3.X) section of the documentation.

## Documentation
Documentation is located on the [wiki](https://github.com/Taritsyn/JavaScriptEngineSwitcher/wiki) of this Repo.

## Release History
See the [changelog](CHANGELOG.md).

## License
[Apache License Version 2.0](http://github.com/Taritsyn/JavaScriptEngineSwitcher/blob/master/LICENSE.txt)

## Who's Using JavaScript Engine Switcher
If you use the JavaScript Engine Switcher in some project, please send me a message so I can include it in this list:

 * [Autoprefixer Host for .NET](https://github.com/Taritsyn/AutoprefixerHost) by Andrey Taritsyn
 * [Bundle Transformer](https://github.com/Taritsyn/BundleTransformer) by Andrey Taritsyn
 * [Cruncher](https://github.com/JimBobSquarePants/Cruncher) by James South
 * [E.F.F.C JavaScriptEngineSwitcher Extention Lib](https://github.com/redwolf0817/EFFC.JavaScriptEngineSwitcher.Extention) by ItTrending
 * [GFMParserSample.Net](https://github.com/mad4-red/GFMParserSample.Net)
 * [JSPool](http://dan.cx/projects/jspool) by Daniel Lo Nigro
 * [MediaGetCore](https://github.com/XuPeiYao/MediaGetCore) by Xu Pei-Yao
 * [ProteanCMS](https://github.com/Eonic/ProteanCMS) by Trevor Spink
 * [ReactJS.NET](http://reactjs.net/) by Daniel Lo Nigro
 * [Sitecore JavaScript Presentation Module](https://github.com/asmagin/sitecore-js-presentation) by Alex Smagin
 * [T1.Scripts](http://nuget.org/packages/T1.Scripts)
 * [Wyam](http://wyam.io/)
 * [ZeroReact.NET](https://github.com/DaniilSokolyuk/ZeroReact.NET) by Daniil Sokolyuk
 * [zxcvbn.net](https://github.com/darcythomas/zxcvbn.net) by Darcy Thomas