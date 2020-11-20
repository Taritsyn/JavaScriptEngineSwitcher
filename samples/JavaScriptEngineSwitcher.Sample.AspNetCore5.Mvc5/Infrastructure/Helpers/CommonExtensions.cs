using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JavaScriptEngineSwitcher.Sample.AspNetCore5.Mvc5.Infrastructure.Helpers
{
	public static class CommonExtensions
	{
		public static HtmlString EncodedReplace(this IHtmlHelper htmlHelper, string input,
			string pattern, string replacement)
		{
			return new HtmlString(Regex.Replace(htmlHelper.Encode(input), pattern, replacement));
		}
	}
}