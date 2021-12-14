// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Filters;
using System.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.DtoProperties;
using System.Web.Http.Routing;
using System.Collections;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EducationalInstitutionControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        private Mock<IEducationalInstitutionsService> educationalInstitutionsServiceMock;
        private Mock<ILogger> loggerMock;

        private EducationalInstitutionsController educationalInstitutionsController;
        private List<Dtos.EducationalInstitution> educationalInstitutionCollection;
        private Ellucian.Web.Http.Models.QueryStringFilter criteria = new Web.Http.Models.QueryStringFilter("criteria", "");
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            educationalInstitutionsServiceMock = new Mock<IEducationalInstitutionsService>();
            loggerMock = new Mock<ILogger>();

            educationalInstitutionCollection = new List<Dtos.EducationalInstitution>()
            {
                new Dtos.EducationalInstitution() { Id = "id1", Title = "title1", Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool, HomeInstitution = Dtos.EnumProperties.HomeInstitutionType.Home },
                new Dtos.EducationalInstitution() { Id = "id2", Title = "title2", Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool, HomeInstitution = Dtos.EnumProperties.HomeInstitutionType.Home },
                new Dtos.EducationalInstitution() { Id = "id3", Title = "title3", Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool, HomeInstitution = Dtos.EnumProperties.HomeInstitutionType.Home }
            };

            educationalInstitutionsController = new EducationalInstitutionsController(educationalInstitutionsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            educationalInstitutionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            educationalInstitutionsController = null;
            educationalInstitutionCollection = null;
            loggerMock = null;
            educationalInstitutionsServiceMock = null;
        }

        #region EducationalInstitutions

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_CredentialFilter_EmptyValue()
        {
            var filterGroupName = "criteria";
            var page = new Paging(10, 0);

            educationalInstitutionsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.EducationalInstitution()
                  {
                      Credentials = new List<Credential3DtoProperty> {
                      new Credential3DtoProperty { Type  = Credential3Type.ColleaguePersonId, Value = null } }
                  });

            var result = await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "", criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_CredentialFilter_EmptyType()
        {
            var filterGroupName = "criteria";
            var page = new Paging(10, 0);

            educationalInstitutionsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.EducationalInstitution()
                  {
                      Credentials = new List<Credential3DtoProperty> {
                      new Credential3DtoProperty { Type = null, Value = "00009999" } }
                  });

            var result = await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "", criteria);
        }

        [TestMethod]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_CredentialFilter_InvalidType()
        {
            var filterGroupName = "criteria";
            var page = new Paging(10, 0);

            educationalInstitutionsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.EducationalInstitution()
                  {
                      Credentials = new List<Credential3DtoProperty> {
                      new Credential3DtoProperty { Type  = Credential3Type.BannerUdcId, Value = "00009999" } }
                  });

            var result = await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "", criteria);
            Assert.IsNotNull(result);
            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);
            IEnumerable<Dtos.EducationalInstitution> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitution>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.EducationalInstitution>;
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_CredentialFilter_Multiple()
        {
            var filterGroupName = "criteria";
            var page = new Paging(10, 0);

            educationalInstitutionsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.EducationalInstitution()
                  {
                      Credentials = new List<Credential3DtoProperty> {
                      new Credential3DtoProperty { Type  = Credential3Type.ColleaguePersonId, Value = "00009999" },
                      new Credential3DtoProperty { Type  = Credential3Type.ColleaguePersonId, Value = "00009998" }}
                  });

            var result = await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "", criteria);
            Assert.IsNotNull(result);
            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);
            IEnumerable<Dtos.EducationalInstitution> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitution>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.EducationalInstitution>;
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_PermissionsException()
        {
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                 It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<EducationalInstitutionType>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_ArgumentException()
        {
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(), 
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<EducationalInstitutionType>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_RepositoryException()
        {
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<EducationalInstitutionType>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_IntegrationApiException()
        {
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<EducationalInstitutionType>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutions_Exception()
        {
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<EducationalInstitutionType>(), It.IsAny<bool>())).Throws<Exception>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4));
        }

        //GET v6.1.0 / v6
        //Successful
        //GetEducationalInstitutionsAsync

        [TestMethod]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsAsync_Permissions()
        {
            var educationalInsitutionsCollection = new List<Dtos.EducationalInstitution>()
            {
                new Dtos.EducationalInstitution() { Id = "id1", Title = "title1", Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool, HomeInstitution = Dtos.EnumProperties.HomeInstitutionType.Home },
                new Dtos.EducationalInstitution() { Id = "id2", Title = "title2", Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool, HomeInstitution = Dtos.EnumProperties.HomeInstitutionType.Home },
                new Dtos.EducationalInstitution() { Id = "id3", Title = "title3", Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool, HomeInstitution = Dtos.EnumProperties.HomeInstitutionType.Home }
            };

            var expectedCollection = new Tuple<IEnumerable<Dtos.EducationalInstitution>, int>(educationalInsitutionsCollection, educationalInsitutionsCollection.Count);

            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "EducationalInstitutions" },
                    { "action", "GetEducationalInstitutionsAsync" }
                };
            HttpRoute route = new HttpRoute("educational-institutions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            educationalInstitutionsController.Request.SetRouteData(data);
            educationalInstitutionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.ViewEducationalInstitution);

            var controllerContext = educationalInstitutionsController.ControllerContext;
            var actionDescriptor = educationalInstitutionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            educationalInstitutionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<EducationalInstitutionType?>(), It.IsAny<bool>())).ReturnsAsync(expectedCollection);
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4));

            Object filterObject;
            educationalInstitutionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewEducationalInstitution));

        }

        //GET v6.1.0 / v6
        //Exception
        //GetEducationalInstitutionsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "EducationalInstitutions" },
                    { "action", "GetEducationalInstitutionsAsync" }
                };
            HttpRoute route = new HttpRoute("educational-institutions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            educationalInstitutionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = educationalInstitutionsController.ControllerContext;
            var actionDescriptor = educationalInstitutionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<EducationalInstitutionType>(), It.IsAny<bool>())).Throws<PermissionsException>();
                educationalInstitutionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view educational-institutions."));
                await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4));
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion GetEducationalInstitutions

        #region GetEducationalInstitutionsByGuid

        [TestMethod]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id,It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = (await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id));
            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid_NullArgument()
        {
            await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid_EmptyArgument()
        {
            await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid_PermissionsException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id, It.IsAny<bool>())).Throws<PermissionsException>();
            await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid_ArgumentException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id, It.IsAny<bool>())).Throws<ArgumentException>();
            await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid_RepositoryException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id, It.IsAny<bool>())).Throws<RepositoryException>();
            await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid_IntegrationApiException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id, It.IsAny<bool>())).Throws<IntegrationApiException>();
            await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuid_Exception()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id, It.IsAny<bool>())).Throws<Exception>();
            await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id);
        }

        #endregion GetEducationalInstitutionsByGuid

        #region GetEducationalInstitutionsWithFilters

        [TestMethod]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByType_NullArgument()
        {
            var result = await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), null);
            Assert.IsNotNull(result);
            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);
            IEnumerable<Dtos.EducationalInstitution> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitution>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.EducationalInstitution>;
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByType_EmptyArgument()
        {
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByType_PermissionsException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<Dtos.EnumProperties.EducationalInstitutionType>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "secondarySchool");

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByType_ArgumentException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<Dtos.EnumProperties.EducationalInstitutionType>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "secondarySchool");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByType_RepositoryException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<Dtos.EnumProperties.EducationalInstitutionType>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "secondarySchool");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByType_IntegrationApiException()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<Dtos.EnumProperties.EducationalInstitutionType>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "secondarySchool");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByType_Exception()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionsByTypeAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Dtos.EducationalInstitution>(), It.IsAny<Dtos.EnumProperties.EducationalInstitutionType>(), It.IsAny<bool>())).Throws<Exception>();
            await educationalInstitutionsController.GetEducationalInstitutionsAsync(new Paging(0, 4), "secondarySchool");
        }

        //GET by id v6.1.0 / v6
        //Successful
        //GetEducationalInstitutionsByGuidAsync

        [TestMethod]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuidAsync_Permissions()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "EducationalInstitutions" },
                    { "action", "GetEducationalInstitutionsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("educational-institutions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            educationalInstitutionsController.Request.SetRouteData(data);
            educationalInstitutionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.ViewEducationalInstitution);

            var controllerContext = educationalInstitutionsController.ControllerContext;
            var actionDescriptor = educationalInstitutionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            educationalInstitutionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = (await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id));

            Object filterObject;
            educationalInstitutionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewEducationalInstitution));

        }

        //GET by id v6.1.0 / v6
        //Exception
        //GetEducationalInstitutionsByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_GetEducationalInstitutionsByGuidAsync_Invalid_Permissions()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "EducationalInstitutions" },
                    { "action", "GetEducationalInstitutionsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("educational-institutions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            educationalInstitutionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = educationalInstitutionsController.ControllerContext;
            var actionDescriptor = educationalInstitutionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                educationalInstitutionsServiceMock.Setup(x => x.GetEducationalInstitutionByGuidAsync(expected.Id, It.IsAny<bool>())).Throws<PermissionsException>();
                educationalInstitutionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view educational-institutions."));
                await educationalInstitutionsController.GetEducationalInstitutionsByGuidAsync(expected.Id);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }
        #endregion GetEducationalInstitutionsByType

        #region Put

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_PutEducationalInstitutions_NullArgument()
        {
            await educationalInstitutionsController.PutEducationalInstitutionsAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_PutEducationalInstitutions_EmptyArgument()
        {
            await educationalInstitutionsController.PutEducationalInstitutionsAsync("", null);
        }
      
        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_PostEducationalInstitutions_NullArgument()
        {
            await educationalInstitutionsController.PostEducationalInstitutionsAsync(null);
        }
 
        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionsController_DeleteEducationalInstitutions_EmptyArgument()
        {
            var expected = educationalInstitutionCollection.FirstOrDefault();
            await educationalInstitutionsController.DeleteEducationalInstitutionByGuidAsync("");
        }
 
        #endregion
    }
}