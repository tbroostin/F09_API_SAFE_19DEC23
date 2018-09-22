// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Ellucian.Web.Http.Routes
{
    /// <summary>
    /// Determine if this API will service a request based on the media types in the request's header.
    /// 
    /// Case 1 - If the request specifies no media type, or it has only generic media types such as 
    /// "application/json", then this API will service the request if its satisfy versionless request flag is set to true.
    /// 
    /// Case 2 - If this API supports custom media types, it will only service a request that includes 
    /// one of those custom media types.
    /// 
    /// Case 3 - If this API doesn't use custom media types, then it will only service a request that includes 
    /// a standard media type, such as "application/vnd.ellucian.v*", that contains a matching route version number.
    /// </summary>
    public class HeaderVersionConstraint : IRouteConstraint
    {
        const string ellucianMediaTypeHeader = "X-Ellucian-Media-Type";
        const string ellucianMediaTypePrefix = "application/vnd.ellucian.v";
        const string hedtechMediaTypePrefix = "application/vnd.hedtech.v";
        const string hedtechIntegrationMediaTypePrefix = "application/vnd.hedtech.integration";
        const char MediaTypeDelimiter = '+';
        const string RequestedContentTypeKey = "RequestedContentType";
        static readonly List<string> GenericMediaTypes = new List<string>()
        {
            // Must be all lower case for case-insensitive comparison.
            "*/*",
            "application/json",
            "application/xml",
            "application/plain",
            "text/json",
            "text/xml",
            "text/plain"
        };

        public string RouteVersion { get; private set; }
        private bool _satisfyVersionlessRequest = false;
        private string[] _customMediaTypes = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderVersionConstraint"/> class.
        /// </summary>
        /// <param name="routeVersion">The route version.</param>
        /// <param name="satisfyVersionlessRequest">True if version # isn't required (for endpoints that have a default version).</param>
        /// <param name="customMediaTypes">The supported custom media types. If this is defined, the route version # will not be used to route.</param>
        public HeaderVersionConstraint(string routeVersion = "0", bool satisfyVersionlessRequest = false, params string[] customMediaTypes)
        {
            RouteVersion = routeVersion;
            _satisfyVersionlessRequest = satisfyVersionlessRequest;
            _customMediaTypes = customMediaTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderVersionConstraint"/> class.
        /// </summary>
        /// <param name="routeVersion">The route version.</param>
        /// <param name="satisfyVersionlessRequest">True if version # isn't required (for endpoints that have a default version).</param>
        /// <param name="customMediaTypes">The supported custom media types. If this is defined, the route version # will not be used to route.</param>
        public HeaderVersionConstraint(int routeVersion = 0, bool satisfyVersionlessRequest = false, params string[] customMediaTypes)
        {
            RouteVersion = routeVersion.ToString();
            _satisfyVersionlessRequest = satisfyVersionlessRequest;
            _customMediaTypes = customMediaTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderVersionConstraint"/> class.
        /// </summary>
        /// <param name="satisfyVersionlessRequest">True if version # isn't required (for endpoints that have a default version).</param>
        /// <param name="customMediaTypes">The custom media types.</param>
        public HeaderVersionConstraint(bool satisfyVersionlessRequest = false, params string[] customMediaTypes)
        {
            RouteVersion = "0";
            _satisfyVersionlessRequest = satisfyVersionlessRequest;
            _customMediaTypes = customMediaTypes;
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderVersionConstraint"/> class.
        /// </summary>
        /// <param name="customMediaTypes">The custom media types.</param>
        public HeaderVersionConstraint(string[] customMediaTypes)
        {
            RouteVersion = "0";
            _satisfyVersionlessRequest = false;
            _customMediaTypes = customMediaTypes;
        }
        

        public bool Match(System.Web.HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (httpContext == null || httpContext.Request == null && httpContext.Request.Headers == null)
            {
                // invalid request with null context/headers cannot be routed.
                return false;
            }

            // Gather a list of all media types found in the accept headers...
            List<string> requestedMediaTypes = httpContext.Request.AcceptTypes == null ?
               new List<string>() : httpContext.Request.AcceptTypes.ToList();

            // ...and the legacy Ellucian media type header (if provided):
            if (!string.IsNullOrEmpty(httpContext.Request.Headers[ellucianMediaTypeHeader]))
            {
                string legacyHeaders = httpContext.Request.Headers[ellucianMediaTypeHeader];
                requestedMediaTypes.AddRange(legacyHeaders.Split(','));
            }
         
            /*********************************************************************************************
             * Case 1 - If the request specifies no media type, or it has only generic media types such as 
             * "application/json", then this API will service the request if its satisfyVersionlessRequest 
             * flag is set to true.
             ********************************************************************************************/

            if (requestedMediaTypes.Count <= 0 ||
                    (requestedMediaTypes.Count > 0 &&
                     !requestedMediaTypes.Select(x => x.ToLowerInvariant()).Except(GenericMediaTypes).Any()))
            {
                return _satisfyVersionlessRequest;
            }

            /*********************************************************************************************
             * Case 2 - If this API supports custom media types, it will only service a request that  
             * includes one of those custom media types. 
             * EEDM custom media types, such as "application/vnd.hedtech.integration" must throw a 415 if they are 
             * not matched with a defined version number.  This is accomplished by defining controller routes with an
             * EEDM custom media type containing the application/vnd.hedtech.integration prefix AND a '*'
             * Upon matching, that route must be associated with an action which throws an error. 
             ********************************************************************************************/

            if (_customMediaTypes != null && _customMediaTypes.Count() > 0)
            {
                 var matches = _customMediaTypes.Intersect(requestedMediaTypes, StringComparer.OrdinalIgnoreCase)
                    .Union(requestedMediaTypes.Where(rmt => rmt.StartsWith(hedtechIntegrationMediaTypePrefix) 
                            && _customMediaTypes.Any(ct => ct.Contains("*"))));

                if (matches != null && matches.Any()) 
                {
                    if (!values.ContainsKey(RequestedContentTypeKey)) 
                    { 
                        values.Add(RequestedContentTypeKey, matches.First());
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            
            /*********************************************************************************************
             * Case 3 - If this API doesn't use custom media types, then it will only service a request  
             * that includes a standard media type, such as "application/vnd.ellucian.v*", that contains 
             * a matching route version number.
             ********************************************************************************************/

            // Find all instances of the standard media type "application/vnd.ellucian.v*N*" 
            // in the list of requested media types, and extract API version number *N*.
            var requestedLegacyMediaTypes = requestedMediaTypes.Where(a => a.StartsWith(ellucianMediaTypePrefix, StringComparison.OrdinalIgnoreCase));
            foreach (var requestedLegacyMediaType in requestedLegacyMediaTypes)
            {
                string[] splitString = requestedLegacyMediaType == null ? null : requestedLegacyMediaType.Split(MediaTypeDelimiter);
                if (splitString != null && splitString.Length > 0)
                {
                    // Parse the version number and compare to the specified route version number
                    string parsedValue = splitString[0].Length > ellucianMediaTypePrefix.Length ?
                        splitString[0].Substring(ellucianMediaTypePrefix.Length) : string.Empty;
                
                    if (parsedValue == RouteVersion)
                    {
                        if (!values.ContainsKey(RequestedContentTypeKey))
                        {
                            values.Add(RequestedContentTypeKey, requestedLegacyMediaType);
                        }
                        return true;
                    }
                }
            }

            // If no "vnd.ellucian" media type requested or matched, find all instances
            // of the other standard media type "application/vnd.hedtech.v*N*" in the list of requested media types, 
            // and extract API version number *N*.
            var requestedHedtechMediaTypes = requestedMediaTypes.Where(a => a.StartsWith(hedtechMediaTypePrefix, StringComparison.OrdinalIgnoreCase));
            foreach (var requestedHedtechMediaType in requestedHedtechMediaTypes)
            {
                string[] splitString = requestedHedtechMediaType == null ? null : requestedHedtechMediaType.Split(MediaTypeDelimiter);
                if (splitString != null && splitString.Length > 0)
                {
                    // Parse the version number and compare to the specified route version number
                    string parsedValue = splitString[0].Length > hedtechMediaTypePrefix.Length ?
                        splitString[0].Substring(hedtechMediaTypePrefix.Length) : string.Empty;

                    if (parsedValue == RouteVersion)
                    {
                        if (!values.ContainsKey(RequestedContentTypeKey))
                        {
                            values.Add(RequestedContentTypeKey, requestedHedtechMediaType);
                        }
                        return true;
                    }
                }
            }

            // Could not find a match based on version number extracted from 
            // the request's standard media types (if there was any).
            return false;
        }
    }
}