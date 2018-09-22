// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IOrganizationalRelationshipRepository
    {
        // Add
        // return type, method name, parameter list
        /// <summary>
        /// Add a relationship
        /// </summary>
        /// <param name="organizationalRelationshipEntity">Organizational relationship entity</param>
        /// <returns>Added organizationalRelationship</returns>
        Task<OrganizationalRelationship> AddAsync(OrganizationalRelationship organizationalRelationshipEntity);

        // Update
        /// <summary>
        /// Update a relationship
        /// </summary>
        /// <param name="organizationalRelationshipEntity">Organizational relationship entity</param>
        /// <returns>Updated organizationalRelationship</returns>
        Task<OrganizationalRelationship> UpdateAsync(OrganizationalRelationship organizationalRelationshipEntity);

        // Delete
        /// <summary>
        /// Delete a relationship
        /// </summary>
        /// <param name="id">Organizational relationship entity</param>
        Task DeleteAsync(string id);
    }
}
