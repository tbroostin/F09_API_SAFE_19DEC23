//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Api.Controllers.BudgetManagement;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;

namespace Ellucian.Colleague.Api.Tests.Controllers.BudgetManagement
{
    [TestClass]
    public class BudgetPhasesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IBudgetPhasesService> budgetPhasesServiceMock;
        private Mock<ILogger> loggerMock;
        private BudgetPhasesController budgetPhasesController;
        private IEnumerable<Domain.BudgetManagement.Entities.Budget> allBudget;
        private List<Dtos.BudgetPhases> budgetPhasesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            budgetPhasesServiceMock = new Mock<IBudgetPhasesService>();
            loggerMock = new Mock<ILogger>();
            budgetPhasesCollection = new List<Dtos.BudgetPhases>();

            allBudget = new List<Domain.BudgetManagement.Entities.Budget>()
            {
                    new Domain.BudgetManagement.Entities.Budget()
                    { BudgetPhaseGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                        RecordKey = "2015FY",
                        Title = "2015 Operating Budget" },
                    new Domain.BudgetManagement.Entities.Budget()
                    {
                        BudgetPhaseGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                        RecordKey = "2016FY",
                        Title = "2016 Operating Budget" },
                    new Domain.BudgetManagement.Entities.Budget()
                    { BudgetPhaseGuid =  "d2253ac7-9931-4560-b42f-1fccd43c952e",
                        RecordKey = "2017FY",
                        Title = "2017 Operating Budget" }
            };

            foreach (var source in allBudget)
            {
                var budgetPhases = new Ellucian.Colleague.Dtos.BudgetPhases
                {
                    Id = source.BudgetPhaseGuid,
                    Code = source.RecordKey,
                    Title = source.Title,
                    Description = null
                };
                budgetPhasesCollection.Add(budgetPhases);
            }

            budgetPhasesController = new BudgetPhasesController(budgetPhasesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            budgetPhasesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            budgetPhasesController = null;
            allBudget = null;
            budgetPhasesCollection = null;
            loggerMock = null;
            budgetPhasesServiceMock = null;
        }

        [TestMethod]
        public async Task BudgetPhasesController_GetBudgetPhases_ValidateFields_Nocache()
        {
            budgetPhasesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), false)).ReturnsAsync(budgetPhasesCollection);

            var sourceContexts = (await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>())).ToList();
            Assert.AreEqual(budgetPhasesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = budgetPhasesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task BudgetPhasesController_GetBudgetPhases_ValidateFields_Cache()
        {
            budgetPhasesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), true)).ReturnsAsync(budgetPhasesCollection);

            var sourceContexts = (await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>())).ToList();
            Assert.AreEqual(budgetPhasesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = budgetPhasesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhases_KeyNotFoundException()
        {
            //
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), false))
                .Throws<KeyNotFoundException>();
            await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhases_PermissionsException()
        {

            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), false))
                .Throws<PermissionsException>();
            await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhases_ArgumentException()
        {

            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), false))
                .Throws<ArgumentException>();
            await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhases_RepositoryException()
        {

            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), false))
                .Throws<RepositoryException>();
            await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhases_IntegrationApiException()
        {

            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), false))
                .Throws<IntegrationApiException>();
            await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuidAsync_ValidateFields()
        {
            var expected = budgetPhasesCollection.FirstOrDefault();
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await budgetPhasesController.GetBudgetPhasesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhases_Exception()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesAsync(It.IsAny<string>(), false)).Throws<Exception>();
            await budgetPhasesController.GetBudgetPhasesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuidAsync_Exception()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await budgetPhasesController.GetBudgetPhasesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuid_KeyNotFoundException()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await budgetPhasesController.GetBudgetPhasesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuid_PermissionsException()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await budgetPhasesController.GetBudgetPhasesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuid_ArgumentException()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await budgetPhasesController.GetBudgetPhasesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuid_RepositoryException()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await budgetPhasesController.GetBudgetPhasesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuid_IntegrationApiException()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await budgetPhasesController.GetBudgetPhasesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_GetBudgetPhasesByGuid_Exception()
        {
            budgetPhasesServiceMock.Setup(x => x.GetBudgetPhasesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await budgetPhasesController.GetBudgetPhasesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_PostBudgetPhasesAsync_Exception()
        {
            await budgetPhasesController.PostBudgetPhasesAsync(budgetPhasesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_PutBudgetPhasesAsync_Exception()
        {
            var sourceContext = budgetPhasesCollection.FirstOrDefault();
            await budgetPhasesController.PutBudgetPhasesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhasesController_DeleteBudgetPhasesAsync_Exception()
        {
            await budgetPhasesController.DeleteBudgetPhasesAsync(budgetPhasesCollection.FirstOrDefault().Id);
        }
    }
}