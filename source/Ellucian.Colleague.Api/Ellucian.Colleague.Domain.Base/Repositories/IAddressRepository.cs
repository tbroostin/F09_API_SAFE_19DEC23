// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for Address Repository
    /// </summary>
    public interface IAddressRepository : IEthosExtended
    {
        /// <summary>
        /// Get All Current Addresses for a person
        /// </summary>
        /// <param name="personId">Person Id</param>
        /// <returns>List of Address Objects</returns>
        IEnumerable<Address> GetPersonAddresses(string personId);

        /// <summary>
        /// Get all current addresses for a list of people.
        /// </summary>
        /// <param name="personIds">List of Person Ids</param>
        /// <returns>List of Address Objects</returns>
        IEnumerable<Address> GetPersonAddressesByIds(List<string> personIds);

        /// <summary>
        /// Get Addresses by guid
        /// </summary>
        /// <param name="guid">Address id</param>
        /// <returns>Address Entity</returns>
        Task<Address> GetAddressAsync(string guid);

        /// <summary>
        /// Get Addresses by guid with enhanced error collections
        /// </summary>
        /// <param name="guid">Address id</param>
        /// <returns>Address Entity</returns>
        Task<Address> GetAddress2Async(string guid);

        /// <summary>
        /// Get All Places
        /// </summary>
        /// <returns>Collection of Place Entities</returns>
        Task<IEnumerable<Place>> GetPlacesAsync();

        /// <summary>
        /// Get an Address Id from a GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Address Key in Colleague</returns>
        Task<string> GetAddressFromGuidAsync(string guid);

        /// <summary>
        /// Get an Address Id from a GUID
        /// </summary>
        /// <param name="addressKey">Key to Addres Record</param>
        /// <param name="addressEntity">Address Domain object for update into Colleague</param>
        /// <returns>Updated Address Entity</returns>
        Task<Address> UpdateAsync(string addressKey, Address addressEntity);


        /// <summary>
        /// Get an Address Id from a GUID
        /// </summary>
        /// <param name="addressKey">Key to Addres Record</param>
        /// <param name="addressEntity">Address Domain object for update into Colleague</param>
        /// <returns>Updated Address Entity</returns>
        Task<Address> Update2Async(string addressKey, Address addressEntity);

        /// <summary>
        /// Delete an Address Id from a GUID
        /// </summary>
        /// <param name="id">GUID pointing to address to delete</param>
        Task DeleteAsync(string id);

        /// <summary>
        /// Get the Host Country from INTL.PARAMS INTERNATIONAL record in Colleague
        /// </summary>
        /// <returns>String representing either USA or CANADA</returns>
        Task<string> GetHostCountryAsync();

        /// <summary>
        /// Get all addresses with paging
        /// </summary>
        /// <param name="offset">Starting position for paging</param>
        /// <param name="limit">amount of records to retrieve for paging</param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Address>, int>> GetAddressesAsync(int offset, int limit);

        /// <summary>
        /// Get all addresses with paging with enhanced error collections
        /// </summary>
        /// <param name="offset">Starting position for paging</param>
        /// <param name="limit">amount of records to retrieve for paging</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Address>, int>> GetAddresses2Async(int offset, int limit, string[] filterPersonIds = null);

        /// <summary>
        ///  Get an Address Guid from an ID
        /// </summary>
        /// <param name="id">Address id</param>
        /// <returns>Address Guid</returns>
        Task<string> GetAddressGuidFromIdAsync(string id);

        /// <summary>
        /// Using a collection of zipcode ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="zipCodeIds">collection of zipCode ids</param>
        /// <returns>Dictionary consisting of a zipCodeIds (key) and guids (value)</returns>
        Task<Dictionary<string, string>> GetZipCodeGuidsCollectionAsync(IEnumerable<string> zipCodeIds);
 
    }
}