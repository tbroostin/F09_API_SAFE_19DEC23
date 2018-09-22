// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordinates information to enable retrieval and update of student transcript requests and enrollment verification requests
    /// </summary>
    [RegisterType]
    public class StudentRequestService : BaseCoordinationService, IStudentRequestService
    {
        private readonly IStudentRequestRepository studentRequestRepository;

        /// <summary>
        /// Initialize the service for requests
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="studentWaiverRepository">request repository access</param>
        /// <param name="logger">error logging</param>
        public StudentRequestService(IAdapterRegistry adapterRegistry, IStudentRequestRepository studentRequestRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.studentRequestRepository = studentRequestRepository;
        }


        /// <summary>
        /// Request- Transcript or Enrollment
        /// </summary>
        /// <param name="studentRequest"><see cref="Dtos.Student.StudentRequest"/>New student request object to add - it could be either transcript request or enrollment verification request</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        public async Task<Dtos.Student.StudentRequest> CreateStudentRequestAsync(Dtos.Student.StudentRequest studentRequest)
        {
            List<string> RequiredParametersNames = new List<string>();
            // Throw exception if incoming student request is null
            if (studentRequest == null)
            {
                var message = string.Format("Student request object must be provided.");
                logger.Error(message);
                throw new ArgumentNullException("studentRequest", message);
            }

            if (string.IsNullOrEmpty(studentRequest.StudentId))
            {
                RequiredParametersNames.Add("StudentId");
            }
            if (string.IsNullOrEmpty(studentRequest.RecipientName))
            {
                RequiredParametersNames.Add("RecipientName");
            }
            if (studentRequest.MailToAddressLines == null || (studentRequest.MailToAddressLines != null && studentRequest.MailToAddressLines.Count <= 0))
            {
                RequiredParametersNames.Add("MailToAddressLines");

            }

            if (RequiredParametersNames != null && RequiredParametersNames.Count > 0)
            {
                string propertyNames = string.Join(",", RequiredParametersNames.ToArray());
                var message = string.Format("Student  request of type {0} is missing {1}  required properties.", studentRequest.GetType().ToString(), propertyNames.ToString());
                logger.Error(message);
                throw new ArgumentException("studentRequest", message);
            }
           
            // Throw exception if request is being submitted by someone other than the student.
            if (CurrentUser.PersonId != studentRequest.StudentId)
            {
                var message = string.Format("User does not have permissions to create a student request of type {0} for this student.", studentRequest.GetType().ToString());
                logger.Error(message);
                throw new PermissionsException(message);
            }
            Domain.Student.Entities.StudentRequest requestToAdd = null;
            try
            {
                //convert dto to domain - dto is abstract type, domain returned will also be abstract type
                var requestEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentRequest, Domain.Student.Entities.StudentRequest>();
                requestToAdd = requestEntityAdapter.MapToType(studentRequest );
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error converting incoming student request  Dto of type {0} to Domain Entity: ", studentRequest.GetType().ToString()) + ex.Message);
                throw;
            }
            try{

            Domain.Student.Entities.StudentRequest newRequest = await studentRequestRepository.CreateStudentRequestAsync(requestToAdd);

            var requestDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>();
            return  requestDtoAdapter.MapToType(newRequest);
            }
             
            catch (Exception ex)
            {
                logger.Info(ex, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Retrieve the specified student request
        /// </summary>
        /// <param name="requestId">Id of the student request to retrieve</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        public async Task<Dtos.Student.StudentRequest> GetStudentRequestAsync(string requestId)
        {
            // Throw argument null exception if request Id not provided
            if (string.IsNullOrEmpty(requestId))
            {
                var message = "StudentRequest Id must be provided";
                logger.Info(message);
                throw new ArgumentNullException(message);
            }

            // Get the request from the repository. 
            // Log and rethrow any general exception that occurs in the repository or during dto conversion.
            Domain.Student.Entities.StudentRequest studentRequest = null;
            try
            {
                studentRequest = await studentRequestRepository.GetAsync(requestId);
            }
            catch (KeyNotFoundException)
            {
                logger.Info("StudentRequest not found in repository for given request Id " + requestId);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read request from repository using student request id " + requestId + " Exception message: " + ex.Message;
                logger.Info(message);
                throw new Exception(message);
            }

            // Throw exception if item is being requested by someone other than the student.
            if (CurrentUser.PersonId != studentRequest.StudentId)
            {
                var message = string.Format("User does not have permission to get this student request.");
                logger.Error(message);
                throw new PermissionsException(message);
            }

            try
            {
                // Get the right adapter for the type mapping
                var requestDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>();
                return requestDtoAdapter.MapToType(studentRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting incoming student request Entity to Dto: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieve the specified types of student requests for a specific student
        /// </summary>
        /// <param name="studentId">Id of the student </param>
        /// <param name="requestType">Type of request to retrieve -- "Transcript" or "Enrollment" (required)</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        public async Task<List<Dtos.Student.StudentRequest>> GetStudentRequestsAsync(string studentId, string requestType)
        {
            // Throw argument null exception if student Id not provided
            if (string.IsNullOrEmpty(studentId))
            {
                var message = "Student Id must be provided";
                logger.Info(message);
                throw new ArgumentNullException(message);
            }

            // Throw argument null exception if request type not provided
            if (string.IsNullOrEmpty(requestType) || (requestType != "Transcript" && requestType != "Enrollment"))
            {
                var message = "Request type of 'Transcript' or 'Enrollment' must be provided";
                logger.Info(message);
                throw new ArgumentNullException(message);
            }
            // Throw exception if request is being submitted by someone other than the student.
            if (CurrentUser.PersonId != studentId)
            {
                var message = string.Format("User does not have permission to get student requests for this student.");
                logger.Error(message);
                throw new PermissionsException(message);
            }
            //Get the requests from the repository. 
            // Log and rethrow any general exception that occurs in the repository or during dto conversion.
            List<Domain.Student.Entities.StudentRequest> studentRequests = new List<Domain.Student.Entities.StudentRequest>();
            
            try
            {
                studentRequests = await studentRequestRepository.GetStudentRequestsAsync(studentId);
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read the student requests from repository using student id " + studentId + " Exception message: " + ex.Message;
                logger.Info(message);
                throw new Exception(message);
            }
            // Filter the retrieved requests by request type
            List<Domain.Student.Entities.StudentRequest> filteredStudentRequests = null;
            if (requestType == "Transcript")
            {
                filteredStudentRequests = studentRequests.Where(s => s.GetType() == typeof(StudentTranscriptRequest)).ToList();
            }
            else
            {
                filteredStudentRequests = studentRequests.Where(s => s.GetType() == typeof(StudentEnrollmentRequest)).ToList();
            }

            try
            {
                // Get the right adapter for the type mapping
                List<Dtos.Student.StudentRequest> studentRequestDtos = new List<Dtos.Student.StudentRequest>();
                var requestDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest>();
                foreach (var requestEntity in filteredStudentRequests)
                {
                    var requestDto = requestDtoAdapter.MapToType(requestEntity);
                    studentRequestDtos.Add(requestDto);
                }
                return studentRequestDtos;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting student request entities to Dtos: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieve sstudent request fee information for the given student Id and request Id asynchronously.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="requestId">request Id tin student request file</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentRequestFee">Student Request</see> object that was created</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.StudentRequestFee> GetStudentRequestFeeAsync(string studentId, string requestId)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(requestId))
            {
                var message = "Student Id and request Id must be provided";
                logger.Info(message);
                throw new ArgumentNullException(message);
            }
            Domain.Student.Entities.StudentRequestFee studentRequestFeeEntity = null;
            // Throw exception if request is being submitted by someone other than the student.
            if (CurrentUser.PersonId != studentId)
            {
                var message = string.Format("User does not have permission to get student requests  fees for this student.");
                logger.Error(message);
                throw new PermissionsException(message);
            }
            // Retrieve the request and verify that it is a request for the student - if not you should not be able to get the corresponding fees.
            StudentRequest studentRequestEntity;
            try
            {
                studentRequestEntity = await studentRequestRepository.GetAsync(requestId);
            }
            catch (Exception)
            {
                // if get of the request isn't successful (not found or error), don't get fees.
                throw;
            }
            if (studentRequestEntity.StudentId != studentId)
            {
                var message = string.Format("User does not have permission to get student requests fees for a request that does not belong to them.");
                logger.Error(message);
                throw new PermissionsException(message);
            }
            // Now that permissions have been checked make get and return the fees.
            try
            {
                studentRequestFeeEntity = await studentRequestRepository.GetStudentRequestFeeAsync(studentId, requestId);
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to get student request fee from repository using  student Id {0} and request Id {1}  Exception message: ", studentId, requestId, ex.Message);
                logger.Info(ex, message);
                throw;
            }
            try
            {
                var applicationFeeDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRequestFee, Dtos.Student.StudentRequestFee>();
                var studentRequestFeeDto = applicationFeeDtoAdapter.MapToType(studentRequestFeeEntity);
                return studentRequestFeeDto;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting student request fee Entity to Dto: " + ex.Message);
                throw;
            }
        }
    }
}
