// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using dtoDomain = Ellucian.Colleague.Dtos.ColleagueFinance;
using entityDomain = Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class FinanceQueryServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        private FinanceQueryService service = null;

        private TestGeneralLedgerUserRepository testGlUserRepository = new TestGeneralLedgerUserRepository();
        private TestFinanceQueryRepository testFinanceQueryRepository = new TestFinanceQueryRepository();
        private GeneralLedgerUser generalLedgerUser;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private TestGeneralLedgerAccountRepository testGlAccountRepository = new TestGeneralLedgerAccountRepository();
        private entityDomain.GeneralLedgerAccountStructure testGlAccountStructure;
        private entityDomain.GeneralLedgerClassConfiguration testGlClassConfiguration;
        private dtoDomain.FinanceQueryCriteria filterCriteriaDto = null;
        private FinanceQueryCriteria filterCriteriaEntity = null;

        private GeneralLedgerAccountStructure param2_glAccountStructure;
        private FinanceQueryGlAccount glAccount_10_EJK88_10000;
        private FinanceQueryGlAccount glAccount_10_EJK88_20000;
        private FinanceQueryGlAccount glAccount_10_EJK88_30000;
        private FinanceQueryGlAccount glAccount_11_EJK88_10000;
        private FinanceQueryGlAccount glAccount_11_EJK88_20000;

        private Dictionary<string, Tuple<int, int>> glComponentStructureDict;

        private List<dtoDomain.CostCenterComponentQueryCriteria> componentCriteriaDto;
        private List<FinanceQueryComponentSortCriteria> sortCriteria;
        private List<FinanceQueryGlAccount> allGlAccounts;
        private List<FinanceQueryGlAccount> fund10_glAccounts;
        private List<FinanceQueryGlAccount> fund11_glAccounts;
        private List<FinanceQueryGlAccount> glAccounts_object_EJK88;
        private List<FinanceQueryGlAccountLineItem> allGlAccountLineItems;

        // Define user factories
        private GeneralLedgerUserAllAccounts glUserFactoryAll = new GeneralLedgerCurrentUser.GeneralLedgerUserAllAccounts();

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            BuildService(testFinanceQueryRepository, testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository);
            this.param2_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            generalLedgerUser = null;
            var componentQueryCriteria = new dtoDomain.CostCenterComponentQueryCriteria()
            {
                ComponentName = "FUND",
                IndividualComponentValues = new List<string>()
                  {
                      "11"
                  },

            };
            componentCriteriaDto = new List<dtoDomain.CostCenterComponentQueryCriteria>() { componentQueryCriteria };
            var sortComponentCriteriaDto = new List<dtoDomain.FinanceQueryComponentSortCriteria>();

            filterCriteriaDto = new dtoDomain.FinanceQueryCriteria()
            {
                FiscalYear = testGlConfigurationRepository.StartYear.ToString(),
                IncludeActiveAccountsWithNoActivity = true,
                ComponentCriteria = componentCriteriaDto,
                ComponentSortCriteria = sortComponentCriteriaDto
            };

            var componentCriteria = new List<entityDomain.CostCenterComponentQueryCriteria>() { new CostCenterComponentQueryCriteria("OBJECT") };
            var sortComponentCriteria = new List<entityDomain.FinanceQueryComponentSortCriteria>() { new FinanceQueryComponentSortCriteria("OBJECT", 1) };
            filterCriteriaEntity = new FinanceQueryCriteria(componentCriteria, sortComponentCriteria);
            filterCriteriaEntity.FiscalYear = testGlConfigurationRepository.StartYear.ToString();
            filterCriteriaEntity.ComponentCriteria = new List<Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria>();
            filterCriteriaEntity.IncludeActiveAccountsWithNoActivity = true;

            glComponentStructureDict = BuildGlComponentStructureDictionary(param2_glAccountStructure);

            allGlAccounts = new List<FinanceQueryGlAccount>();
            glAccounts_object_EJK88 = new List<FinanceQueryGlAccount>();

            fund10_glAccounts = new List<FinanceQueryGlAccount>();
            glAccount_10_EJK88_10000 = PopulateFinanceQueryGlAccount("10_00_01_00_EJK88_10000", 10.00m, 10000.00m, 0, 100.00m);
            glAccount_10_EJK88_20000 = PopulateFinanceQueryGlAccount("10_00_01_00_EJK88_20000", 0, 0, 0, 0);
            glAccount_10_EJK88_30000 = PopulateFinanceQueryGlAccount("10_00_01_00_EJK88_30000", 0, 30000.00m, 0, 0);

            fund10_glAccounts.Add(glAccount_10_EJK88_10000);
            fund10_glAccounts.Add(glAccount_10_EJK88_20000);
            fund10_glAccounts.Add(glAccount_10_EJK88_30000);

            fund11_glAccounts = new List<FinanceQueryGlAccount>();
            glAccount_11_EJK88_10000 = PopulateFinanceQueryGlAccount("11_00_01_00_EJK88_10000", 10.00m, 20000.00m, 100.00m, 0);
            glAccount_11_EJK88_20000 = PopulateFinanceQueryGlAccount("11_00_01_00_EJK88_20000", 10.00m, 0, 0, 0);

            fund11_glAccounts.Add(glAccount_11_EJK88_10000);
            fund11_glAccounts.Add(glAccount_11_EJK88_20000);

            glAccounts_object_EJK88.AddRange(fund10_glAccounts);
            glAccounts_object_EJK88.AddRange(fund11_glAccounts);

            allGlAccounts.AddRange(glAccounts_object_EJK88);

            allGlAccountLineItems = PopulateFinanceQueryGlAccountLineItems(allGlAccounts);

        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;

            testGlUserRepository = null;
            generalLedgerUser = null;
            testFinanceQueryRepository = null;
            testGlConfigurationRepository = null;
            testGlAccountStructure = null;
            testGlClassConfiguration = null;
        }
        #endregion

        #region GetGLAccountsListAsync
        [TestMethod]
        public async Task GetGLAccountsListAsync_NoSortSubtotals_Success()
        {
            //Act
            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGlUserRepository.GetGeneralLedgerUserAsync(glUserFactoryAll.CurrentUser.PersonId,
                testGlAccountStructure.FullAccessRole, testGlClassConfiguration.ClassificationName,
                testGlClassConfiguration.ExpenseClassValues);

            // Get the list of gl accounts domain entities from the test repository.
            var financeQueryEntityList = await testFinanceQueryRepository.GetGLAccountsListAsync(generalLedgerUser,
                testGlAccountStructure, testGlClassConfiguration, filterCriteriaEntity, null);

            // Assert that the correct number of gl accounts is return.
            var financeQueryDtos = financeQueryList.ToList();
            financeQueryEntityList = financeQueryEntityList.ToList();
            Assert.AreEqual(financeQueryDtos.Count(), 1);
            Assert.IsNotNull(financeQueryEntityList);


            //Assert that each finance query DTO is represented in the data from the repository.
            foreach (var financeQueryDto in financeQueryDtos)
            {
                Assert.IsNotNull(financeQueryDto);
                Assert.AreEqual(financeQueryEntityList.Where(x => x.IsUmbrellaVisible).Select(s => s.GlAccount).Sum(x => x.BudgetAmount), financeQueryDto.TotalBudget);
                Assert.AreEqual(financeQueryEntityList.Where(x => x.IsUmbrellaVisible).Select(s => s.GlAccount).Sum(x => x.ActualAmount), financeQueryDto.TotalActuals);
                Assert.AreEqual(financeQueryEntityList.Where(x => x.IsUmbrellaVisible).Select(s => s.GlAccount).Sum(x => x.EncumbranceAmount), financeQueryDto.TotalEncumbrances);
                Assert.AreEqual(financeQueryEntityList.Where(x => x.IsUmbrellaVisible).Select(s => s.GlAccount).Sum(x => x.RequisitionAmount), financeQueryDto.TotalRequisitions);

                Assert.IsNotNull(financeQueryDto.SubTotals);
                Assert.AreEqual(1, financeQueryDto.SubTotals.Count);
                Assert.IsNotNull(financeQueryDto.SubTotals.First().FinanceQueryGlAccountLineItems);
                Assert.IsTrue(financeQueryDto.SubTotals.First().FinanceQueryGlAccountLineItems.Count > 0);
                foreach (var dtoGlAccount in financeQueryDto.SubTotals.First().FinanceQueryGlAccountLineItems)
                {
                    Assert.IsNotNull(dtoGlAccount.GlAccount);
                    var result = financeQueryEntityList.FirstOrDefault(x => x.GlAccountNumber == dtoGlAccount.GlAccount.GlAccountNumber);
                    Assert.IsNotNull(result);
                    var entityGlAccount = result.GlAccount;
                    Assert.IsNotNull(entityGlAccount);
                    if (dtoGlAccount.IsUmbrellaVisible)
                    {
                        Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.GlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.GlAccount.Budget);
                        Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.GlAccount.Encumbrances);
                    }
                    else
                    {
                        Assert.AreEqual(0, dtoGlAccount.GlAccount.Actuals);
                        Assert.AreEqual(0, dtoGlAccount.GlAccount.Budget);
                        Assert.AreEqual(0, dtoGlAccount.GlAccount.Encumbrances);
                    }
                    Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.GlAccount.FormattedGlAccount);
                    Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.GlAccount.Description);
                }

            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_Subtotals_Success()
        {
            filterCriteriaDto.ComponentSortCriteria = new List<dtoDomain.FinanceQueryComponentSortCriteria>();
            filterCriteriaDto.ComponentSortCriteria.Add(new dtoDomain.FinanceQueryComponentSortCriteria() { ComponentName = TestGlAccountRepository.UNIT_CODE, IsDisplaySubTotal = true, Order = 1 });
            filterCriteriaDto.ComponentSortCriteria.Add(new dtoDomain.FinanceQueryComponentSortCriteria() { ComponentName = TestGlAccountRepository.FUND_CODE, IsDisplaySubTotal = true, Order = 2 });
            filterCriteriaDto.ComponentSortCriteria.Add(new dtoDomain.FinanceQueryComponentSortCriteria() { ComponentName = TestGlAccountRepository.OBJECT_CODE, IsDisplaySubTotal = true, Order = 3 });

            Mock<IFinanceQueryRepository> financeQueryRepository = new Mock<IFinanceQueryRepository>();
            financeQueryRepository.Setup(x => x.GetGLAccountsListAsync(It.IsAny<GeneralLedgerUser>(), It.IsAny<GeneralLedgerAccountStructure>(), It.IsAny<GeneralLedgerClassConfiguration>(), It.IsAny<FinanceQueryCriteria>(), It.IsAny<string>())).Returns(() =>
                {
                    return Task.FromResult(allGlAccountLineItems.AsEnumerable());
                });
            BuildService(financeQueryRepository.Object, testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository);

            //Act
            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);

            Assert.IsNotNull(financeQueryList);
            Assert.IsTrue(financeQueryList.Count() == 1);

            dtoDomain.FinanceQuery result = financeQueryList.First();
            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SubTotals);
            Assert.IsTrue(result.SubTotals.Count == 5);

            var subtotal = result.SubTotals[0];
            //Sub totals OBJECT - 10000 / FUND - 10 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.SubtotalComponents.Count == 1);

            var subtotalComponent = subtotal.SubtotalComponents.First();
            AssertDtoSubtotalComponent(glAccount_10_EJK88_10000, "Object", "10000", subtotalComponent);

            subtotal = result.SubTotals[1];
            //Sub totals OBJECT - 20000 / FUND - 10 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.SubtotalComponents.Count == 1);

            subtotalComponent = subtotal.SubtotalComponents.First();
            AssertDtoSubtotalComponent(glAccount_10_EJK88_20000, "Object", "20000", subtotalComponent);

            subtotal = result.SubTotals[2];
            //Sub totals OBJECT - 30000 / FUND - 10 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.SubtotalComponents.Count == 2);

            subtotalComponent = subtotal.SubtotalComponents.First();
            AssertDtoSubtotalComponent(glAccount_10_EJK88_30000, "Object", "30000", subtotalComponent);

            subtotalComponent = subtotal.SubtotalComponents.Skip(1).Single();
            //Sub totals OBJECT - FUND - 10 / OBJECT EJK88
            AssertDtoSubtotalComponents(fund10_glAccounts, "Fund", "10", subtotalComponent);

            subtotal = result.SubTotals[3];
            //Sub totals OBJECT - 10000 / FUND - 11 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.SubtotalComponents.Count == 1);

            subtotalComponent = subtotal.SubtotalComponents.First();
            AssertDtoSubtotalComponent(glAccount_11_EJK88_10000, "Object", "10000", subtotalComponent);

            subtotal = result.SubTotals[4];
            //Sub totals OBJECT - 20000 / FUND - 11 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.SubtotalComponents.Count == 3);

            subtotalComponent = subtotal.SubtotalComponents.First();
            AssertDtoSubtotalComponent(glAccount_11_EJK88_20000, "Object", "20000", subtotalComponent);

            subtotalComponent = subtotal.SubtotalComponents.Skip(1).First();
            //Sub totals OBJECT - FUND - 11 / OBJECT EJK88
            AssertDtoSubtotalComponents(fund11_glAccounts, "Fund", "11", subtotalComponent);

            subtotalComponent = subtotal.SubtotalComponents.Skip(2).First();
            //Sub totals OBJECT - OBJECT EJK88            
            AssertDtoSubtotalComponents(glAccounts_object_EJK88, "Unit", "EJK88", subtotalComponent);

            //Grand totals
            AssertDtoGrandTotals(allGlAccounts, result);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NoDescription()
        {
            Dictionary<string, string> descDictionary = new Dictionary<string, string>();
            var glAccountRepository = new Mock<IGeneralLedgerAccountRepository>();
            glAccountRepository.Setup(x => x.GetGlAccountDescriptionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<GeneralLedgerAccountStructure>())).Returns(() =>
            {
                return Task.FromResult(descDictionary);
            });


            BuildService(testFinanceQueryRepository, testGlUserRepository, testGlConfigurationRepository, glAccountRepository.Object);
            //Act
            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGlUserRepository.GetGeneralLedgerUserAsync(glUserFactoryAll.CurrentUser.PersonId,
                testGlAccountStructure.FullAccessRole, testGlClassConfiguration.ClassificationName,
                testGlClassConfiguration.ExpenseClassValues);

            // Get the list of gl accounts domain entities from the test repository.
            var financeQueryEntityList = await testFinanceQueryRepository.GetGLAccountsListAsync(generalLedgerUser,
                testGlAccountStructure, testGlClassConfiguration, filterCriteriaEntity, null);

            // Assert that the correct number of gl accounts is return.
            var financeQueryDtos = financeQueryList.ToList();
            financeQueryEntityList = financeQueryEntityList.ToList();
            Assert.AreEqual(financeQueryDtos.Count(), 1);
            Assert.IsNotNull(financeQueryEntityList);

            //Assert that each finance query DTO is represented in the data from the repository.
            foreach (var financeQueryDto in financeQueryDtos)
            {
                Assert.IsNotNull(financeQueryDto);

                Assert.IsNotNull(financeQueryDto.SubTotals);
                Assert.AreEqual(1, financeQueryDto.SubTotals.Count);
                Assert.IsNotNull(financeQueryDto.SubTotals.First().FinanceQueryGlAccountLineItems);
                Assert.IsTrue(financeQueryDto.SubTotals.First().FinanceQueryGlAccountLineItems.Count > 0);
                foreach (var dtoGlAccount in financeQueryDto.SubTotals.First().FinanceQueryGlAccountLineItems)
                {
                    Assert.IsNotNull(dtoGlAccount.GlAccount);
                    var result = financeQueryEntityList.FirstOrDefault(x => x.GlAccountNumber == dtoGlAccount.GlAccount.GlAccountNumber);
                    Assert.IsNotNull(result);
                    Assert.IsNotNull(result);
                    var entityGlAccount = result.GlAccount;
                    Assert.AreEqual(string.Empty, dtoGlAccount.GlAccount.Description);
                }
            }
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_RepositoryReturnsNull()
        {
            //Arrange
            Mock<IFinanceQueryRepository> financeQueryRepository = new Mock<IFinanceQueryRepository>();
            financeQueryRepository.Setup(x => x.GetGLAccountsListAsync(It.IsAny<GeneralLedgerUser>(), It.IsAny<GeneralLedgerAccountStructure>(), It.IsAny<GeneralLedgerClassConfiguration>(), It.IsAny<FinanceQueryCriteria>(), It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            BuildService(financeQueryRepository.Object, testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository);

            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);

            //Assert            
            var financeQueryDtos = financeQueryList.ToList();
            Assert.IsNotNull(financeQueryDtos);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_RepositoryReturnsEmptyList()
        {
            //Arrange
            var glAccountLineItems = new List<FinanceQueryGlAccountLineItem>();
            Mock<IFinanceQueryRepository> financeQueryRepository = new Mock<IFinanceQueryRepository>();
            financeQueryRepository.Setup(x => x.GetGLAccountsListAsync(It.IsAny<GeneralLedgerUser>(), It.IsAny<GeneralLedgerAccountStructure>(), It.IsAny<GeneralLedgerClassConfiguration>(), It.IsAny<FinanceQueryCriteria>(), It.IsAny<string>())).ReturnsAsync(glAccountLineItems);

            BuildService(financeQueryRepository.Object, testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository);

            //Act
            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);

            //Assert            
            var financeQueryDtos = financeQueryList.ToList();
            Assert.IsNotNull(financeQueryDtos);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NoGlAccess()
        {
            GeneralLedgerUser glUser = await testGlUserRepository.GetGeneralLedgerUserAsync2("9999999", null, null);
            var glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });


            BuildService(testFinanceQueryRepository, glUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository);
            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);
            Assert.AreEqual(0, financeQueryList.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ValidateSortCriteria_Success()
        {
            var sortCriteria1 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria1.ComponentName = "FUND";

            var sortComponentCriteriaDto = new List<Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria>() { sortCriteria1 };

            filterCriteriaDto = new Dtos.ColleagueFinance.FinanceQueryCriteria();
            filterCriteriaDto.FiscalYear = testGlConfigurationRepository.StartYear.ToString();
            filterCriteriaDto.IncludeActiveAccountsWithNoActivity = true;

            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);
            Assert.IsNotNull(financeQueryList.Count());
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_ValidateSortCriteria_NullSortCriteria()
        {
            filterCriteriaDto = new Dtos.ColleagueFinance.FinanceQueryCriteria();
            filterCriteriaDto.FiscalYear = testGlConfigurationRepository.StartYear.ToString();
            filterCriteriaDto.IncludeActiveAccountsWithNoActivity = true;
            filterCriteriaDto.ComponentCriteria = componentCriteriaDto;
            filterCriteriaDto.ComponentSortCriteria = null;

            var financeQueryList = await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);
            Assert.IsNotNull(financeQueryList.Count());
        }

        #endregion

        #region QueryFinanceQueryDetailSelectionByPostAsync (CSV)

        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_Success()
        {
            //Act
            var financeQueryList = await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGlUserRepository.GetGeneralLedgerUserAsync(glUserFactoryAll.CurrentUser.PersonId,
                testGlAccountStructure.FullAccessRole, testGlClassConfiguration.ClassificationName,
                testGlClassConfiguration.ExpenseClassValues);

            // Get the list of gl accounts domain entities from the test repository.
            var financeQueryEntityList = await testFinanceQueryRepository.GetFinanceQueryActivityDetailAsync(generalLedgerUser,
                testGlAccountStructure, testGlClassConfiguration, filterCriteriaEntity, null);

            // Assert that the correct number of gl accounts is return.
            var financeQueryDtos = financeQueryList.ToList();
            financeQueryEntityList = financeQueryEntityList.ToList();
            Assert.AreEqual(financeQueryDtos.Count(), 1);
            Assert.IsNotNull(financeQueryEntityList);


        }

        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_RepositoryReturnsNull()
        {
            //Arrange
            Mock<IFinanceQueryRepository> financeQueryRepository = new Mock<IFinanceQueryRepository>();
            financeQueryRepository.Setup(x => x.GetGLAccountsListAsync(It.IsAny<GeneralLedgerUser>(), It.IsAny<GeneralLedgerAccountStructure>(), It.IsAny<GeneralLedgerClassConfiguration>(), It.IsAny<FinanceQueryCriteria>(), It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            BuildService(financeQueryRepository.Object, testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository);

            var financeQueryList = await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);

            //Assert            
            var financeQueryDtos = financeQueryList.ToList();
            Assert.IsNotNull(financeQueryDtos);
        }

        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_RepositoryReturnsEmptyList()
        {
            //Arrange
            var glAccountLineItems = new List<FinanceQueryGlAccountLineItem>();
            Mock<IFinanceQueryRepository> financeQueryRepository = new Mock<IFinanceQueryRepository>();
            financeQueryRepository.Setup(x => x.GetGLAccountsListAsync(It.IsAny<GeneralLedgerUser>(), It.IsAny<GeneralLedgerAccountStructure>(), It.IsAny<GeneralLedgerClassConfiguration>(), It.IsAny<FinanceQueryCriteria>(), It.IsAny<string>())).ReturnsAsync(glAccountLineItems);

            BuildService(financeQueryRepository.Object, testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository);

            //Act
            var financeQueryList = await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);

            //Assert            
            var financeQueryDtos = financeQueryList.ToList();
            Assert.IsNotNull(financeQueryDtos);
        }

        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_NoGlAccess()
        {
            GeneralLedgerUser glUser = await testGlUserRepository.GetGeneralLedgerUserAsync2("9999999", null, null);
            var glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });


            BuildService(testFinanceQueryRepository, glUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository);
            var financeQueryList = await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);
            Assert.AreEqual(0, financeQueryList.Count());
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_NullCriteria()
        {
            await service.QueryFinanceQueryDetailSelectionByPostAsync(null);
        }

        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_NullAccountStructure()
        {
            GeneralLedgerAccountStructure accountStructure = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            glConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).Returns(() =>
            {
                return Task.FromResult(accountStructure);
            });

            var expectedMessage = "GL account structure is not set up.";
            var actualMessage = "";
            try
            {
                BuildService(testFinanceQueryRepository, testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository);
                await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_NullGlClassConfiguration()
        {
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            var accountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            glConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).Returns(() =>
            {
                return Task.FromResult(accountStructure);
            });

            glConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(glClassConfiguration);
            });

            var expectedMessage = "GL class configuration is not set up.";
            var actualMessage = "";
            try
            {
                BuildService(testFinanceQueryRepository, testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository);
                await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_NullGlUser()
        {
            GeneralLedgerUser glUser = null;
            var glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });

            var expectedMessage = "No GL user definition available.";
            var actualMessage = "";
            try
            {
                BuildService(testFinanceQueryRepository, glUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository);
                await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_ValidateSortCriteria_InvalidSortCriteria()
        {
            var sortCriteria = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria.ComponentName = "Dummy";

            var sortComponentCriteriaDto = new List<Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria>() { sortCriteria };

            filterCriteriaDto = new Dtos.ColleagueFinance.FinanceQueryCriteria();
            filterCriteriaDto.FiscalYear = testGlConfigurationRepository.StartYear.ToString();
            filterCriteriaDto.IncludeActiveAccountsWithNoActivity = true;
            filterCriteriaDto.ComponentCriteria = componentCriteriaDto;
            filterCriteriaDto.ComponentSortCriteria = sortComponentCriteriaDto;

            await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);

        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task QueryFinanceQueryDetailSelectionByPostAsync_ValidateSortCriteria_SortCriteria_Morethan3()
        {
            var sortCriteria1 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria1.ComponentName = "FUND";
            var sortCriteria2 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria2.ComponentName = "OBJECT";
            var sortCriteria3 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria3.ComponentName = "LOCATION_SUBCLASS";
            var sortCriteria4 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria4.ComponentName = "PROGRAM";

            var sortComponentCriteriaDto = new List<Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria>() { sortCriteria1, sortCriteria2, sortCriteria3, sortCriteria4 };

            filterCriteriaDto = new Dtos.ColleagueFinance.FinanceQueryCriteria()
            {
                FiscalYear = testGlConfigurationRepository.StartYear.ToString(),
                IncludeActiveAccountsWithNoActivity = true,
                ComponentCriteria = componentCriteriaDto,
                ComponentSortCriteria = sortComponentCriteriaDto
            };

            await service.QueryFinanceQueryDetailSelectionByPostAsync(filterCriteriaDto);

        }

        #endregion

        #region GetGLAccountsListAsync error checking

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullCriteria()
        {
            var expectedParamName = "criteria";
            var actualParamName = "";
            try
            {
                await service.QueryFinanceQuerySelectionByPostAsync(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullAccountStructure()
        {
            GeneralLedgerAccountStructure accountStructure = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            glConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).Returns(() =>
            {
                return Task.FromResult(accountStructure);
            });

            var expectedMessage = "GL account structure is not set up.";
            var actualMessage = "";
            try
            {
                BuildService(testFinanceQueryRepository, testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository);
                await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullGlClassConfiguration()
        {
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            var accountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            glConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).Returns(() =>
            {
                return Task.FromResult(accountStructure);
            });

            glConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(glClassConfiguration);
            });

            var expectedMessage = "GL class configuration is not set up.";
            var actualMessage = "";
            try
            {
                BuildService(testFinanceQueryRepository, testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository);
                await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetGLAccountsListAsync_NullGlUser()
        {
            GeneralLedgerUser glUser = null;
            var glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });

            var expectedMessage = "No GL user definition available.";
            var actualMessage = "";
            try
            {
                BuildService(testFinanceQueryRepository, glUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository);
                await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task GetGLAccountsListAsync_ValidateSortCriteria_InvalidSortCriteria()
        {
            var sortCriteria = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria.ComponentName = "Dummy";

            var sortComponentCriteriaDto = new List<Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria>() { sortCriteria };

            filterCriteriaDto = new Dtos.ColleagueFinance.FinanceQueryCriteria();
            filterCriteriaDto.FiscalYear = testGlConfigurationRepository.StartYear.ToString();
            filterCriteriaDto.IncludeActiveAccountsWithNoActivity = true;
            filterCriteriaDto.ComponentCriteria = componentCriteriaDto;
            filterCriteriaDto.ComponentSortCriteria = sortComponentCriteriaDto;

            await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);

        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task GetGLAccountsListAsync_ValidateSortCriteria_SortCriteria_Morethan3()
        {
            var sortCriteria1 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria1.ComponentName = "FUND";
            var sortCriteria2 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria2.ComponentName = "OBJECT";
            var sortCriteria3 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria3.ComponentName = "LOCATION_SUBCLASS";
            var sortCriteria4 = new Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria();
            sortCriteria4.ComponentName = "PROGRAM";

            var sortComponentCriteriaDto = new List<Dtos.ColleagueFinance.FinanceQueryComponentSortCriteria>() { sortCriteria1, sortCriteria2, sortCriteria3, sortCriteria4 };

            filterCriteriaDto = new Dtos.ColleagueFinance.FinanceQueryCriteria()
            {
                FiscalYear = testGlConfigurationRepository.StartYear.ToString(),
                IncludeActiveAccountsWithNoActivity = true,
                ComponentCriteria = componentCriteriaDto,
                ComponentSortCriteria = sortComponentCriteriaDto
            };

            await service.QueryFinanceQuerySelectionByPostAsync(filterCriteriaDto);

        }

        #endregion

        #region GetUniqueSortCriteria

        [TestMethod]
        public void GetUniqueSortCriteria_EmptyList_Test()
        {
            List<FinanceQueryComponentSortCriteria> inputList = new List<FinanceQueryComponentSortCriteria>();

            inputList = new List<FinanceQueryComponentSortCriteria>();
            var outCome = service.GetUniqueSortCriteria(inputList);
            Assert.IsFalse(outCome.Any());
        }

        [TestMethod]
        public void GetUniqueSortCriteria_DuplicateComponentName_IsSubtotal_Test()
        {
            List<FinanceQueryComponentSortCriteria> inputList = new List<FinanceQueryComponentSortCriteria>();
            inputList = new List<FinanceQueryComponentSortCriteria>();
            FinanceQueryComponentSortCriteria criteria_1 = new FinanceQueryComponentSortCriteria("FUND", 1, null);
            FinanceQueryComponentSortCriteria criteria_2 = new FinanceQueryComponentSortCriteria("FUND.GROUP", 2, true);
            FinanceQueryComponentSortCriteria criteria_3 = new FinanceQueryComponentSortCriteria("SUBCATEGORY", 3, false);

            inputList.Add(criteria_1);
            inputList.Add(criteria_2);
            inputList.Add(criteria_3);
            var outCome = service.GetUniqueSortCriteria(inputList);

            Assert.IsTrue(outCome.Any());
            Assert.AreEqual(3, outCome.Count());
        }

        [TestMethod]
        public void GetUniqueSortCriteria_DuplicateComponentName_IsSubtotal_1_Test()
        {
            List<FinanceQueryComponentSortCriteria> inputList = new List<FinanceQueryComponentSortCriteria>();
            FinanceQueryComponentSortCriteria criteria_1 = new FinanceQueryComponentSortCriteria("OBJECT", 1, null);
            FinanceQueryComponentSortCriteria criteria_2 = new FinanceQueryComponentSortCriteria("OBJECT", 2, false);
            FinanceQueryComponentSortCriteria criteria_3 = new FinanceQueryComponentSortCriteria("OBJECT", 3, true);

            inputList.Add(criteria_1);
            inputList.Add(criteria_2);
            inputList.Add(criteria_3);
            var outCome = service.GetUniqueSortCriteria(inputList);

            inputList.Add(criteria_2);
            inputList.Add(criteria_3);
            outCome = service.GetUniqueSortCriteria(inputList);

            Assert.IsTrue(outCome.Any());
            Assert.AreEqual(1, outCome.Count());
            Assert.AreEqual(3, outCome.Where(x => x.ComponentName == "OBJECT").Select(y => y.Order).First());
            Assert.IsTrue(outCome.Where(x => x.ComponentName == "OBJECT").Select(y => y.IsDisplaySubTotal).First().Value);

            inputList = new List<FinanceQueryComponentSortCriteria>();
            criteria_1 = new FinanceQueryComponentSortCriteria("OBJECT", 1, false);
            criteria_2 = new FinanceQueryComponentSortCriteria("OBJECT", 2, null);
            criteria_3 = new FinanceQueryComponentSortCriteria("OBJECT", 3, false);

            inputList.Add(criteria_1);
            inputList.Add(criteria_2);
            inputList.Add(criteria_3);
            outCome = service.GetUniqueSortCriteria(inputList);

            Assert.IsTrue(outCome.Any());
            Assert.AreEqual(1, outCome.Count());
            Assert.AreEqual(1, outCome.Where(x => x.ComponentName == "OBJECT").Select(y => y.Order).First());
            Assert.IsFalse(outCome.Where(x => x.ComponentName == "OBJECT").Select(y => y.IsDisplaySubTotal).First().Value);

        }

        [TestMethod]
        public void GetUniqueSortCriteria_DuplicateComponentName_IsSubtotal_2_Test()
        {
            List<FinanceQueryComponentSortCriteria> inputList = new List<FinanceQueryComponentSortCriteria>();


            inputList = new List<FinanceQueryComponentSortCriteria>();
            FinanceQueryComponentSortCriteria criteria_1 = new FinanceQueryComponentSortCriteria("OBJECT", 1, false);
            FinanceQueryComponentSortCriteria criteria_2 = new FinanceQueryComponentSortCriteria("FUND", 2, false);
            FinanceQueryComponentSortCriteria criteria_3 = new FinanceQueryComponentSortCriteria("OBJECT", 3, false);

            inputList.Add(criteria_1);
            inputList.Add(criteria_2);
            inputList.Add(criteria_3);
            var outCome = service.GetUniqueSortCriteria(inputList);

            Assert.IsTrue(outCome.Any());
            Assert.AreEqual(2, outCome.Count());
            Assert.AreEqual(1, outCome.Where(x => x.ComponentName == "OBJECT").Select(y => y.Order).First());



            inputList = new List<FinanceQueryComponentSortCriteria>();
            criteria_1 = new FinanceQueryComponentSortCriteria("DEPARTMENT", 1, true);
            criteria_2 = new FinanceQueryComponentSortCriteria("FUND.GROUP", 2, false);
            criteria_3 = new FinanceQueryComponentSortCriteria("FUND.GROUP", 3, true);

            inputList.Add(criteria_1);
            inputList.Add(criteria_2);
            inputList.Add(criteria_3);
            outCome = service.GetUniqueSortCriteria(inputList);
            Assert.IsTrue(outCome.Any());
            Assert.AreEqual(2, outCome.Count());
            Assert.IsTrue(outCome.Count(x => x.IsDisplaySubTotal.HasValue && x.IsDisplaySubTotal.Value) == 2);
            Assert.AreEqual("DEPARTMENT", outCome[0].ComponentName);
            Assert.AreEqual("FUND.GROUP", outCome[1].ComponentName);

        }

        [TestMethod]
        public void GetUniqueSortCriteria_Test()
        {
            List<FinanceQueryComponentSortCriteria> inputList = new List<FinanceQueryComponentSortCriteria>();

            inputList = new List<FinanceQueryComponentSortCriteria>();
            FinanceQueryComponentSortCriteria criteria_1 = new FinanceQueryComponentSortCriteria("FUND", 1, null);
            FinanceQueryComponentSortCriteria criteria_2 = new FinanceQueryComponentSortCriteria("FUND.GROUP", 2, true);
            FinanceQueryComponentSortCriteria criteria_3 = new FinanceQueryComponentSortCriteria("SUBCATEGORY", 3, false);

            inputList.Add(criteria_1);
            inputList.Add(criteria_2);
            inputList.Add(criteria_3);
            var outCome = service.GetUniqueSortCriteria(inputList);

            Assert.IsTrue(outCome.Any());
            Assert.AreEqual(3, outCome.Count());
            Assert.IsTrue(outCome.Any(x => x.Order == 1));
            Assert.AreEqual("FUND", outCome.First(x => x.Order == 1).ComponentName);
        }

        [TestMethod]
        public void GetUniqueSortCriteria_SingleSortCriteria_Test()
        {
            List<FinanceQueryComponentSortCriteria> inputList = new List<FinanceQueryComponentSortCriteria>();

            inputList = new List<FinanceQueryComponentSortCriteria>();
            FinanceQueryComponentSortCriteria criteria_1 = new FinanceQueryComponentSortCriteria("FUND", 1, true);

            inputList.Add(criteria_1);
            var outCome = service.GetUniqueSortCriteria(inputList);

            Assert.IsTrue(outCome.Any());
            Assert.AreEqual(1, outCome.Count());
            Assert.AreEqual("FUND", outCome.First().ComponentName);

        }

        #endregion

        #region BuildFinanceQueryWithSubtotalEntities

        /// <summary>
        /// Subtotal by one gl component and no gl accounts from repository
        /// </summary>
        [TestMethod]
        public void BuildFinanceQueryWithSubtotalEntities_Subtotal_NoGlAccounts()
        {
            List<FinanceQueryGlAccountLineItem> allGlAccounts = new List<FinanceQueryGlAccountLineItem>();
            sortCriteria = new List<FinanceQueryComponentSortCriteria>();
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.FUND_CODE, 1, true));
            Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary = new Dictionary<string, List<string>>();
            //Act
            var result = service.BuildFinanceQueryWithSubtotalEntities(allGlAccounts, sortCriteria, param2_glAccountStructure, glComponentStructureDict, subtotalCriteriaComponentValuesDictionary);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FinanceQuerySubtotals);
            Assert.IsTrue(result.FinanceQuerySubtotals.Count == 0);
        }

        /// <summary>
        /// Subtotal by one gl component and empty gl acomponent structure dictionary
        /// </summary>
        [ExpectedException(typeof(ApplicationException))]
        [TestMethod]
        public void BuildFinanceQueryWithSubtotalEntities_Subtotal_EmptyGlAcomponentDictionary()
        {            
            sortCriteria = new List<FinanceQueryComponentSortCriteria>();
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.FUND_CODE, 1, true));
            Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary = new Dictionary<string, List<string>>();
            //Act
            var result = service.BuildFinanceQueryWithSubtotalEntities(allGlAccountLineItems, sortCriteria, param2_glAccountStructure, new Dictionary<string, Tuple<int, int>>(), subtotalCriteriaComponentValuesDictionary);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FinanceQuerySubtotals);
            Assert.IsTrue(result.FinanceQuerySubtotals.Count == 0);
        }

        /// <summary>
        /// Subtotal by invalid gl component
        /// </summary>
        [ExpectedException(typeof(ApplicationException))]
        [TestMethod]
        public void BuildFinanceQueryWithSubtotalEntities_Subtotal_IvalidSubtotalComponent()
        {
            sortCriteria = new List<FinanceQueryComponentSortCriteria>();
            sortCriteria.Add(new FinanceQueryComponentSortCriteria("Dummy", 1, true));
            Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary = new Dictionary<string, List<string>>();
            //Act
            var result = service.BuildFinanceQueryWithSubtotalEntities(allGlAccountLineItems, sortCriteria, param2_glAccountStructure, glComponentStructureDict, subtotalCriteriaComponentValuesDictionary);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FinanceQuerySubtotals);
            Assert.IsTrue(result.FinanceQuerySubtotals.Count == 0);
        }


        /// <summary>
        /// Subtotal by one gl component
        /// </summary>
        [TestMethod]
        public void BuildFinanceQueryWithSubtotalEntities_Subtotal_One_Component()
        {
            sortCriteria = new List<FinanceQueryComponentSortCriteria>();
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.FUND_CODE, 1, true));
            Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary = new Dictionary<string, List<string>>();

            //Act
            var result = service.BuildFinanceQueryWithSubtotalEntities(allGlAccountLineItems, sortCriteria, param2_glAccountStructure, glComponentStructureDict, subtotalCriteriaComponentValuesDictionary);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FinanceQuerySubtotals);
            Assert.IsTrue(result.FinanceQuerySubtotals.Count == 2);

            var subtotal = result.FinanceQuerySubtotals[0];
            //Sub totals FUND - 10
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 3);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 1);

            var subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponents(fund10_glAccounts, "Fund", "10", subtotalComponent);


            subtotal = result.FinanceQuerySubtotals[1];
            //Sub totals FUND - 11
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 2);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 1);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponents(fund11_glAccounts, "Fund", "11", subtotalComponent);

            //Grand totals
            AssertGrandTotals(allGlAccounts, result);
        }

        /// <summary>
        /// Subtotal by two gl components
        /// </summary>
        [TestMethod]
        public void BuildFinanceQueryWithSubtotalEntities_Subtotal_Two_Components()
        {
            sortCriteria = new List<FinanceQueryComponentSortCriteria>();
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.UNIT_CODE, 1, true));
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.FUND_CODE, 2, true));
            Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary = new Dictionary<string, List<string>>();

            //Act
            var result = service.BuildFinanceQueryWithSubtotalEntities(allGlAccountLineItems, sortCriteria, param2_glAccountStructure, glComponentStructureDict, subtotalCriteriaComponentValuesDictionary);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FinanceQuerySubtotals);
            Assert.IsTrue(result.FinanceQuerySubtotals.Count == 2);

            var subtotal = result.FinanceQuerySubtotals[0];
            //Sub totals Fund - 10
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 3);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 1);

            var subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponents(fund10_glAccounts, "Fund", "10", subtotalComponent);

            subtotal = result.FinanceQuerySubtotals[1];
            //Sub totals Fund - 11 & Unit - EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 2);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 2);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponents(fund11_glAccounts, "Fund", "11", subtotalComponent);

            //Sub totals Unit - EJK88
            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.Skip(1).Single();
            AssertSubtotalComponents(allGlAccounts, "Unit", "EJK88", subtotalComponent);

            //Grand totals
            AssertGrandTotals(allGlAccounts, result);
        }

        /// <summary>
        /// Subtotal by three gl components
        /// </summary>
        [TestMethod]
        public void BuildFinanceQueryWithSubtotalEntities_Subtotal_Three_Components()
        {
            sortCriteria = new List<FinanceQueryComponentSortCriteria>();
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.UNIT_CODE, 1, true));
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.FUND_CODE, 2, true));
            sortCriteria.Add(new FinanceQueryComponentSortCriteria(TestGlAccountRepository.OBJECT_CODE, 3, true));
            Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary = new Dictionary<string, List<string>>();

            //Act
            var result = service.BuildFinanceQueryWithSubtotalEntities(allGlAccountLineItems, sortCriteria, param2_glAccountStructure, glComponentStructureDict, subtotalCriteriaComponentValuesDictionary);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FinanceQuerySubtotals);
            Assert.IsTrue(result.FinanceQuerySubtotals.Count == 5);

            var subtotal = result.FinanceQuerySubtotals[0];
            //Sub totals OBJECT - 10000 / FUND - 10 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 1);

            var subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponent(glAccount_10_EJK88_10000, "Object", "10000", subtotalComponent);

            subtotal = result.FinanceQuerySubtotals[1];
            //Sub totals OBJECT - 20000 / FUND - 10 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 1);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponent(glAccount_10_EJK88_20000, "Object", "20000", subtotalComponent);

            subtotal = result.FinanceQuerySubtotals[2];
            //Sub totals OBJECT - 30000 / FUND - 10 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 2);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponent(glAccount_10_EJK88_30000, "Object", "30000", subtotalComponent);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.Skip(1).Single();
            //Sub totals OBJECT - FUND - 10 / OBJECT EJK88
            AssertSubtotalComponents(fund10_glAccounts, "Fund", "10", subtotalComponent);

            subtotal = result.FinanceQuerySubtotals[3];
            //Sub totals OBJECT - 10000 / FUND - 11 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 1);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponent(glAccount_11_EJK88_10000, "Object", "10000", subtotalComponent);

            subtotal = result.FinanceQuerySubtotals[4];
            //Sub totals OBJECT - 20000 / FUND - 11 / OBJECT EJK88
            Assert.IsTrue(subtotal.FinanceQueryGlAccountLineItems.Count == 1);
            Assert.IsTrue(subtotal.FinanceQuerySubtotalComponents.Count == 3);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.First();
            AssertSubtotalComponent(glAccount_11_EJK88_20000, "Object", "20000", subtotalComponent);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.Skip(1).First();
            //Sub totals OBJECT - FUND - 11 / OBJECT EJK88
            AssertSubtotalComponents(fund11_glAccounts, "Fund", "11", subtotalComponent);

            subtotalComponent = subtotal.FinanceQuerySubtotalComponents.Skip(2).First();
            //Sub totals OBJECT - OBJECT EJK88            
            AssertSubtotalComponents(glAccounts_object_EJK88, "Unit", "EJK88", subtotalComponent);

            //Grand totals
            AssertGrandTotals(allGlAccounts, result);
        }

        #endregion

        #region Private methods

        private void AssertSubtotalComponent(FinanceQueryGlAccount glAccountExpected, string breakComponentNameExpected, string breakComponentValueExpected, FinanceQuerySubtotalComponent subtotalComponent)
        {
            Assert.AreEqual(breakComponentNameExpected, subtotalComponent.SubtotalComponentName);
            Assert.AreEqual(breakComponentValueExpected, subtotalComponent.SubtotalComponentValue);
            Assert.AreEqual(glAccountExpected.ActualAmount, subtotalComponent.SubTotalActuals);
            Assert.AreEqual(glAccountExpected.BudgetAmount, subtotalComponent.SubTotalBudget);
            Assert.AreEqual(glAccountExpected.EncumbranceAmount, subtotalComponent.SubTotalEncumbrances);
            Assert.AreEqual(glAccountExpected.RequisitionAmount, subtotalComponent.SubTotalRequisitions);
        }

        private void AssertSubtotalComponents(List<FinanceQueryGlAccount> glAccountsExpected, string breakComponentNameExpected, string breakComponentValueExpected, FinanceQuerySubtotalComponent subtotalComponent)
        {
            Assert.AreEqual(breakComponentNameExpected, subtotalComponent.SubtotalComponentName);
            Assert.AreEqual(breakComponentValueExpected, subtotalComponent.SubtotalComponentValue);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.ActualAmount), subtotalComponent.SubTotalActuals);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.BudgetAmount), subtotalComponent.SubTotalBudget);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.EncumbranceAmount), subtotalComponent.SubTotalEncumbrances);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.RequisitionAmount), subtotalComponent.SubTotalRequisitions);
        }

        private void AssertGrandTotals(List<FinanceQueryGlAccount> allGlAccounts, FinanceQuery result)
        {
            Assert.AreEqual(allGlAccounts.Sum(x => x.ActualAmount), result.FinanceQuerySubtotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.ActualAmount));
            Assert.AreEqual(allGlAccounts.Sum(x => x.BudgetAmount), result.FinanceQuerySubtotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.BudgetAmount));
            Assert.AreEqual(allGlAccounts.Sum(x => x.EncumbranceAmount), result.FinanceQuerySubtotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.EncumbranceAmount));
            Assert.AreEqual(allGlAccounts.Sum(x => x.RequisitionAmount), result.FinanceQuerySubtotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.RequisitionAmount));
        }

        private void AssertDtoSubtotalComponent(FinanceQueryGlAccount glAccountExpected, string breakComponentNameExpected, string breakComponentValueExpected, dtoDomain.FinanceQuerySubtotalComponent subtotalComponent)
        {
            Assert.AreEqual(breakComponentNameExpected, subtotalComponent.SubtotalComponentName);
            Assert.AreEqual(breakComponentValueExpected, subtotalComponent.SubtotalComponentValue);
            Assert.AreEqual(glAccountExpected.ActualAmount, subtotalComponent.SubtotalActuals);
            Assert.AreEqual(glAccountExpected.BudgetAmount, subtotalComponent.SubtotalBudget);
            Assert.AreEqual(glAccountExpected.EncumbranceAmount, subtotalComponent.SubtotalEncumbrances);
            Assert.AreEqual(glAccountExpected.RequisitionAmount, subtotalComponent.SubtotalRequisitions);
        }

        private void AssertDtoSubtotalComponents(List<FinanceQueryGlAccount> glAccountsExpected, string breakComponentNameExpected, string breakComponentValueExpected, dtoDomain.FinanceQuerySubtotalComponent subtotalComponent)
        {
            Assert.AreEqual(breakComponentNameExpected, subtotalComponent.SubtotalComponentName);
            Assert.AreEqual(breakComponentValueExpected, subtotalComponent.SubtotalComponentValue);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.ActualAmount), subtotalComponent.SubtotalActuals);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.BudgetAmount), subtotalComponent.SubtotalBudget);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.EncumbranceAmount), subtotalComponent.SubtotalEncumbrances);
            Assert.AreEqual(glAccountsExpected.Sum(x => x.RequisitionAmount), subtotalComponent.SubtotalRequisitions);
        }

        private void AssertDtoGrandTotals(List<FinanceQueryGlAccount> allGlAccounts, dtoDomain.FinanceQuery result)
        {
            Assert.AreEqual(allGlAccounts.Sum(x => x.ActualAmount), result.SubTotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.Actuals));
            Assert.AreEqual(allGlAccounts.Sum(x => x.BudgetAmount), result.SubTotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.Budget));
            Assert.AreEqual(allGlAccounts.Sum(x => x.EncumbranceAmount), result.SubTotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.Encumbrances));
            Assert.AreEqual(allGlAccounts.Sum(x => x.RequisitionAmount), result.SubTotals.SelectMany(x => x.FinanceQueryGlAccountLineItems).Select(x => x.GlAccount).Sum(x => x.Requisitions));
        }


        private FinanceQueryGlAccount PopulateFinanceQueryGlAccount(string glAccountNo, decimal actualAmount = 0.00m, decimal budgetedAmount = 0.00m, decimal encumbranceAmount = 0.00m, decimal requistionAmount = 0.00m)
        {
            FinanceQueryGlAccount glAccount = new FinanceQueryGlAccount(glAccountNo);
            glAccount.ActualAmount = actualAmount;
            glAccount.BudgetAmount = budgetedAmount;
            glAccount.EncumbranceAmount = encumbranceAmount;
            glAccount.RequisitionAmount = requistionAmount;
            return glAccount;
        }

        private List<FinanceQueryGlAccountLineItem> PopulateFinanceQueryGlAccountLineItems(List<FinanceQueryGlAccount> glAccounts)
        {
            List<FinanceQueryGlAccountLineItem> lineItems = new List<FinanceQueryGlAccountLineItem>();
            foreach (var financeQueryGlAccount in glAccounts)
            {
                lineItems.Add(PopulateFinanceQueryGlAccountLineItem(financeQueryGlAccount));
            }

            return lineItems;
        }
        private FinanceQueryGlAccountLineItem PopulateFinanceQueryGlAccountLineItem(FinanceQueryGlAccount glAccount, bool isPooledAccount = false, bool isUmbrellaVisible = true)
        {
            return new FinanceQueryGlAccountLineItem(glAccount, isPooledAccount, isUmbrellaVisible);
        }

        private Dictionary<string, Tuple<int, int>> BuildGlComponentStructureDictionary(GeneralLedgerAccountStructure generalLedgerAccountStructure)
        {
            Dictionary<string, Tuple<int, int>> glSubComponentStructureDictionary = new Dictionary<string, Tuple<int, int>>();

            foreach (var subComponent in generalLedgerAccountStructure.Subcomponents)
            {
                var key = subComponent.ComponentName;
                if (!glSubComponentStructureDictionary.ContainsKey(key))
                {
                    glSubComponentStructureDictionary.Add(key, new Tuple<int, int>(subComponent.StartPosition, subComponent.ComponentLength));
                }
            }

            return glSubComponentStructureDictionary;
        }
        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple cost center service objects.
        /// </summary>
        private void BuildService(IFinanceQueryRepository financeQueryRepository,
            IGeneralLedgerUserRepository glUserRepository,
            IGeneralLedgerConfigurationRepository glConfigurationRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository)
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            var roleRepository = new Mock<IRoleRepository>().Object;

            var loggerObject = loggerMock.Object;
            var adapterRegistryObject = adapterRegistryMock.Object;


            // Set up and mock the adapter, and setup the GetAdapter method.
            var financeQueryAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.FinanceQuery, Dtos.ColleagueFinance.FinanceQuery>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.FinanceQuery, Dtos.ColleagueFinance.FinanceQuery>()).Returns(financeQueryAdapter);

            var subTotalsAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.FinanceQuerySubtotalComponent, Dtos.ColleagueFinance.FinanceQuerySubtotalComponent>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.FinanceQuerySubtotalComponent, Dtos.ColleagueFinance.FinanceQuerySubtotalComponent>()).Returns(subTotalsAdapter);

            // Set up the current user with all cost centers and set up the service.
            service = new FinanceQueryService(financeQueryRepository, glUserRepository, glConfigurationRepository, generalLedgerAccountRepository, adapterRegistryObject, glUserFactoryAll, roleRepository, loggerObject);
        }
        #endregion
    }
}