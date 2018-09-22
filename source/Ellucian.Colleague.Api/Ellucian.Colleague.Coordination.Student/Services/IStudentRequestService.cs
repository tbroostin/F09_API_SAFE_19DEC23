// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for student request service
    /// </summary>
    public interface IStudentRequestService
    {
        /// <summary>
        /// Request- Transcript or Enrollment
        /// </summary>
        /// <param name="studentRequest"><see cref="Dtos.Student.StudentRequest"/>New student request object to add - it could be either transcript request or enrollment verification request</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        Task<Ellucian.Colleague.Dtos.Student.StudentRequest> CreateStudentRequestAsync(Dtos.Student.StudentRequest studentRequest);

        /// <summary>
        /// Retrieve the specified student request
        /// </summary>
        /// <param name="requestId">Id of the student request to retrieve</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
         Task<Dtos.Student.StudentRequest> GetStudentRequestAsync(string requestId);

        /// <summary>
        /// Retrieves the specific type of student requests for a student
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="requestType">Type of requests to retrieve - "Transript" or "Enrollment" (required)</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
         Task<List<Dtos.Student.StudentRequest>> GetStudentRequestsAsync(string studentId, string requestType);

         // <summary>
         /// Retrieve sstudent request fee information for the given student Id and request Id asynchronously.
         /// </summary>
         /// <param name="studentId">Id of the student</param>
         /// <param name="requestId">request Id in student request file</param>
         /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentRequestFee">Student Request</see> object that was created</returns>
         Task<Ellucian.Colleague.Dtos.Student.StudentRequestFee> GetStudentRequestFeeAsync(string studentId, string requestId);
    }
}


