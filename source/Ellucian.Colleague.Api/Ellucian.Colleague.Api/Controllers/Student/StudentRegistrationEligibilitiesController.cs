//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentRegistrationEligibilities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentRegistrationEligibilitiesController : BaseCompressedApiController
    {
        private readonly IStudentRegistrationEligibilitiesService _studentRegistrationEligibilitiesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentRegistrationEligibilitiesController class.
        /// </summary>
        /// <param name="studentRegistrationEligibilitiesService">Service of type <see cref="IStudentRegistrationEligibilitiesService">IStudentRegistrationEligibilitiesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentRegistrationEligibilitiesController(IStudentRegistrationEligibilitiesService studentRegistrationEligibilitiesService, ILogger logger)
        {
            _studentRegistrationEligibilitiesService = studentRegistrationEligibilitiesService;
            _logger = logger;
        }

        /// <summary>
        /// Return a single studentRegistrationEligibilities matching required filters of
        /// Student and Academic Period.
        /// </summary>
        /// <returns>StudentRegistrationEligibilities <see cref="Dtos.StudentRegistrationEligibilities"/> object representing matching studentRegistrationEligibilities</returns>
        [HttpGet, EedmResponseFilter, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentRegistrationEligibilities))]
        public async Task<Ellucian.Colleague.Dtos.StudentRegistrationEligibilities> GetStudentRegistrationEligibilitiesAsync(QueryStringFilter criteria)
        {
            string studentId = string.Empty, academicPeriodId = string.Empty;

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
                var criteriaObj = GetFilterObject<Dtos.StudentRegistrationEligibilities>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    studentId = criteriaObj.Student != null ? criteriaObj.Student.Id : string.Empty;
                    academicPeriodId = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new Dtos.StudentRegistrationEligibilities();

                var items = await _studentRegistrationEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(studentId, academicPeriodId, bypassCache);

                AddDataPrivacyContextProperty((await _studentRegistrationEligibilitiesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());

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
        /// Read (GET) a studentRegistrationEligibilities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentRegistrationEligibilities</param>
        /// <returns>A studentRegistrationEligibilities object <see cref="Dtos.StudentRegistrationEligibilities"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.StudentRegistrationEligibilities> GetStudentRegistrationEligibilitiesByGuidAsync(string guid)
        {
            //GET by guid is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create (POST) a new studentRegistrationEligibilities
        /// </summary>
        /// <param name="studentRegistrationEligibilities">DTO of the new studentRegistrationEligibilities</param>
        /// <returns>A studentRegistrationEligibilities object <see cref="Dtos.StudentRegistrationEligibilities"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentRegistrationEligibilities> PostStudentRegistrationEligibilitiesAsync([FromBody] Dtos.StudentRegistrationEligibilities studentRegistrationEligibilities)
        {
            // Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing studentRegistrationEligibilities
        /// </summary>
        /// <param name="guid">GUID of the studentRegistrationEligibilities to update</param>
        /// <param name="studentRegistrationEligibilities">DTO of the updated studentRegistrationEligibilities</param>
        /// <returns>A studentRegistrationEligibilities object <see cref="Dtos.StudentRegistrationEligibilities"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentRegistrationEligibilities> PutStudentRegistrationEligibilitiesAsync([FromUri] string guid, [FromBody] Dtos.StudentRegistrationEligibilities studentRegistrationEligibilities)
        {
            // Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a studentRegistrationEligibilities
        /// </summary>
        /// <param name="guid">GUID to desired studentRegistrationEligibilities</param>
        [HttpDelete]
        public async Task DeleteStudentRegistrationEligibilitiesAsync(string guid)
        {
            // Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}