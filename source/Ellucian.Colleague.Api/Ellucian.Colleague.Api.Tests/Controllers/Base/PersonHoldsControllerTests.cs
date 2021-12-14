// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Web.Http.Models;
using Newtonsoft.Json.Linq;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Base;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Collections;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonHoldsControllerTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        private PersonHoldsController personHoldsController;
        private Mock<IPersonHoldsService> personHoldsServiceMock;
        Mock<ILogger> loggerMock = new Mock<ILogger>();
        private IAdapterRegistry AdapterRegistry;
        private List<Dtos.PersonHold> personHoldDtoList = new List<PersonHold>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            personHoldsServiceMock = new Mock<IPersonHoldsService>();

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);

            var personHoldTypeList = new List<PersonHoldType>();

            personHoldDtoList = new TestPersonHoldsRepository().GetPersonHolds() as List<Dtos.PersonHold>;

            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);

            personHoldsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personHoldsController = new PersonHoldsController(AdapterRegistry, personHoldsServiceMock.Object,
                loggerMock.Object) {Request = new HttpRequestMessage()};
            personHoldsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personHoldsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(personHold));

            
        }

        [TestCleanup]
        public void Cleanup()
        {
            personHoldsController = null;
            personHoldDtoList = null;
        }       

        #region Exceptions Testing
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldsAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            await personHoldsController.GetPersonsActiveHoldsAsync(new Paging(10,10));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.GetPersonsActiveHoldAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            await personHoldsController.GetPersonsActiveHoldAsync(It.IsAny<string>());
        }

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync_ArgumentNullException()
        //{
        //    personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());

        //    await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(It.IsAny<string>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync_Exception()
        //{
        //    personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

        //    await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(It.IsAny<string>());
        //}

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Null_Id()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PutPersonHoldAsync("", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Null_DTO()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PutPersonHoldAsync("1234", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Id_NoMatch_PersonHoldId()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold() { Id = "5678"});
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_NilGUID_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync(Guid.Empty.ToString(), new Dtos.PersonHold() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_NilGuid_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync(Guid.Empty.ToString(), new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_KeyNotFoundException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new KeyNotFoundException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new Exception());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_Null_DTO()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PostPersonHoldAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_KeyNotFoundException()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new KeyNotFoundException());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new Exception());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync(It.IsAny<string>())).Throws(new ArgumentNullException());

            await personHoldsController.DeletePersonHoldAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_Id_Null()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync(It.IsAny<string>())).Throws(new ArgumentNullException());

            await personHoldsController.DeletePersonHoldAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync("1234")).Throws(new Exception());

            await personHoldsController.DeletePersonHoldAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_PermissionsException()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync("1234")).Throws(new PermissionsException());

            await personHoldsController.DeletePersonHoldAsync("1234");
        }
        #endregion

        #region All GETS

        //Get
        //Version 6
        //GetPersonsActiveHoldsAsync

        //Example success 
        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "GetPersonsActiveHoldsAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.ViewPersonHold, PersonHoldsPermissionCodes.CreateUpdatePersonHold });

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var resp = await personHoldsController.GetPersonsActiveHoldsAsync(new Paging(10, 0));
            
            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.ViewPersonHold));
            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.CreateUpdatePersonHold));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldsAsync_Invalid_Permissionsn()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "GetPersonsActiveHoldsAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
   
            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
                personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view person-holds."));
                var resp = await personHoldsController.GetPersonsActiveHoldsAsync(new Paging(10, 10));
            }
            catch (PermissionsException ex)
            {               
                throw ex;
            }
        }


        //Get
        //Version 6
        //GetPersonsActiveHoldsByPersonIdAsync

        //Example success 
        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync_Permissions()
        {
            var personId = "895cebf0-e6e8-4169-aac6-e0e14dfefdd4";
            var personHoldsByPersId = personHoldDtoList.Where(i => i.Person.Id.Equals(personId));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "GetPersonsActiveHoldsByPersonIdAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.ViewPersonHold, PersonHoldsPermissionCodes.CreateUpdatePersonHold });

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(personId, It.IsAny<bool>())).ReturnsAsync(personHoldsByPersId);
            var resp = await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(personId);

            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.ViewPersonHold));
            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.CreateUpdatePersonHold));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync_Exception()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "GetPersonsActiveHoldsByPersonIdAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
                personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view person-holds."));
                var resp = await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(It.IsAny<string>());

            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Get by Id
        //Version 6
        //GetPersonsActiveHoldAsync

        //Example success 
        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync_Permissions()
        {
            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "GetPersonsActiveHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.ViewPersonHold, PersonHoldsPermissionCodes.CreateUpdatePersonHold });

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(id, It.IsAny<bool>())).ReturnsAsync(personHold);

            var resp = await personHoldsController.GetPersonsActiveHoldAsync(id); 

            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.ViewPersonHold));
            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.CreateUpdatePersonHold));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "GetPersonsActiveHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
                personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view person-holds."));
                var resp = await personHoldsController.GetPersonsActiveHoldAsync(It.IsAny<string>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldsAsync()
        {
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            
            var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var personHolds = await personHoldsController.GetPersonsActiveHoldsAsync(new Paging(10,0));
           
            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await personHolds.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonHold> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonHold>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonHold>;
            
            var result = results.FirstOrDefault();

            Assert.IsTrue(personHolds is IHttpActionResult);
            
            foreach (var personHold in personHoldDtoList)
            {
                var persHold = results.FirstOrDefault(i => i.Id == personHold.Id);

                Assert.AreEqual(personHold.Id, persHold.Id);
                Assert.AreEqual(personHold.Comment, persHold.Comment);
                Assert.AreEqual(personHold.EndOn, persHold.EndOn);
                Assert.AreEqual(personHold.NotificationIndicator, persHold.NotificationIndicator);
                Assert.AreEqual(personHold.Person.Id, persHold.Person.Id);
                Assert.AreEqual(personHold.PersonHoldTypeType, persHold.PersonHoldTypeType);
                Assert.AreEqual(personHold.StartOn, persHold.StartOn);
            }
        }
        
        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync()
        {
            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(id, It.IsAny<bool>())).ReturnsAsync(personHold);
            var result = await personHoldsController.GetPersonsActiveHoldAsync(id);

            Assert.AreEqual(personHold.Id, result.Id);
            Assert.AreEqual(personHold.Comment, result.Comment);
            Assert.AreEqual(personHold.EndOn, result.EndOn);
            Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
            Assert.AreEqual(personHold.Person.Id, result.Person.Id);
            Assert.AreEqual(personHold.PersonHoldTypeType, result.PersonHoldTypeType);
            Assert.AreEqual(personHold.StartOn, result.StartOn);
        }

        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync()
        {
            var personId = "895cebf0-e6e8-4169-aac6-e0e14dfefdd4";
            var personHoldsByPersId = personHoldDtoList.Where(i => i.Person.Id.Equals(personId));
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(personId, It.IsAny<bool>())).ReturnsAsync(personHoldsByPersId);
            var result = await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(personId);

            Assert.AreEqual(personHoldsByPersId.Count(), result.Count());
            foreach (var personHold in personHoldsByPersId)
            {
                var persHold = result.FirstOrDefault(i => i.Id == personHold.Id);

                Assert.AreEqual(personHold.Id, persHold.Id);
                Assert.AreEqual(personHold.Comment, persHold.Comment);
                Assert.AreEqual(personHold.EndOn, persHold.EndOn);
                Assert.AreEqual(personHold.NotificationIndicator, persHold.NotificationIndicator);
                Assert.AreEqual(personHold.Person.Id, persHold.Person.Id);
                Assert.AreEqual(personHold.PersonHoldTypeType, persHold.PersonHoldTypeType);
                Assert.AreEqual(personHold.StartOn, persHold.StartOn);
            }
        }
        #endregion

        #region PUT

        //Put
        //Version 6
        //PutPersonHoldAsync

        //Example success 
        [TestMethod]
        public async Task PersonHoldsController_PutPersonHoldAsync_Permissions()
        {
            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "PutPersonHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.CreateUpdatePersonHold });

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(id, It.IsAny<Dtos.PersonHold>())).ReturnsAsync(personHold);
            var resp = await personHoldsController.PutPersonHoldAsync(id, personHold);

            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.CreateUpdatePersonHold));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Exceptions()
        {
            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "PutPersonHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
                personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new Exception());
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update person-holds."));
                var resp = await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public async Task PersonHoldsController_PutPersonHoldAsync()
        {
            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(id, It.IsAny<Dtos.PersonHold>())).ReturnsAsync(personHold);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(id, It.IsAny<bool>())).ReturnsAsync(personHold);
            var result = await personHoldsController.PutPersonHoldAsync(id, personHold);

            Assert.AreEqual(personHold.Id, result.Id);
            Assert.AreEqual(personHold.Comment, result.Comment);
            Assert.AreEqual(personHold.EndOn, result.EndOn);
            Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
            Assert.AreEqual(personHold.Person.Id, result.Person.Id);
            Assert.AreEqual(personHold.PersonHoldTypeType, result.PersonHoldTypeType);
            Assert.AreEqual(personHold.StartOn, result.StartOn);
        }
        #endregion

        #region POST

        //POST
        //Version 6
        //PostPersonHoldAsync

        //Example success 
        [TestMethod]
        public async Task PersonHoldsController_PostPersonHoldAsync_Permissions()
        {
            
            var id = Guid.Empty.ToString();
            var personHold = personHoldDtoList.First();
            personHold.Id = id;
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "PostPersonHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.CreateUpdatePersonHold });

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(personHold)).ReturnsAsync(personHold);
            var resp = await personHoldsController.PostPersonHoldAsync(personHold);

            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.CreateUpdatePersonHold));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonsActiveHoldsAsync_Exception()
        {
            var id = Guid.Empty.ToString();
            var personHold = personHoldDtoList.First();
            personHold.Id = id;
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "PostPersonHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
                personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new Exception());
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create person-holds."));
                var resp = await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public async Task PersonHoldsController_PostPersonsActiveHoldsAsync_Permissions()
        {

            var id = Guid.Empty.ToString();
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            var newId = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var newPersonHold = personHoldDtoList.FirstOrDefault(i => i.Id == newId);
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(personHold)).ReturnsAsync(newPersonHold);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "PostPersonHoldsAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.CreateUpdatePersonHold });

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                   .Returns(true);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var resp = await personHoldsController.PostPersonHoldAsync(personHold);

            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.CreateUpdatePersonHold));

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonsActiveHoldsAsync_Invalid_Permissions()
        {
            var id = Guid.Empty.ToString();
            var personHold = personHoldDtoList.First();
            personHold.Id = id;
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "PostPersonHoldsAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);

                personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User is not authorized to view person-holds."));
                var resp = await personHoldsController.PostPersonHoldAsync(personHold);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public async Task PersonHoldsController_PostPersonHoldAsync()
        {
            var id = Guid.Empty.ToString();
            var personHold = personHoldDtoList.First();
            personHold.Id = id;
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(personHold)).ReturnsAsync(personHold);
            var result = await personHoldsController.PostPersonHoldAsync(personHold);

            Assert.AreEqual(personHold.Id, result.Id);
            Assert.AreEqual(personHold.Comment, result.Comment);
            Assert.AreEqual(personHold.EndOn, result.EndOn);
            Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
            Assert.AreEqual(personHold.Person.Id, result.Person.Id);
            Assert.AreEqual(personHold.PersonHoldTypeType, result.PersonHoldTypeType);
            Assert.AreEqual(personHold.StartOn, result.StartOn);
        }
        #endregion

        #region DELETE

        //Delete
        //Version 6
        //DeletePersonHoldAsync

        //Example success 
        [TestMethod]
        public async Task PersonHoldsController_DeletePersonsActiveHoldsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "DeletePersonHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.DeletePersonHold});

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync("1234")).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            await personHoldsController.DeletePersonHoldAsync("1234");

            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.DeletePersonHold));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonsActiveHoldsAsync_Exception()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "DeletePersonHoldAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                //var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
                personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new Exception());
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to delete person-holds."));
                await personHoldsController.DeletePersonHoldAsync("1234");
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public async Task PersonHoldsController_DeletePersonsActiveHoldsAsync_Invalid_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "DeletePersonHoldsAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { PersonHoldsPermissionCodes.DeletePersonHold});

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                   .Returns(true);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            await personHoldsController.DeletePersonHoldAsync("1234");

            Object filterObject;
            personHoldsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(PersonHoldsPermissionCodes.DeletePersonHold));

        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonsActiveHoldsAsync_No_Permission()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "PersonHolds" },
                { "action", "DeletePersonHoldsAsync" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personHoldsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personHoldsController.ControllerContext;
            var actionDescriptor = personHoldsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);

                personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                personHoldsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User is not authorized to delete person-holds."));
                await personHoldsController.DeletePersonHoldAsync("1234");
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public async Task PersonHoldsController_DeletePersonHoldAsync_HttpResponseMessage()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync("1234")).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            await personHoldsController.DeletePersonHoldAsync("1234");
        }
        #endregion   
    }
}
