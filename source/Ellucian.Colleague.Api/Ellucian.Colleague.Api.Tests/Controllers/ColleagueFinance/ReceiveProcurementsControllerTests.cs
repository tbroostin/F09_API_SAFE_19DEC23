using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ReceiveProcurementsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        ///

        #region DECLARATION
        public TestContext TestContext { get; set; }
        private Mock<IReceiveProcurementsService> receiveProcurementsServiceMock;
        private Mock<ILogger> loggerMock;
        private ReceiveProcurementsController receiveProcurementsController;
        private IEnumerable<Domain.ColleagueFinance.Entities.ReceiveProcurementSummary> allReceiveProcurementSummaries;
        private Dtos.ColleagueFinance.ReceiveProcurementSummary receiveProcurementSummary;
        private List<Dtos.ColleagueFinance.ReceiveProcurementSummary> receiveProcurementSummaryCollection;
        private Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationResponse procurementAcceptReturnItemInformationResponse;
        private Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest procurementAcceptReturnItemInformationRequest;
        private string personId = "0000100";

        #endregion

        #region TEST SETUP
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            receiveProcurementsServiceMock = new Mock<IReceiveProcurementsService>();
            loggerMock = new Mock<ILogger>();
            receiveProcurementSummaryCollection = new List<Dtos.ColleagueFinance.ReceiveProcurementSummary>();

            InitializeTestData();

            receiveProcurementsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            receiveProcurementsServiceMock.Setup(s => s.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(receiveProcurementSummaryCollection);

            receiveProcurementsServiceMock.Setup(s => s.AcceptOrReturnProcurementItemsAsync(It.IsAny<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest>())).ReturnsAsync(procurementAcceptReturnItemInformationResponse);

            receiveProcurementsController = new ReceiveProcurementsController(receiveProcurementsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            receiveProcurementsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            receiveProcurementsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            receiveProcurementsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(receiveProcurementSummary));
        }

        [TestCleanup]
        public void Cleanup()
        {
            receiveProcurementsController = null;
            receiveProcurementsServiceMock = null;
            loggerMock = null;
            TestContext = null;
        }

        private void InitializeTestData()
        {
            receiveProcurementSummary = new Dtos.ColleagueFinance.ReceiveProcurementSummary() { Id = "249", LineItemInformation = new List<Dtos.ColleagueFinance.LineItemSummary>() { new Dtos.ColleagueFinance.LineItemSummary() { ItemId = "1156", ItemDescription = "Franklin Planner Refill", ItemName = "FP2009 SU", ItemQuantity = decimal.Parse("2.000"), ItemUnitOfIssue = "EA" }, new Dtos.ColleagueFinance.LineItemSummary() { ItemId = "1243", ItemName = "Pens", ItemDescription = "Pens - Black", ItemQuantity = decimal.Parse("3.000"), ItemUnitOfIssue = "" } } };
            receiveProcurementSummaryCollection = new List<Dtos.ColleagueFinance.ReceiveProcurementSummary>() { receiveProcurementSummary };

            procurementAcceptReturnItemInformationResponse = new Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationResponse()
            {
                ErrorMessages = new List<string>() { "" },
                ErrorOccurred = false,
                WarningMessages = new List<string>() { "" },
                WarningOccurred = false,
                ProcurementItemsInformationResponse = new List<Dtos.ColleagueFinance.ProcurementItemInformationResponse>() {
                     new Dtos.ColleagueFinance.ProcurementItemInformationResponse() { PurchaseOrderId = "1", PurchaseOrderNumber="P001", ItemId = "001", ItemDescription="Item1"  },
                     new Dtos.ColleagueFinance.ProcurementItemInformationResponse() { PurchaseOrderId = "2", PurchaseOrderNumber="P002", ItemId = "002", ItemDescription="Item2"  }
                 }
            };

            procurementAcceptReturnItemInformationRequest = new Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest()
            {
                AcceptAll = false,
                ArrivedVia = "",
                IsPoFilterApplied = false,
                PackingSlip = "",
                StaffUserId = personId,
                ProcurementItemsInformation = new List<Dtos.ColleagueFinance.ProcurementItemInformation>() {
                     new Dtos.ColleagueFinance.ProcurementItemInformation() { PurchaseOrderId = "001", PurchaseOrderNumber = "P001", ItemId = "001", ItemDescription = "Item1", QuantityOrdered = decimal.Parse("10.000"), QuantityAccepted= decimal.Parse("10.000"), Vendor ="Vendor1" },
                     new Dtos.ColleagueFinance.ProcurementItemInformation() { PurchaseOrderId = "002", PurchaseOrderNumber = "P002", ItemId = "002", ItemDescription = "Item2", QuantityOrdered = decimal.Parse("5.000"), QuantityAccepted= decimal.Parse("5.000"), Vendor ="Vendor2" }
                }
            };

        }
    

        #endregion

        #region TEST METHODS

        #region GET
        [TestMethod]
        public async Task ReceiveProcurementsController_GetReceiveProcurementsByPersonIdAsync()
        {
            var expected = receiveProcurementSummaryCollection.AsEnumerable();
            receiveProcurementsServiceMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var receiveProcurements = await receiveProcurementsController.GetReceiveProcurementsByPersonIdAsync(personId);
            Assert.AreEqual(receiveProcurementSummaryCollection.ToList().Count, expected.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_GetReceiveProcurementsByPersonIdAsync_PermissionException()
        {
            receiveProcurementsServiceMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            
            await receiveProcurementsController.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_GetReceiveProcurementsByPersonIdAsync_PersonId_Null()
        {
            await receiveProcurementsController.GetReceiveProcurementsByPersonIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_GetReceiveProcurementsByPersonIdAsync_ArgumentNullException()
        {
            receiveProcurementsServiceMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await receiveProcurementsController.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_GetReceiveProcurementsByPersonIdAsync_Exception()
        {
            receiveProcurementsServiceMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await receiveProcurementsController.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_GetReceiveProcurementsByPersonIdAsync_KeyNotFoundException()
        {
            receiveProcurementsServiceMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await receiveProcurementsController.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_GetReceiveProcurementsByPersonIdAsync_ApplicationException()
        {
            receiveProcurementsServiceMock.Setup(r => r.GetReceiveProcurementsByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
            await receiveProcurementsController.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        #endregion
        
        #region POST 

        [TestMethod]
        public async Task ReceiveProcurementsController_PostAcceptOrReturnProcurementItemsAsync_Valid()
        {
            receiveProcurementsServiceMock.Setup(x => x.AcceptOrReturnProcurementItemsAsync(It.IsAny<Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest>())).ReturnsAsync(procurementAcceptReturnItemInformationResponse);
            var response = await receiveProcurementsController.PostAcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequest);
            Assert.IsNotNull(response);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_PostAcceptOrReturnProcurementItemsAsync_NullBody()
        {
            await receiveProcurementsController.PostAcceptOrReturnProcurementItemsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_PostAcceptOrReturnProcurementItemsAsync_PermissionsException()
        {
            receiveProcurementsServiceMock.Setup(x => x.AcceptOrReturnProcurementItemsAsync(It.IsAny<Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest>())).ThrowsAsync(new PermissionsException());
            await receiveProcurementsController.PostAcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_PostAcceptOrReturnProcurementItemsAsync_ArgumentNullException()
        {
            receiveProcurementsServiceMock.Setup(x => x.AcceptOrReturnProcurementItemsAsync(It.IsAny<Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest>())).ThrowsAsync(new ArgumentNullException());
            await receiveProcurementsController.PostAcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ReceiveProcurementsController_PostAcceptOrReturnProcurementItemsAsync_Exception()
        {
            receiveProcurementsServiceMock.Setup(x => x.AcceptOrReturnProcurementItemsAsync(It.IsAny<Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest>())).ThrowsAsync(new Exception());
            await receiveProcurementsController.PostAcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequest);
        }

        #endregion

        #endregion
    }
}