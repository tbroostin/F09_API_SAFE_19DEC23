// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class BudgetPhaseLineItemsControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IBudgetPhaseLineItemsService> budgetPhaseLineItemsServiceMock;
        private Mock<ILogger> loggerMock;
        private BudgetPhaseLineItemsController budgetPhaseLineItemsController;
        private List<Dtos.BudgetPhaseLineItems> budgetPhaseLineItemsCollection;
        private Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int> budgetPhaseLineItemsCollectionTuple;
        private IList<EthosExtensibleData> extendData;
        int offset = 0;
        int limit = 2;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            budgetPhaseLineItemsServiceMock = new Mock<IBudgetPhaseLineItemsService>();
            loggerMock = new Mock<ILogger>();
            budgetPhaseLineItemsCollection = new List<Dtos.BudgetPhaseLineItems>();

            BuildData();

            budgetPhaseLineItemsController = new BudgetPhaseLineItemsController(budgetPhaseLineItemsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            budgetPhaseLineItemsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            budgetPhaseLineItemsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            budgetPhaseLineItemsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            budgetPhaseLineItemsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(budgetPhaseLineItemsCollection.FirstOrDefault()));
        }

        private void BuildData()
        {
            budgetPhaseLineItemsCollection = new List<Dtos.BudgetPhaseLineItems>()
            {
                new Dtos.BudgetPhaseLineItems()
                {
                    Id = "83f78f38-cb00-403b-a107-557dabf0f451"
                },
                new Dtos.BudgetPhaseLineItems()
                {
                    Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"
                }
            };

            extendData = new List<EthosExtensibleData>() { };

            budgetPhaseLineItemsCollectionTuple = new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(budgetPhaseLineItemsCollection, budgetPhaseLineItemsCollection.Count);
            budgetPhaseLineItemsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            budgetPhaseLineItemsServiceMock.Setup(s => s.GetExtendedEthosDataByResource(It.IsAny<EthosResourceRouteInfo>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<EthosExtensibleData>());
            budgetPhaseLineItemsServiceMock.Setup(s => s.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), true)).ReturnsAsync(budgetPhaseLineItemsCollection.FirstOrDefault());
        }

        [TestCleanup]
        public void Cleanup()
        {
            budgetPhaseLineItemsController = null;
            budgetPhaseLineItemsCollection = null;
            loggerMock = null;
            budgetPhaseLineItemsServiceMock = null;
        }

        #region GETALL

        [TestMethod]
        public async Task BudgetPhaseLineItemsController_GetAll_ValidateFields_Nocache()
        {
            budgetPhaseLineItemsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), false)).ReturnsAsync(budgetPhaseLineItemsCollectionTuple);

            var items = await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(null);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await items.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Dtos.BudgetPhaseLineItems>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.BudgetPhaseLineItems>;

            Assert.AreEqual(budgetPhaseLineItemsCollection.Count, actuals.Count());
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemsController_GetAll_ValidateFields_Cache()
        {
            budgetPhaseLineItemsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), true)).ReturnsAsync(budgetPhaseLineItemsCollectionTuple);

            var items = await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(new Web.Http.Models.Paging(limit, offset));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await items.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Dtos.BudgetPhaseLineItems>>)httpResponseMessage.Content)
                                .Value as IEnumerable<Dtos.BudgetPhaseLineItems>;
            Assert.IsNotNull(actuals);

            Assert.AreEqual(budgetPhaseLineItemsCollection.Count, actuals.Count());
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemsController_GetAll_With_Criteria_Object()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(budgetPhaseLineItemsCollectionTuple);

            string criteria = "{'budgetPhase': {'id': '83f78f38-cb00-403b-a107-557dabf0f451'}, 'accountingStringComponentValues': {'id': '83f78f38-cb00-403b-a107-557dabf0f451'}}";
            QueryStringFilter queryStrFilter = new QueryStringFilter("criteria", criteria);
            var filterObject = budgetPhaseLineItemsCollection.FirstOrDefault();
            filterObject.AccountingStringComponentValues = new List<Dtos.GuidObject2>()
            {
                new Dtos.GuidObject2("83f78f38-cb00-403b-a107-557dabf0f451")
            };
            budgetPhaseLineItemsController.Request.Properties.Add("FilterObjectcriteria", JObject.FromObject(filterObject));

            budgetPhaseLineItemsServiceMock.Setup(s => s.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), true)).ReturnsAsync(filterObject);

            var items = await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(new Web.Http.Models.Paging(limit, offset), It.IsAny< QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await items.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Dtos.BudgetPhaseLineItems>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.BudgetPhaseLineItems>;

            Assert.IsNotNull(actuals);

            Assert.AreEqual(budgetPhaseLineItemsCollection.Count, actuals.Count());
        }

        [TestMethod]
        public async Task BudgetPhaseLineItemsController_GetAll_With_Criteria_Object_ArgumentNullException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(budgetPhaseLineItemsCollectionTuple);

            string criteria = "{'budgetPhase': {'id': '83f78f38-cb00-403b-a107-557dabf0f451'}, 'accountingStringComponentValues': {'id': '83f78f38-cb00-403b-a107-557dabf0f451'}}";
            QueryStringFilter queryStrFilter = new QueryStringFilter("criteria", criteria);
            var filterObject = budgetPhaseLineItemsCollection.FirstOrDefault();
            filterObject.AccountingStringComponentValues = new List<Dtos.GuidObject2>()
            {
                new Dtos.GuidObject2("")
            };
            budgetPhaseLineItemsController.Request.Properties.Add("FilterObjectcriteria", JObject.FromObject(filterObject));
            budgetPhaseLineItemsController.Request.Properties.Add("EmptyFilterProperties", true);
            budgetPhaseLineItemsServiceMock.Setup(s => s.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), true)).ReturnsAsync(filterObject);

            var items = await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(new Web.Http.Models.Paging(limit, offset), It.IsAny<QueryStringFilter>());
            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await items.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Dtos.BudgetPhaseLineItems>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.BudgetPhaseLineItems>;

            Assert.AreEqual(0, actuals.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItems_KeyNotFoundException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItems_PermissionsException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItems_ArgumentException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItems_RepositoryException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
               .Throws<RepositoryException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItems_IntegrationApiException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
              .Throws<IntegrationApiException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItems_Exception()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
             .Throws<Exception>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        #endregion

        #region GET

        [TestMethod]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuidAsync_ValidateFields()
        {
            budgetPhaseLineItemsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = budgetPhaseLineItemsCollection.FirstOrDefault();
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(expected.Id, false)).ReturnsAsync(expected);

            var actual = await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(expected.Id);

            Assert.IsNotNull(actual);

            Assert.AreEqual(actual.Id, expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuid_Exception_On_Empty_Guid()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuid_KeyNotFoundException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuid_PermissionsException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuid_ArgumentException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuid_RepositoryException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuid_IntegrationApiException()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_GetBudgetPhaseLineItemsByGuid_Exception()
        {
            budgetPhaseLineItemsServiceMock.Setup(x => x.GetBudgetPhaseLineItemsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await budgetPhaseLineItemsController.GetBudgetPhaseLineItemsByGuidAsync(string.Empty);
        }

        #endregion

        #region NOTSUPPORTED

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_PostBudgetPhaseLineItemsAsync_NotSupported()
        {
            await budgetPhaseLineItemsController.PostBudgetPhaseLineItemsAsync(new Dtos.BudgetPhaseLineItems() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_PutBudgetPhaseLineItemsAsync_NotSupported()
        {
            await budgetPhaseLineItemsController.PutBudgetPhaseLineItemsAsync(Guid.NewGuid().ToString(), new Dtos.BudgetPhaseLineItems() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetPhaseLineItemsController_DeleteBudgetPhaseLineItemsAsync_NotSupported()
        {
            await budgetPhaseLineItemsController.DeleteBudgetPhaseLineItemsAsync(Guid.NewGuid().ToString());
        }

        #endregion
    }
}
