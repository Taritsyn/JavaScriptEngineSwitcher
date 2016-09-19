

   --------------------------------------------------------------------------------
                    README file for JS Engine Switcher: MSIE v2.0.0

   --------------------------------------------------------------------------------

      Copyright (c) 2013-2016 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   JavaScriptEngineSwitcher.Msie contains adapter `MsieJsEngine` (wrapper for the
   MSIE JavaScript Engine for .Net (http://github.com/Taritsyn/MsieJavaScriptEngine)).
   For correct working of the MSIE JavaScript Engine it is recommended to install
   Internet Explorer 9 and above on a server.

   =============
   RELEASE NOTES
   =============
   1. Removed dependency on `System.Configuration.dll` (no longer supported
      configuration by using the `Web.config` and `App.config` files);
   2. Added support of .NET Core 1.0.1 (only supported `ChakraIeJsRt` and
      `ChakraEdgeJsRt` modes) and .NET Framework 4.5.1;
   3. Added support of MSIE JavaScript Engine version 2.0.0.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub -
   http://github.com/Taritsyn/JavaScriptEngineSwitcher