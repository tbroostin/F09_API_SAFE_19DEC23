//Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Ellucian.Web.Http.Routes
{
    /// <summary>
    /// Provides a matching route if ContentType of GET/PUT requests matches the value specified in the route constraint.
    /// If constraint is not met, proceeds to match next route or returns 404 error
    /// </summary>
    /// <seealso cref="System.Web.Routing.IRouteConstraint" />
    public class ContentTypeConstraint : IRouteConstraint
    {
        private string contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTypeConstraint"/> class.
        /// </summary>
        /// <param name="contentType">Text to match the incoming request content type with</param>
        public ContentTypeConstraint(string contentType)
        {
            this.contentType = contentType;
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
        public bool Match(System.Web.HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool matches = false;


            // Using an enumeration of defined routes, get an enumeration of potentially supported routes.
            var associatedMajorCustomMediaTypes = GetAllSupportedRoutes(new List<string> { contentType });

           // var matches = associatedMajorCustomMediaTypes
             //      .Intersect(contentType, StringComparer.OrdinalIgnoreCase);

            if (!String.IsNullOrEmpty(this.contentType))
            {
                //matches = httpContext.Request != null &&
                //     httpContext.Request.ContentType != null &&
                //     httpContext.Request.ContentType.Equals(this.contentType, System.StringComparison.InvariantCultureIgnoreCase);

                matches = httpContext.Request != null &&
                     httpContext.Request.ContentType != null &&
                     associatedMajorCustomMediaTypes.ToList().Contains(httpContext.Request.ContentType);
               // httpContext.Request.ContentType.Contains(this.contentType, System.StringComparison.InvariantCultureIgnoreCase);

            }

            return matches;
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
}
