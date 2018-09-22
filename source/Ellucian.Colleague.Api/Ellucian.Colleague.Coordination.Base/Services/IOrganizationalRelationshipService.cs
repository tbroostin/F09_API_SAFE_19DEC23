// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// A service for reading and updating organizational relationships
    /// </summary>
    public interface IOrganizationalRelationshipService
    {
        /// <summary>
        /// Add an organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">Organizational relationship</param>
        /// <returns>Added organizationalRelationship</returns>
        Task<OrganizationalRelationship> AddAsync(OrganizationalRelationship organizationalRelationship);

        /// <summary>
        /// Update an organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">Organizational relationship DTO</param>
        /// <returns>Updated organizationalRelationship</returns>
        Task<OrganizationalRelationship> UpdateAsync(OrganizationalRelationship organizationalRelationship);

        /// <summary>
        /// Delete an organizational relationship
        /// </summary>
        /// <param name="id">Organizational relationship ID</param>
        Task DeleteAsync(string id);
    }
}
