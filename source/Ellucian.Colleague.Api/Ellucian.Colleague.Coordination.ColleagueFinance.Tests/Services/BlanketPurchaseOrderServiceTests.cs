// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

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
    /// <summary>
    /// This class tests that the service returns a specified blanket purchase order.
    ///  We use GeneralLedgerCurrentUser to mimic the user logged in.
    /// </summary>
    [TestClass]
    public class BlanketPurchaseOrderServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private BlanketPurchaseOrderService service;
        private BlanketPurchaseOrderService service2;
        private BlanketPurchaseOrderService serviceForNoPermission;

        private TestBlanketPurchaseOrderRepository testBlanketPurchaseOrderRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;
        private Mock<IBlanketPurchaseOrderRepository> mockBlanketPurchaseOrderRepository;

        private Mock<IGeneralLedgerConfigurationRepository> mockGlConfigurationRepository;
        private Mock<IGeneralLedgerUserRepository> mockGeneralLedgerUserRepository;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewBlanketPurchaseOrder;
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(224, "BLANKET.PURCHASE.ORDER.VIEWER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();


        [TestInitialize]
        public void Initialize()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();

            // Set up the mock BPO repository
            mockBlanketPurchaseOrderRepository = new Mock<IBlanketPurchaseOrderRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();

            // Create permission domain entities for viewing the blanket purchase order.
            permissionViewBlanketPurchaseOrder = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrder);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewBlanketPurchaseOrder);

            // Build all service objects to use each of the user factories built above
            BuildValidBlanketPurchaseOrderService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables.
            service = null;
            service2 = null;
            serviceForNoPermission = null;

            testBlanketPurchaseOrderRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;
            mockBlanketPurchaseOrderRepository = null;
            mockGlConfigurationRepository = null;
            mockGeneralLedgerUserRepository = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleViewPermissions = null;
        }
        #endregion

        #region Tests for GetBlanketPurchaseOrderAsync with a view permission

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync()
        {
            // Get a specified blanket purchase order
            var blanketPurchaseOrderId = "1";
            var personId = "1";
            var blanketPurchaseOrderDto = await service.GetBlanketPurchaseOrderAsync(blanketPurchaseOrderId);

            // Get the blanket purchase order domain entity from the test repository
            var blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(blanketPurchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(blanketPurchaseOrderDto.Id, blanketPurchaseOrderDomainEntity.Id);
            Assert.AreEqual(blanketPurchaseOrderDto.Number, blanketPurchaseOrderDomainEntity.Number);
            Assert.AreEqual(blanketPurchaseOrderDto.Amount, blanketPurchaseOrderDomainEntity.Amount);
            Assert.AreEqual(blanketPurchaseOrderDto.ApType, blanketPurchaseOrderDomainEntity.ApType);
            Assert.AreEqual(blanketPurchaseOrderDto.Comments, blanketPurchaseOrderDomainEntity.Comments);
            Assert.AreEqual(blanketPurchaseOrderDto.CurrencyCode, blanketPurchaseOrderDomainEntity.CurrencyCode);
            Assert.AreEqual(blanketPurchaseOrderDto.Date, blanketPurchaseOrderDomainEntity.Date);
            Assert.AreEqual(blanketPurchaseOrderDto.ExpirationDate, blanketPurchaseOrderDomainEntity.ExpirationDate);
            Assert.AreEqual(blanketPurchaseOrderDto.InitiatorName, blanketPurchaseOrderDomainEntity.InitiatorName);
            Assert.AreEqual(blanketPurchaseOrderDto.InternalComments, blanketPurchaseOrderDomainEntity.InternalComments);
            Assert.AreEqual(blanketPurchaseOrderDto.MaintenanceDate, blanketPurchaseOrderDomainEntity.MaintenanceDate);
            Assert.AreEqual(blanketPurchaseOrderDto.Status.ToString(), blanketPurchaseOrderDomainEntity.Status.ToString());
            Assert.AreEqual(blanketPurchaseOrderDto.StatusDate, blanketPurchaseOrderDomainEntity.StatusDate);
            Assert.AreEqual(blanketPurchaseOrderDto.VendorId, blanketPurchaseOrderDomainEntity.VendorId);
            Assert.AreEqual(blanketPurchaseOrderDto.VendorName, blanketPurchaseOrderDomainEntity.VendorName);

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < blanketPurchaseOrderDto.Approvers.Count(); i++)
            {
                var approverDto = blanketPurchaseOrderDto.Approvers[i];
                var approverDomain = blanketPurchaseOrderDto.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the list of requisition DTOs matches the domain entity
            for (int i = 0; i < blanketPurchaseOrderDto.Requisitions.Count(); i++)
            {
                Assert.AreEqual(blanketPurchaseOrderDto.Requisitions[i], blanketPurchaseOrderDomainEntity.Requisitions[i]);
            }

            // Confirm that the data in the list of voucher DTOs matches the domain entity
            for (int i = 0; i < blanketPurchaseOrderDto.Vouchers.Count(); i++)
            {
                Assert.AreEqual(blanketPurchaseOrderDto.Vouchers[i], blanketPurchaseOrderDomainEntity.Vouchers[i]);
            }

            // Confirm that the data in the GL distribution DTOs matches the domain entity
            for (int i = 0; i < blanketPurchaseOrderDto.GlDistributions.Count(); i++)
            {
                var glDistributionDto = blanketPurchaseOrderDto.GlDistributions[i];
                var glDistributionDomain = blanketPurchaseOrderDomainEntity.GlDistributions[i];
                Assert.AreEqual(glDistributionDto.Description, glDistributionDomain.GlAccountDescription);
                Assert.AreEqual(glDistributionDto.EncumberedAmount, glDistributionDomain.EncumberedAmount);
                Assert.AreEqual(glDistributionDto.ExpensedAmount, glDistributionDomain.ExpensedAmount);
                Assert.AreEqual(glDistributionDto.GlAccount, glDistributionDomain.GlAccountNumber);
                Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionDomain.ProjectLineItemCode);
                Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionDomain.ProjectNumber);
            }
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_BpoHasRequisitions()
        {
            var bpoId = "1";
            var personId = "1";
            var bpoDto = await service.GetBlanketPurchaseOrderAsync(bpoId);

            // Get the blanket purchase order domain entiy from the test repository
            var bpoDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the list of requisition DTOs matches the domain entity
            for (int i = 0; i < bpoDto.Requisitions.Count(); i++)
            {
                Assert.AreEqual(bpoDto.Requisitions[i], bpoDomainEntity.Requisitions[i]);
            }
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_StatusClosed()
        {
            var bpoId = "5";
            var personId = "1";
            var bpoDto = await service.GetBlanketPurchaseOrderAsync(bpoId);

            // Get the blanket purchase order domain entiy from the test repository
            var bpoDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(bpoDomainEntity.Status.ToString(), bpoDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_StatusInProgress()
        {
            var bpoId = "3";
            var personId = "1";
            var bpoDto = await service.GetBlanketPurchaseOrderAsync(bpoId);

            // Get the blanket purchase order domain entiy from the test repository
            var bpoDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(bpoDomainEntity.Status.ToString(), bpoDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_StatusNotApproved()
        {
            var bpoId = "6";
            var personId = "1";
            var bpoDto = await service.GetBlanketPurchaseOrderAsync(bpoId);

            // Get the blanket purchase order domain entiy from the test repository
            var bpoDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(bpoDomainEntity.Status.ToString(), bpoDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_StatusOutstanding()
        {
            var bpoId = "4";
            var personId = "1";
            var bpoDto = await service.GetBlanketPurchaseOrderAsync(bpoId);

            // Get the blanket purchase order domain entiy from the test repository
            var bpoDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(bpoDomainEntity.Status.ToString(), bpoDto.Status.ToString(), "Status must be the same.");
        }
        
        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_StatusVoided()
        {
            var bpoId = "7";
            var personId = "1";
            var bpoDto = await service.GetBlanketPurchaseOrderAsync(bpoId);

            // Get the blanket purchase order domain entiy from the test repository
            var bpoDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(bpoDomainEntity.Status.ToString(), bpoDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_NullId()
        {
            var expectedParamName = "blanketPurchaseOrderDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetBlanketPurchaseOrderAsync(null);
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_EmptyId()
        {
            var expectedParamName = "blanketPurchaseOrderDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetBlanketPurchaseOrderAsync("");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_NullAccountStructure()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.GetBlanketPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_NullGlClassConfiguration()
        {
            // Mock the general ledger class repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            await service2.GetBlanketPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_NullGeneralLedgerUser()
        {
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerUser glUser = null;
            mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));
            await service2.GetBlanketPurchaseOrderAsync("1");
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_RepositoryReturnsNullObject()
        {
            // Mock the GetBlanketPurchaseOrder repository method to return a null object within the Service method
            BlanketPurchaseOrder nullBlanketPurchaseOrder = null;
            mockBlanketPurchaseOrderRepository.Setup<Task<BlanketPurchaseOrder>>(bpoRepo => bpoRepo.GetBlanketPurchaseOrderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>())).Returns(Task.FromResult(nullBlanketPurchaseOrder));
            var blanketPurchaseOrderDto = await service2.GetBlanketPurchaseOrderAsync("1");
        }


        #endregion

        #region Tests for GetBlanketPurchaseOrderAsync without a view permission

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetBlanketPurchaseOrderAsync_PermissionException()
        {
            await serviceForNoPermission.GetBlanketPurchaseOrderAsync("1");
        }
        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple blanket purchase order service objects
        /// </summary>
        /// <returns>Nothing.</returns>
        private void BuildValidBlanketPurchaseOrderService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository = roleRepositoryMock.Object;

            var loggerObject = new Mock<ILogger>().Object;

            testBlanketPurchaseOrderRepository = new TestBlanketPurchaseOrderRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var blanketPurchaseOrderDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BlanketPurchaseOrder, Dtos.ColleagueFinance.BlanketPurchaseOrder>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BlanketPurchaseOrder, Dtos.ColleagueFinance.BlanketPurchaseOrder>()).Returns(blanketPurchaseOrderDtoAdapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new BlanketPurchaseOrderService(testBlanketPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);
            service2 = new BlanketPurchaseOrderService(mockBlanketPurchaseOrderRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new BlanketPurchaseOrderService(testBlanketPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, noPermissionsUser, roleRepository, loggerObject);
        }
        #endregion
    }
}
