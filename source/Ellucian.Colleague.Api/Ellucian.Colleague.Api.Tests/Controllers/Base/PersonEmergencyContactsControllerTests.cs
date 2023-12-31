﻿//Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonEmergencyContactsControllerTests
    {
        [TestClass]
        public class PersonEmergencyContactsControllerTests_GET_GET_ALL_DELETE
        {
            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext { get; set; }

            private Mock<IEmergencyInformationService> personEmergencyContactsServiceMock;
            private Mock<ILogger> loggerMock;
            private PersonEmergencyContactsController personEmergencyContactsController;
            private List<Dtos.PersonEmergencyContacts> dtos;
            private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                BuildDataAndMocks();

                personEmergencyContactsController = new PersonEmergencyContactsController(personEmergencyContactsServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                personEmergencyContactsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            private void BuildDataAndMocks()
            {
                personEmergencyContactsServiceMock = new Mock<IEmergencyInformationService>();
                loggerMock = new Mock<ILogger>();
                dtos = new List<Dtos.PersonEmergencyContacts>()
                {
                    new Dtos.PersonEmergencyContacts()
                    {
                        Id = guid,
                        Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                        {
                            Name = new Dtos.PersonContactName()
                            {
                                FirstName = "FirstName",
                                FullName = "FirstName, LastName",
                                LastName = "LastName",
                                MiddleName = "MiddleName"
                            },
                            Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                            {
                                new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                                {
                                    ContactAvailability = new Dtos.GuidObject2(""),
                                    CountryCallingCode = "91",
                                    Extension = "1234",
                                    Number = "800 555 1212"
                                }
                            },
                            Relationship  = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                            {
                                Detail = new Dtos.GuidObject2(""),
                                Type = "Parent"
                            },
                            Types = new List<Dtos.GuidObject2>()
                            {
                                new Dtos.GuidObject2("")
                            }
                        }
                    }
                };
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonEmergencyContacts>, int> tuple =
                    new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(dtos, dtos.Count());
                personEmergencyContactsServiceMock.Setup(ser => ser.GetPersonEmergencyContacts2Async(0, 100, It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(tuple);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personEmergencyContactsController = null;
                loggerMock = null;
                personEmergencyContactsServiceMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsAsync_KeyNotFoundException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await personEmergencyContactsController.GetPersonEmergencyContactsAsync(new Web.Http.Models.Paging(100, 0), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsAsync_PermissionsException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await personEmergencyContactsController.GetPersonEmergencyContactsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsAsync_ArgumentException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await personEmergencyContactsController.GetPersonEmergencyContactsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsAsync_RepositoryException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await personEmergencyContactsController.GetPersonEmergencyContactsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsAsync_IntegrationApiException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await personEmergencyContactsController.GetPersonEmergencyContactsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsAsync_Exception()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await personEmergencyContactsController.GetPersonEmergencyContactsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            public async Task GetPersonEmergencyContactsAsync()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                var sourceContexts = await personEmergencyContactsController.GetPersonEmergencyContactsAsync(new Web.Http.Models.Paging(100, 0), It.IsAny<QueryStringFilter>(),
                    It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.PersonEmergencyContacts> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonEmergencyContacts>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.PersonEmergencyContacts>;
                Assert.IsNotNull(sourceContexts);
                Assert.AreEqual(1, actuals.Count());
                Assert.AreEqual(dtos.FirstOrDefault().Id, actuals.FirstOrDefault().Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsByGuidAsync_Guid_Null_HttpResponseException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsByGuidAsync_KeyNotFoundException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsByGuidAsync_PermissionsException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsByGuidAsync_ArgumentException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsByGuidAsync_RepositoryException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsByGuidAsync_IntegrationApiException()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmergencyContactsByGuidAsync_Exception()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
            }

            [TestMethod]
            public async Task GetPersonEmergencyContactsByGuidAsync()
            {
                personEmergencyContactsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dtos.FirstOrDefault());
                var result = await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_Null_Empty_Guid()
            {
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_PermissionsException()
            {
                personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new PermissionsException());
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_KeyNotFoundException()
            {
                personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_ArgumentException()
            {
                personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new ArgumentException());
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_InvalidOperationException()
            {
                personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new InvalidOperationException());
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_RepositoryException()
            {
                personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new RepositoryException());
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_IntegrationApiException()
            {
                personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new IntegrationApiException());
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmergencyContactsAsync_Exception()
            {
                personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new Exception());
                await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
            }

            [TestMethod]
            public async Task DeletePersonEmergencyContactsAsync()
            {
                var actual = await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
                Assert.AreEqual(HttpStatusCode.NoContent, actual.StatusCode);
                personEmergencyContactsServiceMock.Verify(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>()));
            }

            //GET by id v1.0.0
            //Successful
            //GetPersonEmergencyContactsByGuidAsync
            [TestMethod]
            public async Task PersonEmergencyContactsController_GetPersonEmergencyContactsByGuidAsync_Permissions()
            {

                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "GetPersonEmergencyContactsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);
                personEmergencyContactsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonContact, BasePermissionCodes.UpdatePersonContact });

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dtos.FirstOrDefault());
                var result = await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");

                Object filterObject;
                personEmergencyContactsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyPersonContact));
                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonContact));


            }

            //GET by id v1.0.0
            //Exception
            //GetPersonEmergencyContactsByGuidAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonEmergencyContactsController_GetPersonEmergencyContactsByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "GetPersonEmergencyContactsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view person-emergency-contacts."));
                    await personEmergencyContactsController.GetPersonEmergencyContactsByGuidAsync("1234");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v1.0.0
            //Successful
            //GetPersonEmergencyContactsAsync
            [TestMethod]
            public async Task PersonEmergencyContactsController_GetPersonEmergencyContactsAsync_Permissions()
            {
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonEmergencyContacts>, int> tuple =
                    new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(dtos, dtos.Count());
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "GetPersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);
                personEmergencyContactsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonContact, BasePermissionCodes.UpdatePersonContact });

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                personEmergencyContactsServiceMock.Setup(ser => ser.GetPersonEmergencyContacts2Async(0, 100, It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                var sourceContexts = await personEmergencyContactsController.GetPersonEmergencyContactsAsync(new Web.Http.Models.Paging(100, 0), It.IsAny<QueryStringFilter>(),It.IsAny<QueryStringFilter>());
                
                Object filterObject;
                personEmergencyContactsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyPersonContact));
                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonContact));


            }

            //GET v1.0.0
            //Exception
            //GetPersonEmergencyContactsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonEmergencyContactsController_GetPersonEmergencyContactsAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "GetPersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    personEmergencyContactsServiceMock.Setup(s => s.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(),
                                        It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view person-emergency-contacts."));
                    await personEmergencyContactsController.GetPersonEmergencyContactsAsync(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //DELETE
            //Successful
            //DeletePersonEmergencyContactsAsync
            [TestMethod]
            public async Task PersonEmergencyContactsController_DeletePersonEmergencyContactsAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "DeletePersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);
                personEmergencyContactsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdatePersonContact );

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                personEmergencyContactsServiceMock.Setup(ser => ser.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Returns(Task.FromResult(new TaskStatus()));
                var actual = await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");

                Object filterObject;
                personEmergencyContactsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonContact));


            }

            //DELETE
            //Exception
            //DeletePersonEmergencyContactsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonEmergencyContactsController_DeletePersonEmergencyContactsAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "DeletePersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    personEmergencyContactsServiceMock.Setup(s => s.DeletePersonEmergencyContactsAsync(It.IsAny<string>())).Throws(new PermissionsException());
                    personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to delete person-emergency-contacts."));
                    await personEmergencyContactsController.DeletePersonEmergencyContactsAsync("1234");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }

        [TestClass]
        public class PersonEmergencyContactsControllerTests_PU_POST
        {
            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext { get; set; }

            private Mock<IEmergencyInformationService> personEmergencyContactsServiceMock;
            private Mock<ILogger> loggerMock;
            private PersonEmergencyContactsController personEmergencyContactsController;
            private List<Dtos.PersonEmergencyContacts> dtos;
            private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                BuildDataAndMocks();

                personEmergencyContactsController = new PersonEmergencyContactsController(personEmergencyContactsServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                personEmergencyContactsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            private void BuildDataAndMocks()
            {
                personEmergencyContactsServiceMock = new Mock<IEmergencyInformationService>();
                loggerMock = new Mock<ILogger>();
                dtos = new List<Dtos.PersonEmergencyContacts>()
                {
                    new Dtos.PersonEmergencyContacts()
                    {
                        Id = guid,
                        Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                        {
                            Name = new Dtos.PersonContactName()
                            {
                                FirstName = "FirstName",
                                FullName = "FirstName, LastName",
                                LastName = "LastName",
                                MiddleName = "MiddleName"
                            },
                            Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                            {
                                new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                                {
                                    ContactAvailability = new Dtos.GuidObject2(""),
                                    CountryCallingCode = "91",
                                    Extension = "1234",
                                    Number = "800 555 1212"
                                }
                            },
                            Relationship  = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                            {
                                Detail = new Dtos.GuidObject2(""),
                                Type = "Parent"
                            },
                            Types = new List<Dtos.GuidObject2>()
                            {
                                new Dtos.GuidObject2("")
                            }
                        }
                    }
                };
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonEmergencyContacts>, int> tuple =
                    new Tuple<IEnumerable<Dtos.PersonEmergencyContacts>, int>(dtos, dtos.Count());
                personEmergencyContactsServiceMock.Setup(ser => ser.GetPersonEmergencyContacts2Async(0, 100, It.IsAny<Dtos.PersonEmergencyContacts>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(tuple);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personEmergencyContactsController = null;
                loggerMock = null;
                personEmergencyContactsServiceMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_Null_Guid_KeyNotFoundException()
            {
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(string.Empty, It.IsAny<Dtos.PersonEmergencyContacts>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_Null_RequestBody_KeyNotFoundException()
            {
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, It.IsAny<Dtos.PersonEmergencyContacts>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_Null_Guid_In_Parameter()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = Guid.Empty.ToString();
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(Guid.Empty.ToString(), requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_Guid_Not_Same_In_RequestBody_HttpResponseException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = Guid.Empty.ToString();
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_PermissionsException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new PermissionsException());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_ArgumentException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new ArgumentException());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_RepositoryException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new RepositoryException());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_IntegrationApiException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new IntegrationApiException());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_ConfigurationException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new System.Configuration.ConfigurationException());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_KeyNotFoundException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmergencyContactsAsync_Exception()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new Exception());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
            }

            [TestMethod]
            public async Task PutPersonEmergencyContactsAsync()
            {
                var requestBody = dtos.FirstOrDefault();
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ReturnsAsync(dtos.FirstOrDefault());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);

                Assert.IsNotNull(result);
                Assert.AreEqual(dtos.FirstOrDefault().Id, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmergencyContactsAsync_PermissionsException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new PermissionsException());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmergencyContactsAsync_ArgumentException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new ArgumentException());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmergencyContactsAsync_RepositoryException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new RepositoryException());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmergencyContactsAsync_IntegrationApiException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new IntegrationApiException());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmergencyContactsAsync_ConfigurationException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new System.Configuration.ConfigurationException());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmergencyContactsAsync_KeyNotFoundException()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmergencyContactsAsync_Exception()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ThrowsAsync(new Exception());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
            }

            [TestMethod]
            public async Task PostPersonEmergencyContactsAsync()
            {
                var newGuid = Guid.NewGuid().ToString();
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = newGuid;
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>()))
                    .ReturnsAsync(dtos.FirstOrDefault());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);

                Assert.IsNotNull(result);
                Assert.AreEqual(dtos.FirstOrDefault().Id, result.Id);
            }

            //PUT v1.0.0
            //Successful
            //PutPersonEmergencyContactsAsync
            [TestMethod]
            public async Task PersonEmergencyContactsController_PutPersonEmergencyContactsAsync_Permissions()
            {
                var requestBody = dtos.FirstOrDefault();
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "PutPersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);
                personEmergencyContactsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdatePersonContact);

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>())).ReturnsAsync(dtos.FirstOrDefault());
                var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);

                Object filterObject;
                personEmergencyContactsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonContact));


            }

            //PUT v1.0.0
            //Exception
            //PutPersonEmergencyContactsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonEmergencyContactsController_PutPersonEmergencyContactsAsync_Invalid_Permissions()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "PutPersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    
                    personEmergencyContactsServiceMock.Setup(ser => ser.UpdatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>())).ThrowsAsync(new PermissionsException());
                    personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update person-emergency-contacts."));
                    var result = await personEmergencyContactsController.PutPersonEmergencyContactsAsync(guid, requestBody);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //POST v1.0.0
            //Successful
            //PostPersonEmergencyContactsAsync
            [TestMethod]
            public async Task PersonEmergencyContactsController_PostPersonEmergencyContactsAsync_Permissions()
            {
                var newGuid = Guid.NewGuid().ToString();
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = newGuid;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "PostPersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);
                personEmergencyContactsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdatePersonContact);

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>())).ReturnsAsync(dtos.FirstOrDefault());
                var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);

                Object filterObject;
                personEmergencyContactsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonContact));


            }

            //POST v1.0.0
            //Exception
            //PostPersonEmergencyContactsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonEmergencyContactsController_PostPersonEmergencyContactsAsync_Invalid_Permissions()
            {
                var requestBody = dtos.FirstOrDefault();
                requestBody.Id = string.Empty;
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmergencyContacts" },
                    { "action", "PostPersonEmergencyContactsAsync" }
                };
                HttpRoute route = new HttpRoute("person-emergency-contacts", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                personEmergencyContactsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = personEmergencyContactsController.ControllerContext;
                var actionDescriptor = personEmergencyContactsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    
                    personEmergencyContactsServiceMock.Setup(ser => ser.CreatePersonEmergencyContactsAsync(It.IsAny<Dtos.PersonEmergencyContacts>())).ThrowsAsync(new PermissionsException());
                    personEmergencyContactsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create person-emergency-contacts."));
                    var result = await personEmergencyContactsController.PostPersonEmergencyContactsAsync(requestBody);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }
    }
}