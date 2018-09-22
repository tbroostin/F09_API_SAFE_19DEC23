// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class PaymentTransactionsControllerTests
    {
        [TestClass]
        public class PaymentTransactionsControllerTests_V12
        {
            [TestClass]
            public class PaymentTransactionsControllerTests_GET
            {
                #region DECLARATION

                public TestContext TestContext { get; set; }

                private PaymentTransactionsController paymentTransactionsController;
                private Mock<IPaymentTransactionsService> paymentTransactionsServiceMock;
                private Mock<ILogger> loggerMock;
                private IEnumerable<Dtos.PaymentTransactions> paymentTransactionsCollection;
                private Tuple<IEnumerable<Dtos.PaymentTransactions>, int> paymentTransactionsTuple;
                private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("document", "");
                private string document = string.Empty;

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                    EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                    paymentTransactionsServiceMock = new Mock<IPaymentTransactionsService>();
                    loggerMock = new Mock<ILogger>();

                    paymentTransactionsController = new PaymentTransactionsController(paymentTransactionsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                    paymentTransactionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                    paymentTransactionsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                    InitializeTestData();
                }

                [TestCleanup]
                public void Cleanup()
                {
                    paymentTransactionsController = null;
                    paymentTransactionsServiceMock = null;
                    loggerMock = null;
                    TestContext = null;
                }

                private void InitializeTestData()
                {
                    paymentTransactionsCollection = new List<Dtos.PaymentTransactions>()
                    {
                        new PaymentTransactions() { Id = "2a082180-b897-46f3-8435-df25caaca922" },
                        new PaymentTransactions() { Id = "2a082180-b897-46f3-8435-df25caaca923" },
                        new PaymentTransactions() { Id = "2a082180-b897-46f3-8435-df25caaca924" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransactions>, int>(paymentTransactionsCollection, 3);
                    document = "{ 'type':'type', 'id':'2a082180-b897-46f3-8435-df25caaca924'}";
                }

                #endregion

                #region CACHE-NOCACHE

                [TestMethod]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_Nocache()
                {
                    paymentTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };

                    paymentTransactionsServiceMock.Setup(x => x.GetPaymentTransactionsAsync(0, 10,string.Empty, Dtos.EnumProperties.InvoiceTypes.NotSet,false)).ReturnsAsync(paymentTransactionsTuple);

                    var results = await paymentTransactionsController.GetPaymentTransactionsAsync(new Paging(10, 0), criteriaFilter);

                    Assert.IsNotNull(results);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.PaymentTransactions> actuals =
                        ((ObjectContent<IEnumerable<Dtos.PaymentTransactions>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PaymentTransactions>;

                    Assert.AreEqual(paymentTransactionsCollection.Count(), actuals.Count());

                    foreach (var actual in actuals)
                    {
                        var expected = paymentTransactionsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(expected);
                        Assert.AreEqual(expected.Id, actual.Id);
                    }
                }

                [TestMethod]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_Cache()
                {
                    paymentTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                    paymentTransactionsServiceMock.Setup(x => x.GetPaymentTransactionsAsync(0, 10, string.Empty, Dtos.EnumProperties.InvoiceTypes.NotSet, true)).ReturnsAsync(paymentTransactionsTuple);

                    var results = await paymentTransactionsController.GetPaymentTransactionsAsync(new Paging(10, 0), criteriaFilter);

                    Assert.IsNotNull(results);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.PaymentTransactions> actuals =
                        ((ObjectContent<IEnumerable<Dtos.PaymentTransactions>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PaymentTransactions>;

                    Assert.AreEqual(paymentTransactionsCollection.Count(), actuals.Count());

                    foreach (var actual in actuals)
                    {
                        var expected = paymentTransactionsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(expected);
                        Assert.AreEqual(expected.Id, actual.Id);
                    }
                }

                #endregion

                #region GETALL

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_KeyNotFoundException()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),It.IsAny<Dtos.EnumProperties.InvoiceTypes>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                    await paymentTransactionsController.GetPaymentTransactionsAsync(null, criteriaFilter);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_PermissionsException()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Dtos.EnumProperties.InvoiceTypes>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    await paymentTransactionsController.GetPaymentTransactionsAsync(null, criteriaFilter);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_ArgumentException()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Dtos.EnumProperties.InvoiceTypes>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                    await paymentTransactionsController.GetPaymentTransactionsAsync(null, criteriaFilter);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_RepositoryException()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Dtos.EnumProperties.InvoiceTypes>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                    await paymentTransactionsController.GetPaymentTransactionsAsync(null, criteriaFilter);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_IntegrationApiException()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Dtos.EnumProperties.InvoiceTypes>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                    await paymentTransactionsController.GetPaymentTransactionsAsync(null, criteriaFilter);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_Exception()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Dtos.EnumProperties.InvoiceTypes>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                    await paymentTransactionsController.GetPaymentTransactionsAsync(null, criteriaFilter);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_DocumentType_Null()
                {
                    var filterGroupName = "document";

                    paymentTransactionsController.Request.Properties.Add(
                          string.Format("FilterObject{0}", filterGroupName),
                          new Dtos.Filters.DocumentFilter()
                          {
                              Document =
                          new Dtos.Filters.DocumentCredentialsDtoProperty()
                          {  Id = "2a082180-b897-46f3-8435-df25caaca924" }
                          });

                    paymentTransactionsServiceMock.Setup(x => x.GetPaymentTransactionsAsync(0, 10, string.Empty, Dtos.EnumProperties.InvoiceTypes.NotSet, false)).ReturnsAsync(paymentTransactionsTuple);
                    await paymentTransactionsController.GetPaymentTransactionsAsync(new Paging(10, 0), criteriaFilter);

                }

                [TestMethod]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_DocumentType_Invoice()
                {
                    var filterGroupName = "document";

                    paymentTransactionsController.Request.Properties.Add(
                          string.Format("FilterObject{0}", filterGroupName),
                          new Dtos.Filters.DocumentFilter()
                          {
                              Document =
                          new Dtos.Filters.DocumentCredentialsDtoProperty()
                          { Type = Dtos.EnumProperties.InvoiceTypes.Invoice, Id = "2a082180-b897-46f3-8435-df25caaca924" }
                          });


                    paymentTransactionsServiceMock.Setup(x => x.GetPaymentTransactionsAsync(0, 10, "2a082180-b897-46f3-8435-df25caaca924", Dtos.EnumProperties.InvoiceTypes.Invoice, false)).ReturnsAsync(paymentTransactionsTuple);

                    var result = await paymentTransactionsController.GetPaymentTransactionsAsync(new Paging(10, 0), criteriaFilter);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.PaymentTransactions> actuals =
                        ((ObjectContent<IEnumerable<Dtos.PaymentTransactions>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PaymentTransactions>;

                    Assert.IsNotNull(actuals);
                    Assert.AreEqual(actuals.Count(), 3);

                }

                [TestMethod]
                public async Task PaymentTransactionsController_GetPaymentTransactionsAsync_DocumentType_Refund()
                {
                    var filterGroupName = "document";

                    paymentTransactionsController.Request.Properties.Add(
                          string.Format("FilterObject{0}", filterGroupName),
                          new Dtos.Filters.DocumentFilter() { Document =
                          new Dtos.Filters.DocumentCredentialsDtoProperty() { Type = Dtos.EnumProperties.InvoiceTypes.Refund, Id = "2a082180-b897-46f3-8435-df25caaca924" } });

                    paymentTransactionsServiceMock.Setup(x => x.GetPaymentTransactionsAsync(0, 10, "2a082180-b897-46f3-8435-df25caaca924", Dtos.EnumProperties.InvoiceTypes.Refund, false)).ReturnsAsync(paymentTransactionsTuple);

                    var result = await paymentTransactionsController.GetPaymentTransactionsAsync(new Paging(10, 0), criteriaFilter);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.PaymentTransactions> actuals =
                        ((ObjectContent<IEnumerable<Dtos.PaymentTransactions>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PaymentTransactions>;

                    Assert.IsNotNull(actuals);
                    Assert.AreEqual(actuals.Count(), 3);
                }
             
                #endregion

                #region GETBYID

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync_KeyNotFound()
                {
                    paymentTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                    await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync_Permission()
                {
                    paymentTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync_Argument()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                    await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync_Repository()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                    await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync_IntegrationApi()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                    await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync_Exception()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                    await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync_Guid_Empty()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                    await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("");
                }

                [TestMethod]
                public async Task PaymentTransactionsController_GetPaymentTransactionsByGuidAsync()
                {
                    paymentTransactionsServiceMock.Setup(e => e.GetPaymentTransactionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(paymentTransactionsCollection.FirstOrDefault());
                    var result = await paymentTransactionsController.GetPaymentTransactionsByGuidAsync("2a082180-b897-46f3-8435-df25caaca922");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Id, paymentTransactionsCollection.FirstOrDefault().Id);
                }

                #endregion

                #region UNSUPPORTED

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_PostPaymentTransactionsAsync_Unsupported()
                {
                    await paymentTransactionsController.PostPaymentTransactionsAsync(new PaymentTransactions() { });
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_PutPaymentTransactionsAsync_Unsupported()
                {
                    await paymentTransactionsController.PutPaymentTransactionsAsync("2a082180-b897-46f3-8435-df25caaca922", new PaymentTransactions() { });
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PaymentTransactionsController_DeletePaymentTransactionsAsync_Unsupported()
                {
                    await paymentTransactionsController.DeletePaymentTransactionsAsync("2a082180-b897-46f3-8435-df25caaca922");
                }

                #endregion


            }
        }
    }
}
