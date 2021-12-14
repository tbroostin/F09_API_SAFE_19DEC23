//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentMealPlansControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentMealPlansService> studentMealPlansServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentMealPlansController studentMealPlansController;
        private List<Dtos.StudentMealPlans> studentMealPlansDtoCollection;
        private List<Dtos.StudentMealPlans2> studentMealPlansDtoCollection2;
        Tuple<IEnumerable<Dtos.StudentMealPlans>, int> studentMealPlansDtoTuple;
        Tuple<IEnumerable<Dtos.StudentMealPlans2>, int> studentMealPlansDtoTuple2;
        //private List<Dtos.StudentMealPlans> StudentMealPlansDtoCollection;
        //private List<Dtos.StudentMealPlans2> StudentMealPlansDtoCollection2;
        //Tuple<IEnumerable<Dtos.StudentMealPlans>, int> StudentMealPlansDtoTuple;
        //Tuple<IEnumerable<Dtos.StudentMealPlans2>, int> StudentMealPlansDtoTuple2;
        int offset = 0;
        int limit = 100;
        private Paging page;
        string guid = "";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentMealPlansServiceMock = new Mock<IStudentMealPlansService>();
            loggerMock = new Mock<ILogger>();
            studentMealPlansDtoCollection = new List<Dtos.StudentMealPlans>();
            studentMealPlansDtoCollection2 = new List<Dtos.StudentMealPlans2>();

            BuildData();

            studentMealPlansController = new StudentMealPlansController(studentMealPlansServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentMealPlansController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        private void BuildData()
        {
            page = new Paging(limit, offset);
            guid = "699d2457-a030-4990-8e30-e469878d156f";
            studentMealPlansDtoCollection = new List<StudentMealPlans>() 
            {
                new StudentMealPlans()
                {   
                    Id = "699d2457-a030-4990-8e30-e469878d156f",
                    AcademicPeriod = new GuidObject2("49ea04ce-d26e-42af-909c-07deefab2c68"),
                    AssignedRate = new GuidObject2("e2052590-62d1-4da5-9f75-df2d1906a123"),
                    NumberOfPeriods = 5,
                    EndOn = DateTime.Today.AddDays(30),
                    MealPlan = new GuidObject2("10f4bf71-a6d6-444a-86d2-ed4a89af7791"),
                    Person = new GuidObject2("bde4f128-8fca-42a2-9881-1d276ee2269f"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.StudentMealPlansStatus.Assigned,
                    StatusDate = DateTime.Today,
                    MealCard = "xyz meal card",
                    Comment = "Owens Food Court",
                    Consumption = new StudentMealPlansConsumption()
                    {
                        Units = 2
                    }
                },
                new StudentMealPlans()
                {   
                    Id = "3e39426d-9a42-4da0-823f-bf8a54ad15d0",
                    AcademicPeriod = new GuidObject2("80fd416e-a26c-44b8-ba82-389bbebb3fb3"),
                    OverrideRate = new StudentMealPlansOverrideRateDtoProperty()
                    {
                        Rate = new Amount2DtoProperty()
                        {
                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                            Value = 2.0m
                        },
                        RatePeriod = Dtos.EnumProperties.MealPlanRatesRatePeriod.Week,                  
                        AccountingCode = new GuidObject2("d53ae40c-e929-4ce9-a121-727e31af3d93"),
                        OverrideReason  = new GuidObject2("170c42c5-4492-4fdc-a78a-bc9a0053c375")  
                    },
                    NumberOfPeriods = 5,
                    EndOn = DateTime.Today.AddDays(30),
                    MealPlan = new GuidObject2("10f4bf71-a6d6-444a-86d2-ed4a89af7791"),
                    Person = new GuidObject2("bde4f128-8fca-42a2-9881-1d276ee2269f"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.StudentMealPlansStatus.Assigned,
                    StatusDate = DateTime.Today,
                    MealCard = "abc meal card",
                    Comment = "Dietrick Dining Hall",
                    Consumption = new StudentMealPlansConsumption()
                    {
                        Percent = 50
                    }                    
                }
            };
            studentMealPlansDtoTuple = new Tuple<IEnumerable<StudentMealPlans>, int>(studentMealPlansDtoCollection, studentMealPlansDtoCollection.Count());

            studentMealPlansDtoCollection2 = new List<StudentMealPlans2>()
            {
                new StudentMealPlans2()
                {
                    Id = "699d2457-a030-4990-8e30-e469878d156f",
                    AcademicPeriod = new GuidObject2("49ea04ce-d26e-42af-909c-07deefab2c68"),
                    AssignedRates = new List<GuidObject2>()
                    {
                        new GuidObject2("e2052590-62d1-4da5-9f75-df2d1906a123")
                    },
                    NumberOfPeriods = 5,
                    EndOn = DateTime.Today.AddDays(30),
                    MealPlan = new GuidObject2("10f4bf71-a6d6-444a-86d2-ed4a89af7791"),
                    Person = new GuidObject2("bde4f128-8fca-42a2-9881-1d276ee2269f"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.StudentMealPlansStatus.Assigned,
                    StatusDate = DateTime.Today,
                    MealCard = "xyz meal card",
                    Comment = "Owens Food Court",
                    Consumption = new StudentMealPlansConsumption()
                    {
                        Units = 2
                    }
                },
                new StudentMealPlans2()
                {
                    Id = "3e39426d-9a42-4da0-823f-bf8a54ad15d0",
                    AcademicPeriod = new GuidObject2("80fd416e-a26c-44b8-ba82-389bbebb3fb3"),
                    OverrideRate = new StudentMealPlansOverrideRateDtoProperty()
                    {
                        Rate = new Amount2DtoProperty()
                        {
                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                            Value = 2.0m
                        },
                        RatePeriod = Dtos.EnumProperties.MealPlanRatesRatePeriod.Week,
                        AccountingCode = new GuidObject2("d53ae40c-e929-4ce9-a121-727e31af3d93"),
                        OverrideReason  = new GuidObject2("170c42c5-4492-4fdc-a78a-bc9a0053c375")
                    },
                    NumberOfPeriods = 5,
                    EndOn = DateTime.Today.AddDays(30),
                    MealPlan = new GuidObject2("10f4bf71-a6d6-444a-86d2-ed4a89af7791"),
                    Person = new GuidObject2("bde4f128-8fca-42a2-9881-1d276ee2269f"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.StudentMealPlansStatus.Assigned,
                    StatusDate = DateTime.Today,
                    MealCard = "abc meal card",
                    Comment = "Dietrick Dining Hall",
                    Consumption = new StudentMealPlansConsumption()
                    {
                        Percent = 50
                    }
                }
            };
            studentMealPlansDtoTuple2 = new Tuple<IEnumerable<StudentMealPlans2>, int>(studentMealPlansDtoCollection2, studentMealPlansDtoCollection2.Count());           
            studentMealPlansDtoTuple = new Tuple<IEnumerable<StudentMealPlans>, int>(studentMealPlansDtoCollection, studentMealPlansDtoCollection.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentMealPlansController = null;
            studentMealPlansDtoCollection = null;
            loggerMock = null;
            studentMealPlansServiceMock = null;
        }

        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlans_ValidateFields_Nocache()
        {
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentMealPlansController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).ReturnsAsync(studentMealPlansDtoTuple);

            var sourceContexts = await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            List<Dtos.StudentMealPlans> StudentMealPlansAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>>)httpResponseMessage.Content).Value as List<Dtos.StudentMealPlans>;

            Assert.AreEqual(studentMealPlansDtoCollection.Count, StudentMealPlansAssessments.Count);
            for (var i = 0; i < StudentMealPlansAssessments.Count; i++)
            {
                var expected = studentMealPlansDtoCollection[i];
                var actual = StudentMealPlansAssessments[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.NumberOfPeriods, actual.NumberOfPeriods);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StatusDate, actual.StatusDate);
                Assert.AreEqual(expected.MealCard, actual.MealCard);
                Assert.AreEqual(expected.Comment, actual.Comment);
                if (expected.Consumption != null)
                {
                    Assert.AreEqual(expected.Consumption.Units, actual.Consumption.Units);
                    Assert.AreEqual(expected.Consumption.Percent, actual.Consumption.Percent);
                }
                if (expected.OverrideRate != null)
                {
                    Assert.AreEqual(expected.OverrideRate.Rate.Value, actual.OverrideRate.Rate.Value);
                    Assert.AreEqual(expected.OverrideRate.Rate.Currency, actual.OverrideRate.Rate.Currency);
                    Assert.AreEqual(expected.OverrideRate.RatePeriod, actual.OverrideRate.RatePeriod);
                    if (expected.OverrideRate.AccountingCode != null)
                    {
                        Assert.AreEqual(expected.OverrideRate.AccountingCode.Id, actual.OverrideRate.AccountingCode.Id);
                    }
                    if (expected.OverrideRate.OverrideReason != null)
                    {
                        Assert.AreEqual(expected.OverrideRate.OverrideReason.Id, actual.OverrideRate.OverrideReason.Id);
                    }
                }
            }
        }

        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlans_ValidateFields_PageNull()
        {
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentMealPlansController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).ReturnsAsync(studentMealPlansDtoTuple);
            page = null;
            var sourceContexts = await studentMealPlansController.GetStudentMealPlansAsync(page,criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            List<Dtos.StudentMealPlans> StudentMealPlansAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>>)httpResponseMessage.Content).Value as List<Dtos.StudentMealPlans>;

            Assert.AreEqual(studentMealPlansDtoCollection.Count, StudentMealPlansAssessments.Count);
            for (var i = 0; i < StudentMealPlansAssessments.Count; i++)
            {
                var expected = studentMealPlansDtoCollection[i];
                var actual = StudentMealPlansAssessments[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_ValidateFields()
        {
            var expected = studentMealPlansDtoCollection.FirstOrDefault();
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await studentMealPlansController.GetStudentMealPlansByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_Exception()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).Throws<Exception>();
            await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_KeyNotFoundException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_PermissionsException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_ArgumentException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_RepositoryException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_IntegrationApiException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_Empty_Null_Guid()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await studentMealPlansController.GetStudentMealPlansByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_KeyNotFoundException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await studentMealPlansController.GetStudentMealPlansByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_PermissionsException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await studentMealPlansController.GetStudentMealPlansByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_ArgumentException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await studentMealPlansController.GetStudentMealPlansByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_RepositoryException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await studentMealPlansController.GetStudentMealPlansByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_IntegrationApiException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await studentMealPlansController.GetStudentMealPlansByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_Exception()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await studentMealPlansController.GetStudentMealPlansByGuidAsync("1");
        }

        #region 16.0.0

        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlans2_ValidateFields_Nocache()
        {
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentMealPlansController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).ReturnsAsync(studentMealPlansDtoTuple2);

            var sourceContexts = await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            List<Dtos.StudentMealPlans2> StudentMealPlansAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>>)httpResponseMessage.Content).Value as List<Dtos.StudentMealPlans2>;

            Assert.AreEqual(studentMealPlansDtoCollection.Count, StudentMealPlansAssessments.Count);
            for (var i = 0; i < StudentMealPlansAssessments.Count; i++)
            {
                var expected = studentMealPlansDtoCollection[i];
                var actual = StudentMealPlansAssessments[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.NumberOfPeriods, actual.NumberOfPeriods);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StatusDate, actual.StatusDate);
                Assert.AreEqual(expected.MealCard, actual.MealCard);
                Assert.AreEqual(expected.Comment, actual.Comment);
                if (expected.Consumption != null)
                {
                    Assert.AreEqual(expected.Consumption.Units, actual.Consumption.Units);
                    Assert.AreEqual(expected.Consumption.Percent, actual.Consumption.Percent);
                }
                if (expected.OverrideRate != null)
                {
                    Assert.AreEqual(expected.OverrideRate.Rate.Value, actual.OverrideRate.Rate.Value);
                    Assert.AreEqual(expected.OverrideRate.Rate.Currency, actual.OverrideRate.Rate.Currency);
                    Assert.AreEqual(expected.OverrideRate.RatePeriod, actual.OverrideRate.RatePeriod);
                    if (expected.OverrideRate.AccountingCode != null)
                    {
                        Assert.AreEqual(expected.OverrideRate.AccountingCode.Id, actual.OverrideRate.AccountingCode.Id);
                    }
                    if (expected.OverrideRate.OverrideReason != null)
                    {
                        Assert.AreEqual(expected.OverrideRate.OverrideReason.Id, actual.OverrideRate.OverrideReason.Id);
                    }
                }
            }
        }

        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlans2_ValidateFields_PageNull()
        {
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentMealPlansController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).ReturnsAsync(
                studentMealPlansDtoTuple2);
            page = null;
            var sourceContexts = await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            List<Dtos.StudentMealPlans2> StudentMealPlansAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>>)httpResponseMessage.Content).Value as List<Dtos.StudentMealPlans2>;

            Assert.AreEqual(studentMealPlansDtoCollection.Count, StudentMealPlansAssessments.Count);
            for (var i = 0; i < StudentMealPlansAssessments.Count; i++)
            {
                var expected = studentMealPlansDtoCollection[i];
                var actual = StudentMealPlansAssessments[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_ValidateFields()
        {
            var expected = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(expected.Id)).ReturnsAsync(expected);

            var actual = await studentMealPlansController.GetStudentMealPlansByGuid2Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans2_Exception()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).Throws<Exception>();
            await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans2_KeyNotFoundException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans2_PermissionsException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans2_ArgumentException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans2_RepositoryException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans2_IntegrationApiException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_Empty_Null_Guid()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<Exception>();
            await studentMealPlansController.GetStudentMealPlansByGuid2Async(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_KeyNotFoundException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await studentMealPlansController.GetStudentMealPlansByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync2_PermissionsException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<PermissionsException>();
            await studentMealPlansController.GetStudentMealPlansByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_ArgumentException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<ArgumentException>();
            await studentMealPlansController.GetStudentMealPlansByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_RepositoryException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<RepositoryException>();
            await studentMealPlansController.GetStudentMealPlansByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_IntegrationApiException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<IntegrationApiException>();
            await studentMealPlansController.GetStudentMealPlansByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_Exception()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<Exception>();
            await studentMealPlansController.GetStudentMealPlansByGuid2Async("1");
        }

        [TestMethod]
        public async Task PostStudentMealPlans2Async()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>())).ReturnsAsync(mp);
            var result = await studentMealPlansController.PostStudentMealPlans2Async(mp);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostStudentMealPlans2Async_HttpResponseException()
        {
            var result = await studentMealPlansController.PostStudentMealPlans2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostStudentMealPlans2Async_KeyNotFoundException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new KeyNotFoundException());
            var result = await studentMealPlansController.PostStudentMealPlans2Async(mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostStudentMealPlans2Async_PermissionsException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new PermissionsException());
            await studentMealPlansController.PostStudentMealPlans2Async(mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostStudentMealPlans2Async_ArgumentException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new ArgumentException());
            await studentMealPlansController.PostStudentMealPlans2Async(mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task PostStudentMealPlans2Async_RepositoryException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new RepositoryException());
            await studentMealPlansController.PostStudentMealPlans2Async(mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task PostStudentMealPlans2Async_IntegrationApiException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new IntegrationApiException());
            var result = await studentMealPlansController.PostStudentMealPlans2Async(mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task PostStudentMealPlans2Async_Exception()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new Exception());
            var result = await studentMealPlansController.PostStudentMealPlans2Async(mp);
        }

        [TestMethod]
        public async Task PutStudentMealPlans2Async()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>())).ReturnsAsync(mp);
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutStudentMealPlans2Async_HttpResponseException_ReqBodyNull()
        {
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutStudentMealPlans2Async_HttpResponseException_GuidNull()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            var result = await studentMealPlansController.PutStudentMealPlans2Async("", mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutStudentMealPlans2Async_KeyNotFoundException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new KeyNotFoundException());
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutStudentMealPlans2Async_PermissionsException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new PermissionsException());
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutStudentMealPlans2Async_ArgumentException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new ArgumentException());
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task PutStudentMealPlans2Async_RepositoryException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new RepositoryException());
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task PutStudentMealPlans2Async_IntegrationApiException()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new IntegrationApiException());
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task PutStudentMealPlans2Async_Exception()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>()))
                .ThrowsAsync(new Exception());
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
        }

        //GET by id v16.0.0
        //Successful
        //GetStudentMealPlansByGuid2Async
        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_Permissions()
        {
            var expected = studentMealPlansDtoCollection2.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlansByGuid2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment });

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));


            studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(expected.Id)).ReturnsAsync(expected);
            var actual = await studentMealPlansController.GetStudentMealPlansByGuid2Async(expected.Id);

            Object filterObject;
            studentMealPlansController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewMealPlanAssignment));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateMealPlanAssignment));


        }

        //GET by id v16.0.0
        //Exception
        //GetStudentMealPlansByGuid2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuid2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlansByGuid2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuid2Async(It.IsAny<string>())).Throws<PermissionsException>();
                studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-meal-plans."));
                await studentMealPlansController.GetStudentMealPlansByGuid2Async("1");
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET by id v10.1.0 / v10
        //Successful
        //GetStudentMealPlansByGuidAsync
        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_Permissions()
        {
            var expected = studentMealPlansDtoCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlansByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment });

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(expected.Id)).ReturnsAsync(expected);
            var actual = await studentMealPlansController.GetStudentMealPlansByGuidAsync(expected.Id);

            Object filterObject;
            studentMealPlansController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewMealPlanAssignment));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateMealPlanAssignment));


        }

        //GET by id v10.1.0 / v10
        //Exception
        //GetStudentMealPlansByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlansByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-meal-plans."));
                await studentMealPlansController.GetStudentMealPlansByGuidAsync("1");
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET v16.0.0
        //Successful
        //GetStudentMealPlans2Async
        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlans2Async_Permissions()
        {
            var expected = studentMealPlansDtoCollection2.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlans2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment });

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).ReturnsAsync(studentMealPlansDtoTuple2);
            var sourceContexts = await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);

            Object filterObject;
            studentMealPlansController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewMealPlanAssignment));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateMealPlanAssignment));


        }

        //GET v16.0.0
        //Exception
        //GetStudentMealPlans2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlans2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlans2Async(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>())).Throws<PermissionsException>();
                studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-meal-plans."));
                await studentMealPlansController.GetStudentMealPlans2Async(page, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET v10.1.0 / v10
        //Successful
        //GetStudentMealPlansAsync
        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlansAsync_Permissions()
        {
            var expected = studentMealPlansDtoCollection2.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlansAsync" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment });

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).ReturnsAsync(studentMealPlansDtoTuple);
            var sourceContexts = await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);

            Object filterObject;
            studentMealPlansController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewMealPlanAssignment));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateMealPlanAssignment));


        }

        //GET v10.1.0 / v10
        //Exception
        //GetStudentMealPlansAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlansAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "GetStudentMealPlansAsync" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<Dtos.StudentMealPlans>(), It.IsAny<bool>())).Throws<PermissionsException>();
                studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-meal-plans."));
                await studentMealPlansController.GetStudentMealPlansAsync(page, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //PUT v16.0.0
        //Successful
        //PutStudentMealPlans2Async
        [TestMethod]
        public async Task StudentMealPlansController_PutStudentMealPlans2Async_Permissions()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "PutStudentMealPlans2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment);

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>())).ReturnsAsync(mp);
            var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);

            Object filterObject;
            studentMealPlansController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateMealPlanAssignment));


        }

        //PUT v16.0.0
        //Exception
        //PutStudentMealPlans2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_PutStudentMealPlans2Async_Invalid_Permissions()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "PutStudentMealPlans2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentMealPlansServiceMock.Setup(ser => ser.PutStudentMealPlans2Async(It.IsAny<string>(), It.IsAny<Dtos.StudentMealPlans2>())).ThrowsAsync(new PermissionsException());
                studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update student-meal-plans."));
                var result = await studentMealPlansController.PutStudentMealPlans2Async(guid, mp);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //POST v16.0.0
        //Successful
        //PostStudentMealPlans2Async
        [TestMethod]
        public async Task StudentMealPlansController_PostStudentMealPlans2Async_Permissions()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "PostStudentMealPlans2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);
            studentMealPlansController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment);

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>())).ReturnsAsync(mp);
            var result = await studentMealPlansController.PostStudentMealPlans2Async(mp);

            Object filterObject;
            studentMealPlansController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateMealPlanAssignment));


        }

        //PUT v16.0.0
        //Exception
        //PostStudentMealPlans2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_PostStudentMealPlans2Async_Invalid_Permissions()
        {
            var mp = studentMealPlansDtoCollection2.FirstOrDefault();
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentMealPlans" },
                    { "action", "PostStudentMealPlans2Async" }
                };
            HttpRoute route = new HttpRoute("student-meal-plans", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentMealPlansController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentMealPlansController.ControllerContext;
            var actionDescriptor = studentMealPlansController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentMealPlansServiceMock.Setup(ser => ser.PostStudentMealPlans2Async(It.IsAny<Dtos.StudentMealPlans2>())).ThrowsAsync(new PermissionsException());
                studentMealPlansServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create student-meal-plans."));
                await studentMealPlansController.PostStudentMealPlans2Async(mp);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}