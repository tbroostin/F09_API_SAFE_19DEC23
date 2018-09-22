//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmploymentPerformanceReviewsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmploymentPerformanceReviewsService> employmentPerformanceReviewsServiceMock;
        private Mock<ILogger> loggerMock;
        private EmploymentPerformanceReviewsController employmentPerformanceReviewsController;      
        private IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReview> allEmploymentPerformanceReviews;
        private List<Dtos.EmploymentPerformanceReviews> employmentPerformanceReviewsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            employmentPerformanceReviewsServiceMock = new Mock<IEmploymentPerformanceReviewsService>();
            loggerMock = new Mock<ILogger>();
            employmentPerformanceReviewsCollection = new List<Dtos.EmploymentPerformanceReviews>();

            allEmploymentPerformanceReviews  = new List<Domain.HumanResources.Entities.EmploymentPerformanceReview>()
                {
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", new DateTime(18080), "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", new DateTime(18080), "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", new DateTime(18080), "d2253ac7-9931-4560-b42f-1fccd43c952e", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                };
            
            foreach (var source in allEmploymentPerformanceReviews)
            {
                var employmentPerformanceReviews = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviews
                {
                    Id = source.Guid,
                    Person = new GuidObject2(source.PersonId),
                    Job = new GuidObject2(source.PerposId),
                    CompletedOn = (DateTime) source.CompletedDate,
                    Type = new GuidObject2(source.RatingCycleCode),
                    Rating = new EmploymentPerformanceReviewsRatingDtoProperty() { Detail = new GuidObject2(source.RatingCycleCode) }
                };
                employmentPerformanceReviewsCollection.Add(employmentPerformanceReviews);
            }

            employmentPerformanceReviewsController = new EmploymentPerformanceReviewsController(employmentPerformanceReviewsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentPerformanceReviewsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            employmentPerformanceReviewsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentPerformanceReviewsController = null;
            allEmploymentPerformanceReviews = null;
            employmentPerformanceReviewsCollection = null;
            loggerMock = null;
            employmentPerformanceReviewsServiceMock = null;
        }

        #region GET
        [TestMethod]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_ValidateFields_Nocache()
        {
            employmentPerformanceReviewsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var employmentPerformanceReviewsTuple
                    = new Tuple<IEnumerable<EmploymentPerformanceReviews>, int>(employmentPerformanceReviewsCollection, employmentPerformanceReviewsCollection.Count);

            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(employmentPerformanceReviewsTuple);

            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            var employmentPerformanceReviews = (await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await employmentPerformanceReviews.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.EmploymentPerformanceReviews>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.EmploymentPerformanceReviews>;

            Assert.IsNotNull(results);
            Assert.AreEqual(employmentPerformanceReviewsCollection.Count, results.Count());
            foreach (var actual in results)
            {
                var expected = employmentPerformanceReviewsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Job.Id, actual.Job.Id);
                Assert.AreEqual(expected.CompletedOn, actual.CompletedOn);
                Assert.AreEqual(expected.Type.Id, actual.Type.Id);
                Assert.AreEqual(expected.Rating.Detail.Id, actual.Rating.Detail.Id);
                if (expected.Comment != null)
                    Assert.AreEqual(expected.Comment, actual.Comment);
                if (expected.ReviewedBy != null)
                    Assert.AreEqual(expected.ReviewedBy.Id, actual.ReviewedBy.Id);
            }
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_ValidateFields_Cache()
        {
            employmentPerformanceReviewsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            var employmentPerformanceReviewsTuple
                    = new Tuple<IEnumerable<EmploymentPerformanceReviews>, int>(employmentPerformanceReviewsCollection, employmentPerformanceReviewsCollection.Count);

            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(employmentPerformanceReviewsTuple);

            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            var employmentPerformanceReviews = (await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await employmentPerformanceReviews.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.EmploymentPerformanceReviews>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.EmploymentPerformanceReviews>;

            Assert.IsNotNull(results);
            Assert.AreEqual(employmentPerformanceReviewsCollection.Count, results.Count());
            foreach (var actual in results)
            {
                var expected = employmentPerformanceReviewsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Job.Id, actual.Job.Id);
                Assert.AreEqual(expected.CompletedOn, actual.CompletedOn);
                Assert.AreEqual(expected.Type.Id, actual.Type.Id);
                Assert.AreEqual(expected.Rating.Detail.Id, actual.Rating.Detail.Id);
                if (expected.Comment != null)
                    Assert.AreEqual(expected.Comment, actual.Comment);
                if (expected.ReviewedBy != null)
                    Assert.AreEqual(expected.ReviewedBy.Id, actual.ReviewedBy.Id);
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_KeyNotFoundException()
        {
            //
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_PermissionsException()
        {

            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<PermissionsException>();
            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_ArgumentException()
        {

            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<ArgumentException>();
            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_RepositoryException()
        {

            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<RepositoryException>();
            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_IntegrationApiException()
        {

            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging);
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuidAsync_ValidateFields()
        {
            var expected = employmentPerformanceReviewsCollection.FirstOrDefault();
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(expected.Id);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Job.Id, actual.Job.Id);
            Assert.AreEqual(expected.CompletedOn, actual.CompletedOn);
            Assert.AreEqual(expected.Type.Id, actual.Type.Id);
            Assert.AreEqual(expected.Rating.Detail.Id, actual.Rating.Detail.Id);
            if (expected.Comment != null)
                Assert.AreEqual(expected.Comment, actual.Comment);
            if (expected.ReviewedBy != null)
                Assert.AreEqual(expected.ReviewedBy.Id, actual.ReviewedBy.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviews_Exception()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();
            Paging paging = new Paging(employmentPerformanceReviewsCollection.Count(), 0);
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsAsync(paging);       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuidAsync_Exception()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuid_KeyNotFoundException()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuid_PermissionsException()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuid_ArgumentException()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuid_RepositoryException()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuid_IntegrationApiException()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_GetEmploymentPerformanceReviewsByGuid_Exception()
        {
            employmentPerformanceReviewsServiceMock.Setup(x => x.GetEmploymentPerformanceReviewsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await employmentPerformanceReviewsController.GetEmploymentPerformanceReviewsByGuidAsync(expectedGuid);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task EmploymentPerformanceReviewsController_PutEmploymentPerformanceReviewAsync()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PutEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault().Id, employmentPerformanceReviewsCollection.FirstOrDefault())).ReturnsAsync(employmentPerformanceReviewsCollection.FirstOrDefault());
            var result = await employmentPerformanceReviewsController.PutEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault().Id, employmentPerformanceReviewsCollection.FirstOrDefault());
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Id, result.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Person.Id, result.Person.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Job.Id, result.Job.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Type.Id, result.Type.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Rating.Detail.Id, result.Rating.Detail.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().CompletedOn, result.CompletedOn);
            if (result.Comment != null)
                Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Comment, result.Comment);
            if (result.ReviewedBy != null)
                Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().ReviewedBy.Id, result.ReviewedBy);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PutEmploymentPerformanceReviewAsync_ArgumentNullException()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PutEmploymentPerformanceReviewsAsync(It.IsAny<string>(), It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new ArgumentNullException());
            await employmentPerformanceReviewsController.PutEmploymentPerformanceReviewsAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PutEmploymentPerformanceReviewAsync_IdNull_ArgumentNullException()
        {
            employmentPerformanceReviewsCollection.FirstOrDefault().Id = string.Empty;
            employmentPerformanceReviewsServiceMock.Setup(i => i.PutEmploymentPerformanceReviewsAsync(It.IsAny<string>(), It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new ArgumentNullException());
            await employmentPerformanceReviewsController.PutEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault().Id, employmentPerformanceReviewsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PutEmploymentPerformanceReviewAsync_InvalidOperationException()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PutEmploymentPerformanceReviewsAsync(It.IsAny<string>(), It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new InvalidOperationException());
            await employmentPerformanceReviewsController.PutEmploymentPerformanceReviewsAsync("id", employmentPerformanceReviewsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PutEmploymentPerformanceReviewAsync_KeyNotFoundException()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PutEmploymentPerformanceReviewsAsync(It.IsAny<string>(), It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new KeyNotFoundException());
            await employmentPerformanceReviewsController.PutEmploymentPerformanceReviewsAsync("id", employmentPerformanceReviewsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PutEmploymentPerformanceReviewAsync_Exception()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PutEmploymentPerformanceReviewsAsync(It.IsAny<string>(), It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new Exception());
            await employmentPerformanceReviewsController.PutEmploymentPerformanceReviewsAsync("id", employmentPerformanceReviewsCollection.FirstOrDefault());
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task EmploymentPerformanceReviewsController_PostEmploymentPerformanceReviewAsync()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault())).ReturnsAsync(employmentPerformanceReviewsCollection.FirstOrDefault());
            var result = await employmentPerformanceReviewsController.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault());
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Id, result.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Person.Id, result.Person.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Job.Id, result.Job.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Type.Id, result.Type.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Rating.Detail.Id, result.Rating.Detail.Id);
            Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().CompletedOn, result.CompletedOn);
            if (result.Comment != null)
                Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().Comment, result.Comment);
            if (result.ReviewedBy != null)
                Assert.AreEqual(employmentPerformanceReviewsCollection.FirstOrDefault().ReviewedBy.Id, result.ReviewedBy);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PostEmploymentPerformanceReviewAsync_ArgumentNullException()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PostEmploymentPerformanceReviewsAsync(It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new ArgumentNullException());
            await employmentPerformanceReviewsController.PostEmploymentPerformanceReviewsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PostEmploymentPerformanceReviewAsync_IdNull_ArgumentNullException()
        {
            employmentPerformanceReviewsCollection.FirstOrDefault().Id = string.Empty;
            employmentPerformanceReviewsServiceMock.Setup(i => i.PostEmploymentPerformanceReviewsAsync(It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new ArgumentNullException());
            await employmentPerformanceReviewsController.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PostEmploymentPerformanceReviewAsync_InvalidOperationException()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PostEmploymentPerformanceReviewsAsync(It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new InvalidOperationException());
            await employmentPerformanceReviewsController.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PostEmploymentPerformanceReviewAsync_KeyNotFoundException()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PostEmploymentPerformanceReviewsAsync(It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new KeyNotFoundException());
            await employmentPerformanceReviewsController.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewsController_PostEmploymentPerformanceReviewAsync_Exception()
        {
            employmentPerformanceReviewsServiceMock.Setup(i => i.PostEmploymentPerformanceReviewsAsync(It.IsAny<Dtos.EmploymentPerformanceReviews>())).ThrowsAsync(new Exception());
            await employmentPerformanceReviewsController.PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsCollection.FirstOrDefault());
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task EmploymentPerformanceReviewsController_DeleteEmploymentPerformanceReviewAsync_HttpResponseMessage()
        {
            string id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
            employmentPerformanceReviewsServiceMock.Setup(i => i.DeleteEmploymentPerformanceReviewAsync(id)).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            await employmentPerformanceReviewsController.DeleteEmploymentPerformanceReviewsAsync(id);
        }
        #endregion
    }

}