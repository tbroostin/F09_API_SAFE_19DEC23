//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class TaxFormConsentsControllerTests
    {
        private Mock<IAdapterRegistry> adapterRegistry;
        private Mock<ILogger> loggerMock;
        private Mock<ITaxFormConsentService> taxFormConsentService;

        private TaxFormConsentsController taxFormConsentsController;
        private List<TaxFormConsent> taxFormConsentCollection;

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
            taxFormConsentService = new Mock<ITaxFormConsentService>();
            taxFormConsentCollection = new List<TaxFormConsent>();
            loggerMock = new Mock<ILogger>();
            taxFormConsentsController = new TaxFormConsentsController(adapterRegistry.Object, loggerMock.Object, taxFormConsentService.Object);
        }

        [TestCleanup]
        public void cleanup()
        {
            taxFormConsentService = null;
            taxFormConsentsController = null;
            taxFormConsentCollection = null;
            adapterRegistry = null;
        }

        private void BuildTaxFormConsentDto(TaxForms currentTaxForm = TaxForms.Form1099MI)
        {
            taxFormConsentCollection.Add(new TaxFormConsent()
            {
                HasConsented = true,
                PersonId = "000007",
                TaxForm = currentTaxForm,
                TimeStamp = DateTime.Now
            });
        }

        #region POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_SubmitConsent_PermissionsException()
        {
            taxFormConsentService.Setup(x => x.PostAsync(taxFormConsentCollection.FirstOrDefault())).Throws<PermissionsException>();
            try
            {
                await taxFormConsentsController.PostAsync(taxFormConsentCollection.FirstOrDefault());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to save tax form consents.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_SubmitConsent_NullArgumentException()
        {
            taxFormConsentService.Setup(x => x.PostAsync(null)).Throws<ArgumentNullException>();
            try
            {
                await taxFormConsentsController.PostAsync(null);
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
        public async Task TaxFormConsentsController_SubmitConsent_CatchAllException()
        {
            taxFormConsentService.Setup(x => x.PostAsync(taxFormConsentCollection.FirstOrDefault())).Throws<ApplicationException>();
            try
            {
                await taxFormConsentsController.PostAsync(taxFormConsentCollection.FirstOrDefault());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to save the tax form consent.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task TaxFormConsentsController_SubmitConsentForT4()
        {
            BuildTaxFormConsentDto(TaxForms.FormT4);
            taxFormConsentService.Setup(x => x.PostAsync(taxFormConsentCollection.FirstOrDefault())).ReturnsAsync(taxFormConsentCollection.FirstOrDefault());
            var result = (await taxFormConsentsController.PostAsync(taxFormConsentCollection.FirstOrDefault()));
            Assert.AreEqual(result.HasConsented, result.HasConsented, "HasConsented");
            Assert.AreEqual(result.PersonId, result.PersonId, "PersonId");
            Assert.AreEqual(result.TaxForm, result.TaxForm, "TaxForm");
            Assert.AreEqual(result.TimeStamp, result.TimeStamp, "TimeStamp");
        }

        [TestMethod]
        public async Task TaxFormConsentsController_SubmitConsentFor1099Misc()
        {
            BuildTaxFormConsentDto(TaxForms.Form1099MI);
            taxFormConsentService.Setup(x => x.PostAsync(taxFormConsentCollection.FirstOrDefault())).ReturnsAsync(taxFormConsentCollection.FirstOrDefault());
            var result = (await taxFormConsentsController.PostAsync(taxFormConsentCollection.FirstOrDefault()));
            Assert.AreEqual(result.HasConsented, result.HasConsented, "HasConsented");
            Assert.AreEqual(result.PersonId, result.PersonId, "PersonId");
            Assert.AreEqual(result.TaxForm, result.TaxForm, "TaxForm");
            Assert.AreEqual(result.TimeStamp, result.TimeStamp, "TimeStamp");
        }

        [TestMethod]
        public async Task TaxFormConsentsController_SubmitConsentForT4A()
        {
            BuildTaxFormConsentDto(TaxForms.FormT4A);
            taxFormConsentService.Setup(x => x.PostAsync(taxFormConsentCollection.FirstOrDefault())).ReturnsAsync(taxFormConsentCollection.FirstOrDefault());
            var result = (await taxFormConsentsController.PostAsync(taxFormConsentCollection.FirstOrDefault()));
            Assert.AreEqual(result.HasConsented, result.HasConsented, "HasConsented");
            Assert.AreEqual(result.PersonId, result.PersonId, "PersonId");
            Assert.AreEqual(result.TaxForm, result.TaxForm, "TaxForm");
            Assert.AreEqual(result.TimeStamp, result.TimeStamp, "TimeStamp");
        }

        [TestMethod]
        public async Task TaxFormConsentsController_SubmitConsentFor1098()
        {
            BuildTaxFormConsentDto(TaxForms.Form1098);
            taxFormConsentService.Setup(x => x.PostAsync(taxFormConsentCollection.FirstOrDefault())).ReturnsAsync(taxFormConsentCollection.FirstOrDefault());
            var result = (await taxFormConsentsController.PostAsync(taxFormConsentCollection.FirstOrDefault()));
            Assert.AreEqual(result.HasConsented, result.HasConsented, "HasConsented");
            Assert.AreEqual(result.PersonId, result.PersonId, "PersonId");
            Assert.AreEqual(result.TaxForm, result.TaxForm, "TaxForm");
            Assert.AreEqual(result.TimeStamp, result.TimeStamp, "TimeStamp");
        }
        #endregion

        #region GET

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_GetConsent_PersonIDNullException()
        {
            try
            {
                var result = await taxFormConsentsController.GetAsync(null, TaxForms.Form1099MI);
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
        public async Task TaxFormConsentsController_GetConsent_PersonIDEmptyException()
        {
            try
            {
                var result = await taxFormConsentsController.GetAsync("", TaxForms.Form1099MI);
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
        public async Task TaxFormConsentsController_GetConsent_PermissionsException()
        {
            BuildTaxFormConsentDto();
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).Throws<PermissionsException>();
            try
            {
                var result = await taxFormConsentsController.GetAsync(currentConsent.PersonId, TaxForms.Form1099MI);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to access tax form consents.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_GetConsent_ArgumentNullException()
        {
            BuildTaxFormConsentDto();
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).Throws<ArgumentNullException>();
            try
            {
                var result = await taxFormConsentsController.GetAsync(currentConsent.PersonId, TaxForms.Form1099MI);
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
        public async Task TaxFormConsentsController_GetConsent_Exception()
        {
            BuildTaxFormConsentDto();
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).Throws<ApplicationException>();
            try
            {
                var result = await taxFormConsentsController.GetAsync(currentConsent.PersonId, TaxForms.Form1099MI);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the tax form consent", responseJson.Message);
                throw;
            }

        }

        [TestMethod]
        public async Task TaxFormConsentsController_GetConsentForT4Async()
        {
            BuildTaxFormConsentDto(TaxForms.FormT4);
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).ReturnsAsync(taxFormConsentCollection);
            var result = await taxFormConsentsController.GetAsync(currentConsent.PersonId, currentConsent.TaxForm);            
            Assert.AreEqual(result.FirstOrDefault().HasConsented, currentConsent.HasConsented, "HasConsented");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().PersonId, currentConsent.PersonId, "PersonId");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TaxForm, currentConsent.TaxForm, "TaxForm");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TimeStamp, currentConsent.TimeStamp, "TimeStamp");
        }

        [TestMethod]
        public async Task TaxFormConsentsController_GetConsentFor1099MiscAsync()
        {
            BuildTaxFormConsentDto(TaxForms.Form1099MI);
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).ReturnsAsync(taxFormConsentCollection);
            var result = await taxFormConsentsController.GetAsync(currentConsent.PersonId, currentConsent.TaxForm);
            Assert.AreEqual(result.FirstOrDefault().HasConsented, currentConsent.HasConsented, "HasConsented");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().PersonId, currentConsent.PersonId, "PersonId");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TaxForm, currentConsent.TaxForm, "TaxForm");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TimeStamp, currentConsent.TimeStamp, "TimeStamp");
        }

        [TestMethod]
        public async Task TaxFormConsentsController_GetConsentForT4aAsync()
        {
            BuildTaxFormConsentDto(TaxForms.FormT4A);
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).ReturnsAsync(taxFormConsentCollection);
            var result = await taxFormConsentsController.GetAsync(currentConsent.PersonId, currentConsent.TaxForm);
            Assert.AreEqual(result.FirstOrDefault().HasConsented, currentConsent.HasConsented, "HasConsented");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().PersonId, currentConsent.PersonId, "PersonId");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TaxForm, currentConsent.TaxForm, "TaxForm");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TimeStamp, currentConsent.TimeStamp, "TimeStamp");
        }

        [TestMethod]
        public async Task TaxFormConsentsController_GetConsentFor1098Async()
        {
            BuildTaxFormConsentDto(TaxForms.Form1098);
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).ReturnsAsync(taxFormConsentCollection);
            var result = await taxFormConsentsController.GetAsync(currentConsent.PersonId, currentConsent.TaxForm);
            Assert.AreEqual(result.FirstOrDefault().HasConsented, currentConsent.HasConsented, "HasConsented");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().PersonId, currentConsent.PersonId, "PersonId");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TaxForm, currentConsent.TaxForm, "TaxForm");
            Assert.AreEqual(taxFormConsentCollection.FirstOrDefault().TimeStamp, currentConsent.TimeStamp, "TimeStamp");
        }
        #endregion
    }
}
