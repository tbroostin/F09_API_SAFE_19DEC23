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
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class BudgetCodesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IBudgetCodesService> budgetCodesServiceMock;
        private Mock<ILogger> loggerMock;
        private BudgetCodesController budgetCodesController;
        private IEnumerable<Domain.ColleagueFinance.Entities.Budget> allBudget;
        private List<Dtos.BudgetCodes> budgetCodesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            budgetCodesServiceMock = new Mock<IBudgetCodesService>();
            loggerMock = new Mock<ILogger>();
            budgetCodesCollection = new List<Dtos.BudgetCodes>();

            allBudget = new List<Domain.ColleagueFinance.Entities.Budget>()
                {
                    new Domain.ColleagueFinance.Entities.Budget()
                    { BudgetCodeGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                        RecordKey = "2015FY",
                        Title = "2015 Operating Budget" },
                    new Domain.ColleagueFinance.Entities.Budget()
                    {
                        BudgetCodeGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                        RecordKey = "2016FY",
                        Title = "2016 Operating Budget" },
                    new Domain.ColleagueFinance.Entities.Budget()
                    { BudgetCodeGuid =  "d2253ac7-9931-4560-b42f-1fccd43c952e",
                        RecordKey = "2017FY",
                        Title = "2017 Operating Budget" }
                };

            foreach (var source in allBudget)
            {
                var budgetCodes = new Ellucian.Colleague.Dtos.BudgetCodes
                {
                    Id = source.BudgetCodeGuid,
                    Code = source.RecordKey,
                    Title = source.Title,
                    Description = null
                };
                budgetCodesCollection.Add(budgetCodes);
            }

            budgetCodesController = new BudgetCodesController(budgetCodesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            budgetCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            budgetCodesController = null;
            allBudget = null;
            budgetCodesCollection = null;
            loggerMock = null;
            budgetCodesServiceMock = null;
        }

        [TestMethod]
        public async Task BudgetCodesController_GetBudgetCodes_ValidateFields_Nocache()
        {
            budgetCodesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(false)).ReturnsAsync(budgetCodesCollection);

            var sourceContexts = (await budgetCodesController.GetBudgetCodesAsync()).ToList();
            Assert.AreEqual(budgetCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = budgetCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task BudgetCodesController_GetBudgetCodes_ValidateFields_Cache()
        {
            budgetCodesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(true)).ReturnsAsync(budgetCodesCollection);

            var sourceContexts = (await budgetCodesController.GetBudgetCodesAsync()).ToList();
            Assert.AreEqual(budgetCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = budgetCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodes_KeyNotFoundException()
        {
            //
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(false))
                .Throws<KeyNotFoundException>();
            await budgetCodesController.GetBudgetCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodes_PermissionsException()
        {

            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(false))
                .Throws<PermissionsException>();
            await budgetCodesController.GetBudgetCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodes_ArgumentException()
        {

            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(false))
                .Throws<ArgumentException>();
            await budgetCodesController.GetBudgetCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodes_RepositoryException()
        {

            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(false))
                .Throws<RepositoryException>();
            await budgetCodesController.GetBudgetCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodes_IntegrationApiException()
        {

            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(false))
                .Throws<IntegrationApiException>();
            await budgetCodesController.GetBudgetCodesAsync();
        }

        [TestMethod]
        public async Task BudgetCodesController_GetBudgetCodesByGuidAsync_ValidateFields()
        {
            var expected = budgetCodesCollection.FirstOrDefault();
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await budgetCodesController.GetBudgetCodesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodes_Exception()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesAsync(false)).Throws<Exception>();
            await budgetCodesController.GetBudgetCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodesByGuidAsync_Exception()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await budgetCodesController.GetBudgetCodesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodesByGuid_KeyNotFoundException()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await budgetCodesController.GetBudgetCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodesByGuid_PermissionsException()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await budgetCodesController.GetBudgetCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodesByGuid_ArgumentException()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await budgetCodesController.GetBudgetCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodesByGuid_RepositoryException()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await budgetCodesController.GetBudgetCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodesByGuid_IntegrationApiException()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await budgetCodesController.GetBudgetCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_GetBudgetCodesByGuid_Exception()
        {
            budgetCodesServiceMock.Setup(x => x.GetBudgetCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await budgetCodesController.GetBudgetCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_PostBudgetCodesAsync_Exception()
        {
            await budgetCodesController.PostBudgetCodesAsync(budgetCodesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_PutBudgetCodesAsync_Exception()
        {
            var sourceContext = budgetCodesCollection.FirstOrDefault();
            await budgetCodesController.PutBudgetCodesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetCodesController_DeleteBudgetCodesAsync_Exception()
        {
            await budgetCodesController.DeleteBudgetCodesAsync(budgetCodesCollection.FirstOrDefault().Id);
        }
    }
}