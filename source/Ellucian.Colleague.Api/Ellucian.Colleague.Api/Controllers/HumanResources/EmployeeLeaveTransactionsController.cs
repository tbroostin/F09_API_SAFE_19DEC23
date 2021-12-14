//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmployeeLeaveTransactions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeLeaveTransactionsController : BaseCompressedApiController
    {
        private readonly IEmployeeLeaveTransactionsService _employeeLeaveTransactionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmployeeLeaveTransactionsController class.
        /// </summary>
        /// <param name="employeeLeaveTransactionsService">Service of type <see cref="IEmployeeLeaveTransactionsService">IEmployeeLeaveTransactionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmployeeLeaveTransactionsController(IEmployeeLeaveTransactionsService employeeLeaveTransactionsService, ILogger logger)
        {
            _employeeLeaveTransactionsService = employeeLeaveTransactionsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all employeeLeaveTransactions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of EmployeeLeaveTransactions <see cref="Dtos.EmployeeLeaveTransactions"/> objects representing matching employeeLeaveTransactions</returns>
        [HttpGet, EedmResponseFilter, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetEmployeeLeaveTransactionsAsync(Paging page)
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
                _employeeLeaveTransactionsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                var pageOfItems = await _employeeLeaveTransactionsService.GetEmployeeLeaveTransactionsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _employeeLeaveTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employeeLeaveTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.EmployeeLeaveTransactions>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a employeeLeaveTransactions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeaveTransactions</param>
        /// <returns>A employeeLeaveTransactions object <see cref="Dtos.EmployeeLeaveTransactions"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions)]
        public async Task<Dtos.EmployeeLeaveTransactions> GetEmployeeLeaveTransactionsByGuidAsync(string guid)
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
                _employeeLeaveTransactionsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _employeeLeaveTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _employeeLeaveTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _employeeLeaveTransactionsService.GetEmployeeLeaveTransactionsByGuidAsync(guid);
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
        /// Create (POST) a new employeeLeaveTransactions
        /// </summary>
        /// <param name="employeeLeaveTransactions">DTO of the new employeeLeaveTransactions</param>
        /// <returns>A employeeLeaveTransactions object <see cref="Dtos.EmployeeLeaveTransactions"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.EmployeeLeaveTransactions> PostEmployeeLeaveTransactionsAsync([FromBody] Dtos.EmployeeLeaveTransactions employeeLeaveTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employeeLeaveTransactions
        /// </summary>
        /// <param name="guid">GUID of the employeeLeaveTransactions to update</param>
        /// <param name="employeeLeaveTransactions">DTO of the updated employeeLeaveTransactions</param>
        /// <returns>A employeeLeaveTransactions object <see cref="Dtos.EmployeeLeaveTransactions"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.EmployeeLeaveTransactions> PutEmployeeLeaveTransactionsAsync([FromUri] string guid, [FromBody] Dtos.EmployeeLeaveTransactions employeeLeaveTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employeeLeaveTransactions
        /// </summary>
        /// <param name="guid">GUID to desired employeeLeaveTransactions</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task DeleteEmployeeLeaveTransactionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}