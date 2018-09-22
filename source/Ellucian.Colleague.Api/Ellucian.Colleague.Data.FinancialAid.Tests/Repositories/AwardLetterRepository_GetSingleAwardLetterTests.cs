/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class GetSingleAwardLetterTests : AwardLetterRepositoryTests
    {
        private AwardLetter expectedAwardLetter;
        private AwardLetter actualAwardLetter;

        //private StudentAwardYear awardYear;
        private StudentAwardYear studentAwardYear;
        private Fafsa fafsaRecord;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            studentId = "0003914";

            officeRepository = new TestFinancialAidOfficeRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

            expectedRepository = new TestAwardLetterRepository();
            studentAwardYear = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService).First();
            fafsaRecord = new Fafsa("5", studentId, studentAwardYear.Code);

            expectedAwardLetter = expectedRepository.GetAwardLetter(studentId, studentAwardYear, fafsaRecord);

            csAcyrTestResponseData = BuildCsAcyrTestResponseData(expectedRepository.studentCsYearData);
            ysAcyrTestResponseData = BuildYsAcyrTestResponseData(expectedRepository.studentYsYearData);
            evalTransactionTestResponseData = BuildTransactionTestResponseData(expectedRepository.paramsTransactionData);
            faSysParamsResponseData = BuildSystemParametersResponseData(expectedRepository.systemParametersData);
            awardLetterParametersResponseData = BuildAwardLetterParametersResponseData(expectedRepository.awardLetterParameterData);

            actualRepository = BuildAwardLetterRepository();
            actualAwardLetter = actualRepository.GetAwardLetter(studentId, studentAwardYear, fafsaRecord);
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

            studentAwardYear = null;
            expectedRepository = null;
            csAcyrTestResponseData = null;
            evalTransactionTestResponseData = null;
            faSysParamsResponseData = null;
            awardLetterParametersResponseData = null;
            actualRepository = null;
        }

        #region GetSingleAwardLetterTests

        [TestMethod]
        public void ExpectedAwardLetterEqualsActual()
        {
            Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentIdRequiredTest()
        {
            actualRepository.GetAwardLetter("", studentAwardYear, fafsaRecord);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentAwardYearRequiredTest()
        {
            actualRepository.GetAwardLetter(studentId, null, fafsaRecord);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void InvalidAwardLetterParametersId_ThrowsExceptionTest()
        {
            var invalidParamsTransaction = evalTransactionTestResponseData.First(t => t.AwardYear == studentAwardYear.Code);
            var invalidRecordId = "FOOBAR";

            invalidParamsTransaction.Result = invalidRecordId;

            actualRepository = BuildAwardLetterRepository();
            actualRepository.GetAwardLetter(studentId, studentAwardYear, fafsaRecord);
        }
        #endregion
    }

}
