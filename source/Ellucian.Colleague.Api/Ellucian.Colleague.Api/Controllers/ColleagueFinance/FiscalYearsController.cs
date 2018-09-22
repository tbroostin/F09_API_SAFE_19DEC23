// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
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

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Fiscal Years controller.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class FiscalYearsController : BaseCompressedApiController
    {
        private readonly IFiscalYearsService fiscalYearsService;
        private readonly ICostCenterService costCenterService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the Fiscal Years controller.
        /// </summary>
        /// <param name="costCenterService">GL cost center service object.</param>
        /// <param name="fiscalYearsService">Service of type <see cref="IFiscalYearsService">IFiscalYearsService</see></param>
        /// <param name="logger">Logger object.</param>
        public FiscalYearsController(ICostCenterService costCenterService, IFiscalYearsService fiscalYearsService, ILogger logger)
        {
            this.fiscalYearsService = fiscalYearsService;
            this.costCenterService = costCenterService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all the GL fiscal years available to the user - these include the
        /// fiscal year for today plus a maximum of 5 previous fiscal years, if available.
        /// </summary>
        /// <returns>List of fiscal years.</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        public async Task<IEnumerable<string>> GetAsync()
        {
            try
            {
                // Call the service method to obtain the list of fiscal years to display in the cost center views.
                return await costCenterService.GetFiscalYearsAsync();
            }
            catch (ConfigurationException cnex)
            {
                logger.Error(cnex, cnex.Message);
                throw CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the available fiscal years.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the fiscal year for today's date based on the General Ledger configuration.
        /// </summary>
        /// <returns>The fiscal year for today's date.</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        public async Task<string> GetFiscalYearForTodayAsync()
        {
            try
            {
                // Call the service method to obtain today's fiscal year to apply in the cost centers view.
                return await costCenterService.GetFiscalYearForTodayAsync();
            }
            catch (ConfigurationException cnex)
            {
                logger.Error(cnex, cnex.Message);
                throw CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the fiscal year for today's date.", HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Return all fiscalYears
        /// </summary>
        /// <returns>List of FiscalYears <see cref="Dtos.FiscalYears"/> objects representing matching fiscalYears</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.FiscalYears)), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FiscalYears>> GetFiscalYearsAsync(QueryStringFilter criteria)
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
                string reportingSegment = string.Empty;
                var fiscalYearsFilter = GetFilterObject<Dtos.FiscalYears>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.FiscalYears>(new List<Dtos.FiscalYears>());

                if (fiscalYearsFilter != null)
                {
                    if (!string.IsNullOrEmpty(fiscalYearsFilter.ReportingSegment))
                    {
                        reportingSegment = fiscalYearsFilter.ReportingSegment;
                    }
                }

                var fiscalYears = await fiscalYearsService.GetFiscalYearsAsync(bypassCache, reportingSegment);

                if (fiscalYears != null && fiscalYears.Any())
                {
                    AddEthosContextProperties(await fiscalYearsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await fiscalYearsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              fiscalYears.Select(a => a.Id).ToList()));
                }
                return fiscalYears;
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a fiscalYears using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fiscalYears</param>
        /// <returns>A fiscalYears object <see cref="Dtos.FiscalYears"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FiscalYears> GetFiscalYearsByGuidAsync(string guid)
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
                  await fiscalYearsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await fiscalYearsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await fiscalYearsService.GetFiscalYearsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new fiscalYears
        /// </summary>
        /// <param name="fiscalYears">DTO of the new fiscalYears</param>
        /// <returns>A fiscalYears object <see cref="Dtos.FiscalYears"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FiscalYears> PostFiscalYearsAsync([FromBody] Dtos.FiscalYears fiscalYears)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fiscalYears
        /// </summary>
        /// <param name="guid">GUID of the fiscalYears to update</param>
        /// <param name="fiscalYears">DTO of the updated fiscalYears</param>
        /// <returns>A fiscalYears object <see cref="Dtos.FiscalYears"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FiscalYears> PutFiscalYearsAsync([FromUri] string guid, [FromBody] Dtos.FiscalYears fiscalYears)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fiscalYears
        /// </summary>
        /// <param name="guid">GUID to desired fiscalYears</param>
        [HttpDelete]
        public async Task DeleteFiscalYearsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}