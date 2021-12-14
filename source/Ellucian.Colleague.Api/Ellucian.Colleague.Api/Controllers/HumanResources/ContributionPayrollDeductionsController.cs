//Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Newtonsoft.Json;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to ContributionPayrollDeductions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class ContributionPayrollDeductionsController : BaseCompressedApiController
    {
        private readonly IContributionPayrollDeductionsService _contributionPayrollDeductionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ContributionPayrollDeductionsController class.
        /// </summary>
        /// <param name="contributionPayrollDeductionsService">Service of type <see cref="IContributionPayrollDeductionsService">IContributionPayrollDeductionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ContributionPayrollDeductionsController(IContributionPayrollDeductionsService contributionPayrollDeductionsService, ILogger logger)
        {
            _contributionPayrollDeductionsService = contributionPayrollDeductionsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all contributionPayrollDeductions
        /// </summary>
        /// <returns>List of ContributionPayrollDeductions <see cref="Dtos.ContributionPayrollDeductions"/> objects representing matching contributionPayrollDeductions</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.ViewContributionPayrollDeductions)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.ContributionPayrollDeductions))]
        public async Task<IHttpActionResult> GetContributionPayrollDeductionsAsync(Paging page, QueryStringFilter criteria)
        {
            string arrangement = string.Empty;

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
                _contributionPayrollDeductionsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.ContributionPayrollDeductions>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    arrangement = criteriaObj.Arrangement != null ? criteriaObj.Arrangement.Id : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.ContributionPayrollDeductions>>(new List<Dtos.ContributionPayrollDeductions>(), page, 0, this.Request);

                var pageOfItems = await _contributionPayrollDeductionsService.GetContributionPayrollDeductionsAsync(page.Offset, page.Limit, arrangement, bypassCache);

                AddEthosContextProperties(
                    await _contributionPayrollDeductionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _contributionPayrollDeductionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (JsonSerializationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch
                (KeyNotFoundException e)
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
        /// Read (GET) a contributionPayrollDeductions using a GUID
        /// </summary>
        /// <param name="id">GUID to desired contributionPayrollDeductions</param>
        /// <returns>A contributionPayrollDeductions object <see cref="Dtos.ContributionPayrollDeductions"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter, PermissionsFilter(HumanResourcesPermissionCodes.ViewContributionPayrollDeductions)]
        public async Task<Dtos.ContributionPayrollDeductions> GetContributionPayrollDeductionsByIdAsync([FromUri]string id)
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
                _contributionPayrollDeductionsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _contributionPayrollDeductionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _contributionPayrollDeductionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));
                return await _contributionPayrollDeductionsService.GetContributionPayrollDeductionsByGuidAsync(id);
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
        /// Create (POST) a new contributionPayrollDeductions
        /// </summary>
        /// <param name="contributionPayrollDeductions">DTO of the new contributionPayrollDeductions</param>
        /// <returns>A contributionPayrollDeductions object <see cref="Dtos.ContributionPayrollDeductions"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.ContributionPayrollDeductions> PostContributionPayrollDeductionsAsync([FromBody] Dtos.ContributionPayrollDeductions contributionPayrollDeductions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing contributionPayrollDeductions
        /// </summary>
        /// <param name="id">GUID of the contributionPayrollDeductions to update</param>
        /// <param name="contributionPayrollDeductions">DTO of the updated contributionPayrollDeductions</param>
        /// <returns>A contributionPayrollDeductions object <see cref="Dtos.ContributionPayrollDeductions"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.ContributionPayrollDeductions> PutContributionPayrollDeductionsAsync([FromUri] string id, [FromBody] Dtos.ContributionPayrollDeductions contributionPayrollDeductions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a contributionPayrollDeductions
        /// </summary>
        /// <param name="id">GUID to desired contributionPayrollDeductions</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task DeleteContributionPayrollDeductionsAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}