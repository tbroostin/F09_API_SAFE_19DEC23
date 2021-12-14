using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class ReceiveProcurementsServiceTests : GeneralLedgerCurrentUser
    {
        #region DECLARATION

        protected Domain.Entities.Role updateProcurementReceiving = new Domain.Entities.Role(1, "UPDATE.RECEIVING");

        private Mock<IReceiveProcurementsRepository> receiveProcurementsRepositoryMock;
        private Mock<IColleagueFinanceReferenceDataRepository> financeReferenceDataRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IStaffRepository> staffRepositoryMock;
        private Mock<ILogger> loggerMock;

        private ReceiveProcurementUser currentUserFactory;

        private ReceiveProcurementsService receiveProcurementsService;

        private ReceiveProcurementSummary receiveProcurementSummary;

        private IEnumerable<ReceiveProcurementSummary> receiveProcurementSummaryCollection;

        private Domain.ColleagueFinance.Entities.ReceiveProcurementSummary receiveProcurementSummaryDomainEntity;

        private IEnumerable<Domain.ColleagueFinance.Entities.ReceiveProcurementSummary> receiveProcurementSummaryDomainEntities;

        private ProcurementAcceptReturnItemInformationRequest procurementAcceptReturnItemInformationRequestDto;

        private Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationResponse procurementAcceptReturnItemInformationResponseEntity;

        private Staff staffEntity;

        private IEnumerable<Domain.Entities.Role> roles;

        private string personId = "0000143";

        //private Tuple<IEnumerable<Grant>, int> grantTuple;


        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {

            receiveProcurementsRepositoryMock = new Mock<IReceiveProcurementsRepository>();
            financeReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            staffRepositoryMock = new Mock<IStaffRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new ReceiveProcurementUser();

            InitializeTestData();

            InitializeTestMock();

            receiveProcurementsService = new ReceiveProcurementsService(receiveProcurementsRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, staffRepositoryMock.Object, loggerMock.Object);
        }

        private void InitializeTestData()
        {
            receiveProcurementSummary = new Dtos.ColleagueFinance.ReceiveProcurementSummary() { Id = "249", LineItemInformation = new List<Dtos.ColleagueFinance.LineItemSummary>() { new Dtos.ColleagueFinance.LineItemSummary() { ItemId = "1156", ItemDescription = "Franklin Planner Refill", ItemName = "FP2009 SU", ItemQuantity = decimal.Parse("2.000"), ItemUnitOfIssue = "EA" }, new Dtos.ColleagueFinance.LineItemSummary() { ItemId = "1243", ItemName = "Pens", ItemDescription = "Pens - Black", ItemQuantity = decimal.Parse("3.000"), ItemUnitOfIssue = "" } } };
            receiveProcurementSummaryCollection = new List<Dtos.ColleagueFinance.ReceiveProcurementSummary>() { receiveProcurementSummary };

            List<Domain.ColleagueFinance.Entities.LineItemSummary> lineItemSummary = new List<Domain.ColleagueFinance.Entities.LineItemSummary>()
            {
                new Domain.ColleagueFinance.Entities.LineItemSummary("1", "Item 1", decimal.Parse("2.000")){ ItemDescription = "Desc 1", ItemUnitOfIssue = "EA", ItemMSDSFlag = false },
                new Domain.ColleagueFinance.Entities.LineItemSummary("2", "Item 2", decimal.Parse("3.000")){ ItemDescription = "Desc 2", ItemUnitOfIssue = "PA" , ItemMSDSFlag = true}
            };

            List<Domain.ColleagueFinance.Entities.VendorContactSummary> vendorContactcs = new List<Domain.ColleagueFinance.Entities.VendorContactSummary>()
            {
                new Domain.ColleagueFinance.Entities.VendorContactSummary() { Email = "mail1@email.com", Name = "contact name 1", Phone = "674532", Title = "Ms"  },
                new Domain.ColleagueFinance.Entities.VendorContactSummary() { Email = "mail2@email.com", Name = "contact name 2", Phone = "6745321", Title = "Mr"  }
            };

            List<Domain.ColleagueFinance.Entities.RequisitionLinkSummary> requisitions = new List<Domain.ColleagueFinance.Entities.RequisitionLinkSummary>() {
                new Domain.ColleagueFinance.Entities.RequisitionLinkSummary("001","001")
            };

            Domain.ColleagueFinance.Entities.VendorInfo vendorInformation = new Domain.ColleagueFinance.Entities.VendorInfo();
            vendorInformation.Address = "Address123";
            vendorInformation.City = "City1";
            vendorInformation.Country = "USA";
            vendorInformation.State = "TX";
            vendorInformation.VendorId = "123";
            vendorInformation.VendorName = "Ven_Name";
            vendorInformation.VendorMiscName = "Ven_Misc_Name";
            vendorInformation.Zip = "789675";
            vendorInformation.VendorContacts = vendorContactcs;

            receiveProcurementSummaryDomainEntity = new Domain.ColleagueFinance.Entities.ReceiveProcurementSummary("123", "P0001", lineItemSummary, vendorInformation, requisitions);

            receiveProcurementSummaryDomainEntities = new List<Domain.ColleagueFinance.Entities.ReceiveProcurementSummary>() { receiveProcurementSummaryDomainEntity };

            List<LineItemSummary> lineItemSummaryDto = new List<LineItemSummary>()
            {
                new LineItemSummary() { ItemId = "1", ItemName = "Item 1", ItemDescription = "Desc 1", ItemQuantity = decimal.Parse("2.000"), ItemUnitOfIssue ="EA", ItemMSDSFlag = true },
                new LineItemSummary() { ItemId = "2", ItemName = "Item 2", ItemDescription = "Desc 2", ItemQuantity = decimal.Parse("3.000"), ItemUnitOfIssue ="PA", ItemMSDSFlag = false }

            };

            List<VendorContactSummary> vendorContactsDto = new List<VendorContactSummary>()
            {
                new VendorContactSummary() { Email = "mail1@email.com", Name = "contact name 1", Phone = "674532", Title = "Ms"  },
                new VendorContactSummary() { Email = "mail2@email.com", Name = "contact name 2", Phone = "6745321", Title = "Mr"  }
            };

            VendorInfo vendorInformationDto = new VendorInfo();
            vendorInformationDto.Address = "Address123";
            vendorInformationDto.City = "City1";
            vendorInformationDto.Country = "USA";
            vendorInformationDto.State = "TX";
            vendorInformationDto.VendorId = "123";
            vendorInformationDto.VendorName = "Ven_Name";
            vendorInformationDto.VendorMiscName = "Ven_Misc_Name";
            vendorInformationDto.Zip = "789675";
            vendorInformationDto.VendorContacts = vendorContactsDto;

            receiveProcurementSummary = new ReceiveProcurementSummary() { Id = "123", Number = "P001", LineItemInformation = lineItemSummaryDto, VendorInformation = vendorInformationDto };

            procurementAcceptReturnItemInformationRequestDto = new ProcurementAcceptReturnItemInformationRequest()
            {
                AcceptAll = false,
                ArrivedVia = "FX",
                IsPoFilterApplied = false,
                PackingSlip = "",
                StaffUserId = personId,
                ProcurementItemsInformation = new List<ProcurementItemInformation>() {
                    new ProcurementItemInformation() { PurchaseOrderId = "001", PurchaseOrderNumber="P001", ItemId = "001", ItemDescription = "Item1", QuantityOrdered  = decimal.Parse("10.000"), QuantityAccepted = decimal.Parse("10.000"), ItemMsdsFlag = false, ItemMsdsReceived= false, Vendor = "Vendor1", ConfirmationEmail="", QuantityRejected=null, ReOrder=false, ReturnAuthorizationNumber="", ReturnComments = "", ReturnDate=null, ReturnReason="", ReturnVia="" },
                    new ProcurementItemInformation() { PurchaseOrderId = "002", PurchaseOrderNumber="P002", ItemId = "002", ItemDescription = "Item2", QuantityOrdered  = decimal.Parse("2.000"), QuantityAccepted = decimal.Parse("2.000"), ItemMsdsFlag = false, ItemMsdsReceived= false, Vendor = "Vendor2", ConfirmationEmail="", QuantityRejected=null, ReOrder=false, ReturnAuthorizationNumber="", ReturnComments = "", ReturnDate=null, ReturnReason="", ReturnVia="" }
                }
            };

            procurementAcceptReturnItemInformationResponseEntity = new Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationResponse()
            {
                ErrorOccurred = false,
                ErrorMessages = new List<string>() { "" },
                WarningOccurred = false,
                WarningMessages = new List<string>() { "" },
                ProcurementItemsInformationResponse = new List<Domain.ColleagueFinance.Entities.ProcurementItemInformationResponse>() {
                new Domain.ColleagueFinance.Entities.ProcurementItemInformationResponse() { PurchaseOrderId = "001", PurchaseOrderNumber = "P001", ItemId = "001", ItemDescription = "Item1" },
                new Domain.ColleagueFinance.Entities.ProcurementItemInformationResponse() { PurchaseOrderId = "002", PurchaseOrderNumber = "P002", ItemId = "002", ItemDescription = "Item2" }
                }
            };
            

            receiveProcurementSummaryCollection = new List<ReceiveProcurementSummary>() { receiveProcurementSummary };

            staffEntity = new Staff("0000143", "Jef") { };

            roles = new List<Domain.Entities.Role>() { new Domain.Entities.Role(1, "UPDATE.RECEIVING") };

            roles.FirstOrDefault().AddPermission(new Permission(ColleagueFinancePermissionCodes.ViewUpdateProcurementReceiving));

        }

        private void InitializeTestMock()
        {
            updateProcurementReceiving.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewUpdateProcurementReceiving));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updateProcurementReceiving });
            roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
            staffRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(staffEntity);
            receiveProcurementsRepositoryMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(receiveProcurementSummaryDomainEntities);
            receiveProcurementsRepositoryMock.Setup(r => r.AcceptOrReturnProcurementItemsAsync(It.IsAny<Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationRequest>())).ReturnsAsync(procurementAcceptReturnItemInformationResponseEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {
            receiveProcurementsService = null;
            receiveProcurementsRepositoryMock = null;
            staffRepositoryMock = null;
            financeReferenceDataRepositoryMock = null;
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            configurationRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
        }

        #endregion

        #region TEST METHODS

        #region GET

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsService_GetReceiveProcurementsByPersonIdAsync_PeronId_Null()
        {
            await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsService_GetReceiveProcurementsByPersonIdAsync_PeronId_Empty()
        {
            await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync("");
        }
        
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ReceiveProcurementsService_GetReceiveProcurementsByPersonIdAsync_NotLoggedInUser()
        {
            await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync("0000456");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ReceiveProcurementsService_GetReceiveProcurementsByPersonIdAsync_NoStffRecord()
        {
            staffRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ReceiveProcurementsService_GetReceiveProcurementsByPersonIdAsync_PermissionsException()
        {
            roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"RECEIVING")
                    };

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        public async Task ReceiveProcurementsService_GetReceiveProcurementsByPersonIdAsync_NullResult()
        {
            receiveProcurementsRepositoryMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        public async Task ReceiveProcurementsService_GetReceiveProcurementsByPersonIdAsync()
        {
            var POId = "123";
            var receiveProcurementsDtos = await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync(personId);

            Assert.IsNotNull(receiveProcurementsDtos);
            Assert.AreEqual(receiveProcurementsDtos.ToList().Count, receiveProcurementSummaryDomainEntities.ToList().Count);

            var receiveProcurementsDto = receiveProcurementsDtos.Where(x => x.Id == POId).FirstOrDefault();
            var receiveProcurementSummaryDomainEntity = receiveProcurementSummaryDomainEntities.Where(x => x.Id == POId).FirstOrDefault();

            // Confirm that the data in the DTO matches the domain entity

            Assert.AreEqual(receiveProcurementsDto.Id, receiveProcurementSummaryDomainEntity.Id);
            Assert.AreEqual(receiveProcurementsDto.Number, receiveProcurementSummaryDomainEntity.Number);

            Assert.AreEqual(receiveProcurementsDto.LineItemInformation.ToList().Count, receiveProcurementSummaryDomainEntity.LineItemInformation.ToList().Count);

            for (int i = 0; i < receiveProcurementsDto.LineItemInformation.Count(); i++)
            {
                Assert.AreEqual(receiveProcurementsDto.LineItemInformation[i].ItemId, receiveProcurementSummaryDomainEntity.LineItemInformation[i].ItemId);
                Assert.AreEqual(receiveProcurementsDto.LineItemInformation[i].ItemName, receiveProcurementSummaryDomainEntity.LineItemInformation[i].ItemName);
                Assert.AreEqual(receiveProcurementsDto.LineItemInformation[i].ItemQuantity, receiveProcurementSummaryDomainEntity.LineItemInformation[i].ItemQuantity);
                Assert.AreEqual(receiveProcurementsDto.LineItemInformation[i].ItemDescription, receiveProcurementSummaryDomainEntity.LineItemInformation[i].ItemDescription);
                Assert.AreEqual(receiveProcurementsDto.LineItemInformation[i].ItemUnitOfIssue, receiveProcurementSummaryDomainEntity.LineItemInformation[i].ItemUnitOfIssue);
            }

            Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorId, receiveProcurementSummaryDomainEntity.VendorInformation.VendorId);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorName, receiveProcurementSummaryDomainEntity.VendorInformation.VendorName);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorMiscName, receiveProcurementSummaryDomainEntity.VendorInformation.VendorMiscName);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.Address, receiveProcurementSummaryDomainEntity.VendorInformation.Address);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.City, receiveProcurementSummaryDomainEntity.VendorInformation.City);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.Country, receiveProcurementSummaryDomainEntity.VendorInformation.Country);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.State, receiveProcurementSummaryDomainEntity.VendorInformation.State);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.Zip, receiveProcurementSummaryDomainEntity.VendorInformation.Zip);
            Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorContacts.ToList().Count, receiveProcurementSummaryDomainEntity.VendorInformation.VendorContacts.Count);

            for (int i = 0; i < receiveProcurementsDto.VendorInformation.VendorContacts.Count(); i++)
            {
                Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorContacts[i].Name, receiveProcurementSummaryDomainEntity.VendorInformation.VendorContacts[i].Name);
                Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorContacts[i].Phone, receiveProcurementSummaryDomainEntity.VendorInformation.VendorContacts[i].Phone);
                Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorContacts[i].Title, receiveProcurementSummaryDomainEntity.VendorInformation.VendorContacts[i].Title);
                Assert.AreEqual(receiveProcurementsDto.VendorInformation.VendorContacts[i].Email, receiveProcurementSummaryDomainEntity.VendorInformation.VendorContacts[i].Email);
            }


        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync_PermissionsException()
        {
            roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"RECEIVING")
                    };
            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);
            var result = await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestDto);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync_NotLoggedInUser()
        {
            procurementAcceptReturnItemInformationRequestDto.StaffUserId = "0000456";
            await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestDto);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync_NoStffRecord()
        {
            staffRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync_Body_Null()
        {
            await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync_StaffUserId_Null()
        {
            procurementAcceptReturnItemInformationRequestDto.StaffUserId = null;
            await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync_StaffUserId_Empty()
        {
            procurementAcceptReturnItemInformationRequestDto.StaffUserId = "";
            await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync_ItemInformation_Null()
        {
            procurementAcceptReturnItemInformationRequestDto.ProcurementItemsInformation = null;
            await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestDto);
        }

        [TestMethod]
        public async Task ReceiveProcurementsService_AcceptOrReturnProcurementItemsAsync()
        {
            var receiveProcurementsDto = await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestDto);

            Assert.IsNotNull(receiveProcurementsDto);
            
            // Confirm that the data in the DTO matches the domain entity

            Assert.AreEqual(receiveProcurementsDto.ErrorMessages, procurementAcceptReturnItemInformationResponseEntity.ErrorMessages);
            Assert.AreEqual(receiveProcurementsDto.ErrorOccurred, procurementAcceptReturnItemInformationResponseEntity.ErrorOccurred);
            Assert.AreEqual(receiveProcurementsDto.WarningMessages, procurementAcceptReturnItemInformationResponseEntity.WarningMessages);
            Assert.AreEqual(receiveProcurementsDto.WarningOccurred, procurementAcceptReturnItemInformationResponseEntity.WarningOccurred);

            Assert.AreEqual(receiveProcurementsDto.ProcurementItemsInformationResponse.ToList().Count, procurementAcceptReturnItemInformationResponseEntity.ProcurementItemsInformationResponse.ToList().Count);
        }

            #endregion

            #endregion
        }
}
