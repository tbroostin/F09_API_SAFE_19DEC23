// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class SelfservicePreferencesRepository : BaseColleagueRepository, ISelfservicePreferencesRepository
    {
        public SelfservicePreferencesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Retrieves the Self Service preference for the given user
        /// </summary>
        /// <param name="personId">The person</param>
        /// <param name="preferenceType">The preference type</param>
        /// <returns>The user's preference of the given type if found, otherwise null</returns>
        public async Task<SelfservicePreference> GetPreferenceAsync(string personId, string preferenceType)
        {
            var criteria = "WITH SELFSRVPREFS.PERSON.ID EQ '" + personId + "' AND SELFSRVPREFS.PREFKEY EQ '" + preferenceType + "'";
            var prefIds = await DataReader.SelectAsync("SELFSERVICE.PREFERENCES", criteria);
            if (prefIds.Any())
            {
                var prefContract = await DataReader.ReadRecordAsync<SelfservicePreferences>(prefIds.First());
                var preferences = Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string,dynamic>>(prefContract.SelfsrvprefsPrefdata);
                var prefEntity = new SelfservicePreference(
                    prefContract.Recordkey,
                    prefContract.SelfsrvprefsPersonId,
                    prefContract.SelfsrvprefsPrefkey,
                    preferences
                    );
                return prefEntity;
            }
            else
            {
                // Did not find a preference matching the type for the person.
                return null;
            }
        }


        /// <summary>
        /// Updates self service user preferences with the given data. New preferences may pass an empty, non-null id.
        /// </summary>
        /// <param name="id">Id of the preference to update. Empty string if new.</param>
        /// <param name="personId">Person Id for the preference.</param>
        /// <param name="preferenceType">Type of the preference</param>
        /// <param name="preferences">Preferences to store</param>
        /// <returns>The updated user preference</returns>
        public async Task<SelfservicePreference> UpdatePreferenceAsync(string id, string personId, string preferenceType, IDictionary<string, dynamic> preferences)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id", "id cannot be null");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "personId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(preferenceType))
            {
                throw new ArgumentNullException("preferenceKey", "preferenceKey cannot be null or empty");
            }
            var updateSelfservicePreferencesRequest = new UpdateSelfservicePreferencesRequest() {
                SelfservicePreferencesId = id,
                PersonId = personId,
                SelfsrvPrefKey = preferenceType,
                SelfsrvPrefData = Newtonsoft.Json.JsonConvert.SerializeObject(preferences)
            };
            var preferencesResponse = await transactionInvoker.ExecuteAsync<UpdateSelfservicePreferencesRequest, UpdateSelfservicePreferencesResponse>(updateSelfservicePreferencesRequest);
            // TODO figure out returning the updated value from the transaction
            if (!string.IsNullOrEmpty(preferencesResponse.ErrorOccurred) && !preferencesResponse.ErrorOccurred.Equals("0"))
            {
                throw new ApplicationException(preferencesResponse.ErrorMessage);
            }
            var updatedPreferenceEntity = await GetPreferenceAsync(personId, preferenceType);
            return updatedPreferenceEntity;
        }

        /// <summary>
        /// Deletes the Self Service preference for the given user
        /// </summary>
        /// <param name="personId">The person</param>
        /// <param name="preferenceType">The preference type</param>
        /// <returns>nothing</returns>
        public async Task DeletePreferenceAsync(string personId, string preferenceType)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "personId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(preferenceType))
            {
                throw new ArgumentNullException("preferenceKey", "preferenceKey cannot be null or empty");
            }
            
            var deleteSelfservicePreferenceRequest = new DeleteSelfservicePreferenceRequest()
            {
                PersonId = personId,
                Prefkey = preferenceType
            };

            DeleteSelfservicePreferenceResponse preferenceResponse = new DeleteSelfservicePreferenceResponse();

            try
            {
                preferenceResponse = await transactionInvoker.ExecuteAsync<DeleteSelfservicePreferenceRequest, DeleteSelfservicePreferenceResponse>(deleteSelfservicePreferenceRequest);
            }
            catch(Exception e)
            {
                logger.Error(e, "Transaction failed");
                throw;
            }
            
            if (!string.IsNullOrEmpty(preferenceResponse.ErrorOccurred) && !preferenceResponse.ErrorOccurred.Equals("0"))
            {
                throw new ApplicationException(preferenceResponse.ErrorMessage);
            }
        }
    }
}
