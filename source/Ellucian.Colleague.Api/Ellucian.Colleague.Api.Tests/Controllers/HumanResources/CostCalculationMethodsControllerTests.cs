//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class CostCalculationMethodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICostCalculationMethodsService> costCalculationMethodsServiceMock;
        private Mock<ILogger> loggerMock;
        private CostCalculationMethodsController costCalculationMethodsController;      
        private IEnumerable<Domain.HumanResources.Entities.CostCalculationMethod> allBdCalcMethods;
        private List<Dtos.CostCalculationMethods> costCalculationMethodsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            costCalculationMethodsServiceMock = new Mock<ICostCalculationMethodsService>();
            loggerMock = new Mock<ILogger>();
            costCalculationMethodsCollection = new List<Dtos.CostCalculationMethods>();

            allBdCalcMethods = new List<Domain.HumanResources.Entities.CostCalculationMethod>()
                {
                    new Domain.HumanResources.Entities.CostCalculationMethod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.CostCalculationMethod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.CostCalculationMethod("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allBdCalcMethods)
            {
                var costCalculationMethods = new Ellucian.Colleague.Dtos.CostCalculationMethods
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                costCalculationMethodsCollection.Add(costCalculationMethods);
            }

            costCalculationMethodsController = new CostCalculationMethodsController(costCalculationMethodsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            costCalculationMethodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            costCalculationMethodsController = null;
            allBdCalcMethods = null;
            costCalculationMethodsCollection = null;
            loggerMock = null;
            costCalculationMethodsServiceMock = null;
        }

        [TestMethod]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_ValidateFields_Nocache()
        {
            costCalculationMethodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(false)).ReturnsAsync(costCalculationMethodsCollection);
       
            var sourceContexts = (await costCalculationMethodsController.GetCostCalculationMethodsAsync()).ToList();
            Assert.AreEqual(costCalculationMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = costCalculationMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_ValidateFields_Cache()
        {
            costCalculationMethodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(true)).ReturnsAsync(costCalculationMethodsCollection);

            var sourceContexts = (await costCalculationMethodsController.GetCostCalculationMethodsAsync()).ToList();
            Assert.AreEqual(costCalculationMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = costCalculationMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_KeyNotFoundException()
        {
            //
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(false))
                .Throws<KeyNotFoundException>();
            await costCalculationMethodsController.GetCostCalculationMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_PermissionsException()
        {
            
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(false))
                .Throws<PermissionsException>();
            await costCalculationMethodsController.GetCostCalculationMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_ArgumentException()
        {
            
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(false))
                .Throws<ArgumentException>();
            await costCalculationMethodsController.GetCostCalculationMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_RepositoryException()
        {
            
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(false))
                .Throws<RepositoryException>();
            await costCalculationMethodsController.GetCostCalculationMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_IntegrationApiException()
        {
            
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(false))
                .Throws<IntegrationApiException>();
            await costCalculationMethodsController.GetCostCalculationMethodsAsync();
        }

        [TestMethod]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuidAsync_ValidateFields()
        {
            var expected = costCalculationMethodsCollection.FirstOrDefault();
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethods_Exception()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsAsync(false)).Throws<Exception>();
            await costCalculationMethodsController.GetCostCalculationMethodsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuidAsync_Exception()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuid_KeyNotFoundException()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuid_PermissionsException()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuid_ArgumentException()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuid_RepositoryException()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuid_IntegrationApiException()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_GetCostCalculationMethodsByGuid_Exception()
        {
            costCalculationMethodsServiceMock.Setup(x => x.GetCostCalculationMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await costCalculationMethodsController.GetCostCalculationMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_PostCostCalculationMethodsAsync_Exception()
        {
            await costCalculationMethodsController.PostCostCalculationMethodsAsync(costCalculationMethodsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_PutCostCalculationMethodsAsync_Exception()
        {
            var sourceContext = costCalculationMethodsCollection.FirstOrDefault();
            await costCalculationMethodsController.PutCostCalculationMethodsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCalculationMethodsController_DeleteCostCalculationMethodsAsync_Exception()
        {
            await costCalculationMethodsController.DeleteCostCalculationMethodsAsync(costCalculationMethodsCollection.FirstOrDefault().Id);
        }
    }
}