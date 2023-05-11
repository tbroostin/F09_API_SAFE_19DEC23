// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Student Records Release related services
    /// </summary>
    public interface IStudentRecordsReleaseService
    {
        /// <summary>
        /// Add student release records information for a student
        /// </summary>
        /// <param name="StudentRecordsReleaseInfo">The StudentRecordsReleaseInfo to create</param>
        /// <returns>Added StudentRecordsReleaseInfo</returns>
        Task<Dtos.Student.StudentRecordsReleaseInfo> AddStudentRecordsReleaseInfoAsync(Dtos.Student.StudentRecordsReleaseInfo studentRecordsReleaseInfo);

        /// <summary>
        /// Delete a student records release information for a student
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="studentReleaseId">Student Release Id</param>
        /// <returns>StudentRecordsReleaseInfo object</returns>
        Task<Dtos.Student.StudentRecordsReleaseInfo> DeleteStudentRecordsReleaseInfoAsync(string studentId, string studentReleaseId);

        /// <summary>
        /// Update student release records information for a student
        /// </summary>
        /// <param name="StudentRecordsReleaseInfo">The StudentRecordsReleaseInfo to update</param>
        /// <returns>StudentRecordsReleaseInfo entity that was just updated</returns>
        Task<Dtos.Student.StudentRecordsReleaseInfo> UpdateStudentRecordsReleaseInfoAsync(Dtos.Student.StudentRecordsReleaseInfo studentRecordsReleaseInfo);

        /// <summary>
        /// retrieves student records release information asynchronously
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>A collection of <see cref="Dtos.Student.StudentRecordsReleaseInfo"></see>object.</returns>
        Task<IEnumerable<Dtos.Student.StudentRecordsReleaseInfo>> GetStudentRecordsReleaseInformationAsync(string studentId);

        /// <summary>
        /// retrieves student records release deny access information
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>The <see cref="Dtos.Student.StudentRecordsReleaseDenyAccess"></see></returns>
        Task<Dtos.Student.StudentRecordsReleaseDenyAccess> GetStudentRecordsReleaseDenyAccessAsync(string studentId);

        /// <summary>
        /// Deny access to student records release information 
        /// </summary>
        /// <param name="studentRecordsRelDenyAccess">The student records release deny access information for denying access to student records release</param>
        /// <returns>A collection of updated<see cref="Dtos.Student.StudentRecordsReleaseInfo"></see>object.</returns>
        Task<IEnumerable<Dtos.Student.StudentRecordsReleaseInfo>> DenyStudentRecordsReleaseAccessAsync(Dtos.Student.DenyStudentRecordsReleaseAccessInformation studentRecordsRelDenyAccess);

        /// <summary>
        /// Gets the student records release information based on the Id
        /// </summary>
        /// <param name="studentRecordsReleaseId"></param>
        /// <returns>StudentRecordsReleaseInfo object for the requested Id</returns>
        Task<Dtos.Student.StudentRecordsReleaseInfo> GetStudentRecordsReleaseInfoByIdAsync(string studentRecordsReleaseId);

    }
}
