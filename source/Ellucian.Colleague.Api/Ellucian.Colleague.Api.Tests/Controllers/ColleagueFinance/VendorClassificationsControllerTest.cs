// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class VendorClassificationsControllerTest
    {
        public TestContext TestContext { get; set; }

        Mock<IVendorTypesService> vendorTypesServiceMock;
        Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepository;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ILogger> loggerMock;

        VendorClassificationsController vendorClassificationsController;
        IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType> vendorTypesEntities;
        List<Dtos.VendorType> vendorTypesDto = new List<Dtos.VendorType>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            colleagueFinanceReferenceDataRepository = new Mock<IColleagueFinanceReferenceDataRepository>();
            vendorTypesServiceMock = new Mock<IVendorTypesService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            vendorTypesEntities = new List<Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType>()
                {
                    new Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
        
        
            foreach (var vendorTypeEntity in vendorTypesEntities)
            {
                Dtos.VendorType vendorTypeDto = new Dtos.VendorType();
                vendorTypeDto.Id = vendorTypeEntity.Guid;
                vendorTypeDto.Code = vendorTypeEntity.Code;
                vendorTypeDto.Title = vendorTypeEntity.Description;
                vendorTypeDto.Description = string.Empty;
                vendorTypesDto.Add(vendorTypeDto);
            }

            vendorClassificationsController = new VendorClassificationsController(adapterRegistryMock.Object, vendorTypesServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            vendorClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
            new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            vendorClassificationsController = null;
            vendorTypesEntities = null;
            vendorTypesDto = null;
        }

        [TestMethod]
        public async Task VendorClassificationsController_GetAll_NoCache_True()
        {
            vendorClassificationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            vendorTypesServiceMock.Setup(ac => ac.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypesDto);

            var results = await vendorClassificationsController.GetVendorTypesAsync();
            Assert.AreEqual(vendorTypesDto.Count, results.Count());

            foreach (var vendorTypeDto in vendorTypesDto)
            {
                var result = results.FirstOrDefault(i => i.Id == vendorTypeDto.Id);
                Assert.AreEqual(result.Id, vendorTypeDto.Id);
                Assert.AreEqual(result.Code, vendorTypeDto.Code);
                Assert.AreEqual(result.Title, vendorTypeDto.Title);
                Assert.AreEqual(result.Description, vendorTypeDto.Description);
            }
        }

        [TestMethod]
        public async Task VendorClassificationsController_GetAll_NoCache_False()
        {
            vendorClassificationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };

            vendorTypesServiceMock.Setup(ac => ac.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypesDto);

            var results = await vendorClassificationsController.GetVendorTypesAsync();
            Assert.AreEqual(vendorTypesDto.Count, results.Count());

            foreach (var vendorTypeDto in vendorTypesDto)
            {
                var result = results.FirstOrDefault(i => i.Id == vendorTypeDto.Id);
                Assert.AreEqual(result.Id, vendorTypeDto.Id);
                Assert.AreEqual(result.Code, vendorTypeDto.Code);
                Assert.AreEqual(result.Title, vendorTypeDto.Title);
                Assert.AreEqual(result.Description, vendorTypeDto.Description);
            }

        }

        [TestMethod]
        public async Task VendorClassificationsController_GetById()
        {
            string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
            var vendorTypeDto = vendorTypesDto.FirstOrDefault(i => i.Id == id);
            vendorClassificationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = It.IsAny<bool>(),
                Public = true
            };

            vendorTypesServiceMock.Setup(ac => ac.GetVendorTypeByIdAsync(id)).ReturnsAsync(vendorTypeDto);

            var result = await vendorClassificationsController.GetVendorTypeByIdAsync(id);
            Assert.AreEqual(result.Id, vendorTypeDto.Id);
            Assert.AreEqual(result.Code, vendorTypeDto.Code);
            Assert.AreEqual(result.Title, vendorTypeDto.Title);
            Assert.AreEqual(result.Description, vendorTypeDto.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetById_Exception()
        {
            vendorTypesServiceMock.Setup(ac => ac.GetVendorTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await vendorClassificationsController.GetVendorTypeByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypesAsync_Exception()
        {
            vendorTypesServiceMock.Setup(s => s.GetVendorTypesAsync(It.IsAny<bool>())).Throws<Exception>();
            await vendorClassificationsController.GetVendorTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypesAsync_IntegrationApiException()
        {
            vendorTypesServiceMock.Setup(s => s.GetVendorTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
            await vendorClassificationsController.GetVendorTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypeByIdAsync_PermissionsException()
        {
            var expected = vendorTypesDto.FirstOrDefault();
            vendorTypesServiceMock.Setup(x => x.GetVendorTypeByIdAsync(expected.Id)).Throws<PermissionsException>();
            Debug.Assert(expected != null, "expected != null");
            await vendorClassificationsController.GetVendorTypeByIdAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypesAsync_PermissionsException()
        {
            vendorTypesServiceMock.Setup(s => s.GetVendorTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
            await vendorClassificationsController.GetVendorTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypeByIdAsync_KeyNotFoundException()
        {
            var expected = vendorTypesDto.FirstOrDefault();
            vendorTypesServiceMock.Setup(x => x.GetVendorTypeByIdAsync(expected.Id)).Throws<KeyNotFoundException>();
            Debug.Assert(expected != null, "expected != null");
            await vendorClassificationsController.GetVendorTypeByIdAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypesAsync_KeyNotFoundException()
        {
            vendorTypesServiceMock.Setup(s => s.GetVendorTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await vendorClassificationsController.GetVendorTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypesAsync_ArgumentNullException()
        {
            vendorTypesServiceMock.Setup(s => s.GetVendorTypesAsync(It.IsAny<bool>())).Throws<ArgumentNullException>();
            await vendorClassificationsController.GetVendorTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_GetVendorTypesAsync_RepositoryException()
        {
            vendorTypesServiceMock.Setup(s => s.GetVendorTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
            await vendorClassificationsController.GetVendorTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_PUT_Exception()
        {
            var result = await vendorClassificationsController.PutVendorTypeAsync(It.IsAny<string>(), new Dtos.VendorType());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_POST_Exception()
        {
            var result = await vendorClassificationsController.PostVendorTypeAsync(new Dtos.VendorType());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorClassificationsController_DELETE_Exception()
        {
            await vendorClassificationsController.DeleteVendorTypeAsync(It.IsAny<string>());
        }
    }
}
