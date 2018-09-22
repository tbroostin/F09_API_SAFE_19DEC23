//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class VendorPaymentTermsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IVendorPaymentTermsService> vendorPaymentTermsServiceMock;
        private Mock<ILogger> loggerMock;
        private VendorPaymentTermsController vendorPaymentTermsController;
        private IEnumerable<VendorTerm> allVendorTerms;
        private List<Dtos.VendorPaymentTerms> vendorPaymentTermsCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            vendorPaymentTermsServiceMock = new Mock<IVendorPaymentTermsService>();
            loggerMock = new Mock<ILogger>();
            vendorPaymentTermsCollection = new List<Dtos.VendorPaymentTerms>();

            allVendorTerms = new List<VendorTerm>()
                {
                    new VendorTerm("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new VendorTerm("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new VendorTerm("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allVendorTerms)
            {
                var vendorPaymentTerms = new Ellucian.Colleague.Dtos.VendorPaymentTerms
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                vendorPaymentTermsCollection.Add(vendorPaymentTerms);
            }

            vendorPaymentTermsController = new VendorPaymentTermsController(vendorPaymentTermsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            vendorPaymentTermsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            vendorPaymentTermsController = null;
            allVendorTerms = null;
            vendorPaymentTermsCollection = null;
            loggerMock = null;
            vendorPaymentTermsServiceMock = null;
        }

        [TestMethod]
        public async Task VendorPaymentTermsController_GetVendorPaymentTerms_ValidateFields_Nocache()
        {
            vendorPaymentTermsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            vendorPaymentTermsServiceMock.Setup(x => x.GetVendorPaymentTermsAsync(false)).ReturnsAsync(vendorPaymentTermsCollection);

            var sourceContexts = (await vendorPaymentTermsController.GetVendorPaymentTermsAsync()).ToList();
            Assert.AreEqual(vendorPaymentTermsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = vendorPaymentTermsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task VendorPaymentTermsController_GetVendorPaymentTerms_ValidateFields_Cache()
        {
            vendorPaymentTermsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            vendorPaymentTermsServiceMock.Setup(x => x.GetVendorPaymentTermsAsync(true)).ReturnsAsync(vendorPaymentTermsCollection);

            var sourceContexts = (await vendorPaymentTermsController.GetVendorPaymentTermsAsync()).ToList();
            Assert.AreEqual(vendorPaymentTermsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = vendorPaymentTermsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task VendorPaymentTermsController_GetVendorPaymentTermsByGuidAsync_ValidateFields()
        {
            var expected = vendorPaymentTermsCollection.FirstOrDefault();
            vendorPaymentTermsServiceMock.Setup(x => x.GetVendorPaymentTermsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await vendorPaymentTermsController.GetVendorPaymentTermsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorPaymentTermsController_GetVendorPaymentTerms_Exception()
        {
            vendorPaymentTermsServiceMock.Setup(x => x.GetVendorPaymentTermsAsync(false)).Throws<Exception>();
            await vendorPaymentTermsController.GetVendorPaymentTermsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorPaymentTermsController_GetVendorPaymentTermsByGuidAsync_Exception()
        {
            vendorPaymentTermsServiceMock.Setup(x => x.GetVendorPaymentTermsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await vendorPaymentTermsController.GetVendorPaymentTermsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorPaymentTermsController_PostVendorPaymentTermsAsync_Exception()
        {
            await vendorPaymentTermsController.PostVendorPaymentTermsAsync(vendorPaymentTermsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorPaymentTermsController_PutVendorPaymentTermsAsync_Exception()
        {
            var sourceContext = vendorPaymentTermsCollection.FirstOrDefault();
            await vendorPaymentTermsController.PutVendorPaymentTermsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorPaymentTermsController_DeleteVendorPaymentTermsAsync_Exception()
        {
            await vendorPaymentTermsController.DeleteVendorPaymentTermsAsync(vendorPaymentTermsCollection.FirstOrDefault().Id);
        }
    }
}