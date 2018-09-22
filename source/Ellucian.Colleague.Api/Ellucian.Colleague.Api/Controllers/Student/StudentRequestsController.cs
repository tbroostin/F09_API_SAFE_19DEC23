// Copyright 2016 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json;
using Ellucian.Colleague.Api.Converters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to student request data for transcript requests and enrollment verification requests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentRequestsController : BaseCompressedApiController
    {
        private readonly IStudentRequestService _requestService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initiatlize controller for StudentRequests.
        /// </summary>
        /// <param name="requestService">Student request service of type <see cref="IStudentRequestService"/></param>
        /// <param name="logger">logger of type <see cref="ILogger"/>ILogger</param>
        public StudentRequestsController(IStudentRequestService requestService, ILogger logger)
        {
            _requestService = requestService;
            _logger = logger;
        }


        /// <summary>
        /// Create a transcript request for a student
        /// </summary>
        /// <param name="transcriptRequest">The transcript request being added</param>
        /// <returns>Added <see cref="Dtos.Student.StudentTranscriptRequest">transcript request</see></returns>
        ///  <accessComments>
        /// Only the current user can create its own transcript request.
        /// </accessComments>
        [HttpPost]
        public async Task<HttpResponseMessage> PostStudentTranscriptRequestAsync([FromBody]Dtos.Student.StudentTranscriptRequest transcriptRequest)
        {
            List<string> RequiredParametersNames = new List<string>();
            // Throw exception if incoming student transcript request is null
            if (transcriptRequest == null)
            {
                throw new ArgumentNullException("transcriptRequest", "Student transcript request object must be provided.");
            }

            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(transcriptRequest.StudentId))
            {
                RequiredParametersNames.Add("StudentId");
            }
            if (string.IsNullOrEmpty(transcriptRequest.RecipientName))
            {
                RequiredParametersNames.Add("RecipientName");
            }
            if (transcriptRequest.MailToAddressLines == null || (transcriptRequest.MailToAddressLines != null && transcriptRequest.MailToAddressLines.Count <= 0))
            {
                RequiredParametersNames.Add("MailToAddressLines");

            }
            if (RequiredParametersNames != null && RequiredParametersNames.Count > 0)
            {
                string propertyNames = string.Join(",", RequiredParametersNames.ToArray());
                var message = string.Format("Student  Transcript request is missing {0}  required properties.", propertyNames.ToString());
                _logger.Error(message);
                throw new ArgumentException("StudentTranscriptRequest", message);
            }
            try
            {
                Dtos.Student.StudentTranscriptRequest createdRequestDto = await _requestService.CreateStudentRequestAsync(transcriptRequest) as Dtos.Student.StudentTranscriptRequest;
                var response = Request.CreateResponse<Dtos.Student.StudentTranscriptRequest>(HttpStatusCode.Created, createdRequestDto);
                SetResourceLocationHeader("GetStudentTranscriptRequest", new { requestId = createdRequestDto.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException gaex)
            {
                _logger.Info(gaex.ToString());
                SetResourceLocationHeader("GetStudentTranscriptRequest", new { id = gaex.ExistingResourceId });
                throw CreateHttpResponseException(gaex.Message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }


        }

        /// <summary>
        /// create enrollment verification requests
        /// </summary>
        /// <param name="enrollmentRequest"></param>
        /// <returns>Added <see cref="Dtos.Student.StudentEnrollmentRequest"/>Enrollment Request</returns>
        /// <accessComments>
        /// Only the current user can create its own enrollment request.
        /// </accessComments>
        [HttpPost]
        public async Task<HttpResponseMessage> PostStudentEnrollmentRequestAsync([FromBody]Dtos.Student.StudentEnrollmentRequest enrollmentRequest)
        {
            List<string> RequiredParametersNames = new List<string>();
            // Throw exception if incoming student enrollment verification request is nullenrollmentRequest
            if (enrollmentRequest == null)
            {
                throw new ArgumentNullException("enrollmentRequest", "Student enrollment verification request object must be provided.");
            }
            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(enrollmentRequest.StudentId))
            {
                RequiredParametersNames.Add("StudentId");
            }
            if (string.IsNullOrEmpty(enrollmentRequest.RecipientName))
            {
                RequiredParametersNames.Add("RecipientName");
            }
            if (enrollmentRequest.MailToAddressLines == null || (enrollmentRequest.MailToAddressLines != null && enrollmentRequest.MailToAddressLines.Count <= 0))
            {
                RequiredParametersNames.Add("MailToAddressLines");

            }

            if (RequiredParametersNames != null && RequiredParametersNames.Count > 0)
            {
                string propertyNames = string.Join(",", RequiredParametersNames.ToArray());
                var message = string.Format("Student Enrollment  request is missing {0}  required properties.", propertyNames.ToString());
                _logger.Error(message);
                throw new ArgumentException("StudentEnrollmentRequest", message);
            }
            try
            {
                Dtos.Student.StudentEnrollmentRequest createdRequestDto = await _requestService.CreateStudentRequestAsync(enrollmentRequest) as Dtos.Student.StudentEnrollmentRequest;
                var response = Request.CreateResponse<Dtos.Student.StudentEnrollmentRequest>(HttpStatusCode.Created, createdRequestDto);
                SetResourceLocationHeader("GetStudentEnrollmentRequest", new { requestId = createdRequestDto.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException gaex)
            {
                _logger.Info(gaex.ToString());
                SetResourceLocationHeader("GetStudentEnrollmentRequest", new { id = gaex.ExistingResourceId });
                throw CreateHttpResponseException(gaex.Message, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Get student transcript request
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns> <see cref="Dtos.Student.StudentTranscriptRequest"/>Student Transcript Request</returns>
        ///  <accessComments>
        /// Only a student to which request applies can access its own transcript request.
        /// </accessComments>
        public async Task<Dtos.Student.StudentTranscriptRequest> GetStudentTranscriptRequestAsync(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException("Request id for student transcript request retrieval is not provided.");
            }
            try
            {
                return await _requestService.GetStudentRequestAsync(requestId) as Dtos.Student.StudentTranscriptRequest;
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student transcript requests is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid request Id specified to retrieve student transcript request", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the student transcript request." + System.Net.HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Get student enrollment request
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns><see cref="Dtos.Student.StudentEnrollmentRequest"/> Student Enrollment Request </returns>
        /// <accessComments>
        /// Only a student to which request applies can access its own enrollment request.
        /// </accessComments>
        public async Task<Dtos.Student.StudentEnrollmentRequest> GetStudentEnrollmentRequestAsync(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException("Request id for student enrollment verification request retrieval is not provided.");
            }
            try
            {
                return await _requestService.GetStudentRequestAsync(requestId) as Dtos.Student.StudentEnrollmentRequest;
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student enrollment verification request is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid request Id specified to retrieve enrollment verification request.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the student enrollment verification request." + System.Net.HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Get all student enrollment requests for a student
        /// </summary>
        /// <param name="studentId">ID of the student</param>
        /// <returns>List of <see cref="Dtos.Student.StudentEnrollmentRequest">Enrollment Verification Request</see> objects for the student</returns>
        public async Task<List<Dtos.Student.StudentEnrollmentRequest>> GetStudentEnrollmentRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("Student id for student enrollment verification requests retrieval is required.");
            }
            try
            {
                List<Dtos.Student.StudentEnrollmentRequest> enrollmentRequests = new List<Dtos.Student.StudentEnrollmentRequest>();
                var studentRequests = await _requestService.GetStudentRequestsAsync(studentId, "Enrollment");
                foreach (var req in studentRequests)
                {
                    enrollmentRequests.Add(req as Dtos.Student.StudentEnrollmentRequest);
                }
                return enrollmentRequests;
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student enrollment verification requests is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the student enrollment verification requests." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all student transcript requests for a student
        /// </summary>
        /// <param name="studentId">ID of the student</param>
        /// <returns>List of <see cref="Dtos.Student.StudentTranscriptRequest">Transcript Request</see> objects for the student</returns>
        public async Task<List<Dtos.Student.StudentTranscriptRequest>> GetStudentTranscriptRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("Student id for student transcript requests retrieval is required.");
            }
            try
            {
                List<Dtos.Student.StudentTranscriptRequest> transcriptRequests = new List<Dtos.Student.StudentTranscriptRequest>();
                var studentRequests = await _requestService.GetStudentRequestsAsync(studentId, "Transcript");
                foreach (var req in studentRequests)
                {
                    transcriptRequests.Add(req as Dtos.Student.StudentTranscriptRequest);
                }
                return transcriptRequests;
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student transcript requests is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the student transcript requests." + System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves student request fee and distribution code for a specific student Id and request Id
        /// </summary>
        /// <param name="studentId">Id of the student to retrieve</param>
        /// <param name="requestId">Request Id for the specified student request</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentRequestFee">Student Request Fee</see> object.</returns>
        /// <accessComments>
        /// Only a student to which request applies can access its own request fees. 
        /// </accessComments>
        public async Task<Dtos.Student.StudentRequestFee> GetStudentRequestFeeAsync(string studentId, string requestId)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException("Student Id and Request Id are required to get student request fee information.");
            }
            try
            {
                return await _requestService.GetStudentRequestFeeAsync(studentId, requestId);
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student transcript requests fees is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving the student request fee for this program." + System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
