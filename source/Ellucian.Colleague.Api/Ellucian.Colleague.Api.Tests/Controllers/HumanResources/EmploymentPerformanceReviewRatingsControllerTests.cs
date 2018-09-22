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
    public class EmploymentPerformanceReviewRatingsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmploymentPerformanceReviewRatingsService> employmentPerformanceReviewRatingsServiceMock;
        private Mock<ILogger> loggerMock;
        private EmploymentPerformanceReviewRatingsController employmentPerformanceReviewRatingsController;      
        private IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewRating> allEmploymentPerformanceReviewRatings;
        private List<Dtos.EmploymentPerformanceReviewRatings> employmentPerformanceReviewRatingsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            employmentPerformanceReviewRatingsServiceMock = new Mock<IEmploymentPerformanceReviewRatingsService>();
            loggerMock = new Mock<ILogger>();
            employmentPerformanceReviewRatingsCollection = new List<Dtos.EmploymentPerformanceReviewRatings>();

            allEmploymentPerformanceReviewRatings  = new List<Domain.HumanResources.Entities.EmploymentPerformanceReviewRating>()
                {
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewRating("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewRating("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewRating("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allEmploymentPerformanceReviewRatings)
            {
                var employmentPerformanceReviewRatings = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                employmentPerformanceReviewRatingsCollection.Add(employmentPerformanceReviewRatings);
            }

            employmentPerformanceReviewRatingsController = new EmploymentPerformanceReviewRatingsController(employmentPerformanceReviewRatingsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentPerformanceReviewRatingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentPerformanceReviewRatingsController = null;
            allEmploymentPerformanceReviewRatings = null;
            employmentPerformanceReviewRatingsCollection = null;
            loggerMock = null;
            employmentPerformanceReviewRatingsServiceMock = null;
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_ValidateFields_Nocache()
        {
            employmentPerformanceReviewRatingsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(false)).ReturnsAsync(employmentPerformanceReviewRatingsCollection);
       
            var sourceContexts = (await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync()).ToList();
            Assert.AreEqual(employmentPerformanceReviewRatingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentPerformanceReviewRatingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_ValidateFields_Cache()
        {
            employmentPerformanceReviewRatingsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(true)).ReturnsAsync(employmentPerformanceReviewRatingsCollection);

            var sourceContexts = (await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync()).ToList();
            Assert.AreEqual(employmentPerformanceReviewRatingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentPerformanceReviewRatingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_KeyNotFoundException()
        {
            //
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(false))
                .Throws<KeyNotFoundException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_PermissionsException()
        {
            
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(false))
                .Throws<PermissionsException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_ArgumentException()
        {
            
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(false))
                .Throws<ArgumentException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_RepositoryException()
        {
            
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(false))
                .Throws<RepositoryException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_IntegrationApiException()
        {
            
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(false))
                .Throws<IntegrationApiException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync();
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuidAsync_ValidateFields()
        {
            var expected = employmentPerformanceReviewRatingsCollection.FirstOrDefault();
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatings_Exception()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsAsync(false)).Throws<Exception>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuidAsync_Exception()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuid_KeyNotFoundException()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuid_PermissionsException()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuid_ArgumentException()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuid_RepositoryException()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuid_IntegrationApiException()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_GetEmploymentPerformanceReviewRatingsByGuid_Exception()
        {
            employmentPerformanceReviewRatingsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewRatingsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await employmentPerformanceReviewRatingsController.GetEmploymentPerformanceReviewRatingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_PostEmploymentPerformanceReviewRatingsAsync_Exception()
        {
            await employmentPerformanceReviewRatingsController.PostEmploymentPerformanceReviewRatingsAsync(employmentPerformanceReviewRatingsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_PutEmploymentPerformanceReviewRatingsAsync_Exception()
        {
            var sourceContext = employmentPerformanceReviewRatingsCollection.FirstOrDefault();
            await employmentPerformanceReviewRatingsController.PutEmploymentPerformanceReviewRatingsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewRatingsController_DeleteEmploymentPerformanceReviewRatingsAsync_Exception()
        {
            await employmentPerformanceReviewRatingsController.DeleteEmploymentPerformanceReviewRatingsAsync(employmentPerformanceReviewRatingsCollection.FirstOrDefault().Id);
        }
    }
}