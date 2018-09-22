//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Ellucian.Web.Adapters;
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
    public class FinanceConfigurationControllerTests
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
            private Mock<IFinanceConfigurationService> configServiceMock;

            private FinanceConfiguration config;
            private HttpResponse response;

            private FinanceConfigurationController FinanceConfigurationController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IFinanceConfigurationService>();

                config = new FinanceConfiguration()
                    {
                        ActivityDisplay = ActivityDisplay.DisplayByTerm,
                        ECommercePaymentsAllowed = true,
                        IncludeDetail = true,
                        IncludeHistory = true,
                        IncludeSchedule = true,
                        InstitutionName = "Ellucian University",
                        PartialAccountPaymentsAllowed = true,
                        PartialDepositPaymentsAllowed = true,
                        PartialPlanPaymentsAllowed = PartialPlanPayments.Allowed,
                        PaymentDisplay = PaymentDisplay.DisplayByTerm,
                        PaymentReviewMessage = "Message.",
                        RemittanceAddress = new List<string>() { "Ellucian University", "123 Main Street", "Fairfax, VA 22033" },
                        SelfServicePaymentsAllowed = true,
                        ShowCreditAmounts = true,
                        StatementTitle = "Student Statement",
                        SupportEmailAddress = "support@ellucianuniversity.edu",
                        UseGuaranteedChecks = false
                    };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.GetFinanceConfiguration()).Returns(config);

                FinanceConfigurationController = new FinanceConfigurationController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                config = null;
                FinanceConfigurationController = null;
            }

            [TestMethod]
            public void FinanceConfigurationController_Get_Valid()
            {
                var fc = FinanceConfigurationController.Get();
                Assert.IsNotNull(fc);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void FinanceConfigurationController_Get_Exception()
            {
                configServiceMock.Setup(fc => fc.GetFinanceConfiguration()).Throws(new Exception());
                FinanceConfigurationController = new FinanceConfigurationController(configServiceMock.Object, loggerMock.Object);

                var exc = FinanceConfigurationController.Get();
            }
        }

        [TestClass]
        public class GetImmediatePaymentControlTests
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
            private Mock<IFinanceConfigurationService> configServiceMock;

            private ImmediatePaymentControl ipc;
            private HttpResponse response;

            private FinanceConfigurationController FinanceConfigurationController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                configServiceMock = new Mock<IFinanceConfigurationService>();

                ipc = new ImmediatePaymentControl()
                {
                    DeferralAcknowledgementDocumentId = "IPCDEFER",
                    IsEnabled = true,
                    RegistrationAcknowledgementDocumentId = "IPCREG",
                    TermsAndConditionsDocumentId = "IPCTERMS"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                configServiceMock.Setup(fc => fc.GetImmediatePaymentControl()).Returns(ipc);

                FinanceConfigurationController = new FinanceConfigurationController(configServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                configServiceMock = null;
                ipc = null;
                FinanceConfigurationController = null;
            }

            [TestMethod]
            public void FinanceConfigurationController_GetImmediatePaymentControl_Valid()
            {
                var result = FinanceConfigurationController.GetImmediatePaymentControl();
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void FinanceConfigurationController_GetImmediatePaymentControl_Exception()
            {
                configServiceMock.Setup(fc => fc.GetImmediatePaymentControl()).Throws(new Exception());
                FinanceConfigurationController = new FinanceConfigurationController(configServiceMock.Object, loggerMock.Object);

                var exc = FinanceConfigurationController.GetImmediatePaymentControl();
            }
        }
    }
}
