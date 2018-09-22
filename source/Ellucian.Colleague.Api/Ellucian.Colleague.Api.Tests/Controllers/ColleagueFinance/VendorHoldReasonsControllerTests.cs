// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class VendorHoldReasonsControllerTests
    {
        [TestClass]
        public class VendorHoldReasonsControllerGet
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private VendorHoldReasonsController VendorHoldReasonsController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IVendorHoldReasonsService> VendorHoldReasonsService;
            List<VendorHoldReasons> vendorHoldReasonsDtoList;
            private string vendorHoldReasonsGuid = "03ef76f3-61be-4990-8a99-9a80282fc420";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                VendorHoldReasonsService = new Mock<IVendorHoldReasonsService>();

                BuildData();

                VendorHoldReasonsController = new VendorHoldReasonsController(VendorHoldReasonsService.Object,logger);                VendorHoldReasonsController.Request = new HttpRequestMessage();
                VendorHoldReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                VendorHoldReasonsController = null;
                VendorHoldReasonsService = null;
                vendorHoldReasonsDtoList = null;
            }

            #region GET ALL Tests
            [TestMethod]
            public async Task VendorHoldReasons_GetAll_Async()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(vendorHoldReasonsDtoList);

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = vendorHoldReasonsDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task VendorHoldReasons_GetAll_TrueCache_Async()
            {
                VendorHoldReasonsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                VendorHoldReasonsController.Request.Headers.CacheControl.NoCache = true;

                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(true)).ReturnsAsync(vendorHoldReasonsDtoList);

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = vendorHoldReasonsDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            #region Get ALL Exception Tests

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAll_Exception()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAll__PermissionsException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAll_ArgumentException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAll_RepositoryException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAll_IntegrationApiException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAll_KeyNotFoundException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsAsync();
            }
            
            #endregion
            #endregion

            #region GET ID TESTS
            [TestMethod]
            public async Task VendorHoldReasons_GetById_Async()
            {
                var expected = vendorHoldReasonsDtoList.FirstOrDefault(i => i.Id.Equals(vendorHoldReasonsGuid));

                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsByGuidAsync(vendorHoldReasonsGuid)).ReturnsAsync(expected);

                var actual = await VendorHoldReasonsController.GetVendorHoldReasonsByIdAsync(vendorHoldReasonsGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
            }



            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAById_Exception()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsByIdAsync("ABC");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAById_KeyNotFoundException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsByIdAsync("BC");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAById__PermissionsException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsByIdAsync("ABC");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAById_ArgumentException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsByIdAsync("ABC");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAById_RepositoryException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsByIdAsync("ABC");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasons_GetAById_IntegrationApiException()
            {
                VendorHoldReasonsService.Setup(x => x.GetVendorHoldReasonsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await VendorHoldReasonsController.GetVendorHoldReasonsByIdAsync(It.IsAny<string>());
            }
            #endregion
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasonsController_PostThrowsIntAppiExc()
            {
                await VendorHoldReasonsController.PostVendorHoldReasonsAsync(vendorHoldReasonsDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasonsController_PutThrowsIntAppiExc()
            {
                var result = await VendorHoldReasonsController.PutVendorHoldReasonsAsync(vendorHoldReasonsGuid, vendorHoldReasonsDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task VendorHoldReasonsController_DeleteThrowsIntAppiExc()
            {
                await VendorHoldReasonsController.DeleteVendorHoldReasonsAsync(vendorHoldReasonsGuid);
            }

            private void BuildData()
            {
                vendorHoldReasonsDtoList = new List<VendorHoldReasons>() 
                {
                    new VendorHoldReasons(){Id = "03ef76f3-61be-4990-8a99-9a80282fc420", Code = "OB", Description = null, Title = "Out of Business" },
                    new VendorHoldReasons(){Id = "d2f4f0af-6714-48c7-88d5-1c40cb407b6c", Code = "DISC", Description = null, Title = "Vendor Discontinued"},
                    new VendorHoldReasons(){Id = "c517d7a5-f06a-42c8-85ab-b6320e1c0c2a", Code = "QUAL", Description = null, Title = "Quality Hold"},
                    new VendorHoldReasons(){Id = "6c591aaa-5d33-4b19-b5e9-f6cf8956ef0a", Code = "DISP", Description = null, Title = "Disputed Transactions"},
                };



            }
        }
    }
}