//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class HumanResourcesTaxFormStatementsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IHumanResourcesTaxFormStatementService> taxFormStatementServiceMock;
        private Mock<ILogger> loggerMock;
        private HumanResourcesTaxFormStatementsController controller;
        private string personId = "0000001";
        private List<Dtos.Base.TaxFormStatement2> taxFormStatement2List;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            taxFormStatementServiceMock = new Mock<IHumanResourcesTaxFormStatementService>();
            loggerMock = new Mock<ILogger>();

            taxFormStatement2List = new List<Dtos.Base.TaxFormStatement2>();


            controller = new HumanResourcesTaxFormStatementsController(adapterRegistryMock.Object, loggerMock.Object, taxFormStatementServiceMock.Object)
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

        #region GetW2Async controller method tests
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetW2Async_PersonIDNullException()
        {
            try
            {
                await controller.GetW2Async(null);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Person ID must be specified.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetW2Async_PersonIDEmptyException()
        {
            try
            {
                await controller.GetW2Async("");
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Person ID must be specified.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetW2Async_PermissionsException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormW2)).Throws<PermissionsException>();
            try
            {
                await controller.GetW2Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to access W-2 tax form statements.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetW2Async_ArgumentOutOfRangeException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormW2)).Throws<System.ArgumentOutOfRangeException>();
            try
            {
                await controller.GetW2Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Invalid data.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetW2Async_ArgumentNullException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormW2)).Throws<System.ArgumentNullException>();
            try
            {
                await controller.GetW2Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Invalid argument.", responseJson.Message);
                throw;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetW2Async_Exception()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormW2)).Throws<System.Exception>();
            try
            {
                await controller.GetW2Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the W-2 statements", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task HumanResourcesTaxFormStatementsController_GetW2Async()
        {
            taxFormStatement2List = BuildTaxFormStatementsList();
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormW2)).ReturnsAsync(taxFormStatement2List);
            var result = await controller.GetW2Async(personId);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ToList().Count);
            Assert.AreEqual(Dtos.Base.TaxForms.FormW2, result.ToList().First().TaxForm);
            Assert.AreEqual("10", result.ToList().First().PdfRecordId);
            Assert.AreEqual(personId, result.ToList().First().PersonId);
            Assert.AreEqual("2017", result.ToList().First().TaxYear);
        }

        #endregion

        #region Get1095cAsync controller method tests

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_Get1095cAsync_PersonIDNullException()
        {
            try
            {
                await controller.Get1095cAsync(null);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Person ID must be specified.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_Get1095cAsync_PersonIDEmptyException()
        {
            try
            {
                await controller.Get1095cAsync("");
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Person ID must be specified.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_Get1095cAsync_PermissionsException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1095C)).Throws<PermissionsException>();
            try
            {
                await controller.Get1095cAsync(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to access 1095C tax form statements.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_Get1095cAsync_ArgumentOutOfRangesException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1095C)).Throws<System.ArgumentOutOfRangeException>();
            try
            {
                await controller.Get1095cAsync(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Invalid data.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_Get1095cAsync_ArgumentNullException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1095C)).Throws<System.ArgumentNullException>();
            try
            {
                await controller.Get1095cAsync(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Invalid argument.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_Get1095cAsync_Exception()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1095C)).Throws<System.Exception>();
            try
            {
                await controller.Get1095cAsync(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the 1095-C statements", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task HumanResourcesTaxFormStatementsController_Get1099MIAsync()
        {
            taxFormStatement2List = BuildTaxFormStatementsList(Dtos.Base.TaxForms.Form1095C);
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1095C)).ReturnsAsync(taxFormStatement2List);
            var result = await controller.Get1095cAsync(personId);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ToList().Count);
            Assert.AreEqual(Dtos.Base.TaxForms.Form1095C, result.ToList().First().TaxForm);
            Assert.AreEqual("10", result.ToList().First().PdfRecordId);
            Assert.AreEqual(personId, result.ToList().First().PersonId);
            Assert.AreEqual("2017", result.ToList().First().TaxYear);
        }

        #endregion

        #region GetT4Async controller method tests

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetT4Async_PersonIDNullException()
        {
            try
            {
                await controller.GetT4Async(null);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Person ID must be specified.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetT4Async_PersonIDEmptyException()
        {
            try
            {
                await controller.GetT4Async("");
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Person ID must be specified.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetT4Async_PermissionsException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4)).Throws<PermissionsException>();
            try
            {
                await controller.GetT4Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to access T4 tax form statements.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetT4Async_ArgumentOutOfRangesException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4)).Throws<System.ArgumentOutOfRangeException>();
            try
            {
                await controller.GetT4Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Invalid data.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetT4Async_ArgumentNullException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4)).Throws<System.ArgumentNullException>();
            try
            {
                await controller.GetT4Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Invalid argument.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HumanResourcesTaxFormStatementsController_GetT4Async_Exception()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4)).Throws<System.Exception>();
            try
            {
                await controller.GetT4Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the T4 statements", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task HumanResourcesTaxFormStatementsController_GetT4Async()
        {
            taxFormStatement2List = BuildTaxFormStatementsList(Dtos.Base.TaxForms.FormT4);
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT4)).ReturnsAsync(taxFormStatement2List);
            var result = await controller.GetT4Async(personId);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ToList().Count);
            Assert.AreEqual(Dtos.Base.TaxForms.FormT4, result.ToList().First().TaxForm);
            Assert.AreEqual("10", result.ToList().First().PdfRecordId);
            Assert.AreEqual(personId, result.ToList().First().PersonId);
            Assert.AreEqual("2017", result.ToList().First().TaxYear);
        }

        #endregion

        private List<Dtos.Base.TaxFormStatement2> BuildTaxFormStatementsList(Dtos.Base.TaxForms taxFormEnum = Dtos.Base.TaxForms.FormW2, string year = "2017", string pdfRecordId = "10")
        {
            var list = new List<Dtos.Base.TaxFormStatement2>();
            Dtos.Base.TaxFormStatement2 taxStatement = BuildTaxFormStatements2(taxFormEnum, year, pdfRecordId);
            list.Add(taxStatement);
            return list;
        }
        private Dtos.Base.TaxFormStatement2 BuildTaxFormStatements2(Dtos.Base.TaxForms taxFormEnum = Dtos.Base.TaxForms.FormW2, string year = "2017", string pdfRecordId = "10")
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