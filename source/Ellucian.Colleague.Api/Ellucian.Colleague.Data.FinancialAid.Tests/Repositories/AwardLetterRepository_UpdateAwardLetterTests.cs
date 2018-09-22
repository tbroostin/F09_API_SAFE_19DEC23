/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class UpdateAwardLetterTests : AwardLetterRepositoryTests
    {
        private AwardLetter expectedUpdatedAwardLetter;
        private AwardLetter actualUpdatedAwardLetter;

        private AwardLetter inputAwardLetter;

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

            inputAwardLetter = new AwardLetter(studentId, studentAwardYear) { AcceptedDate = null };

            expectedUpdatedAwardLetter = expectedRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);

            csAcyrTestResponseData = BuildCsAcyrTestResponseData(expectedRepository.studentCsYearData);
            ysAcyrTestResponseData = BuildYsAcyrTestResponseData(expectedRepository.studentYsYearData);
            evalTransactionTestResponseData = BuildTransactionTestResponseData(expectedRepository.paramsTransactionData);
            faSysParamsResponseData = BuildSystemParametersResponseData(expectedRepository.systemParametersData);
            awardLetterParametersResponseData = BuildAwardLetterParametersResponseData(expectedRepository.awardLetterParameterData);

            updateAwardLetterResponseData = new UpdateAwardLetterResponse();

            actualRepository = BuildAwardLetterRepository();
            actualUpdatedAwardLetter = actualRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);
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

        [TestMethod]
        public void UpdateAwardLetterWithNullAcceptedDateTest()
        {
            Assert.AreEqual(expectedUpdatedAwardLetter, actualUpdatedAwardLetter);
            Assert.AreEqual(expectedUpdatedAwardLetter.AcceptedDate, actualUpdatedAwardLetter.AcceptedDate);
        }

        [TestMethod]
        public void UpdateAwardLetterWithNonNullAcceptedDateTest()
        {
            inputAwardLetter.AcceptedDate = DateTime.Today;
            expectedUpdatedAwardLetter = expectedRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);
            actualUpdatedAwardLetter = actualRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);

            Assert.AreEqual(expectedUpdatedAwardLetter.AcceptedDate.HasValue, actualUpdatedAwardLetter.AcceptedDate.HasValue);
            Assert.AreEqual(expectedUpdatedAwardLetter.AcceptedDate, actualUpdatedAwardLetter.AcceptedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAwardLetterArgumentThrowsExceptionTest()
        {
            actualRepository.UpdateAwardLetter(null, studentAwardYear, fafsaRecord);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStudentAwardYearArgumentThrowsExceptionTest()
        {
            actualRepository.UpdateAwardLetter(inputAwardLetter, null, fafsaRecord);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InputAwardLetterAndStudentAwardYear_StudentIdMismatchTest()
        {
            inputAwardLetter = new AwardLetter("foobar", studentAwardYear);
            actualRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InputAwardLetterAndStudentAwardYear_AwardYearMismatchTest()
        {
            var anotherAwardYear = new StudentAwardYear(studentId, "2017", new FinancialAidOffice("office"));
            inputAwardLetter = new AwardLetter(studentId, anotherAwardYear);
            actualRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);
        }

        [TestMethod]
        public void InvalidAwardLetterParametersIdThrowsException_DoesNotInvokeTransactionTest()
        {
            var invalidParamsTransaction = evalTransactionTestResponseData.First();
            var invalidRecordId = "FOOBAR";
            var invalidYear = new StudentAwardYear(studentId, invalidParamsTransaction.AwardYear, new FinancialAidOffice("office"));

            inputAwardLetter = new AwardLetter(studentId, invalidYear) { AcceptedDate = DateTime.Today };

            invalidParamsTransaction.Result = invalidRecordId;

            actualRepository = BuildAwardLetterRepository();
            var exceptionCaught = false;
            try
            {
                actualRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);
            }
            catch (KeyNotFoundException)
            {
                exceptionCaught = true;
            }

            Assert.IsTrue(exceptionCaught);

            exceptionCaught = false;

            //verify we didn't call the update transaction specific to the invalid year and the accepted date of the inputAwardLetter
            try
            {
                transManagerMock.Verify(t => t.Execute<UpdateAwardLetterRequest, UpdateAwardLetterResponse>(
                    It.Is<UpdateAwardLetterRequest>(r => r.Year == invalidYear.Code && r.AcceptedDate == DateTime.Today)));
            }
            catch (MockException)
            {
                exceptionCaught = true;
            }

            Assert.IsTrue(exceptionCaught);

        }

        [TestMethod]
        public void VerifyTransactionWasInvokedTest()
        {
            transManagerMock.Verify(t => t.Execute<UpdateAwardLetterRequest, UpdateAwardLetterResponse>(It.IsAny<UpdateAwardLetterRequest>()));
        }

        [TestMethod]
        public void UpdateRequestMatchesInputDataTest()
        {
            Assert.AreEqual(inputAwardLetter.StudentId, actualUpdateRequest.StudentId);
            Assert.AreEqual(inputAwardLetter.AwardYear.Code, actualUpdateRequest.Year);
            Assert.AreEqual(inputAwardLetter.AcceptedDate, actualUpdateRequest.AcceptedDate);
        }

        [TestMethod]
        public void LockedRecordThrowsExceptionTest()
        {
            updateAwardLetterResponseData.ErrorMessage = "YS.ACYR record is locked";
            actualRepository = BuildAwardLetterRepository();
            var exceptionCaught = false;

            try
            {
                actualUpdatedAwardLetter = actualRepository.UpdateAwardLetter(inputAwardLetter, studentAwardYear, fafsaRecord);
            }
            catch (OperationCanceledException)
            {
                exceptionCaught = true;
            }

            Assert.IsTrue(exceptionCaught);
            loggerMock.Verify(l => l.Error(It.IsAny<string>()));
        }


    }

}
