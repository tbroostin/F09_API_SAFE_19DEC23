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
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;
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
        Tuple<IEnumerable<Dtos.StudentMealPlans>, int> studentMealPlansDtoTuple;
        private List<Dtos.StudentMealPlans> StudentMealPlansDtoCollection;
        Tuple<IEnumerable<Dtos.StudentMealPlans>, int> StudentMealPlansDtoTuple;
        int offset = 0;
        int limit = 100;
        private Paging page;


        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentMealPlansServiceMock = new Mock<IStudentMealPlansService>();
            loggerMock = new Mock<ILogger>();
            studentMealPlansDtoCollection = new List<Dtos.StudentMealPlans>();

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

            page = new Paging(limit, offset);
            StudentMealPlansDtoCollection = new List<StudentMealPlans>() 
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
                    AssignedRate = new GuidObject2("e2052590-62d1-4da5-9f75-df2d1906a123"),
                    NumberOfPeriods = 5,
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
            StudentMealPlansDtoTuple = new Tuple<IEnumerable<StudentMealPlans>, int>(StudentMealPlansDtoCollection, StudentMealPlansDtoCollection.Count());
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

            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).ReturnsAsync(StudentMealPlansDtoTuple);

            var sourceContexts = await studentMealPlansController.GetStudentMealPlansAsync(page);
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

            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).ReturnsAsync(StudentMealPlansDtoTuple);
            page = null;
            var sourceContexts = await studentMealPlansController.GetStudentMealPlansAsync(page);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            List<Dtos.StudentMealPlans> StudentMealPlansAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>>)httpResponseMessage.Content).Value as List<Dtos.StudentMealPlans>;

            Assert.AreEqual(StudentMealPlansDtoCollection.Count, StudentMealPlansAssessments.Count);
            for (var i = 0; i < StudentMealPlansAssessments.Count; i++)
            {
                var expected = StudentMealPlansDtoCollection[i];
                var actual = StudentMealPlansAssessments[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentMealPlansController_GetStudentMealPlansByGuidAsync_ValidateFields()
        {
            var expected = StudentMealPlansDtoCollection.FirstOrDefault();
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await studentMealPlansController.GetStudentMealPlansByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_Exception()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).Throws<Exception>();
            await studentMealPlansController.GetStudentMealPlansAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_KeyNotFoundException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_PermissionsException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).Throws<PermissionsException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_ArgumentException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).Throws<ArgumentException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_RepositoryException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).Throws<RepositoryException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentMealPlansController_GetStudentMealPlans_IntegrationApiException()
        {
            studentMealPlansServiceMock.Setup(x => x.GetStudentMealPlansAsync(page.Offset, page.Limit, It.IsAny<bool>())).Throws<IntegrationApiException>();
            await studentMealPlansController.GetStudentMealPlansAsync(page);
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
    }
}