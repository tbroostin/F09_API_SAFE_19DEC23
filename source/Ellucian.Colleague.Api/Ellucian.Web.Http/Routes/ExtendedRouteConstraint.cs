//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.Configuration;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ellucian.Web.Http.Routes
{
    /// <summary>
    /// Provides a matching route if the route name GET/PUT requests matches the value specified in the route constraint.
    /// If constraint is not met, proceeds to match next route or returns 404 error
    /// </summary>
    /// <seealso cref="System.Web.Routing.IRouteConstraint" />
    public class ExtendedRouteConstraint : IRouteConstraint
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTypeConstraint"/> class.
        /// </summary>
        public ExtendedRouteConstraint()
        {
        }

        /// <summary>
        /// Determines whether the URL parameter contains a valid value for this constraint.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <param name="route">The object that this constraint belongs to Not used.</param>
        /// <param name="parameterName">The name of the parameter that is being checked. Not used</param>
        /// <param name="values">An object that contains the parameters for the URL. Not used</param>
        /// <param name="routeDirection">An object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated. Not used</param>
        /// <returns>
        /// True if the content type of incoming request matches the content type provided in the route constraint : Otherwise, False.
        /// </returns>
        public bool Match(System.Web.HttpContextBase httpContext, Route route, string parameterName,
            RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool matches = false;
            bool cache = false;

            try
            {
                if (httpContext != null && httpContext.Request != null && httpContext.Request.Headers != null)
                {
                    var cacheControlHeader = httpContext.Request.Headers["Cache-Control"];

                    if (!string.IsNullOrEmpty(cacheControlHeader))
                    {
                        cache = cacheControlHeader.Equals("no-cache", System.StringComparison.OrdinalIgnoreCase);
                    }
                }
                var dataReader = DependencyResolver.Current.GetService<IExtendRepository>();

                var ethosExtensibilityConfiguration = dataReader.GetEthosExtensibilityConfiguration(cache);

                if (ethosExtensibilityConfiguration != null)
                {
                    matches = ethosExtensibilityConfiguration.ContainsKey(values["resource"].ToString());
                }
            }
            catch (Exception ex)
            {
                //do nothing for now..  
            }
            return matches;

        }
    }
}