//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Finance
{
    [TestClass]
    public class PaymentControllerTests
    {
        [TestClass]
        public class GetPaymentConfirmationTests
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
            private Mock<IPaymentService> paymentServiceMock;
            private Mock<IAccountsReceivableService> arServiceMock;

            private PaymentConfirmation confirmation;
            private HttpResponse response;

            private PaymentController PaymentController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                paymentServiceMock = new Mock<IPaymentService>();
                arServiceMock = new Mock<IAccountsReceivableService>();

                confirmation = new PaymentConfirmation()
                {
                    ConfirmationText = new List<string>() { "This is some...", "...confirmation text." },
                    ConvenienceFeeAmount = 5m,
                    ConvenienceFeeCode = "CF5",
                    ConvenienceFeeDescription = "$5.00 Convenience Fee",
                    ConvenienceFeeGeneralLedgerNumber = "110101000000010100",
                    ProviderAccount = "PPCC"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                paymentServiceMock.Setup(pc => pc.GetPaymentConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(confirmation);

                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                paymentServiceMock = null;
                confirmation = null;
                PaymentController = null;
            }

            [TestMethod]
            public void PaymentController_GetPaymentConfirmation_Valid()
            {
                var pc = PaymentController.GetPaymentConfirmation("BANK", "ECHK", "5.00");
                Assert.IsNotNull(pc);
            }
        }

        [TestClass]
        public class PostPaymentProviderTests
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
            private Mock<IPaymentService> paymentServiceMock;
            private Mock<IAccountsReceivableService> arServiceMock;

            private PaymentProvider provider;
            private HttpResponse response;

            private PaymentController PaymentController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                paymentServiceMock = new Mock<IPaymentService>();
                arServiceMock = new Mock<IAccountsReceivableService>();

                provider = new PaymentProvider()
                {
                    RedirectUrl = "http://www.paymentprovider.com"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                paymentServiceMock.Setup(pc => pc.PostPaymentProvider(It.IsAny<Payment>())).Returns(provider);

                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                paymentServiceMock = null;
                provider = null;
                PaymentController = null;
            }

            [TestMethod]
            public void PaymentController_PostPaymentProvider_Valid()
            {
                var pp = PaymentController.PostPaymentProvider(new Payment());
                Assert.IsNotNull(pp);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_PostPaymentProvider_Exception()
            {
                paymentServiceMock.Setup(pc => pc.PostPaymentProvider(It.IsAny<Payment>())).Throws(new PermissionsException());
                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);

                var pp = PaymentController.PostPaymentProvider(new Payment());
            }
        }

        [TestClass]
        public class GetPaymentReceiptTests
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
            private Mock<IPaymentService> paymentServiceMock;
            private Mock<IAccountsReceivableService> arServiceMock;

            private PaymentReceipt receipt;
            private HttpResponse response;

            private PaymentController PaymentController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                paymentServiceMock = new Mock<IPaymentService>();
                arServiceMock = new Mock<IAccountsReceivableService>();

                receipt = new PaymentReceipt()
                {
                    AcknowledgeFooterImageUrl = new Uri("http://www.paymentprovider.com/images/footer.png"),
                    AcknowledgeFooterText = new List<string>() { "This is some...", "...footer text." },
                    CashReceiptsId = "123",
                    ConvenienceFees = new List<ConvenienceFee>() 
                    {
                        new ConvenienceFee() { Amount = 5m, Code = "CF5", Description = "$5.00 Convenience Fee" }
                    },
                    MerchantEmail = "support@ellucianuniversity.edu",
                    MerchantNameAddress = new List<string>() { "Ellucian University", "123 Main Street", "Fairfax, VA 22033" },
                    MerchantPhone = "703-968-0000",
                    PaymentMethods = new List<PaymentMethod>()
                    {
                        new PaymentMethod() { ConfirmationNumber = "CONF123", ControlNumber = "CTRL123", PayMethodCode = "CC", PayMethodDescription = "Credit Card", 
                            TransactionAmount = 505m, TransactionDescription = "Student Receivables", TransactionNumber = "123" }
                    },
                    Payments = new List<AccountsReceivablePayment>()
                    {
                        new AccountsReceivablePayment()
                        {
                            Description = "Student Payment",
                            Location = "MC",
                            LocationDescription = "Main Campus",
                            NetAmount = 505m,
                            PaymentDescription = "Credit Card",
                            PersonId = "0001234",
                            PersonName = "John Smith",
                            Term = "2014/FA",
                            TermDescription = "2014 Fall Term",
                            Type = "01"
                        }
                    },
                    ReceiptAcknowledgeText = new List<string>() { "This is some...", "...acknowledgement text." },
                    ReceiptDate = DateTime.Today.AddDays(-7),
                    ReceiptNo = "1234",
                    ReceiptPayerId = "0001234",
                    ReceiptPayerName = "John Smith",
                    ReceiptTime = DateTime.Today.AddDays(-7).AddHours(4).AddMinutes(30),
                    ReturnUrl = "http://www.ellucianuniversity.edu/return"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                paymentServiceMock.Setup(pc => pc.GetPaymentReceipt(It.IsAny<string>(), It.IsAny<string>())).Returns(receipt);

                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                paymentServiceMock = null;
                receipt = null;
                PaymentController = null;
            }

            [TestMethod]
            public void PaymentController_GetPaymentReceipt_Valid()
            {
                var pc = PaymentController.GetPaymentReceipt("BANK", "ECHK");
                Assert.IsNotNull(pc);
            }
        }

        [TestClass]
        public class PostProcessElectronicCheckTests
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
            private Mock<IPaymentService> paymentServiceMock;
            private Mock<IAccountsReceivableService> arServiceMock;

            private ElectronicCheckProcessingResult result;
            private HttpResponse response;

            private PaymentController PaymentController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                paymentServiceMock = new Mock<IPaymentService>();
                arServiceMock = new Mock<IAccountsReceivableService>();

                result = new ElectronicCheckProcessingResult()
                {
                    CashReceiptsId = "123"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                paymentServiceMock.Setup(pc => pc.PostProcessElectronicCheck(It.IsAny<Payment>())).Returns(result);

                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                paymentServiceMock = null;
                result = null;
                PaymentController = null;
            }

            [TestMethod]
            public void PaymentController_PostProcessElectronicCheck_Valid()
            {
                var pp = PaymentController.PostProcessElectronicCheck(new Payment());
                Assert.IsNotNull(pp);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_PostProcessElectronicCheck_Exception()
            {
                paymentServiceMock.Setup(pc => pc.PostProcessElectronicCheck(It.IsAny<Payment>())).Throws(new PermissionsException());
                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);

                var pp = PaymentController.PostProcessElectronicCheck(new Payment());
            }
        }

        [TestClass]
        public class GetCheckPayerInformationTests
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
            private Mock<IPaymentService> paymentServiceMock;
            private Mock<IAccountsReceivableService> arServiceMock;

            private ElectronicCheckPayer payer;
            private HttpResponse response;

            private PaymentController PaymentController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                paymentServiceMock = new Mock<IPaymentService>();
                arServiceMock = new Mock<IAccountsReceivableService>();

                payer = new ElectronicCheckPayer()
                {
                    City = "Fairfax",
                    Country = "United States",
                    Email = "john.smith@ellucianuniversity.edu",
                    FirstName = "John",
                    LastName = "Smith",
                    PostalCode = "22033",
                    State = "VA",
                    Street = "123 Main Street",
                    Telephone = "703-968-0001"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                paymentServiceMock.Setup(pc => pc.GetCheckPayerInformation("0001234")).Returns(payer);
                paymentServiceMock.Setup(pc => pc.GetCheckPayerInformation("0001235")).Throws(new PermissionsException());

                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                paymentServiceMock = null;
                payer = null;
                PaymentController = null;
            }

            [TestMethod]
            public void PaymentController_GetCheckPayerInformation_Valid()
            {
                var cpi = PaymentController.GetCheckPayerInformation("0001234");
                Assert.IsNotNull(cpi);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_GetCheckPayerInformation_Exception()
            {
                var cpi = PaymentController.GetCheckPayerInformation("0001235");
            }
        }

        [TestClass]
        public class GetPaymentDistributionsTests
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
            private Mock<IPaymentService> paymentServiceMock;
            private Mock<IAccountsReceivableService> arServiceMock;

            private List<string> distributions;
            private HttpResponse response;

            private PaymentController PaymentController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                paymentServiceMock = new Mock<IPaymentService>();
                arServiceMock = new Mock<IAccountsReceivableService>();

                distributions = new List<string>() { "BANK", "TRAV" };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                arServiceMock.Setup(ar => ar.GetDistributions("0001234", It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Returns(distributions);
                arServiceMock.Setup(ar => ar.GetDistributions("0001235", It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Throws(new PermissionsException());

                PaymentController = new PaymentController(paymentServiceMock.Object, arServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                paymentServiceMock = null;
                distributions = null;
                PaymentController = null;
            }

            [TestMethod]
            public void PaymentController_GetPaymentDistributions_Valid()
            {
                var cpi = PaymentController.GetPaymentDistributions("0001234", "BANK", "IPC");
                Assert.IsNotNull(cpi);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentController_GetPaymentDistributions_Exception()
            {
                var cpi = PaymentController.GetPaymentDistributions("0001235", "BANK", "IPC");
            }
        }

    }
}
