//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAcademicCredentialsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentAcademicCredentialsService> studentAcademicCredentialsServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicCredentialsController studentAcademicCredentialsController; 
        private List<Dtos.StudentAcademicCredentials> dtos;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentAcademicCredentialsServiceMock = new Mock<IStudentAcademicCredentialsService>();
            loggerMock = new Mock<ILogger>();

            BuildData();

            studentAcademicCredentialsController = new StudentAcademicCredentialsController(studentAcademicCredentialsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentAcademicCredentialsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        private void BuildData()
        {
            dtos = new List<Dtos.StudentAcademicCredentials>()
            {
                new Dtos.StudentAcademicCredentials()
                {
                    Id = expectedGuid,
                    AcademicLevel = new Dtos.GuidObject2("1218439e-a3b8-4e53-937f-757a574c8c4b"),
                    Credentials = new List<Dtos.DtoProperties.StudentAcademicCredentialsCredentials>()
                    {
                        new Dtos.DtoProperties.StudentAcademicCredentialsCredentials()
                        {
                             Credential = new Dtos.GuidObject2("b69e1aae-7035-4934-a860-2d7ab747dfb3"),
                             EarnedOn = DateTime.Today.AddDays(-30)
                        },
                        new Dtos.DtoProperties.StudentAcademicCredentialsCredentials()
                        {
                             Credential = new Dtos.GuidObject2("6eec5762-4304-4d59-ac1d-39bbb26cdaae"),
                             EarnedOn = DateTime.Today.AddDays(-60)
                        }
                    },
                    Disciplines = new List<Dtos.GuidObject2>()
                    {
                        new Dtos.GuidObject2("6cdaa318-e5b8-438a-adca-ccbf769dfe69"),
                        new Dtos.GuidObject2("c2aa740e-ee85-4263-86ca-bc7379ca645a")
                    },
                    GraduatedOn = DateTime.Today.AddDays(-5),
                    GraduationYear = DateTime.Today.AddDays(-5).Year.ToString(),
                    GraduationAcademicPeriod = new Dtos.GuidObject2("48b6668a-5878-4389-9466-0632952375a5"),
                    Recognitions = new List<Dtos.GuidObject2>()
                    {
                        new Dtos.GuidObject2("ecb87984-aee5-4f6a-a233-f51b28f00553"),
                        new Dtos.GuidObject2("36c81c7d-28f3-4c9b-b7dd-8123cc0f1368")
                    },
                    Student = new Dtos.GuidObject2("e0fad1fd-a4c5-441f-9fc4-6ec3dd84ab93"),
                    StudentProgram = new Dtos.GuidObject2("1bea9610-a58b-475a-94ea-2cbddb8939f7"),
                    ThesisTitle  = "Some title"
                },
                new Dtos.StudentAcademicCredentials()
                {
                    Id = "ef2e2518-0a2f-4d55-bb01-fa4b6fc16406",
                    AcademicLevel = new Dtos.GuidObject2("2218439e-a3b8-4e53-937f-757a574c8c4b"),
                    Credentials = new List<Dtos.DtoProperties.StudentAcademicCredentialsCredentials>()
                    {
                        new Dtos.DtoProperties.StudentAcademicCredentialsCredentials()
                        {
                             Credential = new Dtos.GuidObject2("c69e1aae-7035-4934-a860-2d7ab747dfb3"),
                             EarnedOn = DateTime.Today.AddDays(-30)
                        },
                        new Dtos.DtoProperties.StudentAcademicCredentialsCredentials()
                        {
                             Credential = new Dtos.GuidObject2("7eec5762-4304-4d59-ac1d-39bbb26cdaae"),
                             EarnedOn = DateTime.Today.AddDays(-60)
                        }
                    },
                    Disciplines = new List<Dtos.GuidObject2>()
                    {
                        new Dtos.GuidObject2("7cdaa318-e5b8-438a-adca-ccbf769dfe69"),
                        new Dtos.GuidObject2("d2aa740e-ee85-4263-86ca-bc7379ca645a")
                    },
                    GraduatedOn = DateTime.Today.AddDays(-5),
                    GraduationYear = DateTime.Today.AddDays(-5).Year.ToString(),
                    GraduationAcademicPeriod = new Dtos.GuidObject2("58b6668a-5878-4389-9466-0632952375a5"),
                    Recognitions = new List<Dtos.GuidObject2>()
                    {
                        new Dtos.GuidObject2("fcb87984-aee5-4f6a-a233-f51b28f00553"),
                        new Dtos.GuidObject2("46c81c7d-28f3-4c9b-b7dd-8123cc0f1368")
                    },
                    Student = new Dtos.GuidObject2("f0fad1fd-a4c5-441f-9fc4-6ec3dd84ab93"),
                    StudentProgram = new Dtos.GuidObject2("2bea9610-a58b-475a-94ea-2cbddb8939f7"),
                    ThesisTitle  = "Some title"
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentAcademicCredentialsController = null;
            dtos = null;
            loggerMock = null;
            studentAcademicCredentialsServiceMock = null;
        }

        [TestMethod]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_ValidateFields_Nocache()
        {
            studentAcademicCredentialsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            Tuple<IEnumerable<StudentAcademicCredentials>, int> tuple = new Tuple<IEnumerable<StudentAcademicCredentials>, int>(dtos, dtos.Count());
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(), 
                It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), 
                It.IsAny<QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentAcademicCredentials> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicCredentials>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.StudentAcademicCredentials>;
            Assert.AreEqual(dtos.Count, actuals.Count());
            for (var i = 0; i < actuals.Count(); i++)
            {
                var expected = dtos[i];
                var actual = actuals.ToList()[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.AcademicLevel.Id, actual.AcademicLevel.Id);
                Assert.AreEqual(expected.Credentials.Count(), actual.Credentials.Count());
                foreach (var actualCredential in actual.Credentials)
                {
                    var credExpected = expected.Credentials.FirstOrDefault(j => j.Credential.Id.Equals(actualCredential.Credential.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.AreEqual(credExpected.Credential.Id, actualCredential.Credential.Id);
                    Assert.AreEqual(credExpected.EarnedOn, actualCredential.EarnedOn);
                }
                Assert.AreEqual(expected.Disciplines.Count(), actual.Disciplines.Count());
                foreach (var actualDiscipline in actual.Disciplines)
                {
                    var disciplineExpected = expected.Disciplines.FirstOrDefault(j => j.Id.Equals(actualDiscipline.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.AreEqual(disciplineExpected.Id, actualDiscipline.Id);
                }
                Assert.AreEqual(expected.GraduatedOn, actual.GraduatedOn);
                Assert.AreEqual(expected.GraduationYear, actual.GraduationYear);
                Assert.AreEqual(expected.GraduationAcademicPeriod.Id, actual.GraduationAcademicPeriod.Id);
                Assert.AreEqual(expected.Recognitions.Count(), actual.Recognitions.Count());
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                Assert.AreEqual(expected.StudentProgram.Id, actual.StudentProgram.Id);
                Assert.AreEqual(expected.ThesisTitle, actual.ThesisTitle);
            }
        }

        [TestMethod]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_ValidateFields_EmptyFilterParameter()
        {
            studentAcademicCredentialsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentAcademicCredentialsController.Request.Properties.Add("EmptyFilterProperties", true);

            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                It.IsAny<QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentAcademicCredentials> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicCredentials>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.StudentAcademicCredentials>;
            Assert.AreEqual(actuals.Count(), 0);
            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_KeyNotFoundException()
        {
            //
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(),
                It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_PermissionsException()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(),
                            It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                            .ThrowsAsync(new PermissionsException());
            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_ArgumentException()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(),
                                        It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                                        .ThrowsAsync(new ArgumentException());
            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_RepositoryException()
        {

            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(),
                                        It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                                        .ThrowsAsync(new RepositoryException());
            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_IntegrationApiException()
        {

            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(),
                                        It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                                        .ThrowsAsync(new IntegrationApiException());
            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentials_Exception()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(),
                                        It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                                        .ThrowsAsync(new Exception());
            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuidAsync_Exception()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuid_KeyNotFoundException()
        {

            studentAcademicCredentialsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuid_PermissionsException()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuid_ArgumentException()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuid_RepositoryException()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuid_IntegrationApiException()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuid_Exception()
        {
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_PostStudentAcademicCredentialsAsync_Exception()
        {
            await studentAcademicCredentialsController.PostStudentAcademicCredentialsAsync(dtos.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_PutStudentAcademicCredentialsAsync_Exception()
        {
            var sourceContext = dtos.FirstOrDefault();
            await studentAcademicCredentialsController.PutStudentAcademicCredentialsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_DeleteStudentAcademicCredentialsAsync_Exception()
        {
            await studentAcademicCredentialsController.DeleteStudentAcademicCredentialsAsync(dtos.FirstOrDefault().Id);
        }

        //GET v1.0.0
        //Successful
        //GetStudentAcademicCredentialsAsync

        [TestMethod]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicCredentials" },
                    { "action", "GetStudentAcademicCredentialsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicCredentialsController.Request.SetRouteData(data);
            studentAcademicCredentialsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicCredentials);

            var controllerContext = studentAcademicCredentialsController.ControllerContext;
            var actionDescriptor = studentAcademicCredentialsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicCredentials>, int>(dtos, 5);

            studentAcademicCredentialsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())) .ReturnsAsync(tuple);
            var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            Object filterObject;
            studentAcademicCredentialsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicCredentials));

        }

        //GET v1.0.0
        //Exception
        //GetStudentAcademicCredentialsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicCredentials" },
                    { "action", "GetStudentAcademicCredentialsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicCredentialsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicCredentialsController.ControllerContext;
            var actionDescriptor = studentAcademicCredentialsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsAsync(0, 100, It.IsAny<Dtos.StudentAcademicCredentials>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<Dtos.Filters.AcademicProgramsFilter>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())) .ThrowsAsync(new PermissionsException());
                studentAcademicCredentialsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-credentials."));
                var sourceContexts = await studentAcademicCredentialsController.GetStudentAcademicCredentialsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET BY ID v1.0.0
        //Successful
        //GetStudentAcademicCredentialsByGuidAsync

        [TestMethod]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuidAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicCredentials" },
                    { "action", "GetStudentAcademicCredentialsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicCredentialsController.Request.SetRouteData(data);
            studentAcademicCredentialsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicCredentials);

            var controllerContext = studentAcademicCredentialsController.ControllerContext;
            var actionDescriptor = studentAcademicCredentialsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicCredentials>, int>(dtos, 4);

            studentAcademicCredentialsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            //studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>(); ;
            await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);

            Object filterObject;
            studentAcademicCredentialsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicCredentials));

        }

        //GET BY ID v1.0.0
        //Exception
        //GetStudentAcademicCredentialsByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicCredentialsController_GetStudentAcademicCredentialsByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicCredentials" },
                    { "action", "GetStudentAcademicCredentialsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicCredentialsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicCredentialsController.ControllerContext;
            var actionDescriptor = studentAcademicCredentialsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                studentAcademicCredentialsServiceMock.Setup(x => x.GetStudentAcademicCredentialsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())) .Throws<PermissionsException>(); 
                studentAcademicCredentialsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-credentials."));
                await studentAcademicCredentialsController.GetStudentAcademicCredentialsByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

    }
}