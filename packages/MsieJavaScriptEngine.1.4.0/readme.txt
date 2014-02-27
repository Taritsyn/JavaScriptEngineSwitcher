

   ----------------------------------------------------------------------
           README file for MSIE JavaScript Engine for .NET 1.4.0

   ----------------------------------------------------------------------

          Copyright 2014 Andrey Taritsyn - http://www.taritsyn.ru
		  
		  
   ===========
   DESCRIPTION
   ===========
   This project is a .NET wrapper for working with the Internet Explorer's 
   JavaScript engines (JsRT version of Chakra, ActiveScript version of 
   Chakra and Classic JavaScript Engine). 
   Project was based on the code of SassAndCoffee.JavaScript 
   (http://github.com/paulcbetts/SassAndCoffee) and Chakra Sample Hosts 
   (http://github.com/panopticoncentral/chakra-host).
   
   =============
   RELEASE NOTES
   =============
   1. Removed following methods: `HasProperty`, `GetPropertyValue`, 
      `SetPropertyValue` and `RemoveProperty`;
   2. Fixed bug #3 "execute code from different threads";
   3. Now in the `ChakraJsRt` mode is available a more detailed
      information about errors;
   4. In ECMAScript 5 Polyfill improved a performance of the
      `String.prototype.trim` function;
   5. JSON2 library was updated to version of February 4, 2014.

   ============
   PROJECT SITE
   ============
   http://github.com/Taritsyn/MsieJavaScriptEngine