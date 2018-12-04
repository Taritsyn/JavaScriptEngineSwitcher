

   --------------------------------------------------------------------------------
                 README file for JS Engine Switcher: Core v3.0.0 RC 2

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
   1. `GetSourceFragmentFromCode` and `GetSourceFragmentFromLine` methods of
      `JsErrorHelpers` class were replaced by the `GetTextFragment` and
      `GetTextFragmentFromLine` methods of `TextHelpers` class;
   2. Part of the auxiliary code was moved to external libraries:
      PolyfillsForOldDotNet and AdvancedStringBuilder.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher