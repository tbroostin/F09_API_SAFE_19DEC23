//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Models;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student;
using System.Collections;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentSectionWaitlistsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        ///
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

        private Mock<IStudentSectionWaitlistsService> studentSectionWaitlistsServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentSectionWaitlistsController studentSectionWaitlistsController;
        private IEnumerable<StudentSectionWaitlist> studentSectionWaitlistsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Tuple<IEnumerable<StudentSectionWaitlist>, int> repodata;
        private Paging page;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentSectionWaitlistsServiceMock = new Mock<IStudentSectionWaitlistsService>();
            loggerMock = new Mock<ILogger>();
            Paging page = new Paging(1, 3);

            studentSectionWaitlistsCollection = new List<StudentSectionWaitlist>()
            {
                new StudentSectionWaitlist()
                {
                    Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    Person = new StudentSectionWaitlistsPersonDtoProperty(){personId = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"},
                    Section = new StudentSectionWaitlistsSectionDtoProperty(){sectionId = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"},
                    Priority = 1
                },
                new Ellucian.Colleague.Dtos.StudentSectionWaitlist()
                {
                    Id = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                    Person = new StudentSectionWaitlistsPersonDtoProperty(){personId = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"},
                    Section = new StudentSectionWaitlistsSectionDtoProperty(){sectionId = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"},
                    Priority = 2
                },
                new Ellucian.Colleague.Dtos.StudentSectionWaitlist()
                {
                    Id = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                    Person = new StudentSectionWaitlistsPersonDtoProperty(){personId = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    Section = new StudentSectionWaitlistsSectionDtoProperty(){sectionId = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    Priority = 3
                }
            };
            repodata = new Tuple<IEnumerable<StudentSectionWaitlist>, int>(studentSectionWaitlistsCollection, 3);




            studentSectionWaitlistsController = new StudentSectionWaitlistsController(studentSectionWaitlistsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
                {
                    RequestUri = new Uri("https://test.com/something?something=somethingelse")
                }
            };
            studentSectionWaitlistsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentSectionWaitlistsController = null;
            studentSectionWaitlistsCollection = null;
            loggerMock = null;
            studentSectionWaitlistsServiceMock = null;
            page = null;
        }

        [TestMethod]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_ValidateFields_Nocache_true()
        {
            studentSectionWaitlistsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, Public = true };

            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(repodata);


            var results = await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentSectionWaitlist> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentSectionWaitlist>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.StudentSectionWaitlist>;

            Assert.AreEqual(studentSectionWaitlistsCollection.Count(), actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = studentSectionWaitlistsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Person.personId, actual.Person.personId);
                Assert.AreEqual(expected.Section.sectionId, actual.Section.sectionId);
                Assert.AreEqual(expected.Priority, actual.Priority);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_KeyNotFoundException()
        {
            //
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(1, 1, false))
                .Throws<KeyNotFoundException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(new Paging(1, 1));
        }


        [TestMethod]
        public async Task GradeSchemeController_GetGradeScheme_ValidateFields_Nocache_false()
        {
            studentSectionWaitlistsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(repodata);

            var sourceContexts = await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);

            int srcCnt = studentSectionWaitlistsCollection.Count();

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentSectionWaitlist> result = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentSectionWaitlist>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.StudentSectionWaitlist>;


            Assert.AreEqual(studentSectionWaitlistsCollection.Count(), result.Count());
            for (var i = 0; i < result.Count(); i++)
            {
                var expected = studentSectionWaitlistsCollection.ElementAt(i);
                var actual = result.ElementAt(i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Person.personId, actual.Person.personId, "personId, Index=" + i.ToString());
                Assert.AreEqual(expected.Section.sectionId, actual.Section.sectionId, "sectionId, Index=" + i.ToString());
                Assert.AreEqual(expected.Priority, actual.Priority, "Priority, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentSectionWaitlists" },
                { "action", "GetStudentSectionWaitlistsAsync" }
            };
            HttpRoute route = new HttpRoute("student-section-waitlists", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentSectionWaitlistsController.Request.SetRouteData(data);
            studentSectionWaitlistsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentSectionWaitlist });

            var controllerContext = studentSectionWaitlistsController.ControllerContext;
            var actionDescriptor = studentSectionWaitlistsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentSectionWaitlistsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                   .Returns(true);
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(repodata);
            var resp = await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);

            Object filterObject;
            studentSectionWaitlistsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentSectionWaitlist));

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentSectionWaitlists" },
                { "action", "GetStudentSectionWaitlistsAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentSectionWaitlistsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentSectionWaitlistsController.ControllerContext;
            var actionDescriptor = studentSectionWaitlistsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentSectionWaitlistsServiceMock.Setup(s => s.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(repodata);
                studentSectionWaitlistsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User is not authorized to view student-section-waitlists."));
                var resp = await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_PermissionsException()
        {

            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_ArgumentException()
        {

            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_RepositoryException()
        {

            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);
        }        
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_IntegrationApiException()
        {

            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);
        }
    


        [TestMethod]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuidAsync_ValidateFields()
        {
            var expected = studentSectionWaitlistsCollection.FirstOrDefault();
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Person.personId, actual.Person.personId, "Personid");
            Assert.AreEqual(expected.Section.sectionId, actual.Section.sectionId, "Sectionid");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlists_Exception()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuidAsync_Exception()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuid_KeyNotFoundException()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuid_PermissionsException()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuid_ArgumentException()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuid_RepositoryException()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuid_IntegrationApiException()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_GetStudentSectionWaitlistsByGuid_Exception()
        {
            studentSectionWaitlistsServiceMock.Setup(x => x.GetStudentSectionWaitlistsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await studentSectionWaitlistsController.GetStudentSectionWaitlistsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_PostStudentSectionWaitlistsAsync_Exception()
        {
            await studentSectionWaitlistsController.PostStudentSectionWaitlistsAsync(studentSectionWaitlistsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_PutStudentSectionWaitlistsAsync_Exception()
        {
            var sourceContext = studentSectionWaitlistsCollection.FirstOrDefault();
            await studentSectionWaitlistsController.PutStudentSectionWaitlistsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentSectionWaitlistsController_DeleteStudentSectionWaitlistsAsync_Exception()
        {
            await studentSectionWaitlistsController.DeleteStudentSectionWaitlistsAsync(studentSectionWaitlistsCollection.FirstOrDefault().Id);
        }
    }
}