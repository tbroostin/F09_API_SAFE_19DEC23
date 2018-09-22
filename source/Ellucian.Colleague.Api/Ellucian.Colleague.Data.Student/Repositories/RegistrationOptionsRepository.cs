// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// This repository returns data associated with the registration options (allowed grading types, etc) assigned to particular users
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RegistrationOptionsRepository : BaseColleagueRepository, IRegistrationOptionsRepository
    {
        /// <summary>
        /// Constructor for the RegistrationOptionsRepository
        /// </summary>
        /// <param name="cacheProvider">An instance of a cache provider</param>
        /// <param name="transactionFactory">An instance of a transaction factory</param>
        /// <param name="logger">An instance of a logger</param>
        public RegistrationOptionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache timout value for data that changes rarely
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Name of the cache used to store users' registration options
        /// </summary>
        const string RegControlsIdCacheKey = "RegControlsIdForUser_";

        /// <summary>
        /// Returns registration options for the given users
        /// </summary>
        /// <param name="ids">List of user ids</param>
        /// <returns>List of RegistrationOptions objects</returns>
        public async Task<IEnumerable<RegistrationOptions>> GetAsync(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("Must provide one more more user ids.");
            }

            // Get all reg controls from Colleague (or from cache)
            ICollection<RegControls> allRegControls = await GetAllRegControlsAsync();

            // Get the Reg Controls Id for each user.
            var regControlsIdsDict = await GetRegControlsIdsAsync(ids);

            // Build RegistrationOptions objects
            return BuildRegistrationOptions(ids, allRegControls, regControlsIdsDict);
        }

        // All data collected, build RegistrationOptions records for the given users.
        private List<RegistrationOptions> BuildRegistrationOptions(IEnumerable<string> ids, ICollection<RegControls> regControlsData, Dictionary<string, string> regControlsIdsDict)
        {
            List<RegistrationOptions> registrationOptions = new List<RegistrationOptions>();

            // Loop through each user and build a RegistrationOptions object
            foreach (var id in ids)
            {
                // Get the reg controls record based on the reg controls id determined for this user
                RegControls regControls = null;
                if (regControlsIdsDict.ContainsKey(id))
                {
                    string regControlsId = regControlsIdsDict[id];
                    if (!string.IsNullOrEmpty(regControlsId))
                    {
                        regControls = regControlsData.Where(r => r.Recordkey == regControlsId).FirstOrDefault();
                    }
                }

                // The user will always have the ability to add a "graded" course
                var allowedGradingTypes = new List<GradingType>() { GradingType.Graded };

                // If a regControls was found for the user, use the information to set the user's other options
                if (regControls != null)
                {
                    // If allow audit flag, add audit grading type
                    if (regControls.RgcAllowAuditFlag != null && regControls.RgcAllowAuditFlag.ToUpper() == "Y")
                    {
                        allowedGradingTypes.Add(GradingType.Audit);
                    }

                    // if allow pass fail flag, add pass fail grading type
                    if (regControls.RgcAllowPassFailFlag != null && regControls.RgcAllowPassFailFlag.ToUpper() == "Y")
                    {
                        allowedGradingTypes.Add(GradingType.PassFail);
                    }
                }

                try
                {
                    registrationOptions.Add(new RegistrationOptions(id, allowedGradingTypes));
                }
                catch (Exception ex)
                {
                    logger.Error("Could not build RegistrationOptions object for user " + id + ". Exception: " + ex.Message);
                }
            }

            // Return the list of registration options objects
            return registrationOptions;
        }


        // Given a list of user IDs, calls a transaction that determines the reg.controls for each user.
        private async Task<Dictionary<string, string>> GetRegControlsIdsAsync(IEnumerable<string> ids)
        {
            var regControlsIdsDict = new Dictionary<string, string>();
            // Determine the reg.controls for each user and build registration options object
            foreach (var id in ids)
            {
                // Get cached reg control ID for the given user. If not found, call transaction to get it and store it.
                var regControlsId = await GetOrAddToCacheAsync<string>("RegControlsIdForUser_" + id,
                    async () =>
                    {
                        try
                        {
                            GetRegControlsIdForUserRequest request = new GetRegControlsIdForUserRequest() { InPersonIds = new List<string>() { id } };
                            GetRegControlsIdForUserResponse response = await transactionInvoker.ExecuteAsync<GetRegControlsIdForUserRequest, GetRegControlsIdForUserResponse>(request);
                            return response.PersonRegControls.Where(prc => prc.PersonIds == id).Select(prc => prc.RegControlsIds).First();
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Unable to retrieve reg.controls Id for user " + id + ". Exception: " + ex.Message);
                        }
                        return null;
                    }
                );
                // Add something to the dict for user user, even if null
                regControlsIdsDict[id] = regControlsId;
            }
            return regControlsIdsDict;
        }


        // Gets all RegControls from Colleague (or cache)
        private async Task<ICollection<RegControls>> GetAllRegControlsAsync()
        {
            ICollection<RegControls> allRegControls = null;
            try
            {
                // Get cached regControls data. If not found, use the data reader to get the reg controls record and cache it
                allRegControls = await GetOrAddToCacheAsync<ICollection<RegControls>>("AllRegControls",
                   async () =>
                    {
                        return await DataReader.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true);
                    }
                );
            }
            catch (Exception ex)
            {
                logger.Error("Unable to read REG.CONTROLS. Exception: " + ex.Message);
            }
            return allRegControls;
        }
    }
}
