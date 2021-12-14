// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class GlAccountBalancesServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IRoleRepository> roleRepository;

        private GlAccountBalancesService service = null;

        private TestGeneralLedgerUserRepository testGlUserRepository = new TestGeneralLedgerUserRepository();
        private TestGlAccountBalancesRepository testGlAccountBalancesRepository = new TestGlAccountBalancesRepository();
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private TestGeneralLedgerAccountRepository testGlAccountRepository = new TestGeneralLedgerAccountRepository();

        private GeneralLedgerAccountStructure param_glAccountStructure;
        private List<string> glAccts;
        private string fiscalYear;
        private GlAccountBalances glAccount_1;
        private GlAccountBalances glAccount_2;
        private GlAccountBalances glAccount_3;
        private List<GlAccountBalances> glAccountBalancesEntities;
        private Domain.Entities.Permission createUpdateRequisition;
        private Domain.Entities.Permission createUpdateVoucher;
        private Domain.Entities.Permission createUpdatePurchaseOrder;
        protected Domain.Entities.Role procurementUserRole = new Domain.Entities.Role(105, "PROCUREMENT.USER");
        // Define user factories
        private ProcurementCreateUpdateDocUser procurementUserWithGlAccess = new GeneralLedgerCurrentUser.ProcurementCreateUpdateDocUser();

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            BuildService(testGlAccountBalancesRepository, testGlUserRepository, testGlConfigurationRepository);
            this.param_glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            fiscalYear = testGlConfigurationRepository.StartYear.ToString();
            glAccts = new List<string>() { "11_00_02_01_20601_53011", "11_00_02_01_20601_53013", "11_00_02_01_20601_53014" };

            glAccountBalancesEntities = new List<GlAccountBalances>();
            glAccount_1 = new GlAccountBalances("11_00_02_01_20601_53011", new List<string>()) { ActualAmount = 10.00m, BudgetAmount = 1000.00m, RequisitionAmount = 100.00m, EncumbranceAmount = 123.00m, GlAccountDescription = "Test1" };
            glAccountBalancesEntities.Add(glAccount_1);

            glAccount_2 = new GlAccountBalances("11_00_02_01_20601_53013", new List<string>()) { ActualAmount = 20.00m, BudgetAmount = 2000.00m, RequisitionAmount = 0, EncumbranceAmount = 0, GlAccountDescription = "Test2" };
            glAccountBalancesEntities.Add(glAccount_2);

            glAccount_3 = new GlAccountBalances("11_00_02_01_20601_53014", new List<string>()) { ActualAmount = 30.00m, BudgetAmount = 3000.00m, RequisitionAmount = 0, EncumbranceAmount = 0, GlAccountDescription = "Test3" };
            glAccount_3.IsPooleeAccount = true;
            glAccount_3.UmbrellaGlAccount = "11-00-01-00-20601-52000";
            glAccountBalancesEntities.Add(glAccount_3);


        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            testGlUserRepository = null;
            testGlAccountBalancesRepository = null;
            testGlConfigurationRepository = null;
        }
        #endregion

        #region QueryGlAccountBalancesAsync error checking
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryGlAccountBalancesAsync_NullGlAcctsCriteria()
        {
            await service.QueryGlAccountBalancesAsync(null, fiscalYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryGlAccountBalancesAsync_NullFiscalYearCriteria()
        {
            await service.QueryGlAccountBalancesAsync(glAccts, null);
        }

        [TestMethod]
        public async Task QueryGlAccountBalancesAsync_NullAccountStructure()
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
                BuildService(testGlAccountBalancesRepository, testGlUserRepository, glConfigurationRepositoryMock.Object);
                await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task QueryGlAccountBalancesAsync_NullGlClassConfiguration()
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
                BuildService(testGlAccountBalancesRepository, testGlUserRepository, glConfigurationRepositoryMock.Object);
                await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task QueryGlAccountBalancesAsync_NullGlUser()
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
                BuildService(testGlAccountBalancesRepository, glUserRepositoryMock.Object, testGlConfigurationRepository);
                await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task QueryGlAccountBalancesAsync_RepositoryReturnsNull()
        {
            //Arrange
            Mock<IGlAccountBalancesRepository> repository = new Mock<IGlAccountBalancesRepository>();
            repository.Setup(x => x.QueryGlAccountBalancesAsync(glAccts, fiscalYear, It.IsAny<GeneralLedgerUser>(), It.IsAny<GeneralLedgerAccountStructure>(), It.IsAny<GeneralLedgerClassConfiguration>())).ReturnsAsync(() => null);

            //Act
            BuildService(repository.Object, testGlUserRepository, testGlConfigurationRepository);

            var result = await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);

            //Assert      
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ToList());
        }

        [TestMethod]
        public async Task QueryGlAccountBalancesAsync_RepositoryReturnsEmptyList()
        {
            //Arrange
            glAccountBalancesEntities = new List<GlAccountBalances>();
            Mock<IGlAccountBalancesRepository> repository = new Mock<IGlAccountBalancesRepository>();
            repository.Setup(x => x.QueryGlAccountBalancesAsync(glAccts, fiscalYear, It.IsAny<GeneralLedgerUser>(), It.IsAny<GeneralLedgerAccountStructure>(), It.IsAny<GeneralLedgerClassConfiguration>())).ReturnsAsync(glAccountBalancesEntities);

            BuildService(repository.Object, testGlUserRepository, testGlConfigurationRepository);

            //Act
            var result = await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);

            //Assert            
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryGlAccountBalancesAsync_NoGlAccess()
        {
            GeneralLedgerUser glUser = await testGlUserRepository.GetGeneralLedgerUserAsync2("9999999", null, null);
            var glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });

            BuildService(testGlAccountBalancesRepository, glUserRepositoryMock.Object, testGlConfigurationRepository);
            var result = await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryGlAccountBalancesAsync_MissingCreateUpdateDocPermission()
        {
            //Removing the CREATE.UPDATE Permission
            procurementUserRole.RemovePermission(createUpdateRequisition);
            procurementUserRole.RemovePermission(createUpdatePurchaseOrder);
            procurementUserRole.RemovePermission(createUpdateVoucher);
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { procurementUserRole });
            var result = await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);
        }

        [TestMethod]
        public async Task QueryGlAccountBalancesAsync_Success()
        {
            // Get the necessary configuration settings.
            var testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var testGlClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            var generalLedgerUser = await testGlUserRepository.GetGeneralLedgerUserAsync(procurementUserWithGlAccess.CurrentUser.PersonId,
               testGlAccountStructure.FullAccessRole, testGlClassConfiguration.ClassificationName,
               testGlClassConfiguration.ExpenseClassValues);
            var expectedList = await testGlAccountBalancesRepository.QueryGlAccountBalancesAsync(glAccts, fiscalYear, generalLedgerUser, testGlAccountStructure, testGlClassConfiguration);
            Assert.IsNotNull(expectedList);
            expectedList = expectedList.ToList();

            //Arrange
            BuildService(testGlAccountBalancesRepository, testGlUserRepository, testGlConfigurationRepository);

            //Act
            var result = await service.QueryGlAccountBalancesAsync(glAccts, fiscalYear);

            //Assert            
            Assert.IsNotNull(result);
            result = result.ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            foreach (var actual in result)
            {
                var expected = expectedList.First(x => x.GlAccountNumber == actual.GlAccountNumber);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.GlAccountDescription, actual.GlAccountDescription);
                Assert.AreEqual(expected.ActualAmount, actual.ActualAmount);
                Assert.AreEqual(expected.BudgetAmount, actual.BudgetAmount);
                Assert.AreEqual(expected.RequisitionAmount, actual.RequisitionAmount);
                Assert.AreEqual(expected.EncumbranceAmount, actual.EncumbranceAmount);
                Assert.AreEqual(expected.IsPooleeAccount, actual.IsPooleeAccount);
                Assert.AreEqual(expected.UmbrellaGlAccount, actual.UmbrellaGlAccount);
                Assert.AreEqual(expected.ErrorMessage, actual.ErrorMessage);
            }

        }


        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple cost center service objects.
        /// </summary>
        private void BuildService(IGlAccountBalancesRepository glAccountBalancesRepository,
            IGeneralLedgerUserRepository glUserRepository,
            IGeneralLedgerConfigurationRepository glConfigurationRepository)
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            roleRepository = new Mock<IRoleRepository>();
            // Mock permissions
            createUpdateRequisition = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateRequisition);
            createUpdatePurchaseOrder = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder);
            createUpdateVoucher = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateVoucher);
            procurementUserRole.AddPermission(createUpdateRequisition);
            procurementUserRole.AddPermission(createUpdatePurchaseOrder);
            procurementUserRole.AddPermission(createUpdateVoucher);
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { procurementUserRole });

            var loggerObject = loggerMock.Object;
            var adapterRegistryObject = adapterRegistryMock.Object;


            // Set up and mock the adapter, and setup the GetAdapter method.
            var glAccountBalancesAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.GlAccountBalances, Dtos.ColleagueFinance.GlAccountBalances>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.GlAccountBalances, Dtos.ColleagueFinance.GlAccountBalances>()).Returns(glAccountBalancesAdapter);

            // Set up the current user with all cost centers and set up the service.
            service = new GlAccountBalancesService(glAccountBalancesRepository, glUserRepository, glConfigurationRepository, adapterRegistryObject, procurementUserWithGlAccess, roleRepository.Object, loggerObject);
        }
        #endregion
    }
}