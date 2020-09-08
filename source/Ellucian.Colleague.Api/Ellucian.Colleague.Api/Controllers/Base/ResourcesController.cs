// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http.Routing;
using Ellucian.Colleague.Coordination.Base.Services;
using Microsoft.Ajax.Utilities;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ResourcesController : BaseCompressedApiController
    {
        private const string GetPagingName = "paging";
        private const string PostBatchName = "batch";
        private const string GetMethod = "get";
        private const string PostMethod = "post";
        private const string EEDM_WEBAPI_RESOURCES_CACHE_KEY = "EEDM_WEBAPI_RESOURCES_CACHE_KEY";
        private const string BulkRequestMediaType = "application/vnd.hedtech.integration.bulk-requests.v1.0.0+json";
        private const string IsBulkSupported = "isBulkSupported";

        private ICacheProvider _cacheProvider;
        private readonly IBulkLoadRequestService _bulkLoadRequestService;
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        public ResourcesController(IBulkLoadRequestService bulkLoadRequestService, ICacheProvider cacheProvider, ILogger logger)
        {
            _cacheProvider = cacheProvider;
            _bulkLoadRequestService = bulkLoadRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves version information for the Colleague Web API.
        /// </summary>
        /// <returns>Version information.</returns>
        public IEnumerable<ApiResources> GetResources()
        {
            List<ApiResources> resourcesDtoList = new List<ApiResources>();
            string mediaFormat = "application/vnd.hedtech.integration";
            string httpMethodConstraintName = "httpMethod";
            string headerVersionConstraintName = "headerVersion";
            string isEEdmSupported = "isEedmSupported";
            

            var routeCollection = Configuration.Routes;
            var httpRoutes = routeCollection
                .Where(r => r.Defaults.Keys != null && r.Defaults.Keys.Contains(isEEdmSupported) && r.Defaults[isEEdmSupported].Equals(true))
                .ToList();

            resourcesDtoList = GetResources(httpRoutes, mediaFormat, httpMethodConstraintName, headerVersionConstraintName);

            return resourcesDtoList.OrderBy(item => item.Name);
        }

        /// <summary>
        /// Gets all the resources
        /// </summary>
        /// <param name="mediaFormat"></param>
        /// <param name="httpMethodConstraintName"></param>
        /// <param name="headerVersionConstraintName"></param>
        /// <param name="httpRoutes"></param>
        public List<ApiResources> GetResources(List<IHttpRoute> httpRoutes, string mediaFormat, string httpMethodConstraintName, string headerVersionConstraintName)
        {

            List<ApiResources> resourcesList = new List<ApiResources>();

            bool bypassCache = false;
            if ((Request != null) && (Request.Headers.CacheControl != null))
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (bypassCache == false)
            {
                if (_cacheProvider != null && _cacheProvider.Contains(EEDM_WEBAPI_RESOURCES_CACHE_KEY))
                {
                    resourcesList = _cacheProvider[EEDM_WEBAPI_RESOURCES_CACHE_KEY] as List<ApiResources>;
                    return resourcesList;
                }
            }

            bool bulkLoadSupport = false;

            try
            {
                bulkLoadSupport = _bulkLoadRequestService.IsBulkLoadSupported();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Bulk Load Support check failed");
            }

            ValidateQueryStringFilter validateQueryStringFilter = new ValidateQueryStringFilter();
            var keywords = validateQueryStringFilter.ValidQueryParameters.ToList();
            string[] headerVersionConstraintValue = null;

            string appJsonContentType = "application/json";
            Assembly asm = Assembly.GetExecutingAssembly();

            var controlleractionlist = asm.GetTypes()
                        .Where(tt => typeof(BaseCompressedApiController).IsAssignableFrom(tt))
                        .SelectMany(tt => tt.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                        .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                        .Select(x => new
                        {
                            Controller = x.DeclaringType.Name,
                            Action = x.Name,
                            x.DeclaringType,
                        })
                        .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();

            
            var deprecatedResources = new DeprecatedResourcesRepository().Get();

            //httpRoutes = httpRoutes.Where(x => x.RouteTemplate.StartsWith("ledger-activities")).ToList();
            foreach (IHttpRoute httpRoute in httpRoutes)
            {
                try
                {
                    //Get the route template
                    var routeTemplate = httpRoute.RouteTemplate;
                    var apiName = string.Empty;
                    var versionless = false;

                    //gets api name
                    apiName = GetApiName(routeTemplate);

                    ApiResources resourceDto = null;
                    Dtos.Representation representationDto = null;

                    DeprecatedResources deprecatedResource = null;
                    if (deprecatedResources != null)
                        deprecatedResource = deprecatedResources.FirstOrDefault(x => x.Name == apiName);

                    //Allowed http method
                    var allowedMethod = string.Empty;

                    //get all constraints
                    IDictionary<string, object> constraints = httpRoute.Constraints;

                    if (((System.Web.Routing.HttpMethodConstraint)constraints[httpMethodConstraintName]).AllowedMethods != null &&
                        ((System.Web.Routing.HttpMethodConstraint)constraints[httpMethodConstraintName]).AllowedMethods.Any())
                    {
                        allowedMethod = ((System.Web.Routing.HttpMethodConstraint)httpRoute.Constraints[httpMethodConstraintName]).AllowedMethods.ToList()[0];
                    }

                    //skip GET/id 
                    if ((allowedMethod.ToLower().Equals("get")) && (routeTemplate.Split('/').Count() > 1))
                    {
                        continue;
                    }

                    //if (allowedMethod.ToLower().Equals("delete"))
                    //{
                    //    resourceDto = resourcesList.FirstOrDefault(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase));

                    //    if (resourceDto != null)
                    //    {
                    //        if (resourceDto.Representations == null) resourceDto.Representations = new List<Dtos.Representation>();

                    //        resourceDto.Representations.Add(new Dtos.Representation()
                    //        {
                    //            XMediaType = appJsonContentType,
                    //            Methods = new List<string>()
                    //            {
                    //                allowedMethod.ToLower()
                    //            }
                    //        });
                    //        continue;
                    //    }
                    //}

                    var headerVersionConstraintConstraintName = ((Web.Http.Routes.HeaderVersionConstraint)constraints[headerVersionConstraintName]);

                    if (headerVersionConstraintConstraintName != null)
                    {
                        var headerVersionConstraint = headerVersionConstraintConstraintName.GetType();

                        var pField = headerVersionConstraint
                                          .GetField("_customMediaTypes", BindingFlags.NonPublic | BindingFlags.Instance);
                        headerVersionConstraintValue = (string[])pField.GetValue(((Web.Http.Routes.HeaderVersionConstraint)constraints[headerVersionConstraintName]));

                        var satisfyVersionlessRequest = headerVersionConstraint
                                      .GetField("_satisfyVersionlessRequest", BindingFlags.NonPublic | BindingFlags.Instance);
                        versionless = (bool)satisfyVersionlessRequest.GetValue(((Web.Http.Routes.HeaderVersionConstraint)constraints[headerVersionConstraintName]));

                    }

                    if ((allowedMethod.ToLower().Equals("put")) || (allowedMethod.ToLower().Equals("post")))
                    {
                        try
                        {
                            var contentTypeConstraint = ((Web.Http.Routes.ContentTypeConstraint)constraints["contentType"])
                                            .GetType();

                            var pField2 = contentTypeConstraint
                                              .GetField("contentType", BindingFlags.NonPublic | BindingFlags.Instance);
                            var contentType = (string)pField2.GetValue(((Web.Http.Routes.ContentTypeConstraint)constraints["contentType"]));
                            headerVersionConstraintValue = new string[] { contentType  };
                        }
                        catch (Exception ex) { //do not throw 
                        }
                    }

                    
                    var eedmResponseFilterAttr = string.Empty;
                    List<string> filters = new List<string>();
                    List<NamedQuery> namedQueries = new List<NamedQuery>();

                    Dtos.DeprecationNotice deprecationNotice = null;

                    if ((deprecatedResource != null) && (headerVersionConstraintValue != null) && (headerVersionConstraintValue.Any()))
                    {
                        var deprecatedResourceRepresentation = deprecatedResource.Representations.FirstOrDefault(x => x.XMediaType == headerVersionConstraintValue[0]);
                        if ((deprecatedResourceRepresentation != null) && (deprecatedResourceRepresentation.DeprecationNotice != null))
                        {
                            deprecationNotice = new Dtos.DeprecationNotice()
                            {
                                DeprecatedOn = deprecatedResourceRepresentation.DeprecationNotice.DeprecatedOn.HasValue ?
                                        deprecatedResourceRepresentation.DeprecationNotice.DeprecatedOn : null,
                                SunsetOn = deprecatedResourceRepresentation.DeprecationNotice.SunsetOn.HasValue ?
                                        deprecatedResourceRepresentation.DeprecationNotice.SunsetOn : null,
                                Description = deprecatedResourceRepresentation.DeprecationNotice.Description
                            };
                        }
                    }

                    if (allowedMethod.ToLower().Equals("get"))
                    {
                        object controller = string.Empty;
                        object action = string.Empty;
                        object requestedContentType = string.Empty;


                        httpRoute.Defaults.TryGetValue("action", out action);
                        httpRoute.Defaults.TryGetValue("controller", out controller);
                        httpRoute.Defaults.TryGetValue("RequestedContentType", out requestedContentType);

                        var controlleraction = controlleractionlist.FirstOrDefault(x => x.Controller == string.Concat(controller.ToString(), "Controller"));
                        var type = controlleraction.DeclaringType;

                        var controllerAction = action.ToString();

                        if (type != null)
                        {
                            if (
                                ((headerVersionConstraintValue != null) && (headerVersionConstraintValue.Any())
                                && ((headerVersionConstraintValue[0].Contains("v6")) || (headerVersionConstraintValue[0].Contains("v7"))))
                                ||
                                ((requestedContentType != null)
                                && ((requestedContentType.ToString().Contains("v6")) || (requestedContentType.ToString().Contains("v7"))))
                                )
                            {
                                filters = LegacyFilters(keywords, type, action.ToString());
                            }
                            else
                            {
                                filters = GetFilters(filters, namedQueries, type, controllerAction);
                            }
                        }
                    }

                    //Check to see if resource list has the resource
                    if (!resourcesList.Any(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase)))
                    {
                        resourceDto = new ApiResources() { Name = apiName };
                        resourcesList.Add(resourceDto);
                    }

                    resourceDto = resourcesList.FirstOrDefault(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase));
                    
                    //if the isBulkSupported default value is set it will be true, false is default, so just check if the defaults contains the isBulkSupported key
                    bool isBulkSupportedOnRoute = httpRoute.Defaults.ContainsKey(IsBulkSupported);

                    //if bulk is supported check the representations to make sure the bulk representations is not there and if not add it
                    if (bulkLoadSupport && isBulkSupportedOnRoute)
                    {
                        //resourceDto.Representations.Add(new Dtos.Representation()
                        //{
                        //    XMediaType = BulkRequestMediaType,
                        //    Methods = new List<string>()
                        //    {
                        //        GetMethod,
                        //        PostMethod
                        //    }
                        //});
                        //resourceDto.Representations.Any(r => r.XMediaType.Equals(BulkRequestMediaType, StringComparison.OrdinalIgnoreCase)
                        var represDto = (resourceDto.Representations != null)  ? resourceDto.Representations.FirstOrDefault(repr => repr.XMediaType.Equals(BulkRequestMediaType, StringComparison.OrdinalIgnoreCase)) : null;
                        if (represDto == null)
                        {
                            var newRepresentation = new Dtos.Representation()
                            {
                                XMediaType = BulkRequestMediaType,
                                Methods = new List<string>()
                                    {
                                         GetMethod,
                                         PostMethod
                                    },                                                             
                            };
                            if (resourceDto.Representations == null) resourceDto.Representations = new List<Dtos.Representation>();
                            resourceDto.Representations.Add(newRepresentation);
                        }
                        else if (!represDto.Methods.Contains(allowedMethod.ToLower()))
                        {
                            resourceDto.Representations.Add(new Dtos.Representation()
                            {
                                XMediaType = BulkRequestMediaType,
                                Methods = new List<string>()
                                {
                                    GetMethod,
                                    PostMethod
                                }
                            });
                        }
                    }

                    //else default route of application/json content type
                    if ((versionless) || (allowedMethod.ToLower().Equals("delete")) || ((headerVersionConstraintValue != null && headerVersionConstraintValue.Any() && !headerVersionConstraintValue[0].Contains(mediaFormat))))
                    {
                        if (resourceDto != null)
                        {
                            if (resourceDto.Representations == null) resourceDto.Representations = new List<Dtos.Representation>();

                            var represDto = resourceDto.Representations.FirstOrDefault(repr => repr.XMediaType.Contains(appJsonContentType));
                            if (represDto == null)
                            {
                                var newRepresentation = new Dtos.Representation()
                                {
                                    XMediaType = appJsonContentType,
                                    Methods = new List<string>()
                                    {
                                        allowedMethod.ToLower()
                                    },
                                    Filters = filters != null && filters.Any() ? filters : null,
                                    NamedQueries = namedQueries != null && namedQueries.Any() ? namedQueries : null,
                                    DeprecationNotice = deprecationNotice != null ? deprecationNotice : null
                                };

                                InsertGetAllPatterns(newRepresentation, appJsonContentType, bulkLoadSupport, isBulkSupportedOnRoute);

                                resourceDto.Representations.Add(newRepresentation);
                            }
                            else if (!represDto.Methods.Contains(allowedMethod.ToLower()))
                            {
                                represDto.Methods.Add(allowedMethod.ToLower());
                                InsertGetAllPatterns(represDto, appJsonContentType, bulkLoadSupport, isBulkSupportedOnRoute);
                            }
                        }
                    }
                    else if (headerVersionConstraintValue != null && headerVersionConstraintValue.Any() && headerVersionConstraintValue[0].Contains(mediaFormat))
                    {
                        var tempXMediaType = headerVersionConstraintValue[0];

                        if (resourceDto.Representations == null)
                        {
                            resourceDto.Representations = new List<Dtos.Representation>();
                        }

                        representationDto = resourceDto.Representations.FirstOrDefault(r => r.XMediaType.Equals(tempXMediaType, StringComparison.OrdinalIgnoreCase));

                        if (representationDto == null)
                        {
                            var newRepresentation = new Dtos.Representation()
                            {
                                XMediaType = tempXMediaType,
                                Methods = new List<string>()
                                {
                                    allowedMethod.ToLower()
                                },
                                Filters = filters.Any() ? filters : null,
                                NamedQueries = namedQueries.Any() ? namedQueries : null,
                                DeprecationNotice = deprecationNotice != null ? deprecationNotice : null

                            };

                            InsertGetAllPatterns(newRepresentation, tempXMediaType, bulkLoadSupport, isBulkSupportedOnRoute);

                            resourceDto.Representations.Add(newRepresentation);
                        }
                        else if (!representationDto.Methods.Contains(allowedMethod.ToLower()))
                        {
                            if (filters.Any())
                                representationDto.Filters = filters;

                            representationDto.Methods.Add(allowedMethod.ToLower());

                            InsertGetAllPatterns(representationDto, tempXMediaType, bulkLoadSupport, isBulkSupportedOnRoute);
                        }
                        else if ((representationDto.Methods.Contains(allowedMethod.ToLower())) && (filters.Any()))
                        {
                            representationDto.Filters = filters;
                        }
                    }
                   
                }
                catch (Exception ex)
                {
                    //no need to throw since not all routes will have _customMediaTypes field
                }
            }
            _cacheProvider.Add(EEDM_WEBAPI_RESOURCES_CACHE_KEY, resourcesList, new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
            });
            return resourcesList;
        }

        /// <summary>
        /// check if there is a get, if there is check and see if paging is already in the getallpatterns section, if not add it
        /// </summary>
        /// <param name="representation">representation to check</param>
        /// <param name="mediaType"></param>
        /// <param name="isBulkSupportedOnRoute"></param>
        /// <param name="isBulkSupportedForClient"></param>
        private void InsertGetAllPatterns(Dtos.Representation representation, string mediaType, bool isBulkSupportedForClient = false, bool isBulkSupportedOnRoute = false)
        {
            
            if (representation == null || !representation.Methods.Contains(GetMethod)) return;

            if (representation.GetAllPatterns == null || !representation.GetAllPatterns.Any())
            {
                representation.GetAllPatterns = new List<GetAllPattern>()
                {new GetAllPattern()
                    {
                        Method = GetMethod,
                        Name = GetPagingName,
                        XMediaType = mediaType
                    }
                };

                if (isBulkSupportedOnRoute && isBulkSupportedForClient)
                {
                    representation.GetAllPatterns.Add(new GetAllPattern()
                    {
                        Method = PostMethod,
                        Name = PostBatchName,
                        XMediaType = BulkRequestMediaType
                    });
                }
            }
            else
            {
                if (!representation.GetAllPatterns.Any(r =>r.Name.Equals(GetPagingName, StringComparison.OrdinalIgnoreCase)))
                {
                    representation.GetAllPatterns.Add(new GetAllPattern()
                    {
                        Method = GetMethod,
                        Name = GetPagingName,
                        XMediaType = mediaType
                    });
                }

                //if bulk isn't marked as supported no need to search if entry is there or add one
                if (isBulkSupportedForClient && isBulkSupportedOnRoute && !representation.GetAllPatterns.Any(r => r.Name.Equals(PostBatchName, StringComparison.OrdinalIgnoreCase)))
                {
                    representation.GetAllPatterns.Add(new GetAllPattern()
                    {
                        Method = PostMethod,
                        Name = PostBatchName,
                        XMediaType = BulkRequestMediaType
                    });
                }
            }
        }

        /// <summary>
        /// Get Filters and named queries
        /// </summary>
        /// <param name="filters">collection of string</param>
        /// <param name="namedQueries">collection of named query objects</param>
        /// <param name="T">type</param>
        /// <param name="controllerAction">controller method name</param>
        /// <returns>collection of filters used in criteria filtergroup</returns>
        private List<string> GetFilters(List<string> filters, List<NamedQuery> namedQueries, Type T, string controllerAction)
        {
            var queryStringFilters = (QueryStringFilterFilter[])T.GetMethod(controllerAction)
                  .GetCustomAttributes(typeof(QueryStringFilterFilter), false);

            if (queryStringFilters != null)
            {
                var queryStringFilter = queryStringFilters.FirstOrDefault(x => x.FilterGroupName == "criteria");

                if (queryStringFilter != null)
                {
                    filters = IterateProperties("criteria", queryStringFilter.FilterType).ToList();
                }

                var namedFilters = queryStringFilters.Where(x => x.FilterGroupName != "criteria");
                if (namedFilters != null && namedFilters.Any())
                {
                    foreach (var namedFilter in namedFilters)
                    {
                        NamedQuery namedQuery = new NamedQuery();
                        namedQuery.Name = namedFilter.FilterGroupName;
                        namedQuery.Filters = IterateProperties(namedFilter.FilterGroupName, namedFilter.FilterType).ToList();
                        namedQueries.Add(namedQuery);
                    }
                }
            }
            return filters;
        }

        /// <summary>
        /// Determine if the type is parent
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>boolean</returns>
        private bool IsParent(Type type)
        {
            return
                   (type.GetCustomAttributes(typeof(DataContractAttribute), true).Any()
                    || type.GetCustomAttributes(typeof(JsonObjectAttribute), true).Any());
        }

        /// <summary>
        /// Get the name to be displayed
        /// </summary>
        /// <param name="prop">PropertyInfo</param>
        /// <returns>string</returns>
        private string GetDisplayName(PropertyInfo prop)
        {

            var dataMemberAttributes = (DataMemberAttribute[])prop.GetCustomAttributes(typeof(DataMemberAttribute), false);
            if (dataMemberAttributes != null && dataMemberAttributes.Any())
                return dataMemberAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.Name))).Name;
            var jsonPropertyAttributes = (JsonPropertyAttribute[])prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
            if (jsonPropertyAttributes != null && jsonPropertyAttributes.Any())
                return jsonPropertyAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.PropertyName))).PropertyName;

            return string.Empty;
        }

        /// <summary>
        /// Determine if a property has the FilterProperty attribute and should be displayed 
        /// </summary>
        /// <param name="prop">propertyinfo</param>
        /// <param name="filtername">string</param>
        /// <returns>boolean</returns>
        private bool IsFilter(PropertyInfo prop, string filtername)
        {
            FilterPropertyAttribute[] customAttributes = (FilterPropertyAttribute[])prop.GetCustomAttributes(typeof(FilterPropertyAttribute), false);  //prop.GetCustomAttributes();
            foreach (var customAttribute in customAttributes)
            {
                if ((customAttribute.Name.Contains(filtername)) && (!customAttribute.Ignore))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// IteratePropertie
        /// </summary>
        /// <param name="filterGroupName">string representing the filterGroup</param>
        /// <param name="T">property type</param>
        /// <param name="baseName">property name</param>
        /// <param name="checkForFilterGroup">validate the property is a member of a filterGroup.  used when a parent object is defined as 
        /// a filterable property, and all the children are then a memeber of that filter.</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<string> IterateProperties(string filterGroupName, Type T, string baseName = "", bool checkForFilterGroup = true)
        {
            var props = T.GetProperties();

            if (props == null)
                yield break;

            foreach (var property in props)
            {
                var name = GetDisplayName(property); // property.Name;
                var type = GetGenericType(property.PropertyType);

                // Is the property a parent type AND a member of a filter group
                // if so, then return all the children associated with it
                if ((IsParent(type)) && (IsFilter(property, filterGroupName)))
                {
                    foreach (var info in IterateProperties(filterGroupName, type, name, false))
                    {
                        yield return string.IsNullOrEmpty(baseName) ? info : string.Format("{0}.{1}", baseName, info);
                    }
                }
                //If the property is a parent that may have filterable properties, continue processing
                else if (IsParent(type))
                {
                    foreach (var info in IterateProperties(filterGroupName, type, name, checkForFilterGroup))
                    {
                        yield return string.IsNullOrEmpty(baseName) ? info : string.Format("{0}.{1}", baseName, info);
                    }
                }
                else
                {
                    if ((!checkForFilterGroup) || (IsFilter(property, filterGroupName)))
                    {
                        var displayName = GetDisplayName(property);
                        yield return string.IsNullOrEmpty(baseName) ? displayName : string.Format("{0}.{1}", baseName, displayName);
                    }
                }
            }
        }

        /// <summary>
        /// Get the generic type for a list
        /// </summary>
        /// <param name="T">Type</param>
        /// <returns>Type</returns>
        private Type GetGenericType(Type T)
        {
            if (!T.IsGenericType)
                return T;

            return T.GetGenericArguments()[0];
        }

        /// <summary>
        /// Get filters used for versions 6 and 7
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="T"></param>
        /// <param name="controllerAction"></param>
        /// <returns></returns>
        private List<string> LegacyFilters(List<string> keywords, Type T, string controllerAction)
        {
            List<string> filters = new List<string>();

            ValidateQueryStringFilter[] attrs = (ValidateQueryStringFilter[])T.GetMethod(controllerAction).GetCustomAttributes(typeof(ValidateQueryStringFilter), false);
            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    var queryParameters = attr.ValidQueryParameters.Where(q => !(string.IsNullOrEmpty(q)) && !keywords.Contains(q));
                    foreach (var queryParameter in queryParameters)
                    {
                        if (!(string.IsNullOrEmpty(queryParameter)))
                            filters.Add(queryParameter);
                    }
                }
            }
            return filters;
        }

        /// <summary>
        /// Gets the api name
        /// </summary>
        /// <param name="routeTemplate"></param>
        /// <returns></returns>
        private string GetApiName(string routeTemplate)
        {
            var idx = routeTemplate.IndexOf("/");
            string apiName = string.Empty;

            if (routeTemplate.StartsWith("qapi"))
            {
                apiName = routeTemplate.Substring(idx + 1);
                return apiName;
            }

            if (idx != -1)
            {
                apiName = routeTemplate.Substring(0, idx);
            }
            else
            {
                apiName = routeTemplate;
            }

            return apiName;
        }
    }
}