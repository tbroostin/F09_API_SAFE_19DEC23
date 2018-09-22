// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayPeriodsControllerTestsv12
    {

        #region Test Context

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        private PayPeriodsController PayPeriodsController;
        private Mock<IPayPeriodsRepository> payPeriodRepositoryMock;
        private IPayPeriodsRepository payPeriodRepository;
        private IAdapterRegistry AdapterRegistry;
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayPeriod> allPayPeriodEntities;
        ILogger logger = new Mock<ILogger>().Object;
        private Mock<IPayPeriodsService> payPeriodServiceMock;
        private IPayPeriodsService payPeriodService;
        List<Ellucian.Colleague.Dtos.PayPeriods> PayPeriodList;
        private string payPeriodsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            payPeriodRepositoryMock = new Mock<IPayPeriodsRepository>();
            payPeriodRepository = payPeriodRepositoryMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.PayPeriod, Dtos.PayPeriods>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            payPeriodServiceMock = new Mock<IPayPeriodsService>();
            payPeriodService = payPeriodServiceMock.Object;

            allPayPeriodEntities = new TestPayPeriodsRepository().GetPayPeriods();
            PayPeriodList = new List<Dtos.PayPeriods>();

            PayPeriodsController = new PayPeriodsController(payPeriodService, logger);
            PayPeriodsController.Request = new HttpRequestMessage();
            PayPeriodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var payPeriod in allPayPeriodEntities)
            {
                var target = ConvertPayPeriodsEntityToDto(payPeriod);
                PayPeriodList.Add(target);
            }

            PayPeriodsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(PayPeriodList.FirstOrDefault()));
        }

        [TestCleanup]
        public void Cleanup()
        {
            PayPeriodsController = null;
            payPeriodRepository = null;
        }

        [TestMethod]
        public async Task PayPeriodsController_GetPayPeriodsAsync()
        {
            PayPeriodsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.PayPeriods>, int>(PayPeriodList, 5);

            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var payPeriods = await PayPeriodsController.GetPayPeriodsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await payPeriods.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayPeriods>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(payPeriods is IHttpActionResult);

            foreach (var payPeriodsDto in PayPeriodList)
            {
                var emp = results.FirstOrDefault(i => i.Id == payPeriodsDto.Id);

                Assert.AreEqual(payPeriodsDto.Id, emp.Id);
                Assert.AreEqual(payPeriodsDto.Title, emp.Title);

            }
        }

        [TestMethod]
        public async Task PayPeriodsController_GetPayPeriodsAsync_PayCycleFilters()
        {
            PayPeriodsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.PayPeriods>, int>(PayPeriodList, 5);

            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            //string criteria = @"{'payCycle':[{'id':'e9e6837f-2c51-431b-9069-4ac4c0da3041'}]}";
            Ellucian.Web.Http.Models.QueryStringFilter criteria = new Web.Http.Models.QueryStringFilter("criteria", "{'payCycle':[{'id':'e9e6837f-2c51-431b-9069-4ac4c0da3041'}]}");
            var payPeriods = await PayPeriodsController.GetPayPeriodsAsync(new Paging(10, 0), criteria);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await payPeriods.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayPeriods>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(payPeriods is IHttpActionResult);

            foreach (var payPeriodsDto in PayPeriodList)
            {
                var emp = results.FirstOrDefault(i => i.Id == payPeriodsDto.Id);

                Assert.AreEqual(payPeriodsDto.Id, emp.Id);
                Assert.AreEqual(payPeriodsDto.Title, emp.Title);
            }
        }

        [TestMethod]
        public async Task PayPeriodsController_GetPayPeriodsAsync_StartOnFilter()
        {
            PayPeriodsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.PayPeriods>, int>(PayPeriodList, 5);

            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            //string criteria = @"{'startOn':'2016-05-01'}";
            Ellucian.Web.Http.Models.QueryStringFilter criteria = new Web.Http.Models.QueryStringFilter("criteria", "{'startOn':'2016-05-01'}");
            var payPeriods = await PayPeriodsController.GetPayPeriodsAsync(new Paging(10, 0), criteria);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await payPeriods.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayPeriods>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(payPeriods is IHttpActionResult);

            foreach (var payPeriodsDto in PayPeriodList)
            {
                var emp = results.FirstOrDefault(i => i.Id == payPeriodsDto.Id);

                Assert.AreEqual(payPeriodsDto.Id, emp.Id);
                Assert.AreEqual(payPeriodsDto.Title, emp.Title);
            }
        }

        [TestMethod]
        public async Task PayPeriodsController_GetPayPeriodsAsync_EndOnFilter()
        {
            PayPeriodsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var tuple = new Tuple<IEnumerable<Dtos.PayPeriods>, int>(PayPeriodList, 5);

            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            //string criteria = @"{'endOn':'2016-05-01'}";
            Ellucian.Web.Http.Models.QueryStringFilter criteria = new Web.Http.Models.QueryStringFilter("criteria", "{'endOn':'2016-05-01'}");
            var payPeriods = await PayPeriodsController.GetPayPeriodsAsync(new Paging(10, 0), criteria);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await payPeriods.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayPeriods>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(payPeriods is IHttpActionResult);

            foreach (var payPeriodsDto in PayPeriodList)
            {
                var emp = results.FirstOrDefault(i => i.Id == payPeriodsDto.Id);

                Assert.AreEqual(payPeriodsDto.Id, emp.Id);
                Assert.AreEqual(payPeriodsDto.Title, emp.Title);
            }
        }

        [TestMethod]
        public async Task GetPayPeriodsByGuidAsync_Validate()
        {
            var thisPayPeriod = PayPeriodList.Where(m => m.Id == payPeriodsGuid).FirstOrDefault();

            payPeriodServiceMock.Setup(x => x.GetPayPeriodsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPayPeriod);

            var payPeriod = await PayPeriodsController.GetPayPeriodsByGuidAsync(payPeriodsGuid);
            Assert.AreEqual(thisPayPeriod.Id, payPeriod.Id);
            Assert.AreEqual(thisPayPeriod.Title, payPeriod.Title);
        }

        [TestMethod]
        public async Task PayPeriodsController_GetHedmAsync_CacheControlNotNull()
        {
            PayPeriodsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            PayPeriodsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            var tuple = new Tuple<IEnumerable<Dtos.PayPeriods>, int>(PayPeriodList, 5);

            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var payPeriods = await PayPeriodsController.GetPayPeriodsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await payPeriods.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayPeriods>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(payPeriods is IHttpActionResult);

            foreach (var payPeriodsDto in PayPeriodList)
            {
                var emp = results.FirstOrDefault(i => i.Id == payPeriodsDto.Id);

                Assert.AreEqual(payPeriodsDto.Id, emp.Id);
                Assert.AreEqual(payPeriodsDto.Title, emp.Title);
            }
        }

        [TestMethod]
        public async Task PayPeriodsController_GetHedmAsync_NoCache()
        {
            PayPeriodsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            PayPeriodsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            PayPeriodsController.Request.Headers.CacheControl.NoCache = true;

            var tuple = new Tuple<IEnumerable<Dtos.PayPeriods>, int>(PayPeriodList, 5);

            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var payPeriods = await PayPeriodsController.GetPayPeriodsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await payPeriods.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayPeriods>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(payPeriods is IHttpActionResult);

            foreach (var payPeriodsDto in PayPeriodList)
            {
                var emp = results.FirstOrDefault(i => i.Id == payPeriodsDto.Id);

                Assert.AreEqual(payPeriodsDto.Id, emp.Id);
                Assert.AreEqual(payPeriodsDto.Title, emp.Title);
            }
        }

        [TestMethod]
        public async Task PayPeriodsController_GetHedmAsync_Cache()
        {
            PayPeriodsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            PayPeriodsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            PayPeriodsController.Request.Headers.CacheControl.NoCache = false;

            var tuple = new Tuple<IEnumerable<Dtos.PayPeriods>, int>(PayPeriodList, 5);

            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var payPeriods = await PayPeriodsController.GetPayPeriodsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await payPeriods.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayPeriods>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(payPeriods is IHttpActionResult);

            foreach (var payPeriodsDto in PayPeriodList)
            {
                var emp = results.FirstOrDefault(i => i.Id == payPeriodsDto.Id);

                Assert.AreEqual(payPeriodsDto.Id, emp.Id);
                Assert.AreEqual(payPeriodsDto.Title, emp.Title);
            }
        }

        [TestMethod]
        public async Task PayPeriodsController_GetByIdHedmAsync()
        {
            var thisPayPeriod = PayPeriodList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

            payPeriodServiceMock.Setup(x => x.GetPayPeriodsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPayPeriod);

            var payPeriod = await PayPeriodsController.GetPayPeriodsByGuidAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
            Assert.AreEqual(thisPayPeriod.Id, payPeriod.Id);
            Assert.AreEqual(thisPayPeriod.Title, payPeriod.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetThrowsIntAppiExc()
        {
            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await PayPeriodsController.GetPayPeriodsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetThrowsIntAppiKeyNotFoundExc()
        {
            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();

            await PayPeriodsController.GetPayPeriodsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetThrowsIntAppiArgumentExc()
        {
            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

            await PayPeriodsController.GetPayPeriodsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetThrowsIntAppiRepositoryExc()
        {
            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

            await PayPeriodsController.GetPayPeriodsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetThrowsIntAppiIntegrationExc()
        {
            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

            await PayPeriodsController.GetPayPeriodsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetThrowsIntAppiPermissionExc()
        {
            payPeriodServiceMock.Setup(s => s.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

            await PayPeriodsController.GetPayPeriodsAsync(new Paging(100, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetByIdThrowsExc()
        {
            await PayPeriodsController.GetPayPeriodsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetByIdThrowsIntAppiExc()
        {
            payPeriodServiceMock.Setup(gc => gc.GetPayPeriodsByGuidAsync(It.IsAny<string>())).Throws<Exception>();

            await PayPeriodsController.GetPayPeriodsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetByIdThrowsIntAppiPermissionExc()
        {
            payPeriodServiceMock.Setup(gc => gc.GetPayPeriodsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();

            await PayPeriodsController.GetPayPeriodsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetByIdThrowsIntAppiKeyNotFoundExc()
        {
            payPeriodServiceMock.Setup(gc => gc.GetPayPeriodsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

            await PayPeriodsController.GetPayPeriodsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetByIdThrowsIntAppiIntegrationExc()
        {
            payPeriodServiceMock.Setup(gc => gc.GetPayPeriodsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();

            await PayPeriodsController.GetPayPeriodsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetByIdThrowsIntAppiArgumentExc()
        {
            payPeriodServiceMock.Setup(gc => gc.GetPayPeriodsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();

            await PayPeriodsController.GetPayPeriodsByGuidAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayPeriodsController_GetByIdThrowsIntAppiRepositoryExc()
        {
            payPeriodServiceMock.Setup(gc => gc.GetPayPeriodsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();

            await PayPeriodsController.GetPayPeriodsByGuidAsync("sdjfh");
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PayPeriod domain entity to its corresponding PayPeriod DTO
        /// </summary>
        /// <param name="source">PayPeriod domain entity</param>
        /// <returns>PayPeriod DTO</returns>
        private Ellucian.Colleague.Dtos.PayPeriods ConvertPayPeriodsEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.PayPeriod source)
        {
            var payPeriod = new Ellucian.Colleague.Dtos.PayPeriods();
            payPeriod.Id = source.Id;
            payPeriod.Title = source.Description;
            payPeriod.StartOn = source.StartDate2;
            payPeriod.EndOn = source.EndDate2;
            payPeriod.PayCycle = new Dtos.GuidObject2(source.PayCycle);

            return payPeriod;
        }
    }
}