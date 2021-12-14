// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using StudentAcademicPeriodProfiles = Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAcademicPeriodProfilesControllerTests
    {
        [TestClass]
        public class GET
        {
            public TestContext TestContext { get; set; }

            Mock<IStudentAcademicPeriodProfilesService> studentAcademicPeriodProfilesServiceMock;
            Mock<ILogger> loggerMock;

            StudentAcademicPeriodProfilesController studentAcademicPeriodProfilesController;
            List<Dtos.StudentAcademicPeriodProfiles> studentAcademicPeriodProfileDtos;
            int offset = 0;
            int limit = 2;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                studentAcademicPeriodProfilesServiceMock = new Mock<IStudentAcademicPeriodProfilesService>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                studentAcademicPeriodProfilesController = new StudentAcademicPeriodProfilesController(studentAcademicPeriodProfilesServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                studentAcademicPeriodProfilesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentAcademicPeriodProfilesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAcademicPeriodProfilesController = null;
                studentAcademicPeriodProfileDtos = null;
                studentAcademicPeriodProfilesServiceMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetAll_NoCache_True()
            {
                studentAcademicPeriodProfilesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var studentAcadProfiles = studentAcademicPeriodProfileDtos.Take(2);
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(studentAcadProfiles, 4);
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = new Paging(limit, offset);
                var actuals = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentAcademicPeriodProfiles> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAcademicPeriodProfiles>;

                Assert.IsNotNull(results);
                Assert.AreEqual(2, results.Count());

                foreach (var result in results)
                {
                    var expected = studentAcadProfiles.FirstOrDefault(i => i.Id.Equals(result.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, result.Id);
                    Assert.AreEqual(expected.AcademicLoad, result.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, result.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, result.AcademicPeriodEnrollmentStatus.Id);
                    Assert.AreEqual(expected.Measures.Count(), result.Measures.Count());

                    foreach (var resultMeasure in result.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Classification.Id.Equals(resultMeasure.Classification.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, resultMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, resultMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, resultMeasure.PerformanceMeasure);
                    }
                    Assert.AreEqual(expected.Person.Id, result.Person.Id);
                    Assert.AreEqual(expected.Residency.Id, result.Residency.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, result.StudentStatus.Id);
                    foreach (var resultTag in result.Tags)
                    {
                        var expectedTag = expected.Tags.FirstOrDefault(i => i.Id.Equals(resultTag.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedTag);

                        Assert.AreEqual(expectedTag.Id, resultTag.Id);
                    }
                    Assert.AreEqual(expected.Type.Id, result.Type.Id);
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetAll_PagingNull()
            {
                studentAcademicPeriodProfilesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(studentAcademicPeriodProfileDtos, It.IsAny<int>());
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = null;
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentAcademicPeriodProfiles> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAcademicPeriodProfiles>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentAcademicPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, actual.AcademicPeriodEnrollmentStatus.Id);
                    Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());

                    foreach (var resultMeasure in actual.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Classification.Id.Equals(resultMeasure.Classification.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, resultMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, resultMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, resultMeasure.PerformanceMeasure);
                    }
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                    Assert.AreEqual(expected.Residency.Id, actual.Residency.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                    foreach (var resultTag in actual.Tags)
                    {
                        var expectedTag = expected.Tags.FirstOrDefault(i => i.Id.Equals(resultTag.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedTag);

                        Assert.AreEqual(expectedTag.Id, resultTag.Id);
                    }
                    Assert.AreEqual(expected.Type.Id, actual.Type.Id);
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetById()
            {
                string id = "af4d47eb-f06b-4add-b5bf-d9529742387a";
                var expected = studentAcademicPeriodProfileDtos[0];

                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(id);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, actual.AcademicPeriodEnrollmentStatus.Id);
                Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());

                foreach (var resultMeasure in actual.Measures)
                {
                    var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Classification.Id.Equals(resultMeasure.Classification.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expectedMeasure);

                    Assert.AreEqual(expectedMeasure.Classification.Id, resultMeasure.Classification.Id);
                    Assert.AreEqual(expectedMeasure.Level.Id, resultMeasure.Level.Id);
                    Assert.AreEqual(expectedMeasure.PerformanceMeasure, resultMeasure.PerformanceMeasure);
                }
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Residency.Id, actual.Residency.Id);
                Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                foreach (var resultTag in actual.Tags)
                {
                    var expectedTag = expected.Tags.FirstOrDefault(i => i.Id.Equals(resultTag.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expectedTag);

                    Assert.AreEqual(expectedTag.Id, resultTag.Id);
                }
                Assert.AreEqual(expected.Type.Id, actual.Type.Id);
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetAllV11_NoCache_True()
            {
                studentAcademicPeriodProfilesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var studentAcadProfiles = studentAcademicPeriodProfileDtos.Take(2);
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(studentAcadProfiles, 4);
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = new Paging(limit, offset);
                var actuals = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentAcademicPeriodProfiles> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAcademicPeriodProfiles>;

                Assert.IsNotNull(results);
                Assert.AreEqual(2, results.Count());

                foreach (var result in results)
                {
                    var expected = studentAcadProfiles.FirstOrDefault(i => i.Id.Equals(result.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, result.Id);
                    Assert.AreEqual(expected.AcademicLoad, result.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, result.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, result.AcademicPeriodEnrollmentStatus.Id);
                    Assert.AreEqual(expected.Measures.Count(), result.Measures.Count());

                    foreach (var resultMeasure in result.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Classification.Id.Equals(resultMeasure.Classification.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, resultMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, resultMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, resultMeasure.PerformanceMeasure);
                    }
                    Assert.AreEqual(expected.Person.Id, result.Person.Id);
                    Assert.AreEqual(expected.Residency.Id, result.Residency.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, result.StudentStatus.Id);
                    foreach (var resultTag in result.Tags)
                    {
                        var expectedTag = expected.Tags.FirstOrDefault(i => i.Id.Equals(resultTag.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedTag);

                        Assert.AreEqual(expectedTag.Id, resultTag.Id);
                    }
                    Assert.AreEqual(expected.Type.Id, result.Type.Id);
                }
            }

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetAllV11_PagingNull()
            {
                studentAcademicPeriodProfilesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(studentAcademicPeriodProfileDtos, It.IsAny<int>());
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = null;
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentAcademicPeriodProfiles> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAcademicPeriodProfiles>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentAcademicPeriodProfileDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicLoad, actual.AcademicLoad);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AcademicPeriodEnrollmentStatus.Id, actual.AcademicPeriodEnrollmentStatus.Id);
                    Assert.AreEqual(expected.Measures.Count(), actual.Measures.Count());

                    foreach (var resultMeasure in actual.Measures)
                    {
                        var expectedMeasure = expected.Measures.FirstOrDefault(i => i.Classification.Id.Equals(resultMeasure.Classification.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedMeasure);

                        Assert.AreEqual(expectedMeasure.Classification.Id, resultMeasure.Classification.Id);
                        Assert.AreEqual(expectedMeasure.Level.Id, resultMeasure.Level.Id);
                        Assert.AreEqual(expectedMeasure.PerformanceMeasure, resultMeasure.PerformanceMeasure);
                    }
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                    Assert.AreEqual(expected.Residency.Id, actual.Residency.Id);
                    Assert.AreEqual(expected.StudentStatus.Id, actual.StudentStatus.Id);
                    foreach (var resultTag in actual.Tags)
                    {
                        var expectedTag = expected.Tags.FirstOrDefault(i => i.Id.Equals(resultTag.Id, StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(expectedTag);

                        Assert.AreEqual(expectedTag.Id, resultTag.Id);
                    }
                    Assert.AreEqual(expected.Type.Id, actual.Type.Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetAsync_PermissionException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(It.IsAny<Paging>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetAsync_ArgumentException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(It.IsAny<Paging>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetAsync_RepositoryException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(It.IsAny<Paging>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetAsync_IntegrationApiException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(It.IsAny<Paging>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetAsync_Exception()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(It.IsAny<Paging>(), It.IsAny<string>(), It.IsAny<string>());
            }

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task StudentAcademicPeriodProfilesController_GetAsyncV11_PermissionException()
            //{
            //    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            //    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(It.IsAny<Paging>(), It.IsAny<string>());
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task StudentAcademicPeriodProfilesController_GetAsyncV11_ArgumentException()
            //{
            //    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            //    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(It.IsAny<Paging>(), It.IsAny<string>());
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task StudentAcademicPeriodProfilesController_GetAsyncV11_RepositoryException()
            //{
            //    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            //    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(It.IsAny<Paging>(), It.IsAny<string>());
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task StudentAcademicPeriodProfilesController_GetAsyncV11_IntegrationApiException()
            //{
            //    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
            //    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(It.IsAny<Paging>(), It.IsAny<string>());
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task StudentAcademicPeriodProfilesController_GetAsyncV11_Exception()
            //{
            //    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
            //    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(It.IsAny<Paging>(), It.IsAny<string>());
            //}

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetById_PermissionException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetById_KeyNotFoundException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetById_ArgumentNullException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetById_RepositoryException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetById_IntegrationApiException()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetById_Exception()
            {
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>());
            }

            //GET v11
            //Successful
            //GetStudentAcademicPeriodProfiles2Async

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetStudentAcademicPeriodProfiles2Async_Permissions()
            {
                Paging paging = new Paging(limit, offset);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPeriodProfiles" },
                    { "action", "GetStudentAcademicPeriodProfiles2Async" }
                };
                HttpRoute route = new HttpRoute("student-academic-period-profiles", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicPeriodProfilesController.Request.SetRouteData(data);
                studentAcademicPeriodProfilesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriodProfile);

                var controllerContext = studentAcademicPeriodProfilesController.ControllerContext;
                var actionDescriptor = studentAcademicPeriodProfilesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(studentAcademicPeriodProfileDtos, 5);

                studentAcademicPeriodProfilesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var actuals = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfiles2Async(paging);

                Object filterObject;
                studentAcademicPeriodProfilesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicPeriodProfile));

            }

            //GET v11
            //Exception
            //GetStudentAcademicPeriodProfiles2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetStudentAcademicPeriodProfiles2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPeriodProfiles" },
                    { "action", "GetStudentAcademicPeriodProfiles2Async" }
                };
                HttpRoute route = new HttpRoute("student-academic-period-profiles", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicPeriodProfilesController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentAcademicPeriodProfilesController.ControllerContext;
                var actionDescriptor = studentAcademicPeriodProfilesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                    studentAcademicPeriodProfilesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-period-profiles."));
                    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(It.IsAny<Paging>(), It.IsAny<string>(), It.IsAny<string>());

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v7
            //Successful
            //GetStudentAcademicPeriodProfilesAsync

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetStudentAcademicPeriodProfilesAsync_Permissions()
            {
                Paging paging = new Paging(limit, offset);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPeriodProfiles" },
                    { "action", "GetStudentAcademicPeriodProfilesAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-period-profiles", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicPeriodProfilesController.Request.SetRouteData(data);
                studentAcademicPeriodProfilesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriodProfile);

                var controllerContext = studentAcademicPeriodProfilesController.ControllerContext;
                var actionDescriptor = studentAcademicPeriodProfilesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(studentAcademicPeriodProfileDtos, 5);

                studentAcademicPeriodProfilesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var actuals = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(paging);

                Object filterObject;
                studentAcademicPeriodProfilesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicPeriodProfile));

            }

            //GET v7
            //Exception
            //GetStudentAcademicPeriodProfilesAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetStudentAcademicPeriodProfilesAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPeriodProfiles" },
                    { "action", "GetStudentAcademicPeriodProfilesAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-period-profiles", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicPeriodProfilesController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentAcademicPeriodProfilesController.ControllerContext;
                var actionDescriptor = studentAcademicPeriodProfilesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfilesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                    studentAcademicPeriodProfilesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-period-profiles."));
                    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfilesAsync(It.IsAny<Paging>(), It.IsAny<string>(), It.IsAny<string>());

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET by id v7, v11
            //Successful
            //GetStudentAcademicPeriodProfileByGuidAsync

            [TestMethod]
            public async Task StudentAcademicPeriodProfilesController_GetStudentAcademicPeriodProfileByGuidAsync_Permissions()
            {
                string id = "af4d47eb-f06b-4add-b5bf-d9529742387a";
                var expected = studentAcademicPeriodProfileDtos[0];
                Paging paging = new Paging(limit, offset);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPeriodProfiles" },
                    { "action", "GetStudentAcademicPeriodProfileByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-period-profiles", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicPeriodProfilesController.Request.SetRouteData(data);
                studentAcademicPeriodProfilesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriodProfile);

                var controllerContext = studentAcademicPeriodProfilesController.ControllerContext;
                var actionDescriptor = studentAcademicPeriodProfilesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(studentAcademicPeriodProfileDtos, 5);

                studentAcademicPeriodProfilesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);
                var actual = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(id);

                Object filterObject;
                studentAcademicPeriodProfilesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicPeriodProfile));

            }

            //GET by id v7, v11
            //Successful
            //GetStudentAcademicPeriodProfileByGuidAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_GetStudentAcademicPeriodProfileByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPeriodProfiles" },
                    { "action", "GetStudentAcademicPeriodProfileByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-period-profiles", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicPeriodProfilesController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentAcademicPeriodProfilesController.ControllerContext;
                var actionDescriptor = studentAcademicPeriodProfilesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentAcademicPeriodProfilesServiceMock.Setup(i => i.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                    studentAcademicPeriodProfilesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-period-profiles."));
                    var results = await studentAcademicPeriodProfilesController.GetStudentAcademicPeriodProfileByGuidAsync(It.IsAny<string>());

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #region PUT POST DELETE
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_PUT_Not_Supported()
            {
                var actual = await studentAcademicPeriodProfilesController.UpdateStudentAcademicPeriodProfilesAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentAcademicPeriodProfiles>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_POST_Not_Supported()
            {
                var actual = await studentAcademicPeriodProfilesController.CreateStudentAcademicPeriodProfilesAsync(It.IsAny<Dtos.StudentAcademicPeriodProfiles>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicPeriodProfilesController_DELETE_Not_Supported()
            {
                await studentAcademicPeriodProfilesController.DeleteStudentAcademicPeriodProfilesAsync(It.IsAny<string>());
            }
            #endregion

            private void BuildData()
            {
                #region Building StudentAcademicPeriodProfiles
                studentAcademicPeriodProfileDtos = new List<StudentAcademicPeriodProfiles>() 
                {
                    new StudentAcademicPeriodProfiles(){ 
                        Id = "af4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.NotSet, 
                        AcademicPeriod = new Dtos.GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("2e5ff61-2f5b-4d4f-8396-b671184bdbd8"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("e6a83c64-1f86-42dc-969f-8ee22fc3ab55"), Level = new Dtos.GuidObject2("04da388d-b6f9-48de-9fa0-a3bf23d22f0f"), PerformanceMeasure = "" },
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("bacddfc2-946a-40a5-91c7-ea172985d195"), Level = new Dtos.GuidObject2("a09bb753-faa2-45fe-b4b3-97de871e9562"), PerformanceMeasure = "" }
                        },
                        Person = new Dtos.GuidObject2("ed809943-eb26-42d0-9a95-d8db912a581f"),
                        Residency = new Dtos.GuidObject2("dc5331ff-eea2-4294-8d3c-3e48876fbf09"),
                        StudentStatus = new Dtos.GuidObject2("43ac15e4-3abe-4d7e-8f8e-53043c05d00c"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("c63c8b8b-484d-4c98-bee5-b83a7f4911f3"),
                            new Dtos.GuidObject2("59a99a2f-3402-4eb8-b96e-06d145237aa4")
                        },
                        Type = new Dtos.GuidObject2("cb7471c3-8426-418a-aeff-ce0898a5ab05")
                    },
                    new StudentAcademicPeriodProfiles(){ 
                        Id = "bf4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.NotSet, 
                        AcademicPeriod = new Dtos.GuidObject2("7f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("62e5ff61-2f5b-4d4f-8396-b671184bdbd8"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("2cb13ed2-87c0-43e6-8c64-90014246ab00"), Level = new Dtos.GuidObject2("1ec7bf68-020f-4cef-bd5b-9c9a9bc65002"), PerformanceMeasure = "" },
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("9777814a-3213-4866-b529-bd7ecaffc43c"), Level = new Dtos.GuidObject2("9719f395-455c-446e-a2c6-e76f7a595588"), PerformanceMeasure = "" }
                        },
                        Person = new Dtos.GuidObject2("6f11fcd7-40bf-4c24-8e97-602c363eb8cf"),
                        Residency = new Dtos.GuidObject2("378af5dc-85fa-4588-8987-9e6c90ffb8fe"),
                        StudentStatus = new Dtos.GuidObject2("2bb2aec4-3311-4050-aa96-fdcd62356079"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("1a6de707-cfe8-4ba3-b41d-2ff9e6800fe4"),
                            new Dtos.GuidObject2("beb84bb8-82f5-4b62-9de0-8a294558ecf3")
                        },
                        Type = new Dtos.GuidObject2("27bb2c50-707d-49ed-a9b4-a2915f7a5c7b")
                    },

                    new StudentAcademicPeriodProfiles(){ 
                        Id = "cf4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.NotSet, 
                        AcademicPeriod = new Dtos.GuidObject2("8f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("72e5ff61-2f5b-4d4f-8396-b671184bdbd8"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("1cb13ed2-87c0-43e6-8c64-90014246ab00"), Level = new Dtos.GuidObject2("2ec7bf68-020f-4cef-bd5b-9c9a9bc65002"), PerformanceMeasure = "" },
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("3777814a-3213-4866-b529-bd7ecaffc43c"), Level = new Dtos.GuidObject2("4719f395-455c-446e-a2c6-e76f7a595588"), PerformanceMeasure = "" }
                        },
                        Person = new Dtos.GuidObject2("1f11fcd7-40bf-4c24-8e97-602c363eb8cf"),
                        Residency = new Dtos.GuidObject2("478af5dc-85fa-4588-8987-9e6c90ffb8fe"),
                        StudentStatus = new Dtos.GuidObject2("3bb2aec4-3311-4050-aa96-fdcd62356079"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("2a6de707-cfe8-4ba3-b41d-2ff9e6800fe4"),
                            new Dtos.GuidObject2("ceb84bb8-82f5-4b62-9de0-8a294558ecf3")
                        },
                        Type = new Dtos.GuidObject2("37bb2c50-707d-49ed-a9b4-a2915f7a5c7b")
                    },


                    new StudentAcademicPeriodProfiles(){ 
                        Id = "df4d47eb-f06b-4add-b5bf-d9529742387a", 
                        AcademicLoad = Dtos.EnumProperties.AcademicLoad.NotSet, 
                        AcademicPeriod = new Dtos.GuidObject2("9f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        AcademicPeriodEnrollmentStatus = new Dtos.GuidObject2("82e5ff61-2f5b-4d4f-8396-b671184bdbd8"),
                        Measures = new List<Dtos.DtoProperties.PerformanceMeasureDtoProperty>()
                        {
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("5cb13ed2-87c0-43e6-8c64-90014246ab00"), Level = new Dtos.GuidObject2("6ec7bf68-020f-4cef-bd5b-9c9a9bc65002"), PerformanceMeasure = "" },
                            new Dtos.DtoProperties.PerformanceMeasureDtoProperty(){ Classification = new Dtos.GuidObject2("7777814a-3213-4866-b529-bd7ecaffc43c"), Level = new Dtos.GuidObject2("8719f395-455c-446e-a2c6-e76f7a595588"), PerformanceMeasure = "" }
                        },
                        Person = new Dtos.GuidObject2("2f11fcd7-40bf-4c24-8e97-602c363eb8cf"),
                        Residency = new Dtos.GuidObject2("578af5dc-85fa-4588-8987-9e6c90ffb8fe"),
                        StudentStatus = new Dtos.GuidObject2("4bb2aec4-3311-4050-aa96-fdcd62356079"),
                        Tags = new List<Dtos.GuidObject2>()
                        {
                            new Dtos.GuidObject2("3a6de707-cfe8-4ba3-b41d-2ff9e6800fe4"),
                            new Dtos.GuidObject2("deb84bb8-82f5-4b62-9de0-8a294558ecf3")
                        },
                        Type = new Dtos.GuidObject2("47bb2c50-707d-49ed-a9b4-a2915f7a5c7b")
                    }                    
                };
                #endregion
            }

        }
    }
}