// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Threading.Tasks;
using System;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AssessmentSpecialCircumstances data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AssessmentSpecialCircumstancesController : BaseCompressedApiController
    {
        private readonly ICurriculumService _curriculumService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AssessmentSpecialCircumstancesController class.
        /// </summary>
        /// <param name="curriculumService">Repository of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AssessmentSpecialCircumstancesController(ICurriculumService curriculumService, ILogger logger)
        {
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all assessment special circumstances.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All AssessmentSpecialCircumstance objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstancesAsync()
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return await _curriculumService.GetAssessmentSpecialCircumstancesAsync(bypassCache);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Assessment special circumstances by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see>object.</returns>
        [HttpGet]
        public async Task<AssessmentSpecialCircumstance> GetAssessmentSpecialCircumstanceByIdAsync(string id)
        {
            try
            {
                return await _curriculumService.GetAssessmentSpecialCircumstanceByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Creates a Assessment Special Circumstance.
        /// </summary>
        /// <param name="assessmentSpecialCircumstance"><see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see></returns>
        [HttpPost]
        public async Task<Dtos.AssessmentSpecialCircumstance> PostAssessmentSpecialCircumstanceAsync([FromBody] Dtos.AssessmentSpecialCircumstance assessmentSpecialCircumstance)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Assessment Special Circumstance.
        /// </summary>
        /// <param name="id">Id of the Assessment Special Circumstance to update</param>
        /// <param name="assessmentSpecialCircumstance"><see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AssessmentSpecialCircumstance">AssessmentSpecialCircumstance</see></returns>
        [HttpPut]
        public async Task<Dtos.AssessmentSpecialCircumstance> PutAssessmentSpecialCircumstanceAsync([FromUri] string id, [FromBody] Dtos.AssessmentSpecialCircumstance assessmentSpecialCircumstance)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Assessment Special Circumstance.
        /// </summary>
        /// <param name="id">ID of the Assessment Special Circumstance to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteAssessmentSpecialCircumstanceAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}