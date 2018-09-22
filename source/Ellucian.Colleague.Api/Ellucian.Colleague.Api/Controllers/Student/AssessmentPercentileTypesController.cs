



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
    /// Provides access to AssessmentPercentileTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AssessmentPercentileTypesController : BaseCompressedApiController
    {
        private readonly IAssessmentPercentileTypesService _assessmentPercentileTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AssessmentPercentileTypesController class.
        /// </summary>
        /// <param name="assessmentPercentileTypesService">Service of type <see cref="IAssessmentPercentileTypesService">IAssessmentPercentileTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AssessmentPercentileTypesController(IAssessmentPercentileTypesService assessmentPercentileTypesService, ILogger logger)
        {
            _assessmentPercentileTypesService = assessmentPercentileTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all assessmentPercentileTypes
        /// </summary>
        /// <returns>List of AssessmentPercentileTypes <see cref="Dtos.AssessmentPercentileTypes"/> objects representing matching assessmentPercentileTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentPercentileTypes>> GetAssessmentPercentileTypesAsync()
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
                return await _assessmentPercentileTypesService.GetAssessmentPercentileTypesAsync(bypassCache);
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
        /// Read (GET) a assessmentPercentileTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired assessmentPercentileTypes</param>
        /// <returns>A assessmentPercentileTypes object <see cref="Dtos.AssessmentPercentileTypes"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.AssessmentPercentileTypes> GetAssessmentPercentileTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _assessmentPercentileTypesService.GetAssessmentPercentileTypesByGuidAsync(guid);
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
        /// Create (POST) a new assessmentPercentileTypes
        /// </summary>
        /// <param name="assessmentPercentileTypes">DTO of the new assessmentPercentileTypes</param>
        /// <returns>A assessmentPercentileTypes object <see cref="Dtos.AssessmentPercentileTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AssessmentPercentileTypes> PostAssessmentPercentileTypesAsync([FromBody] Dtos.AssessmentPercentileTypes assessmentPercentileTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing assessmentPercentileTypes
        /// </summary>
        /// <param name="guid">GUID of the assessmentPercentileTypes to update</param>
        /// <param name="assessmentPercentileTypes">DTO of the updated assessmentPercentileTypes</param>
        /// <returns>A assessmentPercentileTypes object <see cref="Dtos.AssessmentPercentileTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AssessmentPercentileTypes> PutAssessmentPercentileTypesAsync([FromUri] string guid, [FromBody] Dtos.AssessmentPercentileTypes assessmentPercentileTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a assessmentPercentileTypes
        /// </summary>
        /// <param name="guid">GUID to desired assessmentPercentileTypes</param>
        [HttpDelete]
        public async Task DeleteAssessmentPercentileTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}