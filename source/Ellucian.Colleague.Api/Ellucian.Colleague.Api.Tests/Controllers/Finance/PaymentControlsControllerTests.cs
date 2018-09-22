//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
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
    public class PaymentControlsControllerTests
    {
        [TestClass]
        public class GetTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private RegistrationPaymentControl rpc;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                rpc = new RegistrationPaymentControl()
                {
                    AcademicCredits = new List<string>() { "101", "102" },
                    Id = "123",
                    InvoiceIds = new List<string>() { "201", "202" },
                    LastPlanApprovalId = "234",
                    LastTermsApprovalId = "345",
                    PaymentPlanId = "456",
                    Payments = new List<string>() { "301", "302" },
                    PaymentStatus = RegistrationPaymentStatus.Complete,
                    RegisteredSectionIds = new List<string>() { "401", "402" },
                    StudentId = "0001234",
                    TermId = "2014/FA"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetPaymentControl("123")).Returns(rpc);
                rbServiceMock.Setup(pc => pc.GetPaymentControl("124")).Throws(new PermissionsException());

                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                rpc = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_Get_Valid()
            {
                var pc = PaymentControlsController.Get("123");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_Get_Exception()
            {
                var pc = PaymentControlsController.Get("124");
            }
        }

        [TestClass]
        public class GetStudentTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private List<RegistrationPaymentControl> rpcs;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                rpcs = new List<RegistrationPaymentControl>() 
                    { 
                        new RegistrationPaymentControl()
                        {
                            AcademicCredits = new List<string>() { "101", "102" },
                            Id = "123",
                            InvoiceIds = new List<string>() { "201", "202" },
                            LastPlanApprovalId = "234",
                            LastTermsApprovalId = "345",
                            PaymentPlanId = "456",
                            Payments = new List<string>() { "301", "302" },
                            PaymentStatus = RegistrationPaymentStatus.Complete,
                            RegisteredSectionIds = new List<string>() { "401", "402" },
                            StudentId = "0001234",
                            TermId = "2014/FA"
                        }
                    };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetStudentPaymentControls("0001234")).Returns(rpcs);
                rbServiceMock.Setup(pc => pc.GetStudentPaymentControls("0001235")).Throws(new PermissionsException());

                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                rpcs = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_GetStudent_Valid()
            {
                var pc = PaymentControlsController.GetStudent("0001234");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_GetStudent_Exception()
            {
                var pc = PaymentControlsController.GetStudent("0001235");
            }
        }

        [TestClass]
        public class GetDocumentTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private TextDocument doc;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                doc = new TextDocument()
                {
                    Text = new List<string>() { "This is some...", "...document text." }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetPaymentControlDocument("0001234", It.IsAny<string>())).Returns(doc);
                rbServiceMock.Setup(pc => pc.GetPaymentControlDocument("0001235", It.IsAny<string>())).Throws(new PermissionsException());

                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                doc = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_GetDocument_Valid()
            {
                var pc = PaymentControlsController.GetDocument("0001234", "DOC");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_GetDocument_Exception()
            {
                var pc = PaymentControlsController.GetDocument("0001235", "DOC");
            }
        }
       
        [TestClass]
        public class PostAcceptTermsTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private RegistrationTermsApproval app;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                app = new RegistrationTermsApproval()
                {
                    AcknowledgementDocument = new ApprovalDocument()
                    {
                        Id = "DOC",
                        PersonId = "0001234",
                        Text = new List<string>() { "This is some...", "...document text." }
                    },
                    Id = "123",
                    InvoiceIds = new List<string>() { "201", "202" },
                    PaymentControlId = "1234",
                    SectionIds = new List<string>() { "301", "302" },
                    StudentId = "0001234",
                    TermsDocument = new ApprovalDocument()
                    {
                        Id = "DOC2",
                        PersonId = "0001234",
                        Text = new List<string>() { "This is some...", "...document text." }
                    },
                    TermsResponse = new ApprovalResponse()
                    {
                        DocumentId = "DOC3",
                        Id = "401",
                        IsApproved = true,
                        PersonId = "0001234",
                        Received = DateTime.Now.AddMinutes(-3),
                        UserId = "jsmith"
                    },
                    Timestamp = DateTime.Now.AddMinutes(-2)
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.ApproveRegistrationTerms(It.IsAny<PaymentTermsAcceptance>())).Returns(app);

                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                app = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_PostAcceptTerms_Valid()
            {
                var pat = PaymentControlsController.PostAcceptTerms(new PaymentTermsAcceptance());
                Assert.IsNotNull(pat);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_PostAcceptTerms_Exception()
            {
                rbServiceMock.Setup(pc => pc.ApproveRegistrationTerms(It.IsAny<PaymentTermsAcceptance>())).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);

                var pat = PaymentControlsController.PostAcceptTerms(new PaymentTermsAcceptance());
            }
        }

        [TestClass]
        public class PostAcceptTerms2Tests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private RegistrationTermsApproval2 app;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                app = new RegistrationTermsApproval2()
                {
                    AcknowledgementDocumentId = "DOC",
                    Id = "123",
                    InvoiceIds = new List<string>() { "201", "202" },
                    PaymentControlId = "1234",
                    SectionIds = new List<string>() { "301", "302" },
                    StudentId = "0001234",
                    TermsResponseId = "DOC2",
                    AcknowledgementTimestamp = DateTime.Now.AddMinutes(-2)
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.ApproveRegistrationTerms2(It.IsAny<PaymentTermsAcceptance2>())).Returns(app);

                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                app = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_PostAcceptTerms2_Valid()
            {
                var pat = PaymentControlsController.PostAcceptTerms2(new PaymentTermsAcceptance2());
                Assert.IsNotNull(pat);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_PostAcceptTerms2_Exception()
            {
                rbServiceMock.Setup(pc => pc.ApproveRegistrationTerms2(It.IsAny<PaymentTermsAcceptance2>())).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);

                var pat = PaymentControlsController.PostAcceptTerms2(new PaymentTermsAcceptance2());
            }
        }

        [TestClass]
        public class GetOptionsTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private ImmediatePaymentOptions ipo;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                ipo = new ImmediatePaymentOptions()
                {
                    ChargesOnPaymentPlan = false,
                    DeferralPercentage = 75m,
                    DownPaymentAmount = 1000m,
                    DownPaymentDate = DateTime.Today.AddDays(3),
                    MinimumPayment = 1000m,
                    PaymentPlanAmount = 5000m,
                    PaymentPlanFirstDueDate = DateTime.Today.AddDays(7),
                    PaymentPlanReceivableTypeCode = "01",
                    PaymentPlanTemplateId = "DEFAULT",
                    RegistrationBalance = 5000m
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetPaymentOptions("0001234")).Returns(ipo);
                rbServiceMock.Setup(pc => pc.GetPaymentOptions("0001235")).Throws(new PermissionsException());

                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                ipo = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_GetOptions_Valid()
            {
                var pc = PaymentControlsController.GetOptions("0001234");
                Assert.IsNotNull(pc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_GetOptions_Exception()
            {
                var pc = PaymentControlsController.GetOptions("0001235");
                Assert.IsNotNull(pc);
            }
        }

        [TestClass]
        public class PutTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private RegistrationPaymentControl rpc;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                rpc = new RegistrationPaymentControl()
                {
                    AcademicCredits = new List<string>() { "101", "102" },
                    Id = "123",
                    InvoiceIds = new List<string>() { "201", "202" },
                    LastPlanApprovalId = "234",
                    LastTermsApprovalId = "345",
                    PaymentPlanId = "456",
                    Payments = new List<string>() { "301", "302" },
                    PaymentStatus = RegistrationPaymentStatus.Complete,
                    RegisteredSectionIds = new List<string>() { "401", "402" },
                    StudentId = "0001234",
                    TermId = "2014/FA"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.UpdatePaymentControl(It.IsAny<RegistrationPaymentControl>())).Returns(rpc);

                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                rpc = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_Put_Valid()
            {
                var put = PaymentControlsController.Put(new RegistrationPaymentControl());
                Assert.IsNotNull(put);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_Put_Exception()
            {
                rbServiceMock.Setup(pc => pc.UpdatePaymentControl(It.IsAny<RegistrationPaymentControl>())).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);

                var put = PaymentControlsController.Put(new RegistrationPaymentControl());
            }
        }

        [TestClass]
        public class GetSummaryTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private List<Payment> pmts;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                pmts = new List<Payment>()
                {
                    new Payment()
                    {
                        AmountToPay = 5000m,
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetPaymentSummary("123", It.IsAny<string>(), It.IsAny<decimal>())).Returns(pmts);
                rbServiceMock.Setup(pc => pc.GetPaymentSummary("124", It.IsAny<string>(), It.IsAny<decimal>())).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                pmts = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_GetSummary_Valid()
            {
                var summary = PaymentControlsController.GetSummary("123", "ECHK", 5000m);
                Assert.IsNotNull(summary);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_GetSummary_Exception()
            {
                var summary = PaymentControlsController.GetSummary("124", "ECHK", 5000m);
            }
        }

        [TestClass]
        public class PostStartPaymentTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private PaymentProvider prov;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                prov = new PaymentProvider
                {
                    RedirectUrl = "http://www.paymentprovider.com/redirect"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.StartRegistrationPayment(It.IsAny<Payment>())).Returns(prov);
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                prov = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_PostStartPayment_Valid()
            {
                var pmt = PaymentControlsController.PostStartPayment(new Payment());
                Assert.IsNotNull(pmt);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_PostStartPayment_Exception()
            {
                rbServiceMock.Setup(pc => pc.StartRegistrationPayment(It.IsAny<Payment>())).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);

                var pmt = PaymentControlsController.PostStartPayment(new Payment());
            }
        }

        [TestClass]
        public class GetTermsApprovalTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private RegistrationTermsApproval app;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                app = new RegistrationTermsApproval()
                {
                    AcknowledgementDocument = new ApprovalDocument()
                    {
                        Id = "DOC",
                        PersonId = "0001234",
                        Text = new List<string>() { "This is some...", "...document text." }
                    },
                    Id = "123",
                    InvoiceIds = new List<string>() { "201", "202" },
                    PaymentControlId = "1234",
                    SectionIds = new List<string>() { "301", "302" },
                    StudentId = "0001234",
                    TermsDocument = new ApprovalDocument()
                    {
                        Id = "DOC2",
                        PersonId = "0001234",
                        Text = new List<string>() { "This is some...", "...document text." }
                    },
                    TermsResponse = new ApprovalResponse()
                    {
                        DocumentId = "DOC3",
                        Id = "401",
                        IsApproved = true,
                        PersonId = "0001234",
                        Received = DateTime.Now.AddMinutes(-3),
                        UserId = "jsmith"
                    },
                    Timestamp = DateTime.Now.AddMinutes(-2)
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetTermsApproval("123")).Returns(app);
                rbServiceMock.Setup(pc => pc.GetTermsApproval("124")).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                app = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_GetTermsApproval_Valid()
            {
                var pat = PaymentControlsController.GetTermsApproval("123");
                Assert.IsNotNull(pat);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_GetTermsApproval_Exception()
            {
                var pat = PaymentControlsController.GetTermsApproval("124");
            }
        }

        [TestClass]
        public class GetTermsApproval2Tests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private RegistrationTermsApproval2 app;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                app = new RegistrationTermsApproval2()
                {
                    AcknowledgementDocumentId = "DOC",
                    Id = "123",
                    InvoiceIds = new List<string>() { "201", "202" },
                    PaymentControlId = "1234",
                    SectionIds = new List<string>() { "301", "302" },
                    StudentId = "0001234",
                    TermsResponseId = "DOC2",
                    AcknowledgementTimestamp = DateTime.Now.AddMinutes(-2)
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetTermsApproval2("123")).Returns(app);
                rbServiceMock.Setup(pc => pc.GetTermsApproval2("124")).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                app = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_GetTermsApproval2_Valid()
            {
                var pat = PaymentControlsController.GetTermsApproval2("123");
                Assert.IsNotNull(pat);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_GetTermsApproval2_Exception()
            {
                var pat = PaymentControlsController.GetTermsApproval2("124");
            }
        }
        
        [TestClass]
        public class GetProposedPaymentPlanTests
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
            private Mock<IRegistrationBillingService> rbServiceMock;

            private PaymentPlan plan;
            private HttpResponse response;

            private PaymentControlsController PaymentControlsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                rbServiceMock = new Mock<IRegistrationBillingService>();

                plan = new PaymentPlan()
                {
                    CurrentAmount = 5000m
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                rbServiceMock.Setup(pc => pc.GetProposedPaymentPlan("123", It.IsAny<string>())).Returns(plan);
                rbServiceMock.Setup(pc => pc.GetProposedPaymentPlan("124", It.IsAny<string>())).Throws(new PermissionsException());
                PaymentControlsController = new PaymentControlsController(rbServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                rbServiceMock = null;
                plan = null;
                PaymentControlsController = null;
            }

            [TestMethod]
            public void PaymentControlsController_GetProposedPaymentPlan_Valid()
            {
                var pat = PaymentControlsController.GetProposedPaymentPlan("123", "01");
                Assert.IsNotNull(pat);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentControlsController_GetProposedPaymentPlan_Exception()
            {
                var pat = PaymentControlsController.GetProposedPaymentPlan("124", "02");
            }
        }
    }
}
