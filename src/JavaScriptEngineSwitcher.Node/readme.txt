

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: Node v3.24.1

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2026 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Node contains a `NodeJsEngine` adapter (wrapper for the
   Jering.Javascript.NodeJS (https://github.com/JeringTech/Javascript.NodeJS)
   version 7.0.0).

   This package does not contain the `node.exe`. Therefore, you need to install the
   Node.js (https://nodejs.org) and add the `node.exe`'s directory to the `Path`
   environment variable (automatically done by the official installer).

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