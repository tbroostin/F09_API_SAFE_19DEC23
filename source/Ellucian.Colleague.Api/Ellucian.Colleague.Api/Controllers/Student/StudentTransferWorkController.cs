// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.TransferWork;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
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

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Student Transfer Work related data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTransferWorkController : BaseCompressedApiController
    {
        private readonly ITransferWorkService _transferWorkService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentTransferWorkController class.
        /// </summary>
        /// <param name="transferWorkService">Service of type <see cref="ITransferWorkService">ITransferWorkServices</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentTransferWorkController(ITransferWorkService transferWorkService, ILogger logger)
        {
            _transferWorkService = transferWorkService;
            _logger = logger;
        }

        /// <summary>
        /// Get student transfer equivalency work for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the student id is not provided to get the transfer summary data.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve transfer summary data.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
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
        /// <returns>Returns a list of transfer equivalencies for a student.</returns>
        [HttpGet]
        public async Task<IEnumerable<TransferEquivalencies>> GetStudentTransferWorkAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                string errorText = "Student id must be specified to get the Transfer Summary";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _transferWorkService.GetStudentTransferWorkAsync(studentId);
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                _logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student transfer and non course equivelancy work";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.Error(e, string.Format("Could not get the student transfer and non course equivelancy work for {0}", studentId));
                throw CreateHttpResponseException("Could not get the student transfer and non course equivelancy work", HttpStatusCode.BadRequest);
            }
        }
    }
}
