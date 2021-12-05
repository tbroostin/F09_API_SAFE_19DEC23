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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class ExternalEducationControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IExternalEducationService> _externalEducationServiceMock;
        private Mock<ILogger> _loggerMock;

        private ExternalEducationController _externalEducationController;

        private List<Dtos.ExternalEducation> _externalEducationCollection;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string ExternalEducation1Guid = "a830e686-7692-4012-8da5-b1b5d44389b4"; 

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _externalEducationServiceMock = new Mock<IExternalEducationService>();
            _loggerMock = new Mock<ILogger>();

            _externalEducationCollection = new List<Dtos.ExternalEducation>();

            var externalEducation1 = new Ellucian.Colleague.Dtos.ExternalEducation
            {
                Id = ExternalEducation1Guid,
                ClassPercentile = 25,
                ClassRank = 10,
                ClassSize = 40,
                Credential = new GuidObject2("17D431E0-42DC-44B0-89A8-E4D7622D3426"),
                CredentialsDate = _currentDate,
                CreditsEarned = 10,
                Disciplines = new List<GuidObject2>() {new GuidObject2("47D431E0-42DC-44B0-89A8-E4D7622D3426")},
                GraduatedOn = _currentDate,
                Person = new GuidObject2("5674f28b-b216-4055-b236-81a922d93b4c"),
                Institution = new GuidObject2("4474f28b-b216-4055-b236-81a922d93b4c"),
                ThesisTitle = "Hello World",
                TranscriptReceivedOn = _currentDate,
                Recognition = new List<GuidObject2>() {new GuidObject2("DCE7B30F-44CC-4F18-8D9C-6F3D64BA3814")},
                StartOn = new DateDtoProperty() {Day = 17, Month = 3, Year = 2015},
                EndOn = new DateDtoProperty() {Day = 18, Month = 4, Year = 2016},
                PerformanceMeasure = "3.75"
            };

            _externalEducationCollection.Add(externalEducation1);
            
            _externalEducationController = new ExternalEducationController(_externalEducationServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _externalEducationController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _externalEducationController = null;
            _externalEducationCollection = null;
            _loggerMock = null;
            _externalEducationServiceMock = null;
        }

        #region ExternalEducation

        [TestMethod]
        public async Task ExternalEducationController_GetExternalEducations()
        {
            _externalEducationController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            
            _externalEducationController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = false};
           
            var tuple = new Tuple<IEnumerable<Dtos.ExternalEducation>, int>(_externalEducationCollection, 1);

            _externalEducationServiceMock
                .Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(tuple);

            var externalEducation = await _externalEducationController.GetExternalEducationsAsync(new Paging(10, 0));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await externalEducation.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ExternalEducation>>) httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.ExternalEducation>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _externalEducationCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.StartOn.Day, actual.StartOn.Day);
                Assert.AreEqual(expected.StartOn.Month, actual.StartOn.Month);
                Assert.AreEqual(expected.StartOn.Year, actual.StartOn.Year);

                Assert.AreEqual(expected.EndOn.Day, actual.EndOn.Day);
                Assert.AreEqual(expected.EndOn.Month, actual.EndOn.Month);
                Assert.AreEqual(expected.EndOn.Year, actual.EndOn.Year);

                Assert.AreEqual(expected.ClassPercentile, actual.ClassPercentile);
                Assert.AreEqual(expected.ClassRank, actual.ClassRank);
                Assert.AreEqual(expected.ClassSize, actual.ClassSize);
                Assert.AreEqual(expected.Credential.Id, actual.Credential.Id);
                Assert.AreEqual(expected.CredentialsDate, actual.CredentialsDate);
                Assert.AreEqual(expected.CreditsEarned, actual.CreditsEarned);
                Assert.AreEqual(expected.GraduatedOn, actual.GraduatedOn);
                Assert.AreEqual(expected.Disciplines.FirstOrDefault().Id, actual.Disciplines.FirstOrDefault().Id);
                Assert.AreEqual(expected.PerformanceMeasure, actual.PerformanceMeasure);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Institution.Id, actual.Institution.Id);
                Assert.AreEqual(expected.Recognition.FirstOrDefault().Id, actual.Recognition.FirstOrDefault().Id);
               
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducations_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _externalEducationController.GetExternalEducationsAsync(paging, "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducations_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _externalEducationController.GetExternalEducationsAsync(paging, "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducations_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _externalEducationController.GetExternalEducationsAsync(paging, "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducations_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _externalEducationController.GetExternalEducationsAsync(paging, "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducations_Exception()
        {
            var paging = new Paging(100, 0);
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Throws<Exception>();
            await _externalEducationController.GetExternalEducationsAsync(paging, "");
        }


        //GET v6
        //Successful
        //GetExternalEducationsAsync

        [TestMethod]
        public async Task ExternalEducationController_GetExternalEducationsAsync_Permissions()
        {
            var tuple = new Tuple<IEnumerable<Dtos.ExternalEducation>, int>(_externalEducationCollection, 1);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEducation" },
                    { "action", "GetExternalEducationsAsync" }
                };
            HttpRoute route = new HttpRoute("external-education", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _externalEducationController.Request.SetRouteData(data);
            _externalEducationController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewExternalEducation);

            var controllerContext = _externalEducationController.ControllerContext;
            var actionDescriptor = _externalEducationController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _externalEducationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(tuple);
            var externalEducation = await _externalEducationController.GetExternalEducationsAsync(new Paging(10, 0));

            Object filterObject;
            _externalEducationController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewExternalEducation));

        }

        //GET v6
        //Exception
        //GetExternalEducationsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducationsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEducation" },
                    { "action", "GetExternalEducationsAsync" }
                };
            HttpRoute route = new HttpRoute("external-education", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _externalEducationController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _externalEducationController.ControllerContext;
            var actionDescriptor = _externalEducationController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _externalEducationServiceMock.Setup(x => x.GetExternalEducationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>())).Throws<PermissionsException>();
                _externalEducationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view external-education."));
                await _externalEducationController.GetExternalEducationsAsync(paging, "");
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetExternalEducation

        #region GetExternalEducationByGuid

        [TestMethod]
        public async Task ExternalEducationController_GetExternalEducationByGuid()
        {
            _externalEducationController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = false};

            var expected = _externalEducationCollection.FirstOrDefault(x => x.Id.Equals(ExternalEducation1Guid, StringComparison.OrdinalIgnoreCase));

            _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.StartOn.Day, actual.StartOn.Day);
            Assert.AreEqual(expected.StartOn.Month, actual.StartOn.Month);
            Assert.AreEqual(expected.StartOn.Year, actual.StartOn.Year);

            Assert.AreEqual(expected.EndOn.Day, actual.EndOn.Day);
            Assert.AreEqual(expected.EndOn.Month, actual.EndOn.Month);
            Assert.AreEqual(expected.EndOn.Year, actual.EndOn.Year);

            Assert.AreEqual(expected.ClassPercentile, actual.ClassPercentile);
            Assert.AreEqual(expected.ClassRank, actual.ClassRank);
            Assert.AreEqual(expected.ClassSize, actual.ClassSize);
            Assert.AreEqual(expected.Credential.Id, actual.Credential.Id);
            Assert.AreEqual(expected.CredentialsDate, actual.CredentialsDate);
            Assert.AreEqual(expected.CreditsEarned, actual.CreditsEarned);
            Assert.AreEqual(expected.GraduatedOn, actual.GraduatedOn);
            Assert.AreEqual(expected.Disciplines.FirstOrDefault().Id, actual.Disciplines.FirstOrDefault().Id);
            Assert.AreEqual(expected.PerformanceMeasure, actual.PerformanceMeasure);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Institution.Id, actual.Institution.Id);
            Assert.AreEqual(expected.Recognition.FirstOrDefault().Id, actual.Recognition.FirstOrDefault().Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducationByGuid_PermissionsException()
        {
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducationByGuid_ArgumentException()
        {
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducationByGuid_RepositoryException()
        {
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducationByGuid_IntegrationApiException()
        {
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducationByGuid_Exception()
        {
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);
        }

        //GET by id v6
        //Successful
        //GetExternalEducationByGuidAsync

        [TestMethod]
        public async Task ExternalEducationController_GetExternalEducationByGuidAsync_Permissions()
        {
            var expected = _externalEducationCollection.FirstOrDefault(x => x.Id.Equals(ExternalEducation1Guid, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEducation" },
                    { "action", "GetExternalEducationByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("external-education", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _externalEducationController.Request.SetRouteData(data);
            _externalEducationController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewExternalEducation);

            var controllerContext = _externalEducationController.ControllerContext;
            var actionDescriptor = _externalEducationController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _externalEducationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);

            Object filterObject;
            _externalEducationController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewExternalEducation));

        }

        //GET by id v6
        //Exception
        //GetExternalEducationByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_GetExternalEducationByGuidAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEducation" },
                    { "action", "GetExternalEducationByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("external-education", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _externalEducationController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _externalEducationController.ControllerContext;
            var actionDescriptor = _externalEducationController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _externalEducationServiceMock.Setup(x => x.GetExternalEducationByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                _externalEducationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view external-education."));
                await _externalEducationController.GetExternalEducationByGuidAsync(ExternalEducation1Guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion GetExternalEducationByGuid

        #region Put

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_PutExternalEducation_Exception()
        {
            var expected = _externalEducationCollection.FirstOrDefault();
               await _externalEducationController.PutExternalEducationAsync(expected.Id, expected);
        }

        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_PostExternalEducation_Exception()
        {
            var expected = _externalEducationCollection.FirstOrDefault();         
            await _externalEducationController.PostExternalEducationAsync(expected);
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEducationController_DeleteExternalEducation_Exception()
        {
            var expected = _externalEducationCollection.FirstOrDefault();
            await _externalEducationController.DeleteExternalEducationAsync(expected.Id);
        }

        #endregion
    }
}