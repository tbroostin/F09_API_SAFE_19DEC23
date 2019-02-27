//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentTaxFormStatementsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IStudentTaxFormStatementService> taxFormStatementServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentTaxFormStatementsController controller;
        private string personId = "0000001";
        private List<Dtos.Base.TaxFormStatement2> taxFormStatement2List;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            taxFormStatementServiceMock = new Mock<IStudentTaxFormStatementService>();
            loggerMock = new Mock<ILogger>();

            taxFormStatement2List = new List<Dtos.Base.TaxFormStatement2>();


            controller = new StudentTaxFormStatementsController(adapterRegistryMock.Object, loggerMock.Object, taxFormStatementServiceMock.Object)
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

        #region Get1098Async controller method tests
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTaxFormStatementsController_Get1098Async_PersonIDNullException()
        {
            try
            {
                await controller.Get1098Async(null);
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
        public async Task StudentTaxFormStatementsController_Get1098Async_PersonIDEmptyException()
        {
            try
            {
                await controller.Get1098Async("");
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
        public async Task StudentTaxFormStatementsController_Get1098Async_PermissionsException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1098)).Throws<PermissionsException>();
            try
            {
                await controller.Get1098Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to view 1098 data.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTaxFormStatementsController_Get1098Async_ArgumentOutOfRangeException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1098)).Throws<System.ArgumentOutOfRangeException>();
            try
            {
                await controller.Get1098Async(personId);
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
        public async Task StudentTaxFormStatementsController_Get1098Async_ArgumentNullException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1098)).Throws<System.ArgumentNullException>();
            try
            {
                await controller.Get1098Async(personId);
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
        public async Task StudentTaxFormStatementsController_Get1098Async_Exception()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1098)).Throws<System.Exception>();
            try
            {
                await controller.Get1098Async(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the 1098 statements.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task StudentTaxFormStatementsController_Get1098Async()
        {
            taxFormStatement2List = BuildTaxFormStatementsList();
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.Form1098)).ReturnsAsync(taxFormStatement2List);
            var result = await controller.Get1098Async(personId);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ToList().Count);
            Assert.AreEqual(Dtos.Base.TaxForms.Form1098, result.ToList().First().TaxForm);
            Assert.AreEqual("10", result.ToList().First().PdfRecordId);
            Assert.AreEqual(personId, result.ToList().First().PersonId);
            Assert.AreEqual("2017", result.ToList().First().TaxYear);
        }

        #endregion

        #region GetT2202aAsync controller method tests

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTaxFormStatementsController_GetT2202aAsync_PersonIDNullException()
        {
            try
            {
                await controller.GetT2202aAsync(null);
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
        public async Task StudentTaxFormStatementsController_GetT2202aAsync_PersonIDEmptyException()
        {
            try
            {
                await controller.GetT2202aAsync("");
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
        public async Task StudentTaxFormStatementsController_GetT2202aAsync_PermissionsException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A)).Throws<PermissionsException>();
            try
            {
                await controller.GetT2202aAsync(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to view T2202 data.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTaxFormStatementsController_GetT2202aAsync_ArgumentOutOfRangesException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A)).Throws<System.ArgumentOutOfRangeException>();
            try
            {
                await controller.GetT2202aAsync(personId);
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
        public async Task StudentTaxFormStatementsController_GetT2202aAsync_ArgumentNullException()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A)).Throws<System.ArgumentNullException>();
            try
            {
                await controller.GetT2202aAsync(personId);
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
        public async Task StudentTaxFormStatementsController_GetT2202aAsync_Exception()
        {
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A)).Throws<System.Exception>();
            try
            {
                await controller.GetT2202aAsync(personId);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the T2202 statements.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task StudentTaxFormStatementsController_GetT2202aAsync()
        {
            taxFormStatement2List = BuildTaxFormStatementsList(Dtos.Base.TaxForms.FormT2202A);
            taxFormStatementServiceMock.Setup(x => x.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A)).ReturnsAsync(taxFormStatement2List);
            var result = await controller.GetT2202aAsync(personId);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ToList().Count);
            Assert.AreEqual(Dtos.Base.TaxForms.FormT2202A, result.ToList().First().TaxForm);
            Assert.AreEqual("10", result.ToList().First().PdfRecordId);
            Assert.AreEqual(personId, result.ToList().First().PersonId);
            Assert.AreEqual("2017", result.ToList().First().TaxYear);
        }

        #endregion


        private List<Dtos.Base.TaxFormStatement2> BuildTaxFormStatementsList(Dtos.Base.TaxForms taxFormEnum = Dtos.Base.TaxForms.Form1098, string year = "2017", string pdfRecordId = "10")
        {
            var list = new List<Dtos.Base.TaxFormStatement2>();
            Dtos.Base.TaxFormStatement2 taxStatement = BuildTaxFormStatements2(taxFormEnum, year, pdfRecordId);
            list.Add(taxStatement);
            return list;
        }
        private Dtos.Base.TaxFormStatement2 BuildTaxFormStatements2(Dtos.Base.TaxForms taxFormEnum = Dtos.Base.TaxForms.Form1098, string year = "2017", string pdfRecordId = "10")
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