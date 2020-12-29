// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class VendorCommodityControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVendorCommodityService> vendorCommodityServiceMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;

        private VendorCommodityController vendorCommodityController;

        private string vendorId;
        private string commodityCode;
        private string Id;
        private Dtos.ColleagueFinance.VendorCommodity resultDto;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            vendorCommodityServiceMock = new Mock<IVendorCommodityService>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();

            vendorId = "0000200";
            commodityCode = "100900";
            Id = vendorId + "*" + commodityCode;
            BuildData();
            vendorCommodityServiceMock.Setup(s => s.GetVendorCommodityAsync(vendorId, commodityCode)).ReturnsAsync(resultDto);
            vendorCommodityController = new VendorCommodityController(vendorCommodityServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {
            resultDto = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorCommodity
            {
                Id = Id,               
                StdPrice = 10,
                StdPriceDate = new DateTime()
            };

        }

        [TestCleanup]
        public void Cleanup()
        {
            vendorCommodityController = null;
            _loggerMock = null;
            vendorCommodityServiceMock = null;
            resultDto = null;
        }

        [TestMethod]
        public async Task VendorCommodityController_GetVendorCommodityAsync_Success()
        {
            var actualDto = await vendorCommodityController.GetVendorCommodityAsync(vendorId, commodityCode);
            Assert.IsNotNull(actualDto);

            Assert.AreEqual(resultDto.Id, actualDto.Id);
            Assert.AreEqual(resultDto.StdPrice, actualDto.StdPrice);
            Assert.AreEqual(resultDto.StdPriceDate, actualDto.StdPriceDate);            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorCommodityController_GetVendorCommodityAsync_NullCriteria()
        {
            var actualDto = await vendorCommodityController.GetVendorCommodityAsync(null, null);
            Assert.IsNull(actualDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorCommodityController_GetVendorCommodityAsync_EmptyCriteria()
        {
            var actualDto = await vendorCommodityController.GetVendorCommodityAsync(string.Empty, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorCommodityController_GetVendorCommodityAsync_ArgumentNullException()
        {
            vendorCommodityServiceMock.Setup(s => s.GetVendorCommodityAsync(null, null)).Throws(new ArgumentNullException());
            vendorCommodityController = new VendorCommodityController(vendorCommodityServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await vendorCommodityController.GetVendorCommodityAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorCommodityController_GetVendorCommodityAsync_Exception()
        {
            vendorCommodityServiceMock.Setup(s => s.GetVendorCommodityAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
            vendorCommodityController = new VendorCommodityController(vendorCommodityServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await vendorCommodityController.GetVendorCommodityAsync(vendorId, commodityCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorCommodityController_GetVendorCommodityAsync_KeyNotFoundException()
        {
            vendorCommodityServiceMock.Setup(s => s.GetVendorCommodityAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new KeyNotFoundException());
            vendorCommodityController = new VendorCommodityController(vendorCommodityServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await vendorCommodityController.GetVendorCommodityAsync(vendorId, commodityCode);
        }
    }
}
