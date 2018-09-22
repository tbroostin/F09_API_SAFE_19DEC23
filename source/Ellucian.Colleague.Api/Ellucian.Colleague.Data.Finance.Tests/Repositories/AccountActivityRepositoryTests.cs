// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class AccountActivityRepositoryTests : BaseRepositorySetup
    {
        private AccountActivityRepository repository;
        StudentFinancialActivityAdminResponse validResponse;
        StudentFinancialActivityAdminResponse emptyResponse;

        public TestAccountActivityRepository testAccountActivityRepository;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            MockInitialize();

            // Build the test repository
            repository = new AccountActivityRepository(cacheProvider, transFactory, logger);
        }

        [TestClass]
        public class AccountActivityRepository_GetAccountPeriods_Tests : AccountActivityRepositoryTests
        {
            [TestInitialize]
            public void AccountActivityRepository_GetAccountPeriods_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteAccountPeriodsAdminCTX();

                // Build the test repository
                repository = new AccountActivityRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountActivityRepository_GetAccountPeriods_NullCachedPeriods()
            {
                var result = repository.GetAccountPeriods("0001234");
                Assert.AreEqual(3, result.Count());
            }

            [TestMethod]
            public void AccountActivityRepository_GetAccountPeriods_CachedPeriods()
            {
                var result = repository.GetAccountPeriods("0001234");
                var result2 = repository.GetAccountPeriods("0001234");
                Assert.AreEqual(3, result2.Count());
            }
        }

        [TestClass]
        public class AccountActivityRepository_GetNonTermAccountPeriod_Tests : AccountActivityRepositoryTests
        {
            [TestInitialize]
            public void AccountActivityRepository_GetNonTermAccountPeriod_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteAccountPeriodsAdminCTX();

                // Build the test repository
                repository = new AccountActivityRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountActivityRepository_GetNonTermAccountPeriod_NonTermPeriod()
            {
                var result = repository.GetNonTermAccountPeriod("0001236");
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public void AccountActivityRepository_GetNonTermAccountPeriod_NoNonTermPeriod()
            {
                var result = repository.GetNonTermAccountPeriod("0001235");
                Assert.IsNull(result);
            }
        }

        [TestClass]
        public class AccountActivityRepository_GetTermActivityForStudent_Tests : AccountActivityRepositoryTests
        {
            [TestInitialize]
            public void AccountActivityRepository_GetTermActivityForStudent_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteActivityByTermAdminCTX();

                // Build the test repository
                repository = new AccountActivityRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountActivityRepository_GetTermActivityForStudent_NoActivity()
            {
                var result = repository.GetTermActivityForStudent("2014/FA", "0001235");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsFalse(result.Deposits.Deposits.Any());
                Assert.IsFalse(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsFalse(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsFalse(result.Refunds.Refunds.Any());
                Assert.IsFalse(result.Sponsorships.SponsorItems.Any());
                Assert.IsFalse(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetTermActivityForStudent_Activity()
            {
                var result = repository.GetTermActivityForStudent("2014/FA", "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestCleanup]
            public void AccountActivityRepository_GetTermActivityForStudent_Cleanup()
            {
                // Initialize person setup and Mock framework
                validResponse = null;
                emptyResponse = null;
            }
        }

        [TestClass]
        public class AccountActivityRepository_GetTermActivityForStudent2_Tests : AccountActivityRepositoryTests
        {
            [TestInitialize]
            public void AccountActivityRepository_GetTermActivityForStudent2_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteActivityByTermAdminCTX();

                // Build the test repository
                repository = new AccountActivityRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountActivityRepository_GetTermActivityForStudent2_NullStudentId()
            {
                var result = repository.GetTermActivityForStudent2("2014/FA", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountActivityRepository_GetTermActivityForStudent2_EmptyStudentId()
            {
                var result = repository.GetTermActivityForStudent2("2014/FA", string.Empty);
            }

            [TestMethod]
            public void AccountActivityRepository_GetTermActivityForStudent2_NoActivity()
            {
                var result = repository.GetTermActivityForStudent2("2014/FA", "0001235");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsFalse(result.Deposits.Deposits.Any());
                Assert.IsFalse(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsFalse(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsFalse(result.Refunds.Refunds.Any());
                Assert.IsFalse(result.Sponsorships.SponsorItems.Any());
                Assert.IsFalse(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetTermActivityForStudent2_Activity()
            {
                var result = repository.GetTermActivityForStudent2("2014/FA", "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestCleanup]
            public void AccountActivityRepository_GetTermActivityForStudent2_Cleanup()
            {
                // Initialize person setup and Mock framework
                validResponse = null;
                emptyResponse = null;
            }
        }

        [TestClass]
        public class AccountActivityRepository_GetPeriodActivityForStudent_Tests : AccountActivityRepositoryTests
        {
            [TestInitialize]
            public void AccountActivityRepository_GetPeriodActivityForStudent_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteActivityByPeriodAdminCTX();

                // Build the test repository
                repository = new AccountActivityRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent_NoActivity()
            {
                var result = repository.GetPeriodActivityForStudent(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    "0001235");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsFalse(result.Deposits.Deposits.Any());
                Assert.IsFalse(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsFalse(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsFalse(result.Refunds.Refunds.Any());
                Assert.IsFalse(result.Sponsorships.SponsorItems.Any());
                Assert.IsFalse(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent_PastActivity()
            {
                var result = repository.GetPeriodActivityForStudent(new List<string>() { "2014/FA" }, null, DateTime.Today.AddDays(30),
                    "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent_CurrentActivity()
            {
                var result = repository.GetPeriodActivityForStudent(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent_FutureActivity()
            {
                var result = repository.GetPeriodActivityForStudent(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), null,
                    "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestCleanup]
            public void AccountActivityRepository_GetPeriodActivityForStudent_Cleanup()
            {
                // Initialize person setup and Mock framework
                validResponse = null;
                emptyResponse = null;
            }
        }

        [TestClass]
        public class AccountActivityRepository_GetPeriodActivityForStudent2_Tests : AccountActivityRepositoryTests
        {
            [TestInitialize]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteActivityByPeriodAdminCTX();

                // Build the test repository
                repository = new AccountActivityRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_NullTermIds()
            {
                var result = repository.GetPeriodActivityForStudent2(null, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    "0001235");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_NoTermIds()
            {
                var result = repository.GetPeriodActivityForStudent2(new List<string>(), DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    "0001235");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_NullStudentId()
            {
                var result = repository.GetPeriodActivityForStudent2(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_EmptyStudentId()
            {
                var result = repository.GetPeriodActivityForStudent2(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    string.Empty);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_NoActivity()
            {
                var result = repository.GetPeriodActivityForStudent2(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    "0001235");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsFalse(result.Deposits.Deposits.Any());
                Assert.IsFalse(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsFalse(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsFalse(result.Refunds.Refunds.Any());
                Assert.IsFalse(result.Sponsorships.SponsorItems.Any());
                Assert.IsFalse(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_PastActivity()
            {
                var result = repository.GetPeriodActivityForStudent2(new List<string>() { "2014/FA" }, null, DateTime.Today.AddDays(30),
                    "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_CurrentActivity()
            {
                var result = repository.GetPeriodActivityForStudent2(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30),
                    "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestMethod]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_FutureActivity()
            {
                var result = repository.GetPeriodActivityForStudent2(new List<string>() { "2014/FA" }, DateTime.Today.AddDays(-30), null,
                    "0001234");
                Assert.AreEqual(0, result.AmountDue);
                Assert.IsNull(result.AssociatedPeriods);
                Assert.IsTrue(result.Deposits.Deposits.Any());
                Assert.IsTrue(result.FinancialAid.AnticipatedAid.Any());
                Assert.IsNull(result.FinancialAid.DisbursedAid);
                Assert.IsTrue(result.PaymentPlans.PaymentPlans.Any());
                Assert.IsNotNull(result.PaymentPlans.PaymentPlans.FirstOrDefault().PaymentPlanApproval);
                Assert.IsTrue(result.Refunds.Refunds.Any());
                Assert.IsTrue(result.Sponsorships.SponsorItems.Any());
                Assert.IsTrue(result.StudentPayments.StudentPayments.Any());
                Assert.IsNull(result.StartDate);
                Assert.IsNull(result.EndDate);
            }

            [TestCleanup]
            public void AccountActivityRepository_GetPeriodActivityForStudent2_Cleanup()
            {
                // Initialize person setup and Mock framework
                validResponse = null;
                emptyResponse = null;
            }
        }

        [TestClass]
        public class AccountActivityRepository_GetStudentAwardDisbursementInfoAsyncTests : AccountActivityRepositoryTests
        {
            public string studentId;
            public string awardId;
            public string awardYearCode;
            public TIVAwardCategory category;

            public Collection<SlAcyr> slAcyrContracts;
            public Collection<PellAcyr> pellAcyrContracts;
            public Collection<TcAcyr> tcAcyrContracts;
            public Collection<DateAward> dateAwardContracts;
            public Collection<DateAwardDisb> dateAwardDisbContracts;

            public StudentAwardDisbursementInfo actualEntity;


            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                testAccountActivityRepository = new TestAccountActivityRepository();
                studentId = "0004791";
                awardId = "PPLUS1";
                awardYearCode = "2017";
                category = TIVAwardCategory.Loan;

                slAcyrContracts = BuildSlAcyrRecords(testAccountActivityRepository.slAcyrs);
                pellAcyrContracts = BuildPellAcyrContracts(testAccountActivityRepository.pellAcyrs);
                tcAcyrContracts = BuildTcAcyrContracts(testAccountActivityRepository.tcAcyrs);
                dateAwardContracts = BuildDateAwardContracts(testAccountActivityRepository.dateAwards);
                dateAwardDisbContracts = BuildDateAwardDisbursementContracts(testAccountActivityRepository.dateAwardDisbursements);

                BuildActualRepository();
            }

            [TestMethod]
            public async Task StudentAwardDisbursementInfo_IsNotNullTest()
            {
                actualEntity = await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, category);
                Assert.IsNotNull(actualEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ThrowsArgumentNullExceptionTest()
            {
                await repository.GetStudentAwardDisbursementInfoAsync(null, awardYearCode, awardId, category);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardYearCode_ThrowsArgumentNullExceptionTest()
            {
                await repository.GetStudentAwardDisbursementInfoAsync(studentId, null, awardId, category);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardId_ThrowsArgumentNullExceptionTest()
            {
                await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, null, category);
            }

            [TestMethod]
            public async Task StudentAwardDisbursementInfoEntity_EqualsExpectedLoanTest()
            {
                var slAcyr = slAcyrContracts.First(c => c.Recordkey == (studentId+"*"+awardId));
                actualEntity = await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, category);
                Assert.AreEqual(studentId, actualEntity.StudentId);
                Assert.AreEqual(awardId, actualEntity.AwardCode);
                Assert.AreEqual(awardYearCode, actualEntity.AwardYearCode);
                for (int i = 0; i < slAcyr.SlLoanDisbEntityAssociation.Count; i++)
                {
                    Assert.AreEqual(slAcyr.SlLoanDisbEntityAssociation[i].SlAntDisbTermAssocMember, actualEntity.AwardDisbursements[i].AwardPeriodCode);
                    Assert.AreEqual(slAcyr.SlLoanDisbEntityAssociation[i].SlAntDisbDateAssocMember, actualEntity.AwardDisbursements[i].AnticipatedDisbursementDate);
                    Assert.AreEqual(slAcyr.SlLoanDisbEntityAssociation[i].SlActDisbAmtAssocMember, actualEntity.AwardDisbursements[i].LastTransmitAmount);
                    Assert.AreEqual(slAcyr.SlLoanDisbEntityAssociation[i].SlInitDisbDtAssocMember, actualEntity.AwardDisbursements[i].LastTransmitDate);
                }
            }

            [TestMethod]
            public async Task StudentAwardDisbursementInfoEntity_EqualsExpectedPellTest()
            {
                awardId = "PELL";
                var pellAcyr = pellAcyrContracts.First(c => c.Recordkey == (studentId + "*PELL"));
                actualEntity = await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, TIVAwardCategory.Pell);
                Assert.AreEqual(studentId, actualEntity.StudentId);
                Assert.AreEqual(awardId, actualEntity.AwardCode);
                Assert.AreEqual(awardYearCode, actualEntity.AwardYearCode);
                for (int i = 0; i < pellAcyr.PellDisbsEntityAssociation.Count; i++)
                {
                    Assert.AreEqual(pellAcyr.PellDisbsEntityAssociation[i].PellDisbAwardPeriodsAssocMember, actualEntity.AwardDisbursements[i].AwardPeriodCode);
                    Assert.AreEqual(pellAcyr.PellDisbsEntityAssociation[i].PellDisbDatesAssocMember, actualEntity.AwardDisbursements[i].AnticipatedDisbursementDate);
                    Assert.AreEqual(pellAcyr.PellDisbsEntityAssociation[i].PellActDisbAmountsAssocMember, actualEntity.AwardDisbursements[i].LastTransmitAmount);
                    Assert.AreEqual(pellAcyr.PellDisbsEntityAssociation[i].PellInitDisbDatesAssocMember, actualEntity.AwardDisbursements[i].LastTransmitDate);
                }
            }

            [TestMethod]
            public async Task StudentAwardDisbursementInfoEntity_EqualsExpectedTeachAwardTest()
            {
                awardId = "UGTCH";
                var dateAward = tcAcyrContracts.First(c => c.Recordkey == (studentId + "*UGTCH"));
                var dateAwardIds = dateAwardContracts.First(c => c.Recordkey == dateAward.TcDateAwardId).DawDateAwardDisbIds;
                var disbursements = dateAwardDisbContracts.Where(c => dateAwardIds.Contains(c.Recordkey)).ToList();
                actualEntity = await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, TIVAwardCategory.Teach);
                Assert.AreEqual(studentId, actualEntity.StudentId);
                Assert.AreEqual(awardId, actualEntity.AwardCode);
                Assert.AreEqual(awardYearCode, actualEntity.AwardYearCode);
                for (int i = 0; i < disbursements.Count; i++)
                {
                    Assert.AreEqual(disbursements[i].DawdAwardPeriod, actualEntity.AwardDisbursements[i].AwardPeriodCode);
                    Assert.AreEqual(disbursements[i].DawdDate, actualEntity.AwardDisbursements[i].AnticipatedDisbursementDate);
                    Assert.AreEqual(disbursements[i].DawdXmitAmount, actualEntity.AwardDisbursements[i].LastTransmitAmount);
                    Assert.AreEqual(disbursements[i].DawdInitialXmitDate, actualEntity.AwardDisbursements[i].LastTransmitDate);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullSlAcyrReturned_KeyNotFoundExceptionThrownTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<SlAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .ReturnsAsync(null);
                awardId = "PPLUS1";
                await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, TIVAwardCategory.Loan);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullTcAcyrReturned_KeyNotFoundExceptionThrownTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<TcAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .ReturnsAsync(null);
                awardId = "UGTCH";
                await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, TIVAwardCategory.Teach);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullPellAcyrReturned_KeyNotFoundExceptionThrownTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<PellAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .ReturnsAsync(null);
                awardId = "PELL";
                await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, TIVAwardCategory.Pell);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullDateAwardReturned_KeyNotFoundExceptionThrownTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DateAward>(It.IsAny<string>(), true))
                    .ReturnsAsync(null);
                awardId = "UGTCH";
                await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, TIVAwardCategory.Teach);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoDateAwardDisbursementsReturned_KeyNotFoundExceptionThrownTest()
            {
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DateAwardDisb>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(null);
                awardId = "UGTCH";
                await repository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, TIVAwardCategory.Teach);
            }

            private void BuildActualRepository()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<SlAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .Returns<string, string, bool>((file, criteria, b) =>
                {
                    var expectedSlAcyr = slAcyrContracts.First(c => c.Recordkey == criteria);
                    return Task.FromResult(expectedSlAcyr);
                });

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<PellAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .Returns<string, string, bool>((file, criteria, b) =>
                    {
                        var expectedPellAcyr = pellAcyrContracts.First(c => c.Recordkey == criteria);
                        return Task.FromResult(expectedPellAcyr);
                    });

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<TcAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .Returns<string, string, bool>((file, criteria, b) =>
                    {
                        var expectedTcAcyr = tcAcyrContracts.First(c => c.Recordkey == criteria);
                        return Task.FromResult(expectedTcAcyr);
                    });

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DateAward>(It.IsAny<string>(), true))
                    .Returns<string, bool>((id, b) =>
                    {
                        var expectedDateAward = dateAwardContracts.First(c => c.Recordkey == id);
                        return Task.FromResult(expectedDateAward);
                    });
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DateAwardDisb>(It.IsAny<string[]>(), true))
                    .Returns<string[], bool>((ids, b) =>
                    {
                        var expectedDateAwardDisb = dateAwardDisbContracts.Where(c => ids.Contains(c.Recordkey));
                        Collection<DateAwardDisb> disbursements = new Collection<DateAwardDisb>();
                        foreach(var contract in expectedDateAwardDisb)
                        {
                            disbursements.Add(contract);
                        }
                        return Task.FromResult(disbursements);
                    });

                repository = new AccountActivityRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            private Collection<DateAwardDisb> BuildDateAwardDisbursementContracts(List<TestAccountActivityRepository.dateAwardDisbRecord> dateAwardDisbursements)
            {
                Collection<DateAwardDisb> records = new Collection<DateAwardDisb>();
                foreach(var record in dateAwardDisbursements)
                {
                    records.Add(new DateAwardDisb()
                    {
                        Recordkey = record.id,
                        DawdAwardPeriod = record.dawdAwardPeriod,
                        DawdXmitAmount = record.dawdXmitAmount,
                        DawdInitialXmitDate = record.dawdInitialXmitDate,
                        DawdDate = record.dawdDate
                    });
                }
                return records;
            }

            private Collection<DateAward> BuildDateAwardContracts(List<TestAccountActivityRepository.dateAwardRecord> dateAwards)
            {
                Collection<DateAward> records = new Collection<DateAward>();
                foreach(var record in dateAwards)
                {
                    records.Add(new DateAward()
                    {
                        Recordkey = record.id,
                        DawDateAwardDisbIds = record.tcDateIds
                    });
                }
                return records;
            }

            private Collection<TcAcyr> BuildTcAcyrContracts(List<TestAccountActivityRepository.tcAcyrRecord> tcAcyrs)
            {
                Collection<TcAcyr> records = new Collection<TcAcyr>();
                foreach(var record in tcAcyrs)
                {
                    records.Add(new TcAcyr()
                    {
                        Recordkey = studentId + "*" + record.id,
                        TcDateAwardId = record.tcDateId
                    });
                }
                return records;
            }

            private Collection<SlAcyr> BuildSlAcyrRecords(List<TestAccountActivityRepository.slAcyrRecord> slAcyrs)
            {
                Collection<SlAcyr> records = new Collection<SlAcyr>();
                foreach (var record in slAcyrs)
                {
                    var slAcyr = new SlAcyr()
                    {
                        Recordkey = studentId + "*" + record.id,
                        SlActDisbAmt = record.disbursements.Select(d => d.SlActDisbAmt).ToList(),
                        SlAntDisbDate = record.disbursements.Select(d => d.SlAntDisbDate).ToList(),
                        SlInitDisbDt = record.disbursements.Select(d => d.SlInitDisbDt).ToList(),
                        SlAntDisbTerm = record.disbursements.Select(d => d.SlAntDisbTerm).ToList()
                    };
                    slAcyr.buildAssociations();
                    records.Add(slAcyr);
                }

                return records;
            }

            private Collection<PellAcyr> BuildPellAcyrContracts(List<TestAccountActivityRepository.pellAcyrRecord> pellAcyrs)
            {
                Collection<PellAcyr> records = new Collection<PellAcyr>();
                foreach(var record in pellAcyrs)
                {
                    var pellAcyr = new PellAcyr()
                    {
                        Recordkey = studentId + "*" + record.id,
                        PellDisbAwardPeriods = record.disbursements.Select(d => d.pellAntDisbTerm).ToList(),
                        PellActDisbAmounts = record.disbursements.Select(d => d.pellActDisbAmt).ToList(),
                        PellInitDisbDates = record.disbursements.Select(d => d.pellInitDisbDt).ToList(),
                        PellDisbDates = record.disbursements.Select(d => d.pellDisbDate).ToList()
                    };
                    pellAcyr.buildAssociations();
                    records.Add(pellAcyr);
                }
                return records;
            }

        }

        #region Private Data Definition setup

        private void SetupExecuteAccountPeriodsAdminCTX()
        {
            StudentFinActPeriodsAdminResponse validResponse = new StudentFinActPeriodsAdminResponse()
            {
                CurBeginDate = DateTime.Today.AddDays(-30),
                CurEndDate = DateTime.Today.AddDays(30),
                FinancialPeriods = new List<FinancialPeriods>()
                {
                    new FinancialPeriods() { PeriodBalances = 1000m, PeriodDescs = "Past", Periods = "PAST" },
                    new FinancialPeriods() { PeriodBalances = 2000m, PeriodDescs = "Current", Periods = "CUR" },
                    new FinancialPeriods() { PeriodBalances = 3000m, PeriodDescs = "Future", Periods = "FTR" },
                },
                NontermFlag = false,
                PeriodRelatedTerms = new List<PeriodRelatedTerms>()
                {
                    new PeriodRelatedTerms() { RelatedTermPeriods = "PAST", RelatedTerms = "2014 Fall Term" }
                },
                PersonName = "John Smith"
            };
            StudentFinActPeriodsAdminResponse nonTermResponse = new StudentFinActPeriodsAdminResponse()
            {
                CurBeginDate = DateTime.Today.AddDays(-30),
                CurEndDate = DateTime.Today.AddDays(30),
                FinancialPeriods = new List<FinancialPeriods>()
                {
                    new FinancialPeriods() { PeriodBalances = 1000m, PeriodDescs = "Non-Term", Periods = FinanceTimeframeCodes.NonTerm },
                },
                NontermFlag = true,
                PeriodRelatedTerms = new List<PeriodRelatedTerms>()
                {
                    new PeriodRelatedTerms() { RelatedTermPeriods = "NON-TERM", RelatedTerms = "Non-Term" }
                },
                PersonName = "John Smith"
            };
            StudentFinActPeriodsAdminResponse errorResponse = new StudentFinActPeriodsAdminResponse()
            {
                PersonName = "John Smith"
            };
            transManagerMock.Setup<StudentFinActPeriodsAdminResponse>(
                trans => trans.Execute<StudentFinActPeriodsAdminRequest, StudentFinActPeriodsAdminResponse>(It.IsAny<StudentFinActPeriodsAdminRequest>()))
                    .Returns<StudentFinActPeriodsAdminRequest>(request =>
                    {
                        if (request.PersonId == "0001234")
                        {
                            return validResponse;
                        }
                        else if (request.PersonId == "0001236")
                        {
                            return nonTermResponse;
                        }
                        return errorResponse;
                    });
        }

        private void SetupExecuteActivityByTermAdminCTX()
        {
            validResponse = new StudentFinancialActivityAdminResponse()
            {
                AllChargeGroups = new List<AllChargeGroups>()
                {
                    new AllChargeGroups() { AllChargeGroupNames = "Tuition by Total", AllChargeGroupOrders = 1, AllChargeGroupTypes = "TUIBT" },
                    new AllChargeGroups() { AllChargeGroupNames = "Tuition by Section", AllChargeGroupOrders = 2, AllChargeGroupTypes = "TUIBS" },
                    new AllChargeGroups() { AllChargeGroupNames = "Room and Board", AllChargeGroupOrders = 3, AllChargeGroupTypes = "RB" },
                    new AllChargeGroups() { AllChargeGroupNames = "Fees", AllChargeGroupOrders = 4, AllChargeGroupTypes = "FEES" },
                    new AllChargeGroups() { AllChargeGroupNames = "Other", AllChargeGroupOrders = null, AllChargeGroupTypes = "OTHER" },
                    new AllChargeGroups() { AllChargeGroupNames = "Miscellaneous", AllChargeGroupOrders = 99, AllChargeGroupTypes = "OTHER" }
                },
                AnticipatedAidTerms = new List<AnticipatedAidTerms>()
                {
                    new AnticipatedAidTerms() { FaTermsAntAmt = 1000m, FaTermsAwardTerm = "2014/FA", FaTermsDisbAmt = 1500m, FaTermsPeriodAward = "2014/FA"}
                },
                AnticipatedFinancialAid = new List<AnticipatedFinancialAid>()
                {
                    new AnticipatedFinancialAid() { FaAwardAmt = 1000m, FaAwardDesc = "Award", FaAwardIneligAmt = 0m, FaAwardLoanFee = 50m, FaAwardOtherAmt = 0m, FaAwardPeriodAward = "2014/FA" }
                },
                FinancialActivityAppliedDeposits = new List<FinancialActivityAppliedDeposits>()
                {
                    new FinancialActivityAppliedDeposits() { DepAmount = 100m, DepDate = DateTime.Today.AddDays(-30), DepRcptNo = "1234", DepTerm = "2014/FA", DepTypeDesc = "Meal Plan Deposit" },
                    new FinancialActivityAppliedDeposits() { DepAmount = null, DepDate = DateTime.Today.AddDays(-30), DepRcptNo = "1235", DepTerm = "2014/FA", DepTypeDesc = "Rooms Deposit" }
                },
                FinancialActivityChargeGroups = new List<FinancialActivityChargeGroups>()
                {
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "1", ChrgGroupName = "Tuition by Total", ChrgGroupType = "TUIBT",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "2", ChrgGroupName = "Tuition by Section", ChrgGroupType = "TUIBS",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = null, Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "3", ChrgGroupName = "Room and Board", ChrgGroupType = "RB",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "4", ChrgGroupName = "Fees", ChrgGroupType = "FEES",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgGroupName = "Other", ChrgGroupType = "OTHER",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "99", ChrgGroupName = "Miscellaneous", ChrgGroupType = "OTHER",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" }
                },
                FinancialActivityDeposits = new List<FinancialActivityDeposits>()
                {
                    new FinancialActivityDeposits() { DepositAmts = 100m, DepositAppliedAmts = 100m, DepositBalance = 0m, DepositDates = DateTime.Today.AddDays(-30),
                    DepositOtherAmts = 0m, DepositIds = "1234", DepositRefundAmts = 0m, DepositTerms = "2014 Fall Term", DepositTypeDescs = "Meal Plan Deposit" },
                    new FinancialActivityDeposits() { DepositAmts = null, DepositAppliedAmts = null, DepositBalance = null, DepositDates = DateTime.Today.AddDays(-30),
                    DepositOtherAmts = null, DepositIds = "1235", DepositRefundAmts = null, DepositTerms = "2014 Fall Term", DepositTypeDescs = "Rooms Deposit" }
 
                },
                FinancialActivityFAPayments = new List<FinancialActivityFAPayments>()
                {
                    new FinancialActivityFAPayments() { FaTransAmount = 1000m, FaTransDate = DateTime.Today.AddDays(-7), FaTransDesc = "Award", FaTransId = "1236", FaTransTerm = "2014 Fall Term" }
                },
                FinancialActivityRefunds = new List<FinancialActivityRefunds>()
                {
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "VISA", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1237", RefundCcLast4 = "1234", RefundStatus = "Paid", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1237" },
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "ECHK", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1238", RefundCheckDate = DateTime.Today.AddDays(-2), RefundCheckNo = "1234", RefundStatus = "NotApproved", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1238" },
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "VISA", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1239", RefundCcLast4 = "1234", RefundStatus = "Outstanding", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1239" },
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "VISA", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1240", RefundCcLast4 = "1234", RefundStatus = "Reconciled", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1240" },
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "VISA", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1241", RefundCcLast4 = "1234", RefundStatus = "InProgress", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1241" },
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "VISA", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1242", RefundCcLast4 = "1234", RefundStatus = "Voided", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1242" },
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "VISA", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1243", RefundCcLast4 = "1234", RefundStatus = "Cancelled", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1243" },
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "VISA", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1244", RefundCcLast4 = "1234", RefundStatus = "Unknown", RefundStatusDate = DateTime.Today.AddDays(-2), RefundTransNo = "TRANS1244" },

                },
                FinancialActivitySponsorPayments = new List<FinancialActivitySponsorPayments>()
                {
                    new FinancialActivitySponsorPayments() { SponPayAmount = 500m, SponPayDate = DateTime.Today.AddDays(-21), SponPayId = "1238", SponPaySponsor = "Sponsor", SponPayTerm = "2014 Fall Term" }
                },
                FinancialActivityStudentPayments = new List<FinancialActivityStudentPayments>()
                {
                    new FinancialActivityStudentPayments() { StuPayAmount = 100m, StuPayDescription = "Payment", StuPayMethod = "CC", StuPayRcptDate = DateTime.Today.AddDays(-2),
                    StuPayRcptNo = "1239", StuPayRefNo = "1239REF", StuPayTerm = "2014 Fall Term" }
                },
                PaymentPlans = new List<PaymentPlans>()
                {
                    new PaymentPlans() { PlanAmtDue = 150m, PlanBalance = 600m, PlanId = "1240", PlanOrigAmount = 600m, PlanTerm = "2014 Fall Term", PlanType = "Student Receivable", PlanApprovalId = "1",},
                    new PaymentPlans() { PlanAmtDue = null, PlanBalance = null, PlanId = "1241", PlanOrigAmount = null, PlanTerm = "2014 Fall Term", PlanType = "Student Receivable", PlanApprovalId = "2",} 
                },
                PlanSchedules = new List<PlanSchedules>()
                {
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(7), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(14), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(21), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(28), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = null, ScheduleAmtPaid = null, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(7), ScheduleLateCharge = null, ScheduleNetAmtDue = null, SchedulePlanId = "1241", ScheduleSetupCharge = null },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(14), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1241", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(21), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1241", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(28), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1241", ScheduleSetupCharge = 0m },             
             
                },
                PersonName = "John Smith"
            };
            emptyResponse = new StudentFinancialActivityAdminResponse() 
            { 
                PersonName = "John Smith"
            };
            transManagerMock.Setup<StudentFinancialActivityAdminResponse>(
                trans => trans.Execute<StudentFinancialActivityAdminRequest, StudentFinancialActivityAdminResponse>(It.IsAny<StudentFinancialActivityAdminRequest>()))
                    .Returns<StudentFinancialActivityAdminRequest>(request =>
                    {
                        if (request.PersonId == "0001234")
                        {
                            return validResponse;
                        }
                        return emptyResponse;
                    });
        }

        private void SetupExecuteActivityByPeriodAdminCTX()
        {
            validResponse = new StudentFinancialActivityAdminResponse()
            {
                AllChargeGroups = new List<AllChargeGroups>()
                {
                    new AllChargeGroups() { AllChargeGroupNames = "Tuition by Total", AllChargeGroupOrders = 1, AllChargeGroupTypes = "TUIBT" },
                    new AllChargeGroups() { AllChargeGroupNames = "Tuition by Section", AllChargeGroupOrders = 2, AllChargeGroupTypes = "TUIBS" },
                    new AllChargeGroups() { AllChargeGroupNames = "Room and Board", AllChargeGroupOrders = 3, AllChargeGroupTypes = "RB" },
                    new AllChargeGroups() { AllChargeGroupNames = "Fees", AllChargeGroupOrders = 4, AllChargeGroupTypes = "FEES" },
                    new AllChargeGroups() { AllChargeGroupNames = "Other", AllChargeGroupOrders = null, AllChargeGroupTypes = "OTHER" },
                    new AllChargeGroups() { AllChargeGroupNames = "Miscellaneous", AllChargeGroupOrders = 99, AllChargeGroupTypes = "OTHER" }
                },
                AnticipatedAidTerms = new List<AnticipatedAidTerms>()
                {
                    new AnticipatedAidTerms() { FaTermsAntAmt = 1000m, FaTermsAwardTerm = "2014/FA", FaTermsDisbAmt = 1500m, FaTermsPeriodAward = "2014/FA"}
                },
                AnticipatedFinancialAid = new List<AnticipatedFinancialAid>()
                {
                    new AnticipatedFinancialAid() { FaAwardAmt = 1000m, FaAwardDesc = "Award", FaAwardIneligAmt = 0m, FaAwardLoanFee = 50m, FaAwardOtherAmt = 0m, FaAwardPeriodAward = "2014/FA" }
                },
                FinancialActivityAppliedDeposits = new List<FinancialActivityAppliedDeposits>()
                {
                    new FinancialActivityAppliedDeposits() { DepAmount = 100m, DepDate = DateTime.Today.AddDays(-30), DepRcptNo = "1234", DepTerm = "2014/FA", DepTypeDesc = "Meal Plan Deposit" }
                },
                FinancialActivityChargeGroups = new List<FinancialActivityChargeGroups>()
                {
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "1", ChrgGroupName = "Tuition by Total", ChrgGroupType = "TUIBT",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "2", ChrgGroupName = "Tuition by Section", ChrgGroupType = "TUIBS",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "SUTTHS", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "3", ChrgGroupName = "Room and Board", ChrgGroupType = "RB",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "4", ChrgGroupName = "Fees", ChrgGroupType = "FEES",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgGroupName = "Other", ChrgGroupType = "OTHER",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" },
                    new FinancialActivityChargeGroups() { Amount = 2000m, BillingCredits = 3m, BuildingRoom = "BLDG", ChrgDisplayOrder = "99", ChrgGroupName = "Miscellaneous", ChrgGroupType = "OTHER",
                    Classroom = "ROOM", CourseTitle = "Title", Credits = 3m, Date = DateTime.Today.AddDays(14), Days = "MWF", Description = "Desc", Instructor = "Instructor", 
                    InvoiceNumber = "1235", Section = "01", Status = "Active", Term = "2014 Fall Term", Times = "9:00-10:15AM" }
                },
                FinancialActivityDeposits = new List<FinancialActivityDeposits>()
                {
                    new FinancialActivityDeposits() { DepositAmts = 100m, DepositAppliedAmts = 100m, DepositBalance = 0m, DepositDates = DateTime.Today.AddDays(-30),
                    DepositOtherAmts = 0m, DepositIds = "1234", DepositRefundAmts = 0m, DepositTerms = "2014 Fall Term", DepositTypeDescs = "Meal Plan Deposit" }
                },
                FinancialActivityFAPayments = new List<FinancialActivityFAPayments>()
                {
                    new FinancialActivityFAPayments() { FaTransAmount = 1000m, FaTransDate = DateTime.Today.AddDays(-7), FaTransDesc = "Award", FaTransId = "1236", FaTransTerm = "2014 Fall Term" }
                },
                FinancialActivityRefunds = new List<FinancialActivityRefunds>()
                {
                    new FinancialActivityRefunds() { RefundAmount = 200m, RefundDate = DateTime.Today.AddDays(-3), RefundDescription = "Refund", RefundPayMethod = "ECHK", 
                        RefundTerms = "2014 Fall Term", RefundVoucherId = "1237" }
                },
                FinancialActivitySponsorPayments = new List<FinancialActivitySponsorPayments>()
                {
                    new FinancialActivitySponsorPayments() { SponPayAmount = 500m, SponPayDate = DateTime.Today.AddDays(-21), SponPayId = "1238", SponPaySponsor = "Sponsor", SponPayTerm = "2014 Fall Term" }
                },
                FinancialActivityStudentPayments = new List<FinancialActivityStudentPayments>()
                {
                    new FinancialActivityStudentPayments() { StuPayAmount = 100m, StuPayDescription = "Payment", StuPayMethod = "CC", StuPayRcptDate = DateTime.Today.AddDays(-2),
                    StuPayRcptNo = "1239", StuPayRefNo = "1239REF", StuPayTerm = "2014 Fall Term" }
                },
                PaymentPlans = new List<PaymentPlans>()
                {
                    new PaymentPlans() { PlanAmtDue = 150m, PlanBalance = 600m, PlanId = "1240", PlanOrigAmount = 600m, PlanTerm = "2014 Fall Term", PlanType = "Student Receivable", PlanApprovalId = "1"} 
                },
                PlanSchedules = new List<PlanSchedules>()
                {
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(7), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(14), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(21), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },
                    new PlanSchedules() { ScheduleAmtDue = 150m, ScheduleAmtPaid = 0m, ScheduleDatePaid = null, ScheduleDueDate = DateTime.Today.AddDays(28), ScheduleLateCharge = 0m, ScheduleNetAmtDue = 150m, SchedulePlanId = "1240", ScheduleSetupCharge = 0m },             
                },
                PersonName = "John Smith"
            };
            emptyResponse = new StudentFinancialActivityAdminResponse()
            {
                PersonName = "John Smith"
            };
            transManagerMock.Setup<StudentFinancialActivityAdminResponse>(
                trans => trans.Execute<StudentFinancialActivityAdminRequest, StudentFinancialActivityAdminResponse>(It.IsAny<StudentFinancialActivityAdminRequest>()))
                    .Returns<StudentFinancialActivityAdminRequest>(request =>
                    {
                        if (request.PersonId == "0001234")
                        {
                            return validResponse;
                        }
                        return emptyResponse;
                    });
            dataReaderMock.Setup<Ellucian.Data.Colleague.DataContracts.ApplValcodes>(reader =>
                reader.ReadRecord<Ellucian.Data.Colleague.DataContracts.ApplValcodes>("ST.VALCODES", "DAYS.OF.WEEK", true)).
                Returns(new Ellucian.Data.Colleague.DataContracts.ApplValcodes()
                {
                    ValsEntityAssociation = new List<Ellucian.Data.Colleague.DataContracts.ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "M", ValExternalRepresentationAssocMember = "Monday", ValActionCode1AssocMember = "1" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "T", ValExternalRepresentationAssocMember = "Tuesday", ValActionCode1AssocMember = "2" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "W", ValExternalRepresentationAssocMember = "Wednesday", ValActionCode1AssocMember = "3" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "TH", ValExternalRepresentationAssocMember = "Thursday", ValActionCode1AssocMember = "4" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "F", ValExternalRepresentationAssocMember = "Friday", ValActionCode1AssocMember = "5" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "S", ValExternalRepresentationAssocMember = "Saturday", ValActionCode1AssocMember = "6" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "SU", ValExternalRepresentationAssocMember = "Sunday", ValActionCode1AssocMember = "0" },
                    }
                });

        }


        #endregion
    }
}
