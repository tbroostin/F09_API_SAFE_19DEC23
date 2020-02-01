// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    /// <summary>
    /// Test the methods in the Cost Center repository.
    /// </summary>
    [TestClass]
    public class CostCenterRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private CostCenterRepository actualRepository = null;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private TestCostCenterRepository testCostCenterRepository = null;
        private TestGeneralLedgerUserRepository testGlUserRepository = null;
        private CostCenterBuilder CostCenterBuilderObject = null;
        private List<GeneralLedgerComponentDescription> testRepositoryDescriptions = null;
        private ApplValcodes GlSourceCodes = null;
        private GetPurchasingDocumentIdsRequest purchasingDocumentIdsRequest;
        private GetPurchasingDocumentIdsResponse purchasingDocumentIdsResponse;
        private TestGlAccountRepository testGlNumberRepository;
        private List<GeneralLedgerComponentDescription> glComponentsForCostCenter;
        private GetGlAccountDescriptionResponse glAccountsDescriptionResponse;

        private GeneralLedgerUser param1_GlUser;
        private CostCenterStructure param2_costCenterStructure;
        private GeneralLedgerClassConfiguration param6_glClassConfiguration;
        private string param3_SelectedCostCenterId;
        private int param4_FiscalYear;
        private CostCenterQueryCriteria param5_costCenterQueryCriteria;
        private List<CostCenterComponentQueryCriteria> costCenterComponentQueryCriteria;
        private List<CostCenterComponentRangeQueryCriteria> costCenterComponentRangeQueryCriteria;

        private Collection<GlAccts> glAcctsDataContracts;
        private Collection<GlAccts> glAcctsNoAccessUmbrellaDataContracts;
        private Collection<GlsFyr> glsFyrDataContracts;
        private Collection<GlsFyr> glsFyrNoAccessUmbrellaDataContracts;
        private Collection<EncFyr> encFyrDataContracts;
        private Collection<GlpFyr> glpFyrDataContracts;
        private List<CostCenter> expectedCostCenters;
        private List<CostCenterHelper> costCenterHelpers;
        private List<CostCenterHelperClosedYear> costCenterHelpersGls;

        private string[] glsIds = new string[] { };
        private string[] encIds = new string[] { };
        private string[] glAcctsIds = new string[] { };

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            actualRepository = new CostCenterRepository(cacheProvider, transFactory, logger);

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testCostCenterRepository = new TestCostCenterRepository();
            testGlUserRepository = new TestGeneralLedgerUserRepository();
            CostCenterBuilderObject = new CostCenterBuilder();
            glAcctsDataContracts = new Collection<GlAccts>();
            glAcctsNoAccessUmbrellaDataContracts = new Collection<GlAccts>();
            glsFyrDataContracts = new Collection<GlsFyr>();
            glsFyrNoAccessUmbrellaDataContracts = new Collection<GlsFyr>();
            encFyrDataContracts = new Collection<EncFyr>();
            glpFyrDataContracts = new Collection<GlpFyr>();
            expectedCostCenters = new List<CostCenter>();
            costCenterHelpers = new List<CostCenterHelper>();
            costCenterHelpersGls = new List<CostCenterHelperClosedYear>();
            testGlNumberRepository = new TestGlAccountRepository();
            glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();
            glAccountsDescriptionResponse = new GetGlAccountDescriptionResponse();

            BuildGlSourceCodes();

            this.param1_GlUser = new GeneralLedgerUser("1", "Kleehammer");
            this.param2_costCenterStructure = new CostCenterStructure();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = 0;

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

            costCenterComponentQueryCriteria = new List<CostCenterComponentQueryCriteria>();
            costCenterComponentQueryCriteria.Add(costCenterCompQueryCriteria);

            this.param5_costCenterQueryCriteria = new CostCenterQueryCriteria(costCenterComponentQueryCriteria);

            var expenseValues = new List<String>() { "5", "7" };
            var revenueValues = new List<String>() { "4" };
            var assetValues = new List<string>() { "1" };
            var liabilityValues = new List<string>() { "2" };
            var fundBalanceValues = new List<string>() { "3" };
            this.param6_glClassConfiguration = new GeneralLedgerClassConfiguration("GL.Class", expenseValues, revenueValues, assetValues, liabilityValues, fundBalanceValues);
            this.param6_glClassConfiguration.GlClassStartPosition = 18;
            this.param6_glClassConfiguration.GlClassLength = 1;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

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
            CostCenterBuilderObject = null;
            GlSourceCodes = null;
            expectedCostCenters = null;
            costCenterHelpers = null;
            costCenterHelpersGls = null;
            glComponentsForCostCenter = null;
            testGlNumberRepository = null;

            this.param1_GlUser = null;
            this.param2_costCenterStructure = null;
            this.param6_glClassConfiguration = null;
            this.param3_SelectedCostCenterId = null;
            this.param4_FiscalYear = 0;
            this.param5_costCenterQueryCriteria = null;
            glsIds = new string[] { };
            encIds = new string[] { };
            glAcctsIds = new string[] { };
            this.testRepositoryDescriptions = null;
        }
        #endregion

        #region Revenue accounts
        /// <summary>
        /// USE THIS METHOD AS A TEMPLATE FOR FUTURE TESTS. 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetCostCentersAsync_OpenYear_OneCostCenter_NonPooledAccountsOnly_RevenueAccountsOnly_ErrorDeterminingGlClass()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_41001",  // Subtotal 41
                "10_00_01_01_EJK88_41001",  // Subtotal 41
                "10_00_01_02_EJK88_11001"   // Subtotal 11
            };
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddRevenueAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            // Populate the non-pooled accounts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Split up the non-pooled accounts based on the subtotal ID.
            var revenueAccounts = testGlNumberRepository.SetGlNumbers(glAccounts).WithGlSubclass("41").GetFilteredGlNumbers();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());

            var actualCostCenter = actualCostCenters.FirstOrDefault();
            var revenueContracts = this.glAcctsDataContracts
                .Where(x => revenueAccounts.Contains(x.Recordkey)).ToList();
            var revenueGlAcctsMemos = revenueContracts
                .SelectMany(x => x.MemosEntityAssociation
                .Where(y => y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString())).ToList();

            // The expense values should be zero since we have only revenue accounts
            Assert.AreEqual(0, actualCostCenter.TotalBudgetExpenses);
            Assert.AreEqual(0, actualCostCenter.TotalActualsExpenses);
            Assert.AreEqual(0, actualCostCenter.TotalEncumbrancesExpenses);

            Assert.AreEqual(Helper_CalculateBudgetForNonUmbrella(revenueGlAcctsMemos), actualCostCenter.TotalBudgetRevenue);
            Assert.AreEqual(Helper_CalculateActualsForNonUmbrella(revenueGlAcctsMemos), actualCostCenter.TotalActualsRevenue);
            Assert.AreEqual(Helper_CalculateEncumbrancesForNonUmbrella(revenueGlAcctsMemos), actualCostCenter.TotalEncumbrancesRevenue);

            // There should only be one revenue-type subtotal and its amounts should match the amounts on the two revenue accounts.
            Assert.AreEqual(1, actualCostCenter.CostCenterSubtotals.Where(x => x.GlClass == GlClass.Revenue).ToList().Count);
            var subtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.GlClass == GlClass.Revenue);
            Assert.AreEqual(Helper_CalculateBudgetForNonUmbrella(revenueGlAcctsMemos), subtotal.TotalBudget);
            Assert.AreEqual(Helper_CalculateActualsForNonUmbrella(revenueGlAcctsMemos), subtotal.TotalActuals);
            Assert.AreEqual(Helper_CalculateEncumbrancesForNonUmbrella(revenueGlAcctsMemos), subtotal.TotalEncumbrances);

            // The totals for each GL account on the subtotal should match the amounts on the data contracts.
            foreach (var glAccount in subtotal.GlAccounts)
            {
                var glAcctsMemos = revenueContracts
                    .Where(x => x.Recordkey == glAccount.GlAccountNumber)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString())).ToList();

                Assert.AreEqual(Helper_CalculateBudgetForNonUmbrella(glAcctsMemos), glAccount.BudgetAmount);
                Assert.AreEqual(Helper_CalculateActualsForNonUmbrella(glAcctsMemos), glAccount.ActualAmount);
                Assert.AreEqual(Helper_CalculateEncumbrancesForNonUmbrella(glAcctsMemos), glAccount.EncumbranceAmount);
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_OpenYear_NonPooledAccountsOnly_RevenueAccountsOnly()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_00_EJK88_41001",  // Subtotal 41
                "10_00_01_01_EJK88_41001",  // Subtotal 41
                "10_00_01_02_EJK88_42001"   // Subtotal 42
            };
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddRevenueAccounts(glAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            // Populate the non-pooled accounts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Split up the non-pooled accounts based on the subtotal ID.
            var subtotal41Accounts = testGlNumberRepository.SetGlNumbers(glAccounts).WithGlSubclass("41").GetFilteredGlNumbers();
            var subtotal42Accounts = testGlNumberRepository.SetGlNumbers(glAccounts).WithGlSubclass("42").GetFilteredGlNumbers();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());

            var actualCostCenter = actualCostCenters.FirstOrDefault();
            var revenue41Contracts = this.glAcctsDataContracts
                .Where(x => subtotal41Accounts.Contains(x.Recordkey)).ToList();
            var allGlAcctsMemos = this.glAcctsDataContracts
                .SelectMany(x => x.MemosEntityAssociation
                .Where(y => y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString())).ToList();
            var revenue41GlAcctsMemos = revenue41Contracts
                .SelectMany(x => x.MemosEntityAssociation
                .Where(y => y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString())).ToList();

            var revenue42Contracts = this.glAcctsDataContracts
                .Where(x => subtotal42Accounts.Contains(x.Recordkey)).ToList();
            var revenue42GlAcctsMemos = revenue42Contracts
                .SelectMany(x => x.MemosEntityAssociation
                .Where(y => y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString())).ToList();

            // The expense values should be zero since we have only revenue accounts
            Assert.AreEqual(0, actualCostCenter.TotalBudgetExpenses);
            Assert.AreEqual(0, actualCostCenter.TotalActualsExpenses);
            Assert.AreEqual(0, actualCostCenter.TotalEncumbrancesExpenses);

            Assert.AreEqual(Helper_CalculateBudgetForNonUmbrella(allGlAcctsMemos), actualCostCenter.TotalBudgetRevenue);
            Assert.AreEqual(Helper_CalculateActualsForNonUmbrella(allGlAcctsMemos), actualCostCenter.TotalActualsRevenue);
            Assert.AreEqual(Helper_CalculateEncumbrancesForNonUmbrella(allGlAcctsMemos), actualCostCenter.TotalEncumbrancesRevenue);

            // There should two revenue-type subtotal and the amounts should match the amounts on the two revenue accounts.
            Assert.AreEqual(2, actualCostCenter.CostCenterSubtotals.Where(x => x.GlClass == GlClass.Revenue).ToList().Count);

            foreach (var subtotal in actualCostCenter.CostCenterSubtotals)
            {
                if (subtotal.Id == "41")
                {
                    Assert.AreEqual(Helper_CalculateBudgetForNonUmbrella(revenue41GlAcctsMemos), subtotal.TotalBudget);
                    Assert.AreEqual(Helper_CalculateActualsForNonUmbrella(revenue41GlAcctsMemos), subtotal.TotalActuals);
                    Assert.AreEqual(Helper_CalculateEncumbrancesForNonUmbrella(revenue41GlAcctsMemos), subtotal.TotalEncumbrances);
                }

                if (subtotal.Id == "42")
                {
                    Assert.AreEqual(Helper_CalculateBudgetForNonUmbrella(revenue42GlAcctsMemos), subtotal.TotalBudget);
                    Assert.AreEqual(Helper_CalculateActualsForNonUmbrella(revenue42GlAcctsMemos), subtotal.TotalActuals);
                    Assert.AreEqual(Helper_CalculateEncumbrancesForNonUmbrella(revenue42GlAcctsMemos), subtotal.TotalEncumbrances);
                }

                // The totals for each GL account on the subtotal should match the amounts on the data contracts.
                foreach (var glAccount in subtotal.GlAccounts)
                {
                    var glAcctsMemos = this.glAcctsDataContracts
                        .Where(x => x.Recordkey == glAccount.GlAccountNumber)
                        .SelectMany(x => x.MemosEntityAssociation
                        .Where(y => y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString())).ToList();

                    Assert.AreEqual(Helper_CalculateBudgetForNonUmbrella(glAcctsMemos), glAccount.BudgetAmount);
                    Assert.AreEqual(Helper_CalculateActualsForNonUmbrella(glAcctsMemos), glAccount.ActualAmount);
                    Assert.AreEqual(Helper_CalculateEncumbrancesForNonUmbrella(glAcctsMemos), glAccount.EncumbranceAmount);
                }
            }
        }
        #endregion

        #region General Error-Type Scenarios
        [TestMethod]
        public async Task GetCostCentersAsync_NullGeneralLedgerUser()
        {
            this.param1_GlUser = null;
            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NullAccountStructure()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param2_costCenterStructure = null;
            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NullGlClassConfiguration()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param6_glClassConfiguration = null;
            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NullExpenseAccounts()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NoExpenseAccounts()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GenLdgrRecordIsNull()
        {
            // Get the GL User expense accounts from the test repository.
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1] = null;

            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GenLdgrRecordStatusIsNull()
        {
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1].GenLdgrStatus = null;

            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GenLdgrRecordStatusIsEmpty()
        {
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1].GenLdgrStatus = "";

            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_CostCenterDescriptionUsesAllComponents()
        //{
        //    testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, true);
        //    //await Helper_InitializeParameters();

        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithFunction("00").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion
        //    Helper_InitializeGlAcctsDataContracts();
        //    //#region Set up the expected data
        //    //var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
        //    //PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);

        //    //// Populate the poolee accounts for each umbrella
        //    //foreach (var seedUmbrella in seedUmbrellaAccounts)
        //    //{
        //    //    // Get the umbrella ID
        //    //    var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlNumberRepository.FUNCTION_CODE);
        //    //    var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

        //    //    // Get the poolees for this umbrella
        //    //    var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).GetFilteredGlNumbers();
        //    //    PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
        //    //}

        //    //// Populate the non-pooled accounts
        //    //var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
        //    //PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        //    //#endregion

        //    //BuildExpectedCostCentersForOpenYear();
        //    BuildCostCenterHelpersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

        //    // Make sure the number of cost centers are the same
        //    Assert.AreEqual(this.costCenterHelpers.Count, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in this.expectedCostCenters)
        //    {
        //        var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);
        //        Assert.IsNotNull(actualCostCenter);
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);

        //        // Confirm that the subtotal properties match.
        //        foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
        //        {
        //            var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);
        //            Assert.IsNotNull(actualSubtotal);
        //            Assert.IsNull(actualSubtotal.Name);

        //            // Confirm that the GL account data matches.
        //            foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
        //            {
        //                var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);
        //                Assert.IsNotNull(actualGlAccount);
        //                Assert.IsNull(actualGlAccount.GlAccountDescription);
        //            }

        //            // Make sure the pooled account properties match
        //            foreach (var expectedPool in expectedSubtotal.Pools)
        //            {
        //                var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);
        //                Assert.IsNotNull(actualPool);

        //                // Check the umbrella properties
        //                Assert.IsNull(actualPool.Umbrella.GlAccountDescription);

        //                // Make sure the umbrella also exists as a poolee.
        //                var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
        //                Assert.IsNotNull(umbrellaPoolee);

        //                // Check the poolee properties
        //                foreach (var expectedPoolee in expectedPool.Poolees)
        //                {
        //                    var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

        //                    Assert.IsNotNull(actualPoolee);
        //                    Assert.IsNull(actualPoolee.GlAccountDescription);
        //                }
        //            }
        //        }
        //    }
        //}

        //[TestMethod]
        //public async Task GetCostCentersAsync_GlComponentNotFound()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithUnit("AJK55").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

        //    testGlConfigurationRepository.FdDescs.RemoveAt(0);
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    // Populate the GlAccts data contracts for the BulkRead call using the GL User data populated above
        //    PopulateGlAccountDataContracts(this.param1_GlUser.ExpenseAccounts.ToList(), GlBudgetPoolType.None, "");
        //    BuildExpectedCostCentersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();
        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in expectedCostCenters)
        //    {
        //        var selectedCostCenters = actualCostCenters.Where(x => x.Id == expectedCostCenter.Id).ToList();

        //        Assert.AreEqual(1, selectedCostCenters.Count);

        //        var actualCostCenter = selectedCostCenters.First();
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //    }
        //}

        [TestMethod]
        public async Task GetCostCentersAsync_NoCostCenterDescription()
        {
            // Remove all of the component description records
            while (testGlConfigurationRepository.FdDescs.Any()) { testGlConfigurationRepository.FdDescs.RemoveAt(0); }
            while (testGlConfigurationRepository.SoDescs.Any()) { testGlConfigurationRepository.SoDescs.RemoveAt(0); }
            while (testGlConfigurationRepository.LoDescs.Any()) { testGlConfigurationRepository.LoDescs.RemoveAt(0); }
            while (testGlConfigurationRepository.FcDescs.Any()) { testGlConfigurationRepository.FcDescs.RemoveAt(0); }
            while (testGlConfigurationRepository.UnDescs.Any()) { testGlConfigurationRepository.UnDescs.RemoveAt(0); }
            while (testGlConfigurationRepository.ObDescs.Any()) { testGlConfigurationRepository.ObDescs.RemoveAt(0); }

            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithUnit("AJK55").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            // Populate the GlAccts data contracts for the BulkRead call using the GL User data populated above
            PopulateGlAccountDataContracts(this.param1_GlUser.ExpenseAccounts.ToList(), GlBudgetPoolType.None, "");

            var actualCostCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());
            Assert.AreEqual("No cost center description available.", actualCostCenters.First().Name);
        }
        #endregion

        #region Multiple Cost Centers - Open Year Scenarios
        [TestMethod]
        public async Task GetCostCentersAsync_OpenYear_OneYearInFuture()
        {
            // Initialize the test GL configuration repository to pretend we're one year in the future
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, 1));

            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts();
            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpers)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var costCenterUmbrellaAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                var CostCenterNonPooledAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledAmounts.Select(y => y.FiscalYearAmounts)).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                var expectedBudget = Helper_CalculateBudgetForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(CostCenterNonPooledAmounts);
                var expectedActuals = Helper_CalculateActualsForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(CostCenterNonPooledAmounts);
                var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(CostCenterNonPooledAmounts);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    var subtotalUmbrellaAmounts = expectedSubtotal.BudgetPools.Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                    var subtotalNonPooledAmounts = expectedSubtotal.NonPooledAmounts.Select(y => y.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                    expectedBudget = Helper_CalculateBudgetForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(subtotalNonPooledAmounts);
                    expectedActuals = Helper_CalculateActualsForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(subtotalNonPooledAmounts);
                    expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(subtotalNonPooledAmounts);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);

                        expectedBudget = 0m;
                        expectedActuals = 0m;
                        expectedEncumbrances = 0m;
                        expectedBudget = expectedPool.Umbrella.FiscalYearAmounts.FaBudgetMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaBudgetPostedAssocMember.Value;
                        expectedActuals = expectedPool.Umbrella.FiscalYearAmounts.FaActualMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaActualPostedAssocMember.Value;
                        expectedEncumbrances = expectedPool.Umbrella.FiscalYearAmounts.FaEncumbranceMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaEncumbrancePostedAssocMember.Value
                            + expectedPool.Umbrella.FiscalYearAmounts.FaRequisitionMemoAssocMember.Value;

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);

                        // Make sure the umbrella exists as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);
                        Assert.IsNotNull(actualUmbrellaPoolee);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_MultipleCostCenters_OpenYear_WithPools_UmbrellasHaveDirectExpenses()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts();
            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpers)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var costCenterUmbrellaAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                var costCenterNonPooledAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledAmounts.Select(y => y.FiscalYearAmounts)).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                var expectedBudget = Helper_CalculateBudgetForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(costCenterNonPooledAmounts);
                var expectedActuals = Helper_CalculateActualsForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(costCenterNonPooledAmounts);
                var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(costCenterNonPooledAmounts);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    var subtotalUmbrellaAmounts = expectedSubtotal.BudgetPools.Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                    var subtotalNonPooledAmounts = expectedSubtotal.NonPooledAmounts.Select(y => y.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                    expectedBudget = Helper_CalculateBudgetForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(subtotalNonPooledAmounts);
                    expectedActuals = Helper_CalculateActualsForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(subtotalNonPooledAmounts);
                    expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(subtotalNonPooledAmounts);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Check the non-pooled accounts
                    foreach (var expectedNonPooledAmount in expectedSubtotal.NonPooledAmounts)
                    {
                        var actualNonPooledAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedNonPooledAmount.GlNumber);
                        expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });
                        expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });
                        expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });

                        Assert.AreEqual(expectedBudget, actualNonPooledAccount.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualNonPooledAccount.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualNonPooledAccount.EncumbranceAmount);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = 0m;
                        expectedActuals = 0m;
                        expectedEncumbrances = 0m;
                        expectedBudget = Helper_CalculateBudgetForUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });
                        expectedActuals = Helper_CalculateActualsForUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });
                        expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella exists as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(actualUmbrellaPoolee);

                        var umbrellaPooleeActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });
                        var umbrellaPooleeEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });
                        Assert.AreEqual(0m, actualUmbrellaPoolee.BudgetAmount);
                        Assert.AreEqual(umbrellaPooleeActuals, actualUmbrellaPoolee.ActualAmount);
                        Assert.AreEqual(umbrellaPooleeEncumbrances, actualUmbrellaPoolee.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Poolee, actualUmbrellaPoolee.PoolType);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlNumber);

                            expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
                            expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
                            expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                            Assert.IsNull(actualPoolee.GlAccountDescription);
                            Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_MultipleCostCenters_OpenYear_WithPools_UmbrellasDoNotHaveDirectExpenses()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts(false);
            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpers)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var costCenterUmbrellaAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                var costCenterNonPooledAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledAmounts.Select(y => y.FiscalYearAmounts)).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                var expectedBudget = Helper_CalculateBudgetForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(costCenterNonPooledAmounts);
                var expectedActuals = Helper_CalculateActualsForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(costCenterNonPooledAmounts);
                var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(costCenterNonPooledAmounts);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    var subtotalUmbrellaAmounts = expectedSubtotal.BudgetPools.Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                    var subtotalNonPooledAmounts = expectedSubtotal.NonPooledAmounts.Select(y => y.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                    expectedBudget = Helper_CalculateBudgetForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(subtotalNonPooledAmounts);
                    expectedActuals = Helper_CalculateActualsForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(subtotalNonPooledAmounts);
                    expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(subtotalNonPooledAmounts);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = 0m;
                        expectedActuals = 0m;
                        expectedEncumbrances = 0m;
                        expectedBudget = expectedPool.Umbrella.FiscalYearAmounts.FaBudgetMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaBudgetPostedAssocMember.Value;
                        expectedActuals = expectedPool.Umbrella.FiscalYearAmounts.FaActualMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaActualPostedAssocMember.Value;
                        expectedEncumbrances = expectedPool.Umbrella.FiscalYearAmounts.FaEncumbranceMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaEncumbrancePostedAssocMember.Value
                            + expectedPool.Umbrella.FiscalYearAmounts.FaRequisitionMemoAssocMember.Value;

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella does NOT exist as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNull(actualUmbrellaPoolee);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_MultipleCostCenters_OpenYear_WithPools_UmbrellasDoNotHaveDirectExpenses_FilterCriteria()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts(false);
            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetFilteredCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpers)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var costCenterUmbrellaAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                var costCenterNonPooledAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledAmounts.Select(y => y.FiscalYearAmounts)).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                var expectedBudget = Helper_CalculateBudgetForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(costCenterNonPooledAmounts);
                var expectedActuals = Helper_CalculateActualsForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(costCenterNonPooledAmounts);
                var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(costCenterNonPooledAmounts);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    var subtotalUmbrellaAmounts = expectedSubtotal.BudgetPools.Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                    var subtotalNonPooledAmounts = expectedSubtotal.NonPooledAmounts.Select(y => y.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                    expectedBudget = Helper_CalculateBudgetForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(subtotalNonPooledAmounts);
                    expectedActuals = Helper_CalculateActualsForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(subtotalNonPooledAmounts);
                    expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(subtotalNonPooledAmounts);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = 0m;
                        expectedActuals = 0m;
                        expectedEncumbrances = 0m;
                        expectedBudget = expectedPool.Umbrella.FiscalYearAmounts.FaBudgetMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaBudgetPostedAssocMember.Value;
                        expectedActuals = expectedPool.Umbrella.FiscalYearAmounts.FaActualMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaActualPostedAssocMember.Value;
                        expectedEncumbrances = expectedPool.Umbrella.FiscalYearAmounts.FaEncumbranceMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaEncumbrancePostedAssocMember.Value
                            + expectedPool.Umbrella.FiscalYearAmounts.FaRequisitionMemoAssocMember.Value;

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella does NOT exist as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNull(actualUmbrellaPoolee);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_MultipleCostCenters_OpenYear_WithPools_NoPoolees_UmbrellasHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            #endregion

            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpers)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var costCenterUmbrellaAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                var costCenterNonPooledAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledAmounts.Select(y => y.FiscalYearAmounts)).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                var expectedBudget = Helper_CalculateBudgetForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(costCenterNonPooledAmounts);
                var expectedActuals = Helper_CalculateActualsForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(costCenterNonPooledAmounts);
                var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(costCenterNonPooledAmounts);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    var subtotalUmbrellaAmounts = expectedSubtotal.BudgetPools.Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                    var subtotalNonPooledAmounts = expectedSubtotal.NonPooledAmounts.Select(y => y.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                    expectedBudget = Helper_CalculateBudgetForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(subtotalNonPooledAmounts);
                    expectedActuals = Helper_CalculateActualsForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(subtotalNonPooledAmounts);
                    expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(subtotalNonPooledAmounts);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = 0m;
                        expectedActuals = 0m;
                        expectedEncumbrances = 0m;
                        expectedBudget = expectedPool.Umbrella.FiscalYearAmounts.FaBudgetMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaBudgetPostedAssocMember.Value;
                        expectedActuals = expectedPool.Umbrella.FiscalYearAmounts.FaActualMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaActualPostedAssocMember.Value;
                        expectedEncumbrances = expectedPool.Umbrella.FiscalYearAmounts.FaEncumbranceMemoAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.FaEncumbrancePostedAssocMember.Value
                            + expectedPool.Umbrella.FiscalYearAmounts.FaRequisitionMemoAssocMember.Value;

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella exists as a poolee and that it's the only poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(actualUmbrellaPoolee);

                        var umbrellaPooleeActuals = expectedPool.Umbrella.FiscalYearAmounts.GlActualMemosAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.GlActualPostedAssocMember.Value;
                        var umbrellaPooleeEncumbrances = expectedPool.Umbrella.FiscalYearAmounts.GlEncumbranceMemosAssocMember.Value + expectedPool.Umbrella.FiscalYearAmounts.GlEncumbrancePostedAssocMember.Value
                            + expectedPool.Umbrella.FiscalYearAmounts.GlRequisitionMemosAssocMember.Value;
                        Assert.AreEqual(0m, actualUmbrellaPoolee.BudgetAmount);
                        Assert.AreEqual(umbrellaPooleeActuals, actualUmbrellaPoolee.ActualAmount);
                        Assert.AreEqual(umbrellaPooleeEncumbrances, actualUmbrellaPoolee.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Poolee, actualUmbrellaPoolee.PoolType);
                        Assert.AreEqual(1, actualPool.Poolees.Count);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_MultipleCostCenters_OpenYear_WithPools_NoPoolees_UmbrellasDoNotHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null, false);

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            #endregion

            BuildCostCenterHelpersForOpenYear();
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpers)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var costCenterUmbrellaAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                var costCenterNonPooledAmounts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledAmounts.Select(y => y.FiscalYearAmounts)).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                var expectedBudget = Helper_CalculateBudgetForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(costCenterNonPooledAmounts);
                var expectedActuals = Helper_CalculateActualsForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(costCenterNonPooledAmounts);
                var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(costCenterUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(costCenterNonPooledAmounts);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    var subtotalUmbrellaAmounts = expectedSubtotal.BudgetPools.Where(x => x.Umbrella != null).Select(y => y.Umbrella.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U").ToList();
                    var subtotalNonPooledAmounts = expectedSubtotal.NonPooledAmounts.Select(y => y.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "").ToList();

                    expectedBudget = Helper_CalculateBudgetForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateBudgetForNonUmbrella(subtotalNonPooledAmounts);
                    expectedActuals = Helper_CalculateActualsForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateActualsForNonUmbrella(subtotalNonPooledAmounts);
                    expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(subtotalUmbrellaAmounts) + Helper_CalculateEncumbrancesForNonUmbrella(subtotalNonPooledAmounts);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = 0m;
                        expectedActuals = 0m;
                        expectedEncumbrances = 0m;
                        expectedBudget = Helper_CalculateBudgetForUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });
                        expectedActuals = Helper_CalculateActualsForUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });
                        expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(new List<GlAcctsMemos>() { expectedPool.Umbrella.FiscalYearAmounts });

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure there are zero poolees
                        Assert.AreEqual(0, actualPool.Poolees.Count);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_OpenYear_WithPools_NoAccessToUmbrellas_HasPoolees_UmbrellasHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("P1").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnitSubclass("AJK").GetFilteredGlNumbers().FirstOrDefault();
            var dataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", true, false, false);
            glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);


            // Populate the poolee accounts for each umbrella
            var pooleesForUmbrella1 = testGlNumberRepository.WithLocation("P1").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(pooleesForUmbrella1, GlBudgetPoolType.Poolee, seedUmbrellaAccount);
            #endregion

            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.AreEqual(1, actualCostCenters.Count());

            var expectedCostCenter = costCenterHelpers.First();
            var actualCostCenter = actualCostCenters.First();

            // Use the amounts from the "no access" umbrella
            var amounts = this.glAcctsNoAccessUmbrellaDataContracts[0].MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());
            var expectedBudget = Helper_CalculateBudgetForUmbrella(new List<GlAcctsMemos> { amounts });
            var expectedActuals = Helper_CalculateActualsForUmbrella(new List<GlAcctsMemos> { amounts });
            var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(new List<GlAcctsMemos> { amounts });

            Assert.IsNotNull(actualCostCenter);
            Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
            Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
            Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
            Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
            Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

            // Confirm that the subtotal properties match.
            foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
            {
                var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);

                Assert.IsNotNull(actualSubtotal);
                Assert.IsNull(actualSubtotal.Name);
                Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                // There should only be one budget pool
                Assert.AreEqual(expectedSubtotal.BudgetPools.Count, actualSubtotal.Pools.Count);
                Assert.AreEqual(1, actualSubtotal.Pools.Count);

                var expectedPool = expectedSubtotal.BudgetPools.First();
                var actualPool = actualSubtotal.Pools.First();
                Assert.IsTrue(actualPool.IsUmbrellaVisible);

                // Check the umbrella properties
                Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                // Make sure the umbrella does NOT exist as a poolee
                var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                Assert.IsNull(actualUmbrellaPoolee);

                // Check the poolee properties
                Assert.AreEqual(1, actualPool.Poolees.Count);
                var expectedPoolee = expectedPool.Poolees.First();
                var actualPoolee = actualPool.Poolees.First();

                expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
                expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
                expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });

                Assert.IsNotNull(actualPoolee);
                Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_OpenYear_WithPools_NoAccessToUmbrellas_HasPoolees_UmbrellasDoNotHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("P1").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnitSubclass("AJK").GetFilteredGlNumbers().FirstOrDefault();
            var dataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", false, false, false);
            glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);


            // Populate the poolee accounts for each umbrella
            var pooleesForUmbrella1 = testGlNumberRepository.WithLocation("P1").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(pooleesForUmbrella1, GlBudgetPoolType.Poolee, seedUmbrellaAccount);
            #endregion

            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.AreEqual(1, actualCostCenters.Count());

            var expectedCostCenter = costCenterHelpers.First();
            var actualCostCenter = actualCostCenters.First();

            // Use the amounts from the "no access" umbrella
            var amounts = this.glAcctsNoAccessUmbrellaDataContracts[0].MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());
            var expectedBudget = Helper_CalculateBudgetForUmbrella(new List<GlAcctsMemos> { amounts });
            var expectedActuals = Helper_CalculateActualsForUmbrella(new List<GlAcctsMemos> { amounts });
            var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(new List<GlAcctsMemos> { amounts });

            Assert.IsNotNull(actualCostCenter);
            Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
            Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
            Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
            Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
            Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

            // Confirm that the subtotal properties match.
            foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
            {
                var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);

                Assert.IsNotNull(actualSubtotal);
                Assert.IsNull(actualSubtotal.Name);
                Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                // There should only be one budget pool
                Assert.AreEqual(expectedSubtotal.BudgetPools.Count, actualSubtotal.Pools.Count);
                Assert.AreEqual(1, actualSubtotal.Pools.Count);

                var expectedPool = expectedSubtotal.BudgetPools.First();
                var actualPool = actualSubtotal.Pools.First();
                Assert.IsTrue(actualPool.IsUmbrellaVisible);

                // Check the umbrella properties
                Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                // Make sure the umbrella does NOT exist as a poolee
                var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                Assert.IsNull(actualUmbrellaPoolee);

                // Check the poolee properties
                Assert.AreEqual(1, actualPool.Poolees.Count);
                var expectedPoolee = expectedPool.Poolees.First();
                var actualPoolee = actualPool.Poolees.First();

                expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
                expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
                expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });

                Assert.IsNotNull(actualPoolee);
                Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
            }
        }

        //[TestMethod]
        //public async Task GetCostCentersAsync_MultipleCostCenters_OpenYear_WithPools_NoAccessToUmbrellas_NoPoolees_UmbrellasHaveDirectExpenses()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithFunction("00").WithUnit("AJK55").GetFilteredGlNumbers();
        //    this.param1_GlUser.AddExpenseAccounts(seedNonPooledAccounts);
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
        //     
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
        //    foreach (var seedUmbrella in seedUmbrellaAccounts)
        //    {
        //        var dataContract = PopulateSingleGlAcctsDataContract(seedUmbrella, seedUmbrella, "U", false, false, false);
        //        glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);
        //    }

        //    // Populate the non-pooled accounts
        //    PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        //    #endregion

        //    BuildCostCenterHelpersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    // Make sure the number of cost centers are the same
        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    var expectedCostCenter = costCenterHelpers.First();
        //    var actualCostCenter = actualCostCenters.First();

        //    // Use the amounts from the "no access" umbrella
        //    var amounts = this.glAcctsNoAccessUmbrellaDataContracts[0].MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());
        //    var expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos> { amounts });
        //    var expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos> { amounts });
        //    var expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos> { amounts });

        //    Assert.IsNotNull(actualCostCenter);
        //    Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //    Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
        //    Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudget);
        //    Assert.AreEqual(expectedActuals, actualCostCenter.TotalActuals);
        //    Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrances);

        //    // Confirm that the subtotal properties match.
        //    foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
        //    {
        //        var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);

        //        Assert.IsNotNull(actualSubtotal);
        //        Assert.IsNull(actualSubtotal.Name);
        //        Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
        //        Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
        //        Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

        //        // There should zero budget pools
        //        Assert.AreEqual(0, actualSubtotal.Pools.Count);

        //        foreach (var expectedNonPooledAmount in expectedSubtotal.NonPooledAmounts)
        //        {
        //            var actualNonPooledAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedNonPooledAmount.GlNumber);
        //            expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });
        //            expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });
        //            expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });

        //            Assert.AreEqual(expectedBudget, actualNonPooledAccount.BudgetAmount);
        //            Assert.AreEqual(expectedActuals, actualNonPooledAccount.ActualAmount);
        //            Assert.AreEqual(expectedEncumbrances, actualNonPooledAccount.EncumbranceAmount);
        //        }
        //    }
        //}

        [TestMethod]
        public async Task GetCostCentersAsync_OpenYear_WithPools_NoAccessToUmbrellas_NoPoolees_UmbrellasDoNotHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithFunction("00").WithUnit("AJK55").GetFilteredGlNumbers();
            this.param1_GlUser.AddExpenseAccounts(seedNonPooledAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            // Populate the non-pooled accounts
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            #endregion

            BuildCostCenterHelpersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
            Assert.AreEqual(1, actualCostCenters.Count());

            var expectedCostCenter = costCenterHelpers.First();
            var actualCostCenter = actualCostCenters.First();

            // Use the amounts from the "no access" umbrella
            var amounts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledAmounts).Select(x => x.FiscalYearAmounts).ToList();
            var expectedBudget = Helper_CalculateBudgetForNonUmbrella(amounts);
            var expectedActuals = Helper_CalculateActualsForNonUmbrella(amounts);
            var expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(amounts);

            Assert.IsNotNull(actualCostCenter);
            Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
            Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
            Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
            Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
            Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

            // Confirm that the subtotal properties match.
            foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
            {
                var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);

                Assert.IsNotNull(actualSubtotal);
                Assert.IsNull(actualSubtotal.Name);
                Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                // There should zero budget pools
                Assert.AreEqual(0, actualSubtotal.Pools.Count);

                foreach (var expectedNonPooledAmount in expectedSubtotal.NonPooledAmounts)
                {
                    var actualNonPooledAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedNonPooledAmount.GlNumber);
                    expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });
                    expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });
                    expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedNonPooledAmount.FiscalYearAmounts });

                    Assert.AreEqual(expectedBudget, actualNonPooledAccount.BudgetAmount);
                    Assert.AreEqual(expectedActuals, actualNonPooledAccount.ActualAmount);
                    Assert.AreEqual(expectedEncumbrances, actualNonPooledAccount.EncumbranceAmount);
                }
            }
        }
        #endregion

        #region Multiple Cost Centers - Closed Year Scenarios

        [TestMethod]
        [Ignore]
        public async Task GetCostCentersAsync_ClosedYear_OneYearInFuture()
        {
            // Initialize the test GL configuration repository to pretend we're one year in the future
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, 1));

            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);
            PopulateEncFyrDataContracts(seedUmbrellaAccounts);

            // Populate the poolee accounts for each umbrella
            foreach (var seedUmbrella in seedUmbrellaAccounts)
            {
                // Get the umbrella ID
                var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
                var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

                // Get the poolees for this umbrella
                var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithUnitSubclass("AJK").WithFunction(umbrellaId).GetFilteredGlNumbers();
                PopulateGlsFyrDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
                PopulateEncFyrDataContracts(seedPoolees);
            }

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).ToList();
                    umbrellaEncContracts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Check the non-pooled accounts
                    foreach (var expectedNonPooledGls in expectedSubtotal.NonPooledGls)
                    {
                        var actualNonPooledAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedNonPooledGls.Recordkey);

                        // Start with the GLS data
                        expectedBudget = expectedNonPooledGls.BAlocDebitsYtd.Value - expectedNonPooledGls.BAlocCreditsYtd.Value;
                        expectedActuals = expectedNonPooledGls.DebitsYtd.Value - expectedNonPooledGls.CreditsYtd.Value;
                        expectedEncumbrances = expectedNonPooledGls.EOpenBal.Value + expectedNonPooledGls.EncumbrancesYtd.Value
                            - expectedNonPooledGls.EncumbrancesRelievedYtd.Value;

                        // Add the ENC data for encumbrances only.
                        var selectedEnc = expectedSubtotal.NonPooledEnc.FirstOrDefault(x => x.Recordkey == expectedNonPooledGls.Recordkey);
                        if (selectedEnc != null)
                        {
                            expectedEncumbrances += selectedEnc.EncReqAmt.Sum(x => x.Value);
                        }

                        Assert.AreEqual(expectedBudget, actualNonPooledAccount.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualNonPooledAccount.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualNonPooledAccount.EncumbranceAmount);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = expectedPool.Umbrella.BAlocDebitsYtd.Value - expectedPool.Umbrella.BAlocCreditsYtd.Value;
                        expectedActuals = expectedPool.Umbrella.GlsFaMactuals.Sum(x => x.Value);
                        expectedEncumbrances = expectedPool.Umbrella.GlsFaMencumbrances.Sum(x => x.Value);

                        // Add the ENC data for encumbrances only.
                        if (expectedPool.UmbrellaRequisitions != null)
                        {
                            expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
                        }

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella exists as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(actualUmbrellaPoolee);

                        // We know the umbrella has direct expenses so factor those in...
                        var umbrellaPooleeActuals = expectedPool.Umbrella.DebitsYtd.Value - expectedPool.Umbrella.CreditsYtd.Value;
                        var umbrellaPooleeEncumbrances = expectedPool.Umbrella.EOpenBal.Value + expectedPool.Umbrella.EncumbrancesYtd.Value
                            - expectedPool.Umbrella.EncumbrancesRelievedYtd.Value;

                        // Also factor in requisition amounts from ENC...
                        umbrellaPooleeEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);

                        Assert.AreEqual(0m, actualUmbrellaPoolee.BudgetAmount);
                        Assert.AreEqual(umbrellaPooleeActuals, actualUmbrellaPoolee.ActualAmount);
                        Assert.AreEqual(umbrellaPooleeEncumbrances, actualUmbrellaPoolee.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Poolee, actualUmbrellaPoolee.PoolType);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.Recordkey);

                            expectedBudget = expectedPoolee.BAlocDebitsYtd.Value - expectedPoolee.BAlocCreditsYtd.Value;
                            expectedActuals = expectedPoolee.DebitsYtd.Value - expectedPoolee.CreditsYtd.Value;
                            expectedEncumbrances = expectedPoolee.EOpenBal.Value + expectedPoolee.EncumbrancesYtd.Value - expectedPoolee.EncumbrancesRelievedYtd.Value;

                            // Factor in the requisition amounts for this poolee.
                            var selectedEnc = expectedPool.Requisitions.FirstOrDefault(x => x.Recordkey == expectedPoolee.Recordkey);
                            if (selectedEnc != null)
                            {
                                expectedEncumbrances += selectedEnc.EncReqAmt.Sum(x => x.Value);
                            }

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                            Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_UmbrellasHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);
            PopulateEncFyrDataContracts(seedUmbrellaAccounts);

            // Populate the poolee accounts for each umbrella
            foreach (var seedUmbrella in seedUmbrellaAccounts)
            {
                // Get the umbrella ID
                var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
                var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

                // Get the poolees for this umbrella
                var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithUnitSubclass("AJK").WithFunction(umbrellaId).GetFilteredGlNumbers();
                PopulateGlsFyrDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
                PopulateEncFyrDataContracts(seedPoolees);
            }

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).Where(x => x != null).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).Where(x => x != null).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).Where(x => x != null).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).Where(x => x != null).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).Where(x => x != null).ToList();
                    var umbrellaEncAmounts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).Where(x => x != null).SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncAmounts.Sum(x => x.Value) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Check the non-pooled accounts
                    foreach (var expectedNonPooledGls in expectedSubtotal.NonPooledGls)
                    {
                        var actualNonPooledAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedNonPooledGls.Recordkey);

                        // Start with the GLS data
                        expectedBudget = expectedNonPooledGls.BAlocDebitsYtd.Value - expectedNonPooledGls.BAlocCreditsYtd.Value;
                        expectedActuals = expectedNonPooledGls.DebitsYtd.Value - expectedNonPooledGls.CreditsYtd.Value;
                        expectedEncumbrances = expectedNonPooledGls.EOpenBal.Value + expectedNonPooledGls.EncumbrancesYtd.Value
                            - expectedNonPooledGls.EncumbrancesRelievedYtd.Value;

                        // Add the ENC data for encumbrances only.
                        var selectedEncAmounts = expectedSubtotal.NonPooledEnc.FirstOrDefault(x => x.Recordkey == expectedNonPooledGls.Recordkey)
                            .EncReqAmt.Where(x => x.HasValue).ToList();
                        if (selectedEncAmounts != null && selectedEncAmounts.Any())
                        {
                            expectedEncumbrances += selectedEncAmounts.Sum(x => x.Value);
                        }

                        Assert.AreEqual(expectedBudget, actualNonPooledAccount.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualNonPooledAccount.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualNonPooledAccount.EncumbranceAmount);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = expectedPool.Umbrella.BAlocDebitsYtd.Value - expectedPool.Umbrella.BAlocCreditsYtd.Value;
                        expectedActuals = expectedPool.Umbrella.GlsFaMactuals.Sum(x => x.Value);
                        expectedEncumbrances = expectedPool.Umbrella.GlsFaMencumbrances.Sum(x => x.Value);

                        // Add the ENC data for encumbrances only.
                        if (expectedPool.UmbrellaRequisitions != null)
                        {
                            expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
                        }

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella exists as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(actualUmbrellaPoolee);

                        // We know the umbrella has direct expenses so factor those in...
                        var umbrellaPooleeActuals = expectedPool.Umbrella.DebitsYtd.Value - expectedPool.Umbrella.CreditsYtd.Value;
                        var umbrellaPooleeEncumbrances = expectedPool.Umbrella.EOpenBal.Value + expectedPool.Umbrella.EncumbrancesYtd.Value
                            - expectedPool.Umbrella.EncumbrancesRelievedYtd.Value;

                        // Also factor in requisition amounts from ENC...
                        umbrellaPooleeEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);

                        Assert.AreEqual(0m, actualUmbrellaPoolee.BudgetAmount);
                        Assert.AreEqual(umbrellaPooleeActuals, actualUmbrellaPoolee.ActualAmount);
                        Assert.AreEqual(umbrellaPooleeEncumbrances, actualUmbrellaPoolee.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Poolee, actualUmbrellaPoolee.PoolType);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.Recordkey);

                            expectedBudget = expectedPoolee.BAlocDebitsYtd.Value - expectedPoolee.BAlocCreditsYtd.Value;
                            expectedActuals = expectedPoolee.DebitsYtd.Value - expectedPoolee.CreditsYtd.Value;
                            expectedEncumbrances = expectedPoolee.EOpenBal.Value + expectedPoolee.EncumbrancesYtd.Value - expectedPoolee.EncumbrancesRelievedYtd.Value;

                            // Factor in the requisition amounts for this poolee.
                            var selectedEncAmounts = expectedPool.Requisitions.FirstOrDefault(x => x.Recordkey == expectedPoolee.Recordkey)
                                .EncReqAmt.Where(x => x.HasValue).ToList();
                            if (selectedEncAmounts != null && selectedEncAmounts.Any())
                            {
                                expectedEncumbrances += selectedEncAmounts.Sum(x => x.Value);
                            }

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                            Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_UmbrellasDoNotHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null, false);

            // Populate the poolee accounts for each umbrella
            foreach (var seedUmbrella in seedUmbrellaAccounts)
            {
                // Get the umbrella ID
                var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
                var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

                // Get the poolees for this umbrella
                var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithUnitSubclass("AJK").WithFunction(umbrellaId).GetFilteredGlNumbers();
                PopulateGlsFyrDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
                PopulateEncFyrDataContracts(seedPoolees, false, true);
            }

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts, false, true);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + nonPooledEncContracts.Sum(x => x.EncReqAmt.Where(y => y.HasValue).Sum(z => z.Value));

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).ToList();
                    umbrellaEncContracts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    var nonPooledEncAmounts = expectedSubtotal.NonPooledEnc.SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + nonPooledEncAmounts.Sum(x => x.Value);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Check the non-pooled accounts
                    foreach (var expectedNonPooledGls in expectedSubtotal.NonPooledGls)
                    {
                        var actualNonPooledAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedNonPooledGls.Recordkey);

                        // Start with the GLS data
                        expectedBudget = expectedNonPooledGls.BAlocDebitsYtd.Value - expectedNonPooledGls.BAlocCreditsYtd.Value;
                        expectedActuals = expectedNonPooledGls.DebitsYtd.Value - expectedNonPooledGls.CreditsYtd.Value;
                        expectedEncumbrances = expectedNonPooledGls.EOpenBal.Value + expectedNonPooledGls.EncumbrancesYtd.Value
                            - expectedNonPooledGls.EncumbrancesRelievedYtd.Value;

                        // Add the ENC data for encumbrances only.
                        var selectedEncAmounts = expectedSubtotal.NonPooledEnc.FirstOrDefault(x => x.Recordkey == expectedNonPooledGls.Recordkey)
                            .EncReqAmt.Where(x => x.HasValue).ToList();
                        if (selectedEncAmounts != null && selectedEncAmounts.Any())
                        {
                            expectedEncumbrances += selectedEncAmounts.Sum(x => x.Value);
                        }

                        Assert.AreEqual(expectedBudget, actualNonPooledAccount.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualNonPooledAccount.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualNonPooledAccount.EncumbranceAmount);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = expectedPool.Umbrella.BAlocDebitsYtd.Value - expectedPool.Umbrella.BAlocCreditsYtd.Value;
                        expectedActuals = expectedPool.Umbrella.GlsFaMactuals.Sum(x => x.Value);
                        expectedEncumbrances = expectedPool.Umbrella.GlsFaMencumbrances.Sum(x => x.Value);

                        // Add the ENC data for encumbrances only.
                        if (expectedPool.UmbrellaRequisitions != null)
                        {
                            expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
                        }

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella does NOT exist as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNull(actualUmbrellaPoolee);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.Recordkey);

                            expectedBudget = expectedPoolee.BAlocDebitsYtd.Value - expectedPoolee.BAlocCreditsYtd.Value;
                            expectedActuals = expectedPoolee.DebitsYtd.Value - expectedPoolee.CreditsYtd.Value;
                            expectedEncumbrances = expectedPoolee.EOpenBal.Value + expectedPoolee.EncumbrancesYtd.Value - expectedPoolee.EncumbrancesRelievedYtd.Value;

                            // Factor in the requisition amounts for this poolee.
                            var selectedEncAmounts = expectedPool.Requisitions.FirstOrDefault(x => x.Recordkey == expectedPoolee.Recordkey).EncReqAmt.Where(x => x.HasValue).ToList();
                            if (selectedEncAmounts != null && selectedEncAmounts.Any())
                            {
                                expectedEncumbrances += selectedEncAmounts.Sum(x => x.Value);
                            }

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                            Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
                        }
                    }
                }
            }

            //BuildExpectedCostCentersForClosedYear();

            //var actualCostCenters = await RealRepository_GetCostCentersAsync();

            //Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            //// Make sure the number of cost centers are the same
            //Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            //// Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            //foreach (var expectedCostCenter in this.expectedCostCenters)
            //{
            //    var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

            //    Assert.IsNotNull(actualCostCenter);
            //    Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
            //    Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
            //    Assert.AreEqual(expectedCostCenter.TotalBudget, actualCostCenter.TotalBudget);
            //    Assert.AreEqual(expectedCostCenter.TotalActuals, actualCostCenter.TotalActuals);
            //    Assert.AreEqual(expectedCostCenter.TotalEncumbrances, actualCostCenter.TotalEncumbrances);

            //    // Confirm that the subtotal properties match.
            //    foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
            //    {
            //        var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

            //        Assert.IsNotNull(actualSubtotal);
            //        Assert.IsNull(actualSubtotal.Name);
            //        Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
            //        Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
            //        Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

            //        // Confirm that the GL account data matches.
            //        foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
            //        {
            //            var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

            //            Assert.IsNotNull(actualGlAccount);
            //            Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
            //            Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
            //            Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
            //            Assert.IsNull(actualGlAccount.GlAccountDescription);
            //            Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
            //        }

            //        // Make sure the pooled account properties match
            //        foreach (var expectedPool in expectedSubtotal.Pools)
            //        {
            //            var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

            //            Assert.IsNotNull(actualPool);

            //            // Check the umbrella properties
            //            Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
            //            Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
            //            Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
            //            Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
            //            Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
            //            Assert.IsTrue(actualPool.IsUmbrellaVisible);

            //            // Make sure the umbrella does NOT exist as a poolee.
            //            var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
            //            Assert.IsNull(umbrellaPoolee);

            //            // Check the poolee properties
            //            foreach (var expectedPoolee in expectedPool.Poolees)
            //            {
            //                var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

            //                Assert.IsNotNull(actualPoolee);
            //                Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
            //                Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
            //                Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
            //                Assert.IsNull(actualPoolee.GlAccountDescription);
            //                Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
            //            }
            //        }
            //    }
            //}
        }

        [TestMethod]
        [Ignore]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_NoPoolees_UmbrellasHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);
            PopulateEncFyrDataContracts(seedUmbrellaAccounts);

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).ToList();
                    umbrellaEncContracts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = expectedPool.Umbrella.BAlocDebitsYtd.Value - expectedPool.Umbrella.BAlocCreditsYtd.Value;
                        expectedActuals = expectedPool.Umbrella.GlsFaMactuals.Sum(x => x.Value);
                        expectedEncumbrances = expectedPool.Umbrella.GlsFaMencumbrances.Sum(x => x.Value);

                        // Add the ENC data for encumbrances only.
                        if (expectedPool.UmbrellaRequisitions != null)
                        {
                            expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
                        }

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella exists as a poolee and that it's the only poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(actualUmbrellaPoolee);
                        Assert.AreEqual(1, actualPool.Poolees.Count);

                        // We know the umbrella has direct expenses so factor those in...
                        var umbrellaPooleeActuals = expectedPool.Umbrella.DebitsYtd.Value - expectedPool.Umbrella.CreditsYtd.Value;
                        var umbrellaPooleeEncumbrances = expectedPool.Umbrella.EOpenBal.Value + expectedPool.Umbrella.EncumbrancesYtd.Value
                            - expectedPool.Umbrella.EncumbrancesRelievedYtd.Value;

                        // Also factor in requisition amounts from ENC...
                        umbrellaPooleeEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);

                        Assert.AreEqual(0m, actualUmbrellaPoolee.BudgetAmount);
                        Assert.AreEqual(umbrellaPooleeActuals, actualUmbrellaPoolee.ActualAmount);
                        Assert.AreEqual(umbrellaPooleeEncumbrances, actualUmbrellaPoolee.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Poolee, actualUmbrellaPoolee.PoolType);
                    }
                }
            }

            //BuildExpectedCostCentersForClosedYear();

            //var actualCostCenters = await RealRepository_GetCostCentersAsync();

            //Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            //// Make sure the number of cost centers are the same
            //Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            //// Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            //foreach (var expectedCostCenter in this.expectedCostCenters)
            //{
            //    var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

            //    // Confirm that the subtotal properties match.
            //    foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
            //    {

            //        // Make sure the pooled account properties match
            //        foreach (var expectedPool in expectedSubtotal.Pools)
            //        {
            //            var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

            //            Assert.IsNotNull(actualPool);

            //            // Check the umbrella properties
            //            Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
            //            Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
            //            Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
            //            Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
            //            Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
            //            Assert.IsTrue(actualPool.IsUmbrellaVisible);

            //            // Make sure the umbrella also exists as a poolee and that it's the only poolee.
            //            var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
            //            Assert.IsNotNull(umbrellaPoolee);
            //            Assert.AreEqual(1, actualPool.Poolees.Count);
            //        }
            //    }
            //}
        }

        [TestMethod]
        [Ignore]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_NoPoolees_UmbrellasDoNotHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null, false);

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).ToList().Where(x => x != null).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).ToList();
                    var umbrellaEncAmounts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).Where(x => x != null).SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncAmounts.Sum(x => x.Value) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        expectedBudget = expectedPool.Umbrella.BAlocDebitsYtd.Value - expectedPool.Umbrella.BAlocCreditsYtd.Value;
                        expectedActuals = expectedPool.Umbrella.GlsFaMactuals.Sum(x => x.Value);
                        expectedEncumbrances = expectedPool.Umbrella.GlsFaMencumbrances.Sum(x => x.Value);

                        // Add the ENC data for encumbrances only.
                        if (expectedPool.UmbrellaRequisitions != null)
                        {
                            expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
                        }

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella does NOT exist as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNull(actualUmbrellaPoolee);

                        // Make sure there are no poolees
                        Assert.AreEqual(0, actualPool.Poolees.Count);
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_NoAccessToUmbrellas_HasPoolees_UmbrellasHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocationSubclass("P").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            foreach (var seedUmbrella in seedUmbrellaAccounts)
            {
                var dataContract = PopulateSingleGlsFyrDataContract(seedUmbrella, seedUmbrella, "U", true, false, false);
                glsFyrNoAccessUmbrellaDataContracts.Add(dataContract);
                this.glpFyrDataContracts.Add(new GlpFyr() { Recordkey = seedUmbrella, GlpPooleeAcctsList = new List<string>() });
            }
            PopulateEncFyrDataContracts(seedUmbrellaAccounts);

            // Populate the poolee accounts for each umbrella
            var umbrella1 = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnitSubclass("AJK").GetFilteredGlNumbers().FirstOrDefault();
            var pooleesForUmbrella1 = testGlNumberRepository.WithLocationSubclass("P").WithFunction("U1").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(pooleesForUmbrella1, GlBudgetPoolType.Poolee, umbrella1);
            PopulateEncFyrDataContracts(pooleesForUmbrella1);

            var umbrella2 = testGlNumberRepository.WithLocation("UM").WithFunction("U2").WithUnitSubclass("AJK").GetFilteredGlNumbers().FirstOrDefault();
            var pooleesForUmbrella2 = testGlNumberRepository.WithLocationSubclass("P").WithFunction("U2").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(pooleesForUmbrella2, GlBudgetPoolType.Poolee, umbrella2);
            PopulateEncFyrDataContracts(pooleesForUmbrella2);


            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).Where(x => x != null).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).Where(x => x != null).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).Where(x => x != null).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).Where(x => x != null).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                // Include the GLS amounts from the "no access" data contract
                foreach (var dataContract in glsFyrNoAccessUmbrellaDataContracts)
                {
                    var costCenterId = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_costCenterStructure).Item1;
                    if (costCenterId == actualCostCenter.Id)
                    {
                        expectedBudget += dataContract.BAlocDebitsYtd.Value - dataContract.BAlocCreditsYtd.Value;
                        expectedActuals += dataContract.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
                        expectedEncumbrances += dataContract.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
                    }
                }

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).Where(x => x != null).ToList();
                    var umbrellaEncAmounts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).Where(x => x != null).SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncAmounts.Sum(x => x.Value) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    // Include the GLS amounts from the "no access" data contract
                    foreach (var dataContract in glsFyrNoAccessUmbrellaDataContracts)
                    {
                        var result = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_costCenterStructure);
                        var costCenterId = result.Item1;
                        var subtotalId = result.Item3;
                        if (costCenterId == actualCostCenter.Id && subtotalId == actualSubtotal.Id)
                        {
                            expectedBudget += dataContract.BAlocDebitsYtd.Value - dataContract.BAlocCreditsYtd.Value;
                            expectedActuals += dataContract.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
                            expectedEncumbrances += dataContract.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
                        }
                    }

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsFalse(actualPool.IsUmbrellaVisible);

                        // Use the GLS amounts from the "no access" data contract instead of the umbrella directly since the data won't be there
                        var noAccessUmbrella = glsFyrNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == expectedPool.UmbrellaReference);
                        if (noAccessUmbrella != null)
                        {
                            expectedBudget = noAccessUmbrella.BAlocDebitsYtd.Value - noAccessUmbrella.BAlocCreditsYtd.Value;
                            expectedActuals = noAccessUmbrella.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
                            expectedEncumbrances = noAccessUmbrella.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
                        }

                        // Add the ENC data for encumbrances only.
                        if (expectedPool.UmbrellaRequisitions != null)
                        {
                            expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
                        }

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella does NOT exist as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNull(actualUmbrellaPoolee);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.Recordkey);

                            expectedBudget = expectedPoolee.BAlocDebitsYtd.Value - expectedPoolee.BAlocCreditsYtd.Value;
                            expectedActuals = expectedPoolee.DebitsYtd.Value - expectedPoolee.CreditsYtd.Value;
                            expectedEncumbrances = expectedPoolee.EOpenBal.Value + expectedPoolee.EncumbrancesYtd.Value - expectedPoolee.EncumbrancesRelievedYtd.Value;

                            // Factor in the requisition amounts for this poolee.
                            var selectedEncAmounts = expectedPool.Requisitions.FirstOrDefault(x => x.Recordkey == expectedPoolee.Recordkey)
                                .EncReqAmt.Where(x => x.HasValue).ToList();
                            if (selectedEncAmounts != null && selectedEncAmounts.Any())
                            {
                                expectedEncumbrances += selectedEncAmounts.Sum(x => x.Value);
                            }

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                            Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        //[TestMethod]
        //public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_NoAccessToUmbrellas_HasPoolees_UmbrellasHaveDirectExpenses_GlpPooleeReference()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocationSubclass("P").GetFilteredGlNumbers());
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;
        //     
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
        //    foreach (var seedUmbrella in seedUmbrellaAccounts)
        //    {
        //        var dataContract = PopulateSingleGlsFyrDataContract(seedUmbrella, seedUmbrella, "U", true, false, false);
        //        glsFyrNoAccessUmbrellaDataContracts.Add(dataContract);
        //    }
        //    PopulateEncFyrDataContracts(seedUmbrellaAccounts);

        //    // Populate the poolee accounts for each umbrella
        //    var umbrella1 = testGlNumberRepository.WithLocation("UM").WithFunction("U1").GetFilteredGlNumbers().FirstOrDefault();
        //    var pooleesForUmbrella1 = testGlNumberRepository.WithLocationSubclass("P").WithFunction("U1").GetFilteredGlNumbers();
        //    PopulateGlsFyrDataContracts(pooleesForUmbrella1, GlBudgetPoolType.Poolee, umbrella1);
        //    PopulateEncFyrDataContracts(pooleesForUmbrella1);

        //    var umbrella2 = testGlNumberRepository.WithLocation("UM").WithFunction("U2").GetFilteredGlNumbers().FirstOrDefault();
        //    var pooleesForUmbrella2 = testGlNumberRepository.WithLocationSubclass("P").WithFunction("U2").GetFilteredGlNumbers();
        //    //PopulateGlsFyrDataContracts(pooleesForUmbrella2, GlBudgetPoolType.Poolee, umbrella2);
        //    PopulateEncFyrDataContracts(pooleesForUmbrella2);
        //    this.glpFyrDataContracts.Add(new GlpFyr() { Recordkey = umbrella2, GlpPooleeAcctsList = pooleesForUmbrella2 });

        //    // Populate the non-pooled accounts
        //    var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
        //    PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        //    PopulateEncFyrDataContracts(seedNonPooledAccounts);
        //    #endregion

        //    BuildCostCenterHelpersForClosedYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    // Make sure the number of cost centers are the same
        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in costCenterHelpersGls)
        //    {
        //        var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

        //        var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).Where(x => x != null).ToList();
        //        var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).Where(x => x != null).ToList();
        //        var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
        //            + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
        //        var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
        //            - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

        //        var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).Where(x => x != null).ToList();
        //        var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).Where(x => x != null).ToList();
        //        var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
        //            + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
        //            + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

        //        // Include the GLS amounts from the "no access" data contract
        //        foreach (var dataContract in glsFyrNoAccessUmbrellaDataContracts)
        //        {
        //            var costCenterId = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_glAccountStructure).Item1;
        //            if (costCenterId == actualCostCenter.Id)
        //            {
        //                expectedBudget += dataContract.BAlocDebitsYtd.Value - dataContract.BAlocCreditsYtd.Value;
        //                expectedActuals += dataContract.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
        //                expectedEncumbrances += dataContract.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
        //            }
        //        }

        //        Assert.IsNotNull(actualCostCenter);
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
        //        Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(expectedActuals, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrances);

        //        // Confirm that the subtotal properties match.
        //        foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
        //        {
        //            var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
        //            umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).Where(x => x != null).ToList();
        //            var umbrellaEncAmounts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).Where(x => x != null).SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();
        //            nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
        //            nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

        //            expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
        //                + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
        //            expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
        //            - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
        //            expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
        //            + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
        //            + umbrellaEncAmounts.Sum(x => x.Value) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

        //            // Include the GLS amounts from the "no access" data contract
        //            foreach (var dataContract in glsFyrNoAccessUmbrellaDataContracts)
        //            {
        //                var result = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_glAccountStructure);
        //                var costCenterId = result.Item1;
        //                var subtotalId = result.Item3;
        //                if (costCenterId == actualCostCenter.Id && subtotalId == actualSubtotal.Id)
        //                {
        //                    expectedBudget += dataContract.BAlocDebitsYtd.Value - dataContract.BAlocCreditsYtd.Value;
        //                    expectedActuals += dataContract.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
        //                    expectedEncumbrances += dataContract.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
        //                }
        //            }

        //            Assert.IsNotNull(actualSubtotal);
        //            Assert.IsNull(actualSubtotal.Name);
        //            Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
        //            Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
        //            Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

        //            // Make sure the pooled account properties match
        //            foreach (var expectedPool in expectedSubtotal.BudgetPools)
        //            {
        //                var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
        //                Assert.IsFalse(actualPool.IsUmbrellaVisible);

        //                // Use the GLS amounts from the "no access" data contract instead of the umbrella directly since the data won't be there
        //                var noAccessUmbrella = glsFyrNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == expectedPool.UmbrellaReference);
        //                if (noAccessUmbrella != null)
        //                {
        //                    expectedBudget = noAccessUmbrella.BAlocDebitsYtd.Value - noAccessUmbrella.BAlocCreditsYtd.Value;
        //                    expectedActuals = noAccessUmbrella.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
        //                    expectedEncumbrances = noAccessUmbrella.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
        //                }

        //                // Add the ENC data for encumbrances only.
        //                if (expectedPool.UmbrellaRequisitions != null)
        //                {
        //                    expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
        //                }

        //                // Check the umbrella properties
        //                Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
        //                Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
        //                Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
        //                Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

        //                // Make sure the umbrella does NOT exist as a poolee
        //                var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
        //                Assert.IsNull(actualUmbrellaPoolee);

        //                // Check the poolee properties
        //                foreach (var expectedPoolee in expectedPool.Poolees)
        //                {
        //                    var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.Recordkey);

        //                    expectedBudget = expectedPoolee.BAlocDebitsYtd.Value - expectedPoolee.BAlocCreditsYtd.Value;
        //                    expectedActuals = expectedPoolee.DebitsYtd.Value - expectedPoolee.CreditsYtd.Value;
        //                    expectedEncumbrances = expectedPoolee.EOpenBal.Value + expectedPoolee.EncumbrancesYtd.Value - expectedPoolee.EncumbrancesRelievedYtd.Value;

        //                    // Factor in the requisition amounts for this poolee.
        //                    var selectedEncAmounts = expectedPool.Requisitions.FirstOrDefault(x => x.Recordkey == expectedPoolee.Recordkey)
        //                        .EncReqAmt.Where(x => x.HasValue).ToList();
        //                    if (selectedEncAmounts != null && selectedEncAmounts.Any())
        //                    {
        //                        expectedEncumbrances += selectedEncAmounts.Sum(x => x.Value);
        //                    }

        //                    Assert.IsNotNull(actualPoolee);
        //                    Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
        //                    Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
        //                    Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
        //                    Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
        //                }
        //            }
        //        }
        //    }
        //}

        [Ignore]
        [TestMethod]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_NoAccessToUmbrellas_HasPoolees_UmbrellasDoNotHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocationSubclass("P").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            foreach (var seedUmbrella in seedUmbrellaAccounts)
            {
                var dataContract = PopulateSingleGlsFyrDataContract(seedUmbrella, seedUmbrella, "U", false, false, false);
                this.glsFyrNoAccessUmbrellaDataContracts.Add(dataContract);
            }

            // Populate the poolee accounts for each umbrella
            var umbrella1 = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnitSubclass("AJK").GetFilteredGlNumbers().FirstOrDefault();
            var pooleesForUmbrella1 = testGlNumberRepository.WithLocationSubclass("P").WithFunction("U1").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(pooleesForUmbrella1, GlBudgetPoolType.Poolee, umbrella1);
            PopulateEncFyrDataContracts(pooleesForUmbrella1);

            var umbrella2 = testGlNumberRepository.WithLocation("UM").WithFunction("U2").WithUnitSubclass("AJK").GetFilteredGlNumbers().FirstOrDefault();
            var pooleesForUmbrella2 = testGlNumberRepository.WithLocationSubclass("P").WithFunction("U2").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(pooleesForUmbrella2, GlBudgetPoolType.Poolee, umbrella2);
            PopulateEncFyrDataContracts(pooleesForUmbrella2);

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).Where(x => x != null).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).Where(x => x != null).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).Where(x => x != null).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).Where(x => x != null).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                // Include the GLS amounts from the "no access" data contract
                foreach (var dataContract in glsFyrNoAccessUmbrellaDataContracts)
                {
                    var costCenterId = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_costCenterStructure).Item1;
                    if (costCenterId == actualCostCenter.Id)
                    {
                        expectedBudget += dataContract.BAlocDebitsYtd.Value - dataContract.BAlocCreditsYtd.Value;
                        expectedActuals += dataContract.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
                        expectedEncumbrances += dataContract.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
                    }
                }

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).Where(x => x != null).ToList();
                    var umbrellaEncAmounts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).Where(x => x != null).SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncAmounts.Sum(x => x.Value) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    // Include the GLS amounts from the "no access" data contract
                    foreach (var dataContract in glsFyrNoAccessUmbrellaDataContracts)
                    {
                        var result = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_costCenterStructure);
                        var costCenterId = result.Item1;
                        var subtotalId = result.Item3;
                        if (costCenterId == actualCostCenter.Id && subtotalId == actualSubtotal.Id)
                        {
                            expectedBudget += dataContract.BAlocDebitsYtd.Value - dataContract.BAlocCreditsYtd.Value;
                            expectedActuals += dataContract.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
                            expectedEncumbrances += dataContract.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
                        }
                    }

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.BudgetPools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.UmbrellaReference);
                        Assert.IsFalse(actualPool.IsUmbrellaVisible);

                        // Use the GLS amounts from the "no access" data contract instead of the umbrella directly since the data won't be there
                        var noAccessUmbrella = glsFyrNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == expectedPool.UmbrellaReference);
                        if (noAccessUmbrella != null)
                        {
                            expectedBudget = noAccessUmbrella.BAlocDebitsYtd.Value - noAccessUmbrella.BAlocCreditsYtd.Value;
                            expectedActuals = noAccessUmbrella.GlsFaMactuals.Where(x => x.HasValue).Sum(x => x.Value);
                            expectedEncumbrances = noAccessUmbrella.GlsFaMencumbrances.Where(x => x.HasValue).Sum(x => x.Value);
                        }

                        // Add the ENC data for encumbrances only.
                        if (expectedPool.UmbrellaRequisitions != null)
                        {
                            expectedEncumbrances += expectedPool.UmbrellaRequisitions.EncReqAmt.Sum(x => x.Value);
                        }

                        // Check the umbrella properties
                        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
                        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

                        // Make sure the umbrella does NOT exist as a poolee
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNull(actualUmbrellaPoolee);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.Recordkey);

                            expectedBudget = expectedPoolee.BAlocDebitsYtd.Value - expectedPoolee.BAlocCreditsYtd.Value;
                            expectedActuals = expectedPoolee.DebitsYtd.Value - expectedPoolee.CreditsYtd.Value;
                            expectedEncumbrances = expectedPoolee.EOpenBal.Value + expectedPoolee.EncumbrancesYtd.Value - expectedPoolee.EncumbrancesRelievedYtd.Value;

                            // Factor in the requisition amounts for this poolee.
                            var selectedEncAmounts = expectedPool.Requisitions.FirstOrDefault(x => x.Recordkey == expectedPoolee.Recordkey)
                                .EncReqAmt.Where(x => x.HasValue).ToList();
                            if (selectedEncAmounts != null && selectedEncAmounts.Any())
                            {
                                expectedEncumbrances += selectedEncAmounts.Sum(x => x.Value);
                            }

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
                            Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_NoAccessToUmbrellas_NoPoolees_UmbrellasHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
            foreach (var seedUmbrella in seedUmbrellaAccounts)
            {
                var dataContract = PopulateSingleGlsFyrDataContract(seedUmbrella, seedUmbrella, "U", false, false, false);
                this.glsFyrNoAccessUmbrellaDataContracts.Add(dataContract);
            }

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).Where(x => x != null).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).Where(x => x != null).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).Where(x => x != null).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).Where(x => x != null).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).Where(x => x != null).ToList();
                    var umbrellaEncAmounts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).Where(x => x != null).SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncAmounts.Sum(x => x.Value) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // There should be no budget pools
                    Assert.AreEqual(0, actualSubtotal.Pools.Count);
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetCostCentersAsync_MultipleCostCenters_ClosedYear_DataInGlsAndEnc_WithPools_NoAccessToUmbrellas_NoPoolees_UmbrellasDoNotHaveDirectExpenses()
        {
            #region Set up the parameters to pass into the "real repository"
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - 2;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            #endregion

            #region Set up the expected data
            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
            PopulateGlsFyrDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(seedNonPooledAccounts);
            #endregion

            BuildCostCenterHelpersForClosedYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure the number of cost centers are the same
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(costCenterHelpersGls.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in costCenterHelpersGls)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                var umbrellaGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.Umbrella).Where(x => x != null).ToList();
                var nonPooledGlsContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledGls).Where(x => x != null).ToList();
                var expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                    + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                var expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);

                var umbrellaEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.BudgetPools).Select(x => x.UmbrellaRequisitions).Where(x => x != null).ToList();
                var nonPooledEncContracts = expectedCostCenter.Subtotals.SelectMany(x => x.NonPooledEnc).Where(x => x != null).ToList();
                var expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value)) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedActuals, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);
                    umbrellaGlsContracts = expectedSubtotal.BudgetPools.Select(x => x.Umbrella).Where(x => x != null).ToList();
                    var umbrellaEncAmounts = expectedSubtotal.BudgetPools.Select(x => x.UmbrellaRequisitions).Where(x => x != null).SelectMany(x => x.EncReqAmt).Where(x => x.HasValue).ToList();
                    nonPooledGlsContracts = expectedSubtotal.NonPooledGls.ToList();
                    nonPooledEncContracts = expectedSubtotal.NonPooledEnc.ToList();

                    expectedBudget = umbrellaGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - umbrellaGlsContracts.Sum(x => x.BAlocCreditsYtd.Value)
                        + nonPooledGlsContracts.Sum(x => x.BAlocDebitsYtd.Value) - nonPooledGlsContracts.Sum(x => x.BAlocCreditsYtd.Value);
                    expectedActuals = umbrellaGlsContracts.Sum(x => x.GlsFaMactuals.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.DebitsYtd.Value)
                    - nonPooledGlsContracts.Sum(x => x.CreditsYtd.Value);
                    expectedEncumbrances = umbrellaGlsContracts.Sum(x => x.GlsFaMencumbrances.Sum(y => y.Value)) + nonPooledGlsContracts.Sum(x => x.EOpenBal.Value)
                    + nonPooledGlsContracts.Sum(x => x.EncumbrancesYtd.Value) - nonPooledGlsContracts.Sum(x => x.EncumbrancesRelievedYtd.Value)
                    + umbrellaEncAmounts.Sum(x => x.Value) + nonPooledEncContracts.Sum(x => x.EncReqAmt.Sum(y => y.Value));

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

                    // There should be no budget pools
                    Assert.AreEqual(0, actualSubtotal.Pools.Count);
                }
            }
        }

        #endregion

        #region One Cost Center - Open and Closed Year Scenarios
        //[TestMethod]
        //public async Task GetCostCentersAsync_OneCostCenter_OpenYear()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "1000AJK55";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
        //     
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

        //    // Initialize the CTX reponse and populate the descriptions.
        //    for (int i = 0; i < this.param1_GlUser.ExpenseAccounts.Count; i++)
        //    {
        //        glAccountsDescriptionResponse.GlAccountIds.Add(this.param1_GlUser.ExpenseAccounts[i]);
        //        glAccountsDescriptionResponse.GlDescriptions.Add("GL Description #" + i);
        //    }
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);

        //    // Populate the poolee accounts for each umbrella
        //    foreach (var seedUmbrella in seedUmbrellaAccounts)
        //    {
        //        // Get the umbrella ID
        //        var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlNumberRepository.FUNCTION_CODE);
        //        var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

        //        // Get the poolees for this umbrella
        //        var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).GetFilteredGlNumbers();
        //        PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
        //    }

        //    // Populate the non-pooled accounts
        //    var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        //    #endregion

        //    BuildExpectedCostCentersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

        //    // Make sure the number of cost centers are the same
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in this.expectedCostCenters)
        //    {
        //        var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

        //        Assert.IsNotNull(actualCostCenter);
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
        //        Assert.AreEqual(expectedCostCenter.TotalBudget, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(expectedCostCenter.TotalActuals, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(expectedCostCenter.TotalEncumbrances, actualCostCenter.TotalEncumbrances);

        //        // Confirm that the subtotal properties match.
        //        foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
        //        {
        //            var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

        //            Assert.IsNotNull(actualSubtotal);
        //            Assert.AreEqual(expectedSubtotal.Name, actualSubtotal.Name);
        //            Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
        //            Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
        //            Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

        //            // Confirm that the GL account data matches.
        //            foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
        //            {
        //                var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

        //                Assert.IsNotNull(actualGlAccount);
        //                Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
        //                Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
        //                Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
        //                Assert.AreEqual(expectedGlAccount.GlAccountDescription, actualGlAccount.GlAccountDescription);
        //                Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
        //            }

        //            // Make sure the pooled account properties match
        //            foreach (var expectedPool in expectedSubtotal.Pools)
        //            {
        //                var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

        //                Assert.IsNotNull(actualPool);

        //                // Check the umbrella properties
        //                Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
        //                Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
        //                Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
        //                Assert.AreEqual(expectedPool.Umbrella.GlAccountDescription, actualPool.Umbrella.GlAccountDescription);
        //                Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
        //                Assert.IsTrue(actualPool.IsUmbrellaVisible);

        //                // Make sure the umbrella also exists as a poolee.
        //                var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
        //                Assert.IsNotNull(umbrellaPoolee);

        //                // Check the poolee properties
        //                foreach (var expectedPoolee in expectedPool.Poolees)
        //                {
        //                    var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

        //                    Assert.IsNotNull(actualPoolee);
        //                    Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
        //                    Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
        //                    Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
        //                    Assert.AreEqual(expectedPoolee.GlAccountDescription, actualPoolee.GlAccountDescription);
        //                    Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
        //                }
        //            }
        //        }
        //    }
        //}

        //[TestMethod]
        //public void GetCostCentersAsync_OneCostCenter_NoSubtotalDescriptions_NoGlAccountDescriptions()
        //{
        //    Assert.Fail();
        //}
        #endregion

        #region Open Year Error-Type scenarios
        //[TestMethod]
        //public async Task GetCostCentersAsync_GlAccountsHaveNullAmounts()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithUnit("AJK55").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
        //     
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    // Populate the GlAccts data contracts for the BulkRead call using the GL User data populated above
        //    PopulateGlAccountDataContracts(this.param1_GlUser.ExpenseAccounts.ToList(), GlBudgetPoolType.None, "", true, false, true);
        //    BuildExpectedCostCentersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();
        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in expectedCostCenters)
        //    {
        //        var selectedCostCenters = actualCostCenters.Where(x => x.Id == expectedCostCenter.Id).ToList();

        //        Assert.AreEqual(1, selectedCostCenters.Count);

        //        var actualCostCenter = selectedCostCenters.First();
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(0m, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(0m, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(0m, actualCostCenter.TotalEncumbrances);
        //    }
        //}

        //[TestMethod]
        //public async Task GetCostCentersAsync_GlAccountsHaveZeroAmounts()
        //{
        //    Assert.Fail();
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithUnit("AJK55").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
        //     
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    // Populate the GlAccts data contracts for the BulkRead call using the GL User data populated above
        //    PopulateGlAccountDataContracts(this.param1_GlUser.ExpenseAccounts.ToList(), GlBudgetPoolType.None, "", true);
        //    BuildExpectedCostCentersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();
        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in expectedCostCenters)
        //    {
        //        var selectedCostCenters = actualCostCenters.Where(x => x.Id == expectedCostCenter.Id).ToList();

        //        Assert.AreEqual(1, selectedCostCenters.Count);

        //        var actualCostCenter = selectedCostCenters.First();
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(0m, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(0m, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(0m, actualCostCenter.TotalEncumbrances);
        //    }
        //}

        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsBulkReadReturnsNull()
        {
            await Helper_InitializeParameters();

            this.glAcctsDataContracts = null;
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure there are no cost centers
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(0, actualCostCenters.Count());
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsIsEmpty()
        {
            await Helper_InitializeParameters();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // Make sure there are no cost centers
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(0, actualCostCenters.Count());
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsReturnsNullUmbrella()
        {
            var umbrellas = testGlNumberRepository.WithLocation("UM").WithUnit("AJK55").GetFilteredGlNumbers();

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(umbrellas);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

            // Populate the umbrella account
            PopulateGlAccountDataContracts(umbrellas, GlBudgetPoolType.Umbrella, null);
            this.glAcctsDataContracts[0] = null;

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should only be one subtotal
            var actualCostCenter = actualCostCenters.First();
            Assert.AreEqual(1, actualCostCenter.CostCenterSubtotals.Count);

            // There should be only one pool
            var actualSubtotal = actualCostCenter.CostCenterSubtotals.First();
            Assert.AreEqual(1, actualSubtotal.Pools.Count);

            // There should be one poolee in the pool and it should be the umbrella
            var actualPool = actualSubtotal.Pools.First();
            Assert.AreEqual(1, actualPool.Poolees.Count);
            Assert.AreEqual(this.glAcctsDataContracts[1].Recordkey, actualPool.Umbrella.GlAccountNumber);

            // There should be zero non-pooled accounts
            Assert.AreEqual(0, actualSubtotal.GlAccounts.Count);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsContainsNullPoolee()
        {
            var umbrella = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers();
            var poolees = new List<string>();
            poolees.AddRange(testGlNumberRepository.WithLocation("P1").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers());
            poolees.AddRange(testGlNumberRepository.WithLocation("P2").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers());

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(umbrella);
            this.param1_GlUser.AddExpenseAccounts(poolees);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

            // Populate the umbrella account
            PopulateGlAccountDataContracts(umbrella, GlBudgetPoolType.Umbrella, null, false);

            // Populate the poolees
            PopulateGlAccountDataContracts(poolees, GlBudgetPoolType.Poolee, umbrella.First());
            this.glAcctsDataContracts[1] = null;

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should only be one subtotal
            var actualCostCenter = actualCostCenters.First();
            Assert.AreEqual(1, actualCostCenter.CostCenterSubtotals.Count);

            // There should be only one pool
            var actualSubtotal = actualCostCenter.CostCenterSubtotals.First();
            Assert.AreEqual(1, actualSubtotal.Pools.Count);

            // There should only be one poolee in the pool. The umbrella has no direct expenses so it should not show up either.
            var actualPool = actualSubtotal.Pools.First();
            Assert.AreEqual(1, actualPool.Poolees.Count);

            // There should be zero non-pooled accounts
            Assert.AreEqual(0, actualSubtotal.GlAccounts.Count);
        }

        // This scenario should result in the actual cost center having one fewer non-pooled GL account than the
        // data contracts list.
        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsContainsNullNonPooledAccount()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithFunction("00").WithUnit("AJK55").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithFunction("01").WithUnit("AJK55").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithFunction("02").WithUnit("AJK55").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

            // Populate the non-pooled accounts then set one to null
            PopulateGlAccountDataContracts(this.param1_GlUser.ExpenseAccounts.ToList(), GlBudgetPoolType.None, null);
            this.glAcctsDataContracts[0] = null;

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should only be one subtotal
            var actualCostCenter = actualCostCenters.First();
            Assert.AreEqual(1, actualCostCenter.CostCenterSubtotals.Count);

            // Confirm that the list of actual non-pooled GL accounts has one fewer than the list of data contracts.
            var actualSubtotal = actualCostCenter.CostCenterSubtotals.First();
            Assert.AreEqual(this.glAcctsDataContracts.Count - 1, actualSubtotal.GlAccounts.Count);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsContainsNullUmbrellaAssociation()
        {
            var umbrella = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers();
            var poolees = new List<string>();
            poolees.AddRange(testGlNumberRepository.WithLocation("P1").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers());
            poolees.AddRange(testGlNumberRepository.WithLocation("P2").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers());

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(umbrella);
            this.param1_GlUser.AddExpenseAccounts(poolees);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

            // Populate the umbrella account
            PopulateGlAccountDataContracts(umbrella, GlBudgetPoolType.Umbrella, null);

            // Populate the poolees
            PopulateGlAccountDataContracts(poolees, GlBudgetPoolType.Poolee, umbrella.First());
            this.glAcctsDataContracts[0].MemosEntityAssociation[0] = null;

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should be zero cost centers.
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(0, actualCostCenters.Count());
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsContainsNullPooleeAssociation()
        {
            var umbrella = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers();
            var poolees = new List<string>();
            poolees.AddRange(testGlNumberRepository.WithLocation("P1").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers());
            poolees.AddRange(testGlNumberRepository.WithLocation("P2").WithFunction("U1").WithUnit("AJK55").GetFilteredGlNumbers());

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(umbrella);
            this.param1_GlUser.AddExpenseAccounts(poolees);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

            // Populate the umbrella account
            PopulateGlAccountDataContracts(umbrella, GlBudgetPoolType.Umbrella, null, false);

            // Populate the poolees
            PopulateGlAccountDataContracts(poolees, GlBudgetPoolType.Poolee, umbrella.First());
            this.glAcctsDataContracts[1].MemosEntityAssociation[0] = null;

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should only be one subtotal
            var actualCostCenter = actualCostCenters.First();
            Assert.AreEqual(1, actualCostCenter.CostCenterSubtotals.Count);

            // There should be only one pool
            var actualSubtotal = actualCostCenter.CostCenterSubtotals.First();
            Assert.AreEqual(1, actualSubtotal.Pools.Count);

            // There should only be one poolee in the pool, the umbrella has no direct expenses so it should not show up either.
            var actualPool = actualSubtotal.Pools.First();
            Assert.AreEqual(1, actualPool.Poolees.Count);

            // There should be zero non-pooled accounts
            Assert.AreEqual(0, actualSubtotal.GlAccounts.Count);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GlAcctsContainsNullNonPooledAccountAssociation()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithFunction("00").WithUnit("AJK55").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithFunction("01").WithUnit("AJK55").GetFilteredGlNumbers());
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithFunction("02").WithUnit("AJK55").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

            // Set a non-pooled account to null
            PopulateGlAccountDataContracts(this.param1_GlUser.ExpenseAccounts.ToList(), GlBudgetPoolType.None, null);
            this.glAcctsDataContracts[0].MemosEntityAssociation[0] = null;

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center
            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should only be one subtotal
            var actualCostCenter = actualCostCenters.First();
            Assert.AreEqual(1, actualCostCenter.CostCenterSubtotals.Count);

            // Confirm that the list of actual non-pooled GL accounts has one fewer than the list of data contracts.
            var actualSubtotal = actualCostCenter.CostCenterSubtotals.First();
            Assert.AreEqual(this.glAcctsDataContracts.Count - 1, actualSubtotal.GlAccounts.Count);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_UmbrellaDataContractRecordKeyIsNull()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts();

            // Set an umbrella to null.
            string umbrellaGlNumber = testGlNumberRepository.WithLocation("UM").WithFunction("U1").GetFilteredGlNumbers().FirstOrDefault();
            this.glAcctsNoAccessUmbrellaDataContracts.Add(this.glAcctsDataContracts[0]);
            this.glAcctsDataContracts[0].Recordkey = null;

            BuildExpectedCostCentersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Make sure the number of cost centers are the same
            Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            // Get the components to identify which GL numbers are umbrellas, poolees, and non-pooled accounts
            var locationComponent = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE);
            var locationSubclassComponent = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_SUBCLASS_CODE);

            // Sort each of the GL.ACCTS data contracts into the correct cost center bucket.
            var costCenterHelpers = new List<CostCenterHelper>();

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in expectedCostCenters)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);
                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedCostCenter.TotalBudgetExpenses, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedCostCenter.TotalActualsExpenses, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedCostCenter.TotalEncumbrancesExpenses, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Confirm that the GL account data matches.
                    foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
                    {
                        var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

                        Assert.IsNotNull(actualGlAccount);
                        Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
                        Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
                        Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
                        Assert.IsNull(actualGlAccount.GlAccountDescription);
                        Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.Pools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(actualPool);

                        // Check the umbrella properties
                        Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
                        Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
                        Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
                        Assert.AreEqual(expectedPool.IsUmbrellaVisible, actualPool.IsUmbrellaVisible);

                        // Make sure the umbrella also exists as a poolee.
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        var expectedUmbrellaPoolee = expectedPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

                        if (expectedUmbrellaPoolee != null)
                        {
                            Assert.IsNotNull(actualUmbrellaPoolee);
                        }
                        else
                        {
                            Assert.IsNull(actualUmbrellaPoolee);
                        }

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
                            Assert.IsNull(actualPoolee.GlAccountDescription);
                            Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_UmbrellaDataContractRecordKeyIsEmpty()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts();

            // Set an umbrella to null.
            string umbrellaGlNumber = testGlNumberRepository.WithLocation("UM").WithFunction("U1").GetFilteredGlNumbers().FirstOrDefault();
            this.glAcctsNoAccessUmbrellaDataContracts.Add(this.glAcctsDataContracts[0]);
            this.glAcctsDataContracts[0].Recordkey = "";

            BuildExpectedCostCentersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Make sure the number of cost centers are the same
            Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            // Get the components to identify which GL numbers are umbrellas, poolees, and non-pooled accounts
            var locationComponent = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE);
            var locationSubclassComponent = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_SUBCLASS_CODE);

            // Sort each of the GL.ACCTS data contracts into the correct cost center bucket.
            var costCenterHelpers = new List<CostCenterHelper>();

            //var umbrellaAccounts = this.glAcctsDataContracts.Where(x => x != null
            //    && x.Recordkey.Substring(locationComponent.StartPosition, Convert.ToInt32(locationComponent.ComponentLength)) == "UM").ToList();
            //var pooleeAccountsForUmbrella1 = this.glAcctsDataContracts.Where(x => x != null
            //    && x.Recordkey.Substring(locationSubclassComponent.StartPosition, Convert.ToInt32(locationSubclassComponent.ComponentLength)) == "P").ToList();
            //var nonPooledAccounts = this.glAcctsDataContracts.Where(x => x != null
            //    && x.Recordkey.Substring(locationComponent.StartPosition, Convert.ToInt32(locationComponent.ComponentLength)) == "NP").ToList();

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in expectedCostCenters)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                //var allExpectedAmounts = expectedCostCenter.Amounts.Select(x => x.FiscalYearAmounts).Where(x => x.GlPooledTypeAssocMember == "U" || x.GlPooledTypeAssocMember == "").ToList();
                //var expectedBudget = allExpectedAmounts.Sum(x => x.FaBudgetMemoAssocMember + x.FaBudgetPostedAssocMember + x.GlBudgetMemosAssocMember + x.GlBudgetPostedAssocMember);
                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedCostCenter.TotalBudgetExpenses, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedCostCenter.TotalActualsExpenses, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedCostCenter.TotalEncumbrancesExpenses, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Confirm that the GL account data matches.
                    foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
                    {
                        var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

                        Assert.IsNotNull(actualGlAccount);
                        Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
                        Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
                        Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
                        Assert.IsNull(actualGlAccount.GlAccountDescription);
                        Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.Pools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(actualPool);

                        // Check the umbrella properties
                        Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
                        Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
                        Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
                        Assert.AreEqual(expectedPool.IsUmbrellaVisible, actualPool.IsUmbrellaVisible);

                        // Make sure the umbrella also exists as a poolee.
                        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        var expectedUmbrellaPoolee = expectedPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

                        if (expectedUmbrellaPoolee != null)
                        {
                            Assert.IsNotNull(actualUmbrellaPoolee);
                        }
                        else
                        {
                            Assert.IsNull(actualUmbrellaPoolee);
                        }

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
                            Assert.IsNull(actualPoolee.GlAccountDescription);
                            Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_PooleeDataContractHasNoUmbrellaReference()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts();

            // Set a poolee to null
            string pooleeGlNumber = testGlNumberRepository.WithLocationSubclass("P").GetFilteredGlNumbers().FirstOrDefault();
            var pooleeGlAccount = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == pooleeGlNumber);
            var pooleeIndex = this.glAcctsDataContracts.IndexOf(pooleeGlAccount);
            this.glAcctsDataContracts[pooleeIndex].GlBudgetLinkage[0] = "";

            BuildExpectedCostCentersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Make sure the number of cost centers are the same
            Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in this.expectedCostCenters)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedCostCenter.TotalBudgetExpenses, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedCostCenter.TotalActualsExpenses, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedCostCenter.TotalEncumbrancesExpenses, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Confirm that the GL account data matches.
                    foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
                    {
                        var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

                        Assert.IsNotNull(actualGlAccount);
                        Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
                        Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
                        Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
                        Assert.IsNull(actualGlAccount.GlAccountDescription);
                        Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.Pools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

                        Assert.IsNotNull(actualPool);

                        // Check the umbrella properties
                        Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
                        Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
                        Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        // Make sure the umbrella also exists as a poolee.
                        var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(umbrellaPoolee);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
                            Assert.IsNull(actualPoolee.GlAccountDescription);
                            Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_PooleeaDataContractRecordKeyIsNull()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts();

            // Set a poolee to null
            string pooleeGlNumber = testGlNumberRepository.WithLocationSubclass("P").GetFilteredGlNumbers().FirstOrDefault();
            var pooleeGlAccount = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == pooleeGlNumber);
            var pooleeIndex = this.glAcctsDataContracts.IndexOf(pooleeGlAccount);
            this.glAcctsDataContracts[pooleeIndex].Recordkey = null;

            BuildExpectedCostCentersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Make sure the number of cost centers are the same
            Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in this.expectedCostCenters)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedCostCenter.TotalBudgetExpenses, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedCostCenter.TotalActualsExpenses, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedCostCenter.TotalEncumbrancesExpenses, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Confirm that the GL account data matches.
                    foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
                    {
                        var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

                        Assert.IsNotNull(actualGlAccount);
                        Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
                        Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
                        Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
                        Assert.IsNull(actualGlAccount.GlAccountDescription);
                        Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.Pools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

                        Assert.IsNotNull(actualPool);

                        // Check the umbrella properties
                        Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
                        Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
                        Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        // Make sure the umbrella also exists as a poolee.
                        var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(umbrellaPoolee);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
                            Assert.IsNull(actualPoolee.GlAccountDescription);
                            Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersAsync_PooleeDataContractRecordKeyIsEmpty()
        {
            await Helper_InitializeParameters();
            Helper_InitializeGlAcctsDataContracts();

            // Set a poolee to null
            string pooleeGlNumber = testGlNumberRepository.WithLocationSubclass("P").GetFilteredGlNumbers().FirstOrDefault();
            var pooleeGlAccount = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == pooleeGlNumber);
            var pooleeIndex = this.glAcctsDataContracts.IndexOf(pooleeGlAccount);
            this.glAcctsDataContracts[pooleeIndex].Recordkey = "";

            BuildExpectedCostCentersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Make sure the number of cost centers are the same
            Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in this.expectedCostCenters)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedCostCenter.TotalBudgetExpenses, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedCostCenter.TotalActualsExpenses, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedCostCenter.TotalEncumbrancesExpenses, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Confirm that the GL account data matches.
                    foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
                    {
                        var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

                        Assert.IsNotNull(actualGlAccount);
                        Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
                        Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
                        Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
                        Assert.IsNull(actualGlAccount.GlAccountDescription);
                        Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.Pools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

                        Assert.IsNotNull(actualPool);

                        // Check the umbrella properties
                        Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
                        Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
                        Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
                        Assert.IsTrue(actualPool.IsUmbrellaVisible);

                        // Make sure the umbrella also exists as a poolee.
                        var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNotNull(umbrellaPoolee);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
                            Assert.IsNull(actualPoolee.GlAccountDescription);
                            Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }

        //[TestMethod]
        //public async Task GetCostCentersAsync_GlAcctsReadForPooleeReturnsNullContract()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("P1").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
        //     
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").GetFilteredGlNumbers().FirstOrDefault();
        //    var dataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", false, false, false);
        //    glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);

        //    // Populate the poolee accounts for each umbrella
        //    var pooleesForUmbrella1 = testGlNumberRepository.WithLocation("P1").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(pooleesForUmbrella1, GlBudgetPoolType.Poolee, seedUmbrellaAccount);
        //    #endregion

        //    BuildCostCenterHelpersForOpenYear();

        //    // Set an umbrella to null.
        //    string umbrellaGlNumber = testGlNumberRepository.WithLocation("UM").WithFunction("U1").GetFilteredGlNumbers().FirstOrDefault();
        //    //var umbrellaGlAccount = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == umbrellaGlNumber);
        //    //var umbrellaIndex = this.glAcctsDataContracts.IndexOf(umbrellaGlAccount);
        //    //this.glAcctsNoAccessUmbrellaDataContracts.Add(this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == umbrellaGlNumber));
        //    this.glAcctsNoAccessUmbrellaDataContracts[0] = null;
        //    //this.glAcctsDataContracts[umbrellaIndex] = null;

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    // Make sure the number of cost centers are the same
        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    var expectedCostCenter = costCenterHelpers.First();
        //    var actualCostCenter = actualCostCenters.First();

        //    // Use the amounts from the "no access" umbrella
        //    var amounts = this.glAcctsNoAccessUmbrellaDataContracts[0].MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());
        //    var expectedBudget = Helper_CalculateBudgetForUmbrella(new List<GlAcctsMemos> { amounts });
        //    var expectedActuals = Helper_CalculateActualsForUmbrella(new List<GlAcctsMemos> { amounts });
        //    var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(new List<GlAcctsMemos> { amounts });

        //    Assert.IsNotNull(actualCostCenter);
        //    Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //    Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
        //    Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudget);
        //    Assert.AreEqual(expectedActuals, actualCostCenter.TotalActuals);
        //    Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrances);

        //    // Confirm that the subtotal properties match.
        //    foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
        //    {
        //        var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);

        //        Assert.IsNotNull(actualSubtotal);
        //        Assert.IsNull(actualSubtotal.Name);
        //        Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
        //        Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
        //        Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

        //        // There should only be one budget pool
        //        Assert.AreEqual(expectedSubtotal.BudgetPools.Count, actualSubtotal.Pools.Count);
        //        Assert.AreEqual(0, actualSubtotal.Pools.Count);

        //        //var expectedPool = expectedSubtotal.BudgetPools.First();
        //        //var actualPool = actualSubtotal.Pools.First();
        //        //Assert.IsFalse(actualPool.IsUmbrellaVisible);

        //        //// Check the umbrella properties
        //        //Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
        //        //Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
        //        //Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
        //        //Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

        //        //// Make sure the umbrella does NOT exist as a poolee
        //        //var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
        //        //Assert.IsNull(actualUmbrellaPoolee);

        //        //// Check the poolee properties
        //        //Assert.AreEqual(1, actualPool.Poolees.Count);
        //        //var expectedPoolee = expectedPool.Poolees.First();
        //        //var actualPoolee = actualPool.Poolees.First();

        //        //expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
        //        //expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
        //        //expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });

        //        //Assert.IsNotNull(actualPoolee);
        //        //Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
        //        //Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
        //        //Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
        //        //Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
        //    }

        //    //var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    //Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

        //    //// Make sure the number of cost centers are the same
        //    //Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

        //    //// Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    //foreach (var expectedCostCenter in this.expectedCostCenters)
        //    //{
        //    //    var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

        //    //    // Confirm that the subtotal properties match.
        //    //    foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
        //    //    {
        //    //        var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

        //    //        // Make sure the pooled account properties match
        //    //        foreach (var expectedPool in expectedSubtotal.Pools)
        //    //        {
        //    //            var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

        //    //            Assert.IsNotNull(actualPool);

        //    //            // Check the umbrella properties
        //    //            Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
        //    //            Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
        //    //            Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
        //    //            Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
        //    //            Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
        //    //            Assert.IsTrue(actualPool.IsUmbrellaVisible);

        //    //            // Make sure the umbrella also exists as a poolee.
        //    //            var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
        //    //            Assert.IsNotNull(umbrellaPoolee);

        //    //            // Check the poolee properties
        //    //            foreach (var expectedPoolee in expectedPool.Poolees)
        //    //            {
        //    //                var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

        //    //                Assert.IsNotNull(actualPoolee);
        //    //                Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
        //    //                Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
        //    //                Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
        //    //                Assert.IsNull(actualPoolee.GlAccountDescription);
        //    //                Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //}

        //[TestMethod]
        //public async Task GetCostCentersAsync_GlAcctsReadForPooleeReturnsContractWithNullRecordKey()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("P1").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
        //     
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").GetFilteredGlNumbers().FirstOrDefault();
        //    var dataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", true, false, false);
        //    glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);

        //    // Populate the poolee accounts for each umbrella
        //    var pooleesForUmbrella1 = testGlNumberRepository.WithLocation("P1").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(pooleesForUmbrella1, GlBudgetPoolType.Poolee, seedUmbrellaAccount);
        //    #endregion

        //    BuildCostCenterHelpersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    // Make sure the number of cost centers are the same
        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(costCenterHelpers.Count, actualCostCenters.Count());
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    var expectedCostCenter = costCenterHelpers.First();
        //    var actualCostCenter = actualCostCenters.First();

        //    // Use the amounts from the "no access" umbrella
        //    var amounts = this.glAcctsNoAccessUmbrellaDataContracts[0].MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());
        //    var expectedBudget = Helper_CalculateBudgetForUmbrella(new List<GlAcctsMemos> { amounts });
        //    var expectedActuals = Helper_CalculateActualsForUmbrella(new List<GlAcctsMemos> { amounts });
        //    var expectedEncumbrances = Helper_CalculateEncumbrancesForUmbrella(new List<GlAcctsMemos> { amounts });

        //    Assert.IsNotNull(actualCostCenter);
        //    Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //    Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
        //    Assert.AreEqual(expectedBudget, actualCostCenter.TotalBudget);
        //    Assert.AreEqual(expectedActuals, actualCostCenter.TotalActuals);
        //    Assert.AreEqual(expectedEncumbrances, actualCostCenter.TotalEncumbrances);

        //    // Confirm that the subtotal properties match.
        //    foreach (var expectedSubtotal in expectedCostCenter.Subtotals)
        //    {
        //        var actualSubtotal = actualCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == expectedSubtotal.Id);

        //        Assert.IsNotNull(actualSubtotal);
        //        Assert.IsNull(actualSubtotal.Name);
        //        Assert.AreEqual(expectedBudget, actualSubtotal.TotalBudget);
        //        Assert.AreEqual(expectedActuals, actualSubtotal.TotalActuals);
        //        Assert.AreEqual(expectedEncumbrances, actualSubtotal.TotalEncumbrances);

        //        // There should only be one budget pool
        //        Assert.AreEqual(expectedSubtotal.BudgetPools.Count, actualSubtotal.Pools.Count);
        //        Assert.AreEqual(1, actualSubtotal.Pools.Count);

        //        var expectedPool = expectedSubtotal.BudgetPools.First();
        //        var actualPool = actualSubtotal.Pools.First();
        //        Assert.IsFalse(actualPool.IsUmbrellaVisible);

        //        // Check the umbrella properties
        //        Assert.AreEqual(expectedBudget, actualPool.Umbrella.BudgetAmount);
        //        Assert.AreEqual(expectedActuals, actualPool.Umbrella.ActualAmount);
        //        Assert.AreEqual(expectedEncumbrances, actualPool.Umbrella.EncumbranceAmount);
        //        Assert.AreEqual(GlBudgetPoolType.Umbrella, actualPool.Umbrella.PoolType);

        //        // Make sure the umbrella does NOT exist as a poolee
        //        var actualUmbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
        //        Assert.IsNull(actualUmbrellaPoolee);

        //        // Check the poolee properties
        //        Assert.AreEqual(1, actualPool.Poolees.Count);
        //        var expectedPoolee = expectedPool.Poolees.First();
        //        var actualPoolee = actualPool.Poolees.First();

        //        expectedBudget = Helper_CalculateBudgetForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
        //        expectedActuals = Helper_CalculateActualsForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });
        //        expectedEncumbrances = Helper_CalculateEncumbrancesForNonUmbrella(new List<GlAcctsMemos>() { expectedPoolee.FiscalYearAmounts });

        //        Assert.IsNotNull(actualPoolee);
        //        Assert.AreEqual(expectedBudget, actualPoolee.BudgetAmount);
        //        Assert.AreEqual(expectedActuals, actualPoolee.ActualAmount);
        //        Assert.AreEqual(expectedEncumbrances, actualPoolee.EncumbranceAmount);
        //        Assert.AreEqual(GlBudgetPoolType.Poolee, actualPoolee.PoolType);
        //    }
        //}

        //[TestMethod]
        //public void GetCostCentersAsync_GlAcctsReadForPooleeReturnsContractWithEmptyRecordKey()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void GetCostCentersAsync_OpenYear_NonPooledAccountRecordKeyIsNull()
        //{
        //    Assert.Fail();
        //}

        [TestMethod]
        public async Task GetCostCentersAsync_OpenYear_NonPooledAccountRecordKeyIsEmpty()
        {
            BuildExpectedCostCentersForOpenYear();

            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);

            // Make sure the number of cost centers are the same
            Assert.AreEqual(this.expectedCostCenters.Count, actualCostCenters.Count());

            // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
            foreach (var expectedCostCenter in this.expectedCostCenters)
            {
                var actualCostCenter = actualCostCenters.FirstOrDefault(x => x.Id == expectedCostCenter.Id);

                Assert.IsNotNull(actualCostCenter);
                Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
                Assert.AreEqual(expectedCostCenter.UnitId, actualCostCenter.UnitId);
                Assert.AreEqual(expectedCostCenter.TotalBudgetExpenses, actualCostCenter.TotalBudgetExpenses);
                Assert.AreEqual(expectedCostCenter.TotalActualsExpenses, actualCostCenter.TotalActualsExpenses);
                Assert.AreEqual(expectedCostCenter.TotalEncumbrancesExpenses, actualCostCenter.TotalEncumbrancesExpenses);

                // Confirm that the subtotal properties match.
                foreach (var expectedSubtotal in expectedCostCenter.CostCenterSubtotals)
                {
                    var actualSubtotal = actualCostCenter.CostCenterSubtotals.First(x => x.Id == expectedSubtotal.Id);

                    Assert.IsNotNull(actualSubtotal);
                    Assert.IsNull(actualSubtotal.Name);
                    Assert.AreEqual(expectedSubtotal.TotalBudget, actualSubtotal.TotalBudget);
                    Assert.AreEqual(expectedSubtotal.TotalActuals, actualSubtotal.TotalActuals);
                    Assert.AreEqual(expectedSubtotal.TotalEncumbrances, actualSubtotal.TotalEncumbrances);

                    // Confirm that the GL account data matches.
                    foreach (var expectedGlAccount in expectedSubtotal.GlAccounts)
                    {
                        var actualGlAccount = actualSubtotal.GlAccounts.FirstOrDefault(x => x.GlAccountNumber == expectedGlAccount.GlAccountNumber);

                        Assert.IsNotNull(actualGlAccount);
                        Assert.AreEqual(expectedGlAccount.BudgetAmount, actualGlAccount.BudgetAmount);
                        Assert.AreEqual(expectedGlAccount.ActualAmount, actualGlAccount.ActualAmount);
                        Assert.AreEqual(expectedGlAccount.EncumbranceAmount, actualGlAccount.EncumbranceAmount);
                        Assert.IsNull(actualGlAccount.GlAccountDescription);
                        Assert.AreEqual(expectedGlAccount.PoolType, actualGlAccount.PoolType);
                    }

                    // Make sure the pooled account properties match
                    foreach (var expectedPool in expectedSubtotal.Pools)
                    {
                        var actualPool = actualSubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == expectedPool.Umbrella.GlAccountNumber);

                        Assert.IsNotNull(actualPool);

                        // Check the umbrella properties
                        Assert.AreEqual(expectedPool.Umbrella.BudgetAmount, actualPool.Umbrella.BudgetAmount);
                        Assert.AreEqual(expectedPool.Umbrella.ActualAmount, actualPool.Umbrella.ActualAmount);
                        Assert.AreEqual(expectedPool.Umbrella.EncumbranceAmount, actualPool.Umbrella.EncumbranceAmount);
                        Assert.IsNull(actualPool.Umbrella.GlAccountDescription);
                        Assert.AreEqual(expectedPool.Umbrella.PoolType, actualPool.Umbrella.PoolType);
                        Assert.IsFalse(actualPool.IsUmbrellaVisible);

                        // Make sure the umbrella also exists as a poolee.
                        var umbrellaPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == actualPool.Umbrella.GlAccountNumber);
                        Assert.IsNull(umbrellaPoolee);

                        // Check the poolee properties
                        foreach (var expectedPoolee in expectedPool.Poolees)
                        {
                            var actualPoolee = actualPool.Poolees.FirstOrDefault(x => x.GlAccountNumber == expectedPoolee.GlAccountNumber);

                            Assert.IsNotNull(actualPoolee);
                            Assert.AreEqual(expectedPoolee.BudgetAmount, actualPoolee.BudgetAmount);
                            Assert.AreEqual(expectedPoolee.ActualAmount, actualPoolee.ActualAmount);
                            Assert.AreEqual(expectedPoolee.EncumbranceAmount, actualPoolee.EncumbranceAmount);
                            Assert.IsNull(actualPoolee.GlAccountDescription);
                            Assert.AreEqual(expectedPoolee.PoolType, actualPoolee.PoolType);
                        }
                    }
                }
            }
        }
        #endregion

        #region Closed Year Error-Type Scenarios
        [TestMethod]
        public async Task GetCostCentersAsync_GlsBulkReadReturnsNull()
        {
            // Get the GL User expense accounts from the test repository.
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;
            testCostCenterRepository.GlsFyrDataContracts = null;

            var costCenters = await RealRepository_GetCostCentersAsync();
            Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
            Assert.IsTrue(costCenters.Count() == 0);
        }

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_GlsBulkReadReturnsEmptyList()
        //{
        //    Assert.Fail();
        //    // Get the GL User expense accounts from the test repository.
        //    this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;
        //    testCostCenterRepository.GlsFyrDataContracts = null;

        //    var costCenters = await RealRepository_GetCostCentersAsync();
        //    Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
        //    Assert.IsTrue(costCenters.Count() == 0);
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_UmbrellaRecordKeyIsNull()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_UmbrellaRecordKeyIsEmpty()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_PooleeRecordKeyIsNull()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_PooleeRecordKeyIsEmpty()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_PooleeHasNoUmbrellaReference()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_ClosedYear_GlAcctsReadForPooleeReturnsNullContract()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_ClosedYear_GlAcctsReadForPooleeReturnsContractWithNullRecordKey()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_ClosedYear_GlAcctsReadForPooleeReturnsContractWithEmptyRecordKey()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_NonPooledAccountRecordKeyIsNull()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_NonPooledAccountRecordKeyIsEmpty()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_ClosedYear_EncBulkReadReturnsNull()
        //{
        //    // Get the GL User expense accounts from the test repository.
        //    this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;
        //    PopulateGlAcctsDataContracts();
        //    testCostCenterRepository.EncFyrDataContracts = null;

        //    var costCenters = await RealRepository_GetCostCentersAsync();
        //    Assert.IsTrue(costCenters is IEnumerable<CostCenter>);

        //    var allSubtotals = costCenters.SelectMany(x => x.CostCenterSubtotals).ToList();
        //    foreach (var subtotal in allSubtotals)
        //    {
        //        foreach (var glAccount in subtotal.GlAccounts)
        //        {
        //            // The amounts for each GL account should equal the amounts from GLS (but not ENC since that returned null)
        //            //decimal expectedBudget = testCostCenterRepository.GlsFyrDataContracts
        //            Assert.AreEqual(0, glAccount.BudgetAmount);
        //        }
        //    }
        //}

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_ClosedYear_EncBulkReadReturnsEmptyList()
        //{
        //    Assert.Fail();
        //    // Get the GL User expense accounts from the test repository.
        //    this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;
        //    PopulateGlAcctsDataContracts();
        //    testCostCenterRepository.EncFyrDataContracts = null;

        //    var costCenters = await RealRepository_GetCostCentersAsync();
        //    Assert.IsTrue(costCenters is IEnumerable<CostCenter>);

        //    var allSubtotals = costCenters.SelectMany(x => x.CostCenterSubtotals).ToList();
        //    foreach (var subtotal in allSubtotals)
        //    {
        //        foreach (var glAccount in subtotal.GlAccounts)
        //        {
        //            // The amounts for each GL account should equal the amounts from GLS (but not ENC since that returned null)
        //            //decimal expectedBudget = testCostCenterRepository.GlsFyrDataContracts
        //            Assert.AreEqual(0, glAccount.BudgetAmount);
        //        }
        //    }
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_GlpFyrSelectReturnsNull()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_GlpFyrSelectReturnsEmptySet()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_GlpFyrBulkReadReturnsNull()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public void GetCostCentersAsync_GlpFyrBulkReadReturnsEmptySet()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_ClosedYear_GlsAndEncBulkReadReturnsNull()
        //{
        //    // Get the GL User expense accounts from the test repository.
        //    this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;
        //    testCostCenterRepository.GlsFyrDataContracts = null;
        //    testCostCenterRepository.EncFyrDataContracts = null;

        //    var costCenters = await RealRepository_GetCostCentersAsync();
        //    Assert.IsTrue(costCenters is IEnumerable<CostCenter>);
        //    Assert.IsTrue(costCenters.Count() == 0);
        //}

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_NullGlsFyrRecord()
        //{
        //    //// Get the GL User expense accounts from the test repository.
        //    //this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    //this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);

        //    //this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

        //    //// Populate the GLS.FYR data contracts for the BulkRead call using the GL User data populated above
        //    //foreach (var glAccount in this.param1_GlUser.ExpenseAccounts)
        //    //{
        //    //    var dataContract = new GlsFyr()
        //    //    {
        //    //        Recordkey = glAccount,
        //    //        DebitsYtd = 500.00m,
        //    //        CreditsYtd = 750.00m,
        //    //        EncumbrancesYtd = 100.00m,
        //    //        EncumbrancesRelievedYtd = 25.00m,
        //    //        BAlocDebitsYtd = 200.00m,
        //    //        BAlocCreditsYtd = 100.00m,
        //    //    };
        //    //    testCostCenterRepository.GlsFyrDataContracts.Add(dataContract);
        //    //}

        //    //// Set the first GlsFyr object to null
        //    //testCostCenterRepository.GlsFyrDataContracts[0] = null;

        //    //this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

        //    //// Get the fiscal year based on today's date
        //    //var glConfiguration = await testGlConfigurationRepository.GetFiscalYearConfigurationAsync();
        //    //this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    //// Gather the necessary information to determine the expected cost center descriptions.
        //    //var descriptionComponents = this.param2_glAccountStructure.CostCenterComponents.Where(x => x.IsPartOfDescription).ToList();

        //    //// Create a list of cost center domain entities to compare to the actual cost centers.
        //    //var expectedCostCenters = new List<CostCenter>();

        //    //// Loop through each GL account from the GL User object and populate the cost center list.
        //    //foreach (var glAccount in this.param1_GlUser.ExpenseAccounts)
        //    //{
        //    //    string costCenterId = "";
        //    //    var glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();
        //    //    foreach (var component in this.param2_glAccountStructure.CostCenterComponents)
        //    //    {
        //    //        var componentId = glAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));
        //    //        costCenterId += componentId;

        //    //        // Add the component description object if the component is part of the description and it's not already in the list.
        //    //        if (component.IsPartOfDescription && glComponentsForCostCenter.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).Count() == 0)
        //    //        {
        //    //            var componentDescription = new GeneralLedgerComponentDescription(componentId, component.ComponentType);

        //    //            componentDescription.Description = testRepositoryDescriptions.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).FirstOrDefault().Description;
        //    //            glComponentsForCostCenter.Add(componentDescription);
        //    //        }
        //    //    }

        //    //    // Create the domain entity for the GL account
        //    //    var glAccountDomain = new CostCenterGlAccount(glAccount, GlBudgetPoolType.None);

        //    //    // Populate the amount values for each GLS.FYR record
        //    //    var selectedGlsFyrDataContract = testCostCenterRepository.GlsFyrDataContracts.Where(x => x != null && x.Recordkey == glAccount).FirstOrDefault();
        //    //    if (selectedGlsFyrDataContract != null)
        //    //    {
        //    //        glAccountDomain.BudgetAmount += selectedGlsFyrDataContract.BAlocDebitsYtd.HasValue ? selectedGlsFyrDataContract.BAlocDebitsYtd.Value : 0m;
        //    //        glAccountDomain.BudgetAmount -= selectedGlsFyrDataContract.BAlocCreditsYtd.HasValue ? selectedGlsFyrDataContract.BAlocCreditsYtd.Value : 0m;
        //    //        glAccountDomain.EncumbranceAmount += selectedGlsFyrDataContract.EncumbrancesYtd.HasValue ? selectedGlsFyrDataContract.EncumbrancesYtd.Value : 0m;
        //    //        glAccountDomain.EncumbranceAmount -= selectedGlsFyrDataContract.EncumbrancesRelievedYtd.HasValue ? selectedGlsFyrDataContract.EncumbrancesRelievedYtd.Value : 0m;
        //    //        glAccountDomain.ActualAmount += selectedGlsFyrDataContract.DebitsYtd.HasValue ? selectedGlsFyrDataContract.DebitsYtd.Value : 0m;
        //    //        glAccountDomain.ActualAmount -= selectedGlsFyrDataContract.CreditsYtd.HasValue ? selectedGlsFyrDataContract.CreditsYtd.Value : 0m;
        //    //    }

        //    //    // Add the cost center to the list of cost centers for the user if it does not exist already.
        //    //    // Also add the GL account to the list of GL accounts for the cost center.
        //    //    var selectedCostCenters = expectedCostCenters.Where(x => x.Id == costCenterId).ToList();
        //    //    // Obtain the GL account digits that determine the cost center subtotal.
        //    //    var costCenterSubtotalId = glAccount.Substring(param2_glAccountStructure.CostCenterSubtotal.StartPosition, Convert.ToInt32(param2_glAccountStructure.CostCenterSubtotal.ComponentLength));
        //    //    CostCenterSubtotal subtotal = new CostCenterSubtotal(costCenterSubtotalId, glAccountDomain);

        //    //    if (selectedCostCenters.Count() == 0)
        //    //    {
        //    //        var costCenterDomain = CostCenterBuilderObject.WithId(costCenterId)
        //    //            .WithSubtotal(subtotal)
        //    //            .WithGlComponentDescriptions(glComponentsForCostCenter).Build();
        //    //        costCenterDomain.AddCostCenterSubtotal(subtotal);
        //    //        expectedCostCenters.Add(costCenterDomain);
        //    //    }
        //    //    else if (selectedCostCenters.Count() == 1)
        //    //    {
        //    //        var costCenterDomain = selectedCostCenters.First();
        //    //        costCenterDomain.AddCostCenterSubtotal(subtotal);
        //    //        subtotal.AddGlAccount(glAccountDomain);
        //    //    }
        //    //}

        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithFund("10").WithSource("00").WithUnit("AJK55").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

        //    //// Initialize the CTX reponse and populate the descriptions.
        //    //for (int i = 0; i < this.param1_GlUser.ExpenseAccounts.Count; i++)
        //    //{
        //    //    glAccountsDescriptionResponse.GlAccountIds.Add(this.param1_GlUser.ExpenseAccounts[i]);
        //    //    glAccountsDescriptionResponse.GlDescriptions.Add("GL Description #" + i);
        //    //}
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);

        //    // Populate the poolee accounts for each umbrella
        //    foreach (var seedUmbrella in seedUmbrellaAccounts)
        //    {
        //        // Get the umbrella ID
        //        var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
        //        var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

        //        // Get the poolees for this umbrella
        //        var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).GetFilteredGlNumbers();
        //        PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
        //    }

        //    // Populate the non-pooled accounts
        //    var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        //    #endregion

        //    BuildExpectedCostCentersForOpenYear();

        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in expectedCostCenters)
        //    {
        //        var selectedCostCenters = actualCostCenters.Where(x => x.Id == expectedCostCenter.Id).ToList();

        //        Assert.AreEqual(1, selectedCostCenters.Count);

        //        var actualCostCenter = selectedCostCenters.First();
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(expectedCostCenter.TotalBudget, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(expectedCostCenter.TotalActuals, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(expectedCostCenter.TotalEncumbrances, actualCostCenter.TotalEncumbrances);

        //        // Make sure we have the same number of GlAccounts
        //        var glAccounts = actualCostCenter.CostCenterSubtotals.SelectMany(i => i.GlAccounts);
        //        Assert.AreEqual(this.param1_GlUser.ExpenseAccounts.Count - 1, glAccounts.Count());
        //    }
        //}

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_GlsFyrRecordKeyIsNull()
        //{
        //    //// Get the GL User expense accounts from the test repository.
        //    //this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    //this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);

        //    //this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

        //    //// Populate the GLS.FYR data contracts for the BulkRead call using the GL User data populated above
        //    //foreach (var glAccount in this.param1_GlUser.ExpenseAccounts)
        //    //{
        //    //    var dataContract = new GlsFyr()
        //    //    {
        //    //        Recordkey = glAccount,
        //    //        DebitsYtd = 500.00m,
        //    //        CreditsYtd = 750.00m,
        //    //        EncumbrancesYtd = 100.00m,
        //    //        EncumbrancesRelievedYtd = 25.00m,
        //    //        BAlocDebitsYtd = 200.00m,
        //    //        BAlocCreditsYtd = 100.00m,
        //    //    };
        //    //    testCostCenterRepository.GlsFyrDataContracts.Add(dataContract);
        //    //}

        //    //// Set the first GlsFyr object to null
        //    //testCostCenterRepository.GlsFyrDataContracts[0] = null;

        //    //this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

        //    //// Get the fiscal year based on today's date
        //    //var glConfiguration = await testGlConfigurationRepository.GetFiscalYearConfigurationAsync();
        //    //this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    //// Gather the necessary information to determine the expected cost center descriptions.
        //    //var descriptionComponents = this.param2_glAccountStructure.CostCenterComponents.Where(x => x.IsPartOfDescription).ToList();

        //    //// Create a list of cost center domain entities to compare to the actual cost centers.
        //    //var expectedCostCenters = new List<CostCenter>();

        //    //// Loop through each GL account from the GL User object and populate the cost center list.
        //    //foreach (var glAccount in this.param1_GlUser.ExpenseAccounts)
        //    //{
        //    //    string costCenterId = "";
        //    //    var glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();
        //    //    foreach (var component in this.param2_glAccountStructure.CostCenterComponents)
        //    //    {
        //    //        var componentId = glAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));
        //    //        costCenterId += componentId;

        //    //        // Add the component description object if the component is part of the description and it's not already in the list.
        //    //        if (component.IsPartOfDescription && glComponentsForCostCenter.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).Count() == 0)
        //    //        {
        //    //            var componentDescription = new GeneralLedgerComponentDescription(componentId, component.ComponentType);

        //    //            componentDescription.Description = testRepositoryDescriptions.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).FirstOrDefault().Description;
        //    //            glComponentsForCostCenter.Add(componentDescription);
        //    //        }
        //    //    }

        //    //    // Create the domain entity for the GL account
        //    //    var glAccountDomain = new CostCenterGlAccount(glAccount, GlBudgetPoolType.None);

        //    //    // Populate the amount values for each GLS.FYR record
        //    //    var selectedGlsFyrDataContract = testCostCenterRepository.GlsFyrDataContracts.Where(x => x != null && x.Recordkey == glAccount).FirstOrDefault();
        //    //    if (selectedGlsFyrDataContract != null)
        //    //    {
        //    //        glAccountDomain.BudgetAmount += selectedGlsFyrDataContract.BAlocDebitsYtd.HasValue ? selectedGlsFyrDataContract.BAlocDebitsYtd.Value : 0m;
        //    //        glAccountDomain.BudgetAmount -= selectedGlsFyrDataContract.BAlocCreditsYtd.HasValue ? selectedGlsFyrDataContract.BAlocCreditsYtd.Value : 0m;
        //    //        glAccountDomain.EncumbranceAmount += selectedGlsFyrDataContract.EncumbrancesYtd.HasValue ? selectedGlsFyrDataContract.EncumbrancesYtd.Value : 0m;
        //    //        glAccountDomain.EncumbranceAmount -= selectedGlsFyrDataContract.EncumbrancesRelievedYtd.HasValue ? selectedGlsFyrDataContract.EncumbrancesRelievedYtd.Value : 0m;
        //    //        glAccountDomain.ActualAmount += selectedGlsFyrDataContract.DebitsYtd.HasValue ? selectedGlsFyrDataContract.DebitsYtd.Value : 0m;
        //    //        glAccountDomain.ActualAmount -= selectedGlsFyrDataContract.CreditsYtd.HasValue ? selectedGlsFyrDataContract.CreditsYtd.Value : 0m;
        //    //    }

        //    //    // Add the cost center to the list of cost centers for the user if it does not exist already.
        //    //    // Also add the GL account to the list of GL accounts for the cost center.
        //    //    var selectedCostCenters = expectedCostCenters.Where(x => x.Id == costCenterId).ToList();
        //    //    // Obtain the GL account digits that determine the cost center subtotal.
        //    //    var costCenterSubtotalId = glAccount.Substring(param2_glAccountStructure.CostCenterSubtotal.StartPosition, Convert.ToInt32(param2_glAccountStructure.CostCenterSubtotal.ComponentLength));
        //    //    CostCenterSubtotal subtotal = new CostCenterSubtotal(costCenterSubtotalId, glAccountDomain);

        //    //    if (selectedCostCenters.Count() == 0)
        //    //    {
        //    //        var costCenterDomain = CostCenterBuilderObject.WithId(costCenterId)
        //    //            .WithSubtotal(subtotal)
        //    //            .WithGlComponentDescriptions(glComponentsForCostCenter).Build();
        //    //        costCenterDomain.AddCostCenterSubtotal(subtotal);
        //    //        expectedCostCenters.Add(costCenterDomain);
        //    //    }
        //    //    else if (selectedCostCenters.Count() == 1)
        //    //    {
        //    //        var costCenterDomain = selectedCostCenters.First();
        //    //        costCenterDomain.AddCostCenterSubtotal(subtotal);
        //    //        subtotal.AddGlAccount(glAccountDomain);
        //    //    }
        //    //}

        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithFund("10").WithSource("00").WithUnit("AJK55").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

        //    //// Initialize the CTX reponse and populate the descriptions.
        //    //for (int i = 0; i < this.param1_GlUser.ExpenseAccounts.Count; i++)
        //    //{
        //    //    glAccountsDescriptionResponse.GlAccountIds.Add(this.param1_GlUser.ExpenseAccounts[i]);
        //    //    glAccountsDescriptionResponse.GlDescriptions.Add("GL Description #" + i);
        //    //}
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);

        //    // Populate the poolee accounts for each umbrella
        //    foreach (var seedUmbrella in seedUmbrellaAccounts)
        //    {
        //        // Get the umbrella ID
        //        var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
        //        var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

        //        // Get the poolees for this umbrella
        //        var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).GetFilteredGlNumbers();
        //        PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
        //    }

        //    // Populate the non-pooled accounts
        //    var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        //    #endregion

        //    BuildExpectedCostCentersForOpenYear();

        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in expectedCostCenters)
        //    {
        //        var selectedCostCenters = actualCostCenters.Where(x => x.Id == expectedCostCenter.Id).ToList();

        //        Assert.AreEqual(1, selectedCostCenters.Count);

        //        var actualCostCenter = selectedCostCenters.First();
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(expectedCostCenter.TotalBudget, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(expectedCostCenter.TotalActuals, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(expectedCostCenter.TotalEncumbrances, actualCostCenter.TotalEncumbrances);

        //        // Make sure we have the same number of GlAccounts
        //        var glAccounts = actualCostCenter.CostCenterSubtotals.SelectMany(i => i.GlAccounts);
        //        Assert.AreEqual(this.param1_GlUser.ExpenseAccounts.Count - 1, glAccounts.Count());
        //    }
        //}

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_GlsFyrRecordKeyIsEmpty()
        //{
        //    //// Get the GL User expense accounts from the test repository.
        //    //this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    //this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);

        //    //this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

        //    //// Populate the GLS.FYR data contracts for the BulkRead call using the GL User data populated above
        //    //foreach (var glAccount in this.param1_GlUser.ExpenseAccounts)
        //    //{
        //    //    var dataContract = new GlsFyr()
        //    //    {
        //    //        Recordkey = glAccount,
        //    //        DebitsYtd = 500.00m,
        //    //        CreditsYtd = 750.00m,
        //    //        EncumbrancesYtd = 100.00m,
        //    //        EncumbrancesRelievedYtd = 25.00m,
        //    //        BAlocDebitsYtd = 200.00m,
        //    //        BAlocCreditsYtd = 100.00m,
        //    //    };
        //    //    testCostCenterRepository.GlsFyrDataContracts.Add(dataContract);
        //    //}

        //    //// Set the first GlsFyr object to null
        //    //testCostCenterRepository.GlsFyrDataContracts[0] = null;

        //    //this.param2_glAccountStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();

        //    //// Get the fiscal year based on today's date
        //    //var glConfiguration = await testGlConfigurationRepository.GetFiscalYearConfigurationAsync();
        //    //this.param4_FiscalYear = testGlConfigurationRepository.StartYear - 2;

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    //// Gather the necessary information to determine the expected cost center descriptions.
        //    //var descriptionComponents = this.param2_glAccountStructure.CostCenterComponents.Where(x => x.IsPartOfDescription).ToList();

        //    //// Create a list of cost center domain entities to compare to the actual cost centers.
        //    //var expectedCostCenters = new List<CostCenter>();

        //    //// Loop through each GL account from the GL User object and populate the cost center list.
        //    //foreach (var glAccount in this.param1_GlUser.ExpenseAccounts)
        //    //{
        //    //    string costCenterId = "";
        //    //    var glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();
        //    //    foreach (var component in this.param2_glAccountStructure.CostCenterComponents)
        //    //    {
        //    //        var componentId = glAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));
        //    //        costCenterId += componentId;

        //    //        // Add the component description object if the component is part of the description and it's not already in the list.
        //    //        if (component.IsPartOfDescription && glComponentsForCostCenter.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).Count() == 0)
        //    //        {
        //    //            var componentDescription = new GeneralLedgerComponentDescription(componentId, component.ComponentType);

        //    //            componentDescription.Description = testRepositoryDescriptions.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).FirstOrDefault().Description;
        //    //            glComponentsForCostCenter.Add(componentDescription);
        //    //        }
        //    //    }

        //    //    // Create the domain entity for the GL account
        //    //    var glAccountDomain = new CostCenterGlAccount(glAccount, GlBudgetPoolType.None);

        //    //    // Populate the amount values for each GLS.FYR record
        //    //    var selectedGlsFyrDataContract = testCostCenterRepository.GlsFyrDataContracts.Where(x => x != null && x.Recordkey == glAccount).FirstOrDefault();
        //    //    if (selectedGlsFyrDataContract != null)
        //    //    {
        //    //        glAccountDomain.BudgetAmount += selectedGlsFyrDataContract.BAlocDebitsYtd.HasValue ? selectedGlsFyrDataContract.BAlocDebitsYtd.Value : 0m;
        //    //        glAccountDomain.BudgetAmount -= selectedGlsFyrDataContract.BAlocCreditsYtd.HasValue ? selectedGlsFyrDataContract.BAlocCreditsYtd.Value : 0m;
        //    //        glAccountDomain.EncumbranceAmount += selectedGlsFyrDataContract.EncumbrancesYtd.HasValue ? selectedGlsFyrDataContract.EncumbrancesYtd.Value : 0m;
        //    //        glAccountDomain.EncumbranceAmount -= selectedGlsFyrDataContract.EncumbrancesRelievedYtd.HasValue ? selectedGlsFyrDataContract.EncumbrancesRelievedYtd.Value : 0m;
        //    //        glAccountDomain.ActualAmount += selectedGlsFyrDataContract.DebitsYtd.HasValue ? selectedGlsFyrDataContract.DebitsYtd.Value : 0m;
        //    //        glAccountDomain.ActualAmount -= selectedGlsFyrDataContract.CreditsYtd.HasValue ? selectedGlsFyrDataContract.CreditsYtd.Value : 0m;
        //    //    }

        //    //    // Add the cost center to the list of cost centers for the user if it does not exist already.
        //    //    // Also add the GL account to the list of GL accounts for the cost center.
        //    //    var selectedCostCenters = expectedCostCenters.Where(x => x.Id == costCenterId).ToList();
        //    //    // Obtain the GL account digits that determine the cost center subtotal.
        //    //    var costCenterSubtotalId = glAccount.Substring(param2_glAccountStructure.CostCenterSubtotal.StartPosition, Convert.ToInt32(param2_glAccountStructure.CostCenterSubtotal.ComponentLength));
        //    //    CostCenterSubtotal subtotal = new CostCenterSubtotal(costCenterSubtotalId, glAccountDomain);

        //    //    if (selectedCostCenters.Count() == 0)
        //    //    {
        //    //        var costCenterDomain = CostCenterBuilderObject.WithId(costCenterId)
        //    //            .WithSubtotal(subtotal)
        //    //            .WithGlComponentDescriptions(glComponentsForCostCenter).Build();
        //    //        costCenterDomain.AddCostCenterSubtotal(subtotal);
        //    //        expectedCostCenters.Add(costCenterDomain);
        //    //    }
        //    //    else if (selectedCostCenters.Count() == 1)
        //    //    {
        //    //        var costCenterDomain = selectedCostCenters.First();
        //    //        costCenterDomain.AddCostCenterSubtotal(subtotal);
        //    //        subtotal.AddGlAccount(glAccountDomain);
        //    //    }
        //    //}

        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithFund("10").WithSource("00").WithUnit("AJK55").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

        //    //// Initialize the CTX reponse and populate the descriptions.
        //    //for (int i = 0; i < this.param1_GlUser.ExpenseAccounts.Count; i++)
        //    //{
        //    //    glAccountsDescriptionResponse.GlAccountIds.Add(this.param1_GlUser.ExpenseAccounts[i]);
        //    //    glAccountsDescriptionResponse.GlDescriptions.Add("GL Description #" + i);
        //    //}
        //    #endregion

        //    #region Set up the expected data
        //    var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null);

        //    // Populate the poolee accounts for each umbrella
        //    foreach (var seedUmbrella in seedUmbrellaAccounts)
        //    {
        //        // Get the umbrella ID
        //        var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
        //        var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

        //        // Get the poolees for this umbrella
        //        var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).GetFilteredGlNumbers();
        //        PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
        //    }

        //    // Populate the non-pooled accounts
        //    var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").GetFilteredGlNumbers();
        //    PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        //    #endregion

        //    BuildExpectedCostCentersForOpenYear();

        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in expectedCostCenters)
        //    {
        //        var selectedCostCenters = actualCostCenters.Where(x => x.Id == expectedCostCenter.Id).ToList();

        //        Assert.AreEqual(1, selectedCostCenters.Count);

        //        var actualCostCenter = selectedCostCenters.First();
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(expectedCostCenter.TotalBudget, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(expectedCostCenter.TotalActuals, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(expectedCostCenter.TotalEncumbrances, actualCostCenter.TotalEncumbrances);

        //        // Make sure we have the same number of GlAccounts
        //        var glAccounts = actualCostCenter.CostCenterSubtotals.SelectMany(i => i.GlAccounts);
        //        Assert.AreEqual(this.param1_GlUser.ExpenseAccounts.Count - 1, glAccounts.Count());
        //    }
        //}

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_OpenYear_MultipleCostCenters_NullGlAccount()
        //{
        //    #region Set up the parameters to pass into the "real repository"
        //    this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
        //    this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation("NP").WithUnit("AJK55").GetFilteredGlNumbers());
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.param3_SelectedCostCenterId = "";
        //    this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;

        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    #endregion

        //    // Populate the GlAccts data contracts for the BulkRead call using the GL User data populated above
        //    PopulateGlAccountDataContracts(this.param1_GlUser.ExpenseAccounts.ToList(), GlBudgetPoolType.None, "");

        //    // Set the first GlAccts object to null
        //    this.glAcctsDataContracts[0] = null;

        //    BuildExpectedCostCentersForOpenYear();

        //    var actualCostCenters = await RealRepository_GetCostCentersAsync();

        //    Assert.IsTrue(actualCostCenters is IEnumerable<CostCenter>);
        //    Assert.AreEqual(1, actualCostCenters.Count());

        //    // Loop through the expected cost centers and check that the data inside matches the data in the actual cost centers
        //    foreach (var expectedCostCenter in expectedCostCenters)
        //    {
        //        var selectedCostCenters = actualCostCenters.Where(x => x.Id == expectedCostCenter.Id).ToList();

        //        Assert.AreEqual(1, selectedCostCenters.Count);

        //        var actualCostCenter = selectedCostCenters.First();
        //        Assert.AreEqual(expectedCostCenter.Name, actualCostCenter.Name);
        //        Assert.AreEqual(expectedCostCenter.TotalBudget, actualCostCenter.TotalBudget);
        //        Assert.AreEqual(expectedCostCenter.TotalActuals, actualCostCenter.TotalActuals);
        //        Assert.AreEqual(expectedCostCenter.TotalEncumbrances, actualCostCenter.TotalEncumbrances);

        //        // Make sure we have the same number of GlAccounts
        //        var glAccounts = actualCostCenter.CostCenterSubtotals.SelectMany(i => i.GlAccounts);
        //        Assert.AreEqual(this.param1_GlUser.ExpenseAccounts.Count - 1, glAccounts.Count());
        //    }
        //}
        #endregion

        #region Inactive GL accounts with no activity
        [TestMethod]
        public async Task GetCostCentersAsync_OneNonPooledAccountHasNoActivity()
        {
            #region Initialize parameters
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnit("NMK67").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up GL accounts
            var unitId = "NMK67";
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit(unitId).GetFilteredGlNumbers().FirstOrDefault();
            var umbrellaDataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", false, false, false);
            this.glAcctsDataContracts.Add(umbrellaDataContract);

            // Populate the poolee accounts for the umbrella
            var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var umbrellaId = seedUmbrellaAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

            // Get the poolees for this umbrella
            var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrellaAccount);

            // Populate the non-pooled accounts; the first GL account should have all zero amounts.
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnit(unitId).GetFilteredGlNumbers();
            var firstNonPooledAccount = seedNonPooledAccounts.First();
            var nonPooledDataContract = PopulateSingleGlAcctsDataContract(firstNonPooledAccount, "", "", false, true, false);
            this.glAcctsDataContracts.Add(nonPooledDataContract);

            // Populate the non-pooled accounts; the rest of the GL accounts should have activity.
            var remainingNonPooledAccounts = seedNonPooledAccounts.GetRange(1, seedNonPooledAccounts.Count - 1);
            PopulateGlAccountDataContracts(remainingNonPooledAccounts, GlBudgetPoolType.None, null);
            #endregion

            #region Set up the lists of GL account IDs to be returned by the GLS, ENC, and GL.ACCTS select calls.
            var allIds = this.glAcctsDataContracts.Where(x => x.FaBudgetMemo.First() != null && x.FaBudgetMemo.First() > 0).Select(x => x.Recordkey).ToList();
            this.glsIds = allIds.ToArray();
            this.encIds = allIds.ToArray();
            this.glAcctsIds = allIds.ToArray();
            #endregion

            #region Call the repository and make assertions
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center.
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should still be one subtotal.
            Assert.AreEqual(1, actualCostCenters.First().CostCenterSubtotals.Count);

            // There should be one pool.
            var subtotal = actualCostCenters.First().CostCenterSubtotals.First();
            Assert.AreEqual(1, subtotal.Pools.Count);

            // The poolees should all be present; there is no umbrella poolee
            Assert.AreEqual(seedPoolees.Count, subtotal.Pools.First().Poolees.Count);

            // One of the non-pooled accounts should be gone.
            Assert.AreEqual(seedNonPooledAccounts.Count - 1, subtotal.GlAccounts.Count);
            #endregion
        }

        [TestMethod]
        public async Task GetCostCentersAsync_AllNonPooledAccountHaveNoActivity()
        {
            #region Initialize parameters
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnit("NMK67").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up GL accounts
            var unitId = "NMK67";
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit(unitId).GetFilteredGlNumbers().FirstOrDefault();
            var umbrellaDataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", false, false, false);
            this.glAcctsDataContracts.Add(umbrellaDataContract);

            // Populate the poolee accounts for the umbrella
            var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var umbrellaId = seedUmbrellaAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

            // Get the poolees for this umbrella
            var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrellaAccount);

            // Populate the non-pooled accounts; all the GL accounts should have no activity.
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null, false, true, false);
            #endregion

            #region Set up the lists of GL account IDs to be returned by the GLS, ENC, and GL.ACCTS select calls.
            var allIds = this.glAcctsDataContracts.Where(x => x.FaBudgetMemo.First() != null && x.FaBudgetMemo.First() > 0).Select(x => x.Recordkey).ToList();
            this.glsIds = allIds.ToArray();
            this.encIds = allIds.ToArray();
            this.glAcctsIds = allIds.ToArray();
            #endregion

            #region Call the repository and make assertions
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center.
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should still be one subtotal.
            Assert.AreEqual(1, actualCostCenters.First().CostCenterSubtotals.Count);

            // There should be one pool.
            var subtotal = actualCostCenters.First().CostCenterSubtotals.First();
            Assert.AreEqual(1, subtotal.Pools.Count);

            // The poolees should all be present, there is no umbrella poolee.
            Assert.AreEqual(seedPoolees.Count, subtotal.Pools.First().Poolees.Count);

            // There should be zero non-pooled accounts.
            Assert.AreEqual(0, subtotal.GlAccounts.Count);
            #endregion
        }

        [TestMethod]
        public async Task GetCostCentersAsync_OnePooleeHasNoActivity_NoUmbrellaPoolee()
        {
            #region Initialize parameters
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnit("NMK67").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up GL accounts
            var unitId = "NMK67";
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit(unitId).GetFilteredGlNumbers().FirstOrDefault();
            var umbrellaDataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", false, false, false);
            this.glAcctsDataContracts.Add(umbrellaDataContract);

            // Populate the poolee accounts for the umbrella
            var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var umbrellaId = seedUmbrellaAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

            // Populate the poolees; the first one should be have no activity
            var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).WithUnit(unitId).GetFilteredGlNumbers();
            var firstPoolee = seedPoolees.First();
            var pooleeDataContract = PopulateSingleGlAcctsDataContract(firstPoolee, seedUmbrellaAccount, "P", false, true, false);
            this.glAcctsDataContracts.Add(pooleeDataContract);

            // Populate the poolees, the remaining poolees should have activity.
            var remainingPoolees = seedPoolees.GetRange(1, seedPoolees.Count - 1);
            PopulateGlAccountDataContracts(remainingPoolees, GlBudgetPoolType.Poolee, seedUmbrellaAccount);

            // Populate the non-pooled accounts; all should have activity
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            #endregion

            #region Set up the lists of GL account IDs to be returned by the GLS, ENC, and GL.ACCTS select calls.
            var allIds = this.glAcctsDataContracts.Where(x => x.FaBudgetMemo.First() != null && x.FaBudgetMemo.First() > 0).Select(x => x.Recordkey).ToList();
            this.glsIds = allIds.ToArray();
            this.encIds = allIds.ToArray();
            this.glAcctsIds = allIds.ToArray();
            #endregion

            #region Call the repository and make assertions
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center.
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should still be one subtotal.
            Assert.AreEqual(1, actualCostCenters.First().CostCenterSubtotals.Count);

            // There should be one pool.
            var subtotal = actualCostCenters.First().CostCenterSubtotals.First();
            Assert.AreEqual(1, subtotal.Pools.Count);

            // One of the poolees should have been removed; there is no umbrella poolee
            Assert.AreEqual(seedPoolees.Count - 1, subtotal.Pools.First().Poolees.Count);

            // All of the non-pooled accounts should be intact.
            Assert.AreEqual(seedNonPooledAccounts.Count, subtotal.GlAccounts.Count);
            #endregion
        }

        [TestMethod]
        public async Task GetCostCentersAsync_AllPooleesHaveNoActivity_UmbrellaPooleeHasActivity()
        {
            #region Initialize parameters
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnit("NMK67").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up GL accounts
            var unitId = "NMK67";
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit(unitId).GetFilteredGlNumbers().FirstOrDefault();
            var umbrellaDataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", true, false, false);
            this.glAcctsDataContracts.Add(umbrellaDataContract);

            // Populate the poolee accounts for the umbrella
            var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var umbrellaId = seedUmbrellaAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

            // Get the poolees for this umbrella
            var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrellaAccount, false, true);

            // Populate the non-pooled account; all the GL accounts should have activity.
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
            #endregion

            #region Set up the lists of GL account IDs to be returned by the GLS, ENC, and GL.ACCTS select calls.
            var allIds = this.glAcctsDataContracts.Where(x => x.FaBudgetMemo.First() != null && x.FaBudgetMemo.First() > 0).Select(x => x.Recordkey).ToList();
            this.glsIds = allIds.ToArray();
            this.encIds = allIds.ToArray();
            this.glAcctsIds = allIds.ToArray();
            #endregion

            #region Call the repository and make assertions
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center.
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should still be one subtotal.
            Assert.AreEqual(1, actualCostCenters.First().CostCenterSubtotals.Count);

            // There should be one pool.
            var subtotal = actualCostCenters.First().CostCenterSubtotals.First();
            Assert.AreEqual(1, subtotal.Pools.Count);

            // There should be one poolee; the umbrella poolee
            Assert.AreEqual(1, subtotal.Pools.First().Poolees.Count);
            Assert.AreEqual(seedUmbrellaAccount, subtotal.Pools.First().Poolees.First().GlAccountNumber);

            // All of the non-pooled accounts should be intact.
            Assert.AreEqual(seedNonPooledAccounts.Count, subtotal.GlAccounts.Count);
            #endregion
        }

        [TestMethod]
        public async Task GetCostCentersAsync_AllGlAccountsHaveNoActivity_UmbrellaHasActivity()
        {
            #region Initialize parameters
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnit("NMK67").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up GL accounts
            var unitId = "NMK67";
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit(unitId).GetFilteredGlNumbers().FirstOrDefault();
            var umbrellaDataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", true, false, false);
            this.glAcctsDataContracts.Add(umbrellaDataContract);

            // Populate the poolee accounts for the umbrella
            var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var umbrellaId = seedUmbrellaAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

            // Get the poolees for this umbrella; all should have no activity.
            var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrellaAccount, false, true);

            // Populate the non-pooled accounts; all should have no activity.
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null, false, true);
            #endregion

            #region Set up the lists of GL account IDs to be returned by the GLS, ENC, and GL.ACCTS select calls.
            var allIds = this.glAcctsDataContracts.Where(x => x.FaBudgetMemo.First() != null && x.FaBudgetMemo.First() > 0).Select(x => x.Recordkey).ToList();
            this.glsIds = allIds.ToArray();
            this.encIds = allIds.ToArray();
            this.glAcctsIds = allIds.ToArray();
            #endregion

            #region Call the repository and make assertions
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should only be one cost center.
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should still be one subtotal.
            Assert.AreEqual(1, actualCostCenters.First().CostCenterSubtotals.Count);

            // There should be one pool.
            var subtotal = actualCostCenters.First().CostCenterSubtotals.First();
            Assert.AreEqual(1, subtotal.Pools.Count);

            // There should be one poolee; the umbrella poolee
            Assert.AreEqual(1, subtotal.Pools.First().Poolees.Count);
            Assert.AreEqual(seedUmbrellaAccount, subtotal.Pools.First().Poolees.First().GlAccountNumber);

            // There should be zero non-pooled accounts.
            Assert.AreEqual(0, subtotal.GlAccounts.Count);
            #endregion
        }

        [TestMethod]
        public async Task GetCostCentersAsync_AllGlAccountsHaveNoActivity_UmbrellaPooleeHasNoActivity()
        {
            #region Initialize parameters
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnit("NMK67").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up GL accounts
            var unitId = "NMK67";
            var seedUmbrellaAccount = testGlNumberRepository.WithLocation("UM").WithFunction("U1").WithUnit(unitId).GetFilteredGlNumbers().FirstOrDefault();
            var umbrellaDataContract = PopulateSingleGlAcctsDataContract(seedUmbrellaAccount, seedUmbrellaAccount, "U", false, true, false);
            this.glAcctsDataContracts.Add(umbrellaDataContract);

            // Populate the poolee accounts for the umbrella
            var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var umbrellaId = seedUmbrellaAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

            // Get the poolees for this umbrella; all should have no activity.
            var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithFunction(umbrellaId).WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrellaAccount, false, true);

            // Populate the non-pooled accounts; all should have no activity.
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null, false, true);
            #endregion

            #region Set up the lists of GL account IDs to be returned by the GLS, ENC, and GL.ACCTS select calls.
            var allIds = this.glAcctsDataContracts.Where(x => x.FaBudgetMemo.First() != null && x.FaBudgetMemo.First() > 0).Select(x => x.Recordkey).ToList();
            this.glsIds = allIds.ToArray();
            this.encIds = allIds.ToArray();
            this.glAcctsIds = allIds.ToArray();
            #endregion

            #region Call the repository and make assertions
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should one cost center.
            Assert.AreEqual(1, actualCostCenters.Count());

            // There should be one subtotal.
            var actualSubtotals = actualCostCenters.First().CostCenterSubtotals;
            Assert.AreEqual(1, actualSubtotals.Count);

            // There should be one pool in the subtotal
            Assert.AreEqual(1, actualSubtotals.First().Pools.Count);

            // The umbrella should exist.
            var actualPool = actualSubtotals.First().Pools.First();
            Assert.AreEqual(seedUmbrellaAccount, actualPool.Umbrella.GlAccountNumber);

            // There should be zero poolees.
            Assert.AreEqual(0, actualPool.Poolees.Count);

            // There should be zero non-pooled accounts.
            Assert.AreEqual(0, actualSubtotals.First().GlAccounts.Count);
            #endregion
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NoPools_AllGlAccountsAreInactive()
        {
            #region Initialize parameters
            var locationId = "NP";
            var unitId = "NMK67";
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithLocation(locationId).WithUnit(unitId).GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = "";
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday;
            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
            InitializeMockStatements();
            #endregion

            #region Set up GL accounts
            // Populate the non-pooled accounts; all should have no activity.
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation(locationId).WithUnit(unitId).GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null, false, true);
            #endregion

            #region Set up the lists of GL account IDs to be returned by the GLS, ENC, and GL.ACCTS select calls.
            var allIds = this.glAcctsDataContracts.Where(x => x.FaBudgetMemo.First() != null && x.FaBudgetMemo.First() > 0).Select(x => x.Recordkey).ToList();
            this.glsIds = allIds.ToArray();
            this.encIds = allIds.ToArray();
            this.glAcctsIds = allIds.ToArray();
            #endregion

            #region Call the repository and make assertions
            var actualCostCenters = await RealRepository_GetCostCentersAsync();

            // There should zero cost centers.
            Assert.AreEqual(0, actualCostCenters.Count());
            #endregion
        }
        #endregion

        //[TestMethod]
        //[Ignore]
        //public async Task GetCostCentersAsync_DetailMode_OpenYear_NoPools_Success()
        //{
        //    // Get the GL User expense accounts from the test repository.
        //    this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
        //    this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
        //    this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
        //    this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();
        //    PopulateGlAcctsDataContracts();

        //    // Get the fiscal year based on today's date
        //    var glConfiguration = await testGlConfigurationRepository.GetFiscalYearConfigurationAsync();
        //    this.param4_FiscalYear = glConfiguration.FiscalYearForToday;


        //    // Set a few amount fields to zero
        //    //testCostCenterRepository.GlaFyrDataContracts.First().GlaDebit = null;
        //    //testCostCenterRepository.GlaFyrDataContracts.First().GlaCredit = null;
        //    //testCostCenterRepository.GlaFyrDataContracts.First().buildAssociations();
        //    //testCostCenterRepository.EncFyrDataContracts.First().EncReqAmt[0] = null;
        //    testCostCenterRepository.EncFyrDataContracts.First().buildAssociations();
        //    SetupCtxResponseObject();

        //    var actualCostCenter = (await RealRepository_GetCostCentersAsync()).First();

        //    // Each GL account should have the same number of transactions as defined in the GLA and ENC data contracts.
        //    var actualGlAccounts = actualCostCenter.CostCenterSubtotals.SelectMany(x => x.GlAccounts).ToList();
        //    foreach (var glAccount in actualGlAccounts)
        //    {
        //        int glaTransactionCount = 0;
        //        int encTransactionCount = 0;
        //        var glaFyrTransactions = testCostCenterRepository.GlaFyrDataContracts.Where(x => x.Recordkey.Split('*').First() == glAccount.GlAccountNumber).ToList();
        //        var encFyrTransaction = testCostCenterRepository.EncFyrDataContracts.FirstOrDefault(w => w.Recordkey == glAccount.GlAccountNumber);

        //        // The amounts of each GL account should match both the amounts from the summary and detail data sources.
        //        // The GL Budget should match the budget amounts from the summary data source.
        //        var relevantGlAcctAmounts = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == glAccount.GlAccountNumber)
        //            .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString()).ToList();
        //        var expectedBudget = relevantGlAcctAmounts.Select(x => x.GlBudgetMemosAssocMember).Where(x => x.HasValue).Sum(x => x.Value)
        //            + relevantGlAcctAmounts.Select(x => x.GlBudgetPostedAssocMember).Where(x => x.HasValue).Sum(x => x.Value);
        //        Assert.AreEqual(expectedBudget, glAccount.BudgetAmount);

        //        // The GL Actuals should match the actuals amounts from the summary data source.
        //        var expectedActuals = relevantGlAcctAmounts.Select(x => x.GlActualMemosAssocMember).Where(x => x.HasValue).Sum(x => x.Value)
        //            + relevantGlAcctAmounts.Select(x => x.GlActualPostedAssocMember).Where(x => x.HasValue).Sum(x => x.Value);
        //        Assert.AreEqual(expectedActuals, glAccount.ActualAmount);

        //        // The GL Encumbrances should match the encumbrance amounts from the summary data source.
        //        var expectedEncumbrances = relevantGlAcctAmounts.Select(x => x.GlEncumbranceMemosAssocMember).Where(x => x.HasValue).Sum(x => x.Value)
        //            + relevantGlAcctAmounts.Select(x => x.GlEncumbrancePostedAssocMember).Where(x => x.HasValue).Sum(x => x.Value)
        //            + relevantGlAcctAmounts.Select(x => x.GlRequisitionMemosAssocMember).Where(x => x.HasValue).Sum(x => x.Value);
        //        Assert.AreEqual(expectedEncumbrances, glAccount.EncumbranceAmount);

        //        // Make sure each GLA transaction is represented in the domain if there are transactions associated with this GL number
        //        if (glaFyrTransactions != null)
        //        {
        //            // Calculate the number of GLA transactions that should be included by removing encumbrance transactions with a zero amount.
        //            glaTransactionCount = glaFyrTransactions.Where(x =>
        //                x.GlaSource == "PJ" || ((x.GlaDebit ?? 0) - (x.GlaCredit ?? 0)) != 0m).Count();

        //            foreach (var glaTransaction in glaFyrTransactions)
        //            {
        //                var glaTransactionType = GlTransactionType.Actual;
        //                var GLSourceCode = this.GlSourceCodes.ValsEntityAssociation.Where(x =>
        //                                x.ValInternalCodeAssocMember == glaTransaction.GlaSource).FirstOrDefault();
        //                switch (GLSourceCode.ValActionCode2AssocMember)
        //                {
        //                    case "1":
        //                        glaTransactionType = GlTransactionType.Actual;
        //                        break;
        //                    case "3":
        //                        glaTransactionType = GlTransactionType.Encumbrance;
        //                        break;
        //                }

        //                var selectedEntity = glAccount.Transactions.FirstOrDefault(x =>
        //                    x.Amount == (glaTransaction.GlaDebit ?? 0) - (glaTransaction.GlaCredit ?? 0)
        //                    && x.Description == glaTransaction.GlaDescription
        //                    && x.GlAccount == glaTransaction.Recordkey.Split('*').FirstOrDefault()
        //                    && x.GlTransactionType == glaTransactionType
        //                    && x.Id == glaTransaction.Recordkey
        //                    && x.ReferenceNumber == glaTransaction.GlaRefNo
        //                    && x.TransactionDate == glaTransaction.GlaTrDate);
        //                Assert.IsNotNull(selectedEntity);

        //                // Make sure the selected entity has the correct document ID
        //                string expectedDocumentId = null;
        //                if (selectedEntity.ReferenceNumber[0].ToString().ToUpper() == "B")
        //                    expectedDocumentId = this.purchasingDocumentIdsResponse.BpoIds[this.purchasingDocumentIdsResponse.BpoNumbers.IndexOf(selectedEntity.ReferenceNumber)];

        //                if (selectedEntity.ReferenceNumber[0].ToString().ToUpper() == "P")
        //                    expectedDocumentId = this.purchasingDocumentIdsResponse.PoIds[this.purchasingDocumentIdsResponse.PoNumbers.IndexOf(selectedEntity.ReferenceNumber)];

        //                Assert.AreEqual(expectedDocumentId, selectedEntity.DocumentId);
        //            }
        //        }

        //        if (encFyrTransaction != null && encFyrTransaction.EncReqEntityAssociation != null)
        //        {
        //            encTransactionCount = encFyrTransaction.EncReqEntityAssociation.Where(x => (x.EncReqAmtAssocMember ?? 0) != 0).Count();

        //            // Make sure each ENC transaction is represented in the domain if there are transactions associated with this GL number
        //            foreach (var encTransaction in encFyrTransaction.EncReqEntityAssociation)
        //            {
        //                // Since zero dollar encumbrance and requisition transactions are removed we only want to perform a check if
        //                // the transaction amount has non-zero amount.
        //                if (encTransaction.EncReqAmtAssocMember > 0)
        //                {
        //                    var selectedEntity = glAccount.Transactions.FirstOrDefault(x =>
        //                        x.Amount == (encTransaction.EncReqAmtAssocMember ?? 0)
        //                        && x.Description == encTransaction.EncReqVendorAssocMember
        //                        && x.GlAccount == encFyrTransaction.Recordkey
        //                        && x.GlTransactionType == GlTransactionType.Requisition
        //                        && x.Id == encTransaction.EncReqIdAssocMember
        //                        && x.ReferenceNumber == encTransaction.EncReqNoAssocMember
        //                        && x.TransactionDate == encTransaction.EncReqDateAssocMember);
        //                    Assert.IsNotNull(selectedEntity);

        //                    // Make sure the selected entity has the correct document ID
        //                    string expectedDocumentId = null;
        //                    expectedDocumentId = this.purchasingDocumentIdsResponse.ReqIds[this.purchasingDocumentIdsResponse.ReqNumbers.IndexOf(selectedEntity.ReferenceNumber)];

        //                    Assert.AreEqual(expectedDocumentId, selectedEntity.DocumentId);
        //                }
        //            }
        //        }

        //        // Lastly, make sure the GL account domain entity has the correct number of transactions.
        //        Assert.AreEqual(glaTransactionCount + encTransactionCount, glAccount.Transactions.Count);
        //    }
        //}

        #region Helper Methods
        private async Task<bool> Helper_InitializeParameters(string costCenterId = "", int yearSubtractor = 0)
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_costCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            this.param3_SelectedCostCenterId = costCenterId;
            this.param4_FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday - yearSubtractor;

            this.testRepositoryDescriptions = testGlConfigurationRepository.GetComponentDescriptions().ToList();

            return true;
        }

        private void Helper_InitializeGlAcctsDataContracts(bool umbrellasHaveDirectExpenses = true)
        {
            var seedUmbrellaAccounts = testGlNumberRepository.WithLocation("UM").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedUmbrellaAccounts, GlBudgetPoolType.Umbrella, null, umbrellasHaveDirectExpenses);

            // Populate the poolee accounts for each umbrella
            foreach (var seedUmbrella in seedUmbrellaAccounts)
            {
                // Get the umbrella ID
                var component = testGlNumberRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
                var umbrellaId = seedUmbrella.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));

                // Get the poolees for this umbrella
                var seedPoolees = testGlNumberRepository.WithLocationSubclass("P").WithUnitSubclass("AJK").WithFunction(umbrellaId).GetFilteredGlNumbers();
                PopulateGlAccountDataContracts(seedPoolees, GlBudgetPoolType.Poolee, seedUmbrella);
            }

            // Populate the non-pooled accounts
            var seedNonPooledAccounts = testGlNumberRepository.WithLocation("NP").WithUnitSubclass("AJK").GetFilteredGlNumbers();
            PopulateGlAccountDataContracts(seedNonPooledAccounts, GlBudgetPoolType.None, null);
        }

        private decimal Helper_CalculateBudgetForUmbrella(List<GlAcctsMemos> amounts)
        {
            var budgetAmount = amounts.Sum(x => x.FaBudgetMemoAssocMember.Value) + amounts.Sum(x => x.FaBudgetPostedAssocMember.Value);
            return budgetAmount;
        }

        private decimal Helper_CalculateBudgetForNonUmbrella(List<GlAcctsMemos> amounts)
        {
            var budgetAmount = amounts.Sum(x => x.GlBudgetMemosAssocMember.Value) + amounts.Sum(x => x.GlBudgetPostedAssocMember.Value);
            return budgetAmount;
        }

        private decimal Helper_CalculateActualsForUmbrella(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.FaActualMemoAssocMember.Value) + amounts.Sum(x => x.FaActualPostedAssocMember.Value);
            return actualsAmount;
        }

        private decimal Helper_CalculateActualsForNonUmbrella(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.GlActualMemosAssocMember.Value) + amounts.Sum(x => x.GlActualPostedAssocMember.Value);
            return actualsAmount;
        }

        private decimal Helper_CalculateEncumbrancesForUmbrella(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.FaEncumbranceMemoAssocMember.Value) + amounts.Sum(x => x.FaEncumbrancePostedAssocMember.Value)
                + amounts.Sum(x => x.FaRequisitionMemoAssocMember.Value);
            return actualsAmount;
        }

        private decimal Helper_CalculateEncumbrancesForNonUmbrella(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.GlEncumbranceMemosAssocMember.Value) + amounts.Sum(x => x.GlEncumbrancePostedAssocMember.Value)
                + amounts.Sum(x => x.GlRequisitionMemosAssocMember.Value);
            return actualsAmount;
        }
        #endregion

        #region CostCenterHelper Class and Methods
        private void BuildCostCenterHelpersForOpenYear()
        {
            // Sort each of the GL.ACCTS data contracts into the correct cost center bucket.
            foreach (var dataContract in glAcctsDataContracts.Where(x => x != null).ToList())
            {
                var amounts = dataContract.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());
                var result = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_costCenterStructure);
                var costCenterId = result.Item1;
                var costCenterName = result.Item2;
                var subtotalId = result.Item3;

                var selectedCostCenter = costCenterHelpers.FirstOrDefault(x => x.Id == costCenterId);
                if (selectedCostCenter != null)
                {
                    // Try to find the subtotal for this GL account.
                    var selectedSubtotal = selectedCostCenter.Subtotals.FirstOrDefault(x => x.Id == subtotalId);
                    if (selectedSubtotal != null)
                    {
                        // If the GL number belongs in a pool then create a budget pool helper and stick the amounts data into the appropriate place in the pool.
                        // Otherwise, insert the amounts data into the non-pooled list.
                        if (amounts.GlPooledTypeAssocMember == "U" || amounts.GlPooledTypeAssocMember == "P")
                        {
                            // Only create a new pool if the umbrella GL account isn't already associated to one.
                            var selectedPool = selectedSubtotal.BudgetPools.FirstOrDefault(x => x.UmbrellaReference == amounts.GlBudgetLinkageAssocMember);
                            if (selectedPool != null)
                            {
                                // Set the umbrella
                                if (amounts.GlPooledTypeAssocMember == "U")
                                {
                                    selectedPool.Umbrella = new GlAccountAmountsHelper(dataContract.Recordkey, amounts);
                                }

                                // Add the poolee to the list of poolees
                                if (amounts.GlPooledTypeAssocMember == "P")
                                {
                                    selectedPool.Poolees.Add(new GlAccountAmountsHelper(dataContract.Recordkey, amounts));
                                }
                            }
                            else
                            {
                                // Create the budget pool
                                var budgetPool = CreateBudgetPoolHelper(dataContract.Recordkey, amounts);

                                // Lastly, add the budget pool to the list of pools for the subtotal.
                                selectedSubtotal.AddBudgetPoolHelper(budgetPool);
                            }
                        }
                        else
                        {
                            selectedSubtotal.AddNonPooledAmountsAssociation(dataContract.Recordkey, amounts);
                        }
                    }
                    else
                    {
                        // Create a new subtotal
                        var subtotal = new SubtotalHelper(subtotalId, this.param4_FiscalYear.ToString());

                        // If the GL number belongs in a pool then create a budget pool helper and stick the amounts data into the appropriate place in the pool.
                        // Otherwise, insert the amounts data into the non-pooled list.
                        if (amounts.GlPooledTypeAssocMember == "U" || amounts.GlPooledTypeAssocMember == "P")
                        {
                            // Create the budget pool
                            var budgetPool = CreateBudgetPoolHelper(dataContract.Recordkey, amounts);

                            // Lastly, add the budget pool to the list of pools for the subtotal.
                            subtotal.AddBudgetPoolHelper(budgetPool);
                        }
                        else
                        {
                            subtotal.AddNonPooledAmountsAssociation(dataContract.Recordkey, amounts);
                        }

                        selectedCostCenter.AddSubtotal(subtotal);
                    }
                }
                else
                {
                    // Create the subtotal.
                    var subtotal = new SubtotalHelper(subtotalId, this.param4_FiscalYear.ToString());

                    // If the GL number belongs in a pool then create a budget pool helper and stick the amounts data into the appropriate place in the pool.
                    // Otherwise, insert the amounts data into the non-pooled list.
                    if (amounts.GlPooledTypeAssocMember == "U" || amounts.GlPooledTypeAssocMember == "P")
                    {
                        // Create the budget pool
                        var budgetPool = CreateBudgetPoolHelper(dataContract.Recordkey, amounts);

                        // Lastly, add the budget pool to the list of pools for the subtotal.
                        subtotal.AddBudgetPoolHelper(budgetPool);
                    }
                    else
                    {
                        subtotal.AddNonPooledAmountsAssociation(dataContract.Recordkey, amounts);
                    }

                    var costCenter = new CostCenterHelper(costCenterId, costCenterName);
                    costCenter.AddSubtotal(subtotal);
                    costCenter.UnitId = dataContract.Recordkey.Substring(this.param2_costCenterStructure.Unit.StartPosition, Convert.ToInt32(this.param2_costCenterStructure.Unit.ComponentLength));
                    costCenterHelpers.Add(costCenter);
                }
            }
        }

        private void BuildCostCenterHelpersForClosedYear()
        {
            // Sort each of the GL.ACCTS data contracts into the correct cost center bucket.
            foreach (var dataContract in glsFyrDataContracts.Where(x => x != null).ToList())
            {
                var result = CalculateCostCenterIdNameSubtotalId(dataContract.Recordkey, this.param2_costCenterStructure);
                var costCenterId = result.Item1;
                var costCenterName = result.Item2;
                var subtotalId = result.Item3;

                var selectedCostCenter = costCenterHelpersGls.FirstOrDefault(x => x.Id == costCenterId);
                if (selectedCostCenter != null)
                {
                    // Try to find the subtotal for this GL account.
                    var selectedSubtotal = selectedCostCenter.Subtotals.FirstOrDefault(x => x.Id == subtotalId);
                    if (selectedSubtotal != null)
                    {
                        // If the GL number belongs in a pool then create a budget pool helper and stick the amounts data into the appropriate place in the pool.
                        // Otherwise, insert the amounts data into the non-pooled list.
                        if (dataContract.GlsPooledType == "U" || dataContract.GlsPooledType == "P")
                        {
                            // Only create a new pool if the umbrella GL account isn't already associated to one.
                            var selectedPool = selectedSubtotal.BudgetPools.FirstOrDefault(x => x.UmbrellaReference == dataContract.GlsBudgetLinkage);
                            if (selectedPool != null)
                            {
                                // Set the umbrella
                                if (dataContract.GlsPooledType == "U")
                                {
                                    selectedPool.Umbrella = dataContract;
                                }

                                // Add the poolee to the list of poolees
                                if (dataContract.GlsPooledType == "P")
                                {
                                    selectedPool.Poolees.Add(dataContract);
                                }
                            }
                            else
                            {
                                // Create the budget pool
                                var budgetPool = new BudgetPoolHelperClosedYear(dataContract.GlsBudgetLinkage);

                                // Set the umbrella
                                if (dataContract.GlsPooledType == "U")
                                {
                                    budgetPool.Umbrella = dataContract;
                                }

                                // Add the poolee to the list of poolees
                                if (dataContract.GlsPooledType == "P")
                                {
                                    budgetPool.Poolees.Add(dataContract);
                                }

                                // Lastly, add the budget pool to the list of pools for the subtotal.
                                selectedSubtotal.AddBudgetPoolHelper(budgetPool);
                            }
                        }
                        else
                        {
                            selectedSubtotal.AddNonPooledGlsContract(dataContract);
                        }
                    }
                    else
                    {
                        // Create a new subtotal
                        var subtotal = new SubtotalHelperClosedYear(subtotalId, this.param4_FiscalYear.ToString());

                        // If the GL number belongs in a pool then create a budget pool helper and stick the amounts data into the appropriate place in the pool.
                        // Otherwise, insert the amounts data into the non-pooled list.
                        if (dataContract.GlsPooledType == "U" || dataContract.GlsPooledType == "P")
                        {
                            // Create the budget pool
                            var budgetPool = new BudgetPoolHelperClosedYear(dataContract.GlsBudgetLinkage);

                            // Set the umbrella
                            if (dataContract.GlsPooledType == "U")
                            {
                                budgetPool.Umbrella = dataContract;
                            }

                            // Add the poolee to the list of poolees
                            if (dataContract.GlsPooledType == "P")
                            {
                                budgetPool.Poolees.Add(dataContract);
                            }

                            // Lastly, add the budget pool to the list of pools for the subtotal.
                            subtotal.AddBudgetPoolHelper(budgetPool);
                        }
                        else
                        {
                            subtotal.AddNonPooledGlsContract(dataContract);
                        }

                        selectedCostCenter.AddSubtotal(subtotal);
                    }
                }
                else
                {
                    // Create the subtotal.
                    var subtotal = new SubtotalHelperClosedYear(subtotalId, this.param4_FiscalYear.ToString());

                    // If the GL number belongs in a pool then create a budget pool helper and stick the amounts data into the appropriate place in the pool.
                    // Otherwise, insert the amounts data into the non-pooled list.
                    if (dataContract.GlsPooledType == "U" || dataContract.GlsPooledType == "P")
                    {
                        // Create the budget pool
                        var budgetPool = new BudgetPoolHelperClosedYear(dataContract.GlsBudgetLinkage);

                        // Set the umbrella
                        if (dataContract.GlsPooledType == "U")
                        {
                            budgetPool.Umbrella = dataContract;
                        }

                        // Add the poolee to the list of poolees
                        if (dataContract.GlsPooledType == "P")
                        {
                            budgetPool.Poolees.Add(dataContract);
                        }

                        // Lastly, add the budget pool to the list of pools for the subtotal.
                        subtotal.AddBudgetPoolHelper(budgetPool);
                    }
                    else
                    {
                        subtotal.AddNonPooledGlsContract(dataContract);
                    }

                    var costCenter = new CostCenterHelperClosedYear(costCenterId, costCenterName);
                    costCenter.AddSubtotal(subtotal);
                    costCenter.UnitId = dataContract.Recordkey.Substring(this.param2_costCenterStructure.Unit.StartPosition, Convert.ToInt32(this.param2_costCenterStructure.Unit.ComponentLength));
                    costCenterHelpersGls.Add(costCenter);
                }
            }

            // Now process all ENC.FYR records
            foreach (var dataContract in this.encFyrDataContracts.Where(x => x != null).ToList())
            {
                // Try to find the requisition GL number in the umbrellas and poolees
                bool glNumberFound = false;
                var allPools = costCenterHelpersGls.SelectMany(x => x.Subtotals).SelectMany(x => x.BudgetPools).ToList();
                foreach (var budgetPool in allPools)
                {
                    if (budgetPool.UmbrellaReference == dataContract.Recordkey)
                    {
                        budgetPool.UmbrellaRequisitions = dataContract;
                        glNumberFound = true;
                    }

                    if (!glNumberFound)
                    {
                        var selectedPoolee = budgetPool.Poolees.FirstOrDefault(x => x.Recordkey == dataContract.Recordkey);
                        if (selectedPoolee != null)
                        {
                            budgetPool.Requisitions.Add(dataContract);
                        }
                    }
                }

                // Try to find the requisition GL number in the non-pooled accounts
                if (!glNumberFound)
                {
                    var allSubtotals = costCenterHelpersGls.SelectMany(x => x.Subtotals).ToList();
                    foreach (var subtotal in allSubtotals)
                    {
                        var selectedNonPooledGls = subtotal.NonPooledGls.FirstOrDefault(x => x.Recordkey == dataContract.Recordkey);
                        if (selectedNonPooledGls != null)
                        {
                            subtotal.NonPooledEnc.Add(dataContract);
                        }
                    }
                }
            }
        }

        private BudgetPoolHelper CreateBudgetPoolHelper(string glNumber, GlAcctsMemos amounts)
        {
            var budgetPool = new BudgetPoolHelper(amounts.GlBudgetLinkageAssocMember);
            if (amounts.GlPooledTypeAssocMember == "U")
            {
                budgetPool.Umbrella = new GlAccountAmountsHelper(glNumber, amounts);
            }

            if (amounts.GlPooledTypeAssocMember == "P")
            {
                budgetPool.Poolees.Add(new GlAccountAmountsHelper(glNumber, amounts));
            }

            return budgetPool;
        }

        private Tuple<string, string, string> CalculateCostCenterIdNameSubtotalId(string glNumber, CostCenterStructure costCenterStructure)
        {
            // Determine which component description objects will be needed to calculate the cost center Id and Name.
            var costCenterId = string.Empty;
            var costCenterName = string.Empty;
            var subtotalId = string.Empty;
            glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();

            if (!string.IsNullOrEmpty(glNumber))
            {
                foreach (var component in costCenterStructure.CostCenterComponents)
                {
                    if (component != null)
                    {
                        var componentId = glNumber.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));
                        costCenterId += componentId;

                        if (component.IsPartOfDescription && glComponentsForCostCenter.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).Count() == 0)
                        {
                            var componentDescription = new GeneralLedgerComponentDescription(componentId, component.ComponentType);

                            var selectedComponent = testRepositoryDescriptions.FirstOrDefault(x => x.Id == componentId && x.ComponentType == component.ComponentType);

                            if (selectedComponent != null)
                            {
                                componentDescription.Description = selectedComponent.Description;
                            }

                            glComponentsForCostCenter.Add(componentDescription);
                        }
                    }
                }

                if (glComponentsForCostCenter.Any())
                {
                    costCenterName = String.Join(" : ", glComponentsForCostCenter
                    .Where(x => !string.IsNullOrEmpty(x.Description)).Select(x => x.Description));

                    if (string.IsNullOrEmpty(costCenterName))
                        costCenterName = "No cost center description available.";
                }

                subtotalId = glNumber.Substring(costCenterStructure.CostCenterSubtotal.StartPosition, Convert.ToInt32(costCenterStructure.CostCenterSubtotal.ComponentLength));
            }

            return new Tuple<string, string, string>(costCenterId, costCenterName, subtotalId);
        }

        private class CostCenterHelper
        {
            public string Id { get { return id; } }
            private string id;

            public string Name { get { return name; } }
            private string name;

            public List<SubtotalHelper> Subtotals { get { return subtotals; } }
            private List<SubtotalHelper> subtotals = new List<SubtotalHelper>();

            public string UnitId { get; set; }

            public CostCenterHelper(string id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public void AddSubtotal(SubtotalHelper subtotal)
            {
                this.subtotals.Add(subtotal);
            }
        }

        private class SubtotalHelper
        {
            public string Id { get { return id; } }
            private string id;

            public List<BudgetPoolHelper> BudgetPools { get { return budgetPools; } }
            private List<BudgetPoolHelper> budgetPools = new List<BudgetPoolHelper>();

            public List<GlAccountAmountsHelper> NonPooledAmounts { get { return this.nonPooledAmounts; } }
            private List<GlAccountAmountsHelper> nonPooledAmounts = new List<GlAccountAmountsHelper>();

            private string fiscalYear;

            public SubtotalHelper(string id, string fiscalYear)
            {
                this.id = id;
                this.fiscalYear = fiscalYear;
            }

            public void AddNonPooledAmountsAssociation(string glNumber, GlAcctsMemos amounts)
            {
                nonPooledAmounts.Add(new GlAccountAmountsHelper(glNumber, amounts));
            }

            public void AddBudgetPoolHelper(BudgetPoolHelper budgetPool)
            {
                this.budgetPools.Add(budgetPool);
            }
        }

        private class BudgetPoolHelper
        {
            public string UmbrellaReference { get { return umbrellaReference; } }
            private string umbrellaReference;

            public GlAccountAmountsHelper Umbrella { get; set; }
            public List<GlAccountAmountsHelper> Poolees { get; set; }

            public BudgetPoolHelper(string umbrellaReference)
            {
                this.umbrellaReference = umbrellaReference;
                this.Poolees = new List<GlAccountAmountsHelper>();
            }
        }

        private class GlAccountAmountsHelper
        {
            public string GlNumber { get; set; }
            public GlAcctsMemos FiscalYearAmounts { get; set; }

            public GlAccountAmountsHelper(string glNumber, GlAcctsMemos amounts)
            {
                GlNumber = glNumber;
                FiscalYearAmounts = amounts;
            }
        }

        private class CostCenterHelperClosedYear
        {
            public string Id { get { return id; } }
            private string id;

            public string Name { get { return name; } }
            private string name;

            public List<SubtotalHelperClosedYear> Subtotals { get { return subtotals; } }
            private List<SubtotalHelperClosedYear> subtotals = new List<SubtotalHelperClosedYear>();

            public string UnitId { get; set; }

            public CostCenterHelperClosedYear(string id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public void AddSubtotal(SubtotalHelperClosedYear subtotal)
            {
                this.subtotals.Add(subtotal);
            }
        }

        private class SubtotalHelperClosedYear
        {
            public string Id { get { return id; } }
            private string id;

            public List<BudgetPoolHelperClosedYear> BudgetPools { get { return budgetPools; } }
            private List<BudgetPoolHelperClosedYear> budgetPools = new List<BudgetPoolHelperClosedYear>();

            public List<GlsFyr> NonPooledGls { get { return this.nonPooledGls; } }
            private List<GlsFyr> nonPooledGls = new List<GlsFyr>();

            public List<EncFyr> NonPooledEnc { get { return this.nonPooledEnc; } }
            private List<EncFyr> nonPooledEnc = new List<EncFyr>();

            private string fiscalYear;

            public SubtotalHelperClosedYear(string id, string fiscalYear)
            {
                this.id = id;
                this.fiscalYear = fiscalYear;
            }

            public void AddNonPooledGlsContract(GlsFyr dataContract)
            {
                nonPooledGls.Add(dataContract);
            }

            public void AddNonPooledEncContract(EncFyr dataContract)
            {
                nonPooledEnc.Add(dataContract);
            }

            public void AddBudgetPoolHelper(BudgetPoolHelperClosedYear budgetPool)
            {
                this.budgetPools.Add(budgetPool);
            }
        }

        private class BudgetPoolHelperClosedYear
        {
            public string UmbrellaReference { get { return umbrellaReference; } }
            private string umbrellaReference;

            public GlsFyr Umbrella { get; set; }
            public EncFyr UmbrellaRequisitions { get; set; }

            public List<GlsFyr> Poolees { get; set; }
            public List<EncFyr> Requisitions { get; set; }

            public BudgetPoolHelperClosedYear(string umbrellaReference)
            {
                this.umbrellaReference = umbrellaReference;
                this.Poolees = new List<GlsFyr>();
                this.Requisitions = new List<EncFyr>();
            }
        }
        #endregion

        #region Private methods
        public async Task<IEnumerable<CostCenter>> RealRepository_GetCostCentersAsync()
        {
            return await actualRepository.GetCostCentersAsync(this.param1_GlUser,
                this.param2_costCenterStructure,
                this.param6_glClassConfiguration,
                this.param3_SelectedCostCenterId,
                this.param4_FiscalYear.ToString(),
                this.param5_costCenterQueryCriteria,
                "0000001");
        }

        public async Task<IEnumerable<CostCenter>> RealRepository_GetFilteredCostCentersAsync()
        {
            return await actualRepository.GetCostCentersAsync(this.param1_GlUser,
                this.param2_costCenterStructure,
                this.param6_glClassConfiguration,
                this.param3_SelectedCostCenterId,
                this.param4_FiscalYear.ToString(),
                this.param5_costCenterQueryCriteria,
                "0000001");
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

        private void SetupCtxResponseObject()
        {
            // Get the reference numbers for the requisitions, POs and BPOs.
            var reqNumbers = testCostCenterRepository.EncFyrDataContracts.SelectMany(x => x.EncReqEntityAssociation)
                .Where(x => !string.IsNullOrEmpty(x.EncReqNoAssocMember)).Select(x => x.EncReqNoAssocMember).ToList();
            var poNumbers = testCostCenterRepository.GlaFyrDataContracts.Where(x => !string.IsNullOrEmpty(x.GlaRefNo) && x.GlaRefNo[0].ToString().ToUpper() == "P")
                .Select(x => x.GlaRefNo).ToList();
            var bpoNumbers = testCostCenterRepository.GlaFyrDataContracts.Where(x => !string.IsNullOrEmpty(x.GlaRefNo) && x.GlaRefNo[0].ToString().ToUpper() == "B")
                .Select(x => x.GlaRefNo).ToList();

            // Generate unique document IDs for each reference number
            int id = 1;
            var reqIds = reqNumbers.Select(x => (id++).ToString()).ToList();
            var poIds = poNumbers.Select(x => (id++).ToString()).ToList();
            var bpoids = bpoNumbers.Select(x => (id++).ToString()).ToList();

            // Initialize the CTX request and response.
            this.purchasingDocumentIdsRequest = new GetPurchasingDocumentIdsRequest()
            {
                ReqNumbers = reqNumbers,
                PoNumbers = poNumbers,
                BpoNumbers = bpoNumbers
            };
            this.purchasingDocumentIdsResponse = new GetPurchasingDocumentIdsResponse()
            {
                ReqNumbers = reqNumbers,
                PoNumbers = poNumbers,
                BpoNumbers = bpoNumbers,
                ReqIds = reqIds,
                PoIds = poIds,
                BpoIds = bpoids
            };
        }

        private void PopulateGlAccountDataContracts(List<string> glAccounts, GlBudgetPoolType poolType, string umbrellaReference,
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

                var dataContract = PopulateSingleGlAcctsDataContract(glAccount, dataContractUmbrellaReference, dataContractPoolType, umbrellaHasDirectExpenses, amountsAreZero, amountsAreNull);
                this.glAcctsDataContracts.Add(dataContract);
            }
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
            // Default the amounts for the GL.ACCTS data contracts
            decimal? bAlocDebitsYtd = 10000m,
                bAlocCreditsYtd = 500m,
                eOpenBal = 150m,
                encumbrancesYtd = 50m,
                encumbrancesRelievedYtd = 75m,
                debitsYtd = 500m,
                creditsYtd = 300m;

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
            }

            // Zero out the numbers if the parameter says so...
            if (amountsAreZero)
            {
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
            }

            // Null out the numbers if the parameter says so...
            if (amountsAreNull)
            {
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
            }

            var dataContract = new GlsFyr()
            {
                Recordkey = glAccount,
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
                GlsPooledType = dataContractPoolType
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
                EncReqAmt = encReqAmt
            };

            return dataContract;
        }

        private void PopulateGlAcctsDataContracts()
        {
            // Initialize the pooled indicator. Use this variable to determine whether a GL account should be
            // marked as an umbrella, poolee, or non-pooled GL account.
            //   1: Umbrella
            //   2: Poolee
            //   3: Non-pooled
            int pooledIndicator = 1;
            string pooledType = "";
            string umbrellaReference = null;
            foreach (var glAccount in this.param1_GlUser.ExpenseAccounts)
            {
                // Default the amounts for the GL.ACCTS data contracts
                decimal? encumbrancePosted = 0m,
                    encumbranceMemos = 250m,
                    requisitionMemos = 375m,
                    actualPosted = 0m,
                    actualMemos = 500m;

                // Sum up the GLA transactions.
                foreach (var glaContract in testCostCenterRepository.GlaFyrDataContracts)
                {
                    var glaGlAccountNumber = glaContract.Recordkey.Split('*').First();

                    // Build the GlAccts data contract using the relevant transaction amounts.
                    if (glAccount == glaGlAccountNumber)
                    {
                        // Sum up the actuals transactions
                        if (glaContract.GlaSource == "PJ")
                        {
                            actualPosted += (glaContract.GlaDebit ?? 0) - (glaContract.GlaCredit ?? 0);
                        }

                        // Sum up the encumbrance transactions
                        if (glaContract.GlaSource == "EP")
                            encumbrancePosted += (glaContract.GlaDebit ?? 0) - (glaContract.GlaCredit ?? 0);
                    }
                }

                // Sum up the ENC transactions.
                foreach (var encContract in testCostCenterRepository.EncFyrDataContracts)
                {
                    if (glAccount == encContract.Recordkey)
                    {
                        encumbrancePosted += encContract.EncReqAmt.Where(x => x.HasValue).Sum(x => x.Value);
                    }
                }

                // Only populate pooled information for GL User 32
                if (this.param1_GlUser.Id == "0000032")
                {
                    // Get the 2-digit umbrella code at the end of the GL account and the pooled type.
                    var umbrellaCode = glAccount.Substring(13, 2);
                    pooledType = glAccount.Substring(9, 1);

                    if (pooledType == "U")
                    {
                        // Save the current GL number as the umbrella reference
                        umbrellaReference = glAccount;

                        // Umbrella #2 should not be represented as a poolee.
                        if (umbrellaCode == "U2")
                        {
                            encumbrancePosted = 0m;
                            encumbranceMemos = 0m;
                            requisitionMemos = 0m;
                            actualPosted = 0m;
                            actualMemos = 0m;
                        }
                    }
                    else if (pooledType == "P")
                    {
                        // Find the GL account in the user's list that has that 
                        umbrellaReference = this.param1_GlUser.ExpenseAccounts.FirstOrDefault(x =>
                            x.Substring(9, 1) == "U" && x.Substring(13, 2) == umbrellaCode);
                    }
                    else
                    {
                        // Non-pooled account
                        pooledType = "";
                        umbrellaReference = "";
                    }
                }

                if (umbrellaReference == null)
                {
                    umbrellaReference = "";
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
                    GlBudgetPosted = new List<decimal?>() { 10000m, 9000m, 8000m },
                    GlBudgetMemos = new List<decimal?>() { 500m, 400m, 300m },
                    GlEncumbrancePosted = new List<decimal?>() { encumbrancePosted, encumbrancePosted, encumbrancePosted },
                    GlEncumbranceMemos = new List<decimal?>() { encumbranceMemos, encumbranceMemos, encumbranceMemos },
                    GlRequisitionMemos = new List<decimal?>() { requisitionMemos, requisitionMemos, requisitionMemos },
                    GlActualMemos = new List<decimal?>() { actualMemos, actualMemos, actualMemos },
                    GlActualPosted = new List<decimal?>() { actualPosted, actualPosted, actualPosted },
                    FaBudgetMemo = new List<decimal?>() { 1000m, 2000m, 3000m },
                    FaBudgetPosted = new List<decimal?>() { 100m, 200m, 300m },
                    FaEncumbranceMemo = new List<decimal?>() { 10m, 20m, 30m },
                    FaEncumbrancePosted = new List<decimal?>() { 10m, 20m, 30m },
                    FaRequisitionMemo = new List<decimal?>() { 10m, 20m, 30m },
                    FaActualPosted = new List<decimal?>() { 1m, 2m, 3m },
                    FaActualMemo = new List<decimal?>() { 1m, 2m, 3m },
                    GlPooledType = new List<string>() { pooledType, "", "" },
                    GlBudgetLinkage = new List<string>() { umbrellaReference, "", "" },
                };
                dataContract.buildAssociations();
                this.glAcctsDataContracts.Add(dataContract);

                // Update the pooled indicator so we only have one pool.
            }
        }

        private void BuildExpectedCostCentersForOpenYear()
        {
            // Slim down the list of GL.ACCTS data contracts if we're looking at a single cost center.
            var dataContracts = new Collection<GlAccts>();

            if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
            {
                foreach (var dataContract in this.glAcctsDataContracts)
                {
                    // Determine the cost center ID for this expense GL account
                    var expenseGlAccountCostCenterId = string.Empty;
                    foreach (var component in this.param2_costCenterStructure.CostCenterComponents)
                    {
                        if (component != null)
                        {
                            var componentId = dataContract.Recordkey.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));
                            expenseGlAccountCostCenterId += componentId;
                        }
                    }

                    // If the GL account cost center ID matches the selected one,
                    // add the GL account to the list of GL accounts to process.
                    if (expenseGlAccountCostCenterId == this.param3_SelectedCostCenterId)
                    {
                        dataContracts.Add(dataContract);
                    }
                }
            }
            else
            {
                dataContracts = this.glAcctsDataContracts;
            }

            var baseQuery = dataContracts.Where(x => x != null && x.MemosEntityAssociation != null
                    && x.MemosEntityAssociation.Where(y => y != null && y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString()).Any());
            var umbrellaAccounts = baseQuery.Where(x => x.MemosEntityAssociation.Where(y => y.GlPooledTypeAssocMember.ToUpper() == "U").Any()).ToList();
            var pooleeAccounts = baseQuery.Where(x => x.MemosEntityAssociation.Where(y => y.GlPooledTypeAssocMember.ToUpper() == "P").Any()).ToList();
            var nonPooledAccounts = dataContracts.Where(x => x != null && x.MemosEntityAssociation != null && x.MemosEntityAssociation
                .Where(y => y != null && y.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString() && string.IsNullOrEmpty(y.GlPooledTypeAssocMember)).Any()).ToList();

            // Create budget pools for each umbrella
            var budgetPoolEntities = new List<GlBudgetPool>();
            foreach (var umbrella in umbrellaAccounts)
            {
                var amounts = umbrella.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());

                if (!string.IsNullOrEmpty(umbrella.Recordkey))
                {
                    var umbrellaGlAccount = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Umbrella);
                    umbrellaGlAccount.BudgetAmount = amounts.FaBudgetMemoAssocMember.HasValue ? amounts.FaBudgetMemoAssocMember.Value : 0m;
                    umbrellaGlAccount.BudgetAmount += amounts.FaBudgetPostedAssocMember.HasValue ? amounts.FaBudgetPostedAssocMember.Value : 0m;
                    umbrellaGlAccount.ActualAmount = amounts.FaActualMemoAssocMember.HasValue ? amounts.FaActualMemoAssocMember.Value : 0m;
                    umbrellaGlAccount.ActualAmount += amounts.FaActualPostedAssocMember.HasValue ? amounts.FaActualPostedAssocMember.Value : 0m;
                    umbrellaGlAccount.EncumbranceAmount = amounts.FaEncumbranceMemoAssocMember.HasValue ? amounts.FaEncumbranceMemoAssocMember.Value : 0m;
                    umbrellaGlAccount.EncumbranceAmount += amounts.FaEncumbrancePostedAssocMember.HasValue ? amounts.FaEncumbrancePostedAssocMember.Value : 0m;
                    umbrellaGlAccount.EncumbranceAmount += amounts.FaRequisitionMemoAssocMember.HasValue ? amounts.FaRequisitionMemoAssocMember.Value : 0m;

                    // Populate the GL account description
                    if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
                    {
                        int index = glAccountsDescriptionResponse.GlAccountIds.IndexOf(umbrellaGlAccount.GlAccountNumber);

                        if (index < glAccountsDescriptionResponse.GlDescriptions.Count)
                        {
                            umbrellaGlAccount.GlAccountDescription = glAccountsDescriptionResponse.GlDescriptions[index];
                        }
                    }

                    var budgetPool = new GlBudgetPool(umbrellaGlAccount);
                    budgetPool.IsUmbrellaVisible = true;

                    // Add a poolee to represent direct expenses for the umbrella
                    if ((amounts.GlEncumbrancePostedAssocMember.HasValue && amounts.GlEncumbrancePostedAssocMember.Value != 0)
                        || (amounts.GlEncumbranceMemosAssocMember.HasValue && amounts.GlEncumbranceMemosAssocMember.Value != 0)
                        || (amounts.GlRequisitionMemosAssocMember.HasValue && amounts.GlRequisitionMemosAssocMember.Value != 0)
                        || (amounts.GlActualPostedAssocMember.HasValue && amounts.GlActualPostedAssocMember.Value != 0)
                        || (amounts.GlActualMemosAssocMember.HasValue && amounts.GlActualMemosAssocMember.Value != 0))
                    {
                        var umbrellaPoolee = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Poolee);
                        umbrellaPoolee.ActualAmount = amounts.GlActualMemosAssocMember.HasValue ? amounts.GlActualMemosAssocMember.Value : 0m;
                        umbrellaPoolee.ActualAmount += amounts.GlActualPostedAssocMember.HasValue ? amounts.GlActualPostedAssocMember.Value : 0m;
                        umbrellaPoolee.EncumbranceAmount = amounts.GlEncumbranceMemosAssocMember.HasValue ? amounts.GlEncumbranceMemosAssocMember.Value : 0m;
                        umbrellaPoolee.EncumbranceAmount += amounts.GlEncumbrancePostedAssocMember.HasValue ? amounts.GlEncumbrancePostedAssocMember.Value : 0m;
                        umbrellaPoolee.EncumbranceAmount += amounts.GlRequisitionMemosAssocMember.HasValue ? amounts.GlRequisitionMemosAssocMember.Value : 0m;

                        // Populate the GL account description
                        if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
                        {
                            int index = glAccountsDescriptionResponse.GlAccountIds.IndexOf(umbrellaPoolee.GlAccountNumber);

                            if (index < glAccountsDescriptionResponse.GlDescriptions.Count)
                            {
                                umbrellaPoolee.GlAccountDescription = glAccountsDescriptionResponse.GlDescriptions[index];
                            }
                        }

                        budgetPool.AddPoolee(umbrellaPoolee);
                    }

                    budgetPoolEntities.Add(budgetPool);
                }
            }

            // Add each poolee to the appropriate umbrella
            foreach (var poolee in pooleeAccounts)
            {
                var amounts = poolee.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());

                if (!string.IsNullOrEmpty(poolee.Recordkey))
                {
                    var pooleeEntity = new CostCenterGlAccount(poolee.Recordkey, GlBudgetPoolType.Poolee);
                    pooleeEntity.BudgetAmount = amounts.GlBudgetMemosAssocMember.HasValue ? amounts.GlBudgetMemosAssocMember.Value : 0m;
                    pooleeEntity.BudgetAmount += amounts.GlBudgetPostedAssocMember.HasValue ? amounts.GlBudgetPostedAssocMember.Value : 0m;
                    pooleeEntity.ActualAmount = amounts.GlActualMemosAssocMember.HasValue ? amounts.GlActualMemosAssocMember.Value : 0m;
                    pooleeEntity.ActualAmount += amounts.GlActualPostedAssocMember.HasValue ? amounts.GlActualPostedAssocMember.Value : 0m;
                    pooleeEntity.EncumbranceAmount = amounts.GlEncumbranceMemosAssocMember.HasValue ? amounts.GlEncumbranceMemosAssocMember.Value : 0m;
                    pooleeEntity.EncumbranceAmount += amounts.GlEncumbrancePostedAssocMember.HasValue ? amounts.GlEncumbrancePostedAssocMember.Value : 0m;
                    pooleeEntity.EncumbranceAmount += amounts.GlRequisitionMemosAssocMember.HasValue ? amounts.GlRequisitionMemosAssocMember.Value : 0m;

                    // Populate the GL account description
                    if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
                    {
                        int index = glAccountsDescriptionResponse.GlAccountIds.IndexOf(pooleeEntity.GlAccountNumber);

                        if (index < glAccountsDescriptionResponse.GlDescriptions.Count)
                        {
                            pooleeEntity.GlAccountDescription = glAccountsDescriptionResponse.GlDescriptions[index];
                        }
                    }

                    var selectedBudgetPool = budgetPoolEntities.FirstOrDefault(x => x.Umbrella.GlAccountNumber == amounts.GlBudgetLinkageAssocMember);
                    if (selectedBudgetPool != null)
                    {
                        selectedBudgetPool.AddPoolee(pooleeEntity);
                    }
                    else
                    {
                        // Get the data contract for the umbrella in question.
                        var umbrellaDataContract = glAcctsNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == amounts.GlBudgetLinkageAssocMember);

                        if (umbrellaDataContract != null)
                        {
                            var umbrellaAmounts = umbrellaDataContract.MemosEntityAssociation.FirstOrDefault(x => x != null && x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());

                            if (umbrellaAmounts != null)
                            {
                                // Create the umbrella account domain entity.
                                var umbrellaGlAccount = new CostCenterGlAccount(umbrellaDataContract.Recordkey, GlBudgetPoolType.Umbrella);
                                umbrellaGlAccount.BudgetAmount = umbrellaAmounts.FaBudgetMemoAssocMember.HasValue ? umbrellaAmounts.FaBudgetMemoAssocMember.Value : 0m;
                                umbrellaGlAccount.BudgetAmount += umbrellaAmounts.FaBudgetPostedAssocMember.HasValue ? umbrellaAmounts.FaBudgetPostedAssocMember.Value : 0m;
                                umbrellaGlAccount.ActualAmount = umbrellaAmounts.FaActualMemoAssocMember.HasValue ? umbrellaAmounts.FaActualMemoAssocMember.Value : 0m;
                                umbrellaGlAccount.ActualAmount += umbrellaAmounts.FaActualPostedAssocMember.HasValue ? umbrellaAmounts.FaActualPostedAssocMember.Value : 0m;
                                umbrellaGlAccount.EncumbranceAmount = umbrellaAmounts.FaEncumbranceMemoAssocMember.HasValue ? umbrellaAmounts.FaEncumbranceMemoAssocMember.Value : 0m;
                                umbrellaGlAccount.EncumbranceAmount += umbrellaAmounts.FaEncumbrancePostedAssocMember.HasValue ? umbrellaAmounts.FaEncumbrancePostedAssocMember.Value : 0m;
                                umbrellaGlAccount.EncumbranceAmount += umbrellaAmounts.FaRequisitionMemoAssocMember.HasValue ? umbrellaAmounts.FaRequisitionMemoAssocMember.Value : 0m;

                                // Populate the GL account description
                                if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
                                {
                                    int index = glAccountsDescriptionResponse.GlAccountIds.IndexOf(umbrellaGlAccount.GlAccountNumber);

                                    if (index < glAccountsDescriptionResponse.GlDescriptions.Count)
                                    {
                                        umbrellaGlAccount.GlAccountDescription = glAccountsDescriptionResponse.GlDescriptions[index];
                                    }
                                }

                                // Create the budget pool.
                                var budgetPool = new GlBudgetPool(umbrellaGlAccount);

                                //// Add a poolee to represent direct expenses for the umbrella
                                //if ((umbrellaAmounts.GlEncumbrancePostedAssocMember.HasValue && umbrellaAmounts.GlEncumbrancePostedAssocMember.Value != 0)
                                //    || (umbrellaAmounts.GlEncumbranceMemosAssocMember.HasValue && umbrellaAmounts.GlEncumbranceMemosAssocMember.Value != 0)
                                //    || (umbrellaAmounts.GlRequisitionMemosAssocMember.HasValue && umbrellaAmounts.GlRequisitionMemosAssocMember.Value != 0)
                                //    || (umbrellaAmounts.GlActualPostedAssocMember.HasValue && umbrellaAmounts.GlActualPostedAssocMember.Value != 0)
                                //    || (umbrellaAmounts.GlActualMemosAssocMember.HasValue && umbrellaAmounts.GlActualMemosAssocMember.Value != 0))
                                //{
                                //    var umbrellaPoolee = new CostCenterGlAccount(umbrellaDataContract.Recordkey, GlBudgetPoolType.Poolee);
                                //    umbrellaPoolee.ActualAmount = umbrellaAmounts.GlActualMemosAssocMember.HasValue ? umbrellaAmounts.GlActualMemosAssocMember.Value : 0m;
                                //    umbrellaPoolee.ActualAmount += umbrellaAmounts.GlActualPostedAssocMember.HasValue ? umbrellaAmounts.GlActualPostedAssocMember.Value : 0m;
                                //    umbrellaPoolee.EncumbranceAmount = umbrellaAmounts.GlEncumbranceMemosAssocMember.HasValue ? umbrellaAmounts.GlEncumbranceMemosAssocMember.Value : 0m;
                                //    umbrellaPoolee.EncumbranceAmount += umbrellaAmounts.GlEncumbrancePostedAssocMember.HasValue ? umbrellaAmounts.GlEncumbrancePostedAssocMember.Value : 0m;
                                //    umbrellaPoolee.EncumbranceAmount += umbrellaAmounts.GlRequisitionMemosAssocMember.HasValue ? umbrellaAmounts.GlRequisitionMemosAssocMember.Value : 0m;

                                //    // Populate the GL account description
                                //    if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
                                //    {
                                //        int index = glAccountsDescriptionResponse.GlAccountIds.IndexOf(umbrellaPoolee.GlAccountNumber);

                                //        if (index < glAccountsDescriptionResponse.GlDescriptions.Count)
                                //        {
                                //            umbrellaPoolee.GlAccountDescription = glAccountsDescriptionResponse.GlDescriptions[index];
                                //        }
                                //    }

                                //    budgetPool.AddPoolee(umbrellaPoolee);
                                //}

                                // Finally, add the new budget pool to the budget pool entities list.
                                budgetPoolEntities.Add(budgetPool);
                            }
                        }
                    }
                }
            }

            // Add each non-pooled account to the non pooled accounts list
            var nonPooledEntities = new List<CostCenterGlAccount>();
            foreach (var nonPooledAccount in nonPooledAccounts)
            {
                var amounts = nonPooledAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == this.param4_FiscalYear.ToString());

                var nonPooledEntity = new CostCenterGlAccount(nonPooledAccount.Recordkey, GlBudgetPoolType.None);
                nonPooledEntity.BudgetAmount = amounts.GlBudgetMemosAssocMember.HasValue ? amounts.GlBudgetMemosAssocMember.Value : 0m;
                nonPooledEntity.BudgetAmount += amounts.GlBudgetPostedAssocMember.HasValue ? amounts.GlBudgetPostedAssocMember.Value : 0m;
                nonPooledEntity.ActualAmount = amounts.GlActualMemosAssocMember.HasValue ? amounts.GlActualMemosAssocMember.Value : 0m;
                nonPooledEntity.ActualAmount += amounts.GlActualPostedAssocMember.HasValue ? amounts.GlActualPostedAssocMember.Value : 0m;
                nonPooledEntity.EncumbranceAmount = amounts.GlEncumbranceMemosAssocMember.HasValue ? amounts.GlEncumbranceMemosAssocMember.Value : 0m;
                nonPooledEntity.EncumbranceAmount += amounts.GlEncumbrancePostedAssocMember.HasValue ? amounts.GlEncumbrancePostedAssocMember.Value : 0m;
                nonPooledEntity.EncumbranceAmount += amounts.GlRequisitionMemosAssocMember.HasValue ? amounts.GlRequisitionMemosAssocMember.Value : 0m;

                // Populate the GL account description
                if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
                {
                    int index = glAccountsDescriptionResponse.GlAccountIds.IndexOf(nonPooledEntity.GlAccountNumber);

                    if (index < glAccountsDescriptionResponse.GlDescriptions.Count)
                    {
                        nonPooledEntity.GlAccountDescription = glAccountsDescriptionResponse.GlDescriptions[index];
                    }
                }

                nonPooledEntities.Add(nonPooledEntity);
            }

            // Insert the budget pools into the cost centers list.
            foreach (var budgetPool in budgetPoolEntities)
            {
                AddBudgetPoolToCostCentersList(budgetPool, this.param2_costCenterStructure);
            }

            // Insert the non-pooled accounts into the cost centers list
            foreach (var nonPooledAccount in nonPooledEntities)
            {
                AddNonPooledAccountToCostCenterList(nonPooledAccount, this.param2_costCenterStructure);
            }
        }

        private void BuildExpectedCostCentersForClosedYear()
        {
            var umbrellaAccounts = this.glsFyrDataContracts.Where(x => x.GlsPooledType.ToUpper() == "U").ToList();
            var pooleeAccounts = this.glsFyrDataContracts.Where(x => x.GlsPooledType.ToUpper() == "P").ToList();
            var nonPooledAccounts = this.glsFyrDataContracts.Where(x => string.IsNullOrEmpty(x.GlsPooledType)).ToList();

            // Create budget pools for each umbrella
            var budgetPoolEntities = new List<GlBudgetPool>();
            foreach (var umbrella in umbrellaAccounts)
            {
                var umbrellaGlAccount = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Umbrella);
                umbrellaGlAccount.BudgetAmount = umbrella.BAlocDebitsYtd.HasValue ? umbrella.BAlocDebitsYtd.Value : 0m;
                umbrellaGlAccount.BudgetAmount -= umbrella.BAlocCreditsYtd.HasValue ? umbrella.BAlocCreditsYtd.Value : 0m;
                foreach (var amount in umbrella.GlsFaMactuals)
                {
                    umbrellaGlAccount.ActualAmount += amount.HasValue ? amount.Value : 0m;
                }

                foreach (var amount in umbrella.GlsFaMencumbrances)
                {
                    umbrellaGlAccount.EncumbranceAmount += amount.HasValue ? amount.Value : 0m;
                }

                var budgetPool = new GlBudgetPool(umbrellaGlAccount);
                budgetPool.IsUmbrellaVisible = true;

                // Add a poolee to represent direct expenses for the umbrella
                if ((umbrella.EOpenBal.HasValue && umbrella.EOpenBal.Value != 0)
                    || (umbrella.EncumbrancesYtd.HasValue && umbrella.EncumbrancesYtd.Value != 0)
                    || (umbrella.EncumbrancesRelievedYtd.HasValue && umbrella.EncumbrancesRelievedYtd.Value != 0)
                    || (umbrella.DebitsYtd.HasValue && umbrella.DebitsYtd.Value != 0)
                    || (umbrella.CreditsYtd.HasValue && umbrella.CreditsYtd.Value != 0))
                {
                    var umbrellaPoolee = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Poolee);
                    umbrellaPoolee.EncumbranceAmount += umbrella.EOpenBal.HasValue ? umbrella.EOpenBal.Value : 0m;
                    umbrellaPoolee.EncumbranceAmount += umbrella.EncumbrancesYtd.HasValue ? umbrella.EncumbrancesYtd.Value : 0m;
                    umbrellaPoolee.EncumbranceAmount -= umbrella.EncumbrancesRelievedYtd.HasValue ? umbrella.EncumbrancesRelievedYtd.Value : 0m;
                    umbrellaPoolee.ActualAmount += umbrella.DebitsYtd.HasValue ? umbrella.DebitsYtd.Value : 0m;
                    umbrellaPoolee.ActualAmount -= umbrella.CreditsYtd.HasValue ? umbrella.CreditsYtd.Value : 0m;

                    budgetPool.AddPoolee(umbrellaPoolee);
                }

                budgetPoolEntities.Add(budgetPool);
            }

            // Add each poolee to the appropriate umbrella
            foreach (var poolee in pooleeAccounts)
            {
                var pooleeEntity = new CostCenterGlAccount(poolee.Recordkey, GlBudgetPoolType.Poolee);
                pooleeEntity.BudgetAmount += poolee.BAlocDebitsYtd.HasValue ? poolee.BAlocDebitsYtd.Value : 0m;
                pooleeEntity.BudgetAmount -= poolee.BAlocCreditsYtd.HasValue ? poolee.BAlocCreditsYtd.Value : 0m;
                pooleeEntity.EncumbranceAmount += poolee.EOpenBal.HasValue ? poolee.EOpenBal.Value : 0m;
                pooleeEntity.EncumbranceAmount += poolee.EncumbrancesYtd.HasValue ? poolee.EncumbrancesYtd.Value : 0m;
                pooleeEntity.EncumbranceAmount -= poolee.EncumbrancesRelievedYtd.HasValue ? poolee.EncumbrancesRelievedYtd.Value : 0m;
                pooleeEntity.ActualAmount += poolee.DebitsYtd.HasValue ? poolee.DebitsYtd.Value : 0m;
                pooleeEntity.ActualAmount -= poolee.CreditsYtd.HasValue ? poolee.CreditsYtd.Value : 0m;

                var selectedBudgetPool = budgetPoolEntities.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolee.GlsBudgetLinkage);
                if (selectedBudgetPool != null)
                {
                    selectedBudgetPool.AddPoolee(pooleeEntity);
                }
                else
                {
                    // Get the data contract for the umbrella in question.
                    var umbrellaDataContract = glsFyrNoAccessUmbrellaDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlsBudgetLinkage);

                    // Create the umbrella account domain entity.
                    var umbrellaGlAccount = new CostCenterGlAccount(poolee.GlsBudgetLinkage, GlBudgetPoolType.Umbrella);
                    umbrellaGlAccount.BudgetAmount = umbrellaDataContract.BAlocDebitsYtd.HasValue ? umbrellaDataContract.BAlocDebitsYtd.Value : 0m;
                    umbrellaGlAccount.BudgetAmount -= umbrellaDataContract.BAlocCreditsYtd.HasValue ? umbrellaDataContract.BAlocCreditsYtd.Value : 0m;
                    foreach (var amount in umbrellaDataContract.GlsFaMactuals)
                    {
                        umbrellaGlAccount.ActualAmount += amount.HasValue ? amount.Value : 0m;
                    }

                    foreach (var amount in umbrellaDataContract.GlsFaMencumbrances)
                    {
                        umbrellaGlAccount.EncumbranceAmount += amount.HasValue ? amount.Value : 0m;
                    }

                    // Create the budget pool.
                    var budgetPool = new GlBudgetPool(umbrellaGlAccount);

                    // Add a poolee to represent direct expenses for the umbrella
                    //if ((umbrellaDataContract.EOpenBal.HasValue && umbrellaDataContract.EOpenBal.Value != 0)
                    //|| (umbrellaDataContract.EncumbrancesYtd.HasValue && umbrellaDataContract.EncumbrancesYtd.Value != 0)
                    //|| (umbrellaDataContract.EncumbrancesRelievedYtd.HasValue && umbrellaDataContract.EncumbrancesRelievedYtd.Value != 0)
                    //|| (umbrellaDataContract.DebitsYtd.HasValue && umbrellaDataContract.DebitsYtd.Value != 0)
                    //|| (umbrellaDataContract.CreditsYtd.HasValue && umbrellaDataContract.CreditsYtd.Value != 0))
                    //{
                    //    var umbrellaPoolee = new CostCenterGlAccount(umbrellaDataContract.Recordkey, GlBudgetPoolType.Poolee);
                    //    umbrellaPoolee.EncumbranceAmount += umbrellaDataContract.EOpenBal.HasValue ? umbrellaDataContract.EOpenBal.Value : 0m;
                    //    umbrellaPoolee.EncumbranceAmount += umbrellaDataContract.EncumbrancesYtd.HasValue ? umbrellaDataContract.EncumbrancesYtd.Value : 0m;
                    //    umbrellaPoolee.EncumbranceAmount -= umbrellaDataContract.EncumbrancesRelievedYtd.HasValue ? umbrellaDataContract.EncumbrancesRelievedYtd.Value : 0m;
                    //    umbrellaPoolee.ActualAmount += umbrellaDataContract.DebitsYtd.HasValue ? umbrellaDataContract.DebitsYtd.Value : 0m;
                    //    umbrellaPoolee.ActualAmount -= umbrellaDataContract.CreditsYtd.HasValue ? umbrellaDataContract.CreditsYtd.Value : 0m;

                    //    budgetPool.AddPoolee(umbrellaPoolee);
                    //}

                    // Finally, add the new budget pool to the budget pool entities list.
                    budgetPoolEntities.Add(budgetPool);
                }
            }

            // Add each non-pooled account to the non pooled accounts list
            var nonPooledEntities = new List<CostCenterGlAccount>();
            foreach (var nonPooledAccount in nonPooledAccounts)
            {
                var nonPooledEntity = new CostCenterGlAccount(nonPooledAccount.Recordkey, GlBudgetPoolType.None);
                nonPooledEntity.BudgetAmount += nonPooledAccount.BAlocDebitsYtd.HasValue ? nonPooledAccount.BAlocDebitsYtd.Value : 0m;
                nonPooledEntity.BudgetAmount -= nonPooledAccount.BAlocCreditsYtd.HasValue ? nonPooledAccount.BAlocCreditsYtd.Value : 0m;
                nonPooledEntity.EncumbranceAmount += nonPooledAccount.EOpenBal.HasValue ? nonPooledAccount.EOpenBal.Value : 0m;
                nonPooledEntity.EncumbranceAmount += nonPooledAccount.EncumbrancesYtd.HasValue ? nonPooledAccount.EncumbrancesYtd.Value : 0m;
                nonPooledEntity.EncumbranceAmount -= nonPooledAccount.EncumbrancesRelievedYtd.HasValue ? nonPooledAccount.EncumbrancesRelievedYtd.Value : 0m;
                nonPooledEntity.ActualAmount += nonPooledAccount.DebitsYtd.HasValue ? nonPooledAccount.DebitsYtd.Value : 0m;
                nonPooledEntity.ActualAmount -= nonPooledAccount.CreditsYtd.HasValue ? nonPooledAccount.CreditsYtd.Value : 0m;

                nonPooledEntities.Add(nonPooledEntity);
            }

            // Insert the budget pools into the cost centers list.
            foreach (var budgetPool in budgetPoolEntities)
            {
                AddBudgetPoolToCostCentersList(budgetPool, this.param2_costCenterStructure);
            }

            // Insert the non-pooled accounts into the cost centers list
            foreach (var nonPooledAccount in nonPooledEntities)
            {
                AddNonPooledAccountToCostCenterList(nonPooledAccount, this.param2_costCenterStructure);
            }

            // Now that the cost centers have been built, look at the requisition data for the GL accounts from ENC.FYR
            // and include it in the appropriate cost center.
            var allUmbrellaAccounts = this.expectedCostCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.Pools.Select(z => z.Umbrella))).ToList();
            var allPooleeAccounts = this.expectedCostCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.Pools.SelectMany(z => z.Poolees))).ToList();
            var allNonPooledAccounts = this.expectedCostCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.GlAccounts)).ToList();

            // Loop through all of the ENC.FYR records and try to find the GL account in the umbrellas, poolees, and non-pooled accounts.
            foreach (var dataContract in this.encFyrDataContracts)
            {
                decimal totalEncAmount = 0m;
                foreach (var amount in dataContract.EncReqAmt)
                {
                    totalEncAmount += amount.HasValue ? amount.Value : 0m;
                }

                bool glAccountFound = false;

                // Try to find the requistion GL number in the umbrellas
                var selectedUmbrellaAccount = allUmbrellaAccounts.FirstOrDefault(x => x.GlAccountNumber == dataContract.Recordkey);
                if (selectedUmbrellaAccount != null)
                {
                    selectedUmbrellaAccount.EncumbranceAmount += totalEncAmount;

                    // Now update the poolee entry for this umbrella, or create one...
                    var umbrellaPoolee = allPooleeAccounts.FirstOrDefault(x => x.GlAccountNumber == selectedUmbrellaAccount.GlAccountNumber);
                    if (umbrellaPoolee != null)
                    {
                        umbrellaPoolee.EncumbranceAmount += totalEncAmount;
                    }
                    else
                    {
                        var poolee = new CostCenterGlAccount(dataContract.Recordkey, GlBudgetPoolType.Poolee);
                        poolee.EncumbranceAmount = totalEncAmount;

                        // Find the pool with the matching umbrella and insert it into the pool.
                        var selectedPool = this.expectedCostCenters.SelectMany(x => x.CostCenterSubtotals).SelectMany(x => x.Pools)
                            .FirstOrDefault(x => x.Umbrella.GlAccountNumber == selectedUmbrellaAccount.GlAccountNumber);
                        selectedPool.AddPoolee(poolee);
                    }
                    glAccountFound = true;
                }

                // Try to find the requistion GL number in the poolees
                if (!glAccountFound)
                {
                    var selectedPooleeAccount = allPooleeAccounts.FirstOrDefault(x => x.GlAccountNumber == dataContract.Recordkey);
                    if (selectedPooleeAccount != null)
                    {
                        selectedPooleeAccount.EncumbranceAmount += totalEncAmount;
                        glAccountFound = true;
                    }
                }

                // Try to find the requistion GL number in the non-pooled accounts
                if (!glAccountFound)
                {
                    var selectedNonPooledAccount = allNonPooledAccounts.FirstOrDefault(x => x.GlAccountNumber == dataContract.Recordkey);
                    if (selectedNonPooledAccount != null)
                    {
                        selectedNonPooledAccount.EncumbranceAmount += totalEncAmount;
                        glAccountFound = true;
                    }
                }

                // Add the requisition amounts to a cost center if the GL account is not already in any of the cost centers.
            }
        }

        private void InitializeMockStatements()
        {
            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GlAccts>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                var dataContractsToReturn = new Collection<GlAccts>();

                if (!string.IsNullOrEmpty(this.param3_SelectedCostCenterId))
                {
                    foreach (var dataContract in this.glAcctsDataContracts)
                    {
                        // Determine the cost center ID for this expense GL account
                        var expenseGlAccountCostCenterId = string.Empty;
                        foreach (var component in this.param2_costCenterStructure.CostCenterComponents)
                        {
                            if (component != null)
                            {
                                var componentId = dataContract.Recordkey.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));
                                expenseGlAccountCostCenterId += componentId;
                            }
                        }

                        // If the GL account cost center ID matches the selected one,
                        // add the GL account to the list of GL accounts to process.
                        if (expenseGlAccountCostCenterId == this.param3_SelectedCostCenterId)
                        {
                            dataContractsToReturn.Add(dataContract);
                        }
                    }
                }
                else
                {
                    dataContractsToReturn = this.glAcctsDataContracts;
                }


                return Task.FromResult(dataContractsToReturn);
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
            dataReaderMock.Setup(dr => dr.SelectAsync(glAcctsFilename, It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(glAcctsIds);
            });
        }

        private string CalculateCostCenterId(string glAccount, CostCenterStructure costCenterStructure)
        {
            // Determine which component description objects will be needed to calculate the cost center Id and Name.
            var costCenterId = string.Empty;
            glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();

            if (!string.IsNullOrEmpty(glAccount))
            {
                foreach (var component in costCenterStructure.CostCenterComponents)
                {
                    if (component != null)
                    {
                        var componentId = glAccount.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength));
                        costCenterId += componentId;

                        if (component.IsPartOfDescription && glComponentsForCostCenter.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).Count() == 0)
                        {
                            var componentDescription = new GeneralLedgerComponentDescription(componentId, component.ComponentType);

                            var selectedComponent = testRepositoryDescriptions.FirstOrDefault(x => x.Id == componentId && x.ComponentType == component.ComponentType);

                            if (selectedComponent != null)
                            {
                                componentDescription.Description = selectedComponent.Description;
                            }

                            glComponentsForCostCenter.Add(componentDescription);
                        }
                    }
                }
            }

            return costCenterId;
        }

        private string CalculateSubtotalId(string glNumber, CostCenterStructure costCenterStructure)
        {
            // Obtain the GL account digits that determine the cost center subtotal.
            var costCenterSubtotalId = glNumber.Substring(costCenterStructure.CostCenterSubtotal.StartPosition, Convert.ToInt32(costCenterStructure.CostCenterSubtotal.ComponentLength));

            return costCenterSubtotalId;
        }

        private void AddBudgetPoolToCostCentersList(GlBudgetPool budgetPool, CostCenterStructure costCenterStructure)
        {
            var costCenterId = CalculateCostCenterId(budgetPool.Umbrella.GlAccountNumber, costCenterStructure);
            var subtotalId = CalculateSubtotalId(budgetPool.Umbrella.GlAccountNumber, costCenterStructure);

            // Add the cost center to the list of cost centers for the user if it does not exist already.
            // Also add the GL account to the list of GL accounts for the cost center.
            var selectedCostCenter = this.expectedCostCenters.FirstOrDefault(x => x.Id == costCenterId);
            if (selectedCostCenter != null)
            {
                // If the cost center already contains this subtotal 

                // already exists in the list of subtotals then add the pool to the subtotal, otherwise create
                // a new subtotal with the umbrella and add all poolees to that subtotal pool. Lastly, add the subtotal to the
                // list of subtotals.
                var selectedSubtotal = selectedCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == subtotalId);
                if (selectedSubtotal != null)
                {
                    selectedSubtotal.AddBudgetPool(budgetPool);
                }
                else
                {
                    var subtotal = new CostCenterSubtotal(subtotalId, budgetPool, GlClass.Expense);
                    subtotal.Name = GetSubtotalDescription(subtotalId);

                    selectedCostCenter.AddCostCenterSubtotal(subtotal);
                }
            }
            else
            {
                var subtotal = new CostCenterSubtotal(subtotalId, budgetPool, GlClass.Expense);
                subtotal.Name = GetSubtotalDescription(subtotalId);

                var newCostCenter = new CostCenter(costCenterId, subtotal, glComponentsForCostCenter);
                newCostCenter.UnitId = budgetPool.Umbrella.GlAccountNumber.Substring(costCenterStructure.Unit.StartPosition, Convert.ToInt32(costCenterStructure.Unit.ComponentLength));
                this.expectedCostCenters.Add(newCostCenter);
            }
        }

        private void AddNonPooledAccountToCostCenterList(CostCenterGlAccount glAccount, CostCenterStructure costCenterStructure)
        {
            var costCenterId = CalculateCostCenterId(glAccount.GlAccountNumber, costCenterStructure);
            var subtotalId = CalculateSubtotalId(glAccount.GlAccountNumber, costCenterStructure);

            // Add the cost center to the list of cost centers for the user if it does not exist already.
            // Also add the GL account to the list of GL accounts for the cost center.
            var selectedCostCenter = this.expectedCostCenters.FirstOrDefault(x => x.Id == costCenterId);

            // The cost center already exists so we need to add the GL account to a new or existing subtotal.
            if (selectedCostCenter != null)
            {
                // Check if the cost center contains the subtotal.
                var selectedSubtotal = selectedCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == subtotalId);

                // If the subtotal exists in the cost center add the GL account to the subtotal non-pooled accounts.
                // If the subtotal does not exist in the cost center then create a new subtotal using the GL account
                //    and add that subtotal to the cost center.
                if (selectedSubtotal != null)
                {
                    selectedSubtotal.AddGlAccount(glAccount);
                }
                else
                {
                    var subtotal = new CostCenterSubtotal(subtotalId, glAccount, GlClass.Expense);
                    subtotal.Name = GetSubtotalDescription(subtotalId);

                    selectedCostCenter.AddCostCenterSubtotal(subtotal);
                }
            }
            // The cost center does not exist so we need to add the GL account to a new subtotal then add
            // that subtotal to a new cost center and add that cost center to the cost centers list.
            else
            {
                var subtotal = new CostCenterSubtotal(subtotalId, glAccount, GlClass.Expense);
                subtotal.Name = GetSubtotalDescription(subtotalId);

                var newCostCenter = new CostCenter(costCenterId, subtotal, glComponentsForCostCenter);
                newCostCenter.UnitId = glAccount.GlAccountNumber.Substring(costCenterStructure.Unit.StartPosition, Convert.ToInt32(costCenterStructure.Unit.ComponentLength));
                this.expectedCostCenters.Add(newCostCenter);
            }
        }

        private string GetSubtotalDescription(string subtotalId)
        {
            string description = "";
            var obDescDataContract = testGlConfigurationRepository.ObDescs.FirstOrDefault(x => x.Recordkey == subtotalId);

            if (obDescDataContract != null)
            {
                description = obDescDataContract.ObDescription;
            }

            return description;
        }
        #endregion
    }
}