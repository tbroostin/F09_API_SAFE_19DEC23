// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface to Student Records Release functionality
    /// </summary>
    public interface IStudentRecordsReleaseRepository
    {
        /// <summary>
        /// Add student records release information for student
        /// </summary>
        /// <param name="studentRecordsRelease">The studentRecordsRelease to create</param>
        /// <returns>Added StudentRecordsReleaseInfo</returns>
        Task<StudentRecordsReleaseInfo> AddStudentRecordsReleaseInfoAsync(StudentRecordsReleaseInfo studentRecordsRelease);

        /// <summary>
        /// Delete a student records release information for a student
        /// </summary>
        /// <param name="studentReleaseId">Student Release Id</param>
        /// <returns>StudentRecordsReleaseInfo object</returns>
        Task<StudentRecordsReleaseInfo> DeleteStudentRecordsReleaseInfoAsync(string studentReleaseId);

        /// <summary>
        /// Update student records release information for student
        /// </summary>
        /// <param name="studentRecordsRelease">The studentRecordsRelease to update</param>
        /// <returns>StudentRecordsReleaseInfo object which was just updated</returns>
        Task<StudentRecordsReleaseInfo> UpdateStudentRecordsReleaseInfoAsync(StudentRecordsReleaseInfo studentRecordsRelease);

        /// <summary>
        /// Gets the student records release information based on the Id
        /// </summary>
        /// <param name="studentRecordsReleaseId"></param>
        /// <returns>StudentRecordsReleaseInfo object for the requested Id</returns>
        Task<StudentRecordsReleaseInfo> GetStudentRecordsReleaseInfoByIdAsync(string studentRecordsReleaseId);
        /// <summary>
        /// Get student records release information for the specified student.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>A collection of <see cref="StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see> object.</returns>
        Task<IEnumerable<StudentRecordsReleaseInfo>> GetStudentRecordsReleaseInfoAsync(string studentId);

        /// <summary>
        /// Get student records release deny access information for the specified student.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>The<see cref="StudentRecordsReleaseDenyAccess">StudentRecordsReleaseDenyAccess</see></returns>
        Task<StudentRecordsReleaseDenyAccess> GetStudentRecordsReleaseDenyAccessAsync(string studentId);

        /// <summary>
        /// Deny access to student records release information 
        /// </summary>
        /// <param name="studentRecordsRelDenyAccess">The student records release deny access information for denying access to student records release</param>
        /// <returns>A collection of updated<see cref="Dtos.Student.StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see> dto objects</returns>
        Task<IEnumerable<StudentRecordsReleaseInfo>> DenyStudentRecordsReleaseAccessAsync(DenyStudentRecordsReleaseAccessInformation studentRecordsRelDenyAccess);
    }
}
