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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to InstructorCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructorCategoriesController : BaseCompressedApiController
    {
        private readonly IInstructorCategoriesService _instructorCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorCategoriesController class.
        /// </summary>
        /// <param name="instructorCategoriesService">Service of type <see cref="IInstructorCategoriesService">IInstructorCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public InstructorCategoriesController(IInstructorCategoriesService instructorCategoriesService, ILogger logger)
        {
            _instructorCategoriesService = instructorCategoriesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all instructorCategories
        /// </summary>
        /// <returns>List of InstructorCategories <see cref="Dtos.InstructorCategories"/> objects representing matching instructorCategories</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorCategories>> GetInstructorCategoriesAsync()
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
                return await _instructorCategoriesService.GetInstructorCategoriesAsync(bypassCache);
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
        /// Read (GET) a instructorCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructorCategories</param>
        /// <returns>A instructorCategories object <see cref="Dtos.InstructorCategories"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.InstructorCategories> GetInstructorCategoriesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _instructorCategoriesService.GetInstructorCategoriesByGuidAsync(guid);
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
        /// Create (POST) a new instructorCategories
        /// </summary>
        /// <param name="instructorCategories">DTO of the new instructorCategories</param>
        /// <returns>A instructorCategories object <see cref="Dtos.InstructorCategories"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.InstructorCategories> PostInstructorCategoriesAsync([FromBody] Dtos.InstructorCategories instructorCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructorCategories
        /// </summary>
        /// <param name="guid">GUID of the instructorCategories to update</param>
        /// <param name="instructorCategories">DTO of the updated instructorCategories</param>
        /// <returns>A instructorCategories object <see cref="Dtos.InstructorCategories"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.InstructorCategories> PutInstructorCategoriesAsync([FromUri] string guid, [FromBody] Dtos.InstructorCategories instructorCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructorCategories
        /// </summary>
        /// <param name="guid">GUID to desired instructorCategories</param>
        [HttpDelete]
        public async Task DeleteInstructorCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}