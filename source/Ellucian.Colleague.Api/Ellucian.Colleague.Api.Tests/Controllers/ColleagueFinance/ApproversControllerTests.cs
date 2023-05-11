//Copyright 2021 Ellucian Company L.P.and its affiliates.
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
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
    public class ApproversControllerTests
    {

        #region DECLARATION
        public TestContext TestContext { get; set; }
        private Mock<IApproverService> approverServiceMock;
        private Mock<ILogger> loggerMock;
        private ApproversController approverController;
        private Dtos.ColleagueFinance.NextApprover nextApprover;
        private List<Dtos.ColleagueFinance.NextApprover> nextApproverCollection;
        private string personId = "0000100";
        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            approverServiceMock = new Mock<IApproverService>();
            loggerMock = new Mock<ILogger>();
            nextApproverCollection = new List<Dtos.ColleagueFinance.NextApprover>();

            InitializeTestData();

            //approverServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            approverServiceMock.Setup(s => s.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(nextApproverCollection);

            approverController = new ApproversController(approverServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            approverController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            approverController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            approverController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(nextApprover));
        }

        [TestCleanup]
        public void Cleanup()
        {
            approverController = null;
            approverServiceMock = null;
            loggerMock = null;
            TestContext = null;
        }

        private void InitializeTestData()
        {
            nextApprover = new Dtos.ColleagueFinance.NextApprover() { NextApproverId = "ABC", NextApproverPersonId = "0000123", NextApproverName = "John Doe" };
            nextApproverCollection = new List<Dtos.ColleagueFinance.NextApprover>() { nextApprover };
        }

        #endregion

        #region TEST METHODS

        #region GET

        [TestMethod]
        public async Task ApproverController_GetNextApproverByKeywordAsync()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var nextApprovers = await approverController.GetNextApproverByKeywordAsync(personId);
            Assert.AreEqual(nextApproverCollection.ToList().Count, expected.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_GetNextApproverByKeywordAsync_Keyword_AsNull()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var nextApprovers = await approverController.GetNextApproverByKeywordAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_GetNextApproverByKeywordAsync_ArgumentNullException()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            var nextApprovers = await approverController.GetNextApproverByKeywordAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_GetNextApproverByKeywordAsync_PermissionException()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            var nextApprovers = await approverController.GetNextApproverByKeywordAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_GetNextApproverByKeywordAsync_KeyNotFoundException()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            var nextApprovers = await approverController.GetNextApproverByKeywordAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_GetNextApproverByKeywordAsync_Exception()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var nextApprovers = await approverController.GetNextApproverByKeywordAsync(personId);
        }

        #endregion

        #region QAPI

        [TestMethod]
        public async Task ApproverController_QueryNextApproverByKeywordAsync()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var nextApprovers = await approverController.QueryNextApproverByKeywordAsync(new KeywordSearchCriteria() { Keyword = personId });
            Assert.AreEqual(nextApproverCollection.ToList().Count, expected.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_QueryNextApproverByKeywordAsync_Keyword_AsNull()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var nextApprovers = await approverController.QueryNextApproverByKeywordAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_QueryNextApproverByKeywordAsync_ArgumentNullException()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            var nextApprovers = await approverController.QueryNextApproverByKeywordAsync(new KeywordSearchCriteria() { Keyword = personId });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_QueryNextApproverByKeywordAsync_PermissionException()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            var nextApprovers = await approverController.QueryNextApproverByKeywordAsync(new KeywordSearchCriteria() { Keyword = personId });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_QueryNextApproverByKeywordAsync_KeyNotFoundException()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            var nextApprovers = await approverController.QueryNextApproverByKeywordAsync(new KeywordSearchCriteria() { Keyword = personId });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApproverController_QueryNextApproverByKeywordAsync_Exception()
        {
            var expected = nextApproverCollection.AsEnumerable();
            approverServiceMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var nextApprovers = await approverController.QueryNextApproverByKeywordAsync(new KeywordSearchCriteria() { Keyword = personId });
        }

        #endregion

        #endregion
    }
}
