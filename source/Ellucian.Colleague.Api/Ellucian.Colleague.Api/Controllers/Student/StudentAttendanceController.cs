// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides information about student attendances data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAttendanceController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IStudentAttendanceService _studentAttendanceService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentAttendanceService"></param>
        /// <param name="logger"></param>
        public StudentAttendanceController(IStudentAttendanceService studentAttendanceService, ILogger logger)
        {
            _studentAttendanceService = studentAttendanceService;
            _logger = logger;
        }

        /// <summary>
        /// Query by post method used to get student attendance information based on criteria
        /// </summary>
        /// <remarks>If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database; otherwise, cached data is returned from the repository.</remarks>
        /// <param name="criteria">Object containing the section for which attendances are requested and other parameter choices.</param>
        /// <returns><see cref="StudentAttendance">Student Attendance</see> DTOs.</returns>
        /// <accessComments>Only a faculty user who is assigned to the requested course section can view student attendance data for that course section</accessComments>
        [HttpPost]
        public async Task<IEnumerable<StudentAttendance>> QueryStudentAttendancesAsync(StudentAttendanceQueryCriteria criteria)
        {
            if (criteria == null || string.IsNullOrEmpty(criteria.SectionId))
            {
                string errorText = "criteria must contain a SectionId";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
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
                return await _studentAttendanceService.QueryStudentAttendancesAsync(criteria, useCache);
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

        /// <summary>
        /// Add new or update an existing student attendance for a student, section and meeting instance.
        /// </summary>
        /// <param name="studentAttendance">Object containing the section for which attendances are requested and other parameter choices.</param>
        /// <returns>Updated <see cref="StudentAttendance">Student Attendance</see> DTO.</returns>
        /// <accessComments>Only a faculty user who is assigned to the requested course section can update student attendance data for that course section</accessComments>
        [HttpPut]
        public async Task<StudentAttendance> PutStudentAttendanceAsync([FromBody] StudentAttendance studentAttendance)
        {
            if (studentAttendance == null)
            {
                string errorText = "Must provide the student attendance to update.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentAttendanceService.UpdateStudentAttendanceAsync(studentAttendance);
            }
            catch (PermissionsException pe)
            {
                _logger.Info(pe.ToString());
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (RecordLockException re)
            {
                _logger.Info(re.ToString());
                throw CreateHttpResponseException(re.Message, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }
    }

}
