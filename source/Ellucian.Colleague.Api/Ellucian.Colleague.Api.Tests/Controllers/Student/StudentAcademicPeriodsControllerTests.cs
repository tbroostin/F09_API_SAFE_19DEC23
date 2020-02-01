//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAcademicPeriodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentAcademicPeriodsService> studentAcademicPeriodsServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicPeriodsController studentAcademicPeriodsController;
        private List<Domain.Student.Entities.StudentAcademicPeriod> allStudentAcadPeriodEntities;
        private List<Dtos.StudentAcademicPeriods> studentAcademicPeriodsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter personFilterFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
        private Paging page = new Paging(10, 0);
        private List<AcademicPeriod> allAcademicPeriods = null;
        private string acadPeriodGuid = string.Empty;
        private List<Domain.Student.Entities.Student> allStudents = null;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            studentAcademicPeriodsServiceMock = new Mock<IStudentAcademicPeriodsService>();
            loggerMock = new Mock<ILogger>();
            studentAcademicPeriodsCollection = new List<Dtos.StudentAcademicPeriods>();

            allAcademicPeriods = new TestAcademicPeriodRepository().Get().ToList();
            List<Domain.Student.Entities.Student> allStudents = (await new TestStudentRepository().GetAllAsync()).ToList(); ;
            List<AcademicLevel> allAcademicLevels = (await new TestAcademicLevelRepository().GetAsync()).ToList();
            var acadLevelGrad = allAcademicLevels.FirstOrDefault(a1 => a1.Code.Equals("GR"));
            var acadLevelUnderGrad = allAcademicLevels.FirstOrDefault(a2 => a2.Code.Equals("UG"));

            var acadPeriod = allAcademicPeriods.FirstOrDefault();
            acadPeriodGuid = acadPeriod.Guid;

            allStudentAcadPeriodEntities = new List<Domain.Student.Entities.StudentAcademicPeriod>();

            var student1 = allStudents.FirstOrDefault(s1 => s1.Id.Equals("00004001"));

            allStudentAcadPeriodEntities.Add(new StudentAcademicPeriod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", student1.Id,
                acadPeriod.Code)
            {
                StudentTerms = new List<StudentTerm>
                { new StudentTerm(Guid.NewGuid().ToString(), student1.Id, acadPeriod.Code, acadLevelGrad.Code) }
            });

            var student2 = allStudents.FirstOrDefault(s2 => s2.Id.Equals("00004002"));

            allStudentAcadPeriodEntities.Add(new StudentAcademicPeriod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", student2.Id,
                  acadPeriod.Code)
            {
                StudentTerms = new List<StudentTerm>
                {
                    new StudentTerm(Guid.NewGuid().ToString(), student2.Id, acadPeriod.Code, acadLevelGrad.Code) }
            
            });

            var student3 = allStudents.FirstOrDefault(s3 => s3.Id.Equals("00004003"));

            allStudentAcadPeriodEntities.Add(new StudentAcademicPeriod("d2253ac7-9931-4560-b42f-1fccd43c952e", student3.Id,
                acadPeriod.Code)
            {
                StudentTerms = new List<StudentTerm>
                { new StudentTerm(Guid.NewGuid().ToString(), student3.Id, acadPeriod.Code, acadLevelUnderGrad.Code) }
            });

            foreach (var source in allStudentAcadPeriodEntities)
            {
                var studentAcademicPeriod = new Ellucian.Colleague.Dtos.StudentAcademicPeriods
                {
                    Id = source.Guid,
                };
                var student = allStudents.FirstOrDefault(s => s.Id.Equals(source.StudentId));
                studentAcademicPeriod.Person = new GuidObject2(student.StudentGuid);
                studentAcademicPeriod.AcademicPeriod = new GuidObject2(acadPeriod.Guid);
                var studentAcademicPeriodAcademicLevels = new List<StudentAcademicPeriodsAcademicLevels>();
                foreach (var level in source.Term)
                {
                    var studentAcademicPeriodsAcademicLevel = new StudentAcademicPeriodsAcademicLevels();
                    var acadLevelGuid = string.Empty;
                    if (level.Equals(acadLevelGrad.Code))
                        acadLevelGuid = acadLevelGrad.Guid;
                    else if (level.Equals(acadLevelUnderGrad.Code))
                        acadLevelGuid = acadLevelUnderGrad.Guid;
                    studentAcademicPeriodsAcademicLevel.AcademicLevel = new GuidObject2(acadLevelGuid);
                    studentAcademicPeriodAcademicLevels.Add(studentAcademicPeriodsAcademicLevel);
                }
                studentAcademicPeriod.AcademicLevels = studentAcademicPeriodAcademicLevels;
                studentAcademicPeriodsCollection.Add(studentAcademicPeriod);
            }

            studentAcademicPeriodsController = new StudentAcademicPeriodsController(studentAcademicPeriodsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentAcademicPeriodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentAcademicPeriodsController = null;
            allStudentAcadPeriodEntities = null;
            studentAcademicPeriodsCollection = null;
            loggerMock = null;
            studentAcademicPeriodsServiceMock = null;
            allStudents = null;
    }

        [TestMethod]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriods_ValidateFields_Nocache()
        {
            studentAcademicPeriodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriods>, int> studentAcadPerdiodTuple
                    = new Tuple<IEnumerable<StudentAcademicPeriods>, int>(studentAcademicPeriodsCollection, studentAcademicPeriodsCollection.Count());

            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), 
                It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), false)).ReturnsAsync(studentAcadPerdiodTuple);

            var sourceContexts = (await studentAcademicPeriodsController.GetStudentAcademicPeriodsAsync(page, criteriaFilter, personFilterFilter));


            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriods>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentAcademicPeriods>;

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = studentAcademicPeriodsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id, "id");
                
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id, "academic period");
               // Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

       

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriods_KeyNotFoundException()
        {
            //
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), false))
                .Throws<KeyNotFoundException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsAsync(page, criteriaFilter, personFilterFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriods_PermissionsException()
        {

            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), false)).Throws<PermissionsException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsAsync(page, criteriaFilter, personFilterFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriods_ArgumentException()
        {

            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
               It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), false)).Throws<ArgumentException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsAsync(page, criteriaFilter, personFilterFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriods_RepositoryException()
        {

            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
               It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), false)).Throws<RepositoryException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsAsync(page, criteriaFilter, personFilterFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriods_IntegrationApiException()
        {

            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
               It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), false)).Throws<IntegrationApiException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsAsync(page, criteriaFilter, personFilterFilter);
        }

        [TestMethod]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuidAsync_ValidateFields()
        {
            var expected = studentAcademicPeriodsCollection.FirstOrDefault();
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriods_Exception()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(),
               It.IsAny<string>(), It.IsAny<StudentAcademicPeriods>(), false)).Throws<Exception>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsAsync(page, criteriaFilter, personFilterFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuidAsync_Exception()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuid_KeyNotFoundException()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuid_PermissionsException()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuid_ArgumentException()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuid_RepositoryException()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuid_IntegrationApiException()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_GetStudentAcademicPeriodsByGuid_Exception()
        {
            studentAcademicPeriodsServiceMock.Setup(x => x.GetStudentAcademicPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await studentAcademicPeriodsController.GetStudentAcademicPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_PostStudentAcademicPeriodsAsync_Exception()
        {
            await studentAcademicPeriodsController.PostStudentAcademicPeriodsAsync(studentAcademicPeriodsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_PutStudentAcademicPeriodsAsync_Exception()
        {
            var sourceContext = studentAcademicPeriodsCollection.FirstOrDefault();
            await studentAcademicPeriodsController.PutStudentAcademicPeriodsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodsController_DeleteStudentAcademicPeriodsAsync_Exception()
        {
            await studentAcademicPeriodsController.DeleteStudentAcademicPeriodsAsync(studentAcademicPeriodsCollection.FirstOrDefault().Id);
        }
    }
}