// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    /// <summary>
    /// Test that the service returns a valid requisition.
    /// We use GeneralLedgerCurrentUser to mimic the user logged in.
    /// </summary>
    [TestClass]
    public class RequisitionServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup

        private RequisitionService service;
        private RequisitionService service2;
        private RequisitionService serviceForNoPermission;

        private TestRequisitionRepository testRequisitionRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;

        private Mock<IRequisitionRepository> mockRequisitionRepository;
        private Mock<IGeneralLedgerConfigurationRepository> mockGlConfigurationRepository;
        private Mock<IGeneralLedgerUserRepository> mockGeneralLedgerUserRepository;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;


        private Mock<IStaffRepository> staffRepositoryMock;
        private Mock<IGeneralLedgerAccountRepository> generalLedgerAccountRepositoryMock;

        private Domain.Entities.Permission permissionViewRequisition;
        private Domain.Entities.Permission permissionDeleteRequisition;
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(228, "REQUISITION.VIEWER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();

        private Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria filterCriteria;
        private ProcurementDocumentFilterCriteria filterCriteriaDomainEntity;

        [TestInitialize]
        public void Initialize()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();

            // Set up mock requisition repository
            mockRequisitionRepository = new Mock<IRequisitionRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            staffRepositoryMock = new Mock<IStaffRepository>();
            generalLedgerAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();

            // Create permission domain entities for viewing the requisition.
            permissionViewRequisition = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewRequisition);
            permissionDeleteRequisition = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.DeleteRequisition);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewRequisition);
            glUserRoleViewPermissions.AddPermission(permissionDeleteRequisition);
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("1", "Test LastName")));

            Dictionary<string, string> descDictionary = new Dictionary<string, string>();

            generalLedgerAccountRepositoryMock.Setup(x => x.GetGlAccountDescriptionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<GeneralLedgerAccountStructure>())).Returns(() =>
            {
                return Task.FromResult(descDictionary);
            });

            // build all service objects to use in testing
            BuildValidRequisitionService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables
            service = null;
            service2 = null;
            serviceForNoPermission = null;

            testRequisitionRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;
            mockRequisitionRepository = null;
            mockGlConfigurationRepository = null;
            mockGeneralLedgerUserRepository = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleViewPermissions = null;
        }
        #endregion

        #region Tests for GetRequisitionAsync with a view permission

        [TestMethod]
        public async Task GetRequisitionAsync()
        {
            var requisitionId = "9";
            var personId = "1";
            var requisitionDto = await service.GetRequisitionAsync(requisitionId);

            // Get the requisition domain entity from the test repository
            var requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(requisitionDto.Id, requisitionDomainEntity.Id);
            Assert.AreEqual(requisitionDto.Number, requisitionDomainEntity.Number);
            Assert.AreEqual(requisitionDto.Amount, requisitionDomainEntity.Amount);
            Assert.AreEqual(requisitionDto.ApType, requisitionDomainEntity.ApType);
            Assert.AreEqual(requisitionDto.BlanketPurchaseOrder, requisitionDomainEntity.BlanketPurchaseOrder);
            Assert.AreEqual(requisitionDto.Comments, requisitionDomainEntity.Comments);
            Assert.AreEqual(requisitionDto.CurrencyCode, requisitionDomainEntity.CurrencyCode);
            Assert.AreEqual(requisitionDto.Date, requisitionDomainEntity.Date);
            Assert.AreEqual(requisitionDto.DesiredDate, requisitionDomainEntity.DesiredDate);
            Assert.AreEqual(requisitionDto.InitiatorName, requisitionDomainEntity.InitiatorName);
            Assert.AreEqual(requisitionDto.InternalComments, requisitionDomainEntity.InternalComments);
            Assert.AreEqual(requisitionDto.MaintenanceDate, requisitionDomainEntity.MaintenanceDate);
            Assert.AreEqual(requisitionDto.RequestorName, requisitionDomainEntity.RequestorName);
            Assert.AreEqual(requisitionDto.ShipToCode, requisitionDomainEntity.ShipToCode);
            Assert.AreEqual(requisitionDto.Status.ToString(), requisitionDomainEntity.Status.ToString());
            Assert.AreEqual(requisitionDto.StatusDate, requisitionDomainEntity.StatusDate);
            Assert.AreEqual(requisitionDto.VendorId, requisitionDomainEntity.VendorId);
            Assert.AreEqual(requisitionDto.VendorName, requisitionDomainEntity.VendorName);

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < requisitionDto.Approvers.Count(); i++)
            {
                var approverDto = requisitionDto.Approvers[i];
                var approverDomain = requisitionDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the list of purchase order DTOs matches the domain entity
            for (int i = 0; i < requisitionDto.PurchaseOrders.Count(); i++)
            {
                Assert.AreEqual(requisitionDto.PurchaseOrders[i], requisitionDomainEntity.PurchaseOrders[i]);
            }

            // Confirm that the data in the line item DTOs matches the domain entity
            for (int i = 0; i < requisitionDto.LineItems.Count(); i++)
            {
                var lineItemDto = requisitionDto.LineItems[i];
                var lineItemDomain = requisitionDomainEntity.LineItems[i];
                Assert.AreEqual(lineItemDto.Comments, lineItemDomain.Comments);
                Assert.AreEqual(lineItemDto.Description, lineItemDomain.Description);
                Assert.AreEqual(lineItemDto.DesiredDate, lineItemDomain.DesiredDate);
                Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemDomain.ExtendedPrice);
                Assert.AreEqual(lineItemDto.Price, lineItemDomain.Price);
                Assert.AreEqual(lineItemDto.Quantity, lineItemDomain.Quantity);
                Assert.AreEqual(lineItemDto.TaxForm, lineItemDomain.TaxForm);
                Assert.AreEqual(lineItemDto.TaxFormCode, lineItemDomain.TaxFormCode);
                Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemDomain.TaxFormLocation);
                Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemDomain.UnitOfIssue);
                Assert.AreEqual(lineItemDto.VendorPart, lineItemDomain.VendorPart);

                // Confirm that the data in the line item GL distribution DTOs matches the domain entity
                for (int j = 0; j < lineItemDto.GlDistributions.Count(); j++)
                {
                    var glDistributionDto = lineItemDto.GlDistributions[j];
                    var glDistributionDomain = lineItemDomain.GlDistributions[j];
                    Assert.AreEqual(glDistributionDto.Amount, glDistributionDomain.Amount);
                    Assert.AreEqual(glDistributionDto.GlAccount, glDistributionDomain.GlAccountNumber);
                    Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionDomain.ProjectLineItemCode);
                    Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionDomain.ProjectNumber);
                    Assert.AreEqual(glDistributionDto.Quantity, glDistributionDomain.Quantity);
                }

                // Confirm that the data in the line item tax DTOs matches the domain entity
                for (int k = 0; k < lineItemDto.LineItemTaxes.Count(); k++)
                {
                    var lineItemTaxDto = lineItemDto.LineItemTaxes[k];
                    var lineItemTaxDomain = lineItemDomain.LineItemTaxes[k];
                    Assert.AreEqual(lineItemTaxDto.TaxCode, lineItemTaxDomain.TaxCode);
                    Assert.AreEqual(lineItemTaxDto.TaxAmount, lineItemTaxDomain.TaxAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_OneGlAccountIsMasked()
        {
            var requisitionId = "999";
            var personId = "1";
            var requisitionDto = await service.GetRequisitionAsync(requisitionId);

            // Get the purchase order domain entity from the test repository
            var requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(requisitionDto.Id, requisitionDomainEntity.Id);

            // There should only be one line item.
            Assert.AreEqual(1, requisitionDomainEntity.LineItems.Count);
            Assert.AreEqual(1, requisitionDto.LineItems.Count);

            // Confirm that the DTO line item data matches the data in the domain entity.
            var lineItemDto = requisitionDto.LineItems.First();
            var lineItemEntity = requisitionDomainEntity.LineItems.First();

            Assert.AreEqual(lineItemDto.Comments, lineItemEntity.Comments);
            Assert.AreEqual(lineItemDto.Description, lineItemEntity.Description);
            Assert.AreEqual(lineItemDto.ExpectedDeliveryDate, lineItemEntity.ExpectedDeliveryDate);
            Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemEntity.ExtendedPrice);
            Assert.AreEqual(lineItemDto.Price, lineItemEntity.Price);
            Assert.AreEqual(lineItemDto.Quantity, lineItemEntity.Quantity);
            Assert.AreEqual(lineItemDto.TaxForm, lineItemEntity.TaxForm);
            Assert.AreEqual(lineItemDto.TaxFormCode, lineItemEntity.TaxFormCode);
            Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemEntity.TaxFormLocation);
            Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemEntity.UnitOfIssue);
            Assert.AreEqual(lineItemDto.VendorPart, lineItemEntity.VendorPart);

            var glConfiguration = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();

            // Confirm that the data in the line item GL distribution DTOs matches the domain entity
            foreach (var glDistributionEntity in lineItemEntity.GlDistributions)
            {
                var glDistributionDto = lineItemDto.GlDistributions.FirstOrDefault(x => x.GlAccount == glDistributionEntity.GlAccountNumber);

                // Check the values in each of the non-masked GL accounts.
                if (glDistributionDto != null)
                {
                    Assert.AreEqual(glDistributionDto.GlAccount, glDistributionEntity.GlAccountNumber);
                    Assert.AreEqual(glDistributionDto.FormattedGlAccount, glDistributionEntity.GetFormattedMaskedGlAccount(glConfiguration.MajorComponentStartPositions));
                    Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionEntity.ProjectNumber);
                    Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionEntity.ProjectLineItemCode);
                    Assert.AreEqual(glDistributionDto.Quantity, glDistributionEntity.Quantity);
                    Assert.AreEqual(glDistributionDto.Amount, glDistributionEntity.Amount);
                }
            }

            // Get all of the masked GL account DTOs and confirm that their values are either 0 or null.
            var maskedGlDistributionEntity = lineItemEntity.GlDistributions.FirstOrDefault(x => x.Masked);
            var maskedGlAccountNumber = maskedGlDistributionEntity.GetFormattedMaskedGlAccount(glConfiguration.MajorComponentStartPositions);
            var maskedGlDistributionDtos = lineItemDto.GlDistributions.Where(x => x.FormattedGlAccount == maskedGlAccountNumber).ToList();

            foreach (var glDistributionDto in maskedGlDistributionDtos)
            {
                Assert.AreEqual(null, glDistributionDto.GlAccount);
                Assert.AreEqual(maskedGlAccountNumber, glDistributionDto.FormattedGlAccount);
                Assert.AreEqual(null, glDistributionDto.ProjectNumber);
                Assert.AreEqual(null, glDistributionDto.ProjectLineItemCode);
                Assert.AreEqual(0m, glDistributionDto.Quantity);
                Assert.AreEqual(0m, glDistributionDto.Amount);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_InProgressStatus()
        {
            var requisitionId = "4";
            var personId = "1";
            var requisitionDto = await service.GetRequisitionAsync(requisitionId);

            // Get the requisition domain entity from the test repository
            var requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(requisitionDto.Status.ToString(), requisitionDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetRequisitionAsync_NotApprovedStatus()
        {
            var requisitionId = "2";
            var personId = "1";
            var requisitionDto = await service.GetRequisitionAsync(requisitionId);

            // Get the requisition domain entity from the test repository
            var requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(requisitionDto.Status.ToString(), requisitionDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetRequisitionAsync_PoCreatedStatus()
        {
            var requisitionId = "6";
            var personId = "1";
            var requisitionDto = await service.GetRequisitionAsync(requisitionId);

            // Get the requisition domain entity from the test repository
            var requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(requisitionDto.Status.ToString(), requisitionDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetRequisitionAsync_HasMultiplePurchaseOrders()
        {
            var requisitionId = "3";
            var personId = "1";
            var requisitionDto = await service.GetRequisitionAsync(requisitionId);

            // Get the requisition domain entity from the test repository
            var requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the list of purchase order DTOs matches the domain entity
            foreach (var purchaseOrder in requisitionDomainEntity.PurchaseOrders)
            {
                Assert.IsTrue(requisitionDto.PurchaseOrders.Any(poId => poId == purchaseOrder), "The PO IDs in the domain entity must be the same as the IDs in the DTO.");
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_NullId()
        {
            var expectedParamName = "requisitionDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetRequisitionAsync(null);
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_EmptyId()
        {
            var expectedParamName = "requisitionDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetRequisitionAsync("");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisitionAsync_NullAccountStructure()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.GetRequisitionAsync("9");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisitionAsync_NullGlClassConfiguration()
        {
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            await service2.GetRequisitionAsync("9");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisitionAsync_NullGeneralLedgerUser()
        {
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerUser glUser = null;
            mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));
            await service2.GetRequisitionAsync("9");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisitionAsync_RepositoryReturnsNullObject()
        {
            // Mock the GetRequisition repository method to return a null object within the Service method
            Requisition nullRequisition = null;
            this.mockRequisitionRepository.Setup<Task<Requisition>>(reqRepo => reqRepo.GetRequisitionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>())).Returns(Task.FromResult(nullRequisition));
            var requisitionDto = await service2.GetRequisitionAsync("1");
        }
        #endregion

        #region Tests for GetRequisitionAsync without a view permission
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRequisitionAsync_PermissionException()
        {
            await serviceForNoPermission.GetRequisitionAsync("9");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_ArgumentNullException()
        {
            await service2.GetRequisitionsSummaryByPersonIdAsync(null);
        }

        #endregion

        #region Tests for GetRequisitionsSummaryByPersonIdAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_EmptyPersonId_ArgumentNullException()
        {
            await service2.GetRequisitionsSummaryByPersonIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_CurrentUserDifferentFromRequest()
        {
            await service2.GetRequisitionsSummaryByPersonIdAsync("0016357");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_MissingPermissionException()
        {
            await serviceForNoPermission.GetRequisitionsSummaryByPersonIdAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_StaffRecordMissingException()
        {
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            await service2.GetRequisitionsSummaryByPersonIdAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        public async Task GetRequisitionsSummaryByPersonIdAsync()
        {
            var requisitionId = "9";
            var personId = currentUserFactory.CurrentUser.PersonId;
            var requisitionSummaryListDto = await service.GetRequisitionsSummaryByPersonIdAsync(personId);


            // Get the requisition domain entity from the test repository
            var requisitionSummaryListDomainEntity = await testRequisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);

            Assert.IsNotNull(requisitionSummaryListDto);
            Assert.AreEqual(requisitionSummaryListDto.ToList().Count, requisitionSummaryListDomainEntity.ToList().Count);

            var requisitionDto = requisitionSummaryListDto.Where(x => x.Id == requisitionId).FirstOrDefault();
            var requisitionDomainEntity = requisitionSummaryListDomainEntity.Where(x => x.Id == requisitionId).FirstOrDefault();

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(requisitionDto.Id, requisitionDomainEntity.Id);
            Assert.AreEqual(requisitionDto.Number, requisitionDomainEntity.Number);
            Assert.AreEqual(requisitionDto.Amount, requisitionDomainEntity.Amount);
            Assert.AreEqual(requisitionDto.Date, requisitionDomainEntity.Date);
            Assert.AreEqual(requisitionDto.InitiatorName, requisitionDomainEntity.InitiatorName);
            Assert.AreEqual(requisitionDto.RequestorName, requisitionDomainEntity.RequestorName);
            Assert.AreEqual(requisitionDto.Status.ToString(), requisitionDomainEntity.Status.ToString());
            Assert.AreEqual(requisitionDto.StatusDate, requisitionDomainEntity.StatusDate);
            Assert.AreEqual(requisitionDto.VendorId, requisitionDomainEntity.VendorId);
            Assert.AreEqual(requisitionDto.VendorName, requisitionDomainEntity.VendorName);


            // Confirm that the data in the list of purchase order DTOs matches the domain entity
            for (int i = 0; i < requisitionDto.PurchaseOrders.Count(); i++)
            {
                Assert.AreEqual(requisitionDto.PurchaseOrders[i].Id, requisitionDomainEntity.PurchaseOrders[i].Id);
                Assert.AreEqual(requisitionDto.PurchaseOrders[i].Number, requisitionDomainEntity.PurchaseOrders[i].Number);
            }
        }
        #endregion

        #region Tests for Requisition Delete

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RequisitionService_DeleteRequisitionsAsync_ArgumentNullException()
        {
            await service2.DeleteRequisitionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RequisitionService_DeleteRequisitionsAsync_EmptyPersonId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "";
            await service2.DeleteRequisitionsAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RequisitionService_DeleteRequisitionsAsync_EmptyRequisitionId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "0000004";
            await service2.DeleteRequisitionsAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RequisitionService_DeleteRequisitionsAsync_EmptyEmail_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "000123";
            abc.RequisitionId = "000123";
            abc.ConfirmationEmailAddresses = "";
            await service2.DeleteRequisitionsAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task RequisitionService_DeleteRequisitionsAsync_NotLoggedInuser()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "000123";
            abc.RequisitionId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.DeleteRequisitionsAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task RequisitionService_DeleteRequisitionsAsync_StaffRecordMissingException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "000123";
            abc.RequisitionId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));

            await service2.DeleteRequisitionsAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task RequisitionService_DeleteRequisitionsAsync_PermissionsException()
        {
            List<Domain.Entities.Role> roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"DELETE.REQ")
                    };

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(roles);

            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "000123";
            abc.RequisitionId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.DeleteRequisitionsAsync(abc);
        }

        [TestMethod]
        public async Task RequisitionService_DeleteRequisitionsAsync()
        {
            
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "0000001";
            abc.RequisitionId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";

            //roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(roles);
            var deleteReqDtos = await service2.DeleteRequisitionsAsync(abc);
            Assert.IsNotNull(deleteReqDtos);
        }

        [TestMethod]
        public async Task RequisitionService_DeleteRequisitionsAsyncWithValues()
        {

            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest();
            abc.PersonId = "0000001";
            abc.RequisitionId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";

            RequisitionDeleteResponse res = new RequisitionDeleteResponse();
            res.RequisitionId = "000111";
            res.RequisitionNumber = "REQ0001";
            res.ErrorOccured = false;
            res.ErrorMessages = null;
            res.WarningOccured = true;
            res.WarningMessages = new List<string>() { "Please refresh Page" };

            //var loggerObject = new Mock<ILogger>().Object;

            mockRequisitionRepository.Setup(r => r.DeleteRequisitionsAsync(It.IsAny<RequisitionDeleteRequest>())).ReturnsAsync(res);
            
            var deleteReqDtos = await service2.DeleteRequisitionsAsync(abc);
            Assert.IsNotNull(deleteReqDtos);
        }

        #endregion

        #region Tests for QueryRequisitionSummariesAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryRequisitionSummariesAsync_NUllCriteria_ArgumentNullException()
        {
            await service2.QueryRequisitionSummariesAsync(null);
        }

        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryRequisitionSummariesAsync_EmptyCriteria_ArgumentNullException()
        {
            filterCriteria = new Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria();
            await service2.QueryRequisitionSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryRequisitionSummariesAsync_CurrentUserDifferentFromRequest()
        {
            filterCriteria = new Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria();
            filterCriteria.PersonId = "0016357";            

            await service2.QueryRequisitionSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryRequisitionSummariesAsync_MissingPermissionException()
        {
            await serviceForNoPermission.QueryRequisitionSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryRequisitionSummariesAsync_StaffRecordMissingException()
        {
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            await service2.QueryRequisitionSummariesAsync(filterCriteria);
        }

        [TestMethod]
        public async Task QueryRequisitionSummariesAsync()
        {
            var requisitionId = "1";
            var personId = currentUserFactory.CurrentUser.PersonId;
            var requisitionSummaryListDto = await service.QueryRequisitionSummariesAsync(filterCriteria);

            filterCriteriaDomainEntity = new ProcurementDocumentFilterCriteria();
            filterCriteriaDomainEntity.PersonId = currentUserFactory.CurrentUser.PersonId;
            filterCriteriaDomainEntity.VendorIds = new List<string>() { "0009876" };

            // Get the requisition domain entity from the test repository
            var requisitionSummaryListDomainEntity = await testRequisitionRepository.QueryRequisitionSummariesAsync(filterCriteriaDomainEntity);

            Assert.IsNotNull(requisitionSummaryListDto);
            Assert.AreEqual(requisitionSummaryListDto.ToList().Count, requisitionSummaryListDomainEntity.ToList().Count);

            var requisitionDto = requisitionSummaryListDto.Where(x => x.Id == requisitionId).FirstOrDefault();
            var requisitionDomainEntity = requisitionSummaryListDomainEntity.Where(x => x.Id == requisitionId).FirstOrDefault();

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(requisitionDto.Id, requisitionDomainEntity.Id);
            Assert.AreEqual(requisitionDto.Number, requisitionDomainEntity.Number);
            Assert.AreEqual(requisitionDto.Amount, requisitionDomainEntity.Amount);
            Assert.AreEqual(requisitionDto.Date, requisitionDomainEntity.Date);
            Assert.AreEqual(requisitionDto.InitiatorName, requisitionDomainEntity.InitiatorName);
            Assert.AreEqual(requisitionDto.RequestorName, requisitionDomainEntity.RequestorName);
            Assert.AreEqual(requisitionDto.Status.ToString(), requisitionDomainEntity.Status.ToString());
            Assert.AreEqual(requisitionDto.StatusDate, requisitionDomainEntity.StatusDate);
            Assert.AreEqual(requisitionDto.VendorId, requisitionDomainEntity.VendorId);
            Assert.AreEqual(requisitionDto.VendorName, requisitionDomainEntity.VendorName);


            // Confirm that the data in the list of purchase order DTOs matches the domain entity
            for (int i = 0; i < requisitionDto.PurchaseOrders.Count(); i++)
            {
                Assert.AreEqual(requisitionDto.PurchaseOrders[i].Id, requisitionDomainEntity.PurchaseOrders[i].Id);
                Assert.AreEqual(requisitionDto.PurchaseOrders[i].Number, requisitionDomainEntity.PurchaseOrders[i].Number);
            }
        }
        #endregion

        #region Build service method

        /// <summary>
        /// Builds multiple requisition service objects.
        /// </summary>
        private void BuildValidRequisitionService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository = roleRepositoryMock.Object;

            var loggerObject = new Mock<ILogger>().Object;

            testRequisitionRepository = new TestRequisitionRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            var colleagueFinanceReferenceDataRepository = new TestColleagueFinanceReferenceDataRepository();
            var accountFundsAvailableRepo = new Mock<IAccountFundsAvailableRepository>();
            var referenceDataRepo = new Mock<IReferenceDataRepository>();
            var buyersDataRepo = new Mock<IBuyerRepository>();
            var personDataRepo = new Mock<IPersonRepository>();
            var vendorsDataRepo = new Mock<IVendorsRepository>();
            var addressDataRepo = new Mock<IAddressRepository>();
            var confDatRepo = new Mock<IConfigurationRepository>();
            var procurementsUtilityServiceMock = new Mock<IProcurementsUtilityService>();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var requisitionDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Requisition, Dtos.ColleagueFinance.Requisition>(adapterRegistry.Object, loggerObject);
            var requisitionDtoAdapter_Delete = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.RequisitionDeleteResponse, Dtos.ColleagueFinance.RequisitionDeleteResponse>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.Requisition, Dtos.ColleagueFinance.Requisition>()).Returns(requisitionDtoAdapter);

            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.RequisitionDeleteResponse, Dtos.ColleagueFinance.RequisitionDeleteResponse>()).Returns(requisitionDtoAdapter_Delete);

            var procurementCriteria_Adapter = new AutoMapperAdapter<Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ProcurementDocumentFilterCriteria>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ProcurementDocumentFilterCriteria>()).Returns(procurementCriteria_Adapter);


            // Set up the service objects
            service = new RequisitionService(testRequisitionRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, accountFundsAvailableRepo.Object, referenceDataRepo.Object, buyersDataRepo.Object, personDataRepo.Object,
                vendorsDataRepo.Object, addressDataRepo.Object, confDatRepo.Object, adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerObject);
            service2 = new RequisitionService(mockRequisitionRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object,
                colleagueFinanceReferenceDataRepository, accountFundsAvailableRepo.Object, referenceDataRepo.Object, buyersDataRepo.Object, personDataRepo.Object,
                vendorsDataRepo.Object, addressDataRepo.Object, confDatRepo.Object, adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new RequisitionService(testRequisitionRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, accountFundsAvailableRepo.Object, referenceDataRepo.Object, buyersDataRepo.Object, personDataRepo.Object,
                vendorsDataRepo.Object, addressDataRepo.Object, confDatRepo.Object, adapterRegistry.Object, noPermissionsUser, roleRepository, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerObject);

            filterCriteria = new Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria();
            filterCriteria.PersonId = currentUserFactory.CurrentUser.PersonId;
            filterCriteria.VendorIds = new List<string>() { "0009876" };

        }
        #endregion
    }

    [TestClass]
    public class RequisitionServiceTests_V11
    {
        [TestClass]
        public class RequisitionServiceTests_GET_AND_DELETE : GeneralLedgerCurrentUser
        {
            #region DECLARATION

            protected Domain.Entities.Role getRequisitions = new Domain.Entities.Role(1, "VIEW.REQUISITIONS");

            private Mock<IRequisitionRepository> requisitionRepositoryMock;
            private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
            private Mock<IGeneralLedgerUserRepository> generalLedgerUserRepositoryMock;
            private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IBuyerRepository> buyerRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IVendorsRepository> vendorsRepositoryMock;
            private Mock<IAddressRepository> addressRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<IStaffRepository> staffRepositoryMock;
            private Mock<IGeneralLedgerAccountRepository> generalLedgerAccountRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountFundsAvailableRepository> accountFundsAvailableRepo;
            private UserFactorySubset currentUserFactory;
            private Mock<IProcurementsUtilityService> procurementsUtilityServiceMock;

            private RequisitionService requisitionService;

            private Requisition requisition;
            private IEnumerable<ShipToDestination> shipToDestinations;
            private IEnumerable<FreeOnBoardType> freeOnBoardTypes;
            private IEnumerable<Domain.Base.Entities.Country> countries;
            private IEnumerable<Domain.Base.Entities.State> states;
            private Domain.Base.Entities.Address addressEntity;
            private IEnumerable<VendorTerm> vendorTerms;
            private IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes;
            private IEnumerable<Domain.ColleagueFinance.Entities.FxaTransferFlags> fxaFlags;
            private IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType> unitTypes;
            private IEnumerable<Domain.Base.Entities.CommerceTaxCode> commerceTaxCodes;
            private LineItem lineItem;
            private LineItemGlDistribution lineItemGlDistribution;
            private LineItemTax lineItemTax;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                requisitionRepositoryMock = new Mock<IRequisitionRepository>();
                generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
                generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
                colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                buyerRepositoryMock = new Mock<IBuyerRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                vendorsRepositoryMock = new Mock<IVendorsRepository>();
                addressRepositoryMock = new Mock<IAddressRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                accountFundsAvailableRepo = new Mock<IAccountFundsAvailableRepository>();
                staffRepositoryMock = new Mock<IStaffRepository>();
                generalLedgerAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
                currentUserFactory = new GeneralLedgerCurrentUser.UserFactorySubset();
                procurementsUtilityServiceMock = new Mock<IProcurementsUtilityService>();
                requisitionService = new RequisitionService(requisitionRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object, generalLedgerUserRepositoryMock.Object,
                    colleagueFinanceReferenceDataRepositoryMock.Object, accountFundsAvailableRepo.Object, referenceDataRepositoryMock.Object, buyerRepositoryMock.Object, personRepositoryMock.Object, vendorsRepositoryMock.Object,
                    addressRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerMock.Object);

                InitializeTestData();

                InitializeMock();
            }

            [TestCleanup]
            public void Cleanup()
            {
                requisitionRepositoryMock = null;
                generalLedgerConfigurationRepositoryMock = null;
                generalLedgerUserRepositoryMock = null;
                colleagueFinanceReferenceDataRepositoryMock = null;
                buyerRepositoryMock = null;
                referenceDataRepositoryMock = null;
                vendorsRepositoryMock = null;
                personRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;

                requisitionService = null;
            }

            private void InitializeTestData()
            {
                addressEntity = new Domain.Base.Entities.Address()
                {
                    Guid = guid,
                    AddressLines = new List<string>() { "12245 Spring Hill Blvd." },
                    City = "Denver",
                    State = "SA",
                    PostalCode = "88495"
                };

                shipToDestinations = new List<ShipToDestination>() { new ShipToDestination(guid, "1", "description") };

                freeOnBoardTypes = new List<FreeOnBoardType>() { new FreeOnBoardType(guid, "1", "description") };

                countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country("USA", "America", "US", "USA"),
                    new Domain.Base.Entities.Country("CAN", "Canada", "CA", "CAN"),
                    new Domain.Base.Entities.Country("AUS", "Austria", "AU", "AUS"),
                    new Domain.Base.Entities.Country("BRA", "Brazil", "BR", "BRA"),
                    new Domain.Base.Entities.Country("MEX", "Mexico", "MX", "MEX"),
                    new Domain.Base.Entities.Country("NLD", "Netherland", "ND", "NLD"),
                    new Domain.Base.Entities.Country("GBR", "London", "GB", "GBR"),
                    new Domain.Base.Entities.Country("ITL", "ITALY", "IL", "ILT"),
                    new Domain.Base.Entities.Country("JPN", "Japan", "JP", "JPN")
                };

                states = new List<Domain.Base.Entities.State>()
                {
                    new Domain.Base.Entities.State("VA", "description", "GBR"),
                    new Domain.Base.Entities.State("AV", "description", "IND"),
                    new Domain.Base.Entities.State("SA", "sa", "USA"),
                };

                vendorTerms = new List<VendorTerm>() { new VendorTerm(guid, "1", "description") };

                commodityCodes = new List<Domain.ColleagueFinance.Entities.CommodityCode>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityCode(guid, "1", "description")
                };

                fxaFlags = new List<Domain.ColleagueFinance.Entities.FxaTransferFlags>()
                {
                    new Domain.ColleagueFinance.Entities.FxaTransferFlags(guid, "1", "description")
                };

                unitTypes = new List<Domain.ColleagueFinance.Entities.CommodityUnitType>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityUnitType(guid, "1", "description")
                };

                commerceTaxCodes = new List<Domain.Base.Entities.CommerceTaxCode>()
                {
                    new Domain.Base.Entities.CommerceTaxCode(guid, "1", "description")
                };

                requisition = getRequisition();

                lineItem = new LineItem("1", "description", 1, 100, 10)
                {
                    Comments = "comments",
                    CommodityCode = "1",
                    FixedAssetsFlag = "1",
                    DesiredDate = DateTime.Now,
                    ExpectedDeliveryDate = DateTime.Now.AddDays(10),
                    InvoiceNumber = "1",
                    LineItemStatus = LineItemStatus.Outstanding,
                    StatusDate = DateTime.Now,
                    TaxForm = "1",
                    TaxFormCode = "1",
                    TaxFormLocation = "1",
                    TradeDiscountAmount = 10,
                    TradeDiscountPercentage = 1,
                    UnitOfIssue = "1",
                    VendorPart = "1"
                };

                lineItemGlDistribution = new LineItemGlDistribution("11-00-02-67-60000-54005", 10, 100, 1)
                {
                    GlAccountDescription = "description",
                    Masked = true,
                    ProjectId = "1",
                    ProjectLineItemCode = "1",
                    ProjectLineItemId = "1",
                    ProjectNumber = "1"
                };

                lineItem.AddGlDistribution(lineItemGlDistribution);

                lineItemTax = new LineItemTax("1", 100) { LineGlNumber = "1", TaxAmount = 100, TaxGlNumber = "1" };

                lineItem.AddTax(lineItemTax);

                requisition.AddLineItem(lineItem);
            }

            private Requisition getRequisition(RequisitionStatus status = RequisitionStatus.Outstanding)
            {
                return new Requisition("000123", guid, "1", "vendorName", status, DateTime.Now, DateTime.Now)
                {
                    CurrencyCode = "USD",
                    HostCountry = "CAN",
                    IntgSubmittedBy = guid,
                    MaintenanceDate = DateTime.Now,
                    DesiredDate = DateTime.Now,
                    Buyer = "1",
                    InitiatorName = "initiatorName",
                    DefaultInitiator = "1",
                    ShipToCode = "1",
                    Fob = "1",
                    AltShippingName = "AltShippingName",
                    AltShippingAddress = new List<string>() { "Address1" },
                    IntgAltShipCountry = "USA",
                    MiscCountry = "CAN",
                    AltShippingCity = "City",
                    AltShippingCountry = "USA",
                    AltShippingPhone = "1234",
                    AltShippingPhoneExt = "111",
                    AltShippingState = "VA",
                    AltShippingZip = "12345",
                    VendorId = "1",
                    VendorPreferredAddressId = "1",
                    MiscName = new List<string>() { "Name1" },
                    IntgCorpPerIndicator = "PERSON",
                    MiscAddress = new List<string>() { "Address1" },
                    VendorTerms = "1",
                    Comments = "comments",
                    InternalComments = "internal Comments",
                    Type="procurement"
                };
            }

            private async void InitializeMock()
            {
                getRequisitions.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewRequisitions));
                getRequisitions.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.DeleteRequisitions));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getRequisitions });

                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ReturnsAsync(requisition);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<String>())).ReturnsAsync(guid);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                buyerRepositoryMock.Setup(b => b.GetBuyerGuidFromIdAsync(It.IsAny<String>())).ReturnsAsync(guid);
                buyerRepositoryMock.Setup(b => b.GetBuyerIdFromGuidAsync(It.IsAny<String>())).ReturnsAsync("1");
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetShipToDestinationsAsync(It.IsAny<bool>())).ReturnsAsync(shipToDestinations);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFreeOnBoardTypesAsync(It.IsAny<bool>())).ReturnsAsync(freeOnBoardTypes);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTerms);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFxaTransferFlagsAsync(It.IsAny<bool>())).ReturnsAsync(fxaFlags);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(unitTypes);
                foreach (var record in shipToDestinations)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetShipToDestinationGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in freeOnBoardTypes)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFreeOnBoardTypeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in vendorTerms)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetVendorTermGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in commodityCodes)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityCodeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in fxaFlags)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFxaTransferFlagGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in unitTypes)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityUnitTypeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }
                referenceDataRepositoryMock.Setup(f => f.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(commerceTaxCodes);
                foreach (var record in commerceTaxCodes)
                {
                    referenceDataRepositoryMock.Setup(f => f.GetCommerceTaxCodeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);
                referenceDataRepositoryMock.Setup(r => r.GetStateCodesAsync(It.IsAny<bool>())).ReturnsAsync(states);
                vendorsRepositoryMock.Setup(v => v.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                addressRepositoryMock.Setup(a => a.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                addressRepositoryMock.Setup(p => p.GetAddressAsync(It.IsAny<string>())).ReturnsAsync(addressEntity);
                addressRepositoryMock.Setup(p => p.GetHostCountryAsync()).ReturnsAsync("USA");
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", guid);
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);

                var vendorGuidCollection = new Dictionary<string, string>();
                vendorGuidCollection.Add("1", guid);
                vendorsRepositoryMock.Setup(p => p.GetVendorGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);


            }

            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_ArgumentNullException_When_Guid_Null()
            {
                await requisitionService.GetRequisitionsByGuidAsync(null);
            }

            

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_Exception_From_Repository()
            {
                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ThrowsAsync(new KeyNotFoundException());

                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_Exception_From_Repository1()
            {
                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ThrowsAsync(new InvalidOperationException());

                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_Exception_From_Repository2()
            {
                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ThrowsAsync(new RepositoryException());

                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

           
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_Exception_From_Repository3()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ThrowsAsync(new Exception());

                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_ConvertRequisitionEntityToDtoAsync_Exception_When_Requisition_Null()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ReturnsAsync(() => null);
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_ConvertRequisitionEntityToDtoAsync_Exception_When_Invalid_SubmittedBy()
            {
                // Make sure executing of setting default currency code when sending invalid currency code.

                requisition = getRequisition(RequisitionStatus.NotApproved);

                requisition.CurrencyCode = "ABC";

                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ReturnsAsync(requisition);

                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<String>())).ReturnsAsync(() => null);
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_ConvertRequisitionEntityToDtoAsync_Exception_When_Invalid_Buyer()
            {
                // Setting the values to cover lines of GetCurrencyIsoCode
                requisition = getRequisition(RequisitionStatus.Outstanding);
                requisition.HostCountry = null;
                requisition.CurrencyCode = "ABC";
                requisition.HostCountry = "CANADA";

                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ReturnsAsync(requisition);
                buyerRepositoryMock.Setup(b => b.GetBuyerIdFromGuidAsync(It.IsAny<String>())).ReturnsAsync("1");
                buyerRepositoryMock.Setup(p => p.GetBuyerGuidFromIdAsync(It.IsAny<String>())).ReturnsAsync(() => null);
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_When_Invalid_DefaultInitiator()
            {
                requisition = getRequisition(RequisitionStatus.PoCreated);
                requisition.IntgSubmittedBy = null;
                requisition.CurrencyCode = "ABC";
                requisition.HostCountry = "USA";

                requisitionRepositoryMock.Setup(r => r.GetRequisitionsByGuidAsync(It.IsAny<String>())).ReturnsAsync(requisition);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<String>())).ReturnsAsync(() => null);
                personRepositoryMock.Setup(b => b.GetPersonIdFromGuidAsync(It.IsAny<String>())).ReturnsAsync("1");
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("99999", guid);
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_Unable_To_Retrieve_ShipToDestination()
            {
                requisition.CurrencyCode = null;
                requisition.HostCountry = "CAN";

                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetShipToDestinationGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_When_Invalid_ShipToDestination()
            {
                requisition.ShipToCode = "2";
                requisition.CurrencyCode = null;
                requisition.HostCountry = "CANADA";
                requisition.IntgSubmittedBy = null;
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetShipToDestinationGuidAsync(requisition.ShipToCode)).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_Unable_To_Retrieve_FreeOnBoardTypes()
            {
                requisition.CurrencyCode = null;
                requisition.HostCountry = "USA";

                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFreeOnBoardTypeGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_When_Invalid_FreeOnBoardType()
            {
                requisition.Fob = "2";
                requisition.IntgSubmittedBy = null;
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFreeOnBoardTypeGuidAsync(requisition.Fob)).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_BuildOverrideShippingDestinationDtoAsync_Exception_On_Invalid_IntgAltShipCountry()
            {
                requisition.IntgSubmittedBy = null;
                requisition.IntgAltShipCountry = "ASU";
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_BuildOverrideShippingDestinationDtoAsync_Exception_On_Invalid_Country_ISO3Code()
            {
                requisition.IntgAltShipCountry = "ITL";
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_BuildOverrideShippingDestinationDtoAsync_ArgumentException_When_State_NotFound()
            {
                requisition.AltShippingState = "AU";
                requisition.IntgSubmittedBy = null;
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_ConvertRequisitionEntityToDtoAsync_Exception_When_Invalid_VendorId()
            {
                requisition.IntgAltShipCountry = null;
                requisition.MiscCountry = "CAN";

                vendorsRepositoryMock.Setup(v => v.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_GetRequisitionsByGuidAsync_ConvertRequisitionEntityToDtoAsync_IntegrationApiException_Unable_To_Retrieve_VendorTerms()
            {
                requisition.IntgAltShipCountry = null;
                requisition.MiscCountry = null;

                requisition.VendorId = null;
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetVendorTermGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_IntegrationApiException_On_Invalid_VendorTerms()
            {
                requisition.IntgSubmittedBy = null;
                requisition.IntgAltShipCountry = null;
                requisition.MiscCountry = null;
                requisition.AltShippingState = "AV";
                requisition.VendorId = null;
                requisition.IntgCorpPerIndicator = "ORGANIZATION";
                requisition.VendorTerms = "2";
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetVendorTermGuidAsync(requisition.VendorTerms)).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_Unable_To_Retrieve_CommodityCode()
            {
                requisition.IntgAltShipCountry = null;
                requisition.MiscCountry = null;
                requisition.AltShippingState = "AV";
                requisition.AltShippingCountry = "CAN";
                requisition.VendorId = null;
                requisition.IntgCorpPerIndicator = null;
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityCodeGuidAsync(It.IsAny<String>())).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_Unable_To_Retrieve_fxaflag()
            {
                requisition.IntgAltShipCountry = null;
                requisition.MiscCountry = null;
                requisition.AltShippingState = "AV";
                requisition.AltShippingCountry = "CAN";
                requisition.VendorId = null;
                requisition.IntgCorpPerIndicator = null;
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFxaTransferFlagGuidAsync(It.IsAny<String>())).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_IntegrationApiException_On_Invalid_CommodityCode()
            {

                requisition.IntgSubmittedBy = null;
                requisition.IntgAltShipCountry = "AUS";
                requisition.IntgCorpPerIndicator = "P"; // Invalid data to cover default case of switch in code.
                lineItem.CommodityCode = "2";
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityCodeGuidAsync(lineItem.CommodityCode)).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_KeyNotFoundException_On_Invalid_fxaflag()
            {

                requisition.IntgSubmittedBy = null;
                requisition.IntgAltShipCountry = "AUS";
                requisition.IntgCorpPerIndicator = "P"; // Invalid data to cover default case of switch in code.
                lineItem.FixedAssetsFlag = "2";
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFxaTransferFlagGuidAsync(lineItem.CommodityCode)).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_Unable_To_Retrieve_UnitOfIssues()
            {
                requisition.IntgAltShipCountry = "BRA";
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityUnitTypeGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_IntegrationApiException_On_Invalid_UnitOfIssue()
            {
                requisition.IntgSubmittedBy = null;
                requisition.IntgAltShipCountry = "MEX";
                lineItem.UnitOfIssue = "2";
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityUnitTypeGuidAsync(lineItem.UnitOfIssue)).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_ConvertRequisitionEntityToDtoAsync_Exception_Unable_To_Retrieve_CommerceTaxCodes()
            {
                requisition.IntgSubmittedBy = null;
                requisition.IntgAltShipCountry = "NLD";
                referenceDataRepositoryMock.Setup(f => f.GetCommerceTaxCodeGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await requisitionService.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task RequisitionService_GetRequisitionsByGuidAsync()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisition.IntgAltShipCountry = "JPN";

                requisition.AddLineItem(new LineItem("2", "description", 100, 1000, 10)
                {
                    TradeDiscountPercentage = 1
                });

                var result = await requisitionService.GetRequisitionsByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
                Assert.IsTrue(result.LineItems.Count == 2);
            }

            #endregion

            #region GETALL

            [TestMethod]
            public async Task RequisitionService_GetRequisitionsAsync()
            {
                var requisitions = new List<Requisition>() { requisition };
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitions.Add(new Requisition("2", Guid.NewGuid().ToString(), "2", "name", RequisitionStatus.PoCreated, DateTime.Now, DateTime.Now));
                requisition.IntgSubmittedBy = null;
                var tupleRequisitions = new Tuple<IEnumerable<Requisition>, int>(requisitions, 2);
                requisitionRepositoryMock.Setup(r => r.GetRequisitionsAsync(0, 2, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleRequisitions);

                var result = await requisitionService.GetRequisitionsAsync(0, 2, new Dtos.Requisitions());

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Item1.Count() == 2);
                Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
            }

            #endregion

            #region DELETE

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionService_DeleteRequisitionAsync_ArgumentNullException_When_Guid_Null()
            {
                await requisitionService.DeleteRequisitionAsync(null);
            }

         
            [TestMethod]
            public async Task RequisitionService_DeleteRequisitionAsync()
            {
                requisitionRepositoryMock.Setup(r => r.DeleteRequisitionAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await requisitionService.DeleteRequisitionAsync(guid);
            }

            #endregion
        }

        [TestClass]
        public class RequisitionServiceTests_POST_AND_PUT : GeneralLedgerCurrentUser
        {
            #region DECLARATION

            protected Domain.Entities.Role getRequisitions = new Domain.Entities.Role(1, "VIEW.REQUISITIONS");
            protected Domain.Entities.Role updateRequisitions = new Domain.Entities.Role(1, "UPDATE.REQUISITIONS");

            private Mock<IRequisitionRepository> requisitionRepositoryMock;
            private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
            private Mock<IGeneralLedgerUserRepository> generalLedgerUserRepositoryMock;
            private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IBuyerRepository> buyerRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IVendorsRepository> vendorsRepositoryMock;
            private Mock<IAddressRepository> addressRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<IStaffRepository> staffRepositoryMock;
            Mock<IGeneralLedgerAccountRepository> generalLedgerAccountRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountFundsAvailableRepository> accountFundsAvailableRepo;
            private UserFactorySubset currentUserFactory;
            private Mock<IProcurementsUtilityService> procurementsUtilityServiceMock;

            private RequisitionService requisitionService;

            private Requisition requisition;
            private List<AccountsPayableSources> accountsPayableSources;
            private List<ShipToDestination> shipToDestinations;
            private List<FreeOnBoardType> freeOnBoardTypes;
            private Dtos.Requisitions dtoRequistion;
            private Dtos.Requisitions dtoPostRequistion;
            private List<VendorTerm> vendorTerms;
            private List<FundsAvailable> fundsAvailable;
            private List<CommodityCode> commoditycodes;
            private List<FxaTransferFlags> fxaFlagss;
            private List<CommodityUnitType> unitTypes;
            private List<Domain.Base.Entities.CommerceTaxCode> taxcodes;
            private IEnumerable<Domain.Base.Entities.Country> countries;
            private IEnumerable<Domain.Base.Entities.State> states;
            private LineItem lineItem;
            private LineItemGlDistribution lineItemGlDistribution;
            private LineItemTax lineItemTax;
            private Domain.Base.Entities.Address addressEntity;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                requisitionRepositoryMock = new Mock<IRequisitionRepository>();
                generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
                generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
                colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                buyerRepositoryMock = new Mock<IBuyerRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                vendorsRepositoryMock = new Mock<IVendorsRepository>();
                addressRepositoryMock = new Mock<IAddressRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                accountFundsAvailableRepo = new Mock<IAccountFundsAvailableRepository>();
                staffRepositoryMock = new Mock<IStaffRepository>();
                generalLedgerAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
                procurementsUtilityServiceMock = new Mock<IProcurementsUtilityService>();
                currentUserFactory = new GeneralLedgerCurrentUser.UserFactorySubset();

                requisitionService = new RequisitionService(requisitionRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object, generalLedgerUserRepositoryMock.Object,
                    colleagueFinanceReferenceDataRepositoryMock.Object, accountFundsAvailableRepo.Object, referenceDataRepositoryMock.Object, buyerRepositoryMock.Object, personRepositoryMock.Object, vendorsRepositoryMock.Object,
                    addressRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerMock.Object);

                InitializeTestData();

                InitializeMock();
            }

            [TestCleanup]
            public void Cleanup()
            {
                requisitionRepositoryMock = null;
                generalLedgerConfigurationRepositoryMock = null;
                generalLedgerUserRepositoryMock = null;
                colleagueFinanceReferenceDataRepositoryMock = null;
                buyerRepositoryMock = null;
                referenceDataRepositoryMock = null;
                vendorsRepositoryMock = null;
                personRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;

                requisitionService = null;
            }

            private void InitializeTestData()
            {
                addressEntity = new Domain.Base.Entities.Address()
                {
                    Guid = guid,
                    AddressLines = new List<string>() { "12245 Spring Hill Blvd." },
                    City = "Denver",
                    State = "SA",
                    PostalCode = "88495"
                };

                countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country("USA", "America", "US", "USA"),
                    new Domain.Base.Entities.Country("CAN", "Canada", "CA", "CAN"),
                    new Domain.Base.Entities.Country("AUS", "Austria", "AU", "AUS"),
                    new Domain.Base.Entities.Country("BRA", "Brazil", "BR", "BRA"),
                    new Domain.Base.Entities.Country("MEX", "Mexico", "MX", "MEX"),
                    new Domain.Base.Entities.Country("NLD", "Netherland", "ND", "NLD"),
                    new Domain.Base.Entities.Country("GBR", "London", "GB", "GBR"),
                    new Domain.Base.Entities.Country("ITL", "ITALY", "IT", "ILT"),
                    new Domain.Base.Entities.Country("JPN", "Japan", "JP", "JPN")
                };

                states = new List<Domain.Base.Entities.State>()
                {
                    new Domain.Base.Entities.State("VA", "description", "GBR"),
                    new Domain.Base.Entities.State("AV", "description", "IND"),
                    new Domain.Base.Entities.State("SA", "sa", "USA")
                };

                accountsPayableSources = new List<AccountsPayableSources>() { new AccountsPayableSources(guid, "1", "desc") };

                shipToDestinations = new List<ShipToDestination>() { new ShipToDestination(guid, "1", "desc") };

                freeOnBoardTypes = new List<FreeOnBoardType>() { new FreeOnBoardType(guid, "1", "desc") };

                vendorTerms = new List<VendorTerm>() { new VendorTerm(guid, "1", "desc") };

                commoditycodes = new List<CommodityCode>() { new CommodityCode(guid, "1", "desc") };

                fxaFlagss = new List<FxaTransferFlags>() { new FxaTransferFlags(guid, "1", "desc") };

                unitTypes = new List<CommodityUnitType>() { new CommodityUnitType(guid, "1", "desc") };

                taxcodes = new List<Domain.Base.Entities.CommerceTaxCode>() { new Domain.Base.Entities.CommerceTaxCode(guid, "1", "desc") };

                requisition = getRequisition();

                lineItem = new LineItem("1", "description", 1, 100, 10)
                {
                    Comments = "comments",
                    CommodityCode = "1",
                    FixedAssetsFlag = "1",
                    DesiredDate = DateTime.Now,
                    ExpectedDeliveryDate = DateTime.Now.AddDays(10),
                    InvoiceNumber = "1",
                    LineItemStatus = LineItemStatus.Accepted,
                    StatusDate = DateTime.Now,
                    TaxForm = "1",
                    TaxFormCode = "1",
                    TaxFormLocation = "1",
                    TradeDiscountAmount = 10,
                    TradeDiscountPercentage = 1,
                    UnitOfIssue = "1",
                    VendorPart = "1"
                };

                lineItemGlDistribution = new LineItemGlDistribution("11-00-02-67-60000-54005", 10, 100, 1)
                {
                    GlAccountDescription = "description",
                    Masked = true,
                    ProjectId = "1",
                    ProjectLineItemCode = "1",
                    ProjectLineItemId = "1",
                    ProjectNumber = "1"
                };

                lineItem.AddGlDistribution(lineItemGlDistribution);

                lineItemTax = new LineItemTax("1", 100) { LineGlNumber = "1", TaxAmount = 100, TaxGlNumber = "1" };

                lineItem.AddTax(lineItemTax);

                requisition.AddLineItem(lineItem);

                dtoRequistion = new Dtos.Requisitions()
                {
                    Id = guid,
                    SubmittedBy = new Dtos.GuidObject2(guid),
                    Type = Dtos.EnumProperties.RequisitionTypes.Eprocurement,
                    Buyer = new Dtos.GuidObject2(guid),
                    TransactionDate = DateTime.Today,
                    RequestedOn = DateTime.Today,
                    RequisitionNumber = "1",
                    PaymentSource = new Dtos.GuidObject2(guid),
                    PaymentTerms = new Dtos.GuidObject2(guid),
                    Shipping = new Dtos.DtoProperties.ShippingDtoProperty()
                    {
                        ShipTo = new Dtos.GuidObject2(guid),
                        FreeOnBoard = new Dtos.GuidObject2(guid)
                    },
                    OverrideShippingDestination = new Dtos.OverrideShippingDestinationDtoProperty()
                    {
                        Description = "desc",
                        AddressLines = new List<string>() { "address1" },
                        Place = new Dtos.AddressPlace()
                        {
                            Country = new Dtos.AddressCountry()
                            {
                                Code = Dtos.EnumProperties.IsoCode.USA,
                                Locality = "USA",
                                PostalCode = "12345",
                                Region = new Dtos.AddressRegion() { Code = "US-SA" }
                            }
                        }
                    },
                    Status = Dtos.EnumProperties.RequisitionsStatus.Outstanding,
                    Vendor = new Dtos.VendorDtoProperty()
                    {

                        ManualVendorDetails = new Dtos.ManualVendorDetailsDtoProperty()
                        {
                            Name = "name",
                            AddressLines = new List<string>() { "address1" },
                            Type = Dtos.EnumProperties.ManualVendorType.Organization,
                            Place = new Dtos.AddressPlace()
                            {
                                Country = new Dtos.AddressCountry()
                                {
                                    Code = Dtos.EnumProperties.IsoCode.USA,
                                    Locality = "USA",
                                    PostalCode = "12345",
                                    Region = new Dtos.AddressRegion() { Code = "US-SA" }
                                }
                            }
                        }
                    },
                    Initiator = new Dtos.DtoProperties.InitiatorDtoProperty()
                    {
                        Name = "name",
                        Detail = new Dtos.GuidObject2(guid)
                    },
                    Comments = new List<Dtos.CommentsDtoProperty>()
                    {
                        new Dtos.CommentsDtoProperty(){ Comment = "comment1", Type = Dtos.EnumProperties.CommentTypes.NotPrinted },
                        new Dtos.CommentsDtoProperty(){ Comment = "comment2", Type = Dtos.EnumProperties.CommentTypes.Printed }
                    },
                    LineItems = new List<Dtos.RequisitionsLineItemsDtoProperty>()
                    {
                        new Dtos.RequisitionsLineItemsDtoProperty()
                        {
                            LineItemNumber = "1",
                            Description = "desc",
                            Quantity = 10,
                            PartNumber = "1",
                            DesiredDate = DateTime.Today,
                            UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 1000, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD },
                            CommodityCode = new Dtos.GuidObject2(guid),
                            fixedAssetDesignation = new Dtos.GuidObject2(guid),
                            UnitOfMeasure = new Dtos.GuidObject2(guid),
                            Comments = new List<Dtos.CommentsDtoProperty>() { new Dtos.CommentsDtoProperty() { Type = Dtos.EnumProperties.CommentTypes.Printed, Comment = "comment"} },
                            TradeDiscount = new Dtos.TradeDiscountDtoProperty()
                            {
                                Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD },
                                Percent = 2
                            },
                            AccountDetail = new List<Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty>()
                            {
                                new Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty()
                                {
                                    Allocation = new Dtos.DtoProperties.RequisitionsAllocationDtoProperty()
                                    {
                                        Allocated = new Dtos.DtoProperties.RequisitionsAllocatedDtoProperty() {
                                            Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                                            {
                                                Value = 100,
                                                Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                                            },
                                            Percentage = 1,
                                            Quantity =10
                                        }
                                    },
                                    AccountingString = "11-00-02-67-60000-54005"
                                }
                            },
                            Taxes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(guid) }
                        }
                    }
                };
                dtoPostRequistion = new Dtos.Requisitions();
                dtoPostRequistion = dtoRequistion;
                dtoPostRequistion.Id = Guid.Empty.ToString();
                fundsAvailable = new List<FundsAvailable>()
                {
                    new FundsAvailable("1")
                    {
                        AvailableStatus = FundsAvailableStatus.Override,
                    }
                };
            }

            private Requisition getRequisition(RequisitionStatus status = RequisitionStatus.Outstanding)
            {
                return new Requisition("000123", guid, "1", "vendorName", status, DateTime.Now, DateTime.Now)
                {
                    CurrencyCode = "USD",
                    HostCountry = "CAN",
                    IntgSubmittedBy = guid,
                    MaintenanceDate = DateTime.Now,
                    DesiredDate = DateTime.Now,
                    Buyer = "1",
                    InitiatorName = "initiatorName",
                    DefaultInitiator = "1",
                    ShipToCode = "1",
                    Fob = "1",
                    AltShippingName = "AltShippingName",
                    AltShippingAddress = new List<string>() { "Address1" },
                    IntgAltShipCountry = "USA",
                    MiscCountry = "CAN",
                    AltShippingCity = "City",
                    AltShippingCountry = "USA",
                    AltShippingPhone = "1234",
                    AltShippingPhoneExt = "111",
                    AltShippingState = "VA",
                    AltShippingZip = "12345",
                    VendorId = "1",
                    VendorPreferredAddressId = "1",
                    MiscName = new List<string>() { "Name1" },
                    IntgCorpPerIndicator = "PERSON",
                    MiscAddress = new List<string>() { "Address1" },
                    VendorTerms = "1",
                    Comments = "comments",
                    InternalComments = "internal Comments"
                };
            }

            private async void  InitializeMock()
            {

                getRequisitions.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewRequisitions));
                updateRequisitions.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdateRequisitions));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getRequisitions, updateRequisitions });

                referenceDataRepositoryMock.Setup(r => r.GetStateCodesAsync(It.IsAny<bool>())).ReturnsAsync(states);
                referenceDataRepositoryMock.Setup(r => r.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);
                addressRepositoryMock.Setup(a => a.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                vendorsRepositoryMock.Setup(v => v.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                buyerRepositoryMock.Setup(b => b.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                buyerRepositoryMock.Setup(b => b.GetBuyerIdFromGuidAsync(It.IsAny<String>())).ReturnsAsync("1");
                requisitionRepositoryMock.Setup(r => r.CreateRequisitionAsync(It.IsAny<Requisition>())).ReturnsAsync(requisition);
                requisitionRepositoryMock.Setup(r => r.UpdateRequisitionAsync(It.IsAny<Requisition>())).ReturnsAsync(requisition);
                addressRepositoryMock.Setup(a => a.GetAddressFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                addressRepositoryMock.Setup(p => p.GetAddressAsync(It.IsAny<string>())).ReturnsAsync(addressEntity);

                requisitionRepositoryMock.Setup(r => r.GetRequisitionsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<String>())).ReturnsAsync("1");
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<String>())).ReturnsAsync(guid);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(accountsPayableSources);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetShipToDestinationsAsync(It.IsAny<bool>())).ReturnsAsync(shipToDestinations);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFreeOnBoardTypesAsync(It.IsAny<bool>())).ReturnsAsync(freeOnBoardTypes);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTerms);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commoditycodes);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFxaTransferFlagsAsync(It.IsAny<bool>())).ReturnsAsync(fxaFlagss);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(unitTypes);
                foreach (var record in accountsPayableSources)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetAccountsPayableSourceGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in shipToDestinations)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetShipToDestinationGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in freeOnBoardTypes)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFreeOnBoardTypeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in vendorTerms)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetVendorTermGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in commoditycodes)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityCodeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in fxaFlagss)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFxaTransferFlagGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                foreach (var record in unitTypes)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityUnitTypeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }
                referenceDataRepositoryMock.Setup(f => f.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxcodes);
                foreach (var record in taxcodes)
                {
                    referenceDataRepositoryMock.Setup(f => f.GetCommerceTaxCodeGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }
                accountFundsAvailableRepo.Setup(a => a.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(fundsAvailable);
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", guid);
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);


                var vendorGuidCollection = new Dictionary<string, string>();
                vendorGuidCollection.Add("1", guid);
                vendorsRepositoryMock.Setup(p => p.GetVendorGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);



            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionService_CreateRequisitionsAsync_Dto_Null()
            {
                await requisitionService.CreateRequisitionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionService_CreateRequisitionsAsync_Dto_Id_Null()
            {
                await requisitionService.CreateRequisitionsAsync(new Dtos.Requisitions() { Id = null });
            }

          
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_LineItems_With_Same_GLNumber()
            {
                dtoRequistion.LineItems.FirstOrDefault().AccountDetail.Add(new Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty()
                {
                    AccountingString = "1",
                });
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_LineItems_Funds_NotAvailable()
            {
                fundsAvailable.FirstOrDefault().AvailableStatus = FundsAvailableStatus.NotAvailable;
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_RequestedOn_As_MinDate()
            {
                dtoRequistion.RequestedOn = DateTime.MinValue;
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_Initiator_As_Null()
            {
                dtoRequistion.Initiator = null;
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_InitiatorName_As_Null()
            {
                dtoRequistion.Initiator.Name = null;
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_LineItems_As_Empty()
            {
                dtoRequistion.LineItems = new List<Dtos.RequisitionsLineItemsDtoProperty>() { };
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_SubmittedBy_Null()
            {
                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<String>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_Buyer_Null()
            {
                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<String>()))
                                    .Returns(Task.FromResult<string>("1"))
                                    .Returns(Task.FromResult<string>("1"))
                                    .Returns(Task.FromResult<string>(null));
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_Initiator_Null()
            {
                buyerRepositoryMock.SetupSequence(p => p.GetBuyerIdFromGuidAsync(It.IsAny<String>()))
                                    .Returns(Task.FromResult<string>(null))
                                    .Returns(Task.FromResult<string>("1"))
                                    .Returns(Task.FromResult<string>("1"))
                                    .Returns(Task.FromResult<string>("1"));
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_PaymentSource_Notfound()
            {
                dtoRequistion.PaymentSource = new Dtos.GuidObject2(Guid.NewGuid().ToString());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_ShipToDest_Null()
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetShipToDestinationsAsync(true)).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_ShipToDest_NotFound()
            {
                dtoRequistion.Shipping.ShipTo = new Dtos.GuidObject2(Guid.NewGuid().ToString());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_FreeOnBoard_Null()
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFreeOnBoardTypesAsync(true)).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_FreeOnBoard_NotFound()
            {
                dtoRequistion.Shipping.FreeOnBoard = new Dtos.GuidObject2(Guid.NewGuid().ToString());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_RepositoryException_For_Vendor()
            {
                dtoRequistion.Vendor = new Dtos.VendorDtoProperty()
                {
                    ExistingVendor = new Dtos.ExistingVendorDetailsDtoProperty()
                    {
                        Vendor = new Dtos.GuidObject2(guid),
                        AlternativeVendorAddress = new Dtos.GuidObject2(guid)
                    }
                };
                requisitionRepositoryMock.Setup(r => r.GetRequisitionsIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_Vendor_Notfound()
            {
                dtoRequistion.Vendor = new Dtos.VendorDtoProperty()
                {
                    ExistingVendor = new Dtos.ExistingVendorDetailsDtoProperty()
                    {
                        Vendor = new Dtos.GuidObject2(guid),
                        AlternativeVendorAddress = new Dtos.GuidObject2(guid)
                    }
                };

                requisitionRepositoryMock.Setup(r => r.GetRequisitionsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_RepositoryException_For_Address()
            {
                dtoRequistion.Vendor = new Dtos.VendorDtoProperty()
                {
                    ExistingVendor = new Dtos.ExistingVendorDetailsDtoProperty()
                    {
                        Vendor = new Dtos.GuidObject2(guid),
                        AlternativeVendorAddress = new Dtos.GuidObject2(guid)
                    }
                };

                addressRepositoryMock.Setup(a => a.GetAddressFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_VendorAddress_Notfound()
            {
                dtoRequistion.Vendor = new Dtos.VendorDtoProperty()
                {
                    ExistingVendor = new Dtos.ExistingVendorDetailsDtoProperty()
                    {
                        Vendor = new Dtos.GuidObject2(guid),
                        AlternativeVendorAddress = new Dtos.GuidObject2(guid)
                    }
                };

                addressRepositoryMock.Setup(r => r.GetAddressFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_ManualVendorDetails_Type_noName()
            {
                dtoRequistion.Vendor = new Dtos.VendorDtoProperty() { };
                dtoRequistion.Vendor.ManualVendorDetails = new Dtos.ManualVendorDetailsDtoProperty();
                dtoRequistion.Vendor.ManualVendorDetails.Name = "Jack";
                dtoRequistion.Vendor.ManualVendorDetails.Type = null;
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_ManualVendorDetails_Name_NoType()
            {
                dtoRequistion.Vendor = new Dtos.VendorDtoProperty() { };
                dtoRequistion.Vendor.ManualVendorDetails = new Dtos.ManualVendorDetailsDtoProperty();
                dtoRequistion.Vendor.ManualVendorDetails.Name = "";
                dtoRequistion.Vendor.ManualVendorDetails.Type = Dtos.EnumProperties.ManualVendorType.Organization;
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_VendorTerm_Notfound()
            {
                dtoRequistion.PaymentTerms = new Dtos.GuidObject2(Guid.NewGuid().ToString());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_CommodityCodes_Null()
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityCodesAsync(true)).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_FixedAssetDesignation_Null()
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetFxaTransferFlagsAsync(true)).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_CommodityUnitTypes_Null()
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetCommodityUnitTypesAsync(true)).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_CommodityCode_Notfound()
            {
                dtoRequistion.LineItems.FirstOrDefault().CommodityCode.Id = Guid.NewGuid().ToString();
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_FixedAssetDesignation_Notfound()
            {
                dtoRequistion.LineItems.FirstOrDefault().fixedAssetDesignation.Id = Guid.NewGuid().ToString();
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_CommodityUnitType_Notfound()
            {
                dtoRequistion.LineItems.FirstOrDefault().UnitOfMeasure.Id = Guid.NewGuid().ToString();
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_CreateRequisitionsAsync_DtoToEntity_LineItem_TaxCode_Notfound()
            {
                dtoRequistion.LineItems.FirstOrDefault().Taxes.FirstOrDefault().Id = Guid.NewGuid().ToString();
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task RequisitionService_CreateRequisitionsAsync_RepositoryException()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.CreateRequisitionAsync(It.IsAny<Requisition>())).ThrowsAsync(new RepositoryException());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_CreateRequisitionsAsync_EntityToDto_Entity_Null()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.CreateRequisitionAsync(It.IsAny<Requisition>())).ReturnsAsync(() => null);
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task RequisitionService_CreateRequisitionsAsync_EntityToDto_AccountPayableSources_Notfound()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisition.ApType = "2";
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetAccountsPayableSourceGuidAsync(requisition.ApType)).ThrowsAsync(new RepositoryException());
                await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);
            }

            [TestMethod]
            public async Task RequisitionService_CreateRequisitionsAsync()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                var result = await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            public async Task RequisitionService_CreateRequisitionsAsync_SubmittedByEmpty()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                var result = await requisitionService.CreateRequisitionsAsync(dtoPostRequistion);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionService_UpdateRequisitionsAsync_Dto_Null()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                var result = await requisitionService.UpdateRequisitionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionService_UpdateRequisitionsAsync_DtoId_Null()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                var result = await requisitionService.UpdateRequisitionsAsync(new Dtos.Requisitions() { Id = null });
            }

            [TestMethod]
            public async Task RequisitionService_UpdateRequisitionsAsync_Create_Requisition()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.SetupSequence(r => r.GetRequisitionsIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null)).Returns(Task.FromResult<string>("1"));
                var result = await requisitionService.UpdateRequisitionsAsync(dtoRequistion);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            public async Task RequisitionService_UpdateRequisitionsAsync_Create_Requisition_SubmittedByEmpty()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.SetupSequence(r => r.GetRequisitionsIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null)).Returns(Task.FromResult<string>("1"));
                var result = await requisitionService.UpdateRequisitionsAsync(dtoRequistion);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task RequisitionService_UpdateRequisitionsAsync_RepositoryException()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.UpdateRequisitionAsync(It.IsAny<Requisition>())).ThrowsAsync(new RepositoryException());
                await requisitionService.UpdateRequisitionsAsync(dtoRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RequisitionService_UpdateRequisitionsAsync_KeyNotFoundException()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.UpdateRequisitionAsync(It.IsAny<Requisition>())).ThrowsAsync(new KeyNotFoundException());
                await requisitionService.UpdateRequisitionsAsync(dtoRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task RequisitionService_UpdateRequisitionsAsync_ArgumentException()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.UpdateRequisitionAsync(It.IsAny<Requisition>())).ThrowsAsync(new ArgumentException());
                await requisitionService.UpdateRequisitionsAsync(dtoRequistion);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task RequisitionService_UpdateRequisitionsAsync_Exception()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                requisitionRepositoryMock.Setup(r => r.UpdateRequisitionAsync(It.IsAny<Requisition>())).ThrowsAsync(new Exception());
                await requisitionService.UpdateRequisitionsAsync(dtoRequistion);
            }

            [TestMethod]
            public async Task RequisitionService_UpdateRequisitionsAsync()
            {
                var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
                generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
                requisition.IntgSubmittedBy = null;
                var result = await requisitionService.UpdateRequisitionsAsync(dtoRequistion);

                Assert.IsNotNull(result);
            }
        }
    }
}
