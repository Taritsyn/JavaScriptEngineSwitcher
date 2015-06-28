Change log
==========

## June 28, 2015 - v1.2.9
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of MSIE and Jint JavaScript engines
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.4 and in configuration settings added 2 new properties: `UseEcmaScript5Polyfill` (default `false`) and `UseJson2Library` (default `false`)
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.2.1 (support of V8 version 4.2.77.18)
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of June 23, 2015
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of June 24, 2015 and in configuration settings added one new property - `AllowDebuggerStatement` (default `false`)

## May 26, 2015 - v1.2.8
 * In JavaScriptEngineSwitcher.Jint added support of Jint version 2.5.0

## May 11, 2015 - v1.2.7
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of V8 JavaScript engine
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.2 (support of V8 version 4.2.77.18)

## May 5, 2015 - v1.2.6
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.3
 * In JavaScriptEngineSwitcher.V8 fixed a [bug #12](http://github.com/Taritsyn/JavaScriptEngineSwitcher/issues/12) “V8 config can be null”
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of April 30, 2015

## April 5, 2015 - v1.2.5
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense updated definitions for configuration settings of Jint JavaScript engine
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.2
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of March 29, 2015 and in configuration settings was changed default value of `MaxRecursionDepth` property from `20678` to `-1`

## February 19, 2015 - v1.2.4
 * In `JsEngineBase` class all public non-abstract methods are now is virtual
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense added definitions for configuration settings of Jurassic and Jint JavaScript engines
 * In JavaScriptEngineSwitcher.Jurassic added the ability to change configuration settings of Jurassic JavaScript engine: `EnableDebugging` (default `false`), `EnableIlAnalysis` (default `false`) and `StrictMode` (default `false`)
 * In JavaScriptEngineSwitcher.Jint added the ability to change configuration settings of Jint JavaScript engine: `EnableDebugging` (default `false`), `MaxRecursionDepth` (default `20678`), `MaxStatements` (default `0`), `StrictMode` (default `false`) and `Timeout` (default `0`)

## February 16, 2015 - v1.2.3
 * In JavaScriptEngineSwitcher.ConfigurationIntelliSense added definitions for configuration settings of V8 JavaScript engine
 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.1 (support of V8 version 3.30.33.16) and added the ability to change configuration settings of V8 JavaScript engine: `EnableDebugging` (default `false`), `DebugPort` (default `9222`), `DisableGlobalMembers` (default `false`), `MaxNewSpaceSize` (default `0`), `MaxOldSpaceSize` (default `0`) and `MaxExecutableSize` (default `0`)
 * In JavaScriptEngineSwitcher.Jint added support of Jint version of February 14, 2015

## January 13, 2015 - v1.2.2
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.1

## October 22, 2014 - v1.2.1

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version 5.4.0 (support of V8 version 3.26.31.15)
* In JavaScriptEngineSwitcher.Jint added support of Jint version of October 21, 2014

## October 13, 2014 - v1.2.0

* From JavaScriptEngineSwitcher.V8 and JavaScriptEngineSwitcher.Jint removed dependency on `System.Web.Extensions.dll`
* All assemblies is now targeted on the .NET Framework 4 Client Profile
* In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.5.0
* In JavaScriptEngineSwitcher.Jint added support of Jint version of October 9, 2014

## September 17, 2014 - v1.1.13

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of September 6, 2014
* In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of September 5, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of September 16, 2014

## August 19, 2014 - v1.1.12

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of August 1, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of August 16, 2014

## July 22, 2014 - v1.1.11

 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.4

## July 10, 2014 - v1.1.10

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of July 5, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version 2.2.0

## June 14, 2014 - v1.1.9

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of June 10, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of June 5, 2014

## May 20, 2014 - v1.1.8

* In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of May 17, 2014
* In JavaScriptEngineSwitcher.Jint added support of Jint version of May 16, 2014

## May 7, 2014 - v1.1.7

 * In JavaScriptEngineSwitcher.V8 added support of Microsoft ClearScript.V8 version of April 29, 2014

## April 27, 2014 - v1.1.6

 * In solution was enabled NuGet package restore
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.3
 * In JavaScriptEngineSwitcher.Jurassic added support of Jurassic version of April 26, 2014

## April 5, 2014 - v1.1.5

 * In JavaScriptEngineSwitcher.Jint added support of Jint version of April 5, 2014

## March 24, 2014 - v1.1.4

 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.2

## March 22, 2014 - v1.1.3

 * Added module based on the [Jint](http://github.com/sebastienros/jint). Special thanks to [Daniel Lo Nigro](http://github.com/Daniel15) for the idea of this module. 
 * In JavaScriptEngineSwitcher.Core fixed minor bugs
 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.1

## February 27, 2014 - v1.1.2

 * In JavaScriptEngineSwitcher.Msie added support of MSIE JavaScript Engine version 1.4.0

## January 17, 2014 - v1.1.1

 * In JavaScriptEngineSwitcher.V8 added support of the [ClearScript](http://clearscript.codeplex.com/) version 5.3.11 (support of V8 version 3.24.17)

## January 16, 2014 - v1.1.0

 * In JavaScriptEngineSwitcher.Msie added support of [MSIE JavaScript Engine](http://github.com/Taritsyn/MsieJavaScriptEngine) version 1.3.0
 * In JavaScriptEngineSwitcher.V8 improved performance of `CallFunction` method
 * In JavaScriptEngineSwitcher.Jurassic added support of [Jurassic](http://jurassic.codeplex.com/) version of January 11, 2014

## December 30, 2013 - v1.0.0

 * Added support of JavaScript `undefined` type
 * In JavaScriptEngineSwitcher.Msie added support of [MSIE JavaScript Engine](http://github.com/Taritsyn/MsieJavaScriptEngine) version 1.2.0

## December 7, 2013 - v0.9.5

 * In JavaScriptEngineSwitcher.V8 the [Noesis Javascript .NET](http://javascriptdotnet.codeplex.com/) was replaced by the [Microsoft ClearScript.V8](http://clearscript.codeplex.com/) library (solves a problem of `V8JsEngine` stable work on 64-bit version of IIS 8.X)
 * In JavaScriptEngineSwitcher.Jurassic added support of [Jurassic](http://jurassic.codeplex.com/) version of September 30, 2013
 
## November 19, 2013 - v0.9.3

 * In JavaScriptEngineSwitcher.V8 changed a mechanism for loading the Noesis Javascript .NET assemblies for different processor architectures
 * Deleted a `/configuration/jsEngineSwitcher/v8` configuration section

## September 5, 2013 - v0.9.2

 * Added JavaScriptEngineSwitcher.ConfigurationIntelliSense NuGet package
 
## September 4, 2013 - v0.9.0
 * Initial version uploaded