// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Security;
using System;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class GenderIdentityTypesControllerTests
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
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IGenderIdentityTypeService> genderIdentityTypeServiceMock;
        Mock<ILogger> loggerMock;

        GenderIdentityTypesController genderIdentityTypesController;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.GenderIdentityType> genderIdentityTypeEntityItems;
        List<Ellucian.Colleague.Dtos.Base.GenderIdentityType> genderIdentityTypeDtoItems = new List<Ellucian.Colleague.Dtos.Base.GenderIdentityType>();

        private List<Dtos.GenderIdentities> genderIdentitiesCollection;
        private string expectedGuid = "1";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            genderIdentityTypeServiceMock = new Mock<IGenderIdentityTypeService>();
            loggerMock = new Mock<ILogger>();

            genderIdentityTypeEntityItems = new List<Domain.Base.Entities.GenderIdentityType>
            {
                new Domain.Base.Entities.GenderIdentityType("1", "FEM", "Female" ),
                new Domain.Base.Entities.GenderIdentityType("2", "MAL", "Male" ),
                new Domain.Base.Entities.GenderIdentityType("3", "TFM", "Transexual (F/M)" ),
            };
            foreach (var genderIdentityItem in genderIdentityTypeEntityItems)
            {
                genderIdentityTypeDtoItems.Add(new Dtos.Base.GenderIdentityType() { Code = genderIdentityItem.Code, Description = genderIdentityItem.Description });
            }
            genderIdentitiesCollection = new List<Dtos.GenderIdentities>();
            foreach (var genderIdentityItem in genderIdentityTypeEntityItems)
            {
                genderIdentitiesCollection.Add(new Dtos.GenderIdentities() { Code = genderIdentityItem.Code, Description = genderIdentityItem.Description, Id = genderIdentityItem.Guid });
            }

            genderIdentityTypesController = new GenderIdentityTypesController(genderIdentityTypeServiceMock.Object, loggerMock.Object);
            genderIdentityTypesController.Request = new HttpRequestMessage();
            genderIdentityTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            genderIdentityTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            genderIdentityTypeEntityItems = null;
            genderIdentityTypeDtoItems = null;
            genderIdentityTypesController = null;
        }

        [TestMethod]
        public async Task GenderIdentityTypesController_GetAsync()
        {
            //Arrange
            genderIdentityTypeServiceMock.Setup(e => e.GetBaseGenderIdentityTypesAsync(It.IsAny<bool>())).ReturnsAsync(genderIdentityTypeDtoItems);
            //Act
            var genderIdentityTypesResults = (await genderIdentityTypesController.GetAsync()).ToList();
            //Assert
            int count = genderIdentityTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = genderIdentityTypeDtoItems[i];
                var actual = genderIdentityTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }
        [TestMethod]
        public async Task genderIdentityTypesController_GetGenderIdentities_ValidateFields_Nocache()
        {
            genderIdentityTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(false)).ReturnsAsync(genderIdentitiesCollection);

            var sourceContexts = (await genderIdentityTypesController.GetGenderIdentitiesAsync()).ToList();
            Assert.AreEqual(genderIdentitiesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = genderIdentitiesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task genderIdentityTypesController_GetGenderIdentities_ValidateFields_Cache()
        {
            genderIdentityTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(true)).ReturnsAsync(genderIdentitiesCollection);

            var sourceContexts = (await genderIdentityTypesController.GetGenderIdentitiesAsync()).ToList();
            Assert.AreEqual(genderIdentitiesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = genderIdentitiesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentities_KeyNotFoundException()
        {
            //
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await genderIdentityTypesController.GetGenderIdentitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentities_PermissionsException()
        {

            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await genderIdentityTypesController.GetGenderIdentitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentities_ArgumentException()
        {

            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await genderIdentityTypesController.GetGenderIdentitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentities_RepositoryException()
        {

            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await genderIdentityTypesController.GetGenderIdentitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentities_IntegrationApiException()
        {

            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await genderIdentityTypesController.GetGenderIdentitiesAsync();
        }

        [TestMethod]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuidAsync_ValidateFields()
        {
            var expected = genderIdentitiesCollection.FirstOrDefault();
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentities_Exception()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesAsync(It.IsAny<bool>())).Throws<Exception>();
            await genderIdentityTypesController.GetGenderIdentitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuidAsync_Exception()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuid_KeyNotFoundException()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuid_PermissionsException()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuid_ArgumentException()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuid_RepositoryException()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuid_IntegrationApiException()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_GetGenderIdentitiesByGuid_Exception()
        {
            genderIdentityTypeServiceMock.Setup(x => x.GetGenderIdentitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await genderIdentityTypesController.GetGenderIdentitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_PostGenderIdentitiesAsync_Exception()
        {
            await genderIdentityTypesController.PostGenderIdentitiesAsync(genderIdentitiesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_PutGenderIdentitiesAsync_Exception()
        {
            var sourceContext = genderIdentitiesCollection.FirstOrDefault();
            await genderIdentityTypesController.PutGenderIdentitiesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task genderIdentityTypesController_DeleteGenderIdentitiesAsync_Exception()
        {
            await genderIdentityTypesController.DeleteGenderIdentitiesAsync(genderIdentitiesCollection.FirstOrDefault().Id);
        }
    }
}
