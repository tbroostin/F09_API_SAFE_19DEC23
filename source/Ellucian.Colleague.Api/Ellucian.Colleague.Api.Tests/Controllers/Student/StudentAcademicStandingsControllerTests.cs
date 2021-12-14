//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
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
using Ellucian.Colleague.Coordination.Student;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
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
    public class StudentAcademicStandingsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentAcademicStandingsService> studentAcademicStandingsServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicStandingsController studentAcademicStandingsController;
        private IEnumerable<StudentStanding> allStudentStandings;
        private List<Dtos.StudentAcademicStandings> studentAcademicStandingsCollection;

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicProgram> allAcademicPrograms;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> allAcademicLevels;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding2> allAcademicStandings;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentAcademicStandingsServiceMock = new Mock<IStudentAcademicStandingsService>();
            loggerMock = new Mock<ILogger>();
            studentAcademicStandingsCollection = new List<Dtos.StudentAcademicStandings>();

            allAcademicPrograms = await new TestAcademicProgramRepository().GetAsync();
            allAcademicLevels = await new TestAcademicLevelRepository().GetAsync();
            allAcademicStandings = await new TestStudentReferenceDataRepository().GetAcademicStandings2Async(false);
              

            allStudentStandings = new List<StudentStanding>()
                {
                    new StudentStanding("1", "0001585", "PROB", DateTime.Now)
                    {
                        Guid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                        Program = "BA-MATH",
                        Level = "UG",
                        CalcStandingCode = "PROB",
                        Type = StudentStandingType.AcademicLevel
                    },
                     new StudentStanding("2", "0001585", "PROB", DateTime.Now)
                    {
                        Guid = "502b4820-2c20-4066-b31f-5fcb420eb3f8",
                        Program = "AA-NURS",
                        Level = "GR",
                        CalcStandingCode = "PROB",
                        Type = StudentStandingType.Program
                    },
                     new StudentStanding("3", "0001585", "PROB", DateTime.Now)
                    {
                        Guid = "c46a1225-8b6c-46cf-a119-78e1a54c603d",
                        Program = "BA-MATH",
                        Level = "UG",
                        CalcStandingCode = "PROB",
                        Type = StudentStandingType.Term,
                        OverrideReason = "Student is in good standing."
                    }
                    
                };

            foreach (var source in allStudentStandings)
            {
                var studentAcademicStandings = new Ellucian.Colleague.Dtos.StudentAcademicStandings
                {
                    Id = source.Guid,
                    OverrideReason = source.OverrideReason
                    
                };
                if (!(string.IsNullOrEmpty(source.Program)))
                {
                    var studentAcademicStandingsProgram = allAcademicPrograms.FirstOrDefault(ap => ap.Code == source.Program);
                    if (studentAcademicStandingsProgram != null)
                    {
                        studentAcademicStandings.Program = new GuidObject2(studentAcademicStandingsProgram.Guid);
                    }
                }
                if (!(string.IsNullOrEmpty(source.Level)))
                {
                    var studentAcademicStandingsLevel = allAcademicLevels.FirstOrDefault(al => al.Code == source.Level);
                    if (studentAcademicStandingsLevel != null)
                    {
                        studentAcademicStandings.Level = new GuidObject2(studentAcademicStandingsLevel.Guid);
                    }
                }

                if (!(string.IsNullOrEmpty(source.StandingCode)))
                {
                    var studentAcademicStandingsStandings = allAcademicStandings.FirstOrDefault(ast => ast.Code == source.StandingCode);
                    if (studentAcademicStandingsStandings != null)
                    {
                        studentAcademicStandings.Standing = new GuidObject2(studentAcademicStandingsStandings.Guid);
                    }
                }

                if (!(string.IsNullOrEmpty(source.CalcStandingCode)))
                {
                    var studentAcademicStandingsStandings = allAcademicStandings.FirstOrDefault(ast => ast.Code == source.CalcStandingCode);
                    if (studentAcademicStandingsStandings != null)
                    {
                        studentAcademicStandings.OverrideStanding = new GuidObject2(studentAcademicStandingsStandings.Guid);
                    }
                }

                switch (source.Type)
                {
                    case StudentStandingType.AcademicLevel:
                        studentAcademicStandings.Type = StudentAcademicStandingsType.Level;
                        break;
                    case StudentStandingType.Program:
                        studentAcademicStandings.Type = StudentAcademicStandingsType.Program;
                        break;
                    case StudentStandingType.Term:
                        studentAcademicStandings.Type = StudentAcademicStandingsType.Academicperiod;
                        break;
                    default:
                        studentAcademicStandings.Type = StudentAcademicStandingsType.NotSet;
                        break;
                }

               
                studentAcademicStandingsCollection.Add(studentAcademicStandings);
            }

            studentAcademicStandingsController = new StudentAcademicStandingsController(studentAcademicStandingsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentAcademicStandingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            studentAcademicStandingsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentAcademicStandingsController = null;
            allStudentStandings = null;
            studentAcademicStandingsCollection = null;
            loggerMock = null;
            studentAcademicStandingsServiceMock = null;
        }


        [TestMethod]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandings()
        {
            studentAcademicStandingsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var studentAcademicStandingsTuple
                    = new Tuple<IEnumerable<StudentAcademicStandings>, int>(studentAcademicStandingsCollection, studentAcademicStandingsCollection.Count);

            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicStandingsTuple);

            Paging paging = new Paging(studentAcademicStandingsCollection.Count(), 0);
            var studentAcademicStandings = (await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await studentAcademicStandings.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.StudentAcademicStandings>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAcademicStandings>;

            Assert.IsNotNull(results);
            Assert.AreEqual(studentAcademicStandingsCollection.Count, results.Count());
            foreach (var actual in results)
            {
                var expected = studentAcademicStandingsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.Program != null)
                    Assert.AreEqual(expected.Program.Id, actual.Program.Id);
                if (expected.Level != null)
                    Assert.AreEqual(expected.Level.Id, actual.Level.Id);
                if (expected.Standing != null)
                    Assert.AreEqual(expected.Standing.Id, actual.Standing.Id);
                if (expected.OverrideStanding != null)
                    Assert.AreEqual(expected.OverrideStanding.Id, actual.OverrideStanding.Id);
               
                Assert.AreEqual(expected.OverrideReason, actual.OverrideReason);
                
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingAsync_Exception()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10,0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingAsync_KeyNotFoundException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingAsync_PermissionsException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingAsync_ArgumentException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingAsync_RepositoryException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingAsync_IntegrationApiException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10, 0));
        }

        [TestMethod]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuid()
        {
            var guid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d";

            studentAcademicStandingsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = false};

            var expected = studentAcademicStandingsCollection.FirstOrDefault(x => x.Id == guid);
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(expected);

            var actual = (await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(guid));

            Assert.IsNotNull(actual);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            if (expected.Program != null)
                Assert.AreEqual(expected.Program.Id, actual.Program.Id);
            if (expected.Level != null)
                Assert.AreEqual(expected.Level.Id, actual.Level.Id);
            if (expected.Standing != null)
                Assert.AreEqual(expected.Standing.Id, actual.Standing.Id);
            if (expected.OverrideStanding != null)
                Assert.AreEqual(expected.OverrideStanding.Id, actual.OverrideStanding.Id);

            Assert.AreEqual(expected.OverrideReason, actual.OverrideReason);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuidAsync_EmptyGuid()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuidAsync_Exception()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(new Guid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuidAsync_KeyNotFoundException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(new Guid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuidAsync_PermissionsException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(new Guid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuidAsync_ArgumentException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(new Guid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuidAsync_RepositoryException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(new Guid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_GetStudentAcademicStandingsByGuidAsync_IntegrationApiException()
        {
            studentAcademicStandingsServiceMock.Setup(x => x.GetStudentAcademicStandingsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await studentAcademicStandingsController.GetStudentAcademicStandingsByGuidAsync(new Guid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_PostStudentAcademicStandingsAsync_Exception()
        {
            await studentAcademicStandingsController.PostStudentAcademicStandingsAsync(studentAcademicStandingsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_PutStudentAcademicStandingsAsync_Exception()
        {
            var sourceContext = studentAcademicStandingsCollection.FirstOrDefault();
            await studentAcademicStandingsController.PutStudentAcademicStandingsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicStandingsController_DeleteStudentAcademicStandingsAsync_Exception()
        {
            await studentAcademicStandingsController.DeleteStudentAcademicStandingsAsync(studentAcademicStandingsCollection.FirstOrDefault().Id);
        }

        [TestMethod]
        public async Task studentAcademicStandingsController_GetStudentAcademicStandingsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentAcademicStandings" },
                { "action", "GetStudentAcademicStandingsAsync" }
            };
            HttpRoute route = new HttpRoute("student-academic-standings", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicStandingsController.Request.SetRouteData(data);
            studentAcademicStandingsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcadStandings });

            var controllerContext = studentAcademicStandingsController.ControllerContext;
            var actionDescriptor = studentAcademicStandingsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicStandings>, int>(studentAcademicStandingsCollection, 5);
            studentAcademicStandingsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                   .Returns(true);
            studentAcademicStandingsServiceMock.Setup(s => s.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var resp = await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10, 0));

            Object filterObject;
            studentAcademicStandingsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable<object>)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcadStandings));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task studentAcademicStandingsController_GetPersonsActiveHoldsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentAcademicStandings" },
                { "action", "GetStudentAcademicStandingsAsync" }
            };
            HttpRoute route = new HttpRoute("student-academic-standings", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicStandingsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicStandingsController.ControllerContext;
            var actionDescriptor = studentAcademicStandingsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicStandings>, int>(studentAcademicStandingsCollection, 5);

                studentAcademicStandingsServiceMock.Setup(s => s.GetStudentAcademicStandingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                studentAcademicStandingsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User is not authorized to view student-academic-standings."));
                var resp = await studentAcademicStandingsController.GetStudentAcademicStandingsAsync(new Paging(10, 0));
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }
    }
}