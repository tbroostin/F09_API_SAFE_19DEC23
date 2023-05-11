// Copyright 2018-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides information about student attendances data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentSectionAttendancesController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IStudentSectionAttendancesService _studentSectionAttendancesService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentSectionAttendancesService"></param>
        /// <param name="logger"></param>
        public StudentSectionAttendancesController(IStudentSectionAttendancesService studentSectionAttendancesService, ILogger logger)
        {
            _studentSectionAttendancesService = studentSectionAttendancesService;
            _logger = logger;
        }

        /// <summary>
        /// Query by post method used to get student attendance information based on criteria.
        /// Criteria must have a studentId. SectionIds in criteria is optional.
        ///This returns student attendances for the given studentId and for the sections provided.
        ///If no section is provided in criteria then by default attendances for all the sections that belong to given studentId are returned.
        /// </summary>
        /// <param name="criteria">Object containing the studentId and then section for which attendances are requested.</param>
        /// <returns><see cref="StudentAttendance">Student Attendance</see> DTOs.</returns>
        /// <accessComments>Only an authenticated user can retrieve its own attendances.</accessComments>
        [HttpPost]
        public async Task<StudentSectionsAttendances> QueryStudentSectionAttendancesAsync(StudentSectionAttendancesQueryCriteria criteria)
        {
            if (criteria == null || string.IsNullOrEmpty(criteria.StudentId))
            {
                string errorText = "criteria must contain a StudentId";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            
            try
            {
                return await _studentSectionAttendancesService.QueryStudentSectionAttendancesAsync(criteria);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                _logger.Info(pe.ToString());
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}