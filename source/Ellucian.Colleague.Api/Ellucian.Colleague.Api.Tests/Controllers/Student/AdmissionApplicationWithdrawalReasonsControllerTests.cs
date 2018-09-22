//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionApplicationWithdrawalReasonsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionApplicationWithdrawalReasonsService> admissionApplicationWithdrawalReasonsServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionApplicationWithdrawalReasonsController admissionApplicationWithdrawalReasonsController;
        private IEnumerable<WithdrawReason> allWithdrawReasons;
        private List<Dtos.AdmissionApplicationWithdrawalReasons> admissionApplicationWithdrawalReasonsCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionApplicationWithdrawalReasonsServiceMock = new Mock<IAdmissionApplicationWithdrawalReasonsService>();
            loggerMock = new Mock<ILogger>();
            admissionApplicationWithdrawalReasonsCollection = new List<Dtos.AdmissionApplicationWithdrawalReasons>();

            allWithdrawReasons = new List<WithdrawReason>()
                {
                    new Domain.Student.Entities.WithdrawReason("761597be-0a12-4aa8-8ffe-afc04b62da41", "AC", "Academic Reasons"),
                    new Domain.Student.Entities.WithdrawReason("8cc60bb6-1e0e-45f1-bf10-b53d6809275e", "FP", "Financial Problems"),
                    new Domain.Student.Entities.WithdrawReason("6196cc8c-6e2c-4bb5-8859-b2553b24c772", "MILIT", "Serve In The Armed Forces"),
              };

            foreach (var source in allWithdrawReasons)
            {
                var admissionApplicationWithdrawalReasons = new Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionApplicationWithdrawalReasonsCollection.Add(admissionApplicationWithdrawalReasons);
            }

            admissionApplicationWithdrawalReasonsController = new AdmissionApplicationWithdrawalReasonsController(admissionApplicationWithdrawalReasonsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            admissionApplicationWithdrawalReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionApplicationWithdrawalReasonsController = null;
            allWithdrawReasons = null;
            admissionApplicationWithdrawalReasonsCollection = null;
            loggerMock = null;
            admissionApplicationWithdrawalReasonsServiceMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsController_GetAdmissionApplicationWithdrawalReasons_ValidateFields_Nocache()
        {
            admissionApplicationWithdrawalReasonsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            admissionApplicationWithdrawalReasonsServiceMock.Setup(x => x.GetAdmissionApplicationWithdrawalReasonsAsync(false)).ReturnsAsync(admissionApplicationWithdrawalReasonsCollection);

            var sourceContexts = (await admissionApplicationWithdrawalReasonsController.GetAdmissionApplicationWithdrawalReasonsAsync()).ToList();
            Assert.AreEqual(admissionApplicationWithdrawalReasonsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationWithdrawalReasonsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsController_GetAdmissionApplicationWithdrawalReasons_ValidateFields_Cache()
        {
            admissionApplicationWithdrawalReasonsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            admissionApplicationWithdrawalReasonsServiceMock.Setup(x => x.GetAdmissionApplicationWithdrawalReasonsAsync(true)).ReturnsAsync(admissionApplicationWithdrawalReasonsCollection);

            var sourceContexts = (await admissionApplicationWithdrawalReasonsController.GetAdmissionApplicationWithdrawalReasonsAsync()).ToList();
            Assert.AreEqual(admissionApplicationWithdrawalReasonsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationWithdrawalReasonsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsController_GetAdmissionApplicationWithdrawalReasonsByGuidAsync_ValidateFields()
        {
            var expected = admissionApplicationWithdrawalReasonsCollection.FirstOrDefault();
            admissionApplicationWithdrawalReasonsServiceMock.Setup(x => x.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await admissionApplicationWithdrawalReasonsController.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationWithdrawalReasonsController_GetAdmissionApplicationWithdrawalReasons_Exception()
        {
            admissionApplicationWithdrawalReasonsServiceMock.Setup(x => x.GetAdmissionApplicationWithdrawalReasonsAsync(false)).Throws<Exception>();
            await admissionApplicationWithdrawalReasonsController.GetAdmissionApplicationWithdrawalReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationWithdrawalReasonsController_GetAdmissionApplicationWithdrawalReasonsByGuidAsync_Exception()
        {
            admissionApplicationWithdrawalReasonsServiceMock.Setup(x => x.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionApplicationWithdrawalReasonsController.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationWithdrawalReasonsController_PostAdmissionApplicationWithdrawalReasonsAsync_Exception()
        {
            await admissionApplicationWithdrawalReasonsController.PostAdmissionApplicationWithdrawalReasonsAsync(admissionApplicationWithdrawalReasonsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationWithdrawalReasonsController_PutAdmissionApplicationWithdrawalReasonsAsync_Exception()
        {
            var sourceContext = admissionApplicationWithdrawalReasonsCollection.FirstOrDefault();
            await admissionApplicationWithdrawalReasonsController.PutAdmissionApplicationWithdrawalReasonsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationWithdrawalReasonsController_DeleteAdmissionApplicationWithdrawalReasonsAsync_Exception()
        {
            await admissionApplicationWithdrawalReasonsController.DeleteAdmissionApplicationWithdrawalReasonsAsync(admissionApplicationWithdrawalReasonsCollection.FirstOrDefault().Id);
        }
    }
}