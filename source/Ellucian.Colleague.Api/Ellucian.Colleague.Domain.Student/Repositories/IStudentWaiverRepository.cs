// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for the Waiver repository
    /// </summary>
    public interface IStudentWaiverRepository
    {
        /// <summary>
        /// Returns all waiver information for all students for the given section
        /// </summary>
        /// <param name="sectionId">The ID of the section</param>
        /// <returns>List of waiver objects found for the given section</returns>
        Task<List<StudentWaiver>> GetSectionWaiversAsync(string sectionId);

        /// <summary>
        /// Creates a waiver for a specific student and section
        /// </summary>
        /// <param name="waiver">The waiver object to add</param>
        /// <returns>The waiver retrieved after it was added</returns>
        Task<StudentWaiver> CreateSectionWaiverAsync(StudentWaiver waiver);

        /// <summary>
        /// Returns the requested section waiver
        /// </summary>
        /// <param name="waiverId"></param>
        /// <returns></returns>
        Task<StudentWaiver> GetAsync(string waiverId);

        /// <summary>
        /// Returns all waiver information for the given student Id
        /// </summary>
        /// <param name="sectionId">The ID of the student</param>
        /// <returns>List of waiver objects found for the given section</returns>
        Task<List<StudentWaiver>> GetStudentWaiversAsync(string studentId);

    }
}
