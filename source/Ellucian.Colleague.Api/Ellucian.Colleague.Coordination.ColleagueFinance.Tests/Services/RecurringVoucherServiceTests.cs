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
    /// This class tests that the service returns a specified recurring voucher.
    /// </summary>
    [TestClass]
    public class RecurringVoucherServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private RecurringVoucherService service;
        private RecurringVoucherService service2;
        private RecurringVoucherService serviceForNoPermission;

        private TestRecurringVoucherRepository testRecurringVoucherRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;
        private Mock<IRecurringVoucherRepository> mockRecurringVoucherRepository;

        private Mock<IGeneralLedgerConfigurationRepository> mockGlConfigurationRepository;
        private Mock<IGeneralLedgerUserRepository> mockGeneralLedgerUserRepository;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewRecurringVoucher;
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(227, "RECURRING.VOUCHER.VIEWER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();

        [TestInitialize]
        public void Initialize()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();

            // Initialize the mock recurring voucher repository
            mockRecurringVoucherRepository = new Mock<IRecurringVoucherRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();

            // Create permission domain entities for viewing the blanket purchase order.
            permissionViewRecurringVoucher = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewRecurringVoucher);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewRecurringVoucher);

            // Build all service objects to use each of the user factories built above
            BuildValidRecurringVoucherService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables.
            service = null;
            service2 = null;
            serviceForNoPermission = null;

            testRecurringVoucherRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;

            mockRecurringVoucherRepository = null;
            mockGlConfigurationRepository = null;
            mockGeneralLedgerUserRepository = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleViewPermissions = null;
        }
        #endregion

        #region Tests for GetRecurringVoucherAsync with a view permission
        [TestMethod]
        public async Task GetRecurringVoucherAsync_AllData()
        {
            // Get a specified voucher
            var recurringVoucherId = "RV0001000";
            var recurringVoucherDto = await service.GetRecurringVoucherAsync(recurringVoucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var recurringVoucherDomainEntity = await testRecurringVoucherRepository.GetRecurringVoucherAsync(recurringVoucherId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(recurringVoucherDto.RecurringVoucherId, recurringVoucherDomainEntity.Id);
            Assert.AreEqual(recurringVoucherDto.Amount, recurringVoucherDomainEntity.Amount);
            Assert.AreEqual(recurringVoucherDto.ApType, recurringVoucherDomainEntity.ApType);
            Assert.AreEqual(recurringVoucherDto.Comments, recurringVoucherDomainEntity.Comments);
            Assert.AreEqual(recurringVoucherDto.Date, recurringVoucherDomainEntity.Date);
            Assert.AreEqual(recurringVoucherDto.InvoiceDate, recurringVoucherDomainEntity.InvoiceDate);
            Assert.AreEqual(recurringVoucherDto.InvoiceNumber, recurringVoucherDomainEntity.InvoiceNumber);
            Assert.AreEqual(recurringVoucherDto.MaintenanceDate, recurringVoucherDomainEntity.MaintenanceDate);
            Assert.AreEqual(recurringVoucherDto.Status.ToString(), recurringVoucherDomainEntity.Status.ToString());
            Assert.AreEqual(recurringVoucherDto.VendorId, recurringVoucherDomainEntity.VendorId);
            Assert.AreEqual(recurringVoucherDto.VendorName, recurringVoucherDomainEntity.VendorName);
            Assert.AreEqual(recurringVoucherDto.TotalScheduleAmountInLocalCurrency, recurringVoucherDomainEntity.TotalScheduleAmountInLocalCurrency, "Local total amounts should match.");
            Assert.AreEqual(recurringVoucherDto.TotalScheduleTaxAmountInLocalCurrency, recurringVoucherDomainEntity.TotalScheduleTaxAmountInLocalCurrency, "Local total amounts should match.");
            Assert.AreEqual(recurringVoucherDto.CurrencyCode, recurringVoucherDomainEntity.CurrencyCode, "Currency code should match.");

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < recurringVoucherDto.Approvers.Count(); i++)
            {
                var approverDto = recurringVoucherDto.Approvers[i];
                var approverDomain = recurringVoucherDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }
        }
        
        [TestMethod]
        public async Task GetRecurringVoucherAsync_StatusNotApproved()
        {
            var recurringVoucherId = "RV0001000";

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var recurringVoucherDomainEntity = testRecurringVoucherRepository.CreateRecurringVoucherDomainEntity(RecurringVoucherStatus.NotApproved);

            // Get a specified recurring voucher

            var recurringVoucherDto = await service.GetRecurringVoucherAsync(recurringVoucherId);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(recurringVoucherDto.Status.ToString(), recurringVoucherDomainEntity.Status.ToString());
        }
        
        [TestMethod]
        public async Task GetRecurringVoucherAsync_StatusVoided()
        {
            var recurringVoucherId = "RV0001000";

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var recurringVoucherDomainEntity = testRecurringVoucherRepository.CreateRecurringVoucherDomainEntity(RecurringVoucherStatus.Voided);

            // Get a specified recurring voucher

            var recurringVoucherDto = await service.GetRecurringVoucherAsync(recurringVoucherId);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(recurringVoucherDto.Status.ToString(), recurringVoucherDomainEntity.Status.ToString());
        }
        
        [TestMethod]
        public async Task GetRecurringVoucherAsync_StatusCancelled()
        {
            var recurringVoucherId = "RV0001000";

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var recurringVoucherDomainEntity = testRecurringVoucherRepository.CreateRecurringVoucherDomainEntity(RecurringVoucherStatus.Cancelled);

            // Get a specified recurring voucher

            var recurringVoucherDto = await service.GetRecurringVoucherAsync(recurringVoucherId);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(recurringVoucherDto.Status.ToString(), recurringVoucherDomainEntity.Status.ToString());
        }
        
        [TestMethod]
        public async Task GetRecurringVoucherAsync_StatusClosed()
        {
            var recurringVoucherId = "RV0001000";

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var recurringVoucherDomainEntity = testRecurringVoucherRepository.CreateRecurringVoucherDomainEntity(RecurringVoucherStatus.Closed);

            // Get a specified recurring voucher

            var recurringVoucherDto = await service.GetRecurringVoucherAsync(recurringVoucherId);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(recurringVoucherDto.Status.ToString(), recurringVoucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_NullId()
        {
            var expectedParamName = "recurringVoucherDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetRecurringVoucherAsync(null);
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_EmptyId()
        {
            var expectedParamName = "recurringVoucherDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetRecurringVoucherAsync("");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRecurringVoucherAsync_NullAccountStructure()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.GetRecurringVoucherAsync("RV0001000");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRecurringVoucherAsync_NullGlClassConfiguration()
        {
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            await service2.GetRecurringVoucherAsync("RV0001000");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRecurringVoucherAsync_NullGeneralLedgerUser()
        {
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerUser glUser = null;
            mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));
            await service2.GetRecurringVoucherAsync("RV0001000");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRecurringVoucherAsync_RepositoryReturnsNullObject()
        {
            // Mock the GetRecurringVoucher repository method to return a null object within the Service method
            RecurringVoucher nullRecurringVoucher = null;
            this.mockRecurringVoucherRepository.Setup<Task<RecurringVoucher>>(recurringVoucherRepo => recurringVoucherRepo.GetRecurringVoucherAsync(It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>())).Returns(Task.FromResult(nullRecurringVoucher));
            var recurringVoucherDto = await service2.GetRecurringVoucherAsync("RV0000001");
        }
        #endregion

        #region Tests for GetRecurringVoucherAsync without a view permission

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRecurringVoucherAsync_PermissionException()
        {
            await serviceForNoPermission.GetRecurringVoucherAsync("RV0001000");
        }
        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple recurring voucher service objects.
        /// </summary>
        /// <returns>Nothing.</returns>
        private void BuildValidRecurringVoucherService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository = roleRepositoryMock.Object;

            var loggerObject = new Mock<ILogger>().Object;

            testRecurringVoucherRepository = new TestRecurringVoucherRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var recurringVoucherDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.RecurringVoucher, Dtos.ColleagueFinance.RecurringVoucher>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.RecurringVoucher, Dtos.ColleagueFinance.RecurringVoucher>()).Returns(recurringVoucherDtoAdapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new RecurringVoucherService(testRecurringVoucherRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);
            service2 = new RecurringVoucherService(mockRecurringVoucherRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new RecurringVoucherService(testRecurringVoucherRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, noPermissionsUser, roleRepository, loggerObject);
        }
        #endregion
    }
}
