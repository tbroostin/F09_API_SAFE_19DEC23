// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Results;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class AttachmentsControllerTests
    {
        private const string encrContentKeyHeader = "X-Encr-Content-Key";
        private const string encrIVHeader = "X-Encr-IV";
        private const string encrTypeHeader = "X-Encr-Type";
        private const string encrKeyIdHeader = "X-Encr-Key-Id";

        private Mock<ILogger> loggerMock;
        private Mock<IAttachmentService> attachmentService;

        private AttachmentsController attachmentsController;
        private List<Attachment> attachments;

        private ApiSettings apiSettings;

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void AttachmentsControllerTestsInitialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            attachmentService = new Mock<IAttachmentService>();
            attachments = new List<Attachment>
            {
                new Attachment()
                {
                    Id = "32c2d2ec-d91d-4c86-be0d-6e38b5b0f1b7",
                    CollectionId = "24671c69-4a1d-47b9-8c28-06ade7f995c5",
                    ContentType = "application/pdf",
                    Owner = "0003896",
                    Name = "mypdf.pdf"
                },
                new Attachment()
                {
                    Id = "16a9e995-cfb9-444f-b34a-f4b25b98a188",
                    CollectionId = "97b91db7-c0a6-4e48-83ce-8f0032959b59",
                    ContentType = "application/pdf",
                    Owner = "0009999",
                    Name = "mypdf2.pdf"
                }
            };
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");
            attachmentsController = new AttachmentsController(attachmentService.Object, loggerMock.Object, apiSettings)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            attachmentService = null;
            attachmentsController = null;
            attachments = null;
        }

        #endregion

        [TestClass]
        public class AttachmentsController_GetAttachmentsAsyncTests : AttachmentsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AttachmentsControllerTestsInitialize();
            }

            // GetAttachmentsAsync success
            [TestMethod]
            public async Task AttachmentsController_GetAttachmentsAsyncSuccess()
            {
                attachmentService.Setup(x => x.GetAttachmentsAsync(string.Empty, string.Empty, string.Empty)).ReturnsAsync(attachments);
                var actual = await attachmentsController.GetAttachmentsAsync(string.Empty, string.Empty, string.Empty);
                Assert.AreEqual(attachments.Count(), actual.Count());
            }

            // GetAttachmentsAsync permissions exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_GetAttachmentsAsync_PermissionsException()
            {
                try
                {
                    attachmentService.Setup(x => x.GetAttachmentsAsync(string.Empty, string.Empty, string.Empty)).Throws<PermissionsException>();
                    await attachmentsController.GetAttachmentsAsync(string.Empty, string.Empty, string.Empty);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            // GetAttachmentsAsync general exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_GetAttachmentsAsync_Exception()
            {
                try
                {
                    attachmentService.Setup(x => x.GetAttachmentsAsync(string.Empty, string.Empty, string.Empty)).Throws<Exception>();
                    await attachmentsController.GetAttachmentsAsync(string.Empty, string.Empty, string.Empty);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class AttachmentsController_GetAttachmentContentAsyncTests : AttachmentsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AttachmentsControllerTestsInitialize();
            }

            // GetAttachmentContentAsync success
            [TestMethod]
            public async Task AttachmentsController_GetAttachmentContentAsync_Success()
            {
                attachmentService.Setup(x => x.GetAttachmentContentAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Tuple<Attachment, string, AttachmentEncryption>(attachments.First(), "test", null));
                var actual = (FileContentActionResult)await attachmentsController.GetAttachmentContentAsync("test");
                Assert.AreEqual(attachments.First().ContentType, actual.ContentType);
                Assert.AreEqual(attachments.First().Name, actual.FileName);
                Assert.AreEqual("test", actual.FilePath);

                // encryption header must not be set
                string headerValue;
                var result = actual.CustomHeaders.TryGetValue(encrKeyIdHeader, out headerValue);
                Assert.AreEqual(false, result);
                result = actual.CustomHeaders.TryGetValue(encrIVHeader, out headerValue);
                Assert.AreEqual(false, result);
                result = actual.CustomHeaders.TryGetValue(encrContentKeyHeader, out headerValue);
                Assert.AreEqual(false, result);
                result = actual.CustomHeaders.TryGetValue(encrTypeHeader, out headerValue);
                Assert.AreEqual(false, result);
            }

            // GetAttachmentContentAsync success w/ ecryption properties
            [TestMethod]
            public async Task AttachmentsController_GetAttachmentContentAsync_SuccessWithEncryption()
            {
                var encryptionMetadata = new AttachmentEncryption("7c655b4d-c425-4aff-af98-337004ec8cfe", "AES256",
                    Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA=="),
                    Convert.FromBase64String("d7ztE4TNClbBVp4tbRb94w=="));
                attachmentService.Setup(x => x.GetAttachmentContentAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Tuple<Attachment, string, AttachmentEncryption>(attachments.First(), "test", encryptionMetadata));
                var actual = (FileContentActionResult) await attachmentsController.GetAttachmentContentAsync("test");
                Assert.AreEqual(attachments.First().ContentType, actual.ContentType);
                Assert.AreEqual(attachments.First().Name, actual.FileName);
                Assert.AreEqual("test", actual.FilePath);

                // encryption metadata
                string headerValue;
                actual.CustomHeaders.TryGetValue(encrKeyIdHeader, out headerValue);
                Assert.AreEqual(encryptionMetadata.EncrKeyId, headerValue);
                actual.CustomHeaders.TryGetValue(encrIVHeader, out headerValue);
                Assert.AreEqual(Convert.ToBase64String(encryptionMetadata.EncrIV), headerValue);
                actual.CustomHeaders.TryGetValue(encrContentKeyHeader, out headerValue);
                Assert.AreEqual(Convert.ToBase64String(encryptionMetadata.EncrContentKey), headerValue);
                actual.CustomHeaders.TryGetValue(encrTypeHeader, out headerValue);
                Assert.AreEqual(encryptionMetadata.EncrType, headerValue);
            }

            // GetAttachmentContentAsync permissions exception
            [TestMethod]
            public async Task AttachmentsController_GetAttachmentContentAsync_PermissionsException()
            {
                try
                {
                    attachmentService.Setup(x => x.GetAttachmentContentAsync(It.IsAny<string>())).Throws<PermissionsException>();
                    await attachmentsController.GetAttachmentContentAsync("test");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                }
            }

            // GetAttachmentContentAsync keynotfound exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_GetAttachmentContentAsync_KeyNotFoundException()
            {
                try
                {
                    attachmentService.Setup(x => x.GetAttachmentContentAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
                    await attachmentsController.GetAttachmentContentAsync("test");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw;
                }
            }

            // GetAttachmentContentAsync general exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_GetAttachmentContentAsync_Exception()
            {
                try
                {
                    attachmentService.Setup(x => x.GetAttachmentContentAsync(It.IsAny<string>())).Throws<Exception>();
                    await attachmentsController.GetAttachmentContentAsync("test");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class AttachmentsController_PostAsyncTests : AttachmentsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AttachmentsControllerTestsInitialize();
            }

            // PostAsync success
            [TestMethod]
            public async Task AttachmentsController_PostAsyncSuccess()
            {
                attachmentsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                attachmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                attachmentsController.Request.Content = new StringContent(JsonConvert.SerializeObject(attachments.First()));
                attachmentsController.Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                attachmentService.Setup(x => x.PostAttachmentAsync(It.IsAny<Attachment>(), It.IsAny<AttachmentEncryption>())).ReturnsAsync(attachments.First());
                var actual = await attachmentsController.PostAsync();
                Assert.AreEqual(attachments.First().Id, actual.Id);
            }

            // PostAsync success w/ encryption metadata
            [TestMethod]
            public async Task AttachmentsController_PostAsyncSuccessWithEncryption()
            {
                attachmentsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                attachmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                attachmentsController.Request.Content = new StringContent(JsonConvert.SerializeObject(attachments.First()));
                attachmentsController.Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                attachmentsController.Request.Headers.Add(encrKeyIdHeader, "7c655b4d-c425-4aff-af98-337004ec8cfe");
                attachmentsController.Request.Headers.Add(encrTypeHeader, "AES256");
                attachmentsController.Request.Headers.Add(encrContentKeyHeader, "zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA==");
                attachmentsController.Request.Headers.Add(encrIVHeader, "d7ztE4TNClbBVp4tbRb94w==");

                attachmentService.Setup(x => x.PostAttachmentAsync(It.IsAny<Attachment>(), It.IsAny<AttachmentEncryption>())).ReturnsAsync(attachments.First());
                var actual = await attachmentsController.PostAsync();
                Assert.AreEqual(attachments.First().Id, actual.Id);
            }

            // PostAsync permissions exception
            [TestMethod]
            public async Task AttachmentsController_PostAsync_PermissionsException()
            {
                try
                {
                    attachmentsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                    attachmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                    attachmentsController.Request.Content = new StringContent(JsonConvert.SerializeObject(attachments.First()));
                    attachmentsController.Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    attachmentService.Setup(x => x.PostAttachmentAsync(It.IsAny<Attachment>(), It.IsAny<AttachmentEncryption>())).Throws<PermissionsException>();
                    await attachmentsController.PostAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                }
            }

            // PostAsync general exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_PostAsync_Exception()
            {
                try
                {
                    attachmentService.Setup(x => x.PostAttachmentAsync(attachments.First(), It.IsAny<AttachmentEncryption>())).Throws<Exception>();
                    await attachmentsController.PostAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class AttachmentsController_PutAsyncTests : AttachmentsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AttachmentsControllerTestsInitialize();
            }

            [TestMethod]
            public async Task AttachmentsController_PutAsyncSuccess()
            {
                attachmentService.Setup(x => x.PutAttachmentAsync(It.IsAny<string>(), It.IsAny<Attachment>())).ReturnsAsync(attachments.First());

                var attachment = attachments.First();
                var actual = await attachmentsController.PutAsync(attachment.Id, attachment);
                Assert.AreEqual(attachment.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_PutAsync_PermissionsException()
            {
                try
                {
                    attachmentService.Setup(x => x.PutAttachmentAsync(It.IsAny<string>(), It.IsAny<Attachment>())).Throws<PermissionsException>();

                    var attachment = attachments.First();
                    await attachmentsController.PutAsync(attachment.Id, attachment);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_PutAsync_KeyNotFoundException()
            {
                try
                {
                    attachmentService.Setup(x => x.PutAttachmentAsync(It.IsAny<string>(), It.IsAny<Attachment>())).Throws<KeyNotFoundException>();

                    var attachment = attachments.First();
                    await attachmentsController.PutAsync(attachment.Id, attachment);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_PutAsync_Exception()
            {
                try
                {
                    attachmentService.Setup(x => x.PutAttachmentAsync(It.IsAny<string>(), It.IsAny<Attachment>())).Throws<Exception>();

                    var attachment = attachments.First();
                    await attachmentsController.PutAsync(attachment.Id, attachment);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class AttachmentsController_DeleteAttachmentAsyncTests : AttachmentsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AttachmentsControllerTestsInitialize();
            }

            // DeleteAsync success
            [TestMethod]
            public async Task AttachmentsController_DeleteAsyncSuccess()
            {
                attachmentService.Setup(x => x.DeleteAttachmentAsync(attachments.First().Id)).Returns(Task.FromResult<object>(null));
                var response = await attachmentsController.DeleteAsync(attachments.First().Id);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }

            // DeleteAsync permissions exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_DeleteAsync_PermissionsException()
            {
                try
                {
                    attachmentService.Setup(x => x.DeleteAttachmentAsync(attachments.First().Id)).Throws<PermissionsException>();
                    await attachmentsController.DeleteAsync(attachments.First().Id);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            // DeleteAsync general exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_DeleteAsync_Exception()
            {
                try
                {
                    attachmentService.Setup(x => x.DeleteAttachmentAsync(attachments.First().Id)).Throws<Exception>();
                    await attachmentsController.DeleteAsync(attachments.First().Id);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            // DeleteAsync keynotfound exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AttachmentsController_DeleteAsync_KeyNotFoundException()
            {
                try
                {
                    attachmentService.Setup(x => x.DeleteAttachmentAsync(attachments.First().Id)).Throws<KeyNotFoundException>();
                    await attachmentsController.DeleteAsync(attachments.First().Id);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}