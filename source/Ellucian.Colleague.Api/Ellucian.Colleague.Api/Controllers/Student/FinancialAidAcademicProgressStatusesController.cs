//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to FinancialAidAcademicProgressStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidAcademicProgressStatusesController : BaseCompressedApiController
    {
        private readonly IFinancialAidAcademicProgressStatusesService _financialAidAcademicProgressStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidAcademicProgressStatusesController class.
        /// </summary>
        /// <param name="financialAidAcademicProgressStatusesService">Service of type <see cref="IFinancialAidAcademicProgressStatusesService">IFinancialAidAcademicProgressStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FinancialAidAcademicProgressStatusesController(IFinancialAidAcademicProgressStatusesService financialAidAcademicProgressStatusesService, ILogger logger)
        {
            _financialAidAcademicProgressStatusesService = financialAidAcademicProgressStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all financialAidAcademicProgressStatuses
        /// </summary>
        /// <returns>List of FinancialAidAcademicProgressStatuses <see cref="Dtos.FinancialAidAcademicProgressStatuses"/> objects representing matching financialAidAcademicProgressStatuses</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("restrictedVisibility", typeof(Dtos.Filters.RestrictedVisibilityFilter))]
        public async Task<IEnumerable<Dtos.FinancialAidAcademicProgressStatuses>> GetFinancialAidAcademicProgressStatusesAsync(QueryStringFilter restrictedVisibility)
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
                RestrictedVisibility? restrictedVisibilityValue = null;
                var restrictedVisibilityObj = GetFilterObject<Dtos.Filters.RestrictedVisibilityFilter>(_logger, "restrictedVisibility");
                if (restrictedVisibilityObj != null)
                {

                    restrictedVisibilityValue = restrictedVisibilityObj.RestrictedVisibility;
                }
                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.FinancialAidAcademicProgressStatuses>();

                var financialAidAcademicProgressStatuses = await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesAsync(restrictedVisibilityValue, bypassCache);

                if (financialAidAcademicProgressStatuses != null && financialAidAcademicProgressStatuses.Any())
                {
                    AddEthosContextProperties(await _financialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _financialAidAcademicProgressStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              financialAidAcademicProgressStatuses.Select(a => a.Id).ToList()));
                }
                return financialAidAcademicProgressStatuses;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Read (GET) a financialAidAcademicProgressStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired financialAidAcademicProgressStatuses</param>
        /// <returns>A financialAidAcademicProgressStatuses object <see cref="Dtos.FinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FinancialAidAcademicProgressStatuses> GetFinancialAidAcademicProgressStatusesByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _financialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _financialAidAcademicProgressStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Create (POST) a new financialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="financialAidAcademicProgressStatuses">DTO of the new financialAidAcademicProgressStatuses</param>
        /// <returns>A financialAidAcademicProgressStatuses object <see cref="Dtos.FinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidAcademicProgressStatuses> PostFinancialAidAcademicProgressStatusesAsync([FromBody] Dtos.FinancialAidAcademicProgressStatuses financialAidAcademicProgressStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing financialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="guid">GUID of the financialAidAcademicProgressStatuses to update</param>
        /// <param name="financialAidAcademicProgressStatuses">DTO of the updated financialAidAcademicProgressStatuses</param>
        /// <returns>A financialAidAcademicProgressStatuses object <see cref="Dtos.FinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidAcademicProgressStatuses> PutFinancialAidAcademicProgressStatusesAsync([FromUri] string guid, [FromBody] Dtos.FinancialAidAcademicProgressStatuses financialAidAcademicProgressStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a financialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="guid">GUID to desired financialAidAcademicProgressStatuses</param>
        [HttpDelete]
        public async Task DeleteFinancialAidAcademicProgressStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}