using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Ellucian.Web.Http.Routes
{
    /// <summary>
    /// Provides a mechanism for matching requests to actions based on the query string.
    /// </summary>
    public class QueryStringConstraint : IRouteConstraint
    {
        List<string> _queryStringKeys = new List<string>();
        bool _allowOther = true;

        /// <summary>
        /// Creates a new QueryStringConstraint instance with the provided list of keys. For example, if you need to match
        /// a route with a query string like &quot;id=123&name=john&quot; then you would provide { &quot;id&quot;, &quot;name&quot; } as this list.
        /// </summary>
        /// <param name="queryStringsToMatch">A collection of query string keys that are to be matched with the request</param>
        /// <param name="allowOtherKeys">A flag specifying whether other keys are allowed in the query string. By default
        /// this will be true. If this is set to false then the query string key set must exactly match the provided
        /// queryStringsToMatch collection for the route to match.</param>
        public QueryStringConstraint(IEnumerable<string> queryStringsToMatch, bool allowOtherKeys = true)
        {
            _queryStringKeys = queryStringsToMatch.ToList();
            _allowOther = allowOtherKeys;
        }

        /// <summary>
        /// Determines whether a given Url matches based on the query string parameter.
        /// </summary>
        /// <param name="httpContext">The HttpContextBase of the request, provided automatically</param>
        /// <param name="route">Not used</param>
        /// <param name="parameterName">Not used</param>
        /// <param name="values">Not used</param>
        /// <param name="routeDirection">Not used</param>
        /// <returns>True if this request matches the route constraints based on the keys that are being requested, otherwise false</returns>
        public bool Match(HttpContextBase httpContext, System.Web.Routing.Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool matches = true;
            foreach (string key in _queryStringKeys)
            {
                if (!httpContext.Request.QueryString.AllKeys.Contains(key))
                {
                    matches = false;
                    break;
                }
            }
            if (matches && !_allowOther)
            {
                foreach (string key in httpContext.Request.QueryString.AllKeys)
                {
                    if (!_queryStringKeys.Contains(key))
                    {
                        matches = false;
                        break;
                    }
                }
            }
            return matches;
        }
    }
}