using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

namespace TestV8Full
{
    class Program
    {
        static void Main(string[] args)
        {
			try
			{
				using (var engine = new V8JsEngine())
				{
					engine.Evaluate("1 + 1");
				}
			}
			catch(JsException e)
			{
				Console.WriteLine(e);
			}
        }
    }
}
