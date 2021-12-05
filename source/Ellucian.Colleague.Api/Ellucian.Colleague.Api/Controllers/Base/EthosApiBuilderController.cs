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
using System.Net.Http;
using System.Text.RegularExpressions;

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
        public async Task<object> GetEthosApiBuilderAsync([FromUri] string resource, [FromUri] string id, Paging page, [ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder)
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
                        await DeleteEthosApiBuilderAsync(resource, id);
                        return new HttpResponseMessage(HttpStatusCode.NoContent);
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
        public async Task<IHttpActionResult> GetEthosApiBuilderAsync(string resource, Paging page)
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

            if (!string.IsNullOrEmpty(routeInfo.ParentName))
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.{0}.v{1}+json", routeInfo.ExtendedSchemaResourceId.ToLower(), extendedEthosVersion.ApiVersionNumber));
            }
            else
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));
            }
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
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
        public async Task<Ellucian.Colleague.Dtos.EthosApiBuilder> GetEthosApiBuilderByIdAsync(string resource, string id)
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
            if (!string.IsNullOrEmpty(routeInfo.ParentName))
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.{0}.v{1}+json", routeInfo.ExtendedSchemaResourceId.ToLower(), extendedEthosVersion.ApiVersionNumber));
            }
            else
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));
            }
           
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(string.Format("Must provide an id for '{0}'.", routeInfo.ResourceName));
                }

                Dtos.EthosApiBuilder ethosApiBuilder = null;
                if (string.IsNullOrEmpty(configuration.ApiType) || configuration.ApiType.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    ethosApiBuilder = await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync(id, configuration.ResourceName);
                }
                else
                {
                    ethosApiBuilder = new Dtos.EthosApiBuilder() { Id = id };
                }

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
        public async Task<Dtos.EthosApiBuilder> PostEthosApiBuilderAsync(string resource, [ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder)
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

            if (!string.IsNullOrEmpty(routeInfo.ParentName))
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.{0}.v{1}+json", routeInfo.ExtendedSchemaResourceId.ToLower(), extendedEthosVersion.ApiVersionNumber));
            }
            else
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));
            }

            try
            {
                if (ethosApiBuilder == null)
                {
                    throw new ArgumentNullException(string.Format("Must provide a request body for '{0}'.", routeInfo.ResourceName));
                }
                if (string.IsNullOrEmpty(ethosApiBuilder.Id) && !string.IsNullOrEmpty(configuration.PrimaryGuidSource))
                {
                    throw new ArgumentNullException(string.Format("Must provide a valid GUID for '{0}' POST in request body.", routeInfo.ResourceName));
                }
                if (ethosApiBuilder.Id != Guid.Empty.ToString() && !string.IsNullOrEmpty(configuration.PrimaryGuidSource))
                {
                    throw new ArgumentException(string.Format("The requested GUID cannot be consumed on a '{0}' POST request.", routeInfo.ResourceName));
                }

                if (!string.IsNullOrEmpty(configuration.PrimaryGuidSource) || !string.IsNullOrEmpty(configuration.PrimaryKeyName))
                {
                    if (!string.IsNullOrEmpty(configuration.PrimaryKeyName) && string.IsNullOrEmpty(ethosApiBuilder.Id))
                    {
                        ethosApiBuilder.Id = "$NEW";
                    }
                    Validate(ethosApiBuilder);
                }
                else
                {
                    if (!string.IsNullOrEmpty(configuration.ProcessId))
                    {
                        ethosApiBuilder.Id = configuration.ProcessId;
                    }
                    else
                    {
                        ethosApiBuilder.Id = configuration.PrimaryEntity;
                    }
                }

                //get Data Privacy List
                var dpList = await _ethosApiBuilderService.GetDataPrivacyListByApi(routeInfo, true);

                //call import extend method that needs the extracted extension data and the config
                await _ethosApiBuilderService.ImportExtendedEthosData(await ExtractExtendedData(await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo), _logger));

                var ethosApiBuilderReturn = await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, configuration.ResourceName);

                AddEthosContextProperties(dpList,
                    await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo, new List<string>() { ethosApiBuilderReturn.Id }));

                // For Ethos Subroutine APIs, null out the ID property, which contains the Subroutine Name.
                if (!string.IsNullOrEmpty(configuration.ApiType) && configuration.ApiType.Equals("S", StringComparison.OrdinalIgnoreCase) ||
                    (string.IsNullOrEmpty(configuration.PrimaryGuidSource) && configuration.PrimaryEntity == ethosApiBuilderReturn.Id))
                {
                    ethosApiBuilderReturn.Id = null;
                }

                return ethosApiBuilderReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
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
        public async Task<Dtos.EthosApiBuilder> PutEthosApiBuilderAsync(string resource, string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder)
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

            if (!string.IsNullOrEmpty(routeInfo.ParentName))
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.{0}.v{1}+json", routeInfo.ExtendedSchemaResourceId.ToLower(), extendedEthosVersion.ApiVersionNumber));
            }
            else
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType(string.Format("application/vnd.hedtech.integration.v{0}+json", extendedEthosVersion.ApiVersionNumber));
            }

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(string.Format("Must provide an id for '{0}'.", routeInfo.ResourceName));
                }
                
                if (!string.IsNullOrWhiteSpace(id) && id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Format("Nil GUID must not be used in {0} PUT operation.", routeInfo.ResourceName));
                }

                if (!string.IsNullOrWhiteSpace(ethosApiBuilder.Id) && ethosApiBuilder.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Format("Nil GUID must not be used in {0} PUT operation.", routeInfo.ResourceName));
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
                await _ethosApiBuilderService.ImportExtendedEthosData(await ExtractExtendedData(await _ethosApiBuilderService.GetExtendedEthosConfigurationByResource(routeInfo), _logger));

                Dtos.EthosApiBuilder ethosApiBuilderReturn = null;
                bool performPostInstead = false;
                try
                {
                    ethosApiBuilderReturn = await _ethosApiBuilderService.PutEthosApiBuilderAsync(id,
                      await PerformPartialPayloadMerge(ethosApiBuilder, async () => await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync(id, configuration.ResourceName),
                      dpList, _logger), configuration.ResourceName);
                }
                catch (IntegrationApiException ex)
                {
                    if (ex.Errors != null && ex.Errors.FirstOrDefault() != null && ex.Errors.FirstOrDefault().Code == "GUID.Not.Found")
                    {
                        performPostInstead = true;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (performPostInstead)
                {
                    ethosApiBuilderReturn = await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, configuration.ResourceName);
                }

                AddEthosContextProperties(dpList,
                    await _ethosApiBuilderService.GetExtendedEthosDataByResource(routeInfo, new List<string>() { ethosApiBuilderReturn.Id }));

                return ethosApiBuilderReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound, id));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
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
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var routeInfo = GetEthosResourceRouteInfo(resource);
            var configuration = await GetApiConfiguration("DELETE", routeInfo, bypassCache);

            if (!string.IsNullOrEmpty(routeInfo.ParentName))
            {
                NotAcceptableStatusException();
            }
            else
            {
                CustomMediaTypeAttributeFilter.SetCustomMediaType("application/json");
            }
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException(string.Format("{0} id cannot be null or empty", resource));
                }
                await _ethosApiBuilderService.DeleteEthosApiBuilderAsync(id, configuration.ResourceName);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("GUID.Not.Found", "An error occurred translating the GUID to an ID.", e.Message, HttpStatusCode.NotFound, id));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
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

        #region routing

        /// <summary>
        /// Respond to route and execute the appropriate method.  This will only be hit if there are routes defined on the resource, therefore always return a 406 if an error occurs.
        /// </summary>       
        /// <param name="id"></param>
        /// <param name="guid"></param>
        /// <param name="resource"></param>
        /// <param name="page"></param>
        /// <param name="ethosApiBuilder"></param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut, HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PagingFilter(IgnorePaging = true), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<object> GetAlternativeRouteOrNotAcceptable([ModelBinder(typeof(EedmModelBinder))] Dtos.EthosApiBuilder ethosApiBuilder, [FromUri] string resource = null, [FromUri] string id = null, [FromUri] string guid = null, Paging page = null)
        {
            bool bypassCache = false;
            bool methodSupported = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                id = guid;
            }
            try
            {
                var actionRequestContext = ActionContext.Request;

                if (actionRequestContext == null)
                {
                    NotAcceptableStatusException();
                }

                var routeData = actionRequestContext.GetRouteData();

                if (routeData.Route == null)
                {
                    NotAcceptableStatusException();
                }

                if (string.IsNullOrEmpty(resource))
                {
                    resource = routeData.Route.RouteTemplate;
                    if (resource.LastIndexOf("/") > 0)
                    {
                        resource = resource.Remove(resource.LastIndexOf("/"));
                    }

                    if (resource.StartsWith("{"))
                    {
                        resource = resource.Remove(1, 1);
                    }
                    if (resource.EndsWith("}"))
                    {
                        resource = resource.Remove(resource.Length - 1, 1);
                    }
                }


                var routeInfo = GetEthosResourceRouteInfo(resource, true);

                if (routeInfo == null)
                {
                    NotAcceptableStatusException();
                }

                var method = routeInfo.RequestMethod.ToString();
                if (method == "PUT" && string.IsNullOrEmpty(id))
                {
                    NotAcceptableStatusException();
                }
                else
                {
                    if (method == "POST" && !string.IsNullOrEmpty(id))
                    {
                        NotAcceptableStatusException();
                    }
                }

                var configuration = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfo, bypassCache);
                if (configuration == null || string.IsNullOrEmpty(configuration.ResourceName))
                {
                    NotAcceptableStatusException();
                }
                if (!string.IsNullOrEmpty(configuration.ParentResourceName) && !configuration.ParentResourceName.Equals(routeInfo.ResourceName, StringComparison.OrdinalIgnoreCase))
                {
                    NotAcceptableStatusException();
                }

                //  If this an Ethos Extension on an Ellucian delivered API, throw 406
                if (configuration.HttpMethods == null || !configuration.HttpMethods.Any())
                {
                    NotAcceptableStatusException();
                }

               
                foreach (var meth in configuration.HttpMethods)
                {
                    if (meth.Method == method)
                    {
                        methodSupported = true;
                    }
                }

            }
            catch (JsonException ex)
            {
                var message = string.Concat("'", "Invalid query parameter for filtering. ", ex.Message);
                var exception = new IntegrationApiException();
                exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
            }
            catch (Exception)
            {
                NotAcceptableStatusException();
            }
        
            if (!methodSupported)
            {
                MethodNotAllowedException();
            }

            return await GetEthosApiBuilderAsync(resource, id, page, ethosApiBuilder);
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
        public EthosResourceRouteInfo GetEthosResourceRouteInfo(string resource, bool alternativeRepresenation = false)
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

            ethosRouteInfo.RequestMethod = Request.Method;

            ethosRouteInfo.ResourceName = resource;
            if (!string.IsNullOrEmpty(contentType))
            {
                string hedtechIntegrationMediaTypePrefix = "application/vnd.hedtech.integration.";
                var alternativeRepresenationName = contentType.Replace(hedtechIntegrationMediaTypePrefix, "");

                if (alternativeRepresenationName.LastIndexOf(".v") > 0)
                {
                    alternativeRepresenationName = alternativeRepresenationName.Remove(alternativeRepresenationName.LastIndexOf(".v"));
                    if (!string.IsNullOrEmpty(alternativeRepresenationName))
                    {
                        ethosRouteInfo.ExtendedSchemaResourceId = alternativeRepresenationName;
                        ethosRouteInfo.ParentName = resource;
                    }
                }
                
                ethosRouteInfo.ResourceVersionNumber = ExtractVersionNumberOnly(contentType);
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

        private string ExtractVersionNumberOnly(string original)
        {          
            if (original.LastIndexOf(".v") > 0)
            {
                // remove the period and the leading 'v'
                original = original.Remove(0, original.LastIndexOf(".v") + 2);                
            }
            

            if (Regex.Matches(original, @"[a-zA-Z]", RegexOptions.Compiled).Count > 0)
            {
                throw CreateHttpResponseException(
                    new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            }

            var regex = new Regex(@"(?:(\d+)\.)?(?:(\d+)\.)?(?:(\d+)\.\d+)|(?:(\d+))", RegexOptions.Compiled);
            Match semanticVersion = regex.Match(original);
            if (semanticVersion.Success)
            {
                return semanticVersion.Value;
            }
            else return string.Empty;
        }

        private async Task<EthosApiConfiguration> GetApiConfiguration(string method, EthosResourceRouteInfo routeInfo, bool bypassCache)
        {
            if (routeInfo == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            }
            var configuration = await _ethosApiBuilderService.GetEthosApiConfigurationByResource(routeInfo, bypassCache);

            if (configuration == null || string.IsNullOrEmpty(configuration.ResourceName))
            {
                throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            }

            //  If an spec is expected to have a parent, but no accept header is provided,
            //  OR if a spec doesnt have a parent, but one is provided in the accept header, 
            //  OR is a spec has a parent, but its different from what is provided, then throw a 406
            //  OR is an Ethos Extension on an ellucian delivered API
            if ((!string.IsNullOrEmpty(configuration.ParentResourceName) && string.IsNullOrEmpty(routeInfo.ParentName))
                || (string.IsNullOrEmpty(configuration.ParentResourceName) && !string.IsNullOrEmpty(routeInfo.ParentName)) 
                || (!string.IsNullOrEmpty(configuration.ParentResourceName) && !configuration.ParentResourceName.Equals(routeInfo.ParentName, StringComparison.OrdinalIgnoreCase)) 
                || (configuration.HttpMethods == null || !configuration.HttpMethods.Any()) )
            {
                NotAcceptableStatusException();
            }

            // If no methods are defined, then throw a 405
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
                if (page != null && (page.Offset > 0 || page.Limit > 0))
                {
                    var message = string.Concat("Paging is disabled for the '", configuration.ResourceName, "' resource.");
                    var exception = new IntegrationApiException();
                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                    throw exception;
                }
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

        private Dictionary<string, Tuple<List<string>, string>> GetEthosExtendedFilters()
        {
            Dictionary<string, Tuple<List<string>, string>> extendedFilterDefinitions = new Dictionary<string, Tuple<List<string>, string>>();

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
                                var oper = string.Empty;

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
                                                            var pathSplit = path.Split('.');
                                                            path = string.Empty;
                                                            foreach (var part in pathSplit)
                                                            {
                                                                if (part.StartsWith("$"))
                                                                {
                                                                    oper = part;
                                                                }
                                                                else
                                                                {
                                                                    path = string.Concat(path, ".", part).TrimStart('.');
                                                                }
                                                            }
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
                                                        var pathSplit = path.Split('.');
                                                        path = string.Empty;
                                                        foreach (var part in pathSplit)
                                                        {
                                                            if (part.StartsWith("$"))
                                                            {
                                                                oper = part;
                                                            }
                                                            else
                                                            {
                                                                path = string.Concat(path, ".", part).TrimStart('.');
                                                            }
                                                        }
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
                                                var pathSplit = path.Split('.');
                                                path = string.Empty;
                                                foreach (var part in pathSplit)
                                                {
                                                    if (part.StartsWith("$"))
                                                    {
                                                        oper = part;
                                                    }
                                                    else
                                                    {
                                                        path = string.Concat(path, ".", part).TrimStart('.');
                                                    }
                                                }
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
                                    if (jToken.HasValues && jToken.Last != null && jToken.Last.HasValues && jToken.Last.Last != null && jToken.Last.Last.HasValues)
                                    {
                                        path = jToken.Last.Last.Path;
                                        if (jToken.Last.Last.Last != null && jToken.Last.Last.Last.HasValues)
                                        {
                                            path = jToken.Last.Last.Last.Path;
                                            if (jToken.Last.Last.Last.Last != null && jToken.Last.Last.Last.Last.HasValues)
                                            {
                                                path = jToken.Last.Last.Last.Last.Path;
                                            }
                                        }
                                    }
                                    var pathSplit = path.Split('.');
                                    path = string.Empty;
                                    foreach (var part in pathSplit)
                                    {
                                        if (part.StartsWith("$"))
                                        {
                                            oper = part;
                                        }
                                        else
                                        {
                                            path = string.Concat(path, ".", part).TrimStart('.');
                                        }
                                    }
                                    var temp = jToken.HasValues && jToken.Last != null ? jToken.Last.Values() : jToken.Values();
                                    if (jToken.HasValues && jToken.Last != null && jToken.Last.HasValues && jToken.Last.Last != null && jToken.Last.Last.HasValues)
                                    {
                                        temp = jToken.Last.Last.Values();
                                        if (jToken.Last.Last.Last != null && jToken.Last.Last.Last.HasValues)
                                        {
                                            temp = jToken.Last.Last.Last.Values();
                                            if (jToken.Last.Last.Last.Last != null && jToken.Last.Last.Last.Last.HasValues)
                                            {
                                                temp = jToken.Last.Last.Last.Last.Values();
                                            }
                                        }
                                    }
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
                                        extendedFilterDefinitions.Add(path, new Tuple<List<string>, string>(values, oper));
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
                catch (JsonException ex)
                {
                    throw ex;
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

        private async Task<Dictionary<string, EthosExtensibleDataFilter>> ValidateFilterParameters(Dictionary<string, Tuple<List<string>, string>> filterDictionary, EthosExtensibleData extendedEthosVersion)
        {
            // Validate the query names
            var queryString = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            List<string> queryNames = new List<string>();
            foreach (var query in queryString.Keys)
            {
                if (!query.ToString().Equals("offset", StringComparison.OrdinalIgnoreCase) && !query.ToString().Equals("limit", StringComparison.OrdinalIgnoreCase))
                {
                    var matchingQueryName = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(ed => ed.QueryName == query.ToString());
                    if (matchingQueryName == null)
                    {
                        var message = string.Concat("'", query, "' is an invalid query parameter for filtering.");
                        var exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                        throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
                    }
                }
                queryNames.Add(query.ToString());
            }
            
            // Validate query string
            var returnDictionary = new Dictionary<string, EthosExtensibleDataFilter>();
            foreach (var filter in filterDictionary)
            {
                var filterName = filter.Key;
                var filterValues = filter.Value.Item1;
                var filterOper = filter.Value.Item2;

                var matchingProperty = extendedEthosVersion.ExtendedDataFilterList.FirstOrDefault(ed => ed.FullJsonPath == filterName);
                if (matchingProperty == null)
                {
                    var message = string.Concat("'", filterName, "' is an invalid query parameter for filtering.");
                    var exception = new IntegrationApiException();
                    exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                    throw exception;
                }
                else
                {
                    if (!queryNames.Contains(matchingProperty.QueryName))
                    {
                        var message = string.Concat("'", filterName, "' is an invalid query parameter for filtering. Expecting '", matchingProperty.QueryName, "='");
                        var exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                        throw exception;
                    }

                    // Validate Filter Operators
                    bool validFilterOper = true;
                    if (matchingProperty.ValidFilterOpers != null && matchingProperty.ValidFilterOpers.Any())
                    {
                        if (!string.IsNullOrEmpty(filterOper) && !matchingProperty.ValidFilterOpers.Contains(filterOper))
                        {
                            validFilterOper = false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filterOper))
                        {
                            switch (filterOper)
                            {
                                case "$lte":
                                    break;
                                case "$gte":
                                    break;
                                case "$lt":
                                    break;
                                case "$gt":
                                    break;
                                case "$ne":
                                    break;
                                case "$eq":
                                    break;
                                default:
                                    validFilterOper = false;
                                    break;
                            }
                        }
                    }
                    if (!validFilterOper)
                    {
                        var message = string.Concat("'", filterOper, "' is an invalid query parameter for filtering. ");
                        var exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", message));
                        throw exception;
                    }

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
                        matchingProperty.FilterOper = filterOper;
                        returnDictionary.Add(filterName, matchingProperty);
                    }
                }
            }
            return returnDictionary;
        }

        #endregion
    }
}