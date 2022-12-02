// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
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
    /// Provides access to Graduation Application Information.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class GraduationApplicationsController : BaseCompressedApiController
    {
        private readonly IGraduationApplicationService _graduationApplicationService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the GraduationApplicationController class.
        /// </summary>
        /// <param name="graduationApplicationService">Graduation Application Service</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public GraduationApplicationsController(IGraduationApplicationService graduationApplicationService, ILogger logger)
        {
            _graduationApplicationService = graduationApplicationService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a graduation application by student Id and program code asynchronously
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">Graduation Application program code</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object.</returns>
        /// <accessComments>
        /// A single graduation application can only be retrieved by the student who submitted the application.
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        public async Task<Dtos.Student.GraduationApplication> GetGraduationApplicationAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentException("Graduation Application is missing student Id and/or program Id required for retrieval.");
            }
            try
            {
                return await _graduationApplicationService.GetGraduationApplicationAsync(studentId, programCode);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Graduation Application is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid Graduation Application Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the requested graduation application.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new Graduation Application asynchronously. Student must be pass any applicable graduation application eligibility rules and must not
        /// have previously submitted an application for the same program. 
        /// </summary>
        /// <param name="studentId">Student id passed through url</param>
        /// <param name="programCode">Program Code passed through url</param>
        /// <param name="graduationApplication">GraduationApplication dto object</param>
        /// <returns>
        /// If successful, returns the newly created Graduation Application in an http response with resource locator information. 
        /// If failure, returns the exception information. If failure due to existing Graduation Application already exists for the given student and program,
        /// it also returns resource locator to use to retrieve the existing item.
        /// </returns>
        /// <accessComments>
        /// A graduation application can only be created by the student applying for graduation.
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        public async Task<HttpResponseMessage> PostGraduationApplicationAsync(string studentId, string programCode, [FromBody]Dtos.Student.GraduationApplication graduationApplication)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplication == null)
            {
                throw new ArgumentNullException("graduationApplication", "Graduation Application object must be provided.");
            }

            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(graduationApplication.StudentId) || string.IsNullOrEmpty(graduationApplication.ProgramCode))
            {
                throw new ArgumentException("Graduation Application is missing a required property.");
            }
            try
            {
                Dtos.Student.GraduationApplication createdApplicationDto = await _graduationApplicationService.CreateGraduationApplicationAsync(graduationApplication);
                var response = Request.CreateResponse<Dtos.Student.GraduationApplication>(HttpStatusCode.Created, createdApplicationDto);
                SetResourceLocationHeader("GetGraduationApplication", new { studentId = createdApplicationDto.StudentId, programCode = createdApplicationDto.ProgramCode });
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException gaex)
            {
                _logger.Info(gaex.ToString());
                SetResourceLocationHeader("GetGraduationApplication", new { studentId = graduationApplication.StudentId, programCode = graduationApplication.ProgramCode });
                throw CreateHttpResponseException(gaex.Message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieve list of all the graduation applications  submitted for  the student
        /// </summary>
        /// <param name="studentId">Id of the Student</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">List of Graduation Application</see></returns>
        /// <accessComments>
        /// Graduation Applications for the student can be retrieved only if:
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
        ///  Privacy is enforced by this response. If any student has an assigned privacy code that the advisor is not authorized to access, the GraduaionApplication response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned. In this situation, 
        /// all details except the student Id are cleared from the specific GraduationApplication object.
        /// </accessComments>
        public async Task<IEnumerable<Dtos.Student.GraduationApplication>> GetGraduationApplicationsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("Student Id is required for retrieval.");
            }
            try
            {
                var privacyWrapper= await _graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                var graduationApplication = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.GraduationApplication>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return graduationApplication;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Graduation Application is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid Student Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the graduation applications.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates an existing Graduation Application asynchronously.
        /// </summary>
        /// <param name="studentId">Student id passed through url</param>
        /// <param name="programCode">Program Code passed through url</param>
        /// <param name="graduationApplication">GraduationApplication dto object</param>
        /// <returns>
        /// If successful, returns the updated Graduation Application in an http response. 
        /// If failure, returns the exception information. 
        /// </returns>
        /// <accessComments>
        /// A graduation application can only be updated by the student of the application.
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        public async Task<HttpResponseMessage> PutGraduationApplicationAsync(string studentId, string programCode, [FromBody]Dtos.Student.GraduationApplication graduationApplication)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplication == null)
            {
                throw new ArgumentNullException("graduationApplication", "Graduation Application object must be provided.");
            }
            graduationApplication.StudentId = studentId;
            graduationApplication.ProgramCode = programCode;
            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrWhiteSpace(graduationApplication.StudentId) || string.IsNullOrWhiteSpace(graduationApplication.ProgramCode))
            {
                throw new ArgumentException("Graduation Application is missing a required property.");
            }

            try
            {
                Dtos.Student.GraduationApplication updatedApplicationDto = await _graduationApplicationService.UpdateGraduationApplicationAsync(graduationApplication);
                var response = Request.CreateResponse<Dtos.Student.GraduationApplication>(HttpStatusCode.OK, updatedApplicationDto);
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid Graduation Application Id specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves graduation application fee and payment information for a specific student Id and programCode
        /// </summary>
        /// <param name="studentId">Id of the student to retrieve</param>
        /// <param name="programCode">Program code for the specified graduation application</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplicationFee">Graduation Application Fee</see> object.</returns>
        /// <accessComments>Users may request their own data.</accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        public async Task<Dtos.Student.GraduationApplicationFee> GetGraduationApplicationFeeAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentException("Student Id and program Id are required to get application fee information.");
            }
            try
            {
                return await _graduationApplicationService.GetGraduationApplicationFeeAsync(studentId, programCode);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the graduation application fee for this program.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Checks to see if the student is eligible to apply for graduation in specific programs.
        /// </summary>
        /// <param name="criteria">Identifies the student and the programs for which eligibility is requested</param>
        /// <returns>A list of <see cref="GraduationApplicationProgramEligibility">Graduation Application Program Eligibility </see> items</returns>
        /// <accessComments>
        /// Graduation Application Eligibility for the student can be retrieved only if:
        /// 1. A Student is requesting their own eligibility or
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
        [HttpPost]
        public async Task<IEnumerable<GraduationApplicationProgramEligibility>> QueryGraduationApplicationEligibilityAsync(GraduationApplicationEligibilityCriteria criteria)
        {
            if (criteria == null)
            {
                _logger.Error("Missing graduation application eligibility criteria");
                throw CreateHttpResponseException("Missing graduation application eligibility criteria", HttpStatusCode.BadRequest);
            }

            try
            {
                return await _graduationApplicationService.GetGraduationApplicationEligibilityAsync(criteria.StudentId, criteria.ProgramCodes);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}