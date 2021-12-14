// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    #region Purchase Orders V15_1_0
    [TestClass]
    public class BlanketPurchaseOrdersControllerTests_v16_0_0
    {
        public TestContext TestContext { get; set; }
        private Mock<IBlanketPurchaseOrderService> _mockBlanketPurchaseOrdersService;
        private Mock<ILogger> _loggerMock;
        private BlanketPurchaseOrdersController _blanketPurchaseOrdersController;

        private Dtos.BlanketPurchaseOrders _blanketPurchaseOrder;
        private List<Dtos.BlanketPurchaseOrders> _blanketPurchaseOrders;
        private string Guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";
        private Paging page;
        private int limit;
        private int offset;
        private Tuple<IEnumerable<BlanketPurchaseOrders>, int> blanketPurchaseOrdersTuple;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _loggerMock = new Mock<ILogger>();
            _mockBlanketPurchaseOrdersService = new Mock<IBlanketPurchaseOrderService>();

            _blanketPurchaseOrders = new List<BlanketPurchaseOrders>();

            _blanketPurchaseOrder = new BlanketPurchaseOrders()
            {
                Id = Guid,
                Buyer = new GuidObject2("bdc82e04-52ea-49b4-8317-dcefaabf702c"),
                Classification = new GuidObject2("b61b3a19-f164-47ad-afbc-dc5947340cdc"),
                Comments = new List<BlanketPurchaseOrdersComments>()
                {
                    new BlanketPurchaseOrdersComments()
                    {
                        Comment = "Hello World",
                        Type = CommentTypes.Printed
                    }
                },
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
                Status = BlanketPurchaseOrdersStatus.Closed,
                TransactionDate = new DateTime(2017, 1, 1),
                Vendor = new BlanketPurchaseOrdersVendor()
                {
                    ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty()
                    {
                        Vendor = new GuidObject2("b61b3a19-f164-47ad-afbc-dc5947340cdc")
                    }
                },
            };
            _blanketPurchaseOrders.Add(_blanketPurchaseOrder);

            var blanketPurchaseOrder2 = new BlanketPurchaseOrders()
            { Id = "NewGuid2" };
            _blanketPurchaseOrders.Add(blanketPurchaseOrder2);
            var blanketPurchaseOrder3 = new BlanketPurchaseOrders()
            { Id = "NewGuid3" };
            _blanketPurchaseOrders.Add(blanketPurchaseOrder3);


            _mockBlanketPurchaseOrdersService.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            _blanketPurchaseOrdersController = new BlanketPurchaseOrdersController(_mockBlanketPurchaseOrdersService.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _blanketPurchaseOrdersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            limit = 100;
            offset = 0;
            page = new Paging(limit, offset);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _mockBlanketPurchaseOrdersService = null;
            _blanketPurchaseOrdersController = null;
        }

        [TestMethod]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync()
        {
            var expected = _blanketPurchaseOrders.FirstOrDefault(po => po.Id == Guid);
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Buyer.Id, "bdc82e04-52ea-49b4-8317-dcefaabf702c");
            Assert.AreEqual(expected.Classification.Id, "b61b3a19-f164-47ad-afbc-dc5947340cdc");
            Assert.AreEqual(1, actual.Comments.Count);
            Assert.AreEqual(expected.Comments[0].Comment, "Hello World");
            Assert.AreEqual(expected.Comments[0].Type, CommentTypes.Printed);
        }

        [TestMethod]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync()
        {

            blanketPurchaseOrdersTuple = new Tuple<IEnumerable<BlanketPurchaseOrders>, int>(_blanketPurchaseOrders, 3);

            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrdersTuple);
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<BlanketPurchaseOrders> ActualsAPI = ((ObjectContent<IEnumerable<BlanketPurchaseOrders>>)httpResponseMessage.Content).Value as List<BlanketPurchaseOrders>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = _blanketPurchaseOrders.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_cache()
        {

            blanketPurchaseOrdersTuple = new Tuple<IEnumerable<BlanketPurchaseOrders>, int>(_blanketPurchaseOrders, 3);

            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrdersTuple);
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<BlanketPurchaseOrders> ActualsAPI = ((ObjectContent<IEnumerable<BlanketPurchaseOrders>>)httpResponseMessage.Content).Value as List<BlanketPurchaseOrders>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = _blanketPurchaseOrders.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_NoCache()
        {
            blanketPurchaseOrdersTuple = new Tuple<IEnumerable<BlanketPurchaseOrders>, int>(_blanketPurchaseOrders, 3);

            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrdersTuple);
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<BlanketPurchaseOrders> ActualsAPI = ((ObjectContent<IEnumerable<BlanketPurchaseOrders>>)httpResponseMessage.Content).Value as List<BlanketPurchaseOrders>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = _blanketPurchaseOrders.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_Paging()
        {
            var DtosAPI = new List<Dtos.BlanketPurchaseOrders>();
            DtosAPI.Add(_blanketPurchaseOrder);

            page = new Paging(1, 1);

            blanketPurchaseOrdersTuple = new Tuple<IEnumerable<BlanketPurchaseOrders>, int>(DtosAPI, 1);

            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(1, 1, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrdersTuple);
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<BlanketPurchaseOrders> ActualsAPI = ((ObjectContent<IEnumerable<BlanketPurchaseOrders>>)httpResponseMessage.Content).Value as List<BlanketPurchaseOrders>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = DtosAPI.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync_KeyNotFoundExecpt()
        {
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync_PermissionsException()
        {
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync_ArgumentException()
        {
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync_RepositoryException()
        {
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync_IntegrationApiException()
        {
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync_Exception()
        {
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersByGuidAsync_nullGuid()
        {
            // _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(null);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_KeyNotFoundExecpt()
        {
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_PermissionsException()
        {
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_ArgumentException()
        {
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_RepositoryException()
        {
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_IntegrationApiException()
        {
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersControllerTests_GetBlanketPurchaseOrdersAsync_Exception()
        {
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _blanketPurchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
        }

        //GET by id v16.1.0 v16.0.0 
        //Successful
        //GetBlanketPurchaseOrdersByGuidAsync
        [TestMethod]
        public async Task BlanketPurchaseOrdersController_GetBlanketPurchaseOrdersByGuidAsync_Permissions()
        {
            var expected = _blanketPurchaseOrders.FirstOrDefault(po => po.Id == Guid);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "GetBlanketPurchaseOrdersByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _blanketPurchaseOrdersController.Request.SetRouteData(data);
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders, ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders });

            var controllerContext = _blanketPurchaseOrdersController.ControllerContext;
            var actionDescriptor = _blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _mockBlanketPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);

            Object filterObject;
            _blanketPurchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders));
            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders));


        }

        //GET by id v16.1.0 v16.0.0 
        //Exception
        //GetBlanketPurchaseOrdersByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersController_GetBlanketPurchaseOrdersByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "GetBlanketPurchaseOrdersByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _blanketPurchaseOrdersController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _blanketPurchaseOrdersController.ControllerContext;
            var actionDescriptor = _blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                _mockBlanketPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view blanket-purchase-orders."));
                var actual = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersByGuidAsync(Guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET v16.1.0 v16.0.0 
        //Successful
        //GetBlanketPurchaseOrdersAsync
        [TestMethod]
        public async Task BlanketPurchaseOrdersController_GetBlanketPurchaseOrdersAsync_Permissions()
        {
            blanketPurchaseOrdersTuple = new Tuple<IEnumerable<BlanketPurchaseOrders>, int>(_blanketPurchaseOrders, 3);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "GetBlanketPurchaseOrdersAsync" }
                };
            HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _blanketPurchaseOrdersController.Request.SetRouteData(data);
            _blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders, ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders });

            var controllerContext = _blanketPurchaseOrdersController.ControllerContext;
            var actionDescriptor = _blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _mockBlanketPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrdersTuple);
            var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);

            Object filterObject;
            _blanketPurchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders));
            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders));


        }

        //GET v16.1.0 v16.0.0 
        //Exception
        //GetBlanketPurchaseOrdersAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BlanketPurchaseOrdersController_GetBlanketPurchaseOrdersAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "GetBlanketPurchaseOrdersAsync" }
                };
            HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _blanketPurchaseOrdersController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _blanketPurchaseOrdersController.ControllerContext;
            var actionDescriptor = _blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _mockBlanketPurchaseOrdersService.Setup(x => x.GetBlanketPurchaseOrdersAsync(offset, limit, It.IsAny<Dtos.BlanketPurchaseOrders>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException()); _mockBlanketPurchaseOrdersService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
     .Throws(new PermissionsException("User 'npuser' does not have permission to view blanket-purchase-orders."));
                var actuals = await _blanketPurchaseOrdersController.GetBlanketPurchaseOrdersAsync(page, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


    }

    [TestClass]
    public class BlanketPurchaseOrdersControllerTests_V16_0_0
    {
        [TestClass]
        public class BlanketPurchaseOrdersControllerTests_POST_v16_0_0
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<IBlanketPurchaseOrderService> blanketPurchaseOrderServiceMock;
            private Mock<ILogger> loggerMock;
            private BlanketPurchaseOrdersController blanketPurchaseOrdersController;

            private BlanketPurchaseOrders blanketPurchaseOrders;
            private BlanketPurchaseOrdersVendor vendor;
            private OverrideShippingDestinationDtoProperty shippingDestination;
            private AddressPlace place;
            private Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty initiator;
            private Dtos.DtoProperties.Amount2DtoProperty amount;
            private List<Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty> accountDetails;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                blanketPurchaseOrderServiceMock = new Mock<IBlanketPurchaseOrderService>();

                InitializeTestData();

                blanketPurchaseOrderServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                blanketPurchaseOrdersController = new BlanketPurchaseOrdersController(blanketPurchaseOrderServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                blanketPurchaseOrderServiceMock = null;
                blanketPurchaseOrdersController = null;
            }

            private void InitializeTestData()
            {
                amount = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = CurrencyIsoCode.USD, Value = 100 };

                accountDetails = new List<Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty()
                    {
                        AccountingString = "1",
                        Allocation = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocationDtoProperty()
                        {
                            Allocated = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocatedDtoProperty() { Amount = amount },
                            TaxAmount = amount,
                            AdditionalAmount = amount,
                            DiscountAmount = amount
                        }
                    }
                };

                //lineItems = new List<BlanketPurchaseOrdersLineItemsDtoProperty>()
                //{
                //    new BlanketPurchaseOrdersLineItemsDtoProperty()
                //    {
                //        CommodityCode = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                //        UnitOfMeasure = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                //        UnitPrice = amount,
                //        AdditionalAmount = amount,
                //        TaxCodes = new List<GuidObject2>() { new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b") },
                //        TradeDiscount = new TradeDiscountDtoProperty() { Amount = amount },
                //        AccountDetail = accountDetails,
                //        Description = "Desc",
                //        Quantity = 1,
                //        PartNumber = "0123456789"
                //    }
                //};

                initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty()
                {
                    Detail = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                };

                place = new AddressPlace()
                {
                    Country = new AddressCountry() { Code = IsoCode.USA }
                };

                vendor = new BlanketPurchaseOrdersVendor()
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

                blanketPurchaseOrders = new BlanketPurchaseOrders()
                {
                    Vendor = vendor,
                    OrderedOn = DateTime.Today,
                    TransactionDate = DateTime.Today.AddDays(10),
                    OverrideShippingDestination = shippingDestination,
                    PaymentSource = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Comments = new List<BlanketPurchaseOrdersComments>() { new BlanketPurchaseOrdersComments() { Comment = "c", Type = CommentTypes.NotPrinted } },
                    Buyer = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Initiator = initiator,
                    PaymentTerms = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Classification = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    SubmittedBy = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_IntegrationApiException_When_PurchaseOrder_Null()
            {
                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Vendor_Null()
            {
                blanketPurchaseOrders.Vendor = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Vendor_ExistingVendor_Null()
            {
                blanketPurchaseOrders.Vendor.ExistingVendor.Vendor.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_OrderOn_Is_Default_Date()
            {
                blanketPurchaseOrders.OrderedOn = default(DateTime);

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_TransactionDate_Is_Default_Date()
            {
                blanketPurchaseOrders.TransactionDate = default(DateTime);

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_OrderOn_GreaterThan_DeliveredBy()
            {
                blanketPurchaseOrders.OrderedOn = DateTime.Today.AddDays(5);

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Destination_Country_Not_CAN_AND_USA()
            {
                blanketPurchaseOrders.OverrideShippingDestination.Place.Country.Code = IsoCode.AUS;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_Contact_Ext_Length_Morethan_4()
            {
                blanketPurchaseOrders.OverrideShippingDestination.Contact.Extension = "12345";

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_ManualVendor_Country_Not_CAN_AND_USA()
            {
                blanketPurchaseOrders.Vendor.ManualVendorDetails.Place.Country.Code = IsoCode.AUS;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_PaymentSource_Null()
            {
                blanketPurchaseOrders.PaymentSource = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Comment_Null()
            {
                blanketPurchaseOrders.Comments.FirstOrDefault().Comment = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Comment_Type_NotSet()
            {
                blanketPurchaseOrders.Comments.FirstOrDefault().Type = CommentTypes.NotSet;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Buyer_Null()
            {
                blanketPurchaseOrders.Buyer.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Initiator_Detail_Null()
            {
                blanketPurchaseOrders.Initiator.Detail.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_PayTerms_Null()
            {
                blanketPurchaseOrders.PaymentTerms.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_Clasification_Null()
            {
                blanketPurchaseOrders.Classification.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_SubmittedBy_Null()
            {
                blanketPurchaseOrders.SubmittedBy.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_Null()
            {
                //blanketPurchaseOrders.LineItems = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_CommodityCode_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().CommodityCode.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitOfMeasure_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().UnitOfMeasure.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Value_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().UnitPrice.Value = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().UnitPrice.Currency = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Value_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Value = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Currency = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TaxCodeId_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TaxCodes.FirstOrDefault().Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_AmountAndPer_NotNull()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Percent = 10;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_Amount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Value = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Currency = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AccountingString_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().AccountingString = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Value = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Currency = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Value = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Currency = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Value = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Currency = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Value = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Currency = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_SubmittedBy_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().SubmittedBy.Id = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_Description_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().Description = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_Quantity_Is_Zero()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().Quantity = 0;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().UnitPrice = null;

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentNullException_When_PO_LineItems_PartNumber_Length_Morethan_11()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().PartNumber = "012345678911";

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_KeyNotFoundException()
            {
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new KeyNotFoundException());

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_PermissionsException()
            {
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new PermissionsException());

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_RepositoryException()
            {
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new RepositoryException());

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_IntegrationApiException()
            {
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new IntegrationApiException());

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ConfigurationException()
            {
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new ConfigurationException());

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_Exception()
            {
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new Exception());

                await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
            }

            //[TestMethod]
            //public async Task BPOController_PostBlanketPurchaseOrdersAsync()
            //{
            //    blanketPurchaseOrders.Id = "00000000-0000-0000-0000-000000000000";
            //    blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ReturnsAsync(blanketPurchaseOrders);

            //    var result = await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);

            //    Assert.IsNotNull(result);
            //    Assert.AreEqual(blanketPurchaseOrders.Id, result.Id);
            //}

            //POST v16.1.0 v16.0.0 
            //Successful
            //PostBlanketPurchaseOrdersAsync
            [TestMethod]
            public async Task BlanketPurchaseOrdersController_PostBlanketPurchaseOrdersAsync_Permissions()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "PostBlanketPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                blanketPurchaseOrdersController.Request.SetRouteData(data);
                blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders);

                var controllerContext = blanketPurchaseOrdersController.ControllerContext;
                var actionDescriptor = blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                blanketPurchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ReturnsAsync(blanketPurchaseOrders);
                var result = await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);

                Object filterObject;
                blanketPurchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders));


            }

            //POST v16.1.0 v16.0.0 
            //Exception
            //PostBlanketPurchaseOrdersAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BlanketPurchaseOrdersController_PostBlanketPurchaseOrdersAsync_Invalid_Permissions()
            {
                blanketPurchaseOrders.Id = Guid.Empty.ToString();
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "PostBlanketPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                blanketPurchaseOrdersController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = blanketPurchaseOrdersController.ControllerContext;
                var actionDescriptor = blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    blanketPurchaseOrderServiceMock.Setup(s => s.PostBlanketPurchaseOrdersAsync(It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new PermissionsException());
                    blanketPurchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view blanket-purchase-orders."));
                    await blanketPurchaseOrdersController.PostBlanketPurchaseOrdersAsync(blanketPurchaseOrders);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }

        [TestClass]
        public class BlanketPurchaseOrdersControllerTests_PUT_v11
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<IBlanketPurchaseOrderService> blanketPurchaseOrderServiceMock;
            private Mock<ILogger> loggerMock;
            private BlanketPurchaseOrdersController blanketPurchaseOrdersController;

            private BlanketPurchaseOrders blanketPurchaseOrders;
            private BlanketPurchaseOrdersVendor vendor;
            private OverrideShippingDestinationDtoProperty shippingDestination;
            private AddressPlace place;
            private Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty initiator;
            private Dtos.DtoProperties.Amount2DtoProperty amount;
            private List<Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty> accountDetails;

            private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                blanketPurchaseOrderServiceMock = new Mock<IBlanketPurchaseOrderService>();

                InitializeTestData();

                blanketPurchaseOrderServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                blanketPurchaseOrdersController = new BlanketPurchaseOrdersController(blanketPurchaseOrderServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                blanketPurchaseOrdersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                blanketPurchaseOrdersController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(blanketPurchaseOrders));
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                blanketPurchaseOrderServiceMock = null;
                blanketPurchaseOrdersController = null;
            }

            private void InitializeTestData()
            {
                amount = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = CurrencyIsoCode.USD, Value = 100 };

                accountDetails = new List<Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.BlanketPurchaseOrdersAccountDetailDtoProperty()
                    {
                        AccountingString = "1",
                        Allocation = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocationDtoProperty()
                        {
                            Allocated = new Dtos.DtoProperties.BlanketPurchaseOrdersAllocatedDtoProperty() { Amount = amount },
                            TaxAmount = amount,
                            AdditionalAmount = amount,
                            DiscountAmount = amount
                        }
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
                vendor = new BlanketPurchaseOrdersVendor()
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

                blanketPurchaseOrders = new BlanketPurchaseOrders()
                {
                    Id = "1adc2629-e8a7-410e-b4df-572d02822f8b",
                    Vendor = vendor,
                    OrderedOn = DateTime.Today,
                    TransactionDate = DateTime.Today,
                    OverrideShippingDestination = shippingDestination,
                    PaymentSource = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Comments = new List<BlanketPurchaseOrdersComments>() { new BlanketPurchaseOrdersComments() { Comment = "c", Type = CommentTypes.NotPrinted } },
                    Buyer = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Initiator = initiator,
                    PaymentTerms = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    Classification = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),
                    SubmittedBy = new GuidObject2("1adc2629-e8a7-410e-b4df-572d02822f8b"),

                };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_IntegrationApiException_When_Guid_Null()
            {
                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(null, It.IsAny<BlanketPurchaseOrders>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_IntegrationApiException_When_PurchaseOrder_Null()
            {
                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync("1", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_IntegrationApiException_When_Guid_Is_EmptyGuid()
            {
                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(Guid.Empty.ToString(), blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_IntegrationApiException_When_Guid_And_PurchaseOrderId_NotEqual()
            {
                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync("2", blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Vendor_Null()
            {
                blanketPurchaseOrders.Vendor = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Vendor_ExistingVendor_Null()
            {
                blanketPurchaseOrders.Vendor.ExistingVendor.Vendor.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_OrderOn_Is_Default_Date()
            {
                blanketPurchaseOrders.OrderedOn = default(DateTime);

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_TransactionDate_Is_Default_Date()
            {
                blanketPurchaseOrders.TransactionDate = default(DateTime);

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_OrderOn_GreaterThan_DeliveredBy()
            {
                blanketPurchaseOrders.OrderedOn = DateTime.Today.AddDays(5);

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Destination_Country_Not_CAN_AND_USA()
            {
                blanketPurchaseOrders.OverrideShippingDestination.Place.Country.Code = IsoCode.AUS;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_Contact_Ext_Length_Morethan_4()
            {
                blanketPurchaseOrders.OverrideShippingDestination.Contact.Extension = "12345";

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_ManualVendor_Country_Not_CAN_AND_USA()
            {
                blanketPurchaseOrders.Vendor.ManualVendorDetails.Place.Country = new AddressCountry();
                blanketPurchaseOrders.Vendor.ManualVendorDetails.Place.Country.Code = IsoCode.AUS;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_PaymentSource_Null()
            {
                blanketPurchaseOrders.PaymentSource = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Comment_Null()
            {
                blanketPurchaseOrders.Comments.FirstOrDefault().Comment = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Comment_Type_NotSet()
            {
                blanketPurchaseOrders.Comments.FirstOrDefault().Type = CommentTypes.NotSet;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Buyer_Null()
            {
                blanketPurchaseOrders.Buyer.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Initiator_Detail_Null()
            {
                blanketPurchaseOrders.Initiator.Detail.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_PayTerms_Null()
            {
                blanketPurchaseOrders.PaymentTerms.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_Clasification_Null()
            {
                blanketPurchaseOrders.Classification.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_SubmittedBy_Null()
            {
                blanketPurchaseOrders.SubmittedBy.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_Null()
            {
                //blanketPurchaseOrders.LineItems = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_CommodityCode_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().CommodityCode.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_UnitOfMeasure_Null()
            {
                // blanketPurchaseOrders.LineItems.FirstOrDefault().UnitOfMeasure.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Value_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().UnitPrice.Value = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().UnitPrice.Currency = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Value_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Value = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Currency = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_TaxCodeId_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TaxCodes.FirstOrDefault().Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_AmountAndPer_NotNull()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Percent = 10;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_Amount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Value = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_TradeDiscount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Currency = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AccountingString_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().AccountingString = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Value = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Currency = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Value = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Currency = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Value = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Currency = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Value = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Currency_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Currency = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_SubmittedBy_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().SubmittedBy.Id = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_Description_Null()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().Description = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_Quantity_Is_Zero()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().Quantity = 0;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_UnitPrice_Null()
            {
                // blanketPurchaseOrders.LineItems.FirstOrDefault().UnitPrice = null;

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PutBPOAsync_ArgumentNullException_When_PO_LineItems_PartNumber_Length_Morethan_11()
            {
                //blanketPurchaseOrders.LineItems.FirstOrDefault().PartNumber = "012345678911";

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_KeyNotFoundException()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new KeyNotFoundException());

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ArgumentException()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new ArgumentException());

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_PermissionsException()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new PermissionsException());

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_RepositoryException()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new RepositoryException());

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_IntegrationApiException()
            {
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new IntegrationApiException());

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_ConfigurationException()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new ConfigurationException());

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BPOController_PostBlanketPurchaseOrdersAsync_Exception()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new Exception());

                await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
            }

            //[TestMethod]
            //public async Task BPOController_PutBlanketPurchaseOrdersAsync()
            //{
            //    blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ReturnsAsync(blanketPurchaseOrders);
            //    blanketPurchaseOrderServiceMock.Setup(s => s.GetBlanketPurchaseOrdersByGuidAsync(guid)).ReturnsAsync(blanketPurchaseOrders);

            //    blanketPurchaseOrders.Id = null;

            //    var result = await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);

            //    Assert.IsNotNull(result);
            //}

            //PUT v16.1.0 v16.0.0 
            //Successful
            //GetBlanketPurchaseOrdersAsync
            [TestMethod]
            public async Task BlanketPurchaseOrdersController_GetBlanketPurchaseOrdersAsync_Permissions()
            {
                blanketPurchaseOrders.OrderDetails = new List<BlanketPurchaseOrdersOrderdetails>()
                {
                    new BlanketPurchaseOrdersOrderdetails()
                    {
                        CommodityCode = new GuidObject2(Guid.NewGuid().ToString()),
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 1m
                        },
                        Description = "Description",
                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = CurrencyIsoCode.USD,
                            Value = 5m
                        }
                    }
                };
                blanketPurchaseOrders.Id = null;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "GetBlanketPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                blanketPurchaseOrdersController.Request.SetRouteData(data);
                blanketPurchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders);

                var controllerContext = blanketPurchaseOrdersController.ControllerContext;
                var actionDescriptor = blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                blanketPurchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ReturnsAsync(blanketPurchaseOrders);
                var result = await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);

                Object filterObject;
                blanketPurchaseOrdersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders));


            }

            //PUT v16.1.0 v16.0.0 
            //Exception
            //GetBlanketPurchaseOrdersAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BlanketPurchaseOrdersController_GetBlanketPurchaseOrdersAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "BlanketPurchaseOrders" },
                    { "action", "GetBlanketPurchaseOrdersAsync" }
                };
                HttpRoute route = new HttpRoute("blanket-purchase-orders", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                blanketPurchaseOrdersController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = blanketPurchaseOrdersController.ControllerContext;
                var actionDescriptor = blanketPurchaseOrdersController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    blanketPurchaseOrderServiceMock.Setup(s => s.PutBlanketPurchaseOrdersAsync(It.IsAny<string>(), It.IsAny<BlanketPurchaseOrders>())).ThrowsAsync(new PermissionsException());
                    blanketPurchaseOrderServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view blanket-purchase-orders."));
                    await blanketPurchaseOrdersController.PutBlanketPurchaseOrdersAsync(guid, blanketPurchaseOrders);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

        }
    }

    #endregion
}