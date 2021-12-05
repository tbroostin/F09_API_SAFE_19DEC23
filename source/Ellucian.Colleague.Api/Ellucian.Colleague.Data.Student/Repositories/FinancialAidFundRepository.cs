//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Provides read-only access to fundamental FinancialAid data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinancialAidFundRepository : BaseColleagueRepository, IFinancialAidFundRepository
    {
        private RepositoryException exception = new RepositoryException();
        const string AllSelectedRecordsCache = "AllSelectedRecordKeys";
        const int AllSelectedRecordsCacheTimeout = 20;

        /// <summary>
        /// Constructor for the FinancialAidReferenceDataRepository. 
        /// CacheTimeout value is set for Level1
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public FinancialAidFundRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get Meal Plan Request
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>

        /// <returns>A list of FinancialAidFund domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<FinancialAidFund>, int>> GetFinancialAidFundsAsync(int offset, int limit, string code, string source, string aidType, List<string> classifications, string awardCategory, bool bypassCache)
        {
            var FinancialAidFundsEntities = new List<FinancialAidFund>();            

            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllSelectedRecordsCache, "AWARDS", code, source, aidType, classifications, awardCategory);
            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
               this,
               ContainsKey,
               GetOrAddToCacheAsync,
               AddOrUpdateCacheAsync,
               transactionInvoker,
               selectedRecordCacheKey,
               "AWARDS",
               offset,
               limit,
               AllSelectedRecordsCacheTimeout,
               async () =>
               {
                   // Filters
                   var criteria = new StringBuilder();
                   if (!string.IsNullOrEmpty(code))
                   {
                       criteria.Append(string.Format("WITH AW.ID = '{0}'", code));
                   }
                   if (!string.IsNullOrEmpty(source))
                   {
                       if (criteria.Length > 0) criteria.Append(" AND ");
                       criteria.Append(string.Format("WITH AW.TYPE = '{0}'", source.Substring(0, 1).ToUpper()));
                   }
                   if (!string.IsNullOrEmpty(aidType))
                   {
                       var categoryCriteria = new StringBuilder();
                       switch (aidType.ToLower())
                       {
                           case "loan":
                               categoryCriteria.Append("WITH AC.LOAN.FLAG = 'Y'");
                               break;
                           case "grant":
                               categoryCriteria.Append("WITH AC.GRANT.FLAG = 'Y'");
                               break;
                           case "scholarship":
                               categoryCriteria.Append("WITH AC.SCHOLARSHIP.FLAG = 'Y'");
                               break;
                           case "work":
                               categoryCriteria.Append("WITH AC.WORK.FLAG = 'Y'");
                               break;
                       }
                       if (categoryCriteria.Length > 0)
                       {
                           string[] financialAidCategoryIds = await DataReader.SelectAsync("AWARD.CATEGORIES", categoryCriteria.ToString());
                           if (financialAidCategoryIds == null || !financialAidCategoryIds.Any())
                           {
                               return new CacheSupport.KeyCacheRequirements()
                               {
                                   NoQualifyingRecords = true
                               };
                           }
                           string codeList = string.Empty;
                           foreach (var categoryCode in financialAidCategoryIds)
                           {
                               codeList = string.Concat(codeList, "'", categoryCode, "'");
                           }
                           if (!string.IsNullOrEmpty(codeList))
                           {
                               if (criteria.Length > 0) criteria.Append(" AND ");
                               criteria.Append(string.Format("WITH AW.CATEGORY = {0}", codeList));
                           }
                           else
                           {
                               return new CacheSupport.KeyCacheRequirements()
                               {
                                   NoQualifyingRecords = true
                               };
                           }
                       }
                       else
                       {
                           return new CacheSupport.KeyCacheRequirements()
                           {
                               NoQualifyingRecords = true
                           };
                       }
                   }
                   if (classifications != null && classifications.Any())
                   {
                       foreach (var classification in classifications)
                       {
                           if (criteria.Length > 0) criteria.Append(" AND ");
                           criteria.Append(string.Format("WITH AW.REPORTING.FUNDING.TYPE = '{0}'", classification));
                       }
                   }
                   if (!string.IsNullOrEmpty(awardCategory))
                   {
                       if (criteria.Length > 0) criteria.Append(" AND ");
                       criteria.Append(string.Format("WITH AW.CATEGORY = {0}", awardCategory));
                   }
                   return new CacheSupport.KeyCacheRequirements()
                   {
                       criteria = criteria.ToString()
                   };
               });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<FinancialAidFund>, int>(FinancialAidFundsEntities, 0);
            }

            var totalCount = keyCacheObject.TotalCount.Value;

            var subList = keyCacheObject.Sublist.ToArray();
            var FinancialAidFunds = await DataReader.BulkReadRecordAsync<Awards>("AWARDS", subList);
            {
                if (FinancialAidFunds == null)
                {
                    throw new KeyNotFoundException("No records selected from AWARDS in Colleague.");
                }
            }

            foreach (var financialAidFundsEntity in FinancialAidFunds)
            {
                if (financialAidFundsEntity != null && !string.IsNullOrEmpty(financialAidFundsEntity.Recordkey))
                {
                    var financialAidFund = BuildFinancialAidFund(financialAidFundsEntity);
                    if (financialAidFund != null)
                    {
                        FinancialAidFundsEntities.Add(BuildFinancialAidFund(financialAidFundsEntity));
                    }
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<FinancialAidFund>, int>(FinancialAidFundsEntities, totalCount);
        }

        /// <summary>
        /// Get Financial Aid Fund data contract to entity
        /// </summary>
        /// <param name="source">Awards data contract</param>
        /// <returns>FinancialAidFund domain entitiy</returns>
        private FinancialAidFund BuildFinancialAidFund(Awards source)
        {
            if (source != null && !string.IsNullOrEmpty(source.RecordGuid))
            {
                var financialAidFund = new FinancialAidFund(source.RecordGuid, source.Recordkey, !string.IsNullOrEmpty(source.AwDescription) ? source.AwDescription : source.Recordkey);
                financialAidFund.Description2 = source.AwExplanationText;
                financialAidFund.Source = source.AwType;
                financialAidFund.CategoryCode = source.AwCategory;
                financialAidFund.FundingType = source.AwReportingFundingType;
                return financialAidFund;
            }
            else
            {
                exception.AddError(new RepositoryError("Bad.Data", string.Concat("No Guid found, Entity:'AWARDS', Record ID:'", source.Recordkey, "'")));
                return null;
            }
        }

        /// <summary>
        /// Returns a award for a specified Financial Aid Funds key.
        /// </summary>
        /// <param name="ids">Key to Financial Aid Funds to be returned</param>
        /// <returns>FinancialAidFund Objects</returns>
        public async Task<FinancialAidFund> GetFinancialAidFundByIdAsync(string id)
        {
            var financialAidFundId = await GetRecordInfoFromGuidAsync(id);

            if (financialAidFundId == null)
                throw new KeyNotFoundException();

            var award = await DataReader.ReadRecordAsync<Awards>("AWARDS", financialAidFundId.PrimaryKey);

            return BuildFinancialAidFund(award);
        }

        public async Task<IEnumerable<FinancialAidFund>> GetFinancialAidFundsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<Awards, FinancialAidFund>("AllFinancialAidFunds", "AWARDS",
                (fa, g) => new FinancialAidFund(g, fa.Recordkey, !string.IsNullOrEmpty(fa.AwDescription) ? fa.AwDescription : fa.Recordkey) { Description2 = fa.AwExplanationText, Source = fa.AwType, CategoryCode = fa.AwCategory, FundingType = fa.AwReportingFundingType }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get FinancialAidFundsFinancialProperty for the given award years
        /// </summary>
        /// <param name="fundYears">The funding years for which to get funds</param>
        /// <returns>A list of FinancialAidFundsFinancialProperty objects for the given award id and fund years</returns>
        public async Task<IEnumerable<FinancialAidFundsFinancialProperty>> GetFinancialAidFundFinancialsAsync(string awardId, IEnumerable<string> fundYears, string hostCountry)
        {
            var criteria = new StringBuilder();

            if (!string.IsNullOrEmpty(awardId))
            {
            //    throw new ArgumentNullException("awardId");
                criteria.AppendFormat("WITH FUNDOFC.FUND.ID EQ '{0}'", awardId);
            }

            //var criteria = string.Format("WITH FUNDOFC.FUND.ID EQ '{0}'", awardId);

            if (fundYears == null || !fundYears.Any())
            {
                logger.Info(string.Format("Cannot get budget components for student {0} with no studentAwardYears", awardId));
                return new List<FinancialAidFundsFinancialProperty>();
            }

            var faFinancials = new List<FinancialAidFundsFinancialProperty>();
            foreach (var faFundYear in fundYears)
            {
                var fundOfficeAcyrFile = "FUND.OFFICE." + faFundYear;
                var fofcRecord = await DataReader.BulkReadRecordAsync<FundOfficeAcyr>(fundOfficeAcyrFile, criteria.ToString());
                if (fofcRecord != null)
                {
                    foreach (var fofcEntity in fofcRecord)
                    {
                        try
                        {
                            if (fofcEntity.FundofcBudgetAmt != null) {
                                faFinancials.Add(
                                    new FinancialAidFundsFinancialProperty(
                                        faFundYear,
                                        fofcEntity.Recordkey.Split('*')[1],
                                        (decimal) fofcEntity.FundofcBudgetAmt,
                                        fofcEntity.Recordkey.Split('*')[0],
                                        fofcEntity.FundofcBudgetAmt + (fofcEntity.FundofcOverAmt != null ? fofcEntity.FundofcOverAmt : 0) + (fofcEntity.FundofcOverPct != null ? (((decimal) fofcEntity.FundofcOverPct/100)*fofcEntity.FundofcBudgetAmt) : 0))
                                    );
                            }
                        }
                        catch (Exception e)
                        {
                            var message =
                                string.Format("Unable to create financial {0} for award {1}, fund year {2}", fofcEntity.Recordkey, awardId, faFundYear);
                            logger.Error(e, message);
                        }
                    }
                }
            }

            return faFinancials;
        }
    }
}
