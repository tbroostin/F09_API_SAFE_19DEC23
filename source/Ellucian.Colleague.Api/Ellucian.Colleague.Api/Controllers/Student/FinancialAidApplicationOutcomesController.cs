//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to FinancialAidApplicationOutcomes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidApplicationOutcomesController : BaseCompressedApiController
    {
        private readonly IFinancialAidApplicationOutcomeService financialAidApplicationOutcomeService;
        //private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidApplicationOutcomesController class.
        /// </summary>
        /// <param name="financialAidApplicationOutcomeService">FinancialAidApplicationOutcomeService</param>
        /// <param name="logger">Interface to logger</param>
        public FinancialAidApplicationOutcomesController(IFinancialAidApplicationOutcomeService financialAidApplicationOutcomeService,
            ILogger logger)
        {
            //this.adapterRegistry = adapterRegistry;
            this.financialAidApplicationOutcomeService = financialAidApplicationOutcomeService;
            this.logger = logger;
        }

        /// <summary>
        /// Return all financialAidApplicationOutcomes
        /// </summary>
        /// <returns>List of FinancialAidApplicationOutcomes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewFinancialAidApplicationOutcomes)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.FinancialAidApplicationOutcome)), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetFinancialAidApplicationOutcomesAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            try
            {
                financialAidApplicationOutcomeService.ValidatePermissions(GetPermissionsMetaData());
                var criteriaObject = GetFilterObject<Dtos.FinancialAidApplicationOutcome>(logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.FinancialAidApplication>>(new List<Dtos.FinancialAidApplication>(), page, 0, this.Request);
                
                var pageOfItems = await financialAidApplicationOutcomeService.GetAsync(page.Offset, page.Limit, criteriaObject, bypassCache);

                AddEthosContextProperties(
                    await financialAidApplicationOutcomeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await financialAidApplicationOutcomeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.FinancialAidApplicationOutcome>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a financialAidApplicationOutcomes using a GUID
        /// </summary>
        /// <param name="id">GUID to desired financialAidApplicationOutcomes</param>
        /// <returns>A single financialAidApplicationOutcomes object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewFinancialAidApplicationOutcomes)]
        public async Task<Dtos.FinancialAidApplicationOutcome> GetFinancialAidApplicationOutcomesByGuidAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                financialAidApplicationOutcomeService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                     await financialAidApplicationOutcomeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await financialAidApplicationOutcomeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { id }));

                return await financialAidApplicationOutcomeService.GetByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new financialAidApplicationOutcomes
        /// </summary>
        /// <param name="financialAidApplicationOutcomes">DTO of the new financialAidApplicationOutcomes</param>
        /// <returns>A single financialAidApplicationOutcomes object</returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidApplicationOutcome> CreateAsync([FromBody] Dtos.FinancialAidApplicationOutcome financialAidApplicationOutcomes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidApplicationOutcomes
        /// </summary>
        /// <param name="id">GUID of the financialAidApplicationOutcomes to update</param>
        /// <param name="financialAidApplicationOutcomes">DTO of the updated financialAidApplicationOutcomes</param>
        /// <returns>A single financialAidApplicationOutcomes object</returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidApplicationOutcome> UpdateAsync([FromUri] string id, [FromBody] Dtos.FinancialAidApplicationOutcome financialAidApplicationOutcomes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidApplicationOutcomes
        /// </summary>
        /// <param name="id">GUID to desired financialAidApplicationOutcomes</param>
        [HttpDelete]
        public async Task DeleteAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
