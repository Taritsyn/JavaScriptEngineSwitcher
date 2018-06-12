Change log
==========

## v2.4.19 - June 12, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.9
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 7, 2018
 * In JavaScriptEngineSwitcher.ChakraCore changed a implementation of the `Dispose` method

## v2.4.18 - May 24, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.8
 * In JavaScriptEngineSwitcher.ChakraCore fixed a [error #34](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/34) “Finalazier thread is blocked because of JavaScriptEngineSwitcher.ChakraCore.ChakraCoreJsEngine”

## v2.4.17 - May 9, 2018
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.8.4
   * JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm package has been replaced by the JavaScriptEngineSwitcher.ChakraCore.Native.win-arm package

## v2.4.16 - April 13, 2018
 * In JavaScriptEngineSwitcher.V8.Native.win-* and JavaScriptEngineSwitcher.ChakraCore.Native.win-* packages the directories with `win7-*` RIDs was renamed to `win-*`

## v2.4.15 - April 11, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.7
 * In JavaScriptEngineSwitcher.ChakraCore and JavaScriptEngineSwitcher.Vroom fixed a minor errors
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.8.3

## v2.4.14 - March 14, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.8.2

## v2.4.13 - February 24, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.6
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 5.4.10
   * Improved implementation of the `CallFunction` method
   * Removed unnecessary locks from the `V8JsEngine` class
   * In configuration settings of the V8 JS engine was added 3 new properties: `HeapSizeSampleInterval` (default `TimeSpan.Zero`), `MaxHeapSize` (default `UIntPtr.Zero`) and `MaxStackUsage` (default `UIntPtr.Zero`)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of February 24, 2018
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.11.58

## v2.4.12 - February 20, 2018
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.8.1
   * JavaScriptEngineSwitcher.ChakraCore.Native.debian-x64 package has been replaced by the JavaScriptEngineSwitcher.ChakraCore.Native.linux-x64 package
   * ICU-57 library was embedded into the `libChakraCore.so` and `libChakraCore.dylib` assemblies
   * Prevented an occurrence of the “Host may not have set any promise continuation callback. Promises may not be executed.” error
   * In configuration settings of the ChakraCore JS engine was added two new properties - `MemoryLimit` and `DisableFatalOnOOM` (default `false`)
   * Now during calling of the `CollectGarbage` method is no longer performed blocking

## v2.4.11 - December 24, 2017
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.5
 * In JavaScriptEngineSwitcher.ChakraCore fixed a error, that occurred during finding the suitable method overload, that receives numeric values and interfaces as parameters, of the host object

## v2.4.10 - July 4, 2017
 * Now during the rethrowing of exceptions are preserved the full call stack trace
 * In JavaScriptEngineSwitcher.ChakraCore was reduced a number of delegate-wrappers
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.3

## v2.4.9 - June 28, 2017
 * Added support of identifier names compliant with ECMAScript 5
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.2

## v2.4.8 - June 27, 2017
 * In JavaScriptEngineSwitcher.ChakraCore an attempt was made to prevent a blocking of finalizer's thread

## v2.4.7 - June 26, 2017
 * In JavaScriptEngineSwitcher.ChakraCore now the original exception is added to instance of the `JsRuntimeException` class as an inner exception

## v2.4.6 - June 16, 2017
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.5.2

## v2.4.5 - June 8, 2017
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.5.1

## v2.4.4 - May 31, 2017
 * In JavaScriptEngineSwitcher.ChakraCore an attempt was made to prevent occurrence of the access violation exception

## v2.4.3 - May 26, 2017
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.5.0

## v2.4.2 - May 24, 2017
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of May 13, 2017
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of May 24, 2017
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.4.4

## v2.4.1 - April 27, 2017
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.4.3

## v2.4.0 - April 26, 2017
 * Added support of .NET Core 1.0.4
 * In `IJsEngine` interface was added overloaded versions of the `Evaluate`, `Evaluate<T>` and `Execute` methods, which take the document name as second parameter
 * Now all JS engines provide extended information about the error location
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.1
 * In JavaScriptEngineSwitcher.V8:
   * Added support of Microsoft ClearScript.V8 version of April 19, 2017 (support of V8 version 5.5.372.40)
   * Now the `Evaluate` and `Execute` methods of `V8ScriptEngine` class are called with the `discard` parameter equal to `false`
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of March 9, 2017
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.10.4
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.4.2

## v2.3.2 - February 12, 2017
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.1.2
 * In JavaScriptEngineSwitcher.ChakraCore fixed a error causing a crash during finalization

## v2.3.1 - February 10, 2017
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.1.1
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of February 8, 2017
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.4.1

## v2.3.0 - January 16, 2017
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.10.3
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.4.0

## v2.2.0 - December 20, 2016
 * Added support of .NET Core 1.0.3
 * Downgraded .NET Framework version from 4.5.1 to 4.5
 * Now when you call the overloaded version of the `ExecuteResource` method, that takes the type, need to pass the resource name without the namespace
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.1.0
 * In JavaScriptEngineSwitcher.V8 now the Microsoft ClearScript.V8 requires `msvcp140.dll` assembly from the [Visual C++ Redistributable for Visual Studio 2015](http://www.microsoft.com/en-us/download/details.aspx?id=48145)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of December 8, 2016
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Attempt to prevent occurrence of the access violation exception in the `CallFunction` method
   * Fixed a error “Out of stack space”

## v2.1.2 - November 8, 2016
 * Fixed a error #22 [“Make Exception serializable”](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/22)
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.0.1

## v2.1.1 - November 3, 2016
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.8 (support of V8 version 5.4.500.40)
 * In JavaScriptEngineSwitcher.V8.Native.win-x86, JavaScriptEngineSwitcher.V8.Native.win-x64, JavaScriptEngineSwitcher.ChakraCore.Native.win-x86 and JavaScriptEngineSwitcher.ChakraCore.Native.win-x64 packages fixed a compatibility error with the .NET Framework 4.5.1 target in .NET Core Applications

## v2.1.0 - November 2, 2016
 * In JavaScriptEngineSwitcher.V8:
   * Fixed a error, that occurred during parsing of the original error  message
   * Native assemblies have been moved to separate packages: JavaScriptEngineSwitcher.V8.Native.win-x86 and JavaScriptEngineSwitcher.V8.Native.win-x64
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of October 27, 2016
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Fixed a errors, that occurred during marshaling of Unicode strings in Unix-like operating systems
   * Native assemblies for Windows have been moved to separate packages: JavaScriptEngineSwitcher.ChakraCore.Native.win-x86 and JavaScriptEngineSwitcher.ChakraCore.Native.win-x64
   * Added a packages, that contains a native assemblies for Windows (ARM), Debian-based Linux (x64) and OS X (x64)
   * ChakraCore was updated to version of October 29, 2016
   * New version of the ChakraCore for Windows requires `msvcp140.dll` assembly from the [Visual C++ Redistributable for Visual Studio 2015](http://www.microsoft.com/en-us/download/details.aspx?id=48145)

## v2.1.0 Beta 2 - October 31, 2016
 * In JavaScriptEngineSwitcher.ChakraCore added experimental support of Windows (ARM)

## v2.1.0 Beta 1 - October 30, 2016
 * In JavaScriptEngineSwitcher.V8:
   * Fixed a error, that occurred during parsing of the original error  message
   * Native assemblies have been moved to separate packages: JavaScriptEngineSwitcher.V8.Native.win-x86 and JavaScriptEngineSwitcher.V8.Native.win-x64
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Fixed a errors, that occurred during marshaling of Unicode strings in Unix-based operating systems
   * Native assemblies have been moved to separate packages: JavaScriptEngineSwitcher.ChakraCore.Native.win-x86 and JavaScriptEngineSwitcher.ChakraCore.Native.win-x64
   * ChakraCore was updated to version of October 29, 2016
   * New version of the ChakraCore for Windows requires `msvcp140.dll` assembly from the [Visual C++ Redistributable for Visual Studio 2015](http://www.microsoft.com/en-us/download/details.aspx?id=48145)
   * Added the JavaScriptEngineSwitcher.ChakraCore.Native.debian-x64 and JavaScriptEngineSwitcher.ChakraCore.Native.osx-x64 packages

## v2.0.3 - October 17, 2016
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of October 17, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.10.2
 * In JavaScriptEngineSwitcher.ChakraCore was made switch to new ChakraCore API

## v2.0.2 - October 16, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.10.1 and .NET Core 1.0.1

## v2.0.1 - October 6, 2016
 * Added module based on the [VroomJs](http://github.com/pauldotknopf/vroomjs-core). Special thanks to [Daniel Lo Nigro](http://github.com/Daniel15).

## v2.0.0 - September 19, 2016
 * Removed dependency on `System.Configuration.dll` (no longer supported configuration by using the `Web.config` and `App.config` files)
 * In JavaScriptEngineSwitcher.Core, JavaScriptEngineSwitcher.Msie (.NET Core version only works in JsRT modes) and JavaScriptEngineSwitcher.ChakraCore added support of .NET Core 1.0.1
 * Now all modules are support of .NET Framework 4.5.1 and can be used in web applications based on the “ASP.NET Core Web Application (.NET Framework)” template
 * In `IJsEngine` interface was added `SupportsGarbageCollection` property and `CollectGarbage` method
 * `JsRuntimeErrorHelpers` class was renamed to `JsErrorHelpers` class
 * Created a JavaScriptEngineSwitcher.Extensions.MsDependencyInjection package, that contains extension methods for adding the JS engine switcher in an `IServiceCollection`
 * JavaScriptEngineSwitcher.ConfigurationIntelliSense package is no longer required for the current version of the JavaScript Engine Switcher
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.0.0
 * In JavaScriptEngineSwitcher.V8:
   * Added support of Microsoft ClearScript.V8 version 5.4.7 (support of V8 version 5.3.332.45)
   * In configuration settings of the V8 JS engine was changed type of `DebugPort` property from `int` to `ushort`
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of September 19, 2016
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Added support of ChakraCore version 1.3
   * Added the ability to change configuration settings of the ChakraCore JS engine: `DisableBackgroundWork` (default `false`), `DisableNativeCodeGeneration` (default `false`), `DisableEval` (default `false`) and `EnableExperimentalFeatures` (default `false`)

## v2.0.0 Beta 1 - September 17, 2016
 * In `IJsEngine` interface was added `SupportsGarbageCollection` property and `CollectGarbage` method
 * `JsRuntimeErrorHelpers` class was renamed to `JsErrorHelpers` class
 * Added support of .NET Core 1.0.1
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.0.0 Beta 2
 * In JavaScriptEngineSwitcher.V8:
   * Added support of Microsoft ClearScript.V8 version 5.4.7 (support of V8 version 5.3.332.45)
   * In configuration settings of the V8 JS engine was changed type of `DebugPort` property from `int` to `ushort`
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of September 13, 2016
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Added support of ChakraCore version 1.3
   * Added the ability to change configuration settings of the ChakraCore JS engine: `DisableBackgroundWork` (default `false`), `DisableNativeCodeGeneration` (default `false`), `DisableEval` (default `false`) and `EnableExperimentalFeatures` (default `false`)

## v2.0.0 Alpha 2 - September 3, 2016
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.0.0 Alpha 1

## v2.0.0 Alpha 1 - August 23, 2016
 * Removed dependency on `System.Configuration.dll` (no longer supported configuration by using the `Web.config` and `App.config` files)
 * In JavaScriptEngineSwitcher.Core and JavaScriptEngineSwitcher.ChakraCore added support of .NET Core 1.0
 * Now all modules are support of .NET Framework 4.5.1 and can be used in web applications based on the “ASP.NET Core Web Application (.NET Framework)” template
 * Created a JavaScriptEngineSwitcher.Extensions.MsDependencyInjection package, that contains extension methods for adding the JS engine switcher in an `IServiceCollection`
 * JavaScriptEngineSwitcher.ConfigurationIntelliSense package is no longer required for the current version of the JavaScript Engine Switcher

## v1.5.9 - July 27, 2016
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of July 15, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.9.1
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.2

## v1.5.8 - June 30, 2016
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of June 20, 2016 (support of V8 version 5.1.281.65)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 29, 2016

## v1.5.7 - June 16, 2016
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 14, 2016

## v1.5.6 - June 8, 2016
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 4, 2016

## v1.5.5 - May 30, 2016
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.6 (support of V8 version 5.1.281.50)

## v1.5.4 - May 24, 2016
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.7.1
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of May 9, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of May 12, 2016
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version of May 24, 2016

## v1.5.3 - April 13, 2016
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of April 2, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of April 12, 2016

## v1.5.2 - March 10, 2016
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.5 (support of V8 version 4.9.385.30)

## v1.5.1 - March 7, 2016
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of March 5, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of March 6, 2016
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version of March 6, 2016

## v1.5.0 - March 4, 2016
 * Added the `EmbedHostObject` (embeds a instance of simple class, structure or delegate to script code) and `EmbedHostType` (embeds a host type to script code) methods
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of MSIE JavaScript engine
 * Added module based on the ChakraCore
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.7.0 and in configuration settings was added one new property - `EnableDebugging` (default `false`)
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of March 2, 2016
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of March 4, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of March 3, 2016

## v1.5.0 Beta 1 - February 26, 2016
 * Added the `EmbedHostType` method (embeds a host type to script code)
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.7.0 Beta 1
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of February 24, 2016
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of February 18, 2016

## v1.5.0 Alpha 3 - February 5, 2016
 * In JavaScriptEngineSwitcher.ChakraCore added support of the `EmbedHostObject` method

## v1.5.0 Alpha 2 - January 16, 2016
 * Added module based on the [ChakraCore](http://github.com/Microsoft/ChakraCore). JavaScriptEngineSwitcher.ChakraCore does not yet support the `EmbedHostObject` method.
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of MSIE JavaScript engine
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.7.0 Alpha 2 and in configuration settings was added one new property - `EnableDebugging` (default `false`)

## v1.5.0 Alpha 1 - January 5, 2016
 * Added the `EmbedHostObject` method (embeds a instance of simple class, structure or delegate to script code)

## v1.4.1 - December 8, 2015
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.4 (support of V8 version 4.7.80.25)
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of December 2, 2015

## v1.4.0 - December 3, 2015
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of MSIE JavaScript engine
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.6.0 and `ChakraJsRt` mode was renamed to `ChakraIeJsRt`
 * In JavaScriptEngineSwitcher.V8 NuGet package solved the problem with restoring native assemblies

## v1.3.1 - November 5, 2015
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of August 24, 2015
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.7.1

## v1.3.0 - August 27, 2015
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.3 (support of V8 version 4.4.63.29) and now requires assemblies `msvcp120.dll` and `msvcr120.dll` from the [Visual C++ Redistributable Packages for Visual Studio 2013](http://www.microsoft.com/en-us/download/details.aspx?id=40784)

## v1.2.11 - July 23, 2015
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.6
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of July 14, 2015
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of July 8, 2015

## v1.2.10 - June 29, 2015
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.5

## v1.2.9 - June 28, 2015
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of MSIE and Jint JavaScript engines
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.4 and in configuration settings added 2 new properties: `UseEcmaScript5Polyfill` (default `false`) and `UseJson2Library` (default `false`)
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.2.1 (support of V8 version 4.2.77.18)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 23, 2015
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of June 24, 2015 and in configuration settings added one new property - `AllowDebuggerStatement` (default `false`)

## v1.2.8 - May 26, 2015
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.5.0

## v1.2.7 - May 11, 2015
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of V8 JavaScript engine
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.2 (support of V8 version 4.2.77.18)

## v1.2.6 - May 5, 2015
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.3
 * In JavaScriptEngineSwitcher.V8 fixed a [bug #12](http://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/12) “V8 config can be null”
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of April 30, 2015

## v1.2.5 - April 5, 2015
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of Jint JavaScript engine
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.2
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of March 29, 2015 and in configuration settings was changed default value of `MaxRecursionDepth` property from `20678` to `-1`

## v1.2.4 - February 19, 2015
 * In `JsEngineBase` class all public non-abstract methods are now is virtual
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense added definitions for configuration settings of Jurassic and Jint JavaScript engines
 * In JavaScriptEngineSwitcher.Jurassic added the ability to change configuration settings of Jurassic JavaScript engine: `EnableDebugging` (default `false`), `EnableIlAnalysis` (default `false`) and `StrictMode` (default `false`)
 * In JavaScriptEngineSwitcher.Jint added the ability to change configuration settings of Jint JavaScript engine: `EnableDebugging` (default `false`), `MaxRecursionDepth` (default `20678`), `MaxStatements` (default `0`), `StrictMode` (default `false`) and `Timeout` (default `0`)

## v1.2.3 - February 16, 2015
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense added definitions for configuration settings of V8 JavaScript engine
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.1 (support of V8 version 3.30.33.16) and added the ability to change configuration settings of V8 JavaScript engine: `EnableDebugging` (default `false`), `DebugPort` (default `9222`), `DisableGlobalMembers` (default `false`), `MaxNewSpaceSize` (default `0`), `MaxOldSpaceSize` (default `0`) and `MaxExecutableSize` (default `0`)
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of February 14, 2015

## v1.2.2 - January 13, 2015
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.1

## v1.2.1 - October 22, 2014

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.0 (support of V8 version 3.26.31.15)
* In JavaScriptEngineSwitcher.Jint added support of Jint version of October 21, 2014

## v1.2.0 - October 13, 2014

* From JavaScriptEngineSwitcher.V8 and JavaScriptEngineSwitcher.Jint removed dependency on `System.Web.Extensions.dll`
* All assemblies is now targeted on the .NET Framework 4 Client Profile
* In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.0
* In JavaScriptEngineSwitcher.Jint added support of Jint version of October 9, 2014

## v1.1.13 - September 17, 2014

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of September 6, 2014
* In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of September 5, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of September 16, 2014

## v1.1.12 - August 19, 2014

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of August 1, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of August 16, 2014

## v1.1.11 - July 22, 2014

 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.4

## v1.1.10 - July 10, 2014

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of July 5, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version 2.2.0

## v1.1.9 - June 14, 2014

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of June 10, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of June 5, 2014

## v1.1.8 - May 20, 2014

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of May 17, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of May 16, 2014

## v1.1.7 - May 7, 2014

 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of April 29, 2014

## v1.1.6 - April 27, 2014

 * In solution was enabled NuGet package restore
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.3
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of April 26, 2014

## v1.1.5 - April 5, 2014

 * In JavaScriptEngineSwitcher.Jint added support of Jint version of April 5, 2014

## v1.1.4 - March 24, 2014

 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.2

## v1.1.3 - March 22, 2014

 * Added module based on the [Jint](http://github.com/sebastienros/jint). Special thanks to [Daniel Lo Nigro](http://github.com/Daniel15) for the idea of this module. 
 * In JavaScriptEngineSwitcher.Core fixed minor bugs
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.1

## v1.1.2 - February 27, 2014

 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.0

## v1.1.1 - January 17, 2014

 * In JavaScriptEngineSwitcher.V8 added support of the [ClearScript](http://github.com/Microsoft/ClearScript) version 5.3.11 (support of V8 version 3.24.17)

## v1.1.0 - January 16, 2014

 * In JavaScriptEngineSwitcher.Msie added support of [MSIE JavaScript Engine](http://github.com/Taritsyn/MsieJavaScriptEngine) version 1.3.0
 * In JavaScriptEngineSwitcher.V8 improved performance of `CallFunction` method
 * In JavaScriptEngineSwitcher.Jurassic added support of [Jurassic](http://github.com/paulbartrum/jurassic) version of January 11, 2014

## v1.0.0 - December 30, 2013

 * Added support of JavaScript `undefined` type
 * In JavaScriptEngineSwitcher.Msie added support of [MSIE JavaScript Engine](http://github.com/Taritsyn/MsieJavaScriptEngine) version 1.2.0

## v0.9.5 - December 7, 2013

 * In JavaScriptEngineSwitcher.V8 the [Noesis Javascript .NET](http://github.com/JavascriptNet/Javascript.Net) was replaced by the [Microsoft ClearScript.V8](http://github.com/Microsoft/ClearScript) library (solves a problem of `V8JsEngine` stable work on 64-bit version of IIS 8.X)
 * In JavaScriptEngineSwitcher.Jurassic added support of [Jurassic](http://github.com/paulbartrum/jurassic) version of September 30, 2013

## v0.9.3 - November 19, 2013

 * In JavaScriptEngineSwitcher.V8 changed a mechanism for loading the Noesis Javascript .NET assemblies for different processor architectures
 * Deleted a `/configuration/jsEngineSwitcher/v8` configuration section

## v0.9.2 - September 5, 2013

 * Added JavaScriptEngineSwitcher.ConfigurationIntelliSense NuGet package
 
## v0.9.0 - September 4, 2013
 * Initial version uploaded