//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Finance
{
    [TestClass]
    public class ReceivablesControllerTests
    {
        [TestClass]
        public class GetAccountHolderTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private AccountHolder accountHolder;
            private HttpResponse response;
            private PrivacyWrapper<AccountHolder> privacyWrapper;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                accountHolder = new AccountHolder()
                {
                    Id = "0001234",
                    LastName = "Smith",
                    FirstName = "John",
                    DepositsDue = new List<DepositDue>()
                    {
                        new DepositDue() { Id = "123", Amount = 500m }
                    }
                };

                privacyWrapper = new PrivacyWrapper<AccountHolder>(accountHolder, false);

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.GetAccountHolder("0001234")).Returns(accountHolder);
                arServiceMock.Setup(pc => pc.GetAccountHolder2("0001234")).Returns(privacyWrapper);
                arServiceMock.Setup(pc => pc.GetAccountHolder("0001235")).Throws(new PermissionsException());
                arServiceMock.Setup(pc => pc.GetAccountHolder2("0001235")).Throws(new PermissionsException());

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountHolder = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public void PaymentController_GetAccountHolder_Valid()
            {
                var pc = ReceivablesController.GetAccountHolder("0001234");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_GetAccountHolder_Exception()
            {
                var pc = ReceivablesController.GetAccountHolder("0001235");
            }
        }

        [TestClass]
        public class SearchAccountHoldersAsync
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private AccountHolder accountHolder;
            private HttpResponse response;
            private PrivacyWrapper<IEnumerable<AccountHolder>> privacyWrapper;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                accountHolder = new AccountHolder()
                {
                    Id = "0001234",
                    LastName = "Smith",
                    FirstName = "John",
                    DepositsDue = new List<DepositDue>()
                    {
                        new DepositDue() { Id = "123", Amount = 500m }
                    }
                };

                privacyWrapper = new PrivacyWrapper<IEnumerable<AccountHolder>>(new List<AccountHolder>() { accountHolder }, false);

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync("0001234")).ReturnsAsync(new List<AccountHolder>() { accountHolder });
                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync("0001235")).ThrowsAsync(new PermissionsException());
                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync("0001236")).ThrowsAsync(new ArgumentException());

                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync2("0001234")).ReturnsAsync(privacyWrapper);
                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync2("0001235")).ThrowsAsync(new PermissionsException());
                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync2("0001236")).ThrowsAsync(new ArgumentException());

                arServiceMock.Setup(pc => pc.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = "0001234" })).ReturnsAsync(privacyWrapper);
                arServiceMock.Setup(pc => pc.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = "0001235" })).ThrowsAsync(new PermissionsException());
                arServiceMock.Setup(pc => pc.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = "0001236" })).ThrowsAsync(new ArgumentException());


                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountHolder = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public async Task PaymentController_QueryAccountHoldersByPostAsync_Valid()
            {
                var pc = await ReceivablesController.QueryAccountHoldersByPostAsync("0001234");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentController_QueryAccountHoldersByPostAsync_PermissionsException()
            {
               var pc = await ReceivablesController.QueryAccountHoldersByPostAsync("0001235");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentController_QueryAccountHoldersByPostAsync_GenericException()
            {
                var pc = await ReceivablesController.QueryAccountHoldersByPostAsync("0001236");
            }
        }

        [TestClass]
        public class QueryAccountHoldersByPost2Async
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private AccountHolder accountHolder;
            private HttpResponse response;
            private PrivacyWrapper<IEnumerable<AccountHolder>> privacyWrapper;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                accountHolder = new AccountHolder()
                {
                    Id = "0001234",
                    LastName = "Smith",
                    FirstName = "John",
                    DepositsDue = new List<DepositDue>()
                    {
                        new DepositDue() { Id = "123", Amount = 500m }
                    }
                };

                privacyWrapper = new PrivacyWrapper<IEnumerable<AccountHolder>>(new List<AccountHolder>() { accountHolder }, false) { HasPrivacyRestrictions = true };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync2(It.IsAny<string>())).ReturnsAsync(privacyWrapper);

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountHolder = null;
                ReceivablesController = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullCriteria_QueryAccountHoldersByPost2Async_ThrowsHttpResponseExceptionTest()
            {
                await ReceivablesController.QueryAccountHoldersByPostAsync2(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullIdsAndKeyword_QueryAccountHoldersByPost2Async_ThrowsHttpResponseExceptionTest()
            {
                await ReceivablesController.QueryAccountHoldersByPostAsync2(null);
            }

            [TestMethod]
            public async Task QueryAccountHoldersByPost2Async_ReturnsExpectedResultTest()
            {
                var accountHolders = await ReceivablesController.QueryAccountHoldersByPostAsync2("0001234");
                Assert.AreEqual(1, accountHolders.Count());
            }

            [TestMethod]
            public async Task QueryAccountHoldersByPost2Async_CatchesAndHandlesPermissionsExceptionTest()
            {
                bool exceptionThrown = false;
                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync2(It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
                try
                {
                    await ReceivablesController.QueryAccountHoldersByPostAsync2("0001235");
                }
                catch (HttpResponseException e)
                {
                    exceptionThrown = true;
                    Assert.AreEqual(HttpStatusCode.Forbidden, e.Response.StatusCode);
                }
                Assert.IsTrue(exceptionThrown);
            }

            [TestMethod]
            public async Task QueryAccountHoldersByPost2Async_CatchesAndHandlesGenericExceptionTest()
            {
                bool exceptionThrown = false;
                arServiceMock.Setup(pc => pc.SearchAccountHoldersAsync2(It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
                try
                {
                    await ReceivablesController.QueryAccountHoldersByPostAsync2("0001235");
                }
                catch (HttpResponseException e)
                {
                    exceptionThrown = true;
                    Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestClass]
        public class QueryAccountHoldersByPost3Async
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private AccountHolder accountHolder;
            private HttpResponse response;
            private PrivacyWrapper<IEnumerable<AccountHolder>> privacyWrapper;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                accountHolder = new AccountHolder()
                {
                    Id = "0001234",
                    LastName = "Smith",
                    FirstName = "John",
                    DepositsDue = new List<DepositDue>()
                    {
                        new DepositDue() { Id = "123", Amount = 500m }
                    }
                };

                privacyWrapper = new PrivacyWrapper<IEnumerable<AccountHolder>>(new List<AccountHolder>() { accountHolder }, false) { HasPrivacyRestrictions = true };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.SearchAccountHolders3Async(It.IsAny<AccountHolderQueryCriteria>())).ReturnsAsync(privacyWrapper);

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountHolder = null;
                ReceivablesController = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullCriteria_QueryAccountHoldersByPost3Async_ThrowsHttpResponseExceptionTest()
            {
                await ReceivablesController.QueryAccountHoldersByPost3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullIdsAndKeyword_QueryAccountHoldersByPost3Async_ThrowsHttpResponseExceptionTest()
            {
                await ReceivablesController.QueryAccountHoldersByPost3Async(new AccountHolderQueryCriteria() { QueryKeyword = null, Ids = null});
            }

            [TestMethod]
            public async Task QueryAccountHoldersByPost3Async_ReturnsExpectedResultTest()
            {
                var accountHolders = await ReceivablesController.QueryAccountHoldersByPost3Async(new AccountHolderQueryCriteria() { QueryKeyword = "0001234", Ids = null });
                Assert.AreEqual(1, accountHolders.Count());
            }

            [TestMethod]
            public async Task QueryAccountHoldersByPost3Async_CatchesAndHandlesPermissionsExceptionTest()
            {
                bool exceptionThrown = false;
                arServiceMock.Setup(pc => pc.SearchAccountHolders3Async(It.IsAny<AccountHolderQueryCriteria>()))
                    .ThrowsAsync(new PermissionsException());
                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
                try
                {
                    await ReceivablesController.QueryAccountHoldersByPost3Async(new AccountHolderQueryCriteria() { QueryKeyword = "0001235", Ids = new List<string>() });
                }
                catch (HttpResponseException e)
                {
                    exceptionThrown = true;
                    Assert.AreEqual(HttpStatusCode.Forbidden, e.Response.StatusCode);
                }
                Assert.IsTrue(exceptionThrown);
            }

            [TestMethod]
            public async Task QueryAccountHoldersByPost3Async_CatchesAndHandlesGenericExceptionTest()
            {
                bool exceptionThrown = false;
                arServiceMock.Setup(pc => pc.SearchAccountHolders3Async(It.IsAny<AccountHolderQueryCriteria>()))
                    .ThrowsAsync(new Exception());
                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
                try
                {
                    await ReceivablesController.QueryAccountHoldersByPost3Async(new AccountHolderQueryCriteria() { QueryKeyword = "0001235", Ids = new List<string>() });
                }
                catch (HttpResponseException e)
                {
                    exceptionThrown = true;
                    Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestClass]
        public class GetInvoicesTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private List<Invoice> invoices;
            private List<string> invoiceIds;
            private List<string> invalidIds;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                invoices = new List<Invoice>()
                {
                    new Invoice() { Id = "123", Amount = 500m },
                    new Invoice() { Id = "124", Amount = 1000m }
                };
                invoiceIds = new List<string>() { "123", "124" };
                invalidIds = new List<string>() { "122", "125" };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.GetInvoices(invoiceIds)).Returns(invoices);
                arServiceMock.Setup(pc => pc.GetInvoices(invalidIds)).Throws(new PermissionsException());

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                invoices = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public void PaymentController_GetInvoices_Valid()
            {
                var pc = ReceivablesController.GetInvoices("123,124");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_GetInvoices_Exception()
            {
                var pc = ReceivablesController.GetInvoices("122,125");
            }

            [TestMethod]
            public void GetInvoices_ReturnsEmptyListTest()
            {
                Assert.IsTrue(!ReceivablesController.GetInvoices(null).Any());
            }
        }

        [TestClass]
        public class QueryInvoicesByPostAsyncTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private IEnumerable<Invoice> invoices;
            private IEnumerable<string> invoiceIds;
            private IEnumerable<string> invalidIds;

            private HttpResponse response;

            private ReceivablesController receivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                invoices = new List<Invoice>()
                {
                    new Invoice() { Id = "123", Amount = 500m },
                    new Invoice() { Id = "124", Amount = 1000m }
                };
                invoiceIds = new List<string>() { "123", "124" };
                invalidIds = new List<string>() { "122", "125" };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.QueryInvoicesAsync(invoiceIds)).Returns(Task.FromResult(invoices));

                receivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                invoices = null;
                receivablesController = null;
            }

            [TestMethod]
            public async Task ReceivablesController_QueryInvoicesAsync_Valid()
            {
                InvoiceQueryCriteria criteria = new InvoiceQueryCriteria();
                criteria.InvoiceIds = new List<string>() { "123", "124" };
                var pc = await receivablesController.QueryInvoicesByPostAsync(criteria);
                Assert.IsNotNull(pc);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReceivablesController_QueryInvoiceAsync_ConvertToHttpResponseException_WhenPermissionException()
            {
                // arrange--Mock permissions exception from student service Get
                arServiceMock.Setup(svc => svc.QueryInvoicesAsync(invalidIds)).ThrowsAsync(new PermissionsException());
                InvoiceQueryCriteria criteria = new InvoiceQueryCriteria();
                criteria.InvoiceIds = invalidIds;
                var response = await receivablesController.QueryInvoicesByPostAsync(criteria);
            }
        }

        [TestClass]
        public class QueryInvoicePaymentsByPostAsyncTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private IEnumerable<InvoicePayment> invoices;
            private IEnumerable<string> invoiceIds;
            private IEnumerable<string> invalidIds;

            private HttpResponse response;

            private ReceivablesController receivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                invoices = new List<InvoicePayment>()
                {
                    new InvoicePayment() { Id = "123", Amount = 500m },
                    new InvoicePayment() { Id = "124", Amount = 1000m }
                };
                invoiceIds = new List<string>() { "123", "124" };
                invalidIds = new List<string>() { "122", "125" };

                receivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                invoices = null;
                receivablesController = null;
            }

            [TestMethod]
            public async Task ReceivablesController_QueryInvoicePaymentsAsync_Valid()
            {
                arServiceMock.Setup(pc => pc.QueryInvoicePaymentsAsync(invoiceIds)).Returns(Task.FromResult(invoices));
                
                InvoiceQueryCriteria criteria = new InvoiceQueryCriteria();
                criteria.InvoiceIds = new List<string>() { "123", "124" };

                // act
                var response = await receivablesController.QueryInvoicePaymentsByPostAsync(criteria);

                // assert
                Assert.IsNotNull(response);
                Assert.IsTrue(response is IEnumerable<InvoicePayment>);
                //Assert.AreEqual(2, response.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReceivablesController_QueryInvoicePaymentsAsync_ConvertToHttpResponseException_WhenPermissionException()
            {
                // arrange--Mock permissions exception from student service Get
                arServiceMock.Setup(svc => svc.QueryInvoicePaymentsAsync(invalidIds)).ThrowsAsync(new PermissionsException());
                InvoiceQueryCriteria criteria = new InvoiceQueryCriteria();
                criteria.InvoiceIds = invalidIds;
                var response = await receivablesController.QueryInvoicePaymentsByPostAsync(criteria);
            }
        }

        [TestClass]
        public class GetPaymentsTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private List<ReceivablePayment> payments;
            private List<string> paymentIds;
            private List<string> invalidIds;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                payments = new List<ReceivablePayment>()
                {
                    new ReceivablePayment() { Id = "123", Amount = 500m },
                    new ReceivablePayment() { Id = "124", Amount = 1000m }
                };
                paymentIds = new List<string>() { "123", "124" };
                invalidIds = new List<string>() { "122", "125" };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.GetPayments(paymentIds)).Returns(payments);
                arServiceMock.Setup(pc => pc.GetPayments(invalidIds)).Throws(new PermissionsException());

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                payments = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public void PaymentController_GetPayments_Valid()
            {
                var pc = ReceivablesController.GetPayments("123,124");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_GetPayments_Exception()
            {
                var pc = ReceivablesController.GetPayments("122,125");
            }
        }

        [TestClass]
        public class GetDepositsDueTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private List<DepositDue> depositsDue;
            private List<string> depDueIds;
            private List<string> invalidIds;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                depositsDue = new List<DepositDue>()
                {
                    new DepositDue() { Id = "123", Amount = 500m },
                    new DepositDue() { Id = "124", Amount = 1000m }
                };
                depDueIds = new List<string>() { "123", "124" };
                invalidIds = new List<string>() { "122", "125" };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.GetDepositsDue("0001234")).Returns(depositsDue);
                arServiceMock.Setup(pc => pc.GetDepositsDue("0001235")).Throws(new PermissionsException());

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                depositsDue = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public void PaymentController_GetDepositsDue_Valid()
            {
                var pc = ReceivablesController.GetDepositsDue("0001234");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_GetDepositsDue_Exception()
            {
                var pc = ReceivablesController.GetDepositsDue("0001235");
            }
        }

        [TestClass]
        public class GetDepositTypesTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private List<DepositType> depositTypes;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                depositTypes = new List<DepositType>()
                {
                    new DepositType() { Code = "MEALS", Description = "Meal Plan Deposit" },
                    new DepositType() { Code = "RESHL", Description = "Residence Hall Deposit" }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.GetDepositTypes()).Returns(depositTypes);

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                depositTypes = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public void PaymentController_GetDepositTypes_Valid()
            {
                var pc = ReceivablesController.GetDepositTypes();
                Assert.IsNotNull(pc);
            }
        }

        [TestClass]
        public class GetReceivableTypesTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private List<ReceivableType> receivableTypes;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                receivableTypes = new List<ReceivableType>()
                {
                    new ReceivableType() { Code = "01", Description = "Student Receivables" },
                    new ReceivableType() { Code = "02", Description = "Continuing Ed Receivables" }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.GetReceivableTypes()).Returns(receivableTypes);

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                receivableTypes = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public void PaymentController_GetReceivableTypes_Valid()
            {
                var pc = ReceivablesController.GetReceivableTypes();
                Assert.IsNotNull(pc);
            }
        }

        [TestClass]
        public class QueryAccountHolderPaymentPlanOptionsAsync
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private PaymentPlanEligibility eligibility;
            private List<BillingTermPaymentPlanInformation> billingTerms;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                billingTerms = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", PaymentPlanTemplateId = "DEFAULT", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", PaymentPlanTemplateId = "UNIQUE", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m }
                };
                eligibility = new PaymentPlanEligibility()
                {
                    EligibleItems = billingTerms
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                ppServiceMock.Setup(pc => pc.GetBillingTermPaymentPlanInformationAsync(It.IsAny<IEnumerable<BillingTermPaymentPlanInformation>>())).ReturnsAsync(eligibility);

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                billingTerms = null;
                ReceivablesController = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentController_QueryAccountHolderPaymentPlanOptionsAsync_Null_BillingTerms()
            {
                await ReceivablesController.QueryAccountHolderPaymentPlanOptionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentController_QueryAccountHolderPaymentPlanOptionsAsync_Permissions_Violation()
            {
                var terms = new List<BillingTermPaymentPlanInformation>()
                    {
                        new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                        new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m },
                        new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM3", ReceivableTypeCode = "03", PaymentPlanAmount = 10000m },
                    };
                ppServiceMock.Setup(pc => pc.GetBillingTermPaymentPlanInformationAsync(It.IsAny<IEnumerable<BillingTermPaymentPlanInformation>>())).ThrowsAsync(new PermissionsException());
                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);

                var bt = await ReceivablesController.QueryAccountHolderPaymentPlanOptionsAsync(terms);
            }

            [TestMethod]
            public async Task PaymentController_QueryAccountHolderPaymentPlanOptionsAsync_Valid()
            {
                var terms = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM3", ReceivableTypeCode = "03", PaymentPlanAmount = 10000m },
                };
                var bt = await ReceivablesController.QueryAccountHolderPaymentPlanOptionsAsync(terms);
                Assert.IsNotNull(bt);
                Assert.IsNotNull(bt.EligibleItems);
                Assert.AreEqual(billingTerms.Count, bt.EligibleItems.Count());
            }
        }

        [TestClass]
        public class QueryAccountHolderPaymentPlanOptions2Async
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private PaymentPlanEligibility eligibility;
            private List<BillingTermPaymentPlanInformation> billingTerms;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                billingTerms = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", PaymentPlanTemplateId = "DEFAULT", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", PaymentPlanTemplateId = "UNIQUE", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m }
                };
                eligibility = new PaymentPlanEligibility()
                {
                    EligibleItems = billingTerms
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                ppServiceMock.Setup(pc => pc.GetBillingTermPaymentPlanInformation2Async(It.IsAny<PaymentPlanQueryCriteria>())).ReturnsAsync(eligibility);

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                billingTerms = null;
                ReceivablesController = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentController_QueryAccountHolderPaymentPlanOptionsAsync_Null_BillingTerms()
            {
                await ReceivablesController.QueryAccountHolderPaymentPlanOptions2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentController_QueryAccountHolderPaymentPlanOptionsAsync_Permissions_Violation()
            {
                var terms = new PaymentPlanQueryCriteria()
                {
                    BillingTerms = new List<BillingTermPaymentPlanInformation>()
                    {
                        new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                        new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m },
                        new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM3", ReceivableTypeCode = "03", PaymentPlanAmount = 10000m },
                    }
                };
                ppServiceMock.Setup(pc => pc.GetBillingTermPaymentPlanInformation2Async(It.IsAny<PaymentPlanQueryCriteria>())).ThrowsAsync(new PermissionsException());
                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);

                var bt = await ReceivablesController.QueryAccountHolderPaymentPlanOptions2Async(terms);
            }

            [TestMethod]
            public async Task PaymentController_QueryAccountHolderPaymentPlanOptionsAsync_Valid()
            {
                var terms = new PaymentPlanQueryCriteria()
                {
                    BillingTerms = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM3", ReceivableTypeCode = "03", PaymentPlanAmount = 10000m },
                }
                };
                var bt = await ReceivablesController.QueryAccountHolderPaymentPlanOptions2Async(terms);
                Assert.IsNotNull(bt);
                Assert.IsNotNull(bt.EligibleItems);
                Assert.AreEqual(billingTerms.Count, bt.EligibleItems.Count());
            }
        }

        [TestClass]
        public class GetChargeCodesAsyncsTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> arServiceMock;
            private Mock<IPaymentPlanService> ppServiceMock;

            private List<ChargeCode> chargeCodes;

            private HttpResponse response;

            private ReceivablesController ReceivablesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                arServiceMock = new Mock<IAccountsReceivableService>();
                ppServiceMock = new Mock<IPaymentPlanService>();

                chargeCodes = new List<ChargeCode>()
                {
                    new ChargeCode() { Code = "MEALS", Description = "Meal Plan Deposit" },
                    new ChargeCode() { Code = "RESHL", Description = "Residence Hall Deposit" }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(pc => pc.GetChargeCodesAsync()).ReturnsAsync(chargeCodes);

                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                chargeCodes = null;
                ReceivablesController = null;
            }

            [TestMethod]
            public async Task ReceivablesController_GetChargeCodesAsyncs_Valid()
            {
                var pc = await ReceivablesController.GetChargeCodesAsync();
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReceivablesController_GetChargeCodesAsyncs_Error()
            {
                arServiceMock.Setup(pc => pc.GetChargeCodesAsync()).ThrowsAsync(new Exception());
                ReceivablesController = new ReceivablesController(arServiceMock.Object, ppServiceMock.Object, loggerMock.Object);

                var cc = await ReceivablesController.GetChargeCodesAsync();
            }
        }
    }
}
