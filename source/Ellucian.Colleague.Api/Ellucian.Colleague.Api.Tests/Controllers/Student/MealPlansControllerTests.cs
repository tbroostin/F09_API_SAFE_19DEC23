//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class MealPlansControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IMealPlansService> mealPlansServiceMock;
        private Mock<ILogger> loggerMock;
        private MealPlansController mealPlansController;      
        private IEnumerable<MealPlan> allMealPlans;
        private List<Dtos.MealPlans> mealPlansCollection;

        [TestInitialize]
        public async void Initialize() 
        {
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            mealPlansServiceMock = new Mock<IMealPlansService>();
            loggerMock = new Mock<ILogger>();
            mealPlansCollection = new List<Dtos.MealPlans>();

            allMealPlans  = new List<MealPlan>()
                {
                    new MealPlan("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new MealPlan("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new MealPlan("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allMealPlans)
            {
                var mealPlans = new Ellucian.Colleague.Dtos.MealPlans
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                mealPlansCollection.Add(mealPlans);
            }

            mealPlansController = new MealPlansController(mealPlansServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            mealPlansController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            mealPlansController = null;
            allMealPlans = null;
            mealPlansCollection = null;
            loggerMock = null;
            mealPlansServiceMock = null;
        }

        [TestMethod]
        public async Task MealPlansController_GetMealPlans_ValidateFields_Nocache()
        {
            mealPlansController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            mealPlansServiceMock.Setup(x => x.GetMealPlansAsync(false)).ReturnsAsync(mealPlansCollection);
       
            var sourceContexts = (await mealPlansController.GetMealPlansAsync()).ToList();
            Assert.AreEqual(mealPlansCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = mealPlansCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task MealPlansController_GetMealPlans_ValidateFields_Cache()
        {
            mealPlansController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            mealPlansServiceMock.Setup(x => x.GetMealPlansAsync(true)).ReturnsAsync(mealPlansCollection);

            var sourceContexts = (await mealPlansController.GetMealPlansAsync()).ToList();
            Assert.AreEqual(mealPlansCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = mealPlansCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task MealPlansController_GetMealPlansByGuidAsync_ValidateFields()
        {
            var expected = mealPlansCollection.FirstOrDefault();
            mealPlansServiceMock.Setup(x => x.GetMealPlansByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await mealPlansController.GetMealPlansByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlansController_GetMealPlans_Exception()
        {
            mealPlansServiceMock.Setup(x => x.GetMealPlansAsync(false)).Throws<Exception>();
            await mealPlansController.GetMealPlansAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlansController_GetMealPlansByGuidAsync_Exception()
        {
            mealPlansServiceMock.Setup(x => x.GetMealPlansByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await mealPlansController.GetMealPlansByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlansController_PostMealPlansAsync_Exception()
        {
            await mealPlansController.PostMealPlansAsync(mealPlansCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlansController_PutMealPlansAsync_Exception()
        {
            var sourceContext = mealPlansCollection.FirstOrDefault();
            await mealPlansController.PutMealPlansAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlansController_DeleteMealPlansAsync_Exception()
        {
            await mealPlansController.DeleteMealPlansAsync(mealPlansCollection.FirstOrDefault().Id);
        }
    }
}