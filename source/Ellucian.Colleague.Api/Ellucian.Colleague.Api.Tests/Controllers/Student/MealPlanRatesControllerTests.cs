//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class MealPlanRatesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IMealPlanRatesService> mealPlanRatesServiceMock;
        private Mock<ILogger> loggerMock;
        private MealPlanRatesController mealPlanRatesController;
        private IEnumerable<Domain.Student.Entities.MealPlanRates> allMealPlans;
        private List<Dtos.MealPlanRates> mealPlanRatesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            mealPlanRatesServiceMock = new Mock<IMealPlanRatesService>();
            loggerMock = new Mock<ILogger>();
            mealPlanRatesCollection = new List<Dtos.MealPlanRates>();

            allMealPlans = new List<Domain.Student.Entities.MealPlanRates>()
                {
                    new Domain.Student.Entities.MealPlanRates("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.MealPlanRates("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.MealPlanRates("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allMealPlans)
            {
                var mealPlanRates = new Ellucian.Colleague.Dtos.MealPlanRates
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                mealPlanRatesCollection.Add(mealPlanRates);
            }

            mealPlanRatesController = new MealPlanRatesController(mealPlanRatesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            mealPlanRatesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            mealPlanRatesController = null;
            allMealPlans = null;
            mealPlanRatesCollection = null;
            loggerMock = null;
            mealPlanRatesServiceMock = null;
        }

        [TestMethod]
        public async Task MealPlanRatesController_GetMealPlanRates_ValidateFields_Nocache()
        {
            mealPlanRatesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(false)).ReturnsAsync(mealPlanRatesCollection);

            var sourceContexts = (await mealPlanRatesController.GetMealPlanRatesAsync()).ToList();
            Assert.AreEqual(mealPlanRatesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = mealPlanRatesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task MealPlanRatesController_GetMealPlanRates_ValidateFields_Cache()
        {
            mealPlanRatesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(true)).ReturnsAsync(mealPlanRatesCollection);

            var sourceContexts = (await mealPlanRatesController.GetMealPlanRatesAsync()).ToList();
            Assert.AreEqual(mealPlanRatesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = mealPlanRatesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRates_KeyNotFoundException()
        {
            //
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(false))
                .Throws<KeyNotFoundException>();
            await mealPlanRatesController.GetMealPlanRatesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRates_PermissionsException()
        {

            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(false))
                .Throws<PermissionsException>();
            await mealPlanRatesController.GetMealPlanRatesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRates_ArgumentException()
        {

            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(false))
                .Throws<ArgumentException>();
            await mealPlanRatesController.GetMealPlanRatesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRates_RepositoryException()
        {

            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(false))
                .Throws<RepositoryException>();
            await mealPlanRatesController.GetMealPlanRatesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRates_IntegrationApiException()
        {

            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(false))
                .Throws<IntegrationApiException>();
            await mealPlanRatesController.GetMealPlanRatesAsync();
        }

        [TestMethod]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuidAsync_ValidateFields()
        {
            var expected = mealPlanRatesCollection.FirstOrDefault();
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await mealPlanRatesController.GetMealPlanRatesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRates_Exception()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesAsync(false)).Throws<Exception>();
            await mealPlanRatesController.GetMealPlanRatesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuidAsync_Exception()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await mealPlanRatesController.GetMealPlanRatesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuid_KeyNotFoundException()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await mealPlanRatesController.GetMealPlanRatesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuid_PermissionsException()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await mealPlanRatesController.GetMealPlanRatesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuid_ArgumentException()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await mealPlanRatesController.GetMealPlanRatesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuid_RepositoryException()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await mealPlanRatesController.GetMealPlanRatesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuid_IntegrationApiException()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await mealPlanRatesController.GetMealPlanRatesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_GetMealPlanRatesByGuid_Exception()
        {
            mealPlanRatesServiceMock.Setup(x => x.GetMealPlanRatesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await mealPlanRatesController.GetMealPlanRatesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_PostMealPlanRatesAsync_Exception()
        {
            await mealPlanRatesController.PostMealPlanRatesAsync(mealPlanRatesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_PutMealPlanRatesAsync_Exception()
        {
            var sourceContext = mealPlanRatesCollection.FirstOrDefault();
            await mealPlanRatesController.PutMealPlanRatesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRatesController_DeleteMealPlanRatesAsync_Exception()
        {
            await mealPlanRatesController.DeleteMealPlanRatesAsync(mealPlanRatesCollection.FirstOrDefault().Id);
        }
    }
}