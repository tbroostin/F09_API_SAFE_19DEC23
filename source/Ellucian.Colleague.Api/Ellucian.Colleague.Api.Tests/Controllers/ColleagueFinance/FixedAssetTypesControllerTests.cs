//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class FixedAssetTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFixedAssetTypesService> fixedAssetTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private FixedAssetTypesController fixedAssetTypesController;      
        private IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes> allAssetTypes;
        private List<Dtos.FixedAssetType> fixedAssetTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            fixedAssetTypesServiceMock = new Mock<IFixedAssetTypesService>();
            loggerMock = new Mock<ILogger>();
            fixedAssetTypesCollection = new List<Dtos.FixedAssetType>();

            allAssetTypes  = new List<Domain.ColleagueFinance.Entities.AssetTypes>()
                {
                    new Domain.ColleagueFinance.Entities.AssetTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.AssetTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.AssetTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allAssetTypes)
            {
                var fixedAssetTypes = new Ellucian.Colleague.Dtos.FixedAssetType
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                fixedAssetTypesCollection.Add(fixedAssetTypes);
            }

            fixedAssetTypesController = new FixedAssetTypesController(fixedAssetTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            fixedAssetTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            fixedAssetTypesController = null;
            allAssetTypes = null;
            fixedAssetTypesCollection = null;
            loggerMock = null;
            fixedAssetTypesServiceMock = null;
        }

        [TestMethod]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_ValidateFields_Nocache()
        {
            fixedAssetTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(false)).ReturnsAsync(fixedAssetTypesCollection);
       
            var sourceContexts = (await fixedAssetTypesController.GetFixedAssetTypesAsync()).ToList();
            Assert.AreEqual(fixedAssetTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fixedAssetTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_ValidateFields_Cache()
        {
            fixedAssetTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(true)).ReturnsAsync(fixedAssetTypesCollection);

            var sourceContexts = (await fixedAssetTypesController.GetFixedAssetTypesAsync()).ToList();
            Assert.AreEqual(fixedAssetTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fixedAssetTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_KeyNotFoundException()
        {
            //
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await fixedAssetTypesController.GetFixedAssetTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_PermissionsException()
        {
            
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(false))
                .Throws<PermissionsException>();
            await fixedAssetTypesController.GetFixedAssetTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_ArgumentException()
        {
            
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(false))
                .Throws<ArgumentException>();
            await fixedAssetTypesController.GetFixedAssetTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_RepositoryException()
        {
            
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(false))
                .Throws<RepositoryException>();
            await fixedAssetTypesController.GetFixedAssetTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_IntegrationApiException()
        {
            
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(false))
                .Throws<IntegrationApiException>();
            await fixedAssetTypesController.GetFixedAssetTypesAsync();
        }

        [TestMethod]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuidAsync_ValidateFields()
        {
            var expected = fixedAssetTypesCollection.FirstOrDefault();
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypes_Exception()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesAsync(false)).Throws<Exception>();
            await fixedAssetTypesController.GetFixedAssetTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuidAsync_Exception()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuid_KeyNotFoundException()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuid_PermissionsException()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuid_ArgumentException()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuid_RepositoryException()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuid_IntegrationApiException()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_GetFixedAssetTypesByGuid_Exception()
        {
            fixedAssetTypesServiceMock.Setup(x => x.GetFixedAssetTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await fixedAssetTypesController.GetFixedAssetTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_PostFixedAssetTypesAsync_Exception()
        {
            await fixedAssetTypesController.PostFixedAssetTypesAsync(fixedAssetTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_PutFixedAssetTypesAsync_Exception()
        {
            var sourceContext = fixedAssetTypesCollection.FirstOrDefault();
            await fixedAssetTypesController.PutFixedAssetTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetTypesController_DeleteFixedAssetTypesAsync_Exception()
        {
            await fixedAssetTypesController.DeleteFixedAssetTypesAsync(fixedAssetTypesCollection.FirstOrDefault().Id);
        }
    }
}