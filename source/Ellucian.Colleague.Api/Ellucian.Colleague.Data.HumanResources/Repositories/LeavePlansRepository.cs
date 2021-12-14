/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
        
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LeavePlansRepository : BaseColleagueRepository, ILeavePlansRepository
    {
        const string AllLeavePlanCache = "AllLeavePlan";
        const int AllLeavePlanCacheTimeout = 20; // Clear from cache every 20 minutes
        RepositoryException exception = null;

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
            int totalCount = 0;
            string[] subList = null;
            var leavePlanEntitiesCollection = new List<Domain.HumanResources.Entities.LeavePlan>();
            Collection<DataContracts.Leavplan> leavePlanDataContractCollection = null;

            try
            {
                var leavePlanCacheKey = CacheSupport.BuildCacheKey(AllLeavePlanCache);

                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    leavePlanCacheKey,
                    "LEAVPLAN",
                    offset,
                    limit,
                    AllLeavePlanCacheTimeout,
                    async () =>
                    {
                        return new CacheSupport.KeyCacheRequirements() { };
                    });

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<Domain.HumanResources.Entities.LeavePlan>, int>(null, 0);
                }
                subList = keyCache.Sublist.ToArray();

                totalCount = keyCache.TotalCount.Value;

                leavePlanDataContractCollection = await DataReader.BulkReadRecordAsync<DataContracts.Leavplan>("LEAVPLAN", subList);
                if (leavePlanDataContractCollection == null || leavePlanDataContractCollection.Count == 0)
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "An unexpected error occurred extracting LEAVPLAN data."));

                }
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            
            foreach (var plan in leavePlanDataContractCollection)
            {
                try
                {
                    bool error = false;
                    if (string.IsNullOrWhiteSpace(plan.RecordGuid))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "LeavePlans GUID can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (string.IsNullOrWhiteSpace(plan.Recordkey))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "LeavePlans id can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (string.IsNullOrWhiteSpace(plan.LpnDesc))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "LeavePlans description/title can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (string.IsNullOrWhiteSpace(plan.LpnType))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "LeavePlans type can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (string.IsNullOrWhiteSpace(plan.LpnAccrualMethod))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "LeavePlans accrualMethod can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;

                    }
                    if ((plan.LpnStartDate == null) || (!plan.LpnStartDate.HasValue))
                    {
                         if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "LeavePlans startDate can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;

                    }

                    if (!error)
                        leavePlanEntitiesCollection.Add(BuildLeavePlans(plan));
                }
                catch (Exception ex)
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                    {
                        SourceId = plan.Recordkey,
                        Id = plan.RecordGuid
                    });
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<Domain.HumanResources.Entities.LeavePlan>, int>(leavePlanEntitiesCollection, totalCount);
        }


        /// <summary>
        /// Using a collection of LEAVPLAN ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="leavplanIds">collection of LEAVPLAN ids</param>
        /// <returns>Dictionary consisting of a LEAVPLAN (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetLeavplanGuidsCollectionAsync(IEnumerable<string> leavplanIds)
        {
            if ((leavplanIds == null) || (leavplanIds != null && !leavplanIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var leavplanGuidCollection = new Dictionary<string, string>();

            var leavplanGuidLookup = leavplanIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("LEAVPLAN", p, false)).ToArray();

            var recordKeyLookupResults = await DataReader.SelectAsync(leavplanGuidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!leavplanGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        leavplanGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch (Exception) // Do not throw error.
                {
                }
            }

            return leavplanGuidCollection;
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
            LeavePlan retval = null;
            try
            {
                bool error = false;
                if (string.IsNullOrWhiteSpace(leavePlanRecord.RecordGuid))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "LeavePlans GUID can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = leavePlanRecord.Recordkey,
                        Id = leavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (string.IsNullOrWhiteSpace(leavePlanRecord.Recordkey))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "LeavePlans id can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = leavePlanRecord.Recordkey,
                        Id = leavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (string.IsNullOrWhiteSpace(leavePlanRecord.LpnDesc))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "LeavePlans description/title can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = leavePlanRecord.Recordkey,
                        Id = leavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (string.IsNullOrWhiteSpace(leavePlanRecord.LpnType))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "LeavePlans type can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = leavePlanRecord.Recordkey,
                        Id = leavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (string.IsNullOrWhiteSpace(leavePlanRecord.LpnAccrualMethod))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "LeavePlans accrualMethod can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = leavePlanRecord.Recordkey,
                        Id = leavePlanRecord.RecordGuid
                    });
                    error = true;

                }
                if ((leavePlanRecord.LpnStartDate == null) || (!leavePlanRecord.LpnStartDate.HasValue))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "LeavePlans startDate can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = leavePlanRecord.Recordkey,
                        Id = leavePlanRecord.RecordGuid
                    });
                    error = true;

                }

                if (!error)
                    retval = BuildLeavePlans(leavePlanRecord);
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    SourceId = leavePlanRecord.Recordkey,
                    Id = leavePlanRecord.RecordGuid
                });
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return retval;
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
                EndDate = leavePlanRecord.LpnEndDate,
                RollOverLeaveType = leavePlanRecord.LpnRolloverLeaveType,
                AccuralFrequency = leavePlanRecord.LpnAccrualFrequency,
                WaitDays = leavePlanRecord.LpnDaysAllowed,
                AllowNegative = leavePlanRecord.LpnAllowNegative,
                IsLeaveReportingPlan = !string.IsNullOrWhiteSpace(leavePlanRecord.LpnLeaveReporting) && leavePlanRecord.LpnLeaveReporting.Equals("Y", StringComparison.OrdinalIgnoreCase)
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
