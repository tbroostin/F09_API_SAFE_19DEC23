//Copyright 2020 Ellucian Company L.P.and its affiliates.
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class InitiatorControllerTests
    {

        #region DECLARATION
        public TestContext TestContext { get; set; }
        private Mock<IInitiatorService> initiatorServiceMock;
        private Mock<ILogger> loggerMock;
        private InitiatorController initiatorController;
        private Dtos.ColleagueFinance.Initiator initiator;
        private List<Dtos.ColleagueFinance.Initiator> initiatorCollection;
        private string personId = "0000100";
        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            initiatorServiceMock = new Mock<IInitiatorService>();
            loggerMock = new Mock<ILogger>();
            initiatorCollection = new List<Dtos.ColleagueFinance.Initiator>();

            InitializeTestData();

            initiatorServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            initiatorServiceMock.Setup(s => s.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ReturnsAsync(initiatorCollection);

            initiatorController = new InitiatorController(initiatorServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            initiatorController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            initiatorController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            initiatorController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(initiator));
        }

        [TestCleanup]
        public void Cleanup()
        {
            initiatorController = null;
            initiatorServiceMock = null;
            loggerMock = null;
            TestContext = null;
        }

        private void InitializeTestData()
        {
            initiator = new Dtos.ColleagueFinance.Initiator() { Code = "ABC", Id = "0000123", Name = "John Doe" };
            initiatorCollection = new List<Dtos.ColleagueFinance.Initiator>() { initiator };
        }

        #endregion

        #region TEST METHODS

            #region GET

        [TestMethod]
        public async Task InitiatorController_GetInitiatorByKeywordAsync()
        {
            var expected = initiatorCollection.AsEnumerable();
            initiatorServiceMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var initiators = await initiatorController.GetInitiatorByKeywordAsync(personId);
            Assert.AreEqual(initiatorCollection.ToList().Count, expected.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InitiatorController_GetInitiatorByKeywordAsync_Keyword_AsNull()
        {
            var expected = initiatorCollection.AsEnumerable();
            initiatorServiceMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var initiators = await initiatorController.GetInitiatorByKeywordAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InitiatorController_GetInitiatorByKeywordAsync_ArgumentNullException()
        {
            var expected = initiatorCollection.AsEnumerable();
            initiatorServiceMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            var initiators = await initiatorController.GetInitiatorByKeywordAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InitiatorController_GetInitiatorByKeywordAsync_PermissionException()
        {
            var expected = initiatorCollection.AsEnumerable();
            initiatorServiceMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            var initiators = await initiatorController.GetInitiatorByKeywordAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InitiatorController_GetInitiatorByKeywordAsync_KeyNotFoundException()
        {
            var expected = initiatorCollection.AsEnumerable();
            initiatorServiceMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            var initiators = await initiatorController.GetInitiatorByKeywordAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InitiatorController_GetInitiatorByKeywordAsync_Exception()
        {
            var expected = initiatorCollection.AsEnumerable();
            initiatorServiceMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var initiators = await initiatorController.GetInitiatorByKeywordAsync(personId);
        }

        #endregion

        #endregion

    }
}
