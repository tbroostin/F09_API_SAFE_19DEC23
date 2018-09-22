/* Copyright 2014 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for emergency information repositories.
    /// </summary>
    public interface IEmergencyInformationRepository
    {
        /// <summary>
        /// Gets the emergency information for the specified person.
        /// </summary>
        /// <param name="personId">The person's ID.</param>
        /// <returns>An EmergencyInformation object.</returns>
        EmergencyInformation Get(string personId);

        /// <summary>
        /// Updates emergency information for a person.
        /// </summary>
        /// <param name="emergencyInformation">An EmergencyInformation object.</param>
        /// <returns>An updated EmergencyInformation object.</returns>
        EmergencyInformation UpdateEmergencyInformation(EmergencyInformation emergencyInformation);

        /// <summary>
        /// Gets person contacts
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person"></param>
        /// <returns>Tuple<IEnumerable<PersonContact>, int></returns>
        Task<Tuple<IEnumerable<PersonContact>, int>> GetPersonContactsAsync(int offset, int limit, bool bypassCache, string person = "");

        /// <summary>
        /// Gets a person contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns>PersonContact</returns>
        Task<PersonContact> GetPersonContactByIdAsync(string id);
    }
}
