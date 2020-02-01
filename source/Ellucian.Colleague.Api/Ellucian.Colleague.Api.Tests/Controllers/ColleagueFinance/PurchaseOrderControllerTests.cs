// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    #region Purchase Orders V11
    [TestClass]
    public class PurchaseOrdersControllerTests_v11
    {
        public TestContext TestContext { get; set; }
        private Mock<IPurchaseOrderService> _mockPurchaseOrdersService;
        private Mock<ILogger> _loggerMock;
        private PurchaseOrdersController _purchaseOrdersController;

        private Dtos.PurchaseOrders2 _purchaseOrder;
        private List<Dtos.PurchaseOrders2> _purchaseOrders;
        private string Guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";
        private string personId = "0000100";
        private Paging page;
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

        private void BuildRequisitionSummaryData()
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
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2()
        {
            var expected = _purchaseOrders.FirstOrDefault(po => po.Id == Guid);
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync2(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(Guid);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Buyer.Id, "bdc82e04-52ea-49b4-8317-dcefaabf702c");
            Assert.AreEqual(expected.Classification.Id, "b61b3a19-f164-47ad-afbc-dc5947340cdc");
            Assert.AreEqual(1, actual.Comments.Count);
            Assert.AreEqual(expected.Comments[0].Comment, "Hello World");
            Assert.AreEqual(expected.Comments[0].Type, CommentTypes.Printed);
        }

        [TestMethod]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2()
        {

            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
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
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_cache()
        {

            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
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
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_NoCache()
        {
            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(_purchaseOrders, 3);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
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
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_Paging()
        {
            var DtosAPI = new List<Dtos.PurchaseOrders2>();
            DtosAPI.Add(_purchaseOrder);

            page = new Paging(1, 1);

            purchaseOrdersTuple = new Tuple<IEnumerable<PurchaseOrders2>, int>(DtosAPI, 1);

            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(1, 1, It.IsAny<bool>())).ReturnsAsync(purchaseOrdersTuple);
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
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
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2_KeyNotFoundExecpt()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync2(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2_PermissionsException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync2(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2_ArgumentException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync2(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2_RepositoryException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync2(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2_IntegrationApiException()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync2(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2_Exception()
        {
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync2(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(Guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersByGuidAsync2_nullGuid()
        {
            // _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await _purchaseOrdersController.GetPurchaseOrdersByGuidAsync2(null);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_KeyNotFoundExecpt()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_PermissionsException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_ArgumentException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_RepositoryException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_IntegrationApiException()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseOrdersControllerTests_GetPurchaseOrdersAsync2_Exception()
        {
            _purchaseOrdersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _purchaseOrdersController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            _mockPurchaseOrdersService.Setup(x => x.GetPurchaseOrdersAsync2(offset, limit, It.IsAny<bool>())).ThrowsAsync(new Exception());
            var actuals = await _purchaseOrdersController.GetPurchaseOrdersAsync2(page);
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
    }

    [TestClass]
    public class PurchaseOrdersControllerTests_V11
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
            public async Task PoController_PostPurchaseOrdersAsync2_IntegrationApiException_When_PurchaseOrder_Null()
            {
                await purchaseOrdersController.PostPurchaseOrdersAsync2(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Vendor_Null()
            {
                purchaseOrders.Vendor = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Vendor_ExistingVendor_Null()
            {
                purchaseOrders.Vendor.ExistingVendor.Vendor.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_OrderOn_Is_Default_Date()
            {
                purchaseOrders.OrderedOn = default(DateTime);

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_TransactionDate_Is_Default_Date()
            {
                purchaseOrders.TransactionDate = default(DateTime);

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_OrderOn_GreaterThan_DeliveredBy()
            {
                purchaseOrders.OrderedOn = DateTime.Today.AddDays(5);

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Destination_Country_Not_CAN_AND_USA()
            {
                purchaseOrders.OverrideShippingDestination.Place.Country.Code = IsoCode.AUS;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_Contact_Ext_Length_Morethan_4()
            {
                purchaseOrders.OverrideShippingDestination.Contact.Extension = "12345";

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_ManualVendor_Country_Not_CAN_AND_USA()
            {
                purchaseOrders.Vendor.ManualVendorDetails.Place.Country.Code = IsoCode.AUS;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_PaymentSource_Null()
            {
                purchaseOrders.PaymentSource = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Comment_Null()
            {
                purchaseOrders.Comments.FirstOrDefault().Comment = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Comment_Type_NotSet()
            {
                purchaseOrders.Comments.FirstOrDefault().Type = CommentTypes.NotSet;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Buyer_Null()
            {
                purchaseOrders.Buyer.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Initiator_Detail_Null()
            {
                purchaseOrders.Initiator.Detail.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_PayTerms_Null()
            {
                purchaseOrders.PaymentTerms.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_Clasification_Null()
            {
                purchaseOrders.Classification.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_SubmittedBy_Null()
            {
                purchaseOrders.SubmittedBy.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_Null()
            {
                purchaseOrders.LineItems = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_CommodityCode_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().CommodityCode.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitOfMeasure_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitOfMeasure.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitPrice_Value_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitPrice.Value = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitPrice_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitPrice.Currency = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Value_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Value = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Currency = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TaxCodeId_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().TaxCodes.FirstOrDefault().Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TradeDiscount_AmountAndPer_NotNull()
            {
                purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Percent = 10;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TradeDiscount_Amount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Value = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TradeDiscount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Currency = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AccountingString_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().AccountingString = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Value = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Currency = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Value = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Currency = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Value = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Currency = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Value = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Currency = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_SubmittedBy_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().SubmittedBy.Id = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_Description_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().Description = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_Quantity_Is_Zero()
            {
                purchaseOrders.LineItems.FirstOrDefault().Quantity = 0;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitPrice_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitPrice = null;

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_PartNumber_Length_Morethan_11()
            {
                purchaseOrders.LineItems.FirstOrDefault().PartNumber = "012345678911";

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_KeyNotFoundException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync2(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new KeyNotFoundException());

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_PermissionsException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync2(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new PermissionsException());

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_RepositoryException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync2(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new RepositoryException());

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_IntegrationApiException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync2(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new IntegrationApiException());

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ConfigurationException()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync2(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new ConfigurationException());

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_Exception()
            {
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync2(It.IsAny<PurchaseOrders2>())).ThrowsAsync(new Exception());

                await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);
            }

            [TestMethod]
            public async Task PoController_PostPurchaseOrdersAsync2()
            {
                purchaseOrders.Id = "00000000-0000-0000-0000-000000000000";
                purchaseOrderServiceMock.Setup(s => s.PostPurchaseOrdersAsync2(It.IsAny<PurchaseOrders2>())).ReturnsAsync(purchaseOrders);

                var result = await purchaseOrdersController.PostPurchaseOrdersAsync2(purchaseOrders);

                Assert.IsNotNull(result);
                Assert.AreEqual(purchaseOrders.Id, result.Id);
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
                    ExistingVendor = new Dtos.DtoProperties.PurchaseOrdersExistingVendorDtoProperty() {
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

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_IntegrationApiException_When_Guid_Null()
            {
                await purchaseOrdersController.PutPurchaseOrdersAsync2(null, It.IsAny<PurchaseOrders2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_IntegrationApiException_When_PurchaseOrder_Null()
            {
                await purchaseOrdersController.PutPurchaseOrdersAsync2("1", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_IntegrationApiException_When_Guid_Is_EmptyGuid()
            {
                await purchaseOrdersController.PutPurchaseOrdersAsync2(Guid.Empty.ToString(), purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_IntegrationApiException_When_Guid_And_PurchaseOrderId_NotEqual()
            {
                await purchaseOrdersController.PutPurchaseOrdersAsync2("2", purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Vendor_Null()
            {
                purchaseOrders.Vendor = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Vendor_ExistingVendor_Null()
            {
                purchaseOrders.Vendor.ExistingVendor.Vendor.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_OrderOn_Is_Default_Date()
            {
                purchaseOrders.OrderedOn = default(DateTime);

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_TransactionDate_Is_Default_Date()
            {
                purchaseOrders.TransactionDate = default(DateTime);

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_OrderOn_GreaterThan_DeliveredBy()
            {
                purchaseOrders.OrderedOn = DateTime.Today.AddDays(5);

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Destination_Country_Not_CAN_AND_USA()
            {
                purchaseOrders.OverrideShippingDestination.Place.Country.Code = IsoCode.AUS;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_Contact_Ext_Length_Morethan_4()
            {
                purchaseOrders.OverrideShippingDestination.Contact.Extension = "12345";

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_ManualVendor_Country_Not_CAN_AND_USA()
            {
                purchaseOrders.Vendor.ManualVendorDetails.Place.Country = new AddressCountry();
                purchaseOrders.Vendor.ManualVendorDetails.Place.Country.Code = IsoCode.AUS;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_PaymentSource_Null()
            {
                purchaseOrders.PaymentSource = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Comment_Null()
            {
                purchaseOrders.Comments.FirstOrDefault().Comment = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Comment_Type_NotSet()
            {
                purchaseOrders.Comments.FirstOrDefault().Type = CommentTypes.NotSet;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Buyer_Null()
            {
                purchaseOrders.Buyer.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Initiator_Detail_Null()
            {
                purchaseOrders.Initiator.Detail.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_PayTerms_Null()
            {
                purchaseOrders.PaymentTerms.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_Clasification_Null()
            {
                purchaseOrders.Classification.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_SubmittedBy_Null()
            {
                purchaseOrders.SubmittedBy.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_Null()
            {
                purchaseOrders.LineItems = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_CommodityCode_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().CommodityCode.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitOfMeasure_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitOfMeasure.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitPrice_Value_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitPrice.Value = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitPrice_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitPrice.Currency = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Value_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Value = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AdditionalAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AdditionalAmount.Currency = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TaxCodeId_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().TaxCodes.FirstOrDefault().Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TradeDiscount_AmountAndPer_NotNull()
            {
                purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Percent = 10;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TradeDiscount_Amount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Value = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_TradeDiscount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().TradeDiscount.Amount.Currency = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AccountingString_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().AccountingString = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Value = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocatedAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.Allocated.Amount.Currency = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Value = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_AllocationTaxAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.TaxAmount.Currency = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Value = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_AdditionalAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.AdditionalAmount.Currency = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Value = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_DiscountAmount_Currency_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation.DiscountAmount.Currency = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_SubmittedBy_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().SubmittedBy.Id = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_AccountDetails_Allocation_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().AccountDetail.FirstOrDefault().Allocation = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_Description_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().Description = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_Quantity_Is_Zero()
            {
                purchaseOrders.LineItems.FirstOrDefault().Quantity = 0;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_UnitPrice_Null()
            {
                purchaseOrders.LineItems.FirstOrDefault().UnitPrice = null;

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PutPurchaseOrdersAsync2_ArgumentNullException_When_PO_LineItems_PartNumber_Length_Morethan_11()
            {
                purchaseOrders.LineItems.FirstOrDefault().PartNumber = "012345678911";

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_KeyNotFoundException()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync2(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new KeyNotFoundException());

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_PermissionsException()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync2(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new PermissionsException());

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_RepositoryException()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync2(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new RepositoryException());

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_IntegrationApiException()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync2(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new IntegrationApiException());

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_ConfigurationException()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync2(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new ConfigurationException());

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PoController_PostPurchaseOrdersAsync2_Exception()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync2(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ThrowsAsync(new Exception());

                await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);
            }

            [TestMethod]
            public async Task PoController_PutPurchaseOrdersAsync()
            {
                purchaseOrderServiceMock.Setup(s => s.PutPurchaseOrdersAsync2(It.IsAny<string>(), It.IsAny<PurchaseOrders2>())).ReturnsAsync(purchaseOrders);
                purchaseOrderServiceMock.Setup(s => s.GetPurchaseOrdersByGuidAsync2(guid)).ReturnsAsync(purchaseOrders);

                purchaseOrders.Id = null;

                var result = await purchaseOrdersController.PutPurchaseOrdersAsync2(guid, purchaseOrders);

                Assert.IsNotNull(result);
            }
        }
    }

    #endregion
}