// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for the Request repository
    /// </summary>
    public interface IStudentRequestRepository
    {

        /// <summary>
        /// Creates a student transcript request or enrollment verification request
        /// </summary>
        /// <param name="request">The request object to add. It could be either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see></param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        Task<StudentRequest> CreateStudentRequestAsync(StudentRequest request);

        /// <summary>
        /// Get the student transcript request or enrollment verification request
        /// </summary>
        /// <param name="requestId">Id of a student request log</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        Task<StudentRequest> GetAsync(string requestId);

        /// <summary>
        /// Get all student requests for a student. This list could be items of either transcript request or enrollment verification request type.
        /// </summary>
        /// <param name="studentId">Id of a student request log</param>
        /// <returns>A list of student requests for a student that are either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see></returns>
        Task<List<StudentRequest>> GetStudentRequestsAsync(string studentId);

        /// <summary>
        /// Returns the student request fee for given student Id and program Code asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="requestId">Request  Id</param>
        /// <returns> <see cref="StudentRequestFee"/>The requested student request fee object</returns>
        Task<StudentRequestFee> GetStudentRequestFeeAsync(string studentId, string requestId);
    }
}
