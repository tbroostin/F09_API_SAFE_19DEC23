// Copyright 2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{

    [TestClass]
    public class AidApplicationDemographicsControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IAidApplicationDemographicsService> _aidApplicationDemographicsServiceMock;
        private Mock<ILogger> _loggerMock;

        private AidApplicationDemographicsController _aidApplicationDemoController;

        private List<AidApplicationDemographics> _aidApplicationDemoCollection;
        private AidApplicationDemographics _aidApplicationDemoDto;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string AidApplicationDemoId = "1";
        private const string PersonId = "0001000";
        private const string AidYear = "2020";
        private const string AidApplicationType = "CALISIR";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _aidApplicationDemographicsServiceMock = new Mock<IAidApplicationDemographicsService>();
            _loggerMock = new Mock<ILogger>();

            _aidApplicationDemoCollection = new List<AidApplicationDemographics>();

            var demographics1 = new AidApplicationDemographics
            {
                Id = AidApplicationDemoId,
                PersonId = PersonId,
                AidYear = AidYear,
                ApplicationType = AidApplicationType,
                ApplicantAssignedId = "987654321",
                FirstName = "First Name",
                LastName = "Last Name",
                OrigName = "La",
                MiddleInitial = "MI",

                Address = new Dtos.FinancialAid.Address()
                {
                    AddressLine = "St 123",
                    City = "Malvern",
                    State = "PA",
                    Country = "USA",
                    ZipCode = "19355"
                },
                PhoneNumber = "001-1000000",
                AlternatePhoneNumber = "001-200000",
                BirthDate = new DateTime(2001, 10, 20),
                CitizenshipStatusType = AidApplicationCitizenshipStatus.Citizen,
                EmailAddress = "testid@ellucian.com",
                StudentTaxIdNumber ="123456789"
            };

            _aidApplicationDemoCollection.Add(demographics1);

            BuildData();

            _aidApplicationDemoController = new AidApplicationDemographicsController(_aidApplicationDemographicsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _aidApplicationDemoController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _aidApplicationDemoController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_aidApplicationDemoDto));
        }

        private void BuildData()
        {
            _aidApplicationDemoDto = new AidApplicationDemographics
            {
                Id = AidApplicationDemoId,
                PersonId = PersonId,
                AidYear = AidYear,
                ApplicationType = AidApplicationType,
                ApplicantAssignedId = "987654321",
                FirstName = "First Name",
                LastName = "Last Name",
                OrigName = "La",
                MiddleInitial = "MI",

                Address = new Dtos.FinancialAid.Address()
                {
                    AddressLine = "St 123",
                    City = "Malvern",
                    State = "PA",
                    Country = "USA",
                    ZipCode = "19355"
                },
                PhoneNumber = "001-1000000",
                AlternatePhoneNumber = "001-200000",
                BirthDate = new DateTime(2001, 10, 20),
                CitizenshipStatusType = AidApplicationCitizenshipStatus.Citizen,
                EmailAddress = "testid@ellucian.com",
                StudentTaxIdNumber = "123456789"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _aidApplicationDemoController = null;
            _aidApplicationDemoCollection = null;
            _loggerMock = null;
            _aidApplicationDemographicsServiceMock = null;
        }

        #region AidApplicationDemographics

        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics()
        {
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationDemoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationDemographics>, int>(_aidApplicationDemoCollection, 1);

            _aidApplicationDemographicsServiceMock
                .Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationDemographics>()))
                .ReturnsAsync(tuple);

            var aidApplicationDemoRecord = await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationDemoRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationDemographics>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationDemographics>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationDemoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.PersonId, actual.PersonId);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
                Assert.AreEqual(expected.ApplicationType, actual.ApplicationType);

            }
        }

        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_PersonId()
        {
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationDemoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationDemographics>, int>(_aidApplicationDemoCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationDemoController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplicationDemographics() { PersonId = PersonId });

            _aidApplicationDemographicsServiceMock
                .Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationDemographics>()))
                .ReturnsAsync(tuple);

            var aidApplicationDemoRecord = await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationDemoRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationDemographics>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationDemographics>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationDemoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_status()
        {
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationDemoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationDemographics>, int>(_aidApplicationDemoCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationDemoController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplicationDemographics() { AidYear = "2010" });

            _aidApplicationDemographicsServiceMock
                .Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationDemographics>()))
                .ReturnsAsync(tuple);

            var aidApplicationDemoRecord = await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationDemoRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationDemographics>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationDemographics>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationDemoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_classifications()
        {
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationDemoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationDemographics>, int>(_aidApplicationDemoCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationDemoController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplicationDemographics() { ApplicationType = "CALISIR" });

            _aidApplicationDemographicsServiceMock
                .Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationDemographics>()))
                .ReturnsAsync(tuple);

            var aidApplicationDemoRecord = await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationDemoRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationDemographics>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationDemographics>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationDemoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_KeyNotFoundException()
        {
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<KeyNotFoundException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<ArgumentException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<RepositoryException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<IntegrationApiException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographics_Exception()
        {
            var paging = new Paging(100, 0);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<Exception>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(paging, criteriaFilter);
        }

        //Successful
        //GetAidApplicationDemographicsAsync
        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsAsync_Permissions()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationDemoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "GetAidApplicationDemographicsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplicationDemographics);

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplicationDemographics>, int>(_aidApplicationDemoCollection, 1);

            _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationDemographicsServiceMock
                .Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationDemographics>()))
                .ReturnsAsync(tuple);
            var aidApplicationDemoRecord = await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationDemoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplicationDemographics));


        }

        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsAsync_UpdatePermissions()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationDemoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "GetAidApplicationDemographicsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationDemographics);

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplicationDemographics>, int>(_aidApplicationDemoCollection, 1);

            _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationDemographicsServiceMock
                .Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationDemographics>()))
                .ReturnsAsync(tuple);
            var aidApplicationDemoRecord = await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationDemoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationDemographics));


        }

        //Exception
        //GetAidApplicationDemographicsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "GetAidApplicationDemographicsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
                _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-application-demographics."));
                await _aidApplicationDemoController.GetAidApplicationDemographicsAsync(paging, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplicationDemographics

        #region GetAidApplicationDemographicsByGuid

        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById()
        {
            _aidApplicationDemoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expected = _aidApplicationDemoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationDemoId, StringComparison.OrdinalIgnoreCase));

            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            //Assert.AreEqual(expected.StartOn.Day, actual.StartOn.Day);
            //Assert.AreEqual(expected.StartOn.Month, actual.StartOn.Month);
            //Assert.AreEqual(expected.StartOn.Year, actual.StartOn.Year);

            //Assert.AreEqual(expected.EndOn.Day, actual.EndOn.Day);
            //Assert.AreEqual(expected.EndOn.Month, actual.EndOn.Month);
            //Assert.AreEqual(expected.EndOn.Year, actual.EndOn.Year);

            //Assert.AreEqual(expected.ClassPercentile, actual.ClassPercentile);
            //Assert.AreEqual(expected.ClassRank, actual.ClassRank);
            //Assert.AreEqual(expected.ClassSize, actual.ClassSize);
            //Assert.AreEqual(expected.Credential.Id, actual.Credential.Id);
            //Assert.AreEqual(expected.CredentialsDate, actual.CredentialsDate);
            //Assert.AreEqual(expected.CreditsEarned, actual.CreditsEarned);
            //Assert.AreEqual(expected.GraduatedOn, actual.GraduatedOn);
            //Assert.AreEqual(expected.Disciplines.FirstOrDefault().Id, actual.Disciplines.FirstOrDefault().Id);
            //Assert.AreEqual(expected.PerformanceMeasure, actual.PerformanceMeasure);
            //Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            //Assert.AreEqual(expected.Institution.Id, actual.Institution.Id);
            //Assert.AreEqual(expected.Recognition.FirstOrDefault().Id, actual.Recognition.FirstOrDefault().Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById_NullException()
        {
            await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById_KeyNotFoundException()
        {
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById_PermissionsException()
        {
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById_ArgumentException()
        {
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById_RepositoryException()
        {
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById_IntegrationApiException()
        {
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsById_Exception()
        {
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);
        }

        //Successful
        //GetAidApplicationDemographicsByIdAsync
        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsByIdAsync_Permissions()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationDemoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "GetAidApplicationDemographicsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplicationDemographics);

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);

            Object filterObject;
            _aidApplicationDemoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplicationDemographics));


        }

        [TestMethod]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsByIdAsync_UpdatePermissions()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationDemoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "GetAidApplicationDemographicsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationDemographics);

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);

            Object filterObject;
            _aidApplicationDemoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationDemographics));


        }

        //Exception
        //GetAidApplicationDemographicsByIdAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_GetAidApplicationDemographicsByIdAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "GetAidApplicationDemographicsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).Throws<PermissionsException>();
                _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-application-demographics."));
                await _aidApplicationDemoController.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplicationDemographicsById

        #region Put

        [TestMethod]
        public async Task AidApplicationDemographicsController_PUT()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, It.IsAny<AidApplicationDemographics>())).ReturnsAsync(_aidApplicationDemoDto);
            _aidApplicationDemographicsServiceMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId)).ReturnsAsync(_aidApplicationDemoDto);
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
            Assert.IsNotNull(result);

            Assert.AreEqual(_aidApplicationDemoDto.Id, result.Id);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_ValidateUpdateRequest_RequestId_Null()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(AidApplicationDemoId)).ReturnsAsync(_aidApplicationDemoDto);
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, It.IsAny<AidApplicationDemographics>())).ReturnsAsync(_aidApplicationDemoDto);
            _aidApplicationDemoDto.Id = string.Empty;
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AidApplicationDemographicsController_POST()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ReturnsAsync(_aidApplicationDemoDto);
            _aidApplicationDemoDto.Id = string.Empty;
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_POST_Null_Dto()
        {
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_POST_KeyNotFoundException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ThrowsAsync(new KeyNotFoundException());
            _aidApplicationDemoDto.Id = string.Empty;
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_POST_PermissionsException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ThrowsAsync(new PermissionsException());
            _aidApplicationDemoDto.Id = string.Empty;
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_POST_ArgumentException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ThrowsAsync(new ArgumentException());
            _aidApplicationDemoDto.Id = string.Empty;
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_POST_RepositoryException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_POST_IntegrationApiException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_POST_Exception()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_PermissionsException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto)).ThrowsAsync(new PermissionsException());
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_RepositoryException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_IntegrationApiException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_Exception1()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_KeyNotFoundException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto)).ThrowsAsync(new KeyNotFoundException());
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_ArgumentException()
        {
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto)).ThrowsAsync(new ArgumentException());
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_ValidateUpdateRequest_Id_Null_Exception()
        {
            await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(string.Empty, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_ValidateUpdateRequest_Request_Null_Exception()
        {
            await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_ValidateUpdateRequest_EmptyGuid_Null_Exception()
        {
            await _aidApplicationDemoController.PutAidApplicationDemographicsAsync("1", _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_ValidateUpdateRequest_RequestIdIsEmptyGuid_Null_Exception()
        {
            _aidApplicationDemoDto.Id = "";
            await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_ValidateUpdateRequest_GuidsNotMatching_Exception()
        {
            _aidApplicationDemoDto.Id = "2";
            await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographics_Exception()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault();
            await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(expected.Id, expected);
        }
                
        //Successful
        //PutAidApplicationDemographicsAsync
        [TestMethod]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographicsAsync_Permissions()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationDemoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "PutAidApplicationDemographicsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationDemographics);

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationDemographicsServiceMock.Setup(x => x.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(_aidApplicationDemoDto);
            _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, It.IsAny<AidApplicationDemographics>())).ReturnsAsync(_aidApplicationDemoDto);
            var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);

            Object filterObject;
            _aidApplicationDemoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationDemographics));


        }

        //Exception
        //PutAidApplicationDemographicsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PutAidApplicationDemographicsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "PutAidApplicationDemographicsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationDemographicsServiceMock.Setup(i => i.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update aid-application-demographics."));
                var result = await _aidApplicationDemoController.PutAidApplicationDemographicsAsync(AidApplicationDemoId, _aidApplicationDemoDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Successful
        //PostAidApplicationDemographicsAsync
        [TestMethod]
        public async Task AidApplicationDemographicsController_PostAidApplicationDemographicsAsync_Permissions()
        {
            _aidApplicationDemoDto.Id = string.Empty;
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "PostAidApplicationDemographicsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);
            _aidApplicationDemoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationDemographics);

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ReturnsAsync(_aidApplicationDemoDto);
            var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);

            Object filterObject;
            _aidApplicationDemoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationDemographics));


        }

        //Exception
        //PostAidApplicationDemographicsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PostAidApplicationDemographicsAsync_Invalid_Permissions()
        {
            _aidApplicationDemoDto.Id = string.Empty;
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationDemographics" },
                    { "action", "PostAidApplicationDemographicsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationDemographics", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationDemoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationDemoController.ControllerContext;
            var actionDescriptor = _aidApplicationDemoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationDemographicsServiceMock.Setup(i => i.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationDemographicsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create aid-application-demographics."));
                var result = await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(_aidApplicationDemoDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_PostAidApplicationDemographics_Exception()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault();
            await _aidApplicationDemoController.PostAidApplicationDemographicsAsync(expected);
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationDemographicsController_DeleteAidApplicationDemographicsAsync_Exception()
        {
            var expected = _aidApplicationDemoCollection.FirstOrDefault();
            await _aidApplicationDemoController.DeleteAidApplicationDemographicsAsync(expected.Id);
        }

        #endregion
    }
    
}
