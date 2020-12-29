// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Accesses Colleague for a person's relationship information.
    /// </summary>
    public interface IRelationshipRepository : IEthosExtended
    {
        /// <summary>
        /// Retrieves an enumeration of the given person's relationships with other persons or organizations
        /// </summary>
        /// <param name="personId">The identifier of the person of interest</param>
        /// <returns>Enumeration of <see cref="Relationship"/> relationships</returns>
        Task<IEnumerable<Relationship>> GetPersonRelationshipsAsync(string id);

        /// <summary>
        /// Gets a collection of unique IDs for persons and organizations with whom the supplied person has a relationship
        /// </summary>
        /// <param name="id">The identifier of the entity of interest</param>
        /// <returns>Collection of unique IDs for persons and organizations with whom the supplied person has a relationship</returns>
        Task<IEnumerable<string>> GetRelatedPersonIdsAsync(string id);

        /// <summary>
        /// Get a list of personal relationships using criteria
        /// </summary>
        /// <returns>A list of personal relationships Entities</returns>
        Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetRelationshipsAsync(int offset, int limit, List<string> guardianRelTypesWithInverse,
            string subjectPerson, string relatedPerson, string directRelationshipType, string directRelationshipDetailId);

        /// <summary>
        /// Get a list of personal relationships using criteria
        /// </summary>
        /// <returns>A list of personal relationships Entities</returns>
        Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetRelationships2Async(int offset, int limit, string[] persons = null, string relationType = "", string inverseRelationType = "");

        /// <summary>
        /// Creates the given relationship type between the two given entities
        /// </summary>
        /// <param name="isTheId">P1, in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="relationshipType">the relationship type in the the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="ofTheId">P2, in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <returns>the created <see cref="Relationship"/></returns>
        Task<Relationship> PostRelationshipAsync(string isTheId, string relationshipType, string ofTheId);

        /// <summary>
        /// Gets all person relationships by page
        /// </summary>
        /// <param name="offset">offset</param>
        /// <param name="limit">limit</param>
        /// <param name="bypassCache">bypassCache true/false</param>
        /// <returns>Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int></returns>
        Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetAllAsync(int offset, int limit, bool bypassCache, List<string> guardianRelTypesWithInverse);

        /// <summary>
        /// Gets personal relationship by id
        /// </summary>
        /// <param name="id">guid</param>
        /// <returns>Domain.Base.Entities.Relationship</returns>
        Task<Domain.Base.Entities.Relationship> GetPersonRelationshipByIdAsync(string id);

        /// <summary>
        /// Gets personal relationship by id
        /// </summary>
        /// <param name="id">guid</param>
        /// <returns>Domain.Base.Entities.Relationship</returns>
        Task<Domain.Base.Entities.Relationship> GetPersonalRelationshipById2Async(string id);

        /// <summary>
        /// Gets guardian relationship types defaults
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetDefaultGuardianRelationshipTypesAsync(bool bypassCache);

        /// <summary>
        /// Get the relationship record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetPersonalRelationshipsIdFromGuidAsync(string guid);

        /// <summary>
        /// Gets guardian relationship by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Domain.Base.Entities.Relationship> GetPersonGuardianRelationshipByIdAsync(string id);

        /// <summary>
        /// Gets all guardian relationships
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="person"></param>
        /// <param name="guardianRelTypesWithInverse"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetAllGuardiansAsync(int offset, int limit, string person, List<string> guardianRelTypesWithInverse);

        /// <summary>
        /// Gets all non person relationships
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="orgId"></param>
        /// <param name="instId"></param>
        /// <param name="personCode"></param>
        /// <param name="relationshipTypeCode"></param>
        /// <param name="inverseRelationshipTypeCode"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>>GetNonPersonRelationshipsAsync(int offset, int limit, string orgId, string instId, string person, string relationshipType, string inverseRelationshipTypeCode, bool bypassCache);

        /// <summary>
        /// Gets nonpersonal relationship by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Domain.Base.Entities.Relationship</returns>
        Task<Domain.Base.Entities.Relationship> GetNonPersonRelationshipByIdAsync(string id);

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        Task<string> GetUnidataFormattedDate(string date);

        /// <summary>
        /// Update person relationship
        /// </summary>
        /// <param name="Relationship">personrelationship entity</param>
        /// <returns>Domain.Base.Entities.Relationship</returns>
        Task<Domain.Base.Entities.Relationship> UpdatePersonalRelationshipsAsync(Domain.Base.Entities.Relationship personRelationshipsEntity);

        Task<Tuple<Domain.Base.Entities.Relationship, string>> CreatePersonalRelationshipInitiationProcessAsync(Domain.Base.Entities.PersonalRelationshipInitiation personalRelationshipsEntity);

        /// <summary>
        /// Delete person relationship
        /// </summary>
        /// <param name="guid">personrelationship guid</param>
        /// <returns></returns>
        Task<Domain.Base.Entities.Relationship> DeletePersonRelationshipAsync(string guid);
    }
}
