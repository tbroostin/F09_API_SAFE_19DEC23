/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class AwardLetterRepository_GetAwardLettersTests : AwardLetterRepositoryTests
    {
        #region Declare Initialize and Cleanup

        //Test data
        private IEnumerable<AwardLetter> expectedAwardLetters;
        private IEnumerable<AwardLetter> actualAwardLetters;

        private IEnumerable<StudentAwardYear> studentAwardYears;
        private List<Fafsa> fafsaRecords;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            studentId = "0003914";

            officeRepository = new TestFinancialAidOfficeRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            fafsaRepository = new TestFafsaRepository();
            currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

            expectedRepository = new TestAwardLetterRepository();
            studentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
            fafsaRecords = new List<Fafsa>();
            foreach (var year in studentAwardYears)
            {
                fafsaRecords.AddRange(fafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, year.Code).Result);
            }

            expectedAwardLetters = expectedRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            csAcyrTestResponseData = BuildCsAcyrTestResponseData(expectedRepository.studentCsYearData);
            ysAcyrTestResponseData = BuildYsAcyrTestResponseData(expectedRepository.studentYsYearData);
            evalTransactionTestResponseData = BuildTransactionTestResponseData(expectedRepository.paramsTransactionData);
            faSysParamsResponseData = BuildSystemParametersResponseData(expectedRepository.systemParametersData);
            awardLetterParametersResponseData = BuildAwardLetterParametersResponseData(expectedRepository.awardLetterParameterData);

            actualRepository = BuildAwardLetterRepository();
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);
        }

        [TestCleanup]
        public void Cleanup()
        {
            cacheProviderMock = null;
            dataReaderMock = null;
            localCacheMock = null;
            loggerMock = null;
            transFactoryMock = null;
            transManagerMock = null;

            studentAwardYears = null;
            fafsaRecords = null;
            expectedRepository = null;
            expectedAwardLetters = null;
            csAcyrTestResponseData = null;
            evalTransactionTestResponseData = null;
            faSysParamsResponseData = null;
            awardLetterParametersResponseData = null;
            actualRepository = null;
            actualAwardLetters = null;
        }

        #endregion

        #region GetAllAwardLettersTests

        [TestMethod]
        public void NumberOfAwardLettersAreEqualTest()
        {
            Assert.IsTrue(expectedAwardLetters.Count() > 0);
            Assert.IsTrue(actualAwardLetters.Count() > 0);
            Assert.AreEqual(expectedAwardLetters.Count(), actualAwardLetters.Count());
        }

        [TestMethod]
        public void ActualAwardLettersAreEqualTest()
        {
            foreach (var actual in actualAwardLetters)
            {
                var expected = expectedAwardLetters.FirstOrDefault(e => e.Equals(actual));
                Assert.IsNotNull(expected);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStudentIdArgThrowsExceptionTest()
        {
            actualRepository.GetAwardLetters("", studentAwardYears, fafsaRecords);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAvailableAwardYearsArgThrowsExceptionTest()
        {
            actualRepository.GetAwardLetters(studentId, null, fafsaRecords);
        }

        [TestMethod]
        public void NoAvailableAwardYearsLogsInfoMessageTest()
        {
            studentAwardYears = new List<StudentAwardYear>();
            actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            loggerMock.Verify(l => l.Info(It.IsAny<string>()));
        }

        [TestMethod]
        public void NoAvailableAwardYearsReturnsEmptyListTest()
        {
            studentAwardYears = new List<StudentAwardYear>();
            var actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);
            Assert.AreEqual(0, actualAwardLetters.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFafsaRecordsArg_ThrowsExceptionTest()
        {
            actualRepository.GetAwardLetters(studentId, studentAwardYears, null);
        }

        [TestMethod]
        public void NoAvailableFafsaRecords_LogsInfoMessageTest()
        {
            fafsaRecords = new List<Fafsa>();
            actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            loggerMock.Verify(l => l.Info(It.IsAny<string>()));
        }        

        #endregion

        #region Helper Method Tests

        [TestMethod]
        public void EvalRuleTableTransactionMessagesAreLoggedTest()
        {
            foreach (var transactionTest in evalTransactionTestResponseData)
            {
                foreach (var message in transactionTest.LogMessages)
                {
                    loggerMock.Verify(logger => logger.Info(message));
                }
            }
        }

        [TestMethod]
        public void AwardLetterParametersDataRecordDoesNotExist_NoAwardLetterObjectTest()
        {
            var invalidParamsTransaction = evalTransactionTestResponseData.First();
            var invalidRecordId = "FOOBAR";
            var invalidYear = invalidParamsTransaction.AwardYear;

            invalidParamsTransaction.Result = invalidRecordId;

            actualRepository = BuildAwardLetterRepository();
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            var nullAwardLetter = actualAwardLetters.FirstOrDefault(a => a.AwardYear.Code == invalidYear);
            Assert.IsNull(nullAwardLetter);

            loggerMock.Verify(logger => logger.Error(string.Format("Award Letter Parameters record {0} does not exist. Verify Award Letter Rule Table for {1} is setup correctly.", invalidRecordId, invalidYear)));
            loggerMock.Verify(logger => logger.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
        }

        [TestMethod]
        public void NoNeedBlockDoesNotSetNeedBlockAttributesTest()
        {
            var noNeedAwardLetterData = awardLetterParametersResponseData.First(al => al.AltrNeedBlock.ToUpper() != "Y");
            var noNeedAwardLetterTransaction = evalTransactionTestResponseData.First(tt => tt.Result == noNeedAwardLetterData.Recordkey);
            var noNeedAwardLetterYear = noNeedAwardLetterTransaction.AwardYear;

            int expectedAmount = 0;

            var noNeedAwardLetterActual = actualAwardLetters.First(al => al.AwardYear.Code == noNeedAwardLetterYear);
            Assert.IsFalse(noNeedAwardLetterActual.IsNeedBlockActive);
            Assert.AreEqual(expectedAmount, noNeedAwardLetterActual.BudgetAmount);
            Assert.AreEqual(expectedAmount, noNeedAwardLetterActual.EstimatedFamilyContributionAmount);
            Assert.AreEqual(expectedAmount, noNeedAwardLetterActual.NeedAmount);
        }

        //[TestMethod]
        //The following test is no longer needed with the latest re-write Award Letters using the AwardLetterHistory.
        //We no longer use a CS.ACYR record for getting award letter data.
        //public void NullCsRecordLogsInfoMessage()
        //{
        //    var awardLetterWithNeedData = awardLetterParametersResponseData.First(al => al.AltrNeedBlock.ToUpper() == "Y");
        //    var awardLetterWithNeedYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithNeedData.Recordkey).AwardYear;

        //    var nullCsRecord = csAcyrTestResponseData.First(cs => cs.AwardYear == awardLetterWithNeedYear);

        //    var index = csAcyrTestResponseData.IndexOf(nullCsRecord);
        //    csAcyrTestResponseData[index] = null;

        //    actualRepository = BuildAwardLetterRepository();
        //    string acyrFile = "CS." + awardLetterWithNeedYear;
        //    CsAcyr nullCsAcyr = null;
        //    dataReaderMock.Setup<CsAcyr>(reader => reader.ReadRecord<CsAcyr>(acyrFile, studentId, true)).Returns(nullCsAcyr);
        //    actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

        //    loggerMock.Verify(logger => logger.Info(string.Format("Student {0} has no {1} record", studentId, acyrFile)));
        //}

        //[TestMethod]
        //The following test is no longer needed with the latest re-write Award Letters using the AwardLetterHistory.
        //We no longer use the YS.ACYR record for storing the Award Letter Accepted Date.
        //public void NullYsRecordLogsInfoMessage()
        //{
        //    var yearCodeWithNullYsAcyr = studentAwardYears.First().Code;

        //    var nullYsRecord = ysAcyrTestResponseData.First(ys => ys.AwardYear == yearCodeWithNullYsAcyr);
        //    var index = ysAcyrTestResponseData.IndexOf(nullYsRecord);
        //    ysAcyrTestResponseData[index] = null;

        //    actualRepository = BuildAwardLetterRepository();
        //    string acyrFile = "YS." + yearCodeWithNullYsAcyr;
        //    YsAcyr nullYsAcyr = null;
        //    dataReaderMock.Setup(reader => reader.ReadRecord<YsAcyr>(acyrFile, studentId, true)).Returns(nullYsAcyr);
        //    actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

        //    loggerMock.Verify(logger => logger.Info(string.Format("Student {0} has no {1} record", studentId, acyrFile)));
        //}

        //[TestMethod]
        //The following test is no longer needed with the latest re-write Award Letters using the AwardLetterHistory.
        //We no longer use the YS.ACYR record for storing the Award Letter Accepted Date.
        //public void NullYsRecordDoesNotSetAcceptedDate()
        //{
        //    var yearCodeWithNullYsAcyr = studentAwardYears.First().Code;

        //    var nullYsRecord = ysAcyrTestResponseData.First(ys => ys.AwardYear == yearCodeWithNullYsAcyr);
        //    var index = ysAcyrTestResponseData.IndexOf(nullYsRecord);
        //    ysAcyrTestResponseData[index] = null;

        //    actualRepository = BuildAwardLetterRepository();
        //    string acyrFile = "YS." + yearCodeWithNullYsAcyr;
        //    YsAcyr nullYsAcyr = null;
        //    dataReaderMock.Setup(reader => reader.ReadRecord<YsAcyr>(acyrFile, studentId, true)).Returns(nullYsAcyr);
        //    actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

        //    var nullAcceptedDateAwardLetter = actualAwardLetters.First(a => a.AwardYear.Code == yearCodeWithNullYsAcyr);
        //    Assert.IsFalse(nullAcceptedDateAwardLetter.AcceptedDate.HasValue);
        //}

        [TestMethod]
        public void NullCsRecordDoesNotSetNeedBlockAttributesTest()
        {
            var awardLetterWithNeedData = awardLetterParametersResponseData.First(al => al.AltrNeedBlock.ToUpper() == "Y");
            var awardLetterWithNeedYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithNeedData.Recordkey).AwardYear;

            var nullCsRecord = csAcyrTestResponseData.First(cs => cs.AwardYear == awardLetterWithNeedYear);
            var index = csAcyrTestResponseData.IndexOf(nullCsRecord);
            csAcyrTestResponseData[index] = null;

            actualRepository = BuildAwardLetterRepository();
            string acyrFile = "CS." + awardLetterWithNeedYear;
            CsAcyr nullCsAcyr = null;
            dataReaderMock.Setup<CsAcyr>(reader => reader.ReadRecord<CsAcyr>(acyrFile, studentId, true)).Returns(nullCsAcyr);
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            int expectedAmount = 0;

            var noNeedAwardLetterActual = actualAwardLetters.First(al => al.AwardYear.Code == awardLetterWithNeedYear);
            Assert.IsFalse(noNeedAwardLetterActual.IsNeedBlockActive);
            Assert.AreEqual(expectedAmount, noNeedAwardLetterActual.BudgetAmount);
            Assert.AreEqual(expectedAmount, noNeedAwardLetterActual.EstimatedFamilyContributionAmount);
            Assert.AreEqual(expectedAmount, noNeedAwardLetterActual.NeedAmount);
        }

        [TestMethod]
        public void NullBudgetAndNeedAmountsEqualZeroAmountsInEntityTest()
        {
            var zeroNeedAwardLetterData = awardLetterParametersResponseData.First(al => al.AltrNeedBlock.ToUpper() == "Y");
            var zeroNeedAwardLetterYear = evalTransactionTestResponseData.First(tt => tt.Result == zeroNeedAwardLetterData.Recordkey).AwardYear;

            var zeroNeedCsRecord = csAcyrTestResponseData.First(cs => cs.AwardYear == zeroNeedAwardLetterYear);
            zeroNeedCsRecord.CsBudgetAdj = null;
            zeroNeedCsRecord.CsInstAdj = null;
            zeroNeedCsRecord.CsNeed = null;
            zeroNeedCsRecord.CsStdTotalExpenses = null;

            actualRepository = BuildAwardLetterRepository();
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            int expectedAmount = 0;

            var zeroNeedAwardLetterActual = actualAwardLetters.First(al => al.AwardYear.Code == zeroNeedAwardLetterYear);
            Assert.IsTrue(zeroNeedAwardLetterActual.IsNeedBlockActive);
            Assert.AreEqual(expectedAmount, zeroNeedAwardLetterActual.BudgetAmount);
            Assert.AreEqual(expectedAmount, zeroNeedAwardLetterActual.NeedAmount);
        }

        [TestMethod]
        public void EmptyEfcAmountEqualsZeroEfcAmountInEntityTest()
        {
            var emptyEfcAwardLetterData = awardLetterParametersResponseData.First(al => al.AltrNeedBlock.ToUpper() == "Y");
            var emptyEfcAwardLetterYear = evalTransactionTestResponseData.First(tt => tt.Result == emptyEfcAwardLetterData.Recordkey).AwardYear;

            var emptyEfcCsRecord = csAcyrTestResponseData.First(cs => cs.AwardYear == emptyEfcAwardLetterYear);
            emptyEfcCsRecord.CsFc = string.Empty;

            actualRepository = BuildAwardLetterRepository();
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            int expectedAmount = 0 + emptyEfcCsRecord.CsInstAdj.Value;

            var emptyEfcAwardLetterActual = actualAwardLetters.First(al => al.AwardYear.Code == emptyEfcAwardLetterYear);
            Assert.IsTrue(emptyEfcAwardLetterActual.IsNeedBlockActive);
            Assert.AreEqual(expectedAmount, emptyEfcAwardLetterActual.EstimatedFamilyContributionAmount);
        }

        [TestMethod]
        public void NonNumericEfcValueEqualsZeroEfcAmountInEntityTest()
        {
            var corruptEfcAwardLetterData = awardLetterParametersResponseData.First(al => al.AltrNeedBlock.ToUpper() == "Y");
            var corruptEfcAwardLetterYear = evalTransactionTestResponseData.First(tt => tt.Result == corruptEfcAwardLetterData.Recordkey).AwardYear;

            var corruptEfcCsRecord = csAcyrTestResponseData.First(cs => cs.AwardYear == corruptEfcAwardLetterYear);
            corruptEfcCsRecord.CsFc = "FOOBAR";

            actualRepository = BuildAwardLetterRepository();
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            int expectedAmount = 0 + corruptEfcCsRecord.CsInstAdj.Value;

            var emptyEfcAwardLetterActual = actualAwardLetters.First(al => al.AwardYear.Code == corruptEfcAwardLetterYear);
            Assert.IsTrue(emptyEfcAwardLetterActual.IsNeedBlockActive);
            Assert.AreEqual(expectedAmount, emptyEfcAwardLetterActual.EstimatedFamilyContributionAmount);

            loggerMock.Verify(logger => logger.Debug(string.Format("CsFc has invalid value {0}. CS.{1} with key {2} may be corrupt", corruptEfcCsRecord.CsFc, corruptEfcAwardLetterYear, studentId)));
        }

        [TestMethod]
        public void NoOfficeBlockDoesNotSetContactBlockAttributesTest()
        {
            var noContactAwardLetterData = awardLetterParametersResponseData.First(al => al.AltrOfficeBlock.ToUpper() != "Y");
            var noContactAwardLetterTransaction = evalTransactionTestResponseData.First(tt => tt.Result == noContactAwardLetterData.Recordkey);
            var noContactAwardLetterYear = noContactAwardLetterTransaction.AwardYear;

            string expectedStringValue = string.Empty;

            var noContactAwardLetterActual = actualAwardLetters.First(al => al.AwardYear.Code == noContactAwardLetterYear);
            Assert.IsFalse(noContactAwardLetterActual.IsContactBlockActive);
            Assert.AreEqual(expectedStringValue, noContactAwardLetterActual.ContactName);
            Assert.AreEqual(expectedStringValue, noContactAwardLetterActual.ContactPhoneNumber);
            Assert.IsTrue(noContactAwardLetterActual.ContactAddress.Count() == 0);
        }

        [TestMethod]
        public void OfficeNameUsedIfItExistsTest()
        {
            var awardLetterWithContactData = awardLetterParametersResponseData.First(al => al.AltrOfficeBlock.ToUpper() == "Y");
            var awardLetterWithContactYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithContactData.Recordkey).AwardYear;

            var currentOffice = studentAwardYears.First(y => y.Code == awardLetterWithContactYear).CurrentOffice;

            Assert.IsFalse(string.IsNullOrEmpty(currentOffice.Name));

            var actualContactName = actualAwardLetters.First(al => al.AwardYear.Code == awardLetterWithContactYear).ContactName;

            Assert.AreEqual(currentOffice.Name, actualContactName);
        }

        [TestMethod]
        public void DefaultNameUsedIfOfficeNameDoesNotExistTest()
        {
            var awardLetterWithContactData = awardLetterParametersResponseData.First(al => al.AltrOfficeBlock.ToUpper() == "Y");
            var awardLetterWithContactYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithContactData.Recordkey).AwardYear;

            var currentOffice = studentAwardYears.First(y => y.Code == awardLetterWithContactYear).CurrentOffice;
            currentOffice.Name = string.Empty;
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            var expectedResult = faSysParamsResponseData.FspInstitutionName;
            var actualResult = actualAwardLetters.First(al => al.AwardYear.Code == awardLetterWithContactYear).ContactName;

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void OfficePhoneUsedIfItExistsTest()
        {
            var awardLetterWithContactData = awardLetterParametersResponseData.First(al => al.AltrOfficeBlock.ToUpper() == "Y");
            var awardLetterWithContactYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithContactData.Recordkey).AwardYear;

            var currentOffice = studentAwardYears.First(y => y.Code == awardLetterWithContactYear).CurrentOffice;

            Assert.IsFalse(string.IsNullOrEmpty(currentOffice.PhoneNumber));

            var actualContactPhone = actualAwardLetters.First(al => al.AwardYear.Code == awardLetterWithContactYear).ContactPhoneNumber;

            Assert.AreEqual(currentOffice.PhoneNumber, actualContactPhone);
        }

        [TestMethod]
        public void DefaultPhoneUsedIfOfficePhoneDoesNotExistTest()
        {
            var awardLetterWithContactData = awardLetterParametersResponseData.First(al => al.AltrOfficeBlock.ToUpper() == "Y");
            var awardLetterWithContactYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithContactData.Recordkey).AwardYear;

            var currentOffice = studentAwardYears.First(y => y.Code == awardLetterWithContactYear).CurrentOffice;
            currentOffice.PhoneNumber = string.Empty;
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            var expectedResult = faSysParamsResponseData.FspPellPhoneNumber;
            var actualResult = actualAwardLetters.First(al => al.AwardYear.Code == awardLetterWithContactYear).ContactPhoneNumber;

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void OfficeAddressLinesUsedIfTheyAllExist()
        {
            var awardLetterWithContactData = awardLetterParametersResponseData.First(al => al.AltrOfficeBlock.ToUpper() == "Y");
            var awardLetterWithContactYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithContactData.Recordkey).AwardYear;

            var currentOffice = studentAwardYears.First(y => y.Code == awardLetterWithContactYear).CurrentOffice;

            Assert.IsTrue(currentOffice.AddressLabel.Count() > 0);

            var actualAddress = actualAwardLetters.First(al => al.AwardYear.Code == awardLetterWithContactYear).ContactAddress;

            for (int i = 0; i < currentOffice.AddressLabel.Count(); i++)
            {
                Assert.AreEqual(currentOffice.AddressLabel[i], actualAddress[i]);
            }
        }

        [TestMethod]
        public void DefaultAddressUsedIfOfficeAddressDoesNotExistTest()
        {
            var awardLetterWithContactData = awardLetterParametersResponseData.First(al => al.AltrOfficeBlock.ToUpper() == "Y");
            var awardLetterWithContactYear = evalTransactionTestResponseData.First(tt => tt.Result == awardLetterWithContactData.Recordkey).AwardYear;

            var currentOffice = studentAwardYears.First(y => y.Code == awardLetterWithContactYear).CurrentOffice;
            currentOffice.AddressLabel = new List<string>();

            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            var expectedAddress = faSysParamsResponseData.FspInstitutionAddress;
            expectedAddress.Add(faSysParamsResponseData.FspInstitutionCsz);

            var actualAddress = actualAwardLetters.First(al => al.AwardYear.Code == awardLetterWithContactYear).ContactAddress;

            for (int i = 0; i < expectedAddress.Count(); i++)
            {
                Assert.AreEqual(expectedAddress[i], actualAddress[i]);
            }
        }

        /// <summary>
        /// Tests if AwardNameTitle in the actual letter equals the one in the expected letter
        /// </summary>
        [TestMethod]
        public void AwardNameTitleActualLetterEqualsAwardNameTitleExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                Assert.AreEqual(expectedAwardLetter.AwardNameTitle, actualAwardLetterForYear.AwardNameTitle);
            }
        }

        /// <summary>
        /// Tests if AwardTotalTitle in actual letter equals the one in the expected letter
        /// </summary>
        [TestMethod]
        public void AwardTotalTitleActualLetterEqualsAwardTotalTitleExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                Assert.AreEqual(expectedAwardLetter.AwardTotalTitle, actualAwardLetterForYear.AwardTotalTitle);
            }
        }

        /// <summary>
        /// Tests if the number of award categories groups in the actual letter (for all years) equals the
        /// number of award categories groups in the corresponding expected letter
        /// </summary>
        [TestMethod]
        public void NumberAwardCategoriesGroupsActualLetterEqualsNumberAwardCategoriesGroupsExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                Assert.AreEqual(expectedAwardLetter.AwardCategoriesGroups.Count, actualAwardLetterForYear.AwardCategoriesGroups.Count);
            }
        }

        /// <summary>
        /// Tests if for all of the award categories groups in the expected award letter there is a corresponding one in the actual 
        /// letter and their titles are equal
        /// </summary>
        [TestMethod]
        public void AwardCategoriesGroupTitlesActualLetterEqualAwardCategoriesGroupTitleExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                foreach (var expectedGroup in expectedAwardLetter.AwardCategoriesGroups)
                {
                    var actualGroup = actualAwardLetterForYear.AwardCategoriesGroups.FirstOrDefault(acg => acg.GroupType == expectedGroup.GroupType &&
                        acg.SequenceNumber == expectedGroup.SequenceNumber);
                    Assert.IsNotNull(actualGroup);

                    Assert.AreEqual(expectedGroup.Title, actualGroup.Title);
                }
            }
        }

        /// <summary>
        /// Tests if member count of each of the award categories groups in all of the actual award letters equals
        /// the member count in a corresponding award categories group in a corresponding expected award letter
        /// </summary>
        [TestMethod]
        public void AwardCategoriesGroupMembersCountActualLetterEqualsAwardCategoriesGroupMemberCountExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                foreach (var expectedGroup in expectedAwardLetter.AwardCategoriesGroups)
                {
                    var actualGroup = actualAwardLetterForYear.AwardCategoriesGroups.FirstOrDefault(acg => acg.GroupType == expectedGroup.GroupType &&
                        acg.SequenceNumber == expectedGroup.SequenceNumber);
                    Assert.IsNotNull(actualGroup);

                    Assert.AreEqual(expectedGroup.Members.Count, actualGroup.Members.Count);
                }
            }
        }

        /// <summary>
        /// Tests if members of each of the award categories groups in each actual letter equal the members in the corresponding 
        /// group in the corresponding expected award letter
        /// </summary>
        [TestMethod]
        public void AwardCategoriesGroupMembersActualLetterEqualAwardCategoriesGroupMembersExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                foreach (var expectedGroup in expectedAwardLetter.AwardCategoriesGroups)
                {
                    var actualGroup = actualAwardLetterForYear.AwardCategoriesGroups.FirstOrDefault(acg => acg.GroupType == expectedGroup.GroupType &&
                        acg.SequenceNumber == expectedGroup.SequenceNumber);
                    Assert.IsNotNull(actualGroup);

                    Assert.IsTrue(expectedGroup.Members.All(m => actualGroup.Members.Contains(m)));
                }
            }
        }

        /// <summary>
        /// Tests if the number of award period column groups in the actual letter (for all years) equals the
        /// number of award period column groups in the corresponding expected letter
        /// </summary>
        [TestMethod]
        public void NumberAwardPeriodColumnGroupsActualLetterEqualsNumberAwardPeriodColumnGroupsExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                Assert.AreEqual(expectedAwardLetter.AwardPeriodColumnGroups.Count, actualAwardLetterForYear.AwardPeriodColumnGroups.Count);
            }
        }

        /// <summary>
        /// Tests if for all of the award period column groups in the expected award letter there is a corresponding one in the actual 
        /// letter and their titles are equal
        /// </summary>
        [TestMethod]
        public void AwardPeriodColumnGroupTitlesActualLetterEqualAwardPeriodColumnGroupTitlesExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                foreach (var expectedGroup in expectedAwardLetter.AwardPeriodColumnGroups)
                {
                    var actualGroup = actualAwardLetterForYear.AwardPeriodColumnGroups.FirstOrDefault(acg => acg.GroupType == expectedGroup.GroupType &&
                        acg.SequenceNumber == expectedGroup.SequenceNumber);
                    Assert.IsNotNull(actualGroup);

                    Assert.AreEqual(expectedGroup.Title, actualGroup.Title);
                }
            }
        }

        /// <summary>
        /// Tests if member count of each of the award period column groups in all of the actual award letters equals
        /// the member count in a corresponding award period column group in a corresponding expected award letter
        /// </summary>
        [TestMethod]
        public void AwardPeriodColumnGroupMembersCountActualLetterEqualsAwardPeriodColumnGroupMemberCountExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                foreach (var expectedGroup in expectedAwardLetter.AwardPeriodColumnGroups)
                {
                    var actualGroup = actualAwardLetterForYear.AwardPeriodColumnGroups.FirstOrDefault(acg => acg.GroupType == expectedGroup.GroupType &&
                        acg.SequenceNumber == expectedGroup.SequenceNumber);
                    Assert.IsNotNull(actualGroup);

                    Assert.AreEqual(expectedGroup.Members.Count, actualGroup.Members.Count);
                }
            }
        }

        /// <summary>
        /// Tests if members of each of the award period column groups in each actual letter equal the members in the corresponding 
        /// group in the corresponding expected award letter
        /// </summary>
        [TestMethod]
        public void AwardPeriodColumnGroupMembersActualLetterEqualAwardPeriodColumnGroupMembersExpectedLetterTest_BuildAwardLetter()
        {
            foreach (var expectedAwardLetter in expectedAwardLetters)
            {
                var actualAwardLetterForYear = actualAwardLetters.FirstOrDefault(aal => aal.AwardYear == expectedAwardLetter.AwardYear);
                Assert.IsNotNull(actualAwardLetterForYear);

                foreach (var expectedGroup in expectedAwardLetter.AwardPeriodColumnGroups)
                {
                    var actualGroup = actualAwardLetterForYear.AwardPeriodColumnGroups.FirstOrDefault(acg => acg.GroupType == expectedGroup.GroupType &&
                        acg.SequenceNumber == expectedGroup.SequenceNumber);
                    Assert.IsNotNull(actualGroup);

                    Assert.IsTrue(expectedGroup.Members.All(m => actualGroup.Members.Contains(m)));
                }
            }
        }

        /// <summary>
        /// Tests if in the abscence of fafsa records the housing code gets set to null
        /// </summary>
        [TestMethod]
        public void NoAvailableFafsaRecords_ReturnsNullHousingCode()
        {
            fafsaRecords = new List<Fafsa>();
            var awardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);
            foreach (var awardLetter in awardLetters)
            {
                Assert.IsNull(awardLetter.HousingCode);
            }

        }

        [TestMethod]
        public void NoHousingCodeDisplayDoesNotSetHousingCodeAttributesTest()
        {
            var awardLetterWithHousingCodeData = awardLetterParametersResponseData.FirstOrDefault(al => al.AltrHousingCode.ToUpper() != "Y");
            var noHousingCodeAwardLetterTransaction = evalTransactionTestResponseData.FirstOrDefault(tt => tt.Result == awardLetterWithHousingCodeData.Recordkey);
            var noHousingCodeAwardLetterYear = noHousingCodeAwardLetterTransaction.AwardYear;
            
            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

            HousingCode? expectedCode = null;
            var noHousingCodeAwardLetterActual = actualAwardLetters.First(al => al.AwardYear.Code == noHousingCodeAwardLetterYear);
            Assert.IsFalse(noHousingCodeAwardLetterActual.IsHousingCodeActive);
            Assert.AreEqual(expectedCode, noHousingCodeAwardLetterActual.HousingCode);
            
        }

        [Ignore]
        [TestMethod]
        public void HousingCodeGetsSetCorrectlyTest()
        {
            var housingCodeAwardLetterData = awardLetterParametersResponseData.FirstOrDefault(al => al.AltrHousingCode.ToUpper() == "Y");
            var housingCodeAwardLetterYear = evalTransactionTestResponseData.FirstOrDefault(tt => tt.Result == housingCodeAwardLetterData.Recordkey).AwardYear;

            var housingCodeFafsaRecord = fafsaRecords.FirstOrDefault(fr => fr.AwardYear == housingCodeAwardLetterYear);
            var housingCodeYearOffice = studentAwardYears.FirstOrDefault(ay => ay.Code == housingCodeAwardLetterYear);
            
            HousingCode? expectedCode = null;
            housingCodeFafsaRecord.HousingCodes.TryGetValue(housingCodeYearOffice.CurrentOffice.TitleIVCode, out expectedCode);

            actualAwardLetters = actualRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);
            var actualAwardLetter = actualAwardLetters.FirstOrDefault(al => al.AwardYear.Code == housingCodeAwardLetterYear);

            Assert.AreEqual(expectedCode, actualAwardLetter.HousingCode);

        }

        #endregion






    }
}
