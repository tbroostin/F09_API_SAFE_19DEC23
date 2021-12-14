//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonMatchingRequests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonMatchingRequestsController : BaseCompressedApiController
    {
        private readonly IPersonMatchingRequestsService _personMatchingRequestsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonMatchingRequestsController class.
        /// </summary>
        /// <param name="personMatchingRequestsService">Service of type <see cref="IPersonMatchingRequestsService">IPersonMatchingRequestsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonMatchingRequestsController(IPersonMatchingRequestsService personMatchingRequestsService, ILogger logger)
        {
            _personMatchingRequestsService = personMatchingRequestsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personMatchingRequests
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">JSON formatted selection criteria.</param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters.</param>
        /// <returns>List of PersonMatchingRequests <see cref="Dtos.PersonMatchingRequests"/> objects representing matching personMatchingRequests</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonMatchRequest, BasePermissionCodes.CreatePersonMatchRequestProspects })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonMatchingRequests))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetPersonMatchingRequestsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                _personMatchingRequestsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var criteriaObject = GetFilterObject<Dtos.PersonMatchingRequests>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.PersonMatchingRequests>>(new List<Dtos.PersonMatchingRequests>(), page, 0, this.Request);

                var pageOfItems = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(page.Offset, page.Limit, criteriaObject, personFilterValue, bypassCache);

                AddEthosContextProperties(
                  await _personMatchingRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personMatchingRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonMatchingRequests>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a personMatchingRequests using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonMatchRequest, BasePermissionCodes.CreatePersonMatchRequestProspects })]
        public async Task<Dtos.PersonMatchingRequests> GetPersonMatchingRequestsByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _personMatchingRequestsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _personMatchingRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personMatchingRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personMatchingRequestsService.GetPersonMatchingRequestsByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new personMatchingRequests
        /// </summary>
        /// <param name="personMatchingRequests">DTO of the new personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonMatchingRequests> PostPersonMatchingRequestsAsync([FromBody] Dtos.PersonMatchingRequests personMatchingRequests)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personMatchingRequests
        /// </summary>
        /// <param name="guid">GUID of the personMatchingRequests to update</param>
        /// <param name="personMatchingRequests">DTO of the updated personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonMatchingRequests> PutPersonMatchingRequestsAsync([FromUri] string guid, [FromBody] Dtos.PersonMatchingRequests personMatchingRequests)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #region initiationsProspects

        /// <summary>
        /// Create (POST) a new personMatchingRequests
        /// </summary>
        /// <param name="personMatchingRequests">DTO of the new personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter, PermissionsFilter(BasePermissionCodes.CreatePersonMatchRequestProspects)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.PersonMatchingRequests> PostPersonMatchingRequestsInitiationsProspectsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonMatchingRequestsInitiationsProspects personMatchingRequests)
        {
            if (personMatchingRequests == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null personMatchingRequests argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(personMatchingRequests.Id) || !personMatchingRequests.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                _personMatchingRequestsService.ValidatePermissions(GetPermissionsMetaData());
                var dpList = await _personMatchingRequestsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);
                await _personMatchingRequestsService.ImportExtendedEthosData(await ExtractExtendedData(await _personMatchingRequestsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personMatchingRequestsReturn = await _personMatchingRequestsService.CreatePersonMatchingRequestsInitiationsProspectsAsync(personMatchingRequests);

                AddEthosContextProperties(dpList,
                    await _personMatchingRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personMatchingRequestsReturn.Id }));

                return personMatchingRequestsReturn;
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
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }

        }

        /// <summary>
        /// Update (PUT) an existing personMatchingRequests
        /// </summary>
        /// <param name="guid">GUID of the personMatchingRequests to update</param>
        /// <param name="personMatchingRequests">DTO of the updated personMatchingRequests</param>
        /// <returns>A personMatchingRequests object <see cref="Dtos.PersonMatchingRequests"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonMatchingRequests> PutPersonMatchingRequestsInitiationsProspectsAsync([FromUri] string guid, [FromBody] Dtos.PersonMatchingRequestsInitiationsProspects personMatchingRequests)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        /// <summary>
        /// Delete (DELETE) a personMatchingRequests
        /// </summary>
        /// <param name="guid">GUID to desired personMatchingRequests</param>
        [HttpDelete]
        public async Task DeletePersonMatchingRequestsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}