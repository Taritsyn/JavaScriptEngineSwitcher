

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: Jint v3.0.0

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2018 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Jint contains adapter `JintJsEngine` (wrapper for the
   Jint JavaScript Engine (http://github.com/sebastienros/jint) version 2.11.58).

   =============
   RELEASE NOTES
   =============
   1. Jint was updated to version 2.11.58;
   2. Added a ability to pre-compile scripts;
   3. In configuration settings of the Jint JS engine a `Timeout` property has been
      replaced by the `TimeoutInterval` property (default `TimeSpan.Zero`) and was
      added one new property - `LocalTimeZone` (default `TimeZoneInfo.Local`);
   4. Added support of .NET Standard 2.0.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher