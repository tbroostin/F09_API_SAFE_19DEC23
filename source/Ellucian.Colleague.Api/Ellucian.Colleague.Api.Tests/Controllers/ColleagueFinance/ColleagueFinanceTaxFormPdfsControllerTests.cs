//Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ColleagueFinanceTaxFormPdfsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IColleagueFinanceTaxFormPdfService> taxFormPdfServiceMock;
        private Mock<ITaxFormConsentService> taxFormConsentServiceMock;
        private Mock<IConfigurationService> configurationServiceMock;

        private Mock<ILogger> loggerMock;
        private ColleagueFinanceTaxFormPdfsController controller;
        private string personId = "0000001";
        private List<Dtos.Base.TaxFormConsent> taxFormConsentList;
        private Form1099MIPdfData form1099MiData;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            taxFormPdfServiceMock = new Mock<IColleagueFinanceTaxFormPdfService>();
            taxFormConsentServiceMock = new Mock<ITaxFormConsentService>();
            configurationServiceMock = new Mock<IConfigurationService>();
            loggerMock = new Mock<ILogger>();


            taxFormConsentList = new List<Dtos.Base.TaxFormConsent>();
            taxFormConsentServiceMock.Setup(m => m.GetAsync(It.IsAny<string>(), Dtos.Base.TaxForms.Form1099MI)).ReturnsAsync(taxFormConsentList);

            form1099MiData = new Form1099MIPdfData("2017", "Ellucian University");
            taxFormPdfServiceMock.Setup(m => m.Get1099MiscPdfDataAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(form1099MiData);

            taxFormPdfServiceMock.Setup(m => m.Populate1099MiscPdf(It.IsAny<Form1099MIPdfData>(), It.IsAny<string>())).Returns(new byte[0]);
            controller = new ColleagueFinanceTaxFormPdfsController(adapterRegistryMock.Object, loggerMock.Object, taxFormPdfServiceMock.Object, taxFormConsentServiceMock.Object, configurationServiceMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }
        [TestCleanup]
        public void Cleanup()
        {
            taxFormPdfServiceMock = null;
            taxFormConsentServiceMock = null;
            controller = null;
            adapterRegistryMock = null;
            loggerMock = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormPdfsController_Get1099MiTaxFormPdfAsync_PersonIDNullException()
        {
            await controller.Get1099MiscTaxFormPdfAsync(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormPdfsController_Get1099MiTaxFormPdfAsync_PersonIDEmptyException()
        {
            await controller.Get1099MiscTaxFormPdfAsync("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormPdfsController_Get1099MiTaxFormPdfAsync_RecordIdNullException()
        {
            await controller.Get1099MiscTaxFormPdfAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormPdfsController_Get1099MiTaxFormPdfAsync_RecordIdEmptyException()
        {
            await controller.Get1099MiscTaxFormPdfAsync(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task ColleagueFinanceTaxFormPdfsController_Get1099MiTaxFormPdfAsync_ConsentNotFoundException()
        {            
            await controller.Get1099MiscTaxFormPdfAsync(personId, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task ColleagueFinanceTaxFormPdfsController_Get1099MiTaxFormPdfAsync_ConsentWithheldException()
        {
            taxFormConsentList.Add(BuildTaxFormConsent(hasConsented: false));
            taxFormConsentServiceMock.Setup(m => m.GetAsync(It.IsAny<string>(), Dtos.Base.TaxForms.Form1099MI)).ReturnsAsync(taxFormConsentList);
            await controller.Get1099MiscTaxFormPdfAsync(personId, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormPdfsController_Get1099MiTaxFormPdfAsync_TemplateException()
        {
            InitData();
            await controller.Get1099MiscTaxFormPdfAsync(personId, "1");
        }
        
         
        private void InitData()
        {
            taxFormConsentList.Add(BuildTaxFormConsent());
            taxFormConsentServiceMock.Setup(m => m.GetAsync(It.IsAny<string>(), Dtos.Base.TaxForms.Form1099MI)).ReturnsAsync(taxFormConsentList);
        }

        private Dtos.Base.TaxFormConsent BuildTaxFormConsent(Dtos.Base.TaxForms taxFormEnum = Dtos.Base.TaxForms.Form1099MI, string year = "2017", string pdfRecordId = "10", bool hasConsented = true)
        {
            Dtos.Base.TaxFormConsent taxStatement = new Dtos.Base.TaxFormConsent()
            {
                TaxForm = taxFormEnum,
                PersonId = personId,
                HasConsented = hasConsented,
                TimeStamp = DateTime.Now
            };
            return taxStatement;
        }
    }
}
