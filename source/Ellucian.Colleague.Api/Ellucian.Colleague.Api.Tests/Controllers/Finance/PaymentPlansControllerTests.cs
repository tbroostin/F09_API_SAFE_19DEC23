//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Finance
{
    [TestClass]
    public class PaymentPlansControllerTests
    {
        [TestClass]
        public class GetPaymentPlanTemplatesTests
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
            private Mock<IPaymentPlanService> configServiceMock;

            private List<PaymentPlanTemplate> allTemplates;
            private HttpResponse response;

            private PaymentPlansController PaymentPlansController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IPaymentPlanService>();

                allTemplates = new List<PaymentPlanTemplate>()
                {
                    new PaymentPlanTemplate()
                    {
                        Id = "DEFAULT"
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.GetPaymentPlanTemplates()).Returns(allTemplates);

                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                allTemplates = null;
                PaymentPlansController = null;
            }

            [TestMethod]
            public void PaymentPlansController_GetPaymentPlanTemplates_Valid()
            {
                var templates = PaymentPlansController.GetPaymentPlanTemplates();
                Assert.IsNotNull(templates);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanTemplates_Exception()
            {
                configServiceMock.Setup(fc => fc.GetPaymentPlanTemplates()).Throws(new Exception());
                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);

                var exc = PaymentPlansController.GetPaymentPlanTemplates();
            }
        }

        [TestClass]
        public class GetPaymentPlanTests
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
            private Mock<IPaymentPlanService> configServiceMock;

            private PaymentPlan plan;
            private HttpResponse response;

            private PaymentPlansController PaymentPlansController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IPaymentPlanService>();

                plan = new PaymentPlan()
                {
                    Id = "123"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.GetPaymentPlan("123")).Returns(plan);
                configServiceMock.Setup(fc => fc.GetPaymentPlan("124")).Throws(new PermissionsException());
                configServiceMock.Setup(fc => fc.GetPaymentPlan("125")).Throws(new Exception());

                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                plan = null;
                PaymentPlansController = null;
            }

            [TestMethod]
            public void PaymentPlansController_GetPaymentPlan_Valid()
            {
                var templates = PaymentPlansController.GetPaymentPlan("123");
                Assert.IsNotNull(templates);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlan_NullId()
            {
                var templates = PaymentPlansController.GetPaymentPlan(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlan_EmptyId()
            {
                var templates = PaymentPlansController.GetPaymentPlan(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlan_PermissionsException()
            {
                var templates = PaymentPlansController.GetPaymentPlan("124");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlan_GenericException()
            {
                var templates = PaymentPlansController.GetPaymentPlan("125");
            }
        }

        [TestClass]
        public class GetPaymentPlanTemplateTests
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
            private Mock<IPaymentPlanService> configServiceMock;

            private PaymentPlanTemplate template;
            private HttpResponse response;

            private PaymentPlansController PaymentPlansController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IPaymentPlanService>();

                template = new PaymentPlanTemplate()
                {
                    Id = "DEFAULT"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.GetPaymentPlanTemplate("DEFAULT")).Returns(template);
                configServiceMock.Setup(fc => fc.GetPaymentPlanTemplate("INVALID")).Throws(new KeyNotFoundException());
                configServiceMock.Setup(fc => fc.GetPaymentPlanTemplate("CORRUPT")).Throws(new Exception());

                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                template = null;
                PaymentPlansController = null;
            }

            [TestMethod]
            public void PaymentPlansController_GetPaymentPlanTemplate_Valid()
            {
                var templates = PaymentPlansController.GetPaymentPlanTemplate("DEFAULT");
                Assert.IsNotNull(templates);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanTemplate_NullId()
            {
                var templates = PaymentPlansController.GetPaymentPlanTemplate(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanTemplate_EmptyId()
            {
                var templates = PaymentPlansController.GetPaymentPlanTemplate(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanTemplate_PermissionsException()
            {
                var templates = PaymentPlansController.GetPaymentPlanTemplate("INVALID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanTemplate_GenericException()
            {
                var templates = PaymentPlansController.GetPaymentPlanTemplate("CORRUPT");
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
            private Mock<IPaymentPlanService> configServiceMock;

            private PaymentPlanApproval approval;
            private HttpResponse response;

            private PaymentPlansController PaymentPlansController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IPaymentPlanService>();

                approval = new PaymentPlanApproval()
                {
                    Id = "123"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.ApprovePaymentPlanTerms(It.IsAny<PaymentPlanTermsAcceptance>())).Returns(approval);

                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                approval = null;
                PaymentPlansController = null;
            }

            [TestMethod]
            public void PaymentPlansController_PostAcceptTerms_Valid()
            {
                var templates = PaymentPlansController.PostAcceptTerms(new PaymentPlanTermsAcceptance());
                Assert.IsNotNull(templates);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_PostAcceptTerms_PermissionsException()
            {
                configServiceMock.Setup(fc => fc.ApprovePaymentPlanTerms(It.IsAny<PaymentPlanTermsAcceptance>())).Throws(new PermissionsException());
                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);

                var templates = PaymentPlansController.PostAcceptTerms(new PaymentPlanTermsAcceptance());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_PostAcceptTerms_GenericException()
            {
                configServiceMock.Setup(fc => fc.ApprovePaymentPlanTerms(It.IsAny<PaymentPlanTermsAcceptance>())).Throws(new Exception());
                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);

                var templates = PaymentPlansController.PostAcceptTerms(new PaymentPlanTermsAcceptance());
            }
        }

        [TestClass]
        public class GetPaymentPlanApprovalTests
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
            private Mock<IPaymentPlanService> configServiceMock;

            private PaymentPlanApproval approval;
            private HttpResponse response;

            private PaymentPlansController PaymentPlansController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IPaymentPlanService>();

                approval = new PaymentPlanApproval()
                {
                    Id = "123"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.GetPaymentPlanApproval("123")).Returns(approval);
                configServiceMock.Setup(fc => fc.GetPaymentPlanApproval("124")).Throws(new PermissionsException());
                configServiceMock.Setup(fc => fc.GetPaymentPlanApproval("125")).Throws(new Exception());

                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                approval = null;
                PaymentPlansController = null;
            }

            [TestMethod]
            public void PaymentPlansController_GetPaymentPlanApproval_Valid()
            {
                var templates = PaymentPlansController.GetPaymentPlanApproval("123");
                Assert.IsNotNull(templates);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanApproval_NullId()
            {
                var templates = PaymentPlansController.GetPaymentPlanApproval(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanApproval_EmptyId()
            {
                var templates = PaymentPlansController.GetPaymentPlanApproval(string.Empty);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanApproval_PermissionsException()
            {
                var templates = PaymentPlansController.GetPaymentPlanApproval("124");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPaymentPlanApproval_GenericException()
            {
                var templates = PaymentPlansController.GetPaymentPlanApproval("125");
            }
        }

        [TestClass]
        public class GetPlanPaymentSummaryTests
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
            private Mock<IPaymentPlanService> configServiceMock;

            private Payment payment;
            private HttpResponse response;

            private PaymentPlansController PaymentPlansController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IPaymentPlanService>();

                payment = new Payment()
                {
                    PersonId = "0001234"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.GetPlanPaymentSummary("123", It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>())).Returns(payment);
                configServiceMock.Setup(fc => fc.GetPlanPaymentSummary("124", It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>())).Throws(new PermissionsException());

                PaymentPlansController = new PaymentPlansController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                payment = null;
                PaymentPlansController = null;
            }

            [TestMethod]
            public void PaymentPlansController_GetPlanPaymentSummary_Valid()
            {
                var templates = PaymentPlansController.GetPlanPaymentSummary("123", "ECHK", 1000m, "234");
                Assert.IsNotNull(templates);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PaymentPlansController_GetPlanPaymentSummary_PermissionsException()
            {
                var templates = PaymentPlansController.GetPlanPaymentSummary("124", "ECHK", 1000m, "234");
            }
        }

        [TestClass]
        public class GetProposedPaymentPlanAsync
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
            private Mock<IPaymentPlanService> payPlanServiceMock;

            private PaymentPlan paymentPlan;
            private HttpResponse response;

            private PaymentPlansController PaymentPlansController;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                payPlanServiceMock = new Mock<IPaymentPlanService>();

                paymentPlan = new PaymentPlan()
                {
                    Id = "123",
                    PersonId = "0001234",
                    TermId = "2016/FA"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                payPlanServiceMock.Setup(pps => pps.GetProposedPaymentPlanAsync(paymentPlan.PersonId, paymentPlan.TermId, It.IsAny<string>(), 500m)).ReturnsAsync(paymentPlan);
                payPlanServiceMock.Setup(pps => pps.GetProposedPaymentPlanAsync("0001235", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).ThrowsAsync(new PermissionsException());
                payPlanServiceMock.Setup(pps => pps.GetProposedPaymentPlanAsync(paymentPlan.PersonId, paymentPlan.TermId, It.IsAny<string>(), -500m)).ThrowsAsync(new Exception());
                PaymentPlansController = new PaymentPlansController(payPlanServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                payPlanServiceMock = null;
                paymentPlan = null;
                PaymentPlansController = null;
            }

            [TestMethod]
            public async Task PaymentPlansController_GetProposedPaymentPlanAsync_Valid()
            {
                var plan = await PaymentPlansController.GetProposedPaymentPlanAsync(paymentPlan.PersonId, paymentPlan.TermId, "01", 500m);
                Assert.IsNotNull(plan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentPlansController_GetProposedPaymentPlanAsync_PermissionsException()
            {
                var templates = await PaymentPlansController.GetProposedPaymentPlanAsync("0001235", paymentPlan.TermId, "01", 500m);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PaymentPlansController_GetProposedPaymentPlanAsync_GenericException()
            {
                var templates = await PaymentPlansController.GetProposedPaymentPlanAsync(paymentPlan.PersonId, paymentPlan.TermId, "01", -500m);
            }
        }
    }
}
