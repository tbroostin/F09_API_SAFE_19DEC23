//Copyright 2017 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentPerformanceReviewTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentPerformanceReviewTypesController : BaseCompressedApiController
    {
        private readonly IEmploymentPerformanceReviewTypesService _employmentPerformanceReviewTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentPerformanceReviewTypesController class.
        /// </summary>
        /// <param name="employmentPerformanceReviewTypesService">Service of type <see cref="IEmploymentPerformanceReviewTypesService">IEmploymentPerformanceReviewTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmploymentPerformanceReviewTypesController(IEmploymentPerformanceReviewTypesService employmentPerformanceReviewTypesService, ILogger logger)
        {
            _employmentPerformanceReviewTypesService = employmentPerformanceReviewTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all employmentPerformanceReviewTypes
        /// </summary>
                /// <returns>List of EmploymentPerformanceReviewTypes <see cref="Dtos.EmploymentPerformanceReviewTypes"/> objects representing matching employmentPerformanceReviewTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes>> GetEmploymentPerformanceReviewTypesAsync()
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
            {                    return await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesAsync(bypassCache);
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
        /// Read (GET) a employmentPerformanceReviewTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewTypes</param>
        /// <returns>A employmentPerformanceReviewTypes object <see cref="Dtos.EmploymentPerformanceReviewTypes"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.EmploymentPerformanceReviewTypes> GetEmploymentPerformanceReviewTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesByGuidAsync(guid);
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
        /// Create (POST) a new employmentPerformanceReviewTypes
        /// </summary>
        /// <param name="employmentPerformanceReviewTypes">DTO of the new employmentPerformanceReviewTypes</param>
        /// <returns>A employmentPerformanceReviewTypes object <see cref="Dtos.EmploymentPerformanceReviewTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EmploymentPerformanceReviewTypes> PostEmploymentPerformanceReviewTypesAsync([FromBody] Dtos.EmploymentPerformanceReviewTypes employmentPerformanceReviewTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentPerformanceReviewTypes
        /// </summary>
        /// <param name="guid">GUID of the employmentPerformanceReviewTypes to update</param>
        /// <param name="employmentPerformanceReviewTypes">DTO of the updated employmentPerformanceReviewTypes</param>
        /// <returns>A employmentPerformanceReviewTypes object <see cref="Dtos.EmploymentPerformanceReviewTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EmploymentPerformanceReviewTypes> PutEmploymentPerformanceReviewTypesAsync([FromUri] string guid, [FromBody] Dtos.EmploymentPerformanceReviewTypes employmentPerformanceReviewTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentPerformanceReviewTypes
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewTypes</param>
        [HttpDelete]
        public async Task DeleteEmploymentPerformanceReviewTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}