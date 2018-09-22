﻿// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using SectionRegistration = Ellucian.Colleague.Dtos.Student.SectionRegistration;
using System.Diagnostics;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Accesses Student data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentsController : BaseCompressedApiController
    {
        private readonly IEmergencyInformationService _emergencyInformationService;
        private readonly IAcademicHistoryService _academicHistoryService;
        private readonly IStudentService _studentService;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly IRequirementRepository _requirementRepository;
        private readonly IStudentRestrictionService _studentRestrictionService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// Initializes a new instance of the StudentsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="academicHistoryService">Service of type <see cref="IAcademicHistoryService">IAcademicHistoryService</see></param>
        /// <param name="studentService">Service of type <see cref="IStudentService">IStudentService</see></param>
        /// <param name="studentProgramRepository">Repository of type <see cref="IStudentProgramRepository">IStudentProgramRepository</see></param>
        /// <param name="studentRestrictionService">Service of type <see cref="IStudentRestrictionService">IStudentRestrictionService</see></param>
        /// <param name="requirementRepository">Repository of type <see cref="IRequirementRepository">IRequirementRepository</see></param>
        /// <param name="emergencyInformationService">Service of type <see cref="IEmergencyInformationService">IEmergencyInformationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings"><see cref="ApiSettings"/>instance</param>
        public StudentsController(IAdapterRegistry adapterRegistry, IAcademicHistoryService academicHistoryService,
                                  IStudentService studentService, IStudentProgramRepository studentProgramRepository,
                                  IStudentRestrictionService studentRestrictionService,
                                  IRequirementRepository requirementRepository,
                                  IEmergencyInformationService emergencyInformationService,
                                  ILogger logger,
                                  ApiSettings apiSettings)
        {
            _academicHistoryService = academicHistoryService;
            _studentService = studentService;
            _studentProgramRepository = studentProgramRepository;
            _studentRestrictionService = studentRestrictionService;
            _requirementRepository = requirementRepository;
            _adapterRegistry = adapterRegistry;
            _emergencyInformationService = emergencyInformationService;
            _logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Action to get Students from a list of Ids
        /// </summary>
        /// <param name="criteria">Criteria contains List of Student IDs.</param>
        /// <returns>Student DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Student>> QueryStudentsAsync([FromBody] StudentQueryCriteria criteria)
        {
            try
            {
                return await _studentService.GetStudentsByIdAsync(criteria.StudentIds, criteria.InheritFromPerson, criteria.GetDegreePlan);
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
        /// Action to get Students from a list of Ids
        /// </summary>
        /// <param name="criteria">Criteria contains List of Student IDs.</param>
        /// <returns>Student DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentBatch>> QueryStudentsByIdAsync([FromBody] StudentQueryCriteria criteria)
        {
            try
            {
                return await _studentService.QueryStudentsByIdAsync(criteria.StudentIds, false, false);
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
        /// Action to get Students from a list of Ids
        /// </summary>
        /// <param name="criteria">Criteria contains List of Student IDs.</param>
        /// <returns>StudentBatch2 DTO Objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentBatch2>> QueryStudentsByIdAsync2([FromBody] StudentQueryCriteria criteria)
        {
            try
            {
                return await _studentService.QueryStudentsByIdAsync2(criteria.StudentIds, false, false, criteria.Term);
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
        /// Action to get Students from a list of Ids.
        /// Marital status can be null.
        /// </summary>
        /// <param name="criteria">Criteria contains List of Student IDs.</param>
        /// <returns>StudentBatch3 DTO Objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentBatch3>> QueryStudentsByIdAsync3([FromBody] StudentQueryCriteria criteria)
        {
            try
            {
                return await _studentService.QueryStudentsByIdAsync3(criteria.StudentIds, false, false, criteria.Term);
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
        /// Action to get Students from a list of Ids.
        /// Marital status can be null.
        /// Filter out student advisements which ended today or earlier.
        /// </summary>
        /// <param name="criteria">Criteria contains List of Student IDs.</param>
        /// <returns>StudentBatch3 DTO Objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentBatch3>> QueryStudentsById4Async([FromBody] StudentQueryCriteria criteria)
        {
            try
            {
                return await _studentService.QueryStudentsById4Async(criteria.StudentIds, false, false, criteria.Term);
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
        /// Gets list of students keys for a given term only.  Other parameters are ignored.
        /// </summary>
        /// <param name="studentQuery">Query parameter object</param>
        /// <returns>List of students for a term.  Only the termId parameter is used at this time.<see cref="Student">Student IDs</see></returns>
        /// <accessComments>
        /// User with permission of VIEW.STUDENT.INFORMATION can retrieve student Ids for given term.
        /// </accessComments>
        public async Task<IEnumerable<string>> PostStudentIdsAsync([FromBody] StudentQuery studentQuery)
        {
            try
            {
                return await _studentService.SearchIdsAsync(studentQuery.termId);
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)              // Something bad happened
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException("An error occurred during search: " + e.Message, HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Gets information the programs in which the specified student is enrolled.
        /// </summary>
        /// <param name="studentId">Student's ID</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well</param>
        /// <returns>All <see cref="StudentProgram">Programs</see> in which the specified student is enrolled.</returns>
        [Obsolete("Obsolete as of Api version 1.10, use version 2 of this API")]
        public async Task<IEnumerable<StudentProgram>> GetStudentProgramsAsync(string studentId, bool currentOnly = true)
        {
            try
            {
                await _studentService.CheckStudentAccessAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("student", studentId);
            }

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetAsync(studentId);

            // Limit set of student programs to current programs if requested
            if (currentOnly == true)
            {
                studentPrograms = studentPrograms.Where(x => x.EndDate == null || x.EndDate >= DateTime.Today);
            }

            List<StudentProgram> studentProgramDtos = new List<StudentProgram>();

            if (studentPrograms.Count() > 0)
            {
                var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram>();
                var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();
                foreach (var prog in studentPrograms)
                {
                    var studentProgramDto = studentProgramDtoAdapter.MapToType(prog);
                    foreach (var additionalReq in studentProgramDto.AdditionalRequirements)
                    {
                        if (!String.IsNullOrEmpty(additionalReq.RequirementCode))
                        {
                            additionalReq.Requirement = requirementDtoAdapter.MapToType((await _requirementRepository.GetAsync(additionalReq.RequirementCode)));
                        }
                    }
                    studentProgramDtos.Add(studentProgramDto);
                }
            }
            return studentProgramDtos;
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistory">Academic History</see> for the student.</returns>
        [Obsolete("Obsolete as of Api version 1.5, use version 2 of this API")]
        public async Task<AcademicHistory> GetAcademicHistoryAsync(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            try
            {
                return await _academicHistoryService.GetAcademicHistoryAsync(studentId, bestFit, filter, term);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistory2">Academic History</see> for the student.</returns>
        [Obsolete("Obsolete as of API version 1.11, use GetAcademicHistory3Async instead")]
        public async Task<AcademicHistory2> GetAcademicHistory2Async(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            try
            {
                return await _academicHistoryService.GetAcademicHistory2Async(studentId, bestFit, filter, term);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistory3">Academic History</see> for the student.</returns>
        [Obsolete("Obsolete as of API version 1.18, use GetAcademicHistory4Async instead")]
        public async Task<AcademicHistory3> GetAcademicHistory3Async(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            try
            {
                return await _academicHistoryService.GetAcademicHistory3Async(studentId, bestFit, filter, term);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the academic history for the student. This groups the information on a term by term basis (separating out the non-term classes).
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <param name="includeDrops">(Optional) used to include dropped academic credits</param>
        /// <returns>The <see cref="AcademicHistory4">Academic History</see> for the student.</returns>
        public async Task<AcademicHistory4> GetAcademicHistory4Async(string studentId, bool bestFit = false, bool filter = true, string term = null, bool includeDrops = false)
        {
            try
            {
                return await _academicHistoryService.GetAcademicHistory4Async(studentId, bestFit, filter, term, includeDrops);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the Student Restrictions for the provided student. Obsolete as of 1.11 - use GetStudentRestrictionsAsync2
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All  <see cref="PersonRestriction">Student Restrictions</see> for the provided student.</returns>
        [Obsolete("Obsolete as of API version 1.11, use GetStudentRestrictionsAsync2")]
        public async Task<IEnumerable<PersonRestriction>> GetStudentRestrictionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Unable to get student restrictions. Invalid studentId " + studentId);
                throw CreateHttpResponseException("Unable to get student restrictions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentRestrictionService.GetStudentRestrictionsAsync(studentId, false);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the Student Restrictions for the provided student.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All  <see cref="PersonRestriction">Student Restrictions</see> for the provided student.</returns>
        [Obsolete("Obsolete as of API version 1.16, use GetStudentRestrictions3Async")]
        public async Task<IEnumerable<PersonRestriction>> GetStudentRestrictionsAsync2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Unable to get student restrictions. Invalid studentId " + studentId);
                throw CreateHttpResponseException("Unable to get student restrictions. Invalid studentId", HttpStatusCode.BadRequest);
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

                return await _studentRestrictionService.GetStudentRestrictionsAsync(studentId, useCache);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("Unable to process student restrictions", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the Student Restrictions for the provided student.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All  <see cref="PersonRestriction">Student Restrictions</see> for the provided student.</returns>
        public async Task<IEnumerable<PersonRestriction>> GetStudentRestrictions3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Unable to get student restrictions. Invalid studentId " + studentId);
                throw CreateHttpResponseException("Unable to get student restrictions. Invalid studentId", HttpStatusCode.BadRequest);
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

                return await _studentRestrictionService.GetStudentRestrictions2Async(studentId, useCache);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("Unable to process student restrictions", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the Student Restrictions for the provided list of students or list of Restriction keys.
        /// </summary>
        /// <param name="criteria">DTO object which contains Student keys or Restriction keys for selection</param>
        /// <returns>Returns a list of <see cref="PersonRestriction">PersonRestriction</see> DTO objects for the provided list of students or restrictions.</returns>    
        /// <accessComments>
        /// Only users with VIEW.STUDENT.INFORMATION permission can retrieve student restrictions for the provided list of students or list of restriction keys.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<PersonRestriction>> PostStudentRestrictionsQuery([FromBody] StudentRestrictionsQueryCriteria criteria)
        {
            try
            {
                if (criteria.Ids != null && criteria.Ids.Count() > 0)
                {
                    return await _studentRestrictionService.GetStudentRestrictionsByIdsAsync(criteria.Ids);
                }
                else
                {
                    return await _studentRestrictionService.GetStudentRestrictionsByStudentIdsAsync(criteria.StudentIds);
                }
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)              // Something bad happened
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException("An error occurred during search: " + e.Message, HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Check to see if the user is eligible to register for the provided term
        /// </summary>
        /// <param name="id">The user's ID</param>
        /// <returns>A list of <see cref="RegistrationMessage"/>Registration Messages.</returns>
        [Obsolete("Obsolete as of API version 1.3, use version 2 of this API")]
        public async Task<IEnumerable<RegistrationMessage>> GetRegistrationEligibilityAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.Error("Invalid id");
                throw CreateHttpResponseException("Invalid id", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentService.CheckRegistrationEligibilityAsync(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Checks to see if the student is eligible to register.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns><see cref="RegistrationEligibility">Registration Eligibility </see> information containing messages, which, if present, indicate the student
        /// is ineligible, in addition to a boolean HasOverride, set to true if the current user has the ability to override ineligibility.</returns>
        public async Task<Dtos.Student.RegistrationEligibility> GetRegistrationEligibility2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Invalid studentId");
                throw CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentService.CheckRegistrationEligibility2Async(studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the transcript restrictions for the provided student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>Information used to determine if a student should be prevented from seeing or requesting their transcript.</returns>
        public async Task<Dtos.Student.TranscriptAccess> GetTranscriptRestrictions2Async(string studentId)
        {
            try
            {
                //var sectionPermission = await _service.GetAsync(sectionId);
                //return sectionPermission;
                var transcriptAccessDto = await _studentService.GetTranscriptRestrictions2Async(studentId);
                return transcriptAccessDto;
            }
            catch (KeyNotFoundException)
            {
                // Student not found.  Error already logged in repository
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the transcript restrictions for the provided student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All transcript restrictions for the provided student.</returns>
        public async Task<IEnumerable<Dtos.Student.TranscriptRestriction>> GetTranscriptRestrictionsAsync(string studentId)
        {
            try
            {
                IEnumerable<Domain.Student.Entities.TranscriptRestriction> restrictionsDomain = await _studentService.GetTranscriptRestrictionsAsync(studentId);
                List<Dtos.Student.TranscriptRestriction> restrictionsDto = new List<Dtos.Student.TranscriptRestriction>();
                if (restrictionsDomain.Count() > 0)
                {
                    var restrictionAdapter = new AutoMapperAdapter<Domain.Student.Entities.TranscriptRestriction, Dtos.Student.TranscriptRestriction>(_adapterRegistry, _logger);
                    foreach (var rest in restrictionsDomain)
                    {
                        restrictionsDto.Add(restrictionAdapter.MapToType(rest));
                    }
                }
                return restrictionsDto;
            }
            catch (KeyNotFoundException)
            {
                // Student not found.  Error already logged in repository
                throw CreateNotFoundException("student", studentId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves the ungraded Terms for the provided student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>All ungraded  <see cref="Term">Terms</see> for the student.</returns>
        ///  <accessComments>
        /// Ungraded terms for a student can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEES
        /// UPDATE.ANY.ADVISEES
        /// ALL.ACCESS.ANY.ADVISEES
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// </accessComments>
        public async Task<IEnumerable<Term>> GetUngradedTermsAsync(string studentId)
        {
            try
            {
                return await _studentService.GetUngradedTermsAsync(studentId);
            }
            catch (KeyNotFoundException)
            {
                // Student not found.  Error already logged in repository
                throw CreateNotFoundException("student", studentId);
            }
            catch (PermissionsException)
            {
                throw CreateHttpResponseException("User does not have permission to view student", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Retrieves Students, including references to the student's Degree Plan, Programs and Restrictions and some demographic information, for those students who match the provided query parameters. At a minimum, Date of Birth and Last Name are required parameters.
        /// </summary>
        /// <param name="studentQuery">Query parameter object</param>
        /// <returns>All <see cref="Student">Students</see> who matched the query.</returns>
        /// <accessComments>
        /// Only users with permission VIEW.ANY.ADVISEE can perform search on students.
        /// </accessComments>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Student>> PostSearchStudentAsync([FromBody] StudentQuery studentQuery)
        {
            if (studentQuery.dateOfBirth == null || string.IsNullOrEmpty(studentQuery.lastName))
            {
                throw CreateHttpResponseException("This search requires last name and date of birth", HttpStatusCode.BadRequest);
            }

            try
            {
                return await _studentService.SearchAsync(studentQuery.lastName, studentQuery.dateOfBirth, studentQuery.firstName, studentQuery.formerName, studentQuery.studentId, studentQuery.governmentId);
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException ae)   // Nothing matched
            {
                _logger.Error(ae.Message);
                return new List<Ellucian.Colleague.Dtos.Student.Student>();
            }
            catch (Exception e)              // Something bad happened
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException("An error occurred during search: " + e.Message, HttpStatusCode.NotFound);
            }


        }
        /// <summary>
        /// Accepts a transcript order and enters it into Colleague.  
        /// </summary>
        /// <param name="transcriptRequest">PESC XML Transcript Request</param>
        /// <returns>HTTP 201 if successful; in the body, the status of the request, and the date, if any, of expected future processing.</returns>
        public async Task<HttpResponseMessage> PostTranscriptOrderAsync([FromBody] TranscriptRequest transcriptRequest)
        {

            string dataresponse = null;

            if (transcriptRequest == null || transcriptRequest.TransmissionData == null)
            {
                throw CreateHttpResponseException("The XML request was not understood", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(transcriptRequest.TransmissionData.RequestTrackingID))
            {
                throw CreateHttpResponseException("The XML request was missing element TranscriptRequest:TransmissionData:RequestTrackingID", HttpStatusCode.BadRequest);
            }
            if (transcriptRequest.TransmissionData.RequestTrackingID.Length > 35)
            {
                throw CreateHttpResponseException("TranscriptRequest:TransmissionData:RequestTrackingID cannot be over 35 bytes", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(transcriptRequest.TransmissionData.Source.Organization.DUNS))
            {
                throw CreateHttpResponseException("The XML request was missing element TranscriptRequest:TransmissionData:Source.Organization.DUNS", HttpStatusCode.BadRequest);
            }

            try
            {

                dataresponse = await _studentService.OrderTranscriptAsync(transcriptRequest);
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }

            string orderId = transcriptRequest.TransmissionData.RequestTrackingID;

            TranscriptResponse jsonResponseContainer = new TranscriptResponse() { ResponseData = dataresponse };

            var httpResponse = Request.CreateResponse(HttpStatusCode.Created, jsonResponseContainer);
            httpResponse.Headers.Location = new Uri(Request.RequestUri, String.Format("transcript-orders/{0}", orderId));
            return httpResponse;

        }

        /// <summary>
        /// gets the current status of a transcript order
        /// </summary>
        /// <param name="orderId">Third-party-generated order ID</param>
        /// <param name="currentStatusCode">The cloud's current understanding of the order's status</param>
        /// <returns>Base-64 encoded PESC XML Transcript Response</returns>
        public async Task<HttpResponseMessage> GetTranscriptOrderStatusAsync(string orderId, string currentStatusCode)
        {

            string dataresponse = null;

            if (string.IsNullOrEmpty(orderId))
            {
                throw CreateHttpResponseException("Request missing orderId", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(currentStatusCode))
            {
                throw CreateHttpResponseException("Request missing currentStatusCode", HttpStatusCode.BadRequest);
            }
            try
            {
                dataresponse = await _studentService.CheckTranscriptStatusAsync(orderId, currentStatusCode);
            }
            catch (PermissionsException pex)  // Not logged in or didn't have right permissions
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                throw CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }

            TranscriptResponse jsonResponseContainer = new TranscriptResponse() { ResponseData = dataresponse };

            var httpResponse = Request.CreateResponse(HttpStatusCode.OK, jsonResponseContainer);
            return httpResponse;

        }

        /// <summary>
        /// Retrieves a pdf of the student's unofficial transcript. 
        /// </summary>
        /// <param name="studentId">The system id for the student whose transcript is being requested</param>
        /// <param name="transcriptGrouping">The transcript grouping of transcript to return. If empty, transcripts of all grouping types will be returned for the student</param>
        /// <returns>A pdf of the student's unofficial transcript</returns>
        public async Task<HttpResponseMessage> GetUnofficialTranscriptAsync(string studentId, string transcriptGrouping = null)
        {
            try
            {
                // Only service requests for pdf.  Don't want to return JSON, plain-text, or anything else by design.
                if (Request.Headers.Accept.Where(rqa => rqa.MediaType == "application/pdf").Count() > 0)
                {
                    var path = HttpContext.Current.Server.MapPath("~/Reports/Student/UnofficialTranscript.rdlc");
                    var deviceInfoPath = HttpContext.Current.Server.MapPath("~/Reports/Student/UnofficialTranscriptDeviceInfo.txt");
                    var reportWatermarkPath = !string.IsNullOrEmpty(apiSettings.UnofficialWatermarkPath) ? apiSettings.UnofficialWatermarkPath : "";
                    if (string.IsNullOrEmpty(reportWatermarkPath))
                    {
                        reportWatermarkPath = "~/Content/Images/unofficial-watermark.png";
                    }
                    if (!reportWatermarkPath.StartsWith("~"))
                    {
                        reportWatermarkPath = "~" + reportWatermarkPath;
                    }
                    reportWatermarkPath = HttpContext.Current.Server.MapPath(reportWatermarkPath);
                    string filenameToUse = string.Empty;
                    var officialTranscriptInfo = await _studentService.GetUnofficialTranscriptAsync(studentId, path, transcriptGrouping, reportWatermarkPath, deviceInfoPath);
                    var renderedBytes=officialTranscriptInfo.Item1;
                    var fileNameToUse=officialTranscriptInfo.Item2;
                    // Create the http response object, use the byte array for the response content, and set header content type to the mime type of the report
                    var response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(renderedBytes);

                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileNameToUse
                    };
                    response.Content.Headers.ContentLength = renderedBytes.Length;
                    return response;
                }
                // If the request didn't specify pdf, it's an unsupported request
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (NotSupportedException)
            {
                throw CreateHttpResponseException("Only application/pdf and application/json are served from this endpoint", HttpStatusCode.NotAcceptable);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException)
            {
                throw CreateNotFoundException("unofficial transcript", studentId.ToString());
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Get all the emergency information for a person.
        /// </summary>
        /// <param name="studentId">Pass in a student's ID</param>
        /// <returns>Returns all the emergency information for the specified person</returns>
        [Obsolete("Obsolete as of API version 1.9, use GET /persons/{personId}/emergency-information")]
        public async Task<Ellucian.Colleague.Dtos.Base.EmergencyInformation> GetEmergencyInformationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("Invalid student ID", HttpStatusCode.BadRequest);
            }

            try
            {
                return await _emergencyInformationService.GetEmergencyInformationAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("student", studentId);
            }
        }

        /// <summary>
        /// Update a person's emergency information.
        /// </summary>
        /// <param name="emergencyInformation">An emergency information object</param>
        /// <returns>The updated emergency information object</returns>
        [Obsolete("Obsolete as of API version 1.9, use PUT /persons/{personId}/emergency-information")]
        public EmergencyInformation PutEmergencyInformation(EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                throw CreateHttpResponseException("Request missing emergency information", HttpStatusCode.BadRequest);
            }
            try
            {
                var updatedEmergencyInformation = _emergencyInformationService.UpdateEmergencyInformation(emergencyInformation);

                return updatedEmergencyInformation;
            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateHttpResponseException("Unable to update emergency information", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Process course section registration requests for a student. 
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="sectionRegistrations">Registration requests to process</param>
        /// <returns>A registration response which includes any messages from registration</returns>
        [HttpPut]
        public async Task<RegistrationResponse> RegisterAsync(string studentId, [FromBody] IEnumerable<SectionRegistration> sectionRegistrations)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Invalid studentId");
                throw CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (sectionRegistrations == null || sectionRegistrations.Count() == 0)
            {
                _logger.Error("Invalid sectionRegistration");
                throw CreateHttpResponseException("Invalid sectionRegistration. Must provide at least one.", HttpStatusCode.BadRequest);
            }
            try
            {
                RegistrationResponse response = await _studentService.RegisterAsync(studentId, sectionRegistrations);
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("An error occurred during request processing: " + e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves information for the specified student, including references to the student's DegreePlan, Programs and Restrictions and some demographic information.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>Information about this <see cref="Student">Student</see></returns>
        /// <accessComments>
        /// Student information can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 2. Proxy user is accessing the student's data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEES
        /// UPDATE.ANY.ADVISEES
        /// ALL.ACCESS.ANY.ADVISEES
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 5. A user with permission of VIEW.STUDENT.INFORMATION is accessing the student's data.
        /// 
        ///  Privacy is enforced by this response. If any student has an assigned privacy code that the advisor or faculty is not authorized to access, the Student response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned. In this situation, 
        /// all details except the student name are cleared from the specific Student object.
        /// </accessComments>
        public async Task<Ellucian.Colleague.Dtos.Student.Student> GetStudentAsync(string studentId)
        {
            try
            {
                var privacyWrapper = await _studentService.GetAsync(studentId);
                var student = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Student;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return student;
                
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException)
            {
                throw CreateNotFoundException("student", studentId);
            }
            catch (Exception exception)
            {
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves information for the searched student.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>Information about this <see cref="Student">Student</see></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Student>> QueryStudentByPost2Async([FromBody]StudentSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            _logger.Info("Entering QueryStudentByPost2Async");
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                // The service to execute this search is in StudentService.
                var privacyWrapper = await _studentService.Search3Async(criteria, pageSize, pageIndex);
                var students = privacyWrapper.Dto as List<Dtos.Student.Student>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                watch.Stop();
                _logger.Info("QueryStudentByPost2Async... completed in " + watch.ElapsedMilliseconds.ToString());

                return (IEnumerable<Dtos.Student.Student>)students;
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets information the programs in which the specified student is enrolled.
        /// </summary>
        /// <param name="studentId">Student's ID</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well</param>
        /// <returns>All <see cref="StudentProgram2">Programs</see> in which the specified student is enrolled.</returns>
        public async Task<IEnumerable<StudentProgram2>> GetStudentPrograms2Async(string studentId, bool currentOnly = true)
        {
            try
            {
                await _studentService.CheckStudentAccessAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("student", studentId);
            }

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetAsync(studentId);

            // Limit set of student programs to current programs if requested
            if (currentOnly == true)
            {
                studentPrograms = studentPrograms.Where(x => x.EndDate == null || x.EndDate >= DateTime.Today);
            }

            List<StudentProgram2> studentProgramDtos = new List<StudentProgram2>();

            if (studentPrograms.Count() > 0)
            {
                var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram2>();
                var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();
                foreach (var prog in studentPrograms)
                {
                    var studentProgramDto = studentProgramDtoAdapter.MapToType(prog);
                    foreach (var additionalReq in studentProgramDto.AdditionalRequirements)
                    {
                        if (!String.IsNullOrEmpty(additionalReq.RequirementCode))
                        {
                            additionalReq.Requirement = requirementDtoAdapter.MapToType((await _requirementRepository.GetAsync(additionalReq.RequirementCode)));
                        }
                    }
                    studentProgramDtos.Add(studentProgramDto);
                }
            }
            return studentProgramDtos;
        }

        /// <summary>
        /// Return a list of Students objects based on page.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="person">GUID for a reference to link a student to the common HEDM persons entity.</param>
        /// <param name="type">GUID for the type of the student.</param>
        /// <param name="cohorts">GUID for the groupings of students for reporting/tracking purposes (cohorts) to which the student is associated.</param>
        /// <param name="residency">GUID for the residency type for selecting students.</param>
        /// <returns>List of Students <see cref="Dtos.Students"/> objects representing matching Students</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "person", "type", "cohorts", "residency" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetStudentsAsync(Paging page, [FromUri] string person = "", [FromUri] string type = "", [FromUri] string cohorts = "", [FromUri] string residency = "")
        {
            string criteria = string.Concat(person, type, cohorts, residency);

            //valid query parameter but empty argument
            if (!string.IsNullOrEmpty(criteria) && (string.IsNullOrEmpty(criteria.Replace("\"", "")) || string.IsNullOrEmpty(criteria.Replace("'", ""))))
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Students>>(new List<Dtos.Students>(), page, 0, this.Request);
            }

            if (person == null || person == "null" || type == null || type == "null" || cohorts == null ||
                cohorts == "null" || residency == null || residency == "null")
                // null vs. empty string means they entered a filter with no criteria and we should return an empty set.
                return new PagedHttpActionResult<IEnumerable<Dtos.Students>>(new List<Dtos.Students>(), page, 0, this.Request);

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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Students>>(new List<Dtos.Students>(), page, 0, this.Request);

                var pageOfItems = await _studentService.GetStudentsAsync(page.Offset, page.Limit, bypassCache, person, type, cohorts, residency);

                AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Students>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            
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
        /// Retrieves a Student by Guid.
        /// </summary>
        /// <returns>An <see cref="Dtos.Students">Students</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Students> GetStudentsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                var student = await _studentService.GetStudentsByGuidAsync(guid);

                if (student != null)
                {

                    AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { student.Id }));
                }


                return student;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Creates a Student
        /// </summary>
        /// <param name="student"><see cref="Dtos.Students">Student</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Students">Student</see></returns>
        [HttpPost]
        public async Task<Dtos.Students> PostStudentAsync([FromBody] Dtos.Students student)
        {
            //Create is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates a Student.
        /// </summary>
        /// <param name="guid">Id of the Student to update</param>
        /// <param name="student"><see cref="Dtos.Student">Student</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Students">Student</see></returns>
        [HttpPut]
        public async Task<Dtos.Students> PutStudentAsync([FromUri] string guid, [FromBody] Dtos.Students student)
        {
            //Update is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Student
        /// </summary>
        /// <param name="guid">Id of the Student to delete</param>
        [HttpDelete]
        public async Task DeleteStudentByGuidAsync([FromUri] string guid)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}