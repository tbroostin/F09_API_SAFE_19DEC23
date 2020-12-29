//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class TaxFormsControllerTests
    {
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

        private Mock<ITaxFormsService> taxFormsServiceMock;
        private Mock<ILogger> loggerMock;
        private TaxFormCodesController controller;
        private IEnumerable<TaxForm> taxFormEnities;
        private IEnumerable<Dtos.ColleagueFinance.TaxForm> taxFormDtos;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            
            loggerMock = new Mock<ILogger>();
            taxFormsServiceMock = new Mock<ITaxFormsService>();
            loggerMock = new Mock<ILogger>();
            taxFormEnities = new List<TaxForm>();
            taxFormsServiceMock.Setup(m => m.GetTaxFormsAsync()).ReturnsAsync(taxFormDtos);


            controller = new TaxFormCodesController(taxFormsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            taxFormDtos = new List<Dtos.ColleagueFinance.TaxForm> { new Dtos.ColleagueFinance.TaxForm { Code ="1098T", Description = "1098-T TaxForm" },
            new Dtos.ColleagueFinance.TaxForm { Code ="T4", Description = "T4 TaxForm" }};

        }

        [TestMethod]
        public async Task TaxFormsController_GetTaxFormsAsync()
        {

            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync()).ReturnsAsync(taxFormDtos);

            var taxFormsList = (await controller.GetTaxFormsAsync()).ToList();
            Assert.AreEqual(taxFormDtos.ToList().Count, taxFormsList.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormsController_GetTaxFormsAsync_KeyNotFoundException()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync())
                .Throws<KeyNotFoundException>();
            await controller.GetTaxFormsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetTaxFormsAsync_Exception()
        {
            taxFormsServiceMock.Setup(x => x.GetTaxFormsAsync())
                .Throws<Exception>();

            await controller.GetTaxFormsAsync();
        }
    }
}
