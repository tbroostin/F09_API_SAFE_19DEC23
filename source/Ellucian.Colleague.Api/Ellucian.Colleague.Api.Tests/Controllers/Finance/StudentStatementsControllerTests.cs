//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Finance
{
    [TestClass]
    public class StudentStatementsControllerTests
    {
        [TestClass]
        public class GetStudentStatementTests
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
            private Mock<IStudentStatementService> studentStatementServiceMock;
            private ApiSettings apiSettings;

            private string accountHolderId;
            private string timeframeId;
            private DateTime? startDate;
            private DateTime? endDate;
            private StudentStatement expectedStatementDto;
            private HttpResponse response;

            private StudentStatementsController StudentStatementsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                studentStatementServiceMock = new Mock<IStudentStatementService>();
                apiSettings = new ApiSettings("TEST") { ReportLogoPath = "~/SomePath/image.png" };

                accountHolderId = "0000894";
                timeframeId = "2014/FA";
                startDate = null;
                endDate = null;
                expectedStatementDto = new StudentStatement()
                {
                    AccountDetails = new DetailedAccountPeriod()
                    {
                        AmountDue = 13500m,
                        AssociatedPeriods = new List<string>(),
                        Balance = 13500m,
                        Charges = new ChargesCategory()
                        {
                            FeeGroups = new List<FeeType>(),
                            Miscellaneous = new OtherType(),
                            OtherGroups = new List<OtherType>(),
                            RoomAndBoardGroups = new List<RoomAndBoardType>(),
                            TuitionBySectionGroups = new List<TuitionBySectionType>(),
                            TuitionByTotalGroups = new List<TuitionByTotalType>(),
                        },
                        Deposits = new DepositCategory() { Deposits = new List<ActivityRemainingAmountItem>() },
                        Description = "Description",
                        DueDate = DateTime.Today.AddDays(7),
                        EndDate = DateTime.Today.AddDays(30),
                        FinancialAid = new FinancialAidCategory() { AnticipatedAid = new List<ActivityFinancialAidItem>(), DisbursedAid = new List<ActivityDateTermItem>() },
                        Id = "Id",
                        PaymentPlans = new PaymentPlanCategory() { PaymentPlans = new List<ActivityPaymentPlanDetailsItem>() },
                        Refunds = new RefundCategory() { Refunds = new List<ActivityPaymentMethodItem>() },
                        Sponsorships = new SponsorshipCategory() { SponsorItems = new List<ActivitySponsorPaymentItem>() },
                        StartDate = DateTime.Today.AddDays(30),
                        StudentPayments = new StudentPaymentCategory() { StudentPayments = new List<ActivityPaymentPaidItem>() }
                    },
                    AccountSummary = new StudentStatementSummary()
                    {
                        ChargeInformation = new List<ActivityTermItem>(),
                        CurrentDepositsDueAmount = 0m,
                        NonChargeInformation = new List<ActivityTermItem>(),
                        PaymentPlanAdjustmentsAmount = 0m,
                        SummaryDateRange = "30 days prior to 30 days out",
                        TimeframeDescription = "2014 Fall Term"
                    },
                    ActivityDisplay = Dtos.Finance.Configuration.ActivityDisplay.DisplayByTerm,
                    CourseSchedule = new List<StudentStatementScheduleItem>(),
                    CurrentBalance = 10000m,
                    Date = DateTime.Today,
                    DepositsDue = new List<StudentStatementDepositDue>(),
                    DisclosureStatement = "Disclosure text",
                    DueDate = DateTime.Today.AddDays(7).ToShortDateString(),
                    FutureBalance = 1000m,
                    FutureBalanceDescription = "Balance after today",
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Ellucian University",
                    OtherBalance = 500m,
                    Overdue = false,
                    PreviousBalance = 2000m,
                    PreviousBalanceDescription = "Balance before today",
                    RemittanceAddress = "4375 Fair Lakes Court Fairfax, VA 22033",
                    StudentAddress = "123 Main Street Fairfax VA 22030",
                    StudentId = "0000894",
                    StudentName = "John Smith",
                    TimeframeId = "2014/FA",
                    Title = "Statement Title",
                    TotalAmountDue = 5000m,
                    TotalBalance = 13500m
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate)).
                    Returns(Task.FromResult(expectedStatementDto));

                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                studentStatementServiceMock = null;
                accountHolderId = null;
                timeframeId = null;
                startDate = null;
                endDate = null;
                expectedStatementDto = null;
                StudentStatementsController = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_NullAccountHolderId()
            {
                var statement = await StudentStatementsController.GetStudentStatementAsync(null, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_EmptyAccountHolderId()
            {
                var statement = await StudentStatementsController.GetStudentStatementAsync(string.Empty, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_NullTimeframeId()
            {
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, null, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_EmptyTimeframeId()
            {
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, string.Empty, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_NullStatement()
            {
                expectedStatementDto = null;
                studentStatementServiceMock.Setup( ss =>  ss.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate)).
                    Returns(Task.FromResult(expectedStatementDto));
                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_ArgumentException()
            {
                expectedStatementDto = null;
                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate)).
                    Throws(new ArgumentException());
                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_PermissionsException()
            {
                expectedStatementDto = null;
                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate)).
                    Throws(new PermissionsException());
                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_InvalidOperationException()
            {
                expectedStatementDto = null;
                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate)).
                    Throws(new InvalidOperationException());
                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_ApplicationException()
            {
                expectedStatementDto = null;
                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate)).
                    Throws(new ApplicationException());
                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatementsController_GetStudentStatement_GenericException()
            {
                expectedStatementDto = null;
                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate)).
                    Throws(new Exception());
                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            public async Task StudentStatementsController_GetStudentStatement_Valid_ImageWithRelativePath()
            {
                byte[] bytes = new byte[1000];
                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementReport(It.IsAny<StudentStatement>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>())).Returns(bytes);
                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementsController_GetStudentStatement_Valid_ImageWithoutRelativePath()
            {
                apiSettings = new ApiSettings("TEST") { ReportLogoPath = "/SomePath/image.png" };
                byte[] bytes = new byte[1000];
                studentStatementServiceMock.Setup(ss => ss.GetStudentStatementReport(It.IsAny<StudentStatement>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>())).Returns(bytes);

                StudentStatementsController = new StudentStatementsController(adapterRegistryMock.Object, studentStatementServiceMock.Object, loggerMock.Object, apiSettings);
                var statement = await StudentStatementsController.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

        }
    }
}
