// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Phone Number services
    /// </summary>
    public interface IPhoneNumberService
    {
        /// <summary>
        /// Get a list of phone numbers from a list of Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneNumber>> QueryPhoneNumbersAsync(PhoneNumberQueryCriteria criteria);

        /// <summary>
        /// Get a list of phone numbers from a list of Pilot Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PilotPhoneNumber>> QueryPilotPhoneNumbersAsync(PhoneNumberQueryCriteria criteria);
        /// <summary>
        /// Get all current phone numbers for a person
        /// </summary>
        /// <param name="personId">Person to get phone numbers for</param>
        /// <returns>PhoneNumber Object<see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        Task<Dtos.Base.PhoneNumber> GetPersonPhones2Async(string personId);
    }
}
