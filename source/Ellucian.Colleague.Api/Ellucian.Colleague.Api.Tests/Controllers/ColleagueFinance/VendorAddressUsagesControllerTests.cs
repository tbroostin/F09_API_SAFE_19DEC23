//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class VendorAddressUsagesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IVendorAddressUsagesService> vendorAddressUsagesServiceMock;
        private Mock<ILogger> loggerMock;
        private VendorAddressUsagesController vendorAddressUsagesController;      
        private IEnumerable<Domain.ColleagueFinance.Entities.IntgVendorAddressUsages> allIntgVendorAddressUsages;
        private List<Dtos.VendorAddressUsages> vendorAddressUsagesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            vendorAddressUsagesServiceMock = new Mock<IVendorAddressUsagesService>();
            loggerMock = new Mock<ILogger>();
            vendorAddressUsagesCollection = new List<Dtos.VendorAddressUsages>();

            allIntgVendorAddressUsages  = new List<Domain.ColleagueFinance.Entities.IntgVendorAddressUsages>()
                {
                    new Domain.ColleagueFinance.Entities.IntgVendorAddressUsages("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.IntgVendorAddressUsages("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.IntgVendorAddressUsages("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allIntgVendorAddressUsages)
            {
                var vendorAddressUsages = new Ellucian.Colleague.Dtos.VendorAddressUsages
                {
                    Id = source.Guid,
                    //Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                vendorAddressUsagesCollection.Add(vendorAddressUsages);
            }

            vendorAddressUsagesController = new VendorAddressUsagesController(vendorAddressUsagesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            vendorAddressUsagesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            vendorAddressUsagesController = null;
            allIntgVendorAddressUsages = null;
            vendorAddressUsagesCollection = null;
            loggerMock = null;
            vendorAddressUsagesServiceMock = null;
        }

        [TestMethod]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_ValidateFields_Nocache()
        {
            vendorAddressUsagesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(false)).ReturnsAsync(vendorAddressUsagesCollection);
       
            var sourceContexts = (await vendorAddressUsagesController.GetVendorAddressUsagesAsync()).ToList();
            Assert.AreEqual(vendorAddressUsagesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = vendorAddressUsagesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                //Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_ValidateFields_Cache()
        {
            vendorAddressUsagesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(true)).ReturnsAsync(vendorAddressUsagesCollection);

            var sourceContexts = (await vendorAddressUsagesController.GetVendorAddressUsagesAsync()).ToList();
            Assert.AreEqual(vendorAddressUsagesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = vendorAddressUsagesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                //Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_KeyNotFoundException()
        {
            //
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(false))
                .Throws<KeyNotFoundException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_PermissionsException()
        {
            
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(false))
                .Throws<PermissionsException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_ArgumentException()
        {
            
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(false))
                .Throws<ArgumentException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_RepositoryException()
        {
            
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(false))
                .Throws<RepositoryException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_IntegrationApiException()
        {
            
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(false))
                .Throws<IntegrationApiException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesAsync();
        }

        [TestMethod]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuidAsync_ValidateFields()
        {
            var expected = vendorAddressUsagesCollection.FirstOrDefault();
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            //Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsages_Exception()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesAsync(false)).Throws<Exception>();
            await vendorAddressUsagesController.GetVendorAddressUsagesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuidAsync_Exception()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuid_KeyNotFoundException()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuid_PermissionsException()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuid_ArgumentException()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuid_RepositoryException()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuid_IntegrationApiException()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_GetVendorAddressUsagesByGuid_Exception()
        {
            vendorAddressUsagesServiceMock.Setup(x => x.GetVendorAddressUsagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await vendorAddressUsagesController.GetVendorAddressUsagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_PostVendorAddressUsagesAsync_Exception()
        {
            await vendorAddressUsagesController.PostVendorAddressUsagesAsync(vendorAddressUsagesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_PutVendorAddressUsagesAsync_Exception()
        {
            var sourceContext = vendorAddressUsagesCollection.FirstOrDefault();
            await vendorAddressUsagesController.PutVendorAddressUsagesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorAddressUsagesController_DeleteVendorAddressUsagesAsync_Exception()
        {
            await vendorAddressUsagesController.DeleteVendorAddressUsagesAsync(vendorAddressUsagesCollection.FirstOrDefault().Id);
        }
    }
}