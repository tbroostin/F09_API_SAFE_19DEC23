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
    /// Provides access to EmploymentPerformanceReviewRatings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentPerformanceReviewRatingsController : BaseCompressedApiController
    {
        private readonly IEmploymentPerformanceReviewRatingsService _employmentPerformanceReviewRatingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentPerformanceReviewRatingsController class.
        /// </summary>
        /// <param name="employmentPerformanceReviewRatingsService">Service of type <see cref="IEmploymentPerformanceReviewRatingsService">IEmploymentPerformanceReviewRatingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmploymentPerformanceReviewRatingsController(IEmploymentPerformanceReviewRatingsService employmentPerformanceReviewRatingsService, ILogger logger)
        {
            _employmentPerformanceReviewRatingsService = employmentPerformanceReviewRatingsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all employmentPerformanceReviewRatings
        /// </summary>
        /// <returns>List of EmploymentPerformanceReviewRatings <see cref="Dtos.EmploymentPerformanceReviewRatings"/> objects representing matching employmentPerformanceReviewRatings</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings>> GetEmploymentPerformanceReviewRatingsAsync()
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
                return await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsAsync(bypassCache);
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
        /// Read (GET) a employmentPerformanceReviewRatings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewRatings</param>
        /// <returns>A employmentPerformanceReviewRatings object <see cref="Dtos.EmploymentPerformanceReviewRatings"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.EmploymentPerformanceReviewRatings> GetEmploymentPerformanceReviewRatingsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsByGuidAsync(guid);
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
        /// Create (POST) a new employmentPerformanceReviewRatings
        /// </summary>
        /// <param name="employmentPerformanceReviewRatings">DTO of the new employmentPerformanceReviewRatings</param>
        /// <returns>A employmentPerformanceReviewRatings object <see cref="Dtos.EmploymentPerformanceReviewRatings"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EmploymentPerformanceReviewRatings> PostEmploymentPerformanceReviewRatingsAsync([FromBody] Dtos.EmploymentPerformanceReviewRatings employmentPerformanceReviewRatings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentPerformanceReviewRatings
        /// </summary>
        /// <param name="guid">GUID of the employmentPerformanceReviewRatings to update</param>
        /// <param name="employmentPerformanceReviewRatings">DTO of the updated employmentPerformanceReviewRatings</param>
        /// <returns>A employmentPerformanceReviewRatings object <see cref="Dtos.EmploymentPerformanceReviewRatings"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EmploymentPerformanceReviewRatings> PutEmploymentPerformanceReviewRatingsAsync([FromUri] string guid, [FromBody] Dtos.EmploymentPerformanceReviewRatings employmentPerformanceReviewRatings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentPerformanceReviewRatings
        /// </summary>
        /// <param name="guid">GUID to desired employmentPerformanceReviewRatings</param>
        [HttpDelete]
        public async Task DeleteEmploymentPerformanceReviewRatingsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}