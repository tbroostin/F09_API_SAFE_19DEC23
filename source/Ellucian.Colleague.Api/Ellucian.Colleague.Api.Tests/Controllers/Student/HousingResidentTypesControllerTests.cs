//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class HousingResidentTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IHousingResidentTypesService> housingResidentTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private HousingResidentTypesController housingResidentTypesController;      
        private IEnumerable<Domain.Student.Entities.HousingResidentType> allHousingResidentTypes;
        private List<Dtos.HousingResidentTypes> housingResidentTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            housingResidentTypesServiceMock = new Mock<IHousingResidentTypesService>();
            loggerMock = new Mock<ILogger>();
            housingResidentTypesCollection = new List<Dtos.HousingResidentTypes>();

            allHousingResidentTypes  = new List<Domain.Student.Entities.HousingResidentType>()
                {
                    new Domain.Student.Entities.HousingResidentType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.HousingResidentType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.HousingResidentType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allHousingResidentTypes)
            {
                var housingResidentTypes = new Ellucian.Colleague.Dtos.HousingResidentTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                housingResidentTypesCollection.Add(housingResidentTypes);
            }

            housingResidentTypesController = new HousingResidentTypesController(housingResidentTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            housingResidentTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            housingResidentTypesController = null;
            allHousingResidentTypes = null;
            housingResidentTypesCollection = null;
            loggerMock = null;
            housingResidentTypesServiceMock = null;
        }

        [TestMethod]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_ValidateFields_Nocache()
        {
            housingResidentTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(false)).ReturnsAsync(housingResidentTypesCollection);
       
            var sourceContexts = (await housingResidentTypesController.GetHousingResidentTypesAsync()).ToList();
            Assert.AreEqual(housingResidentTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = housingResidentTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_ValidateFields_Cache()
        {
            housingResidentTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(true)).ReturnsAsync(housingResidentTypesCollection);

            var sourceContexts = (await housingResidentTypesController.GetHousingResidentTypesAsync()).ToList();
            Assert.AreEqual(housingResidentTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = housingResidentTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_KeyNotFoundException()
        {
            //
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await housingResidentTypesController.GetHousingResidentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_PermissionsException()
        {
            
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(false))
                .Throws<PermissionsException>();
            await housingResidentTypesController.GetHousingResidentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_ArgumentException()
        {
            
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(false))
                .Throws<ArgumentException>();
            await housingResidentTypesController.GetHousingResidentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_RepositoryException()
        {
            
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(false))
                .Throws<RepositoryException>();
            await housingResidentTypesController.GetHousingResidentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_IntegrationApiException()
        {
            
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(false))
                .Throws<IntegrationApiException>();
            await housingResidentTypesController.GetHousingResidentTypesAsync();
        }

        [TestMethod]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuidAsync_ValidateFields()
        {
            var expected = housingResidentTypesCollection.FirstOrDefault();
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypes_Exception()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesAsync(false)).Throws<Exception>();
            await housingResidentTypesController.GetHousingResidentTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuidAsync_Exception()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuid_KeyNotFoundException()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuid_PermissionsException()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuid_ArgumentException()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuid_RepositoryException()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuid_IntegrationApiException()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_GetHousingResidentTypesByGuid_Exception()
        {
            housingResidentTypesServiceMock.Setup(x => x.GetHousingResidentTypesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await housingResidentTypesController.GetHousingResidentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_PostHousingResidentTypesAsync_Exception()
        {
            await housingResidentTypesController.PostHousingResidentTypesAsync(housingResidentTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_PutHousingResidentTypesAsync_Exception()
        {
            var sourceContext = housingResidentTypesCollection.FirstOrDefault();
            await housingResidentTypesController.PutHousingResidentTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HousingResidentTypesController_DeleteHousingResidentTypesAsync_Exception()
        {
            await housingResidentTypesController.DeleteHousingResidentTypesAsync(housingResidentTypesCollection.FirstOrDefault().Id);
        }
    }
}