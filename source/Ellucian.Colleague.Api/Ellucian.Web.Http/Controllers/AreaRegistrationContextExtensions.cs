using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using System.Web.Http;

namespace Ellucian.Web.Http.Controllers
{
    public static class AreaRegistrationContextExtensions
    {
        public static Route MapHttpRoute(this AreaRegistrationContext context, string name, string routeTemplate, object defaults = null, object constraints = null, string namespaceOverride = null)
        {
            var areaRouteName = string.Concat(context.AreaName, name);

            var route = context.Routes.MapHttpRoute(areaRouteName, routeTemplate, defaults, constraints);

            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

            route.DataTokens.Add("area", context.AreaName);

            if (!string.IsNullOrEmpty(namespaceOverride))
            {
                route.DataTokens.Add("Namespaces", new string[] { namespaceOverride });
            }

            return route;
        }
    }
}
