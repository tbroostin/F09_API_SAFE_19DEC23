// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestGlAccountBalancesRepository : IGlAccountBalancesRepository
    {
        private List<FinanceQuery> financeQueryList = null;
        private TestGlAccountRepository testGlAccountRepository = null;
        List<GlAccountBalances> glAccountBalances = null;
        public TestGlAccountBalancesRepository(bool forSubtotal = false)
        {
            financeQueryList = new List<FinanceQuery>();
            FinanceQuery financeQuery = new FinanceQuery();
            glAccountBalances = new List<GlAccountBalances>();
                        
            var glAccount1 = new GlAccountBalances("11_00_01_00_20601_51000", new List<string>()) { ActualAmount = 10.00m, BudgetAmount = 1000.00m, RequisitionAmount = 100.00m, EncumbranceAmount = 123.00m, GlAccountDescription="Test1" };
            glAccountBalances.Add(glAccount1);

            var glAccount2 = new GlAccountBalances("11_00_01_00_20601_52000", new List<string>()) { ActualAmount = 20.00m, BudgetAmount = 2000.00m, RequisitionAmount = 0, EncumbranceAmount = 0, GlAccountDescription = "Test2" };
            glAccount2.IsPooleeAccount = false;            
            glAccountBalances.Add(glAccount2);

            var glAccount3 = new GlAccountBalances("11_00_01_00_20601_53000", new List<string>()) { ActualAmount = 30.00m, BudgetAmount = 3000.00m, RequisitionAmount = 0, EncumbranceAmount = 0, GlAccountDescription = "Test3" };
            glAccount3.IsPooleeAccount = true;
            glAccount3.UmbrellaGlAccount = "11-00-01-00-20601-52000";
            glAccountBalances.Add(glAccount3);
        }
        public async Task<IEnumerable<GlAccountBalances>> QueryGlAccountBalancesAsync(List<string> glAccounts, string fiscalYear, GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            return await Task.Run(() => glAccountBalances);
        }
    }
}