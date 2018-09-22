// Copyright 2017 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class GlObjectCodeServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private GlObjectCodeService service = null;

        private TestGeneralLedgerUserRepository testGlUserRepository = new TestGeneralLedgerUserRepository();
        private TestGlObjectCodeRepository testGlObjectCodeRepository = new TestGlObjectCodeRepository();
        private GeneralLedgerUser generalLedgerUser;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private TestGeneralLedgerAccountRepository testGlAccountRepository = new TestGeneralLedgerAccountRepository();
        private GeneralLedgerAccountStructure testGlAccountStructure;
        private GeneralLedgerClassConfiguration testGlClassConfiguration;
        private Dtos.ColleagueFinance.CostCenterQueryCriteria filterCriteriaDto = null;
        private Domain.ColleagueFinance.Entities.CostCenterQueryCriteria filterCriteriaEntity = null;

        // Define user factories
        private GeneralLedgerUserAllAccounts glUserFactoryAll = new GeneralLedgerCurrentUser.GeneralLedgerUserAllAccounts();
        private UserFactorySubset glUserFactorySubset = new GeneralLedgerCurrentUser.UserFactorySubset();
        private UserFactoryNone glUserFactoryNone = new GeneralLedgerCurrentUser.UserFactoryNone();
        private UserFactoryNonExistant glUserFactoryNonExistant = new GeneralLedgerCurrentUser.UserFactoryNonExistant();

        [TestInitialize]
        public void Initialize()
        {
            BuildService(testGlObjectCodeRepository, testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository);
            generalLedgerUser = null;
            filterCriteriaDto = new Dtos.ColleagueFinance.CostCenterQueryCriteria()
            {
                FiscalYear = testGlConfigurationRepository.StartYear.ToString(),
                ComponentCriteria = new List<Dtos.ColleagueFinance.CostCenterComponentQueryCriteria>(),
                IncludeActiveAccountsWithNoActivity = true
            };

            var componentCriteria = new List<CostCenterComponentQueryCriteria>() { new CostCenterComponentQueryCriteria("OBJECT") };
            filterCriteriaEntity = new Domain.ColleagueFinance.Entities.CostCenterQueryCriteria(componentCriteria);
            filterCriteriaEntity.FiscalYear = testGlConfigurationRepository.StartYear.ToString();
            filterCriteriaEntity.ComponentCriteria = new List<Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria>();
            filterCriteriaEntity.IncludeActiveAccountsWithNoActivity = true;
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;

            testGlUserRepository = null;
            generalLedgerUser = null;
            testGlObjectCodeRepository = null;
            testGlConfigurationRepository = null;
            testGlAccountStructure = null;
            testGlClassConfiguration = null;
        }
        #endregion

        #region GetGlObjectCodesAsync
        [TestMethod]
        public async Task GetGlObjectCodesAsync_Success()
        {
            // Get all GL objects for the user
            var glObjectDtos = await service.QueryGlObjectCodesAsync(filterCriteriaDto);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGlUserRepository.GetGeneralLedgerUserAsync(glUserFactoryAll.CurrentUser.PersonId,
                testGlAccountStructure.FullAccessRole, testGlClassConfiguration.ClassificationName,
                testGlClassConfiguration.ExpenseClassValues);

            // Get the list of cost center domain entities from the test repository.
            var glObjectCodeEntities = await testGlObjectCodeRepository.GetGlObjectCodesAsync(generalLedgerUser,
                testGlAccountStructure, testGlClassConfiguration, filterCriteriaEntity, null);

            // Assert that the correct number of cost centers is return.
            Assert.AreEqual(glObjectDtos.Count(), glObjectCodeEntities.Count());

            // Assert that each GL object code DTO is represented in the data from the repository.
            foreach (var glObjectCodeDto in glObjectDtos)
            {
                var glObjectCodeEntity = glObjectCodeEntities.Where(x => x.Id == glObjectCodeDto.Id).FirstOrDefault();
                Assert.AreEqual(glObjectCodeEntity.Name, glObjectCodeDto.Name);
                Assert.AreEqual(glObjectCodeEntity.GlClass.ToString(), glObjectCodeDto.GlClass.ToString());
                Assert.AreEqual(glObjectCodeEntity.TotalBudget, glObjectCodeDto.TotalBudget);
                Assert.AreEqual(glObjectCodeEntity.TotalActuals, glObjectCodeDto.TotalActuals);
                Assert.AreEqual(glObjectCodeEntity.TotalEncumbrances, glObjectCodeDto.TotalEncumbrances);

                foreach (var dtoGlAccount in glObjectCodeDto.GlAccounts)
                {
                    var entityGlAccount = glObjectCodeEntity.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                    Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                    Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                    Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                }

                foreach (var poolDto in glObjectCodeDto.Pools)
                {
                    var poolEntity = glObjectCodeEntity.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                    Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);
                    Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                    Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                    Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                    Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);
                    Assert.AreEqual(poolEntity.Umbrella.PoolType.ToString(), poolDto.Umbrella.PoolType.ToString());

                    // Check the poolees
                    foreach (var pooleeDto in poolDto.Poolees)
                    {
                        var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                        Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                        Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                        Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                        Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);
                        Assert.AreEqual(poolEntity.Umbrella.PoolType.ToString(), poolDto.Umbrella.PoolType.ToString());
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_NullBudgetPools()
        {
            // Get all GL objects for the user
            var glObjectDtos = await service.QueryGlObjectCodesAsync(filterCriteriaDto);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGlUserRepository.GetGeneralLedgerUserAsync(glUserFactoryAll.CurrentUser.PersonId,
                testGlAccountStructure.FullAccessRole, testGlClassConfiguration.ClassificationName,
                testGlClassConfiguration.ExpenseClassValues);

            // Get the list of cost center domain entities from the test repository.
            var glObjectCodeEntities = await testGlObjectCodeRepository.GetGlObjectCodesAsync(generalLedgerUser,
                testGlAccountStructure, testGlClassConfiguration, filterCriteriaEntity, null);

            // Assert that the correct number of cost centers is return.
            Assert.AreEqual(glObjectDtos.Count(), glObjectCodeEntities.Count());

            // Assert that each GL object code DTO is represented in the data from the repository.
            foreach (var glObjectCodeDto in glObjectDtos)
            {
                var glObjectCodeEntity = glObjectCodeEntities.Where(x => x.Id == glObjectCodeDto.Id).FirstOrDefault();
                Assert.AreEqual(glObjectCodeEntity.Name, glObjectCodeDto.Name);
                Assert.AreEqual(glObjectCodeEntity.GlClass.ToString(), glObjectCodeDto.GlClass.ToString());
                Assert.AreEqual(glObjectCodeEntity.TotalBudget, glObjectCodeDto.TotalBudget);
                Assert.AreEqual(glObjectCodeEntity.TotalActuals, glObjectCodeDto.TotalActuals);
                Assert.AreEqual(glObjectCodeEntity.TotalEncumbrances, glObjectCodeDto.TotalEncumbrances);

                foreach (var dtoGlAccount in glObjectCodeDto.GlAccounts)
                {
                    var entityGlAccount = glObjectCodeEntity.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                    Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                    Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                    Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                }

                foreach (var poolDto in glObjectCodeDto.Pools)
                {
                    var poolEntity = glObjectCodeEntity.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                    Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);
                    Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                    Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                    Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                    Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);
                    Assert.AreEqual(poolEntity.Umbrella.PoolType.ToString(), poolDto.Umbrella.PoolType.ToString());

                    // Check the poolees
                    foreach (var pooleeDto in poolDto.Poolees)
                    {
                        var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                        Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                        Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                        Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                        Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);
                        Assert.AreEqual(poolEntity.Umbrella.PoolType.ToString(), poolDto.Umbrella.PoolType.ToString());
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_NoGlAccess()
        {
            GeneralLedgerUser glUser = await testGlUserRepository.GetGeneralLedgerUserAsync2("9999999", null, null);
            var glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });


            BuildService(testGlObjectCodeRepository, glUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository);
            var glObjectCodes = await service.QueryGlObjectCodesAsync(filterCriteriaDto);
            Assert.AreEqual(0, glObjectCodes.Count());
        }
        #endregion

        #region GetGlObjectCodesAsync error checking
        [TestMethod]
        public async Task GetGlObjectCodesAsync_NullCriteria()
        {
            var expectedParamName = "criteria";
            var actualParamName = "";
            try
            {
                await service.QueryGlObjectCodesAsync(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_NullAccountStructure()
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
                BuildService(testGlObjectCodeRepository, testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository);
                await service.QueryGlObjectCodesAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_NullGlClassConfiguration()
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
                BuildService(testGlObjectCodeRepository, testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository);
                await service.QueryGlObjectCodesAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetGlObjectCodesAsync_NullGlUser()
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
                BuildService(testGlObjectCodeRepository, glUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository);
                await service.QueryGlObjectCodesAsync(filterCriteriaDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region GetFiscalYearsAsync
        [TestMethod]
        public async Task GetFiscalYearsAsync_Success()
        {
            var fiscalYearConfiguration = await testGlConfigurationRepository.GetFiscalYearConfigurationAsync();
            var expectedFiscalYears = (await testGlConfigurationRepository.GetAllFiscalYearsAsync(fiscalYearConfiguration.FiscalYearForToday)).ToList();
            var actualFiscalYears = (await service.GetFiscalYearsAsync()).ToList();

            Assert.AreEqual(expectedFiscalYears.Count(), actualFiscalYears.Count());
            for (int i = 0; i < expectedFiscalYears.Count(); i++)
            {
                Assert.AreEqual(expectedFiscalYears[i], actualFiscalYears[i]);
            }
        }
        #endregion

        #region GetFiscalYearsAsync error checking
        [TestMethod]
        public async Task GetFiscalYearsAsync_NullFiscalYearConfiguration()
        {
            GeneralLedgerFiscalYearConfiguration fiscalYearConfiguration = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            glConfigurationRepositoryMock.Setup(x => x.GetFiscalYearConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(fiscalYearConfiguration);
            });

            var expectedMessage = "Fiscal year configuration is not set up.";
            var actualMessage = "";
            try
            {
                BuildService(testGlObjectCodeRepository, testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository);
                await service.GetFiscalYearsAsync();
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple cost center service objects.
        /// </summary>
        private void BuildService(IGlObjectCodeRepository glObjectCodeRepository,
            IGeneralLedgerUserRepository glUserRepository,
            IGeneralLedgerConfigurationRepository glConfigurationRepository,
            IGeneralLedgerAccountRepository glAccountRepository)
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var costCenterDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.GlObjectCode, Dtos.ColleagueFinance.GlObjectCode>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.GlObjectCode, Dtos.ColleagueFinance.GlObjectCode>()).Returns(costCenterDtoAdapter);

            // Set up the current user with all cost centers and set up the service.
            service = new GlObjectCodeService(glObjectCodeRepository, glUserRepository, glConfigurationRepository, glAccountRepository, adapterRegistry.Object, glUserFactoryAll, roleRepository, loggerObject);
        }
        #endregion
    }
}