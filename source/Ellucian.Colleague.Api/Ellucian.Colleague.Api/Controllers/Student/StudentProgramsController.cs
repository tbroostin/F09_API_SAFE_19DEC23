// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
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
    public class StudentProgramsController : BaseCompressedApiController
    {
        private readonly IStudentProgramService _studentProgramService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CoursesController class.
        /// </summary>
        /// <param name="service">Service of type <see cref="IStudentProgramService">IStudentProgramService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentProgramsController(IStudentProgramService service, ILogger logger)
        {
            _studentProgramService = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves <see cref="Dtos.Student.StudentProgram2">student programs</see> for the specified student IDs
        /// </summary>
        /// <param name="criteria">Criteria for retrieving student program information</param>
        /// <returns>List of <see cref="Dtos.Student.StudentProgram2">student programs</see></returns>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes may view student program information for any student:
        /// 
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// VIEW.STUDENT.INFORMATION
        /// 
        /// An authenticated user with any of the following permission codes who does not have one of the permission codes above may view student program information for assigned advisees only:
        /// 
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentProgram2>> QueryStudentProgramsAsync([FromBody] StudentProgramsQueryCriteria criteria)
        {
            if (criteria.StudentIds == null)
            {
                _logger.Error("Invalid studentIds");
                throw CreateHttpResponseException("At least one Student Id is required.", HttpStatusCode.BadRequest);
            }
            if (!criteria.IncludeInactivePrograms && criteria.IncludeHistory)
            {
                _logger.Error("Conflict between IncludeInactivePrograms and IncludeHistory parameters.");
                throw CreateHttpResponseException("Cannot exclude inactive programs when requesting historical data.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentProgramService.GetStudentProgramsByIdsAsync(criteria.StudentIds, criteria.IncludeInactivePrograms, criteria.Term, criteria.IncludeHistory);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Create a new academic program for a student.
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAcademicProgram">Information for adding a academic program for student</param>
        /// <returns><see cref="Dtos.Student.StudentProgram2"> Newly created student program</see></returns>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes can add student program
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// VIEW.STUDENT.INFORMATION
        /// An authenticated user with any of the following permission codes who does not have one of the permission codes above can add student program:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpPost]
        public async Task<Dtos.Student.StudentProgram2> AddStudentProgramAsync([FromUri] string studentId, [FromBody] StudentAcademicProgram studentAcademicProgram)
        {
            if (studentId == null)
            {
                string errorText = "Must provide the studentId to create a new program for student.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (studentAcademicProgram == null)
            {
                string errorText = "Must provide the studentAcademicProgram item to create a new program for student.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentProgramService.AddStudentProgram(studentAcademicProgram);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Update (PUT) an existing student academic program.
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAcademicProgram">Information for updating a academic program for student</param>
        /// <returns><see cref="Dtos.Student.StudentProgram2"> Updated student program</see></returns>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes can update student program
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// VIEW.STUDENT.INFORMATION
        /// An authenticated user with any of the following permission codes who does not have one of the permission codes above can update student program:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        [HttpPut]
        public async Task<Dtos.Student.StudentProgram2> UpdateStudentProgramAsync([FromUri] string studentId, [FromBody] StudentAcademicProgram studentAcademicProgram)
        {
            if (studentId == null)
            {
                string errorText = "Must provide the studentId to update academic program for student.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (studentAcademicProgram == null)
            {
                string errorText = "Must provide the studentAcademicProgram to update academic program for student.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentProgramService.UpdateStudentProgram(studentAcademicProgram);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}
