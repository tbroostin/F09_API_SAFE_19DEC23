// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestFinanceQueryRepository : IFinanceQueryRepository
    {
        private List<FinanceQuery> financeQueryList = null;
        private TestGlAccountRepository testGlAccountRepository = null;
        List<FinanceQueryGlAccountLineItem> glAccountLineItems = null;
        public List<string> SeedGlNumbers { get { return seedGlNumbers; } }
        private List<string> seedGlNumbers = new List<string>();

        public TestFinanceQueryRepository(bool forSubtotal = false)
        {
            financeQueryList = new List<FinanceQuery>();
            FinanceQuery financeQuery = new FinanceQuery();
            glAccountLineItems = new List<FinanceQueryGlAccountLineItem>();
            List<FinanceQuerySubtotal> financeQuerySubtotals = new List<FinanceQuerySubtotal>();
            List<FinanceQuerySubtotalComponent> financeQuerySubtotalComponents = new List<FinanceQuerySubtotalComponent>();

            // Create gl accounts and add to same object.
            var glAccount1 = new FinanceQueryGlAccount("11_00_01_00_20601_51000") { ActualAmount = 10.00m, BudgetAmount = 1000.00m, RequisitionAmount = 100.00m, EncumbranceAmount = 123.00m };
            glAccountLineItems.Add(PopulateFinanceQueryGlAccountLineItem(glAccount1, GlBudgetPoolType.None));

            var glAccount2 = new FinanceQueryGlAccount("11_00_01_00_20601_52000") { ActualAmount = 20.00m, BudgetAmount = 2000.00m, RequisitionAmount = 0, EncumbranceAmount = 0 };
            glAccountLineItems.Add(PopulateFinanceQueryGlAccountLineItem(glAccount2, GlBudgetPoolType.Umbrella, true));

            var glAccount3 = new FinanceQueryGlAccount("11_00_01_00_20601_53000") { ActualAmount = 30.00m, BudgetAmount = 3000.00m, RequisitionAmount = 0, EncumbranceAmount = 0 };
            glAccountLineItems.Add(PopulateFinanceQueryGlAccountLineItem(glAccount3, GlBudgetPoolType.Umbrella, true, false));
                        
            FinanceQuerySubtotal subtotal = new FinanceQuerySubtotal
            {
                FinanceQueryGlAccountLineItems = glAccountLineItems,
                FinanceQuerySubtotalComponents = financeQuerySubtotalComponents
            };
            financeQuerySubtotals.Add(subtotal);
            financeQuery.FinanceQuerySubtotals = financeQuerySubtotals;
            financeQueryList.Add(financeQuery);
        }


        public async Task<IEnumerable<FinanceQueryGlAccountLineItem>> GetGLAccountsListAsync(GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, FinanceQueryCriteria criteria, string personId)
        {
            return await Task.Run(() => glAccountLineItems);
        }

        private FinanceQueryGlAccountLineItem PopulateFinanceQueryGlAccountLineItem(FinanceQueryGlAccount glAccount, GlBudgetPoolType poolType, bool isPooledAccount = false, bool isUmbrellaVisible = true)
        {
            return new FinanceQueryGlAccountLineItem(glAccount, isPooledAccount, isUmbrellaVisible);
        }


    }
}