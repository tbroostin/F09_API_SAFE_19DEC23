// Copyright 2013-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
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
             *  includes one of those custom media types.
             *  
             * EEDM custom media types, such as "application/vnd.hedtech.integration" must throw a 406 if they are 
             *  not matched with a defined version number.  This is accomplished by defining controller routes with an
             *  EEDM custom media type containing the application/vnd.hedtech.integration prefix AND a '*'
             *  Upon matching, that route must be associated with an action which throws an error. 
             * 
             * Supports matching on associated major version (for example: If an API supports 6.1.0, then it will be 
             *  returned whether I request,  6, 6.0, 6.0.0, 6.1, 6.1.0, 6.0.0.0 
             ********************************************************************************************/

            if (_customMediaTypes != null && _customMediaTypes.Any())
            {
                // Using an enumeration of defined routes, get an enumeration of potentially supported routes.
                var associatedMajorCustomMediaTypes = GetAllSupportedRoutes(_customMediaTypes);
                
                // Return a match if the requestedMediaType (accept header) matches the list of potential routes, 
                var matches = associatedMajorCustomMediaTypes
                    .Intersect(requestedMediaTypes, StringComparer.OrdinalIgnoreCase);

                // In addition, if there is a customMediaType in the format application/vnd.hedtech.integration.v*+json then
                // then we will want to match on that too, since this represents the http 406 not acceptable route 
                if (!matches.Any())
                {
                    matches = requestedMediaTypes.Where(rmt => rmt.StartsWith(hedtechIntegrationMediaTypePrefix)
                      && associatedMajorCustomMediaTypes.Any(ct => ct.Contains("*")));
                }

                if (matches.Any())
                {
                    // A RequestedContentTypeKey should not be defined on a versioned route and should only be used for versionless requests.
                    // this catches implmentation errors in routeConfig as a safeguard.
                    if ((!_satisfyVersionlessRequest) && (values.ContainsKey(RequestedContentTypeKey)))
                    {
                        values.Remove(RequestedContentTypeKey);
                    }

                    //read the SemanticVersionEnabled from the APi Web.config to see if semantic version is disabled. If so return the requested version.
                    bool enableSemanticVersion = true;
                    bool status = false;
                    if (bool.TryParse(WebConfigurationManager.AppSettings["SemanticVersionEnabled"], out status))
                    {
                        enableSemanticVersion = status;
                    }

                    if (enableSemanticVersion == true)
                    {
                        if (!values.ContainsKey(RequestedContentTypeKey))
                    
                        {
                            // If there is only one customMediaType defined, then use that value,
                            // Otherwise, the associatedMajorCustomMediaTypes needs to be sorted
                            if (_customMediaTypes.Count() == 1)
                                values.Add(RequestedContentTypeKey, _customMediaTypes.First());
                            else
                            {
                                var maxSupportedVersion = GetMaxVersion(associatedMajorCustomMediaTypes);
                                values.Add(RequestedContentTypeKey, maxSupportedVersion);
                            }
                        }
                        return true;
                    }
                    else
                    {
                        if (!values.ContainsKey(RequestedContentTypeKey))
                        {
                            values.Add(RequestedContentTypeKey, requestedMediaTypes.FirstOrDefault());
                        }
                        return true;
                    }
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

        /// <summary>
        /// Using an enuemration of defined routes, get an enumeration of potentially supported routes.
        /// </summary>
        /// <param name="definedRoutes">Enumeration of defined routes.</param>
        /// <returns>Enumeration of potentially supported routes.</returns>
        public static IEnumerable<string> GetAllSupportedRoutes(IEnumerable<string> definedRoutes)
        {
            if ((definedRoutes == null) || (!definedRoutes.Any()))
            {
                return new List<string>();
            }

            int startIndex = 0;
            var extractedVersion = string.Empty;
            var allSupportedRoutes = new List<string>();
           
            //the defined routes will get added to the collection
            allSupportedRoutes.AddRange(definedRoutes);

            //Used to determine if the supportedVersion is sematically versioned.
            var regex = new Regex(@"(?:(\d+)\.)?(?:(\d+)\.)?(?:(\d+)\.\d+)", RegexOptions.Compiled);

            foreach (var definedRoute in definedRoutes)
            { 
                Match semanticVersion = regex.Match(definedRoute);

                //If this is a semantic route, then we want to obtain the other potential routes
                if (semanticVersion.Success)
                {
                    extractedVersion = semanticVersion.Value;
                    startIndex = semanticVersion.Index;

                    var convertedVersion = new Version(extractedVersion);
                  
                    var supportedMediaTypeWithoutVersion = definedRoute.Remove(startIndex, extractedVersion.Length);
                    // // create a potential supported route for the major version only (no minor or patch).
                    allSupportedRoutes.Add(supportedMediaTypeWithoutVersion
                        .Insert(startIndex, convertedVersion.Major.ToString()));

                    // extract each route in decending order, iterating through the potential minor versions
                    for (int minor = convertedVersion.Minor; minor >= 0; minor--)
                    {
                        // support for this version is not supported but may be at a later release
                        //create a potential supported route for the major/minor version (no patch).
                        //allSupportedRoutes.Add(supportedMediaTypeWithoutVersion
                        //    .Insert(startIndex, string.Concat(convertedVersion.Major, ".", minor)));
                        
                        //create a potential supported route for the major/minor version with patch equal 0.
                        allSupportedRoutes.Add(supportedMediaTypeWithoutVersion
                            .Insert(startIndex, string.Concat(convertedVersion.Major, ".", minor, ".0")));
                    }
                }
            }
            return allSupportedRoutes.Distinct();
        }

        /// <summary>
        /// Sort an enumeration, of customMediaTypes, by the version number.
        /// </summary>
        /// <param name="definedRoutes"></param>
        /// <returns>The customMediaType with the largest version number</returns>
        private string GetMaxVersion(IEnumerable<string> definedRoutes)
        {
            var retVal = string.Empty;
            if (definedRoutes == null || !definedRoutes.Any())
            {
                return retVal;
            }
            // if there is only one value, there is nothing to sort
            if (definedRoutes.Count() == 1)
            {
                return definedRoutes.First();
            }

            var distinctRoutes = definedRoutes.Distinct();

            //Get a keyvaluePair consisting of a version number and related customMediaType
            // example: {"1.1.0",  "application/vnd.custom.integration.v1.1.0+json"}
            var versionsDict = new Dictionary<string, string>();
            foreach (var definedRoute in distinctRoutes)
            {
                var input = ExtractVersionNumberOnly(definedRoute);
                versionsDict.Add(input, definedRoute);
            }

            //Compare the keys from the keyvaluePair to get the largest possible version number
            // v1.0.0 is considered to be greater than v1
            var myComparer = new VersionNumberComparer();
            var myArray = versionsDict.Keys.ToList();
            myArray.Sort(myComparer);
            
            // From the sortedKeys,get the last value in the array
            // (which represents the largest version number) and get its associated  customMediaType        
            versionsDict.TryGetValue(myArray.Last(), out retVal);

            return retVal;
        }    

        /// <summary>
        /// Extract the version number from a customMediaType.  Extracts integers or semantic versions.
        /// </summary>
        /// <param name="original"></param>
        /// <returns>Version number.  May contain none, or unknown number of decimals</returns>
        private string ExtractVersionNumberOnly(string original)
        {
            var regex = new Regex(@"(?:(\d+)\.)?(?:(\d+)\.)?(?:(\d+)\.\d+)|(?:(\d+))", RegexOptions.Compiled);
            Match semanticVersion = regex.Match(original);
            if (semanticVersion.Success)
            {
                return semanticVersion.Value;
            }
            else return string.Empty;
        }
    }

    class VersionNumberComparer : IComparer<string>
    {
        /// <summary>
        /// Compare strings which represent semantic version numbers and/or integers
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>x is greater return 1 else if y is greater return -1</returns>
        public int Compare(string x, string y)
        {
            if (x == y) return 0;

            var first = x.Split(new char[] { '.' }).Select(xx => int.Parse(xx)).ToList();
            var second = y.Split(new char[] { '.' }).Select(yy => int.Parse(yy)).ToList();

            var stackFirst = new Queue<int>(first);
            var stackSecond = new Queue<int>(second);

            var largest = first.Count > second.Count ? first.Count : second.Count;

            for (int i = 0; i < largest; i++)
            {
                if ((stackFirst.Count == 0) && (stackSecond.Count > 0))
                {
                    return -1;
                }
                else if ((stackFirst.Count > 0) && (stackSecond.Count == 0))
                {
                    return 1;
                }
                else
                {
                    var s1 = stackFirst.Dequeue();
                    var s2 = stackSecond.Dequeue();

                    if (s1 > s2)
                    {
                        return 1;
                    }
                    else if (s1 < s2)
                    {
                        return -1;
                    }
                    else continue;
                }
            }

            return 0;
        }
    }
}