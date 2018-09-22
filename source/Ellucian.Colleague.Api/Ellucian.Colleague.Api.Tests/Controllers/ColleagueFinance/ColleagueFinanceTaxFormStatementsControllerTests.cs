//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ColleagueFinanceTaxFormStatementsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IColleagueFinanceTaxFormStatementService> taxFormStatementServiceMock;
        private Mock<ILogger> loggerMock;
        private ColleagueFinanceTaxFormStatementsController controller;
        private string personId = "0000001";
        private List<Dtos.Base.TaxFormStatement2> taxFormStatement2List;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            taxFormStatementServiceMock = new Mock<IColleagueFinanceTaxFormStatementService>();
            loggerMock = new Mock<ILogger>();

            taxFormStatement2List = new List<Dtos.Base.TaxFormStatement2>();

            
            controller = new ColleagueFinanceTaxFormStatementsController(adapterRegistryMock.Object, loggerMock.Object, taxFormStatementServiceMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            taxFormStatementServiceMock = null;
            controller = null;
            adapterRegistryMock = null;
            loggerMock = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_GetT4aAsync_PersonIDNullException()
        {
            await controller.GetT4aAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_GetT4aAsync_PersonIDEmptyException()
        {
            await controller.GetT4aAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_GetT4aAsync_PermissionsException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4A)).Throws<PermissionsException>();
            await controller.GetT4aAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_GetT4aAsync_Exception()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4A)).Throws<System.Exception>();
            await controller.GetT4aAsync(personId);
        }

        [TestMethod]
        public async Task ColleagueFinanceTaxFormStatementsController_GetT4aAsync()
        {
            taxFormStatement2List = BuildTaxFormStatementsList();
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4A)).ReturnsAsync(taxFormStatement2List);
            var result = await controller.GetT4aAsync(personId);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ToList().Count);
            Assert.AreEqual(Dtos.Base.TaxForms.FormT4A, result.ToList().First().TaxForm);
            Assert.AreEqual("10", result.ToList().First().PdfRecordId);
            Assert.AreEqual(personId, result.ToList().First().PersonId);
            Assert.AreEqual("2017", result.ToList().First().TaxYear);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_Get1099MIAsync_PersonIDNullException()
        {
            await controller.Get1099MIAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_Get1099MIAsync_PersonIDEmptyException()
        {
            await controller.Get1099MIAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_Get1099MIAsync_PermissionsException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1099MI)).Throws<PermissionsException>();
            await controller.Get1099MIAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceTaxFormStatementsController_Get1099MIAsync_Exception()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1099MI)).Throws<System.Exception>();
            await controller.Get1099MIAsync(personId);
        }

        [TestMethod]
        public async Task ColleagueFinanceTaxFormStatementsController_Get1099MIAsync()
        {
            taxFormStatement2List = BuildTaxFormStatementsList(Dtos.Base.TaxForms.Form1099MI);
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1099MI)).ReturnsAsync(taxFormStatement2List);
            var result = await controller.Get1099MIAsync(personId);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ToList().Count);
            Assert.AreEqual(Dtos.Base.TaxForms.Form1099MI, result.ToList().First().TaxForm);
            Assert.AreEqual("10", result.ToList().First().PdfRecordId);
            Assert.AreEqual(personId, result.ToList().First().PersonId);
            Assert.AreEqual("2017", result.ToList().First().TaxYear);
        }

        private List<Dtos.Base.TaxFormStatement2> BuildTaxFormStatementsList(Dtos.Base.TaxForms taxFormEnum = Dtos.Base.TaxForms.FormT4A, string year = "2017", string pdfRecordId = "10")
        {
            var list = new List<Dtos.Base.TaxFormStatement2>();
            Dtos.Base.TaxFormStatement2 taxStatement = BuildTaxFormStatements2(taxFormEnum, year, pdfRecordId);
            list.Add(taxStatement);
            return list;
        }
        private Dtos.Base.TaxFormStatement2 BuildTaxFormStatements2(Dtos.Base.TaxForms taxFormEnum = Dtos.Base.TaxForms.FormT4A, string year="2017", string pdfRecordId = "10")
        {
            Dtos.Base.TaxFormStatement2 taxStatement = new Dtos.Base.TaxFormStatement2()
            {
                PdfRecordId = pdfRecordId,
                TaxForm = taxFormEnum,
                PersonId = personId,
                TaxYear = year
            };
            return taxStatement;
        }
    }
}