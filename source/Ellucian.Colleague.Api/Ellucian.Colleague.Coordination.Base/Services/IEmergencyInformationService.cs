﻿/* Copyright 2014-2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for an emergency information service that retrieves a person's emergency information.
    /// </summary>
    public interface IEmergencyInformationService : IBaseService
    {
        /// <summary>
        /// Get all the emergency information for a person. The person whose emergency information is returned
        /// must be same person as the current user.
        /// </summary>
        /// <param name="personId">The person's ID.</param>
        /// <returns>An EmergencyInformation object.</returns>
        Task<EmergencyInformation> GetEmergencyInformationAsync(string personId);

        /// <summary>
        /// Get a person's emergency information.
        /// </summary>
        /// <param name="personId">The person's ID</param>
        /// <returns>An EmergencyInformation object</returns>
        Task<PrivacyWrapper<EmergencyInformation>> GetEmergencyInformation2Async(string personId);

        /// <summary>
        /// Update a person's emergency information.
        /// </summary>
        /// <param name="emergencyInformation">An emergency information object</param>
        /// <returns>The updated emergency information object</returns>
        EmergencyInformation UpdateEmergencyInformation(EmergencyInformation emergencyInformation);

        /// <summary>
        /// Gets persons emergency contacts information
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person"></param>
        /// <returns>Tuple<IEnumerable<Dtos.PersonContactSubject>, int></returns>
        Task<Tuple<IEnumerable<Dtos.PersonContactSubject>, int>> GetPersonEmergencyContactsAsync(int offset, int limit, bool bypassCache, string person = "");

        /// <summary>
        /// Gets persons emergency contact information
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.PersonContactSubject</returns>
        Task<Dtos.PersonContactSubject> GetPersonEmergencyContactByIdAsync(string id);

        #region PersonEmergencyContacts

        /// <summary>
        /// Gets all person-emergency-contacts
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonEmergencyContacts">personEmergencyContacts</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonEmergencyContacts>, int>> GetPersonEmergencyContacts2Async(int offset, int limit, Dtos.PersonEmergencyContacts criteriaObj, string personFilterValue, bool bypassCache = false);

        /// <summary>
        /// Get a personEmergencyContacts by guid.
        /// </summary>
        /// <param name="guid">Guid of the personEmergencyContacts in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonEmergencyContacts">personEmergencyContacts</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonEmergencyContacts> GetPersonEmergencyContactsByGuid2Async(string guid, bool bypassCache = true);

        /// <summary>
        /// Update a personEmergencyContacts.
        /// </summary>
        /// <param name="personEmergencyContacts">The <see cref="PersonEmergencyContacts">personEmergencyContacts</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonEmergencyContacts">personEmergencyContacts</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonEmergencyContacts> UpdatePersonEmergencyContactsAsync(Ellucian.Colleague.Dtos.PersonEmergencyContacts personEmergencyContacts);

        /// <summary>
        /// Create a personEmergencyContacts.
        /// </summary>
        /// <param name="personEmergencyContacts">The <see cref="PersonEmergencyContacts">personEmergencyContacts</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonEmergencyContacts">personEmergencyContacts</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonEmergencyContacts> CreatePersonEmergencyContactsAsync(Ellucian.Colleague.Dtos.PersonEmergencyContacts personEmergencyContacts);

        /// <summary>
        /// Delete a personEmergencyContacts by guid.
        /// </summary>
        /// <param name="guid">Guid of the personEmergencyContacts in Colleague.</param>
        Task DeletePersonEmergencyContactsAsync(string guid);

        #endregion
    }
}
