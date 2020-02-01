// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class ContentKeysControllerTests
    {
        private Mock<IAdapterRegistry> adapterRegistry;
        private Mock<ILogger> loggerMock;
        private Mock<IContentKeyService> contentKeyService;
        private ContentKeysController contentKeysController;

        private ContentKey fakeContentKey;
        private ContentKeyRequest fakeContentKeyRequest;

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void ContentKeysControllerTestsInitialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            adapterRegistry = new Mock<IAdapterRegistry>();
            contentKeyService = new Mock<IContentKeyService>();
            loggerMock = new Mock<ILogger>();
            contentKeysController = new ContentKeysController(adapterRegistry.Object, contentKeyService.Object, loggerMock.Object);

            fakeContentKey = new ContentKey()
            {
                EncryptionKeyId = "7c655b4d-c425-4aff-af98-337004ec8cfe",
                EncryptedKey = Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                        + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                        + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                        + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                        + "2ZxLIvqA=="),
                Key = Convert.FromBase64String("KRiO9vG1XXoWgtb9GkTp3McpoRcL8yMcST/TFETXdf4=")
            };
            fakeContentKeyRequest = new ContentKeyRequest()
            {
                EncryptionKeyId = "7c655b4d-c425-4aff-af98-337004ec8cfe",
                EncryptedKey = Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA==")
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            contentKeyService = null;
            contentKeysController = null;
            adapterRegistry = null;
        }

        #endregion

        [TestClass]
        public class ContentKeysController_GetContentKeyAsyncTests : ContentKeysControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.ContentKeysControllerTestsInitialize();
            }

            [TestMethod]
            public async Task ContentKeysController_GetContentKeyAsync_Success()
            {
                contentKeyService.Setup(x => x.GetContentKeyAsync(It.IsAny<string>())).ReturnsAsync(fakeContentKey);
                var actual = await contentKeysController.GetContentKeyAsync(fakeContentKey.EncryptionKeyId);
                var actualJson = JsonConvert.SerializeObject(actual);
                var expectedJson = JsonConvert.SerializeObject(fakeContentKey);
                Assert.AreEqual(expectedJson, actualJson);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ContentKeysController_GetContentKeyAsync_KeyNotFoundException()
            {
                try
                {
                    contentKeyService.Setup(x => x.GetContentKeyAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                    await contentKeysController.GetContentKeyAsync(fakeContentKey.EncryptionKeyId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ContentKeysController_GetContentKeyAsync_Exception()
            {
                try
                {
                    contentKeyService.Setup(x => x.GetContentKeyAsync(It.IsAny<string>())).Throws(new Exception());
                    await contentKeysController.GetContentKeyAsync(fakeContentKey.EncryptionKeyId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class ContentKeysController_PostContentKeyAsyncTests : ContentKeysControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.ContentKeysControllerTestsInitialize();
            }

            [TestMethod]
            public async Task ContentKeysController_PostContentKeyAsync_Success()
            {
                contentKeyService.Setup(x => x.PostContentKeyAsync(It.IsAny<ContentKeyRequest>())).ReturnsAsync(fakeContentKey);
                var actual = await contentKeysController.PostContentKeyAsync(fakeContentKeyRequest);
                var actualJson = JsonConvert.SerializeObject(actual);
                var expectedJson = JsonConvert.SerializeObject(fakeContentKey);
                Assert.AreEqual(expectedJson, actualJson);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ContentKeysController_PostContentKeyAsync_KeyNotFoundException()
            {
                try
                {
                    contentKeyService.Setup(x => x.PostContentKeyAsync(It.IsAny<ContentKeyRequest>())).Throws(new KeyNotFoundException());
                    await contentKeysController.PostContentKeyAsync(fakeContentKeyRequest);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ContentKeysController_PostContentKeyAsync_Exception()
            {
                try
                {
                    contentKeyService.Setup(x => x.PostContentKeyAsync(It.IsAny<ContentKeyRequest>())).Throws(new Exception());
                    await contentKeysController.PostContentKeyAsync(fakeContentKeyRequest);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}