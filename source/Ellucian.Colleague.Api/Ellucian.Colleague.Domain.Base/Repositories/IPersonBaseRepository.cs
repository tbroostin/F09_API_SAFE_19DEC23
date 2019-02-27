// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IPersonBaseRepository
    {
        /// <summary>
        /// Retrieves a Base Person object (cached 2 hours)
        /// </summary>
        /// <param name="personId">Id of the person to retrieve</param>
        /// <param name="useCache">Indicate whether to get from cache if available</param>
        /// <returns><see cref="PersonBase">PersonBase</see> object</returns>
        Task<PersonBase> GetPersonBaseAsync(string personId, bool useCache = true);
        
        /// <summary>
        /// Gets PersonBase entities (Same as person but without Preferred Address)
        /// </summary>
        /// <param name="personId">Ids of base persons to retrieve</param>
        /// <param name="useCache">Indicates whether to retrieve from cache if available</param>
        /// <returns></returns>
        Task<IEnumerable<PersonBase>> GetPersonsBaseAsync(IEnumerable<string> personIds, bool hasLastName = false);

        /// <summary>
        /// Gets Person Guid from Opers
        /// </summary>
        /// <param name="opersKey"></param>
        /// <returns></returns>
        Task<string> GetPersonGuidFromOpersAsync(string opersKey);


          /// <summary>
          /// Gets person id from opers
          /// </summary>
          /// <param name="opersKey">opersKey</param>
          /// <returns></returns>
          Task<string> GetPersonIdFromOpersAsync(string opersKey);        

        /// <summary>
        /// Retrieves the information for PersonBase for ids provided,
        /// and the matching PersonBases if a first and last name are provided.  
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
        /// <param name="ids">Enumeration of ids for which to retrieve records</param>
        /// <param name="keyword">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <param name="useCache">True if you want to use cached data</param>
        /// <returns>An enumeration of <see cref="PersonBase">PersonBase</see> information</returns>
        Task<IEnumerable<PersonBase>> SearchByIdsOrNamesAsync(IEnumerable<string> ids, string keyword, bool useCache = true);
    }
}
