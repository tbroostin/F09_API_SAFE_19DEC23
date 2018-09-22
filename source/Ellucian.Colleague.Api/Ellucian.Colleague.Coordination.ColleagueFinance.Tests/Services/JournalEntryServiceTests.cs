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
    ///  Test that the service returns a valid journal entry.
    ///  We use GeneralLedgerCurrentUser to mimic the user logged in
    /// </summary>
    [TestClass]
    public class JournalEntryServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private JournalEntryService service = null;
        private JournalEntryService service2 = null;
        private JournalEntryService serviceForNoPermission;

        private TestJournalEntryRepository testJournalEntryRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;
        private Mock<IJournalEntryRepository> mockJournalEntryRepository;

        private Mock<IGeneralLedgerConfigurationRepository> mockGlConfigurationRepository;
        private Mock<IGeneralLedgerUserRepository> mockGeneralLedgerUserRepository;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewJournalEntry;
        private Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(225, "JOURNAL.ENTRY.VIEWER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();



        [TestInitialize]
        public void Inititalize()
        {
            // Set up the mock objects
            roleRepositoryMock = new Mock<IRoleRepository>();

            mockJournalEntryRepository = new Mock<IJournalEntryRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();

            // Create permission domain entities for viewing the journal entry.
            permissionViewJournalEntry = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewJournalEntry);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewJournalEntry);

            // build all service objects to use in testing
            BuildValidJournalEntryService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            service2 = null;
            serviceForNoPermission = null;

            testJournalEntryRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;

            mockJournalEntryRepository = null;
            mockGlConfigurationRepository = null;
            mockGeneralLedgerUserRepository = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleViewPermissions = null;
        }

        #endregion

        #region Tests for GetJournalEntryAsync with a view permission
        [TestMethod]
        public async Task GetJournalEntryAsync()
        {
            var journalEntryId = "J000001";
            var personId = "1";
            var journalEntryDto = await service.GetJournalEntryAsync(journalEntryId);

            // Build the journal entry object
            var journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(journalEntryDto.Author, journalEntryDomainEntity.Author);
            Assert.AreEqual(journalEntryDto.AutomaticReversal, journalEntryDomainEntity.AutomaticReversal);
            Assert.AreEqual(journalEntryDto.Comments, journalEntryDomainEntity.Comments);
            Assert.AreEqual(journalEntryDto.TotalCredits, journalEntryDomainEntity.TotalCredits);
            Assert.AreEqual(journalEntryDto.Date, journalEntryDomainEntity.Date);
            Assert.AreEqual(journalEntryDto.TotalDebits, journalEntryDomainEntity.TotalDebits);
            Assert.AreEqual(journalEntryDto.EnteredByName, journalEntryDomainEntity.EnteredByName);
            Assert.AreEqual(journalEntryDto.EnteredDate, journalEntryDomainEntity.EnteredDate);
            Assert.AreEqual(journalEntryDto.Id, journalEntryDomainEntity.Id);
            Assert.AreEqual(journalEntryDto.Status.ToString(), journalEntryDomainEntity.Status.ToString());

            Assert.AreEqual(journalEntryDto.Type.ToString(), journalEntryDomainEntity.Type.ToString());

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < journalEntryDto.Approvers.Count(); i++)
            {
                var approverDto = journalEntryDto.Approvers[i];
                var approverDomain = journalEntryDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the journal entry item DTOs matches the domain entity
            for (int i = 0; i < journalEntryDto.Items.Count(); i++)
            {
                var journalEntryItemDto = journalEntryDto.Items[i];
                var journalEntryItemDomain = journalEntryDomainEntity.Items[i];
                Assert.AreEqual(journalEntryItemDto.Credit, journalEntryItemDomain.Credit);
                Assert.AreEqual(journalEntryItemDto.Debit, journalEntryItemDomain.Debit);
                Assert.AreEqual(journalEntryItemDto.Description, journalEntryItemDomain.Description);
                Assert.AreEqual(journalEntryItemDto.GlAccount, journalEntryItemDomain.GlAccountNumber);
                Assert.AreEqual(journalEntryItemDto.ProjectLineItemCode, journalEntryItemDomain.ProjectLineItemCode);
                Assert.AreEqual(journalEntryItemDto.ProjectNumber, journalEntryItemDomain.ProjectNumber);
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_StatusNotApproved()
        {
            var journalEntryId = "J000002";
            var personId = "1";
            var journalEntryDto = await service.GetJournalEntryAsync(journalEntryId);

            // Build the journal entry object
            var journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(journalEntryDto.Author, journalEntryDomainEntity.Author);
            Assert.AreEqual(journalEntryDto.AutomaticReversal, journalEntryDomainEntity.AutomaticReversal);
            Assert.AreEqual(journalEntryDto.Comments, journalEntryDomainEntity.Comments);
            Assert.AreEqual(journalEntryDto.TotalCredits, journalEntryDomainEntity.TotalCredits);
            Assert.AreEqual(journalEntryDto.Date, journalEntryDomainEntity.Date);
            Assert.AreEqual(journalEntryDto.TotalDebits, journalEntryDomainEntity.TotalDebits);
            Assert.AreEqual(journalEntryDto.EnteredByName, journalEntryDomainEntity.EnteredByName);
            Assert.AreEqual(journalEntryDto.EnteredDate, journalEntryDomainEntity.EnteredDate);
            Assert.AreEqual(journalEntryDto.Id, journalEntryDomainEntity.Id);
            Assert.AreEqual(journalEntryDto.Status.ToString(), journalEntryDomainEntity.Status.ToString());

            Assert.AreEqual(journalEntryDto.Type.ToString(), journalEntryDomainEntity.Type.ToString());

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < journalEntryDto.Approvers.Count(); i++)
            {
                var approverDto = journalEntryDto.Approvers[i];
                var approverDomain = journalEntryDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the journal entry item DTOs matches the domain entity
            for (int i = 0; i < journalEntryDto.Items.Count(); i++)
            {
                var journalEntryItemDto = journalEntryDto.Items[i];
                var journalEntryItemDomain = journalEntryDomainEntity.Items[i];
                Assert.AreEqual(journalEntryItemDto.Credit, journalEntryItemDomain.Credit);
                Assert.AreEqual(journalEntryItemDto.Debit, journalEntryItemDomain.Debit);
                Assert.AreEqual(journalEntryItemDto.Description, journalEntryItemDomain.Description);
                Assert.AreEqual(journalEntryItemDto.GlAccount, journalEntryItemDomain.GlAccountNumber);
                Assert.AreEqual(journalEntryItemDto.ProjectLineItemCode, journalEntryItemDomain.ProjectLineItemCode);
                Assert.AreEqual(journalEntryItemDto.ProjectNumber, journalEntryItemDomain.ProjectNumber);
            }
        }
        
        [TestMethod]
        public async Task GetJournalEntryAsync_StatusUnfinished_TypeOpeningBalance()
        {
            var journalEntryId = "J000007";
            var personId = "1";
            var journalEntryDto = await service.GetJournalEntryAsync(journalEntryId);

            // Build the journal entry object
            var journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(journalEntryDto.Author, journalEntryDomainEntity.Author);
            Assert.AreEqual(journalEntryDto.AutomaticReversal, journalEntryDomainEntity.AutomaticReversal);
            Assert.AreEqual(journalEntryDto.Comments, journalEntryDomainEntity.Comments);
            Assert.AreEqual(journalEntryDto.TotalCredits, journalEntryDomainEntity.TotalCredits);
            Assert.AreEqual(journalEntryDto.Date, journalEntryDomainEntity.Date);
            Assert.AreEqual(journalEntryDto.TotalDebits, journalEntryDomainEntity.TotalDebits);
            Assert.AreEqual(journalEntryDto.EnteredByName, journalEntryDomainEntity.EnteredByName);
            Assert.AreEqual(journalEntryDto.EnteredDate, journalEntryDomainEntity.EnteredDate);
            Assert.AreEqual(journalEntryDto.Id, journalEntryDomainEntity.Id);
            Assert.AreEqual(journalEntryDto.Status.ToString(), journalEntryDomainEntity.Status.ToString());

            Assert.AreEqual(journalEntryDto.Type.ToString(), journalEntryDomainEntity.Type.ToString());

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < journalEntryDto.Approvers.Count(); i++)
            {
                var approverDto = journalEntryDto.Approvers[i];
                var approverDomain = journalEntryDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the journal entry item DTOs matches the domain entity
            for (int i = 0; i < journalEntryDto.Items.Count(); i++)
            {
                var journalEntryItemDto = journalEntryDto.Items[i];
                var journalEntryItemDomain = journalEntryDomainEntity.Items[i];
                Assert.AreEqual(journalEntryItemDto.Credit, journalEntryItemDomain.Credit);
                Assert.AreEqual(journalEntryItemDto.Debit, journalEntryItemDomain.Debit);
                Assert.AreEqual(journalEntryItemDto.Description, journalEntryItemDomain.Description);
                Assert.AreEqual(journalEntryItemDto.GlAccount, journalEntryItemDomain.GlAccountNumber);
                Assert.AreEqual(journalEntryItemDto.ProjectLineItemCode, journalEntryItemDomain.ProjectLineItemCode);
                Assert.AreEqual(journalEntryItemDto.ProjectNumber, journalEntryItemDomain.ProjectNumber);
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullId()
        {
            var expectedParamName = "journalEntryDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetJournalEntryAsync(null);
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyId()
        {
            var expectedParamName = "journalEntryDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetJournalEntryAsync("");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetJournalEntryAsync_NullAccountStructure()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.GetJournalEntryAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetJournalEntryAsync_NullGlClassConfiguration()
        {
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            await service2.GetJournalEntryAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetJournalEntryAsync_NullGeneralLedgerUser()
        {
            // Mock the general ledger user repository method to return a null object with the service method
            GeneralLedgerUser glUser = null;
            mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));
            await service2.GetJournalEntryAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetJournalEntryAsync_RepositoryReturnsNullObject()
        {
            // Mock the GetJournalEntry repository method to return a null object within the Service method
            JournalEntry nullJournalEntry = null;
            this.mockJournalEntryRepository.Setup<Task<JournalEntry>>(jeRepo => jeRepo.GetJournalEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>())).Returns(Task.FromResult(nullJournalEntry));

            GeneralLedgerClassConfiguration glClassConfiguration = await testGeneralLedgerConfigurationRepository.GetClassConfigurationAsync();
            this.mockGlConfigurationRepository.Setup(repo => repo.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));

            GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            this.mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));

            GeneralLedgerUser glUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
            this.mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));

            var requisitionDto = await service2.GetJournalEntryAsync("1");
        }
        #endregion

        #region Tests for GetJournalEntryAsync without a view permission
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetJournalEntryAsync_PermissionException()
        {
            await serviceForNoPermission.GetJournalEntryAsync("J000001");
        }
        #endregion

        #region Build service method

        /// <summary>
        /// Builds multiple journal entry service objects
        /// </summary>
        private void BuildValidJournalEntryService()
        {
            // Use Mock to create mock implementations that are based on the same interfaces
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository = roleRepositoryMock.Object;

            var loggerObject = new Mock<ILogger>().Object;

            testJournalEntryRepository = new TestJournalEntryRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var journalEntryDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.JournalEntry, Dtos.ColleagueFinance.JournalEntry>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.JournalEntry, Dtos.ColleagueFinance.JournalEntry>()).Returns(journalEntryDtoAdapter);

            // Set up the service
            service = new JournalEntryService(testJournalEntryRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);
            service2 = new JournalEntryService(mockJournalEntryRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new JournalEntryService(testJournalEntryRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, noPermissionsUser, roleRepository, loggerObject);
        }

        #endregion
    }
}
