// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
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
    public class StudentAptitudeAssessmentsControllerTests
    {
        [TestClass]
        public class Get
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private Mock<IStudentAptitudeAssessmentsService> StudentAptitudeAssessmentsServiceMock;
            private StudentAptitudeAssessmentsController StudentAptitudeAssessmentsController;
            private IStudentAptitudeAssessmentsService StudentAptitudeAssessmentsService;
            private IEnumerable<StudentAptitudeAssessments> studentAptAssesmentsDtos;
            private ILogger logger = new Mock<ILogger>().Object;
            private Paging page;
            private int limit;
            private int offset;
            private Tuple<IEnumerable<StudentAptitudeAssessments>, int> stuAptAssessmentsDtosTuple;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                StudentAptitudeAssessmentsServiceMock = new Mock<IStudentAptitudeAssessmentsService>();

                StudentAptitudeAssessmentsService = StudentAptitudeAssessmentsServiceMock.Object;
                studentAptAssesmentsDtos = StudentAptitudeAssessmentsControllerTests.BuildStudentAptitudeAssessments();
                string guid = studentAptAssesmentsDtos.ElementAt(0).Id;

                StudentAptitudeAssessmentsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                StudentAptitudeAssessmentsController = new StudentAptitudeAssessmentsController(StudentAptitudeAssessmentsService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                StudentAptitudeAssessmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                limit = 200;
                offset = 0;
                page = new Paging(limit, offset);
                stuAptAssessmentsDtosTuple = new Tuple<IEnumerable<StudentAptitudeAssessments>, int>(studentAptAssesmentsDtos, 3);
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAptitudeAssessmentsServiceMock = null;
                StudentAptitudeAssessmentsService = null;
                StudentAptitudeAssessmentsController = null;
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessmentsByIdAsync()
            {
                string guid = studentAptAssesmentsDtos.ElementAt(0).Id;
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsByGuidAsync(guid, It.IsAny<bool>())).ReturnsAsync(studentAptAssesmentsDtos.ElementAt(0));
                var StudentAptitudeAssessments = await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuidAsync(guid);
                var expected = StudentAptitudeAssessments;
                var actual = studentAptAssesmentsDtos.ElementAt(0);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                Assert.AreEqual(expected.Assessment, actual.Assessment);
                Assert.AreEqual(expected.Form, actual.Form);
                Assert.AreEqual(expected.Percentile, actual.Percentile);
                Assert.AreEqual(expected.Preference, actual.Preference);
                Assert.AreEqual(expected.Reported, actual.Reported);
                Assert.AreEqual(expected.Score, actual.Score);
                Assert.AreEqual(expected.Source, actual.Source);
                Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.Student, actual.Student);
                Assert.AreEqual(expected.Update, actual.Update);
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessmentsByAsyncCache()
            {
                StudentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                StudentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsAsync(offset, limit, It.IsAny<bool>())).ReturnsAsync(stuAptAssessmentsDtosTuple);
                var acadProg = await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await acadProg.ExecuteAsync(cancelToken);
                List<Dtos.StudentAptitudeAssessments> StudentAptitudeAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>>)httpResponseMessage.Content).Value as List<Dtos.StudentAptitudeAssessments>;
                for (var i = 0; i < StudentAptitudeAssessments.Count; i++)
                {
                    var expected = studentAptAssesmentsDtos.ToList()[i];
                    var actual = StudentAptitudeAssessments[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                    Assert.AreEqual(expected.Assessment, actual.Assessment);
                    Assert.AreEqual(expected.Form, actual.Form);
                    Assert.AreEqual(expected.Percentile, actual.Percentile);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Reported, actual.Reported);
                    Assert.AreEqual(expected.Score, actual.Score);
                    Assert.AreEqual(expected.Source, actual.Source);
                    Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Update, actual.Update);
                }
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessmentssByAsyncNoCache()
            {
                StudentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                StudentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(stuAptAssessmentsDtosTuple);
                var HttpAction = (await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page));
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await HttpAction.ExecuteAsync(cancelToken);
                List<Dtos.StudentAptitudeAssessments> StudentAptitudeAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>>)httpResponseMessage.Content).Value as List<Dtos.StudentAptitudeAssessments>;
                for (var i = 0; i < StudentAptitudeAssessments.Count; i++)
                {
                    var expected = studentAptAssesmentsDtos.ToList()[i];
                    var actual = StudentAptitudeAssessments[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                    Assert.AreEqual(expected.Assessment, actual.Assessment);
                    Assert.AreEqual(expected.Form, actual.Form);
                    Assert.AreEqual(expected.Percentile, actual.Percentile);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Reported, actual.Reported);
                    Assert.AreEqual(expected.Score, actual.Score);
                    Assert.AreEqual(expected.Source, actual.Source);
                    Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Update, actual.Update);
                }
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessmentsByAsyncNoPaging()
            {
                StudentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                StudentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(stuAptAssessmentsDtosTuple);
                var HttpAction = (await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(null));
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await HttpAction.ExecuteAsync(cancelToken);
                List<Dtos.StudentAptitudeAssessments> StudentAptitudeAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>>)httpResponseMessage.Content).Value as List<Dtos.StudentAptitudeAssessments>;
                for (var i = 0; i < StudentAptitudeAssessments.Count; i++)
                {
                    var expected = studentAptAssesmentsDtos.ToList()[i];
                    var actual = StudentAptitudeAssessments[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                    Assert.AreEqual(expected.Assessment, actual.Assessment);
                    Assert.AreEqual(expected.Form, actual.Form);
                    Assert.AreEqual(expected.Percentile, actual.Percentile);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Reported, actual.Reported);
                    Assert.AreEqual(expected.Score, actual.Score);
                    Assert.AreEqual(expected.Source, actual.Source);
                    Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Update, actual.Update);
                }
            }
            #region Exception Tests
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuidAsync_PermissionsException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuidAsync("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuidAsync_ArgumentNullException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuidAsync("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuidAsync_KeyNotFoundException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuidAsync("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuidAsync("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuidAsync_RepositoryException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuidAsync("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuidAsync("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuidAsync_IntegrationApiException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuidAsync("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuidAsync_Exception()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuidAsync("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsAsync_PermissionsException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsAsync_ArgumentNullException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsAsync_KeyNotFoundException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsAsync_RepositoryException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsAsync_IntegrationApiException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsAsync_Exception()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
            }

            #endregion

        }

        [TestClass]
        public class Get2
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private Mock<IStudentAptitudeAssessmentsService> studentAptitudeAssessmentsServiceMock;
            private StudentAptitudeAssessmentsController studentAptitudeAssessmentsController;
            private IStudentAptitudeAssessmentsService studentAptitudeAssessmentsService;
            private IEnumerable<StudentAptitudeAssessments> studentAptAssesmentsDtos;
            private IEnumerable<StudentAptitudeAssessments2> studentAptAssesmentsDtos2;
            private ILogger logger = new Mock<ILogger>().Object;
            private Paging page;
            private int limit;
            private int offset;
            private Tuple<IEnumerable<StudentAptitudeAssessments>, int> stuAptAssessmentsDtosTuple;
            private Tuple<IEnumerable<StudentAptitudeAssessments2>, int> stuAptAssessmentsDtosTuple2;
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                studentAptitudeAssessmentsServiceMock = new Mock<IStudentAptitudeAssessmentsService>();

                studentAptitudeAssessmentsService = studentAptitudeAssessmentsServiceMock.Object;
                studentAptAssesmentsDtos = StudentAptitudeAssessmentsControllerTests.BuildStudentAptitudeAssessments();
                studentAptAssesmentsDtos2 = StudentAptitudeAssessmentsControllerTests.BuildStudentAptitudeAssessments2();
               
                //studentAptitudeAssessmentsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                studentAptitudeAssessmentsController = new StudentAptitudeAssessmentsController(studentAptitudeAssessmentsService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                studentAptitudeAssessmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                limit = 200;
                offset = 0;
                page = new Paging(limit, offset);
                stuAptAssessmentsDtosTuple = new Tuple<IEnumerable<StudentAptitudeAssessments>, int>(studentAptAssesmentsDtos, 3);
                stuAptAssessmentsDtosTuple2 = new Tuple<IEnumerable<StudentAptitudeAssessments2>, int>(studentAptAssesmentsDtos2, 3);

                studentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAptitudeAssessmentsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(studentAptAssesmentsDtos.ElementAt(0)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAptitudeAssessmentsServiceMock = null;
                studentAptitudeAssessmentsService = null;
                studentAptitudeAssessmentsController = null;
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessmentsByGuid3Async()
            {
                string guid = studentAptAssesmentsDtos.ElementAt(0).Id;
                studentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsByGuid3Async(guid, It.IsAny<bool>())).ReturnsAsync(studentAptAssesmentsDtos2.ElementAt(0));
                var StudentAptitudeAssessments = await studentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid3Async(guid);
                var expected = StudentAptitudeAssessments;
                var actual = studentAptAssesmentsDtos2.ElementAt(0);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                Assert.AreEqual(expected.Assessment, actual.Assessment);
                Assert.AreEqual(expected.Form, actual.Form);
                Assert.AreEqual(expected.Percentile, actual.Percentile);
                Assert.AreEqual(expected.Preference, actual.Preference);
                Assert.AreEqual(expected.Reported, actual.Reported);
                Assert.AreEqual(expected.Score, actual.Score);
                Assert.AreEqual(expected.Source, actual.Source);
                Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.Student, actual.Student);
                Assert.AreEqual(expected.Update, actual.Update);
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessments3AsyncCache()
            {
                studentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                studentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessments3Async( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), offset, limit, It.IsAny<bool>())).ReturnsAsync(stuAptAssessmentsDtosTuple2);
                var acadProg = await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter);
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await acadProg.ExecuteAsync(cancelToken);
                List<Dtos.StudentAptitudeAssessments2> StudentAptitudeAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2>>)httpResponseMessage.Content).Value as List<Dtos.StudentAptitudeAssessments2>;
                for (var i = 0; i < StudentAptitudeAssessments.Count; i++)
                {
                    var expected = studentAptAssesmentsDtos2.ToList()[i];
                    var actual = StudentAptitudeAssessments[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                    Assert.AreEqual(expected.Assessment, actual.Assessment);
                    Assert.AreEqual(expected.Form, actual.Form);
                    Assert.AreEqual(expected.Percentile, actual.Percentile);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Reported, actual.Reported);
                    Assert.AreEqual(expected.Score, actual.Score);
                    Assert.AreEqual(expected.Source, actual.Source);
                    Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Update, actual.Update);
                }
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessments3AsyncNoCache()
            {
                studentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                studentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(stuAptAssessmentsDtosTuple2);
                var HttpAction = (await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter));
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await HttpAction.ExecuteAsync(cancelToken);
                List<Dtos.StudentAptitudeAssessments2> StudentAptitudeAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2>>)httpResponseMessage.Content).Value as List<Dtos.StudentAptitudeAssessments2>;
                for (var i = 0; i < StudentAptitudeAssessments.Count; i++)
                {
                    var expected = studentAptAssesmentsDtos2.ToList()[i];
                    var actual = StudentAptitudeAssessments[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                    Assert.AreEqual(expected.Assessment, actual.Assessment);
                    Assert.AreEqual(expected.Form, actual.Form);
                    Assert.AreEqual(expected.Percentile, actual.Percentile);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Reported, actual.Reported);
                    Assert.AreEqual(expected.Score, actual.Score);
                    Assert.AreEqual(expected.Source, actual.Source);
                    Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Update, actual.Update);
                }
            }

            [TestMethod]
            public async Task ReturnsStudentAptitudeAssessments3AsyncNoPaging()
            {
                studentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                studentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(stuAptAssessmentsDtosTuple2);
                var HttpAction = (await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(null, criteriaFilter, personFilter));
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await HttpAction.ExecuteAsync(cancelToken);
                List<Dtos.StudentAptitudeAssessments2> StudentAptitudeAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2>>)httpResponseMessage.Content).Value as List<Dtos.StudentAptitudeAssessments2>;
                for (var i = 0; i < StudentAptitudeAssessments.Count; i++)
                {
                    var expected = studentAptAssesmentsDtos2.ToList()[i];
                    var actual = StudentAptitudeAssessments[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AssessedOn, actual.AssessedOn);
                    Assert.AreEqual(expected.Assessment, actual.Assessment);
                    Assert.AreEqual(expected.Form, actual.Form);
                    Assert.AreEqual(expected.Percentile, actual.Percentile);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Reported, actual.Reported);
                    Assert.AreEqual(expected.Score, actual.Score);
                    Assert.AreEqual(expected.Source, actual.Source);
                    Assert.AreEqual(expected.SpecialCircumstances, actual.SpecialCircumstances);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Update, actual.Update);
                }
            }

            #region DELETE
            [TestMethod]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_HttpResponseMessage()
            {
                studentAptitudeAssessmentsServiceMock.Setup(s => s.DeleteStudentAptitudeAssessmentAsync("1234")).Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync("1234");
            }
            #endregion

            #region Exception Tests
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid3Async_PermissionsException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid3Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid3Async_ArgumentNullException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid3Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid3Async_KeyNotFoundException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid3Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid3Async("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid3Async_RepositoryException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid3Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid3Async("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid3Async_IntegrationApiException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid3Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid3Async_Exception()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid3Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments3Async_PermissionsException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments3Async_ArgumentNullException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments3Async_KeyNotFoundException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments3Async_RepositoryException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments3Async_IntegrationApiException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments3Async_Exception()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await studentAptitudeAssessmentsController.GetStudentAptitudeAssessments3Async(page, criteriaFilter, personFilter);
            }

            //delete

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_EmptyArguement()
            {
                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_PermissionsException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.DeleteStudentAptitudeAssessmentAsync(It.IsAny<string>())).Throws(new PermissionsException());
                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync(Guid.NewGuid().ToString());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_ArgumentNullException()
            {
                studentAptitudeAssessmentsServiceMock
                     .Setup(s => s.DeleteStudentAptitudeAssessmentAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_KeyNotFoundException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.DeleteStudentAptitudeAssessmentAsync(It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_RepositoryException()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.DeleteStudentAptitudeAssessmentAsync(It.IsAny<string>()))
                    .Throws(new RepositoryException());
                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_IntegrationApiException()
            {
                studentAptitudeAssessmentsServiceMock
                   .Setup(s => s.DeleteStudentAptitudeAssessmentAsync(It.IsAny<string>()))
                    .Throws(new IntegrationApiException());
                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_Exception()
            {
                studentAptitudeAssessmentsServiceMock
                    .Setup(s => s.DeleteStudentAptitudeAssessmentAsync(It.IsAny<string>()))
                    .Throws(new Exception());
                await studentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync(Guid.NewGuid().ToString());
            }
            #endregion

        }
        [TestClass]
        public class StudentAptitudeAssessmentsControllerTests_POST
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<IStudentAptitudeAssessmentsService> StudentAptitudeAssessmentsServiceMock;
            private StudentAptitudeAssessmentsController StudentAptitudeAssessmentsController;
            private IStudentAptitudeAssessmentsService StudentAptitudeAssessmentsService;
            private IEnumerable<StudentAptitudeAssessments> StudentAptitudeAssessmentsCollection;
            private IEnumerable<StudentAptitudeAssessments2> StudentAptitudeAssessmentsCollection2;
            private StudentAptitudeAssessments studentAptAssesment;
            private ILogger logger = new Mock<ILogger>().Object;
            private Paging page;
            private int limit;
            private int offset;
            private Tuple<IEnumerable<StudentAptitudeAssessments>, int> stuAptAssessmentsDtosTuple;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                StudentAptitudeAssessmentsServiceMock = new Mock<IStudentAptitudeAssessmentsService>();

                StudentAptitudeAssessmentsService = StudentAptitudeAssessmentsServiceMock.Object;
                StudentAptitudeAssessmentsCollection = StudentAptitudeAssessmentsControllerTests.BuildStudentAptitudeAssessments();
                StudentAptitudeAssessmentsCollection2 = StudentAptitudeAssessmentsControllerTests.BuildStudentAptitudeAssessments2();
                string guid = StudentAptitudeAssessmentsCollection.ElementAt(0).Id;
                studentAptAssesment = StudentAptitudeAssessmentsCollection.ElementAt(0);
                StudentAptitudeAssessmentsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                StudentAptitudeAssessmentsController = new StudentAptitudeAssessmentsController(StudentAptitudeAssessmentsService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                StudentAptitudeAssessmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestCleanup]
            public void Cleanup()
            {
                
            }

            #endregion

            [TestMethod]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessments2Async(expected)).ReturnsAsync(expected);

                expected.Id = "00000000-0000-0000-0000-000000000000";

                var actual = await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(expected);
                //Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.AssessedOn, actual.AssessedOn, "AssessedOn");
                Assert.AreEqual(expected.Assessment.Id, actual.Assessment.Id, "Assessment.Id");
                Assert.AreEqual(expected.Form, actual.Form, "Form");
                Assert.AreEqual(expected.Percentile, actual.Percentile, "Percentile");
                Assert.AreEqual(expected.Preference, actual.Preference, "Preference");
                Assert.AreEqual(expected.Reported, actual.Reported, "Reported");
                Assert.AreEqual(expected.Score, actual.Score, "Score");
                Assert.AreEqual(expected.Source, actual.Source, "Source");
                Assert.AreEqual(expected.SpecialCircumstances[0], actual.SpecialCircumstances[0], "SpecialCircumstances");
                Assert.AreEqual(expected.Status, actual.Status, "Status");
                Assert.AreEqual(expected.Student.Id, actual.Student.Id, "Student");
                Assert.AreEqual(expected.Update, actual.Update, "Update");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3_NullArgument()
            {
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3_PermissionsException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessments2Async(expected)).Throws<PermissionsException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(expected);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3_ArgumentException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessments2Async(expected)).Throws<ArgumentException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3_RepositoryException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessments2Async(expected)).Throws<RepositoryException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3_IntegrationApiException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessments2Async(expected)).Throws<IntegrationApiException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3_ConfigurationException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessments2Async(expected)).Throws<ConfigurationException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments3_Exception()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessments2Async(expected)).Throws<Exception>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments3Async(expected);
            }
           
        }

        [TestClass]
        public class StudentAptitudeAssessmentsControllerTests_PUT
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<IStudentAptitudeAssessmentsService> StudentAptitudeAssessmentsServiceMock;
            private StudentAptitudeAssessmentsController StudentAptitudeAssessmentsController;
            private IStudentAptitudeAssessmentsService StudentAptitudeAssessmentsService;
            private IEnumerable<StudentAptitudeAssessments> StudentAptitudeAssessmentsCollection;
            private IEnumerable<StudentAptitudeAssessments2> StudentAptitudeAssessmentsCollection2;
            private StudentAptitudeAssessments studentAptAssesment;
            private ILogger logger = new Mock<ILogger>().Object;
            private Paging page;
            private int limit;
            private int offset;
            private Tuple<IEnumerable<StudentAptitudeAssessments>, int> stuAptAssessmentsDtosTuple;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                StudentAptitudeAssessmentsServiceMock = new Mock<IStudentAptitudeAssessmentsService>();

                StudentAptitudeAssessmentsService = StudentAptitudeAssessmentsServiceMock.Object;
                StudentAptitudeAssessmentsCollection = StudentAptitudeAssessmentsControllerTests.BuildStudentAptitudeAssessments();
                StudentAptitudeAssessmentsCollection2 = StudentAptitudeAssessmentsControllerTests.BuildStudentAptitudeAssessments2();
                string guid = StudentAptitudeAssessmentsCollection.ElementAt(0).Id;
                studentAptAssesment = StudentAptitudeAssessmentsCollection.ElementAt(0);
                StudentAptitudeAssessmentsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                StudentAptitudeAssessmentsController = new StudentAptitudeAssessmentsController(StudentAptitudeAssessmentsService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                StudentAptitudeAssessmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                StudentAptitudeAssessmentsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(StudentAptitudeAssessmentsCollection.FirstOrDefault()));

            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAptitudeAssessmentsServiceMock = null;
                StudentAptitudeAssessmentsService = null;
                StudentAptitudeAssessmentsController = null;
            }

            #endregion

           
            [TestMethod]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessments2Async(It.IsAny<Dtos.StudentAptitudeAssessments2>())).ReturnsAsync(expected);
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsByGuid3Async(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

                var actual = await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(expected.Id, expected);
                Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.AssessedOn, actual.AssessedOn, "AssessedOn");
                Assert.AreEqual(expected.Assessment.Id, actual.Assessment.Id, "Assessment.Id");
                Assert.AreEqual(expected.Form, actual.Form, "Form");
                Assert.AreEqual(expected.Percentile, actual.Percentile, "Percentile");
                Assert.AreEqual(expected.Preference, actual.Preference, "Preference");
                Assert.AreEqual(expected.Reported, actual.Reported, "Reported");
                Assert.AreEqual(expected.Score, actual.Score, "Score");
                Assert.AreEqual(expected.Source, actual.Source, "Source");
                Assert.AreEqual(expected.SpecialCircumstances[0], actual.SpecialCircumstances[0], "SpecialCircumstances");
                Assert.AreEqual(expected.Status, actual.Status, "Status");
                Assert.AreEqual(expected.Student.Id, actual.Student.Id, "Student");
                Assert.AreEqual(expected.Update, actual.Update, "Update");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_NullArgument()
            {
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_EmptyArgument()
            {
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async("", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_PermissionsException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessments2Async(expected)).Throws<PermissionsException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(expected.Id, expected);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_ArgumentException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessments2Async(expected)).Throws<ArgumentException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_RepositoryException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessments2Async(expected)).Throws<RepositoryException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_IntegrationApiException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessments2Async(expected)).Throws<IntegrationApiException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_ConfigurationException()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessments2Async(expected)).Throws<ConfigurationException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments3_Exception()
            {
                var expected = StudentAptitudeAssessmentsCollection2.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessments2Async(expected)).Throws<Exception>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments3Async(expected.Id, expected);
            }
            
        }

        private static List<StudentAptitudeAssessments> BuildStudentAptitudeAssessments()
        {
            var StudentAptitudeAssessmentsDtos = new List<Dtos.StudentAptitudeAssessments>();
            var stuAssessmentDto1 = new Dtos.StudentAptitudeAssessments()
            {
                Id = "AB1234567890",
                AssessedOn = new DateTimeOffset(DateTime.Today),
                Assessment = new GuidObject2("P12345678910"),
                Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "1" },
                Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("C12345678910"), Value = 79 } },
                Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 200 },
                Source = new GuidObject2("S12345678910"),
                SpecialCircumstances = new List<GuidObject2>() {  new GuidObject2("L12345678910"), new GuidObject2("AL1234567890") },
                Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Original
            };
            var stuAssessmentDto2 = new Dtos.StudentAptitudeAssessments()
            {
                Id = "BC1234567890",
                AssessedOn = new DateTimeOffset(DateTime.Today),
                Assessment = new GuidObject2("P12345678910"),
                Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "2" },
                Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"), Value = 79 } },
                Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                Source = new GuidObject2("S12345678910"),
                SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("1df164eb-8178-5678-a9f7-24f27f3991d8"), new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8") },
                Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Inactive,
                Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Revised
            };
            var stuAssessmentDto3 = new Dtos.StudentAptitudeAssessments()
            {
                Id = "CD1234567890",
                AssessedOn = new DateTimeOffset(DateTime.Today),
                Assessment = new GuidObject2("P12345678910"),
                Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "3" },
                Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"), Value = 79 } },
                Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.NotSet,
                Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                Source = new GuidObject2("S12345678910"),
                SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("1df164eb-8178-5678-a9f7-24f27f3991d8"), new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8") },
                Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Recentered

            };
            StudentAptitudeAssessmentsDtos.Add(stuAssessmentDto1);
            StudentAptitudeAssessmentsDtos.Add(stuAssessmentDto2);
            StudentAptitudeAssessmentsDtos.Add(stuAssessmentDto3);
            return StudentAptitudeAssessmentsDtos;
        }

        private static List<StudentAptitudeAssessments2> BuildStudentAptitudeAssessments2()
        {
            var StudentAptitudeAssessmentsDtos = new List<Dtos.StudentAptitudeAssessments2>();
            var stuAssessmentDto1 = new Dtos.StudentAptitudeAssessments2()
            {
                Id = "AB1234567890",
                AssessedOn = new DateTimeOffset(DateTime.Today),
                Assessment = new GuidObject2("P12345678910"),
                Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "1" },
                Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("C12345678910"), Value = 79 } },
                Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 200 },
                Source = new GuidObject2("S12345678910"),
                SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("L12345678910"), new GuidObject2("AL1234567890") },
                Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Original
            };
            var stuAssessmentDto2 = new Dtos.StudentAptitudeAssessments2()
            {
                Id = "BC1234567890",
                AssessedOn = new DateTimeOffset(DateTime.Today),
                Assessment = new GuidObject2("P12345678910"),
                Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "2" },
                Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"), Value = 79 } },
                Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.Primary,
                Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                Source = new GuidObject2("S12345678910"),
                SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("1df164eb-8178-5678-a9f7-24f27f3991d8"), new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8") },
                Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Inactive,
                Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Revised
            };
            var stuAssessmentDto3 = new Dtos.StudentAptitudeAssessments2()
            {
                Id = "CD1234567890",
                AssessedOn = new DateTimeOffset(DateTime.Today),
                Assessment = new GuidObject2("P12345678910"),
                Form = new StudentAptitudeAssessmentsForm() { Name = "ACT", Number = "3" },
                Percentile = new List<StudentAptitudeAssessmentsPercentile>() { new StudentAptitudeAssessmentsPercentile() { Type = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"), Value = 79 } },
                Preference = Dtos.EnumProperties.StudentAptitudeAssessmentsPreference.NotSet,
                Reported = Dtos.EnumProperties.StudentAptitudeAssessmentsReported.Official,
                Score = new StudentAptitudeAssessmentsScore() { Type = Dtos.EnumProperties.StudentAptitudeAssessmentsScoreType.Numeric, Value = 190 },
                Source = new GuidObject2("S12345678910"),
                SpecialCircumstances = new List<GuidObject2>() { new GuidObject2("1df164eb-8178-5678-a9f7-24f27f3991d8"), new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8") },
                Status = Dtos.EnumProperties.StudentAptitudeAssessmentsStatus.Active,
                Student = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                Update = Dtos.EnumProperties.StudentAptitudeAssessmentsUpdateStatus.Recentered

            };
            StudentAptitudeAssessmentsDtos.Add(stuAssessmentDto1);
            StudentAptitudeAssessmentsDtos.Add(stuAssessmentDto2);
            StudentAptitudeAssessmentsDtos.Add(stuAssessmentDto3);
            return StudentAptitudeAssessmentsDtos;
        }
    }
}