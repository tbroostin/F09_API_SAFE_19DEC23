// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    /// <summary>
    /// Test that the service returns a proper list of cost centers for a GL user.
    /// </summary>
    [TestClass]
    public class CostCenterServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private CostCenterService serviceAll = null;
        private CostCenterService serviceSubset = null;
        private CostCenterService serviceNone = null;
        private CostCenterService serviceNonExistant = null;

        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;
        private TestCostCenterRepository testCostCenterRepository;
        private GeneralLedgerUser generalLedgerUser;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository;
        private TestColleagueFinanceWebConfigurationsRepository testColleagueFinanceWebConfigurationsRepository;
        private GeneralLedgerAccountStructure testGlAccountStructure;
        private CostCenterStructure testGlCostCenterStructure;
        private GeneralLedgerClassConfiguration testGlClassConfiguration;
        private IEnumerable<GeneralLedgerComponentDescription> testGlComponentDescriptions;

        // Define user factories
        private UserFactoryAll glUserFactoryAll = new GeneralLedgerCurrentUser.UserFactoryAll();
        private UserFactorySubset glUserFactorySubset = new GeneralLedgerCurrentUser.UserFactorySubset();
        private UserFactoryNone glUserFactoryNone = new GeneralLedgerCurrentUser.UserFactoryNone();
        private UserFactoryNonExistant glUserFactoryNonExistant = new GeneralLedgerCurrentUser.UserFactoryNonExistant();

        [TestInitialize]
        public void Initialize()
        {
            BuildValidCostCenterService();

            generalLedgerUser = null;
        }

        [TestCleanup]
        public void Cleanup()
        {
            serviceAll = null;
            serviceSubset = null;
            serviceNone = null;
            serviceNonExistant = null;

            testGeneralLedgerUserRepository = null;
            generalLedgerUser = null;
            testCostCenterRepository = null;
            testGlConfigurationRepository = null;
            testGlAccountStructure = null;
            testGlCostCenterStructure = null;
            testGlClassConfiguration = null;
            testGlComponentDescriptions = null;
            testColleagueFinanceWebConfigurationsRepository = null;
        }
        #endregion

        #region GetCostCentersAsync test methods
        [TestMethod]
        public async Task GetCostCentersForUser_AllNoRevenue()
        {
            // Get all cost centers for the user
            // General Ledger Current User 0000004 has all expense accounts assigned.
            string fiscalYear = "2016";
            var costCenterDtos = await serviceAll.GetAsync(fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(glUserFactoryAll.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration.ClassificationName, testGlClassConfiguration.ExpenseClassValues);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that the correct number of cost centers is return.
            Assert.IsTrue(costCenterDtos.Count() == costCenterDomainEntities.Count());

            // Assert that each cost center in the DTOs is in the cost center repository.
            foreach (var costCenterDto in costCenterDtos)
            {
                var costCenterEntity = costCenterDomainEntities.Where(x => x.Id == costCenterDto.Id).FirstOrDefault();
                Assert.AreEqual(costCenterEntity.Name, costCenterDto.Name);
                Assert.AreEqual(costCenterEntity.TotalActualsExpenses, costCenterDto.TotalActuals);
                Assert.AreEqual(costCenterEntity.TotalBudgetExpenses, costCenterDto.TotalBudget);
                Assert.AreEqual(costCenterEntity.TotalEncumbrancesExpenses, costCenterDto.TotalEncumbrances);

                foreach (var dtoSubtotal in costCenterDto.CostCenterSubtotals)
                {
                    var entitySubtotal = costCenterEntity.CostCenterSubtotals.Where(x => x.Id == dtoSubtotal.Id).FirstOrDefault();
                    Assert.AreEqual(entitySubtotal.Name, dtoSubtotal.Name);
                    Assert.AreEqual(entitySubtotal.TotalBudget, dtoSubtotal.TotalBudget);
                    Assert.AreEqual(entitySubtotal.TotalActuals, dtoSubtotal.TotalActuals);
                    Assert.AreEqual(entitySubtotal.TotalEncumbrances, dtoSubtotal.TotalEncumbrances);
                    Assert.AreEqual(entitySubtotal.GlClass.ToString(), dtoSubtotal.GlClass.ToString());

                    foreach (var dtoGlAccount in dtoSubtotal.GlAccounts)
                    {
                        var entityGlAccount = entitySubtotal.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                        Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                        Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                        Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                    }

                    foreach (var poolDto in dtoSubtotal.Pools)
                    {
                        var poolEntity = entitySubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                        Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);

                        // Check the umbrella
                        if (poolEntity.IsUmbrellaVisible)
                        {
                            Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                            Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                        }
                        else
                        {
                            Assert.AreEqual(0m, poolDto.Umbrella.Budget);
                            Assert.AreEqual(0m, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(0m, poolDto.Umbrella.Encumbrances);
                        }
                        Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);

                        var poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                        switch (poolEntity.Umbrella.PoolType)
                        {
                            case Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                break;
                            case Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                break;
                        }
                        Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);

                        // Check the poolees
                        foreach (var pooleeDto in poolDto.Poolees)
                        {
                            var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                            Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                            Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                            Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                            Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);

                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                            switch (poolEntity.Umbrella.PoolType)
                            {
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                    break;
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                    break;
                            }
                            Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersForUser_All()
        {
            // Get all cost centers for the user
            // General Ledger Current User 0000004 has all expense accounts assigned.
            string fiscalYear = "2016";
            var costCenterDtos = await serviceAll.GetAsync(fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryAll.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that the correct number of cost centers is return.
            Assert.IsTrue(costCenterDtos.Count() == costCenterDomainEntities.Count());

            // Assert that each cost center in the DTOs is in the cost center repository.
            foreach (var costCenterDto in costCenterDtos)
            {
                var costCenterEntity = costCenterDomainEntities.Where(x => x.Id == costCenterDto.Id).FirstOrDefault();
                Assert.AreEqual(costCenterEntity.Name, costCenterDto.Name);
                Assert.AreEqual(costCenterEntity.TotalActualsExpenses, costCenterDto.TotalActuals);
                Assert.AreEqual(costCenterEntity.TotalBudgetExpenses, costCenterDto.TotalBudget);
                Assert.AreEqual(costCenterEntity.TotalEncumbrancesExpenses, costCenterDto.TotalEncumbrances);
                Assert.AreEqual(costCenterEntity.TotalActualsRevenue, costCenterDto.TotalActualsRevenue);
                Assert.AreEqual(costCenterEntity.TotalBudgetRevenue, costCenterDto.TotalBudgetRevenue);
                Assert.AreEqual(costCenterEntity.TotalEncumbrancesRevenue, costCenterDto.TotalEncumbrancesRevenue);

                foreach (var dtoSubtotal in costCenterDto.CostCenterSubtotals)
                {
                    var entitySubtotal = costCenterEntity.CostCenterSubtotals.Where(x => x.Id == dtoSubtotal.Id).FirstOrDefault();
                    Assert.AreEqual(entitySubtotal.Name, dtoSubtotal.Name);
                    Assert.AreEqual(entitySubtotal.TotalBudget, dtoSubtotal.TotalBudget);
                    Assert.AreEqual(entitySubtotal.TotalActuals, dtoSubtotal.TotalActuals);
                    Assert.AreEqual(entitySubtotal.TotalEncumbrances, dtoSubtotal.TotalEncumbrances);
                    Assert.AreEqual(entitySubtotal.GlClass.ToString(), dtoSubtotal.GlClass.ToString());

                    foreach (var dtoGlAccount in dtoSubtotal.GlAccounts)
                    {
                        var entityGlAccount = entitySubtotal.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                        Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                        Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                        Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                    }

                    foreach (var poolDto in dtoSubtotal.Pools)
                    {
                        var poolEntity = entitySubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                        Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);

                        // Check the umbrella
                        if (poolEntity.IsUmbrellaVisible)
                        {
                            Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                            Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                        }
                        else
                        {
                            Assert.AreEqual(0m, poolDto.Umbrella.Budget);
                            Assert.AreEqual(0m, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(0m, poolDto.Umbrella.Encumbrances);
                        }
                        Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);

                        var poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                        switch (poolEntity.Umbrella.PoolType)
                        {
                            case Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                break;
                            case Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                break;
                        }
                        Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);

                        // Check the poolees
                        foreach (var pooleeDto in poolDto.Poolees)
                        {
                            var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                            Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                            Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                            Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                            Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);

                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                            switch (poolEntity.Umbrella.PoolType)
                            {
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                    break;
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                    break;
                            }
                            Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersForUser_All_NullFiscalYear()
        {
            // Get all cost centers for the user
            // General Ledger Current User 0000004 has all expense accounts assigned.
            string fiscalYear = null;
            var costCenterDtos = await serviceAll.GetAsync(fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryAll.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that the correct number of cost centers is return.
            Assert.IsTrue(costCenterDtos.Count() == costCenterDomainEntities.Count());

            // Assert that each cost center in the DTOs is in the cost center repository.
            foreach (var costCenterDto in costCenterDtos)
            {
                var costCenterEntity = costCenterDomainEntities.Where(x => x.Id == costCenterDto.Id).FirstOrDefault();
                Assert.AreEqual(costCenterEntity.Name, costCenterDto.Name);
                Assert.AreEqual(costCenterEntity.TotalActualsExpenses, costCenterDto.TotalActuals);
                Assert.AreEqual(costCenterEntity.TotalBudgetExpenses, costCenterDto.TotalBudget);
                Assert.AreEqual(costCenterEntity.TotalEncumbrancesExpenses, costCenterDto.TotalEncumbrances);

                foreach (var dtoSubtotal in costCenterDto.CostCenterSubtotals)
                {
                    var entitySubtotal = costCenterEntity.CostCenterSubtotals.Where(x => x.Id == dtoSubtotal.Id).FirstOrDefault();
                    Assert.AreEqual(entitySubtotal.Name, dtoSubtotal.Name);
                    Assert.AreEqual(entitySubtotal.TotalBudget, dtoSubtotal.TotalBudget);
                    Assert.AreEqual(entitySubtotal.TotalActuals, dtoSubtotal.TotalActuals);
                    Assert.AreEqual(entitySubtotal.TotalEncumbrances, dtoSubtotal.TotalEncumbrances);

                    foreach (var dtoGlAccount in dtoSubtotal.GlAccounts)
                    {
                        var entityGlAccount = entitySubtotal.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                        Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                        Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                        Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                    }

                    foreach (var poolDto in dtoSubtotal.Pools)
                    {
                        var poolEntity = entitySubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                        Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);

                        // Check the umbrella
                        if (poolEntity.IsUmbrellaVisible)
                        {
                            Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                            Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                        }
                        else
                        {
                            Assert.AreEqual(0m, poolDto.Umbrella.Budget);
                            Assert.AreEqual(0m, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(0m, poolDto.Umbrella.Encumbrances);
                        }
                        Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);

                        var poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                        switch (poolEntity.Umbrella.PoolType)
                        {
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                break;
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                break;
                        }
                        Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);

                        // Check the poolees
                        foreach (var pooleeDto in poolDto.Poolees)
                        {
                            var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                            Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                            Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                            Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                            Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);

                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                            switch (poolEntity.Umbrella.PoolType)
                            {
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                    break;
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                    break;
                            }
                            Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCentersForUser_Subset()
        {
            // General Ledger Current User 0000001 has some expense accounts assigned.
            string fiscalYear = "2016";
            var costCenterDtos = await serviceSubset.GetAsync(fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactorySubset.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that the correct number of cost centers is return.
            Assert.IsTrue(costCenterDtos.Count() == costCenterDomainEntities.Count());

            // Assert that each cost center in the DTOs is in the cost center repository.
            foreach (var costCenter in costCenterDtos)
            {
                Assert.IsTrue(costCenterDomainEntities.Any(x => x.Id == costCenter.Id));
            }
        }

        [TestMethod]
        public async Task GetCostCentersForUser_None()
        {
            // General Ledger Current User 0000002 has no expense account assigned.
            string fiscalYear = "2016";
            var costCenterDtos = await serviceNone.GetAsync(fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryNone.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that there are no cost centers for this user.
            Assert.IsTrue(costCenterDtos.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCentersForUser_NonExistant()
        {
            // General Ledger Current User 0000003 has expense accounts that don't exist.
            string fiscalYear = "2016";
            var costCenterDtos = await serviceNonExistant.GetAsync(fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryNonExistant.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that there are no cost centers for this user.
            Assert.IsTrue(costCenterDtos.Count() == 0);
        }

        [TestMethod]
        public async Task GetCostCenterAsync_All()
        {
            // Get all cost centers for the user
            // General Ledger Current User 0000004 has all expense accounts assigned.
            string fiscalYear = "2016";
            var costCenterDto = await serviceAll.GetCostCenterAsync("1", fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryAll.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterEntity = (await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "1", fiscalYear, null, null)).First();

            // Assert that each cost center in the DTOs is in the cost center repository.
            Assert.AreEqual(costCenterEntity.Id, costCenterDto.Id);
            Assert.AreEqual(costCenterEntity.Name, costCenterDto.Name);
            Assert.AreEqual(costCenterEntity.TotalActualsExpenses, costCenterDto.TotalActuals);
            Assert.AreEqual(costCenterEntity.TotalBudgetExpenses, costCenterDto.TotalBudget);
            Assert.AreEqual(costCenterEntity.TotalEncumbrancesExpenses, costCenterDto.TotalEncumbrances);

            foreach (var dtoSubtotal in costCenterDto.CostCenterSubtotals)
            {
                var entitySubtotal = costCenterEntity.CostCenterSubtotals.Where(x => x.Id == dtoSubtotal.Id).FirstOrDefault();
                Assert.AreEqual(entitySubtotal.Name, dtoSubtotal.Name);
                Assert.AreEqual(entitySubtotal.TotalBudget, dtoSubtotal.TotalBudget);
                Assert.AreEqual(entitySubtotal.TotalActuals, dtoSubtotal.TotalActuals);
                Assert.AreEqual(entitySubtotal.TotalEncumbrances, dtoSubtotal.TotalEncumbrances);

                foreach (var dtoGlAccount in dtoSubtotal.GlAccounts)
                {
                    var entityGlAccount = entitySubtotal.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                    Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                    Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                    Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                }

                foreach (var poolDto in dtoSubtotal.Pools)
                {
                    var poolEntity = entitySubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                    Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);

                    // Check the umbrella
                    if (poolEntity.IsUmbrellaVisible)
                    {
                        Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                        Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                        Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                    }
                    else
                    {
                        Assert.AreEqual(0m, poolDto.Umbrella.Budget);
                        Assert.AreEqual(0m, poolDto.Umbrella.Actuals);
                        Assert.AreEqual(0m, poolDto.Umbrella.Encumbrances);
                    }
                    Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);

                    var poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                    switch (poolEntity.Umbrella.PoolType)
                    {
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                            break;
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                            break;
                    }
                    Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);

                    // Check the poolees
                    foreach (var pooleeDto in poolDto.Poolees)
                    {
                        var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                        Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                        Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                        Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                        Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);

                        poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                        switch (poolEntity.Umbrella.PoolType)
                        {
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                break;
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                break;
                        }
                        Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetCostCenterAsync_AllNoFiscalYear()
        {
            // Get all cost centers for the user
            // General Ledger Current User 0000004 has all expense accounts assigned.
            string fiscalYear = "";
            var costCenterDto = await serviceAll.GetCostCenterAsync("1", fiscalYear);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryAll.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterEntity = (await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "1", fiscalYear, null, "0000001")).First();

            // Assert that each cost center in the DTOs is in the cost center repository.
            Assert.AreEqual(costCenterEntity.Id, costCenterDto.Id);
            Assert.AreEqual(costCenterEntity.Name, costCenterDto.Name);
            Assert.AreEqual(costCenterEntity.TotalActualsExpenses, costCenterDto.TotalActuals);
            Assert.AreEqual(costCenterEntity.TotalBudgetExpenses, costCenterDto.TotalBudget);
            Assert.AreEqual(costCenterEntity.TotalEncumbrancesExpenses, costCenterDto.TotalEncumbrances);

            foreach (var dtoSubtotal in costCenterDto.CostCenterSubtotals)
            {
                var entitySubtotal = costCenterEntity.CostCenterSubtotals.Where(x => x.Id == dtoSubtotal.Id).FirstOrDefault();
                Assert.AreEqual(entitySubtotal.Name, dtoSubtotal.Name);
                Assert.AreEqual(entitySubtotal.TotalBudget, dtoSubtotal.TotalBudget);
                Assert.AreEqual(entitySubtotal.TotalActuals, dtoSubtotal.TotalActuals);
                Assert.AreEqual(entitySubtotal.TotalEncumbrances, dtoSubtotal.TotalEncumbrances);

                foreach (var dtoGlAccount in dtoSubtotal.GlAccounts)
                {
                    var entityGlAccount = entitySubtotal.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                    Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                    Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                    Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                    Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                }

                foreach (var poolDto in dtoSubtotal.Pools)
                {
                    var poolEntity = entitySubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                    Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);

                    // Check the umbrella
                    if (poolEntity.IsUmbrellaVisible)
                    {
                        Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                        Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                        Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                    }
                    else
                    {
                        Assert.AreEqual(0m, poolDto.Umbrella.Budget);
                        Assert.AreEqual(0m, poolDto.Umbrella.Actuals);
                        Assert.AreEqual(0m, poolDto.Umbrella.Encumbrances);
                    }
                    Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);

                    var poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                    switch (poolEntity.Umbrella.PoolType)
                    {
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                            break;
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                            break;
                    }
                    Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);

                    // Check the poolees
                    foreach (var pooleeDto in poolDto.Poolees)
                    {
                        var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                        Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                        Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                        Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                        Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);

                        poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                        switch (poolEntity.Umbrella.PoolType)
                        {
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                break;
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                break;
                        }
                        Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);
                    }
                }
            }
        }
        #endregion

        #region QueryCostCentersAsync test methods

        [TestMethod]
        public async Task QueryCostCentersForUser()
        {
            // Query all cost centers for the user with a query criteria.
            // General Ledger Current User 0000004 has all expense accounts assigned.
            var componentQueryCriteria = new Dtos.ColleagueFinance.CostCenterComponentQueryCriteria()
            {
                ComponentName = "FUND",
                IndividualComponentValues = new List<string>()
                  {
                      "10"
                  },

            };
            var queryCriteria = new Dtos.ColleagueFinance.CostCenterQueryCriteria()
            {
                Ids = null,
                FiscalYear = "2016",
                ComponentCriteria = new List<Dtos.ColleagueFinance.CostCenterComponentQueryCriteria>()
                {
                    componentQueryCriteria
                }

            };
            var costCenterDtos = await serviceAll.QueryCostCentersAsync(queryCriteria);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryAll.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that the correct number of cost centers is return.
            Assert.IsTrue(costCenterDtos.Count() == costCenterDomainEntities.Count());

            // Assert that each cost center in the DTOs is in the cost center repository.
            foreach (var costCenterDto in costCenterDtos)
            {
                var costCenterEntity = costCenterDomainEntities.Where(x => x.Id == costCenterDto.Id).FirstOrDefault();
                Assert.AreEqual(costCenterEntity.Name, costCenterDto.Name);
                Assert.AreEqual(costCenterEntity.TotalActualsExpenses, costCenterDto.TotalActuals);
                Assert.AreEqual(costCenterEntity.TotalBudgetExpenses, costCenterDto.TotalBudget);
                Assert.AreEqual(costCenterEntity.TotalEncumbrancesExpenses, costCenterDto.TotalEncumbrances);

                foreach (var dtoSubtotal in costCenterDto.CostCenterSubtotals)
                {
                    var entitySubtotal = costCenterEntity.CostCenterSubtotals.Where(x => x.Id == dtoSubtotal.Id).FirstOrDefault();
                    Assert.AreEqual(entitySubtotal.Name, dtoSubtotal.Name);
                    Assert.AreEqual(entitySubtotal.TotalBudget, dtoSubtotal.TotalBudget);
                    Assert.AreEqual(entitySubtotal.TotalActuals, dtoSubtotal.TotalActuals);
                    Assert.AreEqual(entitySubtotal.TotalEncumbrances, dtoSubtotal.TotalEncumbrances);

                    foreach (var dtoGlAccount in dtoSubtotal.GlAccounts)
                    {
                        var entityGlAccount = entitySubtotal.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                        Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                        Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                        Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                    }

                    foreach (var poolDto in dtoSubtotal.Pools)
                    {
                        var poolEntity = entitySubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                        Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);

                        // Check the umbrella
                        if (poolEntity.IsUmbrellaVisible)
                        {
                            Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                            Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                        }
                        else
                        {
                            Assert.AreEqual(0m, poolDto.Umbrella.Budget);
                            Assert.AreEqual(0m, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(0m, poolDto.Umbrella.Encumbrances);
                        }
                        Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);

                        var poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                        switch (poolEntity.Umbrella.PoolType)
                        {
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                break;
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                break;
                        }
                        Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);

                        // Check the poolees
                        foreach (var pooleeDto in poolDto.Poolees)
                        {
                            var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                            Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                            Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                            Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                            Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);

                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                            switch (poolEntity.Umbrella.PoolType)
                            {
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                    break;
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                    break;
                            }
                            Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task QueryCostCentersAsync_WithCostCenter()
        {
            // Query all cost centers for the user with a query criteria.
            // General Ledger Current User 0000004 has all expense accounts assigned.
            var componentQueryCriteria = new Dtos.ColleagueFinance.CostCenterComponentQueryCriteria()
            {
                ComponentName = "FUND",
                IndividualComponentValues = new List<string>()
                  {
                      "10"
                  },

            };
            var queryCriteria = new Dtos.ColleagueFinance.CostCenterQueryCriteria()
            {
                Ids = null,
                FiscalYear = "2016",
                ComponentCriteria = new List<Dtos.ColleagueFinance.CostCenterComponentQueryCriteria>()
                {
                    componentQueryCriteria
                }

            };
            var costCenterDtos = await serviceAll.QueryCostCentersAsync(queryCriteria);

            // Assert that the correct number of cost centers is return.
            Assert.AreEqual(1, costCenterDtos.Count());
        }

        [TestMethod]
        public async Task QueryCostCentersForUser_NullFiscalYear()
        {
            // Query all cost centers for the user with a query criteria.
            // General Ledger Current User 0000004 has all expense accounts assigned.
            var componentQueryCriteria = new Dtos.ColleagueFinance.CostCenterComponentQueryCriteria()
            {
                ComponentName = "FUND",
                IndividualComponentValues = new List<string>()
                  {
                      "10"
                  },

            };
            var queryCriteria = new Dtos.ColleagueFinance.CostCenterQueryCriteria()
            {
                Ids = null,
                FiscalYear = null,
                ComponentCriteria = new List<Dtos.ColleagueFinance.CostCenterComponentQueryCriteria>()
                {
                    componentQueryCriteria
                }

            };
            var costCenterDtos = await serviceAll.QueryCostCentersAsync(queryCriteria);

            // Get the necessary configuration settings and build the GL user object.
            testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            testGlCostCenterStructure = await testGlConfigurationRepository.GetCostCenterStructureAsync();
            testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            generalLedgerUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(glUserFactoryAll.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole, testGlClassConfiguration);

            // Get the list of cost center domain entities from the test repository.
            var costCenterDomainEntities = await testCostCenterRepository.GetCostCentersAsync(generalLedgerUser, testGlCostCenterStructure, testGlClassConfiguration, "", "", null, null);

            // Assert that the correct number of cost centers is return.
            Assert.IsTrue(costCenterDtos.Count() == costCenterDomainEntities.Count());

            // Assert that each cost center in the DTOs is in the cost center repository.
            foreach (var costCenterDto in costCenterDtos)
            {
                var costCenterEntity = costCenterDomainEntities.Where(x => x.Id == costCenterDto.Id).FirstOrDefault();
                Assert.AreEqual(costCenterEntity.Name, costCenterDto.Name);
                Assert.AreEqual(costCenterEntity.TotalActualsExpenses, costCenterDto.TotalActuals);
                Assert.AreEqual(costCenterEntity.TotalBudgetExpenses, costCenterDto.TotalBudget);
                Assert.AreEqual(costCenterEntity.TotalEncumbrancesExpenses, costCenterDto.TotalEncumbrances);

                foreach (var dtoSubtotal in costCenterDto.CostCenterSubtotals)
                {
                    var entitySubtotal = costCenterEntity.CostCenterSubtotals.Where(x => x.Id == dtoSubtotal.Id).FirstOrDefault();
                    Assert.AreEqual(entitySubtotal.Name, dtoSubtotal.Name);
                    Assert.AreEqual(entitySubtotal.TotalBudget, dtoSubtotal.TotalBudget);
                    Assert.AreEqual(entitySubtotal.TotalActuals, dtoSubtotal.TotalActuals);
                    Assert.AreEqual(entitySubtotal.TotalEncumbrances, dtoSubtotal.TotalEncumbrances);

                    foreach (var dtoGlAccount in dtoSubtotal.GlAccounts)
                    {
                        var entityGlAccount = entitySubtotal.GlAccounts.Where(x => x.GlAccountNumber == dtoGlAccount.GlAccountNumber).FirstOrDefault();
                        Assert.AreEqual(entityGlAccount.ActualAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.BudgetAmount, dtoGlAccount.Actuals);
                        Assert.AreEqual(entityGlAccount.EncumbranceAmount, dtoGlAccount.Encumbrances);
                        Assert.AreEqual(entityGlAccount.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), dtoGlAccount.FormattedGlAccount);
                        Assert.AreEqual(entityGlAccount.GlAccountDescription, dtoGlAccount.Description);
                    }

                    foreach (var poolDto in dtoSubtotal.Pools)
                    {
                        var poolEntity = entitySubtotal.Pools.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolDto.Umbrella.GlAccountNumber);
                        Assert.AreEqual(poolEntity.IsUmbrellaVisible, poolDto.IsUmbrellaVisible);

                        // Check the umbrella
                        if (poolEntity.IsUmbrellaVisible)
                        {
                            Assert.AreEqual(poolEntity.Umbrella.BudgetAmount, poolDto.Umbrella.Budget);
                            Assert.AreEqual(poolEntity.Umbrella.ActualAmount, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(poolEntity.Umbrella.EncumbranceAmount, poolDto.Umbrella.Encumbrances);
                        }
                        else
                        {
                            Assert.AreEqual(0m, poolDto.Umbrella.Budget);
                            Assert.AreEqual(0m, poolDto.Umbrella.Actuals);
                            Assert.AreEqual(0m, poolDto.Umbrella.Encumbrances);
                        }
                        Assert.AreEqual(poolEntity.Umbrella.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), poolDto.Umbrella.FormattedGlAccount);

                        var poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                        switch (poolEntity.Umbrella.PoolType)
                        {
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                break;
                            case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                break;
                        }
                        Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);

                        // Check the poolees
                        foreach (var pooleeDto in poolDto.Poolees)
                        {
                            var pooleeEntity = poolEntity.Poolees.FirstOrDefault(x => x.GlAccountNumber == pooleeDto.GlAccountNumber);
                            Assert.AreEqual(pooleeEntity.BudgetAmount, pooleeDto.Budget);
                            Assert.AreEqual(pooleeEntity.ActualAmount, pooleeDto.Actuals);
                            Assert.AreEqual(pooleeEntity.EncumbranceAmount, pooleeDto.Encumbrances);
                            Assert.AreEqual(pooleeEntity.GetFormattedGlAccount(testGlAccountStructure.MajorComponentStartPositions), pooleeDto.FormattedGlAccount);

                            poolType = Dtos.ColleagueFinance.GlBudgetPoolType.None;
                            switch (poolEntity.Umbrella.PoolType)
                            {
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Umbrella;
                                    break;
                                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                                    poolType = Dtos.ColleagueFinance.GlBudgetPoolType.Poolee;
                                    break;
                            }
                            Assert.AreEqual(poolType, poolDto.Umbrella.PoolType);
                        }
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryCostCentersForUser_NullQueryCriteria()
        {
            var costCenterDtos = await serviceAll.QueryCostCentersAsync(null);
        }

        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple cost center service objects.
        /// </summary>
        private void BuildValidCostCenterService()
        {
            #region Initialize mock objects

            // A CostCenterService requires seven parameters for its constructor:
            //   1. a cost center repository object.
            //   2. a general ledger user repository object.
            //   3. a GL configuration repository object.
            //   4. a Colleague Finance Web Configurations repository object.
            //   5. an adapterRegistry object.
            //   6. a GL user factory object.
            //   7. a role repository object.
            //   8. a logger object.
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            testCostCenterRepository = new TestCostCenterRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testColleagueFinanceWebConfigurationsRepository = new TestColleagueFinanceWebConfigurationsRepository();

            // Get the account structure configuration so we know how to format the GL numbers and how to calculate the cost center.
            //var costCenterStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var costCenterDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.CostCenter, Dtos.ColleagueFinance.CostCenter>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.CostCenter, Dtos.ColleagueFinance.CostCenter>()).Returns(costCenterDtoAdapter);

            #endregion

            #region Set up the services

            // Set up the current user with all cost centers and set up the service.
            serviceAll = new CostCenterService(testCostCenterRepository, testGeneralLedgerUserRepository, testGlConfigurationRepository, testColleagueFinanceWebConfigurationsRepository, adapterRegistry.Object, glUserFactoryAll, roleRepository, loggerObject);
            // Set up the current user with a subset of cost centers and set up the service.
            serviceSubset = new CostCenterService(testCostCenterRepository, testGeneralLedgerUserRepository, testGlConfigurationRepository, testColleagueFinanceWebConfigurationsRepository, adapterRegistry.Object, glUserFactorySubset, roleRepository, loggerObject);
            // Set up the current user with no cost centers and set up the service.
            serviceNone = new CostCenterService(testCostCenterRepository, testGeneralLedgerUserRepository, testGlConfigurationRepository, testColleagueFinanceWebConfigurationsRepository, adapterRegistry.Object, glUserFactoryNone, roleRepository, loggerObject);
            // Set up the current user with access to cost centers that do not exist and set up the service.
            serviceNonExistant = new CostCenterService(testCostCenterRepository, testGeneralLedgerUserRepository, testGlConfigurationRepository, testColleagueFinanceWebConfigurationsRepository, adapterRegistry.Object, glUserFactoryNonExistant, roleRepository, loggerObject);

            #endregion
        }
        #endregion
    }
}