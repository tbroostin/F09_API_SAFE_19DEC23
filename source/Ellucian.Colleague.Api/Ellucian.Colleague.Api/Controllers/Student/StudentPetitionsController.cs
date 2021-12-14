// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provide access to faculty Consent and student petition data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentPetitionsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly ISectionPermissionService _service;
        private readonly IStudentPetitionService _studentPetitionService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="studentPetitionService"></param>
        /// <param name="logger"></param>
        public StudentPetitionsController(ISectionPermissionService service,IStudentPetitionService studentPetitionService, ILogger logger)
        {
            _service = service;
            _studentPetitionService = studentPetitionService;
            _logger = logger;
        }


        /// <summary>
        /// Creates a new Student Petition.
        /// </summary>
        /// <param name="studentPetition">StudentPetition dto object</param>
        /// <returns>
        /// If successful, returns the newly created Student Petition in an http response with resource locator information. 
        /// If failure, returns the exception information. If failure due to existing Student Petition found for the given student and section,
        /// also returns resource locator to use to retrieve the existing item.
        /// </returns>
        /// <accessComments>
        /// User must have correct permission code, depending on petition type:
        /// CREATE.STUDENT.PETITION
        /// CREATE.FACULTY.CONSENT
        /// </accessComments>
        public async Task<HttpResponseMessage> PostStudentPetitionAsync([FromBody]Dtos.Student.StudentPetition studentPetition)
        {
            try
            {
                Dtos.Student.StudentPetition createdPetitionDto = await _service.AddStudentPetitionAsync(studentPetition);
                var response = Request.CreateResponse<Dtos.Student.StudentPetition>(HttpStatusCode.Created, createdPetitionDto);
                SetResourceLocationHeader("GetStudentPetition", new { studentPetitionId = createdPetitionDto.Id, sectionId = createdPetitionDto.SectionId, type = createdPetitionDto.Type.ToString() });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingStudentPetitionException swex)
            {
                _logger.Info(swex.ToString());

                // Create the get existing student petition by ID.
                SetResourceLocationHeader("GetStudentPetition", new { id = swex.ExistingStudentPetitionId, sectionId = swex.ExistingStudentPetitionSectionId, type = swex.ExistingStudentPetitionType });

                throw CreateHttpResponseException(swex.Message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update Student Petition
        /// </summary>
        /// <param name="studentPetition">StudentPetition dto object</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to update student petitions.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>
        /// If successful, returns the updated Student Petition in an http response with resource locator information. 
        /// If failure, returns the exception information. 
        /// </returns>
        /// <accessComments>
        /// User must have correct permission code, depending on petition type:
        /// CREATE.STUDENT.PETITION
        /// CREATE.FACULTY.CONSENT
        /// </accessComments>
        public async Task<HttpResponseMessage> PutStudentPetitionAsync([FromBody] Dtos.Student.StudentPetition studentPetition)
        {
            try
            {
                Dtos.Student.StudentPetition updatedPetitionDto = await _service.UpdateStudentPetitionAsync(studentPetition);
                var response = Request.CreateResponse<Dtos.Student.StudentPetition>(HttpStatusCode.Created, updatedPetitionDto);
                SetResourceLocationHeader("GetStudentPetition", new { studentPetitionId = updatedPetitionDto.Id, sectionId = updatedPetitionDto.SectionId, type = updatedPetitionDto.Type.ToString() });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Error(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the requested student petition based on the student petition Id, section Id, and type.
        /// The user making this request must be an instructor of the section for which the petition is being requested or it will generate a permission exception.
        /// </summary>
        /// <param name="studentPetitionId">Id of the student Petition (Required)</param>
        /// <param name="sectionId">Id of the section for which the petition is requested. (Required)</param>
        /// <param name="type">Type of student petition desired since same ID can yield either type. If not provided it will default to a petition of type StudentPetition.</param>
        /// <returns>Student Petition object</returns>
        /// <accessComments>
        /// User must be faculty in specified section to get data.
        /// </accessComments>
        public async Task<Dtos.Student.StudentPetition> GetAsync(string studentPetitionId, string sectionId, StudentPetitionType type)
        {
            try
            {
                return await _service.GetStudentPetitionAsync(studentPetitionId, sectionId, type);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Student Petition is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid Student Petition Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the requested student petition." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the student petitions and faculty consents.
        /// </summary>
        /// <param name="studentId">Id of the student </param>
        /// <returns>Collection of Student Petition object</returns>
        /// <accessComments>
        /// 1. A student can access their own data
        /// 2. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        public async Task<IEnumerable<Dtos.Student.StudentPetition>> GetAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Unable to get student petitions. Invalid studentId " + studentId);
                throw CreateHttpResponseException("Unable to get student petitions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentPetitionService.GetAsync(studentId);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Student Petition is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the student petitions." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the student overload petitions
        /// </summary>
        /// <param name="studentId">Id of the student </param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve overload petitions.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="StudentOverloadPetition">StudentOverloadPetition</see> object</returns>
        /// <accessComments>
        /// 1. A student can access their own data
        /// 2. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        public async Task<IEnumerable<Dtos.Student.StudentOverloadPetition>> GetStudentOverloadPetitionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Unable to get student overload petitions. Invalid studentId " + studentId);
                throw CreateHttpResponseException("Unable to get student overload petitions. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentPetitionService.GetStudentOverloadPetitionsAsync(studentId);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Student Overload Petition is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the student overload petitions." + System.Net.HttpStatusCode.BadRequest);
            }
        }
    }

}
