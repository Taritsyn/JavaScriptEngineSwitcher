

   --------------------------------------------------------------------------------
          README file for JS Engine Switcher: MS Dependency Injection v3.21.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2024 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Extensions.MsDependencyInjection contains extension
   methods for adding the JS engine switcher in an `IServiceCollection`.

   =============
   RELEASE NOTES
   =============
   1. Added support for .NET Standard 2.1;
   2. `AddJsEngineSwitcher(Action<IJsEngineSwitcher>)` and
      `AddJsEngineSwitcher(IJsEngineSwitcher, Action<IJsEngineSwitcher>)`
      extension methods are replaced by new methods accordingly:
      `AddJsEngineSwitcher(Action<JsEngineSwitcherOptions>)` and
      `AddJsEngineSwitcher(IJsEngineSwitcher, Action<JsEngineSwitcherOptions>)`;
   3. `AllowCurrentProperty` property of `JsEngineSwitcherOptions` class allows to
      forbid usage of the `JsEngineSwitcher.Current` property. This feature can be
      used to fix a error #115 “Concurrency issue when initializing JS engine
      switcher in startup”. Special thanks to Ville Häkli.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher