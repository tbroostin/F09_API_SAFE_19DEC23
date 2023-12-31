﻿// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Adapters;
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentFinancialAidAwardsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IStudentFinancialAidAwardService> studentFinancialAidAwardServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            StudentFinancialAidAwardsController studentFinancialAidAwardsController;
            List<Dtos.StudentFinancialAidAward> studentFinancialAidAwardDtos;
            List<Dtos.StudentFinancialAidAward2> studentFinancialAidAward2Dtos;
            Dtos.StudentFinancialAidAward2 criteria = new Dtos.StudentFinancialAidAward2();

            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                studentFinancialAidAwardServiceMock = new Mock<IStudentFinancialAidAwardService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                studentFinancialAidAwardDtos = BuildData();
                studentFinancialAidAward2Dtos = BuildData2();

                studentFinancialAidAwardsController = new StudentFinancialAidAwardsController(studentFinancialAidAwardServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                studentFinancialAidAwardsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private List<Dtos.StudentFinancialAidAward> BuildData()
            {
                List<Dtos.StudentFinancialAidAward> studentFinancialAidAwards = new List<Dtos.StudentFinancialAidAward>() 
                {
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        AidYear = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                    },
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",   
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        AidYear = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        AwardFund = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"), 
                        AidYear = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return studentFinancialAidAwards;
            }

            private List<Dtos.StudentFinancialAidAward2> BuildData2()
            {
                List<Dtos.StudentFinancialAidAward2> studentFinancialAidAwards2 = new List<Dtos.StudentFinancialAidAward2>() 
                {
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        AidYear = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                    },
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",   
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        AidYear = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        AwardFund = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"), 
                        AidYear = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return studentFinancialAidAwards2;
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentFinancialAidAwardsController = null;
                studentFinancialAidAwardDtos = null;
                studentFinancialAidAward2Dtos = null;
                studentFinancialAidAwardServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll_NoCache_True()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, true, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward>;


                Assert.AreEqual(studentFinancialAidAwardDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll_NoCache_False()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward>;


                Assert.AreEqual(studentFinancialAidAwardDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll_NullPage()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, true, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward>;


                Assert.AreEqual(studentFinancialAidAwardDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetById()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(id, false)).ReturnsAsync(studentFinancialAidAward);

                var actual = await studentFinancialAidAwardsController.GetByIdAsync(id);

                var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
                Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                Assert.AreEqual(expected.Student, actual.Student);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll_PermissionException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, false)).ThrowsAsync(new PermissionsException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll_ArgumentException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, false)).ThrowsAsync(new ArgumentException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll_RepositoryException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, false)).ThrowsAsync(new RepositoryException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll_Exception()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, false)).ThrowsAsync(new Exception());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById_PermissionsException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actual = await studentFinancialAidAwardsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById_RepositoryException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actual = await studentFinancialAidAwardsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById_ArgumentException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actual = await studentFinancialAidAwardsController.GetByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById_Exception()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actual = await studentFinancialAidAwardsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById_KeyNotFoundException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await studentFinancialAidAwardsController.GetByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_PUT_Not_Supported()
            {
                var actual = await studentFinancialAidAwardsController.UpdateAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentFinancialAidAward2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_POST_Not_Supported()
            {
                var actual = await studentFinancialAidAwardsController.CreateAsync(It.IsAny<Dtos.StudentFinancialAidAward2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_DELETE_Not_Supported()
            {
                await studentFinancialAidAwardsController.DeleteAsync(It.IsAny<string>());
            }

            //GET by id v7
            //Successful
            //GetByIdAsync
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetByIdAsync_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetByIdAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(id, false)).ReturnsAsync(studentFinancialAidAward);
                var actual = await studentFinancialAidAwardsController.GetByIdAsync(id);

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentFinancialAidAwards));


            }

            //GET by id v7
            //Exception
            //GetByIdAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetByIdAsync_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetByIdAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var actual = await studentFinancialAidAwardsController.GetByIdAsync("ds");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v7
            //Successful
            //GetAsync
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentFinancialAidAwards));


            }

            //GET v7
            //Exception
            //GetAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAsync_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, false)).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetAsync(new Paging(limit, offset));
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            //Restricted

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll_NoCache_True()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, true, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward>;


                Assert.AreEqual(studentFinancialAidAwardDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll_NoCache_False()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward>;


                Assert.AreEqual(studentFinancialAidAwardDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll_NullPage()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, true, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward>;


                Assert.AreEqual(studentFinancialAidAwardDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(id, true)).ReturnsAsync(studentFinancialAidAward);

                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync(id);

                var expected = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
                Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                Assert.AreEqual(expected.Student, actual.Student);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll_PermissionException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, true)).ThrowsAsync(new PermissionsException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll_ArgumentException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, true)).ThrowsAsync(new ArgumentException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll_RepositoryException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, true)).ThrowsAsync(new RepositoryException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll_Exception()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, true)).ThrowsAsync(new Exception());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById_PermissionsException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById_RepositoryException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById_ArgumentException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById_Exception()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById_KeyNotFoundException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync("ds");
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll2_NoCache_True()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny< Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), true, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll3_NoCache_True()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), true, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll2_NoCache_False()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny< Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll3_NoCache_False()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetAll2_NullPage()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny< Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), true, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(null, It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetById2()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(id, false, false)).ReturnsAsync(studentFinancialAidAward);

                var actual = await studentFinancialAidAwardsController.GetById2Async(id);

                var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
                Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                Assert.AreEqual(expected.Student, actual.Student);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll2_ArgumentException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new ArgumentException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll2_RepositoryException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new RepositoryException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll2_Exception()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new Exception());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll3_PermissionsException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new PermissionsException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll3_ArgumentException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new ArgumentException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll3_RepositoryException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new RepositoryException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetAll3_Exception()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new Exception());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById2_PermissionsException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actual = await studentFinancialAidAwardsController.GetById2Async("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById2_RepositoryException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actual = await studentFinancialAidAwardsController.GetById2Async("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById2_ArgumentException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actual = await studentFinancialAidAwardsController.GetById2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById2_Exception()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actual = await studentFinancialAidAwardsController.GetById2Async("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById2_KeyNotFoundException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await studentFinancialAidAwardsController.GetById2Async("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_PUT2_Not_Supported()
            {
                var actual = await studentFinancialAidAwardsController.UpdateAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentFinancialAidAward2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_POST2_Not_Supported()
            {
                var actual = await studentFinancialAidAwardsController.CreateAsync(It.IsAny<Dtos.StudentFinancialAidAward2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_DELETE2_Not_Supported()
            {
                await studentFinancialAidAwardsController.DeleteAsync(It.IsAny<string>());
            }


            //GET by id v11.1.0 v 11
            //Successful
            //GetById2Async
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetById2Async_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetById2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(id, false, false)).ReturnsAsync(studentFinancialAidAward);
                var actual = await studentFinancialAidAwardsController.GetById2Async(id);

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentFinancialAidAwards));


            }

            //GET by id v11.1.0 v 11
            //Exception
            //GetById2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetById2Async_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetById2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var actual = await studentFinancialAidAwardsController.GetById2Async("ds");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v11.1.0
            //Successful
            //Get3Async
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_Get3Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "Get3Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentFinancialAidAwards));


            }

            //GET v11.1.0
            //Exception
            //Get3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_Get3Async_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "Get3Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v11
            //Successful
            //Get2Async
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_Get2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "Get2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentFinancialAidAwards));


            }

            //GET v11
            //Exception
            //Get2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_Get2Async_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "Get2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, false)).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var studentFinancialAidAwards = await studentFinancialAidAwardsController.Get2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v11
            //Successful
            //GetRestrictedByIdAsync
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestrictedByIdAsync_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAwardDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestrictedByIdAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(id, true)).ReturnsAsync(studentFinancialAidAward);
                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync(id);

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentFinancialAidAwards));


            }

            //GET v11
            //Exception
            //GetRestrictedByIdAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedByIdAsync_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestrictedByIdAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync("ds");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v7
            //Successful
            //GetRestrictedAsync
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestrictedAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentFinancialAidAwards));


            }

            //GET v7
            //Exception
            //GetRestrictedAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAsync_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestrictedAsync" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, 4);

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.GetAsync(offset, limit, false, true)).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                                            .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Restricted

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_NoCache_True()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny< Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), true, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_NoCache_False()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny< Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_NullPage()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny< Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), true, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(null, It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await studentFinancialAidAwards.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentFinancialAidAward2> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAward2>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.StudentFinancialAidAward2>;


                Assert.AreEqual(studentFinancialAidAward2Dtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear, actual.AidYear);
                    Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                    Assert.AreEqual(expected.Student, actual.Student);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_PermissionException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny< Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true))
                    .ThrowsAsync(new PermissionsException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_ArgumentException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true))
                    .ThrowsAsync(new ArgumentException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_RepositoryException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true))
                    .ThrowsAsync(new RepositoryException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_Exception()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true))
                    .ThrowsAsync(new Exception());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(id, true, false)).ReturnsAsync(studentFinancialAidAward);

                var actual = await studentFinancialAidAwardsController.GetRestrictedById2Async(id);

                var expected = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
                Assert.AreEqual(expected.AwardFund, actual.AwardFund);
                Assert.AreEqual(expected.Student, actual.Student);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll2_PermissionException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, criteria, It.IsAny<string>(), false, true)).ThrowsAsync(new PermissionsException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll2_ArgumentException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ThrowsAsync(new ArgumentException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll2_RepositoryException()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ThrowsAsync(new RepositoryException());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedAll2_Exception()
            {
                studentFinancialAidAwardsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ThrowsAsync(new Exception());
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestrictedAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetByIdRestricted2Async_PermissionsException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedById2Async("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetByIdRestricted2Async_RepositoryException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedById2Async("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById2_ArgumentException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetByIdRestricted2Async_Exception()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actual = await studentFinancialAidAwardsController.GetRestrictedById2Async("ds");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetByIdRestricted2Async_KeyNotFoundException()
            {
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await studentFinancialAidAwardsController.GetRestrictedById2Async("ds");
            }

            //GET by id v11.1.0 v 11
            //Successful
            //GetRestrictedById2Async
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById2Async_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var studentFinancialAidAward = studentFinancialAidAward2Dtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestrictedById2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(id, true, false)).ReturnsAsync(studentFinancialAidAward);
                var actual = await studentFinancialAidAwardsController.GetRestrictedById2Async(id);

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards));


            }

            //GET by id v11.1.0 v 11
            //Exception
            //GetRestrictedById2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestrictedById2Async_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestrictedById2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.GetById2Async(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var actual = await studentFinancialAidAwardsController.GetRestrictedById2Async("ds");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v11.1.0 
            //Successful
            //GetRestricted3Async
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestricted3Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestricted3Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards));


            }

            //GET v11.1.0
            //Exception
            //GetRestricted3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestricted3Async_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestricted3Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted3Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v11
            //Successful
            //GetRestricted2Async
            [TestMethod]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestricted2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);
                studentFinancialAidAwardsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards);

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAward2Dtos, 4);

                studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ReturnsAsync(tuple);
                var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());

                Object filterObject;
                studentFinancialAidAwardsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards));


            }

            //GET v11
            //Exception
            //GetRestricted2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentFinancialAidAwardsController_GetRestricted2Async_Invalid_Permissions()
            {
                var paging = new Paging(100, 0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentFinancialAidAwards" },
                    { "action", "GetRestricted2Async" }
                };
                HttpRoute route = new HttpRoute("student-financial-aid-awards", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentFinancialAidAwardsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentFinancialAidAwardsController.ControllerContext;
                var actionDescriptor = studentFinancialAidAwardsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentFinancialAidAwardServiceMock.Setup(ci => ci.Get2Async(offset, limit, It.IsAny<Dtos.StudentFinancialAidAward2>(), It.IsAny<string>(), false, true)).ThrowsAsync(new PermissionsException());
                    studentFinancialAidAwardServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-financial-aid-awards."));
                    var studentFinancialAidAwards = await studentFinancialAidAwardsController.GetRestricted2Async(new Paging(limit, offset), It.IsAny<QueryStringFilter>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

        }
    }
}