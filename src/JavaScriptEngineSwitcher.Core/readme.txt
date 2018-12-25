

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: Core v3.0.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2018 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScript Engine Switcher determines unified interface for access to the basic
   features of popular JavaScript engines (MSIE JavaScript Engine for .NET,
   Microsoft ClearScript.V8, Jurassic, Jint, ChakraCore, VroomJs and NiL.JS). This
   library allows you to quickly and easily switch to using of another JavaScript
   engine.

   =============
   RELEASE NOTES
   =============
   1.  In the `JsEngineSwitcher` class a `Instance` property was renamed to the
       `Current` property;
   2.  Now you can implement your own version of the `JsEngineSwitcher` class;
   3.  Format of the error messages was unified;
   4.  Created a new exception classes: `JsCompilationException`,
       `JsEngineException`, `JsFatalException`, `JsInterruptedException`,
       `JsTimeoutException`, `JsScriptException` and `JsUsageException`. These
       exceptions are responsible for handling errors, some of which were
       previously handled by the `JsRuntimeException` class;
   5.  In the `JsException` class was added two new properties: `Category` and
       `Description`;
   6.  From the `JsRuntimeException` class was removed one property - `ErrorCode`;
   7.  In the `JsRuntimeException` class was added three new properties: `Type`,
       `DocumentName` and `CallStack`;
   8.  `JsEngineLoadException` class now is inherited from the `JsEngineException`
       class;
   9.  Removed a `EmptyValueException` class;
   10. `Format` method of the `JsErrorHelpers` class was renamed to the
       `GenerateErrorDetails`;
   11. Part of the auxiliary code was moved to external libraries:
       PolyfillsForOldDotNet and AdvancedStringBuilder;
   12. In `IJsEngine` interface was added two new  properties:
       `SupportsScriptInterruption` and `SupportsScriptPrecompilation`, and four
       new methods: `Interrupt`, `Precompile`, `PrecompileFile` and
       `PrecompileResource`;
   13. Added support of .NET Standard 2.0.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher