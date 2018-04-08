

   --------------------------------------------------------------------------------
                README file for JS Engine Switcher: Core v3.0.0 Alpha 9

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2018 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScript Engine Switcher determines unified interface for access to the basic
   features of popular JavaScript engines (MSIE JavaScript Engine for .Net,
   Microsoft ClearScript.V8, Jurassic, Jint and ChakraCore). This library allows
   you to quickly and easily switch to using of another JavaScript engine.

   =============
   RELEASE NOTES
   =============
   1. Format of the error messages was unified;
   2. Created a new exception classes: `JsCompilationException`,
      `JsEngineException`, `JsFatalException`, `JsTimeoutException` and
      `JsUsageException`. These exceptions are responsible for handling errors,
      some of which were previously handled by the `JsRuntimeException` class;
   3. In the `JsException` class was added two new properties: `Category` and
      `Description`;
   4. From the `JsRuntimeException` class was removed one property - `ErrorCode`;
   5. In the `JsRuntimeException` class was added three new properties: `Type`,
      `DocumentName` and `CallStack`;
   6. `JsScriptInterruptedException` class was renamed to the
      `JsInterruptedException` class and now is inherited from the
      `JsRuntimeException` class;
   7. `JsEngineLoadException` class now is inherited from the `JsEngineException`
      class;
   8. Removed a `EmptyValueException` class;
   9. `Format` method of the `JsErrorHelpers` class was renamed to the
      `GenerateErrorDetails`.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher