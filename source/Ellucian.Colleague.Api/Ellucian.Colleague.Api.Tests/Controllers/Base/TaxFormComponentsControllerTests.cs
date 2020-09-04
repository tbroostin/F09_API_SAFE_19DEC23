//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class TaxFormComponentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ITaxFormComponentsService> taxFormComponentsServiceMock;
        private Mock<ILogger> loggerMock;
        private TaxFormComponentsController taxFormComponentsController;      
        private IEnumerable<Domain.Base.Entities.BoxCodes> allBoxCodes;
        private List<Dtos.TaxFormComponents> taxFormComponentsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            taxFormComponentsServiceMock = new Mock<ITaxFormComponentsService>();
            loggerMock = new Mock<ILogger>();
            taxFormComponentsCollection = new List<Dtos.TaxFormComponents>();

            allBoxCodes  = new List<Domain.Base.Entities.BoxCodes>()
                {
                    new Domain.Base.Entities.BoxCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "W2"),
                    new Domain.Base.Entities.BoxCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "W2"),
                    new Domain.Base.Entities.BoxCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "W9")
                };
            
            foreach (var source in allBoxCodes)
            {
                var taxFormComponents = new Ellucian.Colleague.Dtos.TaxFormComponents
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                taxFormComponentsCollection.Add(taxFormComponents);
            }
            
            taxFormComponentsController = new TaxFormComponentsController(taxFormComponentsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            taxFormComponentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            taxFormComponentsController = null;
            allBoxCodes = null;
            taxFormComponentsCollection = null;
            loggerMock = null;
            taxFormComponentsServiceMock = null;
        }

        [TestMethod]
        public async Task TaxFormComponentsController_GetTaxFormComponents_ValidateFields_Nocache()
        {
            taxFormComponentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(false)).ReturnsAsync(taxFormComponentsCollection);
       
            var sourceContexts = (await taxFormComponentsController.GetTaxFormComponentsAsync()).ToList();
            Assert.AreEqual(taxFormComponentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = taxFormComponentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task TaxFormComponentsController_GetTaxFormComponents_ValidateFields_Cache()
        {
            taxFormComponentsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(true)).ReturnsAsync(taxFormComponentsCollection);

            var sourceContexts = (await taxFormComponentsController.GetTaxFormComponentsAsync()).ToList();
            Assert.AreEqual(taxFormComponentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = taxFormComponentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponents_KeyNotFoundException()
        {
            //
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(false))
                .Throws<KeyNotFoundException>();
            await taxFormComponentsController.GetTaxFormComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponents_PermissionsException()
        {
            
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(false))
                .Throws<PermissionsException>();
            await taxFormComponentsController.GetTaxFormComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponents_ArgumentException()
        {
            
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(false))
                .Throws<ArgumentException>();
            await taxFormComponentsController.GetTaxFormComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponents_RepositoryException()
        {
            
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(false))
                .Throws<RepositoryException>();
            await taxFormComponentsController.GetTaxFormComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponents_IntegrationApiException()
        {
            
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(false))
                .Throws<IntegrationApiException>();
            await taxFormComponentsController.GetTaxFormComponentsAsync();
        }

        [TestMethod]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuidAsync_ValidateFields()
        {
            var expected = taxFormComponentsCollection.FirstOrDefault();
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponents_Exception()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsAsync(false)).Throws<Exception>();
            await taxFormComponentsController.GetTaxFormComponentsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuidAsync_Exception()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuid_KeyNotFoundException()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuid_PermissionsException()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuid_ArgumentException()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuid_RepositoryException()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuid_IntegrationApiException()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_GetTaxFormComponentsByGuid_Exception()
        {
            taxFormComponentsServiceMock.Setup(x => x.GetTaxFormComponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await taxFormComponentsController.GetTaxFormComponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_PostTaxFormComponentsAsync_Exception()
        {
            await taxFormComponentsController.PostTaxFormComponentsAsync(taxFormComponentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_PutTaxFormComponentsAsync_Exception()
        {
            var sourceContext = taxFormComponentsCollection.FirstOrDefault();
            await taxFormComponentsController.PutTaxFormComponentsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormComponentsController_DeleteTaxFormComponentsAsync_Exception()
        {
            await taxFormComponentsController.DeleteTaxFormComponentsAsync(taxFormComponentsCollection.FirstOrDefault().Id);
        }
    }
}