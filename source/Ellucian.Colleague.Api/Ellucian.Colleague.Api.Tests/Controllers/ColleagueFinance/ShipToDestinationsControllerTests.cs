//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ShipToDestinationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IShipToDestinationsService> shipToDestinationsServiceMock;
        private Mock<ILogger> loggerMock;
        private ShipToDestinationsController shipToDestinationsController;
        private IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination> allShipToCodes;
        private IEnumerable<Domain.ColleagueFinance.Entities.ShipToCode> allShipToCodeList;
        private List<Dtos.ShipToDestinations> shipToDestinationsCollection;
        private List<ShipToCode> shipToCodesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            shipToDestinationsServiceMock = new Mock<IShipToDestinationsService>();
            loggerMock = new Mock<ILogger>();
            shipToDestinationsCollection = new List<Dtos.ShipToDestinations>();

            allShipToCodes = new List<Domain.ColleagueFinance.Entities.ShipToDestination>()
                {
                    new Domain.ColleagueFinance.Entities.ShipToDestination("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.ShipToDestination("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.ShipToDestination("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allShipToCodes)
            {
                var shipToDestinations = new Ellucian.Colleague.Dtos.ShipToDestinations
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                shipToDestinationsCollection.Add(shipToDestinations);
            }

            shipToCodesCollection = new List<ShipToCode>();

            allShipToCodeList = new List<Domain.ColleagueFinance.Entities.ShipToCode>(){
                    new Domain.ColleagueFinance.Entities.ShipToCode("CD","Datatel - Central Dist. Office"),
                    new Domain.ColleagueFinance.Entities.ShipToCode("DT","Datatel - Downtown"),
                    new Domain.ColleagueFinance.Entities.ShipToCode("EC","Datatel - Extension Center"),
                    new Domain.ColleagueFinance.Entities.ShipToCode("MC","Datatel - Main Campus")
            };
            foreach (var source in allShipToCodeList)
            {
                var shipToCode = new ShipToCode
                {
                    Code = source.Code,
                    Description = source.Description
                };
                shipToCodesCollection.Add(shipToCode);
            }

            shipToDestinationsController = new ShipToDestinationsController(shipToDestinationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            shipToDestinationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            shipToDestinationsController = null;
            allShipToCodes = null;
            shipToDestinationsCollection = null;
            loggerMock = null;
            shipToDestinationsServiceMock = null;
        }

        [TestMethod]
        public async Task ShipToDestinationsController_GetShipToDestinations_ValidateFields_Nocache()
        {
            shipToDestinationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(false)).ReturnsAsync(shipToDestinationsCollection);

            var sourceContexts = (await shipToDestinationsController.GetShipToDestinationsAsync()).ToList();
            Assert.AreEqual(shipToDestinationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = shipToDestinationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task ShipToDestinationsController_GetShipToDestinations_ValidateFields_Cache()
        {
            shipToDestinationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(true)).ReturnsAsync(shipToDestinationsCollection);

            var sourceContexts = (await shipToDestinationsController.GetShipToDestinationsAsync()).ToList();
            Assert.AreEqual(shipToDestinationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = shipToDestinationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinations_KeyNotFoundException()
        {
            //
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(false))
                .Throws<KeyNotFoundException>();
            await shipToDestinationsController.GetShipToDestinationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinations_PermissionsException()
        {

            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(false))
                .Throws<PermissionsException>();
            await shipToDestinationsController.GetShipToDestinationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinations_ArgumentException()
        {

            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(false))
                .Throws<ArgumentException>();
            await shipToDestinationsController.GetShipToDestinationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinations_RepositoryException()
        {

            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(false))
                .Throws<RepositoryException>();
            await shipToDestinationsController.GetShipToDestinationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinations_IntegrationApiException()
        {

            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(false))
                .Throws<IntegrationApiException>();
            await shipToDestinationsController.GetShipToDestinationsAsync();
        }

        [TestMethod]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuidAsync_ValidateFields()
        {
            var expected = shipToDestinationsCollection.FirstOrDefault();
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await shipToDestinationsController.GetShipToDestinationsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinations_Exception()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsAsync(false)).Throws<Exception>();
            await shipToDestinationsController.GetShipToDestinationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuidAsync_Exception()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await shipToDestinationsController.GetShipToDestinationsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuid_KeyNotFoundException()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await shipToDestinationsController.GetShipToDestinationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuid_PermissionsException()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await shipToDestinationsController.GetShipToDestinationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuid_ArgumentException()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await shipToDestinationsController.GetShipToDestinationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuid_RepositoryException()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await shipToDestinationsController.GetShipToDestinationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuid_IntegrationApiException()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await shipToDestinationsController.GetShipToDestinationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToDestinationsByGuid_Exception()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToDestinationsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await shipToDestinationsController.GetShipToDestinationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_PostShipToDestinationsAsync_Exception()
        {
            await shipToDestinationsController.PostShipToDestinationsAsync(shipToDestinationsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_PutShipToDestinationsAsync_Exception()
        {
            var sourceContext = shipToDestinationsCollection.FirstOrDefault();
            await shipToDestinationsController.PutShipToDestinationsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_DeleteShipToDestinationsAsync_Exception()
        {
            await shipToDestinationsController.DeleteShipToDestinationsAsync(shipToDestinationsCollection.FirstOrDefault().Id);
        }

        [TestMethod]
        public async Task ShipToDestinationsController_GetShipToCodesAsync_ValidTests()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToCodesAsync()).ReturnsAsync(shipToCodesCollection);

            var sourceContexts = (await shipToDestinationsController.GetShipToCodesAsync()).ToList();
            Assert.AreEqual(shipToCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = shipToCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.Description, actual.Description, "Description, Index=" + i.ToString());
            }
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToCodes_KeyNotFoundException()
        {
            shipToDestinationsServiceMock.Setup(x => x.GetShipToCodesAsync())
                .Throws<KeyNotFoundException>();
            await shipToDestinationsController.GetShipToCodesAsync();
        }
       
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShipToDestinationsController_GetShipToCodes_PermissionsException()
        {

            shipToDestinationsServiceMock.Setup(x => x.GetShipToCodesAsync())
                .Throws<PermissionsException>();
            await shipToDestinationsController.GetShipToCodesAsync();
        }
    }
}