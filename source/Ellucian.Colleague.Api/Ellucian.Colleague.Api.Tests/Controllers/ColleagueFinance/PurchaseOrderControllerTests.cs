// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Web.Http.Routing;
using Ellucian.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Collections;
using Ellucian.Colleague.Domain.ColleagueFinance;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    #region Purchase Orders V11
    [TestClass]
    public class PurchaseOrdersControllerTests_GET_v11
    {
        public TestContext TestContext { get; set; }
        private Mock<IPurchaseOrderService> _mockPurchaseOrdersService;
        private Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse purchaseOrderResponseCollection;
        private Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest purchaseOrderRequestCollection;
        private Mock<ILogger> _loggerMock;
        private PurchaseOrdersController _purchaseOrdersController;

        private Dtos.PurchaseOrders2 _purchaseOrder;
        private List<Dtos.PurchaseOrders2> _purchaseOrders;
        private string Guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";
        private string personId = "0000100";
        private Paging page;
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");
        private int limit;
        private int offset;
        private Tuple<IEnumerable<PurchaseOrders2>, int> purchaseOrdersTuple;
        private List<PurchaseOrderSummary> purchaseOrderSummaryCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            purchaseOrderSummaryCollection = new List<PurchaseOrderSummary>();
            _loggerMock = new Mock<ILogger>();
            _mockPurchaseOrdersService = new Mock<IPurchaseOrderService>();

            _purchaseOrders = new List<PurchaseOrders2>();

            _purchaseOrder = new PurchaseOrders2()
            {
                Id = Guid,
                Buyer = new GuidObject2("bdc82e04-52ea-49b4-8317-dcefaabf702c"),
                Classification = new GuidObject2("b61b3a19-f164-47ad-afbc-dc5947340cdc"),
                Comments = new List<PurchaseOrdersCommentsDtoProperty>()
                {
                    new PurchaseOrdersCommentsDtoProperty()
                    {
                        Comment = "Hello World",
                        Type = CommentTypes.Printed
                    }
                },
                DeliveredBy = new DateTime(2017, 1, 1),
                Initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty()
                {
                    Name = "John Doe",
                    Detail = new GuidObject2("1b64f194-ed6b-4131-82fe-0ede17271509")
                },
                OrderedOn = new DateTime(2017, 1, 1),
                OrderNumber = "P0000001",
                PaymentSource = new GuidObject2("6d9bf0e5-a261-47c6-a245-297473d9acf0"),
                PaymentTerms = new GuidObject2("b61b3a19-f164-47ad-afbc-dc5947340cdc"),
                ReferenceNumbers = new List<string>() { "123" },
                Status = PurchaseOrdersStatus.Closed,
                TransactionDate = new DateTime(2017, 1, 1),
                Vendor = new PurchaseOrdersVendorDtoProperty2()
                {
                    ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                    {
                        Vendor = new GuidObject2("b61b3a19-f164-47ad-afbc-dc5947340cdc")
                    }
                },
            };
            _purchaseOrders.Add(_purchaseOrder);

            var purchaseOrder2 = new PurchaseOrders2()
            { Id = "NewGuid2" };
            _purchaseOrders.Add(purchaseOrder2);
            var purchaseOrder3 = new PurchaseOrders2()
            { Id = "NewGuid3" };
            _purchaseOrders.Add(purchaseOrder3);


            _mockPurchaseOrdersService.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            _purchaseOrdersController = new PurchaseOrdersController(_mockPurchaseOrdersService.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _purchaseOrdersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            limit = 100;
            offset = 0;
            page = new Paging(limit, offset);
        }

        private void BuildPurchaseOrderPostRequest()
        {
        }
        private void BuildPurchaseOrderSummaryData()
        {
            purchaseOrderSummaryCollection = new List<PurchaseOrderSummary>()
            {
                new PurchaseOrderSummary()
                {
                   Id = "1",
                   Date = DateTime.Today.AddDays(2),
                   InitiatorName = "Test User",
                   RequestorName = "Test User",
                   Status = PurchaseOrderStatus.InProgress,
                   StatusDate = DateTime.Today.AddDays(2),
                   VendorId = "0000190",
                   VendorName = "Basic Office Supply",
                   Amount = 10.00m,
                   Number = "0000001",
                   Requisitions = new List<RequisitionLinkSummary>()
                   {
                       new RequisitionLinkSummary()
                       {
                           Id = "1",
                           Number = "0000001"
                       }
                   }

                },
                new PurchaseOrderSummary()
                {
                     Id = "2",
                   Date = DateTime.Today.AddDays(2),
                   InitiatorName = "Test User",
                   RequestorName = "Test User",
                   Status = PurchaseOrderStatus.InProgress,
                   StatusDate = DateTime.Today.AddDays(2),
                   VendorId = "0000190",
                   VendorName = "Basic Office Supply",
                   Amount = 10.00m,
                   Number = "0000002",
                   Requisitions = new List<RequisitionLinkSummary>()
                   {
                       new RequisitionLinkSummary()
                       {
                           Id = "2",
                           Number = "0000002"
                       }
                   }
                }

            };
            _mockPurchaseOrdersService.Setup(r => r.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrderSummaryCollection);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _mockPurchaseOrdersService = null;
            _purchaseOrdersController = null;
        }

        [TestMethod]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync()
        {
            var expected = _purchaseOrders.FirstOrDefault(po => po.Id == Guid);
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Buyer.Id, "bdc82e04-52ea-49b4-8317-dcefaabf702c");
            Assert.AreEqual(expected.Classification.Id, "b61b3a19-f164-47ad-afbc-dc5947340cdc");
            Assert.AreEqual(1, actual.Comments.Count);
            Assert.AreEqual(expected.Comments[0].Comment, "Hello World");
            Assert.AreEqual(expected.Comments[0].Type, CommentTypes.Printed);
        }

        [TestMethod]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync()
        {

            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, It.IsAny<PurchaseOrders2>(), It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<PurchaseOrders2> ActualsAPI = ((ObjectContent<IEnumerable<PurchaseOrders2>>)httpResponseMessage.Content).Value as List<PurchaseOrders2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = _purchaseOrders.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_cache()
        {

            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, It.IsAny<PurchaseOrders2>(), It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<PurchaseOrders2> ActualsAPI = ((ObjectContent<IEnumerable<PurchaseOrders2>>)httpResponseMessage.Content).Value as List<PurchaseOrders2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = _purchaseOrders.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_NoCache()
        {
            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, It.IsAny<PurchaseOrders2>(), It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<PurchaseOrders2> ActualsAPI = ((ObjectContent<IEnumerable<PurchaseOrders2>>)httpResponseMessage.Content).Value as List<PurchaseOrders2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = _purchaseOrders.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_Paging()
        {
            var DtosAPI = new List<Dtos.PurchaseOrders2>();
            DtosAPI.Add(_purchaseOrder);

            page = new Paging(1, 1);

            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(DtosAPI, 1);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(1, 1, It.IsAny<PurchaseOrders2>(), It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<PurchaseOrders2> ActualsAPI = ((ObjectContent<IEnumerable<PurchaseOrders2>>)httpResponseMessage.Content).Value as List<PurchaseOrders2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = DtosAPI.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_EmptyFilterParams()
        {

            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _purchaseOrdersController.Request.Properties.Add("EmptyFilterProperties", true);
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, It.IsAny<PurchaseOrders2>(), It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<PurchaseOrders2> ActualsAPI = ((ObjectContent<IEnumerable<PurchaseOrders2>>)httpResponseMessage.Content).Value as List<PurchaseOrders2>;
            Assert.AreEqual(0, ActualsAPI.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync_KeyNotFoundExecpt()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync_PermissionsException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync_ArgumentException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync_RepositoryException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync_IntegrationApiException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync_Exception()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync_nullGuid()
        {
            // _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(null);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_KeyNotFoundExecpt()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, new PurchaseOrders2(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_PermissionsException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, new PurchaseOrders2(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_ArgumentException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, new PurchaseOrders2(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_RepositoryException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, new PurchaseOrders2(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_IntegrationApiException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, new PurchaseOrders2(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync_Exception()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, new PurchaseOrders2(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_GetPurchaseOrderSummaryByPersonIdAsync_PersonId_Null()
        {
            await _purchaseOrdersController.GetPurchaseOrderSummaryByPersonIdAsync(null);
        }

        [TestMethod]
        public async Task PoController_GetPurchaseOrderSummaryByPersonIdAsync()
        {
            var expected = purchaseOrderSummaryCollection.AsEnumerable();
            _mockPurchaseOrdersService.Setup(r => r.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var requisitions = await _purchaseOrdersController.GetPurchaseOrderSummaryByPersonIdAsync(personId);
            Assert.AreEqual(requisitions.ToList().Count, expected.Count());
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_GetPurchaseOrderSummaryByPersonIdAsync_ArgumentNullException()
        {
            _mockPurchaseOrdersService.Setup(r => r.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await _purchaseOrdersController.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_GetPurchaseOrderSummaryByPersonIdAsync_Exception()
        {
            _mockPurchaseOrdersService.Setup(r => r.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await _purchaseOrdersController.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_GetPurchaseOrderSummaryByPersonIdAsync_KeyNotFoundException()
        {
            _mockPurchaseOrdersService.Setup(r => r.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await _purchaseOrdersController.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_GetPurchaseOrderSummaryByPersonIdAsync_ApplicationException()
        {
            _mockPurchaseOrdersService.Setup(r => r.GetPurchaseOrderSummaryByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
            await _purchaseOrdersController.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        //GET by id v11.2.0 v11.1.0 v11
        //Successful
        //GetPurchaseOrdersByGuidAsync
        [TestMethod]
        public async Task PurchaseOrdersController_GetPurchaseOrdersByGuidAsync_Permissions()
        {
            var expected = _purchaseOrders.FirstOrDefault(po => po.Id == Guid);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "GetPurchaseOrdersByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _purchaseOrdersController.Request.SetRouteData(data);
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewPurchaseOrders, ColleagueFinancePermissionCodes.UpdatePurchaseOrders });

            var controllerContext = _purchaseOrdersController.ControllerContext;
            var actionDescriptor = _purchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _mockPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);

            Object filterObject;
            _purchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));
            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewPurchaseOrders));


        }

        //GET by id v11.2.0 v11.1.0 v11
        //Exception
        //GetPurchaseOrdersByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersController_GetPurchaseOrdersByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "GetPurchaseOrdersByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _purchaseOrdersController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _purchaseOrdersController.ControllerContext;
            var actionDescriptor = _purchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                _mockPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view purchase-orders."));
                var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync(Guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET v11.2.0 v11.1.0 v11
        //Successful
        //GetPurchaseOrdersAsync
        [TestMethod]
        public async Task PurchaseOrdersController_GetPurchaseOrdersAsync_Permissions()
        {
            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "GetPurchaseOrdersAsync" }
                };
            HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _purchaseOrdersController.Request.SetRouteData(data);
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewPurchaseOrders, ColleagueFinancePermissionCodes.UpdatePurchaseOrders });

            var controllerContext = _purchaseOrdersController.ControllerContext;
            var actionDescriptor = _purchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _mockPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, It.IsAny<PurchaseOrders2>(), It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);

            Object filterObject;
            _purchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewPurchaseOrders));
            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));

        }

        //GET v11.2.0 v11.1.0 v11
        //Exception
        //GetPurchaseOrdersAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersController_GetPurchaseOrdersAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "GetPurchaseOrdersAsync" }
                };
            HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _purchaseOrdersController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _purchaseOrdersController.ControllerContext;
            var actionDescriptor = _purchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync(offset, limit, new PurchaseOrders2(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                _mockPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view purchase-orders."));
                var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync(page, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #region "Post"

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_PostPurchaseOrderAsync_ArgumentNullException()
        {
            _mockPurchaseOrdersService.Setup(r => r.CreateUpdatePurchaseOrderAsync(It.IsAny<PurchaseOrderCreateUpdateRequest>())).ThrowsAsync(new ArgumentNullException());

            await _purchaseOrdersController.PostPurchaseOrderAsync(null);
        }

        [TestMethod]
        public async Task PoController_PostPurchaseOrderAsync()
        {

            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "0000001",
                ConfEmailAddresses = new List<string>() { "anamika.a@ellucian.com" },
                InitiatorInitials = "ANA",
                IsPersonVendor = false,
                PurchaseOrder = new Dtos.ColleagueFinance.PurchaseOrder
                {
                    Id = "",
                    Number = "",
                    Status = 0,
                    StatusDate = new DateTime(2020, 01, 01),
                    Amount = 90.00M,
                    CurrencyCode = "",
                    Date = new DateTime(2020, 01, 01),
                    MaintenanceDate = new DateTime(2020, 01, 01),
                    VendorId = "0000189",
                    VendorName = "Beatrice Clarke & Company",
                    InitiatorName = "Anamika A",
                    RequestorName = "Anamika A",
                    ApType = "AP",
                    ShipToCode = "DT",
                    CommodityCode = "",
                    Comments = "This is Purchase Order creation",
                    InternalComments = "This is Purchase Order creation"
                }
            };

            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse po = new Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse();
            po.PurchaseOrderId = "P00001";
            po.PurchaseOrderNumber = "P00001";
            po.PurchaseOrderDate = new DateTime(2020, 01, 01);
            po.ErrorOccured = false;
            po.ErrorMessages = null;


            _mockPurchaseOrdersService.Setup(r => r.CreateUpdatePurchaseOrderAsync(It.IsAny<PurchaseOrderCreateUpdateRequest>())).ReturnsAsync(po);
            var result = await _purchaseOrdersController.PostPurchaseOrderAsync(purchaseOrderRequestCollection);

            Assert.IsNotNull(result);
        }
        #endregion

        #region VOID
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_VoidPurchaseOrderAsync_ArgumentNullException()
        {
            _mockPurchaseOrdersService.Setup(r => r.VoidPurchaseOrderAsync(It.IsAny<PurchaseOrderVoidRequest>())).ThrowsAsync(new ArgumentNullException());

            await _purchaseOrdersController.VoidPurchaseOrderAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PoController_VoidPurchaseOrderAsync_PermissionException()
        {
            PurchaseOrderVoidRequest request = new PurchaseOrderVoidRequest()
            {
                PersonId = "0000123",
                PurchaseOrderId = "P00001",
                InternalComments = "PO Voided",
                ConfirmationEmailAddresses = "abc@mail.com"
            };
            _mockPurchaseOrdersService.Setup(r => r.VoidPurchaseOrderAsync(It.IsAny<PurchaseOrderVoidRequest>())).ThrowsAsync(new PermissionsException());

            await _purchaseOrdersController.VoidPurchaseOrderAsync(request);
        }

        [TestMethod]
        public async Task PoController_VoidPurchaseOrderAsync()
        {
            PurchaseOrderVoidRequest request = new PurchaseOrderVoidRequest()
            {
                PersonId = "0000123",
                PurchaseOrderId = "123",
                InternalComments = "PO Voided",
                ConfirmationEmailAddresses = "abc@mail.com"
            };

            PurchaseOrderVoidResponse response = new PurchaseOrderVoidResponse()
            {
                PurchaseOrderId = "123",
                PurchaseOrderNumber = "P0000123",
                ErrorOccured = false,
                ErrorMessages = null,
                WarningOccured = false,
                WarningMessages = null
            };

            _mockPurchaseOrdersService.Setup(r => r.VoidPurchaseOrderAsync(It.IsAny<PurchaseOrderVoidRequest>())).ReturnsAsync(response);
            var result = await _purchaseOrdersController.VoidPurchaseOrderAsync(request);

            Assert.IsNotNull(result);
        }
        #endregion
    }

    [TestClass]
    public class PurchaseOrdersControllerTests_PUT_POST_V11
    {
        [TestClass]
        public class PurchaseOrdersControllerTests_POST_v11
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<IPurchaseOrderService> purchaseOrderServiceMock;
            private Mock<ILogger> loggerMock;
            private PurchaseOrdersController purchaseOrdersController;

            private PurchaseOrders2 purchaseOrders;
            private PurchaseOrdersVendorDtoProperty2 vendor;
            private OverrideShippingDestinationDtoProperty shippingDestination;
            private AddressPlace place;
            private Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty initiator;
            private List<PurchaseOrdersLineItemsDtoProperty> lineItems;
            private Dtos.DtoProperties.Amount2DtoProperty amount;
            private List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty> accountDetails;
            private List<PurchaseOrderSummary> purchaseOrderSummaryCollection;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                purchaseOrderServiceMock = new Mock<IPurchaseOrderService>();

                InitializeTestData();

                purchaseOrderServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                purchaseOrdersController = new PurchaseOrdersController(purchaseOrderServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                purchaseOrderServiceMock = null;
                purchaseOrdersController = null;
            }

            private void InitializeTestData()
            {
                amount = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = CurrencyIsoCode.USD, Value = 100 };

                accountDetails = new List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty()
                    {
                        AccountingString = "1",
                        Allocation = new Dtos.DtoProperties.PurchaseOrdersAllocationDtoProperty()
                        {
                            Allocated = new Dtos.DtoProperties.PurchaseOrdersAllocatedDtoProperty() { Amount = amount },
                            TaxAmount = amount,
                            AdditionalAmount = amount,
                            DiscountAmount = amount
                        },
                        SubmittedBy = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b")
                    }
                };

                lineItems = new List<PurchaseOrdersLineItemsDtoProperty>()
                {
                    new PurchaseOrdersLineItemsDtoProperty()
                    {
                        CommodityCode = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                        UnitOfMeasure = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                        UnitPrice = amount,
                        AdditionalAmount = amount,
                        TaxCodes = new List<GuidObject2>() { new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b") },
                        TradeDiscount = new TradeDiscountDtoProperty() { Amount = amount },
                        AccountDetail = accountDetails,
                        Description = "Desc",
                        Quantity = 1,
                        PartNumber = "0123456789"
                    }
                };

                initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty()
                {
                    Detail = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                };

                place = new AddressPlace()
                {
                    Country = new AddressCountry() { Code = IsoCode.USA }
                };

                vendor = new PurchaseOrdersVendorDtoProperty2()
                {
                    ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                    {
                        Vendor = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b")
                    },
                    ManualVendorDetails = new ManualVendorDetailsDtoProperty() { Place = place }
                };

                shippingDestination = new OverrideShippingDestinationDtoProperty()
                {
                    AddressLines = new List<string>() { "Line1" },
                    Place = place,
                    Contact = new PhoneDtoProperty() { Extension = "1234" }
                };

                purchaseOrders = new PurchaseOrders2()
                {
                    Vendor = vendor,
                    OrderedOn = DateTime.Today,
                    TransactionDate = DateTime.Today.AddDays(10),
                    DeliveredBy = DateTime.Today.AddDays(2),
                    OverrideShippingDestination = shippingDestination,
                    PaymentSource = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Comments = new List<PurchaseOrdersCommentsDtoProperty>() { new PurchaseOrdersCommentsDtoProperty() { Comment = "c", Type = CommentTypes.NotPrinted } },
                    Buyer = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Initiator = initiator,
                    PaymentTerms = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Classification = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    SubmittedBy = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    LineItems = lineItems
                };
            }

            #endregion


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync_KeyNotFoundException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new KeyNotFoundException());

                await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync_PermissionsException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new PermissionsException());

                await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync_RepositoryException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new RepositoryException());

                await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync_IntegrationApiException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new IntegrationApiException());

                await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync_ConfigurationException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new ConfigurationException());

                await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync_Exception()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new Exception());

                await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);
            }
            [TestMethod]
            public async Task PoController_PostPurchaseOrdersAsync()
            {
                purchaseOrders.Id = "00000000-0000-0000-0000-000000000000";
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ReturnsAsync(purchaseOrders);

                var result = await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);

                Assert.IsNotNull(result);
                Assert.AreEqual(purchaseOrders.Id, result.Id);
            }

            //POST v11.2.0 v11.1.0 v11
            //Successful
            //PostPurchaseOrdersAsync
            [TestMethod]
            public async Task PurchaseOrdersController_PostPurchaseOrdersAsync_Permissions()
            {
                purchaseOrders.Id = "00000000-0000-0000-0000-000000000000";
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "PostPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                purchaseOrdersController.Request.SetRouteData(data);
                purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.UpdatePurchaseOrders);

                var controllerContext = purchaseOrdersController.ControllerContext;
                var actionDescriptor = purchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                purchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ReturnsAsync(purchaseOrders);
                var result = await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);

                Object filterObject;
                purchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));


            }

            //POST v11.2.0 v11.1.0 v11
            //Exception
            //PostPurchaseOrdersAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PurchaseOrdersController_PostPurchaseOrdersAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "PostPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                purchaseOrdersController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = purchaseOrdersController.ControllerContext;
                var actionDescriptor = purchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new PermissionsException());
                    purchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create purchase-orders."));
                    await purchaseOrdersController.PostPurchaseOrdersAsync(purchaseOrders);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }

        [TestClass]
        public class PurchaseOrdersControllerTests_PUT_v11
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<IPurchaseOrderService> purchaseOrderServiceMock;
            private Mock<ILogger> loggerMock;
            private PurchaseOrdersController purchaseOrdersController;

            private PurchaseOrders2 purchaseOrders;
            private PurchaseOrdersVendorDtoProperty2 vendor;
            private OverrideShippingDestinationDtoProperty shippingDestination;
            private AddressPlace place;
            private Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty initiator;
            private List<PurchaseOrdersLineItemsDtoProperty> lineItems;
            private Dtos.DtoProperties.Amount2DtoProperty amount;
            private List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty> accountDetails;

            private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                purchaseOrderServiceMock = new Mock<IPurchaseOrderService>();

                InitializeTestData();

                purchaseOrderServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                purchaseOrdersController = new PurchaseOrdersController(purchaseOrderServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                purchaseOrdersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                purchaseOrdersController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(purchaseOrders));
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                purchaseOrderServiceMock = null;
                purchaseOrdersController = null;
            }

            private void InitializeTestData()
            {
                amount = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = CurrencyIsoCode.USD, Value = 100 };

                accountDetails = new List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty()
                    {
                        AccountingString = "1",
                        Allocation = new Dtos.DtoProperties.PurchaseOrdersAllocationDtoProperty()
                        {
                            Allocated = new Dtos.DtoProperties.PurchaseOrdersAllocatedDtoProperty() { Amount = amount },
                            TaxAmount = amount,
                            AdditionalAmount = amount,
                            DiscountAmount = amount
                        },
                        SubmittedBy = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b")
                    }
                };

                lineItems = new List<PurchaseOrdersLineItemsDtoProperty>()
                {
                    new PurchaseOrdersLineItemsDtoProperty()
                    {
                        CommodityCode = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                        UnitOfMeasure = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                        UnitPrice = amount,
                        AdditionalAmount = amount,
                        TaxCodes = new List<GuidObject2>() { new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b") },
                        TradeDiscount = new TradeDiscountDtoProperty() { Amount = amount },
                        AccountDetail = accountDetails,
                        Description = "Desc",
                        Quantity = 1,
                        PartNumber = "0123456789"
                    }
                };

                initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty()
                {
                    Detail = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                };

                place = new AddressPlace()
                {
                    Country = new AddressCountry() { Code = IsoCode.USA }
                };

                //removing manualVendorDetails from payload, Since the object is not complete for Colleague needs
                // and we are now issueing an error if it exist in the payload.
                vendor = new PurchaseOrdersVendorDtoProperty2()
                {
                    ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                    {
                        Vendor = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b")
                    },
                    ManualVendorDetails = new ManualVendorDetailsDtoProperty()
                    {
                        Place = new AddressPlace()
                    }
                };

                shippingDestination = new OverrideShippingDestinationDtoProperty()
                {
                    AddressLines = new List<string>() { "Line1" },
                    Place = place,
                    Contact = new PhoneDtoProperty() { Extension = "1234" }
                };

                purchaseOrders = new PurchaseOrders2()
                {
                    Id = "1adc2629-e8a7-410e-b4df-572d02822f8b",
                    Vendor = vendor,
                    OrderedOn = DateTime.Today,
                    TransactionDate = DateTime.Today,
                    DeliveredBy = DateTime.Today.AddDays(2),
                    OverrideShippingDestination = shippingDestination,
                    PaymentSource = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Comments = new List<PurchaseOrdersCommentsDtoProperty>() { new PurchaseOrdersCommentsDtoProperty() { Comment = "c", Type = CommentTypes.NotPrinted } },
                    Buyer = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Initiator = initiator,
                    PaymentTerms = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Classification = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    SubmittedBy = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    LineItems = lineItems
                };
            }

            #endregion

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_IntegrationApiException_When_Guid_Null()
            //{
            //    await purchaseOrdersController.PutPurchaseOrdersAsync(null, It.IsAny<PurchaseOrders2>());
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_IntegrationApiException_When_PurchaseOrder_Null()
            //{
            //    await purchaseOrdersController.PutPurchaseOrdersAsync("1", null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_IntegrationApiException_When_Guid_Is_EmptyGuid()
            //{
            //    await purchaseOrdersController.PutPurchaseOrdersAsync(Guid.Empty.ToString(), purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_IntegrationApiException_When_Guid_And_PurchaseOrderId_NotEqual()
            //{
            //    await purchaseOrdersController.PutPurchaseOrdersAsync("2", purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Vendor_Null()
            //{
            //    purchaseOrders.Vendor = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Vendor_ExistingVendor_Null()
            //{
            //    purchaseOrders.Vendor.ExistingVendor.Vendor.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_OrderOn_Is_Default_Date()
            //{
            //    purchaseOrders.OrderedOn = default(DateTime);

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_TransactionDate_Is_Default_Date()
            //{
            //    purchaseOrders.TransactionDate = default(DateTime);

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_OrderOn_GreaterThan_DeliveredBy()
            //{
            //    purchaseOrders.OrderedOn = DateTime.Today.AddDays(5);

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Destination_Country_Not_CAN_AND_USA()
            //{
            //    purchaseOrders.OverrideShippingDestination.Place.Country.Code = IsoCode.AUS;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_Contact_Ext_Length_Morethan_4()
            //{
            //    purchaseOrders.OverrideShippingDestination.Contact.Extension = "12345";

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_ManualVendor_Country_Not_CAN_AND_USA()
            //{
            //    purchaseOrders.Vendor.ManualVendorDetails.Place.Country = new AddressCountry();
            //    purchaseOrders.Vendor.ManualVendorDetails.Place.Country.Code = IsoCode.AUS;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_PaymentSource_Null()
            //{
            //    purchaseOrders.PaymentSource = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Comment_Null()
            //{
            //    purchaseOrders.Comments.FirstOrDefault().Comment = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Comment_Type_NotSet()
            //{
            //    purchaseOrders.Comments.FirstOrDefault().Type = CommentTypes.NotSet;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Buyer_Null()
            //{
            //    purchaseOrders.Buyer.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Initiator_Detail_Null()
            //{
            //    purchaseOrders.Initiator.Detail.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_PayTerms_Null()
            //{
            //    purchaseOrders.PaymentTerms.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_Clasification_Null()
            //{
            //    purchaseOrders.Classification.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_SubmittedBy_Null()
            //{
            //    purchaseOrders.SubmittedBy.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_Null()
            //{
            //    purchaseOrders.LineItems = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_CommodityCode_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().CommodityCode.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitOfMeasure_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().UnitOfMeasure.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Value_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().UnitPrice.Value = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Currency_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().UnitPrice.Currency = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Value_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Value = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Currency_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Currency = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TaxCodeId_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().TaxCodes.FirstOrDefault().Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_AmountAndPer_NotNull()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Percent = 10;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_Amount_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Value = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_Currency_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Currency = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AccountingString_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().AccountingString = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Value = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Currency_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Currency = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Value = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Currency_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Currency = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Value = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Currency_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Currency = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Value = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Currency_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Currency = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_SubmittedBy_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().SubmittedBy.Id = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_Description_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().Description = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_Quantity_Is_Zero()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().Quantity = 0;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Null()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().UnitPrice = null;

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PutPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_PartNumber_Length_Morethan_11()
            //{
            //    purchaseOrders.LineItems.FirstOrDefault().PartNumber = "012345678911";

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PostPurchaseOrdersAsync_KeyNotFoundException()
            //{
            //    purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new KeyNotFoundException());

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PostPurchaseOrdersAsync_PermissionsException()
            //{
            //    purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new PermissionsException());

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PostPurchaseOrdersAsync_RepositoryException()
            //{
            //    purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new RepositoryException());

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PostPurchaseOrdersAsync_IntegrationApiException()
            //{
            //    purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new IntegrationApiException());

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PostPurchaseOrdersAsync_ConfigurationException()
            //{
            //    purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new ConfigurationException());

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task PoController_PostPurchaseOrdersAsync_Exception()
            //{
            //    purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new Exception());

            //    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
            //}

            [TestMethod]
            public async Task PoController_PutPurchaseOrdersAsync()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ReturnsAsync(purchaseOrders);
                purchaseOrderServiceMock.Setup(s => s.GetPurchaseOrdersByGuidAsync(guid)).ReturnsAsync(purchaseOrders);

                purchaseOrders.Id = null;

                var result = await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);

                Assert.IsNotNull(result);
            }

            //PUT v11.2.0 v11.1.0 v11
            //Successful
            //PutPurchaseOrdersAsync
            [TestMethod]
            public async Task PurchaseOrdersController_PutPurchaseOrdersAsync_Permissions()
            {
                purchaseOrders.Id = null;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "PutPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                purchaseOrdersController.Request.SetRouteData(data);
                purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.UpdatePurchaseOrders);

                var controllerContext = purchaseOrdersController.ControllerContext;
                var actionDescriptor = purchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                purchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ReturnsAsync(purchaseOrders);
                var result = await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);

                Object filterObject;
                purchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdatePurchaseOrders));


            }

            //PUT v11.2.0 v11.1.0 v11
            //Exception
            //PutPurchaseOrdersAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PurchaseOrdersController_PutPurchaseOrdersAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PurchaseOrders" },
                    { "action", "PutPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                purchaseOrdersController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = purchaseOrdersController.ControllerContext;
                var actionDescriptor = purchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new PermissionsException());
                    purchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update purchase-orders."));
                    await purchaseOrdersController.PutPurchaseOrdersAsync(guid, purchaseOrders);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }
    }

    #region QueryPurchaseOrderSummariesAsync Tests

    [TestClass]
    public class QueryPurchaseOrderSummariesAsyncTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPurchaseOrderService> purchaseOrderServiceMock;
        private Mock<ILogger> loggerMock;
        private PurchaseOrdersController purchaseOrdersController;
        private List<PurchaseOrderSummary> purchaseOrderSummaryCollection;

        private Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria filterCriteria;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            purchaseOrderServiceMock = new Mock<IPurchaseOrderService>();
            loggerMock = new Mock<ILogger>();

            filterCriteria = new ProcurementDocumentFilterCriteria();
            filterCriteria.PersonId = "0000100";
            filterCriteria.VendorIds = new List<string>() { "0000190" };

            purchaseOrderSummaryCollection = new List<PurchaseOrderSummary>();

            BuildPurchaseOrderSummaryData();

            purchaseOrdersController = new PurchaseOrdersController(purchaseOrderServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }



        private void BuildPurchaseOrderSummaryData()
        {
            purchaseOrderSummaryCollection = new List<PurchaseOrderSummary>()
            {
                new PurchaseOrderSummary()
                {
                   Id = "1",
                   Date = DateTime.Today.AddDays(2),
                   InitiatorName = "Test User",
                   RequestorName = "Test User",
                   Status = PurchaseOrderStatus.InProgress,
                   StatusDate = DateTime.Today.AddDays(2),
                   VendorId = "0000190",
                   VendorName = "Basic Office Supply",
                   Amount = 10.00m,
                   Number = "0000001",
                   Requisitions = new List<RequisitionLinkSummary>()
                   {
                       new RequisitionLinkSummary()
                       {
                           Id = "1",
                           Number = "0000001"
                       }
                   }

                },
                new PurchaseOrderSummary()
                {
                     Id = "2",
                   Date = DateTime.Today.AddDays(2),
                   InitiatorName = "Test User",
                   RequestorName = "Test User",
                   Status = PurchaseOrderStatus.InProgress,
                   StatusDate = DateTime.Today.AddDays(2),
                   VendorId = "0000190",
                   VendorName = "Basic Office Supply",
                   Amount = 10.00m,
                   Number = "0000002",
                   Requisitions = new List<RequisitionLinkSummary>()
                   {
                       new RequisitionLinkSummary()
                       {
                           Id = "2",
                           Number = "0000002"
                       }
                   }
                }

            };
            purchaseOrderServiceMock.Setup(r => r.QueryPurchaseOrderSummariesAsync(It.IsAny<ProcurementDocumentFilterCriteria>())).ReturnsAsync(purchaseOrderSummaryCollection);

        }

        [TestCleanup]
        public void Cleanup()
        {
            purchaseOrdersController = null;
            loggerMock = null;
            purchaseOrderServiceMock = null;
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_QueryPurchaseOrderSummariesAsync_Dto_Null()
        {
            await purchaseOrdersController.QueryPurchaseOrderSummariesAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_QueryRequisitionSummariessAsync_PermissionsException()
        {
            purchaseOrderServiceMock.Setup(r => r.QueryPurchaseOrderSummariesAsync(It.IsAny<ProcurementDocumentFilterCriteria>())).ThrowsAsync(new PermissionsException());
            await purchaseOrdersController.QueryPurchaseOrderSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_QueryPurchaseOrderSummariesAsync_Exception()
        {
            purchaseOrderServiceMock.Setup(r => r.QueryPurchaseOrderSummariesAsync(It.IsAny<ProcurementDocumentFilterCriteria>())).ThrowsAsync(new Exception());
            await purchaseOrdersController.QueryPurchaseOrderSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_QueryPurchaseOrderSummariesAsync_ArgumentNullException()
        {
            purchaseOrderServiceMock.Setup(r => r.QueryPurchaseOrderSummariesAsync(It.IsAny<ProcurementDocumentFilterCriteria>())).ThrowsAsync(new ArgumentNullException());
            await purchaseOrdersController.QueryPurchaseOrderSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_QueryPurchaseOrderSummariesAsync_KeyNotFoundException()
        {
            purchaseOrderServiceMock.Setup(r => r.QueryPurchaseOrderSummariesAsync(It.IsAny<ProcurementDocumentFilterCriteria>())).ThrowsAsync(new KeyNotFoundException());
            await purchaseOrdersController.QueryPurchaseOrderSummariesAsync(filterCriteria);
        }

        [TestMethod]
        public async Task RequisitionsController_QueryPurchaseOrderSummariesAsync()
        {
            purchaseOrderServiceMock.Setup(r => r.QueryPurchaseOrderSummariesAsync(It.IsAny<ProcurementDocumentFilterCriteria>())).ReturnsAsync(purchaseOrderSummaryCollection);
            var result = await purchaseOrdersController.QueryPurchaseOrderSummariesAsync(filterCriteria);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 0);
        }

    }

    #endregion
    #endregion
}