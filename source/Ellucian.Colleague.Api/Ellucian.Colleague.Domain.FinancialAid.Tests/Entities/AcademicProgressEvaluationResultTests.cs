/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AcademicProgressEvaluationResultTests
    {
        public string id;
        public string studentId;
        public string statusCode;
        public DateTimeOffset evaluationDateTime;
        public string startTerm;
        public string endTerm;
        public DateTime startDate;
        public DateTime endDate;

        public string programCode;

        public decimal evalAttemptedCredits;
        public decimal evalCompletedCredits;
        public decimal evalCompletedGradePoints;
        public decimal cumAttemptedCredits;
        public decimal cumCompletedCredits;
        public decimal cumCompletedGradePoints;
        public decimal cumAttemptedCreditsExcludingRemedial;
        public decimal cumCompletedCreditsExcludingRemedial;

        public AcademicProgressEvaluationResult evaluationResult;

        public void AcademicProgressEvaluationResultTestsInitialize()
        {
            id = "1";
            studentId = "0003914";
            statusCode = "U";
            evaluationDateTime = DateTime.Now;
            startTerm = "2015/FALL";
            endTerm = "2016/SPRING";
            startDate = new DateTime(2015, 9, 1);
            endDate = new DateTime(2016, 5, 15);
            programCode = "MATH.BA";
            evalAttemptedCredits = 16m;
            evalCompletedCredits = 16m;
            evalCompletedGradePoints = 48m;
            cumAttemptedCredits = 98.5m;
            cumCompletedCredits = 90.5m;
            cumCompletedGradePoints = 400m;
            cumAttemptedCreditsExcludingRemedial = 90m;
            cumCompletedCreditsExcludingRemedial = 85m;

            evaluationResult = new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode);
        }

        [TestClass]
        public class AcademicProgressEvaluationResultConstructorTests : AcademicProgressEvaluationResultTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationResultTestsInitialize();
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, evaluationResult.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = string.Empty;
                new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode);
            }

            [TestMethod]
            public void StudentIdTest()
            {
                Assert.AreEqual(studentId, evaluationResult.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                studentId = string.Empty;
                new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode);
            }

            [TestMethod]
            public void StatusCodeTest()
            {
                Assert.AreEqual(statusCode, evaluationResult.StatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StatusCodeRequiredTest()
            {
                statusCode = string.Empty;
                new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode);
            }

            [TestMethod]
            public void ProgramCodeTest()
            {
                Assert.AreEqual(programCode, evaluationResult.AcademicProgramCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProgramCodeRequiredTest()
            {
                programCode = string.Empty;
                new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode);
            }

            
            [TestMethod]
            public void EvaluationDateTimeTest()
            {
                Assert.AreEqual(evaluationDateTime, evaluationResult.EvaluationDateTime);
            }

            [TestMethod]
            public void StartTermTest()
            {
                evaluationResult.EvaluationPeriodStartTerm = startTerm;
                Assert.AreEqual(startTerm, evaluationResult.EvaluationPeriodStartTerm);
            }

            [TestMethod]
            public void EndTermTest()
            {
                evaluationResult.EvaluationPeriodEndTerm = endTerm;
                Assert.AreEqual(endTerm, evaluationResult.EvaluationPeriodEndTerm);
            }

            [TestMethod]
            public void StartDateTest()
            {
                evaluationResult.EvaluationPeriodStartDate = startDate;
                Assert.AreEqual(startDate, evaluationResult.EvaluationPeriodStartDate);
            }

            [TestMethod]
            public void EndDateTest()
            {
                evaluationResult.EvaluationPeriodEndDate = endDate;
                Assert.AreEqual(endDate, evaluationResult.EvaluationPeriodEndDate);
            }

            [TestMethod]
            public void SetEvaluationPeriodAttemptedCreditsTest()
            {
                evaluationResult.EvaluationPeriodAttemptedCredits = evalAttemptedCredits;
                Assert.AreEqual(evalAttemptedCredits, evaluationResult.EvaluationPeriodAttemptedCredits);
            }

            [TestMethod]
            public void SetEvaluationPeriodCompleteCreditsTest()
            {
                evaluationResult.EvaluationPeriodCompletedCredits = evalCompletedCredits;
                Assert.AreEqual(evalCompletedCredits, evaluationResult.EvaluationPeriodCompletedCredits);
            }

            [TestMethod]
            public void SetEvaluationPeriodCompleteGradePointsTest()
            {
                evaluationResult.EvaluationPeriodCompletedGradePoints = evalCompletedGradePoints;
                Assert.AreEqual(evalCompletedGradePoints, evaluationResult.EvaluationPeriodCompletedGradePoints);
            }

            [TestMethod]
            public void SetCumulativeAttemptedCreditsTest()
            {
                evaluationResult.CumulativeAttemptedCredits = cumAttemptedCredits;
                Assert.AreEqual(cumAttemptedCredits, evaluationResult.CumulativeAttemptedCredits);
            }

            [TestMethod]
            public void SetCumulativeCompletedCreditsTest()
            {
                evaluationResult.CumulativeCompletedCredits = cumCompletedCredits;
                Assert.AreEqual(cumCompletedCredits, evaluationResult.CumulativeCompletedCredits);
            }

            [TestMethod]
            public void SetCumulativeCompletedGradePointsTest()
            {
                evaluationResult.CumulativeCompletedGradePoints = cumCompletedGradePoints;
                Assert.AreEqual(cumCompletedGradePoints, evaluationResult.CumulativeCompletedGradePoints);
            }

            [TestMethod]
            public void SetCumulativeAttemptedCreditsNoRemedialTest()
            {
                evaluationResult.CumulativeAttemptedCreditsExcludingRemedial = cumAttemptedCreditsExcludingRemedial;
                Assert.AreEqual(cumAttemptedCreditsExcludingRemedial, evaluationResult.CumulativeAttemptedCreditsExcludingRemedial);
            }

            [TestMethod]
            public void SetCumulativeCompletedCreditsNoRemedialTest()
            {
                evaluationResult.CumulativeCompletedCreditsExcludingRemedial = cumCompletedCreditsExcludingRemedial;
                Assert.AreEqual(cumCompletedCreditsExcludingRemedial, evaluationResult.CumulativeCompletedCreditsExcludingRemedial);
            }

        }

        [TestClass]
        public class EqualsTests : AcademicProgressEvaluationResultTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationResultTestsInitialize();
            }

            [TestMethod]
            public void NullObject_FalseTest()
            {
                Assert.IsFalse(evaluationResult.Equals(null));
            }

            [TestMethod]
            public void DiffObjectType_FalseTest()
            {
                Assert.IsFalse(evaluationResult.Equals(new Link("Title", LinkTypes.MPN, "URL")));
            }

            [TestMethod]
            public void IdEqual_TrueTest()
            {
                var testRequest = new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode);
                Assert.AreEqual(testRequest, evaluationResult);
            }

            [TestMethod]
            public void IdEqual_SameHashCodeTest()
            {
                var testRequest = new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode);
                Assert.AreEqual(testRequest.GetHashCode(), evaluationResult.GetHashCode());
            }

            [TestMethod]
            public void DiffId_FalseTest()
            {
                var testRequest = new AcademicProgressEvaluationResult("foobar", studentId, statusCode, evaluationDateTime, programCode);
                Assert.AreNotEqual(testRequest, evaluationResult);
            }

            [TestMethod]
            public void DiffId_DiffHashCodeTest()
            {
                var testRequest = new AcademicProgressEvaluationResult("foobar", studentId, statusCode, evaluationDateTime, programCode);
                Assert.AreNotEqual(testRequest.GetHashCode(), evaluationResult.GetHashCode());
            }


        }
    }
}
