//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class JobApplicationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IJobApplicationsService> jobApplicationsServiceMock;
        private Mock<ILogger> loggerMock;
        private JobApplicationsController jobApplicationsController;
        private IEnumerable<Domain.HumanResources.Entities.JobApplication> allJobapps;
        private List<Dtos.JobApplications> jobApplicationsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            jobApplicationsServiceMock = new Mock<IJobApplicationsService>();
            loggerMock = new Mock<ILogger>();
            jobApplicationsCollection = new List<Dtos.JobApplications>();

            allJobapps = new List<Domain.HumanResources.Entities.JobApplication>()
                {
                    new Domain.HumanResources.Entities.JobApplication("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"),
                    new Domain.HumanResources.Entities.JobApplication("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    new Domain.HumanResources.Entities.JobApplication("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                };

            foreach (var source in allJobapps)
            {
                var jobApplications = new Ellucian.Colleague.Dtos.JobApplications
                {
                    Id = source.Guid,
                    Person = new GuidObject2(source.PersonId)
                };
                jobApplicationsCollection.Add(jobApplications);
            }

            jobApplicationsController = new JobApplicationsController(jobApplicationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            jobApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            jobApplicationsController = null;
            allJobapps = null;
            jobApplicationsCollection = null;
            loggerMock = null;
            jobApplicationsServiceMock = null;
        }

        [TestMethod]
        public async Task JobApplicationsController_GetJobApplications_ValidateFields_Nocache()
        {
            jobApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            jobApplicationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var jobApplicationsTuple
                    = new Tuple<IEnumerable<JobApplications>, int>(jobApplicationsCollection, jobApplicationsCollection.Count);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(jobApplicationsTuple);

            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            var sourceContexts = (await jobApplicationsController.GetJobApplicationsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.JobApplications>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.JobApplications>;

            Assert.IsNotNull(results);
            Assert.AreEqual(jobApplicationsCollection.Count, results.Count());

            foreach (var actual in results)
            {
                var expected = jobApplicationsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            }
        }

        [TestMethod]
        public async Task JobApplicationsController_GetJobApplications_ValidateFields_Cache()
        {
            jobApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            jobApplicationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var jobApplicationsTuple
                    = new Tuple<IEnumerable<JobApplications>, int>(jobApplicationsCollection, jobApplicationsCollection.Count);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(jobApplicationsTuple);

            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            var sourceContexts = (await jobApplicationsController.GetJobApplicationsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.JobApplications>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.JobApplications>;

            Assert.IsNotNull(results);
            Assert.AreEqual(jobApplicationsCollection.Count, results.Count());

            foreach (var actual in results)
            {
                var expected = jobApplicationsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplications_KeyNotFoundException()
        {
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await jobApplicationsController.GetJobApplicationsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplications_PermissionsException()
        {
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await jobApplicationsController.GetJobApplicationsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplications_ArgumentException()
        {
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await jobApplicationsController.GetJobApplicationsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplications_RepositoryException()
        {
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await jobApplicationsController.GetJobApplicationsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplications_IntegrationApiException()
        {
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await jobApplicationsController.GetJobApplicationsAsync(paging);
        }

        [TestMethod]
        public async Task JobApplicationsController_GetJobApplicationsByGuidAsync_ValidateFields()
        {
            var expected = jobApplicationsCollection.FirstOrDefault();
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await jobApplicationsController.GetJobApplicationsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Person.Id, actual.Person.Id, "Person");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplications_Exception()
        {
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);

            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();
            await jobApplicationsController.GetJobApplicationsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsByGuidAsync_Exception()
        {
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await jobApplicationsController.GetJobApplicationsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsByGuid_KeyNotFoundException()
        {
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await jobApplicationsController.GetJobApplicationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsByGuid_PermissionsException()
        {
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await jobApplicationsController.GetJobApplicationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsByGuid_ArgumentException()
        {
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await jobApplicationsController.GetJobApplicationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsByGuid_RepositoryException()
        {
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await jobApplicationsController.GetJobApplicationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsByGuid_IntegrationApiException()
        {
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await jobApplicationsController.GetJobApplicationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsByGuid_Exception()
        {
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await jobApplicationsController.GetJobApplicationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_PostJobApplicationsAsync_Exception()
        {
            await jobApplicationsController.PostJobApplicationsAsync(jobApplicationsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_PutJobApplicationsAsync_Exception()
        {
            var sourceContext = jobApplicationsCollection.FirstOrDefault();
            await jobApplicationsController.PutJobApplicationsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_DeleteJobApplicationsAsync_Exception()
        {
            await jobApplicationsController.DeleteJobApplicationsAsync(jobApplicationsCollection.FirstOrDefault().Id);
        }

        //Permissions

        //Success
        // Get 10
        //GetJobApplicationsAsync

        [TestMethod]
        public async Task JobApplicationsController_GetJobApplicationsAsync_Permissions()
        {
            var jobApplicationsTuple= new Tuple<IEnumerable<JobApplications>, int>(jobApplicationsCollection, jobApplicationsCollection.Count);
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "JobApplications" },
                    { "action", "GetJobApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("job-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            jobApplicationsController.Request.SetRouteData(data);
            jobApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewJobApplications);

            var controllerContext = jobApplicationsController.ControllerContext;
            var actionDescriptor = jobApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.JobApplications>, int>(jobApplicationsCollection, 4);
            jobApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(jobApplicationsTuple);
            var sourceContexts = (await jobApplicationsController.GetJobApplicationsAsync(paging));

            Object filterObject;
            jobApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewJobApplications));
        }

        //Exception
        //Get 10
        //GetJobApplicationsAsync

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetJobApplicationsAsync_Invalid_Permissions()
        {
            Paging paging = new Paging(jobApplicationsCollection.Count(), 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "JobApplications" },
                    { "action", "GetJobApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("job-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            jobApplicationsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = jobApplicationsController.ControllerContext;
            var actionDescriptor = jobApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.JobApplications>, int>(jobApplicationsCollection, 4);


                jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsAsync(It.IsAny<int>(),It.IsAny<int>(), It.IsAny<bool>())).Throws<PermissionsException>();
                jobApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view job-applications."));
                await jobApplicationsController.GetJobApplicationsAsync(paging);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Success
        // Get By Id 10
        //GetJobApplicationsByGuidAsync

        [TestMethod]
        public async Task JobApplications_GetJobApplicationsByGuidAsync_Permissions()
        {
            var expected = jobApplicationsCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "JobApplications" },
                    { "action", "GetJobApplicationsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("job-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            jobApplicationsController.Request.SetRouteData(data);
            jobApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewJobApplications);

            var controllerContext = jobApplicationsController.ControllerContext;
            var actionDescriptor = jobApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.JobApplications>, int>(jobApplicationsCollection, 4);
            jobApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(expected.Id)).ReturnsAsync(expected);
            var actual = await jobApplicationsController.GetJobApplicationsByGuidAsync(expected.Id);

            Object filterObject;
            jobApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewJobApplications));
        }

        //Exception
        //Get By Id 10
        //GetExternalEmploymentsByGuidAsync

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationsController_GetExternalEmploymentsByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "JobApplications" },
                    { "action", "GetExternalEmploymentsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("job-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            jobApplicationsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = jobApplicationsController.ControllerContext;
            var actionDescriptor = jobApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.JobApplications>, int>(jobApplicationsCollection, 4);

                jobApplicationsServiceMock.Setup(x => x.GetJobApplicationsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                jobApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view job-applications."));
                await jobApplicationsController.GetJobApplicationsByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


    }
}