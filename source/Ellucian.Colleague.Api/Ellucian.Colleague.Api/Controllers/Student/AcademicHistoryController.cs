// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
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
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

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
        /// <accessComments>
        /// A person may only query academic history if they have VIEW.STUDENT.INFORMATION permission.
        /// </accessComments>
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
        /// <accessComments>
        /// A person may only query academic history if they have VIEW.STUDENT.INFORMATION permission.
        /// </accessComments>
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
        /// <accessComments>
        /// A person may only query academic history levels if they have VIEW.STUDENT.INFORMATION permission.
        /// </accessComments>
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
        /// <accessComments>
        /// A person may only query academic history levels if they have VIEW.STUDENT.INFORMATION permission.
        /// </accessComments>
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
        /// <accessComments>
        /// A person may only query academic history levels if they have VIEW.STUDENT.INFORMATION permission.
        /// </accessComments>
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
        /// Term: (Optional) Term filter for academic history.
        /// IncludeStudentSections: (Optional) If true, credits are returned as student sections.</param>
        /// <returns>PilotAcademicHistoryLevel DTO Objects</returns>
        /// <accessComments>Users with the VIEW.STUDENT.INFORMATION permission can query students' academic history levels.</accessComments>
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
        /// <accessComments>User with permission of VIEW.STUDENT.INFORMATION can validate existing Student Enrollment.</accessComments>
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
        /// <accessComments>
        /// The faculty assigned to a section may query academic credits for their sections. 
        /// </accessComments>
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
        /// <accessComments>
        /// The faculty assigned to a section may query academic credits for their sections. 
        /// </accessComments>
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

        /// <summary>
        /// Get Academic Credits with Invalid Keys for the list of sections.
        /// This returns collection of Academic Credits for the given sections and list of Invalid Academic Credit Ids that were not found in a file.
        /// </summary>
        /// <remarks>If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database; otherwise, cached data is returned from the repository.</remarks>
        /// <param name="criteria">Contains selection criteria:
        /// Section Ids: List of section IDs. Must include at least 1.
        /// CreditStatuses: (Optional) If no statuses are specified all statuses will be included.</param>
        /// <returns><see cref="AcademicCreditsWithInvalidKeys">Academic Credit with Invalid Keys</see> DTO objects. </returns>
        /// <accessComments>
        /// 1. The faculty assigned to a section may query academic credits for their sections.
        /// 2. A departmental oversight member assigned to the section may query academic credits for their sections with any of the following permission codes
        /// VIEW.SECTION.ROSTER
        /// VIEW.SECTION.GRADING
        /// CREATE.SECTION.GRADING
        /// VIEW.SECTION.DROP.ROSTER
        /// CREATE.SECTION.DROP.STUDENT
        /// VIEW.SECTION.CENSUS
        /// CREATE.SECTION.CENSUS
        /// </accessComments>
        [HttpPost]
        public async Task<AcademicCreditsWithInvalidKeys> QueryAcademicCreditsWithInvalidKeysAsync([FromBody] AcademicCreditQueryCriteria criteria)
        {
            try
            {
                bool useCache = true;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }
                AcademicCreditsWithInvalidKeys academicCreditsWithInvalidKeys = await _academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria, useCache);
                return academicCreditsWithInvalidKeys;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, "Session has expired while retrieving academic credit details.");
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                var message = "User does not have appropriate permissions to retrieve academic credit details.";
                _logger.Error(pex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException aex)
            {
                var message = "Must supply a criteria to retrieve academic credit details.";
                _logger.Error(aex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                var message = "An error occurred while retrieving academic credit details";
                _logger.Error(ex ,message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}
