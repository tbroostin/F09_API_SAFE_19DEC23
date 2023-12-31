﻿//Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Net;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CorrespondenceRequestsControllerTests
    {
        #region Test Context
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
        #endregion

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private Mock<ICorrespondenceRequestsService> correspondenceRequestsServiceMock;

        private string personId;

        private IEnumerable<CorrespondenceRequest> expectedCorrespondenceRequests;
        private List<CorrespondenceRequest> testCorrespondenceRequests;
        private IEnumerable<CorrespondenceRequest> actualCorrespondenceRequests;
        private CorrespondenceAttachmentNotification correspondenceAttachmentNotification;
        private CorrespondenceRequestsController CorrespondenceRequestsController;

        [TestInitialize]
        public async void ControllerTestsInitialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            correspondenceRequestsServiceMock = new Mock<ICorrespondenceRequestsService>();

            personId = "0003914";
            expectedCorrespondenceRequests = new List<CorrespondenceRequest>()
                {
                    new CorrespondenceRequest()
                    {
                        Code = "FA24WES",
                        DueDate = null,
                        Status = CorrespondenceRequestStatus.Received,
                        Instance = "Document 1",
                        PersonId = personId
                    },
                    new CorrespondenceRequest()
                    {
                        Code = "FA83FOO",
                        DueDate = DateTime.Today,
                        Status = CorrespondenceRequestStatus.Incomplete,
                        Instance = "Document 2",
                        PersonId = personId
                    }
                };

            correspondenceAttachmentNotification = new CorrespondenceAttachmentNotification() { PersonId = "x", CommunicationCode = "y", AssignDate = DateTime.Today.AddDays(-2) };
            testCorrespondenceRequests = new List<CorrespondenceRequest>();
            foreach (var document in expectedCorrespondenceRequests)
            {
                var testCorrespondenceRequest = new CorrespondenceRequest();
                foreach (var property in typeof(CorrespondenceRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    property.SetValue(testCorrespondenceRequest, property.GetValue(document, null), null);
                }
                testCorrespondenceRequests.Add(testCorrespondenceRequest);
            }

            correspondenceRequestsServiceMock.Setup<Task<IEnumerable<CorrespondenceRequest>>>(d => d.GetCorrespondenceRequestsAsync(personId)).ReturnsAsync(testCorrespondenceRequests);
            CorrespondenceRequestsController = new CorrespondenceRequestsController(adapterRegistryMock.Object, correspondenceRequestsServiceMock.Object, loggerMock.Object);
            actualCorrespondenceRequests = await CorrespondenceRequestsController.GetCorrespondenceRequestsAsync(personId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            correspondenceRequestsServiceMock = null;
            personId = null;
            expectedCorrespondenceRequests = null;
            testCorrespondenceRequests = null;
            actualCorrespondenceRequests = null;
            CorrespondenceRequestsController = null;
        }

        #region GetCorrespondenceRequests Tests
        [TestClass]
        public class GetCorrespondenceRequestsControllerTests : CorrespondenceRequestsControllerTests
        {

            [TestInitialize]
            public void Initialize()
            {
                base.ControllerTestsInitialize();
            }

            [TestMethod]
            public void CorrespondenceRequestTypeTest()
            {
                Assert.AreEqual(expectedCorrespondenceRequests.GetType(), actualCorrespondenceRequests.GetType());
                foreach (var actualDocument in actualCorrespondenceRequests)
                {
                    Assert.AreEqual(typeof(CorrespondenceRequest), actualDocument.GetType());
                }
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var correspondenceRequestsProperties = typeof(CorrespondenceRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(8, correspondenceRequestsProperties.Length);
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                var correspondenceRequestProperties = typeof(CorrespondenceRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var expectedCorrespondenceRequest in expectedCorrespondenceRequests)
                {
                    var actualCorrespondenceRequest = expectedCorrespondenceRequests.First(d => d.Code == expectedCorrespondenceRequest.Code && d.PersonId == expectedCorrespondenceRequest.PersonId);
                    foreach (var property in correspondenceRequestProperties)
                    {
                        var expectedPropertyValue = property.GetValue(expectedCorrespondenceRequest, null);
                        var actualPropertyValue = property.GetValue(actualCorrespondenceRequest, null);
                        Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonIdRequiredTest()
            {
                await CorrespondenceRequestsController.GetCorrespondenceRequestsAsync(null);
            }

            [TestMethod]
            public async Task PersonIdRequired_BadRequestResponseTest()
            {
                var exceptionCaught = false;
                try
                {
                    await CorrespondenceRequestsController.GetCorrespondenceRequestsAsync(string.Empty);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task CatchPermissionsExceptionAndLogMessageTest()
            {
                correspondenceRequestsServiceMock.Setup(s => s.GetCorrespondenceRequestsAsync(personId)).Throws(new PermissionsException("Permissions Exception"));

                var exceptionCaught = false;
                try
                {
                    await CorrespondenceRequestsController.GetCorrespondenceRequestsAsync(personId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchExceptionAndLogMessageTest()
            {
                correspondenceRequestsServiceMock.Setup(s => s.GetCorrespondenceRequestsAsync(personId)).Throws(new Exception("Generic Exception"));

                var exceptionCaught = false;
                try
                {
                    await CorrespondenceRequestsController.GetCorrespondenceRequestsAsync(personId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }
        #endregion

        [TestClass]
        public class CorrespondenceRequestsController_PutAttachmentNotificationAsyncTests : CorrespondenceRequestsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.ControllerTestsInitialize();
            }

            [TestMethod]
            public async Task CorrespondenceRequestsController_PutAttachmentNotificationAsync_Success()
            {
                correspondenceRequestsServiceMock.Setup(x => x.AttachmentNotificationAsync(It.IsAny<CorrespondenceAttachmentNotification>())).ReturnsAsync(expectedCorrespondenceRequests.First());

                var correspondenceRequest = expectedCorrespondenceRequests.First();
                var actual = await CorrespondenceRequestsController.PutAttachmentNotificationAsync(correspondenceAttachmentNotification);
                Assert.AreEqual(correspondenceRequest, actual);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CorrespondenceRequestsController_PutAttachmentNotificationAsync_PermissionsException()
            {
                try
                {
                    correspondenceRequestsServiceMock.Setup(x => x.AttachmentNotificationAsync(It.IsAny<CorrespondenceAttachmentNotification>())).Throws<PermissionsException>();

                    await CorrespondenceRequestsController.PutAttachmentNotificationAsync(correspondenceAttachmentNotification);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CorrespondenceRequestsController_PutAttachmentNotificationAsync_KeyNotFoundException()
            {
                try
                {
                    correspondenceRequestsServiceMock.Setup(x => x.AttachmentNotificationAsync(It.IsAny<CorrespondenceAttachmentNotification>())).Throws<KeyNotFoundException>();

                    await CorrespondenceRequestsController.PutAttachmentNotificationAsync(correspondenceAttachmentNotification);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CorrespondenceRequestsController_PutAttachmentNotificationAsync_RecordLockException()
            {
                try
                {
                    correspondenceRequestsServiceMock.Setup(x => x.AttachmentNotificationAsync(It.IsAny<CorrespondenceAttachmentNotification>())).Throws<RecordLockException>();

                    await CorrespondenceRequestsController.PutAttachmentNotificationAsync(correspondenceAttachmentNotification);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Conflict, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CorrespondenceRequestsController_PutAttachmentNotificationAsync__Exception()
            {
                try
                {
                    correspondenceRequestsServiceMock.Setup(x => x.AttachmentNotificationAsync(It.IsAny<CorrespondenceAttachmentNotification>())).Throws<Exception>();

                    await CorrespondenceRequestsController.PutAttachmentNotificationAsync(correspondenceAttachmentNotification);
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

