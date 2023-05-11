// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Waiver data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentWaiversController : BaseCompressedApiController
    {
        private readonly IStudentWaiverService _waiverService;
        private readonly ILogger _logger;

        /// <summary>
        /// Provides access to Student Waivers.
        /// </summary>
        /// <param name="waiverService"></param>
        /// <param name="logger"></param>
        public StudentWaiversController(IStudentWaiverService waiverService, ILogger logger)
        {
            _waiverService = waiverService;
            _logger = logger;
        }

        /// <summary>
        /// This route is obsolete as of API 1.21. 
        /// Returns the requested section waiver
        /// </summary>
        /// <param name="waiverId">Id of waiver to retrieve</param>
        /// <returns>Student Waiver</returns>
        /// <accessComments>
        /// Only an assigned faculty for the section to which waiver applies can retrieve the waiver.
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.21. Use version 2 instead.")]
        public async Task<Dtos.Student.StudentWaiver> GetStudentWaiverAsync(string waiverId)
        {
            try
            {
                return await _waiverService.GetStudentWaiverAsync(waiverId);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Waivers is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid Waiver Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the requested Waiver." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the requested section waiver
        /// </summary>
        /// <param name="id">Id of waiver to retrieve</param>
        /// <returns>Student Waiver</returns>
        /// <accessComments>
        /// Only an assigned faculty for the section to which waiver applies can retrieve the waiver.
        /// </accessComments>
        public async Task<Dtos.Student.StudentWaiver> GetStudentWaiver2Async(string id)
        {
            try
            {
                return await _waiverService.GetStudentWaiverAsync(id);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Waivers is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid Waiver Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the requested Waiver." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the waivers found for the specified section. Requestor must have proper permissions to access
        /// the waivers for a section.
        /// </summary>
        /// <param name="sectionId">The section Id to use to retrieve waivers</param>
        /// <returns>List of <see cref="Dtos.Student.StudentWaiver">Waiver</see> objects</returns>
        /// <accessComments>
        /// 1. Only an assigned faculty for the section can retrieve student waivers.
        /// 2. A departmental oversight member assigned to the section may retrieve student waivers with any of the following permission codes
        /// VIEW.SECTION.PREREQUISITE.WAIVER
        /// CREATE.SECTION.REQUISITE.WAIVER
        /// </accessComments>
        public async Task<IEnumerable<Dtos.Student.StudentWaiver>> GetSectionStudentWaiversAsync(string sectionId)
        {
            try
            {
                return await _waiverService.GetSectionStudentWaiversAsync(sectionId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                var message = "Session has expired while retrieving section student waivers";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "Access to Section Waivers is forbidden.";
                _logger.Error(pe, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Invalid section specified.";
                _logger.Error(knfe, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = "Error occurred retrieving waivers for section.";
                _logger.Error(e, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new Section Requisite waiver.
        /// </summary>
        /// <param name="waiver">Section requisite waiver dto object</param>
        /// <returns>
        /// If successful, returns the newly created section requisite waiver in an http response with resource locator information. 
        /// If failure, returns the exception information. If failure due to existing waiver found for the given student and section,
        /// also returns resource locator to use to retrieve the existing item.
        /// </returns>
        /// <accessComments>
        /// 1. A faculty member assigned to the section with CREATE.PREREQUISITE.WAIVER permission can create a new Section Requisite Waiver.
        /// 2. A departmental oversight member assigned to the section with CREATE.SECTION.REQUISITE.WAIVER permission can create a new Section Requisite Waiver.
        /// </accessComments>
        public async Task<HttpResponseMessage> PostStudentWaiverAsync([FromBody]Dtos.Student.StudentWaiver waiver)
        {
            try
            {
                Dtos.Student.StudentWaiver createdWaiverDto = await _waiverService.CreateStudentWaiverAsync(waiver);
                var response = Request.CreateResponse<Dtos.Student.StudentWaiver>(HttpStatusCode.Created, createdWaiverDto);
                SetResourceLocationHeader("GetStudentWaiver2", new { id = createdWaiverDto.Id });
                return response;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                var message = "Session has expired while creating student requisite waiver.";
                _logger.Error(csee, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                var message = "User does not have appropriate permissions to create student requisite waiver.";
                _logger.Info(peex, message);
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingSectionWaiverException swex)
            {
                var message = "Student requisite waiver already exists.";
                _logger.Info(swex, message);
                SetResourceLocationHeader("GetStudentWaiver2", new { id = swex.ExistingSectionWaiverId });
                throw CreateHttpResponseException(message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                var message = "Error occurred while creating student requisite waiver.";
                _logger.Error(ex, message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the waivers found for the specified student. 
        /// </summary>
        /// <param name="studentId">The section Id to use to retrieve waivers</param>
        /// <returns>List of <see cref="Dtos.Student.StudentWaiver">Waiver</see> objects</returns>
        /// <accessComments>
        /// 1. User must be requesting their own data.
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
        public async Task<IEnumerable<Dtos.Student.StudentWaiver>> GetStudentWaiversAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Unable to get student waivers. Invalid studentId " + studentId);
                throw CreateHttpResponseException("Unable to get student waivers. Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _waiverService.GetStudentWaiversAsync(studentId);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student waivers";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                string message = "Access to Section Waivers is forbidden.";
                _logger.Error(pe, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = "Error occurred retrieving waivers for section.";
                _logger.Error(e, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
