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
    public class TaxFormsBaseControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ITaxFormsBaseService> taxFormsServiceMock;
        private Mock<ILogger> loggerMock;
        private TaxFormsBaseController taxFormsController;      
        private IEnumerable<Domain.Base.Entities.TaxForms2> allTaxForms;
        private List<Dtos.TaxForms> taxFormsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            taxFormsServiceMock = new Mock<ITaxFormsBaseService>();
            loggerMock = new Mock<ILogger>();
            taxFormsCollection = new List<Dtos.TaxForms>();

            allTaxForms  = new List<Domain.Base.Entities.TaxForms2>()
                {
                    new Domain.Base.Entities.TaxForms2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "A1"),
                    new Domain.Base.Entities.TaxForms2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "A2"),
                    new Domain.Base.Entities.TaxForms2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "A3")
                };
            
            foreach (var source in allTaxForms)
            {
                var taxForms = new Ellucian.Colleague.Dtos.TaxForms
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                taxFormsCollection.Add(taxForms);
            }

            taxFormsController = new TaxFormsBaseController(taxFormsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            taxFormsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            taxFormsController = null;
            allTaxForms = null;
            taxFormsCollection = null;
            loggerMock = null;
            taxFormsServiceMock = null;
        }

        [TestMethod]
        public async Task TaxFormsController_GetTaxForms_ValidateFields_Nocache()
        {
            taxFormsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(false)).ReturnsAsync(taxFormsCollection);
       
            var sourceContexts = (await taxFormsController.GetTaxFormsAsync()).ToList();
            Assert.AreEqual(taxFormsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = taxFormsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task TaxFormsController_GetTaxForms_ValidateFields_Cache()
        {
            taxFormsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(true)).ReturnsAsync(taxFormsCollection);

            var sourceContexts = (await taxFormsController.GetTaxFormsAsync()).ToList();
            Assert.AreEqual(taxFormsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = taxFormsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxForms_KeyNotFoundException()
        {
            //
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(false))
                .Throws<KeyNotFoundException>();
            await taxFormsController.GetTaxFormsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxForms_PermissionsException()
        {
            
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(false))
                .Throws<PermissionsException>();
            await taxFormsController.GetTaxFormsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxForms_ArgumentException()
        {
            
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(false))
                .Throws<ArgumentException>();
            await taxFormsController.GetTaxFormsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxForms_RepositoryException()
        {
            
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(false))
                .Throws<RepositoryException>();
            await taxFormsController.GetTaxFormsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxForms_IntegrationApiException()
        {
            
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(false))
                .Throws<IntegrationApiException>();
            await taxFormsController.GetTaxFormsAsync();
        }

        [TestMethod]
        public async Task TaxFormsController_GetTaxFormsByGuidAsync_ValidateFields()
        {
            var expected = taxFormsCollection.FirstOrDefault();
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await taxFormsController.GetTaxFormsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxForms_Exception()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync(false)).Throws<Exception>();
            await taxFormsController.GetTaxFormsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsByGuidAsync_Exception()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await taxFormsController.GetTaxFormsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsByGuid_KeyNotFoundException()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await taxFormsController.GetTaxFormsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsByGuid_PermissionsException()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await taxFormsController.GetTaxFormsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsByGuid_ArgumentException()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await taxFormsController.GetTaxFormsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsByGuid_RepositoryException()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await taxFormsController.GetTaxFormsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsByGuid_IntegrationApiException()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await taxFormsController.GetTaxFormsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsByGuid_Exception()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await taxFormsController.GetTaxFormsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_PostTaxFormsAsync_Exception()
        {
            await taxFormsController.PostTaxFormsAsync(taxFormsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_PutTaxFormsAsync_Exception()
        {
            var sourceContext = taxFormsCollection.FirstOrDefault();
            await taxFormsController.PutTaxFormsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_DeleteTaxFormsAsync_Exception()
        {
            await taxFormsController.DeleteTaxFormsAsync(taxFormsCollection.FirstOrDefault().Id);
        }
    }
}