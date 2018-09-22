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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayClassesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPayClassesService> payClassesServiceMock;
        private Mock<ILogger> loggerMock;
        private Ellucian.Colleague.Api.Controllers.HumanResources.PayClassesController payClassesController;      
        private IEnumerable<Domain.HumanResources.Entities.PayClass> allPayclass;
        private List<Dtos.PayClasses> payClassesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            payClassesServiceMock = new Mock<IPayClassesService>();
            loggerMock = new Mock<ILogger>();
            payClassesCollection = new List<Dtos.PayClasses>();

            allPayclass = new List<Domain.HumanResources.Entities.PayClass>()
                {
                    new Domain.HumanResources.Entities.PayClass("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.PayClass("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.PayClass("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allPayclass)
            {
                var payClasses = new Ellucian.Colleague.Dtos.PayClasses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                payClassesCollection.Add(payClasses);
            }

            payClassesController = new PayClassesController(payClassesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            payClassesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            payClassesController = null;
            allPayclass = null;
            payClassesCollection = null;
            loggerMock = null;
            payClassesServiceMock = null;
        }

        [TestMethod]
        public async Task PayClassesController_GetPayClasses_ValidateFields_Nocache()
        {
            payClassesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(false)).ReturnsAsync(payClassesCollection);
       
            var sourceContexts = (await payClassesController.GetPayClassesAsync()).ToList();
            Assert.AreEqual(payClassesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PayClassesController_GetPayClasses_ValidateFields_Cache()
        {
            payClassesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(true)).ReturnsAsync(payClassesCollection);

            var sourceContexts = (await payClassesController.GetPayClassesAsync()).ToList();
            Assert.AreEqual(payClassesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_KeyNotFoundException()
        {
            //
            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(false))
                .Throws<KeyNotFoundException>();
            await payClassesController.GetPayClassesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_PermissionsException()
        {
            
            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(false))
                .Throws<PermissionsException>();
            await payClassesController.GetPayClassesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_ArgumentException()
        {
            
            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(false))
                .Throws<ArgumentException>();
            await payClassesController.GetPayClassesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_RepositoryException()
        {
            
            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(false))
                .Throws<RepositoryException>();
            await payClassesController.GetPayClassesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_IntegrationApiException()
        {
            
            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(false))
                .Throws<IntegrationApiException>();
            await payClassesController.GetPayClassesAsync();
        }

        [TestMethod]
        public async Task PayClassesController_GetPayClassesByGuidAsync_ValidateFields()
        {
            var expected = payClassesCollection.FirstOrDefault();
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await payClassesController.GetPayClassesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_Exception()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesAsync(false)).Throws<Exception>();
            await payClassesController.GetPayClassesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuidAsync_Exception()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await payClassesController.GetPayClassesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_KeyNotFoundException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await payClassesController.GetPayClassesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_PermissionsException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await payClassesController.GetPayClassesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_ArgumentException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await payClassesController.GetPayClassesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_RepositoryException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await payClassesController.GetPayClassesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_IntegrationApiException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await payClassesController.GetPayClassesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_Exception()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await payClassesController.GetPayClassesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_PostPayClassesAsync_Exception()
        {
            await payClassesController.PostPayClassesAsync(payClassesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_PutPayClassesAsync_Exception()
        {
            var sourceContext = payClassesCollection.FirstOrDefault();
            await payClassesController.PutPayClassesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_DeletePayClassesAsync_Exception()
        {
            await payClassesController.DeletePayClassesAsync(payClassesCollection.FirstOrDefault().Id);
        }
    }

    [TestClass]
    public class PayClasses2ControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPayClassesService> payClassesServiceMock;
        private Mock<ILogger> loggerMock;
        private Ellucian.Colleague.Api.Controllers.HumanResources.PayClassesController payClassesController;
        private IEnumerable<Domain.HumanResources.Entities.PayClass> allPayclass;
        private List<Dtos.PayClasses2> payClassesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            payClassesServiceMock = new Mock<IPayClassesService>();
            loggerMock = new Mock<ILogger>();
            payClassesCollection = new List<Dtos.PayClasses2>();

            allPayclass = new List<Domain.HumanResources.Entities.PayClass>()
                {
                    new Domain.HumanResources.Entities.PayClass("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.PayClass("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.PayClass("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allPayclass)
            {
                var payClasses = new Ellucian.Colleague.Dtos.PayClasses2
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                payClassesCollection.Add(payClasses);
            }

            payClassesController = new PayClassesController(payClassesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            payClassesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            payClassesController = null;
            allPayclass = null;
            payClassesCollection = null;
            loggerMock = null;
            payClassesServiceMock = null;
        }

        [TestMethod]
        public async Task PayClassesController_GetPayClasses_ValidateFields_Nocache()
        {
            payClassesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(false)).ReturnsAsync(payClassesCollection);

            var sourceContexts = (await payClassesController.GetPayClasses2Async()).ToList();
            Assert.AreEqual(payClassesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PayClassesController_GetPayClasses_ValidateFields_Cache()
        {
            payClassesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(true)).ReturnsAsync(payClassesCollection);

            var sourceContexts = (await payClassesController.GetPayClasses2Async()).ToList();
            Assert.AreEqual(payClassesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = payClassesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_KeyNotFoundException()
        {
            //
            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(false))
                .Throws<KeyNotFoundException>();
            await payClassesController.GetPayClasses2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_PermissionsException()
        {

            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(false))
                .Throws<PermissionsException>();
            await payClassesController.GetPayClasses2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_ArgumentException()
        {

            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(false))
                .Throws<ArgumentException>();
            await payClassesController.GetPayClasses2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_RepositoryException()
        {

            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(false))
                .Throws<RepositoryException>();
            await payClassesController.GetPayClasses2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_IntegrationApiException()
        {

            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(false))
                .Throws<IntegrationApiException>();
            await payClassesController.GetPayClasses2Async();
        }

        [TestMethod]
        public async Task PayClassesController_GetPayClassesByGuidAsync_ValidateFields()
        {
            var expected = payClassesCollection.FirstOrDefault();
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(expected.Id)).ReturnsAsync(expected);

            var actual = await payClassesController.GetPayClassesByGuid2Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClasses_Exception()
        {
            payClassesServiceMock.Setup(x => x.GetPayClasses2Async(false)).Throws<Exception>();
            await payClassesController.GetPayClasses2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuidAsync_Exception()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(It.IsAny<string>())).Throws<Exception>();
            await payClassesController.GetPayClassesByGuid2Async(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_KeyNotFoundException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await payClassesController.GetPayClassesByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_PermissionsException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await payClassesController.GetPayClassesByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_ArgumentException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await payClassesController.GetPayClassesByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_RepositoryException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await payClassesController.GetPayClassesByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_IntegrationApiException()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await payClassesController.GetPayClassesByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_GetPayClassesByGuid_Exception()
        {
            payClassesServiceMock.Setup(x => x.GetPayClassesByGuid2Async(It.IsAny<string>()))
                .Throws<Exception>();
            await payClassesController.GetPayClassesByGuid2Async(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_PostPayClassesAsync_Exception()
        {
            await payClassesController.PostPayClassesAsync(new PayClasses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_PutPayClassesAsync_Exception()
        {
            var sourceContext = payClassesCollection.FirstOrDefault();
            await payClassesController.PutPayClassesAsync(sourceContext.Id, new PayClasses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayClassesController_DeletePayClassesAsync_Exception()
        {
            await payClassesController.DeletePayClassesAsync(payClassesCollection.FirstOrDefault().Id);
        }
    }
}