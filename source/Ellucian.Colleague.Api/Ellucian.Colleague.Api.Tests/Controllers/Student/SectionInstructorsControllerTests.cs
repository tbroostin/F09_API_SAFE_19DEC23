//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class SectionInstructorsControllerTests_V10
    {
        [TestClass]
        public class SectionInstructorsControllerTests_GET
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<ISectionInstructorsService> serviceMock;
            private Mock<ILogger> loggerMock;

            private SectionInstructorsController controller;
            private int offset = 0, limit = 10;

            private Tuple<IEnumerable<Dtos.SectionInstructors>, int> tupleResult;

            private List<Dtos.SectionInstructors> collection;

            private Dtos.SectionInstructors sectionInstructor;

            private QueryStringFilter criteria = new QueryStringFilter("criteria", "");

            private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                serviceMock = new Mock<ISectionInstructorsService>();
                loggerMock = new Mock<ILogger>();

                InitializeTestData();

                controller = new SectionInstructorsController(serviceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") } };

                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                var filterGroupName = "criteria";
                controller.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.SectionInstructors()
                      {
                          Instructor = new GuidObject2("1"),
                          Section = new GuidObject2("2"),
                          InstructionalEvents = new List<GuidObject2>() { new GuidObject2() { Id = "3" } }
                      });
            }

            [TestCleanup]
            public void Cleanup()
            {
                controller = null;
                collection = null;
                loggerMock = null;
                serviceMock = null;
                tupleResult = null;
            }

            private void InitializeTestData()
            {
                sectionInstructor = new SectionInstructors() { Id = "1" };
                collection = new List<SectionInstructors>() { sectionInstructor };
                tupleResult = new Tuple<IEnumerable<SectionInstructors>, int>(collection, collection.Count);
            }

            #endregion

            [TestMethod]
            public async Task SectionInstructorsController_GetSectionInstructors_ValidateFields_Nocache()
            {
                controller.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false, Public = true };

                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false)).ReturnsAsync(tupleResult);

                var results = await controller.GetSectionInstructorsAsync(new Paging(limit, offset), criteria);

                Assert.IsNotNull(results);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.SectionInstructors>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionInstructors>;

                Assert.AreEqual(collection.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = collection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                }
            }

            [TestMethod]
            public async Task SectionInstructorsController_GetSectionInstructors_ValidateFields_Cache()
            {
                controller.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, Public = true };

                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), true)).ReturnsAsync(tupleResult);

                var results = await controller.GetSectionInstructorsAsync(new Paging(limit, offset), new QueryStringFilter("criteria",""));

                Assert.IsNotNull(results);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.SectionInstructors>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionInstructors>;

                Assert.AreEqual(collection.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = collection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructors_KeyNotFoundException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false))
                           .ThrowsAsync(new KeyNotFoundException());

                await controller.GetSectionInstructorsAsync(new Paging(limit, offset), criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructors_PermissionsException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false))
                           .ThrowsAsync(new PermissionsException());

                await controller.GetSectionInstructorsAsync(new Paging(limit, offset), criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructors_ArgumentException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false))
                           .ThrowsAsync(new ArgumentException());

                await controller.GetSectionInstructorsAsync(new Paging(limit, offset), criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructors_RepositoryException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false))
                           .ThrowsAsync(new RepositoryException());

                await controller.GetSectionInstructorsAsync(new Paging(limit, offset), criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructors_IntegrationApiException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false))
                           .ThrowsAsync(new IntegrationApiException());

                await controller.GetSectionInstructorsAsync(new Paging(limit, offset), criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructors_Exception()
            {
                criteria = new QueryStringFilter("criteria", "");

                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false))
                           .ThrowsAsync(new Exception());

                await controller.GetSectionInstructorsAsync(null, criteria);
            }

            [TestMethod]
            public async Task SectionInstructorsController_GetSectionInstructors()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), false))
                           .ReturnsAsync(tupleResult);

                var result = await controller.GetSectionInstructorsAsync(null, criteria);

                Assert.IsNotNull(result);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.SectionInstructors>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionInstructors>;

                Assert.AreEqual(collection.Count, actuals.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructorsByGuidAsync_IntegrationApiException_Guid_Null()
            {
                await controller.GetSectionInstructorsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructorsByGuidAsync_KeyNotFoundException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await controller.GetSectionInstructorsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructorsByGuidAsync_PermissionsException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await controller.GetSectionInstructorsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructorsByGuidAsync_ArgumentException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                await controller.GetSectionInstructorsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructorsByGuidAsync_RepositoryException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await controller.GetSectionInstructorsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructorsByGuidAsync_IntegrationApiException()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                await controller.GetSectionInstructorsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionInstructorsByGuidAsync_Exception()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                await controller.GetSectionInstructorsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task SectionInstructorsController_GetSectionInstructorsByGuidAsync()
            {
                serviceMock.Setup(x => x.GetSectionInstructorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionInstructor);

                var result = await controller.GetSectionInstructorsByGuidAsync(guid);

                Assert.IsNotNull(result);
            }

        }

        [TestClass]
        public class SectionInstructorsControllerTests_POST
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<ISectionInstructorsService> serviceMock;
            private Mock<ILogger> loggerMock;

            private SectionInstructorsController controller;

            private Dtos.SectionInstructors sectionInstructor;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                serviceMock = new Mock<ISectionInstructorsService>();
                loggerMock = new Mock<ILogger>();

                InitializeTestData();

                controller = new SectionInstructorsController(serviceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") } };

                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                controller = null;
                loggerMock = null;
                serviceMock = null;
            }

            private void InitializeTestData()
            {
                sectionInstructor = new SectionInstructors() { Id = "1" };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostSectionInstructorsAsync_IntegrationApiException_SectionInstructors_Null()
            {
                await controller.PostSectionInstructorsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostSectionInstructorsAsync_IntegrationApiException_SectionInstructors_Id_Null()
            {
                sectionInstructor.Id = null;

                await controller.PostSectionInstructorsAsync(sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostSectionInstructorsAsync_PermissionsException()
            {
                serviceMock.Setup(x => x.CreateSectionInstructorsAsync(It.IsAny<SectionInstructors>())).ThrowsAsync(new PermissionsException());
                await controller.PostSectionInstructorsAsync(sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostSectionInstructorsAsync_ArgumentException()
            {
                serviceMock.Setup(x => x.CreateSectionInstructorsAsync(It.IsAny<SectionInstructors>())).ThrowsAsync(new ArgumentException());
                await controller.PostSectionInstructorsAsync(sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostSectionInstructorsAsync_RepositoryException()
            {
                serviceMock.Setup(x => x.CreateSectionInstructorsAsync(It.IsAny<SectionInstructors>())).ThrowsAsync(new RepositoryException());
                await controller.PostSectionInstructorsAsync(sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostSectionInstructorsAsync_IntegrationApiException()
            {
                serviceMock.Setup(x => x.CreateSectionInstructorsAsync(It.IsAny<SectionInstructors>())).ThrowsAsync(new IntegrationApiException());
                await controller.PostSectionInstructorsAsync(sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostSectionInstructorsAsync_Exception()
            {
                serviceMock.Setup(x => x.CreateSectionInstructorsAsync(It.IsAny<SectionInstructors>())).ThrowsAsync(new Exception());
                await controller.PostSectionInstructorsAsync(sectionInstructor);
            }

            [TestMethod]
            public async Task SectionInstructorsController_PostSectionInstructorsAsync()
            {
                serviceMock.Setup(x => x.CreateSectionInstructorsAsync(It.IsAny<SectionInstructors>())).ReturnsAsync(sectionInstructor);
                var result = await controller.PostSectionInstructorsAsync(sectionInstructor);

                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class SectionInstructorsControllerTests_PUT
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<ISectionInstructorsService> serviceMock;
            private Mock<ILogger> loggerMock;

            private SectionInstructorsController controller;

            private Dtos.SectionInstructors sectionInstructor;

            private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                serviceMock = new Mock<ISectionInstructorsService>();
                loggerMock = new Mock<ILogger>();

                InitializeTestData();

                controller = new SectionInstructorsController(serviceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") } };

                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                controller.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(sectionInstructor));
            }

            [TestCleanup]
            public void Cleanup()
            {
                controller = null;
                loggerMock = null;
                serviceMock = null;
            }

            private void InitializeTestData()
            {
                sectionInstructor = new SectionInstructors() { Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc" };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionInstructorsAsync_IntegrationApiException_Guid_Null()
            {
                await controller.PutSectionInstructorsAsync(null, sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PutSectionInstructorsAsync_ArgumentNullException_SectionInstructors_Null()
            {
                await controller.PutSectionInstructorsAsync(guid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionInstructorsAsync_IntegrationApiException_SectionInstructors_Id_And_Guid_NotSame()
            {
                sectionInstructor.Id = "1";

                await controller.PutSectionInstructorsAsync(guid, sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PutSectionInstructorsAsync_IntegrationApiException_Guid_Empty()
            {
                sectionInstructor.Id = null;
                await controller.PutSectionInstructorsAsync(Guid.Empty.ToString(), sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionInstructorsAsync_PermissionsException()
            {
                serviceMock.Setup(s => s.UpdateSectionInstructorsAsync(It.IsAny<string>(), It.IsAny<Dtos.SectionInstructors>())).ThrowsAsync(new PermissionsException());
                await controller.PutSectionInstructorsAsync(guid, sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionInstructorsAsync_ArgumentException()
            {
                serviceMock.Setup(s => s.UpdateSectionInstructorsAsync(It.IsAny<string>(), It.IsAny<Dtos.SectionInstructors>())).ThrowsAsync(new ArgumentException());
                await controller.PutSectionInstructorsAsync(guid, sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionInstructorsAsync_RepositoryException()
            {
                serviceMock.Setup(s => s.UpdateSectionInstructorsAsync(It.IsAny<string>(), It.IsAny<Dtos.SectionInstructors>())).ThrowsAsync(new RepositoryException());
                await controller.PutSectionInstructorsAsync(guid, sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionInstructorsAsync_IntegrationApiException()
            {
                serviceMock.Setup(s => s.UpdateSectionInstructorsAsync(It.IsAny<string>(), It.IsAny<Dtos.SectionInstructors>())).ThrowsAsync(new IntegrationApiException());
                await controller.PutSectionInstructorsAsync(guid, sectionInstructor);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionInstructorsAsync_Exception()
            {
                serviceMock.Setup(s => s.UpdateSectionInstructorsAsync(It.IsAny<string>(), It.IsAny<Dtos.SectionInstructors>())).ThrowsAsync(new Exception());
                await controller.PutSectionInstructorsAsync(guid, sectionInstructor);
            }

            [TestMethod]
            public async Task SectionInstructorsController_PutSectionInstructorsAsync()
            {
                serviceMock.Setup(s => s.UpdateSectionInstructorsAsync(It.IsAny<string>(), It.IsAny<Dtos.SectionInstructors>())).ReturnsAsync(sectionInstructor);
                serviceMock.Setup(s => s.GetSectionInstructorsByGuidAsync(sectionInstructor.Id)).ReturnsAsync(sectionInstructor);
                var result = await controller.PutSectionInstructorsAsync(guid, sectionInstructor);

                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class SectionInstructorsControllerTests_DELETE
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<ISectionInstructorsService> serviceMock;
            private Mock<ILogger> loggerMock;

            private SectionInstructorsController controller;

            private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                serviceMock = new Mock<ISectionInstructorsService>();
                loggerMock = new Mock<ILogger>();

                controller = new SectionInstructorsController(serviceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") } };

                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                controller = null;
                loggerMock = null;
                serviceMock = null;
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeleteSectionInstructorsAsync_PermissionsException()
            {
                serviceMock.Setup(s => s.DeleteSectionInstructorsAsync(It.IsAny<string>())).Throws(new PermissionsException());
                await controller.DeleteSectionInstructorsAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeleteSectionInstructorsAsync_ArgumentException()
            {
                serviceMock.Setup(s => s.DeleteSectionInstructorsAsync(It.IsAny<string>())).Throws(new ArgumentException());
                await controller.DeleteSectionInstructorsAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeleteSectionInstructorsAsync_RepositoryException()
            {
                serviceMock.Setup(s => s.DeleteSectionInstructorsAsync(It.IsAny<string>())).Throws(new RepositoryException());
                await controller.DeleteSectionInstructorsAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeleteSectionInstructorsAsync_IntegrationApiException()
            {
                serviceMock.Setup(s => s.DeleteSectionInstructorsAsync(It.IsAny<string>())).Throws(new IntegrationApiException());
                await controller.DeleteSectionInstructorsAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeleteSectionInstructorsAsync_Exception()
            {
                serviceMock.Setup(s => s.DeleteSectionInstructorsAsync(It.IsAny<string>())).Throws(new Exception());
                await controller.DeleteSectionInstructorsAsync(guid);
            }

            [TestMethod]
            public async Task SectionInstructorsController_DeleteSectionInstructorsAsync()
            {
                serviceMock.Setup(s => s.DeleteSectionInstructorsAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
                var result = await controller.DeleteSectionInstructorsAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }
    }
}