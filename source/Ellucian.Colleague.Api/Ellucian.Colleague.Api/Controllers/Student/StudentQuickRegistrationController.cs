﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.QuickRegistration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for student Colleague Self-Service Quick Registration operations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentQuickRegistrationController : BaseCompressedApiController
    {
        private readonly IStudentQuickRegistrationService _studentQuickRegistrationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentQuickRegistrationController class.
        /// </summary>
        /// <param name="studentQuickRegistrationService">Service of type <see cref="IStudentQuickRegistrationService">IStudentQuickRegistrationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentQuickRegistrationController(IStudentQuickRegistrationService studentQuickRegistrationService, ILogger logger)
        {

            _studentQuickRegistrationService = studentQuickRegistrationService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a given student's Colleague Self-Service Quick Registration information for the provided academic term codes. 
        /// If the Colleague Self-Service Quick Registration workflow is disabled then no quick registration sections will be returned for any academic terms.
        /// If the Colleague Self-Service Quick Registration workflow is enabled then planned course sections will be returned for any academic terms designated for quick registration. 
        /// </summary>
        /// <param name="studentId">ID of the student for whom Colleague Self-Service Quick Registration data will be retrieved</param>
        /// <returns>A <see cref="StudentQuickRegistration"/> object</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>        /// Authenticated users may retrieve their own Colleague Self-Service Quick Registration information.        /// </accessComments>
        public async Task<StudentQuickRegistration> GetStudentQuickRegistrationSectionsAsync([FromUri]string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("A student ID is required when retrieving a student's quick registration data.");
                throw CreateHttpResponseException("A student ID is required when retrieving a student's quick registration data.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentQuickRegistrationService.GetStudentQuickRegistrationAsync(studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString(), "Unable to retrieve Colleague Self-Service Quick Registration information.");
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}