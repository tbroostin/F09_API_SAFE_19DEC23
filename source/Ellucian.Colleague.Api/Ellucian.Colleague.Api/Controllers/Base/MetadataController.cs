// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Utilities;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class MetadataController : BaseCompressedApiController
    {
        private const string EEDM_WEBAPI_METADATA_CACHE_KEY = "EEDM_WEBAPI_METADATA_CACHE_KEY";
        private const string appJsonContentType = "application/json";
        private const string mediaFormat = "vnd.hedtech.integration";
        private const string httpMethodConstraintName = "httpMethod";
        private const string headerVersionConstraintName = "headerVersion";
        private const string isEEdmSupported = "isEedmSupported";
        private const string isEthosEnabled = "isEthosEnabled";
        private const string sourceSystem = "Colleague";

        private const string GUID_PATTERN = "^[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}$";

        private readonly ICacheProvider _cacheProvider;
        private readonly ILogger _logger;
        private readonly IEthosApiBuilderService _ethosApiBuilderService;

        /// <summary>
        ///MetadataController
        /// </summary>
        public MetadataController(
            IEthosApiBuilderService ethosApiBuilderService, ICacheProvider cacheProvider, ILogger logger)
        {
            _cacheProvider = cacheProvider;
            _logger = logger;
            _ethosApiBuilderService = ethosApiBuilderService;
        }

        /// <summary>
        /// Retrieves all the openAPI Specifications for a resource
        /// </summary>
        /// <returns>OpenAPI Specifications version 3.0.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<object>> GetOpenApiMetadata([FromUri] string resourceName)
        {
            bool bypassCache = false;
            if ((Request != null) && (Request.Headers.CacheControl != null))
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(resourceName))
            {

                throw CreateHttpResponseException(new IntegrationApiException("",
                    new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
                    "API name is needed to return OpenAPI specifications.")));
            }
            try
            {
                var routeCollection = Configuration.Routes;
                var httpRoutes = routeCollection
                       .Where(r => r.Defaults.Keys != null && (r.Defaults.Keys.Contains(isEEdmSupported) && r.Defaults[isEEdmSupported].Equals(true)) || (r.Defaults.Keys.Contains(isEthosEnabled) && r.Defaults[isEthosEnabled].Equals(true)))
                         .ToList();

                if (!string.IsNullOrEmpty(resourceName))
                {
                    httpRoutes = httpRoutes.Where(x => x.RouteTemplate.StartsWith(resourceName)).ToList();
                }
                var openApidocs = await GetOpenApiAsync(httpRoutes, resourceName, bypassCache);
                var openApiDocObjects = new List<object>();
                if (openApidocs != null && openApidocs.Any())
                {
                    foreach (var openApidoc in openApidocs)
                    {
                        try
                        {
                            var openApiJson = openApidoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                            if (!string.IsNullOrEmpty(openApiJson))
                            {
                                var openApiJsonObject = JsonConvert.DeserializeObject(openApiJson);
                                if (openApiJsonObject != null)
                                {
                                    openApiDocObjects.Add(openApiJsonObject);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.ToString() + openApidoc);
                        }
                    }
                }
                return openApiDocObjects;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves openAPI Specifications for a resource for a particular version
        /// </summary>
        /// <returns>OpenAPI Specifications version 3.0.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<object> GetOpenApiMetadataByVersion([FromUri] string resourceName, [FromUri] string version)
        {
            bool bypassCache = false;
            if ((Request != null) && (Request.Headers.CacheControl != null))
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(resourceName))
            {

                throw new IntegrationApiException("",
                    new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
                    "API name is needed to return OpenAPI specifications."));
            }
            if (string.IsNullOrEmpty(version))
            {

                throw new IntegrationApiException("",
                    new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
                    "Version is needed to return OpenAPI specifications for a specific version."));
            }

            try
            {
                var routeCollection = Configuration.Routes;
                var httpRoutes = routeCollection
                       .Where(r => r.Defaults.Keys != null && (r.Defaults.Keys.Contains(isEEdmSupported) && r.Defaults[isEEdmSupported].Equals(true) ||
                       r.Defaults.Keys.Contains(isEthosEnabled) && r.Defaults[isEthosEnabled].Equals(true)))
                         .ToList();

                if (!string.IsNullOrEmpty(resourceName))
                {
                    httpRoutes = httpRoutes.Where(x => x.RouteTemplate.StartsWith(resourceName)).ToList();
                }
                var openApidocs = await GetOpenApiAsync(httpRoutes, resourceName, bypassCache);
                var versionNumberComparer = new MetadataVersionNumberComparer();
                if (openApidocs != null && openApidocs.Any())
                {
                    IEnumerable<OpenApiDocument> openApiDocs = null;
                    string latestVersion = "0.0.0";
                    if (version.ToLower() != "latest")
                    {
                        latestVersion = version;
                    }
                    else
                    {
                        // Sorting isn't giving us what we want in many cases.
                        //var sortedOpenApiDocs = from doc in openApidocs
                        //                        orderby doc.Info.Version descending
                        //                        select doc;
                        //openApiDocs = sortedOpenApiDocs.ToList();
                        foreach (var openApiDoc in openApidocs)
                        {
                            if (openApiDoc.Info != null && !string.IsNullOrEmpty(openApiDoc.Info.Version))
                            {
                                if (versionNumberComparer.Compare(openApiDoc.Info.Version, latestVersion) == 1)
                                {
                                    latestVersion = openApiDoc.Info.Version;
                                }
                            }
                        }
                    }

                    openApiDocs = openApidocs.Where(ver => ver.Info != null && !string.IsNullOrEmpty(ver.Info.Version) && versionNumberComparer.Compare(ver.Info.Version, latestVersion) == 1);

                    if (openApiDocs != null && openApiDocs.Any())
                    {
                        List<object> returnObject = new List<object>();

                        foreach (var openApiDoc in openApiDocs)
                        {
                            try
                            {
                                var openApiJson = openApiDoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                                if (!string.IsNullOrEmpty(openApiJson))
                                {
                                    returnObject.Add(JsonConvert.DeserializeObject(openApiJson));
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex.ToString() + openApiDoc);
                            }
                        }
                        if (returnObject.Count() > 1)
                            return returnObject;
                        else
                            return returnObject.FirstOrDefault();
                    }
                    else
                    {
                        throw new IntegrationApiException("",
                    new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.",
                    "OpenAPI specifications does not exist for this version."));
                    }
                }
                else
                {
                    throw new KeyNotFoundException(string.Format("No openAPI specifications found for version '{0}' of the resource {1}", version, resourceName));
                }
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update not supported
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        [HttpPut]
        public IEnumerable<object> PutOpenApiMetadata([FromBody] object schema)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create not supported
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<object> PostOpenApiMetadata([FromBody] object schema)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete not supported
        /// </summary>
        /// <param name="resourceName"></param>
        [HttpDelete]
        public void DeleteOpenApiMetadata(string resourceName)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <summary>
        /// Gets all the openAPI specifications for a spec-based or Business Process API
        /// </summary>
        /// <param name="httpRoutes"></param>
        /// <param name="selectedResource"></param>
        /// <param name="bypassCache"></param>
        private async Task<IEnumerable<OpenApiDocument>> GetOpenApiAsync(List<IHttpRoute> httpRoutes,
            string selectedResource, bool bypassCache = false)
        {
            var metadataCacheKey = EEDM_WEBAPI_METADATA_CACHE_KEY + selectedResource;
            var openApiDocs = new List<OpenApiDocument>();
            try
            {
                if (bypassCache == false)
                {
                    if (_cacheProvider != null && _cacheProvider.Contains(metadataCacheKey))
                    {
                        openApiDocs = _cacheProvider[metadataCacheKey] as List<OpenApiDocument>;
                        return openApiDocs;
                    }
                }
                EthosApiConfiguration apiConfiguration = new EthosApiConfiguration();
                List<Domain.Base.Entities.EthosExtensibleData> apiVersionConfigs = new List<Domain.Base.Entities.EthosExtensibleData>();
                if (httpRoutes != null && httpRoutes.Any())
                {
                    var apiConfigurationTuple = await BuildApiConfigurationFromRoutesAsync(httpRoutes, selectedResource, bypassCache);
                    apiConfiguration = apiConfigurationTuple.Item1;
                    apiVersionConfigs = apiConfigurationTuple.Item2;
                }
                else
                {
                    // get api configuration Info
                    EthosResourceRouteInfo routeInfo = new EthosResourceRouteInfo()
                    {
                        ResourceName = selectedResource
                    };
                    //get EDM.EXTENSIONS
                    apiConfiguration = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfo, bypassCache);
                    if (apiConfiguration == null)
                    {
                        return openApiDocs;
                    }
                    //get all EDM.EXT.VERSIONS
                    //var AllapiVersionConfigs = await _ethosApiBuilderService.GetAllExtendedEthosConfigurations(bypassCache);
                    apiVersionConfigs = await _ethosApiBuilderService.GetExtendedEthosVersionsConfigurationsByResource(routeInfo, bypassCache, true);
                    if (apiVersionConfigs == null || !apiVersionConfigs.Any())
                    {
                        return openApiDocs;
                    }
                }
                //we have all the specifications data that we need
                foreach (var apiVersionConfig in apiVersionConfigs)
                {
                    var openApiDoc = new OpenApiDocument();
                    //build the info oject
                    openApiDoc.Info = BuildOpenApiInfoProperty(apiConfiguration, apiVersionConfig);
                    //build servers object
                    openApiDoc.Servers = BuildOpenApiServersProperty();
                    //build paths object
                    openApiDoc.Paths = BuildOpenApiPathsProperty(apiConfiguration, apiVersionConfig);
                    //add components to the document
                    openApiDoc.Components = BuildOpenApiComponentsProperty(apiConfiguration, apiVersionConfig);
                    openApiDocs.Add(openApiDoc);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            _cacheProvider.Add(metadataCacheKey, openApiDocs, new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
            });
            return openApiDocs;
        }

        private async Task<Tuple<EthosApiConfiguration, List<Domain.Base.Entities.EthosExtensibleData>>> BuildApiConfigurationFromRoutesAsync(List<IHttpRoute> httpRoutes,
            string selectedResource, bool bypassCache = false)
        {
            List<Domain.Base.Entities.EthosExtensibleData> extensibleDataList = new List<Domain.Base.Entities.EthosExtensibleData>();
            EthosApiConfiguration apiConfiguration = new EthosApiConfiguration();

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

            if (!string.IsNullOrEmpty(selectedResource))
            {
                httpRoutes = httpRoutes.Where(x => x.RouteTemplate.StartsWith(selectedResource)).ToList();
            }
            apiConfiguration.ResourceName = selectedResource;
            apiConfiguration.ReleaseStatus = "R";
            apiConfiguration.ApiDomain = "";
            apiConfiguration.ApiType = "web";
            apiConfiguration.HttpMethods = new List<EthosApiSupportedMethods>();
            Dictionary<string, List<string>> mediaTypeMethodsSupported = new Dictionary<string, List<string>>();

            foreach (IHttpRoute httpRoute in httpRoutes)
            {
                try
                {
                    //Get the route template
                    var routeTemplate = httpRoute.RouteTemplate;
                    var apiName = string.Empty;

                    //gets api name
                    apiName = routeTemplate;

                    //Allowed http method
                    var allowedMethod = string.Empty;

                    //get all constraints
                    IDictionary<string, object> constraints = httpRoute.Constraints;

                    if (((System.Web.Routing.HttpMethodConstraint)constraints[httpMethodConstraintName]).AllowedMethods != null &&
                        ((System.Web.Routing.HttpMethodConstraint)constraints[httpMethodConstraintName]).AllowedMethods.Any())
                    {
                        allowedMethod = ((System.Web.Routing.HttpMethodConstraint)httpRoute.Constraints[httpMethodConstraintName]).AllowedMethods.ToList()[0];
                        var routeSplit = routeTemplate.Split('/');
                        if (routeSplit.Count() == 1)
                        {
                            if (allowedMethod != "POST")
                                allowedMethod = "GET_ALL";
                        }
                        else if (routeSplit.Count() == 2)
                        {
                            if (routeSplit[0].Equals("qapi", StringComparison.OrdinalIgnoreCase))
                            {
                                allowedMethod = "QAPI_POST";
                            }
                            else
                            {
                                if (allowedMethod != "PUT")
                                    allowedMethod = "GET_ID";
                            }
                        }
                    }

                    var headerVersionConstraintConstraintName = ((Web.Http.Routes.HeaderVersionConstraint)constraints[headerVersionConstraintName]);
                    if (headerVersionConstraintConstraintName != null)
                    {
                        var headerVersionConstraint = headerVersionConstraintConstraintName.GetType();

                        var pField = headerVersionConstraint
                                          .GetField("_customMediaTypes", BindingFlags.NonPublic | BindingFlags.Instance);
                        headerVersionConstraintValue = (string[])pField.GetValue(((Web.Http.Routes.HeaderVersionConstraint)constraints[headerVersionConstraintName]));
                    }

                    try
                    {
                        // this would apply to PUT/POST where we would want to use the content-type to define the xMediaType
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
                        var e = ex.Message;
                    }

                    var tempXMediaType = string.Empty;
                    if (headerVersionConstraintValue != null && headerVersionConstraintValue.Any())
                    {
                        tempXMediaType = headerVersionConstraintValue[0];
                        if (tempXMediaType.Contains("/"))
                            tempXMediaType = tempXMediaType.Split('/')[1];
                    }
                    var versionOnly = ExtractVersionNumberOnly(tempXMediaType);

                    object controller = string.Empty;
                    object action = string.Empty;
                    object requestedContentType = string.Empty;


                    httpRoute.Defaults.TryGetValue("action", out action);
                    httpRoute.Defaults.TryGetValue("controller", out controller);
                    httpRoute.Defaults.TryGetValue("RequestedContentType", out requestedContentType);

                    var controlleraction = controlleractionlist
                        .FirstOrDefault(x => x.Controller == string.Concat(controller.ToString(), "Controller"));
                    var controllerType = controlleraction.DeclaringType;

                    var controllerAction = action.ToString();

                    if (controllerType != null)
                    {
                        //apiConfiguration.Description = controllerType.GetDocumentation();
                        // Get information about the schema from the controller attribute SchemasAttribute
                        // and update the configuration with this data.
                        var controllerCustomAttributes = controllerType.GetCustomAttributes();
                        //var controllerSchemaAttributes = controllerCustomAttributes.Where(mi => mi.GetType().Name == "MetadataAttribute");
                        var schemaMetaData = controllerType.GetDocumentation();
                        if (!string.IsNullOrEmpty(schemaMetaData.ApiDescription)) apiConfiguration.Description = schemaMetaData.ApiDescription;
                        if (!string.IsNullOrEmpty(schemaMetaData.ApiDomain)) apiConfiguration.ApiDomain = schemaMetaData.ApiDomain;

                        // Derive domain from module code if it hasn't been defined by the SchemaAttribute
                        if (string.IsNullOrEmpty(apiConfiguration.ApiDomain))
                        {
                            var controllerLicenseModule = controllerCustomAttributes.Where(mi => mi.GetType().Name == "EllucianLicenseModuleAttribute");
                            if (controllerLicenseModule != null && controllerLicenseModule.Any())
                            {
                                foreach (EllucianLicenseModuleAttribute schemaData in controllerLicenseModule)
                                {
                                    if (string.IsNullOrEmpty(apiConfiguration.ApiDomain))
                                    {
                                        if (!string.IsNullOrEmpty(schemaData.ModuleCode))
                                        {
                                            apiConfiguration.ApiDomain = ConvertModuleToDomain(schemaData.ModuleCode);
                                        }
                                    }
                                }
                            }
                        }

                        MethodInfo[] methods = controllerType.GetMethods().Where(m => m.Name == controllerAction).ToArray();
                        if (methods != null && methods.Count() > 0)
                        {
                            if (methods.Count() > 1)
                            {
                                // the above LINQ should result in just one, if not, throwing an exception here to make future troubleshooting easier
                                throw new ColleagueWebApiException("ApiDocumentationProvider.GetActionMethodInfo found more than one matching method.");
                            }
                        }

                        // Now get information about the schema from the specific controller method attribute SchemasAttribute
                        // and update the configuration data and version data.
                        string versionDescription = string.Empty;
                        string versionReleaseStatus = string.Empty;
                        string permission = string.Empty, description = string.Empty, summary = string.Empty;
                        var schemaMetadata = methods.FirstOrDefault().GetDocumentation();
                        if (schemaMetadata != null)
                        {
                            if (!string.IsNullOrEmpty(schemaMetadata.ApiDescription)) versionDescription = schemaMetadata.ApiDescription;
                            if (!string.IsNullOrEmpty(schemaMetadata.ApiVersionStatus)) versionReleaseStatus = schemaMetadata.ApiVersionStatus;
                            if (!string.IsNullOrEmpty(schemaMetadata.HttpMethodPermission)) permission = schemaMetadata.HttpMethodPermission;
                            if (!string.IsNullOrEmpty(schemaMetadata.HttpMethodSummary)) summary = schemaMetadata.HttpMethodSummary;
                            if (!string.IsNullOrEmpty(schemaMetadata.HttpMethodDescription)) description = schemaMetadata.HttpMethodDescription;
                        }

                        var existingMethod = apiConfiguration.HttpMethods.FirstOrDefault(http => http.Method.Equals(allowedMethod, StringComparison.OrdinalIgnoreCase));
                        if (existingMethod == null)
                        {
                            apiConfiguration.HttpMethods.Add(new EthosApiSupportedMethods(allowedMethod, permission, description, summary));
                        }

                        if (allowedMethod.Equals("GET_ALL"))
                        {
                            var methodCustomAttributes = methods.FirstOrDefault().GetCustomAttributes();
                            if (methodCustomAttributes != null && methodCustomAttributes.Any(mi => mi.GetType().Name == "PagingFilter"))
                            {
                                if (!apiConfiguration.PageLimit.HasValue)
                                    apiConfiguration.PageLimit = ((PagingFilter)methodCustomAttributes.First(mi => mi.GetType().Name == "PagingFilter")).DefaultLimit;
                            }
                        }

                        // Keep a dictionary of media types supported bye the different routes selected.
                        if (!string.IsNullOrEmpty(tempXMediaType))
                        {
                            if (mediaTypeMethodsSupported.ContainsKey(tempXMediaType))
                            {
                                mediaTypeMethodsSupported[tempXMediaType].Add(allowedMethod);
                            }
                            else
                            {
                                mediaTypeMethodsSupported.Add(tempXMediaType, new List<string>() { allowedMethod });
                            }
                        }

                        // If we've already processed this media type we can skip building the EthosExtensibleData values.
                        var existingData = extensibleDataList.Where(edl => edl.ExtendedSchemaType == tempXMediaType);
                        if (existingData != null && existingData.Any())
                        {
                            continue;
                        }

                        Domain.Base.Entities.EthosExtensibleData apiVersionConfiguration = new Domain.Base.Entities.EthosExtensibleData(selectedResource, versionOnly, tempXMediaType, "", "");
                        apiVersionConfiguration.Description = versionDescription;
                        apiVersionConfiguration.VersionReleaseStatus = versionReleaseStatus;
                        apiVersionConfiguration.InquiryFields = new List<string>();

                        var apiVersionConfig = GetVersionPropertiesForApi(methods.FirstOrDefault(), apiVersionConfiguration);

                        if (apiVersionConfig != null && apiVersionConfig.ExtendedDataList != null && apiVersionConfig.ExtendedDataList.Any())
                        {
                            // Only get Extended data if we have either id or code in the schema
                            var idColumn = apiVersionConfig.ExtendedDataList.FirstOrDefault(ed => ed.JsonTitle.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                                ed.JsonTitle.Equals("code", StringComparison.OrdinalIgnoreCase));
                            if (idColumn != null)
                            {
                                // Get description, max length, table name, reference file, and reference field from run-time CDD if missing
                                apiVersionConfig.ExtendedDataList = (await _ethosApiBuilderService.GetExtendedEthosDataRowDefault(apiVersionConfig.ExtendedDataList.ToList())).ToList();

                                // Merge any extension data into the apiVersionConfigs
                                EthosResourceRouteInfo routeInfo = new EthosResourceRouteInfo()
                                {
                                    ResourceName = selectedResource
                                };
                                var mergeApiVersionConfigs = await _ethosApiBuilderService.GetExtendedEthosVersionsConfigurationsByResource(routeInfo, false, false);
                                if (mergeApiVersionConfigs != null)
                                {
                                    foreach (var mergeConfig in mergeApiVersionConfigs)
                                    {
                                        if (string.IsNullOrEmpty(mergeConfig.ApiVersionNumber) || mergeConfig.ApiVersionNumber == versionOnly || mergeConfig.ApiVersionNumber == versionOnly.Split('.')[0])
                                        {
                                            foreach (var dataRow in mergeConfig.ExtendedDataList)
                                            {
                                                apiVersionConfig.AddItemToExtendedData(dataRow);
                                            }
                                            foreach (var inquiryColumn in mergeConfig.InquiryFields)
                                            {
                                                apiVersionConfig.InquiryFields.Add(inquiryColumn);
                                            }
                                        }
                                    }
                                }
                            }

                            // Update inquiry only fields for invalid database usage
                            foreach (var dataRow in apiVersionConfig.ExtendedDataList)
                            {
                                if (dataRow.DatabaseUsageType == "I" || string.IsNullOrEmpty(dataRow.DatabaseUsageType))
                                {
                                    if (!string.IsNullOrEmpty(dataRow.ColleagueColumnName))
                                    {
                                        apiVersionConfig.InquiryFields.Add(dataRow.ColleagueColumnName);
                                    }
                                }

                                // Add Filter List for QAPI and GET all
                                var filterRow = new Domain.Base.Entities.EthosExtensibleDataFilter(dataRow.ColleagueColumnName, dataRow.ColleagueFileName, dataRow.JsonTitle,
                                    dataRow.JsonPath, dataRow.JsonPropertyType, new List<string>(), dataRow.ColleaguePropertyLength);

                                filterRow.DatabaseUsageType = dataRow.DatabaseUsageType;
                                filterRow.Required = dataRow.Required;
                                filterRow.SelectFileName = dataRow.ColleagueFileName;
                                filterRow.TransColumnName = dataRow.TransColumnName;
                                filterRow.TransFileName = dataRow.TransFileName;
                                filterRow.TransTableName = dataRow.TransTableName;
                                filterRow.Enumerations = dataRow.Enumerations;

                                apiVersionConfig.AddItemToExtendedDataFilter(filterRow);
                            }

                            extensibleDataList.Add(apiVersionConfig);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var e = ex.Message;
                    //no need to throw since not all routes will have _customMediaTypes field
                }
            }

            // Update supported methods based on media type from versions configuration
            foreach (var versionConfig in extensibleDataList)
            {
                var dictionaryKey = !string.IsNullOrEmpty(versionConfig.ExtendedSchemaType) ? versionConfig.ExtendedSchemaType : "application/json";
                if (mediaTypeMethodsSupported.ContainsKey(dictionaryKey))
                {
                    versionConfig.HttpMethodsSupported = mediaTypeMethodsSupported[dictionaryKey];
                }
            }
            return new Tuple<EthosApiConfiguration, List<Domain.Base.Entities.EthosExtensibleData>>(apiConfiguration, extensibleDataList);
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
        /// Convert the module code designation to an API Domain code.
        /// </summary>
        /// <param name="moduleCode"></param>
        /// <returns></returns>
        private string ConvertModuleToDomain(string moduleCode)
        {
            string domain = "CORE";
            switch (moduleCode)
            {
                case ModuleConstants.Base:
                    {
                        domain = "CORE";
                        break;
                    }
                case ModuleConstants.Student:
                    {
                        domain = "ST";
                        break;
                    }
                case ModuleConstants.Planning:
                    {
                        domain = "ST";
                        break;
                    }
                case ModuleConstants.Finance:
                    {
                        domain = "CF";
                        break;
                    }
                case ModuleConstants.FinancialAid:
                    {
                        domain = "FA";
                        break;
                    }
                case ModuleConstants.ColleagueFinance:
                    {
                        domain = "CF";
                        break;
                    }
                case ModuleConstants.ResidenceLife:
                    {
                        domain = "CORE";
                        break;
                    }
                case ModuleConstants.HumanResources:
                    {
                        domain = "HR";
                        break;
                    }
                case ModuleConstants.ProjectsAccounting:
                    {
                        domain = "CF";
                        break;
                    }
                case ModuleConstants.CampusOrgs:
                    {
                        domain = "CORE";
                        break;
                    }
                case ModuleConstants.TimeManagement:
                    {
                        domain = "HR";
                        break;
                    }
                case ModuleConstants.FALink:
                    {
                        domain = "FA";
                        break;
                    }
                case ModuleConstants.BudgetManagement:
                    {
                        domain = "CF";
                        break;
                    }
            }
            return domain;
        }

        /// <summary>
        /// Get the Json Schema properties
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="apiVersionConfig"></param>
        /// <returns></returns>
        private Domain.Base.Entities.EthosExtensibleData GetVersionPropertiesForApi(MethodInfo mi, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
        {
            var dtoTypeAsArrayInput = false;
            var dtoTypeNameInput = string.Empty;
            apiVersionConfig.ExtendedDataList = new List<Domain.Base.Entities.EthosExtensibleDataRow>();

            // get type name and if array...
            Type type = mi.ReturnType;

            if (type != null)
            {
                if (type.IsArray)
                {
                    type = type.GetElementType();
                    dtoTypeAsArrayInput = true;
                }
                else if (type.IsGenericType && !type.IsValueType)
                {
                    type = type.GetGenericArguments().SingleOrDefault();
                    dtoTypeAsArrayInput = true;
                }
            }
            if (type.IsGenericType && !type.IsValueType)
            {
                type = type.GetGenericArguments().SingleOrDefault();
            }
            bool useCamelCase = false;
            var customAttributes = mi.GetCustomAttributes();
            var ethosEnabledFilter = customAttributes.Where(ca => ca.GetType().Name == "EthosEnabledFilter");
            if (ethosEnabledFilter != null)
            {
                foreach (var attr in ethosEnabledFilter)
                {
                    var filterType = (EthosEnabledFilter)attr;
                    useCamelCase = filterType._useCamelCase;
                }
            }

            if (type.Name == "IHttpActionResult")
            {
                var queryStringFilter = customAttributes.Where(ca => ca.GetType().Name == "QueryStringFilterFilter");
                if (queryStringFilter != null)
                {

                    foreach (var attr in queryStringFilter)
                    {
                        var filterType = (QueryStringFilterFilter)attr;
                        var filterGroup = filterType.FilterGroupName;
                        if (filterGroup == "criteria")
                        {
                            type = filterType.FilterType;
                        }
                    }
                }
            }
            if (type != null)
            {
                dtoTypeNameInput = type.AssemblyQualifiedName;
                if (!string.IsNullOrEmpty(dtoTypeNameInput))
                {
                    try
                    {
                        apiVersionConfig = GetVersionPropertiesFromDto(dtoTypeNameInput, apiVersionConfig, useCamelCase);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }

            return apiVersionConfig;
        }

        /// <summary>
        /// Get the Schema from the DTO
        /// </summary>
        /// <param name="dtoTypeNameInput"></param>
        /// <param name="apiVersionConfig"></param>
        /// <param name="useCamelCase"></param>
        /// <param name="rootJsonPath"></param>
        /// <returns></returns>
        private Domain.Base.Entities.EthosExtensibleData GetVersionPropertiesFromDto(string dtoTypeNameInput, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, bool useCamelCase, string rootJsonPath = "/")
        {
            Type dtoType = Type.GetType(dtoTypeNameInput);

            var properties = dtoType.GetProperties();
            if (properties != null)
            {
                if (properties != null)
                {
                    foreach (var orgProperty in properties)
                    {
                        apiVersionConfig = GetExtensibleDataRow(dtoType, apiVersionConfig, orgProperty, useCamelCase, rootJsonPath);
                    }
                }
            }

            return apiVersionConfig;
        }

        /// <summary>
        /// Build an EthosExtensibleDataRow for the Open API specs to work with.
        /// </summary>
        /// <param name="dtoType"></param>
        /// <param name="apiVersionConfig"></param>
        /// <param name="orgProperty"></param>
        /// <param name="useCamelCase"></param>
        /// <param name="rootJsonPath"></param>
        /// <returns></returns>
        private Domain.Base.Entities.EthosExtensibleData GetExtensibleDataRow(Type dtoType, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, PropertyInfo orgProperty, bool useCamelCase, string rootJsonPath = "/")
        {
            PropertyInfo pi = null;
            FieldInfo fi = null;
            MemberInfo[] mi = null;
            if (orgProperty != null && !string.IsNullOrEmpty(orgProperty.Name))
            {
                pi = orgProperty;
                fi = dtoType.GetField(orgProperty.Name);
                mi = dtoType.GetMember(orgProperty.Name);
            }

            string jsonPropertyName = orgProperty.Name;
            string jsonPath = rootJsonPath;
            string jsonPropertyType = "string";
            string[] enumNames = null;
            if (pi != null)
            {
                var displayName = GetDisplayName(pi);
                if (!string.IsNullOrEmpty(displayName))
                    jsonPropertyName = displayName;
            }
            else
            {
                if (mi != null && mi.Any())
                {
                    var customAttributes = mi.FirstOrDefault().CustomAttributes;
                    if (customAttributes != null & customAttributes.Any())
                    {
                        foreach (var custAttr in customAttributes)
                        {
                            if (custAttr != null && custAttr.NamedArguments != null)
                            {
                                foreach (var arg in custAttr.NamedArguments)
                                {
                                    if (arg.MemberName == "Name")
                                    {
                                        jsonPropertyName = arg.TypedValue.Value.ToString();
                                    }
                                }
                            }
                            if (custAttr != null && custAttr.ConstructorArguments != null)
                            {
                                foreach (var arg in custAttr.ConstructorArguments)
                                {
                                    if (arg.ArgumentType.Name == "String")
                                    {
                                        jsonPropertyName = arg.Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Type fieldType = null;

            if (pi != null)
            {
                fieldType = pi.PropertyType;
            }
            else if (fi != null)
            {
                fieldType = fi.FieldType;
            }
            else if (mi != null && mi.Any())
            {
                var memberTypes = mi.FirstOrDefault().MemberType;

                fieldType = memberTypes.GetType();
            }
            if (fieldType != null)
            {

                if (fieldType.IsGenericType
                    && fieldType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    Type underlyingType = Nullable.GetUnderlyingType(fieldType);
                    if (underlyingType == typeof(DateTime))
                    {
                        jsonPropertyType = GetJsonPropertyTypeForDateTime(pi);
                    }
                    else if (underlyingType.BaseType == typeof(Enum))
                    {
                        enumNames = GetEnumNames(underlyingType);
                    }
                    else if (underlyingType == typeof(bool))
                    {
                        jsonPropertyType = "bool";
                    }
                    else if (underlyingType == typeof(int))
                    {
                        jsonPropertyType = "integer";
                    }
                    else if (underlyingType == typeof(float))
                    {
                        jsonPropertyType = "float";
                    }
                    else if (underlyingType == typeof(decimal))
                    {
                        jsonPropertyType = "decimal";
                    }
                }
                else if (fieldType == typeof(DateTime))
                {
                    jsonPropertyType = GetJsonPropertyTypeForDateTime(pi);
                }
                else if (fieldType == typeof(DateTimeOffset))
                {
                    jsonPropertyType = "datetime";
                }
                else if (fieldType.IsEnum)
                {
                    enumNames = GetEnumNames(fieldType);
                }
                else if (fieldType == typeof(bool))
                {
                    jsonPropertyType = "bool";
                }
                else if (fieldType == typeof(int))
                {
                    jsonPropertyType = "integer";
                }
                else if (fieldType == typeof(float))
                {
                    jsonPropertyType = "float";
                }
                else if (fieldType == typeof(decimal))
                {
                    jsonPropertyType = "decimal";
                }
                else if (fieldType.Name == "IEnumerable`1" || fieldType.Name == "IList`1" || fieldType.Name == "ICollection`1" || fieldType.Name == "List`1")
                {
                    Type underlyingType = fieldType.GetGenericArguments().FirstOrDefault();
                    if (underlyingType.IsEnum)
                    {
                        enumNames = Enum.GetNames(underlyingType);
                    }

                    if (underlyingType.IsClass && underlyingType != typeof(string) && underlyingType != typeof(DateTime) && underlyingType != typeof(DateTimeOffset))
                    {
                        if (useCamelCase)
                        {
                            var firstCharacter = jsonPropertyName.Substring(0, 1).ToLower();
                            var otherCharacters = jsonPropertyName.Substring(1);
                            jsonPropertyName = firstCharacter + otherCharacters;
                        }
                        jsonPath = string.Concat(jsonPath, jsonPropertyName, "[]/");
                        var dtoTypeNameInput = underlyingType.AssemblyQualifiedName;
                        return GetVersionPropertiesFromDto(dtoTypeNameInput, apiVersionConfig, useCamelCase, jsonPath);
                    }
                    else
                        jsonPropertyName = string.Concat(jsonPropertyName, "[]");
                }
                else if (fieldType.IsEnum)
                {
                    enumNames = Enum.GetNames(fieldType);
                }
                else if (fieldType.IsClass && fieldType != typeof(string) && fieldType != typeof(DateTime) && fieldType != typeof(DateTimeOffset))
                {
                    if (useCamelCase)
                    {
                        var firstCharacter = jsonPropertyName.Substring(0, 1).ToLower();
                        var otherCharacters = jsonPropertyName.Substring(1);
                        jsonPropertyName = firstCharacter + otherCharacters;
                    }
                    jsonPath = string.Concat(jsonPath, jsonPropertyName, "/");
                    var dtoTypeNameInput = fieldType.AssemblyQualifiedName;
                    return GetVersionPropertiesFromDto(dtoTypeNameInput, apiVersionConfig, useCamelCase, jsonPath);
                }
            }
            if (!string.IsNullOrEmpty(jsonPropertyName))
            {
                if (useCamelCase)
                {
                    var firstCharacter = jsonPropertyName.Substring(0, 1).ToLower();
                    var otherCharacters = jsonPropertyName.Substring(1);
                    jsonPropertyName = firstCharacter + otherCharacters;
                }

                // Get additional Data from MetadataAttributes for column documentation
                string columnName = string.Empty;
                string fileName = string.Empty;
                int? length = null;
                string dataDescription = string.Empty;
                bool dataRequired = false;
                bool dataIsInquiryOnly = false;
                string referenceFileName = string.Empty;
                string referenceTableName = string.Empty;
                string referenceColumnName = string.Empty;

                MetadataAttribute schema = null;
                if (pi != null)
                {
                    schema = pi.GetDocumentation();
                }
                else if (mi != null)
                {
                    schema = mi.FirstOrDefault().GetDocumentation();
                }
                if (schema != null)
                {
                    dataDescription = schema.DataDescription;
                    columnName = schema.DataElementName;
                    //if there is no column name like in shared DTO, we cannot put the column name, we can get other information from the metadata tags instead. 
                    if (!string.IsNullOrEmpty(schema.DataFileName)) fileName = schema.DataFileName;
                    // overide the maxlength from the CDD for boolean as the CDD size will be 1 but the schema value will be true or false. 
                    if (jsonPropertyType == "bool")
                        length = 5;
                    else if (schema.DataMaxLength > 0)
                        length = schema.DataMaxLength;
                    if (!string.IsNullOrEmpty(schema.DataReferenceFileName)) referenceFileName = schema.DataReferenceFileName;
                    if (!string.IsNullOrEmpty(schema.DataReferenceColumnName)) referenceColumnName = schema.DataReferenceColumnName;
                    if (!string.IsNullOrEmpty(schema.DataReferenceTableName)) referenceTableName = schema.DataReferenceTableName;
                    if (schema.DataIsInquiryOnly) dataIsInquiryOnly = schema.DataIsInquiryOnly;
                    if (schema.DataRequired) dataRequired = schema.DataRequired;
                }

                var dataRow = new Domain.Base.Entities.EthosExtensibleDataRow(columnName, fileName, jsonPropertyName, jsonPath, jsonPropertyType, "", length)
                {
                    Description = dataDescription,
                    Required = dataRequired,
                    TransFileName = referenceFileName,
                    TransColumnName = referenceColumnName,
                    TransTableName = referenceTableName,
                    TransType = referenceFileName.Contains("VALCODES") ? "T" : (!string.IsNullOrEmpty(referenceFileName) ? "F" : string.Empty)
                };

                if (enumNames != null && enumNames.Any())
                {
                    dataRow.TransType = "E";
                    dataRow.Enumerations = new List<Domain.Base.Entities.EthosApiEnumerations>();
                    foreach (var enumValue in enumNames)
                    {
                        dataRow.Enumerations.Add(new Domain.Base.Entities.EthosApiEnumerations(enumValue.Substring(0), enumValue));
                    }
                }
                if (dataRow != null)
                {
                    if (pi != null)
                    {
                        dataRow.PropertyInfo = pi;
                    }
                    apiVersionConfig.AddItemToExtendedData(dataRow);
                    if (!string.IsNullOrEmpty(columnName) && dataIsInquiryOnly)
                    {
                        apiVersionConfig.InquiryFields.Add(columnName);
                    }
                }
            }
            return apiVersionConfig;
        }

        /// <summary>
        /// Returns OpenApiComponents object using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        private OpenApiComponents BuildOpenApiComponentsProperty(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
        {
            var components = new OpenApiComponents();
            var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiVersionConfig.ApiVersionNumber.Replace(".", "_"), "_{0}_{1}");
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", alternateView, "_", versionNumber.Replace(".", "_"), "_{0}_{1}");
                }
            }
            bool isFilterRequestSchemaAdded = false;
            bool isFilterSupported = false;
            //set up paths by looping through supported methods
            if (apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any())
            {
                bool isGetResponseSchemaAdded = false;
                List<string> httpMethodsSupported = new List<string>();
                if (apiVersionConfig.HttpMethodsSupported != null)
                {
                    httpMethodsSupported = apiVersionConfig.HttpMethodsSupported;
                }

                foreach (var httpMethod in apiConfiguration.HttpMethods)
                {
                    if (httpMethodsSupported != null && httpMethodsSupported.Any())
                    {
                        if (!httpMethodsSupported.Contains(httpMethod.Method))
                        {
                            continue;
                        }
                    }
                    var method = string.Empty;
                    if (httpMethod.Method != null && !string.IsNullOrEmpty(httpMethod.Method))
                    {
                        if (!httpMethod.Method.Contains("QAPI"))
                        {
                            method = httpMethod.Method.Split('_')[0].ToLower();
                            switch (method)
                            {
                                //schema for get
                                case "get":
                                    {
                                        if (httpMethod.Method.ToLower() == "get")
                                        {
                                            if (!isGetResponseSchemaAdded)
                                            {
                                                var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
                                                components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), getResponseSchema);
                                                isGetResponseSchemaAdded = true;
                                            }
                                            isFilterSupported = true;
                                        }
                                        else if (httpMethod.Method.ToLower() == "get_id")
                                        {
                                            if (!isGetResponseSchemaAdded)
                                            {
                                                var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
                                                components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), getResponseSchema);
                                                isGetResponseSchemaAdded = true;
                                            }
                                        }
                                        if (httpMethod.Method.ToLower() == "get_all")
                                        {
                                            if (!isGetResponseSchemaAdded)
                                            {
                                                var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
                                                components.Schemas.Add(string.Format(componentSchemaPrefix, method, "response"), getResponseSchema);
                                                isGetResponseSchemaAdded = true;
                                            }
                                            isFilterSupported = true;
                                        }

                                        break;
                                    }
                                //section for put
                                case "put":
                                    {
                                        var putRequestSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig, "put");
                                        components.Schemas.Add(string.Format(componentSchemaPrefix, "put", "request"), putRequestSchema);


                                        if (!isGetResponseSchemaAdded)
                                        {
                                            var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
                                            components.Schemas.Add(string.Format(componentSchemaPrefix, "get", "response"), getResponseSchema);
                                            isGetResponseSchemaAdded = true;
                                            //show the filter schema only if get or get all is supported
                                            if (httpMethod.Method.ToLower() == "get" || httpMethod.Method.ToLower() == "get_all")
                                                isFilterSupported = true;
                                        }
                                        break;
                                    }
                                //section for put
                                case "post":
                                    {

                                        var putRequestSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig, "post");
                                        components.Schemas.Add(string.Format(componentSchemaPrefix, "post", "request"), putRequestSchema);

                                        if (!isGetResponseSchemaAdded)
                                        {
                                            var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
                                            components.Schemas.Add(string.Format(componentSchemaPrefix, "get", "response"), getResponseSchema);
                                            isGetResponseSchemaAdded = true;
                                            //show the filter schema only if get or get all is supported
                                            if (httpMethod.Method.ToLower() == "get" || httpMethod.Method.ToLower() == "get_all")
                                                isFilterSupported = true;
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (!isFilterRequestSchemaAdded)
                            {
                                var getResponseSchema = GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
                                components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "request"), getResponseSchema);
                                isFilterRequestSchemaAdded = true;
                                isFilterSupported = true;
                            }
                            if (!isGetResponseSchemaAdded)
                            {
                                var getResponseSchema = GetOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig);
                                components.Schemas.Add(string.Format(componentSchemaPrefix, "get", "response"), getResponseSchema);
                                isGetResponseSchemaAdded = true;
                                //show the filter schema only if get or get all is supported
                                if (httpMethod.Method.ToLower() == "get" || httpMethod.Method.ToLower() == "get_all")
                                    isFilterSupported = true;
                            }

                        }
                    }
                }
            }
            //add component for id

            if (IsCompositeKey(apiConfiguration) && IsBpa(apiConfiguration) && !SupportGetAllOnly(apiConfiguration) && !SupportPostOnly(apiConfiguration))
            {
                components.Schemas.Add(string.Format(componentSchemaPrefix, "id", "parameter"), GetIdOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig));
            }

            // component for filer schema to be used by criteria and qapi_post
            if (apiVersionConfig != null && apiVersionConfig.ExtendedDataFilterList != null && apiVersionConfig.ExtendedDataFilterList.Any() && isFilterSupported)
            {
                if (!isFilterRequestSchemaAdded)
                {
                    components.Schemas.Add(string.Format(componentSchemaPrefix, "query", "request"), GetFilterOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig));
                    isFilterRequestSchemaAdded = true;
                }
            }
            // component for name query schema to be used by criteria and qapi_post
            if (apiVersionConfig != null && apiVersionConfig.ExtendedDataFilterList != null)
            {
                var nameQueries = apiVersionConfig.ExtendedDataFilterList.Where(query => query.NamedQuery);
                if (nameQueries != null && nameQueries.Any())
                {
                    components.Schemas.Add(string.Format(componentSchemaPrefix, "namedQuery", "parameter"), GetNameQueryOpenApiSchemaFromExtensibleDataAsync(apiConfiguration, apiVersionConfig));

                }
            }


            //add v2 error component
            // add error schema 
            var errorsSchema = new OpenApiSchema();
            errorsSchema.Required.Add("errors");
            errorsSchema.Type = "object";
            var errorsSchemaProperty = new OpenApiSchema() { Type = "array" };
            var errorsSchemaPropertyItems = new OpenApiSchema() { Type = "object" };
            errorsSchemaPropertyItems.Properties.Add("id", new OpenApiSchema() { Type = "string", Description = "The global identifier of the resource in error." });
            errorsSchemaPropertyItems.Properties.Add("sourceId", new OpenApiSchema() { Type = "string", Description = "The source applications data reference identifier for the primary data entity used to create the resource. This is useful for referencing the source item through the applications administrative user interface." });
            errorsSchemaPropertyItems.Properties.Add("code", new OpenApiSchema() { Type = "string", Description = "The error message code used to describe the error details." });
            errorsSchemaPropertyItems.Properties.Add("description", new OpenApiSchema() { Type = "string", Description = "The error description used to describe the error details." });
            errorsSchemaPropertyItems.Properties.Add("message", new OpenApiSchema() { Type = "string", Description = "The detailed actionable error message." });
            errorsSchemaProperty.Items = errorsSchemaPropertyItems;
            errorsSchema.Properties.Add("errors", errorsSchemaProperty);
            components.Schemas.Add("errors_2_0_0", errorsSchema);
            // add security schemes
            components.SecuritySchemes.Add("BearerAuth", new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http, Name = "BearerAuth", In = ParameterLocation.Header, Scheme = "bearer" });
            components.SecuritySchemes.Add("BasicAuth", new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http, Name = "BasicAuth", In = ParameterLocation.Header, Scheme = "basic" });
            return components;
        }

        /// <summary>
        /// Returns OpenApiPaths object using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        private OpenApiPaths BuildOpenApiPathsProperty(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
        {
            var paths = new OpenApiPaths();
            //check if this API has composite Key. if that is the case then we do not display {id} path for those APIs. 
            var isCompositeKey = IsCompositeKey(apiConfiguration);
            //set up paths by looping through supported methods
            if (apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any())
            {
                OpenApiPathItem apiPathItem = null;
                OpenApiPathItem apiByIdPathItem = null;
                OpenApiPathItem qapiPathItem = null;
                bool isGetPathItemAdded = false;
                bool isGetbyIdPathItemAdded = false;
                bool isQapiPathItemAdded = false;
                List<string> httpMethodsSupported = new List<string>();
                if (apiVersionConfig.HttpMethodsSupported != null)
                {
                    httpMethodsSupported = apiVersionConfig.HttpMethodsSupported;
                }
                foreach (var httpMethod in apiConfiguration.HttpMethods)
                {
                    if (httpMethodsSupported != null && httpMethodsSupported.Any())
                    {
                        if (!httpMethodsSupported.Contains(httpMethod.Method))
                        {
                            continue;
                        }
                    }
                    var method = string.Empty;
                    if (httpMethod.Method != null && !string.IsNullOrEmpty(httpMethod.Method))
                    {
                        if (!httpMethod.Method.Contains("QAPI"))
                        {
                            method = httpMethod.Method.Split('_')[0].ToLower();
                            if (apiPathItem == null)
                                apiPathItem = new OpenApiPathItem();
                            if (apiByIdPathItem == null)
                                apiByIdPathItem = new OpenApiPathItem();
                            switch (method)
                            {
                                //section for get
                                case "get":
                                    {
                                        if (httpMethod.Method.ToLower() == "get")
                                        {
                                            if (!isGetPathItemAdded)
                                            {
                                                apiPathItem.AddOperation(OperationType.Get, BuildGetOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                                isGetPathItemAdded = true;
                                            }
                                            if (!isGetbyIdPathItemAdded && !isCompositeKey)
                                            {
                                                apiByIdPathItem.AddOperation(OperationType.Get, BuildGetbyIdOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                                isGetbyIdPathItemAdded = true;
                                            }
                                            if (!isQapiPathItemAdded)
                                            {
                                                qapiPathItem = new OpenApiPathItem();
                                                qapiPathItem.AddOperation(OperationType.Post, BuildQapiPostOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                                isQapiPathItemAdded = true;
                                            }

                                        }
                                        else if (httpMethod.Method.ToLower() == "get_id")
                                        {
                                            if (!isGetbyIdPathItemAdded && !isCompositeKey)
                                            {
                                                apiByIdPathItem.AddOperation(OperationType.Get, BuildGetbyIdOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                                isGetbyIdPathItemAdded = true;
                                            }
                                            else if (!isGetPathItemAdded)
                                            {
                                                apiPathItem.AddOperation(OperationType.Get, BuildGetOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                                isGetPathItemAdded = true;
                                            }
                                        }
                                        else if (httpMethod.Method.ToLower() == "get_all")
                                        {
                                            if (!isGetPathItemAdded)
                                            {
                                                apiPathItem.AddOperation(OperationType.Get, BuildGetOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                                isGetPathItemAdded = true;
                                            }
                                        }
                                        break;
                                    }
                                //section for put
                                case "put":
                                    {
                                        if (!isCompositeKey)
                                            apiByIdPathItem.AddOperation(OperationType.Put, BuildPutOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                        else
                                            apiPathItem.AddOperation(OperationType.Put, BuildPutOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                        break;
                                    }
                                //section for get
                                case "post":
                                    {
                                        apiPathItem.AddOperation(OperationType.Post, BuildPostOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                        break;
                                    }
                                //section for get
                                case "delete":
                                    {
                                        if (!isCompositeKey)
                                            apiByIdPathItem.AddOperation(OperationType.Delete, BuildDeleteOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                        else
                                            apiPathItem.AddOperation(OperationType.Delete, BuildDeleteOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                        break;
                                    }
                            }

                        }
                        else
                        {
                            if (!isQapiPathItemAdded)
                            {
                                qapiPathItem = new OpenApiPathItem();
                                qapiPathItem.AddOperation(OperationType.Post, BuildQapiPostOperationObject(apiConfiguration, apiVersionConfig, method, httpMethod.Description, httpMethod.Permission, httpMethod.Summary));
                                isQapiPathItemAdded = true;
                            }
                        }
                    }
                }
                if (apiPathItem != null && apiPathItem.Operations != null && apiPathItem.Operations.Any())
                {
                    if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
                        paths.Add(string.Concat("/api/", apiConfiguration.ParentResourceName), apiPathItem);
                    else
                        paths.Add(string.Concat("/api/", apiConfiguration.ResourceName), apiPathItem);
                }
                if (apiByIdPathItem != null && !isCompositeKey && apiByIdPathItem.Operations != null && apiByIdPathItem.Operations.Any())
                {
                    if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
                        paths.Add(string.Concat("/api/", apiConfiguration.ParentResourceName, "/{id}"), apiByIdPathItem);
                    else
                        paths.Add(string.Concat("/api/", apiConfiguration.ResourceName, "/{id}"), apiByIdPathItem);
                }
                if (qapiPathItem != null && qapiPathItem.Operations != null && qapiPathItem.Operations.Any())
                {
                    if (!string.IsNullOrEmpty(apiConfiguration.ParentResourceName))
                        paths.Add(string.Concat("/qapi/", apiConfiguration.ParentResourceName), qapiPathItem);
                    else
                        paths.Add(string.Concat("/qapi/", apiConfiguration.ResourceName), qapiPathItem);
                }
            }
            return paths;
        }

        /// <summary>
        /// Returns OpenApiOperation object for Get using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="httpmethodDesc">Description of the httpMethod</param>
        /// <param name="httpMethodPermission">Permission for the httpMethod</param>
        /// <param name="httpMethodSummary">Summary for the httpMethod</param>
        private OpenApiOperation BuildGetOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary)
        {
            var operation = new OpenApiOperation();
            var showAdditionalResponseHeader = false;
            operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = apiConfiguration.ResourceName } };
            //summaart for spec-based 
            if (IsSpecBased(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
                    {
                        operation.Summary = string.Format("Returns resources from {0} in {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
                    }
                    else
                    {
                        operation.Summary = string.Format("Returns resources from {0}.", apiConfiguration.PrimaryEntity);
                    }
                }
            }
            else if (IsBpa(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                else
                    operation.Summary = string.Format("Returns resources from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
            }
            else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                else
                    operation.Summary = string.Format("Returns {0} resources.", apiConfiguration.ResourceName);
            }
            else if (!string.IsNullOrEmpty(httpMethodSummary))
            {
                operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
            }
            if (!string.IsNullOrEmpty(httpmethodDesc))
                operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
            if (!string.IsNullOrEmpty(httpMethodPermission))
            {
                operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
            }
            //add parameters section
            var parameters = new List<OpenApiParameter>();
            var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiVersionConfig.ApiVersionNumber.Replace(".", "_"), "_{0}_{1}");
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", alternateView, "_", versionNumber.Replace(".", "_"), "_{0}_{1}");
                }
            }
            //if Id is required, that means this method only supports get by Id
            if (!SupportGetByIdOnly(apiConfiguration) || (apiVersionConfig.HttpMethodsSupported.Count() == 1 && apiVersionConfig.HttpMethodsSupported.Contains("GET_ID")))
            {
                var limitDesc = "The maximum number of resources requested for this result set.";
                if (apiConfiguration.PageLimit != null)
                {
                    limitDesc = string.Concat(limitDesc, " The maximum valid limit value is ", apiConfiguration.PageLimit + ".");
                }
                parameters.Add(GetPathItemParameters("limit", ParameterLocation.Query, false, limitDesc, "integer", componentSchemaPrefix));
                parameters.Add(GetPathItemParameters("offset", ParameterLocation.Query, false, "The 0 based index for a collection of resources for the page requested.", "integer", componentSchemaPrefix));
                parameters.Add(GetPathItemParameters("criteria", ParameterLocation.Query, false, "The filter criteria as a single URL query parameter. Use this parameter or the individual parameters listed. Must be a JSON representation of the property searching for. This can be any of the properties listed on the URL or QAPI and must be a JSON representation that can be validated against the GET schema. Limit and Offset are the only supported additional parameters on the URL.", "string", componentSchemaPrefix, false, true));
                showAdditionalResponseHeader = true;
            }
            //Display Id for only composite Key
            if (IsCompositeKey(apiConfiguration) && IsBpa(apiConfiguration) && !SupportGetAllOnly(apiConfiguration))
            {
                var idRequired = false;
                if (SupportGetByIdOnly(apiConfiguration))
                    idRequired = true;
                parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, idRequired, "Must be a JSON representation of the properties that make up the id block of a single record. No additional parameters on the URL are allowed.", "string", componentSchemaPrefix, false, true));
            }

            //check to see if there is a name query
            bool hasNameQuery = false;
            if (apiVersionConfig != null && apiVersionConfig.ExtendedDataFilterList != null)
            {
                var nameQueries = apiVersionConfig.ExtendedDataFilterList.Where(query => query.NamedQuery);
                if (nameQueries != null && nameQueries.Any())
                {
                    hasNameQuery = true;
                }
            }
            if (hasNameQuery)
                parameters.Add(GetPathItemParameters("namedQuery", ParameterLocation.Query, false, "A named query is specified as a query parameter and may require arguments which must be expressed using JSON (where the arguments are provided as name-value pairs akin to the ad-hoc query syntax used for filtering by 'equality', as described above).", "object", componentSchemaPrefix, false, true));
            operation.Parameters = parameters;
            //add response section
            var responses = new OpenApiResponses
            {
                { "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200", showAdditionalResponseHeader) },
                { "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
                { "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
                { "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
                { "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
                { "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
                { "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
                { "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
            };
            operation.Responses = responses;
            //add security to the operation            
            operation.Security.Add(BuildOpenApiSecurityRequirement());
            return operation;
        }

        /// <summary>
        /// Returns OpenApiOperation object for Get using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="httpmethodDesc">Description of the httpMethod</param>
        /// <param name="httpMethodPermission">Permission for the httpMethod</param>
        /// <param name="httpMethodSummary">Summary for the httpMethod</param>
        private OpenApiOperation BuildGetbyIdOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary)
        {
            var operation = new OpenApiOperation();
            operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = apiConfiguration.ResourceName } };
            //summary for spec-based 
            if (IsSpecBased(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
                        operation.Summary = string.Format("Returns the requested resource from {0} from {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
                    else
                        operation.Summary = string.Format("Returns the requested resource from {0}.", apiConfiguration.PrimaryEntity);
                }
            }
            else if (IsBpa(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Returns the requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
                }
            }
            else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Returns the requested {0} resource.", apiConfiguration.ResourceName);
                }
            }
            if (!string.IsNullOrEmpty(httpmethodDesc))
                operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
            if (!string.IsNullOrEmpty(httpMethodPermission))
            {
                operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
            }
            //add parameters section
            var parameters = new List<OpenApiParameter>();
            var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiVersionConfig.ApiVersionNumber.Replace(".", "_"), "_{0}_{1}");
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", alternateView, "_", versionNumber.Replace(".", "_"), "_{0}_{1}");
                }
            }
            //check to see if Id is a property or string. If string, check if this is a GUID
            bool isGuid = false;
            if (apiConfiguration != null && !string.IsNullOrEmpty(apiConfiguration.PrimaryGuidSource))
                isGuid = true;
            if (apiConfiguration != null && apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Count > 1 && IsBpa(apiConfiguration))
                parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, true, "Must be a JSON representation of the  properties that make up the id block of a single record. No additional parameters on the URL are allowed. ", "string", componentSchemaPrefix, false, true));
            else
                parameters.Add(GetPathItemParameters("id", ParameterLocation.Path, true, "A global identifier of the resource for use in all external references.", "string", componentSchemaPrefix, isGuid));

            operation.Parameters = parameters;
            //add response section
            var responses = new OpenApiResponses
            {
                { "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200") },
                { "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
                { "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
                { "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
                { "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
                { "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
                { "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
                { "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
            };
            operation.Responses = responses;
            //add security to the operation            
            operation.Security.Add(BuildOpenApiSecurityRequirement());
            return operation;
        }
        /// <summary>
        /// Returns OpenApiOperation object for Put using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="httpmethodDesc">Description of the httpMethod</param>
        /// <param name="httpMethodPermission">Permission for the httpMethod</param>
        ///  <param name="httpMethodSummary">Summary for the httpMethod</param>
        private OpenApiOperation BuildPutOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary)
        {
            var operation = new OpenApiOperation();
            operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = apiConfiguration.ResourceName } };
            //summaart for spec-based 
            if (IsSpecBased(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
                        operation.Summary = string.Format("Updates the requested resource from {0} from {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
                    else
                        operation.Summary = string.Format("Updates the requested resource from from {0}.", apiConfiguration.PrimaryEntity);
                }
            }
            else if (IsBpa(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Updates the requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
                }
            }
            else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Updates the requested {0} resource.", apiConfiguration.ResourceName);
                }
            }
            if (!string.IsNullOrEmpty(httpmethodDesc))
                operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
            if (!string.IsNullOrEmpty(httpMethodPermission))
            {
                operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
            }
            //add parameters section
            var parameters = new List<OpenApiParameter>();
            var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiVersionConfig.ApiVersionNumber.Replace(".", "_"), "_{0}_{1}");
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", alternateView, "_", versionNumber.Replace(".", "_"), "_{0}_{1}");
                }
            }
            //check to see if Id is a property or string. If string, check if this is a GUID
            bool isGuid = false;
            if (apiConfiguration != null && !string.IsNullOrEmpty(apiConfiguration.PrimaryGuidSource))
                isGuid = true;
            if (apiConfiguration != null && apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Count > 1)
                parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, true, "Must be a JSON representation of the  properties that make up the id block of a single record. No additional parameters on the URL are allowed. ", "string", componentSchemaPrefix, isGuid, true));
            else
                parameters.Add(GetPathItemParameters("id", ParameterLocation.Path, true, "A global identifier of the resource for use in all external references.", "string", componentSchemaPrefix, isGuid));

            operation.Parameters = parameters;
            //add request section
            operation.RequestBody = BuildPathItemPutPostRequestBody(apiVersionConfig, "put", componentSchemaPrefix);
            //add response section
            var responses = new OpenApiResponses
            {
                { "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200") },
                { "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
                { "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
                { "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
                { "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
                { "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
                { "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
                { "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
            };
            operation.Responses = responses;
            //add security to the operation            
            operation.Security.Add(BuildOpenApiSecurityRequirement());
            return operation;
        }

        /// <summary>
        /// Returns true if the api is spec-based
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool IsSpecBased(EthosApiConfiguration apiConfiguration)
        {
            bool isSpecBased = false;
            if (apiConfiguration != null && apiConfiguration.ApiType == "A")
                isSpecBased = true;
            return isSpecBased;
        }

        /// <summary>
        /// Returns true if the api is Business Process Based
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool IsBpa(EthosApiConfiguration apiConfiguration)
        {
            bool isBPA = false;
            if (apiConfiguration != null && apiConfiguration.ApiType == "T")
                isBPA = true;
            return isBPA;
        }

        /// <summary>
        /// Returns true if the api is EedmSupported or EthosEnabled
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool IsEthos(EthosApiConfiguration apiConfiguration)
        {
            bool IsEthos = false;
            if (apiConfiguration != null && apiConfiguration.ApiType == "Ethos")
                IsEthos = true;
            return IsEthos;
        }

        /// <summary>
        /// Returns true if the api is web enabled
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool IsWeb(EthosApiConfiguration apiConfiguration)
        {
            bool IsWeb = false;
            if (apiConfiguration != null && apiConfiguration.ApiType == "web")
                IsWeb = true;
            return IsWeb;
        }

        /// <summary>
        /// Returns true if the api only supports get by Id
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool SupportGetByIdOnly(EthosApiConfiguration apiConfiguration)
        {
            bool supportGetByIdOnly = false;
            //if we have only one method and it is get by Id, then we are good. 
            if (apiConfiguration != null && apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any())
            {
                if (apiConfiguration.HttpMethods.Count == 1)
                {
                    var methodSupported = apiConfiguration.HttpMethods.FirstOrDefault();
                    if (methodSupported != null && methodSupported.Method.ToLower() == "get_id")
                        supportGetByIdOnly = true;
                }
                //we could have get_id along with put, post and delete. 
                else
                {
                    supportGetByIdOnly = true;
                    var versionSupported = apiConfiguration.HttpMethods.Where(z => z.Method.Equals("GET_ALL") || z.Method.Equals("GET") || z.Method.Equals("POST_QAPI")).ToList();
                    if (versionSupported != null && versionSupported.Any())
                        supportGetByIdOnly = false;
                }
            }
            return supportGetByIdOnly;
        }

        /// <summary>
        /// Returns true if the api only supports get all
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool SupportGetAllOnly(EthosApiConfiguration apiConfiguration)
        {
            bool supportGetAllOnly = false;
            if (apiConfiguration != null && apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any() && apiConfiguration.HttpMethods.Count == 1)
            {
                var methodSupported = apiConfiguration.HttpMethods.FirstOrDefault();
                if (methodSupported != null && methodSupported.Method.ToLower() == "get_all")
                    supportGetAllOnly = true;
            }
            return supportGetAllOnly;
        }

        /// <summary>
        /// Returns true if the api only supports qapi post or post
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool SupportPostOnly(EthosApiConfiguration apiConfiguration)
        {
            bool supportPostOnly = false;
            if (apiConfiguration != null && apiConfiguration.HttpMethods != null && apiConfiguration.HttpMethods.Any() && apiConfiguration.HttpMethods.Count == 1)
            {
                var methodSupported = apiConfiguration.HttpMethods.FirstOrDefault();
                if (methodSupported != null && (methodSupported.Method.ToLower() == "post_qapi" || methodSupported.Method.ToLower() == "post"))
                    supportPostOnly = true;
            }
            return supportPostOnly;
        }

        /// <summary>
        /// Returns true if the api uses a composite Key
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        private bool IsCompositeKey(EthosApiConfiguration apiConfiguration)
        {
            bool isCompositeKey = false;
            if (apiConfiguration != null && apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Count > 1 && IsBpa(apiConfiguration))
                isCompositeKey = true;
            return isCompositeKey;
        }

        /// <summary>
        /// Returns OpenApiOperation object for QAPI Post using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="httpmethodDesc">Description of the httpMethod</param>
        /// <param name="httpMethodPermission">Permission for the httpMethod</param>
        /// <param name="httpMethodSummary">Summary for the httpMethod</param>
        private OpenApiOperation BuildQapiPostOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary)
        {
            var operation = new OpenApiOperation();
            operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = apiConfiguration.ResourceName } };
            //summaart for spec-based 
            if (IsSpecBased(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    if (!string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
                        operation.Summary = string.Format("Returns requested resource from {0} from {1}.", apiConfiguration.PrimaryTableName, string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity));
                    else
                        operation.Summary = string.Format("Returns the requested resource from {0}.", apiConfiguration.PrimaryEntity);
                }
            }
            else if (IsBpa(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Returns the requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
                }
            }
            else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Returns the requested {0} resources.", apiConfiguration.ResourceName);
                }
            }
            if (!string.IsNullOrEmpty(httpmethodDesc))
                operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
            if (!string.IsNullOrEmpty(httpMethodPermission))
            {
                operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
            }
            //add parameters section
            var parameters = new List<OpenApiParameter>();
            var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiVersionConfig.ApiVersionNumber.Replace(".", "_"), "_{0}_{1}");
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", alternateView, "_", versionNumber.Replace(".", "_"), "_{0}_{1}");
                }
            }
            //if Id is required, that means this method only supports get by Id
            if (!SupportGetByIdOnly(apiConfiguration))
            {
                var limitDesc = "The maximum number of resources requested for this result set.";
                if (apiConfiguration.PageLimit != null)
                {
                    limitDesc = string.Concat(limitDesc, " The maximum valid limit value is ", apiConfiguration.PageLimit);
                }
                parameters.Add(GetPathItemParameters("limit", ParameterLocation.Query, false, limitDesc, "integer", componentSchemaPrefix));
                parameters.Add(GetPathItemParameters("offset", ParameterLocation.Query, false, "The 0 based index for a collection of resources for the page requested.", "integer", componentSchemaPrefix));
            }
            operation.Parameters = parameters;
            //add request section
            operation.RequestBody = BuildPathItemPutPostRequestBody(apiVersionConfig, "query", componentSchemaPrefix);
            //add response section
            var responses = new OpenApiResponses
            {
                { "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200", true) },
                { "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
                { "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
                { "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
                { "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
                { "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
                { "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
                { "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
            };
            operation.Responses = responses;
            //add security to the operation            
            operation.Security.Add(BuildOpenApiSecurityRequirement());
            return operation;
        }


        /// <summary>
        /// Returns OpenApiOperation object for Post using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="httpmethodDesc">Description of the httpMethod</param>
        /// <param name="httpMethodPermission">Permission for the httpMethod</param>
        /// <param name="httpMethodSummary">Summary for the httpMethod</param>
        private OpenApiOperation BuildPostOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary)
        {
            var operation = new OpenApiOperation();
            operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = apiConfiguration.ResourceName } };
            //summaart for spec-based 
            if (IsSpecBased(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Creates a new resource in {0}.", apiConfiguration.PrimaryEntity);
                }
            }
            else if (IsBpa(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Creates a new resource in {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
                }
            }
            else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                {
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                }
                else
                {
                    operation.Summary = string.Format("Creates a new {0} resource.", apiConfiguration.ResourceName);
                }
            }
            if (!string.IsNullOrEmpty(httpmethodDesc))
                operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
            if (!string.IsNullOrEmpty(httpMethodPermission))
            {
                operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
            }
            var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiVersionConfig.ApiVersionNumber.Replace(".", "_"), "_{0}_{1}");
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", alternateView, "_", versionNumber.Replace(".", "_"), "_{0}_{1}");
                }
            }
            //add parameters section
            var parameters = new List<OpenApiParameter>();
            operation.Parameters = parameters;
            //add request section
            operation.RequestBody = BuildPathItemPutPostRequestBody(apiVersionConfig, "post", componentSchemaPrefix);
            //add response section
            var responses = new OpenApiResponses
            {
                { "200", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "200") },
                { "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
                { "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
                { "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
                { "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
                { "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
                { "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
                { "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
            };
            operation.Responses = responses;
            //add security to the operation            
            operation.Security.Add(BuildOpenApiSecurityRequirement());
            return operation;
        }

        /// <summary>
        /// Returns OpenApiOperation object for Delete using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="httpmethodDesc">Description of the httpMethod</param>
        /// <param name="httpMethodPermission">Permission for the httpMethod</param>
        /// <param name="httpMethodSummary">Summary for the httpMethod</param>
        private OpenApiOperation BuildDeleteOperationObject(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string httpmethodDesc, string httpMethodPermission, string httpMethodSummary)
        {
            var operation = new OpenApiOperation();
            operation.Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = apiConfiguration.ResourceName } };
            //summaart for spec-based 
            if (IsSpecBased(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                else
                    operation.Summary = string.Format("Deletes the requested resource from {0}.", apiConfiguration.PrimaryEntity);
            }
            else if (IsBpa(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                else
                    operation.Summary = string.Format("Deletes the requested resource from {0} - {1}.", apiConfiguration.ProcessId, apiConfiguration.ProcessDesc);
            }
            else if (IsBpa(apiConfiguration))
            {
                if (!string.IsNullOrEmpty(httpMethodSummary))
                    operation.Summary = httpMethodSummary.Replace(DmiString._SM, ' ');
                else
                    operation.Summary = string.Format("Deletes the requested {0} resource.", apiConfiguration.ResourceName);
            }
            if (!string.IsNullOrEmpty(httpmethodDesc))
                operation.Description = Regex.Unescape(httpmethodDesc.Replace(DmiString._SM, ' '));
            if (!string.IsNullOrEmpty(httpMethodPermission))
            {
                operation.AddExtension("x-method-permission", new OpenApiString(httpMethodPermission));
            }
            //add parameters section
            var parameters = new List<OpenApiParameter>();
            var componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", apiVersionConfig.ApiVersionNumber.Replace(".", "_"), "_{0}_{1}");
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    componentSchemaPrefix = string.Concat(apiConfiguration.ResourceName, "_", alternateView, "_", versionNumber.Replace(".", "_"), "_{0}_{1}");
                }
            }
            //check to see if Id is a property or string. If string, check if this is a GUID
            bool isGuid = false;
            if (apiConfiguration != null && !string.IsNullOrEmpty(apiConfiguration.PrimaryGuidSource))
                isGuid = true;
            if (apiConfiguration != null && apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Count > 1)
                parameters.Add(GetPathItemParameters("id", ParameterLocation.Query, true, "Must be a JSON representation of the properties that make up the id block of a single record. No additional parameters on the URL are allowed.", "string", componentSchemaPrefix, isGuid, true));
            else
                parameters.Add(GetPathItemParameters("id", ParameterLocation.Path, true, "A global identifier of the resource for use in all external references.", "string", componentSchemaPrefix, isGuid));

            operation.Parameters = parameters;
            //add response section
            var responses = new OpenApiResponses
            {
                { "204", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "204") },
                { "401", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "401") },
                { "403", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "403") },
                { "404", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "404") },
                { "405", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "405") },
                { "406", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "406") },
                { "400", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "400") },
                { "500", BuildPathItemResponse(apiConfiguration, apiVersionConfig, httpMethod, componentSchemaPrefix, "500") }
            };
            operation.Responses = responses;
            //add security to the operation            
            operation.Security.Add(BuildOpenApiSecurityRequirement());
            return operation;
        }

        /// <summary>
        /// Returns OpenApiRequestBody object using the API configuration info
        /// </summary>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="schemaPrefix">prefix for the content schema</param>

        private OpenApiRequestBody BuildPathItemPutPostRequestBody(Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string schemaPrefix)
        {
            var requestBody = new OpenApiRequestBody();
            var mediaTypeContent = new OpenApiMediaType();
            mediaTypeContent.Schema = new OpenApiSchema() { Type = "array", Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = string.Format(schemaPrefix, httpMethod, "request"), Type = ReferenceType.Schema } } };
            if (apiVersionConfig.ExtendedSchemaType.Contains("/"))
                requestBody.Content.Add(string.Format(apiVersionConfig.ExtendedSchemaType), mediaTypeContent);
            else
                requestBody.Content.Add(string.Format("application/{0}", apiVersionConfig.ExtendedSchemaType), mediaTypeContent);
            return requestBody;
        }

        /// <summary>
        /// Returns static OpenApiSecurityRequirement object
        /// </summary>
        private OpenApiSecurityRequirement BuildOpenApiSecurityRequirement()
        {
            var securityRequirement = new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http , Reference = new OpenApiReference() { Id = "BearerAuth", Type = ReferenceType.SecurityScheme }}, new List<string>() { } },
                { new OpenApiSecurityScheme() { Type = SecuritySchemeType.Http,  Reference = new OpenApiReference() { Id = "BasicAuth", Type = ReferenceType.SecurityScheme }}, new List<string>() {  } }
            };
            return securityRequirement;
        }

        /// <summary>
        /// Returns OpenApiResponse object using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        /// <param name="httpMethod">Method supported by the operation</param>
        /// <param name="schema_prefix">prefix for the content schema</param>
        /// <param name="returnCode">http response return code</param>
        /// <param name="showAddResponseHeader">http response return code</param>
        private OpenApiResponse BuildPathItemResponse(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig, string httpMethod, string schema_prefix, string returnCode, bool showAddResponseHeader = false)
        {
            var response = new OpenApiResponse();
            var errMediaTypeContent = new OpenApiMediaType();
            errMediaTypeContent.Schema = new OpenApiSchema() { Type = "array", Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = "errors_2_0_0", Type = ReferenceType.Schema } } };

            switch (returnCode)
            {
                case "200":
                    {
                        response.Description = "OK";
                        //x-media-type header
                        var mediaTypeHeader = new OpenApiHeader();
                        mediaTypeHeader.Description = "The full semantic version with the media type of the response.";
                        mediaTypeHeader.Schema = new OpenApiSchema() { Type = "string" };
                        mediaTypeHeader.Required = true;
                        response.Headers.Add("X-Media-Type", mediaTypeHeader);
                        //X-Content-Restricted
                        var restrictedContentHeader = new OpenApiHeader();
                        restrictedContentHeader.Description = "If the response is not a full representation of the resource, a partial representation is returned. Otherwise, this header is not included.";
                        restrictedContentHeader.Schema = new OpenApiSchema() { Type = "string" };
                        response.Headers.Add("X-Content-Restricted", restrictedContentHeader);
                        if (showAddResponseHeader)
                        {
                            //x-total-count header
                            var totalCountHeader = new OpenApiHeader();
                            totalCountHeader.Description = "Specifies the total number of resources that satisfy the query.";
                            totalCountHeader.Schema = new OpenApiSchema() { Type = "integer" };
                            response.Headers.Add("X-Total-Count", totalCountHeader);

                            if (apiConfiguration.PageLimit != null && apiConfiguration.PageLimit > 0)
                            {
                                // X-Max-Page-Size
                                var maxPageSizeHeader = new OpenApiHeader();
                                maxPageSizeHeader.Description = "Specifies the maximum number of resources returned in a response.";
                                maxPageSizeHeader.Schema = new OpenApiSchema() { Type = "integer" };
                                response.Headers.Add("X-Max-Page-Size", maxPageSizeHeader);
                            }
                        }
                        var mediaTypeContent = new OpenApiMediaType();
                        mediaTypeContent.Schema = new OpenApiSchema() { Type = "array", Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = string.Format(schema_prefix, "get", "response"), Type = ReferenceType.Schema } } };
                        response.Content.Add(string.Format("application/{0}", apiVersionConfig.ExtendedSchemaType), mediaTypeContent);
                        break;
                    }
                case "401":
                    {
                        response.Description = "Failure. Unauthorized.";
                        break;
                    }
                case "403":
                    {
                        response.Description = "Failure. Forbidden. The user does not have the required permissions for the resource.";
                        response.Content.Add("application/vnd.hedtech.integration.errors.v2+json", errMediaTypeContent);
                        break;
                    }
                case "404":
                    {
                        response.Description = "Failure. Not Found. Cannot map the URI to a resource.";
                        break;
                    }
                case "405":
                    {
                        response.Description = "Failure. Method Not Allowed. The client tried to use an HTTP method that the resource does not allow.";
                        response.Content.Add("application/vnd.hedtech.integration.errors.v2+json", errMediaTypeContent);
                        break;
                    }
                case "406":
                    {
                        response.Description = "Failure. Not Acceptable. Unable to generate any of the client’s preferred media types, as indicated by the Accept request header.";
                        response.Content.Add("application/vnd.hedtech.integration.errors.v2+json", errMediaTypeContent);
                        break;
                    }
                case "400":
                    {
                        response.Description = "Failure. Bad Request. The server cannot process the request due to something that is perceived to be a client error.";
                        response.Content.Add("application/vnd.hedtech.integration.errors.v2+json", errMediaTypeContent);
                        break;
                    }
                case "500":
                    {
                        response.Description = "Server error. Unexpected configuration or data.";
                        response.Content.Add("application/vnd.hedtech.integration.errors.v2+json", errMediaTypeContent);
                        break;
                    }
                case "204":
                    {
                        response.Description = "OK, No Content.";
                        break;
                    }

            }


            return response;

        }

        /// <summary>
        /// Returning OpenApiServers Info
        /// </summary>
        private IList<OpenApiServer> BuildOpenApiServersProperty()
        {
            var servers = new List<OpenApiServer>
            {
                AddEthosServers("Ethos Integration API U.S.", "https://integrate.elluciancloud.com"),
                AddEthosServers("Ethos Integration API Canada.", "https://integrate.elluciancloud.ca"),
                AddEthosServers("Ethos Integration API Europe.", "https://integrate.elluciancloud.ie"),
                AddEthosServers("Ethos Integration API Asia-Pacific.", "https://integrate.elluciancloud.com.au"),
                AddEthosServers("Custom server URL.", "{server_url}", "localhost")
            };
            return servers;
        }

        /// <summary>
        /// Returning OpenApiInfo object using the API configuration info
        /// </summary>
        /// <param name="apiConfiguration">main API configuration from EDM.EXTENSIONS</param>
        /// <param name="apiVersionConfig">version configuration info from EDM.EXT.VERSIONS</param>
        private OpenApiInfo BuildOpenApiInfoProperty(EthosApiConfiguration apiConfiguration, Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
        {
            var info = new OpenApiInfo();
            info.Title = apiConfiguration.ResourceName;
            if (!apiVersionConfig.ExtendedSchemaType.Contains(mediaFormat + ".v"))
            {
                var alternateViews = apiVersionConfig.ExtendedSchemaType.Split('.');
                if (alternateViews.Count() > 4)
                {
                    var alternateView = alternateViews[3];
                    var versionNumber = alternateViews[4];
                    info.Title = string.Concat(apiConfiguration.ResourceName, "_", alternateView);
                }
            }
            info.Description = apiConfiguration.Description;
            info.Version = apiVersionConfig.ApiVersionNumber;
            info.AddExtension("x-source-system", new OpenApiString(sourceSystem));
            if (IsBpa(apiConfiguration))
            {
                info.AddExtension("x-source-name", new OpenApiString(apiConfiguration.ProcessId));
                info.AddExtension("x-source-title", new OpenApiString(apiConfiguration.ProcessDesc));
                info.AddExtension("x-api-type", new OpenApiString("Administrative"));
            }
            else if (IsSpecBased(apiConfiguration))
            {
                if (string.IsNullOrEmpty(apiConfiguration.PrimaryTableName))
                    info.AddExtension("x-source-name", new OpenApiString(apiConfiguration.PrimaryEntity));
                else
                    info.AddExtension("x-source-name", new OpenApiString(string.Concat(apiConfiguration.PrimaryApplication, "-", apiConfiguration.PrimaryEntity, " ", apiConfiguration.PrimaryTableName)));
                var resourceName = apiConfiguration.ResourceName;
                if (!string.IsNullOrEmpty(resourceName))
                {
                    resourceName = resourceName.Replace('-', ' ');
                    resourceName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resourceName);
                    info.AddExtension("x-source-title", new OpenApiString(resourceName));
                }
                info.AddExtension("x-api-type", new OpenApiString("Specification-Based"));
            }
            else if (IsEthos(apiConfiguration) || IsWeb(apiConfiguration))
            {
                var resourceName = apiConfiguration.ResourceName;
                if (!string.IsNullOrEmpty(resourceName))
                {
                    resourceName = resourceName.Replace('-', ' ');
                    resourceName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resourceName);
                    info.AddExtension("x-source-title", new OpenApiString(resourceName));
                }
                info.AddExtension("x-api-type", new OpenApiString("web"));
            }

            //display the staus from version if it is there, otherwise use it from the API status.
            string apiStatus = string.Empty;
            if (!string.IsNullOrEmpty(apiVersionConfig.VersionReleaseStatus))
            {
                apiStatus = apiVersionConfig.VersionReleaseStatus;
            }
            else
            {
                apiStatus = apiConfiguration.ReleaseStatus;
            }
            if (!string.IsNullOrEmpty(apiStatus))
            {
                if (apiStatus == "B" || apiStatus.ToLower() == "beta")
                {
                    info.AddExtension("x-release-status", new OpenApiString("beta"));
                    info.Version = string.Concat(apiVersionConfig.ApiVersionNumber, "-beta");
                }
                else if (apiStatus == "R" || apiStatus.ToLower().Contains("releas"))
                    info.AddExtension("x-release-status", new OpenApiString("ga"));
                else
                    info.AddExtension("x-release-status", new OpenApiString("select"));
            }
            if (apiConfiguration.ApiDomain == "ADV")
                info.AddExtension("x-source-domain", new OpenApiString("Advancement"));
            else if (apiConfiguration.ApiDomain == "FA")
                info.AddExtension("x-source-domain", new OpenApiString("Financial Aid"));
            else if (apiConfiguration.ApiDomain == "CF")
                info.AddExtension("x-source-domain", new OpenApiString("Finance"));
            else if (apiConfiguration.ApiDomain == "CORE")
                info.AddExtension("x-source-domain", new OpenApiString("Foundation"));
            else if (apiConfiguration.ApiDomain == "HR")
                info.AddExtension("x-source-domain", new OpenApiString("Human Resources"));
            else if (apiConfiguration.ApiDomain == "REC")
                info.AddExtension("x-source-domain", new OpenApiString("Recruitment"));
            else if (apiConfiguration.ApiDomain == "ST")
                info.AddExtension("x-source-domain", new OpenApiString("Student"));


            return info;
        }

        private OpenApiParameter GetPathItemParameters(string name, ParameterLocation input, bool required, string description, string schemaType, string componentSchemaPrefix, bool isGuid = false, bool hasRef = false)
        {
            var parameter = new OpenApiParameter();
            parameter.Name = name;
            parameter.Description = description;
            parameter.In = input;
            parameter.Required = required;
            if (!string.IsNullOrEmpty(schemaType) && !hasRef)
            {
                if (!isGuid)
                    parameter.Schema = new OpenApiSchema() { Type = schemaType };
                else
                    parameter.Schema = new OpenApiSchema() { Type = schemaType, Format = "guid", Pattern = GUID_PATTERN };
            }
            else if (!string.IsNullOrEmpty(schemaType) && hasRef)
            {
                if (name != "criteria")
                {
                    var schema = new OpenApiSchema() { Type = schemaType, Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = string.Format(componentSchemaPrefix, name, "parameter"), Type = ReferenceType.Schema } } };
                    parameter.Schema = schema;
                }
                else
                {
                    var schema = new OpenApiSchema() { Type = schemaType, Items = new OpenApiSchema() { Reference = new OpenApiReference() { Id = string.Format(componentSchemaPrefix, "query", "request"), Type = ReferenceType.Schema } } };
                    parameter.Schema = schema;
                }

            }
            return parameter;
        }

        private OpenApiServer AddEthosServers(string description, string url, string variable = "")
        {
            var server = new OpenApiServer();
            server.Description = description;
            server.Url = url;
            if (!string.IsNullOrEmpty(variable))
            {
                var serverVar = new OpenApiServerVariable();
                serverVar.Default = variable;
                var serverVarDict = new Dictionary<string, OpenApiServerVariable>
                {
                    { "server_url", serverVar }
                };
                server.Variables = serverVarDict;
            }
            return server;
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
        /// <param name="prop">Type of object to retrieve MetaData for.</param>
        /// <returns>SchemasAttribute</returns>
        private MetadataAttribute[] GetMetadaAttributes(PropertyInfo prop)
        {
            if (prop != null)
            {
                var dataMemberAttributes = (MetadataAttribute[])prop.GetCustomAttributes(typeof(MetadataAttribute), false);
                if (dataMemberAttributes != null && dataMemberAttributes.Any())
                {
                    return dataMemberAttributes;
                }
            }

            return null;
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

        private string ConvertJsonPropertyType(string jsonPropertyType, string jsonTitle = "", string conversion = "")

        {
            string OpenApiSchemaType = "string";
            if ((!string.IsNullOrEmpty(jsonTitle)) && (jsonTitle.EndsWith("[]")))
            {
                return "array";

            }

            if (string.IsNullOrEmpty(jsonPropertyType))
                return OpenApiSchemaType;

            switch (jsonPropertyType.ToLower())
            {
                case "string":
                    OpenApiSchemaType = "string";
                    break;
                case "number":
                    if (!string.IsNullOrEmpty(conversion))
                    {
                        if (conversion.Equals("MD0", StringComparison.OrdinalIgnoreCase))
                        {
                            OpenApiSchemaType = "integer";
                        }
                        else
                        {
                            OpenApiSchemaType = "number";
                        }
                    }
                    else
                    {
                        OpenApiSchemaType = "integer";
                    }


                    break;
                case "bool":
                    OpenApiSchemaType = "boolean";
                    break;
                case "decimal":
                    OpenApiSchemaType = "number";
                    break;
                case "integer":
                    OpenApiSchemaType = "integer";
                    break;
                case "float":
                    OpenApiSchemaType = "number";
                    break;
                case "date":
                    OpenApiSchemaType = "string";
                    break;
                case "time":
                    OpenApiSchemaType = "string";
                    break;
                case "datetime":
                    OpenApiSchemaType = "string";
                    break;
                default:
                    OpenApiSchemaType = "string";
                    break;
            }

            return OpenApiSchemaType;
        }


        private string GetJsonPropertyPattern(string jsonPropertyType)

        {

            if (string.IsNullOrEmpty(jsonPropertyType))
                return string.Empty;

            string retVal = string.Empty;

            switch (jsonPropertyType.ToLower())
            {


                case "string":
                    retVal = string.Empty;
                    break;
                case "number":
                    retVal = string.Empty;
                    break;
                case "date":
                    retVal = "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])$";
                    break;
                case "time":
                    retVal = string.Empty;
                    break;
                case "datetime":
                    retVal = "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])T(2[0-3]|[0-1][0-9]):([0-5][0-9]):([0-5][0-9])(\\.[0-9]+)?(Z|[+-](?:2[0-3]|[0-1][0-9]):[0-5][0-9])?$";
                    break;
                default:
                    retVal = string.Empty;
                    break;
            }

            return retVal;
        }

        /// <summary>
        /// GetIdOpenApiSchemaFromExtensibleData
        /// </summary>
        /// <param name="apiVersionConfig">Domain.Base.Entities.EthosExtensibleData</param>
        /// <param name="apiConfiguration">EthosApiConfiguration</param>
        /// <returns>OpenApiSchema</returns>

        private OpenApiSchema GetIdOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration apiConfiguration,
            Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
        {
            var IdSchema = new OpenApiSchema();
            IdSchema.Type = "object";
            var extendedDataListSorted = apiVersionConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath).ToList();
            if (apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Any())
            {
                foreach (var key in apiConfiguration.ColleagueKeyNames)
                {
                    //find the extendedData for each of the id field
                    var extendedData = extendedDataListSorted.FirstOrDefault(prop => prop.ColleagueColumnName == key);
                    if (extendedData != null)
                    {
                        var response = BuildOpenApiSchemaResponse(extendedData);
                        IdSchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
                        IdSchema.Required.Add(extendedData.JsonTitle);

                    }
                }
            }
            return IdSchema;
        }



        /// <summary>
        /// GetIdOpenApiSchemaFromExtensibleData
        /// </summary>
        /// <param name="apiVersionConfig">Domain.Base.Entities.EthosExtensibleData</param>
        /// <param name="apiConfiguration">EthosApiConfiguration</param>
        /// <returns>OpenApiSchema</returns>

        private OpenApiSchema GetFilterOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration apiConfiguration,
            Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
        {
            var filterSchema = new OpenApiSchema();
            filterSchema.Type = "object";
            var extendedDataListSorted = apiVersionConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath).ToList();
            if (apiVersionConfig.ExtendedDataFilterList != null && apiVersionConfig.ExtendedDataFilterList.Any())
            {
                foreach (var filter in apiVersionConfig.ExtendedDataFilterList)
                {
                    //find the extendedData for each of the id field
                    try
                    {
                        var extendedData = extendedDataListSorted.FirstOrDefault(prop => prop.JsonPath == filter.JsonPath && prop.JsonTitle == filter.JsonTitle);
                        if (extendedData != null)
                        {
                            if (IsWeb(apiConfiguration))
                            {
                                if (!IsFilter(extendedData.PropertyInfo, "criteria"))
                                    continue;
                            }
                            try
                            {
                                var propSplit =
                                extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                                if (!propSplit.Any()) continue;

                                var count = propSplit.Count();

                                if (count == 1)
                                {
                                    var response = BuildOpenApiSchemaResponse(extendedData);

                                    filterSchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);


                                }
                                else
                                {
                                    var parentSchema = filterSchema;
                                    for (int i = 0; i < count; i++)
                                    {
                                        OpenApiSchema childSchema = null;

                                        if (childSchema == null && i < count - 1)
                                        {
                                            if (!parentSchema.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
                                            {

                                                if (propSplit[i].Contains("[]"))
                                                {
                                                    childSchema = new OpenApiSchema { Type = "array" };
                                                }
                                                else
                                                {
                                                    childSchema = new OpenApiSchema { Type = "object" };
                                                }
                                                childSchema.Properties = new Dictionary<string, OpenApiSchema>();
                                                if (parentSchema.Type != "array")
                                                    parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                                else
                                                {
                                                    if (parentSchema.Items == null)
                                                    {
                                                        var childSchemaItems = new OpenApiSchema() { Type = "object" };
                                                        childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                                        parentSchema.Items = childSchemaItems;
                                                    }
                                                    else
                                                    {
                                                        var childSchemaItems = parentSchema.Items;
                                                        if (!childSchemaItems.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
                                                        {
                                                            if (propSplit[i].Contains("[]"))
                                                            {
                                                                childSchema = new OpenApiSchema { Type = "array" };
                                                            }
                                                            else
                                                            {
                                                                childSchema = new OpenApiSchema { Type = "object" };
                                                            }
                                                            childSchema.Properties = new Dictionary<string, OpenApiSchema>();
                                                            childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                                        }
                                                    }
                                                }
                                            }
                                            parentSchema = childSchema;
                                        }
                                        else if (parentSchema != null && i == count - 1)
                                        {
                                            if (parentSchema.Type == "array")
                                            {
                                                //childSchema = new OpenApiSchema { Type = "array" };
                                                if (parentSchema.Items == null)
                                                {
                                                    var childSchemaItems = new OpenApiSchema() { Type = "object" };
                                                    childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
                                                    parentSchema.Items = childSchemaItems;
                                                }
                                                else
                                                {
                                                    var childSchemaItems = parentSchema.Items;
                                                    childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
                                                    parentSchema.Items = childSchemaItems;
                                                }
                                                //parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                            }
                                            else
                                            {
                                                parentSchema.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
                                            }
                                        }
                                    }

                                }


                            }
                            catch (Exception e)
                            {
                                if (_logger != null)
                                {
                                    _logger.Error(e, "Failed to corretly generate schema.");
                                }
                            }
                            //var response = BuildOpenApiSchemaResponse(extendedData);
                            //filterSchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
                        }
                    }
                    catch (Exception)
                    {
                        _logger.Error(filter.JsonTitle);
                    }
                }
            }
            return filterSchema;
        }

        /// <summary>
        /// Build Schema for NameQuery component
        /// </summary>
        /// <param name="apiVersionConfig">Domain.Base.Entities.EthosExtensibleData</param>
        /// <param name="apiConfiguration">EthosApiConfiguration</param>
        /// <returns>OpenApiSchema</returns>

        private OpenApiSchema GetNameQueryOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration apiConfiguration,
            Domain.Base.Entities.EthosExtensibleData apiVersionConfig)
        {
            var nameQuerySchema = new OpenApiSchema();
            nameQuerySchema.Type = "object";
            var nameQueryFilters = apiVersionConfig.ExtendedDataFilterList.Where(query => query.NamedQuery);
            var extendedDataListSorted = apiVersionConfig.ExtendedDataList.OrderBy(ex => ex.FullJsonPath).ToList();
            if (nameQueryFilters != null && nameQueryFilters.Any())
            {
                foreach (var query in nameQueryFilters)
                {
                    var extendedData = new Domain.Base.Entities.EthosExtensibleDataRow(query.ColleagueColumnName, query.ColleagueFileName, query.JsonTitle, query.JsonPath, query.JsonPropertyType, "", query.ColleaguePropertyLength);
                    if (extendedData != null)
                    {
                        extendedData.Description = query.Description;
                        var response = BuildOpenApiSchemaResponse(extendedData, false, false);
                        nameQuerySchema.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
                    }
                }
            }
            return nameQuerySchema;
        }
        /// <summary>
        /// GetOpenApiSchemaFromExtensibleData
        /// </summary>
        /// <param name="extendConfig">Domain.Base.Entities.EthosExtensibleData</param>
        /// <param name="ethosApiConfiguration">EthosApiConfiguration</param>
        /// <param name="method">which method is calling this </param>
        /// <returns>OpenApiSchema</returns>

        private OpenApiSchema GetOpenApiSchemaFromExtensibleDataAsync(EthosApiConfiguration ethosApiConfiguration,
            Domain.Base.Entities.EthosExtensibleData extendConfig, string method = "")
        {
            OpenApiSchema schemaRootNode = null;
            var extendedDataListSorted = new List<Ellucian.Colleague.Domain.Base.Entities.EthosExtensibleDataRow>();
            extendedDataListSorted.AddRange(extendConfig.ExtendedDataList);
            SortedSet<string> requiredProperties = new SortedSet<string>();
            schemaRootNode = new OpenApiSchema()
            {

                Type = "object",
                //Properties = new Dictionary<string, OpenApiSchema>(),
                //AdditionalPropertiesAllowed = false,                   
                //Title = extendConfig.ApiResourceName + "_" + extendConfig.ApiVersionNumber

            };

            //set up the Id property
            if (!string.IsNullOrEmpty(ethosApiConfiguration.PrimaryGuidSource))
            {
                schemaRootNode.Properties.Add("id",
                                new OpenApiSchema
                                {
                                    Type = "string",
                                    Title = "ID",
                                    Format = "guid",
                                    Description = "The global identifier for the resource.",
                                    Pattern = GUID_PATTERN

                                });
                //if this is not a schema for put/post then Id is going to be there in the response and hence it is required.
                if (method != "put")
                    requiredProperties.Add("id");
            }
            else
            {
                //do this for Business process API
                if (IsBpa(ethosApiConfiguration))
                {
                    // we just have a string id property
                    //we have Id object. 
                    if (ethosApiConfiguration != null && ethosApiConfiguration.ColleagueKeyNames != null && ethosApiConfiguration.ColleagueKeyNames.Count > 1)
                    {
                        var IdProperty = new OpenApiSchema()
                        {
                            Type = "object",
                            Title = "ID",
                            Description = "The identifiers for the resource",

                        };
                        foreach (var key in ethosApiConfiguration.ColleagueKeyNames)
                        {
                            //find the extendedData for each of the id field
                            var extendedData = extendedDataListSorted.FirstOrDefault(prop => prop.ColleagueColumnName == key);
                            if (extendedData != null)
                            {
                                var response = BuildOpenApiSchemaResponse(extendedData);
                                IdProperty.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
                                //remove from the list
                                extendedDataListSorted.Remove(extendedData);
                            }
                        }
                        schemaRootNode.Properties.Add("id", IdProperty);

                    }
                    else
                    {
                        //if there is only one key then we are going to display that as an Id. 
                        if (ethosApiConfiguration != null && ethosApiConfiguration.ColleagueKeyNames != null)
                        {
                            var key = ethosApiConfiguration.ColleagueKeyNames.FirstOrDefault();
                            int? length = null;
                            //get additional information about the id property from the list if it is also in the list.
                            var idDataInfo = extendedDataListSorted.FirstOrDefault(prop => prop.ColleagueColumnName == key);
                            if (idDataInfo != null)
                            {
                                length = idDataInfo.ColleaguePropertyLength;
                            }
                            var extendedData = new Domain.Base.Entities.EthosExtensibleDataRow(key, ethosApiConfiguration.ColleagueFileNames.FirstOrDefault(), "id", "/", "string", "", length);
                            extendedData.Description = "The identifier for the resource";
                            if (idDataInfo != null)
                            {
                                extendedData.Conversion = idDataInfo.Conversion;
                                extendedData.TransColumnName = idDataInfo.TransColumnName;
                                extendedData.TransFileName = idDataInfo.TransFileName;
                                extendedData.TransTableName = idDataInfo.TransTableName;
                            }
                            var response = BuildOpenApiSchemaResponse(extendedData);
                            schemaRootNode.Properties.Add("id", response);
                        }
                    }
                    //if this is not a schema for put/post then Id is going to be there in the response and hence it is required.
                    if (method != "put" && method != "post")
                        requiredProperties.Add("id");
                }
                else if (!string.IsNullOrEmpty(ethosApiConfiguration.PrimaryEntity))
                {
                    schemaRootNode.Properties.Add("id",
                                    new OpenApiSchema
                                    {
                                        Type = "string",
                                        Title = "ID",
                                        Description = "The derived identifier for the resource"

                                    });
                    //if this is not a schema for put/post then Id is going to be there in the response and hence it is required.
                    if (method != "put" && method != "post")
                        requiredProperties.Add("id");
                }
            }
            //for put/post we need to remove those fields for the extendedDataList that are inquiry 
            foreach (var extendedData in extendedDataListSorted)
            {
                try
                {
                    bool skipRecord = false;
                    //skip inquiry fields in PUT/POST
                    if (method == "put" || method == "post")
                    {
                        if (extendConfig.InquiryFields != null && extendConfig.InquiryFields.Any() && extendConfig.InquiryFields.Contains(extendedData.ColleagueColumnName))
                        {
                            skipRecord = true;
                        }
                    }
                    //skip preparedResponse in Get Response
                    if (method != "put" && method != "post" && IspredefinedInputs(extendedData))
                    {
                        skipRecord = true;
                    }
                    if (!skipRecord)
                    {
                        var propSplit =
                    extendedData.FullJsonPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                        if (!propSplit.Any()) continue;

                        var count = propSplit.Count();
                        //this is the main level
                        if (count == 1)
                        {
                            var response = BuildOpenApiSchemaResponse(extendedData);
                            schemaRootNode.Properties.Add(extendedData.JsonTitle.Replace("[]", ""), response);
                            if (extendedData.Required)
                            {
                                //due to partial PUT, nothing is required so skip the required block for PUT for web types for now. 
                                // for web type api, for POST, id is generated and hence not required.
                                if (IsWeb(ethosApiConfiguration) && method == "put") { }
                                else if (IsWeb(ethosApiConfiguration) && method == "post" && extendedData.JsonTitle == "id") { }
                                else requiredProperties.Add(extendedData.JsonTitle);
                            }
                        }
                        // this is embedded levels
                        else
                        {
                            var parentSchema = schemaRootNode;
                            SortedSet<string> requiredArrayProperties = new SortedSet<string>();
                            for (int i = 0; i < count; i++)
                            {
                                OpenApiSchema childSchema = null;

                                if (childSchema == null && i < count - 1)
                                {
                                    if (!parentSchema.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
                                    {

                                        if (propSplit[i].Contains("[]"))
                                        {
                                            childSchema = new OpenApiSchema { Type = "array" };
                                        }
                                        else
                                        {
                                            childSchema = new OpenApiSchema { Type = "object" };
                                        }
                                        childSchema.Properties = new Dictionary<string, OpenApiSchema>();
                                        if (parentSchema.Type != "array")
                                            parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                        else
                                        {
                                            if (parentSchema.Items == null)
                                            {
                                                var childSchemaItems = new OpenApiSchema() { Type = "object" };
                                                childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                                if (extendedData.Required)
                                                {
                                                    //due to partial PUT, nothing is required so skip the required block for PUT for web types for now. 
                                                    //this is not an array so we are good here. 
                                                    if (IsWeb(ethosApiConfiguration) && method == "put") { }
                                                    else
                                                    {
                                                        if (childSchemaItems.Required == null && !childSchemaItems.Required.Any())
                                                            childSchemaItems.Required = new SortedSet<string>() { propSplit[i] };
                                                        else
                                                            childSchemaItems.Required.Add(propSplit[i]);
                                                    }
                                                }
                                                parentSchema.Items = childSchemaItems;
                                            }
                                            else
                                            {
                                                var childSchemaItems = parentSchema.Items;
                                                if (!childSchemaItems.Properties.TryGetValue(propSplit[i].Replace("[]", ""), out childSchema))
                                                {
                                                    if (propSplit[i].Contains("[]"))
                                                    {
                                                        childSchema = new OpenApiSchema { Type = "array" };
                                                    }
                                                    else
                                                    {
                                                        childSchema = new OpenApiSchema { Type = "object" };
                                                    }
                                                    childSchema.Properties = new Dictionary<string, OpenApiSchema>();
                                                    childSchemaItems.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                                }
                                                parentSchema.Items = childSchemaItems;
                                            }
                                        }
                                    }
                                    parentSchema = childSchema;
                                }
                                else if (parentSchema != null && i == count - 1)
                                {
                                    if (parentSchema.Type == "array")
                                    {
                                        //childSchema = new OpenApiSchema { Type = "array" };
                                        if (parentSchema.Items == null)
                                        {
                                            var childSchemaItems = new OpenApiSchema() { Type = "object" };
                                            childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
                                            if (extendedData.Required)
                                            {
                                                if (childSchemaItems.Required == null && !childSchemaItems.Required.Any())
                                                    childSchemaItems.Required = new SortedSet<string>() { propSplit[i] };
                                                else
                                                    childSchemaItems.Required.Add(propSplit[i]);
                                            }
                                            parentSchema.Items = childSchemaItems;


                                        }
                                        else
                                        {
                                            var childSchemaItems = parentSchema.Items;
                                            childSchemaItems.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
                                            if (extendedData.Required)
                                            {
                                                if (childSchemaItems.Required == null && !childSchemaItems.Required.Any())
                                                    childSchemaItems.Required = new SortedSet<string>() { propSplit[i] };
                                                else
                                                    childSchemaItems.Required.Add(propSplit[i]);
                                            }
                                            parentSchema.Items = childSchemaItems;

                                        }
                                        //parentSchema.Properties.Add(propSplit[i].Replace("[]", ""), childSchema);
                                    }
                                    else
                                    {
                                        parentSchema.Properties.Add(propSplit[i], BuildOpenApiSchemaResponse(extendedData));
                                        if (extendedData.Required)
                                        {
                                            //due to partial PUT, nothing is required so skip the required block for PUT for web types for now. 
                                            //this is not an array so we are good here. 
                                            if (IsWeb(ethosApiConfiguration) && method == "put") { }
                                            else
                                            {
                                                if (parentSchema.Required == null && !parentSchema.Required.Any())
                                                    parentSchema.Required = new SortedSet<string>() { propSplit[i] };
                                                else
                                                    parentSchema.Required.Add(propSplit[i]);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }


                }
                catch (Exception e)
                {
                    if (_logger != null)
                    {
                        _logger.Error(e, "Failed to corretly generate schema.");
                    }
                }
            }
            //report the required properties for main level properties. 
            if (requiredProperties.Any())
                schemaRootNode.Required = requiredProperties;

            return schemaRootNode;
        }


        private OpenApiSchema BuildOpenApiSchemaResponse(Domain.Base.Entities.EthosExtensibleDataRow extendedData, bool isIdSchema = false, bool displayLineage = true)
        {
            var OpenApiSchema = new OpenApiSchema
            {
                Type = ConvertJsonPropertyType(extendedData.JsonPropertyType, extendedData.JsonTitle, extendedData.Conversion),
                MaxLength = extendedData.ColleaguePropertyLength
            };
            //if this is an array, create items property 
            if (OpenApiSchema.Type == "array")
            {
                var arrayItems = new OpenApiSchema() { Type = "string" };
                OpenApiSchema.Items = arrayItems;
            }
            if (!string.IsNullOrEmpty(extendedData.Description))
            {
                OpenApiSchema.Description = extendedData.Description.Replace(DmiString._VM, ' ').Replace(DmiString._SM, ' ');
            }
            //OpenApiSchema.AdditionalPropertiesAllowed = false;

            if ((extendedData.JsonPropertyType.Equals("date", StringComparison.OrdinalIgnoreCase))
            || (extendedData.JsonPropertyType.Equals("date-time", StringComparison.OrdinalIgnoreCase)))
            {
                OpenApiSchema.MaxLength = null;
            }
            else if (!string.IsNullOrEmpty(extendedData.TransType))
            {
                if (extendedData.TransType.Equals("G", StringComparison.OrdinalIgnoreCase))
                {
                    OpenApiSchema.Pattern = GUID_PATTERN;
                    OpenApiSchema.MaxLength = Guid.Empty.ToString().Length;
                    OpenApiSchema.Format = "guid";
                }
            }
            else if (!string.IsNullOrEmpty(extendedData.Conversion))
            {
                OpenApiSchema.Format = extendedData.Conversion;
            }

            if (string.IsNullOrEmpty(OpenApiSchema.Pattern))
            {
                var pattern = GetJsonPropertyPattern(extendedData.JsonPropertyType);
                if (!string.IsNullOrEmpty(pattern))
                {
                    OpenApiSchema.Pattern = pattern;
                }
            }

            if (!string.IsNullOrEmpty(extendedData.JsonTitle))
            {
                try
                {
                    var jsonTitle = extendedData.JsonTitle.Replace("[]", "");
                    OpenApiSchema.Title = string.Concat(Char.ToUpperInvariant(jsonTitle[0]), jsonTitle.Substring(1));
                }
                catch (Exception)
                {
                    OpenApiSchema.Title = extendedData.JsonTitle;
                }

            }
            if (displayLineage)
            {
                //do not show the lineage for prepared response
                if (!string.IsNullOrEmpty(extendedData.ColleagueColumnName) && !IspredefinedInputs(extendedData))
                {
                    if (!extendedData.ColleagueColumnName.EndsWith(".TRANSLATION"))
                        OpenApiSchema.AddExtension("x-lineageReferenceObject", new OpenApiString(extendedData.ColleagueColumnName));
                    else
                    {
                        if (!string.IsNullOrEmpty(extendedData.TransColumnName))
                            OpenApiSchema.AddExtension("x-lineageReferenceObject", new OpenApiString(extendedData.TransColumnName));
                    }


                }
                //do not show the lineage for prepared response
                if (!string.IsNullOrEmpty(extendedData.TransFileName) && !IspredefinedInputs(extendedData) && !extendedData.ColleagueColumnName.EndsWith(".TRANSLATION"))
                {
                    if (!string.IsNullOrEmpty(extendedData.TransTableName))
                        OpenApiSchema.AddExtension("x-lineageLookupReferenceObject", new OpenApiString(string.Concat(extendedData.TransFileName, " - ", extendedData.TransTableName)));
                    else
                        OpenApiSchema.AddExtension("x-lineageLookupReferenceObject", new OpenApiString(extendedData.TransFileName));
                }
            }
            //for prepared response, we want to show the default value as well as possible values. 
            // 
            if (IspredefinedInputs(extendedData))
            {

                //this has the default value
                OpenApiSchema.Default = new OpenApiString(extendedData.TransType);
                // this has the potential values
                if (!string.IsNullOrEmpty(extendedData.TransFileName))
                {
                    //var responseValues = new List<OpenApiString>();
                    if (extendedData.TransFileName.Contains(";"))
                    {
                        var values = extendedData.TransFileName.Split(';');
                        foreach (var val in values)
                        {
                            OpenApiSchema.Enum.Add(new OpenApiString(val));
                        }
                    }
                    else
                    {
                        OpenApiSchema.Enum.Add(new OpenApiString(extendedData.TransFileName));
                    }
                }
            }
            if (extendedData.TransType == "E")
            {
                if (!string.IsNullOrEmpty(extendedData.TransFileName))
                {
                    //var responseValues = new List<OpenApiString>();
                    if (extendedData.TransFileName.Contains(";"))
                    {
                        var values = extendedData.TransFileName.Split(';');
                        foreach (var val in values)
                        {
                            OpenApiSchema.Enum.Add(new OpenApiString(val));
                        }
                    }
                }
                else
                {
                    foreach (var enumeration in extendedData.Enumerations)
                    {
                        OpenApiSchema.Enum.Add(new OpenApiString(enumeration.EnumerationValue));
                    }
                }
            }
            return OpenApiSchema;
        }

        /// <summary>
        /// Returns true if data field is predefinedInputs
        /// </summary>
        /// <param name="extendedData">EDMV column data</param>
        private bool IspredefinedInputs(Domain.Base.Entities.EthosExtensibleDataRow extendedData)
        {
            bool ispredefinedInputs = false;
            if (extendedData != null && !string.IsNullOrEmpty(extendedData.JsonPath) && extendedData.JsonPath.Contains("predefinedInputs"))
                ispredefinedInputs = true;
            return ispredefinedInputs;
        }

        /// <summary>
        /// Gets all enum member values for given enum type
        /// </summary>
        /// <param name="fieldType">enum type</param>
        /// <returns>enum member values</returns>
        private static string[] GetEnumNames(Type fieldType)
        {
            string[] enumNames;
            var names = Enum.GetNames(fieldType);
            var enumMemberValues = new List<string>();
            foreach (var name in names)
            {
                if (((EnumMemberAttribute[])fieldType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Any())
                {
                    var enumMemberAttribute = ((EnumMemberAttribute[])fieldType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
                    if (enumMemberAttribute != null)
                        enumMemberValues.Add(enumMemberAttribute.Value);
                }
                else
                {
                    enumMemberValues.Add(name);
                }
            }
            enumNames = enumMemberValues.ToArray();
            return enumNames;
        }

        private static string GetJsonConverterTypeName(PropertyInfo pi)
        {
            string converterTypeName = "";
            if (pi != null)
            {
                var jsonConverterAttributes = (JsonConverterAttribute[])pi.GetCustomAttributes(typeof(JsonConverterAttribute), false);
                if (jsonConverterAttributes != null && jsonConverterAttributes.Any())
                {
                    var converterType = jsonConverterAttributes.FirstOrDefault(x => x.ConverterType != null && !string.IsNullOrEmpty(x.ConverterType.Name));
                    converterTypeName = converterType.ConverterType.Name;
                }
            }
            return converterTypeName;
        }

        private static string GetJsonPropertyTypeForDateTime(PropertyInfo pi)
        {
            string jsonConverterTypeName = GetJsonConverterTypeName(pi);
            string jsonPropertyType = !string.IsNullOrEmpty(jsonConverterTypeName) && jsonConverterTypeName == nameof(DateOnlyConverter) ? "date" : "datetime";
            return jsonPropertyType;
        }
    }



    class MetadataVersionNumberComparer : IComparer<string>
    {
        /// <summary>
        /// Compare strings which represent semantic version numbers and/or integers
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>x is greater return 1 else if y is greater return -1</returns>
        public int Compare(string x, string y)
        {
            try
            {
                //remove -beta from the version comparision
                x = x.Replace("-beta", "");
                y = y.Replace("-beta", "");

                if (x == y || x == string.Empty) return 1;

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
            catch
            {
                //display error message for invalid version like x
                throw new KeyNotFoundException("Requested version is not supported.");
            }
        }
    }


}