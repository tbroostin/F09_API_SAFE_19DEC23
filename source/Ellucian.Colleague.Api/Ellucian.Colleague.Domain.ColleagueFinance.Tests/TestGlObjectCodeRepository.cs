// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestGlObjectCodeRepository : IGlObjectCodeRepository
    {
        private List<GlObjectCode> glObjectCodes = null;
        private TestGlAccountRepository testGlAccountRepository = null;

        public List<string> SeedGlNumbers { get { return seedGlNumbers; } }
        private List<string> seedGlNumbers = new List<string>();

        public TestGlObjectCodeRepository()
        {
            glObjectCodes = new List<GlObjectCode>();

            // Create non-pooled accounts, same object.
            var nonPooledAccount1 = new GlObjectCodeGlAccount("11_00_01_00_20601_51000", GlBudgetPoolType.None);
            var nonPooledAccount2 = new GlObjectCodeGlAccount("11_00_01_00_20602_51000", GlBudgetPoolType.None);
            var nonPooledAccount3 = new GlObjectCodeGlAccount("11_00_01_00_20603_51000", GlBudgetPoolType.None);
            var glObjectCode1 = new GlObjectCode("51000", nonPooledAccount1, GlClass.Expense);
            glObjectCode1.Name = "Operating Fund : Main Campus";
            glObjectCode1.AddGlAccount(nonPooledAccount2);
            glObjectCode1.AddGlAccount(nonPooledAccount3);
            glObjectCodes.Add(glObjectCode1);

            // Create pooled accounts
            var umbrellaAccount = new GlObjectCodeGlAccount("11_00_01_00_20601_51001", GlBudgetPoolType.Umbrella);
            var pooleeAccount1 = new GlObjectCodeGlAccount("11_00_01_00_20602_51001", GlBudgetPoolType.Poolee);
            var pooleeAccount2 = new GlObjectCodeGlAccount("11_00_01_00_20603_51001", GlBudgetPoolType.Poolee);
            var pooleeAccount3 = new GlObjectCodeGlAccount("11_00_01_00_20604_51001", GlBudgetPoolType.Poolee);
            var budgetPool = new GlObjectCodeBudgetPool(umbrellaAccount);
            budgetPool.AddPoolee(pooleeAccount1);
            budgetPool.AddPoolee(pooleeAccount2);
            budgetPool.AddPoolee(pooleeAccount3);

            var glObjectCode2 = new GlObjectCode("51001", budgetPool, GlClass.Expense);
            glObjectCode2.Name = "Operating Fund : South Campus";
            glObjectCodes.Add(glObjectCode2);
        }

        public async Task<IEnumerable<GlObjectCode>> GetGlObjectCodesAsync(GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, CostCenterQueryCriteria criteria, string personId)
        {
            return await Task.Run(() => glObjectCodes);
        }
    }
}