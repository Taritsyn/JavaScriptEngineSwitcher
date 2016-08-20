using System.Web.Mvc;

namespace JavaScriptEngineSwitcher.Sample.AspNet4.Mvc4
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}