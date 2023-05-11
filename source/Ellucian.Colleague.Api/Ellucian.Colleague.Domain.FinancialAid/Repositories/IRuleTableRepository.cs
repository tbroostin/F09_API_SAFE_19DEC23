/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IRuleTableRepository
    {
        Task<IEnumerable<ShoppingSheetRuleTable>> GetShoppingSheetRuleTablesAsync(IEnumerable<string> awardYears);


        Task GetCustomVerbiageAsync(ShoppingSheetRuleTable ruleTable, string studentId);
    }
}
