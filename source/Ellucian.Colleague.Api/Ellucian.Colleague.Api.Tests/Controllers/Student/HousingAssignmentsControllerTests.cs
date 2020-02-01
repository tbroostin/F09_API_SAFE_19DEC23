// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class HousingAssignmentsControllerTests
    {
        [TestClass]
        public class HousingAssignmentControllerTests_GET
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private HousingAssignmentController housingAssignmentController;
            private Mock<IHousingAssignmentService> housingAssignmentServiceMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<Dtos.HousingAssignment> housingAssignmentsCollection;
            private IEnumerable<Dtos.HousingAssignment2> housingAssignments2Collection;
            private Tuple<IEnumerable<Dtos.HousingAssignment>, int> housingAssignmentsTuple;
            private Tuple<IEnumerable<Dtos.HousingAssignment2>, int> housingAssignments2Tuple;
            private IEnumerable<HousingAssignmentAdditionalChargeProperty> additionalCharges;
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                housingAssignmentServiceMock = new Mock<IHousingAssignmentService>();
                loggerMock = new Mock<ILogger>();

                housingAssignmentController = new HousingAssignmentController(housingAssignmentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                housingAssignmentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                housingAssignmentController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                InitializeTestData();
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingAssignmentController = null;
                housingAssignmentServiceMock = null;
                loggerMock = null;
                TestContext = null;
            }

            private void InitializeTestData()
            {
                additionalCharges = new List<HousingAssignmentAdditionalChargeProperty>()
                {
                    new HousingAssignmentAdditionalChargeProperty()
                    {
                        AccountingCode = new GuidObject2("3a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        HousingAssignmentRate = new HousingAssignmentRateChargeProperty() {RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.USD, RateValue =2 }
                    }
                };

                housingAssignmentsCollection = new List<Dtos.HousingAssignment>()
                {
                    new Dtos.HousingAssignment()
                    {
                        Id = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", AcademicPeriod = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        AdditionalCharges = additionalCharges, Comment = "Comment", ContractNumber = "ContractNumber",
                        StartOn = DateTime.Today, EndOn = DateTime.Today.AddDays(100), Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned
                    },
                    new Dtos.HousingAssignment()
                    {
                        Id = "1b49eed8-5fe7-4120-b1cf-f23266b9e874", AcademicPeriod = new GuidObject2("2b49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        AdditionalCharges = additionalCharges, Comment = "Comment", ContractNumber = "ContractNumber",
                        StartOn = DateTime.Today, EndOn = DateTime.Today.AddDays(100), Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned
                    }
                };

                housingAssignments2Collection = new List<Dtos.HousingAssignment2>()
                {
                    new Dtos.HousingAssignment2()
                    {
                        Id = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", AcademicPeriod = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        AdditionalCharges = additionalCharges, Comment = "Comment", ContractNumber = "ContractNumber",
                        StartOn = DateTime.Today, EndOn = DateTime.Today.AddDays(100), Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned
                    },
                    new Dtos.HousingAssignment2()
                    {
                        Id = "1b49eed8-5fe7-4120-b1cf-f23266b9e874", AcademicPeriod = new GuidObject2("2b49eed8-5fe7-4120-b1cf-f23266b9e874"),
                        AdditionalCharges = additionalCharges, Comment = "Comment", ContractNumber = "ContractNumber",
                        StartOn = DateTime.Today, EndOn = DateTime.Today.AddDays(100), Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned
                    }
                };

                housingAssignmentsTuple = new Tuple<IEnumerable<Dtos.HousingAssignment>, int>(housingAssignmentsCollection, housingAssignmentsCollection.Count());
                housingAssignments2Tuple = new Tuple<IEnumerable<Dtos.HousingAssignment2>, int>(housingAssignments2Collection, housingAssignments2Collection.Count());
            }

            #endregion

            #region CACHE-NOCACHE

            [TestMethod]
            public async Task HousingAssignmentController_GetHousingAssignments_ValidateFields_Nocache()
            {
                housingAssignmentController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };

                housingAssignmentServiceMock.Setup(x => x.GetHousingAssignmentsAsync(0, 10, It.IsAny<Dtos.HousingAssignment>(), false)).ReturnsAsync(housingAssignmentsTuple);

                var results = await housingAssignmentController.GetHousingAssignmentsAsync(new Paging(10, 0), criteriaFilter);

                Assert.IsNotNull(results);

                var cancelToken = new CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.HousingAssignment> actuals =
                    ((ObjectContent<IEnumerable<Dtos.HousingAssignment>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.HousingAssignment>;

                Assert.AreEqual(housingAssignmentsCollection.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = housingAssignmentsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.AdditionalCharges.FirstOrDefault().AccountingCode.Id, actual.AdditionalCharges.FirstOrDefault().AccountingCode.Id);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                }
            }

            [TestMethod]
            public async Task MealPlanRequestsController_GetMealPlanRequests_ValidateFields_Cache()
            {
                housingAssignmentController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                housingAssignmentServiceMock.Setup(x => x.GetHousingAssignmentsAsync(0, 10, It.IsAny<Dtos.HousingAssignment>(), true)).ReturnsAsync(housingAssignmentsTuple);

                var results = await housingAssignmentController.GetHousingAssignmentsAsync(new Paging(10, 0), criteriaFilter);

                Assert.IsNotNull(results);

                var cancelToken = new CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.HousingAssignment> actuals =
                    ((ObjectContent<IEnumerable<Dtos.HousingAssignment>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.HousingAssignment>;

                Assert.AreEqual(housingAssignmentsCollection.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = housingAssignmentsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.AdditionalCharges.FirstOrDefault().AccountingCode.Id, actual.AdditionalCharges.FirstOrDefault().AccountingCode.Id);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                }
            }

            #endregion

            #region GETALL

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignments_keyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.GetHousingAssignmentsAsync(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignments_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await housingAssignmentController.GetHousingAssignmentsAsync(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignments_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await housingAssignmentController.GetHousingAssignmentsAsync(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignments_RepositoryException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await housingAssignmentController.GetHousingAssignmentsAsync(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignments_IntegrationApiException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await housingAssignmentController.GetHousingAssignmentsAsync(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignments_Exception()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await housingAssignmentController.GetHousingAssignmentsAsync(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignments2_keyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.GetHousingAssignments2Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignments2_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await housingAssignmentController.GetHousingAssignments2Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignments2_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentException());
                await housingAssignmentController.GetHousingAssignments2Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignments22_RepositoryException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await housingAssignmentController.GetHousingAssignments2Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignments2_IntegrationApiException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await housingAssignmentController.GetHousingAssignments2Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignments2_Exception()
            {
                housingAssignmentController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await housingAssignmentController.GetHousingAssignments2Async(null, criteriaFilter);
            }

            [TestMethod]            
            public async Task GetHousingAssignments2()
            {
                housingAssignmentController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>()))
                    .ReturnsAsync(housingAssignments2Tuple);
                var results = await housingAssignmentController.GetHousingAssignments2Async(null, criteriaFilter);
                Assert.IsNotNull(results);
            }
            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid_IntegrationApiException_When_Guid_NullOrEmpty()
            {
                await housingAssignmentController.GetHousingAssignmentByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid_KeyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.GetHousingAssignmentByGuidAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await housingAssignmentController.GetHousingAssignmentByGuidAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await housingAssignmentController.GetHousingAssignmentByGuidAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid_RepositoryException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await housingAssignmentController.GetHousingAssignmentByGuidAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid_IntegrationApiException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await housingAssignmentController.GetHousingAssignmentByGuidAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid_Exception()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await housingAssignmentController.GetHousingAssignmentByGuidAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            public async Task HousingAssignmentController_GetHousingAssignmentByGuid()
            {
                housingAssignmentController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(housingAssignmentsCollection.FirstOrDefault());
                var actual = await housingAssignmentController.GetHousingAssignmentByGuidAsync(Guid.NewGuid().ToString());

                var expected = housingAssignmentsCollection.FirstOrDefault();

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.AdditionalCharges.FirstOrDefault().AccountingCode.Id, actual.AdditionalCharges.FirstOrDefault().AccountingCode.Id);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignmentByGuid2_IntegrationApiException_When_Guid_NullOrEmpty()
            {
                await housingAssignmentController.GetHousingAssignmentByGuid2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignmentByGuid2_KeyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.GetHousingAssignmentByGuid2Async(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignmentByGuid2_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await housingAssignmentController.GetHousingAssignmentByGuid2Async(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignmentByGuid2_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await housingAssignmentController.GetHousingAssignmentByGuid2Async(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignmentByGuid2_RepositoryException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await housingAssignmentController.GetHousingAssignmentByGuid2Async(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignmentByGuid2_IntegrationApiException()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await housingAssignmentController.GetHousingAssignmentByGuid2Async(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetHousingAssignmentByGuid2_Exception()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await housingAssignmentController.GetHousingAssignmentByGuid2Async(Guid.NewGuid().ToString());
            }

            [TestMethod]
            public async Task GetHousingAssignmentByGuid2()
            {
                housingAssignmentController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(housingAssignments2Collection.FirstOrDefault());
                var actual = await housingAssignmentController.GetHousingAssignmentByGuid2Async(Guid.NewGuid().ToString());

                var expected = housingAssignmentsCollection.FirstOrDefault();

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.AdditionalCharges.FirstOrDefault().AccountingCode.Id, actual.AdditionalCharges.FirstOrDefault().AccountingCode.Id);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
            }
            #endregion

            #region UNSUPPORTED

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task HousingAssignmentController_DeleteHousingAssignment_UnSupported_Exception()
            {
                await housingAssignmentController.DeleteHousingAssignmentAsync(It.IsAny<string>());
            }

            #endregion
        }

        [TestClass]
        public class HousingAssignmentControllerTests_POST
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private HousingAssignmentController housingAssignmentController;
            private Mock<IHousingAssignmentService> housingAssignmentServiceMock;
            private Mock<ILogger> loggerMock;

            private HousingAssignment housingAssignment;
            private HousingAssignment2 housingAssignment2;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                housingAssignmentServiceMock = new Mock<IHousingAssignmentService>();
                loggerMock = new Mock<ILogger>();

                housingAssignmentController = new HousingAssignmentController(housingAssignmentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                housingAssignmentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                housingAssignmentController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                InitializeTestData();
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingAssignmentController = null;
                housingAssignmentServiceMock = null;
                loggerMock = null;
                TestContext = null;
                housingAssignment = null;
                housingAssignment2 = null;
            }

            private void InitializeTestData()
            {
                housingAssignment = new Dtos.HousingAssignment()
                {
                    Id = Guid.Empty.ToString(),
                    AcademicPeriod = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Comment = "Comment",
                    ContractNumber = "ContractNumber",
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(100),
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    Person = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StatusDate = DateTime.Today
                };

                housingAssignment2 = new Dtos.HousingAssignment2()
                {
                    Id = Guid.Empty.ToString(),
                    AcademicPeriod = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Comment = "Comment",
                    ContractNumber = "ContractNumber",
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(100),
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    Person = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StatusDate = DateTime.Today
                };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Null()
            {
                await housingAssignmentController.PostHousingAssignmentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Id_Null()
            {
                housingAssignment.Id = null;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Person_Null()
            {
                housingAssignment.Person = null;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Person_Id_Null()
            {
                housingAssignment.Person.Id = null;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Room_Null()
            {
                housingAssignment.Room = null;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Room_Id_Null()
            {
                housingAssignment.Room.Id = null;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_StartOn_Null()
            {
                housingAssignment.StartOn = null;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_StartOn_GreaterThan_Endon_Null()
            {
                housingAssignment.StartOn = DateTime.Today.AddDays(1);
                housingAssignment.EndOn = DateTime.Today;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Invalid_Status()
            {
                housingAssignment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.NotSet;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_IntegrationApiException_HousingAssignment_StatusDate_Null()
            {
                housingAssignment.StatusDate = null;

                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_ArgumentNullException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignmentAsync(It.IsAny<HousingAssignment>())).ThrowsAsync(new ArgumentNullException());
                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignmentAsync(It.IsAny<HousingAssignment>())).ThrowsAsync(new ArgumentException());
                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_InvalidOperationException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignmentAsync(It.IsAny<HousingAssignment>())).ThrowsAsync(new InvalidOperationException());
                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignmentAsync(It.IsAny<HousingAssignment>())).ThrowsAsync(new PermissionsException());
                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_Exception()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignmentAsync(It.IsAny<HousingAssignment>())).ThrowsAsync(new Exception());
                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignmentAsync_KeyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignmentAsync(It.IsAny<HousingAssignment>())).ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            public async Task HousingAssignmentController_PostHousingAssignmentAsync()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignmentAsync(It.IsAny<HousingAssignment>())).ReturnsAsync(housingAssignment);
                var result = await housingAssignmentController.PostHousingAssignmentAsync(housingAssignment);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Null()
            {
                await housingAssignmentController.PostHousingAssignment2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Id_Null()
            {
                housingAssignment2.Id = null;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Id_Not_Nil_Guid()
            {
                housingAssignment2.Id = Guid.NewGuid().ToString();

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Person_Null()
            {
                housingAssignment2.Person = null;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Person_Id_Null()
            {
                housingAssignment2.Person.Id = null;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Room_Null()
            {
                housingAssignment2.Room = null;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Room_Id_Null()
            {
                housingAssignment2.Room.Id = null;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_StartOn_Null()
            {
                housingAssignment2.StartOn = null;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_StartOn_GreaterThan_Endon_Null()
            {
                housingAssignment2.StartOn = DateTime.Today.AddDays(1);
                housingAssignment2.EndOn = DateTime.Today;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_Invalid_Status()
            {
                housingAssignment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.NotSet;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_IntegrationApiException_HousingAssignment_StatusDate_Null()
            {
                housingAssignment2.StatusDate = null;

                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_ArgumentNullException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignment2Async(It.IsAny<HousingAssignment2>())).ThrowsAsync(new ArgumentNullException());
                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignment2Async(It.IsAny<HousingAssignment2>())).ThrowsAsync(new ArgumentException());
                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_InvalidOperationException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignment2Async(It.IsAny<HousingAssignment2>())).ThrowsAsync(new InvalidOperationException());
                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignment2Async(It.IsAny<HousingAssignment2>())).ThrowsAsync(new PermissionsException());
                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_Exception()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignment2Async(It.IsAny<HousingAssignment2>())).ThrowsAsync(new Exception());
                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostHousingAssignment2Async_KeyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignment2Async(It.IsAny<HousingAssignment2>())).ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
            }

            [TestMethod]
            public async Task HousingAssignmentController_PostHousingAssignment2Async()
            {
                housingAssignmentServiceMock.Setup(s => s.CreateHousingAssignment2Async(It.IsAny<HousingAssignment2>())).ReturnsAsync(housingAssignment2);
                var result = await housingAssignmentController.PostHousingAssignment2Async(housingAssignment2);
                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class HousingAssignmentControllerTests_PUT
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private HousingAssignmentController housingAssignmentController;
            private Mock<IHousingAssignmentService> housingAssignmentServiceMock;
            private Mock<ILogger> loggerMock;

            private HousingAssignment housingAssignment;
            private HousingAssignment2 housingAssignment2;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                housingAssignmentServiceMock = new Mock<IHousingAssignmentService>();
                loggerMock = new Mock<ILogger>();

                InitializeTestData();

                housingAssignmentController = new HousingAssignmentController(housingAssignmentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") } };
                housingAssignmentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                housingAssignmentController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(housingAssignment));
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingAssignmentController = null;
                housingAssignmentServiceMock = null;
                loggerMock = null;
                TestContext = null;
                housingAssignment = null;
            }

            private void InitializeTestData()
            {
                housingAssignment = new Dtos.HousingAssignment()
                {
                    Id = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    AcademicPeriod = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Comment = "Comment",
                    ContractNumber = "ContractNumber",
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(100),
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    Person = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StatusDate = DateTime.Today
                };
                housingAssignment2 = new Dtos.HousingAssignment2()
                {
                    Id = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    AcademicPeriod = new GuidObject2("2a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Comment = "Comment",
                    ContractNumber = "ContractNumber",
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(100),
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    Person = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a49eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StatusDate = DateTime.Today
                };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Guid_Null()
            {
                await housingAssignmentController.PutHousingAssignmentAsync(null, housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Null()
            {
                await housingAssignmentController.PutHousingAssignmentAsync(guid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Id_Null()
            {
                housingAssignment.Id = null;

                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Guid_Empty()
            {
                await housingAssignmentController.PutHousingAssignmentAsync(Guid.Empty.ToString(), housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_IntegrationApiException_HousingAssignment_Id_And_Guid_NotMatched()
            {
                housingAssignment.Id = Guid.Empty.ToString();

                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }            

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_ArgumentNullException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignmentAsync(It.IsAny<string>(), It.IsAny<HousingAssignment>())).ThrowsAsync(new ArgumentNullException());
                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignmentAsync(It.IsAny<string>(), It.IsAny<HousingAssignment>())).ThrowsAsync(new ArgumentException());
                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_InvalidOperationException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignmentAsync(It.IsAny<string>(), It.IsAny<HousingAssignment>())).ThrowsAsync(new InvalidOperationException());
                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignmentAsync(It.IsAny<string>(), It.IsAny<HousingAssignment>())).ThrowsAsync(new PermissionsException());
                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_Exception()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignmentAsync(It.IsAny<string>(), It.IsAny<HousingAssignment>())).ThrowsAsync(new Exception());
                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignmentAsync_KeyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignmentAsync(It.IsAny<string>(), It.IsAny<HousingAssignment>())).ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);
            }

            [TestMethod]
            public async Task HousingAssignmentController_PutHousingAssignmentAsync()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuidAsync(guid, false)).ReturnsAsync(housingAssignment);
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignmentAsync(guid, It.IsAny<HousingAssignment>())).ReturnsAsync(housingAssignment);
                var result = await housingAssignmentController.PutHousingAssignmentAsync(guid, housingAssignment);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_IntegrationApiException_HousingAssignment_Guid_Null()
            {
                await housingAssignmentController.PutHousingAssignment2Async(null, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_IntegrationApiException_HousingAssignment_Null()
            {
                await housingAssignmentController.PutHousingAssignment2Async(guid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_IntegrationApiException_HousingAssignment_Id_Null()
            {
                housingAssignment2.Id = null;

                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_IntegrationApiException_HousingAssignment_Guid_Empty()
            {
                await housingAssignmentController.PutHousingAssignment2Async(Guid.Empty.ToString(), housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_IntegrationApiException_HousingAssignment_Id_And_Guid_NotMatched()
            {
                housingAssignment2.Id = Guid.Empty.ToString();

                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_ArgumentNullException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignment2Async(It.IsAny<string>(), It.IsAny<HousingAssignment2>())).ThrowsAsync(new ArgumentNullException());
                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_ArgumentException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignment2Async(It.IsAny<string>(), It.IsAny<HousingAssignment2>())).ThrowsAsync(new ArgumentException());
                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_InvalidOperationException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignment2Async(It.IsAny<string>(), It.IsAny<HousingAssignment2>())).ThrowsAsync(new InvalidOperationException());
                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_PermissionsException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignment2Async(It.IsAny<string>(), It.IsAny<HousingAssignment2>())).ThrowsAsync(new PermissionsException());
                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_Exception()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignment2Async(It.IsAny<string>(), It.IsAny<HousingAssignment2>())).ThrowsAsync(new Exception());
                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutHousingAssignment2Async_KeyNotFoundException()
            {
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignment2Async(It.IsAny<string>(), It.IsAny<HousingAssignment2>())).ThrowsAsync(new KeyNotFoundException());
                await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);
            }

            [TestMethod]
            public async Task HousingAssignmentController_PutHousingAssignment2Async()
            {
                housingAssignmentServiceMock.Setup(h => h.GetHousingAssignmentByGuid2Async(guid, false)).ReturnsAsync(housingAssignment2);
                housingAssignmentServiceMock.Setup(s => s.UpdateHousingAssignment2Async(guid, It.IsAny<HousingAssignment2>())).ReturnsAsync(housingAssignment2);
                var result = await housingAssignmentController.PutHousingAssignment2Async(guid, housingAssignment2);

                Assert.IsNotNull(result);
            }
        }
    }
}
