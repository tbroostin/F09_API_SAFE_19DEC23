// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for PhoneNumber Repository
    /// </summary>
    public interface IPhoneNumberRepository 
    {
        /// <summary>
        /// Get All Current Phone Numbers for a person
        /// </summary>
        /// <param name="personId">Person Id</param>
        /// <returns>PhoneNumber Object</returns>
        PhoneNumber GetPersonPhones(string personId);
        
        /// <summary>
        /// Get all current phone numbers for a list of people.
        /// </summary>
        /// <param name="personIds">List of Person Ids</param>
        /// <returns>List of PhoneNumber Objects</returns>
        Task<IEnumerable<PhoneNumber>> GetPersonPhonesByIdsAsync(List<string> personIds);
        
        /// <summary>
        /// Get all Pilot primary and SMS phone numbers for a list of people.
        /// </summary>
        /// <param name="personIds">List of Person Ids</param>
        /// <returns>List of PhoneNumber Objects</returns>
        Task <IEnumerable<PilotPhoneNumber>> GetPilotPersonPhonesByIdsAsync(List<string> personIds, PilotConfiguration pilotConfiguration);
    }
}
