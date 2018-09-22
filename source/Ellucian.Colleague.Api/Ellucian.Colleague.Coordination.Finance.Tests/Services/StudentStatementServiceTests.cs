// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Finance.Services;
using Moq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Finance;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Finance.Tests;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using System.IO;
using Microsoft.Reporting.WebForms;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class StudentStatementServiceTests : FinanceCoordinationTests
    {
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAccountActivityRepository> aaRepoMock;
        private IAccountActivityRepository aaRepo;
        private Mock<IAccountDueRepository> adRepoMock;
        private IAccountDueRepository adRepo;
        private Mock<IAccountsReceivableRepository> arRepoMock;
        private IAccountsReceivableRepository arRepo;
        private Mock<IFinanceConfigurationRepository> fcRepoMock;
        private IFinanceConfigurationRepository fcRepo;
        private Mock<ISectionRepository> secRepoMock;
        private ISectionRepository secRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IAcademicCreditRepository> acRepoMock;
        private IAcademicCreditRepository acRepo;
        private ICurrentUserFactory userFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ILogger logger;

        private string accountHolderId;
        private string timeframeId;
        private DateTime? startDate;
        private DateTime? endDate;
        private List<Domain.Finance.Entities.AccountHolder> allAccountHolders;
        private List<Domain.Finance.Entities.DepositType> allDepositTypes;
        private List<Domain.Student.Entities.Term> allTerms;
        private FinanceConfiguration financeConfiguration;
        private List<Domain.Finance.Entities.AccountActivity.AccountPeriod> pcfAccountPeriods;
        private List<Domain.Finance.Entities.AccountActivity.AccountPeriod> termAccountPeriods;
        private Domain.Finance.Entities.AccountActivity.AccountPeriod nonTermAccountPeriod;
        private List<Domain.Finance.Entities.FinancialPeriod> financialPeriods;
        private Ellucian.Colleague.Domain.Finance.Entities.DueDateOverrides dueDateOverrides;
        private Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod detailedAccountPeriod;
        private Domain.Finance.Entities.AccountDue.AccountDuePeriod accountDuePeriod;
        private List<Domain.Student.Entities.Student> allStudents;
        private Dictionary<string, List<Domain.Student.Entities.AcademicCredit>> allAcademicCredits;
        private List<Domain.Student.Entities.Section> allSections;
        private List<ReportParameter> allParameters;
        private StudentStatement statementDto;

        private StudentStatementService service;

        [TestInitialize]
        public async void Initialize()
        {
            await SetupAdapters();
            await SetupData();
            await SetupRepositories();

            BuildService();
        }

        private void BuildService()
        {
            service = new StudentStatementService(adapterRegistry, aaRepo, adRepo, arRepo, fcRepo, secRepo, termRepo,
                            acRepo, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            adapterRegistry = null;
            aaRepoMock = null;
            aaRepo = null;
            adRepoMock = null;
            adRepo = null;
            arRepoMock = null;
            arRepo = null;
            fcRepoMock = null;
            fcRepo = null;
            secRepoMock = null;
            secRepo = null;
            termRepoMock = null;
            termRepo = null;
            acRepoMock = null;
            acRepo = null;
            userFactory = null;
            roleRepoMock = null;
            roleRepo = null;
            logger = null;

            accountHolderId = null;
            timeframeId = null;
            startDate = null;
            endDate = null;
            allAccountHolders = null;
            allDepositTypes = null;
            allTerms = null;
            financeConfiguration = null;
            pcfAccountPeriods = null;
            termAccountPeriods = null;
            nonTermAccountPeriod = null;
            financialPeriods = null;
            detailedAccountPeriod = null;
            accountDuePeriod = null;
            allStudents = null;
            allAcademicCredits = null;
            allSections = null;
            allParameters = null;
            statementDto = null;

            service = null;
        }

        [TestClass]
        public class StudentStatementService_GetStudentStatement : StudentStatementServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentStatementService_GetStudentStatement_NullAccountHolderId()
            {
                accountHolderId = null;
                timeframeId = null;
                startDate = null;
                endDate = null;
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentStatementService_GetStudentStatement_EmptyAccountHolderId()
            {
                accountHolderId = string.Empty;
                timeframeId = null;
                startDate = null;
                endDate = null;
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentStatementService_GetStudentStatement_NullTimeframeId()
            {
                timeframeId = null;
                startDate = null;
                endDate = null;
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentStatementService_GetStudentStatement_EmptyTimeframeId()
            {
                timeframeId = string.Empty;
                startDate = null;
                endDate = null;
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentStatementService_GetStudentStatement_UnauthorizedUser()
            {
                var statement = await service.GetStudentStatementAsync("0001234", timeframeId, startDate, endDate);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task StudentStatementService_GetStudentStatementAsync_AdminCanAccessStatementTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate));
            }

            /// <summary>
            /// User is admin no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentStatementService_GetStudentStatementAsync_AdminNoPermissionsCannnotAccessStatementTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task StudentStatementService_GetStudentStatementAsync_ProxyCanAccessStatementTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                accountHolderId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                BuildService();
                Assert.IsNotNull(await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentStatementService_GetStudentStatementAsync_ProxyForDifferentPersonCannotAccessStatementTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                Assert.IsNotNull(await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate));
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_UnableToGenerateStatement()
            {
                termRepoMock.Setup(repo => repo.Get()).Returns(new List<Domain.Student.Entities.Term>());
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_Term()
            {
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_ReportingTerm()
            {
                timeframeId = "2014RFA";
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_NonTerm()
            {
                secRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));
                secRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));
                detailedAccountPeriod = TestDetailedAccountPeriodRepository.PartialDetailedAccountPeriod4(userFactory.CurrentUser.PersonId);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);

                timeframeId = FinanceTimeframeCodes.NonTerm;
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_NonTerm2()
            {
                detailedAccountPeriod = TestDetailedAccountPeriodRepository.PartialDetailedAccountPeriod5(userFactory.CurrentUser.PersonId);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);

                timeframeId = FinanceTimeframeCodes.NonTerm;
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_NonTerm3()
            {
                detailedAccountPeriod = TestDetailedAccountPeriodRepository.PartialDetailedAccountPeriod6(userFactory.CurrentUser.PersonId);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);

                timeframeId = FinanceTimeframeCodes.NonTerm;
                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_PastPeriod()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                financeConfiguration.IncludeSchedule = true;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                detailedAccountPeriod = TestDetailedAccountPeriodRepository.PartialDetailedAccountPeriod3(userFactory.CurrentUser.PersonId);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), null,
                    It.IsAny<DateTime?>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                timeframeId = FinanceTimeframeCodes.PastPeriod;
                endDate = DateTime.Today.AddDays(-31);

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentStatementService_GetStudentStatement_InvalidPastPeriod_NullEndDate()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                timeframeId = FinanceTimeframeCodes.PastPeriod;
                endDate = null;

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentStatementService_GetStudentStatement_InvalidPastPeriod_NonNullStartDate()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                timeframeId = FinanceTimeframeCodes.PastPeriod;
                startDate = DateTime.Today.AddDays(-60);

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_CurrentPeriod()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                financeConfiguration.IncludeSchedule = true;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                detailedAccountPeriod = TestDetailedAccountPeriodRepository.PartialDetailedAccountPeriod2(userFactory.CurrentUser.PersonId);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                timeframeId = FinanceTimeframeCodes.CurrentPeriod;
                startDate = DateTime.Today.AddDays(-30);
                endDate = DateTime.Today.AddDays(30);

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate); 
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentStatementService_GetStudentStatement_InvalidCurrentPeriod_NullStartDate()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                timeframeId = FinanceTimeframeCodes.CurrentPeriod;
                startDate = null;

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentStatementService_GetStudentStatement_InvalidCurrentPeriod_NullEndDate()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                timeframeId = FinanceTimeframeCodes.CurrentPeriod;
                startDate = DateTime.Today.AddDays(-60);
                endDate = null;

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_Valid_FuturePeriod()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                financeConfiguration.IncludeSchedule = true;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                detailedAccountPeriod = TestDetailedAccountPeriodRepository.PartialDetailedAccountPeriod1(userFactory.CurrentUser.PersonId);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(),
                    null, userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                secRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));
                timeframeId = FinanceTimeframeCodes.FuturePeriod;
                startDate = DateTime.Today.AddDays(31);

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate); 
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            public async Task StudentStatementService_GetStudentStatement_NoSchedule()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                detailedAccountPeriod = TestDetailedAccountPeriodRepository.PartialDetailedAccountPeriod1(userFactory.CurrentUser.PersonId);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(),
                    null, userFactory.CurrentUser.PersonId)).Returns(detailedAccountPeriod);
                secRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));
                timeframeId = FinanceTimeframeCodes.FuturePeriod;
                startDate = DateTime.Today.AddDays(31);

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentStatementService_GetStudentStatement_InvalidFuturePeriod_NullStartDate()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                timeframeId = FinanceTimeframeCodes.FuturePeriod;
                startDate = null;

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentStatementService_GetStudentStatement_InvalidPastPeriod_NonNullEndDate()
            {
                financeConfiguration = TestFinanceConfigurationRepository.PeriodFinanceConfiguration;
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
                timeframeId = FinanceTimeframeCodes.FuturePeriod;
                startDate = DateTime.Today.AddDays(31);
                endDate = DateTime.Today.AddDays(60);

                var statement = await service.GetStudentStatementAsync(accountHolderId, timeframeId, startDate, endDate);
                Assert.IsNotNull(statement);
            }
        }

        [TestClass]
        public class StudentStatementService_GetStudentStatementReport : StudentStatementServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementService_GetStudentStatementReport_NullStatement()
            {
                var report = service.GetStudentStatementReport(null, "report path", "resource path", "logo path");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementService_GetStudentStatementReport_NullReportPath()
            {
                var report = service.GetStudentStatementReport(new StudentStatement(), null, "resource path", "logo path");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementService_GetStudentStatementReport_EmptyReportPath()
            {
                var report = service.GetStudentStatementReport(new StudentStatement(), string.Empty, "resource path", "logo path");
            }

            [TestMethod]
            [ExpectedException(typeof(FileNotFoundException))]
            public void StudentStatementService_GetStudentStatementReport_InvalidResourcePath()
            {
                var report = service.GetStudentStatementReport(new StudentStatement(), "report path", "invalid", "logo path");
            }
        }

        private async Task SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            var statementAdapter = new StudentStatementEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.StudentStatement, StudentStatement>()).Returns(statementAdapter);

            var summaryAdapter = new StudentStatementSummaryEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.StudentStatementSummary, StudentStatementSummary>()).Returns(summaryAdapter);

            var dapAdapter = new DetailedAccountPeriodEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, DetailedAccountPeriod>()).Returns(dapAdapter);

            var siAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.StudentStatementScheduleItem, StudentStatementScheduleItem>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.StudentStatementScheduleItem, StudentStatementScheduleItem>()).Returns(siAdapter);

            var adAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.ActivityDisplay, ActivityDisplay>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.ActivityDisplay, ActivityDisplay>()).Returns(adAdapter);
        }

        private async Task SetupData()
        {
            userFactory = new FinanceCoordinationTests.StudentUserFactory();
            accountHolderId = userFactory.CurrentUser.PersonId;
            timeframeId = "2014/FA";
            startDate = null;
            endDate = null;
            allAccountHolders = TestAccountHolderRepository.AccountHolders;
            allDepositTypes = TestDepositTypesRepository.DepositTypes;
            allTerms = new TestTermRepository().Get().ToList();
            financeConfiguration = TestFinanceConfigurationRepository.TermFinanceConfiguration;
            pcfAccountPeriods = TestAccountPeriodRepository.PcfAccountPeriods;
            termAccountPeriods = TestAccountPeriodRepository.TermAccountPeriods;
            nonTermAccountPeriod = TestAccountPeriodRepository.NonTermAccountPeriod;
            financialPeriods = TestFinancialPeriodRepository.FinancialPeriods;
            dueDateOverrides = new Ellucian.Colleague.Domain.Finance.Entities.DueDateOverrides()
            {
                CurrentPeriodOverride = DateTime.Today.AddDays(30),
                FuturePeriodOverride = DateTime.Today.AddDays(90),
                NonTermOverride = DateTime.Today.AddDays(10),
                PastPeriodOverride = DateTime.Today.AddDays(-90),
                TermOverrides = new Dictionary<string, DateTime>()
                {
                    { "Term", DateTime.Today.AddDays(3) },
                }
            };
            detailedAccountPeriod = TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(userFactory.CurrentUser.PersonId);
            accountDuePeriod = TestAccountDuePeriodRepository.AccountDuePeriod(userFactory.CurrentUser.PersonId);
            allStudents =(await new TestStudentRepository().GetAllAsync()).ToList();
            allAcademicCredits = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
            foreach (var ah in allAccountHolders.Select(a => a.Id))
            {
                allAcademicCredits.Add(ah, (await new TestAcademicCreditRepository().GetAsync()).ToList());
            }
            allSections = (await new TestSectionRepository().GetAsync()).ToList();
            allParameters = new List<ReportParameter>() { new ReportParameter("Param1Name", "Param1Value") };
            statementDto = new StudentStatement()
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
                StudentId = accountHolderId,
                StudentName = userFactory.CurrentUser.FormattedName,
                TimeframeId = "2014/FA",
                Title = "Statement Title",
                TotalAmountDue = 5000m,
                TotalBalance = 13500m
            };
        }

        private async Task SetupRepositories()
        {
            aaRepoMock = new Mock<IAccountActivityRepository>();
            aaRepo = aaRepoMock.Object;

            adRepoMock = new Mock<IAccountDueRepository>();
            adRepo = adRepoMock.Object;

            arRepoMock = new Mock<IAccountsReceivableRepository>();
            arRepo = arRepoMock.Object;
            arRepoMock.Setup(repo => repo.DepositTypes).Returns(allDepositTypes);

            fcRepoMock = new Mock<IFinanceConfigurationRepository>();
            fcRepo = fcRepoMock.Object;
            fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);
            fcRepoMock.Setup(repo => repo.GetFinancialPeriods()).Returns(financialPeriods);
            fcRepoMock.Setup(repo => repo.GetDueDateOverrides()).Returns(dueDateOverrides);

            secRepoMock = new Mock<ISectionRepository>();
            secRepo = secRepoMock.Object;
            secRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>() { allSections.Where(s => s.CourseId == "7721" && s.TermId == "2014/FA").FirstOrDefault() }));
            secRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>() { allSections.Where(s => s.CourseId == "7721" && s.TermId == "2014/FA").FirstOrDefault() }));

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            termRepoMock.Setup(repo => repo.Get()).Returns(allTerms);
            foreach (var termId in allTerms.Select(t => t.Code))
            {
                termRepoMock.Setup(repo => repo.Get(termId)).Returns(allTerms.Where(t => t.Code == termId).FirstOrDefault());
            }

            acRepoMock = new Mock<IAcademicCreditRepository>();
            acRepo = acRepoMock.Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            foreach (var personId in allAccountHolders.Select(ah => ah.Id))
            {
                aaRepoMock.Setup(repo => repo.GetAccountPeriods(personId)).
                    Returns(termAccountPeriods);
                aaRepoMock.Setup(repo => repo.GetNonTermAccountPeriod(personId)).
                    Returns(nonTermAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), personId)).Returns(detailedAccountPeriod);
                aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(), personId)).Returns(detailedAccountPeriod);
                adRepoMock.Setup(repo => repo.GetPeriods(personId)).
                    Returns(accountDuePeriod);
                adRepoMock.Setup(repo => repo.Get(personId)).
                    Returns(accountDuePeriod.Current);
                arRepoMock.Setup(repo => repo.GetAccountHolder(personId)).
                    Returns(allAccountHolders.Where(ah => ah.Id == personId).FirstOrDefault());
                acRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<ICollection<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(allAcademicCredits));
            }
        }
    }
}
