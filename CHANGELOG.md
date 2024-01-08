Change log
==========

## v3.23.7 - January 8, 2024
 * In JavaScriptEngineSwitcher.ChakraCore fixed a error that occurred in the `ReflectionHelpers.IsAllowedProperty` method when running on .NET Core 1.0
 * In JavaScriptEngineSwitcher.Msie added support for the MSIE JavaScript Engine version 3.2.4

## v3.23.6 - January 6, 2024
 * In JavaScriptEngineSwitcher.Jint added support for the Jint version 3.0.0 Beta 2059

## v3.23.5 - December 9, 2023
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 2057
   * Added support for .NET 8
 * In JavaScriptEngineSwitcher.Msie added support for the MSIE JavaScript Engine version 3.2.3
 * In JavaScriptEngineSwitcher.NiL added support for the NiL.JS version 2.5.1677

## v3.23.4 - November 11, 2023
 * In JavaScriptEngineSwitcher.Jint added support for the Jint version 3.0.0 Beta 2055
 * In JavaScriptEngineSwitcher.Yantra added support for the YantraJS version 1.2.206

## v3.23.3 - November 6, 2023
 * In JavaScriptEngineSwitcher.Jint added support for the Jint version 3.0.0 Beta 2054
 * In JavaScriptEngineSwitcher.Jurassic added support for the Jurassic version of November 1, 2023
 * In JavaScriptEngineSwitcher.Yantra added support for the YantraJS version 1.2.204

## v3.23.2 - October 26, 2023
 * In JavaScriptEngineSwitcher.Jint added support for the Jint version 3.0.0 Beta 2053
 * In JavaScriptEngineSwitcher.NiL added support for the NiL.JS version 2.5.1674
 * In JavaScriptEngineSwitcher.V8 added support for the Microsoft ClearScript.V8 version 7.4.4 (support of the V8 version 11.8.172.15)
 * In JavaScriptEngineSwitcher.Yantra added support for the YantraJS version 1.2.195

## v3.23.1 - September 19, 2023
 * In JavaScriptEngineSwitcher.Jint added support for the Jint version 3.0.0 Beta 2052
 * In JavaScriptEngineSwitcher.Node added support for the Jering.Javascript.NodeJS version 7.0.0
 * In JavaScriptEngineSwitcher.Yantra added support for the YantraJS version 1.2.188

## v3.23.0 - September 8, 2023
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1673
   * Restored support for .NET Framework 4.6.1

## v3.22.0 - September 5, 2023
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1672
   * No longer supports a .NET Framework 4.6.1

## v3.21.6 - August 30, 2023
 * In JavaScriptEngineSwitcher.Jint added support for the Jint version 3.0.0 Beta 2051

## v3.21.5 - August 21, 2023
 * In JavaScriptEngineSwitcher.V8 added support for the Microsoft ClearScript.V8 version 7.4.3 (support of the V8 version 11.6.189.18)

## v3.21.4 - August 3, 2023
 * In JavaScriptEngineSwitcher.Jint added support for the Jint version 3.0.0 Beta 2050
 * In JavaScriptEngineSwitcher.NiL added support for the NiL.JS version 2.5.1665
 * In JavaScriptEngineSwitcher.Node added support for the Jering.Javascript.NodeJS version 7.0.0 Beta 5
 * In JavaScriptEngineSwitcher.Yantra added support for the YantraJS version 1.2.179

## v3.21.3 - June 1, 2023
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.4.2 (support of V8 version 11.4.183.17)
 * In JavaScriptEngineSwitcher.Yantra added support of YantraJS version 1.2.163

## v3.21.2 - May 1, 2023
 * In JavaScriptEngineSwitcher.Jint:
   * The package is no longer marked as a prerelease
   * In configuration settings of the Jint JS engine was added one new property - `MaxJsonParseDepth` (default `64`)
 * In JavaScriptEngineSwitcher.Node:
   * Jering.Javascript.NodeJS was updated to version 7.0.0 Beta 4
   * Added support of .NET 7

## v3.21.1 - April 11, 2023
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 2049
   * Added support of .NET 6
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.2.2
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.4.1 (support of V8 version 11.2.214.13)
 * In JavaScriptEngineSwitcher.Yantra added support of YantraJS version 1.2.160

## v3.21.0 - April 1, 2023
 * Added a module based on the [YantraJS](https://yantrajs.com). Special thanks to [Akash Kava](https://github.com/ackava)
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version of January 26, 2023
   * In configuration settings of the ChakraCore JS engine was added one new property - `AllowReflection` (default `false`)
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 2048
   * In configuration settings of the Jint JS engine was added two new properties: `AllowReflection` (default `false`) and `DisableEval` (default `false`)
 * In JavaScriptEngineSwitcher.Jurassic improved a conversion of results to a host types
 * In JavaScriptEngineSwitcher.Msie:
   * MSIE JavaScript Engine was updated to version 3.2.1
   * In configuration settings of the MSIE JS engine was added one new property - `AllowReflection` (default `false`)
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1661
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.4.0 (support of V8 version 11.1.277.14)
   * In configuration settings of the V8 JS engine was added one new property - `AllowReflection` (default `false`)

## v3.20.10 - January 23, 2023
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.3.7 (support of V8 version 10.9.194.10)

## v3.20.9 - January 19, 2023
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2046
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1623
   * Added support of .NET 7.0

## v3.20.8 - December 20, 2022
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2044
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.3.6 (support of V8 version 10.8.168.24)

## v3.20.7 - November 13, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.3.5 (support of V8 version 10.7.193.22)

## v3.20.6 - November 11, 2022
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version of November 9, 2022
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2043

## v3.20.5 - October 12, 2022
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version of October 7, 2022
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of September 20, 2022

## v3.20.4 - September 30, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.3.4 (support of V8 version 10.6.194.14)

## v3.20.3 - September 28, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.3.3 (support of V8 version 10.6.194.14)

## v3.20.2 - September 16, 2022
 * In JavaScriptEngineSwitcher.Jint the implementation of script interruption has been refactored
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.3.2 (support of V8 version 10.5.218.8)
   * In configuration settings of the V8 JS engine was added one new property - `DisableDynamicBinding` (default `false`)

## v3.20.1 - September 11, 2022
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2041
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1600
   * JS run-time exception now contains a script call stack

## v3.20.0 - August 31, 2022
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1591
   * No longer supports a .NET Standard

## v3.19.1 - August 28, 2022
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2040

## v3.19.0 - July 21, 2022
 * Fixed a [error #102](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/102) “Resources should conform to correct ICU standard for naming”. Special thanks to [Tim Heuer](https://github.com/timheuer)
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 2039
   * No longer supports a .NET Framework 4.6.1
   * Added support of .NET Framework 4.6.2
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.9

## v3.18.4 - June 29, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.3.1 (support of V8 version 10.3.174.17)

## v3.18.3 - June 3, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.3.0 (support of V8 version 10.2.154.5)

## v3.18.2 - May 24, 2022
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version of January 31, 2022
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1560
 * In JavaScriptEngineSwitcher.Node added support of Jering.Javascript.NodeJS version 6.3.1

## v3.18.1 - May 3, 2022
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of April 30, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.2.5 (support of V8 version 10.1.124.11)

## v3.18.0 - April 21, 2022
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 2038
   * In configuration settings of the Jint JS engine were changed types of the `DebuggerBreakCallback` and `DebuggerStepCallback` properties to the
   <code title="Jint.Runtime.Debugger.DebugHandler.DebugEventHandler">DebugEventHandler</code> type

## v3.17.4 - March 30, 2022
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of March 12, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.2.4 (support of V8 version 10.0.139.8)

## v3.17.3 - March 6, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.2.3 (support of V8 version 9.9.115.8)

## v3.17.2 - February 7, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.2.2 (support of V8 version 9.8.177.9)

## v3.17.1 - January 11, 2022
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.2.1 (support of V8 version 9.7.106.18)

## v3.17.0 - December 27, 2021
 * In JavaScriptEngineSwitcher.Node:
   * Jering.Javascript.NodeJS was updated to version 6.3.0
   * Added support of .NET 6

## v3.16.0 - December 9, 2021
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version of November 11, 2021
   * No longer supports a .NET Core App 2.1
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2037
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1552

## v3.15.0 - November 28, 2021
 * In JavaScriptEngineSwitcher.Node:
   * Jering.Javascript.NodeJS was updated to version 6.2.0
   * Added support of .NET Core App 3.1 and .NET 5.0

## v3.14.0 - November 23, 2021
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2036
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1541
   * No longer supports a .NET Framework 4.0 Client and .NET Framework 4.5
   * Added support of .NET Framework 4.8 and .NET 6.0
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.2.0 (support of V8 version 9.6.180.14)

## v3.13.3 - October 21, 2021
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.1.7 (support of V8 version 9.5.172.21)

## v3.13.2 - October 6, 2021
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 2035
   * In configuration settings of the Jint JS engine was added one new property - `MaxArraySize` (default `uint.MaxValue`)
 * In JavaScriptEngineSwitcher.V8 implemented a handling of error “Internal error. Icu error”

## v3.13.1 - September 22, 2021
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2034
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.1.6 (support of V8 version 9.4.146.16)

## v3.13.0 - August 19, 2021
 * In JavaScriptEngineSwitcher.Jurassic:
   * Jurassic was updated to version of August 18, 2021
   * Debugging is no longer supported

## v3.12.6 - August 10, 2021
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Improved a implementation of the `Dispose` method
   * MSBuild and PowerShell scripts for installing the native assemblies are made more simple and universal
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.8

## v3.12.5 - July 23, 2021
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2033
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.1.5 (support of V8 version 9.2.230.21)
   * In configuration settings of the V8 JS engine was added one new property - `MaxArrayBufferAllocation` (default `UInt64.MaxValue`)

## v3.12.4 - June 29, 2021
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1514
   * Added support of .NET Core App 3.1
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.1.4 (support of V8 version 9.1.269.36)
   * Added support of OS X (ARM64)

## v3.12.3 - May 26, 2021
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.1.3 (support of V8 version 9.1.269.28)

## v3.12.2 - May 20, 2021
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2032

## v3.12.1 - April 18, 2021
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1493
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.1.2 (support of V8 version 9.0.257.19)

## v3.12.0 - March 28, 2021
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of March 19, 2021

## v3.11.4 - March 19, 2021
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1486
   * Added support of .NET 5.0

## v3.11.3 - March 9, 2021
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 2031

## v3.11.2 - March 5, 2021
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1475
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.1.1 (support of V8 version 8.9.255.20)
   * Added support of .NET Standard 2.1
   * Added support of Linux (ARM)
   * In configuration settings of the V8 JS engine was added one new property - `HeapExpansionMultiplier` (default `0`)

## v3.11.1 - February 1, 2021
 * In JavaScriptEngineSwitcher.Jint:
   * Runtime exceptions now contain a stack trace
   * In configuration settings of the Jint JS engine a `AllowDebuggerStatement` property has been returned so as not to break compatibility with previous versions

## v3.11.0 - January 30, 2021
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 2002
   * In configuration settings of the Jint JS engine a `AllowDebuggerStatement` property has been replaced by the `DebuggerStatementHandlingMode` property (default `Ignore`) and was added two new properties: `DebuggerBreakCallback` (default `null`) and `DebuggerStepCallback` (default `null`)

## v3.10.0 - January 22, 2021
 * In JavaScriptEngineSwitcher.Node added support of Jering.Javascript.NodeJS version 5.4.4
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.1 (support of V8 version 8.8.278.14)
   * Added support of Windows (ARM64) and Linux (ARM64)

## v3.9.1 - December 9, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.24 ([Tomáš Deml's patch](https://github.com/microsoft/ChakraCore/issues/5973) applied)

## v3.9.0 - November 19, 2020
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.0 (support of V8 version 8.7.220.25)
   * Added support of .NET Framework 4.7.1 and .NET 5.0
   * Added support of Linux (x64) and OS X (x64)
   * Own versions of the ClearScript's assemblies are no longer build, because the [official NuGet package](https://www.nuget.org/packages/Microsoft.ClearScript.V8) is now used. Therefore, you should also replace in your projects the `JavaScriptEngineSwitcher.V8.Native.*` packages by the `Microsoft.ClearScript.V8.Native.*` packages.

## v3.8.5 - November 11, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.23 ([Tomáš Deml's patch](https://github.com/microsoft/ChakraCore/issues/5973) applied)

## v3.8.4 - November 7, 2020
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 1914

## v3.9.0 Preview 3 - November 6, 2020
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.0 RC5 (support of V8 version 8.7.220.16)
   * Own versions of the ClearScript's assemblies are no longer build, because the [official NuGet package](https://www.nuget.org/packages/Microsoft.ClearScript.V8) is now used. Therefore, you should also replace in your projects the `JavaScriptEngineSwitcher.V8.Native.*` packages by the `Microsoft.ClearScript.V8.Native.*` packages.

## v3.9.0 Preview 2 - October 30, 2020
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.0 RC4 (support of V8 version 8.7.220.10)

## v3.8.3 - October 29, 2020
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.0 RC4 (support of V8 version 8.7.220.10)

## v3.9.0 Preview - October 24, 2020
 * In JavaScriptEngineSwitcher.V8:
   * Cross-platform is implemented by using an [unofficial experimental version of the Microsoft ClearScript.V8](https://github.com/Taritsyn/ClearScript-Experimental) library, which is not targeted at any particular operating system or processor architecture, and can work with various native assemblies
   * Added a packages, that contains a native assemblies for Linux (x64) and OS X (x64)

## v3.8.2 - October 23, 2020
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.0 RC3

## v3.8.1 - October 21, 2020
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 7.0 RC2

## v3.8.0 - October 19, 2020
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 7.0 RC (support of V8 version 8.6.395.17)
   * MSVC runtime was embedded into the native assemblies for Windows

## v3.7.2 - September 9, 2020
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.22 ([Tomáš Deml's patch](https://github.com/microsoft/ChakraCore/issues/5973) applied)
   * Added a experimental support of Windows (ARM64)
 * In JavaScriptEngineSwitcher.Node added support of Jering.Javascript.NodeJS version 5.4.3

## v3.7.1 - August 12, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.21 ([Tomáš Deml's patch](https://github.com/microsoft/ChakraCore/issues/5973) applied)

## v3.7.0 - August 6, 2020
 * In JavaScriptEngineSwitcher.Jurassic:
   * Jurassic was updated to version of August 3, 2020
   * In configuration settings of the Jurassic JS engine was added one new non-standard property - `EnableHostCollectionsEmbeddingByValue` (default `false`)

## v3.6.0 - August 1, 2020
 * In JavaScriptEngineSwitcher.Jint added a ability to interrupt execution of the script
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1466
 * In JavaScriptEngineSwitcher.Node added support of Jering.Javascript.NodeJS version 5.4.2

## v3.5.6 - June 11, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.20 ([Tomáš Deml's patch](https://github.com/microsoft/ChakraCore/issues/5973) applied)
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 1828

## v3.5.5 - May 29, 2020
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 6.0.2 (support of V8 version 8.3.110.9)

## v3.5.4 - May 13, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.19 ([Tomáš Deml's patch](https://github.com/microsoft/ChakraCore/issues/5973) applied)

## v3.5.3 - May 8, 2020
 * In JavaScriptEngineSwitcher.V8 fixed a error that caused incorrect generation of error description for an `JsEngineLoadException` exception

## v3.5.2 - April 15, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.18 ([Tomáš Deml's patch](https://github.com/microsoft/ChakraCore/issues/5973) applied)

## v3.5.1 - April 14, 2020
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 1778
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 6.0.1 (support of V8 version 8.1.307.28)

## v3.5.0 - April 3, 2020
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 1756
   * No longer supports a .NET Framework 4.5
   * Added support of .NET Framework 4.6.1 and .NET Standard 2.1

## v3.4.5 - April 1, 2020
 * In JavaScriptEngineSwitcher.ChakraCore applied the Tomáš Deml's patch to fix the [“Incompatibility in handling of SIGSEGV between ChakraCore and CoreCLR”](https://github.com/microsoft/ChakraCore/issues/5973) error
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1440
 * In JavaScriptEngineSwitcher.Node added a handling of errors that occur when switching to multi-process mode of the Jering.Javascript.NodeJS library

## v3.4.4 - March 11, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.17
 * In JavaScriptEngineSwitcher.Node added support of Jering.Javascript.NodeJS version 5.4.0

## v3.4.3 - March 8, 2020
 * In JavaScriptEngineSwitcher.ChakraCore fixed a [error #82](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/82) “Program crash after function call with too much parameters”
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1431
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.7

## v3.4.2 - March 2, 2020
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 1715
   * Simplified and improved a handling of JS runtime errors

## v3.4.1 - February 26, 2020
 * In JavaScriptEngineSwitcher.Node the default Node JS service is now implemented as a wrapper around the <code title="Jering.Javascript.NodeJS.StaticNodeJSService">StaticNodeJSService</code> class

## v3.4.0 - February 24, 2020
 * Added a module based on the [Jering.Javascript.NodeJS](https://github.com/JeringTech/Javascript.NodeJS)

## v3.3.3 - February 15, 2020
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1428
   * NiL.JS.NetCore package is no longer used
   * Added support of .NET Framework 4.6.1 and .NET Standard 1.6

## v3.3.2 - February 14, 2020
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.16

## v3.3.1 - February 1, 2020
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1403
 * Added a `ClearScript.xml` files to the JavaScriptEngineSwitcher.V8 package

## v3.3.0 - December 27, 2019
 * Enabled a SourceLink in NuGet packages
 * In JavaScriptEngineSwitcher.ChakraCore added support of .NET Standard 2.1
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 6.0.0 (support of V8 version 7.9.317.32)
   * Added support of .NET Core 3.1 on Windows
   * Now the Microsoft ClearScript.V8 requires the [Microsoft Visual C++ Redistributable for Visual Studio 2019](https://visualstudio.microsoft.com/downloads/#microsoft-visual-c-redistributable-for-visual-studio-2019)

## v3.2.4 - December 17, 2019
 * In JavaScriptEngineSwitcher.ChakraCore fixed a errors leading to null reference exceptions in the `ReflectionHelpers` class. Special thanks to [Vanjoge](https://github.com/vanjoge).
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.6

## v3.2.3 - November 15, 2019
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.15
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 1632
 * In JavaScriptEngineSwitcher.NiL:
   * NiL.JS was updated to version 2.5.1388
   * In configuration settings of the NiL JS engine was added one new property - `LocalTimeZone` (default `TimeZoneInfo.Local`)

## v3.2.2 - October 26, 2019
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 3.0.0 Beta 1629

## v3.2.1 - October 21, 2019
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Fixed a error that caused a crash during finalization
   * During calling of the `CollectGarbage` method is again not performed blocking
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.5

## v3.2.0 - October 12, 2019
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 3.0.0 Beta 1612. Special thanks to [Marko Lahma](https://github.com/lahma) and [Sébastien Ros](https://github.com/sebastienros)
   * No longer supports a .NET Framework 4.0 Client and .NET Standard 1.3
   * In configuration settings of the Jint JS engine was added two new properties: `MemoryLimit` (default `0`) and `RegexTimeoutInterval` (default `null`)
   * To install this package, the “Include Prerelease” option must be set in the NuGet Package Manager

## v3.1.10 - October 9, 2019
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.14
   * Slightly improved performance
   * The `CollectGarbage` method is called synchronously again
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.4

## v3.1.9 - September 17, 2019
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1372

## v3.1.8 - September 12, 2019
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.13

## v3.1.7 - August 14, 2019
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.12
   * MSVC runtime was embedded into the native assemblies for Windows

## v3.1.6 - August 5, 2019
 * In JavaScriptEngineSwitcher.V8 fixed a [error #73](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/73) “Trying to Run this in the GAC”

## v3.1.5 - August 2, 2019
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.6.0 (support of V8 version 7.6.303.28)

## v3.1.4 - July 17, 2019
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1339

## v3.1.3 - July 10, 2019
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.11

## v3.1.2 - June 13, 2019
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.10

## v3.1.1 - May 15, 2019
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.9

## v3.1.0 - May 3, 2019
 * In the `JsEngineFactoryCollection` class was added a `Count` property and `GetRegisteredFactories` method
 * Removed a deprecated packages: JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm and JavaScriptEngineSwitcher.ChakraCore.Native.debian-x64
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.5.6 (support of V8 version 7.4.288.26)

## v3.0.10 - April 29, 2019
 * In JavaScriptEngineSwitcher.ChakraCore in configuration settings was changed default value of `DisableFatalOnOOM` property from `false` to `true`
 * In JavaScriptEngineSwitcher.Jint fixed a error that occurred during the recursive execution and evaluation of JS files
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.3
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1327

## v3.0.9 - April 10, 2019
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.8

## v3.0.8 - April 9, 2019
 * In JavaScriptEngineSwitcher.ChakraCore fixed a [error #72](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/72) “(chakra) recursive evaluation”
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1320

## v3.0.7 - March 13, 2019
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.7
   * Fixed a [error #68](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/68) “Embedded delegates are no longer linked with the `Function` prototype”
 * In JavaScriptEngineSwitcher.Jint fixed a error that occurs in the strict mode when generating an error message
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.2

## v3.0.6 - February 15, 2019
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.6

## v3.0.5 - February 8, 2019
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.5.5 (support of V8 version 7.2.502.25)

## v3.0.4 - February 1, 2019
 * In JavaScriptEngineSwitcher.ChakraCore in error message fixed a link to the documentation

## v3.0.3 - January 29, 2019
 * In JavaScriptEngineSwitcher.ChakraCore improved a performance of the `UnicodeToAnsi` method of `EncodingHelpers` class
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1294

## v3.0.2 - January 24, 2019
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Reduced a memory consumption in cases, where not used the embedding of objects and types
   * Fixed a wrong implementation of destruction of the embedded delegates
   * Accelerated a conversion of script types to host types
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.1

## v3.0.1 - January 11, 2019
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.5
   * Fixed a [error #65](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/65) “Memory leak using EmbedHostObject”

## v3.0.0 - December 25, 2018
 * In the `JsEngineSwitcher` class a `Instance` property was renamed to the `Current` property
 * Now you can implement your own version of the `JsEngineSwitcher` class
 * Format of the error messages was unified
 * Created a new exception classes: `JsCompilationException`, `JsEngineException`, `JsFatalException`, `JsInterruptedException`, `JsTimeoutException`, `JsScriptException` and `JsUsageException`. These exceptions are responsible for handling errors, some of which were previously handled by the `JsRuntimeException` class
 * In the `JsException` class was added two new properties: `Category` and `Description`
 * From the `JsRuntimeException` class was removed one property - `ErrorCode`
 * In the `JsRuntimeException` class was added three new properties: `Type`, `DocumentName` and `CallStack`
 * `JsEngineLoadException` class now is inherited from the `JsEngineException` class
 * Removed a `EmptyValueException` class
 * `Format` method of the `JsErrorHelpers` class was renamed to the `GenerateErrorDetails`
 * Part of the auxiliary code was moved to external libraries: [PolyfillsForOldDotNet](https://github.com/Taritsyn/PolyfillsForOldDotNet) and [AdvancedStringBuilder](https://github.com/Taritsyn/AdvancedStringBuilder)
 * In `IJsEngine` interface was added two new  properties: `SupportsScriptInterruption` and `SupportsScriptPrecompilation`, and four new methods: `Interrupt`, `Precompile`, `PrecompileFile` and `PrecompileResource`
 * In JavaScriptEngineSwitcher.Extensions.MsDependencyInjection added a overloaded versions of the `AddJsEngineSwitcher` extension method, which takes an instance of JS engine switcher
 * In JavaScriptEngineSwitcher.ChakraCore, JavaScriptEngineSwitcher.Msie, JavaScriptEngineSwitcher.V8 and JavaScriptEngineSwitcher.Vroom added a ability to interrupt execution of the script
 * In JavaScriptEngineSwitcher.ChakraCore, JavaScriptEngineSwitcher.Jint, JavaScriptEngineSwitcher.Jurassic, JavaScriptEngineSwitcher.Msie and JavaScriptEngineSwitcher.V8 added a ability to pre-compile scripts
 * In all modules, except the JavaScriptEngineSwitcher.V8, added support of .NET Standard 2.0
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.4
   * No longer used the old ChakraCore API for Windows (Internet Explorer-like API)
   * Now the ChakraCore for Windows requires the [Microsoft Visual C++ Redistributable for Visual Studio 2017](https://www.visualstudio.com/downloads/#microsoft-visual-c-redistributable-for-visual-studio-2017)
   * In configuration settings of the ChakraCore JS engine was added one new property - `MaxStackSize` (default `492` or `984` KB)
   * Added support of .NET Framework 4.7.1 and .NET Core App 2.1
 * In JavaScriptEngineSwitcher.Jint:
   * Jint was updated to version 2.11.58
   * In configuration settings of the Jint JS engine a `Timeout` property has been replaced by the `TimeoutInterval` property (default `TimeSpan.Zero`) and was added one new property - `LocalTimeZone` (default `TimeZoneInfo.Local`)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of February 24, 2018
 * In JavaScriptEngineSwitcher.Msie:
   * MSIE JavaScript Engine was updated to version 3.0.0
   * In configuration settings of the MSIE JS engine was added one new property - `MaxStackSize` (default `492` or `984` KB)
 * In JavaScriptEngineSwitcher.V8:
   * Microsoft ClearScript.V8 was updated to version 5.5.4 (support of V8 version 7.0.276.42)
   * Now requires .NET Framework 4.5 or higher
   * Now the Microsoft ClearScript.V8 requires the [Microsoft Visual C++ Redistributable for Visual Studio 2017](https://www.visualstudio.com/downloads/#microsoft-visual-c-redistributable-for-visual-studio-2017)
   * In configuration settings of the V8 JS engine became obsolete the `MaxExecutableSize` property and added two new properties: `AwaitDebuggerAndPauseOnStart` (default `false`) and `EnableRemoteDebugging` (default `false`)
 * In JavaScriptEngineSwitcher.Vroom added support of .NET Framework 4.7.1
 * Added a module based on the [NiL.JS](https://github.com/nilproject/NiL.JS)

## v3.0.0 RC 3 - December 18, 2018
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.4
   * Improved a performance of script pre-compilation

## v3.0.0 RC 2 - December 4, 2018
 * `GetSourceFragmentFromCode` and `GetSourceFragmentFromLine` methods of `JsErrorHelpers` class were replaced by the `GetTextFragment` and `GetTextFragmentFromLine` methods of `TextHelpers` class
 * Part of the auxiliary code was moved to external libraries: [PolyfillsForOldDotNet](https://github.com/Taritsyn/PolyfillsForOldDotNet) and [AdvancedStringBuilder](https://github.com/Taritsyn/AdvancedStringBuilder)
 * Removed a unnecessary `net471` targets
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.0 RC 2
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.5.4 (support of V8 version 7.0.276.42)
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.3
   * In the `NativeMethods` class for the `netstandard` and `netcoreapp` targets was changed a calling convention from `StdCall` to `Cdecl`
   * Charset was explicitly specified in the `JsCreateString`, `JsCopyString` and `JsCreatePropertyId` methods of `NativeMethods` class
   * Added a `netcoreapp2.1` target
 *  In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1282

## v2.4.29 - November 20, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.10
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.3

## v2.4.28 - October 13, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.2

## v2.4.27 - September 21, 2018
 * In JavaScriptEngineSwitcher.ChakraCore was optimized a memory usage in version for Unix

## v3.0.0 RC 1 - September 19, 2018
 * Fixed a [error #59](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/59) “Unhandled exception when JS exception is thrown”
 * In JavaScriptEngineSwitcher.Msie:
   * Added support of MSIE JavaScript Engine version 3.0.0 RC 1
   * In configuration settings of the MSIE JS engine was added one new property - `MaxStackSize` (default `492` or `984` KB)
 * In JavaScriptEngineSwitcher.Jint in configuration settings was added one new property - `LocalTimeZone` (default `TimeZoneInfo.Local`)
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.11.1
   * No longer used the old ChakraCore API for Windows (Internet Explorer-like API)
   * Optimized a memory usage
   * `MaxStackSize` configuration property was removed from the version for .NET Standard 1.3

## v2.4.26 - September 13, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.1

## v2.4.25 - September 8, 2018
 * In JavaScriptEngineSwitcher.Jint in configuration settings was added one new property - `LocalTimeZone` (default `TimeZoneInfo.Local`)
 * In JavaScriptEngineSwitcher.ChakraCore improved a performance in version for Unix

## v2.4.24 - August 30, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.11.0

## v3.0.0 Beta 9 - August 21, 2018
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.5.3 (support of V8 version 6.8.275.28)
 * In JavaScriptEngineSwitcher.Jurassic the original library was rolled back to version of February 24, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.10.2
 * In JavaScriptEngineSwitcher.NiL:
   * Added support of NiL.JS version 2.5.1278
   * Fixed a [error #57](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/57) “Nil - can't locate function name”

## v2.4.23 - August 20, 2018
 * In JavaScriptEngineSwitcher.Jurassic the original library was rolled back to version of February 24, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.10.2

## v2.4.22 - July 11, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.10.1

## v3.0.0 Beta 8 - July 7, 2018
 * ChakraCore was updated to version 1.10.0
 * In configuration settings of the ChakraCore JS engine was added one new property - `DisableExecutablePageAllocation` (default `false`)

## v2.4.21 - July 2, 2018
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.10.0
   * In configuration settings of the ChakraCore JS engine was added one new property - `DisableExecutablePageAllocation` (default `false`)

## v3.0.0 Beta 7 - June 18, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.8.5

## v2.4.20 - June 13, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.8.5

## v3.0.0 Beta 6 - June 12, 2018
 * In JavaScriptEngineSwitcher.Extensions.MsDependencyInjection added a overloaded versions of the `AddJsEngineSwitcher` extension method, which takes an instance of JS engine switcher
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 7, 2018

## v2.4.19 - June 12, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.9
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 7, 2018
 * In JavaScriptEngineSwitcher.ChakraCore changed a implementation of the `Dispose` method

## v3.0.0 Beta 5 - June 6, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.0 Beta 4
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Changed a implementation of the `Dispose` method
   * Prevented a early destruction of delegates, which have been passed to the native methods
 * In JavaScriptEngineSwitcher.NiL added support of NiL.JS version 2.5.1275

## v3.0.0 Beta 4 - May 29, 2018
 * Fixed a [error #54](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/54) “System.Runtime.InteropServices.RuntimeInformation not required for .NET Framework 4.7.1”. Special thanks to [David Gardiner](https://github.com/flcdrg).
 * Now all modules are support of .NET Framework 4.7.1
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.0 Beta 3
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Fixed a implementation of the `JsSerializedLoadScriptCallback` delegate
   * Fixed a [error #34](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/34) “Finalazier thread is blocked because of JavaScriptEngineSwitcher.ChakraCore.ChakraCoreJsEngine”

## v2.4.18 - May 24, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.8
 * In JavaScriptEngineSwitcher.ChakraCore fixed a [error #34](https://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/34) “Finalazier thread is blocked because of JavaScriptEngineSwitcher.ChakraCore.ChakraCoreJsEngine”

## v3.0.0 Beta 3 - May 22, 2018
 * In `IJsEngine` interface was added `SupportsScriptPrecompilation` property and three new methods: `Precompile`, `PrecompileFile` and `PrecompileResource`
 * In JavaScriptEngineSwitcher.Msie, JavaScriptEngineSwitcher.V8, JavaScriptEngineSwitcher.Jurassic, JavaScriptEngineSwitcher.Jint and JavaScriptEngineSwitcher.ChakraCore added a ability to pre-compile scripts
 * Added a module based on the NiL.JS
 * In JavaScriptEngineSwitcher.V8.Native.win-* and JavaScriptEngineSwitcher.ChakraCore.Native.win-* packages the directories with `win7-*` RIDs was renamed to `win-*`
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.0 Beta 2
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.8.4
   * JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm package has been replaced by the JavaScriptEngineSwitcher.ChakraCore.Native.win-arm package

## v2.4.17 - May 9, 2018
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.8.4
   * JavaScriptEngineSwitcher.ChakraCore.Native.win8-arm package has been replaced by the JavaScriptEngineSwitcher.ChakraCore.Native.win-arm package

## v2.4.16 - April 13, 2018
 * In JavaScriptEngineSwitcher.V8.Native.win-* and JavaScriptEngineSwitcher.ChakraCore.Native.win-* packages the directories with `win7-*` RIDs was renamed to `win-*`

## v3.0.0 Beta 2 - April 12, 2018
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.8.3

## v2.4.15 - April 11, 2018
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.7
 * In JavaScriptEngineSwitcher.ChakraCore and JavaScriptEngineSwitcher.Vroom fixed a minor errors
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.8.3

## v3.0.0 Beta 1 - April 8, 2018
 * Format of the error messages was unified
 * Created a new exception classes: `JsCompilationException`, `JsEngineException`, `JsFatalException`, `JsTimeoutException` and `JsUsageException`. These exceptions are responsible for handling errors, some of which were previously handled by the `JsRuntimeException` class.
 * In the `JsException` class was added two new properties: `Category` and `Description`
 * From the `JsRuntimeException` class was removed one property - `ErrorCode`
 * In the `JsRuntimeException` class was added three new properties: `Type`, `DocumentName` and `CallStack`
 * `JsScriptInterruptedException` class was renamed to the `JsInterruptedException` class and now is inherited from the `JsRuntimeException` class
 * `JsEngineLoadException` class now is inherited from the `JsEngineException` class
 * Removed a `EmptyValueException` class
 * `Format` method of the `JsErrorHelpers` class was renamed to the `GenerateErrorDetails`
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.0 Beta 1
 * In JavaScriptEngineSwitcher.V8:
   * Added support of Microsoft ClearScript.V8 version 5.5.2 (support of V8 version 6.5.254.41)
   * Now the Microsoft ClearScript.V8 requires the [Microsoft Visual C++ Redistributable for Visual Studio 2017](https://www.visualstudio.com/downloads/#microsoft-visual-c-redistributable-for-visual-studio-2017)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of February 24, 2018
 * In JavaScriptEngineSwitcher.Jint in configuration settings of the Jint JS engine a `Timeout` property has been replaced by the `TimeoutInterval` property (default `TimeSpan.Zero`)
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.8.2
   * Now the ChakraCore for Windows requires the [Microsoft Visual C++ Redistributable for Visual Studio 2017](https://www.visualstudio.com/downloads/#microsoft-visual-c-redistributable-for-visual-studio-2017)
   * In configuration settings of the ChakraCore JS engine was added one new property - `MaxStackSize` (default `492` or `984` KB)

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

## v3.0.0 Alpha 10 - January 3, 2018
 * In JavaScriptEngineSwitcher.V8:
   * Added support of Microsoft ClearScript.V8 version 5.5.1.1 (support of V8 version 6.3.292.48)
   * In configuration settings of the V8 JS engine was added one new property - `AwaitDebuggerAndPauseOnStart` (default `false`)

## v2.4.11 - December 24, 2017
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 2.2.5
 * In JavaScriptEngineSwitcher.ChakraCore fixed a error, that occurred during finding the suitable method overload, that receives numeric values and interfaces as parameters, of the host object

## v3.0.0 Alpha 9 - December 22, 2017
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.0 Alpha 3
 * In JavaScriptEngineSwitcher.V8 in configuration settings of the V8 JS engine was changed types of `MaxHeapSize` and `MaxStackUsage` properties from `ulong` to `UIntPtr`
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.11.58
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.7.5
   * In configuration settings of the ChakraCore JS engine was added two new properties - `MemoryLimit` and `DisableFatalOnOOM` (default `false`)
   * Now during calling of the `CollectGarbage` method is no longer performed blocking

## v3.0.0 Alpha 8 - November 17, 2017
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 3.0.0 Alpha 2
 * In JavaScriptEngineSwitcher.ChakraCore fixed a error, that occurred during finding the suitable method overload, that receives numeric values and interfaces as parameters, of the host object

## v3.0.0 Alpha 7 - November 12, 2017
 * In JavaScriptEngineSwitcher.V8.Native.win-* and JavaScriptEngineSwitcher.ChakraCore.Native.win* packages fixed a error “When using PackageReference DLL is not copied”
 * In JavaScriptEngineSwitcher.V8:
   * Added support of Microsoft ClearScript.V8 version 5.5.0 (support of V8 version 6.2.414.40)
   * Now requires .NET Framework 4.5 or greater
   * In configuration settings of the V8 JS engine became obsolete the `MaxExecutableSize` property and was added 4 new properties: `EnableRemoteDebugging` (default `false`), `HeapSizeSampleInterval` (default `TimeSpan.Zero`), `MaxHeapSize` (default `0`) and `MaxStackUsage` (default `0`)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of November 2, 2017

## v3.0.0 Alpha 6 - October 17, 2017
 * In all modules, except the JavaScriptEngineSwitcher.V8 module, added support of .NET Standard 2.0
 * In JavaScriptEngineSwitcher.V8 improved implementation of the `CallFunction` method
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of September 1, 2017
 * In JavaScriptEngineSwitcher.ChakraCore:
   * ChakraCore was updated to version 1.7.3
   * JavaScriptEngineSwitcher.ChakraCore.Native.debian-x64 package has been replaced by the JavaScriptEngineSwitcher.ChakraCore.Native.linux-x64 package
   * ICU-57 library was embedded into the `libChakraCore.so` and `libChakraCore.dylib` assemblies

## v3.0.0 Alpha 5 - September 29, 2017
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.10

## v3.0.0 Alpha 4 - September 22, 2017
 * In JavaScriptEngineSwitcher.ChakraCore prevented an occurrence of the “Host may not have set any promise continuation callback. Promises may not be executed.” error

## v3.0.0 Alpha 3 - September 15, 2017
 * In JavaScriptEngineSwitcher.Msie:
   * Added support of MSIE JavaScript Engine version 3.0.0 Alpha 1
   * Added a ability to interrupt execution of the script
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.11.23
 * In JavaScriptEngineSwitcher.ChakraCore:
   * Added support of ChakraCore version 1.7.2
   * Compilation error messages now contains a information about the error location

## v3.0.0 Alpha 2 - July 26, 2017
 * In `JsEngineSwitcher` class `Instance` property was renamed to the `Current` property
 * Now you can implement your own version of the `JsEngineSwitcher` class
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of July 13, 2017
 * In JavaScriptEngineSwitcher.ChakraCore added support of ChakraCore version 1.7.0

## v3.0.0 Alpha 1 - July 12, 2017
 * In `IJsEngine` interface was added `SupportsScriptInterruption` property and `Interrupt` method
 * In JavaScriptEngineSwitcher.V8 and JavaScriptEngineSwitcher.ChakraCore added a ability to interrupt execution of the script

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
 * In JavaScriptEngineSwitcher.V8 now the Microsoft ClearScript.V8 requires `msvcp140.dll` assembly from the [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=53840)
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
   * New version of the ChakraCore for Windows requires `msvcp140.dll` assembly from the [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=53840)

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
   * New version of the ChakraCore for Windows requires `msvcp140.dll` assembly from the [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=53840)
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