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
    public class PayClassificationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPayClassificationsService> payClassificationsServiceMock;
        private Mock<ILogger> loggerMock;
        private PayClassificationsController payClassificationsController;      
        private IEnumerable<Domain.HumanResources.Entities.PayClassification> allPayClassifications;
        private List<Dtos.PayClassifications> payClassificationsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            payClassificationsServiceMock = new Mock<IPayClassificationsService>();
            loggerMock = new Mock<ILogger>();
            payClassificationsCollection = new List<Dtos.PayClassifications>();

            allPayClassifications  = new List<Domain.HumanResources.Entities.PayClassification>()
                {
                    new Domain.HumanResources.Entities.PayClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.PayClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.PayClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allPayClassifications)
            {
                var payClassifications = new Ellucian.Colleague.Dtos.PayClassifications
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description
                };
                payClassificationsCollection.Add(payClassifications);
            }

            payClassificationsController = new PayClassificationsController(payClassificationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            payClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            payClassificationsController = null;
            allPayClassifications = null;
            payClassificationsCollection = null;
            loggerMock = null;
            payClassificationsServiceMock = null;
        }

        [TestMethod]
        public async Task PayClassificationsController_GetPayClassifications_ValidateFields_Nocache()
        {
            payClassificationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(false)).ReturnsAsync(payClassificationsCollection);
       
            var sourceContexts = (await payClassificationsController.GetPayClassificationsAsync()).ToList();
            Assert.AreEqual(payClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PayClassificationsController_GetPayClassifications_ValidateFields_Cache()
        {
            payClassificationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(true)).ReturnsAsync(payClassificationsCollection);

            var sourceContexts = (await payClassificationsController.GetPayClassificationsAsync()).ToList();
            Assert.AreEqual(payClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_KeyNotFoundException()
        {
            //
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(false))
                .Throws<KeyNotFoundException>();
            await payClassificationsController.GetPayClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_PermissionsException()
        {
            
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(false))
                .Throws<PermissionsException>();
            await payClassificationsController.GetPayClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_ArgumentException()
        {
            
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(false))
                .Throws<ArgumentException>();
            await payClassificationsController.GetPayClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_RepositoryException()
        {
            
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(false))
                .Throws<RepositoryException>();
            await payClassificationsController.GetPayClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_IntegrationApiException()
        {
            
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(false))
                .Throws<IntegrationApiException>();
            await payClassificationsController.GetPayClassificationsAsync();
        }

        [TestMethod]
        public async Task PayClassificationsController_GetPayClassificationsByGuidAsync_ValidateFields()
        {
            var expected = payClassificationsCollection.FirstOrDefault();
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await payClassificationsController.GetPayClassificationsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_Exception()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsAsync(false)).Throws<Exception>();
            await payClassificationsController.GetPayClassificationsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuidAsync_Exception()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await payClassificationsController.GetPayClassificationsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_KeyNotFoundException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await payClassificationsController.GetPayClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_PermissionsException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await payClassificationsController.GetPayClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_ArgumentException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await payClassificationsController.GetPayClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_RepositoryException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await payClassificationsController.GetPayClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_IntegrationApiException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await payClassificationsController.GetPayClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_Exception()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await payClassificationsController.GetPayClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_PostPayClassificationsAsync_Exception()
        {
            await payClassificationsController.PostPayClassificationsAsync(payClassificationsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_PutPayClassificationsAsync_Exception()
        {
            var sourceContext = payClassificationsCollection.FirstOrDefault();
            await payClassificationsController.PutPayClassificationsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_DeletePayClassificationsAsync_Exception()
        {
            await payClassificationsController.DeletePayClassificationsAsync(payClassificationsCollection.FirstOrDefault().Id);
        }
    }

    [TestClass]
    public class PayClassifications2ControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPayClassificationsService> payClassificationsServiceMock;
        private Mock<ILogger> loggerMock;
        private PayClassificationsController payClassificationsController;
        private IEnumerable<Domain.HumanResources.Entities.PayClassification> allPayClassifications;
        private List<Dtos.PayClassifications2> payClassificationsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            payClassificationsServiceMock = new Mock<IPayClassificationsService>();
            loggerMock = new Mock<ILogger>();
            payClassificationsCollection = new List<Dtos.PayClassifications2>();

            allPayClassifications = new List<Domain.HumanResources.Entities.PayClassification>()
                {
                    new Domain.HumanResources.Entities.PayClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.PayClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.PayClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allPayClassifications)
            {
                var payClassifications = new Ellucian.Colleague.Dtos.PayClassifications2
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description
                };
                payClassificationsCollection.Add(payClassifications);
            }

            payClassificationsController = new PayClassificationsController(payClassificationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            payClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            payClassificationsController = null;
            allPayClassifications = null;
            payClassificationsCollection = null;
            loggerMock = null;
            payClassificationsServiceMock = null;
        }

        [TestMethod]
        public async Task PayClassificationsController_GetPayClassifications_ValidateFields_Nocache()
        {
            payClassificationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(false)).ReturnsAsync(payClassificationsCollection);

            var sourceContexts = (await payClassificationsController.GetPayClassifications2Async()).ToList();
            Assert.AreEqual(payClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PayClassificationsController_GetPayClassifications_ValidateFields_Cache()
        {
            payClassificationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(true)).ReturnsAsync(payClassificationsCollection);

            var sourceContexts = (await payClassificationsController.GetPayClassifications2Async()).ToList();
            Assert.AreEqual(payClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_KeyNotFoundException()
        {
            //
            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(false))
                .Throws<KeyNotFoundException>();
            await payClassificationsController.GetPayClassifications2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_PermissionsException()
        {

            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(false))
                .Throws<PermissionsException>();
            await payClassificationsController.GetPayClassifications2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_ArgumentException()
        {

            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(false))
                .Throws<ArgumentException>();
            await payClassificationsController.GetPayClassifications2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_RepositoryException()
        {

            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(false))
                .Throws<RepositoryException>();
            await payClassificationsController.GetPayClassifications2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_IntegrationApiException()
        {

            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(false))
                .Throws<IntegrationApiException>();
            await payClassificationsController.GetPayClassifications2Async();
        }

        [TestMethod]
        public async Task PayClassificationsController_GetPayClassificationsByGuidAsync_ValidateFields()
        {
            var expected = payClassificationsCollection.FirstOrDefault();
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await payClassificationsController.GetPayClassificationsByGuid2Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassifications_Exception()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassifications2Async(false)).Throws<Exception>();
            await payClassificationsController.GetPayClassifications2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuidAsync_Exception()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await payClassificationsController.GetPayClassificationsByGuid2Async(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_KeyNotFoundException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await payClassificationsController.GetPayClassificationsByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_PermissionsException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await payClassificationsController.GetPayClassificationsByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_ArgumentException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await payClassificationsController.GetPayClassificationsByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_RepositoryException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await payClassificationsController.GetPayClassificationsByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_IntegrationApiException()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await payClassificationsController.GetPayClassificationsByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassificationsController_GetPayClassificationsByGuid_Exception()
        {
            payClassificationsServiceMock.Setup(x => x.GetPayClassificationsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await payClassificationsController.GetPayClassificationsByGuid2Async(expectedGuid);
        }

    }
}