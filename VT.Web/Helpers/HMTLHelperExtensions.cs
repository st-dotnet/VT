using System.Linq;
using System.Web.Mvc;

namespace VT.Web.Helpers
{
    public static class HmtlHelperExtensions
    {
        public static string IsSelected(this HtmlHelper html, string controller = null, string action = null)
        {
            const string cssClass = "active";
            var currentAction = (string)html.ViewContext.RouteData.Values["action"];
            var currentController = (string)html.ViewContext.RouteData.Values["controller"];
            if (string.IsNullOrEmpty(controller))
                controller = currentController;

            if (string.IsNullOrEmpty(action))
            {
                action = currentAction;
            }
            else
            {
                if (action.Contains("|"))
                {
                    var actionArr = action.Split('|');
                    return controller == currentController && actionArr.Contains(currentAction) ? cssClass : string.Empty;
                }
            }

            return controller == currentController && action == currentAction ?
                cssClass : string.Empty;
        }

        public static string PageClass(this HtmlHelper html)
        {
            var currentAction = (string)html.ViewContext.RouteData.Values["action"];
            return currentAction;
        }
    }
}
