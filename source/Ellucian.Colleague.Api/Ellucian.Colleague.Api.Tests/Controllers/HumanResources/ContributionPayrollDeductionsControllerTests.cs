// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System.Net.Http.Headers;
using Ellucian.Colleague.Dtos;
using System.Threading;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;
using System.Collections;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class ContributionPayrollDeductionsControllerTests
    {

        #region Test Context

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #endregion

        private ContributionPayrollDeductionsController _contributionPayrollDeductionsController;
        private Mock<IContributionPayrollDeductionsRepository> _contributionPayrollDeductionsRepositoryMock;
        private IContributionPayrollDeductionsRepository _contributionPayrollDeductionsRepository;
        private IAdapterRegistry _adapterRegistry;
        private List<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeduction> _allContributionPayrollDeductionsEntities;
        private readonly ILogger _logger = new Mock<ILogger>().Object;
        private Mock<IContributionPayrollDeductionsService> _contributionPayrollDeductionsServiceMock;
        private IContributionPayrollDeductionsService _contributionPayrollDeductionsService;
        private List<Ellucian.Colleague.Dtos.ContributionPayrollDeductions> _contributionPayrollDeductionsList;
        private readonly string contributionPayrollDeductionsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
        private readonly string arrangementGuid = "775c69ff-280b-4ed3-9474-662a43616a8a";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _contributionPayrollDeductionsRepositoryMock = new Mock<IContributionPayrollDeductionsRepository>();
            _contributionPayrollDeductionsRepository = _contributionPayrollDeductionsRepositoryMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            _adapterRegistry = new AdapterRegistry(adapters, _logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeduction, Dtos.ContributionPayrollDeductions>(_adapterRegistry, _logger);
            _adapterRegistry.AddAdapter(testAdapter);

            _contributionPayrollDeductionsServiceMock = new Mock<IContributionPayrollDeductionsService>();
            _contributionPayrollDeductionsService = _contributionPayrollDeductionsServiceMock.Object;

            _allContributionPayrollDeductionsEntities = new List<PayrollDeduction>()
            {
                new PayrollDeduction(contributionPayrollDeductionsGuid, "123", "456", new DateTime(2017, 01, 01), "USD", 52  ),
                new PayrollDeduction("905c69ff-280b-4ed3-9474-662a43616a8a", "123", "456", new DateTime(2017, 01, 01), "USD", 60  )
            };
            _contributionPayrollDeductionsList = new List<Dtos.ContributionPayrollDeductions>();

            _contributionPayrollDeductionsController = new ContributionPayrollDeductionsController(_contributionPayrollDeductionsService, _logger);
            _contributionPayrollDeductionsController.Request = new HttpRequestMessage();
            _contributionPayrollDeductionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var contributionPayrollDeductions in _allContributionPayrollDeductionsEntities)
            {
                Dtos.ContributionPayrollDeductions target = ConvertContributionPayrollDeductionsEntityToDto(contributionPayrollDeductions);
                target.Arrangement = new GuidObject2(arrangementGuid);
                _contributionPayrollDeductionsList.Add(target);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            _contributionPayrollDeductionsController = null;
            _contributionPayrollDeductionsRepository = null;
            _adapterRegistry = null;
            _allContributionPayrollDeductionsEntities = null;
            _contributionPayrollDeductionsList = null;
            _contributionPayrollDeductionsRepositoryMock = null;
            _contributionPayrollDeductionsService = null;
            _contributionPayrollDeductionsServiceMock = null;
        }

        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetContributionPayrollDeductionsAsync()
        {
            _contributionPayrollDeductionsController.Request = new System.Net.Http.HttpRequestMessage() {RequestUri = new Uri("http://localhost")};

            var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);

            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).ReturnsAsync(tuple);
            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(10, 0), It.IsAny<QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await contributionPayrollDeductions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.ContributionPayrollDeductions> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>>) httpResponseMessage.Content).Value as IEnumerable<Dtos.ContributionPayrollDeductions>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(contributionPayrollDeductions is IHttpActionResult);

            foreach (var contributionPayrollDeductionsDto in _contributionPayrollDeductionsList)
            {
                var emp = results.FirstOrDefault(i => i.Id == contributionPayrollDeductionsDto.Id);

                Assert.AreEqual(contributionPayrollDeductionsDto.Id, emp.Id);
                Assert.AreEqual(contributionPayrollDeductionsDto.Amount, emp.Amount);
            }
        }

        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetContributionPayrollDeductions2Async_WithFilters()
        {
            var filterGroupName = "criteria";

            _contributionPayrollDeductionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

            _contributionPayrollDeductionsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };
            _contributionPayrollDeductionsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.ContributionPayrollDeductions() { Arrangement = new GuidObject2() { Id = arrangementGuid } });

            IEnumerable<Dtos.ContributionPayrollDeductions> resultList = _contributionPayrollDeductionsList.Where(x => x.Arrangement.Id == arrangementGuid);

            var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(resultList, 5);

            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            QueryStringFilter criteria = new QueryStringFilter("criteria", "{'arrangement':{'id':\'" + arrangementGuid + "\'}}");

            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(10, 0), criteria);

            Assert.IsNotNull(contributionPayrollDeductions);

            var cancelToken = new System.Threading.CancellationToken(false);

            var httpResponseMessage = await contributionPayrollDeductions.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>>) httpResponseMessage.Content).Value as IEnumerable<Dtos.ContributionPayrollDeductions>;

           
            Assert.IsTrue(contributionPayrollDeductions is IHttpActionResult);

            foreach (var contributionPayrollDeductionsDto in resultList)
            {
                var emp = results.FirstOrDefault(i => i.Id == contributionPayrollDeductionsDto.Id);

                Assert.AreEqual(contributionPayrollDeductionsDto.Id, emp.Id);
                Assert.AreEqual(contributionPayrollDeductionsDto.Arrangement, emp.Arrangement);
            }
        }
        
        [TestMethod]
        public async Task GetContributionPayrollDeductionsByGuidAsync_Validate()
        {
            var thisContributionPayrollDeductions = _contributionPayrollDeductionsList.Where(m => m.Id == contributionPayrollDeductionsGuid).FirstOrDefault();

            _contributionPayrollDeductionsServiceMock.Setup(x => x.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisContributionPayrollDeductions);

            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
            Assert.AreEqual(thisContributionPayrollDeductions.Id, contributionPayrollDeductions.Id);
            Assert.AreEqual(thisContributionPayrollDeductions.Amount, contributionPayrollDeductions.Amount);
        }

        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetHedmAsync_CacheControlNotNull()
        {
            _contributionPayrollDeductionsController.Request = new System.Net.Http.HttpRequestMessage() {RequestUri = new Uri("http://localhost")};
            _contributionPayrollDeductionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);

            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).ReturnsAsync(tuple);
            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(10, 0), It.IsAny<QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await contributionPayrollDeductions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.ContributionPayrollDeductions> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>>) httpResponseMessage.Content).Value as IEnumerable<Dtos.ContributionPayrollDeductions>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(contributionPayrollDeductions is IHttpActionResult);

            foreach (var contributionPayrollDeductionsDto in _contributionPayrollDeductionsList)
            {
                var emp = results.FirstOrDefault(i => i.Id == contributionPayrollDeductionsDto.Id);

                Assert.AreEqual(contributionPayrollDeductionsDto.Id, emp.Id);
                Assert.AreEqual(contributionPayrollDeductionsDto.Amount, emp.Amount);
            }
        }

        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetHedmAsync_NoCache()
        {
            _contributionPayrollDeductionsController.Request = new System.Net.Http.HttpRequestMessage() {RequestUri = new Uri("http://localhost")};
            _contributionPayrollDeductionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            _contributionPayrollDeductionsController.Request.Headers.CacheControl.NoCache = true;

            var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);

            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).ReturnsAsync(tuple);
            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(10, 0), It.IsAny<QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await contributionPayrollDeductions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.ContributionPayrollDeductions> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>>) httpResponseMessage.Content).Value as IEnumerable<Dtos.ContributionPayrollDeductions>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(contributionPayrollDeductions is IHttpActionResult);

            foreach (var contributionPayrollDeductionsDto in _contributionPayrollDeductionsList)
            {
                var emp = results.FirstOrDefault(i => i.Id == contributionPayrollDeductionsDto.Id);

                Assert.AreEqual(contributionPayrollDeductionsDto.Id, emp.Id);
                Assert.AreEqual(contributionPayrollDeductionsDto.Amount, emp.Amount);
            }
        }

        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetHedmAsync_Cache()
        {
            _contributionPayrollDeductionsController.Request = new System.Net.Http.HttpRequestMessage() {RequestUri = new Uri("http://localhost")};
            _contributionPayrollDeductionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            _contributionPayrollDeductionsController.Request.Headers.CacheControl.NoCache = false;

            var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);

            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).ReturnsAsync(tuple);
            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(10, 0), It.IsAny<QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await contributionPayrollDeductions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.ContributionPayrollDeductions> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>>) httpResponseMessage.Content).Value as IEnumerable<Dtos.ContributionPayrollDeductions>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(contributionPayrollDeductions is IHttpActionResult);

            foreach (var contributionPayrollDeductionsDto in _contributionPayrollDeductionsList)
            {
                var emp = results.FirstOrDefault(i => i.Id == contributionPayrollDeductionsDto.Id);

                Assert.AreEqual(contributionPayrollDeductionsDto.Id, emp.Id);
                Assert.AreEqual(contributionPayrollDeductionsDto.Amount, emp.Amount);
            }
        }

        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetByIdHedmAsync()
        {
            var thisContributionPayrollDeductions = _contributionPayrollDeductionsList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

            _contributionPayrollDeductionsServiceMock.Setup(x => x.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisContributionPayrollDeductions);

            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
            Assert.AreEqual(thisContributionPayrollDeductions.Id, contributionPayrollDeductions.Id);
            Assert.AreEqual(thisContributionPayrollDeductions.Amount, contributionPayrollDeductions.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetThrowsIntAppiExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).Throws<Exception>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetThrowsIntAppiKeyNotFoundExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetThrowsIntAppiArgumentExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).Throws<ArgumentException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetThrowsIntAppiRepositoryExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetThrowsIntAppiIntegrationExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetThrowsIntAppiPermissionExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<bool>())).Throws<PermissionsException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetByIdThrowsExc()
        {
            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetByIdThrowsIntAppiExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(gc => gc.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).Throws<Exception>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetByIdThrowsIntAppiPermissionExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(gc => gc.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetByIdThrowsIntAppiKeyNotFoundExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(gc => gc.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetByIdThrowsIntAppiIntegrationExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(gc => gc.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetByIdThrowsIntAppiArgumentExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(gc => gc.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetByIdThrowsIntAppiRepositoryExc()
        {
            _contributionPayrollDeductionsServiceMock.Setup(gc => gc.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();

            await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_PostThrowsIntAppiExc()
        {
            await _contributionPayrollDeductionsController.PostContributionPayrollDeductionsAsync(_contributionPayrollDeductionsList[0]);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_PutThrowsIntAppiExc()
        {
            var result = await _contributionPayrollDeductionsController.PutContributionPayrollDeductionsAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023", _contributionPayrollDeductionsList[0]);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_DeleteThrowsIntAppiExc()
        {
            await _contributionPayrollDeductionsController.DeleteContributionPayrollDeductionsAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
        }

        //Permissions Tests

        //Success
        //Get 8
        //GetContributionPayrollDeductionsAsync
        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetContributionPayrollDeductionsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "ContributionPayrollDeductions" },
                { "action", "GetContributionPayrollDeductionsAsync" }
            };
            HttpRoute route = new HttpRoute("contribution-payroll-deductions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _contributionPayrollDeductionsController.Request.SetRouteData(data);
            _contributionPayrollDeductionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewContributionPayrollDeductions);

            var controllerContext = _contributionPayrollDeductionsController.ControllerContext;
            var actionDescriptor = _contributionPayrollDeductionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);

            _contributionPayrollDeductionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(10, 0), It.IsAny<QueryStringFilter>());

            Object filterObject;
            _contributionPayrollDeductionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewContributionPayrollDeductions));

        }

        //Exception
        //Get 8
        //GetContributionPayrollDeductionsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetContributionPayrollDeductionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "ContributionPayrollDeductions" },
                { "action", "GetContributionPayrollDeductionsAsync" }
            };
            HttpRoute route = new HttpRoute("contribution-payroll-deductions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _contributionPayrollDeductionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _contributionPayrollDeductionsController.ControllerContext;
            var actionDescriptor = _contributionPayrollDeductionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);

                _contributionPayrollDeductionsServiceMock.Setup(s => s.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                _contributionPayrollDeductionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view contribution-payroll-deductions."));
                await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        //Success
        //Get By Id 8
        //GetContributionPayrollDeductionsByIdAsync
        [TestMethod]
        public async Task ContributionPayrollDeductionsController_GetContributionPayrollDeductionsByIdAsync_Permissions()
        {
            var thisContributionPayrollDeductions = _contributionPayrollDeductionsList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "ContributionPayrollDeductions" },
                { "action", "GetContributionPayrollDeductionsByIdAsync" }
            };
            HttpRoute route = new HttpRoute("contribution-payroll-deductions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _contributionPayrollDeductionsController.Request.SetRouteData(data);
            _contributionPayrollDeductionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewContributionPayrollDeductions);

            var controllerContext = _contributionPayrollDeductionsController.ControllerContext;
            var actionDescriptor = _contributionPayrollDeductionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);
            _contributionPayrollDeductionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _contributionPayrollDeductionsServiceMock.Setup(x => x.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisContributionPayrollDeductions);
            var contributionPayrollDeductions = await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");


            Object filterObject;
            _contributionPayrollDeductionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewContributionPayrollDeductions));

        }


        //Exception
        //Get By Id 8
        //GetContributionPayrollDeductionsByIdAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContributionPayrollDeductionsController_GetContributionPayrollDeductionsByIdAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "ContributionPayrollDeductions" },
                { "action", "GetContributionPayrollDeductionsByIdAsync" }
            };
            HttpRoute route = new HttpRoute("contribution-payroll-deductions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _contributionPayrollDeductionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _contributionPayrollDeductionsController.ControllerContext;
            var actionDescriptor = _contributionPayrollDeductionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.ContributionPayrollDeductions>, int>(_contributionPayrollDeductionsList, 5);

                _contributionPayrollDeductionsServiceMock.Setup(gc => gc.GetContributionPayrollDeductionsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                _contributionPayrollDeductionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view contribution-payroll-deductions."));
                await _contributionPayrollDeductionsController.GetContributionPayrollDeductionsByIdAsync(contributionPayrollDeductionsGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ContributionPayrollDeductions domain entity to its corresponding ContributionPayrollDeductions DTO
        /// </summary>
        /// <param name="source">ContributionPayrollDeductions domain entity</param>
        /// <returns>ContributionPayrollDeductions DTO</returns>
        private Ellucian.Colleague.Dtos.ContributionPayrollDeductions ConvertContributionPayrollDeductionsEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeduction source)
        {
            var contributionPayrollDeductions = new Ellucian.Colleague.Dtos.ContributionPayrollDeductions();
            contributionPayrollDeductions.Id = source.Guid;
            //contributionPayrollDeductions.Arrangement = new Dtos.GuidObject2(source.ArrangementGuid);

            return contributionPayrollDeductions;
        }
    }
}
