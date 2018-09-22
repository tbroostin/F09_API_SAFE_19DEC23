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
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class MealTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IMealTypesService> mealTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private MealTypesController mealTypesController;      
        private IEnumerable<MealType> allMealTypes;
        private List<Dtos.MealTypes> mealTypesCollection;

        [TestInitialize]
        public async void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            mealTypesServiceMock = new Mock<IMealTypesService>();
            loggerMock = new Mock<ILogger>();
            mealTypesCollection = new List<Dtos.MealTypes>();

            allMealTypes  = new List<MealType>()
                {
                    new MealType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new MealType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new MealType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allMealTypes)
            {
                var mealTypes = new Ellucian.Colleague.Dtos.MealTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                mealTypesCollection.Add(mealTypes);
            }

            mealTypesController = new MealTypesController(mealTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            mealTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            mealTypesController = null;
            allMealTypes = null;
            mealTypesCollection = null;
            loggerMock = null;
            mealTypesServiceMock = null;
        }

        [TestMethod]
        public async Task MealTypesController_GetMealTypes_ValidateFields_Nocache()
        {
            mealTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            mealTypesServiceMock.Setup(x => x.GetMealTypesAsync(false)).ReturnsAsync(mealTypesCollection);
       
            var sourceContexts = (await mealTypesController.GetMealTypesAsync()).ToList();
            Assert.AreEqual(mealTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = mealTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task MealTypesController_GetMealTypes_ValidateFields_Cache()
        {
            mealTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            mealTypesServiceMock.Setup(x => x.GetMealTypesAsync(true)).ReturnsAsync(mealTypesCollection);

            var sourceContexts = (await mealTypesController.GetMealTypesAsync()).ToList();
            Assert.AreEqual(mealTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = mealTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task MealTypesController_GetMealTypesByGuidAsync_ValidateFields()
        {
            var expected = mealTypesCollection.FirstOrDefault();
            mealTypesServiceMock.Setup(x => x.GetMealTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await mealTypesController.GetMealTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealTypesController_GetMealTypes_Exception()
        {
            mealTypesServiceMock.Setup(x => x.GetMealTypesAsync(false)).Throws<Exception>();
            await mealTypesController.GetMealTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealTypesController_GetMealTypesByGuidAsync_Exception()
        {
            mealTypesServiceMock.Setup(x => x.GetMealTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await mealTypesController.GetMealTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealTypesController_PostMealTypesAsync_Exception()
        {
            await mealTypesController.PostMealTypesAsync(mealTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealTypesController_PutMealTypesAsync_Exception()
        {
            var sourceContext = mealTypesCollection.FirstOrDefault();
            await mealTypesController.PutMealTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealTypesController_DeleteMealTypesAsync_Exception()
        {
            await mealTypesController.DeleteMealTypesAsync(mealTypesCollection.FirstOrDefault().Id);
        }
    }
}