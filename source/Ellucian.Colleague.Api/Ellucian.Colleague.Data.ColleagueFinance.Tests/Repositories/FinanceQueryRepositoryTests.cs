// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    /// <summary>
    /// Test the methods in the Finance query repository.
    /// </summary>
    [TestClass]
    public class FinanceQueryRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private FinanceQueryRepository actualRepository = null;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private TestGeneralLedgerAccountRepository testGeneralLedgerAccountRepository = new TestGeneralLedgerAccountRepository();
        private TestCostCenterRepository testCostCenterRepository = null;
        private TestGeneralLedgerUserRepository testGlUserRepository = null;
        private TestGlAccountRepository testGlNumberRepository;
        private ApplValcodes GlSourceCodes = null;
        private GetPurchasingDocumentIdsRequest purchasingDocumentIdsRequest;
        private GetPurchasingDocumentIdsResponse purchasingDocumentIdsResponse;

        private GetGlAccountDescriptionResponse glAccountsDescriptionResponse;

        private GeneralLedgerUser param1_GlUser;
        private GeneralLedgerAccountStructure param2_glAccountStructure;
        private GeneralLedgerClassConfiguration param3_glClassConfiguration;
        private int param4_FiscalYear;
        private FinanceQueryCriteria param4_financeQueryCriteria;
        private List<CostCenterComponentQueryCriteria> componentQueryCriteria;
        private List<CostCenterComponentRangeQueryCriteria> componentRangeQueryCriteria;
        private List<FinanceQueryComponentSortCriteria> sortCriteria;


        private Collection<GlAccts> glAcctsDataContracts;
        private Collection<GlAccts> glAcctsNoAccessUmbrellaDataContracts;
        private Collection<GlsFyr> glsFyrDataContracts;
        private Collection<GlsFyr> glsFyrNoAccessUmbrellaDataContracts;
        private Collection<EncFyr> encFyrDataContracts;
        private Collection<GlpFyr> glpFyrDataContracts;
        private GeneralLedgerComponent objectMajorComponent;

        private string[] glaIds = new string[] { };
        private string[] glsIds = new string[] { };
        private string[] encIds = new string[] { };
        private string[] glAcctsIds = new string[] { };

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            objectMajorComponent = null;
            apiSettings = new ApiSettings("TEST");
            actualRepository = new FinanceQueryRepository(cacheProvider, transFactory, logger, apiSettings);

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testCostCenterRepository = new TestCostCenterRepository();
            testGlUserRepository = new TestGeneralLedgerUserRepository();
            testGlNumberRepository = new TestGlAccountRepository();

            glAcctsDataContracts = new Collection<GlAccts>();
            glAcctsNoAccessUmbrellaDataContracts = new Collection<GlAccts>();
            glsFyrDataContracts = new Collection<GlsFyr>();
            glsFyrNoAccessUmbrellaDataContracts = new Collection<GlsFyr>();
            encFyrDataContracts = new Collection<EncFyr>();
            glpFyrDataContracts = new Collection<GlpFyr>();

            glAccountsDescriptionResponse = new GetGlAccountDescriptionResponse();

            BuildGlSourceCodes();

            this.param1_GlUser = new GeneralLedgerUser("1", "Kleehammer");
            this.param2_glAccountStructure = new GeneralLedgerAccountStructure();

            var costCenterCompQueryCriteria = new CostCenterComponentQueryCriteria("FUND");
            var componentValues = new List<string>()
                {
                    "10"
                };
            costCenterCompQueryCriteria.IndividualComponentValues = componentValues;
            var costCenterCompRangeQueryCriteria1 = new CostCenterComponentRangeQueryCriteria("10", "12");
            var costCenterCompRangeQueryCriteria2 = new CostCenterComponentRangeQueryCriteria("20", "22");
            costCenterCompQueryCriteria.RangeComponentValues = new List<CostCenterComponentRangeQueryCriteria>();
            costCenterCompQueryCriteria.RangeComponentValues.Add(costCenterCompRangeQueryCriteria1);
            costCenterCompQueryCriteria.RangeComponentValues.Add(costCenterCompRangeQueryCriteria2);

            componentQueryCriteria = new List<CostCenterComponentQueryCriteria>();
            componentQueryCriteria.Add(costCenterCompQueryCriteria);
            sortCriteria = new List<FinanceQueryComponentSortCriteria>();

            this.param4_financeQueryCriteria = new FinanceQueryCriteria(componentQueryCriteria, sortCriteria);
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.StartYear.ToString();
            this.param4_financeQueryCriteria.ComponentCriteria = new List<CostCenterComponentQueryCriteria>();


            var expenseValues = new List<String>() { "5", "7" };
            var revenueValues = new List<String>() { "4" };
            var assetValues = new List<string>() { "1" };
            var liabilityValues = new List<string>() { "2" };
            var fundBalanceValues = new List<string>() { "3" };
            this.param3_glClassConfiguration = new GeneralLedgerClassConfiguration("GL.Class", expenseValues, revenueValues, assetValues, liabilityValues, fundBalanceValues);
            this.param3_glClassConfiguration.GlClassStartPosition = 18;
            this.param3_glClassConfiguration.GlClassLength = 1;


            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
            dataReaderMock = null;
            glAcctsDataContracts = null;
            glAcctsNoAccessUmbrellaDataContracts = null;
            glsFyrDataContracts = null;
            glsFyrNoAccessUmbrellaDataContracts = null;
            encFyrDataContracts = null;
            glpFyrDataContracts = null;
            testGlConfigurationRepository = null;
            testCostCenterRepository = null;
            testGlUserRepository = null;
            testGlNumberRepository = null;
            GlSourceCodes = null;

            this.param1_GlUser = null;
            this.param2_glAccountStructure = null;
            this.param3_glClassConfiguration = null;
            this.param4_FiscalYear = 0;
            this.param4_financeQueryCriteria = null;
            glaIds = new string[] { };
            glsIds = new string[] { };
            encIds = new string[] { };
            glAcctsIds = new string[] { };
            objectMajorComponent = null;
        }
        #endregion

        #region GetGLAccountsListAsync

        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_NonPooledAccountsOnly()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await RealRepository_GetGLAccountsListAsync();

            Assert.IsNotNull(result);

            var financeQueryGlAccountLineItems = result.ToList();

            Assert.IsTrue(financeQueryGlAccountLineItems.Count == glAccounts.Count);

            // Make sure the amounts of each GL account match the amounts in the data contracts.
            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                Assert.IsNotNull(glAccountLineItem);
                var glAccount = glAccountLineItem.GlAccount;

                Assert.IsNotNull(glAccount);

                var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == glAccount.GlAccountNumber)
                    .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == testGlConfigurationRepository.StartYear.ToString()).ToList();

                Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), glAccount.BudgetAmount, "BudgetAmount not matching");
                Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), glAccount.ActualAmount, "ActualAmount not matching");
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), glAccount.EncumbranceAmount, "EncumbranceAmount not matching");
                Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), glAccount.RequisitionAmount, "RequisitionAmount not matching");
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_UmbrellaHasDirectExpenses()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };

            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var fiscalyear = await testGlConfigurationRepository.GetFiscalYearConfigurationAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // The total number of poolees should match the number of poolees as seed accounts + the umbrella since it has direct expenses.
                Assert.AreEqual(pooleeAccounts.Count + 1, glAccountLineItem.Poolees.Count);

                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.RequisitionAmount);

                Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glAccountLineItem.Poolees)
                {
                    // The umbrella should show up as a poolee...
                    List<GlAcctsMemos> glAcctsMemos = null;
                    if (poolee.GlAccountNumber == glAccountLineItem.GlAccount.GlAccountNumber)
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                            .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear
                                && x.GlPooledTypeAssocMember.ToUpper() == "U").ToList();
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                        Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), poolee.RequisitionAmount);
                    }
                    else
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                        .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear
                            && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                        Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                        Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), poolee.RequisitionAmount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_UmbrellaHasNoDirectExpenses()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);

            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // The total number of poolees should match the number of poolees as seed accounts.
                Assert.AreEqual(pooleeAccounts.Count, glAccountLineItem.Poolees.Count);

                Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.RequisitionAmount);

                // Make sure the umbrella does NOT show up as a poolee since it has no direct expenses.
                Assert.IsFalse(glAccountLineItem.Poolees.Any(x => x.GlAccountNumber == umbrellaAccount));

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glAccountLineItem.Poolees)
                {
                    var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                    .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear
                        && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                    Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                    Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), poolee.RequisitionAmount);

                }
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_UmbrellaHasDirectExpenses_NoPoolees()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.RequisitionAmount);


                // The umbrella account should be the only poolee.
                Assert.AreEqual(1, glAccountLineItem.Poolees.Count);

                // Make sure the umbrella shows up as a poolee since it has direct expenses.
                var umbrellaAsPoolee = glAccountLineItem.Poolees.FirstOrDefault(x => x.GlAccountNumber == umbrellaAccount);
                Assert.IsNotNull(umbrellaAsPoolee);

                var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == umbrellaAsPoolee.GlAccountNumber)
                    .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear
                        && x.GlPooledTypeAssocMember.ToUpper() == "U").ToList();

                Assert.AreEqual(0m, umbrellaAsPoolee.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), umbrellaAsPoolee.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), umbrellaAsPoolee.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), umbrellaAsPoolee.RequisitionAmount);

            }
        }


        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_UmbrellaHasDirectExpenses_NoGlAccessToUmbrella()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);

            var dataContract = PopulateSingleGlAcctsDataContract(umbrellaAccount, umbrellaAccount, "U", true, false, false);
            glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // The total number of poolees should match the number of poolees as seed accounts.
                Assert.AreEqual(pooleeAccounts.Count, glAccountLineItem.Poolees.Count);

                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsNoAccessUmbrellaDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.RequisitionAmount);

                Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                Assert.IsFalse(glAccountLineItem.IsUmbrellaVisible);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glAccountLineItem.Poolees)
                {
                    // The umbrella should show up as a poolee...
                    List<GlAcctsMemos> glAcctsMemos = null;
                    if (poolee.GlAccountNumber == glAccountLineItem.GlAccount.GlAccountNumber)
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                            .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear
                                && x.GlPooledTypeAssocMember.ToUpper() == "U").ToList();
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                        Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), poolee.RequisitionAmount);
                    }
                    else
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                        .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear
                            && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                        Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                        Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), poolee.RequisitionAmount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_UmbrellaHasNoDirectExpenses_NoGlAccessToUmbrella()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);

            var dataContract = PopulateSingleGlAcctsDataContract(umbrellaAccount, umbrellaAccount, "U", false, false, false);
            glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // The total number of poolees should match the number of poolees as seed accounts.
                Assert.AreEqual(pooleeAccounts.Count, glAccountLineItem.Poolees.Count);

                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsNoAccessUmbrellaDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForUmbrella_GlAccts(objectGlAcctsMemos), glAccountLineItem.GlAccount.RequisitionAmount);

                Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                Assert.IsFalse(glAccountLineItem.IsUmbrellaVisible);

                // Make sure the umbrella does NOT show up as a poolee since it has no direct expenses.
                Assert.IsFalse(glAccountLineItem.Poolees.Any(x => x.GlAccountNumber == umbrellaAccount));

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glAccountLineItem.Poolees)
                {
                    var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                    .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_financeQueryCriteria.FiscalYear
                        && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                    Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                    Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlAccts(glAcctsMemos), poolee.RequisitionAmount);

                }

            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_AmountsAreNull()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000"   // Object 40000
            };
            var nonPooledAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);
            glAccounts.AddRange(nonPooledAccounts);

            this.glAcctsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.encIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.glsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false, false, true);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount, false, false, true);

            // Populate the non-pooled accounts.
            PopulateGlAccountDataContracts(nonPooledAccounts, GlBudgetPoolType.None, null, false, false, true);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();
            this.param4_financeQueryCriteria.IncludeActiveAccountsWithNoActivity = true;

            var glAcctsFilename = "GL.ACCTS";
            dataReaderMock.Setup(dr => dr.SelectAsync(glAcctsFilename, It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(glAccounts.ToArray());
            });

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(5, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // Make sure the lineitem totals are all zero.
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.RequisitionAmount);

                if (glAccountLineItem.GlAccountNumber == umbrellaAccount)
                {
                    Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                    Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

                    Assert.AreEqual(pooleeAccounts.Count, glAccountLineItem.Poolees.Count);
                    // Make sure the poolee amounts are all zero
                    foreach (var poolee in glAccountLineItem.Poolees)
                    {
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(0m, poolee.ActualAmount);
                        Assert.AreEqual(0m, poolee.EncumbranceAmount);
                        Assert.AreEqual(0m, poolee.RequisitionAmount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_OpenYear_AmountsAreNull_IncludeActiveAccountsWithNoActivity_false()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000"   // Object 40000
            };
            var nonPooledAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);
            glAccounts.AddRange(nonPooledAccounts);

            this.glAcctsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.encIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.glsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false, false, true);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount, false, false, true);

            // Populate the non-pooled accounts.
            PopulateGlAccountDataContracts(nonPooledAccounts, GlBudgetPoolType.None, null, false, false, true);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();
            this.param4_financeQueryCriteria.IncludeActiveAccountsWithNoActivity = false;

            var result = await RealRepository_GetGLAccountsListAsync();
            //no gl account line items will be returned.
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        #endregion

        #region GetFinanceQueryActivityDetailAsync

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_Success()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            PopulateEncFyrDataContracts(glAccounts);

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "10_00_01_01_EJK88_50000", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "1", GlaSource = "JE", GlaSysDate = new DateTime(), GlaTrDate = new DateTime() } }));
            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<EncFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<EncFyr>() { new EncFyr() { Recordkey = "10_00_01_01_EJK88_50000", EncPoEntityAssociation = new List<EncFyrEncPo>() { new EncFyrEncPo() { EncPoNoAssocMember = "1", EncPoDateAssocMember = new DateTime(), EncPoAmtAssocMember = 10m, EncPoVendorAssocMember = "1", EncPoIdAssocMember = "1" } }, EncReqEntityAssociation = new List<EncFyrEncReq>() { new EncFyrEncReq() { EncReqNoAssocMember = "2", EncReqDateAssocMember = new DateTime(), EncReqAmtAssocMember = 20m, EncReqVendorAssocMember = "2", EncReqIdAssocMember = "2" } } } }));
            
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(1, financeQueryActivityDetailItems.Count);
            Assert.AreEqual(3, financeQueryActivityDetailItems.First().Transactions.Count);
            // Make sure the amounts of each GL account match the amounts in the data contracts.
            foreach (var glAccountActivityDetail in financeQueryActivityDetailItems)
            {
                Assert.IsNotNull(glAccountActivityDetail);
                Assert.AreEqual("10_00_01_01_EJK88_50000", glAccountActivityDetail.GlAccountNumber);

            }
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_MissingFiscalYear()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = null;

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
            
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoGlUserAccounts()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            PopulateEncFyrDataContracts(glAccounts);

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "10_00_01_01_EJK88_50000", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "1", GlaSource = "JE", GlaSysDate = new DateTime(), GlaTrDate = new DateTime() } }));
            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<EncFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<EncFyr>() { new EncFyr() { Recordkey = "10_00_01_01_EJK88_50000", EncPoEntityAssociation = new List<EncFyrEncPo>() { new EncFyrEncPo() { EncPoNoAssocMember = "1", EncPoDateAssocMember = new DateTime(), EncPoAmtAssocMember = 10m, EncPoVendorAssocMember = "1", EncPoIdAssocMember = "1" } }, EncReqEntityAssociation = new List<EncFyrEncReq>() { new EncFyrEncReq() { EncReqNoAssocMember = "2", EncReqDateAssocMember = new DateTime(), EncReqAmtAssocMember = 20m, EncReqVendorAssocMember = "2", EncReqIdAssocMember = "2" } } } }));

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);

        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoGlUser()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = null;
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            PopulateEncFyrDataContracts(glAccounts);

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "10_00_01_01_EJK88_50000", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "1", GlaSource = "JE", GlaSysDate = new DateTime(), GlaTrDate = new DateTime() } }));
            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<EncFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<EncFyr>() { new EncFyr() { Recordkey = "10_00_01_01_EJK88_50000", EncPoEntityAssociation = new List<EncFyrEncPo>() { new EncFyrEncPo() { EncPoNoAssocMember = "1", EncPoDateAssocMember = new DateTime(), EncPoAmtAssocMember = 10m, EncPoVendorAssocMember = "1", EncPoIdAssocMember = "1" } }, EncReqEntityAssociation = new List<EncFyrEncReq>() { new EncFyrEncReq() { EncReqNoAssocMember = "2", EncReqDateAssocMember = new DateTime(), EncReqAmtAssocMember = 20m, EncReqVendorAssocMember = "2", EncReqIdAssocMember = "2" } } } }));

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);

        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoGlAccountStructure()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = null;
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
            
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoGlConfiguration()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                null,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
            
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_GenLdgrRecordIsNull()
        {
            // Get the GL User expense accounts from the test repository.
            var glAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1] = null;

            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_GenLdgrRecordStatusIsNull()
        {
            // Get the GL User expense accounts from the test repository.
            var glAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1].GenLdgrStatus = null;

            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoTransactionRecordKey()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "1", GlaSource = "JE", GlaSysDate = new DateTime(), GlaTrDate = new DateTime() } }));

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoTransactionSourceCode()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "10_00_01_01_EJK88_50000", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "1", GlaSource = "", GlaSysDate = new DateTime(), GlaTrDate = new DateTime() } }));

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoTransactionGlaRefNo()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "10_00_01_01_EJK88_50000", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "", GlaSource = "JE", GlaSysDate = new DateTime(), GlaTrDate = new DateTime() } }));

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
        }

        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoTransactionGlaTrDate()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "10_00_01_01_EJK88_50000", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "1", GlaSource = "JE", GlaSysDate = new DateTime(), GlaTrDate = null } }));

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
        }


        [TestMethod]
        public async Task GetFinanceQueryActivityDetailAsync_NoTransactionGlaTrDateWithEstimatedOB()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000 Asset
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000 Liability
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000 Fund Balance
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000 Revenue
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000 Expense
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000 Expense
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            glaIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(new Collection<GlaFyr>() { new GlaFyr() { Recordkey = "10_00_01_01_EJK88_50000", GlaAcctId = "10_00_01_01_EJK88_50000", GlaCredit = 0m, GlaDebit = 0m, GlaDescription = "1", GlaProjectsIds = "1", GlaRefNo = "1", GlaSource = "JE", GlaSysDate = new DateTime(), GlaTrDate = null } }));

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var result = await actualRepository.GetFinanceQueryActivityDetailAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");

            Assert.IsNotNull(result);

            var financeQueryActivityDetailItems = result.ToList();

            Assert.AreEqual(0, financeQueryActivityDetailItems.Count);
        }

        #endregion

        #region Closed year scenarios
        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_NonPooledAccountsOnly()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000

                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            this.glAcctsIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the non-pooled accounts
            PopulateGlsFyrDataContracts(glAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(glAccounts);           

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(12, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // Make sure the totals for each gl account match the amounts in the data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsDataContracts, this.param3_glClassConfiguration), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.RequisitionAmount);
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_ExcludeYEAmounts()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_10000",  // Object 10000
                "10_00_01_00_EJK99_10000",  // Object 10000

                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000

                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            this.glAcctsIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the non-pooled accounts
            PopulateGlsFyrDataContracts(glAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(glAccounts);

            // setup cfwebdefaults.CfwebFqIncludeYeAmounts to "N"
            CfwebDefaults cfwebDefaults = new CfwebDefaults() { CfwebFqIncludeYeAmounts = "N" };
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS", true)).Returns(() =>
            {
                return Task.FromResult(cfwebDefaults);
            });

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(12, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // Make sure the totals for each gl account match the amounts in the data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsDataContracts, this.param3_glClassConfiguration, false), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.RequisitionAmount);
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_UmbrellaHasDirectExpenses()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);

            this.glAcctsIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the umbrella account
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);
            PopulateEncFyrDataContracts(new List<string>() { umbrellaAccount });

            // Populate the poolee accounts
            PopulateGlsFyrDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);
            PopulateEncFyrDataContracts(pooleeAccounts);

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // The total number of poolees should match the number of poolees as seed accounts + the umbrella since it has direct expenses.
                Assert.AreEqual(pooleeAccounts.Count + 1, glAccountLineItem.Poolees.Count);

                // Make sure the totals for each gl account match the amounts in the data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsDataContracts), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.EncumbranceAmount);
                //Assert.AreEqual(Helper_RequisitionsForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.RequisitionAmount);

                Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glAccountLineItem.Poolees)
                {

                    List<GlsFyr> glsFyrContracts = glsFyrContracts = this.glsFyrDataContracts
                         .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    List<EncFyr> encFyrContracts = encFyrContracts = this.encFyrDataContracts
                        .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    // The umbrella should show up as a poolee...
                    if (poolee.GlAccountNumber == umbrellaAccount)
                    {
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                    }
                    else
                    {
                        Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), poolee.BudgetAmount);
                    }
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.EncumbranceAmount);
                    Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.RequisitionAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_UmbrellaHasNoDirectExpenses()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);

            this.glAcctsIds = glAccounts.ToArray();
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the umbrella account
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, umbrellaHasDirectExpenses: false);
            PopulateEncFyrDataContracts(new List<string>() { umbrellaAccount }, amountsAreZero: true);

            // Populate the poolee accounts
            PopulateGlsFyrDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);
            PopulateEncFyrDataContracts(pooleeAccounts);

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // The total number of poolees should match the number of poolees as seed accounts.
                Assert.AreEqual(pooleeAccounts.Count, glAccountLineItem.Poolees.Count);

                // Make sure the totals for each gl account match the amounts in the data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey == glAccountLineItem.GlAccountNumber).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsDataContracts), glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.EncumbranceAmount);
                //Assert.AreEqual(Helper_RequisitionsForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.RequisitionAmount);

                Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);


                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glAccountLineItem.Poolees)
                {
                    List<GlsFyr> glsFyrContracts = glsFyrContracts = this.glsFyrDataContracts
                        .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    List<EncFyr> encFyrContracts = encFyrContracts = this.encFyrDataContracts
                        .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), poolee.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.EncumbranceAmount);
                    Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.RequisitionAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_PooleeWithOnlyRequisitionAmounts_UmbrellaWithGls_UmbrellaIsVisible()
        {
            // The umbrella has posted activity in GLS but no direct expenses.
            // Only one poolee that has only a requisition amount.
            var umbrellaAccount = "11_01_01_00_10001_55200";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_10001_55204"
            };

            var dataContract = new EncFyr()
            {
                Recordkey = "10_00_01_01_10001_55204",
                EncReqAmt = new List<decimal?>() { 123m },
                EncReqEntityAssociation = new List<EncFyrEncReq>()
                {
                    new EncFyrEncReq
                    {

                        EncReqNoAssocMember = "0000001",
                        EncReqDateAssocMember = new DateTime(),
                        EncReqVendorAssocMember = "Supplies",
                        EncReqAmtAssocMember = 123m,
                        EncReqIdAssocMember = "12"
                    }
                 }
            };
            var glpFyr = new GlpFyr()
            {
                Recordkey = "11_01_01_00_10001_55200",
                GlpPooleeAcctsList = new List<string>()
                {
                    "10_00_01_01_10001_55204"
                }
            };

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the GLS record for the umbrella account.
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false);

            encFyrDataContracts.Add(dataContract);
            glpFyrDataContracts.Add(glpFyr);

            this.param4_FiscalYear = Convert.ToInt32(testGlConfigurationRepository.ClosedYear);
            var glpFyrFilename = "GLP." + this.param4_FiscalYear;

            string[] glpFyrIds = new string[] { "11_01_01_00_10001_55200" };
            dataReaderMock.Setup(dr => dr.SelectAsync(glpFyrFilename, null)).Returns(() =>
            {
                return Task.FromResult(glpFyrIds);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlpFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glpFyrDataContracts);
            });

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);
            var glAccountLineItem = financeQueryGlAccountLineItems.First();
            Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
            Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

            Assert.AreEqual(umbrellaAccount, glAccountLineItem.GlAccount.GlAccountNumber);
            Assert.AreEqual(pooleeAccounts.FirstOrDefault(), glAccountLineItem.Poolees.FirstOrDefault().GlAccountNumber);
        }


        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_PooleeWithOnlyRequisitionAmounts_UmbrellaWithGls_UmbrellaIsNotVisible()
        {
            // The umbrella has posted activity in GLS but no direct expenses.
            // Only one poolee that has only a requisition amount.
            var umbrellaAccount = "11_01_01_00_10001_55200";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_10001_55204"
            };

            var dataContract = new EncFyr()
            {
                Recordkey = "10_00_01_01_10001_55204",
                EncReqAmt = new List<decimal?>() { 123m },
                EncReqEntityAssociation = new List<EncFyrEncReq>()
                {
                    new EncFyrEncReq
                    {

                        EncReqNoAssocMember = "0000001",
                        EncReqDateAssocMember = new DateTime(),
                        EncReqVendorAssocMember = "Supplies",
                        EncReqAmtAssocMember = 123m,
                        EncReqIdAssocMember = "12"
                    }
                 }
            };
            var glpFyr = new GlpFyr()
            {
                Recordkey = "11_01_01_00_10001_55200",
                GlpPooleeAcctsList = new List<string>()
                {
                    "10_00_01_01_10001_55204"
                }
            };

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>());
            // Don't add the umbrella to the list of GL accounts for the user.
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the GLS record for the umbrella account.
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false);
            var umbrellaGlsRecord = glsFyrDataContracts.FirstOrDefault();
            glsFyrDataContracts = new Collection<GlsFyr>();

            encFyrDataContracts.Add(dataContract);
            glpFyrDataContracts.Add(glpFyr);

            this.param4_FiscalYear = Convert.ToInt32(testGlConfigurationRepository.ClosedYear);
            var glpFyrFilename = "GLP." + this.param4_FiscalYear;

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlsFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glsFyrDataContracts);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<GlsFyr>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(
                (string fileName, string glNumber, bool flag) =>
                {
                    //var dataContract = this.glsFyrNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == glNumber);

                    return Task.FromResult(umbrellaGlsRecord);
                });

            string[] glpFyrIds = new string[] { "11_01_01_00_10001_55200" };
            dataReaderMock.Setup(dr => dr.SelectAsync(glpFyrFilename, null)).Returns(() =>
            {
                return Task.FromResult(glpFyrIds);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlpFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glpFyrDataContracts);
            });

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            var glAccountLineItem = financeQueryGlAccountLineItems.First();
            Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
            Assert.IsFalse(glAccountLineItem.IsUmbrellaVisible);

            Assert.AreEqual(umbrellaAccount, glAccountLineItem.GlAccount.GlAccountNumber);
            Assert.AreEqual(pooleeAccounts.FirstOrDefault(), glAccountLineItem.Poolees.FirstOrDefault().GlAccountNumber);

            var poolGls = glsFyrDataContracts.Where(y => y.Recordkey == umbrellaAccount).FirstOrDefault();
            var encFyrContract = this.encFyrDataContracts.Where(x => x.Recordkey == pooleeAccounts.FirstOrDefault()).ToList().FirstOrDefault();
            var poolRequisitions = encFyrContract.EncReqAmt.Sum().HasValue ? encFyrContract.EncReqAmt.Sum().Value : 0m;

            Assert.AreEqual(glAccountLineItem.GlAccount.BudgetAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.ActualAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.EncumbranceAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.RequisitionAmount, poolRequisitions);

        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_PooleeWithOnlyRequisitionAmountsAndUmbrellaWithNoGls__UmbrellaIsVisible()
        {
            // The umbrella does not have posted activity in GLS and no direct expenses.
            // Only one poolee that has only a requisition amount.
            // User has access to umbrella.
            var umbrellaAccount = "11_01_01_00_10001_55200";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_10001_55204"
            };

            var dataContract = new EncFyr()
            {
                Recordkey = "10_00_01_01_10001_55204",
                EncReqAmt = new List<decimal?>() { 123m },
                EncReqEntityAssociation = new List<EncFyrEncReq>()
                {
                    new EncFyrEncReq
                    {

                        EncReqNoAssocMember = "0000001",
                        EncReqDateAssocMember = new DateTime(),
                        EncReqVendorAssocMember = "Supplies",
                        EncReqAmtAssocMember = 123m,
                        EncReqIdAssocMember = "12"
                    }
                 }
            };
            var glpFyr = new GlpFyr()
            {
                Recordkey = "11_01_01_00_10001_55200",
                GlpPooleeAcctsList = new List<string>()
                {
                    "10_00_01_01_10001_55204"
                }
            };

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            encFyrDataContracts.Add(dataContract);
            glpFyrDataContracts.Add(glpFyr);

            this.param4_FiscalYear = Convert.ToInt32(testGlConfigurationRepository.ClosedYear);
            var glpFyrFilename = "GLP." + this.param4_FiscalYear;

            string[] glpFyrIds = new string[] { "11_01_01_00_10001_55200" };
            dataReaderMock.Setup(dr => dr.SelectAsync(glpFyrFilename, null)).Returns(() =>
            {
                return Task.FromResult(glpFyrIds);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlpFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glpFyrDataContracts);
            });

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            var glAccountLineItem = financeQueryGlAccountLineItems.First();
            Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
            Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

            Assert.AreEqual(umbrellaAccount, glAccountLineItem.GlAccount.GlAccountNumber);
            Assert.AreEqual(pooleeAccounts.FirstOrDefault(), glAccountLineItem.Poolees.FirstOrDefault().GlAccountNumber);

            var poolGls = glsFyrDataContracts.Where(y => y.Recordkey == umbrellaAccount).FirstOrDefault();
            var encFyrContract = this.encFyrDataContracts.Where(x => x.Recordkey == pooleeAccounts.FirstOrDefault()).ToList().FirstOrDefault();
            var poolRequisitions = encFyrContract.EncReqAmt.Sum().HasValue ? encFyrContract.EncReqAmt.Sum().Value : 0m;

            Assert.AreEqual(glAccountLineItem.GlAccount.BudgetAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.ActualAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.EncumbranceAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.RequisitionAmount, poolRequisitions);

        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_PooleeWithOnlyRequisitionAmountsAndUmbrellaWithNoGls__UmbrellaIsNotVisible()
        {
            // The umbrella does not have posted activity in GLS and no direct expenses.
            // Only one poolee that has only a requisition amount.
            // User does NOT have access to umbrella.
            var umbrellaAccount = "11_01_01_00_10001_55200";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_10001_55204"
            };

            var dataContract = new EncFyr()
            {
                Recordkey = "10_00_01_01_10001_55204",
                EncReqAmt = new List<decimal?>() { 123m },
                EncReqEntityAssociation = new List<EncFyrEncReq>()
                {
                    new EncFyrEncReq
                    {

                        EncReqNoAssocMember = "0000001",
                        EncReqDateAssocMember = new DateTime(),
                        EncReqVendorAssocMember = "Supplies",
                        EncReqAmtAssocMember = 123m,
                        EncReqIdAssocMember = "12"
                    }
                 }
            };
            var glpFyr = new GlpFyr()
            {
                Recordkey = "11_01_01_00_10001_55200",
                GlpPooleeAcctsList = new List<string>()
                {
                    "10_00_01_01_10001_55204"
                }
            };

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>());
            // Don't add the umbrella to the user list of GL accounts.
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            encFyrDataContracts.Add(dataContract);
            glpFyrDataContracts.Add(glpFyr);

            this.param4_FiscalYear = Convert.ToInt32(testGlConfigurationRepository.ClosedYear);
            var glpFyrFilename = "GLP." + this.param4_FiscalYear;

            string[] glpFyrIds = new string[] { "11_01_01_00_10001_55200" };
            dataReaderMock.Setup(dr => dr.SelectAsync(glpFyrFilename, null)).Returns(() =>
            {
                return Task.FromResult(glpFyrIds);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlpFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glpFyrDataContracts);
            });

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            var glAccountLineItem = financeQueryGlAccountLineItems.First();
            Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
            Assert.IsFalse(glAccountLineItem.IsUmbrellaVisible);

            Assert.AreEqual(umbrellaAccount, glAccountLineItem.GlAccount.GlAccountNumber);
            Assert.AreEqual(pooleeAccounts.FirstOrDefault(), glAccountLineItem.Poolees.FirstOrDefault().GlAccountNumber);

            var poolGls = glsFyrDataContracts.Where(y => y.Recordkey == umbrellaAccount).FirstOrDefault();
            var encFyrContract = this.encFyrDataContracts.Where(x => x.Recordkey == pooleeAccounts.FirstOrDefault()).ToList().FirstOrDefault();
            var poolRequisitions = encFyrContract.EncReqAmt.Sum().HasValue ? encFyrContract.EncReqAmt.Sum().Value : 0m;

            Assert.AreEqual(glAccountLineItem.GlAccount.BudgetAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.ActualAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.EncumbranceAmount, 0m);
            Assert.AreEqual(glAccountLineItem.GlAccount.RequisitionAmount, poolRequisitions);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_UmbrellaHasDirectExpenses_NoPoolees()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the umbrella account
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);
            PopulateEncFyrDataContracts(new List<string>() { umbrellaAccount });

            var glObjectCodes = await RealRepository_GetGLAccountsListAsync();

            // Make sure each each GL class is represented in the total number of GL Object codes.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            var glAccountLineItem = financeQueryGlAccountLineItems.First();
            Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
            Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

            Assert.AreEqual(umbrellaAccount, glAccountLineItem.GlAccount.GlAccountNumber);
            // There should be only one poolee, the umbrella.
            Assert.AreEqual(umbrellaAccount, glAccountLineItem.Poolees.FirstOrDefault().GlAccountNumber);

            // Make sure the totals for each GL object code match the amounts in the umbrella data contracts.
            var glsDataContracts = this.glsFyrDataContracts
                .Where(x => x.Recordkey == umbrellaAccount).ToList();
            var encDataContracts = this.encFyrDataContracts
                .Where(x => x.Recordkey == umbrellaAccount).ToList();

            Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glAccountLineItem.GlAccount.BudgetAmount);
            Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsDataContracts), glAccountLineItem.GlAccount.ActualAmount);
            Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.EncumbranceAmount);
            Assert.AreEqual(Helper_RequisitionsForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccountLineItem.GlAccount.RequisitionAmount);

            Assert.AreEqual(1, glAccountLineItem.Poolees.Count);

            // Make sure the umbrella shows up as a poolee since it has direct expenses.
            var umbrellaAsPoolee = glAccountLineItem.Poolees.FirstOrDefault(x => x.GlAccountNumber == umbrellaAccount);
            Assert.IsNotNull(umbrellaAsPoolee);

            // Make sure the umbrella amounts match the amounts in the data contract.
            var glsFyrContracts = this.glsFyrDataContracts
                .Where(x => x.Recordkey == umbrellaAsPoolee.GlAccountNumber).ToList();
            var encFyrContracts = this.encFyrDataContracts
                .Where(x => x.Recordkey == umbrellaAsPoolee.GlAccountNumber).ToList();
            Assert.AreEqual(0m, umbrellaAsPoolee.BudgetAmount);
            Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), umbrellaAsPoolee.ActualAmount);
            Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), umbrellaAsPoolee.EncumbranceAmount);
            Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), umbrellaAsPoolee.RequisitionAmount);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_UmbrellaHasDirectExpenses_NoGlAccessToUmbrella()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var dataContract = PopulateSingleGlsFyrDataContract(umbrellaAccount, umbrellaAccount, "U", true, false, false);
            glsFyrNoAccessUmbrellaDataContracts.Add(dataContract);

            // Populate the poolee accounts for each umbrella
            PopulateGlsFyrDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);
            PopulateEncFyrDataContracts(pooleeAccounts);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Make sure each each GL class is represented in the total number of GL Object codes.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            var glAccountLineItem = financeQueryGlAccountLineItems.First();
            Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
            // The umbrella should NOT be visible.
            Assert.IsFalse(glAccountLineItem.IsUmbrellaVisible);

            // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
            var glsFyrContracts = glsFyrNoAccessUmbrellaDataContracts
                .Where(x => x.Recordkey == umbrellaAccount).ToList();

            Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), glAccountLineItem.GlAccount.BudgetAmount);
            Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsFyrContracts), glAccountLineItem.GlAccount.ActualAmount);
            Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsFyrContracts, null), glAccountLineItem.GlAccount.EncumbranceAmount);

            // The umbrella should NOT show up as a poolee.
            Assert.IsFalse(glAccountLineItem.Poolees.Select(x => x.GlAccountNumber).Contains(umbrellaAccount));

            // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
            foreach (var poolee in glAccountLineItem.Poolees)
            {
                glsFyrContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                var encFyrContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), poolee.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), poolee.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.EncumbranceAmount);
                Assert.AreEqual(Helper_RequisitionsForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.RequisitionAmount);
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_UmbrellaHasNoDirectExpenses_NoGlAccessToUmbrella()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var dataContract = PopulateSingleGlsFyrDataContract(umbrellaAccount, umbrellaAccount, "U", false, false, false);
            glsFyrNoAccessUmbrellaDataContracts.Add(dataContract);

            // Populate the poolee accounts for each umbrella
            PopulateGlsFyrDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);
            PopulateEncFyrDataContracts(pooleeAccounts);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Make sure each each GL class is represented in the total number of GL Object codes.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(1, financeQueryGlAccountLineItems.Count);

            var glAccountLineItem = financeQueryGlAccountLineItems.First();
            Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
            // The umbrella should NOT be visible.
            Assert.IsFalse(glAccountLineItem.IsUmbrellaVisible);

            // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
            var glsFyrContracts = glsFyrNoAccessUmbrellaDataContracts
                .Where(x => x.Recordkey == umbrellaAccount).ToList();

            Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), glAccountLineItem.GlAccount.BudgetAmount);
            Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsFyrContracts), glAccountLineItem.GlAccount.ActualAmount);
            Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsFyrContracts, null), glAccountLineItem.GlAccount.EncumbranceAmount);

            // The umbrella should NOT show up as a poolee.
            Assert.IsFalse(glAccountLineItem.Poolees.Select(x => x.GlAccountNumber).Contains(umbrellaAccount));

            // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
            foreach (var poolee in glAccountLineItem.Poolees)
            {

                glsFyrContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                var encFyrContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), poolee.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), poolee.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.EncumbranceAmount);
            }

        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ClosedYear_AmountsAreNull()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            var pooleeAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000"   // Object 40000
            };
            var nonPooledAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            var glAccounts = new List<string>();
            glAccounts.Add(umbrellaAccount);
            glAccounts.AddRange(pooleeAccounts);
            glAccounts.AddRange(nonPooledAccounts);

            var glAcctsFilename = "GL.ACCTS";
            dataReaderMock.Setup(dr => dr.SelectAsync(glAcctsFilename, It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(glAccounts.ToArray());
            });

            this.glAcctsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.encIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.glsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();

            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false, false, true);

            // Populate the poolee accounts
            PopulateGlsFyrDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount, false, false, true);
            PopulateEncFyrDataContracts(pooleeAccounts, false, true);

            // Populate the non-pooled accounts
            PopulateGlsFyrDataContracts(nonPooledAccounts, GlBudgetPoolType.None, "", false, false, true);
            PopulateEncFyrDataContracts(nonPooledAccounts, false, true);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.AddAllAccounts(nonPooledAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_financeQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;
            this.param4_financeQueryCriteria.IncludeActiveAccountsWithNoActivity = true;

            // Make sure all six GL classes are represented.
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            var financeQueryGlAccountLineItems = result.ToList();
            Assert.AreEqual(5, financeQueryGlAccountLineItems.Count);

            foreach (var glAccountLineItem in financeQueryGlAccountLineItems)
            {
                // Make sure the lineitem totals are all zero.
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.BudgetAmount);
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.ActualAmount);
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.EncumbranceAmount);
                Assert.AreEqual(0m, glAccountLineItem.GlAccount.RequisitionAmount);

                if (glAccountLineItem.GlAccountNumber == umbrellaAccount)
                {
                    Assert.IsTrue(glAccountLineItem.IsUmbrellaAccount);
                    Assert.IsTrue(glAccountLineItem.IsUmbrellaVisible);

                    Assert.AreEqual(pooleeAccounts.Count, glAccountLineItem.Poolees.Count);
                    // Make sure the poolee amounts are all zero
                    foreach (var poolee in glAccountLineItem.Poolees)
                    {
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(0m, poolee.ActualAmount);
                        Assert.AreEqual(0m, poolee.EncumbranceAmount);
                        Assert.AreEqual(0m, poolee.RequisitionAmount);
                    }
                }
            }
        }

        #endregion

        #region General Error-Type Scenarios
        [TestMethod]
        public async Task GetGLAccountsListAsync_NullFiscalYear()
        {
            this.param4_financeQueryCriteria.FiscalYear = null;
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_EmptyFiscalYear()
        {
            this.param4_financeQueryCriteria.FiscalYear = "";
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullGeneralLedgerUser()
        {
            this.param1_GlUser = null;
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullAccountStructure()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param2_glAccountStructure = null;
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullGlClassConfiguration()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param3_glClassConfiguration = null;
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullExpenseAccounts()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NoExpenseAccounts()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_GenLdgrRecordIsNull()
        {
            // Get the GL User expense accounts from the test repository.
            var glAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1] = null;

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_GenLdgrRecordStatusIsNull()
        {
            // Get the GL User expense accounts from the test repository.
            var glAccounts = new List<string>()
            {
                "10_00_01_01_EJK88_20000",  // Object 20000
                "10_00_01_01_EJK99_20000",  // Object 20000

                "10_00_01_02_EJK88_30000",  // Object 30000
                "10_00_01_02_EJK99_30000",  // Object 30000

                "10_00_01_00_EJK88_40000",  // Object 40000
                "10_00_01_00_EJK99_40000",  // Object 40000

                "10_00_01_01_EJK88_50000",  // Object 50000
                "10_00_01_01_EJK99_50000",  // Object 50000
                "10_00_01_02_EJK88_70000",  // Object 70000
                "10_00_01_02_EJK99_70000"   // Object 70000
            };
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddAllAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1].GenLdgrStatus = null;

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_GenLdgrRecordStatusIsEmpty()
        {
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1].GenLdgrStatus = "";

            var result = await RealRepository_GetGLAccountsListAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        #endregion

        #region Private methods

        #region Budget helper methods
        private decimal Helper_BudgetForUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var budgetAmount = amounts.Sum(x => x.FaBudgetMemoAssocMember.Value) + amounts.Sum(x => x.FaBudgetPostedAssocMember.Value);
            return budgetAmount;
        }

        private decimal Helper_BudgetForNonUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var budgetAmount = amounts.Sum(x => x.GlBudgetMemosAssocMember.Value) + amounts.Sum(x => x.GlBudgetPostedAssocMember.Value);
            return budgetAmount;
        }

        private decimal Helper_BudgetForAnyAccount_Gls(IEnumerable<GlsFyr> glsDataContracts)
        {
            var budgetAmount = 0m;
            foreach (var glsDataContract in glsDataContracts)
            {
                budgetAmount += glsDataContract.BAlocDebitsYtd.HasValue ? glsDataContract.BAlocDebitsYtd.Value : 0m;
                budgetAmount -= glsDataContract.BAlocCreditsYtd.HasValue ? glsDataContract.BAlocCreditsYtd.Value : 0m;
            }

            return budgetAmount;
        }
        #endregion

        #region Actuals helper methods
        private decimal Helper_ActualsForUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.FaActualMemoAssocMember.Value) + amounts.Sum(x => x.FaActualPostedAssocMember.Value);
            return actualsAmount;
        }

        private decimal Helper_ActualsForNonUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.GlActualMemosAssocMember.Value) + amounts.Sum(x => x.GlActualPostedAssocMember.Value);
            return actualsAmount;
        }

        private decimal Helper_ActualsForUmbrella_Gls(List<GlsFyr> dataContracts)
        {
            var actualsAmount = 0m;
            foreach (var dataContract in dataContracts)
            {
                foreach (var amount in dataContract.GlsFaMactuals)
                {
                    actualsAmount += amount.HasValue ? amount.Value : 0m;
                }
            }

            return actualsAmount;
        }

        private decimal Helper_ActualsForNonUmbrella_Gls(IEnumerable<GlsFyr> glsDataContracts, GeneralLedgerClassConfiguration glClassConfiguration, bool includeYEClosingAmounts = true)
        {
            var actualsAmount = 0m;
            var glClass = GetGlAccountGlClass(glsDataContracts.First().Recordkey, glClassConfiguration);

            if (glClass == GlClass.FundBalance)
            {
                foreach (var glsDataContract in glsDataContracts)
                {
                    actualsAmount += glsDataContract.OpenBal.HasValue ? glsDataContract.OpenBal.Value : 0m;
                    if (includeYEClosingAmounts)
                    {
                        actualsAmount += glsDataContract.CloseDebits.HasValue ? glsDataContract.CloseDebits.Value : 0m;
                        actualsAmount -= glsDataContract.CloseCredits.HasValue ? glsDataContract.CloseCredits.Value : 0m;
                    }
                }
            }
            else
            {
                foreach (var glsDataContract in glsDataContracts)
                {
                    actualsAmount += glsDataContract.OpenBal.HasValue ? glsDataContract.OpenBal.Value : 0m;
                    actualsAmount += glsDataContract.DebitsYtd.HasValue ? glsDataContract.DebitsYtd.Value : 0m;
                    actualsAmount -= glsDataContract.CreditsYtd.HasValue ? glsDataContract.CreditsYtd.Value : 0m;
                }
            }

            return actualsAmount;
        }
        #endregion

        #region Encumbrances helper methods
        private decimal Helper_EncumbrancesForUmbrella_GlsAndEnc(IEnumerable<GlsFyr> glsDataContracts, IEnumerable<EncFyr> encDataContracts)
        {
            var encumbranceAmount = 0m;

            if (glsDataContracts != null)
            {
                foreach (var glsDataContract in glsDataContracts)
                {
                    foreach (var amount in glsDataContract.GlsFaMencumbrances)
                    {
                        encumbranceAmount += amount.HasValue ? amount.Value : 0m;
                    }
                }
            }

            return encumbranceAmount;
        }

        private decimal Helper_EncumbrancesForNonUmbrella_GlsAndEnc(IEnumerable<GlsFyr> glsDataContracts, IEnumerable<EncFyr> encDataContracts)
        {
            var encumbranceAmount = 0m;
            foreach (var glsDataContract in glsDataContracts)
            {
                encumbranceAmount += glsDataContract.EOpenBal.HasValue ? glsDataContract.EOpenBal.Value : 0m;
                encumbranceAmount += glsDataContract.EncumbrancesYtd.HasValue ? glsDataContract.EncumbrancesYtd.Value : 0m;
                encumbranceAmount -= glsDataContract.EncumbrancesRelievedYtd.HasValue ? glsDataContract.EncumbrancesRelievedYtd.Value : 0m;
            }

            return encumbranceAmount;
        }

        private decimal Helper_EncumbrancesForUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var encumbrances = amounts.Sum(x => x.FaEncumbranceMemoAssocMember.Value) + amounts.Sum(x => x.FaEncumbrancePostedAssocMember.Value);
            return encumbrances;
        }

        private decimal Helper_EncumbrancesForNonUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var encumbrances = amounts.Sum(x => x.GlEncumbranceMemosAssocMember.Value) + amounts.Sum(x => x.GlEncumbrancePostedAssocMember.Value);
            return encumbrances;
        }

        #endregion

        #region Requisitions helper methods

        private decimal Helper_RequisitionsForUmbrella_GlsAndEnc(IEnumerable<GlsFyr> glsDataContracts, IEnumerable<EncFyr> encDataContracts)
        {
            var requisitions = 0m;

            if (encDataContracts != null)
            {
                foreach (var encDataContract in encDataContracts)
                {
                    foreach (var encAmount in encDataContract.EncReqAmt)
                    {
                        requisitions += encAmount.HasValue ? encAmount.Value : 0m;
                    }
                }
            }

            return requisitions;
        }

        private decimal Helper_RequisitionsForNonUmbrella_GlsAndEnc(IEnumerable<GlsFyr> glsDataContracts, IEnumerable<EncFyr> encDataContracts)
        {
            var requisitions = 0m;

            foreach (var encDataContract in encDataContracts)
            {
                foreach (var encAmount in encDataContract.EncReqAmt)
                {
                    requisitions += encAmount.HasValue ? encAmount.Value : 0m;
                }
            }

            return requisitions;
        }

        private decimal Helper_RequisitionsForUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var requisitions = amounts.Sum(x => x.FaRequisitionMemoAssocMember.Value);
            return requisitions;
        }


        private decimal Helper_RequisitionsForNonUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var requisitions = amounts.Sum(x => x.GlRequisitionMemosAssocMember.Value);
            return requisitions;
        }
        #endregion

        public async Task<IEnumerable<FinanceQueryGlAccountLineItem>> RealRepository_GetGLAccountsListAsync()
        {
            return await actualRepository.GetGLAccountsListAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_financeQueryCriteria,
                "0000001");
        }

        private void PopulateGlsFyrDataContracts(List<string> glAccounts, GlBudgetPoolType poolType, string umbrellaReference,
            bool umbrellaHasDirectExpenses = true, bool amountsAreZero = false, bool amountsAreNull = false)
        {
            // Initialize the pool type to blank for non-pooled accounts
            string dataContractPoolType = "";
            string dataContractUmbrellaReference = "";

            // Set the pool type to the "U"mbrella if the type is an umbrella
            if (poolType == GlBudgetPoolType.Umbrella)
            {
                dataContractPoolType = "U";
            }

            // Set the pool type to the "P"oolee if the type is a poolee
            if (poolType == GlBudgetPoolType.Poolee)
            {
                dataContractPoolType = "P";
            }

            foreach (var glAccount in glAccounts)
            {
                // Set the umbrella reference for an umbrella GL account
                if (poolType == GlBudgetPoolType.Umbrella)
                {
                    dataContractUmbrellaReference = glAccount;
                }

                // Set the umbrella reference for a poolee GL account
                if (poolType == GlBudgetPoolType.Poolee)
                {
                    dataContractUmbrellaReference = umbrellaReference;
                }

                var dataContract = PopulateSingleGlsFyrDataContract(glAccount, dataContractUmbrellaReference, dataContractPoolType, umbrellaHasDirectExpenses, amountsAreZero, amountsAreNull);
                glsFyrDataContracts.Add(dataContract);
            }
        }

        private GlsFyr PopulateSingleGlsFyrDataContract(string glAccount, string dataContractUmbrellaReference, string dataContractPoolType,
            bool umbrellaHasDirectExpenses, bool amountsAreZero, bool amountsAreNull)
        {
            // Default the amounts for the GLS.FYR data contracts
            decimal? openBal = 250000m,
                bAlocDebitsYtd = 10000m,
                bAlocCreditsYtd = 500m,
                eOpenBal = 150m,
                encumbrancesYtd = 50m,
                encumbrancesRelievedYtd = 75m,
                debitsYtd = 500m,
                creditsYtd = 300m,
                closeDebits = 569m,
                closeCredits = 93m;

            var glsFaMactuals = new List<decimal?>() { 250m, 250m };
            var glsFaMencumbrances = new List<decimal?>() { 100m, 100m };


            // Zero out the direct expenses fields for the umbrella if the parameter says so
            if (!umbrellaHasDirectExpenses)
            {
                eOpenBal = 0m;
                encumbrancesYtd = 0m;
                encumbrancesRelievedYtd = 0m;
                debitsYtd = 0m;
                creditsYtd = 0m;
                openBal = 0m;
            }

            // Zero out the numbers if the parameter says so...
            if (amountsAreZero)
            {
                openBal = 0m;
                bAlocDebitsYtd = 0m;
                bAlocCreditsYtd = 0m;
                eOpenBal = 0m;
                encumbrancesYtd = 0m;
                encumbrancesRelievedYtd = 0m;
                debitsYtd = 0m;
                creditsYtd = 0m;
                glsFaMactuals = new List<decimal?>() { 0m, 0m };
                glsFaMencumbrances = new List<decimal?>() { 0m, 0m };
                eOpenBal = 0m;
                encumbrancesYtd = 0m;
                encumbrancesRelievedYtd = 0m;
                debitsYtd = 0m;
                creditsYtd = 0m;
                closeDebits = 0m;
                closeCredits = 0m;
            }

            // Null out the numbers if the parameter says so...
            if (amountsAreNull)
            {
                openBal = null;
                bAlocDebitsYtd = null;
                bAlocCreditsYtd = null;
                eOpenBal = null;
                encumbrancesYtd = null;
                encumbrancesRelievedYtd = null;
                debitsYtd = null;
                creditsYtd = null;
                glsFaMactuals = new List<decimal?>() { null, null };
                glsFaMencumbrances = new List<decimal?>() { null, null };
                eOpenBal = null;
                encumbrancesYtd = null;
                encumbrancesRelievedYtd = null;
                debitsYtd = null;
                creditsYtd = null;
                closeDebits = null;
                closeCredits = null;
            }

            var dataContract = new GlsFyr()
            {
                Recordkey = glAccount,
                OpenBal = openBal,
                BAlocDebitsYtd = bAlocDebitsYtd,
                BAlocCreditsYtd = bAlocCreditsYtd,
                GlsFaMactuals = glsFaMactuals,
                GlsFaMencumbrances = glsFaMencumbrances,
                EOpenBal = eOpenBal,
                EncumbrancesYtd = encumbrancesYtd,
                EncumbrancesRelievedYtd = encumbrancesRelievedYtd,
                DebitsYtd = debitsYtd,
                CreditsYtd = creditsYtd,
                GlsBudgetLinkage = dataContractUmbrellaReference,
                GlsPooledType = dataContractPoolType,
                CloseDebits = closeDebits,
                CloseCredits = closeCredits
            };
            dataContract.buildAssociations();

            return dataContract;
        }

        private void PopulateEncFyrDataContracts(List<string> glAccounts, bool amountsAreZero = false, bool amountsAreNull = false)
        {
            foreach (var glAccount in glAccounts)
            {
                var dataContract = PopulateSingleEncFyrDataContract(glAccount, amountsAreZero, amountsAreNull);
                encFyrDataContracts.Add(dataContract);
            }
        }

        private EncFyr PopulateSingleEncFyrDataContract(string glAccount, bool amountsAreZero, bool amountsAreNull)
        {
            // Default the amounts for the GL.ACCTS data contracts
            var encReqAmt = new List<decimal?>() { 100m, 75m };

            // Zero out the numbers if the parameter says so...
            if (amountsAreZero)
            {
                encReqAmt = new List<decimal?>() { 0m, 0m };
            }

            // Null out the numbers if the parameter says so...
            if (amountsAreNull)
            {
                encReqAmt = new List<decimal?>() { null, null };
            }

            var dataContract = new EncFyr()
            {
                Recordkey = glAccount,
                EncReqAmt = encReqAmt,
                EncReqEntityAssociation = new List<EncFyrEncReq>()
                {
                    new EncFyrEncReq
                    {

                        EncReqNoAssocMember = "0000001",
                        EncReqDateAssocMember = new DateTime(),
                        EncReqVendorAssocMember = "Supplies",
                        EncReqAmtAssocMember = 100m,
                        EncReqIdAssocMember = "11"
                    },
                    new EncFyrEncReq
                    {

                        EncReqNoAssocMember = "0000002",
                        EncReqDateAssocMember = new DateTime(),
                        EncReqVendorAssocMember = "Supplies",
                        EncReqAmtAssocMember = 75m,
                        EncReqIdAssocMember = "12"
                    }
                }
            };

            return dataContract;
        }

        private GlClass GetGlAccountGlClass(string glAccount, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            GlClass glClass = new GlClass();
            try
            {
                string glAccountGlClass = glAccount.Substring(glClassConfiguration.GlClassStartPosition, glClassConfiguration.GlClassLength);
                if (!string.IsNullOrEmpty(glAccountGlClass))
                {
                    if (glClassConfiguration.ExpenseClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Expense;
                    }
                    else if (glClassConfiguration.RevenueClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Revenue;
                    }
                    else if (glClassConfiguration.AssetClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Asset;
                    }
                    else if (glClassConfiguration.LiabilityClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Liability;
                    }
                    else if (glClassConfiguration.FundBalanceClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.FundBalance;
                    }
                    else
                    {
                        throw new ApplicationException("Invalid glClass for GL account: " + glAccount);
                    }
                }
                else
                {
                    throw new ApplicationException("Missing glClass for GL account: " + glAccount);
                }

                return glClass;
            }
            catch (ApplicationException aex)
            {
                logger.Warn("Invalid/unsupported GL class.");
                glClass = GlClass.Asset;
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred determining GL class for GL account number.");
                glClass = GlClass.Asset;
            }

            return glClass;
        }

        private void BuildGlSourceCodes()
        {
            // Mock up GL.SOURCE.CODES
            GlSourceCodes = new ApplValcodes();
            GlSourceCodes.ValInternalCode = new List<string>()
            {
                "AA",
                "AE",
                "EP",
                "ER",
                "PJ",
                "JE"
            };
            GlSourceCodes.ValActionCode2 = new List<string>()
            {
                "1",
                "3",
                "3",
                "4",
                "1",
                "1"
            };
            GlSourceCodes.buildAssociations();
        }

        private List<GlAccts> PopulateGlAccountDataContracts(List<string> glAccounts, GlBudgetPoolType poolType, string umbrellaReference,
            bool umbrellaHasDirectExpenses = true, bool amountsAreZero = false, bool amountsAreNull = false)
        {
            List<GlAccts> glAccts = new List<GlAccts>();
            // Initialize the pool type to blank for non-pooled accounts
            string dataContractPoolType = "";
            string dataContractUmbrellaReference = "";

            // Set the pool type to the "U"mbrella if the type is an umbrella
            if (poolType == GlBudgetPoolType.Umbrella)
            {
                dataContractPoolType = "U";
            }

            // Set the pool type to the "P"oolee if the type is a poolee
            if (poolType == GlBudgetPoolType.Poolee)
            {
                dataContractPoolType = "P";
            }

            foreach (var glAccount in glAccounts)
            {
                // Set the umbrella reference for an umbrella GL account
                if (poolType == GlBudgetPoolType.Umbrella)
                {
                    dataContractUmbrellaReference = glAccount;
                }

                // Set the umbrella reference for a poolee GL account
                if (poolType == GlBudgetPoolType.Poolee)
                {
                    dataContractUmbrellaReference = umbrellaReference;
                }

                var dataContract = PopulateSingleGlAcctsDataContract(glAccount, dataContractUmbrellaReference, dataContractPoolType, umbrellaHasDirectExpenses, amountsAreZero, amountsAreNull);
                this.glAcctsDataContracts.Add(dataContract);
            }

            glAccts = glAcctsDataContracts.ToList();
            return glAccts.ToList();
        }


        private GlAccts PopulateSingleGlAcctsDataContract(string glAccount, string dataContractUmbrellaReference, string dataContractPoolType,
            bool umbrellaHasDirectExpenses, bool amountsAreZero, bool amountsAreNull)
        {
            // Default the amounts for the GL.ACCTS data contracts
            decimal? glBudgetPosted = 1m,
                glBudgetMemos = 2m,
                glEncumbrancePosted = 3m,
                glEncumbranceMemos = 4m,
                glRequisitionMemos = 5m,
                glActualPosted = 6m,
                glActualMemos = 7m,
                faBudgetMemos = 8m,
                faBudgetPosted = 9m,
                faEncumbranceMemos = 10m,
                faEncumbrancePosted = 11m,
                faRequisitionMemos = 12m,
                faActualPosted = 13m,
                faActualMemos = 14m;

            // Zero out the direct expenses fields for the umbrella if the parameter says so
            if (!umbrellaHasDirectExpenses)
            {
                glEncumbrancePosted = 0m;
                glEncumbranceMemos = 0m;
                glRequisitionMemos = 0m;
                glActualPosted = 0m;
                glActualMemos = 0m;
            }

            // Zero out the numbers if the parameter says so...
            if (amountsAreZero)
            {
                glBudgetPosted = 0m;
                glBudgetMemos = 0m;
                glEncumbrancePosted = 0m;
                glEncumbranceMemos = 0m;
                glRequisitionMemos = 0m;
                glActualPosted = 0m;
                glActualMemos = 0m;
                faBudgetMemos = 0m;
                faBudgetPosted = 0m;
                faEncumbranceMemos = 0m;
                faEncumbrancePosted = 0m;
                faRequisitionMemos = 0m;
                faActualPosted = 0m;
                faActualMemos = 0m;
            }

            // Null out the numbers if the parameter says so...
            if (amountsAreNull)
            {
                glBudgetPosted = null;
                glBudgetMemos = null;
                glEncumbrancePosted = null;
                glEncumbranceMemos = null;
                glRequisitionMemos = null;
                glActualPosted = null;
                glActualMemos = null;
                faBudgetMemos = null;
                faBudgetPosted = null;
                faEncumbranceMemos = null;
                faEncumbrancePosted = null;
                faRequisitionMemos = null;
                faActualPosted = null;
                faActualMemos = null;
            }

            // Save the posted amounts in the GlAccts data contract so we have consistent data.
            var dataContract = new GlAccts()
            {
                Recordkey = glAccount,
                AvailFundsController = new List<string>()
                {
                    testGlConfigurationRepository.StartYear.ToString(),
                    (testGlConfigurationRepository.StartYear - 1).ToString(),
                    (testGlConfigurationRepository.StartYear - 2).ToString()
                },
                GlBudgetPosted = new List<decimal?>() { glBudgetPosted, glBudgetPosted, glBudgetPosted },
                GlBudgetMemos = new List<decimal?>() { glBudgetMemos, glBudgetMemos, glBudgetMemos },
                GlEncumbrancePosted = new List<decimal?>() { glEncumbrancePosted, glEncumbrancePosted, glEncumbrancePosted },
                GlEncumbranceMemos = new List<decimal?>() { glEncumbranceMemos, glEncumbranceMemos, glEncumbranceMemos },
                GlRequisitionMemos = new List<decimal?>() { glRequisitionMemos, glRequisitionMemos, glRequisitionMemos },
                GlActualMemos = new List<decimal?>() { glActualMemos, glActualMemos, glActualMemos },
                GlActualPosted = new List<decimal?>() { glActualPosted, glActualPosted, glActualPosted },
                FaBudgetMemo = new List<decimal?>() { faBudgetMemos, faBudgetMemos, faBudgetMemos },
                FaBudgetPosted = new List<decimal?>() { faBudgetPosted, faBudgetPosted, faBudgetPosted },
                FaEncumbranceMemo = new List<decimal?>() { faEncumbranceMemos, faEncumbranceMemos, faEncumbranceMemos },
                FaEncumbrancePosted = new List<decimal?>() { faEncumbrancePosted, faEncumbrancePosted, faEncumbrancePosted },
                FaRequisitionMemo = new List<decimal?>() { faRequisitionMemos, faRequisitionMemos, faRequisitionMemos },
                FaActualPosted = new List<decimal?>() { faActualPosted, faActualPosted, faActualPosted },
                FaActualMemo = new List<decimal?>() { faActualMemos, faActualMemos, faActualMemos },
                GlPooledType = new List<string>() { dataContractPoolType, dataContractPoolType, dataContractPoolType },
                GlBudgetLinkage = new List<string>() { dataContractUmbrellaReference, dataContractUmbrellaReference, dataContractUmbrellaReference },
            };
            dataContract.buildAssociations();

            return dataContract;
        }

        private void InitializeMockStatements()
        {
            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlAccts>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.glAcctsDataContracts);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<GlAccts>(It.IsAny<string>(), true)).Returns(
                (string glNumber, bool flag) =>
                {
                    var dataContract = this.glAcctsNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == glNumber);

                    return Task.FromResult(dataContract);
                });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FcDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.FcDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FdDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.FdDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<LoDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.LoDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<ObDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.ObDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<SoDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.SoDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<UnDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.UnDescs);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<GenLdgr>(It.IsAny<string>(), true)).Returns(
                (string key, bool flag) =>
                {
                    GenLdgr dataContract = testGlConfigurationRepository.GenLdgrDataContracts
                        .FirstOrDefault(x => x != null && x.Recordkey == key);

                    return Task.FromResult(dataContract);
                });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlsFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glsFyrDataContracts);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<GlsFyr>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(
                (string fileName, string glNumber, bool flag) =>
                {
                    var dataContract = this.glsFyrNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == glNumber);

                    return Task.FromResult(dataContract);
                });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlpFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glpFyrDataContracts);
            });

            // Mock the ReadRecord method to return an ApplValcodes data contract.
            dataReaderMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(GlSourceCodes);
            });

            var glpFyrFilename = "GLP." + this.param4_FiscalYear;
            dataReaderMock.Setup(dr => dr.SelectAsync(glpFyrFilename, It.IsAny<string>()))
                .Returns(Task.FromResult(testCostCenterRepository.GlaFyrDataContracts.Select(x => x.Recordkey).ToArray()));

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns(Task.FromResult(testCostCenterRepository.GlaFyrDataContracts));

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<EncFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(encFyrDataContracts);
            });

            transManagerMock.Setup(tio => tio.Execute<GetPurchasingDocumentIdsRequest, GetPurchasingDocumentIdsResponse>(It.IsAny<GetPurchasingDocumentIdsRequest>())).Returns(
                (GetPurchasingDocumentIdsRequest request) =>
                {
                    // Only return the pre-populated response object if the data matches. Otherwise, return null.
                    if (request.PoNumbers != null && request.BpoNumbers != null && request.ReqNumbers != null
                        && !request.ReqNumbers.Except(purchasingDocumentIdsRequest.ReqNumbers).Any()
                        && !purchasingDocumentIdsRequest.ReqNumbers.Except(request.ReqNumbers).Any()
                        && !request.PoNumbers.Except(purchasingDocumentIdsRequest.PoNumbers).Any()
                        && !purchasingDocumentIdsRequest.PoNumbers.Except(request.PoNumbers).Any()
                        && !request.BpoNumbers.Except(purchasingDocumentIdsRequest.BpoNumbers).Any()
                        && !purchasingDocumentIdsRequest.BpoNumbers.Except(request.BpoNumbers).Any())
                    {
                        return purchasingDocumentIdsResponse;
                    }

                    return null;
                });

            transManagerMock.Setup(tio => tio.ExecuteAsync<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(It.IsAny<GetGlAccountDescriptionRequest>())).Returns(() =>
            {
                return Task.FromResult(glAccountsDescriptionResponse);
            });

            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(glaIds);
            });

            var glsFyrFilename = "GLS." + this.param4_FiscalYear;
            dataReaderMock.Setup(dr => dr.SelectAsync(glsFyrFilename, It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(glsIds);
            });

            var encFyrFilename = "ENC." + this.param4_FiscalYear;
            dataReaderMock.Setup(dr => dr.SelectAsync(encFyrFilename, It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(encIds);
            });

            var glAcctsFilename = "GL.ACCTS";
            dataReaderMock.Setup(dr => dr.SelectAsync(glAcctsFilename, It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(glAcctsIds);
            });

            CfwebDefaults cfwebDefaults = new CfwebDefaults();
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS", true)).Returns(() =>
            {
                return Task.FromResult(cfwebDefaults);
            });
        }

        #endregion
    }
}
