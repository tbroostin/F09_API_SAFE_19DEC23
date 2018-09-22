// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for person repositories
    /// </summary>
    public interface IPersonRepository : IEthosExtended
    {
        /// <summary>
        /// Get a person entity by guid, without caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        Task<Person> GetPersonByGuidNonCachedAsync(string guid);

        /// <summary>
        /// Get a person entity by guid, without caching for persons-credentials
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        Task<Person> GetPersonCredentialByGuidNonCachedAsync(string guid);

        /// <summary>
        /// Get a person entities by guids, without caching.
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        Task<IEnumerable<Person>> GetPersonByGuidNonCachedAsync(IEnumerable<string> guids);
        /// <summary>
        /// Get a person integration entity by guid, without caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        Task<PersonIntegration> GetPersonIntegrationByGuidNonCachedAsync(string guid);

        /// <summary>
        /// Get a person integration entity by guid, with caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        Task<PersonIntegration> GetPersonIntegrationByGuidAsync(string guid, bool bypassCache);

        /// <summary>
        /// Get a person integration entity by guid, with caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        Task<PersonIntegration> GetPersonIntegration2ByGuidAsync(string guid, bool bypassCache);

        /// <summary>
        /// Get a person integration entities by guids, without caching.
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        Task<IEnumerable<PersonIntegration>> GetPersonIntegrationByGuidNonCachedAsync(IEnumerable<string> guids);

        /// <summary>
        /// Get a person integration entities by guids, without caching
        /// for person-credentials information only
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        Task<IEnumerable<PersonIntegration>> GetPersonCredentialsIntegrationByGuidNonCachedAsync(IEnumerable<string> guids);

        /// <summary>
        /// Get a person integration entities by guids, without caching V12.
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        Task<IEnumerable<PersonIntegration>> GetPersonIntegration2ByGuidNonCachedAsync(IEnumerable<string> guids);

        /// <summary>
        /// Get the person ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        Task<string> GetPersonIdFromGuidAsync(string guid);

        /// <summary>
        /// Get the address ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        Task<string> GetAddressIdFromGuidAsync(string guid);

        /// <summary>
        /// Get the GUID from a person ID
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>The GUID</returns>
        Task<string> GetPersonGuidFromIdAsync(string personId);

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="person">The <see cref="Dtos.Person">person</see> to create in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the person in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the person in the database.</param>
        /// <returns>The newly created <see cref="PersonIntegration">person</see> entity</returns>
        Task<PersonIntegration> Create2Async(PersonIntegration person, IEnumerable<Address> addresses, IEnumerable<Phone> phones);

        /// <summary>
        /// Create a organization.
        /// </summary>
        /// <param name="personOrg">The <see cref="Dtos.Organization2">person</see> to create in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the organization in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the organization in the database.</param>
        /// <returns>The newly created <see cref="PersonIntegration">organization</see> entity</returns>
        Task<PersonIntegration> CreateOrganizationAsync(PersonIntegration personOrg, IEnumerable<Address> addresses, IEnumerable<Phone> phones);

        /// <summary>
        /// Update a organization.
        /// </summary>
        /// <param name="personOrg">The  organization entity to update in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the person in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the person in the database.</param>
        /// <returns>The newly updated <see cref="PersonIntegration">organization</see> entity</returns>
        Task<PersonIntegration> UpdateOrganizationAsync(PersonIntegration person, IEnumerable<Address> addresses, IEnumerable<Phone> phones);

        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="person">The person entity to update in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the person in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the person in the database.</param>
        /// <returns>The newly updated <see cref="PersonIntegration">person</see> entity</returns>
        Task<PersonIntegration> Update2Async(PersonIntegration person, IEnumerable<Address> addresses, IEnumerable<Phone> phones);


        /// <summary>
        /// Search for matching person records.
        /// </summary>
        /// <param name="person"><see cref="Person">Person</see> to use for matching</param>
        /// <returns>List of person Ids</returns>
        Task<IEnumerable<string>> GetMatchingPersonsAsync(Person person);

        /// <summary>
        /// Returns the actual results of the matching algorithm for the supplied search criteria
        /// </summary>
        /// <param name="criteria">The <see cref="PersonMatchCriteria">criteria</see> to use when searching for people</param>
        /// <returns>A list containing the <see cref="PersonMatchResults">results</see> of the matching algorithm</returns>
        Task<IEnumerable<PersonMatchResult>> GetMatchingPersonResultsAsync(PersonMatchCriteria criteria);

        /// <summary>
        /// Determine if the identifier represents a valid person
        /// </summary>
        /// <param name="personId">Identifier to test</param>
        /// <returns>True if the identifier represents a person in the system</returns>
        Task<bool> IsPersonAsync(string personId);

        /// <summary>
        /// Determine if the identifier represents a valid organization
        /// </summary>
        /// <param name="personId">Identifier to test</param>
        /// <returns>True if the identifier represents a person in the system</returns>
        Task<bool> IsCorpAsync(string personId);

        /// <summary>
        /// Determine if the person is a faculty member
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>True if the person is a faculty member, false otherwise</returns>
        Task<bool> IsFacultyAsync(string personId);

        /// <summary>
        /// Determine if the person is a student
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>True if the person is a student, false otherwise</returns>
        Task<bool> IsStudentAsync(string personId);

        /// <summary>
        /// Determine if the person is a faculty advisor
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>True if the person is a faculty advisor, false otherwise</returns>
        Task<bool> IsAdvisorAsync(string personId);

        /// <summary>
        /// Get a list of guids associated with faculty
        /// </summary>
        /// <returns>List of person guids associated with faculty</returns>
        Task<Tuple<IEnumerable<string>, int>> GetFacultyPersonGuidsAsync(int offset, int limit);

        /// <summary>
        /// Get a list of guids associated with persons after filtering
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="personFilterCriteria">Criteria filter object</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns>List of person Idss associated determined by filters</returns>

        Task<Tuple<IEnumerable<string>, int>> GetFilteredPerson2GuidsAsync(int offset, int limit, bool bypassCache, PersonFilterCriteria personFilterCriteria, string personFilter);

        /// <summary>
        /// Get a list of guids associated with organization after filtering
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="role">Specific role of a organization</param>
        /// <param name="credentialType">Credential type of colleagueId</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <returns>List of organization guids associated determined by filters</returns>
        Task<Tuple<IEnumerable<string>, int>> GetFilteredOrganizationGuidsAsync(int offset, int limit, 
            string role, string credentialType, string credentialValue);

        /// <summary>
        /// Get person addresses, email addresses and phones used for integration.
        /// </summary>
        /// <param name="personId">Person's Colleague ID</param>
        /// <param name="emailAddresses">List of <see cref="EmailAddress"> email addresses</see></param>
        /// <param name="phones">List of <see cref="Phone"> phones</see></param>
        /// <param name="addresses">List of <see cref="Address">addresses</see></param>
        /// <returns>Boolean where true is success and false otherwise</returns>
        Task<Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, bool>> GetPersonIntegrationDataAsync(string personId);

        /// <summary>
        /// Get person addresses, email addresses and phones used for integration.
        /// </summary>
        /// <param name="personId">Person's Colleague ID</param>
        /// <param name="emailAddresses">List of <see cref="EmailAddress"> email addresses</see></param>
        /// <param name="phones">List of <see cref="Phone"> phones</see></param>
        /// <param name="addresses">List of <see cref="Address">addresses</see></param>
        /// <returns>Boolean where true is success and false otherwise</returns>
        Task<Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, List<SocialMedia>, bool>> GetPersonIntegrationData2Async(string personId);

        /// <summary>
        /// Wrapper around async call. Used by other branches.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        bool IsPerson(string personId);

        ///// <summary>
        ///// Get a Place 
        ///// </summary>
        ///// <returns>A collection of Place entities</returns>
        //Task<IEnumerable<Place>> GetPlacesAsync();

        /// <summary>
        /// Get the Host Country from INTL.PARAMS INTERNATIONAL record in Colleague
        /// </summary>
        /// <returns>String representing either USA or CANADA</returns>
        Task<string> GetHostCountryAsync();

        /// <summary>
        /// Gets all paged person guids
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<string>, int>> GetPersonGuidsAsync(int offset, int limit, bool bypassCache);

        /// <summary>
        /// Using a collection of person ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="personIds">collection of person ids</param>
        /// <returns>Dictionary consisting of a personId (key) and guid (value)</returns>
        Task<Dictionary<string, string>> GetPersonGuidsCollectionAsync(IEnumerable<string> personIds);

        /// <summary>
        /// Gets all paged person guids with Filter
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<string>, int>> GetPersonGuidsFilteredAsync(int offset, int limit, Dictionary<string, string> credentials, bool bypassCache);

        /// <summary>
        /// Get the institution hierarchy address for 1098 tax forms.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<string>> Get1098HierarchyAddressAsync(string id);        

        /// <summary>
        /// Returns the collection of person pin entities
        /// </summary>
        /// <param name="personGuids"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonPin>> GetPersonPinsAsync(string[] personGuids);
    }
}