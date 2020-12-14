// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.EthosExtend;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to EthosApiBuilder data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EthosApiBuilderController : BaseCompressedApiController
    {
        private readonly IEthosApiBuilderService _ethosApiBuilderService;
        private readonly ILogger _logger;
        
        /// <summary>
        /// Initializes a new instance of the EthosApiBuilderController class.
        /// </summary>
        /// <param name="ethosApiBuilderService">Service of type <see cref="IEthosApiBuilderService">IEthosApiBuilderService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EthosApiBuilderController(IEthosApiBuilderService ethosApiBuilderService, ILogger logger)
        {
            _ethosApiBuilderService = ethosApiBuilderService;
            _logger = logger;
        }

        #region GET

        /// <summary>
        /// Respond to route and execute the appropriate method
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="ethosApiBuilder"></param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut, HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PagingFilter(IgnorePaging = true), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<object> GetEthosApiBuilderAsync([FromUri] string resource, [FromUri] string id, Paging page, [FromBody] Dtos.EthosApiBuilder ethosApiBuilder)
        {
            var method = Request.Method.Method;
            if (string.IsNullOrEmpty(id))
            {
                switch (method)
                {
                    case ("GET"):
                        return await GetEthosApiBuilderAsync(resource, page);
                    case ("POST"):
                        return await PostEthosApiBuilderAsync(resource, ethosApiBuilder);
                }
            }
            else
            {
                switch (method)
                {
                   case ("GET"):
                        return await GetEthosApiBuilderByIdAsync(resource, id);
                   case ("PUT"):
                        return await PutEthosApiBuilderAsync(resource, id, ethosApiBuilder);
                    case ("DELETE"):
                        await DeleteEthosApiBuilderAsync(resource, id); break;
                }
            }
            return new Dtos.EthosApiBuilder();
        }

        /// <summary>
        /// Gets all data records for a single API extension
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [PagingFilter(IgnorePaging = true), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetEthosApiBuilderAsync([FromUri] string resource, Paging page)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var routeInfo = GetEthosResourceRouteInfo(resource);
            if (string.IsNullOrEmpty(routeInfo.ResourceVersionNumber))
            {
                routeInfo.ResourceVersionNumber = await _ethosApiBuilderService.GetEthosExtensibilityResourceDefaultVersion(routeInfo.ResourceName, bypassCache);
            }
            var configuration = await GetApiConfiguration("GET", routeInfo, bypassCache);
            var extendedEthosVersion = await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo, bypassCache);
            if (extendedEthosVersion == null)
            {
                NotAcceptableStatusException();
            }

            CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));

            try
            {
                page = ConfigurePage(page, configuration);

                var filterDictionary = await ValidateFilterParameters(routeInfo.ExtendedFilterDefinitions, extendedEthosVersion);

                var pageOfItems = await _ethosApiBuilderService.GetEthosApiBuilderAsync(page.Offset, page.Limit, configuration.ResourceName, filterDictionary, bypassCache);

                AddEthosContextProperties(await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, bypassCache),
                              await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo,
                              pageOfItems.Item1.Select(a => a.Id).ToList()));
                
                return new PagedHttpActionResult<IEnumerable<Dtos.EthosApiBuilder>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Retrieves a single record by ID for extended data.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id">Id of record to retrieve.</param>
        /// <returns>An extended data object.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.EthosApiBuilder> GetEthosApiBuilderByIdAsync([FromUri] string resource, [FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var routeInfo = GetEthosResourceRouteInfo(resource);
            if (string.IsNullOrEmpty(routeInfo.ResourceVersionNumber))
            {
                routeInfo.ResourceVersionNumber = await _ethosApiBuilderService.GetEthosExtensibilityResourceDefaultVersion(routeInfo.ResourceName, bypassCache);
            }
            var configuration = await GetApiConfiguration("GET", routeInfo, bypassCache);
            var extendedEthosVersion = await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo, bypassCache);
            if (extendedEthosVersion == null)
            {
                NotAcceptableStatusException();
            }

            CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(string.Format("Must provide an id for '{0}'.", routeInfo.ResourceName));
                }

                var ethosApiBuilder = await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync(id, configuration.ResourceName);

                if (ethosApiBuilder != null)
                {
                    AddEthosContextProperties(await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, bypassCache),
                              await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo,
                              new List<string>() { ethosApiBuilder.Id }));
                }
                
                return ethosApiBuilder;
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
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        #endregion

        #region POST
        /// <summary>
        /// Creates a EthosApiBuilder.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="ethosApiBuilder"><see cref="Dtos.EthosApiBuilder">ethosApiBuilder</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.EthosApiBuilder">EthosApiBuilder</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.EthosApiBuilder> PostEthosApiBuilderAsync([FromUri] string resource, [ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var routeInfo = GetEthosResourceRouteInfo(resource);
            var configuration = await GetApiConfiguration("POST", routeInfo, bypassCache);
            var extendedEthosVersion = await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo, bypassCache);
            if (extendedEthosVersion == null)
            {
                NotAcceptableStatusException();
            }

            CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));

            //Creation of a new record is not supported for Colleague extended data but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region PUT
        /// <summary>
        /// Updates a person visa.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id">id of the ethosApiBuilder to update</param>
        /// <param name="ethosApiBuilder"><see cref="Dtos.EthosApiBuilder">ethosApiBuilder</see> to create</param>
        /// <returns>Updated <see cref="Dtos.EthosApiBuilder">Dtos.EthosApiBuilder</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.EthosApiBuilder> PutEthosApiBuilderAsync([FromUri] string resource, [FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var routeInfo = GetEthosResourceRouteInfo(resource);
            var configuration = await GetApiConfiguration("PUT", routeInfo, bypassCache);
            var extendedEthosVersion = await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo, bypassCache);
            if (extendedEthosVersion == null)
            {
                NotAcceptableStatusException();
            }

            CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(string.Format("Must provide an id for '{0}'.", routeInfo.ResourceName));
                }

                if (ethosApiBuilder == null)
                {
                    throw new ArgumentNullException(string.Format("Must provide a request body for '{0}'.", routeInfo.ResourceName));
                }

                if (string.IsNullOrEmpty(ethosApiBuilder.Id))
                {
                    throw new ArgumentNullException(string.Format("Must provide an id for '{0}' in request body.", routeInfo.ResourceName));
                }

                if (!id.Equals(ethosApiBuilder.Id, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("id in URL is not the same as in request body.");
                }

                Validate(ethosApiBuilder);

                //get Data Privacy List
                var dpList = await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, true);

                //call import extend method that needs the extracted extension data and the config
                await _ethosApiBuilderService.ImportExtendedEthosData(await ExtractExtendedData(await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var ethosApiBuilderReturn = await _ethosApiBuilderService.PutEthosApiBuilderAsync(id,
                  await PerformPartialPayloadMerge(ethosApiBuilder, async () => await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync(id, configuration.ResourceName),
                  dpList, _logger), configuration.ResourceName);

                AddEthosContextProperties(dpList,
                    await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo, new List<string>() { id }));

                return ethosApiBuilderReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        #endregion

        #region DELETE
        /// <summary>
        /// Delete (DELETE) a record from an extended data set
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id">id of the record to delete</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task DeleteEthosApiBuilderAsync([FromUri] string resource, [FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Validate
        /// <summary>
        /// Check for all required fields
        /// </summary>
        /// <param name="ethosApiBuilder">ethosApiBuilder</param>
        private static void Validate(Dtos.EthosApiBuilder ethosApiBuilder)
        {
            if (ethosApiBuilder != null && string.IsNullOrEmpty(ethosApiBuilder.Id))
            {
                throw new ArgumentNullException("Must provide an id for extended data resource.");
            }
        }

        /// <summary>
        /// Gets Ethos Resource information from route
        /// </summary>
        /// <returns>EthosResourceRouteInfo</returns>
        public EthosResourceRouteInfo GetEthosResourceRouteInfo(string resource)
        {
            var bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var ethosRouteInfo = new EthosResourceRouteInfo();
            ethosRouteInfo.ReportEthosExtendedErrors = true;

            if (Request == null || Request.Headers == null || Request.Headers.Accept == null)
            {
                return ethosRouteInfo;
            }
            
            var contentType = Request.Headers.Accept.ToString();

            if (contentType == null)
            {
                return ethosRouteInfo;
            }

            if (contentType.Contains("+"))
            {
                contentType = contentType.Split('+')[0].ToString();
            }
            ethosRouteInfo.EthosResourceIdentifier = contentType;

            ethosRouteInfo.ResourceName = resource;
            string[] routeStrings = contentType.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (routeStrings.Any())
            {
                ethosRouteInfo.ExtendedSchemaResourceId = resource.ToUpperInvariant();
                try
                {
                    ethosRouteInfo.ResourceVersionNumber = int.Parse(routeStrings[3].Substring(1)).ToString();
                }
                catch
                {
                    // Do not include in version number string.
                }
                try
                {
                    ethosRouteInfo.ResourceVersionNumber = string.Concat(ethosRouteInfo.ResourceVersionNumber, ".", int.Parse(routeStrings[4]));
                }
                catch
                {
                    // Do not include in version number string.
                }
                try
                {
                    ethosRouteInfo.ResourceVersionNumber = string.Concat(ethosRouteInfo.ResourceVersionNumber, ".", int.Parse(routeStrings[5]));
                }
                catch
                {
                    // Do not include in version number string.
                }
            }
            if (!string.IsNullOrEmpty(ethosRouteInfo.ResourceVersionNumber))
            {
                var versionNumber = ethosRouteInfo.ResourceVersionNumber.Split('.');
                int versionCount = versionNumber.Count();
                string version = versionNumber[0];
                if (versionCount == 1) version += ".0.0";
                if (versionCount == 2) version += "." + versionNumber[1] + ".0";
                if (versionCount == 3) version += "." + versionNumber[1] + "." + versionNumber[2];
                ethosRouteInfo.ResourceVersionNumber = version;
            }

            if (ethosRouteInfo == null || string.IsNullOrEmpty(ethosRouteInfo.ResourceName))
            {
                throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            }

            
            // Update Extended Filter Definitions
            ethosRouteInfo.ExtendedFilterDefinitions = GetEthosExtendedFilters();

            // Update global bypassCache flag for configuration cache
            ethosRouteInfo.BypassCache = bypassCache;

            return ethosRouteInfo;
        }

        private async Task<EthosApiConfiguration> GetApiConfiguration(string method, EthosResourceRouteInfo routeInfo, bool bypassCache)
        {
            var configuration = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfo, bypassCache);
            if (configuration == null || string.IsNullOrEmpty(configuration.ResourceName))
            {
                throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            }

            if (configuration.HttpMethods == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            }

            bool methodSupported = false;
            foreach (var meth in configuration.HttpMethods)
            {
                if (meth.Method == method)
                {
                    methodSupported = true;
                }
            }
            if (!methodSupported)
            {
                throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            }

            return configuration;
        }

        private Paging ConfigurePage(Paging page, EthosApiConfiguration configuration)
        {
            if (configuration.PageLimit == null || !configuration.PageLimit.HasValue || configuration.PageLimit.Value == 0)
            {
                page = new Paging(0, 0)
                {
                    DefaultLimit = null
                };
            }
            else
            {
                if (page == null)
                {
                    if (configuration.PageLimit != null && configuration.PageLimit.HasValue)
                    {
                        page = new Paging(configuration.PageLimit.Value, 0);
                    }
                    else
                    {
                        page = new Paging(100, 0);
                    }
                }
                else
                {
                    if (page.Limit >= configuration.PageLimit)
                    {
                        page = new Paging(configuration.PageLimit.Value, page.Offset);
                    }
                    page.DefaultLimit = configuration.PageLimit;
                }
            }
            return page;
        }

        private Dictionary<string, List<string>> GetEthosExtendedFilters()
        {
            Dictionary<string, List<string>> extendedFilterDefinitions = new Dictionary<string, List<string>>();

            var queryString = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            foreach (var query in queryString.Keys)
            {
                var queryName = query.ToString();
                if (queryName.Equals("offset", StringComparison.OrdinalIgnoreCase) || queryName.Equals("limit", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var value = queryString.Get(queryName);
                string path = string.Empty;

                try
                {
                    object filterObj = JsonConvert.DeserializeObject(value);
                    JObject jObject = (JObject)filterObj;

                    //get all the properties associated with the query string
                    IEnumerable<JProperty> jProperties = jObject.Properties();

                    // Loop through all the properties of that JObject
                    foreach (JProperty jProperty in jProperties)
                    {

                        if ((jProperty != null) && (jProperty.Value != null))
                        {
                            var jToken = (jProperty.Value);
                            if (jToken != null)
                            {
                                List<string> values = new List<string>();
                                if (jToken.Type == JTokenType.Array)
                                {
                                    var children = jToken.Children();
                                    foreach (var cToken in children)
                                    {
                                        var grandChildren = cToken.Children();
                                        if (grandChildren.Any())
                                        {
                                            foreach (var gToken in grandChildren)
                                            {
                                                var ggChildren = gToken.Children();
                                                if (ggChildren.Any())
                                                {
                                                    foreach (var ggToken in ggChildren)
                                                    {
                                                        if (string.IsNullOrEmpty(path))
                                                        {
                                                            path = ggToken.HasValues && ggToken.Last != null ? ggToken.Last.Path : ggToken.Path;
                                                        }
                                                        var temp = ggToken.HasValues && ggToken.Last != null ? ggToken.Last.Values() : ggToken.Values();
                                                        if (temp != null && ggToken.HasValues)
                                                        {
                                                            foreach (var val in temp)
                                                            {
                                                                values.Add(val.ToString());
                                                            }
                                                        }
                                                        else
                                                        {
                                                            values.Add(ggToken.Value<string>());
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(path))
                                                    {
                                                        path = gToken.HasValues && gToken.Last != null ? gToken.Last.Path : gToken.Path;
                                                    }
                                                    var temp = gToken.HasValues && gToken.Last != null ? gToken.Last.Values() : gToken.Values();
                                                    if (temp != null && gToken.HasValues)
                                                    {
                                                        foreach (var val in temp)
                                                        {
                                                            values.Add(val.ToString());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        values.Add(gToken.Value<string>());
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(path))
                                            {
                                                path = cToken.HasValues && cToken.Last != null ? cToken.Last.Path : cToken.Path;
                                            }
                                            var temp = cToken.HasValues && cToken.Last != null ? cToken.Last.Values() : cToken.Values();
                                            if (temp != null)
                                            {
                                                foreach (var val in temp)
                                                {
                                                    values.Add(val.ToString());
                                                }
                                            }
                                            else
                                            {
                                                values.Add(cToken.Value<string>());
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    path = jToken.HasValues && jToken.Last != null ? jToken.Last.Path : jToken.Path;
                                    var temp = jToken.HasValues && jToken.Last != null ? jToken.Last.Values() : jToken.Values();
                                    if (temp != null && jToken.HasValues)
                                    {
                                        foreach (var tokenValue in temp)
                                        {
                                            values.Add(tokenValue.ToString());
                                        }
                                    }
                                    else
                                    {
                                        values.Add(jToken.Value<string>());
                                    }
                                    //path = jToken.Path;
                                    //var val = jToken.ToString();
                                    //values.Add(val.ToString());
                                }

                                path = path.Replace("[0]", "[]");
                                if (values != null && values.Any() && !string.IsNullOrEmpty(path))
                                {
                                    if (!extendedFilterDefinitions.ContainsKey(path))
                                    {
                                        extendedFilterDefinitions.Add(path, values);
                                    }
                                }
                                else
                                {
                                    var message = string.Concat("'", queryName, "' ", value, "' is an invalid query parameter for filtering. ");
                                    throw new Exception(message);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Concat("'", queryName, "' ", value, "' is an invalid query parameter for filtering. ", ex.Message);
                    var exception = new IntegrationApiException();
                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                    throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
                }
            }
            return extendedFilterDefinitions;
        }

        private async Task<Dictionary<string, EthosExtensibleDataFilter>> ValidateFilterParameters(Dictionary<string, List<string>> filterDictionary, EthosExtensibleData extendedEthosVersion)
        {
            var returnDictionary = new Dictionary<string, EthosExtensibleDataFilter>();
            foreach (var filter in filterDictionary)
            {
                var filterName = filter.Key;
                var filterValues = filter.Value;
                var matchingProperty = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(ed => ed.FullJsonPath == filterName);
                if (matchingProperty == null)
                {
                    var message = string.Concat("'", filterName, "' is an invalid query parameter for filtering. ");
                    var exception = new IntegrationApiException();
                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                    throw exception;
                }
                else
                {
                    List<string> newValues = new List<string>();
                    foreach (var value in filterValues)
                    {
                        var newValue = value;
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.Date)
                            {
                                try
                                {
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Month.ToString(), "/", temp.Day.ToString(), "/", temp.Year.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid Date. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.DateTime)
                            {
                                try
                                {
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Month.ToString(), "/", temp.Day.ToString(), "/", temp.Year.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid DateTime. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.Time)
                            {
                                try
                                {
                                    newValue = DateTime.Today.ToString() + "T" + newValue;
                                    DateTime temp = Convert.ToDateTime(newValue);
                                    newValue = string.Concat(temp.Hour.ToString(), ":", temp.Minute.ToString(), ":", temp.Second.ToString());
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid DateTime. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (matchingProperty.JsonPropertyType == JsonPropertyTypeExtensions.Number)
                            {
                                try
                                {
                                    decimal temp = Convert.ToDecimal(newValue);
                                }
                                catch (FormatException ex)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid Decimal Number. ", ex.Message);
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                            }
                            if (!string.IsNullOrEmpty(matchingProperty.GuidColumnName) && !string.IsNullOrEmpty(matchingProperty.GuidFileName))
                            {
                                newValue = await _ethosApiBuilderService.GetRecordIdFromGuidAsync(newValue, matchingProperty.GuidFileName, matchingProperty.GuidColumnName);
                            }
                            if (!string.IsNullOrEmpty(matchingProperty.TransColumnName))
                            {
                                newValue = await _ethosApiBuilderService.GetRecordIdFromTranslationAsync(newValue, matchingProperty.TransFileName, matchingProperty.TransColumnName, matchingProperty.TransTableName);
                            }
                            if (matchingProperty.Enumerations != null && matchingProperty.Enumerations.Any())
                            {
                                var matchingEnumeration = matchingProperty.Enumerations.FirstOrDefault(en => en.EnumerationValue.Equals(newValue, StringComparison.OrdinalIgnoreCase));
                                if (matchingEnumeration == null)
                                {
                                    var message = string.Concat("'", newValue, "' is an invalid enumeration value. ");
                                    var exception = new IntegrationApiException();
                                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                                    throw exception;
                                }
                                else
                                {
                                    newValue = matchingEnumeration.ColleagueValue;
                                }
                            }
                            newValues.Add(newValue);
                        }
                    }
                    if (!returnDictionary.ContainsKey(filterName))
                    {
                        matchingProperty.FilterValue = newValues;
                        returnDictionary.Add(filterName, matchingProperty);
                    }
                }
            }
            return returnDictionary;
        }

        #endregion
    }
}