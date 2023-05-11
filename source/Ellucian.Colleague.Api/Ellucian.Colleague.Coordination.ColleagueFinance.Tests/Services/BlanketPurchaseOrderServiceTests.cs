// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
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
        private Mock<IApprovalConfigurationRepository> mockApprovalConfigurationRepositoryFalse;
        private Mock<IApprovalConfigurationRepository> mockApprovalConfigurationRepositoryTrue;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewBlanketPurchaseOrder;
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(224, "BLANKET.PURCHASE.ORDER.VIEWER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();
        private GeneralLedgerUser glUser;


        [TestInitialize]
        public void Initialize()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();

            // Set up the mock BPO repository
            mockBlanketPurchaseOrderRepository = new Mock<IBlanketPurchaseOrderRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(new GeneralLedgerAccountStructure()));
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(new GeneralLedgerClassConfiguration("ClassName", new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>())));
            
            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            mockApprovalConfigurationRepositoryFalse = new Mock<IApprovalConfigurationRepository>();
            ApprovalConfiguration approvalConfigurationFalse = new ApprovalConfiguration()
            {
                BlanketPurchaseOrdersUseApprovalRoles = false
            };
            mockApprovalConfigurationRepositoryFalse.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult(approvalConfigurationFalse));
            mockApprovalConfigurationRepositoryTrue = new Mock<IApprovalConfigurationRepository>();
            ApprovalConfiguration approvalConfigurationTrue = new ApprovalConfiguration()
            {
                BlanketPurchaseOrdersUseApprovalRoles = true
            };
            mockApprovalConfigurationRepositoryTrue.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult(approvalConfigurationTrue));

            // Create permission domain entities for viewing the blanket purchase order.
            permissionViewBlanketPurchaseOrder = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrder);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewBlanketPurchaseOrder);

            glUser = new GeneralLedgerUser("0000001", "Test");
            glUser.SetGlAccessLevel(GlAccessLevel.No_Access);

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
            mockApprovalConfigurationRepositoryFalse = null;
            mockApprovalConfigurationRepositoryTrue = null;

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
                var blanketPurchaseOrderDto = await service.GetBlanketPurchaseOrderAsync(null);
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
                var blanketPurchaseOrderDto = await service.GetBlanketPurchaseOrderAsync("");
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

        #region Tests for GetBlanketPurchaseOrderAsync with GL approval roles functionality

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_ApprovalConfigurationNull()
        {
            mockApprovalConfigurationRepositoryFalse.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult<ApprovalConfiguration>(null));

            await service2.GetBlanketPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_ApprovalConfigurationEmpty()
        {
            mockApprovalConfigurationRepositoryFalse.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult(new ApprovalConfiguration()));

            await service2.GetBlanketPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_ApprovalConfigurationException()
        {
            mockGeneralLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });
            mockApprovalConfigurationRepositoryFalse.Setup(repo => repo.GetApprovalConfigurationAsync()).Throws(new Exception());

            await service2.GetBlanketPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrderAsync_ApprovalConfigurationBlanketPurchaseOrdersUseApprovalRoles()
        {
            mockGeneralLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });
            mockApprovalConfigurationRepositoryFalse.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult<ApprovalConfiguration>(new ApprovalConfiguration() { BlanketPurchaseOrdersUseApprovalRoles = true }));
            mockGeneralLedgerUserRepository.Setup(x => x.GetGlUserApprovalAndGlAccessAccountsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(() =>
            {
                IEnumerable<string> approvalAccess = new List<string>() { "11_11_11_11_00000_11111", "11_11_11_11_00000_11112" };
                return Task.FromResult(approvalAccess);
            });

            await service2.GetBlanketPurchaseOrderAsync("1");
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
            var colleagueFinanceReferenceDataRepository = new TestColleagueFinanceReferenceDataRepository();
            var referenceDataRepository = new Mock<IReferenceDataRepository>();
            var buyerRepository = new Mock<IBuyerRepository>();
            var vendorsRepository = new Mock<IVendorsRepository>();
            var configurationRepository = new Mock<IConfigurationRepository>();
            var approvalConfigurationRepository = new Mock<IApprovalConfigurationRepository>();
            var accountFundAvailableRepository = new Mock<IAccountFundsAvailableRepository>();
            var personRepository = new Mock<IPersonRepository>();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var blanketPurchaseOrderDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BlanketPurchaseOrder, Dtos.ColleagueFinance.BlanketPurchaseOrder>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BlanketPurchaseOrder, Dtos.ColleagueFinance.BlanketPurchaseOrder>()).Returns(blanketPurchaseOrderDtoAdapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new BlanketPurchaseOrderService(testBlanketPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, referenceDataRepository.Object, buyerRepository.Object, vendorsRepository.Object,
                configurationRepository.Object, adapterRegistry.Object, currentUserFactory, accountFundAvailableRepository.Object,
                personRepository.Object, roleRepository, approvalConfigurationRepository.Object, loggerObject);

            service2 = new BlanketPurchaseOrderService(mockBlanketPurchaseOrderRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object,
                colleagueFinanceReferenceDataRepository, referenceDataRepository.Object, buyerRepository.Object, vendorsRepository.Object,
                configurationRepository.Object, adapterRegistry.Object, currentUserFactory, accountFundAvailableRepository.Object,
                personRepository.Object, roleRepository, mockApprovalConfigurationRepositoryFalse.Object, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new BlanketPurchaseOrderService(testBlanketPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, referenceDataRepository.Object, buyerRepository.Object, vendorsRepository.Object,
                configurationRepository.Object, adapterRegistry.Object, noPermissionsUser, accountFundAvailableRepository.Object,
                personRepository.Object, roleRepository, approvalConfigurationRepository.Object, loggerObject);
        }
        #endregion
    }

    [TestClass]
    public class BlanketPurchaseOrderServiceTests_v16_0_0 : GeneralLedgerCurrentUser
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
        private Mock<IPersonRepository> mockPersonRepository;
        private Mock<IAccountFundsAvailableRepository> mockaccountFundAvailableRepository;
        private Mock<IVendorsRepository> mockVendorsRepository;
        private Mock<IColleagueFinanceReferenceDataRepository> mockColleagueFinanceReferenceDataRepository = new Mock<IColleagueFinanceReferenceDataRepository>();
        private Mock<IReferenceDataRepository> mockReferenceDataRepository = new Mock<IReferenceDataRepository>();

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewBlanketPurchaseOrder;
        private Domain.Entities.Permission permissionPutPostBlanketPurchaseOrder;
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(224, "BLANKET.PURCHASE.ORDER.VIEWER");

        private Domain.Entities.Permission permissionViewBlanketPurchaseOrders = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders);

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
            mockPersonRepository = new Mock<IPersonRepository>();
            mockaccountFundAvailableRepository = new Mock<IAccountFundsAvailableRepository>();
            mockVendorsRepository = new Mock<IVendorsRepository>();

            // Create permission domain entities for viewing the blanket purchase order.
            permissionViewBlanketPurchaseOrder = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrder);
            permissionPutPostBlanketPurchaseOrder = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewBlanketPurchaseOrder);
            glUserRoleViewPermissions.AddPermission(permissionViewBlanketPurchaseOrders);

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
            mockPersonRepository = null;

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
                var blanketPurchaseOrderDto = await service.GetBlanketPurchaseOrderAsync(null);
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
                var blanketPurchaseOrderDto = await service.GetBlanketPurchaseOrderAsync("");
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

        #region EEDM v15.1.0 blanket-purchase-orders tests

        [TestMethod]
        public async Task GetBlanketPurchaseOrdersAsync()
        {
            await service.GetBlanketPurchaseOrdersAsync(0, 100, null, false);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrdersByGuidAsync()
        {
            await service.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
        }


        #endregion

        #region POST/PUT

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PostBlanketPurchaseOrdersAsync_ArgumentNullException()
        {
            await service.PostBlanketPurchaseOrdersAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PostBlanketPurchaseOrdersAsync_ArgumentNullException_nullId()
        {
            Dtos.BlanketPurchaseOrders bpo = new Dtos.BlanketPurchaseOrders() { Id = string.Empty };
            await service.PostBlanketPurchaseOrdersAsync(bpo);
        }
        
        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PostBlanketPurchaseOrdersAsync_IntegrationApiException()
        {
            Dtos.BlanketPurchaseOrders bpo = new Dtos.BlanketPurchaseOrders() { Id = Guid.Empty.ToString() };
            await service.PostBlanketPurchaseOrdersAsync(bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PostBlanketPurchaseOrdersAsync_GetPersonIdFromGuidAsync_RepositoryException()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO_ForErrors();
            bpo.SubmittedBy = new Dtos.GuidObject2("123");
            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockPersonRepository.Setup(repo => repo.GetPersonIdFromGuidAsync("123")).Throws(new RepositoryException());
            await service2.PostBlanketPurchaseOrdersAsync(bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PostBlanketPurchaseOrdersAsync_ExistingVendorIdNull_ArgumentNullException()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO_ForErrors();
            bpo.Vendor.ExistingVendor.Vendor = new Dtos.GuidObject2("");
            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.CreateBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrder>())).ReturnsAsync(entity);
            mockVendorsRepository.Setup(r => r.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
            await service2.PostBlanketPurchaseOrdersAsync(bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PostBlanketPurchaseOrdersAsync_GetPersonIdFromGuidAsync_Exception()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO_ForErrors();
            bpo.SubmittedBy = new Dtos.GuidObject2("123");
            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockPersonRepository.Setup(repo => repo.GetPersonIdFromGuidAsync("123")).Throws(new Exception());
            await service2.PostBlanketPurchaseOrdersAsync(bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PostBlanketPurchaseOrdersAsync_IntegrationApiException_CollectionErrors()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO_ForErrors();

            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.CreateBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrder>())).ReturnsAsync(entity);
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(Guid.Empty.ToString())).ReturnsAsync("");
            await service2.PostBlanketPurchaseOrdersAsync(bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PostBlanketPurchaseOrdersAsync_BudOverCheck_Item2_False()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO();
            List<Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes = new List<CommodityCode>()
            {
                new CommodityCode("a18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityCode("c18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<Domain.ColleagueFinance.Entities.CommodityUnitType> commodityUnitTypes = new List<CommodityUnitType>()
            {
                new CommodityUnitType("d18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityUnitType("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<AccountsPayableSources> acctPayableSources = new List<AccountsPayableSources>()
            {
                new AccountsPayableSources("d18288c0-cca7-45b1-a310-39e376db0c3d", "AP", "Descr 1"),
                new AccountsPayableSources("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.CreateBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrder>())).ReturnsAsync(entity);
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(Guid.Empty.ToString())).ReturnsAsync("1");            
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(commodityUnitTypes);
            mockPersonRepository.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            List<FundsAvailable> fundsAvailable = new List<FundsAvailable>()
            {
                new FundsAvailable("11-01-01-00-40000-54005"){ AvailableStatus = FundsAvailableStatus.Override }
            };
            mockaccountFundAvailableRepository.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(fundsAvailable);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPayableSources);

            var result = await service2.PostBlanketPurchaseOrdersAsync(bpo);
        }

        [TestMethod]
        public async Task PostBlanketPurchaseOrdersAsync()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO();
            bpo.OrderDetails.ElementAt(0).AccountDetails.ElementAt(0).BudgetCheck = Dtos.EnumProperties.PurchaseOrdersAccountBudgetCheck.Override;
            List<Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes = new List<CommodityCode>()
            {
                new CommodityCode("a18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityCode("c18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<Domain.ColleagueFinance.Entities.CommodityUnitType> commodityUnitTypes = new List<CommodityUnitType>()
            {
                new CommodityUnitType("d18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityUnitType("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<AccountsPayableSources> acctPayableSources = new List<AccountsPayableSources>()
            {
                new AccountsPayableSources("d18288c0-cca7-45b1-a310-39e376db0c3d", "AP", "Descr 1"),
                new AccountsPayableSources("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.CreateBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrder>())).ReturnsAsync(entity);
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(Guid.Empty.ToString())).ReturnsAsync("1");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockVendorsRepository.Setup(vr => vr.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockVendorsRepository.Setup(vr => vr.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(commodityUnitTypes);
            mockPersonRepository.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            GuidLookupResult guidLookupResult1 = new GuidLookupResult() { Entity = "ADDRESS", PrimaryKey = "1" };
            mockReferenceDataRepository.Setup(r => r.GetGuidLookupResultFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookupResult1);
            List<FundsAvailable> fundsAvailable = new List<FundsAvailable>()
            {
                new FundsAvailable("11-01-01-00-40000-54005"){ AvailableStatus = FundsAvailableStatus.Override }
            };
            mockaccountFundAvailableRepository.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(fundsAvailable);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPayableSources);

            var result = await service2.PostBlanketPurchaseOrdersAsync(bpo);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task PutBlanketPurchaseOrdersAsync()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO();
            bpo.Id = "b18288c0-cca7-45b1-a310-39e376db0c3d";
            bpo.OrderDetails.ElementAt(0).AccountDetails.ElementAt(0).BudgetCheck = Dtos.EnumProperties.PurchaseOrdersAccountBudgetCheck.Override;
            List<Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes = new List<CommodityCode>()
            {
                new CommodityCode("a18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityCode("c18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<Domain.ColleagueFinance.Entities.CommodityUnitType> commodityUnitTypes = new List<CommodityUnitType>()
            {
                new CommodityUnitType("d18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityUnitType("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<AccountsPayableSources> acctPayableSources = new List<AccountsPayableSources>()
            {
                new AccountsPayableSources("d18288c0-cca7-45b1-a310-39e376db0c3d", "AP", "Descr 1"),
                new AccountsPayableSources("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.UpdateBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrder>())).ReturnsAsync(entity);
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(commodityUnitTypes);
            mockPersonRepository.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockVendorsRepository.Setup(vr => vr.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockVendorsRepository.Setup(vr => vr.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1");
            List<FundsAvailable> fundsAvailable = new List<FundsAvailable>()
            {
                new FundsAvailable("11-01-01-00-40000-54005"){ AvailableStatus = FundsAvailableStatus.Override }
            };
            mockaccountFundAvailableRepository.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(fundsAvailable);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPayableSources);

            var result = await service2.PutBlanketPurchaseOrdersAsync("", bpo);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PutBlanketPurchaseOrdersAsync_NullObject_ArgumentNullException()
        {
            await service2.PutBlanketPurchaseOrdersAsync("", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PutBlanketPurchaseOrdersAsync_NullObjectId_ArgumentNullException()
        {
            Dtos.BlanketPurchaseOrders bpo = new Dtos.BlanketPurchaseOrders();
            await service2.PutBlanketPurchaseOrdersAsync("", bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PutBlanketPurchaseOrdersAsync_Buyer_Id_Null_IntegrationApiException()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO();
            bpo.Id = "b18288c0-cca7-45b1-a310-39e376db0c3d";
            bpo.Buyer = new Dtos.GuidObject2("");
            bpo.OrderDetails.ElementAt(0).AccountDetails.ElementAt(0).BudgetCheck = Dtos.EnumProperties.PurchaseOrdersAccountBudgetCheck.Override;
            List<Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes = new List<CommodityCode>()
            {
                new CommodityCode("a18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityCode("c18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<Domain.ColleagueFinance.Entities.CommodityUnitType> commodityUnitTypes = new List<CommodityUnitType>()
            {
                new CommodityUnitType("d18288c0-cca7-45b1-a310-39e376db0c3d", "Code1", "Descr 1"),
                new CommodityUnitType("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            List<AccountsPayableSources> acctPayableSources = new List<AccountsPayableSources>()
            {
                new AccountsPayableSources("d18288c0-cca7-45b1-a310-39e376db0c3d", "AP", "Descr 1"),
                new AccountsPayableSources("e18288c0-cca7-45b1-a310-39e376db0c3d", "Code2", "Descr 2")
            };

            Domain.ColleagueFinance.Entities.BlanketPurchaseOrder entity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.UpdateBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrder>())).ReturnsAsync(entity);
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(commodityUnitTypes);
            mockPersonRepository.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("b18288c0-cca7-45b1-a310-39e376db0c3d");
            List<FundsAvailable> fundsAvailable = new List<FundsAvailable>()
            {
                new FundsAvailable("11-01-01-00-40000-54005"){ AvailableStatus = FundsAvailableStatus.Override }
            };
            mockaccountFundAvailableRepository.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(fundsAvailable);
            mockColleagueFinanceReferenceDataRepository.Setup(repo => repo.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPayableSources);

            var result = await service2.PutBlanketPurchaseOrdersAsync("", bpo);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PutBlanketPurchaseOrdersAsync_RepositoryException()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO();
            bpo.Id = "b18288c0-cca7-45b1-a310-39e376db0c3d";
            bpo.OrderDetails.ElementAt(0).AccountDetails[0].AccountingString = string.Concat(bpo.OrderDetails.ElementAt(0).AccountDetails[0].AccountingString, "*", "1");
            bpo.Status = Dtos.EnumProperties.BlanketPurchaseOrdersStatus.Closed;

            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetProjectIdsFromReferenceNo(It.IsAny<string[]>())).ThrowsAsync(new RepositoryException());

            var result = await service2.PutBlanketPurchaseOrdersAsync("b18288c0-cca7-45b1-a310-39e376db0c3d", bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PutBlanketPurchaseOrdersAsync_Exception()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO();
            bpo.Id = "b18288c0-cca7-45b1-a310-39e376db0c3d";
            bpo.OrderDetails.ElementAt(0).AccountDetails[0].AccountingString = string.Concat(bpo.OrderDetails.ElementAt(0).AccountDetails[0].AccountingString, "*", "1");
            bpo.Status = Dtos.EnumProperties.BlanketPurchaseOrdersStatus.Closed;

            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetProjectIdsFromReferenceNo(It.IsAny<string[]>())).ThrowsAsync(new Exception());

            var result = await service2.PutBlanketPurchaseOrdersAsync("b18288c0-cca7-45b1-a310-39e376db0c3d", bpo);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PutBlanketPurchaseOrdersAsync_KeyNotFoundException()
        {
            glUserRoleViewPermissions.AddPermission(permissionPutPostBlanketPurchaseOrder);
            Dtos.BlanketPurchaseOrders bpo = BuildBPO();
            bpo.Id = "b18288c0-cca7-45b1-a310-39e376db0c3d";
            bpo.OrderDetails.ElementAt(0).AccountDetails[0].AccountingString = string.Concat(bpo.OrderDetails.ElementAt(0).AccountDetails[0].AccountingString, "*", "1");
            bpo.Status = Dtos.EnumProperties.BlanketPurchaseOrdersStatus.Closed;

            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetBlanketPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            mockBlanketPurchaseOrderRepository.Setup(repo => repo.GetProjectIdsFromReferenceNo(It.IsAny<string[]>())).ThrowsAsync(new KeyNotFoundException());

            var result = await service2.PutBlanketPurchaseOrdersAsync("b18288c0-cca7-45b1-a310-39e376db0c3d", bpo);
        }

        private static Dtos.BlanketPurchaseOrders BuildBPO()
        {
            return new Dtos.BlanketPurchaseOrders()
            {
                Id = Guid.Empty.ToString(),
                OrderedOn = DateTime.Today,
                TransactionDate = DateTime.Today,
                Status = Dtos.EnumProperties.BlanketPurchaseOrdersStatus.Notapproved,
                PaymentSource = new Dtos.GuidObject2("d18288c0-cca7-45b1-a310-39e376db0c3d"),
                Vendor = new Dtos.BlanketPurchaseOrdersVendor()
                {
                    ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                    {
                        Vendor = new Dtos.GuidObject2("4d9962e8-195b-4442-93d7-197901cfb438")
                    }
                },
                OrderDetails = new List<Dtos.BlanketPurchaseOrdersOrderdetails>()
                {
                    new Dtos.BlanketPurchaseOrdersOrderdetails()
                    {
                        Description = "Some description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                            Value = 50m
                        },
                        AccountDetails = new List<Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty()
                            {
                                AccountingString = "11-01-01-00-40000-54005",
                                Allocation = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocationDtoProperty()
                                {
                                    Allocated = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocatedDtoProperty()
                                    {
                                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                                        {
                                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                            Value = 5000m
                                        },
                                        Percentage = 100
                                    },
                                    AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                    {
                                         Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                         Value = 10
                                    }
                                }
                            }
                        },
                        Comments = new List<Dtos.BlanketPurchaseOrdersComments>()
                        {
                            new Dtos.BlanketPurchaseOrdersComments()
                            {
                                Comment = "Comment 1",
                                Type = Dtos.EnumProperties.CommentTypes.Printed
                            },
                            new Dtos.BlanketPurchaseOrdersComments()
                            {
                                Comment = "Comment 2",
                                Type = Dtos.EnumProperties.CommentTypes.NotPrinted
                            }
                        },
                        CommodityCode = new Dtos.GuidObject2("a18288c0-cca7-45b1-a310-39e376db0c3d")
                    }

                },
                Comments = new List<Dtos.BlanketPurchaseOrdersComments>()
                {
                    new Dtos.BlanketPurchaseOrdersComments()
                    {
                        Comment = "Comment 1",
                        Type = Dtos.EnumProperties.CommentTypes.Printed
                    },
                    new Dtos.BlanketPurchaseOrdersComments()
                    {
                        Comment = "Comment 2",
                        Type = Dtos.EnumProperties.CommentTypes.NotPrinted
                    }
                }

            };
        }

        private static Dtos.BlanketPurchaseOrders BuildBPO_ForErrors()
        {
            return new Dtos.BlanketPurchaseOrders()
            {
                Id = Guid.Empty.ToString(),
                OrderedOn = DateTime.Today,
                TransactionDate = DateTime.Today,
                Status = Dtos.EnumProperties.BlanketPurchaseOrdersStatus.Notapproved,
                SubmittedBy = new Dtos.GuidObject2(),
                Buyer = new Dtos.GuidObject2(),
                Initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty() { Detail = new Dtos.GuidObject2() },
                Shipping = new Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty() { ShipTo = new Dtos.GuidObject2("123"), FreeOnBoard = new Dtos.GuidObject2("123"), Attention = "123" },
                Vendor = new Dtos.BlanketPurchaseOrdersVendor()
                {
                    ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                    {
                        Vendor = new Dtos.GuidObject2("4d9962e8-195b-4442-93d7-197901cfb438"),
                        AlternativeVendorAddress = new Dtos.GuidObject2("1234")
                    },
                    ManualVendorDetails = new Dtos.ManualVendorDetailsDtoProperty()
                    {
                        AddressLines = new List<string>() { "Some address" },
                        Contact = new Dtos.PhoneDtoProperty()
                        {
                            Number = "8885551212",
                            PhoneType = Dtos.PhoneType.Work
                        },
                        Name = "Vendor",
                        Place = new Dtos.AddressPlace()
                        {
                            Country = new Dtos.AddressCountry()
                            {
                                Code = Dtos.EnumProperties.IsoCode.USA
                            }
                        }
                    }
                },
                OrderDetails = new List<Dtos.BlanketPurchaseOrdersOrderdetails>()
                {
                    new Dtos.BlanketPurchaseOrdersOrderdetails()
                    {
                        Description = "Some description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                            Value = 50m
                        },
                        AccountDetails = new List<Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty()
                            {
                                AccountingString = "11-01-01-00-40000-54005",
                                Allocation = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocationDtoProperty()
                                {
                                    Allocated = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocatedDtoProperty()
                                    {
                                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                                        {
                                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                            Value = 5000m
                                        },
                                        Percentage = 100
                                    },
                                    AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                    {
                                         Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                         Value = 10
                                    }
                                }                              
                            }
                        },
                        Comments = new List<Dtos.BlanketPurchaseOrdersComments>()
                        {
                            new Dtos.BlanketPurchaseOrdersComments()
                            {
                                Comment = "Comment 1",
                                Type = Dtos.EnumProperties.CommentTypes.Printed
                            },
                            new Dtos.BlanketPurchaseOrdersComments()
                            {
                                Comment = "Comment 2",
                                Type = Dtos.EnumProperties.CommentTypes.NotPrinted
                            }
                        },
                        CommodityCode = new Dtos.GuidObject2("1234"),
                        ReferenceRequisitions = new List<Dtos.DtoProperties.BlanketPurchaseOrdersReferenceDtoProperty>()
                        {
                            new Dtos.DtoProperties.BlanketPurchaseOrdersReferenceDtoProperty()
                            {
                                lineItemNumber = "1",
                                Requisition = new Dtos.GuidObject2("1234")
                            }
                        }
                    }
                    
                },
                OverrideShippingDestination = new Dtos.OverrideShippingDestinationDtoProperty()
                {
                    Description = "Description 1",
                    AddressLines = new List<string>()
                    {
                        "123 Any Str, Some City, SomeState 11111"
                    },
                    Contact = new Dtos.PhoneDtoProperty()
                    {
                        Extension = "111",
                        Guid = Guid.NewGuid().ToString(),
                        Number = "8005551212",
                        PhoneType = Dtos.PhoneType.Work
                    }
                },
                ReferenceNumbers = new List<string>() { "Ref1" },
                PaymentSource = new Dtos.GuidObject2("1234"),
                PaymentTerms = new Dtos.GuidObject2("1234"),
                Comments = new List<Dtos.BlanketPurchaseOrdersComments>()
                {
                    new Dtos.BlanketPurchaseOrdersComments()
                    {
                        Comment = "Comment 1",
                        Type = Dtos.EnumProperties.CommentTypes.Printed
                    },
                    new Dtos.BlanketPurchaseOrdersComments()
                    {
                        Comment = "Comment 2",
                        Type = Dtos.EnumProperties.CommentTypes.NotPrinted
                    }
                }

            };
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
            var colleagueFinanceReferenceDataRepository = new TestColleagueFinanceReferenceDataRepository();
            var referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            var referenceDataRepository = referenceDataRepositoryMock.Object;

            // Set up the mock reference data repository 
            Domain.Base.Entities.Country country = new Domain.Base.Entities.Country("USA", "USA", "USA", "USA", false);
            IEnumerable<Domain.Base.Entities.Country> countries = new List<Domain.Base.Entities.Country>() { country };
            referenceDataRepositoryMock.Setup(rdr => rdr.GetCountryCodesAsync(false)).ReturnsAsync(countries);

            var buyerRepository = new Mock<IBuyerRepository>();
            //var vendorsRepository = new Mock<IVendorsRepository>();
           mockVendorsRepository.Setup(vr => vr.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("cc4ff1f8-95ef-4e0d-9f75-e8f29d49234f");

            var configurationRepository = new Mock<IConfigurationRepository>();
            var accountFundAvailableRepository = new Mock<IAccountFundsAvailableRepository>();
            var personRepository = new Mock<IPersonRepository>();
            var approvalConfigurationRepository = new Mock<IApprovalConfigurationRepository>();
            personRepository.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("b6ee96c5-c963-4933-94e4-183614b63b26");                 

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var blanketPurchaseOrderDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BlanketPurchaseOrder, Dtos.ColleagueFinance.BlanketPurchaseOrder>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BlanketPurchaseOrder, Dtos.ColleagueFinance.BlanketPurchaseOrder>()).Returns(blanketPurchaseOrderDtoAdapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new BlanketPurchaseOrderService(testBlanketPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, referenceDataRepository, buyerRepository.Object, mockVendorsRepository.Object,
                configurationRepository.Object, adapterRegistry.Object, currentUserFactory, accountFundAvailableRepository.Object,
                personRepository.Object, roleRepository, approvalConfigurationRepository.Object, loggerObject);

            service2 = new BlanketPurchaseOrderService(mockBlanketPurchaseOrderRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object,
                mockColleagueFinanceReferenceDataRepository.Object, referenceDataRepository, buyerRepository.Object, mockVendorsRepository.Object,
                configurationRepository.Object, adapterRegistry.Object, currentUserFactory, mockaccountFundAvailableRepository.Object,
                mockPersonRepository.Object, roleRepository, approvalConfigurationRepository.Object, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new BlanketPurchaseOrderService(testBlanketPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, referenceDataRepository, buyerRepository.Object, mockVendorsRepository.Object,
                configurationRepository.Object, adapterRegistry.Object, noPermissionsUser, accountFundAvailableRepository.Object,
                personRepository.Object, roleRepository, approvalConfigurationRepository.Object, loggerObject);

            GeneralLedgerAccountStructure accountStructure = new GeneralLedgerAccountStructure();
            accountStructure.AddMajorComponent(new GeneralLedgerComponent("ComponentName", true, GeneralLedgerComponentType.FullAccount, "5", "4"));
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));

        }
        #endregion
    }

}
