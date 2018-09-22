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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class MealPlanRequestsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IMealPlanRequestsService> mealPlanRequestsServiceMock;
        private Mock<ILogger> loggerMock;
        private MealPlanRequestsController mealPlanRequestsController; 
        private List<Dtos.MealPlanRequests> mealPlanRequestDtosCollection;
        private Tuple<IEnumerable<Dtos.MealPlanRequests>, int> mealPlanRequestsTuple;
        int offset = 0;
        int limit = 10;

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            mealPlanRequestsServiceMock = new Mock<IMealPlanRequestsService>();
            loggerMock = new Mock<ILogger>();

            BuildData();

            mealPlanRequestsController = new MealPlanRequestsController(mealPlanRequestsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            mealPlanRequestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            mealPlanRequestsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            mealPlanRequestsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(mealPlanRequestDtosCollection.FirstOrDefault()));
        }

        private void BuildData()
        {
            mealPlanRequestDtosCollection = new List<Dtos.MealPlanRequests>() 
            { 
                new Dtos.MealPlanRequests()
                {
                    AcademicPeriod = new GuidObject2("0949eed8-5fe7-4120-b1cf-f23266b9e874"),
                    EndOn = DateTime.Today.AddDays(45),
                    Id = "d36dd6b0-7cb7-417c-94ab-f4993838ae01",
                    MealPlan = new GuidObject2("9809ed31-7c07-4c3c-9f17-8f3caa42238e") ,
                    Person = new GuidObject2("af840786-99e5-4227-beee-9ef07a652902"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.MealPlanRequestsStatus.Approved,
                    SubmittedOn = DateTime.Today
                },
                new Dtos.MealPlanRequests()
                {
                    AcademicPeriod = new GuidObject2("af43de07-cf49-43a5-a47d-0f8531a639e0"),
                    EndOn = DateTime.Today.AddDays(45),
                    Id = "4010f918-d40d-4083-a4e9-6f484ea3cbb0",
                    MealPlan = new GuidObject2("49a23389-170a-4fd9-8924-2992a19ad097") ,
                    Person = new GuidObject2("e008d0ff-3ff4-4875-9856-d614beb1eb31"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.MealPlanRequestsStatus.Approved,
                    SubmittedOn = DateTime.Today
                },
                new Dtos.MealPlanRequests()
                {
                    AcademicPeriod = new GuidObject2("01de0a60-376a-41ad-897d-bdbf3d6378ff"),
                    EndOn = DateTime.Today.AddDays(45),
                    Id = "13ba4b74-cfd5-4250-a89d-9209f5eb2e4c",
                    MealPlan = new GuidObject2("5fa7e43b-0d48-41e3-a843-1f48ee5c0d9d") ,
                    Person = new GuidObject2("eac8b262-dd0b-4534-ad54-30b989179757"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.MealPlanRequestsStatus.Approved,
                    SubmittedOn = DateTime.Today

                }
            };
            mealPlanRequestsTuple = new Tuple<IEnumerable<MealPlanRequests>, int>(mealPlanRequestDtosCollection, mealPlanRequestDtosCollection.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            mealPlanRequestsController = null;
            mealPlanRequestDtosCollection = null;
            loggerMock = null;
            mealPlanRequestsServiceMock = null;
            mealPlanRequestsTuple = null;
            TestContext = null;
        }

        [TestMethod]
        public async Task MealPlanRequestsController_GetMealPlanRequests_ValidateFields_Nocache()
        {
            mealPlanRequestsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false, Public = true };

            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(offset, limit, false)).ReturnsAsync(mealPlanRequestsTuple);
       
            var results = await mealPlanRequestsController.GetMealPlanRequestsAsync(new Paging(limit, offset));
            Assert.IsNotNull(results);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.MealPlanRequests> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRequests>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.MealPlanRequests>;
            Assert.AreEqual(mealPlanRequestDtosCollection.Count(), actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = mealPlanRequestDtosCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.SubmittedOn, actual.SubmittedOn);
            }
        }

        [TestMethod]
        public async Task MealPlanRequestsController_GetMealPlanRequests_ValidateFields_Cache()
        {
            mealPlanRequestsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, Public = true };

            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(offset, limit, true)).ReturnsAsync(mealPlanRequestsTuple);

            var results = await mealPlanRequestsController.GetMealPlanRequestsAsync(new Paging(limit, offset));
            Assert.IsNotNull(results);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.MealPlanRequests> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRequests>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.MealPlanRequests>;
            Assert.AreEqual(mealPlanRequestDtosCollection.Count(), actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = mealPlanRequestDtosCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.SubmittedOn, actual.SubmittedOn);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequests_KeyNotFoundException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            await mealPlanRequestsController.GetMealPlanRequestsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequests_PermissionsException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            await mealPlanRequestsController.GetMealPlanRequestsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequests_ArgumentException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            await mealPlanRequestsController.GetMealPlanRequestsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequests_RepositoryException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            await mealPlanRequestsController.GetMealPlanRequestsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequests_IntegrationApiException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            await mealPlanRequestsController.GetMealPlanRequestsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequests_Exception()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            await mealPlanRequestsController.GetMealPlanRequestsAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequestsByGuidAsync_NullId()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await mealPlanRequestsController.GetMealPlanRequestsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequestsByGuidAsync_KeyNotFoundException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await mealPlanRequestsController.GetMealPlanRequestsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequestsByGuidAsync_PermissionsException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await mealPlanRequestsController.GetMealPlanRequestsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequestsByGuidAsync_ArgumentException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await mealPlanRequestsController.GetMealPlanRequestsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequestsByGuidAsync_RepositoryException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await mealPlanRequestsController.GetMealPlanRequestsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequestsByGuidAsync_IntegrationApiException()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await mealPlanRequestsController.GetMealPlanRequestsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_GetMealPlanRequestsByGuidAsync_Exception()
        {
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await mealPlanRequestsController.GetMealPlanRequestsByGuidAsync("1");
        }

        #region POST_V10

        [TestMethod]
        public async Task MealPlanRequestsController_CreateMealPlanRequest()
        {
            MealPlanRequests record = mealPlanRequestDtosCollection.FirstOrDefault();
            mealPlanRequestsServiceMock.Setup(s => s.PostMealPlanRequestsAsync(record)).ReturnsAsync(mealPlanRequestDtosCollection.FirstOrDefault());

            var mealPlanRequest = await mealPlanRequestsController.PostMealPlanRequestsAsync(record);
            Assert.AreEqual(mealPlanRequest.Id, record.Id);
            Assert.AreEqual(mealPlanRequest.AcademicPeriod, record.AcademicPeriod);
            Assert.AreEqual(mealPlanRequest.EndOn, record.EndOn);
            Assert.AreEqual(mealPlanRequest.MealPlan, record.MealPlan);
            Assert.AreEqual(mealPlanRequest.Person, record.Person);
            Assert.AreEqual(mealPlanRequest.StartOn, record.StartOn);
            Assert.AreEqual(mealPlanRequest.Status, record.Status);
            Assert.AreEqual(mealPlanRequest.SubmittedOn, record.SubmittedOn);
        }

        #region EXCEPTIONS

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_ArgumentNull()
        {
            await mealPlanRequestsController.PostMealPlanRequestsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_Id_Null()
        {
            await mealPlanRequestsController.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = string.Empty });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_KeyNotFoundException()
        {
            mealPlanRequestsServiceMock
                .Setup(s => s.PostMealPlanRequestsAsync(It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new KeyNotFoundException());
            await mealPlanRequestsController.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_PermissionsException()
        {
            mealPlanRequestsServiceMock
                .Setup(s => s.PostMealPlanRequestsAsync(It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new PermissionsException());
            await mealPlanRequestsController.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_ArgumentException()
        {
            mealPlanRequestsServiceMock
                .Setup(s => s.PostMealPlanRequestsAsync(It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new ArgumentException());
            await mealPlanRequestsController.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_RepositoryException()
        {
            mealPlanRequestsServiceMock
                .Setup(s => s.PostMealPlanRequestsAsync(It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new RepositoryException());
            await mealPlanRequestsController.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_IntegrationApiException()
        {
            mealPlanRequestsServiceMock
                .Setup(s => s.PostMealPlanRequestsAsync(It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new IntegrationApiException());
            await mealPlanRequestsController.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PostMealPlanRequestsAsync_Exception()
        {
            mealPlanRequestsServiceMock
                .Setup(s => s.PostMealPlanRequestsAsync(It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new Exception());
            await mealPlanRequestsController.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = "123" });
        }

        #endregion

        #endregion

        #region PUT_V10

        [TestMethod]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync()
        {
            MealPlanRequests record = mealPlanRequestDtosCollection.FirstOrDefault();
            string guid = record.Id;
            mealPlanRequestsServiceMock.Setup(x => x.PutMealPlanRequestsAsync(guid, It.IsAny<MealPlanRequests>())).ReturnsAsync(record);
            mealPlanRequestsServiceMock.Setup(x => x.GetMealPlanRequestsByGuidAsync(guid)).ReturnsAsync(record);
            var result = await mealPlanRequestsController.PutMealPlanRequestsAsync(guid, record);
            Assert.AreEqual(result.Id, record.Id);
            Assert.AreEqual(result.AcademicPeriod, record.AcademicPeriod);
            Assert.AreEqual(result.EndOn, record.EndOn);
            Assert.AreEqual(result.MealPlan, record.MealPlan);
            Assert.AreEqual(result.Person, record.Person);
            Assert.AreEqual(result.StartOn, record.StartOn);
            Assert.AreEqual(result.Status, record.Status);
            Assert.AreEqual(result.SubmittedOn, record.SubmittedOn);
        }

        #region EXCEPTIONS

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_ArgumentNull()
        {
            await mealPlanRequestsController.PutMealPlanRequestsAsync("123", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_GuidAsNull()
        {
            await mealPlanRequestsController.PutMealPlanRequestsAsync(null, new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_GuidMismatch()
        {
            await mealPlanRequestsController.PutMealPlanRequestsAsync("234", new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_EmptyGuid()
        {
            await mealPlanRequestsController.PutMealPlanRequestsAsync(Guid.Empty.ToString(), new MealPlanRequests() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_KeyNotFoundException()
        {
            //mealPlanRequestsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            mealPlanRequestsServiceMock
                .Setup(s => s.PutMealPlanRequestsAsync(It.IsAny<string>(), It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new KeyNotFoundException());
            await mealPlanRequestsController.PutMealPlanRequestsAsync("123", new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_PermissionsException()
        {
            //mealPlanRequestsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            mealPlanRequestsServiceMock
                .Setup(s => s.PutMealPlanRequestsAsync(It.IsAny<string>(), It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new PermissionsException());
            await mealPlanRequestsController.PutMealPlanRequestsAsync("123", new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_ArgumentException()
        {
            //mealPlanRequestsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            mealPlanRequestsServiceMock
                .Setup(s => s.PutMealPlanRequestsAsync(It.IsAny<string>(), It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new ArgumentException());
            await mealPlanRequestsController.PutMealPlanRequestsAsync("123", new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_RepositoryException()
        {
            //mealPlanRequestsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            mealPlanRequestsServiceMock
                .Setup(s => s.PutMealPlanRequestsAsync(It.IsAny<string>(), It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new RepositoryException());
            await mealPlanRequestsController.PutMealPlanRequestsAsync("123", new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_IntegrationApiException()
        {
            //mealPlanRequestsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            mealPlanRequestsServiceMock
                .Setup(s => s.PutMealPlanRequestsAsync(It.IsAny<string>(), It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new IntegrationApiException());
            await mealPlanRequestsController.PutMealPlanRequestsAsync("123", new MealPlanRequests() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_PutMealPlanRequestsAsync_Exception()
        {
            //mealPlanRequestsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            mealPlanRequestsServiceMock
                .Setup(s => s.PutMealPlanRequestsAsync(It.IsAny<string>(), It.IsAny<MealPlanRequests>()))
                .ThrowsAsync(new Exception());
            await mealPlanRequestsController.PutMealPlanRequestsAsync("123", new MealPlanRequests() { Id = "123" });
        }

        #endregion

        #endregion

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MealPlanRequestsController_DeleteMealPlanRequestsAsync_Exception()
        {
            await mealPlanRequestsController.DeleteMealPlanRequestsAsync(mealPlanRequestDtosCollection.FirstOrDefault().Id);
        }
    }
}