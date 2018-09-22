// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to FinancialAidAwardPeriods data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidAwardPeriodsController : BaseCompressedApiController
    {
        private readonly IFinancialAidAwardPeriodService _financialAidAwardPeriodService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidAwardPeriodsController class.
        /// </summary>
        /// <param name="financialAidAwardPeriodService">Repository of type <see cref="IFinancialAidAwardPeriodService">IFinancialAidAwardPeriodService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public FinancialAidAwardPeriodsController(IFinancialAidAwardPeriodService financialAidAwardPeriodService, ILogger logger)
        {
            _financialAidAwardPeriodService = financialAidAwardPeriodService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid award periods.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidAwardPeriod objects.</returns>
        [HttpGet, ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Dtos.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync()
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            
            try
            {                
                var items = await _financialAidAwardPeriodService.GetFinancialAidAwardPeriodsAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(
                        await _financialAidAwardPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _financialAidAwardPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            items.Select(i => i.Id).ToList()));
                }

                return items;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Financial Aid Award Periods by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see>object.</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.FinancialAidAwardPeriod> GetFinancialAidAwardPeriodByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                AddEthosContextProperties(
                    await _financialAidAwardPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _financialAidAwardPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _financialAidAwardPeriodService.GetFinancialAidAwardPeriodByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Creates a Financial Aid Award Period.
        /// </summary>
        /// <param name="financialAidAwardPeriod"><see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see></returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidAwardPeriod> PostFinancialAidAwardPeriodAsync([FromBody] Dtos.FinancialAidAwardPeriod financialAidAwardPeriod)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Award Period.
        /// </summary>
        /// <param name="id">Id of the Financial Aid Award Period to update</param>
        /// <param name="financialAidAwardPeriod"><see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidAwardPeriod">FinancialAidAwardPeriod</see></returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidAwardPeriod> PutFinancialAidAwardPeriodAsync([FromUri] string id, [FromBody] Dtos.FinancialAidAwardPeriod financialAidAwardPeriod)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Award Period.
        /// </summary>
        /// <param name="id">ID of the Financial Aid Award Period to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteFinancialAidAwardPeriodAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}