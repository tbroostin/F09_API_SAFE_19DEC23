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
    public class FixedAssetCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFixedAssetCategoriesService> fixedAssetCategoriesServiceMock;
        private Mock<ILogger> loggerMock;
        private FixedAssetCategoriesController fixedAssetCategoriesController;      
        private IEnumerable<Domain.ColleagueFinance.Entities.AssetCategories> allAssetCategories;
        private List<Dtos.FixedAssetCategory> fixedAssetCategoriesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            fixedAssetCategoriesServiceMock = new Mock<IFixedAssetCategoriesService>();
            loggerMock = new Mock<ILogger>();
            fixedAssetCategoriesCollection = new List<Dtos.FixedAssetCategory>();

            allAssetCategories  = new List<Domain.ColleagueFinance.Entities.AssetCategories>()
                {
                    new Domain.ColleagueFinance.Entities.AssetCategories("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.AssetCategories("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.AssetCategories("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allAssetCategories)
            {
                var fixedAssetCategories = new Ellucian.Colleague.Dtos.FixedAssetCategory
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                fixedAssetCategoriesCollection.Add(fixedAssetCategories);
            }

            fixedAssetCategoriesController = new FixedAssetCategoriesController(fixedAssetCategoriesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            fixedAssetCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            fixedAssetCategoriesController = null;
            allAssetCategories = null;
            fixedAssetCategoriesCollection = null;
            loggerMock = null;
            fixedAssetCategoriesServiceMock = null;
        }

        [TestMethod]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_ValidateFields_Nocache()
        {
            fixedAssetCategoriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(false)).ReturnsAsync(fixedAssetCategoriesCollection);
       
            var sourceContexts = (await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync()).ToList();
            Assert.AreEqual(fixedAssetCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fixedAssetCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_ValidateFields_Cache()
        {
            fixedAssetCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(true)).ReturnsAsync(fixedAssetCategoriesCollection);

            var sourceContexts = (await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync()).ToList();
            Assert.AreEqual(fixedAssetCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fixedAssetCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_KeyNotFoundException()
        {
            //
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(false))
                .Throws<KeyNotFoundException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_PermissionsException()
        {
            
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(false))
                .Throws<PermissionsException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_ArgumentException()
        {
            
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(false))
                .Throws<ArgumentException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_RepositoryException()
        {
            
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(false))
                .Throws<RepositoryException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_IntegrationApiException()
        {
            
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(false))
                .Throws<IntegrationApiException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync();
        }

        [TestMethod]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuidAsync_ValidateFields()
        {
            fixedAssetCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = fixedAssetCategoriesCollection.FirstOrDefault();
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategories_Exception()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesAsync(false)).Throws<Exception>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuidAsync_Exception()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuid_KeyNotFoundException()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuid_PermissionsException()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuid_ArgumentException()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuid_RepositoryException()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuid_IntegrationApiException()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_GetFixedAssetCategoriesByGuid_Exception()
        {
            fixedAssetCategoriesServiceMock.Setup(x => x.GetFixedAssetCategoriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await fixedAssetCategoriesController.GetFixedAssetCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_PostFixedAssetCategoriesAsync_Exception()
        {
            await fixedAssetCategoriesController.PostFixedAssetCategoriesAsync(fixedAssetCategoriesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_PutFixedAssetCategoriesAsync_Exception()
        {
            var sourceContext = fixedAssetCategoriesCollection.FirstOrDefault();
            await fixedAssetCategoriesController.PutFixedAssetCategoriesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetCategoriesController_DeleteFixedAssetCategoriesAsync_Exception()
        {
            await fixedAssetCategoriesController.DeleteFixedAssetCategoriesAsync(fixedAssetCategoriesCollection.FirstOrDefault().Id);
        }
    }
}