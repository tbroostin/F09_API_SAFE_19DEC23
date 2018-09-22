//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
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
    public class AccountDueControllerTests
    {
        [TestClass]
        public class GetAccountDuePeriodTests
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
            private Mock<IAccountDueService> accountDueServiceMock;

            private AccountDue validAccountDueDto;
            private HttpResponse response;

            private AccountDueController AccountDueController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountDueServiceMock = new Mock<IAccountDueService>();

                validAccountDueDto = new AccountDue()
                {
                    AccountTerms = new List<AccountTerm>()
                    {
                        new AccountTerm()
                        {
                            Amount = 1000m,
                            DepositDueItems = new List<Dtos.Finance.DepositDue>()
                            {
                                new Dtos.Finance.DepositDue()
                                {
                                    Amount = 500m,
                                    AmountDue = 400m,
                                    AmountPaid = 100m,
                                    Balance = 400m,
                                    DepositType = "MEALS",
                                    DepositTypeDescription = "Meal Plan Deposit",
                                    Distribution = "BANK",
                                    DueDate = DateTime.Today.AddDays(7),
                                    Id = "123",
                                    Overdue = false,
                                    PersonId = "0001234",
                                    SortOrder = "1",
                                    TermDescription = "2014 Fall Term",
                                    TermId = "2014/FA"
                                }
                            },
                            Description = "2014 Fall Term",
                            GeneralItems = new List<Dtos.Finance.AccountDue.AccountsReceivableDueItem>()
                            {
                                new Dtos.Finance.AccountDue.AccountsReceivableDueItem()
                                {
                                    AccountDescription = "Student Receivables",
                                    AccountType = "01",
                                    AmountDue = 400m,
                                    Description = "Charge",
                                    Distribution = "BANK",
                                    DueDate = DateTime.Today.AddDays(7),
                                    Overdue = false,
                                    TermDescription = "2014 Fall Term",
                                    Term = "2014/FA"
                                }
                            },
                            InvoiceItems = new List<Dtos.Finance.AccountDue.InvoiceDueItem>()
                            {
                                new Dtos.Finance.AccountDue.InvoiceDueItem()
                                {
                                    AccountDescription = "Student Receivables",
                                    AccountType = "01",
                                    AmountDue = 400m,
                                    Description = "Charge",
                                    Distribution = "BANK",
                                    DueDate = DateTime.Today.AddDays(7),
                                    InvoiceId = "1234",
                                    Overdue = false,
                                    TermDescription = "2014 Fall Term",
                                    Term = "2014/FA"
                                }
                            },
                            PaymentPlanItems= new List<Dtos.Finance.AccountDue.PaymentPlanDueItem>()
                            {
                                new Dtos.Finance.AccountDue.PaymentPlanDueItem()
                                {
                                    AccountDescription = "Student Receivables",
                                    AccountType = "01",
                                    AmountDue = 400m,
                                    Description = "Charge",
                                    Distribution = "BANK",
                                    DueDate = DateTime.Today.AddDays(7),
                                    Overdue = false,
                                    PaymentPlanCurrent = true,
                                    PaymentPlanId  ="1234",
                                    TermDescription = "2014 Fall Term",
                                    Term = "2014/FA",
                                    UnpaidAmount = 400m
                                }
                            },
                            TermId = "2014/FA",
                        }
                    },
                    PersonName = "John Smith"
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountDueServiceMock.Setup(aa => aa.GetAccountDue("0001234")).Returns(validAccountDueDto);
                accountDueServiceMock.Setup(aa => aa.GetAccountDue("0001235")).Throws(new PermissionsException());

                AccountDueController = new AccountDueController(accountDueServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountDueServiceMock = null;
                validAccountDueDto = null;
                AccountDueController = null;
            }

            [TestMethod]
            public void AccountDueController_GetAccountDueForStudent_Valid()
            {
                var ad = AccountDueController.GetAccountDueForStudent("0001234");
                Assert.IsNotNull(ad);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AccountDueController_GetAccountDueForStudent_PermissionsException()
            {
                var ad = AccountDueController.GetAccountDueForStudent("0001235");
            }
        }

        [TestClass]
        public class GetAccountDuePeriodForStudentTests
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
            private Mock<IAccountDueService> accountDueServiceMock;

            private AccountDuePeriod validAccountDuePeriodDto;
            private HttpResponse response;

            private AccountDueController AccountDueController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountDueServiceMock = new Mock<IAccountDueService>();

                validAccountDuePeriodDto = new AccountDuePeriod()
                {

                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountDueServiceMock.Setup(aa => aa.GetAccountDuePeriod("0001234")).Returns(validAccountDuePeriodDto);
                accountDueServiceMock.Setup(aa => aa.GetAccountDuePeriod("0001235")).Throws(new PermissionsException());

                AccountDueController = new AccountDueController(accountDueServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountDueServiceMock = null;
                validAccountDuePeriodDto = null;
                AccountDueController = null;
            }

            [TestMethod]
            public void AccountDueController_GetAccountDuePeriodForStudent_Valid()
            {
                var ad = AccountDueController.GetAccountDuePeriodForStudent("0001234");
                Assert.IsNotNull(ad);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AccountDueController_GetAccountDuePeriodForStudent_PermissionsException()
            {
                var ad = AccountDueController.GetAccountDuePeriodForStudent("0001235");
            }
        }
    }
}
