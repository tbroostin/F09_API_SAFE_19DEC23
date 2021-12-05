// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionApplicationsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IAdmissionApplicationsService> admissionApplicationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            AdmissionApplicationsController admissionApplicationsController;
            List<Dtos.AdmissionApplication> admissionApplicationDtos;
            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                admissionApplicationServiceMock = new Mock<IAdmissionApplicationsService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                admissionApplicationDtos = BuildData();

                admissionApplicationsController = new AdmissionApplicationsController(admissionApplicationServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                admissionApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private List<Dtos.AdmissionApplication> BuildData()
            {
                List<Dtos.AdmissionApplication> admissionApplications = new List<Dtos.AdmissionApplication>() 
                {
                    new Dtos.AdmissionApplication()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        Owner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        Applicant = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Owner = new Dtos.GuidObject2("0cva17h3-er23-5796-cb9a-32f5tdh065yf")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Type = new Dtos.GuidObject2("b83022ee-ufhs-3idd-88b0-3837a050be4f"),
                        Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.AdmissionApplication()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136", 
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Type = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"), 
                        Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return admissionApplications;
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationsController = null;
                admissionApplicationDtos = null;
                admissionApplicationServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NoCache_True()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsAsync(offset, limit, true)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NoCache_False()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsAsync(offset, limit, false)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NullPage()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsAsync(offset, limit, true)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplicationsAsync(null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetById()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var admissionApplication = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuidAsync(id)).ReturnsAsync(admissionApplication);

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync(id);

                var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Applicant, actual.Applicant);
                Assert.AreEqual(expected.Type, actual.Type);
                Assert.AreEqual(expected.Owner, actual.Owner);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAll_Exception()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsAsync(offset, limit, false)).ThrowsAsync(new Exception());
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetById_Exception()
            {
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetById_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_DELETE_Not_Supported()
            {
                await admissionApplicationsController.DeleteAdmissionApplicationsAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsAsync(offset, limit, It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsAsync(offset, limit, It.IsAny<bool>())).Throws<PermissionsException>();
                await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_ArgumentNullException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsAsync(offset, limit, It.IsAny<bool>())).Throws<ArgumentNullException>();
                await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_ArgumentException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsAsync(offset, limit, It.IsAny<bool>())).Throws<ArgumentException>();
                await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsAsync(offset, limit, It.IsAny<bool>())).Throws<RepositoryException>();
                await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsAsync(offset, limit, It.IsAny<bool>())).Throws<IntegrationApiException>();
                await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_ArgumentNullException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_NoId_Exception()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("");
            }

            // Permissions tests

            //Get
            //Version 6
            //GetAdmissionApplicationsAsync

            //Example success 
            [TestMethod]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsAsync" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications });

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 5);
                admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationServiceMock.Setup(s => s.GetAdmissionApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                var resp = await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewApplications));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsAsync" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    //var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 5);
                    admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsAsync(offset, limit, It.IsAny<bool>())).Throws<PermissionsException>();
                    admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-applications."));
                    var resp = await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Get by Id 
            //Version 6
            //GetAdmissionApplicationsByGuidAsync

            //Example success
            [TestMethod] 
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var admissionApplication = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications });

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 5);
                admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuidAsync(id)).ReturnsAsync(admissionApplication);
                await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("1234");

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewApplications));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication>, int>(admissionApplicationDtos, 5);

                    admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                    admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-applications."));
                    var resp = await admissionApplicationsController.GetAdmissionApplicationsByGuidAsync("1234");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

        }

        [TestClass]
        public class GET_V11
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IAdmissionApplicationsService> admissionApplicationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            AdmissionApplicationsController admissionApplicationsController;
            List<Dtos.AdmissionApplication2> admissionApplicationDtos;
            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                admissionApplicationServiceMock = new Mock<IAdmissionApplicationsService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                admissionApplicationsController = new AdmissionApplicationsController(admissionApplicationServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                admissionApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private void BuildData()
            {
                admissionApplicationDtos = new List<Dtos.AdmissionApplication2>()
                {
                    new Dtos.AdmissionApplication2()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        //Applicant = new Dtos.DtoProperties.AdmissionApplicationStudentDtoProperty()
                        //{
                        //    Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956")
                        //},
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Owner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.AdmissionApplication2()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        //Applicant = new Dtos.DtoProperties.AdmissionApplicationStudentDtoProperty()
                        //{
                        //    Student = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                        //},
                        Applicant = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Owner = new Dtos.GuidObject2("0cva17h3-er23-5796-cb9a-32f5tdh065yf")
                    },
                    new Dtos.AdmissionApplication2()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        //Applicant = new Dtos.DtoProperties.AdmissionApplicationStudentDtoProperty()
                        //{
                        //    Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf")
                        //},
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Type = new Dtos.GuidObject2("b83022ee-ufhs-3idd-88b0-3837a050be4f"),
                        Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.AdmissionApplication2()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        //Applicant = new Dtos.DtoProperties.AdmissionApplicationStudentDtoProperty()
                        //{
                        //    Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf") },
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Type = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
                        Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationsController = null;
                admissionApplicationDtos = null;
                admissionApplicationServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NoCache_True()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications2Async(offset, limit, true)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication2>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NoCache_False()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications2Async(offset, limit, false)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication2>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NullPage()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications2Async(offset, limit, true)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications2Async(null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication2>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetById()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var admissionApplication = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid2Async(id)).ReturnsAsync(admissionApplication);

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async(id);

                var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Applicant, actual.Applicant);
                Assert.AreEqual(expected.Type, actual.Type);
                Assert.AreEqual(expected.Owner, actual.Owner);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAll_Exception()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications2Async(offset, limit, false)).ThrowsAsync(new Exception());
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetById_Exception()
            {
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetById_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_DELETE_Not_Supported()
            {
                await admissionApplicationsController.DeleteAdmissionApplicationsAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).Throws<PermissionsException>();
                await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_ArgumentNullException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).Throws<ArgumentNullException>();
                await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));
            }

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task AdmissionApplicationsController_GetAdmissionApplications_ArgumentNullException()
            //{
            //    admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).Throws<ArgumentNullException>();
            //    await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));
            //}

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).Throws<RepositoryException>();
                await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).Throws<IntegrationApiException>();
                await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).Throws<PermissionsException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_ArgumentNullException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).Throws<ArgumentNullException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).Throws<RepositoryException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).Throws<IntegrationApiException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_NoId_Exception()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).Throws<Exception>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("");
            }

            //Permissions Tests
            
            //Get
            //Version 11
            //GetAdmissionApplications2Async

            //Example success 
            [TestMethod]
            public async Task AdmissionApplicationsController_GetAdmissionApplications2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplications2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications });

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);
                admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).ReturnsAsync(tuple);
                var resp = await admissionApplicationsController.GetAdmissionApplications2Async(new Paging(limit, offset));

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewApplications));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplications2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);

                    admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications2Async(offset, limit, It.IsAny<bool>())).Throws<PermissionsException>();
                    admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-applications."));
                    var resp = await admissionApplicationsController.GetAdmissionApplicationsAsync(new Paging(limit, offset));
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Get by Id 
            //Version 11
            //GetAdmissionApplicationsByGuid2Async

            //Example success
            [TestMethod]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuid2Async_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var admissionApplication = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsByGuid2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications });

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);
                admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid2Async(id)).ReturnsAsync(admissionApplication);

                await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("1234");

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewApplications));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuid2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsByGuid2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    //var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);
                    admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).Throws<PermissionsException>();
                    admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))

                        .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-applications."));
                    var resp = await admissionApplicationsController.GetAdmissionApplicationsByGuid2Async("1234"); 
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }
        }

        [TestClass]

        public class AdmissionApplicationsControllerTests_V11_POST_PUT
        {

            #region DECLARATIONS
            public TestContext TestContext { get; set; }
            private AdmissionApplicationsController admissionApplicationsController;
            private Mock<IAdmissionApplicationsService> admissionApplicationsServiceMock;
            private AdmissionApplication2 admissionApplication2;
            private AdmissionApplication2 putadmissionApplication;
            private Mock<ILogger> loggerMock;

            string guid = "2a082180-b897-46f3-8435-df25caaca922";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                admissionApplicationsServiceMock = new Mock<IAdmissionApplicationsService>();

                loggerMock = new Mock<ILogger>();

                InitializeTestData();

                

                admissionApplicationsController = new AdmissionApplicationsController(admissionApplicationsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                admissionApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionApplicationsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };


                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ReturnsAsync(admissionApplication2);
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ReturnsAsync(putadmissionApplication);
                admissionApplicationsServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid2Async(It.IsAny<string>())).ReturnsAsync(putadmissionApplication);

                admissionApplicationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(putadmissionApplication));

            }
            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationsController = null;
                admissionApplicationsServiceMock = null;
                loggerMock = null;
                TestContext = null;
            }

            private void InitializeTestData()
            {
                admissionApplication2 = new AdmissionApplication2()
                {
                    Id = Guid.Empty.ToString(), Applicant = new GuidObject2("11182180-b897-46f3-8435-df25caaca922"), Type = new GuidObject2("22282180-b897-46f3-8435-df25caaca922"),
                    AcademicPeriod = new GuidObject2("33382180-b897-46f3-8435-df25caaca922"), Owner = new GuidObject2("55582180-b897-46f3-8435-df25caaca922"),
                    Source = new GuidObject2("44482180-b897-46f3-8435-df25caaca922"),
                    AdmissionPopulation = new GuidObject2("66682180-b897-46f3-8435-df25caaca922"),
                    Site = new GuidObject2("77782180-b897-46f3-8435-df25caaca922"),
                    ResidencyType = new GuidObject2("88882180-b897-46f3-8435-df25caaca922"),
                    Program = new GuidObject2("99982180-b897-46f3-8435-df25caaca922"),
                    Level = new GuidObject2("88882180-b897-46f3-8435-df25caaca111"),
                    School = new GuidObject2("88882180-b897-46f3-8435-df25caaca222"), Withdrawal = new Dtos.DtoProperties.AdmissionApplicationsWithdrawal2() { WithdrawalReason = new GuidObject2("22282180-b897-46f3-8435-df25caaca222"), WithdrawnOn = DateTime.Now, InstitutionAttended = new Dtos.DtoProperties.AdmissionApplicationInstitutionAttendedDtoProperty() { Id = "22282180-b897-46f3-8435-df25caaca333" } }

                };

                putadmissionApplication = new AdmissionApplication2()
                {
                    Id = "2a082180-b897-46f3-8435-df25caaca922",
                    Applicant = new GuidObject2("11182180-b897-46f3-8435-df25caaca922"),
                    Type = new GuidObject2("22282180-b897-46f3-8435-df25caaca922"),
                    AcademicPeriod = new GuidObject2("33382180-b897-46f3-8435-df25caaca922"),
                    Owner = new GuidObject2("55582180-b897-46f3-8435-df25caaca922"),
                    Source = new GuidObject2("44482180-b897-46f3-8435-df25caaca922"),
                    AdmissionPopulation = new GuidObject2("66682180-b897-46f3-8435-df25caaca922"),
                    Site = new GuidObject2("77782180-b897-46f3-8435-df25caaca922"),
                    ResidencyType = new GuidObject2("88882180-b897-46f3-8435-df25caaca922"),
                    Program = new GuidObject2("99982180-b897-46f3-8435-df25caaca922"),
                    Level = new GuidObject2("88882180-b897-46f3-8435-df25caaca111"),
                    School = new GuidObject2("88882180-b897-46f3-8435-df25caaca222"),
                    Withdrawal = new Dtos.DtoProperties.AdmissionApplicationsWithdrawal2() { WithdrawalReason = new GuidObject2("22282180-b897-46f3-8435-df25caaca222"), WithdrawnOn = DateTime.Now, InstitutionAttended = new Dtos.DtoProperties.AdmissionApplicationInstitutionAttendedDtoProperty() { Id = "22282180-b897-46f3-8435-df25caaca333" } }
                };
            }
        

            #endregion

            #region POST

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_Null() {
                await admissionApplicationsController.PostAdmissionApplications2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_Id_Empty()
            {
                admissionApplication2.Id = null;
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_Id_NotEmptyGuid()
            {
                admissionApplication2.Id = "11182180-b897-46f3-8435-df25caaca922";
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_KeyNotFoundException()
            {
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_PermissionsException()
            {
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_ArgumentException()
            {
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_RepositoryException()
            {
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_IntegrationApiException()
            {
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_ConfigurationException()
            {
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new ConfigurationException());
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async_Exception()
            {
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
            }

            [TestMethod]
            public async Task AdmissionApplicationController_PostAdmissionApplications2Async()
            {
              var result =  await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplication2.Id);
            }

            //Permissions Tests for post

            //Post
            //Version 11
            //PostAdmissionApplications2Async

            //Example success 
            [TestMethod]
            public async Task AdmissionApplicationsController_PostAdmissionApplications2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "PostAdmissionApplications2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateApplications);

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                //var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);
                admissionApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ReturnsAsync(admissionApplication2);
                var resp = await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_PostAdmissionApplications2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "PostAdmissionApplications2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    //var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);
                    admissionApplicationsServiceMock.Setup(x => x.CreateAdmissionApplicationAsync(It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    admissionApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).
                        Throws(new PermissionsException("User 'npuser' does not have permission to create admission-applications."));
                    var resp = await admissionApplicationsController.PostAdmissionApplications2Async(admissionApplication2);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }
            #endregion

            #region PUT

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_Null()
            {
                await admissionApplicationsController.PutAdmissionApplications2Async(guid,null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_Guid_Null()
            {
                await admissionApplicationsController.PutAdmissionApplications2Async(null, putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_Guid_Empty()
            {
                await admissionApplicationsController.PutAdmissionApplications2Async(Guid.Empty.ToString(), putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_Guid_NotEquals()
            {
                putadmissionApplication.Id = "2a082180-b897-46f3-8435-df25caaca111";
                await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_KeyNotFoundException()
            {
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_PermissionsException()
            {
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await admissionApplicationsController.PutAdmissionApplications2Async(guid,putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_ArgumentException()
            {
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await admissionApplicationsController.PutAdmissionApplications2Async(guid,putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_RepositoryException()
            {
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_putAdmissionApplications2Async_IntegrationApiException()
            {
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_ConfigurationException()
            {
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new ConfigurationException());
                await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async_Exception()
            {
                putadmissionApplication.Id = "";
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(),It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);
            }

            [TestMethod]
            public async Task AdmissionApplicationController_PutAdmissionApplications2Async()
            {
                var result = await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, putadmissionApplication.Id);
            }

            //Permissions Tests for put

            //Put
            //Version 11
            //PutAdmissionApplications2Async

            //Example success 
            [TestMethod]
            public async Task AdmissionApplicationsController_PutAdmissionApplications2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "PutAdmissionApplications2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateApplications);

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                //var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);
                admissionApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ReturnsAsync(putadmissionApplication);
                await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_PutAdmissionApplications2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "PutAdmissionApplications2Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    //var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication2>, int>(admissionApplicationDtos, 5);
                    admissionApplicationsServiceMock.Setup(x => x.UpdateAdmissionApplicationAsync(It.IsAny<string>(), It.IsAny<AdmissionApplication2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    admissionApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).
                        Throws(new PermissionsException("User 'npuser' does not have permission to create admission-applications."));
                    var resp = await admissionApplicationsController.PutAdmissionApplications2Async(guid, putadmissionApplication);

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            #endregion
        }

        [TestClass]
        public class GET_V16_0_0
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IAdmissionApplicationsService> admissionApplicationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            AdmissionApplicationsController admissionApplicationsController;
            List<Dtos.AdmissionApplication3> admissionApplicationDtos;
            int offset = 0;
            int limit = 200;

            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                admissionApplicationServiceMock = new Mock<IAdmissionApplicationsService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                admissionApplicationsController = new AdmissionApplicationsController(admissionApplicationServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                admissionApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private void BuildData()
            {
                admissionApplicationDtos = new List<Dtos.AdmissionApplication3>()
                {
                    new Dtos.AdmissionApplication3()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Owner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.AdmissionApplication3()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        Applicant = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        Owner = new Dtos.GuidObject2("0cva17h3-er23-5796-cb9a-32f5tdh065yf")
                    },
                    new Dtos.AdmissionApplication3()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Type = new Dtos.GuidObject2("b83022ee-ufhs-3idd-88b0-3837a050be4f"),
                        Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.AdmissionApplication3()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Applicant = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        Type = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
                        Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationsController = null;
                admissionApplicationDtos = null;
                admissionApplicationServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NoCache_True()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), true)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication3> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication3>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication3>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NoCache_False()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), false)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication3> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication3>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication3>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetAll_NullPage()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), true)).ReturnsAsync(tuple);
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications3Async(null, criteriaFilter, personFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionApplications.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.AdmissionApplication3> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication3>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.AdmissionApplication3>;


                Assert.AreEqual(admissionApplicationDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Applicant, actual.Applicant);
                    Assert.AreEqual(expected.Type, actual.Type);
                    Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task AdmissionApplicationsController_GetById()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var admissionApplication = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid3Async(id)).ReturnsAsync(admissionApplication);

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async(id);

                var expected = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Applicant, actual.Applicant);
                Assert.AreEqual(expected.Type, actual.Type);
                Assert.AreEqual(expected.Owner, actual.Owner);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAll_Exception()
            {
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 4);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), false)).ThrowsAsync(new Exception());
                var admissionApplications = await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetById_Exception()
            {
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetById_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_DELETE_Not_Supported()
            {
                await admissionApplicationsController.DeleteAdmissionApplicationsAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<bool>())).Throws<PermissionsException>();
                await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_ArgumentNullException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<bool>())).Throws<ArgumentNullException>();
                await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<bool>())).Throws<RepositoryException>();
                await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
                await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).Throws<PermissionsException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_ArgumentNullException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).Throws<ArgumentNullException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).Throws<RepositoryException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).Throws<IntegrationApiException>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuidAsync_NoId_Exception()
            {
                admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).Throws<Exception>();
                await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("");
            }

            //Permissions Tests

            //Get
            //Versions 16.1.0 / 16.0.0
            //GetAdmissionApplications3Async

            //Example success 
            [TestMethod]
            public async Task AdmissionApplicationsController_GetAdmissionApplications3Async_Permissions()
            {
                Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
                Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplications3Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications });

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 5);
                admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationServiceMock.Setup(s => s.GetAdmissionApplications3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                var resp = await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewApplications));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplications3Async_Invalid_Permissions()
            {
                Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
                Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplications3Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 5);

                    admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplications3Async(offset, limit, It.IsAny<Dtos.AdmissionApplication3>(), It.IsAny<Dtos.Filters.PersonFilterFilter2>(), It.IsAny<bool>())).Throws<PermissionsException>();
                    admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-applications."));
                    var resp = await admissionApplicationsController.GetAdmissionApplications3Async(new Paging(limit, offset), criteriaFilter, personFilter);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Get by Id 
            //Versions 16.1.0 / 16.0.0
            //GetAdmissionApplicationsByGuid3Async

            //Example success
            [TestMethod]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuid3Async_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var admissionApplication = admissionApplicationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
                Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsByGuid3Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewApplications, StudentPermissionCodes.UpdateApplications });

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 5);
                admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                admissionApplicationServiceMock.Setup(ci => ci.GetAdmissionApplicationsByGuid3Async(id)).ReturnsAsync(admissionApplication);
                var resp = await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("1234");

                Object filterObject;
                admissionApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewApplications));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateApplications));

            }

            //Example exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AdmissionApplicationsController_GetAdmissionApplicationsByGuid3Async_Invalid_Permissions()
            {
                Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
                Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetAdmissionApplicationsByGuid3Async" }
                };
                HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                admissionApplicationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = admissionApplicationsController.ControllerContext;
                var actionDescriptor = admissionApplicationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.AdmissionApplication3>, int>(admissionApplicationDtos, 5);

                    admissionApplicationServiceMock.Setup(x => x.GetAdmissionApplicationsByGuid3Async(It.IsAny<string>())).Throws<PermissionsException>();
                    admissionApplicationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-applications."));
                    var resp = await admissionApplicationsController.GetAdmissionApplicationsByGuid3Async("1234");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }
        }

        [TestClass]

        public class AdmissionApplicationSubmissionControllerTests
        {
            Mock<IAdmissionApplicationsService> admissionApplicationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            AdmissionApplicationsController admissionApplicationsController;
            Dtos.AdmissionApplication3 admissionApplicationDto;
            Dtos.AdmissionApplicationSubmission admissionApplicationSubmissionDto;
            Dtos.AdmissionApplicationSubmission admissionApplicationSubmission2Dto;
            public TestContext TestContext { get; set; }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                admissionApplicationServiceMock = new Mock<IAdmissionApplicationsService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                BuildDataAndMock();

                admissionApplicationsController = new AdmissionApplicationsController(admissionApplicationServiceMock.Object, loggerMock.Object);
                admissionApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                admissionApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                admissionApplicationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationsController = null;
                admissionApplicationDto = null;
                admissionApplicationSubmissionDto = null;
                admissionApplicationServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            #region Exceptions POST

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_NullRequestBody()
            {
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_Id_Null()
            {
                admissionApplicationSubmissionDto = new AdmissionApplicationSubmission()
                {
                    Id = ""
                };
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_ArgumentException()
            {
                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentException());
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_ConfigurationException()
            {
                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ConfigurationException());
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAdmissionApplicationsSubmissionsAsync_Exception()
            {
                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
            }

            #endregion POST


            #region Exceptions PUT

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_Null_UrlId()
            {
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_RequestBody_Null()
            {
                admissionApplicationSubmissionDto = new AdmissionApplicationSubmission()
                {
                    Id = ""
                };
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_KeyNotFoundException()
            {
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_PermissionsException()
            {
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_ArgumentException()
            {
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentException());
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_RepositoryException()
            {
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_IntegrationApiException()
            {
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_ConfigurationException()
            {
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ConfigurationException());
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_Exception()
            {
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAdmissionApplicationsSubmissionsAsync_UnsetEducGoalException()
            {
                admissionApplicationServiceMock.Setup(s => s.GetAdmissionApplicationsSubmissionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(admissionApplicationSubmission2Dto);
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
            }


            #endregion

            #region PUT/POST 

            [TestMethod]
            public async Task PutAdmissionApplicationsSubmissionsAsync()
            {
                var actual = await admissionApplicationsController.PutAdmissionApplicationsSubmissionsAsync("1234", admissionApplicationSubmissionDto);
                Assert.IsNotNull(actual);
            }

            [TestMethod]
            public async Task PostAdmissionApplicationsSubmissionsAsync()
            {
                var actual = await admissionApplicationsController.PostAdmissionApplicationsSubmissionsAsync(admissionApplicationSubmissionDto);
                Assert.IsNotNull(actual);
            }

            #endregion

            private void BuildDataAndMock()
            {
                admissionApplicationDto = new Dtos.AdmissionApplication3()
                {
                    Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                    Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                    Type = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                    Owner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),
                    ApplicationAcademicPrograms = new List<GuidObject2>()
                    {
                        new GuidObject2("e190d4b5-03b5-41aa-99b8-b8286717c955")
                    }
                };

                admissionApplicationSubmissionDto = new AdmissionApplicationSubmission()
                {
                    Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                    Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                    ApplicationAcademicPrograms = new List<ApplicationAcademicProgram>()
                    {
                        new ApplicationAcademicProgram()
                        {
                            ProgramOwner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),
                            Disciplines  = new List<AdmissionApplicationSubmissionDiscipline>()
                            {
                                new AdmissionApplicationSubmissionDiscipline()
                                {
                                    Discipline = new GuidObject2("447a6b81-109f-43ec-9055-de27cdfba323")
                                }
                            }
                        }
                    }
                };

                admissionApplicationSubmission2Dto = new AdmissionApplicationSubmission()
                {
                    Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                    Applicant = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                    ApplicationAcademicPrograms = new List<ApplicationAcademicProgram>()
                    {
                        new ApplicationAcademicProgram()
                        {
                            ProgramOwner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),
                            Disciplines  = new List<AdmissionApplicationSubmissionDiscipline>()
                            {
                                new AdmissionApplicationSubmissionDiscipline()
                                {
                                    Discipline = new GuidObject2("447a6b81-109f-43ec-9055-de27cdfba323")
                                }
                            }
                        }
                    },
                    EducationalGoal = new Dtos.GuidObject2("u430d4b5-03b5-41aa-99b8-b8286717c956")
                };

                admissionApplicationServiceMock.Setup(s => s.CreateAdmissionApplicationsSubmissionAsync(It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ReturnsAsync(admissionApplicationDto);
                admissionApplicationServiceMock.Setup(s => s.UpdateAdmissionApplicationsSubmissionAsync(It.IsAny<string>(), It.IsAny<AdmissionApplicationSubmission>(), It.IsAny<bool>()))
                    .ReturnsAsync(admissionApplicationDto);
            }
        }
    }
}
