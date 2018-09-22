﻿// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicHistory data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicHistoryController : BaseCompressedApiController
    {
        private readonly IAcademicHistoryService _academicHistoryService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CoursesController class.
        /// </summary>
        /// <param name="service">Service of type <see cref="ICourseService">ICourseService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AcademicHistoryController(IAcademicHistoryService service, ILogger logger)
        {
            _academicHistoryService = service;
            _logger = logger;
        }

        /// <summary>
        /// get Academic History from a list of Student Ids
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Student Ids: List of IDs.
        /// BestFit: (Optional) If true, non-term credit is fitted into terms based on dates.
        /// Filter: (Optional) If true, then filter to only active credits.
        /// Term: (Optional) Term filter for academic history</param>
        /// <returns>AcademicHistory DTO Objects</returns>
        [HttpPost]
        [Obsolete("Obsolete as of API version 1.18, use QueryAcademicHistory2Async instead")]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicHistoryBatch>> QueryAcademicHistoryAsync([FromBody] AcademicHistoryQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryAcademicHistoryAsync(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                // Provide a more descriptive message
                var message = "Could not Query Academic History Level.  See Logging for more details.  Exception thrown: " + e.Message;
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// get Academic History from a list of Student Ids
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Student Ids: List of IDs.
        /// BestFit: (Optional) If true, non-term credit is fitted into terms based on dates.
        /// Filter: (Optional) If true, then filter to only active credits.
        /// Term: (Optional) Term filter for academic history</param>
        /// <returns><see cref="AcademicHistoryBatch2"/> DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicHistoryBatch2>> QueryAcademicHistory2Async([FromBody] AcademicHistoryQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryAcademicHistory2Async(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                // Provide a more descriptive message
                var message = "Could not Query Academic History Level.  See Logging for more details.  Exception thrown: " + e.Message;
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get Academic History for a specific Academic Level from a list of Student Ids.
        /// Academic Level is wrapped around Academic History therefore giving a picture
        /// of only those AcademicCredits which are within the same level.
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Student Ids: List of IDs.
        /// BestFit: (Optional) If true, non-term credit is fitted into terms based on dates.
        /// Filter: (Optional) If true, then filter to only active credits.
        /// Term: (Optional) Term filter for academic history</param>
        /// <returns>AcademicHistoryLevel DTO Objects</returns>
        [Obsolete("Obsolete as of Api version 1.10, use version 2 of this API")]
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicHistoryLevel>> QueryAcademicHistoryLevelAsync([FromBody] AcademicHistoryQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryAcademicHistoryLevelAsync(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                // Provide a more descriptive message
                var message = "Could not Query Academic History Level.  See Logging for more details.  Exception thrown: " + e.Message;
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get Academic History for a specific Academic Level from a list of Student Ids.
        /// Academic Level is wrapped around Academic History therefore giving a picture
        /// of only those AcademicCredits which are within the same level.
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Student Ids: List of IDs.
        /// BestFit: (Optional) If true, non-term credit is fitted into terms based on dates.
        /// Filter: (Optional) If true, then filter to only active credits.
        /// Term: (Optional) Term filter for academic history</param>
        /// <returns>AcademicHistoryLevel2 DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicHistoryLevel2>> QueryAcademicHistoryLevel2Async([FromBody] AcademicHistoryQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryAcademicHistoryLevel2Async(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                // Provide a more descriptive message
                var message = "Could not Query Academic History Level.  See Logging for more details.  Exception thrown: " + e.Message;
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get Academic History for a specific Academic Level from a list of Student Ids.
        /// Academic Level is wrapped around Academic History therefore giving a picture
        /// of only those AcademicCredits which are within the same level.
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Student Ids: List of IDs.
        /// BestFit: (Optional) If true, non-term credit is fitted into terms based on dates.
        /// Filter: (Optional) If true, then filter to only active credits.
        /// Term: (Optional) Term filter for academic history</param>
        /// <returns>AcademicHistoryLevel2 DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicHistoryLevel3>> QueryAcademicHistoryLevel3Async([FromBody] AcademicHistoryQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryAcademicHistoryLevel3Async(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                // Provide a more descriptive message
                var message = "Could not Query Academic History Level.  See Logging for more details.  Exception thrown: " + e.Message;
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get Academic History for a specific Academic Level from a list of Student Ids.
        /// Academic Level is wrapped around Academic History therefore giving a picture
        /// of only those AcademicCredits which are within the same level.
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Student Ids: List of IDs.
        /// BestFit: (Optional) If true, non-term credit is fitted into terms based on dates.
        /// Filter: (Optional) If true, then filter to only active credits.
        /// Term: (Optional) Term filter for academic history</param>
        /// <returns>PilotAcademicHistoryLevel DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.PilotAcademicHistoryLevel>> QueryPilotAcademicHistoryLevelAsync([FromBody] AcademicHistoryQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryPilotAcademicHistoryLevelAsync(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                // Provide a more descriptive message
                var message = "Could not Query Academic History Level.  See Logging for more details.  Exception thrown: " + e.Message;
                throw CreateHttpResponseException(message);
            }
        }


        /// <summary>
        /// Validate existing student Enrollment by passing in a list of keys for each student and returning
        /// a list of keys which are either invalid.
        /// </summary>
        /// <param name="enrollmentKeys">Student Enrollment key structure and return structure<see cref="StudentEnrollment">StudentEnrollment</see></param>
        /// <returns>List of StudentEnrollment DTOs</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentEnrollment>> GetInvalidStudentEnrollmentAsync([FromBody] IEnumerable<StudentEnrollment> enrollmentKeys)
        {
            try
            {
                return await _academicHistoryService.GetInvalidStudentEnrollmentAsync(enrollmentKeys);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                // Provide a more descriptive message
                var message = "Could not get Invalid Student Enrollments.  See Logging for more details.  Exception thrown: " + e.Message;
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get Academic Credits for a list of sections
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Section Ids: List of section IDs. Must include at least 1.
        /// CreditStatuses: (Optional) If no statuses are specified all statuses will be included.</param>
        /// <returns>List of <see cref="AcademicCredit2">Academic Credit</see> DTO objects. </returns>
        [HttpPost]
        [Obsolete("Obsolete as of API version 1.18, use QueryAcademicCredits2Async instead")]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicCredit2>> QueryAcademicCreditsAsync([FromBody] AcademicCreditQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryAcademicCreditsAsync(criteria);
            }
            catch (ArgumentNullException aex)
            {
                _logger.Error(aex.Message);
                throw CreateHttpResponseException(aex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get Academic Credits for a list of sections
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// Section Ids: List of section IDs. Must include at least 1.
        /// CreditStatuses: (Optional) If no statuses are specified all statuses will be included.</param>
        /// <returns>List of <see cref="AcademicCredit3">Academic Credit</see> DTO objects. </returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicCredit3>> QueryAcademicCredits2Async([FromBody] AcademicCreditQueryCriteria criteria)
        {
            try
            {
                return await _academicHistoryService.QueryAcademicCredits2Async(criteria);
            }
            catch (ArgumentNullException aex)
            {
                _logger.Error(aex.Message);
                throw CreateHttpResponseException(aex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
