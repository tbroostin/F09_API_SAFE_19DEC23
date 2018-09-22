// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class TextDocumentsControllerTest
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

        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IDocumentService> documentServiceMock;
        Mock<ILogger> loggerMock;

        TextDocument document;
        string documentId;
        string primaryEntity;
        string primaryId;
        string personId;

        TextDocumentsController textDocumentsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            documentServiceMock = new Mock<IDocumentService>();

            loggerMock = new Mock<ILogger>();
            documentId = "DOC";
            primaryEntity = "ENT";
            primaryId = "KEY";
            personId = "0001234";
            document = new TextDocument() { Text = new List<string>() { "This is line 1.", "This is line 2." }, Subject = "Subject" };

            textDocumentsController = new TextDocumentsController(adapterRegistryMock.Object, documentServiceMock.Object, loggerMock.Object);
            textDocumentsController.Request = new HttpRequestMessage();
            textDocumentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            textDocumentsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            document = null;
            textDocumentsController = null;
        }

        [TestMethod]
        public async Task TextDocumentsController_GetTextDocumentsAsync()
        {
            documentServiceMock.Setup(e => e.GetTextDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    null)).ReturnsAsync(document);
            var textDocumentResult = (await textDocumentsController.GetAsync(documentId, primaryEntity, primaryId, personId));
            Assert.IsNotNull(textDocumentResult);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TextDocumentsController_GetTextDocumentsAsync_Null_DocumentId()
        {
            var textDocumentResult = (await textDocumentsController.GetAsync(null, primaryEntity, primaryId, personId));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TextDocumentsController_GetTextDocumentsAsync_Null_PrimaryEntity()
        {
            var textDocumentResult = (await textDocumentsController.GetAsync(documentId, null, primaryId, personId));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TextDocumentsController_GetTextDocumentsAsync_Null_PrimaryId()
        {
            var textDocumentResult = (await textDocumentsController.GetAsync(documentId, primaryEntity, null, personId));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task TextDocumentsController_GetAddressTypeByGuidAsync_Exception()
        {
            //Arrange
            documentServiceMock.Setup(e => e.GetTextDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    null)).ThrowsAsync(new InvalidOperationException());
            //Act
            var textDocumentResult = (await textDocumentsController.GetAsync(documentId, primaryEntity, primaryId, personId));
        }
    }
}
