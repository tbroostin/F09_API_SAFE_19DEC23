// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class MiscellaneousTextRepository : BaseColleagueRepository, IMiscellaneousTextRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MiscellaneousTextRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public MiscellaneousTextRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get all Miscellaneous Text records
        /// </summary>
        /// <returns>IEnumerable of Miscellaneous Text</returns>
        public async Task<IEnumerable<MiscellaneousText>> GetAllMiscellaneousTextAsync()
        {
            List<MiscellaneousText> mtxtCollection = new List<MiscellaneousText>();
            Collection<MiscText> miscText = await GetAllMiscTextAsync();

            foreach (var textEntry in miscText)
            {
                try
                {
                    mtxtCollection.Add(new MiscellaneousText(textEntry.Recordkey, textEntry.MtxtText));
                }
                catch (Exception e)
                {
                    LogDataError("MISC.TEXT", textEntry.Recordkey, textEntry, e, "Could not retrieve MISC.TEXT content");
                }
            }
            return mtxtCollection;
        }

        /// <summary>
        /// Get all of the MiscText Data Contracts
        /// </summary>
        /// <returns>Collection of MiscText objects</returns>
        private async Task<Collection<MiscText>> GetAllMiscTextAsync()
        {
            return await GetOrAddToCacheAsync<Collection<MiscText>>("MiscText", async () =>
            {
                return await DataReader.BulkReadRecordAsync<MiscText>("MISC.TEXT", "");

            }, CacheTimeout);
        }
    }
}
