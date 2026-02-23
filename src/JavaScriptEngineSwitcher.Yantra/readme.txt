

   --------------------------------------------------------------------------------
                  README file for JS Engine Switcher: Yantra v3.31.0

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2026 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Yantra contains a `YantraJsEngine` adapter (wrapper for the
   YantraJS (https://github.com/yantrajs/yantra) version 1.2.301).

   =============
   RELEASE NOTES
   =============
   1. YantraJS was updated to version 1.2.301;
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