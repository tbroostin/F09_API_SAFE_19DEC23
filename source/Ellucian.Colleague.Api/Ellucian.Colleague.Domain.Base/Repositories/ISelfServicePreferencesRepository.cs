// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Interface for Self Service Preferences Repositories
    /// </summary>
    public interface ISelfservicePreferencesRepository
    {
        /// <summary>
        /// Retrieves the Self Service preference for the given user
        /// </summary>
        /// <param name="personId">The person</param>
        /// <param name="preferenceType">The preference type</param>
        /// <returns>The user's preference of the given type if found, otherwise null</returns>
        Task<SelfservicePreference> GetPreferenceAsync(string personId, string preferenceType);

        /// <summary>
        /// Updates self service user preferences with the given data. New preferences may pass an empty, non-null id.
        /// </summary>
        /// <param name="id">Id of the preference to update. Empty string if new.</param>
        /// <param name="personId">Person Id for the preference.</param>
        /// <param name="preferenceType">Type of the preference</param>
        /// <param name="preferences">Preferences to store</param>
        /// <returns>The updated user preference</returns>
        Task<SelfservicePreference> UpdatePreferenceAsync(string id, string personId, string preferenceType, IDictionary<string, dynamic> preferences);

        /// <summary>
        /// Deletes a specified preference from Colleague
        /// </summary>
        /// <param name="personId">Person ID for the preference</param>
        /// <param name="preferenceType">Type of the preference</param>
        /// <returns>nothing</returns>
        Task DeletePreferenceAsync(string personId, string preferenceType);
    }
}
