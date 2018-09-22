//Copyright 2017-18 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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
    /// Provides access to FinancialAidFundClassifications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidFundClassificationsController : BaseCompressedApiController
    {
        private readonly IFinancialAidFundClassificationsService _financialAidFundClassificationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidFundClassificationsController class.
        /// </summary>
        /// <param name="financialAidFundClassificationsService">Service of type <see cref="IFinancialAidFundClassificationsService">IFinancialAidFundClassificationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FinancialAidFundClassificationsController(IFinancialAidFundClassificationsService financialAidFundClassificationsService, ILogger logger)
        {
            _financialAidFundClassificationsService = financialAidFundClassificationsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all financialAidFundClassifications
        /// </summary>
                /// <returns>List of FinancialAidFundClassifications <see cref="Dtos.FinancialAidFundClassifications"/> objects representing matching financialAidFundClassifications</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFundClassifications>> GetFinancialAidFundClassificationsAsync()
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
                var items = await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsAsync(bypassCache);

                AddEthosContextProperties(await _financialAidFundClassificationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _financialAidFundClassificationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return items;
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
        /// Read (GET) a financialAidFundClassifications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired financialAidFundClassifications</param>
        /// <returns>A financialAidFundClassifications object <see cref="Dtos.FinancialAidFundClassifications"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FinancialAidFundClassifications> GetFinancialAidFundClassificationsByGuidAsync(string guid)
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
                var classification = await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsByGuidAsync(guid);

                if (classification != null)
                {

                    AddEthosContextProperties(await _financialAidFundClassificationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _financialAidFundClassificationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { classification.Id }));
                }

                return classification;
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
        /// Create (POST) a new financialAidFundClassifications
        /// </summary>
        /// <param name="financialAidFundClassifications">DTO of the new financialAidFundClassifications</param>
        /// <returns>A financialAidFundClassifications object <see cref="Dtos.FinancialAidFundClassifications"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidFundClassifications> PostFinancialAidFundClassificationsAsync([FromBody] Dtos.FinancialAidFundClassifications financialAidFundClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidFundClassifications
        /// </summary>
        /// <param name="guid">GUID of the financialAidFundClassifications to update</param>
        /// <param name="financialAidFundClassifications">DTO of the updated financialAidFundClassifications</param>
        /// <returns>A financialAidFundClassifications object <see cref="Dtos.FinancialAidFundClassifications"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidFundClassifications> PutFinancialAidFundClassificationsAsync([FromUri] string guid, [FromBody] Dtos.FinancialAidFundClassifications financialAidFundClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidFundClassifications
        /// </summary>
        /// <param name="guid">GUID to desired financialAidFundClassifications</param>
        [HttpDelete]
        public async Task DeleteFinancialAidFundClassificationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}