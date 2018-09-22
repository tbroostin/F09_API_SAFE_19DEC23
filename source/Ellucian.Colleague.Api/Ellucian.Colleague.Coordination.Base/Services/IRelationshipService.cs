using System;
// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IRelationshipService
    {
        /// <summary>
        /// Returns the list of primary relationships that this person has with other persons or organizations
        /// </summary>
        /// <param name="id">the identifier of the person of interest</param>
        /// <returns>an enumeration of <see cref="Relationship"/> primary relationships</returns>
        Task<IEnumerable<Relationship>> GetPersonPrimaryRelationshipsAsync(string id);

        /// <summary>
        /// Creates the given relationship type between the two given entities
        /// </summary>
        /// <param name="isTheId">P1, in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="relationshipType">the relationship type in the the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="ofTheId">P2, in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <returns>the created <see cref="Relationship"/></returns>
        Task<Relationship> PostRelationshipAsync(string isTheId, string relationshipType, string ofTheId);
    }
}
