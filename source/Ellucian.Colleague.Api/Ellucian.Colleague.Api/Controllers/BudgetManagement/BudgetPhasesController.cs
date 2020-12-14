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
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;

namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Provides access to BudgetPhases
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    public class BudgetPhasesController : BaseCompressedApiController
    {
        private readonly IBudgetPhasesService _budgetPhasesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BudgetPhasesController class.
        /// </summary>
        /// <param name="budgetPhasesService">Service of type <see cref="IBudgetPhasesService">IBudgetPhasesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public BudgetPhasesController(IBudgetPhasesService budgetPhasesService, ILogger logger)
        {
            _budgetPhasesService = budgetPhasesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all budgetPhases
        /// </summary>
         /// <param name="criteria">Filter criteria</param>
        /// <returns>List of BudgetPhases <see cref="Dtos.BudgetPhases"/> objects representing matching budgetPhases</returns>
        [HttpGet, EedmResponseFilter, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.BudgetPhases))]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BudgetPhases>> GetBudgetPhasesAsync(QueryStringFilter criteria)
        {
            string budgetCode = string.Empty;

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
                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.BudgetPhases>();

                var criteriaObj = GetFilterObject<Dtos.BudgetPhases>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    budgetCode = criteriaObj.BudgetCode != null ? criteriaObj.BudgetCode.Id : string.Empty;
                }             
                
                var items = await _budgetPhasesService.GetBudgetPhasesAsync(budgetCode, bypassCache);

                AddEthosContextProperties(
                  await _budgetPhasesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _budgetPhasesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Read (GET) a budgetPhases using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired budgetPhases</param>
        /// <returns>A budgetPhases object <see cref="Dtos.BudgetPhases"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.BudgetPhases> GetBudgetPhasesByGuidAsync(string guid)
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
                   await _budgetPhasesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _budgetPhasesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _budgetPhasesService.GetBudgetPhasesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new budgetPhases
        /// </summary>
        /// <param name="budgetPhases">DTO of the new budgetPhases</param>
        /// <returns>A budgetPhases object <see cref="Dtos.BudgetPhases"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.BudgetPhases> PostBudgetPhasesAsync([FromBody] Dtos.BudgetPhases budgetPhases)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing budgetPhases
        /// </summary>
        /// <param name="guid">GUID of the budgetPhases to update</param>
        /// <param name="budgetPhases">DTO of the updated budgetPhases</param>
        /// <returns>A budgetPhases object <see cref="Dtos.BudgetPhases"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.BudgetPhases> PutBudgetPhasesAsync([FromUri] string guid, [FromBody] Dtos.BudgetPhases budgetPhases)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a budgetPhases
        /// </summary>
        /// <param name="guid">GUID to desired budgetPhases</param>
        [HttpDelete]
        public async Task DeleteBudgetPhasesAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}