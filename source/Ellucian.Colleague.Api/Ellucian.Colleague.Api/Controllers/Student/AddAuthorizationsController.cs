// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Waiver data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AddAuthorizationsController : BaseCompressedApiController
    {
        private readonly IAddAuthorizationService _addAuthorizationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Provides access to Student Waivers.
        /// </summary>
        /// <param name="addAuthorizationService"></param>
        /// <param name="logger"></param>
        public AddAuthorizationsController(IAddAuthorizationService addAuthorizationService, ILogger logger)
        {
            _addAuthorizationService = addAuthorizationService;
            _logger = logger;
        }

        /// <summary>
        /// Update an existing add authorization record.
        /// </summary>
        /// <param name="addAuthorization">Dto containing add authorization being updated.</param>
        /// <returns>Updated <see cref="AddAuthorization">AddAuthorization</see> DTO.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update an add authorization for this section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if the record cannot be updated due to a lock.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>
        /// This action can only be performed by a student who is assigning themselves to a previously unassigned add authorization code, or
        /// by a faculty member assigned to the section on the add authorization being submitted.
        /// </accessComments>
        [HttpPut]
        public async Task<AddAuthorization> PutAddAuthorizationAsync([FromBody] AddAuthorization addAuthorization)
        {
            if (addAuthorization == null)
            {
                string errorText = "Must provide the add authorization item to update.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _addAuthorizationService.UpdateAddAuthorizationAsync(addAuthorization);
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
            catch (System.Collections.Generic.KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Add Authorization not found.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves Add Authorizations for a specific section
        /// </summary>
        /// <param name="sectionId">The id of the section</param>
        /// <returns>The <see cref="AddAuthorization">Add Authorizations</see> for the section.</returns>
        /// <accessComments>
        /// Only permitted for faculty members assigned to the section.
        /// </accessComments>
        [ParameterSubstitutionFilter]
        [HttpGet]
        public async Task<IEnumerable<AddAuthorization>> GetSectionAddAuthorizationsAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                string errText = "Section Id must be provided to get section add authorizations.";
                _logger.Error(errText);
                throw CreateHttpResponseException(errText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _addAuthorizationService.GetSectionAddAuthorizationsAsync(sectionId);
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
        /// Create a new add authorization for a student in a section.
        /// </summary>
        /// <param name="addAuthorizationInput"><see cref="AddAuthorizationInput">Add Authorization Input</see> with information on creating a new authorization.</param>
        /// <returns>Newly created <see cref="AddAuthorization">Add Authorization</see>.</returns>
        /// <accessComments>
        /// This action can only be performed by a faculty member assigned to the section.
        /// </accessComments>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="AddAuthorization">add authorization</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create an add authorization for this section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if an unrevoked authorization already exists for the student in the section.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other creation problem.</exception>
        /// <accessComments>
        /// This action can only be performed by a faculty member assigned to the section.
        /// </accessComments>
        [HttpPost]
        public async Task<HttpResponseMessage> PostAddAuthorizationAsync([FromBody] AddAuthorizationInput addAuthorizationInput)
        {
            if (addAuthorizationInput == null)
            {
                string errorText = "Must provide the add authorization input item to create a new authorization.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                AddAuthorization newAuthorization = await _addAuthorizationService.CreateAddAuthorizationAsync(addAuthorizationInput);
                var response = Request.CreateResponse<AddAuthorization>(HttpStatusCode.Created, newAuthorization);
                SetResourceLocationHeader("GetAddAuthorization", new { id = newAuthorization.Id });
                return response;
            }
            catch (PermissionsException pe)
            {
                _logger.Info(pe.ToString());
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException re)
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

        /// <summary>
        /// Retrieves information about a specific add authorization.
        /// </summary>
        /// <param name="id">Unique system Id of the add authorization</param>
        /// <returns>An <see cref="AddAuthorization">Add Authorization.</see></returns>
        /// <accessComments>
        /// This action can only be performed by the student who is assigned to the authorization, or by 
        /// a faculty member assigned to the section on the authorization.
        /// </accessComments>
        [ParameterSubstitutionFilter]
        public async Task<AddAuthorization> GetAsync(string id)
        {
            try
            {
                var addAuthorization = await _addAuthorizationService.GetAsync(id);
                if (addAuthorization == null)
                {
                    throw CreateNotFoundException("AddAuthorization", id);
                }
                return addAuthorization;
            }
            catch (PermissionsException)
            {
                throw CreateHttpResponseException("Not permitted to view Add Authorization.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException kex)
            {
                this._logger.Error(kex, "Unable to retrieve add authorization Id " + id);
                throw CreateHttpResponseException("Add Authorization not found.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieve add authorizations for a student
        /// </summary>
        /// <param name="studentId">ID of the student for whom add authorizations are being retrieved</param>
        /// <returns>Add Authorizations for the student</returns>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have at least one of the following permissions can request other users' data:
        /// ALL.ACCESS.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// VIEW.ANY.ADVISEE
        /// ALL.ACCESS.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// UPDATE.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// REVIEW.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// VIEW.ASSIGNED.ADVISEES (the student must be an assigned advisee for the user)
        /// </accessComments>
        public async Task<IEnumerable<AddAuthorization>> GetStudentAddAuthorizationsAsync(string studentId)
        {
            try
            {
                var notices = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(studentId);
                return notices;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permission to retrieve add authorizations for student.", HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException("Unable to retrieve add authorizations for student.", HttpStatusCode.BadRequest);
            }
        }

    }
}
