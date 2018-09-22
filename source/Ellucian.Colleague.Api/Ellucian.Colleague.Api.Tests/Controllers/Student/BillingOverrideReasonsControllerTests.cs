//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class BillingOverrideReasonsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IBillingOverrideReasonsService> billingOverrideReasonsServiceMock;
        private Mock<ILogger> loggerMock;
        private BillingOverrideReasonsController billingOverrideReasonsController;      
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons> allBillingOverrideReasons;
        private List<Dtos.BillingOverrideReasons> billingOverrideReasonsCollection;

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            billingOverrideReasonsServiceMock = new Mock<IBillingOverrideReasonsService>();
            loggerMock = new Mock<ILogger>();
            billingOverrideReasonsCollection = new List<Dtos.BillingOverrideReasons>();

            allBillingOverrideReasons = new List<Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allBillingOverrideReasons)
            {
                var billingOverrideReasons = new Ellucian.Colleague.Dtos.BillingOverrideReasons
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                billingOverrideReasonsCollection.Add(billingOverrideReasons);
            }

            billingOverrideReasonsController = new BillingOverrideReasonsController(billingOverrideReasonsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            billingOverrideReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            billingOverrideReasonsController = null;
            allBillingOverrideReasons = null;
            billingOverrideReasonsCollection = null;
            loggerMock = null;
            billingOverrideReasonsServiceMock = null;
        }

        [TestMethod]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_ValidateFields_Nocache()
        {
            billingOverrideReasonsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(false)).ReturnsAsync(billingOverrideReasonsCollection);
       
            var sourceContexts = (await billingOverrideReasonsController.GetBillingOverrideReasonsAsync()).ToList();
            Assert.AreEqual(billingOverrideReasonsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = billingOverrideReasonsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_ValidateFields_Cache()
        {
            billingOverrideReasonsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(true)).ReturnsAsync(billingOverrideReasonsCollection);

            var sourceContexts = (await billingOverrideReasonsController.GetBillingOverrideReasonsAsync()).ToList();
            Assert.AreEqual(billingOverrideReasonsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = billingOverrideReasonsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_ValidateFields()
        {
            var expected = billingOverrideReasonsCollection.FirstOrDefault();
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_Exception()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(false)).Throws<Exception>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_PermissionsException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(false)).Throws<PermissionsException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_KeyNotFoundException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(false)).Throws<KeyNotFoundException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_ArgumentException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(false)).Throws<ArgumentException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_RepositoryException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(false)).Throws<RepositoryException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasons_IntegrationApiException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsAsync(false)).Throws<IntegrationApiException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_EmptyGuid_Exception()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_Exception()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_KeyNotFoundException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_PermissionsException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_ArgumentException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_RepositoryException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_GetBillingOverrideReasonsByGuidAsync_IntegrationApiException()
        {
            billingOverrideReasonsServiceMock.Setup(x => x.GetBillingOverrideReasonsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await billingOverrideReasonsController.GetBillingOverrideReasonsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_PostBillingOverrideReasonsAsync_Exception()
        {
            await billingOverrideReasonsController.PostBillingOverrideReasonsAsync(billingOverrideReasonsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_PutBillingOverrideReasonsAsync_Exception()
        {
            var sourceContext = billingOverrideReasonsCollection.FirstOrDefault();
            await billingOverrideReasonsController.PutBillingOverrideReasonsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BillingOverrideReasonsController_DeleteBillingOverrideReasonsAsync_Exception()
        {
            await billingOverrideReasonsController.DeleteBillingOverrideReasonsAsync(billingOverrideReasonsCollection.FirstOrDefault().Id);
        }
    }
}