//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    /// <summary>
    /// Provides read-only access to fundamental FinancialAid data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinancialAidReferenceDataRepository : BaseColleagueRepository, IFinancialAidReferenceDataRepository
    {
        /// <summary>
        /// Constructor for the FinancialAidReferenceDataRepository. 
        /// CacheTimeout value is set for Level1
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public FinancialAidReferenceDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Public Accessor for Financial Aid Awards. Retrieves and caches all awards defined
        /// in Colleague. 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FinancialAidAward>> GetFinancialAidAwardsAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<FinancialAidAward>>("AllAwards",
                    async() =>
                    {
                        var awardList = new List<FinancialAidAward>();
                        var awardRecords = DataReader.BulkReadRecord<Awards>("", false);
                        var awardCategories = await GetFinancialAidAwardCategoriesAsync();
                        foreach (var awardRecord in awardRecords)
                        {                            
                            var awardCategory = awardCategories
                                .Where(c => c.Code == awardRecord.AwCategory).FirstOrDefault();
                            try
                            {
                              awardList.Add(new FinancialAidAward(awardRecord.Recordkey, awardRecord.AwDescription, awardCategory));
                            }
                            catch (Exception e)
                            {
                                LogDataError("AWARDS", awardRecord.Recordkey, awardRecord, e, string.Format("Failed to add award {0}", awardRecord.Recordkey));
                            }
                        }
                        return awardList;
                    });
        }

        /// <summary>
        /// Public accessor for financial aid award categories
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FinancialAidAwardCategory>> GetFinancialAidAwardCategoriesAsync()
        {

            return await GetCodeItemAsync<AwardCategories, FinancialAidAwardCategory>("AllAwardCategories", "AWARD.CATEGORIES",
                ac =>
                {
                    return new FinancialAidAwardCategory(ac.Recordkey, ac.AcDescription);
                }
            );

        }
    }
}
