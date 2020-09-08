// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    /// <summary>
    /// Test that the service returns a valid purchase order.
    /// We use GeneralLedgerCurrentUser to mimic the user logged in.
    /// </summary>
    [TestClass]
    public class PurchaseOrderServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup

        private PurchaseOrderService service;
        private PurchaseOrderService service2;
        private PurchaseOrderService serviceForNoPermission;
        private PurchaseOrderCreateUpdateResponse responsePurchaseOrder;
        private List<LineItem> poLineItems;

        private TestPurchaseOrderRepository testPurchaseOrderRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;


        private Mock<IPurchaseOrderRepository> mockPurchaseOrderRepository;
        private Mock<IGeneralLedgerConfigurationRepository> mockGlConfigurationRepository;
        private Mock<IGeneralLedgerUserRepository> mockGeneralLedgerUserRepository;

        private Mock<IStaffRepository> staffRepositoryMock;
        private IStaffRepository staffRepository;

        private Mock<IPersonRepository> mockPersonRepository;
        private Mock<IAccountFundsAvailableRepository> mockAccountFundAvailableRepo;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
        private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
        private Mock<IGeneralLedgerAccountRepository> generalLedgerAccountRepositoryMock;
        private Mock<IProcurementsUtilityService> procurementsUtilityServiceMock;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewPurchaseOrder;
        private Domain.Entities.Permission permissionCreateUpdatePurchaseOrder;
        private Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(226, "PURCHASE.ORDER.VIEWER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionUser = new GeneralLedgerCurrentUser.UserFactoryNone();

        [TestInitialize]
        public void Initialize()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();

            // Set up the mock PO repository
            mockPurchaseOrderRepository = new Mock<IPurchaseOrderRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            staffRepositoryMock = new Mock<IStaffRepository>();
            generalLedgerAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
            procurementsUtilityServiceMock = new Mock<IProcurementsUtilityService>();
            // Create permission domain entities for viewing the purchase order.
            permissionCreateUpdatePurchaseOrder = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionCreateUpdatePurchaseOrder);
            // Create permission domain entities for viewing the purchase order.
            permissionViewPurchaseOrder = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewPurchaseOrder);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewPurchaseOrder);
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("1", "Test LastName")));
            Dictionary<string, string> descDictionary = new Dictionary<string, string>();
            generalLedgerAccountRepositoryMock.Setup(x => x.GetGlAccountDescriptionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<GeneralLedgerAccountStructure>())).Returns(() =>
            {
                return Task.FromResult(descDictionary);
            });
            //TODO =>procurementsUtilityServiceMock
            // build all service objects to use in testing
            BuildValidPurchaseOrderService();

            responsePurchaseOrder = new PurchaseOrderCreateUpdateResponse()
            {

                PurchaseOrderId = "1",
                PurchaseOrderNumber = "P0001111",
                PurchaseOrderDate = new DateTime(2020, 01, 08),
                ErrorOccured = false,
                ErrorMessages = null,
                WarningOccured = false,
                WarningMessages = null
            };


            poLineItems = new List<LineItem>()
                {
                    new LineItem("1", "desc", 10, 100, 110)
                    {
                       Comments = "Comments",
                       CommodityCode = "1",
                       VendorPart = "1",
                       UnitOfIssue = "1",
                       TradeDiscountAmount = 100,
                       StatusDate = DateTime.Today,
                       LineItemStatus = LineItemStatus.Outstanding
                    },
                    new LineItem("2", "desc", 10, 100, 110)
                    {
                       Comments = "Comments",
                       CommodityCode = "1",
                       VendorPart = "1",
                       UnitOfIssue = "1",
                       TradeDiscountPercentage = 10
                    }
                };
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables.
            service = null;
            service2 = null;
            serviceForNoPermission = null;

            testPurchaseOrderRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;
            mockPurchaseOrderRepository = null;
            mockGlConfigurationRepository = null;
            mockGeneralLedgerUserRepository = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleViewPermissions = null;
        }
        #endregion

        #region Tests for GetPurchaseOrderAsync with a view permission
        [TestMethod]
        public async Task GetPurchaseOrderAsync()
        {
            var purchaseOrderId = "1";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entity from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(purchaseOrderDto.Id, purchaseOrderDomainEntity.Id);
            Assert.AreEqual(purchaseOrderDto.Number, purchaseOrderDomainEntity.Number);
            Assert.AreEqual(purchaseOrderDto.Amount, purchaseOrderDomainEntity.Amount);
            Assert.AreEqual(purchaseOrderDto.ApType, purchaseOrderDomainEntity.ApType);
            Assert.AreEqual(purchaseOrderDto.Comments, purchaseOrderDomainEntity.Comments);
            Assert.AreEqual(purchaseOrderDto.CurrencyCode, purchaseOrderDomainEntity.CurrencyCode);
            Assert.AreEqual(purchaseOrderDto.Date, purchaseOrderDomainEntity.Date);
            Assert.AreEqual(purchaseOrderDto.DeliveryDate, purchaseOrderDomainEntity.DeliveryDate);
            Assert.AreEqual(purchaseOrderDto.InitiatorName, purchaseOrderDomainEntity.InitiatorName);
            Assert.AreEqual(purchaseOrderDto.InternalComments, purchaseOrderDomainEntity.InternalComments);
            Assert.AreEqual(purchaseOrderDto.MaintenanceDate, purchaseOrderDomainEntity.MaintenanceDate);
            Assert.AreEqual(purchaseOrderDto.RequestorName, purchaseOrderDomainEntity.RequestorName);
            Assert.AreEqual(purchaseOrderDto.ShipToCodeName, purchaseOrderDomainEntity.ShipToCodeName);
            Assert.AreEqual(purchaseOrderDto.Status.ToString(), purchaseOrderDomainEntity.Status.ToString());
            Assert.AreEqual(purchaseOrderDto.StatusDate, purchaseOrderDomainEntity.StatusDate);
            Assert.AreEqual(purchaseOrderDto.VendorId, purchaseOrderDomainEntity.VendorId);
            Assert.AreEqual(purchaseOrderDto.VendorName, purchaseOrderDomainEntity.VendorName);

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < purchaseOrderDto.Approvers.Count(); i++)
            {
                var approverDto = purchaseOrderDto.Approvers[i];
                var approverDomain = purchaseOrderDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the list of requisition DTOs matches the domain entity
            for (int i = 0; i < purchaseOrderDto.Requisitions.Count(); i++)
            {
                Assert.AreEqual(purchaseOrderDto.Requisitions[i], purchaseOrderDomainEntity.Requisitions[i]);
            }

            // Confirm that the data in the list of voucher DTOs matches the domain entity
            for (int i = 0; i < purchaseOrderDto.Vouchers.Count(); i++)
            {
                Assert.AreEqual(purchaseOrderDto.Vouchers[i], purchaseOrderDomainEntity.Vouchers[i]);
            }

            // Confirm that the data in the line item DTOs matches the domain entity
            for (int i = 0; i < purchaseOrderDto.LineItems.Count(); i++)
            {
                var lineItemDto = purchaseOrderDto.LineItems[i];
                var lineItemDomain = purchaseOrderDomainEntity.LineItems[i];
                Assert.AreEqual(lineItemDto.Comments, lineItemDomain.Comments);
                Assert.AreEqual(lineItemDto.Description, lineItemDomain.Description);
                Assert.AreEqual(lineItemDto.ExpectedDeliveryDate, lineItemDomain.ExpectedDeliveryDate);
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
        public async Task GetPurchaseOrderAsync_NoAccessToOneGlAccount()
        {
            var purchaseOrderId = "999";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entity from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(purchaseOrderDto.Id, purchaseOrderDomainEntity.Id);

            // There should only be one line item.
            Assert.AreEqual(1, purchaseOrderDomainEntity.LineItems.Count);
            Assert.AreEqual(1, purchaseOrderDto.LineItems.Count);

            // Confirm that the DTO line item data matches the data in the domain entity.
            var lineItemDto = purchaseOrderDto.LineItems.First();
            var lineItemEntity = purchaseOrderDomainEntity.LineItems.First();

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
        public async Task GetPurchaseOrderAsync_PoHasRequisitions()
        {
            var purchaseOrderId = "3";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            // Confirm that the data in the list of requisition DTOs matches the domain entity
            for (int i = 0; i < purchaseOrderDto.Requisitions.Count(); i++)
            {
                Assert.AreEqual(purchaseOrderDto.Requisitions[i], purchaseOrderDomainEntity.Requisitions[i]);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusAccepted()
        {
            var purchaseOrderId = "7";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusBackordered()
        {
            var purchaseOrderId = "6";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusClosed()
        {
            var purchaseOrderId = "10";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusInProgress()
        {
            var purchaseOrderId = "4";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusNotApproved()
        {
            var purchaseOrderId = "2";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusOutstanding()
        {
            var purchaseOrderId = "3";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusPaid()
        {
            var purchaseOrderId = "11";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusReconciled()
        {
            var purchaseOrderId = "9";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_StatusVoided()
        {
            var purchaseOrderId = "12";
            var personId = "1";
            var purchaseOrderDto = await service.GetPurchaseOrderAsync(purchaseOrderId);

            // Get the purchase order domain entiy from the test repository
            var purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            Assert.AreEqual(purchaseOrderDomainEntity.Status.ToString(), purchaseOrderDto.Status.ToString(), "Status must be the same.");
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_NullId()
        {
            var expectedParamName = "purchaseOrderDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetPurchaseOrderAsync(null);
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetPurchaseOrderAsync_EmptyId()
        {
            var expectedParamName = "purchaseOrderDomainEntity";
            var actualParamName = "";
            try
            {
                var journalEntryDto = await service.GetPurchaseOrderAsync("");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrderAsync_NullAccountStructure()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.GetPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrderAsync_NullGlClassConfiguration()
        {
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            GeneralLedgerAccountStructure GlAccountStructure = new GeneralLedgerAccountStructure() { GlAccountLength = "9" };
            //GeneralLedgerClassConfiguration glClassConfiguration = new GeneralLedgerClassConfiguration("glClassName", new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" });
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(GlAccountStructure);
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            mockGeneralLedgerUserRepository.Setup(glUser => glUser.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(null);

            await service2.GetPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrderAsync_NullGeneralLedgerUser()
        {
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerAccountStructure GlAccountStructure = new GeneralLedgerAccountStructure() { GlAccountLength = "9" };
            GeneralLedgerClassConfiguration glClassConfiguration = new GeneralLedgerClassConfiguration("glClassName", new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" });
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(GlAccountStructure);
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            mockGeneralLedgerUserRepository.Setup(glUser => glUser.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(null);
            await service2.GetPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrderAsync_NoLineItemsReturns()
        {
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerAccountStructure GlAccountStructure = new GeneralLedgerAccountStructure() { GlAccountLength = "9", FullAccessRole = "0" };
            GeneralLedgerClassConfiguration glClassConfiguration = new GeneralLedgerClassConfiguration("glClassName", new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" });
            GeneralLedgerUser user = new GeneralLedgerUser("000001", "Steve");
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(GlAccountStructure);
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            mockGeneralLedgerUserRepository.Setup(glUser => glUser.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).Returns(Task.FromResult(user));
            PurchaseOrder PurchaseOrder = new PurchaseOrder("0000001", "P000001", "ABC Comapny", PurchaseOrderStatus.Outstanding, new DateTime(), new DateTime()) {};
            mockPurchaseOrderRepository.Setup(r => r.GetPurchaseOrderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>())).Returns(Task.FromResult(PurchaseOrder));

            await service2.GetPurchaseOrderAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrderAsync_RepositoryReturnsNullObject()
        {
            // Mock the GetRequisition repository method to return a null object within the Service method
            PurchaseOrder nullPurchaseOrder = null;
            this.mockPurchaseOrderRepository.Setup<Task<PurchaseOrder>>(poRepo => poRepo.GetPurchaseOrderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>())).Returns(Task.FromResult(nullPurchaseOrder));
            var purchaseOrderDto = await service2.GetPurchaseOrderAsync("1");
        }
        #endregion

        #region Tests for GetPurchaseOrderAsync without a view permission

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetPurchaseOrderAsync_PermissionException()
        {
            await serviceForNoPermission.GetPurchaseOrderAsync("1");
        }
        #endregion

        #region Build service method

        /// <summary>
        /// Builds multiple purchase order service objects.
        /// </summary>
        private void BuildValidPurchaseOrderService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository = roleRepositoryMock.Object;

            var loggerObject = new Mock<ILogger>().Object;

            testPurchaseOrderRepository = new TestPurchaseOrderRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            var colleagueFinanceReferenceDataRepository = new TestColleagueFinanceReferenceDataRepository();
            var referenceDataRepo = new Mock<IReferenceDataRepository>();
            var buyersDataRepo = new Mock<IBuyerRepository>();
            var vendorsDataRepo = new Mock<IVendorsRepository>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
            mockAccountFundAvailableRepo = new Mock<IAccountFundsAvailableRepository>();
            mockPersonRepository = new Mock<IPersonRepository>();

            generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var purchaseOrderDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.PurchaseOrder, Dtos.ColleagueFinance.PurchaseOrder>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.PurchaseOrder, Dtos.ColleagueFinance.PurchaseOrder>()).Returns(purchaseOrderDtoAdapter);
            var purchaseOrderCreateUpdateDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.PurchaseOrderCreateUpdateResponse, Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.PurchaseOrderCreateUpdateResponse, Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse>()).Returns(purchaseOrderCreateUpdateDtoAdapter);
            var purchaseOrderVoidResponseAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.PurchaseOrderVoidResponse, Dtos.ColleagueFinance.PurchaseOrderVoidResponse>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.PurchaseOrderVoidResponse, Dtos.ColleagueFinance.PurchaseOrderVoidResponse>()).Returns(purchaseOrderVoidResponseAdapter);
            // Set up the services
            service = new PurchaseOrderService(testPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, referenceDataRepo.Object, buyersDataRepo.Object, vendorsDataRepo.Object, baseConfigurationRepository, adapterRegistry.Object, currentUserFactory, mockAccountFundAvailableRepo.Object, mockPersonRepository.Object, roleRepository, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerObject);
            service2 = new PurchaseOrderService(mockPurchaseOrderRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object,
                colleagueFinanceReferenceDataRepository, referenceDataRepo.Object, buyersDataRepo.Object, vendorsDataRepo.Object, baseConfigurationRepository, adapterRegistry.Object, currentUserFactory, mockAccountFundAvailableRepo.Object, mockPersonRepository.Object, roleRepository, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerObject);
            // Build a service for a user that has no permissions.
            serviceForNoPermission = new PurchaseOrderService(testPurchaseOrderRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository, referenceDataRepo.Object, buyersDataRepo.Object, vendorsDataRepo.Object, baseConfigurationRepository, adapterRegistry.Object, noPermissionUser, mockAccountFundAvailableRepo.Object, mockPersonRepository.Object, roleRepository, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerObject);
        }
        #endregion

        #region Tests for GetPurchaseOrderSummaryByPersonIdAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_EmptyPersonId_ArgumentNullException()
        {
            await service2.GetPurchaseOrderSummaryByPersonIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_CurrentUserDifferentFromRequest()
        {
            await service2.GetPurchaseOrderSummaryByPersonIdAsync("0016357");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_MissingPermissionException()
        {
            await serviceForNoPermission.GetPurchaseOrderSummaryByPersonIdAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_StaffRecordMissingException()
        {
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            await service2.GetPurchaseOrderSummaryByPersonIdAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_SummaryDomainEntityNull()
        {
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(null);
            var result = await service2.GetPurchaseOrderSummaryByPersonIdAsync(currentUserFactory.CurrentUser.PersonId);
            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_Success()
        {
            List<PurchaseOrderSummary> summary = new List<PurchaseOrderSummary>() { new PurchaseOrderSummary("0000001", "P000001", "ABC Company", new DateTime()) };

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(summary);
            var result = await service2.GetPurchaseOrderSummaryByPersonIdAsync(currentUserFactory.CurrentUser.PersonId);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count()>0);
        }

        #endregion

        #region "CreateUpdatePurchaseOrderAsync"

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdatePurchaseOrderAsync_NullpurchaseOrderCreateUpdateRequest_ArgumentNullException()
        {
            await service2.CreateUpdatePurchaseOrderAsync(null);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdatePurchaseOrderAsync_EmptyPersonId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            abc.PersonId = "";
            await service2.CreateUpdatePurchaseOrderAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdatePurchaseOrderAsync_glConfiguration_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.InitiatorInitials = "AB";
            request.IsPersonVendor = false;
            request.PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
            {
                Id = "",
                Number = "",
                Status = 0,
                StatusDate = new DateTime(2020, 01, 01),
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                InitiatorName = "ABC",
                RequestorName = "ABC",
                ApType = "AP",
                ShipToCode = "DT",
                CommodityCode = "",
                Comments = "This is Purchase Order creation",
                InternalComments = "This is Purchase Order creation"
            };

            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.CreateUpdatePurchaseOrderAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdatePurchaseOrderAsync_glClassConfiguration_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.InitiatorInitials = "AB";
            request.IsPersonVendor = false;
            request.PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
            {
                Id = "",
                Number = "",
                Status = 0,
                StatusDate = new DateTime(2020, 01, 01),
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                InitiatorName = "ABC",
                RequestorName = "ABC",
                ApType = "AP",
                ShipToCode = "DT",
                CommodityCode = "",
                Comments = "This is Purchase Order creation",
                InternalComments = "This is Purchase Order creation"
            };

            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            GeneralLedgerAccountStructure GlAccountStructure = new GeneralLedgerAccountStructure() { GlAccountLength = "9" };
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(GlAccountStructure);
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            await service2.CreateUpdatePurchaseOrderAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdatePurchaseOrderAsync_glUser_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.InitiatorInitials = "AB";
            request.IsPersonVendor = false;
            request.PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
            {
                Id = "P00001",
                Number = "",
                Status = 0,
                StatusDate = new DateTime(2020, 01, 01),
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                InitiatorName = "ABC",
                RequestorName = "ABC",
                ApType = "AP",
                ShipToCode = "DT",
                CommodityCode = "",
                Comments = "This is Purchase Order creation",
                InternalComments = "This is Purchase Order creation"
            };

            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure GlAccountStructure = new GeneralLedgerAccountStructure() { GlAccountLength = "9" };
            GeneralLedgerClassConfiguration glClassConfiguration = new GeneralLedgerClassConfiguration("glClassName", new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" }, new List<string>() { "12" });
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(GlAccountStructure);
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            //generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
            mockGeneralLedgerUserRepository.Setup(glUser => glUser.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(null);
            await service2.CreateUpdatePurchaseOrderAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdatePurchaseOrderAsync_EmptyPurchaseOrderDto_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            abc.PersonId = "1";
            abc.PurchaseOrder = null;
            await service2.CreateUpdatePurchaseOrderAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateUpdatePurchaseOrderAsync_StaffRecordMissingException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "1";
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrder po = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrder();
            po.Id = "";
            po.Number = "";
            po.InitiatorName = "ABC";
            po.RequestorName = "ABC";
            request.PurchaseOrder = po;
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            await service2.CreateUpdatePurchaseOrderAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateUpdatePurchaseOrderAsync_CreatePO_WithoutPermission()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.InitiatorInitials = "ABC";
            request.IsPersonVendor = false;
            request.PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
            {
                Id = "",
                Number = "",
                Status = 0,
                StatusDate = new DateTime(2020, 01, 01),
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                InitiatorName = "Ashutosh Sinha",
                RequestorName = "Ashutosh Sinha",
                ApType = "AP",
                ShipToCode = "DT",
                CommodityCode = "",
                Comments = "This is Purchase Order creation",
                InternalComments = "This is Purchase Order creation"
            };

            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            await serviceForNoPermission.CreateUpdatePurchaseOrderAsync(request);
        }

        [TestMethod]
        public async Task CreateUpdatePurchaseOrderAsync_CreatePO_WithPermission()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.InitiatorInitials = "AB";
            request.IsPersonVendor = false;
            request.PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
            {
                Id = "",
                Number = "",
                Status = 0,
                StatusDate = new DateTime(2020, 01, 01),
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                InitiatorName = "ABC",
                RequestorName = "ABC",
                ApType = "AP",
                ShipToCode = "DT",
                CommodityCode = "",
                Comments = "This is Purchase Order creation",
                InternalComments = "This is Purchase Order creation"
            };

            PurchaseOrderCreateUpdateRequest requestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "ANA",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };

            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            mockPurchaseOrderRepository.Setup(r => r.CreatePurchaseOrderAsync(requestEntity)).ReturnsAsync(responsePurchaseOrder);
            var result = await service.CreateUpdatePurchaseOrderAsync(request);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task CreateUpdatePurchaseOrderAsync_UpdatePO_WithGeneralLedgerUserNoAccess()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.InitiatorInitials = "AB";
            request.IsPersonVendor = false;
            request.PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
            {
                Id = "1117",
                Number = "P0001075",
                Status = Dtos.ColleagueFinance.PurchaseOrderStatus.InProgress,
                StatusDate = new DateTime(2020, 01, 01),
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                InitiatorName = "ABC",
                RequestorName = "ABC",
                ApType = "AP",
                ShipToCode = "DT",
                CommodityCode = "",
                Comments = "This is Purchase Order creation",
                InternalComments = "This is Purchase Order creation"
            };

            PurchaseOrderCreateUpdateRequest requestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "AB",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };

            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            var result = await service.CreateUpdatePurchaseOrderAsync(request);
        }

        [TestMethod]
        public async Task CreateUpdatePurchaseOrderAsync_UpdatePO_WithGeneralLedgerUserAllAccess()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.InitiatorInitials = "AB";
            request.IsPersonVendor = false;
            request.PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
            {
                Id = "1",
                Number = "P0001111",
                Status = Dtos.ColleagueFinance.PurchaseOrderStatus.InProgress,
                StatusDate = new DateTime(2020, 01, 01),
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                InitiatorName = "ABC",
                RequestorName = "ABC",
                ApType = "AP",
                ShipToCode = "DT",
                CommodityCode = "",
                Comments = "This is Purchase Order creation",
                InternalComments = "This is Purchase Order creation"
            };

            PurchaseOrderCreateUpdateRequest requestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "AB",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };
            //PurchaseOrder poO = new PurchaseOrder();
            PurchaseOrder originalPo = new PurchaseOrder("1", "P0001111", "Ellucian Consulting, Inc.", PurchaseOrderStatus.InProgress, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            {
                DeliveryDate = new DateTime(2020, 05, 01),
                Amount = 117,
                CurrencyCode = "",
                MaintenanceDate = null,
                VendorId = "0009876",
                InitiatorName = "ABC",
                RequestorName = "ABC",
                ApType = "AP",
                ShipToCode = "MC",
                ShipToCodeName = "MC Datatel - Main Campus",
                DefaultCommodityCode = "",
                Comments = "It is a PO for Pen",
                InternalComments = "Pen is ordered",
                CommodityCode = null,

            };
           
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerUser glUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync("0000001", "Full_Access", null, null);
            PurchaseOrder Po = await testPurchaseOrderRepository.GetPurchaseOrderAsync("1", "0000001", GlAccessLevel.Full_Access, null);
            mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(Po));

            mockPurchaseOrderRepository.Setup(r => r.UpdatePurchaseOrderAsync(requestEntity, originalPo)).ReturnsAsync(responsePurchaseOrder);
            var result = await service.CreateUpdatePurchaseOrderAsync(request);
            Assert.IsNotNull(result);
        }

        #endregion

        #region "VoidPurchaseOrderAsync"

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidPurchaseOrderAsync_NullpurchaseOrderVoidRequest_ArgumentNullException()
        {
            await service2.VoidPurchaseOrderAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidPurchaseOrderAsync_EmptyPersonId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest();
            abc.PersonId = "";
            await service2.VoidPurchaseOrderAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidPurchaseOrderAsync_EmptyPurchaseOrderId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest();
            abc.PersonId = "0000004";
            await service2.VoidPurchaseOrderAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidPurchaseOrderAsync_EmptyEmail_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest();
            abc.PersonId = "000123";
            abc.PurchaseOrderId = "000123";
            abc.ConfirmationEmailAddresses = "";
            await service2.VoidPurchaseOrderAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VoidPurchaseOrderAsync_NotLoggedInuser()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest();
            abc.PersonId = "000123";
            abc.PurchaseOrderId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.VoidPurchaseOrderAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VoidPurchaseOrderAsync_StaffRecordMissingException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest();
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            abc.PersonId = "0000004";
            abc.PurchaseOrderId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.VoidPurchaseOrderAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VoidPurchaseOrderAsync_PermissionsException()
        {
            List<Domain.Entities.Role> roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"UPDATE.PO")
                    };

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(roles);

            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest();
            
            abc.PersonId = "0000004";
            abc.PurchaseOrderId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.VoidPurchaseOrderAsync(abc);
        }

        [TestMethod]
        public async Task VoidPurchaseOrderAsync_Success()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest();

            abc.PersonId = "0000001";
            abc.PurchaseOrderId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";

            PurchaseOrderVoidResponse responseVoidPo = new PurchaseOrderVoidResponse()
            {
                ErrorMessages = new List<string>(),
                ErrorOccured = false,
                PurchaseOrderId = "0000001",
                PurchaseOrderNumber = "P000123",
                WarningMessages = new List<string>(),
                WarningOccured = false
            };

            mockPurchaseOrderRepository.Setup(r => r.VoidPurchaseOrderAsync(It.IsAny<PurchaseOrderVoidRequest>())).ReturnsAsync(responseVoidPo);

            var voidPODtos = await service2.VoidPurchaseOrderAsync(abc);

            Assert.IsNotNull(voidPODtos);
            Assert.AreEqual(responseVoidPo.PurchaseOrderId, voidPODtos.PurchaseOrderId);
            Assert.AreEqual(responseVoidPo.PurchaseOrderNumber, voidPODtos.PurchaseOrderNumber);
            Assert.AreEqual(responseVoidPo.ErrorOccured, voidPODtos.ErrorOccured);
            Assert.AreEqual(responseVoidPo.WarningOccured, voidPODtos.WarningOccured);
            Assert.AreEqual(0, voidPODtos.ErrorMessages.Count);
        }

            #endregion
        }

    #region EEDM Purchase order V11

    [TestClass]
    public class PurchaseOrderServiceTests_GET_v11 : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup

        private PurchaseOrderService service = null;
        private string guid = "b61b3a19-f164-47ad-afbc-dc5947340cdc";
        private TestPurchaseOrderRepository testPurchaseOrderRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;
        private PurchaseOrderUser currentUserFactory = new GeneralLedgerCurrentUser.PurchaseOrderUser();
        private Mock<IPurchaseOrderRepository> mockPurchaseOrderRepository;
        private Mock<IGeneralLedgerAccountRepository> generalLedgerAccountRepositoryMock;

        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepo;
        private Mock<IVendorsRepository> vendorsDataRepo;
        private Mock<IBuyerRepository> buyersDataRepo;
        private Mock<IAccountFundsAvailableRepository> mockAccountFundAvailableRepo;
        private Mock<IPersonRepository> mockPersonRepository;
        private Mock<IRoleRepository> roleRepository;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
        private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
        private Mock<IProcurementsUtilityService> procurementsUtilityServiceMock;
        private Mock<IStaffRepository> staffRepositoryMock;

        protected Domain.Entities.Role viewPurchaseOrderRole = new Domain.Entities.Role(1, "VIEW.PURCHASE.ORDERS");
        protected Domain.Entities.Role updatePurchaseOrderRole = new Domain.Entities.Role(2, "UPDATE.PURCHASE.ORDERS");

        private List<ShipToDestination> shipToDestinationsCollection;
        private List<FreeOnBoardType> freeOnBoardTypesCollection;
        private Collection<VendorTerm> vendorTermsCollection;
        private Collection<CommodityCode> commodityCodeCollection;
        private Collection<CommodityUnitType> commodityUnitTypeCollection;
        private Collection<CommerceTaxCode> commerceTaxCodeCollection;

        [TestInitialize]
        public void Initialize()
        {
            // Set up the mock PO repository
            this.mockPurchaseOrderRepository = new Mock<IPurchaseOrderRepository>();
            generalLedgerAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
            procurementsUtilityServiceMock = new Mock<IProcurementsUtilityService>();

            // build all service objects to use in testing
            staffRepositoryMock = new Mock<IStaffRepository>();
            BuildValidPurchaseOrderService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables.
            service = null;

            testPurchaseOrderRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;
            this.mockPurchaseOrderRepository = null;
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrders2_NoPermissions()
        {
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(null);
            await service.GetPurchaseOrdersAsync(1, 1, new Dtos.PurchaseOrders2());
        }

        [TestMethod]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrder2_MinimumData()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.VendorAddressId = "00120";
            purchaseOrder.AltAddressFlag = true;

            var purchaseOrderCollection = new List<PurchaseOrder>() { purchaseOrder };

            Tuple<IEnumerable<PurchaseOrder>, int> purchaseOrderTuple = new Tuple<IEnumerable<PurchaseOrder>, int>(purchaseOrderCollection, 4);
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(purchaseOrderTuple);
            var vendorsGuid = Guid.NewGuid().ToString();
            var vendorsAddressGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);
            mockPurchaseOrderRepository.Setup(repo => repo.GetGuidFromIdAsync("00120", "ADDRESS")).ReturnsAsync(vendorsAddressGuid);


            var actualCollection = await service.GetPurchaseOrdersAsync(0, 1, new Dtos.PurchaseOrders2());
            Assert.IsNotNull(actualCollection);
            var actual = actualCollection.Item1.FirstOrDefault(x => x.Id == guid);
            Assert.IsNotNull(actual);
            Assert.AreEqual(guid, actual.Id);
            Assert.AreEqual("P000001", actual.OrderNumber);
            Assert.AreEqual(vendorsGuid, actual.Vendor.ExistingVendor.Vendor.Id);
            Assert.AreEqual(vendorsAddressGuid, actual.Vendor.ExistingVendor.AlternativeVendorAddress.Id);
            Assert.AreEqual(actual.Status, Dtos.EnumProperties.PurchaseOrdersStatus.Closed);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_NoPermissions()
        {
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });
            await service.GetPurchaseOrdersByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_RepoReturnsNull()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_NoVendor()
        {
            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "", PurchaseOrderStatus.Outstanding, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);

            var actual = await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_MinimumData()
        {
            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            var actual = await service.GetPurchaseOrdersByGuidAsync(guid);
            Assert.IsNotNull(actual);
            Assert.AreEqual(guid, actual.Id);
            Assert.AreEqual("P000001", actual.OrderNumber);
            Assert.AreEqual(vendorsGuid, actual.Vendor.ExistingVendor.Vendor.Id);
            Assert.AreEqual(actual.Status, Dtos.EnumProperties.PurchaseOrdersStatus.Closed);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_MissingBuyer()
        {
            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Invoiced, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.Buyer = "0000011";
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            var buyersGuid = Guid.NewGuid().ToString();
            buyersDataRepo.Setup(repo => repo.GetBuyerGuidFromIdAsync("0000011")).ReturnsAsync(null);

            await service.GetPurchaseOrdersByGuidAsync(guid);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_MissingDefaultInitiator()
        {
            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.NotApproved, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.DefaultInitiator = "0000012";
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            var initiatorGuid = Guid.NewGuid().ToString();
            buyersDataRepo.Setup(repo => repo.GetBuyerGuidFromIdAsync("0000012")).ReturnsAsync(null);
            await service.GetPurchaseOrdersByGuidAsync(guid);

        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_InvalidShipToCode()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });
            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Backordered, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.ShipToCode = "INVALID";
            purchaseOrder.AltAddressFlag = false;
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_MissingVendorAddress()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Invoiced, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.VendorAddressId = "0000011";
            purchaseOrder.AltAddressFlag = true;
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            mockPurchaseOrderRepository.Setup(repo => repo.GetGuidFromIdAsync("0000011", "ADDRESS")).ReturnsAsync(null);
            await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_InvalidFOB()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });
            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Accepted, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.Fob = "INVALID";
            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_InvalidVendorTerm()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.InProgress, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.VendorTerms = "INVALID";

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            await service.GetPurchaseOrdersByGuidAsync(guid);


        }

        [TestMethod]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var shipToDestination = shipToDestinationsCollection.FirstOrDefault(sd => sd.Code == "CD");
            var fob = freeOnBoardTypesCollection.FirstOrDefault(f => f.Code == "DS");
            var vendorTerm = vendorTermsCollection.FirstOrDefault(f => f.Code == "30");

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.Buyer = "0000011";
            purchaseOrder.DefaultInitiator = "0000012";
            purchaseOrder.InitiatorName = "Renato Carosone";
            purchaseOrder.ShipToCode = shipToDestination.Code;
            purchaseOrder.Fob = fob.Code;
            purchaseOrder.MaintenanceDate = new DateTime(2017, 01, 02);
            purchaseOrder.VendorTerms = vendorTerm.Code;
            purchaseOrder.Comments = "Hello";
            purchaseOrder.InternalComments = "World";
            purchaseOrder.AltShippingAddress = new List<string>() { "123 Main St" };
            purchaseOrder.AltShippingCity = "Fairfax";
            purchaseOrder.AltShippingName = "Shipping Name";
            purchaseOrder.AltShippingPhone = "123-456-7890";
            purchaseOrder.AltShippingPhoneExt = "1";
            purchaseOrder.AltShippingState = "VA";
            purchaseOrder.AltShippingZip = "12345";

            purchaseOrder.MiscAddress = new List<string>() { "123 Main St" };
            purchaseOrder.MiscCity = "Fairfax";
            purchaseOrder.MiscName = new List<string>() { "Name" };
            purchaseOrder.MiscState = "VA";
            purchaseOrder.MiscZip = "12345";

            purchaseOrder.MiscCountry = "US";

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            var buyersGuid = Guid.NewGuid().ToString();
            buyersDataRepo.Setup(repo => repo.GetBuyerGuidFromIdAsync("0000011")).ReturnsAsync(buyersGuid);
            var initiatorGuid = Guid.NewGuid().ToString();
            //buyersDataRepo.Setup(repo => repo.GetBuyerGuidFromIdAsync("0000012")).ReturnsAsync(initiatorGuid);
            mockPersonRepository.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(initiatorGuid);

            var actual = await service.GetPurchaseOrdersByGuidAsync(guid);
            Assert.IsNotNull(actual);
            Assert.AreEqual(guid, actual.Id);
            Assert.AreEqual("P000001", actual.OrderNumber);
            Assert.IsNotNull(actual.Vendor);
            Assert.AreEqual(vendorsGuid, actual.Vendor.ExistingVendor.Vendor.Id);
            Assert.IsNotNull(actual.Buyer);
            Assert.AreEqual(buyersGuid, actual.Buyer.Id);
            Assert.IsNotNull(actual.Initiator);
            Assert.AreEqual(initiatorGuid, actual.Initiator.Detail.Id);
            Assert.AreEqual("Renato Carosone", actual.Initiator.Name);
            Assert.IsNotNull(actual.Shipping);
            Assert.AreEqual(shipToDestination.Guid, actual.Shipping.ShipTo.Id);
            Assert.AreEqual(fob.Guid, actual.Shipping.FreeOnBoard.Id);
            Assert.AreEqual(Dtos.EnumProperties.PurchaseOrdersStatus.Closed, actual.Status);
            var printedComment = actual.Comments.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CommentTypes.Printed);
            Assert.AreEqual("Hello", printedComment.Comment);
            var notPrintedComment = actual.Comments.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CommentTypes.NotPrinted);
            Assert.AreEqual("World", notPrintedComment.Comment);

            Assert.AreEqual(vendorTerm.Guid, actual.PaymentTerms.Id);

            Assert.AreEqual(new DateTime(2017, 01, 02), actual.TransactionDate);
        }


        [TestMethod]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_LineItem_Minimum()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.AddLineItem(new LineItem("1", "rocks", 2, 50, 0));

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);

            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            var actual = await service.GetPurchaseOrdersByGuidAsync(guid);
            Assert.IsNotNull(actual);
            Assert.AreEqual(guid, actual.Id);
            Assert.AreEqual("P000001", actual.OrderNumber);
            Assert.AreEqual(vendorsGuid, actual.Vendor.ExistingVendor.Vendor.Id);
            Assert.AreEqual(actual.Status, Dtos.EnumProperties.PurchaseOrdersStatus.Closed);

            Assert.AreEqual(1, actual.LineItems.Count());
            Assert.AreEqual("1", actual.LineItems[0].LineItemNumber);
            Assert.AreEqual(2, actual.LineItems[0].Quantity);
            Assert.AreEqual(50, actual.LineItems[0].UnitPrice.Value);
            Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, actual.LineItems[0].UnitPrice.Currency);


        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_LineItem_InvalidCommodityCode()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var commodityCode = commodityCodeCollection.FirstOrDefault(cc => cc.Code == "00402");

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.AddLineItem(
                new LineItem("1", "rocks", 2, 50, 0)
                {
                    Comments = "Hello World",
                    CommodityCode = "INVALID"
                }
               );

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);

            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_LineItem_InvalidUnitType()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";
            purchaseOrder.AddLineItem(
                new LineItem("1", "rocks", 2, 50, 0)
                {
                    UnitOfIssue = "INVALID"
                }
                );

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);

            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);
            await service.GetPurchaseOrdersByGuidAsync(guid);
        }

        [TestMethod]
        public async Task PurchaseOrderEEDMServiceTests_GetPurchaseOrdersByGuid_LineItem()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            viewPurchaseOrderRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            roleRepository.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPurchaseOrderRole });

            var commodityCode = commodityCodeCollection.FirstOrDefault(cc => cc.Code == "00402");
            var commodityUnitTypeCode = commodityUnitTypeCollection.FirstOrDefault(cuc => cuc.Code == "rock");
            var commerceTaxCode = commerceTaxCodeCollection.FirstOrDefault(cuc => cuc.Code == "ST");

            var purchaseOrder = new PurchaseOrder("1", guid, "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01));
            purchaseOrder.VendorId = "0000010";

            var purchaseOrderLineItem = new LineItem("1", "rocks", 2, 50, 0)
            {
                Comments = "Hello World",
                CommodityCode = commodityCode.Code,
                UnitOfIssue = commodityUnitTypeCode.Code,
                TradeDiscountAmount = 10,
               

            };
            purchaseOrderLineItem.AddGlDistribution(new LineItemGlDistribution("11-00-02-67-60000-54005", 1, 25));
            purchaseOrderLineItem.AddTax(new LineItemTax(commerceTaxCode.Code, 5));

            purchaseOrder.AddLineItem(purchaseOrderLineItem);

            mockPurchaseOrderRepository.Setup(repo => repo.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);

            var vendorsGuid = Guid.NewGuid().ToString();
            vendorsDataRepo.Setup(repo => repo.GetVendorGuidFromIdAsync("0000010")).ReturnsAsync(vendorsGuid);

            var actual = await service.GetPurchaseOrdersByGuidAsync(guid);
            Assert.IsNotNull(actual);
            Assert.AreEqual(guid, actual.Id);
            Assert.AreEqual("P000001", actual.OrderNumber);
            Assert.AreEqual(vendorsGuid, actual.Vendor.ExistingVendor.Vendor.Id);
            Assert.AreEqual(actual.Status, Dtos.EnumProperties.PurchaseOrdersStatus.Closed);

            Assert.IsNotNull(actual.LineItems);
            Assert.AreEqual(1, actual.LineItems.Count());
            var lineItem = actual.LineItems[0];

            Assert.AreEqual("1", lineItem.LineItemNumber);
            Assert.AreEqual(2, lineItem.Quantity);
            Assert.AreEqual(50, lineItem.UnitPrice.Value);
            Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, lineItem.UnitPrice.Currency);
            Assert.AreEqual("Hello World", lineItem.Comments[0].Comment);
            Assert.AreEqual(Dtos.EnumProperties.CommentTypes.NotPrinted, lineItem.Comments[0].Type);
            Assert.AreEqual(commodityCode.Guid, lineItem.CommodityCode.Id);
            Assert.AreEqual(commodityUnitTypeCode.Guid, lineItem.UnitOfMeasure.Id);


            var lineItemTax = lineItem.TaxCodes.First();
            Assert.IsNotNull(lineItemTax);
            Assert.AreEqual(commerceTaxCode.Guid, lineItemTax.Id);


        }

        #region Build service method

        /// <summary>
        /// Builds multiple purchase order service objects.
        /// </summary>
        private void BuildValidPurchaseOrderService()
        {
            roleRepository = new Mock<IRoleRepository>();
            var loggerObject = new Mock<ILogger>().Object;
            var testColleagueFinanceReferenceDataRepository = new TestColleagueFinanceReferenceDataRepository();

            testPurchaseOrderRepository = new TestPurchaseOrderRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            colleagueFinanceReferenceDataRepository = new Mock<IColleagueFinanceReferenceDataRepository>();  //new TestColleagueFinanceReferenceDataRepository();
            referenceDataRepo = new Mock<IReferenceDataRepository>();
            vendorsDataRepo = new Mock<IVendorsRepository>();
            buyersDataRepo = new Mock<IBuyerRepository>();
            mockAccountFundAvailableRepo = new Mock<IAccountFundsAvailableRepository>();
            mockPersonRepository = new Mock<IPersonRepository>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
            generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();

            commerceTaxCodeCollection = new Collection<CommerceTaxCode>()
                { new CommerceTaxCode("82bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "ST", "TestGUIDdesc") };
            referenceDataRepo.Setup(repo => repo.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(commerceTaxCodeCollection);

            foreach (var type in commerceTaxCodeCollection)
            {
                referenceDataRepo.Setup(repo => repo.GetCommerceTaxCodeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }



            commodityCodeCollection = new Collection<CommodityCode>()
                { new CommodityCode("772bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "00402", "Test Commodity") };
            colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodeCollection);

            foreach (var type in commodityCodeCollection)
            {
                colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityCodeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }


            var apTypes = new Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableSources>()
            { new Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableSources("apTypeGuid321", "AP", "Account Payable") };
            colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(apTypes);

            foreach (var type in apTypes)
            {
                colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetAccountsPayableSourceGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            vendorTermsCollection = new Collection<VendorTerm>()
            {   new VendorTerm("e338c649-db4b-4094-bb05-30ecd56ba82f", "02", "2-10-30"),
                new VendorTerm("d3a915c4-7914-4048-aa17-56d62911264a", "03", "3-10-30"),
                new VendorTerm("88393aeb-8239-4324-8203-707aa1181122", "30", "Net 30 days")
            };
            colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermsCollection);
            foreach (var type in vendorTermsCollection)
            {
                colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetVendorTermGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }


            commodityUnitTypeCollection = new Collection<CommodityUnitType>() {
                new CommodityUnitType("6a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "rock", "Rocks"),
                new CommodityUnitType("449e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "thing", "Things") };
            colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(commodityUnitTypeCollection);

            foreach (var type in commodityUnitTypeCollection)
            {
                colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityUnitTypeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            shipToDestinationsCollection = new List<ShipToDestination>()
                {
                    new ShipToDestination("580b7bbf-db6b-4241-8ce7-f2f94b302dd6", "CD", "Datatel - Central Dist. Office"),
                    new ShipToDestination("20916661-8560-4316-a2f8-72211489a4b9", "DT", "Datatel - Downtown"),
                    new ShipToDestination("7e14d03f-9a39-44db-abea-115b6a1642b0", "EC", "Datatel - Extension Center")
                };
            colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetShipToDestinationsAsync(It.IsAny<bool>()))
                .ReturnsAsync(shipToDestinationsCollection);

            foreach (var type in shipToDestinationsCollection)
            {
                colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetShipToDestinationGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            freeOnBoardTypesCollection = new List<FreeOnBoardType>()
                {
                    new FreeOnBoardType("b69f7e05-89de-466a-8a2c-3c6db4b83501", "DS", "Destination"),
                    new FreeOnBoardType("dcf759b4-5f22-472f-9d1d-e0c6ae7fb62e", "T1", "Test1"),
                    new FreeOnBoardType("4e90b207-c9c7-42e1-8d93-20776e43ae65", "T3", "Test2")
                };
            colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetFreeOnBoardTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(freeOnBoardTypesCollection);

            foreach (var type in freeOnBoardTypesCollection)
            {
                colleagueFinanceReferenceDataRepository.Setup(repo => repo.GetFreeOnBoardTypeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }


            // Mock the reference repository for states
            var states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts"),
                    new State("DC","District of Columbia"),
                    new State("TN","tennessee")
                };
            referenceDataRepo.Setup(repo => repo.GetStateCodesAsync()).ReturnsAsync(states);
            referenceDataRepo.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).ReturnsAsync(states);


            // Mock the reference repository for country
            var countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };
            referenceDataRepo.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

            foreach (var type in countries)
            {
                referenceDataRepo.Setup(repo => repo.GetCountryFromIsoAlpha3CodeAsync(type.IsoAlpha3Code)).ReturnsAsync(type);
            }

            // Set up the service
            service = new PurchaseOrderService(mockPurchaseOrderRepository.Object, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository,
                colleagueFinanceReferenceDataRepository.Object, referenceDataRepo.Object, buyersDataRepo.Object, vendorsDataRepo.Object, baseConfigurationRepository, adapterRegistry.Object,
                currentUserFactory, mockAccountFundAvailableRepo.Object, mockPersonRepository.Object, roleRepository.Object, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerObject);

        }
        #endregion
    }

    [TestClass]
    public class PurchaseOrderServiceTests_POST_V11 : GeneralLedgerCurrentUser
    {
        #region DECLARATION

        protected Domain.Entities.Role createPurchaseOrder = new Domain.Entities.Role(1, "UPDATE.PURCHASE.ORDERS");

        private Mock<IPurchaseOrderRepository> purchaseOrderRepositoryMock;
        private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
        private Mock<IGeneralLedgerUserRepository> generalLedgerUserRepositoryMock;
        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
        private Mock<IBuyerRepository> buyerRepositoryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IVendorsRepository> vendorsRepositoryMock;
        private Mock<IAccountFundsAvailableRepository> mockAccountFundAvailableRepo;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IStaffRepository> staffRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private PurchaseOrderUser currentUserFactory;
        private PurchaseOrderService purchaseOrderService;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private Mock<IGeneralLedgerAccountRepository> generalLedgerAccountRepositoryMock;
        private Mock<IProcurementsUtilityService> procurementsUtilityServiceMock;

        private Dtos.PurchaseOrders2 purchaseOrder;
        private PurchaseOrder domainPurchaseOrder;
        private List<Dtos.PurchaseOrdersLineItemsDtoProperty> lineItems;
        private List<LineItem> domainLineItems;
        private List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty> accountDetails;
        private List<FundsAvailable> accountFundsAvailable;
        private Dtos.DtoProperties.PurchaseOrdersAllocationDtoProperty allocation;
        private Dtos.DtoProperties.PurchaseOrdersAllocatedDtoProperty allocated;
        private Dtos.PurchaseOrdersVendorDtoProperty2 vendor;
        private Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty initiator;
        private Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty shipping;
        private IEnumerable<ShipToDestination> shipToDestination;
        private IEnumerable<FreeOnBoardType> freeOnBoardType;
        private Dtos.OverrideShippingDestinationDtoProperty overrideShippingDestination;
        private Dtos.AddressPlace place;
        private IEnumerable<AccountsPayableSources> paymentSource;
        private IEnumerable<VendorTerm> vendorTerms;
        private IEnumerable<CommodityCode> commodityCodes;
        private IEnumerable<CommodityUnitType> commodityUnitTypes;
        private IEnumerable<CommerceTaxCode> taxCodes;
        private IEnumerable<Country> countries;
        private IEnumerable<State> states;

        private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            purchaseOrderRepositoryMock = new Mock<IPurchaseOrderRepository>();
            generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            buyerRepositoryMock = new Mock<IBuyerRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            vendorsRepositoryMock = new Mock<IVendorsRepository>();
            mockAccountFundAvailableRepo = new Mock<IAccountFundsAvailableRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            currentUserFactory = new GeneralLedgerCurrentUser.PurchaseOrderUser();
            generalLedgerAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
            procurementsUtilityServiceMock = new Mock<IProcurementsUtilityService>();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            staffRepositoryMock = new Mock<IStaffRepository>();

            purchaseOrderService = new PurchaseOrderService(purchaseOrderRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object, generalLedgerUserRepositoryMock.Object,
                colleagueFinanceReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object, buyerRepositoryMock.Object,
                vendorsRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, mockAccountFundAvailableRepo.Object,
                personRepositoryMock.Object, roleRepositoryMock.Object, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerMock.Object);

            InitializeTestData();

            InitializeMock();
        }

        [TestCleanup]
        public void Cleanup()
        {
            purchaseOrderRepositoryMock = null;
            generalLedgerConfigurationRepositoryMock = null;
            generalLedgerUserRepositoryMock = null;
            colleagueFinanceReferenceDataRepositoryMock = null;
            buyerRepositoryMock = null;
            referenceDataRepositoryMock = null;
            vendorsRepositoryMock = null;
            mockAccountFundAvailableRepo = null;
            personRepositoryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            configurationRepositoryMock = null;
        }

        private void InitializeTestData()
        {
            domainLineItems = new List<LineItem>()
                {
                    new LineItem("1", "desc", 10, 100, 110)
                    {
                       Comments = "Comments",
                       CommodityCode = "1",
                       VendorPart = "1",
                       UnitOfIssue = "1",
                       TradeDiscountAmount = 100,
                       StatusDate = DateTime.Today,
                       LineItemStatus = LineItemStatus.Outstanding
                    },
                    new LineItem("2", "desc", 10, 100, 110)
                    {
                       Comments = "Comments",
                       CommodityCode = "1",
                       VendorPart = "1",
                       UnitOfIssue = "1",
                       TradeDiscountPercentage = 10
                    }
                };

            states = new List<State>() { new State("FL", "Florida", "AUS") { } };

            countries = new List<Country>() { new Country("USA", "Desc", "USA", "USA") { } };

            taxCodes = new List<CommerceTaxCode>() { new CommerceTaxCode("1l49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { AppurEntryFlag = true, UseTaxFlag = false } };

            commodityUnitTypes = new List<CommodityUnitType>() { new CommodityUnitType("1k49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            commodityCodes = new List<CommodityCode>() { new CommodityCode("1j49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            vendorTerms = new List<VendorTerm>() { new VendorTerm("1i49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            paymentSource = new List<AccountsPayableSources>() { new AccountsPayableSources("1h49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            place = new Dtos.AddressPlace()
            {
                Country = new Dtos.AddressCountry()
                {
                    Code = Dtos.EnumProperties.IsoCode.USA,
                    Locality = "L",
                    Region = new Dtos.AddressRegion() { Code = "R-R" },
                    PostalCode = "12345"
                }
            };

            overrideShippingDestination = new Dtos.OverrideShippingDestinationDtoProperty()
            {
                Description = "D",
                AddressLines = new List<string>() { "A1" },
                Place = place
            };

            freeOnBoardType = new List<FreeOnBoardType>() { new FreeOnBoardType("1g49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Description") { } };

            shipToDestination = new List<ShipToDestination>() { new ShipToDestination("1f49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Description") { } };

            shipping = new Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty()
            {
                ShipTo = new Dtos.GuidObject2("1f49eed8-5fe7-4120-b1cf-f23266b9e874"),
                FreeOnBoard = new Dtos.GuidObject2("1g49eed8-5fe7-4120-b1cf-f23266b9e874")
            };

            initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty() { Detail = new Dtos.GuidObject2("1f49eed8-5fe7-4120-b1cf-f23266b9e874") };

            vendor = new Dtos.PurchaseOrdersVendorDtoProperty2()
            {
                ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                {
                    Vendor = new Dtos.GuidObject2("1d49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    AlternativeVendorAddress = new Dtos.GuidObject2("1d49eed8-5af7-4120-b1cf-f23266b9e874")
                },
                ManualVendorDetails = new Dtos.ManualVendorDetailsDtoProperty()
                {
                    Name = "Name",
                    AddressLines = new List<string>() { "A1" },
                    Place = place
                }
            };

            allocated = new Dtos.DtoProperties.PurchaseOrdersAllocatedDtoProperty()
            {
                Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD },
                Quantity = 10,
                Percentage = 90
            };

            allocation = new Dtos.DtoProperties.PurchaseOrdersAllocationDtoProperty() { Allocated = allocated };

            accountFundsAvailable = new List<FundsAvailable>();

            accountDetails = new List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty()
                    {
                        SubmittedBy = new Dtos.GuidObject2("1c49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        AccountingString = "1",
                        Allocation = allocation,
                        StatusDate = DateTime.Today,
                        Status = Dtos.EnumProperties.LineItemStatus.Outstanding
                    },
                    new Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty()
                    {
                        SubmittedBy = new Dtos.GuidObject2("2c49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        AccountingString = "2",
                        Allocation = allocation,
                        StatusDate = DateTime.Today,
                        Status = Dtos.EnumProperties.LineItemStatus.Outstanding
                    }
                };

            lineItems = new List<Dtos.PurchaseOrdersLineItemsDtoProperty>()
                {
                    new Dtos.PurchaseOrdersLineItemsDtoProperty()
                    {
                        AccountDetail = accountDetails,
                        Description = "Desc",
                        Quantity = 100,
                        UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10 },
                        CommodityCode = new Dtos.GuidObject2("1j49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        UnitOfMeasure = new Dtos.GuidObject2("1k49eed8-5fe7-4120-b1cf-f23266b9e874")

                    },
                    new Dtos.PurchaseOrdersLineItemsDtoProperty()
                    {
                        AccountDetail = accountDetails,
                        Description = "Desc",
                        Comments = new List<Dtos.PurchaseOrdersCommentsDtoProperty>()
                        {
                            new Dtos.PurchaseOrdersCommentsDtoProperty() { Comment = "comment", Type = Dtos.EnumProperties.CommentTypes.Printed}
                        },
                        TradeDiscount = new Dtos.TradeDiscountDtoProperty()
                        {
                            Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 100}
                            //,Percent = 10
                        },
                        PartNumber = "1",
                        DesiredDate = DateTime.Today.AddDays(2),
                        TaxCodes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1l49eed8-5fe7-4120-b1cf-f23266b9e874") },
                         UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10 },
                    },

                };

            purchaseOrder = new Dtos.PurchaseOrders2()
            {
                Id = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                SubmittedBy = new Dtos.GuidObject2("1b49eed8-5fe7-4120-b1cf-f23266b9e874"),
                LineItems = lineItems,
                TransactionDate = DateTime.Today,
                Vendor = vendor,
                Status = Dtos.EnumProperties.PurchaseOrdersStatus.InProgress,
                OrderNumber = "1",
                OrderedOn = DateTime.Today,
                Type = Dtos.EnumProperties.PurchaseOrdersTypes.Travel,
                DeliveredBy = DateTime.Today.AddDays(2),
                Buyer = new Dtos.GuidObject2("1e49eed8-5fe7-4120-b1cf-f23266b9e874"),
                Initiator = initiator,
                Shipping = shipping,
                OverrideShippingDestination = overrideShippingDestination,
                ReferenceNumbers = new List<string>() { "1" },
                PaymentSource = new Dtos.GuidObject2("1h49eed8-5fe7-4120-b1cf-f23266b9e874"),
                PaymentTerms = new Dtos.GuidObject2("1i49eed8-5fe7-4120-b1cf-f23266b9e874"),
                Comments = new List<Dtos.PurchaseOrdersCommentsDtoProperty>()
                    {
                        new Dtos.PurchaseOrdersCommentsDtoProperty() {Type = Dtos.EnumProperties.CommentTypes.NotPrinted, Comment = "1"},
                        new Dtos.PurchaseOrdersCommentsDtoProperty() {Type = Dtos.EnumProperties.CommentTypes.Printed, Comment = "2"}
                    }
            };

            domainPurchaseOrder = new PurchaseOrder("1", "number", "VendorName", PurchaseOrderStatus.InProgress, DateTime.Today, DateTime.Today)
            {
                CurrencyCode = "USD",
                Type = "Travel",
                SubmittedBy = "1",
                MaintenanceDate = DateTime.Today,
                DeliveryDate = DateTime.Today,
                VoidGlTranDate = DateTime.Today,
                ReferenceNo = new List<string>() { "1" },
                Buyer = "1",
                InitiatorName = "Name",
                DefaultInitiator = "1",
                ShipToCode = "1",
                Fob = "1",
                AltShippingName = "A",
                AltShippingAddress = new List<string>() { "A" },
                MiscCountry = "USA",
                AltShippingCity = "C",
                AltShippingState = "FL",
                AltShippingZip = "Z",
                AltShippingCountry = "USA",
                VendorId = "1",
                VendorAddressId = "00010",
                MiscName = new List<string>() { "Name" },
                MiscAddress = new List<string>() { "Line1" },
                VendorTerms = "1",
                ApType = "1",
                Comments = "comments",
                InternalComments = "Internalcomments",
            };

            domainLineItems.ForEach(d =>
            {
                domainPurchaseOrder.AddLineItem(d);
                d.AddTax(new LineItemTax("1", 100) { LineGlNumber = "1" });
                d.AddGlDistribution(new LineItemGlDistribution("11-00-02-67-60000-54005", 10, 100, 10));
            });

            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());
        }

        private void InitializeMock()
        {
            mockAccountFundAvailableRepo.Setup(a => a.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                           .ReturnsAsync(accountFundsAvailable);

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");

            buyerRepositoryMock.Setup(p => p.GetBuyerIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetShipToDestinationsAsync(It.IsAny<bool>())).ReturnsAsync(shipToDestination);

            foreach (var type in shipToDestination)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetShipToDestinationGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }


            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetFreeOnBoardTypesAsync(It.IsAny<bool>())).ReturnsAsync(freeOnBoardType);

            foreach (var type in freeOnBoardType)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetFreeOnBoardTypeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(paymentSource);

            foreach (var type in paymentSource)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetAccountsPayableSourceGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTerms);

            foreach (var type in vendorTerms)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetVendorTermGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);

            foreach (var type in commodityCodes)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetCommodityCodeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(commodityUnitTypes);

            foreach (var type in commodityUnitTypes)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetCommodityUnitTypeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }


            referenceDataRepositoryMock.Setup(r => r.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);
            foreach (var type in countries)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryFromIsoAlpha3CodeAsync(type.IsoAlpha3Code)).ReturnsAsync(type);
            }

            referenceDataRepositoryMock.Setup(r => r.GetStateCodesAsync(It.IsAny<bool>())).ReturnsAsync(states);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_ArgumentNullException_PurchaseOrders_Null()
        {
            await purchaseOrderService.PostPurchaseOrdersAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_ArgumentNullException_PurchaseOrders_Id_NUll()
        {
            await purchaseOrderService.PostPurchaseOrdersAsync(new Dtos.PurchaseOrders2() { });
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_PermissionsException()
        {
            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_Exception_AccountingString_NullOrEmpty()
        {
            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.LastOrDefault().AccountingString = "1";

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_ArgumentNullException_ExistingVendor_NullOrEmpty()
        {
            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Vendor.ExistingVendor.Vendor.Id = null;

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_ApplicationException_Invalid_PurchaseOrder_Status()
        {
            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Status = Dtos.EnumProperties.PurchaseOrdersStatus.Closed;

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_Exception_When_Person_NullOrEmpty()
        {
            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            buyerRepositoryMock.Setup(p => p.GetBuyerIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_When_Buyer_NullOrEmpty()
        {
            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                                .Returns(Task.FromResult("1"))
                                .Returns(Task.FromResult(""));
            buyerRepositoryMock.SetupSequence(p => p.GetBuyerIdFromGuidAsync(It.IsAny<string>()))
                                .Returns(Task.FromResult("1"))
                                .Returns(Task.FromResult(""));

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_When_PurchaseOrder_Initiator_NullOrEmpty()
        {
            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                                .Returns(Task.FromResult("1"))
                                .Returns(Task.FromResult("1"))
                                .Returns(Task.FromResult(""));
            buyerRepositoryMock.SetupSequence(p => p.GetBuyerIdFromGuidAsync(It.IsAny<string>()))
                               .Returns(Task.FromResult("1"))
                                .Returns(Task.FromResult("1"))
                                .Returns(Task.FromResult(""));
            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_Exception_When_Shipping_Destination_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetShipToDestinationsAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_KeyNotFoundException_When_Shipping_Destination_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Shipping.ShipTo.Id = "1";

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_Exception_When_Shipping_FreeOnboard_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetFreeOnBoardTypesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_KeyNotFoundException_When_Shipping_FreeOnboard_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Shipping.FreeOnBoard.Id = "1";

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_ArgumentException_Invalid_Vendor()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            var numberOfCalls = 0;
            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>()))
                                       .ReturnsAsync(null)
                                       .Callback(() => { numberOfCalls++; if (numberOfCalls == 2) throw new ArgumentException(); });

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_ArgumentNullException_When_VendorId_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_ArgumentException_Invalid_VendorAddressId()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            var numberOfCalls = 0;
            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>()))
                                       .ReturnsAsync(null)
                                       .Callback(() => { numberOfCalls++; if (numberOfCalls == 3) throw new ArgumentException(); });

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_ArgumentNullException_When_VendorAddressId_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_KeyNotFoundException_When_PaymentSource_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });
            mockAccountFundAvailableRepo.Setup(a => a.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                           .ReturnsAsync(accountFundsAvailable);

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrder.PaymentSource.Id = "1";

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_KeyNotFoundException_When_VendorTerms_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrder.PaymentTerms.Id = "1";

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_Exception_When_CommodityCodes_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_Exception_When_CommodityUnitTypes_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_Exception_When_CoomodityCode_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrder.LineItems.FirstOrDefault().CommodityCode.Id = "1";

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_DtoToEntity_Exception_When_CoomodityUnit_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrder.LineItems.FirstOrDefault().UnitOfMeasure.Id = "1";

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_IntegrationApiException()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ThrowsAsync(new RepositoryException());

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(null);

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_SubmittedBy_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_Buyer_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            domainPurchaseOrder.CurrencyCode = null;
            domainPurchaseOrder.Type = "Procurement";

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.SetupSequence(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>()))
                               .Returns(Task.FromResult(guid)).Returns(Task.FromResult(""));


            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_DefaultInitiator_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            domainPurchaseOrder.CurrencyCode = null;
            domainPurchaseOrder.HostCountry = "CAN";
            domainPurchaseOrder.Type = "Eprocurement";

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.SetupSequence(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>()))
                               .Returns(Task.FromResult(guid)).Returns(Task.FromResult(guid)).Returns(Task.FromResult(""));
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_ShipToDestination_Exception()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            domainPurchaseOrder.CurrencyCode = "USD1";
            domainPurchaseOrder.HostCountry = "CAN";
            domainPurchaseOrder.Type = "";
            domainPurchaseOrder.ShipToCode = "invalid";

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_FreeOnBoardType_Exception()
        {

            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            domainPurchaseOrder.CurrencyCode = "USD1";
            domainPurchaseOrder.Fob = "invalid";

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_Country_Null_And_MiscCountry_NotNull()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            domainPurchaseOrder.MiscCountry = "AUS";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_ArgumentException_State_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            domainPurchaseOrder.AltShippingState = "AL";

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_Vendor_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_VendorTerm_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("001");
            domainPurchaseOrder.VendorTerms = "2";

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_ApType_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("001");
            domainPurchaseOrder.ApType = "2";

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_CommodityCode_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("001");
            domainLineItems.FirstOrDefault().CommodityCode = "2";

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_Exception_UnitOfIssue_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("001");
            domainLineItems.FirstOrDefault().UnitOfIssue = "2";

            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_EntityToDto_CommerceTaxCodes_Null()
        {
            var testGlAccountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);
        }

        [TestMethod]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("1");

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            vendorsRepositoryMock.Setup(r => r.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            GuidLookupResult guidLookupResult1 = new GuidLookupResult() { Entity = "ADDRESS", PrimaryKey = "1" };
            referenceDataRepositoryMock.Setup(r => r.GetGuidLookupResultFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookupResult1);

            var result = await purchaseOrderService.PostPurchaseOrdersAsync(purchaseOrder);

            Assert.IsNotNull(result);
        }
    }

    [TestClass]
    public class PurchaseOrderServiceTests_PUT_v11 : GeneralLedgerCurrentUser
    {
        #region DECLARATION

        protected Domain.Entities.Role createPurchaseOrder = new Domain.Entities.Role(1, "UPDATE.PURCHASE.ORDERS");

        private Mock<IPurchaseOrderRepository> purchaseOrderRepositoryMock;
        private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
        private Mock<IGeneralLedgerUserRepository> generalLedgerUserRepositoryMock;
        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
        private Mock<IBuyerRepository> buyerRepositoryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IVendorsRepository> vendorsRepositoryMock;
        private Mock<IAccountFundsAvailableRepository> accountFundAvailableRepoMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IGeneralLedgerAccountRepository> generalLedgerAccountRepositoryMock;
        private Mock<IProcurementsUtilityService> procurementsUtilityServiceMock;
        private PurchaseOrderUser currentUserFactory;

        private PurchaseOrderService purchaseOrderService;

        private Dtos.PurchaseOrders2 purchaseOrder;
        private PurchaseOrder domainPurchaseOrder;
        private List<Dtos.PurchaseOrdersLineItemsDtoProperty> lineItems;
        private List<LineItem> domainLineItems;
        private List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty> accountDetails;
        private List<FundsAvailable> accountFundsAvailable;
        private Dtos.DtoProperties.PurchaseOrdersAllocationDtoProperty allocation;
        private Dtos.DtoProperties.PurchaseOrdersAllocatedDtoProperty allocated;
        private Dtos.PurchaseOrdersVendorDtoProperty2 vendor;
        private Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty initiator;
        private Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty shipping;
        private IEnumerable<ShipToDestination> shipToDestination;
        private IEnumerable<FreeOnBoardType> freeOnBoardType;
        private Dtos.OverrideShippingDestinationDtoProperty overrideShippingDestination;
        private Dtos.AddressPlace place;
        private IEnumerable<AccountsPayableSources> paymentSource;
        private IEnumerable<VendorTerm> vendorTerms;
        private IEnumerable<CommodityCode> commodityCodes;
        private IEnumerable<CommodityUnitType> commodityUnitTypes;
        private IEnumerable<CommerceTaxCode> taxCodes;
        private IEnumerable<Country> countries;
        private IEnumerable<State> states;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private Mock<IStaffRepository> staffRepositoryMock;


        private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            purchaseOrderRepositoryMock = new Mock<IPurchaseOrderRepository>();
            generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            buyerRepositoryMock = new Mock<IBuyerRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            vendorsRepositoryMock = new Mock<IVendorsRepository>();
            accountFundAvailableRepoMock = new Mock<IAccountFundsAvailableRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            currentUserFactory = new GeneralLedgerCurrentUser.PurchaseOrderUser();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            staffRepositoryMock = new Mock<IStaffRepository>();
            generalLedgerAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
            procurementsUtilityServiceMock = new Mock<IProcurementsUtilityService>();

            purchaseOrderService = new PurchaseOrderService(purchaseOrderRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object, generalLedgerUserRepositoryMock.Object,
                colleagueFinanceReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object, buyerRepositoryMock.Object,
                vendorsRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, accountFundAvailableRepoMock.Object,
                personRepositoryMock.Object, roleRepositoryMock.Object, staffRepositoryMock.Object, generalLedgerAccountRepositoryMock.Object, procurementsUtilityServiceMock.Object, loggerMock.Object);

            InitializeTestData();

            InitializeMock();
        }

        [TestCleanup]
        public void Cleanup()
        {
            purchaseOrderRepositoryMock = null;
            generalLedgerConfigurationRepositoryMock = null;
            generalLedgerUserRepositoryMock = null;
            colleagueFinanceReferenceDataRepositoryMock = null;
            buyerRepositoryMock = null;
            referenceDataRepositoryMock = null;
            vendorsRepositoryMock = null;
            accountFundAvailableRepoMock = null;
            personRepositoryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            configurationRepositoryMock = null;
        }

        private void InitializeTestData()
        {
            domainLineItems = new List<LineItem>()
                {
                    new LineItem("1", "desc", 10, 100, 110)
                    {
                       Comments = "Comments",
                       CommodityCode = "1",
                       VendorPart = "1",
                       UnitOfIssue = "1",
                       TradeDiscountAmount = 100,
                       StatusDate = DateTime.Today,
                       LineItemStatus = LineItemStatus.Outstanding
                    },
                    new LineItem("2", "desc", 10, 100, 110)
                    {
                       Comments = "Comments",
                       CommodityCode = "1",
                       VendorPart = "1",
                       UnitOfIssue = "1",
                       TradeDiscountPercentage = 10
                    }
                };

            states = new List<State>() { new State("FL", "Florida", "AUS") { } };

            countries = new List<Country>() { new Country("USA", "Desc", "USA", "USA") { } };

            taxCodes = new List<CommerceTaxCode>() { new CommerceTaxCode("1l49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { AppurEntryFlag = true, UseTaxFlag = false } };

            commodityUnitTypes = new List<CommodityUnitType>() { new CommodityUnitType("1k49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            commodityCodes = new List<CommodityCode>() { new CommodityCode("1j49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            vendorTerms = new List<VendorTerm>() { new VendorTerm("1i49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            paymentSource = new List<AccountsPayableSources>() { new AccountsPayableSources("1h49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { } };

            place = new Dtos.AddressPlace()
            {
                Country = new Dtos.AddressCountry()
                {
                    Code = Dtos.EnumProperties.IsoCode.USA,
                    Locality = "L",
                    Region = new Dtos.AddressRegion() { Code = "R-R" },
                    PostalCode = "12345"
                }
            };

            overrideShippingDestination = new Dtos.OverrideShippingDestinationDtoProperty()
            {
                Description = "D",
                AddressLines = new List<string>() { "A1" },
                Place = place
            };

            freeOnBoardType = new List<FreeOnBoardType>() { new FreeOnBoardType("1g49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Description") { } };

            shipToDestination = new List<ShipToDestination>() { new ShipToDestination("1f49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Description") { } };

            shipping = new Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty()
            {
                ShipTo = new Dtos.GuidObject2("1f49eed8-5fe7-4120-b1cf-f23266b9e874"),
                FreeOnBoard = new Dtos.GuidObject2("1g49eed8-5fe7-4120-b1cf-f23266b9e874")
            };

            initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty() { Detail = new Dtos.GuidObject2("1f49eed8-5fe7-4120-b1cf-f23266b9e874") };

            vendor = new Dtos.PurchaseOrdersVendorDtoProperty2()
            {
                ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                {
                    Vendor = new Dtos.GuidObject2("1d49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    AlternativeVendorAddress = new Dtos.GuidObject2("1234eed8-5fa7-4120-b1cf-f23266b9e874")
                },
                ManualVendorDetails = new Dtos.ManualVendorDetailsDtoProperty()
                {
                    Name = "Name",
                    AddressLines = new List<string>() { "A1" },
                    Place = place
                }
            };

            allocated = new Dtos.DtoProperties.PurchaseOrdersAllocatedDtoProperty()
            {
                Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD },
                Quantity = 10,
                Percentage = 90
            };

            allocation = new Dtos.DtoProperties.PurchaseOrdersAllocationDtoProperty() { Allocated = allocated };

            accountFundsAvailable = new List<FundsAvailable>();

            accountDetails = new List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty()
                    {
                        SubmittedBy = new Dtos.GuidObject2("1c49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        AccountingString = "1",
                        Allocation = allocation,
                        StatusDate = DateTime.Today,
                        Status = Dtos.EnumProperties.LineItemStatus.Outstanding
                    }
                };

            lineItems = new List<Dtos.PurchaseOrdersLineItemsDtoProperty>()
                {
                    new Dtos.PurchaseOrdersLineItemsDtoProperty()
                    {
                        AccountDetail = accountDetails,
                        Description = "Desc",
                        Quantity = 100,
                        UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10 },
                        CommodityCode = new Dtos.GuidObject2("1j49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        UnitOfMeasure = new Dtos.GuidObject2("1k49eed8-5fe7-4120-b1cf-f23266b9e874")

                    },
                    new Dtos.PurchaseOrdersLineItemsDtoProperty()
                    {
                        AccountDetail = accountDetails,
                        Description = "Desc",
                        Comments = new List<Dtos.PurchaseOrdersCommentsDtoProperty>()
                        {
                            new Dtos.PurchaseOrdersCommentsDtoProperty() { Comment = "comment", Type = Dtos.EnumProperties.CommentTypes.Printed}
                        },
                        TradeDiscount = new Dtos.TradeDiscountDtoProperty()
                        {
                            Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 100}
                            //, Percent = 10
                        },
                        PartNumber = "1",
                        DesiredDate = DateTime.Today.AddDays(2),
                        TaxCodes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1l49eed8-5fe7-4120-b1cf-f23266b9e874") },
                        UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10 },
                    },

                };

            purchaseOrder = new Dtos.PurchaseOrders2()
            {
                Id = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                SubmittedBy = new Dtos.GuidObject2("1b49eed8-5fe7-4120-b1cf-f23266b9e874"),
                LineItems = lineItems,
                TransactionDate = DateTime.Today,
                Vendor = vendor,
                Status = Dtos.EnumProperties.PurchaseOrdersStatus.InProgress,
                OrderNumber = "1",
                OrderedOn = DateTime.Today,
                Type = Dtos.EnumProperties.PurchaseOrdersTypes.Travel,
                DeliveredBy = DateTime.Today.AddDays(2),
                Buyer = new Dtos.GuidObject2("1e49eed8-5fe7-4120-b1cf-f23266b9e874"),
                Initiator = initiator,
                Shipping = shipping,
                OverrideShippingDestination = overrideShippingDestination,
                ReferenceNumbers = new List<string>() { "1" },
                PaymentSource = new Dtos.GuidObject2("1h49eed8-5fe7-4120-b1cf-f23266b9e874"),
                PaymentTerms = new Dtos.GuidObject2("1i49eed8-5fe7-4120-b1cf-f23266b9e874"),
                Comments = new List<Dtos.PurchaseOrdersCommentsDtoProperty>()
                    {
                        new Dtos.PurchaseOrdersCommentsDtoProperty() {Type = Dtos.EnumProperties.CommentTypes.NotPrinted, Comment = "1"},
                        new Dtos.PurchaseOrdersCommentsDtoProperty() {Type = Dtos.EnumProperties.CommentTypes.Printed, Comment = "2"}
                    }
            };

            domainPurchaseOrder = new PurchaseOrder("1", "number", "VendorName", PurchaseOrderStatus.InProgress, DateTime.Today, DateTime.Today)
            {
                CurrencyCode = "USD",
                Type = "Travel",
                SubmittedBy = "1",
                MaintenanceDate = DateTime.Today,
                DeliveryDate = DateTime.Today,
                VoidGlTranDate = DateTime.Today,
                ReferenceNo = new List<string>() { "1" },
                Buyer = "1",
                InitiatorName = "Name",
                DefaultInitiator = "1",
                ShipToCode = "1",
                Fob = "1",
                AltShippingName = "A",
                AltShippingAddress = new List<string>() { "A" },
                MiscCountry = "USA",
                AltShippingCity = "C",
                AltShippingState = "FL",
                AltShippingZip = "Z",
                AltShippingCountry = "USA",
                VendorId = "1",
                VendorAddressId = "0001",
                MiscName = new List<string>() { "Name" },
                MiscAddress = new List<string>() { "Line1" },
                VendorTerms = "1",
                ApType = "1",
                Comments = "comments",
                InternalComments = "Internalcomments",
            };

            domainLineItems.ForEach(d =>
            {
                domainPurchaseOrder.AddLineItem(d);
                d.AddTax(new LineItemTax("1", 100) { LineGlNumber = "1" });
                d.AddGlDistribution(new LineItemGlDistribution("11-00-02-67-60000-54005", 10, 100, 10));
            });

            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());
        }

        private void InitializeMock()
        {
            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            accountFundAvailableRepoMock.Setup(a => a.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                           .ReturnsAsync(accountFundsAvailable);

            buyerRepositoryMock.Setup(p => p.GetBuyerIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetShipToDestinationsAsync(It.IsAny<bool>())).ReturnsAsync(shipToDestination);

            foreach (var type in shipToDestination)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetShipToDestinationGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }


            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetFreeOnBoardTypesAsync(It.IsAny<bool>())).ReturnsAsync(freeOnBoardType);

            foreach (var type in freeOnBoardType)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetFreeOnBoardTypeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(paymentSource);

            foreach (var type in paymentSource)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetAccountsPayableSourceGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTerms);

            foreach (var type in vendorTerms)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetVendorTermGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);

            foreach (var type in commodityCodes)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetCommodityCodeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(commodityUnitTypes);

            foreach (var type in commodityUnitTypes)
            {
                colleagueFinanceReferenceDataRepositoryMock.Setup(repo => repo.GetCommodityUnitTypeGuidAsync(type.Code)).ReturnsAsync(type.Guid);
            }


            referenceDataRepositoryMock.Setup(r => r.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

            foreach (var type in countries)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryFromIsoAlpha3CodeAsync(type.IsoAlpha3Code)).ReturnsAsync(type);
            }


            referenceDataRepositoryMock.Setup(r => r.GetStateCodesAsync(It.IsAny<bool>())).ReturnsAsync(states);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_ArgumentNullException_PurchaseOrder_Null()
        {
            await purchaseOrderService.PutPurchaseOrdersAsync(It.IsAny<string>(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_ArgumentNullException_PurchaseOrders_Id_Null()
        {
            await purchaseOrderService.PutPurchaseOrdersAsync(It.IsAny<string>(), new Dtos.PurchaseOrders2() { });
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PurchaseOrdersService_PostPurchaseOrdersAsync_PermissionsException()
        {
            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_Exception_AccountingString_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.LastOrDefault().AccountingString = "1";

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException( typeof( IntegrationApiException ) )]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_Exception_TaxFormComponent_Id_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup( repo => repo.GetAccountStructureAsync() ).ReturnsAsync( testGlAccountStructure );

            createPurchaseOrder.AddPermission( new Ellucian.Colleague.Domain.Entities.Permission( ColleagueFinancePermissionCodes.UpdatePurchaseOrders ) );
            roleRepositoryMock.Setup( rpm => rpm.Roles ).Returns( new List<Domain.Entities.Role>() { createPurchaseOrder } );

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.LastOrDefault().TaxFormComponent = new Dtos.GuidObject2();

            await purchaseOrderService.PutPurchaseOrdersAsync( purchaseOrder.Id, purchaseOrder );
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_ExistingVendor_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Vendor.ExistingVendor.Vendor.Id = null;

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_Invalid_PurchaseOrder_Status()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Status = Dtos.EnumProperties.PurchaseOrdersStatus.Closed;

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_Person_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            buyerRepositoryMock.Setup(p => p.GetBuyerIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

            purchaseOrder.Status = Dtos.EnumProperties.PurchaseOrdersStatus.Notapproved;
            purchaseOrder.Type = Dtos.EnumProperties.PurchaseOrdersTypes.Procurement;

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_Shipping_Destination_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetShipToDestinationsAsync(It.IsAny<bool>())).ReturnsAsync(null);

            purchaseOrder.Status = Dtos.EnumProperties.PurchaseOrdersStatus.Reconciled;

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_Shipping_Destination_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Shipping.ShipTo.Id = "1";
            purchaseOrder.Status = Dtos.EnumProperties.PurchaseOrdersStatus.Voided;

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_Shipping_FreeOnboard_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetFreeOnBoardTypesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_Shipping_FreeOnboard_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.Shipping.FreeOnBoard.Id = "1";

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_Invalid_Vendor()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            var numberOfCalls = 0;
            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>()))
                                       .Returns(Task.FromResult("1"))
                                       .Callback(() => { numberOfCalls++; if (numberOfCalls == 3) throw new ArgumentException(); });

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_VendorId_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.SetupSequence(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>()))
                                       .Returns(Task.FromResult("1"))
                                       .Returns(Task.FromResult("1"))
                                       .Returns(Task.FromResult(""));

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_Invalid_VendorAddressId()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            var numberOfCalls = 0;
            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>()))
                                       .Returns(Task.FromResult("1"))
                                       .Callback(() => { numberOfCalls++; if (numberOfCalls == 4) throw new ArgumentException(); });

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_VendorAddressId_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.SetupSequence(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>()))
                                       .Returns(Task.FromResult("1"))
                                       .Returns(Task.FromResult("1"))
                                       .Returns(Task.FromResult("1"))
                                       .Returns(Task.FromResult(""));

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException( typeof( IntegrationApiException ) )]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_PaymentSource_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup( repo => repo.GetAccountStructureAsync() ).ReturnsAsync( testGlAccountStructure );

            createPurchaseOrder.AddPermission( new Ellucian.Colleague.Domain.Entities.Permission( ColleagueFinancePermissionCodes.UpdatePurchaseOrders ) );
            roleRepositoryMock.Setup( rpm => rpm.Roles ).Returns( new List<Domain.Entities.Role>() { createPurchaseOrder } );

            purchaseOrder.PaymentSource = null;

            await purchaseOrderService.PutPurchaseOrdersAsync( purchaseOrder.Id, purchaseOrder );
        }

        [TestMethod]
        [ExpectedException( typeof( IntegrationApiException ) )]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_PaymentSource_Id_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup( repo => repo.GetAccountStructureAsync() ).ReturnsAsync( testGlAccountStructure );

            createPurchaseOrder.AddPermission( new Ellucian.Colleague.Domain.Entities.Permission( ColleagueFinancePermissionCodes.UpdatePurchaseOrders ) );
            roleRepositoryMock.Setup( rpm => rpm.Roles ).Returns( new List<Domain.Entities.Role>() { createPurchaseOrder } );

            purchaseOrder.PaymentSource.Id = string.Empty;

            await purchaseOrderService.PutPurchaseOrdersAsync( purchaseOrder.Id, purchaseOrder );
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_PaymentSource_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.PaymentSource.Id = "1";

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_VendorTerms_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.PaymentTerms.Id = "1";

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_CommodityCodes_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_CommodityUnitTypes_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_CoomodityCode_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().CommodityCode.Id = "1";

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_DtoToEntity_Exception_When_CoomodityUnit_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);
            purchaseOrder.LineItems.FirstOrDefault().UnitOfMeasure.Id = "1";

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_IntegrationApiException()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Accepted;

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ThrowsAsync(new RepositoryException());

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Backordered;
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);
            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(null);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_SubmittedBy_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Closed;

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_Buyer_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Invoiced;

            domainPurchaseOrder.CurrencyCode = null;
            domainPurchaseOrder.Type = "Procurement";

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>()))
                               .ReturnsAsync(guid);
            personRepositoryMock.SetupSequence(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(guid))
                .Returns(Task.FromResult(""));

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_DefaultInitiator_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Outstanding;

            domainPurchaseOrder.CurrencyCode = null;
            domainPurchaseOrder.HostCountry = "CAN";
            domainPurchaseOrder.Type = "Eprocurement";
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);
            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.SetupSequence(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>()))
                               .Returns(Task.FromResult(guid)).Returns(Task.FromResult(guid)).Returns(Task.FromResult(""));
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_ShipToDestination_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Outstanding;

            domainPurchaseOrder.CurrencyCode = "USD1";
            domainPurchaseOrder.HostCountry = "CAN";
            domainPurchaseOrder.Type = "";
            domainPurchaseOrder.ShipToCode = "2";

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_FreeOnBoardType_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Paid;

            domainPurchaseOrder.CurrencyCode = "USD1";
            domainPurchaseOrder.Fob = "2";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_Country_Null_And_MiscCountry_NotNull()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Reconciled;
            domainPurchaseOrder.MiscCountry = "AUS";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_When_Country_IsoAlpha3Code_NotMatch()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrder.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Status = Dtos.EnumProperties.LineItemStatus.Voided;
            countries.FirstOrDefault().IsoAlpha3Code = "USA1";
            domainPurchaseOrder.MiscCountry = "USA";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";
            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_State_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            domainPurchaseOrder.AltShippingState = "AL";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";
            countries.FirstOrDefault().IsoAlpha3Code = "CAN";
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);
            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_Vendor_NullOrEmpty()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            countries.FirstOrDefault().IsoAlpha3Code = "AUS";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";
            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);
            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_VendorTerm_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            countries.FirstOrDefault().IsoAlpha3Code = "BRA";

            domainPurchaseOrder.VendorTerms = "2";

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_ApType_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            countries.FirstOrDefault().IsoAlpha3Code = "MEX";

            domainPurchaseOrder.ApType = "2";

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_CommodityCode_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            countries.FirstOrDefault().IsoAlpha3Code = "NLD";

            domainLineItems.FirstOrDefault().CommodityCode = "2";

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);
            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_Exception_UnitOfIssue_Exception()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            countries.FirstOrDefault().IsoAlpha3Code = "GBR";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";
            domainLineItems.FirstOrDefault().UnitOfIssue = "invalid";

            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());
            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);
            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_EntityToDto_CommerceTaxCodes_Null()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            countries.FirstOrDefault().IsoAlpha3Code = "BFA";
            domainPurchaseOrder.AltShippingCountry = "";
            domainPurchaseOrder.HostCountry = "";
            domainPurchaseOrder.AddLineItem(domainLineItems.FirstOrDefault());

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(null);

            await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_InvalidTaxCode_ApPurEntry()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            taxCodes = new List<CommerceTaxCode>() { new CommerceTaxCode("1l49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { AppurEntryFlag = false } };

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            var result = await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync_InvalidTaxCode_UseTax()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

            taxCodes = new List<CommerceTaxCode>() { new CommerceTaxCode("1l49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") { UseTaxFlag = It.IsAny<bool>() } };

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            var result = await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task PurchaseOrdersService_PutPurchaseOrdersAsync()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            createPurchaseOrder.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPurchaseOrder });

            purchaseOrderRepositoryMock.Setup(r => r.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            purchaseOrderRepositoryMock.Setup(r => r.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("00011");

            purchaseOrderRepositoryMock.Setup(r => r.UpdatePurchaseOrdersAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(domainPurchaseOrder);

            buyerRepositoryMock.Setup(r => r.GetBuyerGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);

        
            vendorsRepositoryMock.Setup(r => r.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            vendorsRepositoryMock.Setup(r => r.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            referenceDataRepositoryMock.Setup(r => r.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxCodes);

            GuidLookupResult guidLookupResult1 = new GuidLookupResult() { Entity = "ADDRESS", PrimaryKey = "1" };
            referenceDataRepositoryMock.Setup(r => r.GetGuidLookupResultFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookupResult1);

            var result = await purchaseOrderService.PutPurchaseOrdersAsync(purchaseOrder.Id, purchaseOrder);

            Assert.IsNotNull(result);
        }


    }


    #endregion
}