// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
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
    /// Provides access to student registration options data (allowed grading types, etc).
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RegistrationOptionsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IRegistrationOptionsService registrationOptionsService;

        /// <summary>
        /// Initializes a new instance of the ProgramsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="registrationOptionsService">Repository of type <see cref="IRegistrationOptionsService">IRegistrationOptionsService</see></param>
        /// <param name="logger">An instance of a logger</param>
        public RegistrationOptionsController(IAdapterRegistry adapterRegistry, IRegistrationOptionsService registrationOptionsService, ILogger logger)
        {
            this.registrationOptionsService = registrationOptionsService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the registration options for a student 
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        ///  <accessComments>
        /// Registration Options for the student can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 3. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 4. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// </accessComments>
        public async Task<RegistrationOptions> GetAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("id cannot be null or empty");
            }
            try
            {
                var options = await registrationOptionsService.GetRegistrationOptionsAsync(studentId);
                return options;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving registration options.";
                logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student resource is forbidden. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("Student", studentId);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting student resource. See log for details");
            }
        }
    }
}
