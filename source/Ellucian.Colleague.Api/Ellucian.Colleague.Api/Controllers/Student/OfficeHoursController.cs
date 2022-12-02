// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using System;
using slf4net;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.ComponentModel;
using System.Threading.Tasks;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Api.Client.Exceptions;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Faculty Office Hours
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class OfficeHoursController : BaseCompressedApiController
    {
        private readonly IOfficeHoursService _officeHoursService;
        private readonly ILogger _logger;

        /// <summary>
        /// Provides access to Faculty office hours.
        /// </summary>
        /// <param name="officeHoursService"></param>
        /// <param name="logger"></param>
        public OfficeHoursController(IOfficeHoursService officeHoursService, ILogger logger)
        {
            _officeHoursService = officeHoursService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new office hours for faculty.
        /// </summary>
        /// <param name="addofficeHours"><see cref="AddOfficeHours">Add office hours</see> with information on creating a new office hours.</param>
        /// <returns>Newly created <see cref="AddOfficeHours">Office hours</see>.</returns>
        /// <accessComments>
        /// This action can only be performed by either an advisor or a faculty member.
        /// An authenticated user (advisor) may perform this action , if they have one of the following 8 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="AddOfficeHours">office hours</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create office hours.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other creation problem.</exception>
        [HttpPost]
        public async Task<HttpResponseMessage> PostOfficeHoursAsync([FromBody] AddOfficeHours addofficeHours)
        {
            if (addofficeHours == null)
            {
                string errorText = "Must provide the add office hours input item to create a new office hours.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                AddOfficeHours newOfficeHours = await _officeHoursService.AddOfficeHoursAsync(addofficeHours);
                var response = Request.CreateResponse<AddOfficeHours>(HttpStatusCode.Created, newOfficeHours);
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
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while adding office hours";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update an existing office hours for faculty.
        /// </summary>
        /// <param name="updateOfficeHours"><see cref="UpdateOfficeHours">Update office hours</see> with information for updating office hours.</param>
        /// <returns>Newly created <see cref="AddOfficeHours">Office hours</see>.</returns>
        /// <accessComments>
        /// This action can only be performed by either an advisor or a faculty member.
        /// An authenticated user (advisor) may perform this action , if they have one of the following 8 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// <returns>An HttpResponseMessage which includes the updated <see cref="UpdateOfficeHours">office hours</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update office hours.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other update problem.</exception>
        [HttpPut]
        public async Task<UpdateOfficeHours> PutOfficeHoursAsync([FromBody] UpdateOfficeHours updateOfficeHours)
        {
            if (updateOfficeHours == null)
            {
                string errorText = "Must provide the office hours input item to update.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _officeHoursService.UpdateOfficeHoursAsync(updateOfficeHours);
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
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while updating office hours";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// delete office hours for faculty.
        /// </summary>
        /// <param name="deleteOfficeHours"><see cref="DeleteOfficeHours">Delete office hours</see> information for deleting office hours.</param>
        /// <returns>Deleted <see cref="DeleteOfficeHours">Office hours</see>.</returns>
        /// <accessComments>
        /// This action can only be performed by either an advisor or a faculty member.
        /// An authenticated user (advisor) may perform this action , if they have one of the following 8 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        /// <returns>An HttpResponseMessage which includes the deleted <see cref="DeleteOfficeHours">office hours</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create office hours.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if any other creation problem.</exception>
        [HttpPost]
        public async Task<DeleteOfficeHours> DeleteOfficeHoursAsync([FromBody] DeleteOfficeHours deleteOfficeHours)
        {
            if (deleteOfficeHours == null)
            {
                string errorText = "Must provide the office hours input item to delete.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            try
            {
                return await _officeHoursService.DeleteOfficeHoursAsync(deleteOfficeHours);
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
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while deleting office hours";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}