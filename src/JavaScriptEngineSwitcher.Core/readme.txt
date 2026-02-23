

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: Core v3.31.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2026 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScript Engine Switcher determines unified interface for access to the basic
   features of popular JavaScript engines (ChakraCore, Jint, Jurassic, MSIE
   JavaScript Engine for .NET, NiL.JS, Jering.Javascript.NodeJS, Microsoft
   ClearScript.V8, VroomJs and YantraJS). This library allows you to quickly and
   easily switch to using of another JavaScript engine.

   =============
   RELEASE NOTES
   =============
   1. Performed a migration to the modern C# null/not-null checks;
   2. Added support for .NET 10;
   3. In the `lock` statements for .NET 10 target now uses a instances of the
      `System.Threading.Lock` class;
   4. Reduced a memory allocation by using collection expressions.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher