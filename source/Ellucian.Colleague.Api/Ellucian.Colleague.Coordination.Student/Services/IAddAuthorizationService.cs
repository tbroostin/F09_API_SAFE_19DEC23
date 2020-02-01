// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for sstudent petition to retrieve petitions and consents of a student
    /// </summary>
    public interface IAddAuthorizationService
    {
        /// <summary>
        /// Updates an add authorization item.
        /// </summary>
        /// <param name="addAuthorization">AddAuthoriation DTO to update</param>
        /// <returns><IEnumerable<Dtos.Student.AddAuthorization>>Updated AddAuthorization</returns>
        Task<Dtos.Student.AddAuthorization> UpdateAddAuthorizationAsync(Dtos.Student.AddAuthorization addAuthorization);

        /// <summary>
        /// Retrieves add authorizations for a specific section
        /// </summary>
        /// <param name="sectionId">Id of section</param>
        /// <returns>Add Authorizations</returns>
        Task<IEnumerable<Dtos.Student.AddAuthorization>> GetSectionAddAuthorizationsAsync(string sectionId);

        /// <summary>
        /// Create an add authorization information for a student in a section
        /// </summary>
        /// <param name="addAuthorization">The AddAuthorization to create</param>
        /// <returns>Created AddAuthorization</returns>
        Task<Dtos.Student.AddAuthorization> CreateAddAuthorizationAsync(Dtos.Student.AddAuthorizationInput addAuthorizationInput);

        /// <summary>
        /// Retrieve an add authorization item.
        /// </summary>
        /// <param name="addAuthorization">AddAuthoriation DTO to update</param>
        /// <returns><Dtos.Student.AddAuthorization>An AddAuthorization</returns>
        Task<Dtos.Student.AddAuthorization> GetAsync(string id);

        /// <summary>
        /// Retrieves add authorizations for a student
        /// </summary>
        /// <param name="studentId">id of student</param>
        /// <returns>Add Authorizations for the student.</returns>
        Task<IEnumerable<Dtos.Student.AddAuthorization>> GetStudentAddAuthorizationsAsync(string studentId);
    }
}
