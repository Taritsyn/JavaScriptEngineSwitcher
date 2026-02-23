

   --------------------------------------------------------------------------------
                   README file for JS Engine Switcher: Jint v3.31.0

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2026 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Jint contains a `JintJsEngine` adapter (wrapper for the
   Jint (http://github.com/sebastienros/jint) version 4.6.0).

   =============
   RELEASE NOTES
   =============
   1. Jint was updated to version 4.6.0;
   2. Performed a migration to the modern C# null/not-null checks;
   3. Added support for .NET 10;
   4. In the `lock` statements for .NET 10 target now uses a instances of the
      `System.Threading.Lock` class;
   5. Reduced a memory allocation by using collection expressions.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher