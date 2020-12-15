//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;

namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Provides access to BudgetCodes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    public class BudgetCodesController : BaseCompressedApiController
    {
        private readonly IBudgetCodesService _budgetCodesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BudgetCodesController class.
        /// </summary>
        /// <param name="budgetCodesService">Service of type <see cref="IBudgetCodesService">IBudgetCodesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public BudgetCodesController(IBudgetCodesService budgetCodesService, ILogger logger)
        {
            _budgetCodesService = budgetCodesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all budgetCodes
        /// </summary>
        /// <returns>List of BudgetCodes <see cref="Dtos.BudgetCodes"/> objects representing matching budgetCodes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BudgetCodes>> GetBudgetCodesAsync()
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
               var items = await _budgetCodesService.GetBudgetCodesAsync(bypassCache);

                AddEthosContextProperties(
                 await _budgetCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _budgetCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     items.Select(i => i.Id).Distinct().ToList()));

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
        /// Read (GET) a budgetCodes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired budgetCodes</param>
        /// <returns>A budgetCodes object <see cref="Dtos.BudgetCodes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.BudgetCodes> GetBudgetCodesByGuidAsync(string guid)
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
                   await _budgetCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _budgetCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _budgetCodesService.GetBudgetCodesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new budgetCodes
        /// </summary>
        /// <param name="budgetCodes">DTO of the new budgetCodes</param>
        /// <returns>A budgetCodes object <see cref="Dtos.BudgetCodes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.BudgetCodes> PostBudgetCodesAsync([FromBody] Dtos.BudgetCodes budgetCodes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing budgetCodes
        /// </summary>
        /// <param name="guid">GUID of the budgetCodes to update</param>
        /// <param name="budgetCodes">DTO of the updated budgetCodes</param>
        /// <returns>A budgetCodes object <see cref="Dtos.BudgetCodes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.BudgetCodes> PutBudgetCodesAsync([FromUri] string guid, [FromBody] Dtos.BudgetCodes budgetCodes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a budgetCodes
        /// </summary>
        /// <param name="guid">GUID to desired budgetCodes</param>
        [HttpDelete]
        public async Task DeleteBudgetCodesAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}