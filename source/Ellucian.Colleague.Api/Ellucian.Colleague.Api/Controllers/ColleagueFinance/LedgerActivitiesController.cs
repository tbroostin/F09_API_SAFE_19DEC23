//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Http;
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
    /// Provides access to LedgerActivities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class LedgerActivitiesController : BaseCompressedApiController
    {
        private readonly ILedgerActivityService _ledgerActivityService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the LedgerActivitiesController class.
        /// </summary>
        /// <param name="ledgerActivityService">Service of type <see cref="ILedgerActivityService">ILedgerActivityService</see></param>
        /// <param name="logger">Interface to logger</param>
        public LedgerActivitiesController(ILedgerActivityService ledgerActivityService, ILogger logger)
        {
            _ledgerActivityService = ledgerActivityService;
            _logger = logger;
        }

        /// <summary>
        /// Return all ledgerActivities
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="fiscalYear"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(LedgerActivityFilter)), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("fiscalYear", typeof(FiscalYearFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetLedgerActivitiesAsync(Paging page, QueryStringFilter criteria, QueryStringFilter fiscalYear = null)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                string fiscalYearId = string.Empty, fiscalPeriod = string.Empty, reportingSegment = string.Empty, transactionDate = string.Empty;
                var ledgerActivityFilter = GetFilterObject<LedgerActivityFilter>(_logger, "criteria");
                var fiscalYearObj = GetFilterObject<Dtos.Filters.FiscalYearFilter>(_logger, "fiscalYear");

                if (CheckForEmptyFilterParameters())
                   return new PagedHttpActionResult<IEnumerable<Dtos.LedgerActivity>>(new List<Dtos.LedgerActivity>(), page, 0, this.Request);

                if (ledgerActivityFilter != null)
                {
                    if ((ledgerActivityFilter.FiscalYear != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.FiscalYear.Id)))
                        fiscalYearId = ledgerActivityFilter.FiscalYear.Id;

                    // To prevent a breaking change, this version accepts the fiscalPeriod filter using eith "fiscalPeriod" or "period".  
                    // If both are provided, and are different values,  return an empty set.
                    if ((ledgerActivityFilter.FiscalPeriod != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.FiscalPeriod.Id))
                        && (ledgerActivityFilter.Period != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.Period.Id))
                        && ledgerActivityFilter.Period.Id != ledgerActivityFilter.FiscalPeriod.Id)
                    {                   
                        return new PagedHttpActionResult<IEnumerable<Dtos.LedgerActivity>>(new List<Dtos.LedgerActivity>(), page, 0, this.Request);
                    }
                    if ((ledgerActivityFilter.FiscalPeriod != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.FiscalPeriod.Id)))
                        fiscalPeriod = ledgerActivityFilter.FiscalPeriod.Id;
                    else if ((ledgerActivityFilter.Period != null) && !(string.IsNullOrEmpty(ledgerActivityFilter.Period.Id)))
                        fiscalPeriod = ledgerActivityFilter.Period.Id;

                    if (!string.IsNullOrEmpty(ledgerActivityFilter.ReportingSegment))
                        reportingSegment = ledgerActivityFilter.ReportingSegment;
                    if (ledgerActivityFilter.TransactionDate.HasValue)
                        transactionDate = ledgerActivityFilter.TransactionDate.ToString(); 
                }

                if (fiscalYearObj != null && fiscalYearObj.FiscalYear != null)
                {
                    // To prevent a breaking change, this version accepts the fiscalYear filter using eith a standard filter or a named query 
                    // If both are provided return an empty set.  This should never be hit since standard + named queries are not permitted.
                    if (!string.IsNullOrEmpty(fiscalYearId))
                    {
                        return new PagedHttpActionResult<IEnumerable<Dtos.LedgerActivity>>(new List<Dtos.LedgerActivity>(), page, 0, this.Request);
                    }
                    fiscalYearId =
                        string.IsNullOrEmpty(fiscalYearObj.FiscalYear.Id) ?
                        null :
                        fiscalYearObj.FiscalYear.Id;
                }

                var pageOfItems = await _ledgerActivityService.GetLedgerActivitiesAsync(page.Offset, page.Limit, fiscalYearId, fiscalPeriod, reportingSegment, transactionDate, bypassCache);

                AddEthosContextProperties(
                  await _ledgerActivityService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _ledgerActivityService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.LedgerActivity>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
            catch (InvalidOperationException e)
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
        /// Read (GET) a ledgerActivities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired ledgerActivities</param>
        /// <returns>A ledgerActivities object <see cref="Dtos.LedgerActivity"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.LedgerActivity> GetLedgerActivitiesByGuidAsync(string guid)
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
                  await _ledgerActivityService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _ledgerActivityService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _ledgerActivityService.GetLedgerActivityByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new ledgerActivities
        /// </summary>
        /// <param name="ledgerActivities">DTO of the new ledgerActivities</param>
        /// <returns>A ledgerActivities object <see cref="Dtos.LedgerActivity"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.LedgerActivity> PostLedgerActivitiesAsync([FromBody] Dtos.LedgerActivity ledgerActivities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing ledgerActivities
        /// </summary>
        /// <param name="guid">GUID of the ledgerActivities to update</param>
        /// <param name="ledgerActivities">DTO of the updated ledgerActivities</param>
        /// <returns>A ledgerActivities object <see cref="Dtos.LedgerActivity"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.LedgerActivity> PutLedgerActivitiesAsync([FromUri] string guid, [FromBody] Dtos.LedgerActivity ledgerActivities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a ledgerActivities
        /// </summary>
        /// <param name="guid">GUID to desired ledgerActivities</param>
        [HttpDelete]
        public async Task DeleteLedgerActivitiesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}