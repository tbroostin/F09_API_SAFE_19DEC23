// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IOrganizationalPositionRelationshipRepository
    {
        /// <summary>
        /// Add a position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationshipEntity">Organizational position relationship entity</param>
        /// <returns>Added organizational position relationship</returns>
        Task<OrganizationalPositionRelationship> AddAsync(OrganizationalPositionRelationship organizationalPositionRelationshipEntity);

        /// <summary>
        /// Delete a relationship
        /// </summary>
        /// <param name="organizationalPositionRelationshipId">Organizational position relationship ID</param>
        Task DeleteAsync(string organizationalPositionRelationshipId);
    }
}
