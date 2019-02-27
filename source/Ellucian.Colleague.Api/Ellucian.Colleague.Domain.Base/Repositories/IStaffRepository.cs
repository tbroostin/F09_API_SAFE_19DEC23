// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for staff repositories
    /// </summary>
    public interface IStaffRepository
    {
        /// <summary>
        /// Gets the specified staff.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Entities.Staff Get(string id);

        /// <summary>
        /// Gets the specified staff.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<Entities.Staff> GetAsync(string id);

        /// <summary>
        /// Get a list of staff records associated with provided ids
        /// </summary>
        /// <param name="ids">List of identifiers</param>
        /// <returns>List of Staff Entities</returns>
        IEnumerable<Staff> Get(IEnumerable<string> ids);

        /// <summary>
        /// Get a list of staff records associated with provided ids
        /// </summary>
        /// <param name="ids">List of identifiers</param>
        /// <returns>List of Staff Entities</returns>
        Task<IEnumerable<Staff>> GetAsync(IEnumerable<string> ids);

        /// <summary>
        /// Gets the staff login ID for a person.
        /// </summary>
        /// <param name="personId">The person ID for whom to retrieve the staff login ID.</param>
        /// <returns>A string containing the staff login ID.</returns>
        Task<string> GetStaffLoginIdForPersonAsync(string personId);
    }
}
