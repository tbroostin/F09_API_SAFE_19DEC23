//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class DeductionCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IDeductionCategoriesService> deductionCategoriesServiceMock;
        private Mock<ILogger> loggerMock;
        private DeductionCategoriesController deductionCategoriesController;
        private IEnumerable<Domain.HumanResources.Entities.DeductionCategory> allBendedTypes;
        private List<Dtos.DeductionCategories> deductionCategoriesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            deductionCategoriesServiceMock = new Mock<IDeductionCategoriesService>();
            loggerMock = new Mock<ILogger>();
            deductionCategoriesCollection = new List<Dtos.DeductionCategories>();

            allBendedTypes = new List<Domain.HumanResources.Entities.DeductionCategory>()
                {
                    new Domain.HumanResources.Entities.DeductionCategory("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.DeductionCategory("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.DeductionCategory("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allBendedTypes)
            {
                var deductionCategories = new Ellucian.Colleague.Dtos.DeductionCategories
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                deductionCategoriesCollection.Add(deductionCategories);
            }

            deductionCategoriesController = new DeductionCategoriesController(deductionCategoriesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            deductionCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            deductionCategoriesController = null;
            allBendedTypes = null;
            deductionCategoriesCollection = null;
            loggerMock = null;
            deductionCategoriesServiceMock = null;
        }

        [TestMethod]
        public async Task DeductionCategoriesController_GetDeductionCategories_ValidateFields_Nocache()
        {
            deductionCategoriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(false)).ReturnsAsync(deductionCategoriesCollection);
       
            var sourceContexts = (await deductionCategoriesController.GetDeductionCategoriesAsync()).ToList();
            Assert.AreEqual(deductionCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = deductionCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task DeductionCategoriesController_GetDeductionCategories_ValidateFields_Cache()
        {
            deductionCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(true)).ReturnsAsync(deductionCategoriesCollection);

            var sourceContexts = (await deductionCategoriesController.GetDeductionCategoriesAsync()).ToList();
            Assert.AreEqual(deductionCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = deductionCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategories_KeyNotFoundException()
        {
            //
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(false))
                .Throws<KeyNotFoundException>();
            await deductionCategoriesController.GetDeductionCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategories_PermissionsException()
        {
            
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(false))
                .Throws<PermissionsException>();
            await deductionCategoriesController.GetDeductionCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategories_ArgumentException()
        {
            
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(false))
                .Throws<ArgumentException>();
            await deductionCategoriesController.GetDeductionCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategories_RepositoryException()
        {
            
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(false))
                .Throws<RepositoryException>();
            await deductionCategoriesController.GetDeductionCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategories_IntegrationApiException()
        {
            
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(false))
                .Throws<IntegrationApiException>();
            await deductionCategoriesController.GetDeductionCategoriesAsync();
        }

        [TestMethod]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuidAsync_ValidateFields()
        {
            var expected = deductionCategoriesCollection.FirstOrDefault();
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategories_Exception()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesAsync(false)).Throws<Exception>();
            await deductionCategoriesController.GetDeductionCategoriesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuidAsync_Exception()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuid_KeyNotFoundException()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuid_PermissionsException()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuid_ArgumentException()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuid_RepositoryException()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuid_IntegrationApiException()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_GetDeductionCategoriesByGuid_Exception()
        {
            deductionCategoriesServiceMock.Setup(x => x.GetDeductionCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await deductionCategoriesController.GetDeductionCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_PostDeductionCategoriesAsync_Exception()
        {
            await deductionCategoriesController.PostDeductionCategoriesAsync(deductionCategoriesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_PutDeductionCategoriesAsync_Exception()
        {
            var sourceContext = deductionCategoriesCollection.FirstOrDefault();
            await deductionCategoriesController.PutDeductionCategoriesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeductionCategoriesController_DeleteDeductionCategoriesAsync_Exception()
        {
            await deductionCategoriesController.DeleteDeductionCategoriesAsync(deductionCategoriesCollection.FirstOrDefault().Id);
        }
    }
}