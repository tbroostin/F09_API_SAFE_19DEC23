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
    /// Provides access to AttendanceCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AttendanceCategoriesController : BaseCompressedApiController
    {
        private readonly IAttendanceCategoriesService _attendanceCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AttendanceCategoriesController class.
        /// </summary>
        /// <param name="attendanceCategoriesService">Service of type <see cref="IAttendanceCategoriesService">IAttendanceCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AttendanceCategoriesController(IAttendanceCategoriesService attendanceCategoriesService, ILogger logger)
        {
            _attendanceCategoriesService = attendanceCategoriesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all attendanceCategories
        /// </summary>
        /// <returns>List of AttendanceCategories <see cref="Dtos.AttendanceCategories"/> objects representing matching attendanceCategories</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AttendanceCategories>> GetAttendanceCategoriesAsync()
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
                return await _attendanceCategoriesService.GetAttendanceCategoriesAsync(bypassCache);
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
        /// Read (GET) a attendanceCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired attendanceCategories</param>
        /// <returns>A attendanceCategories object <see cref="Dtos.AttendanceCategories"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.AttendanceCategories> GetAttendanceCategoriesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _attendanceCategoriesService.GetAttendanceCategoriesByGuidAsync(guid);
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
        /// Create (POST) a new attendanceCategories
        /// </summary>
        /// <param name="attendanceCategories">DTO of the new attendanceCategories</param>
        /// <returns>A attendanceCategories object <see cref="Dtos.AttendanceCategories"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AttendanceCategories> PostAttendanceCategoriesAsync([FromBody] Dtos.AttendanceCategories attendanceCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing attendanceCategories
        /// </summary>
        /// <param name="guid">GUID of the attendanceCategories to update</param>
        /// <param name="attendanceCategories">DTO of the updated attendanceCategories</param>
        /// <returns>A attendanceCategories object <see cref="Dtos.AttendanceCategories"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AttendanceCategories> PutAttendanceCategoriesAsync([FromUri] string guid, [FromBody] Dtos.AttendanceCategories attendanceCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a attendanceCategories
        /// </summary>
        /// <param name="guid">GUID to desired attendanceCategories</param>
        [HttpDelete]
        public async Task DeleteAttendanceCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}