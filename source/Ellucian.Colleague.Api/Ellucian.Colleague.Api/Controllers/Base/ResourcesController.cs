﻿// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http.Routing;
using Ellucian.Colleague.Coordination.Base.Services;
using slf4net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ResourcesController : BaseCompressedApiController
    {
        private const string GetPagingName = "paging";
        private const string PostBatchName = "batch";
        private const string GetMethod = "get";
        private const string GetAllMethod = "get_all";
        private const string GetIdMethod = "get_id";
        private const string PostQapiMethod = "post_qapi";
        private const string PostMethod = "post";
        private const string EEDM_WEBAPI_RESOURCES_CACHE_KEY = "EEDM_WEBAPI_RESOURCES_CACHE_KEY";
        private const string BulkRequestMediaType = "application/vnd.hedtech.integration.bulk-requests.v1.0.0+json";
        private const string IsBulkSupported = "isBulkSupported";
        private const string appJsonContentType = "application/json";
        private const string mediaFormat = "application/vnd.hedtech.integration";
        private const string httpMethodConstraintName = "httpMethod";
        private const string headerVersionConstraintName = "headerVersion";
        private const string isEEdmSupported = "isEedmSupported";
        private const string isEthosEnabled = "isEthosEnabled";

        private ICacheProvider _cacheProvider;
        private readonly IBulkLoadRequestService _bulkLoadRequestService;
        private readonly ILogger _logger;
        private readonly IEthosApiBuilderService _ethosApiBuilderService;
        private readonly List<string> versionedSupportedMethods = new List<string>() { "put", "post", "get", "get_all", "get_id", "post_qapi" };
        private readonly List<string> versionlessSupportedMethods = new List<string>() { "get", "get_all", "get_id", "post_qapi", "delete" };

        /// <summary>
        /// ResourcesController
        /// </summary>
        public ResourcesController(IBulkLoadRequestService bulkLoadRequestService,
            IEthosApiBuilderService ethosApiBuilderService, ICacheProvider cacheProvider, ILogger logger)
        {
            _cacheProvider = cacheProvider;
            _bulkLoadRequestService = bulkLoadRequestService;
            _logger = logger;
            _ethosApiBuilderService = ethosApiBuilderService;
        }

        /// <summary>
        /// Retrieves version information for the Colleague Web API.
        /// </summary>
        /// <returns>Version information.</returns>
        public async Task<IEnumerable<ApiResources>> GetResources()
        {
            List<ApiResources> resourcesDtoList = new List<ApiResources>();
            
            var routeCollection = Configuration.Routes;
            var httpRoutes = routeCollection
                .Where(r => r.Defaults.Keys != null && ((r.Defaults.Keys.Contains(isEEdmSupported) && r.Defaults[isEEdmSupported].Equals(true)) || (r.Defaults.Keys.Contains(isEthosEnabled) && r.Defaults[isEthosEnabled].Equals(true))))
                .ToList();

            resourcesDtoList = await GetResourcesAsync(httpRoutes, mediaFormat, httpMethodConstraintName, headerVersionConstraintName);

            return resourcesDtoList.OrderBy(item => item.Name);
        }

        /// <summary>
        /// Gets all the resources
        /// </summary>
        /// <param name="mediaFormat"></param>
        /// <param name="httpMethodConstraintName"></param>
        /// <param name="headerVersionConstraintName"></param>
        /// <param name="httpRoutes"></param>
        public async Task<List<ApiResources>> GetResourcesAsync(List<IHttpRoute> httpRoutes, string mediaFormat, string httpMethodConstraintName, string headerVersionConstraintName)
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

            //try
            //{
            //    bulkLoadSupport = _bulkLoadRequestService.IsBulkLoadSupported();
            //}
            //catch (Exception e)
            //{
            //    _logger.Error(e, "Bulk Load Support check failed");
            //}

            ValidateQueryStringFilter validateQueryStringFilter = new ValidateQueryStringFilter();
            var keywords = validateQueryStringFilter.ValidQueryParameters.ToList();
            string[] headerVersionConstraintValue = null;

           
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


            List<DeprecatedResources> deprecatedResources = null;

            var deprecatedResourcesCacheKey = string.Concat(EEDM_WEBAPI_RESOURCES_CACHE_KEY, "_DeprecatedResources");

            if (_cacheProvider != null && _cacheProvider.Contains(deprecatedResourcesCacheKey))
            {
                deprecatedResources = _cacheProvider[deprecatedResourcesCacheKey] as List<DeprecatedResources>;
            }
            else
            {
                deprecatedResources = new DeprecatedResourcesRepository().Get();

                _cacheProvider.Add(deprecatedResourcesCacheKey, deprecatedResources, new System.Runtime.Caching.CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                });
            }

            List<EthosExtensibleData> allExtendedEthosConfigurations = new List<EthosExtensibleData>();
            try
            {
                allExtendedEthosConfigurations = (await _ethosApiBuilderService.GetAllExtendedEthosConfigurations(bypassCache, false)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "Error processing allExtendedEthosConfigurations");
            }

            //httpRoutes = httpRoutes.Where(x => x.RouteTemplate.StartsWith("schema")).ToList();
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
                   object isAdministrative = false;
                    httpRoute.Defaults.TryGetValue("isAdministrative", out isAdministrative);

                    if ((allowedMethod.ToLower().Equals("get")) && (routeTemplate.Split('/').Count() > 1) && isAdministrative == null) //(!routeTemplate.StartsWith("schema", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

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
                            if (constraints.ContainsKey("contentType"))
                            {
                                var contentTypeConstraint = ((Web.Http.Routes.ContentTypeConstraint)constraints["contentType"])
                                                .GetType();

                                var pField2 = contentTypeConstraint
                                                  .GetField("contentType", BindingFlags.NonPublic | BindingFlags.Instance);
                                var contentType = (string)pField2.GetValue(((Web.Http.Routes.ContentTypeConstraint)constraints["contentType"]));
                                headerVersionConstraintValue = new string[] { contentType };
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message, "Error at content type constraint");
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

                    /* To include PageSize in the response uncomment the following lines:
                            1)  pageSize variable declaration
                            2)  pageSize set using DefaultPageSize()  
                            3) the two instances where pageSize is set one the Dtos.Representation  */
                    //int pageSize = 0;
                    object controller = string.Empty;
                    object action = string.Empty;
                    object requestedContentType = string.Empty;
                   

                    if (allowedMethod.ToLower().Equals("get"))
                    {
                        
                        httpRoute.Defaults.TryGetValue("action", out action);
                        httpRoute.Defaults.TryGetValue("controller", out controller);
                        httpRoute.Defaults.TryGetValue("RequestedContentType", out requestedContentType);
                       

                        var controlleraction = controlleractionlist.FirstOrDefault(x => x.Controller == string.Concat(controller.ToString(), "Controller"));
                        var type = controlleraction.DeclaringType;

                        var controllerAction = action.ToString();

                        if (type != null)
                        {
 
                            //pageSize = DefaultPageSize(type, controllerAction);

                            if (
                                ((headerVersionConstraintValue != null) && (headerVersionConstraintValue.Any())
                                && ((headerVersionConstraintValue[0].Contains("v6")) || (headerVersionConstraintValue[0].Contains("v7"))))
                                ||
                                ((requestedContentType != null)
                                && ((requestedContentType.ToString().Contains("v6")) || (requestedContentType.ToString().Contains("v7"))))
                                )
                            {
                                filters = LegacyFilters(keywords, type, action.ToString());
                                namedQueries = LegacyNamedQueries(keywords, type, controllerAction);
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
                            Dtos.Customizations customizations = null;

                            var matchingConfiguration = allExtendedEthosConfigurations.Where(x => x.ApiResourceName.Equals(apiName, StringComparison.OrdinalIgnoreCase)).ToList();
                            if (matchingConfiguration != null && matchingConfiguration.Any())
                            {
                                if (versionless)
                                {
                                    var defaultVersion = GetEthosExtensibilityResourceDefaultVersion(allExtendedEthosConfigurations, apiName);
                                    var matchingConfig = matchingConfiguration.FirstOrDefault(x => string.IsNullOrEmpty(x.ApiVersionNumber) || x.ApiVersionNumber == defaultVersion);
                                    if (matchingConfig != null)
                                    {
                                        if (deprecationNotice == null && !string.IsNullOrEmpty(matchingConfig.DeprecationNotice))
                                        {
                                            deprecationNotice = new Dtos.DeprecationNotice()
                                            {
                                                DeprecatedOn = matchingConfig.DeprecationDate,
                                                Description = matchingConfig.DeprecationNotice,
                                                SunsetOn = matchingConfig.SunsetDate
                                            };
                                        }
                                    }
                                }
                                customizations = new Customizations()
                                {
                                    HasExtendedProperties = true,
                                };
                            }
                            var hasDataPrivacy = await _ethosApiBuilderService.CheckDataPrivacyByApi(apiName, bypassCache);
                            if (hasDataPrivacy)
                            {
                                if (customizations == null) customizations = new Customizations();
                                customizations.HasPrivacyRules = true;
                            }

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
                                    DeprecationNotice = deprecationNotice != null ? deprecationNotice : null,
                                    Customizations = customizations ?? null,
                                    //PageSize = pageSize
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

                        var versionOnly = ExtractVersionNumberOnly(tempXMediaType);

                        if (resourceDto.Representations == null)
                        {
                            resourceDto.Representations = new List<Dtos.Representation>();
                        }

                        representationDto = resourceDto.Representations.FirstOrDefault(r => r.XMediaType.Equals(tempXMediaType, StringComparison.OrdinalIgnoreCase));

                        if (representationDto == null)
                        {
                            Dtos.Customizations customizations = null;

                            var matchingConfiguration = allExtendedEthosConfigurations.Where(x => x.ApiResourceName.Equals(apiName, StringComparison.OrdinalIgnoreCase)).ToList();
                            if (matchingConfiguration != null && matchingConfiguration.Any())
                            {
                                var majorVersion = versionOnly.Split('.')[0];
                                var matchingConfig = matchingConfiguration.FirstOrDefault(x => string.IsNullOrEmpty(x.ApiVersionNumber) || x.ApiVersionNumber == versionOnly || x.ApiVersionNumber == majorVersion);
                                if (matchingConfig != null)
                                {
                                    if (deprecationNotice == null && !string.IsNullOrEmpty(matchingConfig.DeprecationNotice))
                                    {
                                        deprecationNotice = new Dtos.DeprecationNotice()
                                        {
                                            DeprecatedOn = matchingConfig.DeprecationDate,
                                            Description = matchingConfig.DeprecationNotice,
                                            SunsetOn = matchingConfig.SunsetDate
                                        };
                                    }

                                    customizations = new Customizations()
                                    {
                                        HasExtendedProperties = true,
                                    };
                                }
                            }
                            var hasDataPrivacy = await _ethosApiBuilderService.CheckDataPrivacyByApi(apiName, bypassCache);
                            if (hasDataPrivacy)
                            {
                                if (customizations == null) customizations = new Customizations();
                                customizations.HasPrivacyRules = true;
                            }

                            var newRepresentation = new Dtos.Representation()
                            {
                                XMediaType = tempXMediaType,
                                Methods = new List<string>()
                                {
                                    allowedMethod.ToLower()
                                },
                                //PageSize = pageSize
                                Filters = filters.Any() ? filters : null,
                                NamedQueries = namedQueries.Any() ? namedQueries : null,
                                DeprecationNotice = deprecationNotice != null ? deprecationNotice : null,
                                VersionNumber = versionOnly,
                                Customizations = customizations ?? null
                            };

                            InsertGetAllPatterns(newRepresentation, tempXMediaType, bulkLoadSupport, isBulkSupportedOnRoute);
                            AddMajorVersion(resourceDto.Representations, newRepresentation);

                            resourceDto.Representations.Add(newRepresentation);
                        }
                        else if (!representationDto.Methods.Contains(allowedMethod.ToLower()))
                        {
                            if (filters.Any())
                                representationDto.Filters = filters;

                            representationDto.Methods.Add(allowedMethod.ToLower());

                            InsertGetAllPatterns(representationDto, tempXMediaType, bulkLoadSupport, isBulkSupportedOnRoute);
                            AddMajorVersion(resourceDto.Representations, representationDto);
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
                    _logger.Error(ex.Message, "Route does not have _customMediaTypes field");
                }
            }


            #region extendedResources

            VersionNumberComparer versionNumberComparer = null;
            try
            {
                // Debug
                //allExtendedEthosConfigurations = (allExtendedEthosConfigurations.Where(x => x.ApiResourceName == "person-info").ToList());
                var matchingConfigurations = allExtendedEthosConfigurations.Where(x => x.HttpMethodsSupported != null && x.HttpMethodsSupported.Any()).ToList();

                foreach (var extendedEthosConfiguration in matchingConfigurations)
                {
                    var originalApiName = extendedEthosConfiguration.ApiResourceName;

                    var parentAPI = extendedEthosConfiguration.ParentApi;

                    var apiName = (string.IsNullOrEmpty(parentAPI)) ? originalApiName : parentAPI;

                    ApiResources resourceDto = null;
                    Dtos.DeprecationNotice deprecationNotice = null;
                    List<string> filters = new List<string>();
                    List<NamedQuery> namedQueries = new List<NamedQuery>();

                    //For now, we only want to add resources that havent previously existed.
                    if (!resourcesList.Any(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase)))
                    {
                        resourceDto = new ApiResources() { Name = apiName };
                        resourcesList.Add(resourceDto);
                    }
                    
                    if (!string.IsNullOrEmpty(extendedEthosConfiguration.DeprecationNotice))
                    {
                        deprecationNotice = new Dtos.DeprecationNotice()
                        {
                            DeprecatedOn = extendedEthosConfiguration.DeprecationDate,
                            Description = extendedEthosConfiguration.DeprecationNotice,
                            SunsetOn = extendedEthosConfiguration.SunsetDate
                        };
                    }
                    if (extendedEthosConfiguration.ExtendedDataFilterList != null)
                    {
                        var extendedDataFilterList = extendedEthosConfiguration.ExtendedDataFilterList.Where(edfl => edfl.NamedQuery == false);
                        if (extendedDataFilterList != null)
                        {
                            foreach (var dataFilterObj in extendedDataFilterList)
                                filters.Add(dataFilterObj.FullJsonPath.Replace("/", ".").Replace("[]", ""));
                        }

                        var namedQueryFilterList = extendedEthosConfiguration.ExtendedDataFilterList.Where(edfl => edfl.NamedQuery == true);
                        if (namedQueryFilterList != null)
                        {
                            foreach (var namedQuery in namedQueryFilterList)
                            {
                                var item = new NamedQuery();
                                item.Name = namedQuery.ColleagueColumnName;
                                item.Filters = new List<string>()
                                {
                                     namedQuery.FullJsonPath.Replace("/", ".").Replace("[]", "")
                                };
                                namedQueries.Add(item);
                            }
                        }
                    }
                    Customizations customizations = null;

                    // Only report the resource as Custom if defined by the client
                    // and NOT delivered by Ellucian.
                    if (extendedEthosConfiguration.IsCustomResource)
                    {
                        customizations = new Customizations()
                        {
                            IsCustomResource = true
                        };
                    }

                    var hasDataPrivacy = await _ethosApiBuilderService.CheckDataPrivacyByApi(apiName, bypassCache);
                    if (hasDataPrivacy)
                    {
                        if (customizations == null) customizations = new Customizations();
                        customizations.HasPrivacyRules = true;
                    }

                    resourceDto = resourcesList.FirstOrDefault(res => res.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase));                
                    if (resourceDto != null)
                    {
                        bool getAllSupported = true;
                        if (resourceDto.Representations == null) resourceDto.Representations = new List<Dtos.Representation>();
                        var represDto = resourceDto.Representations.FirstOrDefault(repr => repr.XMediaType.Contains(appJsonContentType));
                        if (represDto == null)
                        {
                            var supported =  extendedEthosConfiguration.HttpMethodsSupported
                                .Where(z => versionlessSupportedMethods.Contains(z.ToLower()))
                                .Select(x => x.ToLower()).ToList();

                            getAllSupported = true;
                            if (!supported.Contains(GetAllMethod) && !supported.Contains(PostQapiMethod) && !supported.Contains(GetMethod))
                            {
                                getAllSupported = false;
                            }
                            if (supported.Contains(GetMethod))
                            {
                                supported = supported.Where(z => !z.Equals(GetIdMethod) && !z.Equals(GetAllMethod) && !z.Equals(PostQapiMethod)).ToList();
                            }
                            else if (!supported.Contains(GetAllMethod) && !supported.Contains(PostQapiMethod))
                            {
                                if (supported.Contains(GetIdMethod) && filters != null)
                                {
                                    filters = filters.Where(z => z.StartsWith("id.")).Select(x => x.Split('.')[1]).ToList();
                                }
                                else
                                {
                                    filters = null;
                                }
                            }
                            else if (supported.Contains(GetAllMethod) && supported.Contains(GetIdMethod) && supported.Contains(PostQapiMethod))
                            {
                                supported.Add(GetMethod);
                                supported = supported.Where(z => !z.Equals(GetIdMethod) && !z.Equals(GetAllMethod) && !z.Equals(PostQapiMethod)).ToList();
                            }
                            if (supported.Contains(GetAllMethod) || supported.Contains(GetIdMethod) || supported.Contains(PostQapiMethod))
                            {
                                if (!supported.Contains(GetMethod))
                                {
                                    supported.Add(GetMethod);
                                }
                                supported = supported.Where(z => !z.Equals(GetIdMethod) && !z.Equals(GetAllMethod) && !z.Equals(PostQapiMethod)).ToList();

                            }

                            // Remove special "id." filters from list of any remaining filters
                            if (filters != null) filters = filters.Where(z => !z.StartsWith("id.")).ToList();
                            if (!getAllSupported) filters = null;

                            var newRepresentationVersionless = new Dtos.Representation()
                            {
                                XMediaType = appJsonContentType,
                                Methods = supported,                               
                                Filters = filters != null && filters.Any() ? filters : null,
                                NamedQueries = namedQueries != null && namedQueries.Any() ? namedQueries : null,
                                DeprecationNotice = deprecationNotice != null ? deprecationNotice : null,
                                //PageSize = pageSize
                                VersionNumber = extendedEthosConfiguration.ApiVersionNumber,
                                Customizations = customizations ?? null
                            };

                            if (getAllSupported)
                                InsertGetAllPatterns(newRepresentationVersionless, appJsonContentType, bulkLoadSupport, false);
                            resourceDto.Representations.Add(newRepresentationVersionless);
                        }
                        // alternative representations will never be the default route
                        else if (string.IsNullOrEmpty(parentAPI)) 
                        {
                            // If a custom endpoint has already been added, and defined as versionless, 
                            // then we need to determine if this route should be the versionless instead.  
                            // The versionless route will always have the largest version number.                         
                            if (versionNumberComparer == null)
                            {
                                versionNumberComparer = new VersionNumberComparer();
                            }

                            // Remove special "id." filters from list of any remaining filters
                            if (filters != null) filters = filters.Where(z => !z.StartsWith("id.")).ToList();

                            if (versionNumberComparer.Compare(extendedEthosConfiguration.ApiVersionNumber, represDto.VersionNumber) == 1)
                            {
                                represDto.Filters = filters != null && filters.Any() ? filters : null;
                                represDto.NamedQueries = namedQueries != null && namedQueries.Any() ? namedQueries : null;
                                represDto.DeprecationNotice = deprecationNotice != null ? deprecationNotice : null;
                                //if (!represDto.Methods.Contains(allowedMethod.ToLower())) 
                                //    represDto.Methods.Add(allowedMethod.ToLower());
                                represDto.VersionNumber = extendedEthosConfiguration.ApiVersionNumber;
                                represDto.Customizations = customizations ?? null;
                            }
                        }

                        var xMediaType = extendedEthosConfiguration.ExtendedSchemaType;

                        var versionSupported = extendedEthosConfiguration.HttpMethodsSupported
                            .Where(z => versionedSupportedMethods.Contains(z.ToLower()))
                            .Select(x => x.ToLower()).ToList();

                        getAllSupported = true;
                        if (!versionSupported.Contains(GetAllMethod) && !versionSupported.Contains(PostQapiMethod) && !versionSupported.Contains(GetMethod))
                        {
                            getAllSupported = false;
                        }
                        if (versionSupported.Contains(GetMethod))
                        {
                            versionSupported = versionSupported.Where(z => !z.Equals(GetIdMethod) && !z.Equals(GetAllMethod) && !z.Equals(PostQapiMethod)).ToList();
                        }
                        else if (!versionSupported.Contains(GetAllMethod) && !versionSupported.Contains(PostQapiMethod))
                        {
                            if (versionSupported.Contains(GetIdMethod) && filters != null)
                            {
                                filters = filters.Where(z => z.StartsWith("id.")).Select(x => x.Split('.')[1]).ToList();
                            }
                            else
                            {
                                filters = null;
                            }
                        }
                        else if (versionSupported.Contains(GetAllMethod) && versionSupported.Contains(GetIdMethod) && versionSupported.Contains(PostQapiMethod))
                        {
                            versionSupported.Add(GetMethod);
                            versionSupported = versionSupported.Where(z => !z.Equals(GetIdMethod) && !z.Equals(GetAllMethod) && !z.Equals(PostQapiMethod)).ToList();
                        }
                        if (versionSupported.Contains(GetAllMethod) || versionSupported.Contains(GetIdMethod) || versionSupported.Contains(PostQapiMethod))
                        {
                            if (!versionSupported.Contains(GetMethod))
                            {
                                versionSupported.Add(GetMethod);
                            }
                            versionSupported = versionSupported.Where(z => !z.Equals(GetIdMethod) && !z.Equals(GetAllMethod) && !z.Equals(PostQapiMethod)).ToList();

                        }

                        // Remove special "id." filters from list of any remaining filters
                        if (filters != null) filters = filters.Where(z => !z.StartsWith("id.")).ToList();
                        if (!getAllSupported) filters = null;

                        var newRepresentation = new Dtos.Representation()
                        {
                            XMediaType = xMediaType,
                            Methods = versionSupported, 
                            Filters = filters != null && filters.Any() ? filters : null,
                            NamedQueries = namedQueries != null && namedQueries.Any() ? namedQueries : null,
                            DeprecationNotice = deprecationNotice != null ? deprecationNotice : null,
                            //PageSize = pageSize
                            VersionNumber = extendedEthosConfiguration.ApiVersionNumber,
                            Customizations = customizations ?? null
                        };

                        if (getAllSupported)
                            InsertGetAllPatterns(newRepresentation, xMediaType, bulkLoadSupport, false);

                        resourceDto.Representations.Add(newRepresentation);

                        AddMajorVersion(resourceDto.Representations, newRepresentation, extendedEthosConfiguration.IsCustomResource);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "Error processing allExtendedEthosConfigurations");
            }
            #endregion 

            _cacheProvider.Add(EEDM_WEBAPI_RESOURCES_CACHE_KEY, resourcesList, new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
            });
            return resourcesList;
        }

        /// <summary>
        /// returns the default version of an API
        /// </summary>
        /// <param name="allExtendedEthosConfigurations"> list of configuration</param>
        /// <param name="resourceName"> Name of the API</param>
        /// <returns>Version number.  May contain none, or unknown number of decimals</returns>
        private string GetEthosExtensibilityResourceDefaultVersion(List<EthosExtensibleData> allExtendedEthosConfigurations, string resourceName)
        {

            string defaultVersion = string.Empty;

            if (allExtendedEthosConfigurations != null && allExtendedEthosConfigurations.Any())
            {
                var matchingExtendedConfigData = allExtendedEthosConfigurations.Where(e =>
                    e.ApiResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
                var availableVersions = matchingExtendedConfigData.Where(e => !string.IsNullOrEmpty(e.ApiVersionNumber)).Select(e => e.ApiVersionNumber).OrderBy(n => n.Split('.')[0]).ToList();
                defaultVersion = availableVersions.LastOrDefault();
                if (string.IsNullOrEmpty(defaultVersion))
                {
                    defaultVersion = "1.0.0";
                }

            }
            return defaultVersion;
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

        /// <summary>
        /// publishing the major version route
        /// </summary>
        /// <param name="representations"></param>
        /// <param name="representationToCompare"></param>    
        /// <param name="isCustom">Flag indicating this route is spec based</param> 
        private void AddMajorVersion(List<Dtos.Representation> representations ,
            Dtos.Representation representationToCompare, bool isCustom = false)
        {
            if (representationToCompare == null || representations == null 
                || string.IsNullOrEmpty(representationToCompare.VersionNumber)
                || string.IsNullOrEmpty(representationToCompare.XMediaType))
                return ;
            try
            {
                //does the major version representation already exist for this mediaType?
                //var found = representations.Any(r => r.XMediaType == representationToCompare.XMediaType
                //    && r.MajorVersionAdded);
              
                //versionNumber is expected to be a string in the format {i} or {i.i.i} where i is an integer
                var versionNumber = representationToCompare.VersionNumber;
                //var xMediaType = representationToCompare.XMediaType;
                var first = versionNumber.Split(new char[] { '.' })
                       .Select(xx => int.Parse(xx)).ToList();

                // if the version isnt found, or is already a single digit, then we dont need to do anything else
                if (first == null || !first.Any() || first.Count == 1)
                {
                    return;
                }
                
                var majorVersionXMediaType = representationToCompare
                        .XMediaType.Replace(versionNumber, first[0].ToString());
                //remove beta from the major version
                majorVersionXMediaType = majorVersionXMediaType.Replace("-beta", "");
                //does the major version representation already exist for this mediaType?
                var found = representations.Any(r => r.XMediaType == majorVersionXMediaType);

                if (found)
                    return;

                var majorVersionRepresentation = new Dtos.Representation()
                {
                    //GetAllPatterns = representationToCompare.GetAllPatterns,
                    MajorVersionAdded = representationToCompare.MajorVersionAdded,
                    DeprecationNotice = representationToCompare.DeprecationNotice,
                    Filters = representationToCompare.Filters,
                    Methods = representationToCompare.Methods,
                    NamedQueries = representationToCompare.NamedQueries,
                    VersionNumber = representationToCompare.VersionNumber,
                    XMediaType = representationToCompare.XMediaType,
                    Customizations = representationToCompare.Customizations
                };

                if (isCustom)
                {
                    if (majorVersionRepresentation.Customizations == null) majorVersionRepresentation.Customizations = new Customizations();
                    majorVersionRepresentation.Customizations.IsCustomResource = true;
                }
                majorVersionRepresentation.XMediaType = majorVersionXMediaType;
                //update InsertGetAllPatterns
                if ((representationToCompare.GetAllPatterns != null) && (representationToCompare.GetAllPatterns.Any()))
                {
                    var getAllPatterns = new List<GetAllPattern>();
                    foreach (var pattern in representationToCompare.GetAllPatterns)
                    {
                        if (pattern.Name.Equals("paging"))
                        {
                            GetAllPattern getAllPattern = new GetAllPattern();
                            getAllPattern.XMediaType = pattern.XMediaType.Replace(versionNumber, first[0].ToString()).Replace("-beta","");
                            getAllPattern.Name = pattern.Name;
                            getAllPattern.Method = pattern.Method;
                            getAllPatterns.Add(getAllPattern);
                        }
                        else
                        {
                            getAllPatterns.Add(pattern);
                        }
                    }
                    if (getAllPatterns.Any())
                        majorVersionRepresentation.GetAllPatterns = getAllPatterns;
                }
               // majorVersionRepresentation.MajorVersionAdded = true;
                representations.Add(majorVersionRepresentation);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "Error adding major version representation");
            }
            return;

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
            
            if (representation == null || (!representation.Methods.Contains(GetMethod) && !representation.Methods.Contains(GetAllMethod))) return;

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
                    var filterCacheKey = string.Concat("criteria_", controllerAction, "_", T.FullName);

                    if (_cacheProvider != null && _cacheProvider.Contains(filterCacheKey))
                    {
                        filters = _cacheProvider[filterCacheKey] as List<string>;
                    }
                    else
                    {
                        filters = IterateProperties("criteria", queryStringFilter.FilterType).ToList();

                        _cacheProvider.Add(filterCacheKey, filters, new System.Runtime.Caching.CacheItemPolicy()
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                        });
                    }
                }

                var namedFilters = queryStringFilters.Where(x => x.FilterGroupName != "criteria");
                if (namedFilters != null && namedFilters.Any())
                {
                    foreach (var namedFilter in namedFilters)
                    {
                        NamedQuery namedQuery = new NamedQuery();
                        List<string> namedQueryFilters = null;

                        namedQuery.Name = namedFilter.FilterGroupName;
                        var filterCacheKey = string.Concat(namedFilter.FilterGroupName, "_", controllerAction, "_", T.FullName);

                        if (_cacheProvider != null && _cacheProvider.Contains(filterCacheKey))
                        {
                            namedQueryFilters = _cacheProvider[filterCacheKey] as List<string>;
                        }
                        else
                        {
                            //filters = IterateProperties("criteria", queryStringFilter.FilterType).ToList();
                            namedQueryFilters = IterateProperties(namedFilter.FilterGroupName, namedFilter.FilterType).ToList();
                            _cacheProvider.Add(filterCacheKey, namedQueryFilters, new System.Runtime.Caching.CacheItemPolicy()
                            {
                                AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                            });
                        }

                        if (namedQueryFilters != null && namedQueryFilters.Any())
                        {
                            namedQuery.Filters = namedQueryFilters;
                        }
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
            if (prop == null)
                return string.Empty;

            var dataMemberAttributes = (DataMemberAttribute[])prop.GetCustomAttributes(typeof(DataMemberAttribute), false);
            if (dataMemberAttributes != null && dataMemberAttributes.Any())
                return dataMemberAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.Name))).Name;
            var jsonPropertyAttributes = (JsonPropertyAttribute[])prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
            if (jsonPropertyAttributes != null && jsonPropertyAttributes.Any())
                return jsonPropertyAttributes.FirstOrDefault(x => !(string.IsNullOrEmpty(x.PropertyName))).PropertyName;

            return string.Empty;
        }

        private int DefaultPageSize(Type T, string controllerAction)
        {
            //var customAttribute = (PagingFilter)prop.GetCustomAttribute(typeof(PagingFilter), false);  //prop.GetCustomAttributes();
            //foreach (var customAttribute in customAttributes)
            // {
            try
            {
                var customAttribute = (PagingFilter)T.GetMethod(controllerAction).GetCustomAttribute(typeof(PagingFilter), false);

                if (customAttribute.DefaultLimit != 0)
                {
                    return customAttribute.DefaultLimit;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return 0;
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
            if (customAttributes != null)
            {
                foreach (var customAttribute in customAttributes)
                {
                    if (customAttribute.Name != null)
                    {
                        if ((customAttribute.Name.Contains(filtername)) && (!customAttribute.Ignore))
                        {
                            return true;
                        }
                    }
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
                    if (attr.ValidQueryParameters != null)
                    {
                        var queryParameters = attr.ValidQueryParameters.Where(q => !(string.IsNullOrEmpty(q)) && !keywords.Contains(q));
                        foreach (var queryParameter in queryParameters)
                        {
                            if (!(string.IsNullOrEmpty(queryParameter)))
                                filters.Add(queryParameter);
                        }
                    }
                }
            }
            return filters;
        }

        /// <summary>
        /// Get filters used for versions 6 and 7
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="T"></param>
        /// <param name="controllerAction"></param>
        /// <returns></returns>
        private List<NamedQuery> LegacyNamedQueries(List<string> keywords, Type T, string controllerAction)
        {
            var filters = new List<NamedQuery>();
            try
            {
                ValidateQueryStringFilter[] attrs = (ValidateQueryStringFilter[])T.GetMethod(controllerAction).GetCustomAttributes(typeof(ValidateQueryStringFilter), false);
                if (attrs != null)
                {
                    foreach (var attr in attrs)
                    {
                        if (attr.NamedQueries != null)
                        {
                            var queryParameters = attr.NamedQueries.Where(q => !(string.IsNullOrEmpty(q)) && !keywords.Contains(q));
                            foreach (var queryParameter in queryParameters)
                            {
                                if (!(string.IsNullOrEmpty(queryParameter)))
                                    filters.Add(new NamedQuery() { Name = queryParameter, Filters = new List<string>() { queryParameter } });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // do not throw error is named queries can not be retrieved on legacy versions.
                _logger.Error(ex.Message, "Named queries cannot be retrieved on legacy versions");
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

            try
            {
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
            }
            catch
            {
                return 0;
            }

            return 0;
        }
    }
}