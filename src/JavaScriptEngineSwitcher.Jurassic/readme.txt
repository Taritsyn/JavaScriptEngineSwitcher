

   --------------------------------------------------------------------------------
                  README file for JS Engine Switcher: Jurassic v3.29.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2026 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Jurassic contains a `JurassicJsEngine` adapter (wrapper
   for the Jurassic (http://github.com/paulbartrum/jurassic) version of
   February 4, 2025).

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