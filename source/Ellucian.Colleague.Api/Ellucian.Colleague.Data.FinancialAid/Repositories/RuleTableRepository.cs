/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository class for build rule table objects
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RuleTableRepository : BaseColleagueRepository, IRuleTableRepository
    {
        public RuleTableRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Get all the rule tables for the shopping sheet for the given award years
        /// </summary>
        /// <param name="awardYears">List of award years for which to get rule tables</param>
        /// <returns>A list of rule tables that can be used to generate student-specific messages on the shopping sheet.</returns>
        public async Task<IEnumerable<ShoppingSheetRuleTable>> GetShoppingSheetRuleTablesAsync(IEnumerable<string> awardYears)
        {
            if (awardYears == null || awardYears.Count() == 0)
            {
                return new List<ShoppingSheetRuleTable>();
            }

            var shoppingSheetRuleTables = new List<ShoppingSheetRuleTable>();

            foreach (var year in awardYears)
            {
                var acyrfile = "RULE.TABLE." + year;
                var ruleTables = await DataReader.BulkReadRecordAsync<RuleTableAcyr>(acyrfile, "");
                if (ruleTables != null && ruleTables.Count() > 0)
                {
                    var shoppingSheetRuleTableRecords = ruleTables.Where(rt => !string.IsNullOrEmpty(rt.RtType) && rt.RtType.ToUpper() == "TEXT");
                    foreach (var ruleTableRecord in shoppingSheetRuleTableRecords)
                    {
                        try
                        {
                            var shoppingSheetRuleTable = new ShoppingSheetRuleTable(ruleTableRecord.Recordkey, year, ruleTableRecord.RtFixLiteral)
                            {
                                Description = ruleTableRecord.RtDescription,
                                AlwaysUseDefault = (!string.IsNullOrEmpty(ruleTableRecord.RtDefaultValueCondition) && ruleTableRecord.RtDefaultValueCondition.ToUpper() == "Y"),
                            };

                            if (ruleTableRecord.RtRuleTableEntityAssociation != null && ruleTableRecord.RtRuleTableEntityAssociation.Count() > 0)
                            {
                                ruleTableRecord.RtRuleTableEntityAssociation.ForEach(ruleResult =>
                                    shoppingSheetRuleTable.AddRuleResultPair(ruleResult.RtRuleIdsAssocMember, ruleResult.RtResultsAssocMember));
                            }

                            shoppingSheetRuleTables.Add(shoppingSheetRuleTable);
                        }
                        catch (Exception e)
                        {
                            LogDataError(acyrfile, ruleTableRecord.Recordkey, ruleTableRecord, e, string.Format("Unable to create shopping sheet rule table for year {0}", year));
                        }

                    }
                }
            }
            return shoppingSheetRuleTables;
        }
    }
}
