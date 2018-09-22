//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Finance
{
    [TestClass]
    public class AccountActivityControllerTests
    {
        [TestClass]
        public class GetAccountActivityPeriodsForStudentTests
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
            private Mock<IAccountActivityService> accountActivityServiceMock;
            private ApiSettings apiSettings;

            private DetailedAccountPeriod termDetailedAccountPeriodDto;
            private AccountActivityPeriods pcfAaPeriodsDto;
            private HttpResponse response;

            private AccountActivityController AccountActivityController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountActivityServiceMock = new Mock<IAccountActivityService>();
                apiSettings = new ApiSettings("TEST") { ReportLogoPath = "~/SomePath/image.png" };

                termDetailedAccountPeriodDto = new DetailedAccountPeriod()
                {
                    AmountDue = 1000m,
                    AssociatedPeriods = new List<string>(),
                    Balance = 1000m,
                    Charges = new ChargesCategory(),
                    Deposits = new DepositCategory(),
                    Description = "Term Details",
                    DueDate = DateTime.Today.AddDays(7),
                    FinancialAid = new FinancialAidCategory(),
                    Id = "2014/FA",
                    PaymentPlans = new PaymentPlanCategory(),
                    Refunds = new RefundCategory(),
                    Sponsorships = new SponsorshipCategory(),
                    StudentPayments = new StudentPaymentCategory()
                };

                pcfAaPeriodsDto = new AccountActivityPeriods()
                {
                    NonTermActivity = new AccountPeriod(),
                    Periods = new List<AccountPeriod>()
                    {
                        new AccountPeriod()
                        {
                            AssociatedPeriods = new List<string>() { "2014/FA" },
                            Balance = 1000m,
                            Description = "Current Period",
                            EndDate = DateTime.Today.AddDays(30),
                            Id = FinanceTimeframeCodes.CurrentPeriod,
                            StartDate = DateTime.Today.AddDays(-30)
                        }
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountActivityServiceMock.Setup(aa => aa.GetAccountActivityPeriodsForStudent("0001234")).Returns(pcfAaPeriodsDto);
                accountActivityServiceMock.Setup(aa => aa.GetAccountActivityPeriodsForStudent("0001235")).Throws(new PermissionsException());

                AccountActivityController = new AccountActivityController(accountActivityServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountActivityServiceMock = null;
                termDetailedAccountPeriodDto = null;
                AccountActivityController = null;
            }

            [TestMethod]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_Valid()
            {
                var aaDetails = AccountActivityController.GetAccountActivityPeriodsForStudent("0001234");
                Assert.IsNotNull(aaDetails);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_PermissionsException()
            {
                var aaDetails = AccountActivityController.GetAccountActivityPeriodsForStudent("0001235");
            }
        }

        [TestClass]
        public class GetAccountActivityByTermForStudentTests
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
            private Mock<IAccountActivityService> accountActivityServiceMock;
            private ApiSettings apiSettings;

            private DetailedAccountPeriod termDetailedAccountPeriodDto;
            private AccountActivityPeriods pcfAaPeriodsDto;
            private HttpResponse response;

            private AccountActivityController AccountActivityController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountActivityServiceMock = new Mock<IAccountActivityService>();
                apiSettings = new ApiSettings("TEST") { ReportLogoPath = "~/SomePath/image.png" };

                termDetailedAccountPeriodDto = new DetailedAccountPeriod()
                {
                    AmountDue = 1000m,
                    AssociatedPeriods = new List<string>(),
                    Balance = 1000m,
                    Charges = new ChargesCategory(),
                    Deposits = new DepositCategory(),
                    Description = "Term Details",
                    DueDate = DateTime.Today.AddDays(7),
                    FinancialAid = new FinancialAidCategory(),
                    Id = "2014/FA",
                    PaymentPlans = new PaymentPlanCategory(),
                    Refunds = new RefundCategory(),
                    Sponsorships = new SponsorshipCategory(),
                    StudentPayments = new StudentPaymentCategory()
                };

                pcfAaPeriodsDto = new AccountActivityPeriods()
                {
                    NonTermActivity = new AccountPeriod(),
                    Periods = new List<AccountPeriod>()
                    {
                        new AccountPeriod()
                        {
                            AssociatedPeriods = new List<string>() { "2014/FA" },
                            Balance = 1000m,
                            Description = "Current Period",
                            EndDate = DateTime.Today.AddDays(30),
                            Id = FinanceTimeframeCodes.CurrentPeriod,
                            StartDate = DateTime.Today.AddDays(-30)
                        }
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountActivityServiceMock.Setup(aa => aa.GetAccountActivityByTermForStudent("2014/FA", It.IsAny<string>())).Returns(termDetailedAccountPeriodDto);
                accountActivityServiceMock.Setup(aa => aa.GetAccountActivityByTermForStudent("2015/FA", It.IsAny<string>())).Throws(new PermissionsException());

                AccountActivityController = new AccountActivityController(accountActivityServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountActivityServiceMock = null;
                termDetailedAccountPeriodDto = null;
                AccountActivityController = null;
            }

            [TestMethod]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_Valid()
            {
                var aaDetails = AccountActivityController.GetAccountActivityByTermForStudent("2014/FA", "0001234");
                Assert.IsNotNull(aaDetails);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_PermissionsException()
            {
                var aaDetails = AccountActivityController.GetAccountActivityByTermForStudent("2015/FA", "0001234");
            }
        }

        [TestClass]
        public class GetAccountActivityByTermForStudent2Tests
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
            private Mock<IAccountActivityService> accountActivityServiceMock;
            private ApiSettings apiSettings;

            private DetailedAccountPeriod termDetailedAccountPeriodDto;
            private AccountActivityPeriods pcfAaPeriodsDto;
            private HttpResponse response;

            private AccountActivityController AccountActivityController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountActivityServiceMock = new Mock<IAccountActivityService>();
                apiSettings = new ApiSettings("TEST") { ReportLogoPath = "~/SomePath/image.png" };

                termDetailedAccountPeriodDto = new DetailedAccountPeriod()
                {
                    AmountDue = 1000m,
                    AssociatedPeriods = new List<string>(),
                    Balance = 1000m,
                    Charges = new ChargesCategory(),
                    Deposits = new DepositCategory(),
                    Description = "Term Details",
                    DueDate = DateTime.Today.AddDays(7),
                    FinancialAid = new FinancialAidCategory(),
                    Id = "2014/FA",
                    PaymentPlans = new PaymentPlanCategory(),
                    Refunds = new RefundCategory(),
                    Sponsorships = new SponsorshipCategory(),
                    StudentPayments = new StudentPaymentCategory()
                };

                pcfAaPeriodsDto = new AccountActivityPeriods()
                {
                    NonTermActivity = new AccountPeriod(),
                    Periods = new List<AccountPeriod>()
                    {
                        new AccountPeriod()
                        {
                            AssociatedPeriods = new List<string>() { "2014/FA" },
                            Balance = 1000m,
                            Description = "Current Period",
                            EndDate = DateTime.Today.AddDays(30),
                            Id = FinanceTimeframeCodes.CurrentPeriod,
                            StartDate = DateTime.Today.AddDays(-30)
                        }
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountActivityServiceMock.Setup(aa => aa.GetAccountActivityByTermForStudent2("2014/FA", It.IsAny<string>())).Returns(termDetailedAccountPeriodDto);
                accountActivityServiceMock.Setup(aa => aa.GetAccountActivityByTermForStudent2("2015/FA", It.IsAny<string>())).Throws(new PermissionsException());

                AccountActivityController = new AccountActivityController(accountActivityServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountActivityServiceMock = null;
                termDetailedAccountPeriodDto = null;
                AccountActivityController = null;
            }

            [TestMethod]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_Valid()
            {
                var aaDetails = AccountActivityController.GetAccountActivityByTermForStudent2("2014/FA", "0001234");
                Assert.IsNotNull(aaDetails);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_PermissionsException()
            {
                var aaDetails = AccountActivityController.GetAccountActivityByTermForStudent2("2015/FA", "0001234");
            }
        }

        [TestClass]
        public class PostAccountActivityByPeriodForStudentTests
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
            private Mock<IAccountActivityService> accountActivityServiceMock;
            private ApiSettings apiSettings;

            private DetailedAccountPeriod termDetailedAccountPeriodDto;
            private AccountActivityPeriods pcfAaPeriodsDto;
            private HttpResponse response;

            private AccountActivityController AccountActivityController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountActivityServiceMock = new Mock<IAccountActivityService>();
                apiSettings = new ApiSettings("TEST") { ReportLogoPath = "~/SomePath/image.png" };

                termDetailedAccountPeriodDto = new DetailedAccountPeriod()
                {
                    AmountDue = 1000m,
                    AssociatedPeriods = new List<string>(),
                    Balance = 1000m,
                    Charges = new ChargesCategory(),
                    Deposits = new DepositCategory(),
                    Description = "Term Details",
                    DueDate = DateTime.Today.AddDays(7),
                    FinancialAid = new FinancialAidCategory(),
                    Id = "2014/FA",
                    PaymentPlans = new PaymentPlanCategory(),
                    Refunds = new RefundCategory(),
                    Sponsorships = new SponsorshipCategory(),
                    StudentPayments = new StudentPaymentCategory()
                };

                pcfAaPeriodsDto = new AccountActivityPeriods()
                {
                    NonTermActivity = new AccountPeriod(),
                    Periods = new List<AccountPeriod>()
                    {
                        new AccountPeriod()
                        {
                            AssociatedPeriods = new List<string>() { "2014/FA" },
                            Balance = 1000m,
                            Description = "Current Period",
                            EndDate = DateTime.Today.AddDays(30),
                            Id = FinanceTimeframeCodes.CurrentPeriod,
                            StartDate = DateTime.Today.AddDays(-30)
                        }
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountActivityServiceMock.Setup(aa => aa.PostAccountActivityByPeriodForStudent(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), "0001234")).Returns(termDetailedAccountPeriodDto);
                accountActivityServiceMock.Setup(aa => aa.PostAccountActivityByPeriodForStudent(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), "0001235")).Throws(new PermissionsException());

                AccountActivityController = new AccountActivityController(accountActivityServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountActivityServiceMock = null;
                termDetailedAccountPeriodDto = null;
                AccountActivityController = null;
            }

            [TestMethod]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_Valid()
            {
                var aaDetails = AccountActivityController.PostAccountActivityByPeriodForStudent(new AccountActivityPeriodArguments()
                {
                    AssociatedPeriods = new List<string>() { "2014/FA" },
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = DateTime.Today.AddDays(30)
                }, "0001234");
                Assert.IsNotNull(aaDetails);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_PermissionsException()
            {
                var aaDetails = AccountActivityController.PostAccountActivityByPeriodForStudent(new AccountActivityPeriodArguments()
                {
                    AssociatedPeriods = new List<string>() { "2014/FA" },
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = DateTime.Today.AddDays(30)
                }, "0001235");
            }
        }

        [TestClass]
        public class PostAccountActivityByPeriodForStudent2Tests
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
            private Mock<IAccountActivityService> accountActivityServiceMock;
            private ApiSettings apiSettings;

            private DetailedAccountPeriod termDetailedAccountPeriodDto;
            private AccountActivityPeriods pcfAaPeriodsDto;
            private HttpResponse response;

            private AccountActivityController AccountActivityController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountActivityServiceMock = new Mock<IAccountActivityService>();
                apiSettings = new ApiSettings("TEST") { ReportLogoPath = "~/SomePath/image.png" };

                termDetailedAccountPeriodDto = new DetailedAccountPeriod()
                {
                    AmountDue = 1000m,
                    AssociatedPeriods = new List<string>(),
                    Balance = 1000m,
                    Charges = new ChargesCategory(),
                    Deposits = new DepositCategory(),
                    Description = "Term Details",
                    DueDate = DateTime.Today.AddDays(7),
                    FinancialAid = new FinancialAidCategory(),
                    Id = "2014/FA",
                    PaymentPlans = new PaymentPlanCategory(),
                    Refunds = new RefundCategory(),
                    Sponsorships = new SponsorshipCategory(),
                    StudentPayments = new StudentPaymentCategory()
                };

                pcfAaPeriodsDto = new AccountActivityPeriods()
                {
                    NonTermActivity = new AccountPeriod(),
                    Periods = new List<AccountPeriod>()
                    {
                        new AccountPeriod()
                        {
                            AssociatedPeriods = new List<string>() { "2014/FA" },
                            Balance = 1000m,
                            Description = "Current Period",
                            EndDate = DateTime.Today.AddDays(30),
                            Id = FinanceTimeframeCodes.CurrentPeriod,
                            StartDate = DateTime.Today.AddDays(-30)
                        }
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountActivityServiceMock.Setup(aa => aa.PostAccountActivityByPeriodForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), "0001234")).Returns(termDetailedAccountPeriodDto);
                accountActivityServiceMock.Setup(aa => aa.PostAccountActivityByPeriodForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), "0001235")).Throws(new PermissionsException());

                AccountActivityController = new AccountActivityController(accountActivityServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountActivityServiceMock = null;
                termDetailedAccountPeriodDto = null;
                AccountActivityController = null;
            }

            [TestMethod]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_Valid()
            {
                var aaDetails = AccountActivityController.PostAccountActivityByPeriodForStudent2(new AccountActivityPeriodArguments()
                {
                    AssociatedPeriods = new List<string>() { "2014/FA" },
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = DateTime.Today.AddDays(30)
                }, "0001234");
                Assert.IsNotNull(aaDetails);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AccountActivityController_GetAccountActivityPeriodsForStudent_PermissionsException()
            {
                var aaDetails = AccountActivityController.PostAccountActivityByPeriodForStudent2(new AccountActivityPeriodArguments()
                {
                    AssociatedPeriods = new List<string>() { "2014/FA" },
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = DateTime.Today.AddDays(30)
                }, "0001235");
            }
        }

        [TestClass]
        public class GetStudentAwardDisbursementInfoAsyncTests
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
            private Mock<IAccountActivityService> accountActivityServiceMock;                       

            private AccountActivityController accountActivityController;
            private StudentAwardDisbursementInfo expectedStudentAwardDisbursementInfoDto;
            private StudentAwardDisbursementInfo actualStudentAwardDisbursementInfoDto;
            private Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo studentAwardDisbursementInfoEntity;

            private string studentId;
            private string awardYearCode;
            private string awardId;


            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0004791";
                awardId = "GPLUS";
                awardYearCode = "2017";

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountActivityServiceMock = new Mock<IAccountActivityService>();
                

                var studentAwardDisbInfoAdapter = new StudentAwardDisbursementInfoEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                studentAwardDisbursementInfoEntity = new Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo(studentId, awardId, awardYearCode)
                {
                    AwardDescription = "GPLUS Award",
                    AwardDisbursements = new List<Domain.Finance.Entities.AccountActivity.StudentAwardDisbursement>()
                    {
                        new Domain.Finance.Entities.AccountActivity.StudentAwardDisbursement("16/FA", new DateTime(2016, 05, 06), 2356, DateTime.Today),
                        new Domain.Finance.Entities.AccountActivity.StudentAwardDisbursement("17/FA", new DateTime(2017, 05, 06), 134, new DateTime(2017, 04, 08))
                    }
                };
                expectedStudentAwardDisbursementInfoDto = studentAwardDisbInfoAdapter.MapToType(studentAwardDisbursementInfoEntity);

                accountActivityServiceMock.Setup(s => s.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(expectedStudentAwardDisbursementInfoDto);

                accountActivityController = new AccountActivityController(accountActivityServiceMock.Object, loggerMock.Object);
                actualStudentAwardDisbursementInfoDto = await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId);
            }

            [TestMethod]
            public void StudentAwardDisbursementInfoDto_EqualsExpectedTest()
            {
                Assert.AreEqual(expectedStudentAwardDisbursementInfoDto.StudentId, actualStudentAwardDisbursementInfoDto.StudentId);
                Assert.AreEqual(expectedStudentAwardDisbursementInfoDto.AwardCode, actualStudentAwardDisbursementInfoDto.AwardCode);
                Assert.AreEqual(expectedStudentAwardDisbursementInfoDto.AwardYearCode, actualStudentAwardDisbursementInfoDto.AwardYearCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullStudentId_HttpResponseExceptionIsThrownTest()
            {
                await accountActivityController.GetStudentAwardDisbursementInfoAsync(null, awardYearCode, awardId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullAwardYearCode_HttpResponseExceptionIsThrownTest()
            {
                await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, null, awardId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullAwardId_HttpResponseExceptionIsThrownTest()
            {
                await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ServiceThrowsArgumentNullException_HttpResponseExceptionIsThrown()
            {
                accountActivityServiceMock.Setup(s => s.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException());

                await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ServiceThrowsPermissionsException_HttpResponseExceptionIsThrown()
            {
                accountActivityServiceMock.Setup(s => s.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException());
                try
                {
                    await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ServiceThrowsApplicationException_HttpResponseExceptionIsThrown()
            {
                accountActivityServiceMock.Setup(s => s.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ApplicationException());
                try
                {
                    await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId);
                }
                catch
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ServiceThrowsKeyNotFoundException_HttpResponseExceptionIsThrown()
            {
                accountActivityServiceMock.Setup(s => s.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                try
                {
                    await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId);
                }
                catch(HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ServiceThrowsException_HttpResponseExceptionIsThrown()
            {
                accountActivityServiceMock.Setup(s => s.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());
                try
                {
                    await accountActivityController.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}
