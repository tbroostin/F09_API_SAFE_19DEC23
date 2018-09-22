//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class FinancialAidFundClassificationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidFundClassificationsService> financialAidFundClassificationsServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialAidFundClassificationsController financialAidFundClassificationsController;      
        private IEnumerable<FinancialAidFundClassification> allFinancialAidFundClassifications;
        private List<Dtos.FinancialAidFundClassifications> financialAidFundClassificationsCollection;

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidFundClassificationsServiceMock = new Mock<IFinancialAidFundClassificationsService>();
            loggerMock = new Mock<ILogger>();
            financialAidFundClassificationsCollection = new List<Dtos.FinancialAidFundClassifications>();

            allFinancialAidFundClassifications  = new List<FinancialAidFundClassification>()
                {
                    new FinancialAidFundClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FinancialAidFundClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FinancialAidFundClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allFinancialAidFundClassifications)
            {
                var financialAidFundClassifications = new Ellucian.Colleague.Dtos.FinancialAidFundClassifications
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                financialAidFundClassificationsCollection.Add(financialAidFundClassifications);
            }

            financialAidFundClassificationsController = new FinancialAidFundClassificationsController(financialAidFundClassificationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidFundClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidFundClassificationsController = null;
            allFinancialAidFundClassifications = null;
            financialAidFundClassificationsCollection = null;
            loggerMock = null;
            financialAidFundClassificationsServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsController_GetFinancialAidFundClassifications_ValidateFields_Nocache()
        {
            financialAidFundClassificationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            financialAidFundClassificationsServiceMock.Setup(x => x.GetFinancialAidFundClassificationsAsync(false)).ReturnsAsync(financialAidFundClassificationsCollection);
       
            var sourceContexts = (await financialAidFundClassificationsController.GetFinancialAidFundClassificationsAsync()).ToList();
            Assert.AreEqual(financialAidFundClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidFundClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsController_GetFinancialAidFundClassifications_ValidateFields_Cache()
        {
            financialAidFundClassificationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            financialAidFundClassificationsServiceMock.Setup(x => x.GetFinancialAidFundClassificationsAsync(true)).ReturnsAsync(financialAidFundClassificationsCollection);

            var sourceContexts = (await financialAidFundClassificationsController.GetFinancialAidFundClassificationsAsync()).ToList();
            Assert.AreEqual(financialAidFundClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidFundClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsController_GetFinancialAidFundClassificationsByGuidAsync_ValidateFields()
        {
            var expected = financialAidFundClassificationsCollection.FirstOrDefault();
            financialAidFundClassificationsServiceMock.Setup(x => x.GetFinancialAidFundClassificationsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await financialAidFundClassificationsController.GetFinancialAidFundClassificationsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundClassificationsController_GetFinancialAidFundClassifications_Exception()
        {
            financialAidFundClassificationsServiceMock.Setup(x => x.GetFinancialAidFundClassificationsAsync(false)).Throws<Exception>();
            await financialAidFundClassificationsController.GetFinancialAidFundClassificationsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundClassificationsController_GetFinancialAidFundClassificationsByGuidAsync_Exception()
        {
            financialAidFundClassificationsServiceMock.Setup(x => x.GetFinancialAidFundClassificationsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await financialAidFundClassificationsController.GetFinancialAidFundClassificationsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundClassificationsController_PostFinancialAidFundClassificationsAsync_Exception()
        {
            await financialAidFundClassificationsController.PostFinancialAidFundClassificationsAsync(financialAidFundClassificationsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundClassificationsController_PutFinancialAidFundClassificationsAsync_Exception()
        {
            var sourceContext = financialAidFundClassificationsCollection.FirstOrDefault();
            await financialAidFundClassificationsController.PutFinancialAidFundClassificationsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundClassificationsController_DeleteFinancialAidFundClassificationsAsync_Exception()
        {
            await financialAidFundClassificationsController.DeleteFinancialAidFundClassificationsAsync(financialAidFundClassificationsCollection.FirstOrDefault().Id);
        }
    }
}