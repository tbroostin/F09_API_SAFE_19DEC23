//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ShippingMethodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IShippingMethodsService> shippingMethodsServiceMock;
        private Mock<ILogger> loggerMock;
        private ShippingMethodsController shippingMethodsController;      
        private IEnumerable<Domain.ColleagueFinance.Entities.ShippingMethod> allShippingMethods;
        private List<Dtos.ShippingMethods> shippingMethodsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            shippingMethodsServiceMock = new Mock<IShippingMethodsService>();
            loggerMock = new Mock<ILogger>();
            shippingMethodsCollection = new List<Dtos.ShippingMethods>();

            allShippingMethods = new List<Domain.ColleagueFinance.Entities.ShippingMethod>()
                {
                    new Domain.ColleagueFinance.Entities.ShippingMethod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.ShippingMethod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.ShippingMethod("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allShippingMethods)
            {
                var shippingMethods = new Ellucian.Colleague.Dtos.ShippingMethods
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                shippingMethodsCollection.Add(shippingMethods);
            }

            shippingMethodsController = new ShippingMethodsController(shippingMethodsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            shippingMethodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            shippingMethodsController = null;
            allShippingMethods = null;
            shippingMethodsCollection = null;
            loggerMock = null;
            shippingMethodsServiceMock = null;
        }

        [TestMethod]
        public async Task ShippingMethodsController_GetShippingMethods_ValidateFields_Nocache()
        {
            shippingMethodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(false)).ReturnsAsync(shippingMethodsCollection);
       
            var sourceContexts = (await shippingMethodsController.GetShippingMethodsAsync()).ToList();
            Assert.AreEqual(shippingMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = shippingMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task ShippingMethodsController_GetShippingMethods_ValidateFields_Cache()
        {
            shippingMethodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(true)).ReturnsAsync(shippingMethodsCollection);

            var sourceContexts = (await shippingMethodsController.GetShippingMethodsAsync()).ToList();
            Assert.AreEqual(shippingMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = shippingMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethods_KeyNotFoundException()
        {
            //
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(false))
                .Throws<KeyNotFoundException>();
            await shippingMethodsController.GetShippingMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethods_PermissionsException()
        {
            
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(false))
                .Throws<PermissionsException>();
            await shippingMethodsController.GetShippingMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethods_ArgumentException()
        {
            
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(false))
                .Throws<ArgumentException>();
            await shippingMethodsController.GetShippingMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethods_RepositoryException()
        {
            
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(false))
                .Throws<RepositoryException>();
            await shippingMethodsController.GetShippingMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethods_IntegrationApiException()
        {
            
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(false))
                .Throws<IntegrationApiException>();
            await shippingMethodsController.GetShippingMethodsAsync();
        }

        [TestMethod]
        public async Task ShippingMethodsController_GetShippingMethodsByGuidAsync_ValidateFields()
        {
            var expected = shippingMethodsCollection.FirstOrDefault();
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await shippingMethodsController.GetShippingMethodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethods_Exception()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsAsync(false)).Throws<Exception>();
            await shippingMethodsController.GetShippingMethodsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethodsByGuidAsync_Exception()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await shippingMethodsController.GetShippingMethodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethodsByGuid_KeyNotFoundException()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await shippingMethodsController.GetShippingMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethodsByGuid_PermissionsException()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await shippingMethodsController.GetShippingMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethodsByGuid_ArgumentException()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await shippingMethodsController.GetShippingMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethodsByGuid_RepositoryException()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await shippingMethodsController.GetShippingMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethodsByGuid_IntegrationApiException()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await shippingMethodsController.GetShippingMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_GetShippingMethodsByGuid_Exception()
        {
            shippingMethodsServiceMock.Setup(x => x.GetShippingMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await shippingMethodsController.GetShippingMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_PostShippingMethodsAsync_Exception()
        {
            await shippingMethodsController.PostShippingMethodsAsync(shippingMethodsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_PutShippingMethodsAsync_Exception()
        {
            var sourceContext = shippingMethodsCollection.FirstOrDefault();
            await shippingMethodsController.PutShippingMethodsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ShippingMethodsController_DeleteShippingMethodsAsync_Exception()
        {
            await shippingMethodsController.DeleteShippingMethodsAsync(shippingMethodsCollection.FirstOrDefault().Id);
        }
    }
}