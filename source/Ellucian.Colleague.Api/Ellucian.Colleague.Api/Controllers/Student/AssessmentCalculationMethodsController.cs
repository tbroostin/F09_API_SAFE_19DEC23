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
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AssessmentCalculationMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AssessmentCalculationMethodsController : BaseCompressedApiController
    {
        private readonly IAssessmentCalculationMethodsService _assessmentCalculationMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AssessmentCalculationMethodsController class.
        /// </summary>
        /// <param name="assessmentCalculationMethodsService">Service of type <see cref="IAssessmentCalculationMethodsService">IAssessmentCalculationMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AssessmentCalculationMethodsController(IAssessmentCalculationMethodsService assessmentCalculationMethodsService, ILogger logger)
        {
            _assessmentCalculationMethodsService = assessmentCalculationMethodsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all assessmentCalculationMethods
        /// </summary>
        /// <returns>List of AssessmentCalculationMethods <see cref="Dtos.AssessmentCalculationMethods"/> objects representing matching assessmentCalculationMethods</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentCalculationMethods>> GetAssessmentCalculationMethodsAsync()
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
                return await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsAsync(bypassCache);
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
        /// Read (GET) a assessmentCalculationMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired assessmentCalculationMethods</param>
        /// <returns>A assessmentCalculationMethods object <see cref="Dtos.AssessmentCalculationMethods"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.AssessmentCalculationMethods> GetAssessmentCalculationMethodsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _assessmentCalculationMethodsService.GetAssessmentCalculationMethodsByGuidAsync(guid);
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
        /// Create (POST) a new assessmentCalculationMethods
        /// </summary>
        /// <param name="assessmentCalculationMethods">DTO of the new assessmentCalculationMethods</param>
        /// <returns>A assessmentCalculationMethods object <see cref="Dtos.AssessmentCalculationMethods"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AssessmentCalculationMethods> PostAssessmentCalculationMethodsAsync([FromBody] Dtos.AssessmentCalculationMethods assessmentCalculationMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing assessmentCalculationMethods
        /// </summary>
        /// <param name="guid">GUID of the assessmentCalculationMethods to update</param>
        /// <param name="assessmentCalculationMethods">DTO of the updated assessmentCalculationMethods</param>
        /// <returns>A assessmentCalculationMethods object <see cref="Dtos.AssessmentCalculationMethods"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AssessmentCalculationMethods> PutAssessmentCalculationMethodsAsync([FromUri] string guid, [FromBody] Dtos.AssessmentCalculationMethods assessmentCalculationMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a assessmentCalculationMethods
        /// </summary>
        /// <param name="guid">GUID to desired assessmentCalculationMethods</param>
        [HttpDelete]
        public async Task DeleteAssessmentCalculationMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}