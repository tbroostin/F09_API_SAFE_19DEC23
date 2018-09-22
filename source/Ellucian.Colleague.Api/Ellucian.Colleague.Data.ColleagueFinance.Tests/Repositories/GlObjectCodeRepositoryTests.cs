// Copyright 2017 Ellucian Company L.P. and its affiliates.

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
    public class GlObjectCodeRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private GlObjectCodeRepository actualRepository = null;
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
        private GeneralLedgerAccountStructure param2_glAccountStructure;
        private GeneralLedgerClassConfiguration param3_glClassConfiguration;
        private int param4_FiscalYear;
        private CostCenterQueryCriteria param4_costCenterQueryCriteria;
        private List<CostCenterComponentQueryCriteria> costCenterComponentQueryCriteria;
        private List<CostCenterComponentRangeQueryCriteria> costCenterComponentRangeQueryCriteria;

        private Collection<GlAccts> glAcctsDataContracts;
        private Collection<GlAccts> glAcctsNoAccessUmbrellaDataContracts;
        private Collection<GlsFyr> glsFyrDataContracts;
        private Collection<GlsFyr> glsFyrNoAccessUmbrellaDataContracts;
        private Collection<EncFyr> encFyrDataContracts;
        private Collection<GlpFyr> glpFyrDataContracts;
        private List<CostCenter> expectedCostCenters;
        private GeneralLedgerComponent objectMajorComponent;

        private string[] glsIds = new string[] { };
        private string[] encIds = new string[] { };
        private string[] glAcctsIds = new string[] { };

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            objectMajorComponent = null;
            actualRepository = new GlObjectCodeRepository(cacheProvider, transFactory, logger);

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
            testGlNumberRepository = new TestGlAccountRepository();
            glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();
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

            costCenterComponentQueryCriteria = new List<CostCenterComponentQueryCriteria>();
            costCenterComponentQueryCriteria.Add(costCenterCompQueryCriteria);

            this.param4_costCenterQueryCriteria = new CostCenterQueryCriteria(costCenterComponentQueryCriteria);
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.StartYear.ToString();

            var expenseValues = new List<String>() { "5", "7" };
            var revenueValues = new List<String>() { "4" };
            var assetValues = new List<string>() { "1" };
            var liabilityValues = new List<string>() { "2" };
            var fundBalanceValues = new List<string>() { "3" };
            this.param3_glClassConfiguration = new GeneralLedgerClassConfiguration("GL.Class", expenseValues, revenueValues, assetValues, liabilityValues, fundBalanceValues);
            this.param3_glClassConfiguration.GlClassStartPosition = 18;
            this.param3_glClassConfiguration.GlClassLength = 1;

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
            glComponentsForCostCenter = null;
            testGlNumberRepository = null;

            this.param1_GlUser = null;
            this.param2_glAccountStructure = null;
            this.param3_glClassConfiguration = null;
            this.param4_FiscalYear = 0;
            this.param4_costCenterQueryCriteria = null;
            glsIds = new string[] { };
            encIds = new string[] { };
            glAcctsIds = new string[] { };
            this.testRepositoryDescriptions = null;
            objectMajorComponent = null;
        }
        #endregion

        #region Open year scenarios

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_ValidateGlClass()
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
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts data contracts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Obtain the GL object codes domain entities from the GL accounts.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();

            foreach (var glObjectCode in glObjectCodes)
            {
                var glAccountId = glObjectCode.GlAccounts.First().GlAccountNumber;
                var glClassId = glAccountId.Substring(this.param3_glClassConfiguration.GlClassStartPosition, this.param3_glClassConfiguration.GlClassLength);
                var glObjectCodeGlClass = glObjectCode.Id.Substring(0, 1);
                switch (glObjectCode.GlClass)
                {
                    case GlClass.Asset:
                        Assert.AreEqual(glObjectCodeGlClass, glClassId);
                        break;
                    case GlClass.Expense:
                        Assert.AreEqual(glObjectCodeGlClass, glClassId);
                        break;
                    case GlClass.FundBalance:
                        Assert.AreEqual(glObjectCodeGlClass, glClassId);
                        break;
                    case GlClass.Liability:
                        Assert.AreEqual(glObjectCodeGlClass, glClassId);
                        break;
                    case GlClass.Revenue:
                        Assert.AreEqual(glObjectCodeGlClass, glClassId);
                        break;
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_NonPooledAccountsOnly_AllGLClasses()
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
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository(null, false, glAccounts);

            // Populate the non-pooled accounts
            PopulateGlAccountDataContracts(glAccounts, GlBudgetPoolType.None, null);

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(6, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                var glAccountId = glObjectCode.GlAccounts.First().GlAccountNumber;
                objectMajorComponent = (await testGlConfigurationRepository.GetAccountStructureAsync()).MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);
                Assert.AreEqual(glAccountId.Substring(objectMajorComponent.StartPosition, objectMajorComponent.ComponentLength), glObjectCode.Id);

                var expectedName = testGlConfigurationRepository.ObDescs.FirstOrDefault(x => x.Recordkey == glObjectCode.Id).ObDescription;
                Assert.AreEqual(expectedName, glObjectCode.Name);

                // Check the GL class of the GL object code.
                var expectedGlClass = GlClass.Asset;
                var glClassId = glAccountId.Substring(this.param3_glClassConfiguration.GlClassStartPosition, this.param3_glClassConfiguration.GlClassLength);
                switch (glClassId)
                {
                    case "1":
                        expectedGlClass = GlClass.Asset;
                        break;
                    case "2":
                        expectedGlClass = GlClass.Liability;
                        break;
                    case "3":
                        expectedGlClass = GlClass.FundBalance;
                        break;
                    case "4":
                        expectedGlClass = GlClass.Revenue;
                        break;
                    case "5":
                        expectedGlClass = GlClass.Expense;
                        break;
                    case "7":
                        expectedGlClass = GlClass.Expense;
                        break;
                }
                Assert.AreEqual(expectedGlClass, glObjectCode.GlClass);

                // Make sure the totals for each GL object code match the amounts in the data contracts.
                var objectGlAcctsMemos = this.glAcctsDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id))
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalEncumbrances);

                // Make sure the amounts of each GL account match the amounts in the data contracts.
                foreach (var glAccount in glObjectCode.GlAccounts)
                {
                    var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == glAccount.GlAccountNumber)
                        .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear).ToList();

                    Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), glAccount.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), glAccount.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), glAccount.EncumbranceAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_UmbrellaHasDirectExpenses()
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
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // The total number of poolees should match the number of poolees as seed accounts + the umbrella since it has direct expenses.
                Assert.AreEqual(pooleeAccounts.Count + 1, glObjectCode.Pools.First().Poolees.Count);

                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalEncumbrances);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
                {
                    // The umbrella should show up as a poolee...
                    List<GlAcctsMemos> glAcctsMemos = null;
                    if (poolee.GlAccountNumber == glObjectCode.Pools.First().Umbrella.GlAccountNumber)
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                            .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear
                                && x.GlPooledTypeAssocMember.ToUpper() == "U").ToList();
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                    }
                    else
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                        .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear
                            && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                        Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_UmbrellaHasNoDirectExpenses()
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
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // The total number of poolees should match the number of poolees as seed accounts.
                Assert.AreEqual(pooleeAccounts.Count, glObjectCode.Pools.First().Poolees.Count);

                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalEncumbrances);

                // Make sure the umbrella does NOT show up as a poolee since it has no direct expenses.
                var umbrellaAsPoolee = glObjectCode.Pools.First().Poolees.FirstOrDefault(x => x.GlAccountNumber == umbrellaAccount);
                Assert.IsNull(umbrellaAsPoolee);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
                {
                    var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                    .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear
                        && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                    Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_UmbrellaHasDirectExpenses_NoPoolees()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = this.glAcctsDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalEncumbrances);

                // The umbrella account should be the only poolee.
                Assert.AreEqual(1, glObjectCode.Pools.First().Poolees.Count);

                // Make sure the umbrella shows up as a poolee since it has direct expenses.
                var umbrellaAsPoolee = glObjectCode.Pools.First().Poolees.FirstOrDefault(x => x.GlAccountNumber == umbrellaAccount);
                Assert.IsNotNull(umbrellaAsPoolee);

                var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == umbrellaAsPoolee.GlAccountNumber)
                    .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear
                        && x.GlPooledTypeAssocMember.ToUpper() == "U").ToList();

                Assert.AreEqual(0m, umbrellaAsPoolee.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), umbrellaAsPoolee.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), umbrellaAsPoolee.EncumbranceAmount);
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_UmbrellaHasDirectExpenses_NoGlAccessToUmbrella()
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
            var dataContract = PopulateSingleGlAcctsDataContract(umbrellaAccount, umbrellaAccount, "U", true, false, false);
            glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = glAcctsNoAccessUmbrellaDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalEncumbrances);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
                {
                    // The umbrella should show up as a poolee...
                    List<GlAcctsMemos> glAcctsMemos = null;
                    if (poolee.GlAccountNumber == glObjectCode.Pools.First().Umbrella.GlAccountNumber)
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                            .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear
                                && x.GlPooledTypeAssocMember.ToUpper() == "U").ToList();
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                    }
                    else
                    {
                        glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                        .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear
                            && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                        Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                        Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                        Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_UmbrellaHasNoDirectExpenses_NoGlAccessToUmbrella()
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
            var dataContract = PopulateSingleGlAcctsDataContract(umbrellaAccount, umbrellaAccount, "U", false, false, false);
            glAcctsNoAccessUmbrellaDataContracts.Add(dataContract);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var objectGlAcctsMemos = glAcctsNoAccessUmbrellaDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount)
                    .SelectMany(x => x.MemosEntityAssociation
                    .Where(y => y.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear)).ToList();

                Assert.AreEqual(Helper_BudgetForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlAccts(objectGlAcctsMemos), glObjectCode.TotalEncumbrances);

                // The umbrella should NOT show up as a poolee.
                Assert.IsFalse(glObjectCode.Pools.First().Poolees.Select(x => x.GlAccountNumber).Contains(umbrellaAccount));

                // The umbrella should NOT be visible.
                Assert.IsFalse(glObjectCode.Pools.First().IsUmbrellaVisible);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
                {
                    // The umbrella should show up as a poolee...
                    var glAcctsMemos = this.glAcctsDataContracts.FirstOrDefault(x => x.Recordkey == poolee.GlAccountNumber)
                    .MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == this.param4_costCenterQueryCriteria.FiscalYear
                        && x.GlPooledTypeAssocMember.ToUpper() == "P").ToList();

                    Assert.AreEqual(Helper_BudgetForNonUmbrella_GlAccts(glAcctsMemos), poolee.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_GlAccts(glAcctsMemos), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlAccts(glAcctsMemos), poolee.EncumbranceAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_OpenYear_AmountsAreNull()
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
            this.glAcctsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.encIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            this.glsIds = pooleeAccounts.Union(nonPooledAccounts).ToArray();
            PopulateGlAccountDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false, false, true);

            // Populate the poolee accounts for each umbrella
            PopulateGlAccountDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount, false, false, true);

            // Populate the non-pooled accounts.
            PopulateGlAccountDataContracts(nonPooledAccounts, GlBudgetPoolType.None, null, false, false, true);

            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.AddAllAccounts(nonPooledAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = (await testGlConfigurationRepository.GetFiscalYearConfigurationAsync()).FiscalYearForToday.ToString();
            this.param4_costCenterQueryCriteria.IncludeActiveAccountsWithNoActivity = true;
            objectMajorComponent = (await testGlConfigurationRepository.GetAccountStructureAsync()).MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.AreEqual(3, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the GL object code totals are all zero.
                Assert.AreEqual(0m, glObjectCode.TotalBudget);
                Assert.AreEqual(0m, glObjectCode.TotalActuals);
                Assert.AreEqual(0m, glObjectCode.TotalEncumbrances);

                // Make sure the non-pooled accounts have zero totals.
                var nonPooledAccountsForObject = nonPooledAccounts
                    .Where(x => x.Substring(objectMajorComponent.StartPosition, objectMajorComponent.ComponentLength) == glObjectCode.Id)
                    .ToList();
                Assert.AreEqual(nonPooledAccountsForObject.Count, glObjectCode.GlAccounts.Count);
                foreach (var glAccount in glObjectCode.GlAccounts)
                {
                    Assert.AreEqual(0m, glAccount.BudgetAmount);
                    Assert.AreEqual(0m, glAccount.ActualAmount);
                    Assert.AreEqual(0m, glAccount.EncumbranceAmount);
                }

                // Make sure the poolee amounts are all zero
                foreach (var pool in glObjectCode.Pools)
                {
                    // Make sure the umbrella amounts are zero.
                    Assert.AreEqual(0m, pool.Umbrella.BudgetAmount);
                    Assert.AreEqual(0m, pool.Umbrella.ActualAmount);
                    Assert.AreEqual(0m, pool.Umbrella.EncumbranceAmount);

                    Assert.AreEqual(pooleeAccounts.Count, pool.Poolees.Count);
                    foreach (var poolee in glObjectCode.Pools.First().Poolees)
                    {
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(0m, poolee.ActualAmount);
                        Assert.AreEqual(0m, poolee.EncumbranceAmount);
                    }
                }
            }
        }
        #endregion

        #region Closed year scenarios
        [TestMethod]
        [Ignore]
        public async Task GetGlObjectCodesAsync_ClosedYear_NonPooledAccountsOnly()
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
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the non-pooled accounts
            PopulateGlsFyrDataContracts(glAccounts, GlBudgetPoolType.None, null);
            PopulateEncFyrDataContracts(glAccounts);

            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();

            // Make sure each each GL class is represented in the total number of GL Object codes.
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(6, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the totals for each GL object code match the amounts in the data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsDataContracts, this.param3_glClassConfiguration), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glObjectCode.TotalEncumbrances);

                // Make sure the amounts of each GL account match the amounts in the data contracts.
                foreach (var glAccount in glObjectCode.GlAccounts)
                {
                    glsDataContracts = this.glsFyrDataContracts
                        .Where(x => x.Recordkey == glAccount.GlAccountNumber).ToList();
                    encDataContracts = this.encFyrDataContracts
                        .Where(x => x.Recordkey == glAccount.GlAccountNumber).ToList();

                    Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glAccount.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsDataContracts, this.param3_glClassConfiguration), glAccount.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glAccount.EncumbranceAmount);
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetGlObjectCodesAsync_ClosedYear_UmbrellaHasDirectExpenses()
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
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the umbrella account
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);
            PopulateEncFyrDataContracts(new List<string>() { umbrellaAccount });

            // Populate the poolee accounts
            PopulateGlsFyrDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);
            PopulateEncFyrDataContracts(pooleeAccounts);

            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();

            // Make sure each each GL class is represented in the total number of GL Object codes.
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // The total number of poolees should match the number of poolees as seed accounts + the umbrella since it has direct expenses.
                Assert.AreEqual(pooleeAccounts.Count + 1, glObjectCode.Pools.First().Poolees.Count);

                // Make sure the totals for each GL object code match the amounts in the umbrella data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsDataContracts), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glObjectCode.TotalEncumbrances);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
                {
                    List<GlsFyr> glsFyrContracts = glsFyrContracts = this.glsFyrDataContracts
                        .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    List<EncFyr> encFyrContracts = encFyrContracts = this.encFyrDataContracts
                        .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    if (poolee.GlAccountNumber == glObjectCode.Pools.First().Umbrella.GlAccountNumber)
                    {
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                    }
                    else
                    {
                        Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), poolee.BudgetAmount);
                    }
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.EncumbranceAmount);
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetGlObjectCodesAsync_ClosedYear_UmbrellaHasNoDirectExpenses()
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
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the umbrella account
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null, false);
            PopulateEncFyrDataContracts(new List<string>() { umbrellaAccount });

            // Populate the poolee accounts
            PopulateGlsFyrDataContracts(pooleeAccounts, GlBudgetPoolType.Poolee, umbrellaAccount);
            PopulateEncFyrDataContracts(pooleeAccounts);

            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();

            // Make sure each each GL class is represented in the total number of GL Object codes.
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // The total number of poolees should match the number of poolees as seed accounts.
                Assert.AreEqual(pooleeAccounts.Count, glObjectCode.Pools.First().Poolees.Count);

                // Make sure the totals for each GL object code match the amounts in the umbrella data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsDataContracts), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glObjectCode.TotalEncumbrances);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
                {
                    List<GlsFyr> glsFyrContracts = glsFyrContracts = this.glsFyrDataContracts
                        .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    List<EncFyr> encFyrContracts = encFyrContracts = this.encFyrDataContracts
                        .Where(x => x.Recordkey == poolee.GlAccountNumber).ToList();
                    Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), poolee.BudgetAmount);
                    Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), poolee.ActualAmount);
                    Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), poolee.EncumbranceAmount);
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetGlObjectCodesAsync_ClosedYear_UmbrellaHasDirectExpenses_NoPoolees()
        {
            var umbrellaAccount = "10_00_01_00_EJK88_10000";
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.AddAllAccounts(new List<string>() { umbrellaAccount });
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Populate the umbrella account
            PopulateGlsFyrDataContracts(new List<string>() { umbrellaAccount }, GlBudgetPoolType.Umbrella, null);
            PopulateEncFyrDataContracts(new List<string>() { umbrellaAccount });

            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();

            // Make sure each each GL class is represented in the total number of GL Object codes.
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the totals for each GL object code match the amounts in the umbrella data contracts.
                var glsDataContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();
                var encDataContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey.Contains(glObjectCode.Id)).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsDataContracts), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsDataContracts), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsDataContracts, encDataContracts), glObjectCode.TotalEncumbrances);

                // There should be only one poolee, the umbrella.
                Assert.AreEqual(1, glObjectCode.Pools.First().Poolees.Count);

                // Make sure the umbrella shows up as a poolee since it has direct expenses.
                var umbrellaAsPoolee = glObjectCode.Pools.First().Poolees.FirstOrDefault(x => x.GlAccountNumber == umbrellaAccount);
                Assert.IsNotNull(umbrellaAsPoolee);

                // Make sure the umbrella amounts match the amounts in the data contract.
                var glsFyrContracts = this.glsFyrDataContracts
                    .Where(x => x.Recordkey == umbrellaAsPoolee.GlAccountNumber).ToList();
                var encFyrContracts = this.encFyrDataContracts
                    .Where(x => x.Recordkey == umbrellaAsPoolee.GlAccountNumber).ToList();
                Assert.AreEqual(0m, umbrellaAsPoolee.BudgetAmount);
                Assert.AreEqual(Helper_ActualsForNonUmbrella_Gls(glsFyrContracts, this.param3_glClassConfiguration), umbrellaAsPoolee.ActualAmount);
                Assert.AreEqual(Helper_EncumbrancesForNonUmbrella_GlsAndEnc(glsFyrContracts, encFyrContracts), umbrellaAsPoolee.EncumbranceAmount);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task GetGlObjectCodesAsync_ClosedYear_UmbrellaHasDirectExpenses_NoGlAccessToUmbrella()
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
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var glsFyrContracts = glsFyrNoAccessUmbrellaDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsFyrContracts), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsFyrContracts, null), glObjectCode.TotalEncumbrances);

                // The umbrella should NOT show up as a poolee.
                Assert.IsFalse(glObjectCode.Pools.First().Poolees.Select(x => x.GlAccountNumber).Contains(umbrellaAccount));

                // The umbrella should NOT be visible.
                Assert.IsFalse(glObjectCode.Pools.First().IsUmbrellaVisible);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
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
        }

        [TestMethod]
        [Ignore]
        public async Task GetGlObjectCodesAsync_ClosedYear_UmbrellaHasNoDirectExpenses_NoGlAccessToUmbrella()
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
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.AreEqual(1, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the totals for the GL object code matches the amounts in the umbrella data contracts.
                var glsFyrContracts = glsFyrNoAccessUmbrellaDataContracts
                    .Where(x => x.Recordkey == umbrellaAccount).ToList();

                Assert.AreEqual(Helper_BudgetForAnyAccount_Gls(glsFyrContracts), glObjectCode.TotalBudget);
                Assert.AreEqual(Helper_ActualsForUmbrella_Gls(glsFyrContracts), glObjectCode.TotalActuals);
                Assert.AreEqual(Helper_EncumbrancesForUmbrella_GlsAndEnc(glsFyrContracts, null), glObjectCode.TotalEncumbrances);

                // The umbrella should NOT show up as a poolee.
                Assert.IsFalse(glObjectCode.Pools.First().Poolees.Select(x => x.GlAccountNumber).Contains(umbrellaAccount));

                // The umbrella should NOT be visible.
                Assert.IsFalse(glObjectCode.Pools.First().IsUmbrellaVisible);

                // Make sure the amounts of each poolee GL account match the amounts in the data contracts.
                foreach (var poolee in glObjectCode.Pools.First().Poolees)
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
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_ClosedYear_AmountsAreNull()
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
            this.param1_GlUser.AddAllAccounts(pooleeAccounts);
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param4_costCenterQueryCriteria.FiscalYear = testGlConfigurationRepository.ClosedYear;
            this.param4_costCenterQueryCriteria.IncludeActiveAccountsWithNoActivity = true;
            objectMajorComponent = (await testGlConfigurationRepository.GetAccountStructureAsync()).MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);

            // Make sure all six GL classes are represented.
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.AreEqual(3, glObjectCodes.Count());

            foreach (var glObjectCode in glObjectCodes)
            {
                // Make sure the GL object code totals are all zero.
                Assert.AreEqual(0m, glObjectCode.TotalBudget);
                Assert.AreEqual(0m, glObjectCode.TotalActuals);
                Assert.AreEqual(0m, glObjectCode.TotalEncumbrances);

                // Make sure the non-pooled accounts have zero totals.
                var nonPooledAccountsForObject = nonPooledAccounts
                    .Where(x => x.Substring(objectMajorComponent.StartPosition, objectMajorComponent.ComponentLength) == glObjectCode.Id)
                    .ToList();
                Assert.AreEqual(nonPooledAccountsForObject.Count, glObjectCode.GlAccounts.Count);
                foreach (var glAccount in glObjectCode.GlAccounts)
                {
                    Assert.AreEqual(0m, glAccount.BudgetAmount);
                    Assert.AreEqual(0m, glAccount.ActualAmount);
                    Assert.AreEqual(0m, glAccount.EncumbranceAmount);
                }

                // Make sure the poolee amounts are all zero
                foreach (var pool in glObjectCode.Pools)
                {
                    // Make sure the umbrella amounts are zero.
                    Assert.AreEqual(0m, pool.Umbrella.BudgetAmount);
                    Assert.AreEqual(0m, pool.Umbrella.ActualAmount);
                    Assert.AreEqual(0m, pool.Umbrella.EncumbranceAmount);

                    Assert.AreEqual(pooleeAccounts.Count, pool.Poolees.Count);
                    foreach (var poolee in glObjectCode.Pools.First().Poolees)
                    {
                        Assert.AreEqual(0m, poolee.BudgetAmount);
                        Assert.AreEqual(0m, poolee.ActualAmount);
                        Assert.AreEqual(0m, poolee.EncumbranceAmount);
                    }
                }
            }
        }
        #endregion

        #region General Error-Type Scenarios
        [TestMethod]
        public async Task GetCostCentersAsync_NullFiscalYear()
        {
            this.param4_costCenterQueryCriteria.FiscalYear = null;
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_EmptyFiscalYear()
        {
            this.param4_costCenterQueryCriteria.FiscalYear = "";
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NullGeneralLedgerUser()
        {
            this.param1_GlUser = null;
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NullAccountStructure()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param2_glAccountStructure = null;
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NullGlClassConfiguration()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            this.param3_glClassConfiguration = null;
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NullExpenseAccounts()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_NoExpenseAccounts()
        {
            this.param1_GlUser = new GeneralLedgerUser("0000032", "Kleehammer");
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GenLdgrRecordIsNull()
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

            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GenLdgrRecordStatusIsNull()
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

            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersAsync_GenLdgrRecordStatusIsEmpty()
        {
            this.param1_GlUser = await testGlUserRepository.GetGeneralLedgerUserAsync("0000032", null, null, null);
            this.param1_GlUser.AddExpenseAccounts(testGlNumberRepository.WithUnitSubclass("AJK").GetFilteredGlNumbers());
            this.param1_GlUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            this.param4_FiscalYear = testGlConfigurationRepository.StartYear;
            testGlConfigurationRepository.GenLdgrDataContracts[1].GenLdgrStatus = "";

            var glObjectCodes = await RealRepository_GetGlObjectCodesAsync();
            Assert.IsTrue(glObjectCodes is IEnumerable<GlObjectCode>);
            Assert.IsTrue(glObjectCodes.Count() == 0);
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

        private decimal Helper_ActualsForNonUmbrella_Gls(IEnumerable<GlsFyr> glsDataContracts, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            var actualsAmount = 0m;
            var glClass = GetGlAccountGlClass(glsDataContracts.First().Recordkey, glClassConfiguration);

            if (glClass == GlClass.FundBalance)
            {
                foreach (var glsDataContract in glsDataContracts)
                {
                    actualsAmount += glsDataContract.OpenBal.HasValue ? glsDataContract.OpenBal.Value : 0m;
                    actualsAmount += glsDataContract.CloseDebits.HasValue ? glsDataContract.CloseDebits.Value : 0m;
                    actualsAmount -= glsDataContract.CloseCredits.HasValue ? glsDataContract.CloseCredits.Value : 0m;
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

            if (encDataContracts != null)
            {
                foreach (var encDataContract in encDataContracts)
                {
                    foreach (var encAmount in encDataContract.EncReqAmt)
                    {
                        encumbranceAmount += encAmount.HasValue ? encAmount.Value : 0m;
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

            foreach (var encDataContract in encDataContracts)
            {
                foreach (var encAmount in encDataContract.EncReqAmt)
                {
                    encumbranceAmount += encAmount.HasValue ? encAmount.Value : 0m;
                }
            }

            return encumbranceAmount;
        }

        private decimal Helper_EncumbrancesForUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.FaEncumbranceMemoAssocMember.Value) + amounts.Sum(x => x.FaEncumbrancePostedAssocMember.Value)
                + amounts.Sum(x => x.FaRequisitionMemoAssocMember.Value);
            return actualsAmount;
        }

        private decimal Helper_EncumbrancesForNonUmbrella_GlAccts(List<GlAcctsMemos> amounts)
        {
            var actualsAmount = amounts.Sum(x => x.GlEncumbranceMemosAssocMember.Value) + amounts.Sum(x => x.GlEncumbrancePostedAssocMember.Value)
                + amounts.Sum(x => x.GlRequisitionMemosAssocMember.Value);
            return actualsAmount;
        }
        #endregion

        public async Task<IEnumerable<GlObjectCode>> RealRepository_GetGlObjectCodesAsync()
        {
            return await actualRepository.GetGlObjectCodesAsync(this.param1_GlUser,
                this.param2_glAccountStructure,
                this.param3_glClassConfiguration,
                this.param4_costCenterQueryCriteria,
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
                EncReqAmt = encReqAmt
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
        }
        #endregion
    }
}