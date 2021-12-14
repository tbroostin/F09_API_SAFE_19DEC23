// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonGuardiansControllerTests
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

        private PersonGuardiansController personGuardianRelationshipsController;
        Mock<ILogger> loggerMock = new Mock<ILogger>();
        IAdapterRegistry AdapterRegistry;
        Mock<IPersonGuardianRelationshipService> personGuardianRelationshipServiceMock = new Mock<IPersonGuardianRelationshipService>();

        Ellucian.Colleague.Dtos.PersonGuardianRelationship personGuardianRelationship;
        List<Dtos.PersonGuardianRelationship> personGuardianRelationshipsDtos = new List<PersonGuardianRelationship>();
        Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int> personGuardianRelationshipTuple;
        private Paging page;
        private int limit;
        private int offset;

        string id = "6b8e0811-ef7b-4399-8006-8183fcb4f8e3";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);

            BuildData();
            limit = 4;
            offset = 0;
            page = new Paging(limit, offset);

            personGuardianRelationshipTuple = new Tuple<IEnumerable<PersonGuardianRelationship>, int>(personGuardianRelationshipsDtos, 3);

            personGuardianRelationshipsController = new PersonGuardiansController(AdapterRegistry, personGuardianRelationshipServiceMock.Object, loggerMock.Object);
            personGuardianRelationshipsController.Request = new HttpRequestMessage();
            personGuardianRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        [TestCleanup]
        public void Cleanup()
        {
            personGuardianRelationshipsController = null;
            personGuardianRelationship = null;
        }       

        #region Exceptions Testing

        #region GET

        [TestMethod]
        public async Task PersonGuardiansController_GetAllAsync()
        {
            personGuardianRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personGuardianRelationshipsController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(personGuardianRelationshipTuple);

            var results = await personGuardianRelationshipsController.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny <Paging>());

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonGuardianRelationship> personGuardianRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonGuardianRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonGuardianRelationship>;

            Assert.AreEqual(personGuardianRelationshipsDtos.Count(), 4);
            Assert.AreEqual(personGuardianRelationshipResults.Count(), 4);
            int resultCounts = personGuardianRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personGuardianRelationshipsDtos[i];
                var actual = personGuardianRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
                Assert.AreEqual(expected.Guardians.Count(), actual.Guardians.Count());

                foreach (var guardian in actual.Guardians)
                {
                    Assert.IsNotNull(guardian);
                    Assert.AreEqual(expected.Guardians.FirstOrDefault(g => g.Id.Equals(guardian.Id, StringComparison.OrdinalIgnoreCase)).Id, guardian.Id);
                }
            }
        }

        [TestMethod]
        public async Task PersonGuardiansController_GetAllWithPersonFilterAsync()
        {
            string subjectPersonId = "32633175-304a-4888-9057-e49a20687e2a";
            List<Dtos.PersonGuardianRelationship> expectedDto = new List<PersonGuardianRelationship>(){personGuardianRelationshipTuple.Item1.First()};
            Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int> filteredExpected = new Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>(expectedDto, 1);

            personGuardianRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personGuardianRelationshipsController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<int>(), It.IsAny<int>(), subjectPersonId)).ReturnsAsync(filteredExpected);

            var results = await personGuardianRelationshipsController.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<Paging>(), subjectPersonId);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonGuardianRelationship> personGuardianRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonGuardianRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonGuardianRelationship>;

            Assert.AreEqual(personGuardianRelationshipResults.Count(), 1);
            int resultCounts = personGuardianRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personGuardianRelationshipsDtos[i];
                var actual = personGuardianRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
                Assert.AreEqual(expected.Guardians.Count(), actual.Guardians.Count());

                foreach (var guardian in actual.Guardians)
                {
                    Assert.IsNotNull(guardian);
                    Assert.AreEqual(expected.Guardians.FirstOrDefault(g => g.Id.Equals(guardian.Id, StringComparison.OrdinalIgnoreCase)).Id, guardian.Id);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardiansController_GetAllAsync_Exception()
        {
            personGuardianRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personGuardianRelationshipsController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            var results = await personGuardianRelationshipsController.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        public async Task PersonGuardiansController_GetById()
        {
            var expected = personGuardianRelationshipsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipByIdAsync(id)).ReturnsAsync(expected);

            var actual = await personGuardianRelationshipsController.GetPersonGuardianRelationshipByIdAsync(id);

            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
            Assert.AreEqual(expected.Guardians.Count(), actual.Guardians.Count());

            foreach (var guardian in actual.Guardians)
            {
                Assert.IsNotNull(guardian);
                Assert.AreEqual(expected.Guardians.FirstOrDefault(g => g.Id.Equals(guardian.Id, StringComparison.OrdinalIgnoreCase)).Id, guardian.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardiansController_GetById_Id_Null_Exception()
        {
            var expected = personGuardianRelationshipsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipByIdAsync(id)).ReturnsAsync(expected);

            var actual = await personGuardianRelationshipsController.GetPersonGuardianRelationshipByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardiansController_GetById_Id_Null_KeyNotFoundException()
        {
            var expected = personGuardianRelationshipsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipByIdAsync(id)).ThrowsAsync(new KeyNotFoundException());

            var actual = await personGuardianRelationshipsController.GetPersonGuardianRelationshipByIdAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardiansController_GetById_Exception()
        {
            var expected = personGuardianRelationshipsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipByIdAsync(id)).ThrowsAsync(new Exception());

            var actual = await personGuardianRelationshipsController.GetPersonGuardianRelationshipByIdAsync(id);
        }

        //GET v7
        //Successful
        //GetPersonGuardianRelationshipsAllAndFilterAsync
        [TestMethod]
        public async Task PersonGuardiansController_GetPersonGuardianRelationshipsAllAndFilterAsync_Permissions()
        {
            string subjectPersonId = "32633175-304a-4888-9057-e49a20687e2a";
            List<Dtos.PersonGuardianRelationship> expectedDto = new List<PersonGuardianRelationship>() { personGuardianRelationshipTuple.Item1.First() };
            Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int> filteredExpected = new Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>(expectedDto, 1);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonGuardians" },
                    { "action", "GetPersonGuardianRelationshipsAllAndFilterAsync" }
                };
            HttpRoute route = new HttpRoute("person-external-education-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personGuardianRelationshipsController.Request.SetRouteData(data);
            personGuardianRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdatePersonExternalEducationCredentials);

            var controllerContext = personGuardianRelationshipsController.ControllerContext;
            var actionDescriptor = personGuardianRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personGuardianRelationshipServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<int>(), It.IsAny<int>(), subjectPersonId)).ReturnsAsync(filteredExpected);
            var results = await personGuardianRelationshipsController.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<Paging>(), subjectPersonId);

            Object filterObject;
            personGuardianRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonExternalEducationCredentials));


        }

        //GET v7
        //Exception
        //GetPersonGuardianRelationshipsAllAndFilterAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardiansController_GetPersonGuardianRelationshipsAllAndFilterAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonGuardians" },
                    { "action", "GetPersonGuardianRelationshipsAllAndFilterAsync" }
                };
            HttpRoute route = new HttpRoute("person-guardians", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personGuardianRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personGuardianRelationshipsController.ControllerContext;
            var actionDescriptor = personGuardianRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws<PermissionsException>();
                personGuardianRelationshipServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view person-guardians."));
                var results = await personGuardianRelationshipsController.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<Paging>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET by id v7
        //Successful
        //GetPersonGuardianRelationshipByIdAsync
        [TestMethod]
        public async Task PersonGuardiansController_GetPersonGuardianRelationshipByIdAsync_Permissions()
        {
            var expected = personGuardianRelationshipsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonGuardians" },
                    { "action", "GetPersonGuardianRelationshipByIdAsync" }
                };
            HttpRoute route = new HttpRoute("person-external-education-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personGuardianRelationshipsController.Request.SetRouteData(data);
            personGuardianRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdatePersonExternalEducationCredentials);

            var controllerContext = personGuardianRelationshipsController.ControllerContext;
            var actionDescriptor = personGuardianRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personGuardianRelationshipServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipByIdAsync(id)).ReturnsAsync(expected);
            var actual = await personGuardianRelationshipsController.GetPersonGuardianRelationshipByIdAsync(id);

            Object filterObject;
            personGuardianRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonExternalEducationCredentials));


        }

        //GET by id v7
        //Exception
        //GetPersonGuardianRelationshipByIdAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardiansController_GetPersonGuardianRelationshipByIdAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonGuardians" },
                    { "action", "GetPersonGuardianRelationshipByIdAsync" }
                };
            HttpRoute route = new HttpRoute("person-guardians", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personGuardianRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personGuardianRelationshipsController.ControllerContext;
            var actionDescriptor = personGuardianRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personGuardianRelationshipServiceMock.Setup(x => x.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws<PermissionsException>();
                personGuardianRelationshipServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view person-guardians."));
                var results = await personGuardianRelationshipsController.GetPersonGuardianRelationshipsAllAndFilterAsync(It.IsAny<Paging>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region POST
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardianRelationshipsController_PostAsync_Exception()
        {
            await personGuardianRelationshipsController.PostPersonGuardianRelationshipAsync(It.IsAny<PersonGuardianRelationship>());
        }
        #endregion

        #region PUT
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardianRelationshipsController_PutAsync_Exception()
        {
            await personGuardianRelationshipsController.PutPersonGuardianRelationshipAsync(It.IsAny<string>(), It.IsAny<PersonGuardianRelationship>());
        }
        #endregion

        #region DELETE
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonGuardianRelationshipsController_DeleteAsync_Exception()
        {
            await personGuardianRelationshipsController.DeletePersonGuardianRelationshipAsync(It.IsAny<string>());
        }
        #endregion

        private void BuildData()
        {
            personGuardianRelationshipsDtos = new List<PersonGuardianRelationship>() 
            {
                new PersonGuardianRelationship()
                {
                    Id = "6b8e0811-ef7b-4399-8006-8183fcb4f8e3",
                    SubjectPerson = new GuidObject2("32633175-304a-4888-9057-e49a20687e2a"),
                    Guardians = new List<Dtos.DtoProperties.PersonGuardianDtoProperty>()
                    {
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "ef6ab66f-c32d-46ad-b60b-fb35c38ceb01" },
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "347f8998-39ec-42cd-9224-7b65393c77c0" }
                    }
                },
                new PersonGuardianRelationship()
                {
                    Id = "0c3f1986-66b6-45d6-8a57-5c13879198d2",
                    SubjectPerson = new GuidObject2("0a68f897-0cc7-4ca4-b13d-c737bed82750"),
                    Guardians = new List<Dtos.DtoProperties.PersonGuardianDtoProperty>()
                    {
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "e87a0960-44e2-48d7-92d9-23036bfb589d" },
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "e94dc672-3762-4080-969a-e4cfe494001c" }
                    }
                },
                new PersonGuardianRelationship()
                {
                    Id = "790bda40-8b10-4e8d-9e75-a988bcc16117",
                    SubjectPerson = new GuidObject2("02298436-d03e-442f-886a-b2a996e463a9"),
                    Guardians = new List<Dtos.DtoProperties.PersonGuardianDtoProperty>()
                    {
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "b4e1567b-be37-4b1e-890f-501c4a5a0b2f" },
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "19224e5a-2ea2-4554-86b3-2e86b631c816" }
                    }
                },
                new PersonGuardianRelationship()
                {
                    Id = "ed256e80-086f-4893-8068-67735c16d47f",
                    SubjectPerson = new GuidObject2("60c19e8c-c41d-42e3-9b87-a9007cf1aa60"),
                    Guardians = new List<Dtos.DtoProperties.PersonGuardianDtoProperty>()
                    {
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "588a0385-754f-4486-ad07-9f4a7f60986b" },
                        new Dtos.DtoProperties.PersonGuardianDtoProperty(){ Id = "7b8298dc-02e2-4723-807c-de797b15df98" }
                    }
                }
            };
        }     

        #endregion
    }
}
