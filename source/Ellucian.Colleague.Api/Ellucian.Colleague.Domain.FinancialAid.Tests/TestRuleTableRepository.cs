//Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestRuleTableRepository : IRuleTableRepository
    {
        public class RuleTableRecord
        {
            public string code;
            public string awardYear;
            public string description;
            public string type;
            public string defaultResultString;
            public string subroutineName;
            public string defaultValueCondition;
            public List<RuleTable> ruleTableList;

            public class RuleTable
            {
                public string ruleId;
                public string result;
            }
        }

        public List<RuleTableRecord> ruleTableData = new List<RuleTableRecord>()
        {
            new RuleTableRecord()
            {
                code = "SHOP",
                awardYear = "2015",
                description = "ShoppingSheetRuleTable for 2015",
                type = "TEXT",
                defaultResultString = "DefaultResult",
                subroutineName = "",
                defaultValueCondition = "Y",
                ruleTableList = new List<RuleTableRecord.RuleTable>()
                {
                    new RuleTableRecord.RuleTable() {ruleId = "SSISIR", result = "You have an ISIR record"},
                    new RuleTableRecord.RuleTable() {ruleId = "SSNOISIR", result = "Please submit a FAFSA application"}
                }
            },
            new RuleTableRecord()
            {
                code = "SHOP",
                awardYear = "2014",
                description = "ShoppingSheetRuleTable for 2014",
                type = "TEXT",
                defaultResultString = "DefaultResult",
                subroutineName = "",
                defaultValueCondition = "N",
                ruleTableList = new List<RuleTableRecord.RuleTable>()
                {
                    new RuleTableRecord.RuleTable() {ruleId = "SSISIR", result = "You have an ISIR record"},
                    new RuleTableRecord.RuleTable() {ruleId = "SSNOISIR", result = "Please submit a FAFSA application"}
                }
            },
            new RuleTableRecord()
            {
                code = "SHOP2",
                awardYear = "2014",
                description = "ShoppingSheetRuleTable for 2014",
                type = "TEXT",
                defaultResultString = "DefaultResult",
                subroutineName = "s",
                defaultValueCondition = "Y",
                ruleTableList = new List<RuleTableRecord.RuleTable>()
                {
                    new RuleTableRecord.RuleTable() {ruleId = "SSISIR", result = "You have an ISIR record"},
                    new RuleTableRecord.RuleTable() {ruleId = "SSNOISIR", result = "Please submit a FAFSA application"}
                }
            },
            new RuleTableRecord()
            {
                code = "DFLTPKDF",
                awardYear = "2014",
                description = "Default Package Definition rule table for 2014",
                type = "PKDF",
                defaultResultString = "DFLTRECORDID",
                subroutineName = "S.FA.PKDF.RULE.TABLE",
                ruleTableList = new List<RuleTableRecord.RuleTable>()
                {
                }
            }
        };

        public Task<IEnumerable<ShoppingSheetRuleTable>> GetShoppingSheetRuleTablesAsync(IEnumerable<string> awardYears)
        {
            List<ShoppingSheetRuleTable> shoppingSheetRuleTables = new List<ShoppingSheetRuleTable>();
            if (awardYears == null || awardYears.Count() == 0)
            {
                return Task.FromResult(shoppingSheetRuleTables.AsEnumerable());
            }           

            foreach (var year in awardYears)
            {
                var ruleTables = ruleTableData.Where(td => td.awardYear == year);
                if (ruleTables != null)
                {
                    var shoppingSheetRuleTableRecords = ruleTables.Where(rt => !string.IsNullOrEmpty(rt.type) && rt.type.ToUpper() == "TEXT");
                    foreach (var ruleTableRecord in shoppingSheetRuleTableRecords)
                    {
                        try
                        {
                            var shoppingSheetRuleTable = new ShoppingSheetRuleTable(ruleTableRecord.code, year, ruleTableRecord.defaultResultString)
                            {
                                Description = ruleTableRecord.description,
                                AlwaysUseDefault = (!string.IsNullOrEmpty(ruleTableRecord.defaultValueCondition) && ruleTableRecord.defaultValueCondition.ToUpper() == "Y"),
                            };

                            if (ruleTableRecord.ruleTableList != null)
                            {
                                ruleTableRecord.ruleTableList.ForEach(ruleResult =>
                                    shoppingSheetRuleTable.AddRuleResultPair(ruleResult.ruleId, ruleResult.result));
                            }

                            shoppingSheetRuleTables.Add(shoppingSheetRuleTable);
                        }
                        catch (Exception) { }
                    }
                }
            }
            return Task.FromResult(shoppingSheetRuleTables.AsEnumerable());
        }
    }
}
