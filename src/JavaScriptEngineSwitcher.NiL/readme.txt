

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: NiL v3.31.0

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2026 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.NiL contains a `NiLJsEngine` adapter (wrapper for the
   NiL.JS (https://github.com/nilproject/NiL.JS) version 2.6.1712).

   =============
   RELEASE NOTES
   =============
   1. Performed a migration to the modern C# null/not-null checks;
   2. In the `lock` statements for .NET 9 target now uses a instances of the
      `System.Threading.Lock` class;
   3. Reduced a memory allocation by using collection expressions.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher