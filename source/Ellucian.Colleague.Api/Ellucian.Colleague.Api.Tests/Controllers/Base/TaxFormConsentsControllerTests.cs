//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Api.Controllers.Base;
using System.Web.Http;
using Ellucian.Web.Security;
using System.Web.Http.Hosting;

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
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_SubmitConsent_NullArgument()
        {
            taxFormConsentService.Setup(x => x.PostAsync(null)).Throws<ArgumentNullException>();
            await taxFormConsentsController.PostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_SubmitConsent_PermissionException()
        {
            taxFormConsentService.Setup(x => x.PostAsync(taxFormConsentCollection.FirstOrDefault())).Throws<ApplicationException>();
            await taxFormConsentsController.PostAsync(taxFormConsentCollection.FirstOrDefault());
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
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_GetConsent_PersonIDNullException()
        {
            var result = await taxFormConsentsController.GetAsync(null, TaxForms.Form1099MI);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_GetConsent_PersonIDEmptyException()
        {
            var result = await taxFormConsentsController.GetAsync("", TaxForms.Form1099MI);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TaxFormConsentsController_GetConsent_PermissionsException()
        {
            BuildTaxFormConsentDto();
            var currentConsent = taxFormConsentCollection.FirstOrDefault();
            taxFormConsentService.Setup(x => x.GetAsync(currentConsent.PersonId, currentConsent.TaxForm)).Throws<PermissionsException>();
            var result = await taxFormConsentsController.GetAsync("", TaxForms.Form1099MI);
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
