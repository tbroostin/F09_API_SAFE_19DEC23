// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Filters;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for person services
    /// </summary>
    public interface IPersonService : IBaseService
    {
        /// <summary>
        /// Get a HEDM V6 person by guid.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person2">person</see></returns>
        Task<Person2> GetPerson2ByGuidNonCachedAsync(string guid);


        /// <summary>
        /// Get a HEDM V6 person by guid.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="bypassCache"></param>
        /// <returns>The <see cref="Person2">person</see></returns>
        Task<Person2> GetPerson2ByGuidAsync(string guid, bool bypassCache);


        /// <summary>
        /// Get a person credentials by guid.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Dtos.PersonCredential">person credential</see></returns>
        Task<Dtos.PersonCredential> GetPersonCredentialByGuidAsync(string guid);

        /// <summary>
        /// Get a list of persons by guid without using a cache.
        /// </summary>
        /// <param name="guids">List of person guids</param>
        /// <returns>List of <see cref="Person2">persons</see></returns>
        Task<IEnumerable<Dtos.Person2>> GetPerson2ByGuidNonCachedAsync(IEnumerable<string> guids);

        /// <summary>
        /// Get a list of persons by guid without using a cache.
        /// </summary>
        /// <param name="guids">List of person guids</param>
        /// <returns>List of <see cref="Person3">persons</see></returns>
        Task<IEnumerable<Dtos.Person3>> GetPerson3ByGuidNonCachedAsync(IEnumerable<string> guids);

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="person">The <see cref="Person2">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Person2">person</see></returns>
        Task<Person2> CreatePerson2Async(Person2 person);

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="person">The <see cref="Person3">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Person3">person</see></returns>
        Task<Person3> CreatePerson3Async(Person3 person);

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="person">The <see cref="Person4">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Person4">person</see></returns>
        Task<Person4> CreatePerson4Async(Person4 person);

        /// <summary>
        /// Create a person for v12.1.10.
        /// </summary>
        /// <param name="person">The <see cref="Person5">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Person5">person</see></returns>
        Task<Person5> CreatePerson5Async(Person5 person);

        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="person">The <see cref="Person2">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Person2">person</see></returns>
        Task<Person2> UpdatePerson2Async(Person2 person);

        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="person">The <see cref="Person3">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Person3">person</see></returns>
        Task<Person3> UpdatePerson3Async(Person3 person);

        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="person">The <see cref="Person4">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Person4">person</see></returns>
        Task<Person4> UpdatePerson4Async(Person4 person);

        /// <summary>
        /// Update a person for v12.1.0
        /// </summary>
        /// <param name="person">The <see cref="Person5">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Person5">person</see></returns>
        Task<Person5> UpdatePerson5Async(Person5 person);

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person2">person</see> entity to query by.</param>
        /// <returns>List of matching <see cref="Person2">persons</see></returns>
        Task<IEnumerable<Person2>> QueryPerson2ByPostAsync(Person2 person);

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person3">person</see> entity to query by.</param>
        /// <returns>List of matching <see cref="Person3">persons</see></returns>
        Task<IEnumerable<Person3>> QueryPerson3ByPostAsync(Person3 person);

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person4">person</see> entity to query by.</param>
        /// <param name="bypassCache">flag to bypass cache for reads.</param>
        /// <returns>List of matching <see cref="Person4">persons</see></returns>
        Task<IEnumerable<Person4>> QueryPerson4ByPostAsync(Person4 person, bool bypassCache = false);

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person5">person</see> entity to query by.</param>
        /// <param name="bypassCache">flag to bypass cache for reads.</param>
        /// <returns>List of matching <see cref="Person5">persons</see></returns>
        Task<IEnumerable<Person5>> QueryPerson5ByPostAsync(Person5 person, bool bypassCache = false);

        /// <summary>
        /// Query person by criteria and return the results of the matching algorithm
        /// </summary>
        /// <param name="person">The <see cref="Dtos.Base.PersonMatchCriteria">criteria</see> to query by.</param>
        /// <returns>List of matching <see cref="Dtos.Base.PersonMatchResult">results</see></returns>
        Task<IEnumerable<Dtos.Base.PersonMatchResult>> QueryPersonMatchResultsByPostAsync(Dtos.Base.PersonMatchCriteria criteria);
                
        /// <summary>
        /// Get HEDM V6 person data associated to the specified filters
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass Cache</param>
        /// <param name="title">Specific title</param>
        /// <param name="firstName">Specific first name</param>
        /// <param name="middleName">Specific middle name</param>
        /// <param name="lastNamePrefix">Last name beings with</param>
        /// <param name="lastName">Specific last name</param>
        /// <param name="pedigree">Specific suffix</param>
        /// <param name="preferredName">Specific preferred name</param>
        /// <param name="role">Specific role of a person</param>
        /// <param name="credentialType">Credential type of either colleagueId or ssn</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns>List of Person2 objects for filtered persons only.</returns>
        Task<Tuple<IEnumerable<Dtos.Person2>, int>> GetPerson2NonCachedAsync(int Offset, int Limit, bool bypassCache,
            string title, string firstName, string middleName, string lastNamePrefix, string lastName, string pedigree,
            string preferredName, string role, string credentialType, string credentialValue, string personFilter);

        /// <summary>
        /// Get a person's profile data.
        /// </summary>
        /// <param name="personId">Id of the person</param>
        /// <returns><see cref="Profile"/>Profile data for a person</returns>
        Task<Dtos.Base.Profile> GetProfileAsync(string personId, bool useCache = true);

        /// <summary>
        /// Updates a person's profile data.
        /// </summary>
        /// <param name="profileDto">Profile dto to update</param>
        /// <returns>The updated Profile dto</returns>
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this method instead.")]
        Task<Dtos.Base.Profile> UpdateProfileAsync(Dtos.Base.Profile profileDto);

        /// <summary>
        /// Updates a person's profile data.
        /// </summary>
        /// <param name="profileDto">Profile dto to update</param>
        /// <returns>The updated Profile dto</returns>
        Task<Dtos.Base.Profile> UpdateProfile2Async(Dtos.Base.Profile profileDto);

        /// <summary>
        /// Create a Organization.
        /// </summary>
        /// <param name="person">The <see cref="Organization2">organization</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Organization2">organization</see></returns>
        Task<Organization2> CreateOrganizationAsync(Organization2 organization);

        /// <summary>
        /// Update a Organization.
        /// </summary>
        /// <param name="person">The <see cref="Organization2">organization</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Organization2">organization</see></returns>
        Task<Organization2> UpdateOrganizationAsync(Organization2 organization);

        /// <summary>
        /// Get an organization from its GUID
        /// </summary>
        /// <returns>Organization2 DTO object</returns>
        Task<Organization2> GetOrganization2Async(string guid);

        /// <summary>
        /// Get HEDM V6 Organization data associated to the specified filters
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="role">Specific role of a organization</param>
        /// <param name="credentialType">Credential type of colleagueId</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <returns>List of Organization2 objects for filtered organizations only.</returns>
        Task<Tuple<IEnumerable<Dtos.Organization2>, int>> GetOrganizations2Async(int Offset, int Limit, string role, string credentialType, string credentialValue);

        /// <summary>
        /// Gets all persons credentials
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.PersonCredential>, int>> GetAllPersonCredentialsAsync(int offset, int limit, bool bypassCache);

        //V8 Changes
        /// <summary>
        /// Gets all persons credentials
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.PersonCredential2>, int>> GetAllPersonCredentials2Async(int offset, int limit, bool bypassCache);


        /// <summary>
        /// Gets all persons credentials
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="credentials"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.PersonCredential2>, int>> GetAllPersonCredentials3Async(int offset, int limit,
            PersonCredential2 credentials, bool bypassCache);

        /// <summary>
        /// Gets all persons credentials
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="credentials"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.PersonCredential3>, int>> GetAllPersonCredentials4Async(int offset, int limit,
            PersonCredential3 credentials, bool bypassCache);

        /// <summary>
        /// Get a person credentials by guid.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Dtos.PersonCredential">person credential</see></returns>
        Task<Dtos.PersonCredential2> GetPersonCredential2ByGuidAsync(string guid);

        /// <summary>
        /// Get a person credentials by guid.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Dtos.PersonCredential">person credential</see></returns>
        Task<Dtos.PersonCredential2> GetPersonCredential3ByGuidAsync(string guid);

        /// <summary>
        /// Get a person credentials by guid.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Dtos.PersonCredential">person credential</see></returns>
        Task<Dtos.PersonCredential3> GetPersonCredential4ByGuidAsync(string guid);

        /// <summary>
        /// Get a HEDM V8 person by guid.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="bypassCache"></param>
        /// <returns>The <see cref="Person3">person</see></returns>
        Task<Person3> GetPerson3ByGuidAsync(string guid, bool bypassCache);

        /// <summary>
        /// Gets person by id V12.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Person4> GetPerson4ByGuidAsync(string guid, bool bypassCache);

        /// <summary>
        /// Gets person by id V12.1.0.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Person5> GetPerson5ByGuidAsync(string guid, bool bypassCache);

        /// <summary>
        /// Get HEDM V8 person data associated to the specified filters
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass Cache</param>
        /// <param name="personFilter">person filters</param>
        /// <param name="personFilterFilter">person Filter Filter</param>
        /// <param name="preferredNameFilter">preferred Name filter</param>
        Task<Tuple<IEnumerable<Dtos.Person3>, int>> GetPerson3NonCachedAsync(int offset, int limit, bool bypassCache, Dtos.Filters.PersonFilter personFilter, string personFilterFilter, string preferredNameFilter);
        
        /// <summary>
        /// Get HEDM V12 person data associated to the specified filters
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass Cache</param>
        /// <param name="personFilter">person filters</param>
        /// <param name="personFilterFilter">person Filter Filter</param>
        /// <param name="preferredNameFilter">preferred Name filter</param>
        /// 
        Task<Tuple<IEnumerable<Dtos.Person4>, int>> GetPerson4NonCachedAsync(int offset, int limit, bool bypassCache, Person4 person, string personFilter);

        /// <summary>
        /// Get HEDM V12.1.0 person data associated to the specified filters
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass Cache</param>
        /// <param name="person">person DTO filters</param>
        /// <param name="personFilter">person Filter Filter</param>
        /// 
        Task<Tuple<IEnumerable<Dtos.Person5>, int>> GetPerson5NonCachedAsync(int offset, int limit, bool bypassCache, Person5 person, string personFilter);

        /// <summary>
        /// Retrieves the matching Persons for the ids provided or searches keyword
        /// for the matching Persons if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following keyword input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">Keyword can either be a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="Dtos.Base.Person">Person</see> with populated ID and first, middle and last names</returns>
        Task<IEnumerable<Dtos.Base.Person>> QueryPersonNamesByPostAsync(Dtos.Base.PersonNameQueryCriteria criteria);

        /// <summary>
        /// A method to check the citizenship fields in an incoming DTO
        /// </summary>
        /// <param name="status"></param>
        /// <param name="country"></param>
        Task<string> CheckCitizenshipfields(Dtos.DtoProperties.PersonCitizenshipDtoProperty newStatus, string newCountry, Dtos.DtoProperties.PersonCitizenshipDtoProperty oldStatus, string oldCountry);

        /// <summary>
        /// A method to check the citizenship fields in an incoming DTO
        /// </summary>
        /// <param name="status"></param>
        /// <param name="country"></param>
        Task CheckCitizenshipfields2(Dtos.DtoProperties.PersonCitizenshipDtoProperty newStatus, string newCountry, Dtos.DtoProperties.PersonCitizenshipDtoProperty oldStatus, string oldCountry, string personGuid = "");

        /// <summary>
        /// A method that returns first name, last name and an email address based on the current hierarchy
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        Task<Dtos.Base.PersonProxyDetails> GetPersonProxyDetailsAsync(string personId);


    }
}