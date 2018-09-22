// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
            public async Task ReturnsStudentAptitudeAssessmentsByGuid2Async()
            {
                string guid = studentAptAssesmentsDtos.ElementAt(0).Id;
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsByGuid2Async(guid, It.IsAny<bool>())).ReturnsAsync(studentAptAssesmentsDtos.ElementAt(0));
                var StudentAptitudeAssessments = await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid2Async(guid);
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
            public async Task ReturnsStudentAptitudeAssessments2AsyncCache()
            {
                StudentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                StudentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessments2Async("", offset, limit, It.IsAny<bool>())).ReturnsAsync(stuAptAssessmentsDtosTuple);
                var acadProg = await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(page);
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
            public async Task ReturnsStudentAptitudeAssessments2AsyncNoCache()
            {
                StudentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                StudentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessments2Async("", It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(stuAptAssessmentsDtosTuple);
                var HttpAction = (await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(page));
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
            public async Task ReturnsStudentAptitudeAssessments2AsyncNoPaging()
            {
                StudentAptitudeAssessmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                StudentAptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessments2Async("", It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(stuAptAssessmentsDtosTuple);
                var HttpAction = (await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(null));
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

            #region DELETE
            [TestMethod]
            public async Task StudentAptitudeAssessmentsController_DeleteStudentAptitudeAssessmentsAsync_HttpResponseMessage()
            {
                StudentAptitudeAssessmentsServiceMock.Setup(s => s.DeleteStudentAptitudeAssessmentAsync("1234")).Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

                await StudentAptitudeAssessmentsController.DeleteStudentAptitudeAssessmentsAsync("1234");
            }
            #endregion
            #region Exception Tests
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid2Async_PermissionsException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid2Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid2Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid2Async_ArgumentNullException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid2Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid2Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid2Async_KeyNotFoundException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid2Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid2Async("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid2Async_RepositoryException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid2Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid2Async("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid2Async_IntegrationApiException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid2Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid2Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessmentsByGuid2Async_Exception()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsByGuid2Async("asdf", It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsByGuid2Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments2Async_PermissionsException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments2Async("", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(page);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments2Async_ArgumentNullException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments2Async("", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments2Async_KeyNotFoundException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments2Async("", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments2Async_RepositoryException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments2Async("", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments2Async_IntegrationApiException()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessments2Async(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_GetStudentAptitudeAssessments2Async_Exception()
            {
                StudentAptitudeAssessmentsServiceMock
                    .Setup(s => s.GetStudentAptitudeAssessments2Async("", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await StudentAptitudeAssessmentsController.GetStudentAptitudeAssessmentsAsync(page);
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
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessmentsAsync(expected)).ReturnsAsync(expected);

                expected.Id = "00000000-0000-0000-0000-000000000000";

                var actual = await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(expected);
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
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments_NullArgument()
            {
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments_PermissionsException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessmentsAsync(expected)).Throws<PermissionsException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(expected);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments_ArgumentException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessmentsAsync(expected)).Throws<ArgumentException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments_RepositoryException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessmentsAsync(expected)).Throws<RepositoryException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments_IntegrationApiException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessmentsAsync(expected)).Throws<IntegrationApiException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments_ConfigurationException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessmentsAsync(expected)).Throws<ConfigurationException>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PostStudentAptitudeAssessments_Exception()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.CreateStudentAptitudeAssessmentsAsync(expected)).Throws<Exception>();
                await StudentAptitudeAssessmentsController.PostStudentAptitudeAssessments2Async(expected);
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
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessmentsAsync(It.IsAny<Dtos.StudentAptitudeAssessments>())).ReturnsAsync(expected);
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.GetStudentAptitudeAssessmentsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

                var actual = await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(expected.Id, expected);
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
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_NullArgument()
            {
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_EmptyArgument()
            {
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async("", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_PermissionsException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessmentsAsync(expected)).Throws<PermissionsException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(expected.Id, expected);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_ArgumentException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessmentsAsync(expected)).Throws<ArgumentException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_RepositoryException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessmentsAsync(expected)).Throws<RepositoryException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_IntegrationApiException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessmentsAsync(expected)).Throws<IntegrationApiException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_ConfigurationException()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessmentsAsync(expected)).Throws<ConfigurationException>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAptitudeAssessmentsController_PutStudentAptitudeAssessments_Exception()
            {
                var expected = StudentAptitudeAssessmentsCollection.FirstOrDefault();
                StudentAptitudeAssessmentsServiceMock.Setup(x => x.UpdateStudentAptitudeAssessmentsAsync(expected)).Throws<Exception>();
                await StudentAptitudeAssessmentsController.PutStudentAptitudeAssessments2Async(expected.Id, expected);
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
    }
}