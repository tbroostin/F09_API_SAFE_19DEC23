// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Finance.Services;
using Moq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Finance.Tests;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Coordination.Finance.Adapters;

using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class AccountActivityServiceTests : FinanceCoordinationTests
    {
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IAccountActivityRepository> aaRepoMock;
        private IAccountActivityRepository aaRepo;
        private Mock<IAccountsReceivableRepository> arRepoMock;
        private IAccountsReceivableRepository arRepo;
        private Mock<Domain.Finance.Repositories.IFinancialAidReferenceDataRepository> faRefRepoMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<ILogger> loggerMock;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private AccountActivityService service;
        private TestAccountActivityRepository testAccountActivityRepository;
        private TestFinancialAidReferenceDataRepository testFaRefRepository;

        private List<Domain.Finance.Entities.AccountActivity.AccountPeriod> accountPeriods;
        private Domain.Finance.Entities.AccountActivity.AccountPeriod nonTermAccountPeriod;
        private Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod detailedAccountPeriod;
        private List<Domain.Finance.Entities.DepositDue> depositsDue;
        private Domain.Finance.Entities.AccountHolder accountHolder;
        private Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo disbursementInfo;
        private List<FinancialAidAward> awards;

        private string awardId;
        private string awardYearCode;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            testFaRefRepository = new TestFinancialAidReferenceDataRepository();
            awards = testFaRefRepository.GetFinancialAidAwardsAsync().Result.ToList();
            awardId = awards.FirstOrDefault(a => a.AwardCategory.Code == "PLUS").Code;

            SetupData();
            SetupRepositories();
            SetupAdapters();

            userFactory = new FinanceCoordinationTests.StudentUserFactory();
            BuildService();
        }

        private void BuildService()
        {
            service = new AccountActivityService(adapterRegistry, aaRepo, arRepo, faRefRepoMock.Object, userFactory, roleRepo, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            roleRepoMock = null;
            roleRepo = null;
            aaRepoMock = null;
            aaRepo = null;
            arRepoMock = null;
            arRepo = null;
            userFactory = null;
            service = null;
        }

        [TestClass]
        public class AccountActivityService_GetAccountActivityPeriodsForStudent : AccountActivityServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetAccountActivityPeriodsForStudent_UnauthorizedUser()
            {
                var periods = service.GetAccountActivityPeriodsForStudent("0001234");
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityPeriodsForStudent_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(service.GetAccountActivityPeriodsForStudent("0001234"));
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityPeriodsForStudent_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(service.GetAccountActivityPeriodsForStudent("0003315"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetAccountActivityPeriodsForStudent_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.GetAccountActivityPeriodsForStudent("0003315");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityPeriodsForStudent_Valid()
            {
                var periods = service.GetAccountActivityPeriodsForStudent(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(periods);
            }
        }

        [TestClass]
        public class AccountActivityService_GetAccountActivityByTermForStudent : AccountActivityServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetAccountActivityByTermForStudent_UnauthorizedUser()
            {
                var detailedAccountPeriod = service.GetAccountActivityByTermForStudent("2014/FA", "0001234");
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityByTermForStudent_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(service.GetAccountActivityByTermForStudent("2014/FA", "0001234"));
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityByTermForStudent_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(service.GetAccountActivityByTermForStudent("2014/FA", "0003315"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetAccountActivityByTermForStudent_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.GetAccountActivityByTermForStudent("2014/FA", "0003315");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityByTermForStudent_Valid()
            {
                var detailedAccountPeriod = service.GetAccountActivityByTermForStudent("2014/FA", userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(detailedAccountPeriod);
            }
        }

        [TestClass]
        public class AccountActivityService_GetAccountActivityByTermForStudent2 : AccountActivityServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetAccountActivityByTermForStudent2_UnauthorizedUser()
            {
                var detailedAccountPeriod = service.GetAccountActivityByTermForStudent2("2014/FA", "0001234");
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityByTermForStudent2_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(service.GetAccountActivityByTermForStudent2("2014/FA", "0001234"));
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityByTermForStudent2_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(service.GetAccountActivityByTermForStudent2("2014/FA", "0003315"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetAccountActivityByTermForStudent2_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.GetAccountActivityByTermForStudent2("2014/FA", "0003315");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountActivityService_GetAccountActivityByTermForStudent2_Valid()
            {
                var detailedAccountPeriod = service.GetAccountActivityByTermForStudent2("2014/FA", userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(detailedAccountPeriod);
            }
        }

        [TestClass]
        public class AccountActivityService_PostAccountActivityByPeriodForStudent : AccountActivityServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent_UnauthorizedUser()
            {
                var detailedAccountPeriod = service.PostAccountActivityByPeriodForStudent(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0001234");
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(service.PostAccountActivityByPeriodForStudent(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0001234"));
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(service.PostAccountActivityByPeriodForStudent(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0003315"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.PostAccountActivityByPeriodForStudent(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent_Valid()
            {
                var detailedAccountPeriod = service.PostAccountActivityByPeriodForStudent(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(detailedAccountPeriod);
            }
        }

        [TestClass]
        public class AccountActivityService_PostAccountActivityByPeriodForStudent2 : AccountActivityServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent2_UnauthorizedUser()
            {
                var detailedAccountPeriod = service.PostAccountActivityByPeriodForStudent2(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0001234");
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent2_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(service.PostAccountActivityByPeriodForStudent2(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0001234"));
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent2_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(service.PostAccountActivityByPeriodForStudent2(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0003315"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent2_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.PostAccountActivityByPeriodForStudent2(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), "0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountActivityService_PostAccountActivityByPeriodForStudent2_Valid()
            {
                var detailedAccountPeriod = service.PostAccountActivityByPeriodForStudent2(new List<string>() { "PAST" }, null, DateTime.Today.AddDays(-30), userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(detailedAccountPeriod);
            }
        }

        [TestClass]
        public class AccountActivityService_GetDepositsDue : AccountActivityServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetDepositsDue_UnauthorizedUser()
            {
                var depositsDue = service.GetDepositsDue("0001234");
            }

            [TestMethod]
            public void AccountActivityService_GetDepositsDue_Valid()
            {
                var depositsDue = service.GetDepositsDue(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(depositsDue);
            }
        }

        [TestClass]
        public class AccountActivityService_GetAccountHolder : AccountActivityServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountActivityService_GetAccountHolder_UnauthorizedUser()
            {
                var accountHolder = service.GetAccountHolder("0001234");
            }

            [TestMethod]
            public void AccountActivityService_GetAccountHolder_Valid()
            {
                var accountHolder = service.GetAccountHolder(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountHolder);
            }
        }

        [TestClass]
        public class AccountActivityService_GetStudentAwardDisbursementInfoAsyncTests : AccountActivityServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ArgumentNullExceptionIsThrownTest()
            {
                await service.GetStudentAwardDisbursementInfoAsync(null, awardYearCode, awardId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardYearCode_ArgumentNullExceptionIsThrownTest()
            {
                await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, null, awardId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardId_ArgumentNullExceptionIsThrownTest()
            {
                await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, null);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UnauthorizedUser_PermissionsExceptionIsThrownTest()
            {
                try
                {
                    await service.GetStudentAwardDisbursementInfoAsync("0001234", awardYearCode, awardId);
                }
                catch
                {
                    loggerMock.Verify(l => l.Info(userFactory.CurrentUser + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UnknownAwardId_ApplicationExceptionThrownTest()
            {
                try
                {
                    await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, "foo");
                }
                catch
                {
                    loggerMock.Verify(l => l.Error("Cannot get disbursement info for the specified student award: foo - unknown award"));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullAwardCategory_ApplicationExceptionThrownTest()
            {
                var noCatAward = new FinancialAidAward("NoCatAward", "AwardDescr", null);
                awards.Add(noCatAward);
                try
                {
                    await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, noCatAward.Code);
                }
                catch
                {
                    loggerMock.Verify(l => l.Error("Cannot get disbursement info for the specified award: NoCatAward - award category is missing"));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UnknownAwardCategory_ApplicationExceptionThrownTest()
            {
                var unknownCatAward = new FinancialAidAward("UnknownCatAward", "AwardDescr", new FinancialAidAwardCategory("unknown", "unknown category"));
                awards.Add(unknownCatAward);
                try
                {
                    await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, unknownCatAward.Code);
                }
                catch
                {
                    loggerMock.Verify(l => l.Error("Could not assign a TIV category to the specified award: UnknownCatAward"));
                    throw;
                }
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ReturnedDto_EqualsExpectedTest()
            {
                var actualDto = await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, disbursementInfo.AwardCode);
                var corrAward = awards.FirstOrDefault(a => a.Code == disbursementInfo.AwardCode);
                Assert.AreEqual(disbursementInfo.AwardCode, actualDto.AwardCode);
                Assert.AreEqual(corrAward.Description, actualDto.AwardDescription);
                Assert.AreEqual(disbursementInfo.AwardYearCode, actualDto.AwardYearCode);
                for(int i = 0; i < actualDto.AwardDisbursements.Count(); i++)
                {
                    Assert.AreEqual(disbursementInfo.AwardDisbursements[i].AnticipatedDisbursementDate, actualDto.AwardDisbursements.ToList()[i].AnticipatedDisbursementDate);
                    Assert.AreEqual(disbursementInfo.AwardDisbursements[i].AwardPeriodCode, actualDto.AwardDisbursements.ToList()[i].AwardPeriodCode);
                    Assert.AreEqual(disbursementInfo.AwardDisbursements[i].LastTransmitAmount, actualDto.AwardDisbursements.ToList()[i].LastTransmitAmount);
                    Assert.AreEqual(disbursementInfo.AwardDisbursements[i].LastTransmitDate, actualDto.AwardDisbursements.ToList()[i].LastTransmitDate);
                }
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public async Task AccountActivityService_GetStudentAwardDisbursementInfoAsync_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(await service.GetStudentAwardDisbursementInfoAsync("0003315", awardYearCode, disbursementInfo.AwardCode));
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public async Task AccountActivityService_GetStudentAwardDisbursementInfoAsync_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(await service.GetStudentAwardDisbursementInfoAsync("0003315", awardYearCode, disbursementInfo.AwardCode));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountActivityService_GetStudentAwardDisbursementInfoAsync_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                await service.GetStudentAwardDisbursementInfoAsync("0003316", awardYearCode, disbursementInfo.AwardCode);
            }

            [TestMethod]
            public async Task GetStudentAwardDisbursementInfoAsync_RethrowsArgumentNullExceptionTest()
            {
                bool exceptionCaught = false;
                aaRepoMock.Setup(repo => repo.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TIVAwardCategory>()))
                    .Throws(new ArgumentNullException());
                service = new AccountActivityService(adapterRegistry, aaRepo, arRepoMock.Object, faRefRepoMock.Object, userFactory, roleRepo, loggerMock.Object);
                try
                {
                    await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, disbursementInfo.AwardCode);
                }
                catch
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task GetStudentAwardDisbursementInfoAsync_RethrowsKeyNotFoundExceptionTest()
            {
                bool exceptionCaught = false;
                aaRepoMock.Setup(repo => repo.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TIVAwardCategory>()))
                    .Throws(new KeyNotFoundException());
                service = new AccountActivityService(adapterRegistry, aaRepo, arRepoMock.Object, faRefRepoMock.Object, userFactory, roleRepo, loggerMock.Object);
                try
                {
                    await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, disbursementInfo.AwardCode);
                }
                catch
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task GetStudentAwardDisbursementInfoAsync_RethrowsGenericExceptionExceptionTest()
            {
                bool exceptionCaught = false;
                aaRepoMock.Setup(repo => repo.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TIVAwardCategory>()))
                    .Throws(new Exception());
                service = new AccountActivityService(adapterRegistry, aaRepo, arRepoMock.Object, faRefRepoMock.Object, userFactory, roleRepo, loggerMock.Object);
                try
                {
                    await service.GetStudentAwardDisbursementInfoAsync(userFactory.CurrentUser.PersonId, awardYearCode, disbursementInfo.AwardCode);
                }
                catch
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
            }
        }

        private void SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            
            var accountPeriodAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.AccountPeriod, Dtos.Finance.AccountActivity.AccountPeriod>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.AccountPeriod, Dtos.Finance.AccountActivity.AccountPeriod>()).Returns(accountPeriodAdapter);
            
            var detailedAccountPeriodEntityAdapter = new DetailedAccountPeriodEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Ellucian.Colleague.Dtos.Finance.AccountActivity.DetailedAccountPeriod>()).Returns(detailedAccountPeriodEntityAdapter);
            
            var activityDateTermItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDateTermItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDateTermItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDateTermItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDateTermItem>()).Returns(activityDateTermItemAdapter);

            var activityDepositItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDepositItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDepositItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDepositItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDepositItem>()).Returns(activityDepositItemAdapter);

            var activityFinancialAidTermAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidTerm>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidTerm>()).Returns(activityFinancialAidTermAdapter);

            var activityFinancialAidItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidItem>()).Returns(activityFinancialAidItemAdapter);

            var activityRemainingAmountItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRemainingAmountItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRemainingAmountItem>()).Returns(activityRemainingAmountItemAdapter);

            var activityPaymentMethodItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentMethodItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentMethodItem>()).Returns(activityPaymentMethodItemAdapter);

            var activityPaymentPaidItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPaidItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPaidItem>()).Returns(activityPaymentPaidItemAdapter);

            var activityPaymentPlanScheduleItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanScheduleItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanScheduleItem>()).Returns(activityPaymentPlanScheduleItemAdapter);

            var activityPaymentPlanDetailsItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanDetailsItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanDetailsItem>()).Returns(activityPaymentPlanDetailsItemAdapter);

            var activityRoomAndBoardItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRoomAndBoardItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRoomAndBoardItem>()).Returns(activityRoomAndBoardItemAdapter);

            var activitySponsorPaymentItem = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivitySponsorPaymentItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivitySponsorPaymentItem>()).Returns(activitySponsorPaymentItem);

            var activityTuitionItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityTuitionItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityTuitionItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityTuitionItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityTuitionItem>()).Returns(activityTuitionItemAdapter);

            var feeTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FeeType, Ellucian.Colleague.Dtos.Finance.AccountActivity.FeeType>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FeeType, Ellucian.Colleague.Dtos.Finance.AccountActivity.FeeType>()).Returns(feeTypeAdapter);

            var otherTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.OtherType, Ellucian.Colleague.Dtos.Finance.AccountActivity.OtherType>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.OtherType, Ellucian.Colleague.Dtos.Finance.AccountActivity.OtherType>()).Returns(otherTypeAdapter);

            var roomAndBoardTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RoomAndBoardType, Ellucian.Colleague.Dtos.Finance.AccountActivity.RoomAndBoardType>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RoomAndBoardType, Ellucian.Colleague.Dtos.Finance.AccountActivity.RoomAndBoardType>()).Returns(roomAndBoardTypeAdapter);

            var tuitionBySectionTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionBySectionType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionBySectionType>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionBySectionType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionBySectionType>()).Returns(tuitionBySectionTypeAdapter);

            var tuitionByTotalTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionByTotalType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionByTotalType>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionByTotalType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionByTotalType>()).Returns(tuitionByTotalTypeAdapter);

            var chargesCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ChargesCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.ChargesCategory>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ChargesCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.ChargesCategory>()).Returns(chargesCategoryAdapter);

            var depositCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DepositCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.DepositCategory>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DepositCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.DepositCategory>()).Returns(depositCategoryAdapter);

            var financialAidCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FinancialAidCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.FinancialAidCategory>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FinancialAidCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.FinancialAidCategory>()).Returns(financialAidCategoryAdapter);

            var paymentPlanCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.PaymentPlanCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.PaymentPlanCategory>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.PaymentPlanCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.PaymentPlanCategory>()).Returns(paymentPlanCategoryAdapter);

            var refundCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RefundCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.RefundCategory>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RefundCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.RefundCategory>()).Returns(refundCategoryAdapter);

            var sponsorshipCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.SponsorshipCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.SponsorshipCategory>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.SponsorshipCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.SponsorshipCategory>()).Returns(sponsorshipCategoryAdapter);

            var studentPaymentCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.StudentPaymentCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.StudentPaymentCategory>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.StudentPaymentCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.StudentPaymentCategory>()).Returns(studentPaymentCategoryAdapter);

            var depositDueAdapter = new DepositDueEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.DepositDue, Ellucian.Colleague.Dtos.Finance.DepositDue>()).Returns(depositDueAdapter);

            var depositAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Deposit, Ellucian.Colleague.Dtos.Finance.Deposit>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Deposit, Ellucian.Colleague.Dtos.Finance.Deposit>()).Returns(depositAdapter);

            var accountHolderAdapter = new AccountHolderEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountHolder, Ellucian.Colleague.Dtos.Finance.AccountHolder>()).Returns(accountHolderAdapter);

            var studentAwardDisbursementInfoEntityToDtoAdapter = new StudentAwardDisbursementInfoEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo, Dtos.Finance.AccountActivity.StudentAwardDisbursementInfo>())
                .Returns(studentAwardDisbursementInfoEntityToDtoAdapter);

        }

        private void SetupData()
        {
            accountPeriods = new List<Domain.Finance.Entities.AccountActivity.AccountPeriod>()
            {
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = new List<string>() { "2013/FA", "2014/SP", "2014/S1"},
                    Balance = 10000m,
                    Description = "Past",
                    Id = "PAST",
                    EndDate = DateTime.Today.AddDays(-30),
                    StartDate = null
                },
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = new List<string>() { "2014/FA", "2015/SP", "2015/S1"},
                    Balance = 5000m,
                    Description = "Current",
                    Id = "CUR",
                    EndDate = DateTime.Today.AddDays(30),
                    StartDate = DateTime.Today.AddDays(-29)
                },
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = new List<string>() { "2015/FA", "2016/SP", "2016/S1"},
                    Balance = 1000m,
                    Description = "Future",
                    Id = "FTR",
                    EndDate = null,
                    StartDate = DateTime.Today.AddDays(31)
                }
            };
            nonTermAccountPeriod = new Domain.Finance.Entities.AccountActivity.AccountPeriod()
            {
                AssociatedPeriods = null,
                Balance = 2500m,
                Description = "Non-Term",
                Id = "NONTERM",
                EndDate = null,
                StartDate = null
            };

            detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = new Domain.Finance.Entities.AccountActivity.ChargesCategory()
                {
                    FeeGroups = new List<Domain.Finance.Entities.AccountActivity.FeeType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.FeeType()
                        {
                            DisplayOrder = 1,
                            FeeCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 100m,
                                    Date = DateTime.Today.AddDays(1),
                                    Description = "Fee 1 Description 1",
                                    Id = "100",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 200m,
                                    Date = DateTime.Today.AddDays(2),
                                    Description = "Fee 1 Description 2",
                                    Id = "101",
                                    TermId = "2014/FA"
                                }
                            },
                            Name = "Fees 1"
                        },
                        new Domain.Finance.Entities.AccountActivity.FeeType()
                        {
                            DisplayOrder = 2,
                            FeeCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 300m,
                                    Date = DateTime.Today.AddDays(3),
                                    Description = "Fee 2 Description 1",
                                    Id = "102",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 400m,
                                    Date = DateTime.Today.AddDays(4),
                                    Description = "Fee 2 Description 2",
                                    Id = "103",
                                    TermId = "2014/FA"
                                }
                            },
                            Name = "Fees 2"
                        }
                    },
                    Miscellaneous = new Domain.Finance.Entities.AccountActivity.OtherType()
                    {
                        DisplayOrder = 3,
                        Name = "Miscellaneous",
                        OtherCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                        {
                            new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                            {
                                Amount = 125m,
                                Date = DateTime.Today.AddDays(5),
                                Description = "Misc Description 1",
                                Id = "104",
                                TermId = "2014/FA"
                            },
                            new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                            {
                                Amount = 875m,
                                Date = DateTime.Today.AddDays(6),
                                Description = "Misc Description 2",
                                Id = "105",
                                TermId = "2014/FA"
                            }                       
                        }
                    },
                    OtherGroups = new List<Domain.Finance.Entities.AccountActivity.OtherType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.OtherType()
                        {
                            DisplayOrder = 4,
                            Name = "Other Charges",
                            OtherCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 250m,
                                    Date = DateTime.Today.AddDays(7),
                                    Description = "Other Description 1",
                                    Id = "106",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 750m,
                                    Date = DateTime.Today.AddDays(8),
                                    Description = "Other Description 2",
                                    Id = "107",
                                    TermId = "2014/FA"
                                }                       
                            }
                        }
                    },
                    RoomAndBoardGroups = new List<Domain.Finance.Entities.AccountActivity.RoomAndBoardType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.RoomAndBoardType()
                        {
                            DisplayOrder = 5,
                            Name = "Room and Board 1",
                            RoomAndBoardCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 350m,
                                    Date = DateTime.Today.AddDays(9),
                                    Description = "Room and Board 1 Description 1",
                                    Id = "108",
                                    Room = "Room and Board 1 Room 1",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 650m,
                                    Date = DateTime.Today.AddDays(10),
                                    Description = "Room and Board 1 Description 2",
                                    Id = "109",
                                    Room = "Room and Board 1 Room 2",
                                    TermId = "2014/FA"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.RoomAndBoardType()
                        {
                            DisplayOrder = 6,
                            Name = "Room and Board 2",
                            RoomAndBoardCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 450m,
                                    Date = DateTime.Today.AddDays(11),
                                    Description = "Room and Board 2 Description 1",
                                    Id = "110",
                                    Room = "Room and Board 2 Room 1",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 550m,
                                    Date = DateTime.Today.AddDays(12),
                                    Description = "Room and Board 2 Description 2",
                                    Id = "111",
                                    Room = "Room and Board 2 Room 2",
                                    TermId = "2014/FA"
                                }
                            }
                        }
                    },
                    TuitionBySectionGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionBySectionType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.TuitionBySectionType()
                        {
                            DisplayOrder = 7,
                            Name = "Tuition by Section 1",
                            SectionCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 200m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 1",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "3:00 PM",
                                    Instructor = "Professor Jones",
                                    StartTime = "1:30 PM",
                                    Status = "Active"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 800m,
                                    BillingCredits = 4m,
                                    Ceus = 1.5m,
                                    Classroom = "Classroom 2",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "10:00 AM",
                                    Instructor = "Dr. Smith",
                                    StartTime = "2:30 PM",
                                    Status = "Pending"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.TuitionBySectionType()
                        {
                            DisplayOrder = 8,
                            Name = "Tuition by Section 2",
                            SectionCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 300m,
                                    BillingCredits = 4m,
                                    Ceus = null,
                                    Classroom = "Classroom 3",
                                    Credits = 4m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "9:00 AM",
                                    Instructor = "Professor Duncan",
                                    StartTime = "10:30 AM",
                                    Status = "Dropped"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 700m,
                                    BillingCredits = 2m,
                                    Ceus = 2m,
                                    Classroom = "Classroom 4",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "12:00 PM",
                                    Instructor = "Dr. Rigby",
                                    StartTime = "1:45 PM",
                                    Status = "Active"
                                }
                            }
                        }
                    },
                    TuitionByTotalGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionByTotalType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.TuitionByTotalType()
                        {
                            DisplayOrder = 7,
                            Name = "Tuition by Section 1",
                            TotalCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 130m,
                                    BillingCredits = 13m,
                                    Ceus = 1m,
                                    Classroom = "Classroom 5",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "3:00 PM",
                                    Instructor = "Professor Jones",
                                    StartTime = "1:30 PM",
                                    Status = "Active"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 8070m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 6",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "10:00 AM",
                                    Instructor = "Dr. Smith",
                                    StartTime = "2:30 PM",
                                    Status = "Pending"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.TuitionByTotalType()
                        {
                            DisplayOrder = 8,
                            Name = "Tuition by Section 2",
                            TotalCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 460m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 7",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "9:00 AM",
                                    Instructor = "Professor Duncan",
                                    StartTime = "10:30 AM",
                                    Status = "Dropped"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 540m,
                                    BillingCredits = 3m,
                                    Ceus = 3m,
                                    Classroom = "Classroom 8",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "12:00 PM",
                                    Instructor = "Dr. Rigby",
                                    StartTime = "1:45 PM",
                                    Status = "Active"
                                }
                            }
                        }
                    }
                },
                Deposits = new Domain.Finance.Entities.AccountActivity.DepositCategory()
                {
                    Deposits = new List<Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem()
                        {
                            Amount = 300m,
                            Date = DateTime.Today.AddDays(-3),
                            Description = "Deposit 1",
                            Id = "112",
                            OtherAmount = null,
                            PaidAmount = null,
                            RefundAmount = null,
                            RemainingAmount = 300m,
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem()
                        {
                            Amount = 400m,
                            Date = DateTime.Today.AddDays(-6),
                            Description = "Deposit 2",
                            Id = "113",
                            OtherAmount = null,
                            PaidAmount = 250m,
                            RefundAmount = null,
                            RemainingAmount = 150m,
                            TermId = "2014/FA"
                        }
                    }
                },
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = new Domain.Finance.Entities.AccountActivity.FinancialAidCategory()
                {
                    AnticipatedAid = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem()
                        {
                            AwardAmount = 1000m,
                            AwardDescription = "Anticipated Award 1",
                            AwardTerms = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 100m,
                                    AwardTerm = "2014/FA",
                                    DisbursedAmount = 0m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 900m,
                                    AwardTerm = "2014/WI",
                                    DisbursedAmount = 0m
                                }
                            },
                            Comments = "Anticipated Aid Comments 1",
                            IneligibleAmount = null,
                            LoanFee = 50m,
                            OtherTermAmount = null,
                            PeriodAward = "Anticipated Award 1"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem()
                        {
                            AwardAmount = 2000m,
                            AwardDescription = "Anticipated Award 2",
                            AwardTerms = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 300m,
                                    AwardTerm = "2014/FA",
                                    DisbursedAmount = 100m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 1050m,
                                    AwardTerm = "2014/WI",
                                    DisbursedAmount = 900m
                                }
                            },
                            Comments = "Anticipated Aid Comments 2",
                            IneligibleAmount = 500m,
                            LoanFee = null,
                            OtherTermAmount = 125m,
                            PeriodAward = "Anticipated Award 2"
                        }
                    },
                    DisbursedAid = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                        {
                            Amount = 450m,
                            Date = DateTime.Today.AddDays(13),
                            Description = "Disbursed Award 1",
                            Id = "114",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                        {
                            Amount = 550m,
                            Date = DateTime.Today.AddDays(13),
                            Description = "Disbursed Award 2",
                            Id = "115",
                            TermId = "2014/FA"
                        }
                    }
                },
                Id = "0003315",
                PaymentPlans = new Domain.Finance.Entities.AccountActivity.PaymentPlanCategory()
                {
                    PaymentPlans = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem()
                        {
                            Amount = 9000m,
                            CurrentBalance = 700m,
                            Description = "Payment Plan 1",
                            Id = "116",
                            OriginalAmount = 1500m,
                            PaymentPlanSchedules = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem()
                                {
                                    Amount = 600m,
                                    AmountPaid = 500m,
                                    Date = DateTime.Today.AddDays(-7),
                                    DatePaid = DateTime.Today.AddDays(-1),
                                    Description = "Plan 1 Scheduled Payment 1",
                                    Id = "117",
                                    LateCharge = 100m,
                                    NetAmountDue = 100m,
                                    SetupCharge = 200m,
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem()
                                {
                                    Amount = 400m,
                                    AmountPaid = null,
                                    Date = DateTime.Today,
                                    DatePaid = null,
                                    Description = "Plan 1 Scheduled Payment 2",
                                    Id = "118",
                                    LateCharge = null,
                                    NetAmountDue = 400m,
                                    SetupCharge = null,
                                    TermId = "2014/FA"
                                }
                            },
                            TermId = "2014/FA",
                            Type = "01"
                        },
                    }
                },
                Refunds = new Domain.Finance.Entities.AccountActivity.RefundCategory()
                {
                    Refunds = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem()
                        {
                            Amount = 333m,
                            Date = DateTime.Today.AddDays(-4),
                            Description = "Refund 1",
                            Id = "119",
                            Method = "CC",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem()
                        {
                            Amount = 664m,
                            Date = DateTime.Today.AddDays(-5),
                            Description = "Refund 2",
                            Id = "120",
                            Method = "ECHK",
                            TermId = "2014/FA"
                        }
                    }
                },
                Sponsorships = new Domain.Finance.Entities.AccountActivity.SponsorshipCategory()
                {
                    SponsorItems = new List<Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem()
                        {
                            Amount = 777m,
                            Date = DateTime.Today.AddDays(-6),
                            Description = "Sponsorship 1",
                            Id = "121",
                            Sponsorship = "SPON1",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem()
                        {
                            Amount = 223m,
                            Date = DateTime.Today.AddDays(-7),
                            Description = "Sponsorship 2",
                            Id = "122",
                            Sponsorship = "SPON2",
                            TermId = "2014/FA"
                        }
                    }
                },
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = new Domain.Finance.Entities.AccountActivity.StudentPaymentCategory()
                {
                    StudentPayments = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem()
                        {
                            Amount = 2000m,
                            Date = DateTime.Today.AddDays(-8),
                            Description = "Payment 1",
                            Id = "121",
                            Method = "ECHK",
                            ReferenceNumber = "12345",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem()
                        {
                            Amount = 1000m,
                            Date = DateTime.Today.AddDays(-9),
                            Description = "Payment 2",
                            Id = "122",
                            Method = "CC",
                            ReferenceNumber = "23456",
                            TermId = "2014/FA"
                        }
                    }
                }
            };

            depositsDue = new List<Domain.Finance.Entities.DepositDue>()
            {
                new Domain.Finance.Entities.DepositDue("123", "0000895", 500m, "MEALS", DateTime.Today.AddDays(7))
                {
                    TermId = "TermId"
                }
            };
            depositsDue[0].AddDeposit(new Domain.Finance.Entities.Deposit("123", "0000895", DateTime.Today.AddDays(-7), "MEALS", 300m) { TermId = "TermId" });

            accountHolder = new Domain.Finance.Entities.AccountHolder("0000895", "Smith", null)
            {
                BirthDate = DateTime.Today.AddYears(-18),
                DeceasedDate = null,
                EthnicCodes = new List<string>() { "NHS" },
                Ethnicities = new List<Domain.Base.Entities.EthnicOrigin>() { Domain.Base.Entities.EthnicOrigin.White },
                FirstName = "Firstname",
                Gender = "M",
                GovernmentId = "123-45-6789",
                Guid = "6c1091e2-4e54-4dbb-8b3c-6d186ae71d59",
                MaritalStatus = Domain.Base.Entities.MaritalState.Single,
                MaritalStatusCode = "S",
                MiddleName = "Middlename",
                Nickname = "Nickname",
                PreferredAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                PreferredName = "Nickname Lastname",
                Prefix = "Mr.",
                RaceCodes = new List<string>() { "WH" },
                Suffix = "III"
            };
            accountHolder.AddDepositDue(depositsDue[0]);
            accountHolder.AddEmailAddress(new Domain.Base.Entities.EmailAddress("firstname.lastname@ellucian.edu", "PRI") { IsPreferred = true });
            accountHolder.AddPersonAlt(new Domain.Base.Entities.PersonAlt("0001235", "ALT"));

            awardYearCode = "2017";           
        }

        private void SetupRepositories()
        {
            faRefRepoMock = new Mock<IFinancialAidReferenceDataRepository>();

            testAccountActivityRepository = new TestAccountActivityRepository();
            disbursementInfo = (testAccountActivityRepository.GetStudentAwardDisbursementInfoAsync("0000895", awardYearCode, awardId, TIVAwardCategory.Loan)).Result;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            aaRepoMock = new Mock<IAccountActivityRepository>();
            aaRepo = aaRepoMock.Object;
            aaRepoMock.Setup(repo => repo.GetAccountPeriods(It.IsAny<string>())).Returns(accountPeriods);
            aaRepoMock.Setup(repo => repo.GetNonTermAccountPeriod(It.IsAny<string>())).Returns(nonTermAccountPeriod);
            aaRepoMock.Setup(repo => repo.GetTermActivityForStudent(It.IsAny<string>(), It.IsAny<string>())).Returns(detailedAccountPeriod);
            aaRepoMock.Setup(repo => repo.GetTermActivityForStudent2(It.IsAny<string>(), It.IsAny<string>())).Returns(detailedAccountPeriod);
            aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent(It.IsAny<IEnumerable<string>>(), null, It.IsAny<DateTime?>(), It.IsAny<string>())).Returns(detailedAccountPeriod);
            aaRepoMock.Setup(repo => repo.GetPeriodActivityForStudent2(It.IsAny<IEnumerable<string>>(), null, It.IsAny<DateTime?>(), It.IsAny<string>())).Returns(detailedAccountPeriod);
            aaRepoMock.Setup(repo => repo.GetStudentAwardDisbursementInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TIVAwardCategory>()))
                .ReturnsAsync(disbursementInfo);

            arRepoMock = new Mock<IAccountsReceivableRepository>();
            arRepo = arRepoMock.Object;
            arRepoMock.Setup(repo => repo.GetDepositsDue(It.IsAny<string>())).Returns(depositsDue);
            arRepoMock.Setup(repo => repo.GetAccountHolder(It.IsAny<string>())).Returns(accountHolder);            

            faRefRepoMock.Setup(repo => repo.GetFinancialAidAwardsAsync()).ReturnsAsync(awards.AsEnumerable());

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;          

        }
    }
}
