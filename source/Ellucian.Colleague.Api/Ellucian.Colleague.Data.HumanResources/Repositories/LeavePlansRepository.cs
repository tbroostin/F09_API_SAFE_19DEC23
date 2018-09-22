/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LeavePlansRepository : BaseColleagueRepository, ILeavePlansRepository
    {
        public LeavePlansRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get LeavePlans objects for all LeavePlans bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <returns>Tuple of LeavePlans Entity objects <see cref="LeavePlans"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan>, int>> GetLeavePlansAsync(int offset, int limit, bool bypassCache = false)
        {
            try
            {
                string criteria = string.Empty;
                var leavePlansKeys = await DataReader.SelectAsync("LEAVPLAN", criteria);
                var leavePlansRecords = new List<LeavePlan>();
                var totalCount = 0;
                if (leavePlansKeys == null || !leavePlansKeys.Any())
                {
                    return new Tuple<IEnumerable<LeavePlan>, int>(null, 0);
                }

                totalCount = leavePlansKeys.Count();
                Array.Sort(leavePlansKeys);
                var leavePlansubList = leavePlansKeys.Skip(offset).Take(limit);
                if (leavePlansubList != null && leavePlansubList.Any())
                {
                    try
                    {
                        var leaveplanRecords = await DataReader.BulkReadRecordAsync<DataContracts.Leavplan>(leavePlansubList.ToArray(), bypassCache);
                        foreach (var plan in leaveplanRecords)
                        {

                            leavePlansRecords.Add(BuildLeavePlans(plan));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return new Tuple<IEnumerable<LeavePlan>, int>(leavePlansRecords, totalCount);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Get LeavePlans objects for all LeavePlans 
        /// </summary>
        /// <returns>Tuple of LeavePlans Entity objects <see cref="LeavePlans"/> and a count for paging.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan>> GetLeavePlansAsync(bool bypassCache)
        {
            string cacheKey = "AllLeavePlans_Ethos";

            // modify the cache key when anonymous to prevent an anonymous user from accessing non-anonymous data via the cache
            if (this.DataReader.IsAnonymous)
            {
                cacheKey = string.Join("_", cacheKey, "Anonymous");
            }
            try
            {
                var leaveplans = await GetOrAddToCacheAsync<List<LeavePlan>>(cacheKey,
                async () =>
                {
                    var leaveplanRecords = await DataReader.BulkReadRecordAsync<Leavplan>("LEAVPLAN", "");
                    var leavePlansRecords = new List<LeavePlan>();
                    foreach (var plan in leaveplanRecords)
                    {
                        leavePlansRecords.Add(BuildLeavePlans(plan));
                    }
                    return leavePlansRecords;
                }
             );
                return leaveplans;
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// This method gets the same leave plans as the original version, but this method has a try/catch around each
        /// BuildLeavePlan step, so that we catch individual problems and still return the rest.
        /// 
        /// Also the cache key is different
        /// </summary>
        /// <param name="bypassCache">If you want to bypass the cache</param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan>> GetLeavePlansV2Async(bool bypassCache)
        {
            string cacheKey = "AllLeavePlans2";

            // modify the cache key when anonymous to prevent an anonymous user from accessing non-anonymous data via the cache
            if (this.DataReader.IsAnonymous)
            {
                cacheKey = string.Join("_", cacheKey, "Anonymous");
            }


            var leaveplans = await GetOrAddToCacheAsync(cacheKey,
                async () =>
                {
                    var leaveplanRecords = await DataReader.BulkReadRecordAsync<Leavplan>("LEAVPLAN", "");
                    var leavePlansRecords = new List<LeavePlan>();
                    foreach (var plan in leaveplanRecords)
                    {
                        try
                        {
                            leavePlansRecords.Add(BuildLeavePlans(plan));
                        }
                        catch (Exception e)
                        {
                            LogDataError("LEAVPLAN", plan.Recordkey, plan, e, "Unable to build LEAVPLAN due to unexpected data");
                        }
                    }
                    return leavePlansRecords;
                });

            return leaveplans;
         

        }

        /// <summary>
        /// Get LeavePlans entity for a specific id
        /// </summary>   
        /// <param name="id">id of the LeavePlans record.</param>
        /// <returns>LeavePlans Entity <see cref="LeavePlans"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan> GetLeavePlansByIdAsync(string id)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null || entity.Entity != "LEAVPLAN")
            {
                throw new KeyNotFoundException(string.Format("LeavePlans not found for id {0}", id));
            }
            var leaveplanId = entity.PrimaryKey;
            if (string.IsNullOrWhiteSpace(leaveplanId))
            {
                throw new KeyNotFoundException("LeavePlan id " + id + "does not exist");
            }
            var leavePlanRecord = await DataReader.ReadRecordAsync<DataContracts.Leavplan>("LEAVPLAN", leaveplanId);
            if (leavePlanRecord == null)
            {
                throw new KeyNotFoundException("LeavePlan not found with id " + id);
            }
            return BuildLeavePlans(leavePlanRecord);
        }

        /// <summary>
        /// Helper to build LeavePlan objects
        /// </summary>
        /// <param name="employRecord">the LeavePlan db record</param>
        /// <returns></returns>
        private LeavePlan BuildLeavePlans(Leavplan leavePlanRecord)
        {
            LeavePlan leaveplanEntity = new LeavePlan(leavePlanRecord.RecordGuid, leavePlanRecord.Recordkey, leavePlanRecord.LpnStartDate, leavePlanRecord.LpnDesc, leavePlanRecord.LpnType, leavePlanRecord.LpnAccrualMethod)
            {
                //YearlyStartDate = leavePlanRecord.LpnYrStartDate,
                EndDate = leavePlanRecord.LpnEndDate,
                RollOverLeaveType = leavePlanRecord.LpnRolloverLeaveType,
                AccuralFrequency = leavePlanRecord.LpnAccrualFrequency,
                WaitDays = leavePlanRecord.LpnDaysAllowed,
                AllowNegative = leavePlanRecord.LpnAllowNegative

            };

            if (!string.IsNullOrEmpty(leavePlanRecord.LpnYrStartDate))
            {
                leaveplanEntity.YearlyStartDate = Dmi.Runtime.DmiString.PickDateToDateTime(Convert.ToInt32(leavePlanRecord.LpnYrStartDate));
            }

            if (leavePlanRecord.LpnEarntypes != null && leavePlanRecord.LpnEarntypes.Any())
            {
                leaveplanEntity.EarningsTypes = leavePlanRecord.LpnEarntypes;
            }

            return leaveplanEntity;
        }
    }
}
