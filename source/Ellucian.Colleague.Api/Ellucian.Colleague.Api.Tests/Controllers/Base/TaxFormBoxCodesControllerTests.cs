//Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class TaxFormBoxCodesControllerTests
    {
        private Mock<IAdapterRegistry> adapterRegistry;
        private Mock<ILogger> loggerMock;
        private Mock<ITaxFormBoxCodesService> taxFormBoxCodesService;

        private TaxFormBoxCodesController taxFormBoxCodessController;
        private List<TaxFormBoxCodes> taxFormBoxCodesCollection;

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            adapterRegistry = new Mock<IAdapterRegistry>();
            taxFormBoxCodesService = new Mock<ITaxFormBoxCodesService>();
            taxFormBoxCodesCollection = new List<TaxFormBoxCodes>();
            taxFormBoxCodesCollection.Add(new TaxFormBoxCodes()
            {
                Code = "1",
                Description = "Box-1"
            });
            loggerMock = new Mock<ILogger>();
            taxFormBoxCodessController = new TaxFormBoxCodesController(adapterRegistry.Object, loggerMock.Object, taxFormBoxCodesService.Object);
        }

        [TestCleanup]
        public void cleanup()
        {
            taxFormBoxCodesService = null;
            taxFormBoxCodessController = null;
            taxFormBoxCodesCollection = null;
            adapterRegistry = null;
        }

        #region GET

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormBoxCodesController_GetAllTaxFormBoxCodesAsync_Exception()
        {
            taxFormBoxCodesService.Setup(x => x.GetAllTaxFormBoxCodesAsync()).Throws<Exception>();
            var result = await taxFormBoxCodessController.GetAllTaxFormBoxCodesAsync();
        }

        [TestMethod]
        public async Task TaxFormBoxCodesController_GetAllTaxFormBoxCodesAsync()
        {
            taxFormBoxCodesService.Setup(x => x.GetAllTaxFormBoxCodesAsync()).ReturnsAsync(taxFormBoxCodesCollection);
            var result = await taxFormBoxCodessController.GetAllTaxFormBoxCodesAsync();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 0);            
            Assert.AreEqual(taxFormBoxCodesCollection.Count, result.Count());
        }
        #endregion
    }
}
