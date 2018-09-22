// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface to AddAuthorizationRepository
    /// </summary>
    public interface IAddAuthorizationRepository
    {
        /// <summary>
        /// Updates an <see cref="AddAuthorization">add authorization</see> for a section
        /// </summary>
        /// <param name="addAuthorization">The add authorization to be updated</param>
        /// <returns>Updated <see cref="AddAuthorization">add authorization</see> for the section</returns>
        Task<AddAuthorization> UpdateAddAuthorizationAsync(AddAuthorization addAuthorization);

        /// <summary>
        /// Gets an add authorization by Id
        /// </summary>
        /// <param name="addAuthorizationId">Id of authorization to retrieve</param>
        /// <returns>Add Authorization entity</returns>
        Task<AddAuthorization> GetAsync(string addAuthorizationId);

        /// <summary>
        /// Gets an add authorization by Add Code
        /// </summary>
        /// <param name="sectionId">Section Id of the add authorization to retrieve (required)</param>
        /// <param name="addAuthorizationCode">Add Code of authorization to retrieve (required)</param>
        /// <returns>Add Authorization entity</returns>
        Task<AddAuthorization> GetAddAuthorizationByAddCodeAsync(string sectionId, string addAuthorizationCode);
        
        /// <summary>
        /// Retrieves add authorizations for a section
        /// </summary>
        /// <param name="sectionId">Id of section</param>
        /// <returns>Add Authorization for the section</returns>
        Task<IEnumerable<AddAuthorization>> GetSectionAddAuthorizationsAsync(string sectionId);

        /// <summary>
        /// Retrieves add authorizations for a student
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <returns>Add Authorizations for the student</returns>
        Task<IEnumerable<AddAuthorization>> GetStudentAddAuthorizationsAsync(string studentId);

        /// <summary>
        /// Create an add authorization information for a student in a section
        /// </summary>
        /// <param name="addAuthorization">The AddAuthorization to create</param>
        /// <returns>Created AddAuthorization</returns>
        Task<AddAuthorization> CreateAddAuthorizationAsync(AddAuthorization addAuthorizationInput);


    }
}
