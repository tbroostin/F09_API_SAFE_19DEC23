/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LoadPeriodRepository : BaseColleagueRepository, ILoadPeriodRepository
    {
        private readonly int bulkReadSize;
        public LoadPeriodRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            //24 hours
            //Determines how long until cache expiration
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<IEnumerable<LoadPeriod>> GetLoadPeriodsByIdsAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids", "ids cannot be null or empty");
            }

            var allLoadPeriodEntities = await GetLoadPeriodsAsync();
            return allLoadPeriodEntities.Where(lp => ids.Contains(lp.Id));
        }

        public async Task<IEnumerable<LoadPeriod>> GetLoadPeriodsAsync()
        {
            //Anonymous function gets called only if not in the cache
            return await GetOrAddToCacheAsync("LoadPeriods", async () => {
                var loadPeriodRecords = new List<LoadPeriods>();
                
                var records = await DataReader.BulkReadRecordAsync<LoadPeriods>("LOAD.PERIODS", "");
                if (records != null)
                {
                    loadPeriodRecords.AddRange(records);
                }

                var loadPeriodEntities = new List<LoadPeriod>();

                foreach (var loadPeriod in loadPeriodRecords)
                {
                    loadPeriodEntities.Add(new LoadPeriod(loadPeriod.Recordkey, loadPeriod.LdpdDesc, loadPeriod.LdpdStartDate, loadPeriod.LdpdEndDate));
                }

                return loadPeriodEntities;
            }, CacheTimeout);
            
        }

    }
}
