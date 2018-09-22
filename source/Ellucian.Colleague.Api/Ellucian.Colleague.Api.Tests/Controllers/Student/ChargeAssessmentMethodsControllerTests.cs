//Copyright 2018 Ellucian Company L.P. and its affiliates.
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
    public class ChargeAssessmentMethodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IChargeAssessmentMethodsService> chargeAssessmentMethodsServiceMock;
        private Mock<ILogger> loggerMock;
        private ChargeAssessmentMethodsController chargeAssessmentMethodsController;      
        private IEnumerable<Domain.Student.Entities.ChargeAssessmentMethod> allBillingMethods;
        private List<Dtos.ChargeAssessmentMethods> chargeAssessmentMethodsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            chargeAssessmentMethodsServiceMock = new Mock<IChargeAssessmentMethodsService>();
            loggerMock = new Mock<ILogger>();
            chargeAssessmentMethodsCollection = new List<Dtos.ChargeAssessmentMethods>();

            allBillingMethods  = new List<Domain.Student.Entities.ChargeAssessmentMethod>()
                {
                    new Domain.Student.Entities.ChargeAssessmentMethod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.ChargeAssessmentMethod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.ChargeAssessmentMethod("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allBillingMethods)
            {
                var chargeAssessmentMethods = new Ellucian.Colleague.Dtos.ChargeAssessmentMethods
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                chargeAssessmentMethodsCollection.Add(chargeAssessmentMethods);
            }

            chargeAssessmentMethodsController = new ChargeAssessmentMethodsController(chargeAssessmentMethodsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            chargeAssessmentMethodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            chargeAssessmentMethodsController = null;
            allBillingMethods = null;
            chargeAssessmentMethodsCollection = null;
            loggerMock = null;
            chargeAssessmentMethodsServiceMock = null;
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_ValidateFields_Nocache()
        {
            chargeAssessmentMethodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(false)).ReturnsAsync(chargeAssessmentMethodsCollection);
       
            var sourceContexts = (await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync()).ToList();
            Assert.AreEqual(chargeAssessmentMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = chargeAssessmentMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_ValidateFields_Cache()
        {
            chargeAssessmentMethodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(true)).ReturnsAsync(chargeAssessmentMethodsCollection);

            var sourceContexts = (await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync()).ToList();
            Assert.AreEqual(chargeAssessmentMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = chargeAssessmentMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_KeyNotFoundException()
        {
            //
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(false))
                .Throws<KeyNotFoundException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_PermissionsException()
        {
            
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(false))
                .Throws<PermissionsException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_ArgumentException()
        {
            
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(false))
                .Throws<ArgumentException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_RepositoryException()
        {
            
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(false))
                .Throws<RepositoryException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_IntegrationApiException()
        {
            
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(false))
                .Throws<IntegrationApiException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync();
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuidAsync_ValidateFields()
        {
            var expected = chargeAssessmentMethodsCollection.FirstOrDefault();
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethods_Exception()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsAsync(false)).Throws<Exception>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuidAsync_Exception()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuid_KeyNotFoundException()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuid_PermissionsException()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuid_ArgumentException()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuid_RepositoryException()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuid_IntegrationApiException()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_GetChargeAssessmentMethodsByGuid_Exception()
        {
            chargeAssessmentMethodsServiceMock.Setup(x => x.GetChargeAssessmentMethodsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await chargeAssessmentMethodsController.GetChargeAssessmentMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_PostChargeAssessmentMethodsAsync_Exception()
        {
            await chargeAssessmentMethodsController.PostChargeAssessmentMethodsAsync(chargeAssessmentMethodsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_PutChargeAssessmentMethodsAsync_Exception()
        {
            var sourceContext = chargeAssessmentMethodsCollection.FirstOrDefault();
            await chargeAssessmentMethodsController.PutChargeAssessmentMethodsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ChargeAssessmentMethodsController_DeleteChargeAssessmentMethodsAsync_Exception()
        {
            await chargeAssessmentMethodsController.DeleteChargeAssessmentMethodsAsync(chargeAssessmentMethodsCollection.FirstOrDefault().Id);
        }
    }
}