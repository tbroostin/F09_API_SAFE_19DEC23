// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Organizational position relationship service
    /// </summary>
    public interface IOrganizationalPositionRelationshipService
    {
        /// <summary>
        /// Adds a new organizational position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationship">organizational position relationship to add</param>
        /// <returns>The newly created organizational position relationship</returns>
        Task<Dtos.Base.OrganizationalPositionRelationship> AddAsync(Dtos.Base.OrganizationalPositionRelationship organizationalPositionRelationship);

        /// <summary>
        /// Deletes an organizational position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationshipId">Organizational position relationship id to delete</param>
        Task DeleteAsync(string organizationalPositionRelationshipId);
    }
}
