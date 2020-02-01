// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class AttachmentCollectionsControllerTests
    {
        private Mock<IAdapterRegistry> adapterRegistry;
        private Mock<ILogger> loggerMock;
        private Mock<IAttachmentCollectionService> collectionService;

        private AttachmentCollectionsController collectionsController;
        private List<AttachmentCollection> collections;

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            adapterRegistry = new Mock<IAdapterRegistry>();
            collectionService = new Mock<IAttachmentCollectionService>();
            collections = new List<AttachmentCollection>();
            collections.Add(new AttachmentCollection()
            {
                Id = "32c2d2ec-d91d-4c86-be0d-6e38b5b0f1b7",
                Owner = "0003896"
            });
            collections.Add(new AttachmentCollection()
            {
                Id = "16a9e995-cfb9-444f-b34a-f4b25b98a188",
                Owner = "0009999"
            });
            loggerMock = new Mock<ILogger>();
            collectionsController = new AttachmentCollectionsController(adapterRegistry.Object, collectionService.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            collectionService = null;
            collectionsController = null;
            collections = null;
            adapterRegistry = null;
        }

        #endregion

        #region GetAttachmentCollectionByIdAsync

        // GetAttachmentCollectionByIdAsync success
        [TestMethod]
        public async Task AttachmentCollectionsController_GetAttachmentCollectionByIdAsync()
        {
            collectionService.Setup(x => x.GetAttachmentCollectionByIdAsync(collections.First().Id)).ReturnsAsync(collections.First());
            var actual = await collectionsController.GetAttachmentCollectionByIdAsync(collections.First().Id);
            Assert.AreEqual(collections.First().Id, actual.Id);
        }

        // GetAttachmentCollectionByIdAsync permissions exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_GetAttachmentCollectionByIdAsync_PermissionsException()
        {
            try
            {
                collectionService.Setup(x => x.GetAttachmentCollectionByIdAsync(collections.First().Id)).Throws<PermissionsException>();
                await collectionsController.GetAttachmentCollectionByIdAsync(collections.First().Id);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                throw;
            }
        }

        // GetAttachmentCollectionByIdAsync general exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_GetAttachmentCollectionByIdAsync_Exception()
        {
            try
            {
                collectionService.Setup(x => x.GetAttachmentCollectionByIdAsync(collections.First().Id)).Throws<Exception>();
                await collectionsController.GetAttachmentCollectionByIdAsync(collections.First().Id);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw;
            }
        }

        // GetAttachmentCollectionByIdAsync KeyNotFound exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_GetAttachmentCollectionByIdAsync_KeyNotFoundException()
        {
            try
            {
                collectionService.Setup(x => x.GetAttachmentCollectionByIdAsync(collections.First().Id)).Throws<KeyNotFoundException>();
                await collectionsController.GetAttachmentCollectionByIdAsync(collections.First().Id);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                throw;
            }
        }

        #endregion

        #region GetAttachmentCollectionsByUserAsync

        // GetAttachmentCollectionsByUserAsync success
        [TestMethod]
        public async Task AttachmentCollectionsController_GetAttachmentCollectionsByUserAsync()
        {
            collectionService.Setup(x => x.GetAttachmentCollectionsByUserAsync()).ReturnsAsync(collections);
            var actual = await collectionsController.GetAttachmentCollectionsByUserAsync();
            Assert.AreEqual(collections.Count(), actual.Count());
        }

        // GetAttachmentCollectionsByUserAsync permissions exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_GetAttachmentCollectionsByUserAsync_PermissionsException()
        {
            try
            {
                collectionService.Setup(x => x.GetAttachmentCollectionsByUserAsync()).Throws<PermissionsException>();
                await collectionsController.GetAttachmentCollectionsByUserAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                throw;
            }
        }

        // GetAttachmentCollectionsByUserAsync general exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_GetAttachmentCollectionsByUserAsync_Exception()
        {
            try
            {
                collectionService.Setup(x => x.GetAttachmentCollectionsByUserAsync()).Throws<Exception>();
                await collectionsController.GetAttachmentCollectionsByUserAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw;
            }
        }

        #endregion

        #region PostAsync

        // PostAsync success
        [TestMethod]
        public async Task AttachmentCollectionsController_PostAsync()
        {
            collectionService.Setup(x => x.PostAttachmentCollectionAsync(collections.First())).ReturnsAsync(collections.First());
            var actual = await collectionsController.PostAsync(collections.First());
            Assert.AreEqual(collections.First().Id, actual.Id);
        }

        // PostAsync permissions exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_PostAsync_PermissionsException()
        {
            try
            {
                collectionService.Setup(x => x.PostAttachmentCollectionAsync(collections.First())).Throws<PermissionsException>();
                await collectionsController.PostAsync(collections.First());
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                throw;
            }
        }

        // PostAsync general exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_PostAsync_Exception()
        {
            try
            {
                collectionService.Setup(x => x.PostAttachmentCollectionAsync(collections.First())).Throws<Exception>();
                await collectionsController.PostAsync(collections.First());
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw;
            }
        }

        #endregion

        #region PutAsync

        // PutAsync success
        [TestMethod]
        public async Task AttachmentCollectionsController_PutAsync()
        {
            var attachmentCollection = collections.First();
            collectionService.Setup(x => x.PutAttachmentCollectionAsync(It.IsAny<string>(), It.IsAny<AttachmentCollection>())).ReturnsAsync(collections.First());
            var actual = await collectionsController.PutAsync(attachmentCollection.Id, attachmentCollection);
            Assert.AreEqual(collections.First().Id, actual.Id);
        }

        // PutAsync permissions exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_PutAsync_PermissionsException()
        {
            try
            {
                var attachmentCollection = collections.First();
                collectionService.Setup(x => x.PutAttachmentCollectionAsync(It.IsAny<string>(), It.IsAny<AttachmentCollection>())).Throws<PermissionsException>();
                await collectionsController.PutAsync(attachmentCollection.Id, attachmentCollection);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                throw;
            }
        }

        // PutAsync general exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_PutAsync_Exception()
        {
            try
            {
                var attachmentCollection = collections.First();
                collectionService.Setup(x => x.PutAttachmentCollectionAsync(It.IsAny<string>(), It.IsAny<AttachmentCollection>())).Throws<Exception>();
                await collectionsController.PutAsync(attachmentCollection.Id, attachmentCollection);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw;
            }
        }

        #endregion

        #region GetEffectivePermissionsAsync

        // GetEffectivePermissionsAsync success
        [TestMethod]
        public async Task AttachmentCollectionsController_GetEffectivePermissionsAsync()
        {
            var effectivePermissions = new AttachmentCollectionEffectivePermissions();
            collectionService.Setup(x => x.GetEffectivePermissionsAsync(It.IsAny<string>())).ReturnsAsync(effectivePermissions);
            var actual = await collectionsController.GetEffectivePermissionsAsync("TEST");
            Assert.IsNotNull(actual);
        }

        // GetEffectivePermissionsAsync permissions exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_GetEffectivePermissionsAsync_KeyNotFoundException()
        {
            try
            {
                collectionService.Setup(x => x.GetEffectivePermissionsAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await collectionsController.GetEffectivePermissionsAsync("TEST");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                throw;
            }
        }

        // GetEffectivePermissionsAsync general exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttachmentCollectionsController_GetEffectivePermissionsAsync_Exception()
        {
            try
            {
                collectionService.Setup(x => x.GetEffectivePermissionsAsync(It.IsAny<string>())).Throws<Exception>();
                await collectionsController.GetEffectivePermissionsAsync("TEST");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw;
            }
        }

        #endregion
    }
}