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
using Ellucian.Colleague.Domain.Exceptions;
using System;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonalPronounTypesControllerTests
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
        Mock<IPersonalPronounTypeService> personalPronounTypeServiceMock;
        Mock<ILogger> loggerMock;

        PersonalPronounTypesController personalPronounTypesController;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonalPronounType> personalPronounTypeEntityItems;
        List<Ellucian.Colleague.Dtos.Base.PersonalPronounType> personalPronounTypeDtoItems = new List<Ellucian.Colleague.Dtos.Base.PersonalPronounType>();
        private List<Dtos.PersonalPronouns> personalPronounsCollection;
        private string expectedGuid = "1";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            personalPronounTypeServiceMock = new Mock<IPersonalPronounTypeService>();
            loggerMock = new Mock<ILogger>();

            personalPronounTypeEntityItems = new List<Domain.Base.Entities.PersonalPronounType>
            {
                new Domain.Base.Entities.PersonalPronounType("1", "HE", "He/Him/His" ),
                new Domain.Base.Entities.PersonalPronounType("2", "SHE", "She/Her/Hers" ),
                new Domain.Base.Entities.PersonalPronounType("3", "ZE", "Ze/Zir/Zirs" ),
            };
            foreach (var personalPronounItem in personalPronounTypeEntityItems)
            {
                personalPronounTypeDtoItems.Add(new Dtos.Base.PersonalPronounType() { Code = personalPronounItem.Code, Description = personalPronounItem.Description });
            }

            personalPronounsCollection = new List<Dtos.PersonalPronouns>();
            foreach (var personalPronounItem in personalPronounTypeEntityItems)
            {
                personalPronounsCollection.Add(new Dtos.PersonalPronouns() { Code = personalPronounItem.Code, Description = personalPronounItem.Description, Id = personalPronounItem.Guid });
            }
            personalPronounTypesController = new PersonalPronounTypesController(personalPronounTypeServiceMock.Object, loggerMock.Object);
            personalPronounTypesController.Request = new HttpRequestMessage();
            personalPronounTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personalPronounTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            personalPronounTypeEntityItems = null;
            personalPronounTypeDtoItems = null;
            personalPronounTypesController = null;
            personalPronounsCollection = null;
            loggerMock = null;
        }

        [TestMethod]
        public async Task PersonalPronounTypesController_GetAsync()
        {
            //Arrange
            personalPronounTypeServiceMock.Setup(e => e.GetBasePersonalPronounTypesAsync(It.IsAny<bool>())).ReturnsAsync(personalPronounTypeDtoItems);
            //Act
            var personalPronounTypesResults = (await personalPronounTypesController.GetAsync()).ToList();
            //Assert
            int count = personalPronounTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = personalPronounTypeDtoItems[i];
                var actual = personalPronounTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }
        [TestMethod]
        public async Task PersonalPronounsController_GetPersonalPronouns_ValidateFields_Nocache()
        {
            personalPronounTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(false)).ReturnsAsync(personalPronounsCollection);

            var sourceContexts = (await personalPronounTypesController.GetPersonalPronounsAsync()).ToList();
            Assert.AreEqual(personalPronounsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = personalPronounsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PersonalPronounsController_GetPersonalPronouns_ValidateFields_Cache()
        {
            personalPronounTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(true)).ReturnsAsync(personalPronounsCollection);

            var sourceContexts = (await personalPronounTypesController.GetPersonalPronounsAsync()).ToList();
            Assert.AreEqual(personalPronounsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = personalPronounsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronouns_KeyNotFoundException()
        {
            //
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personalPronounTypesController.GetPersonalPronounsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronouns_PermissionsException()
        {

            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personalPronounTypesController.GetPersonalPronounsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronouns_ArgumentException()
        {

            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personalPronounTypesController.GetPersonalPronounsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronouns_RepositoryException()
        {

            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personalPronounTypesController.GetPersonalPronounsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronouns_IntegrationApiException()
        {

            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personalPronounTypesController.GetPersonalPronounsAsync();
        }

        [TestMethod]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuidAsync_ValidateFields()
        {
            var expected = personalPronounsCollection.FirstOrDefault();
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await personalPronounTypesController.GetPersonalPronounsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronouns_Exception()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsAsync(It.IsAny<bool>())).Throws<Exception>();
            await personalPronounTypesController.GetPersonalPronounsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuidAsync_Exception()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personalPronounTypesController.GetPersonalPronounsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuid_KeyNotFoundException()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personalPronounTypesController.GetPersonalPronounsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuid_PermissionsException()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personalPronounTypesController.GetPersonalPronounsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuid_ArgumentException()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personalPronounTypesController.GetPersonalPronounsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuid_RepositoryException()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personalPronounTypesController.GetPersonalPronounsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuid_IntegrationApiException()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personalPronounTypesController.GetPersonalPronounsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_GetPersonalPronounsByGuid_Exception()
        {
            personalPronounTypeServiceMock.Setup(x => x.GetPersonalPronounsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await personalPronounTypesController.GetPersonalPronounsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_PostPersonalPronounsAsync_Exception()
        {
            await personalPronounTypesController.PostPersonalPronounsAsync(personalPronounsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_PutPersonalPronounsAsync_Exception()
        {
            var sourceContext = personalPronounsCollection.FirstOrDefault();
            await personalPronounTypesController.PutPersonalPronounsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalPronounsController_DeletePersonalPronounsAsync_Exception()
        {
            await personalPronounTypesController.DeletePersonalPronounsAsync(personalPronounsCollection.FirstOrDefault().Id);
        }
    }
}
