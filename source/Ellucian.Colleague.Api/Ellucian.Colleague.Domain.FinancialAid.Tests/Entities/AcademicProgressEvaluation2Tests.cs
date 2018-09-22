// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;


namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AcademicProgressEvaluation2Tests
    {
        public string id, studentId, statusCode, evalPeriodStartTerm,
                        evalPeriodEndTerm, academicProgressTypeCode;

        public DateTime? evalPeriodStartDate;
        public DateTime evalPeriodEndDate;
        public DateTimeOffset evalDateTime;
        public AcademicProgressProgramDetail programDetail;        
        public List<AcademicProgressAppeal> resultAppeals;
        public AcademicProgressEvaluationResult result;
        public AcademicProgressEvaluation2 academicProgressEval;

        public void AcademicProgressEvaluationInitialize()
        {
            id = "13";
            studentId = "0004791";
            statusCode = "P";
            evalPeriodStartTerm = "Fall/16";
            evalPeriodEndTerm = "Fall/17";
            academicProgressTypeCode = "GEN";
            evalPeriodStartDate = new DateTime(2016, 08, 25);
            evalPeriodEndDate = new DateTime(2017, 08, 25);
            evalDateTime = new DateTimeOffset(2016, 08, 20, 0, 0, 0, new TimeSpan());
            programDetail = new AcademicProgressProgramDetail("Econ", 180.0m, 120.0m);            
            resultAppeals = new List<AcademicProgressAppeal>()
            {
                new AcademicProgressAppeal(studentId, "1") { AcademicProgressEvaluationId = 13, AppealCounselorId = "0004657", AppealStatusCode = "REQ", AppealDate = DateTime.Today }
            };
            result = new AcademicProgressEvaluationResult(id, studentId, statusCode, evalDateTime, programDetail.ProgramCode) { 
                EvaluationPeriodEndTerm = evalPeriodEndTerm,
                AppealIds = new List<string>() { "1" },
                AcademicProgressTypeCode = academicProgressTypeCode,
                CumulativeAttemptedCredits = 66,
                CumulativeCompletedCredits = 66,
                CumulativeCompletedGradePoints = 124,
                CumulativeGpaCredits = 59,
                CumulativeGpaGradePoints = 150,
                EvaluationPeriodAttemptedCredits = 12,
                EvaluationPeriodCompletedCredits = 10,
                EvaluationPeriodCompletedGradePoints = 45,
                EvaluationPeriodGpaCredits = 10,
                EvaluationPeriodGpaGradePoints = 50
            };            
        }

        private void CreateEvaluation()
        {
            academicProgressEval = new AcademicProgressEvaluation2(result, programDetail, resultAppeals);
        }

        [TestClass]
        public class AcademicProgressEvaluation2ConstructorTests : AcademicProgressEvaluation2Tests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullResult_ThrowsArgumentNullExceptionTest()
            {
                result = null;
                CreateEvaluation();
            }

            [TestMethod]
            public void NullProgramDetail_EvaluationCreatedTest()
            {
                programDetail = null;
                CreateEvaluation();
                Assert.IsNotNull(academicProgressEval);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ResultProgramCodeNotEqualProgramDetailCode_ThrowsArgumentExceptionTest()
            {
                result = new AcademicProgressEvaluationResult(id, studentId, statusCode, evalDateTime, "MATH");
                CreateEvaluation();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NoEndDateOrTerm_ThrowsApplicationExceptionTest()
            {
                result.EvaluationPeriodEndTerm = string.Empty;
                result.EvaluationPeriodEndDate = null;
                CreateEvaluation();
            }

            [TestMethod]
            public void ObjectIsNotNullTest()
            {
                CreateEvaluation();
                Assert.IsNotNull(academicProgressEval);
            }

            [TestMethod]
            public void EvaluationId_EqualsExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(id, academicProgressEval.Id);
            }

            [TestMethod]
            public void EvaluationStudentId_EqualsExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(studentId, academicProgressEval.StudentId);
            }

            [TestMethod]
            public void EvaluationStatusCode_EqualsExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(statusCode, academicProgressEval.StatusCode);
            }

            [TestMethod]
            public void EvaluationProgressTypeCode_EqualsExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(academicProgressTypeCode, academicProgressEval.AcademicProgressTypeCode);
            }

            [TestMethod]
            public void EvaluationPeriodStartEndTerms_EqualExpectedTest()
            {
                result.EvaluationPeriodStartTerm = evalPeriodStartTerm;
                CreateEvaluation();
                Assert.AreEqual(evalPeriodStartTerm, academicProgressEval.EvaluationPeriodStartTerm);
                Assert.AreEqual(evalPeriodEndTerm, academicProgressEval.EvaluationPeriodEndTerm);
            }

            [TestMethod]
            public void EvaluationPeriodStartEndDate_EqualExpectedTest()
            {
                result.EvaluationPeriodEndTerm = null;
                result.EvaluationPeriodEndDate = evalPeriodEndDate;
                result.EvaluationPeriodStartDate = evalPeriodStartDate;
                CreateEvaluation();
                Assert.AreEqual(evalPeriodStartDate, academicProgressEval.EvaluationPeriodStartDate);
                Assert.AreEqual(evalPeriodEndDate, academicProgressEval.EvaluationPeriodEndDate);
            }

            [TestMethod]
            public void EvaluationDateTime_EqualsExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(evalDateTime, academicProgressEval.EvaluationDateTime);
            }

            [TestMethod]
            public void ProgramDetail_EqualsExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(programDetail, academicProgressEval.ProgramDetail);
            }

            [TestMethod]
            public void AcademicProgressEvaluationDetailProperties_EqualExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(result.CumulativeAttemptedCredits, academicProgressEval.Detail.CumulativeAttemptedCredits);
                Assert.AreEqual(result.CumulativeAttemptedCreditsExcludingRemedial, academicProgressEval.Detail.CumulativeAttemptedCreditsExcludingRemedial);
                Assert.AreEqual(result.CumulativeCompletedCredits, academicProgressEval.Detail.CumulativeCompletedCredits);
                Assert.AreEqual(result.CumulativeCompletedCreditsExcludingRemedial, academicProgressEval.Detail.CumulativeCompletedCreditsExcludingRemedial);
                Assert.AreEqual(result.CumulativeCompletedGradePoints, academicProgressEval.Detail.CumulativeCompletedGradePoints);
                Assert.AreEqual(result.CumulativeGpaCredits, academicProgressEval.Detail.CumulativeGpaCredits);
                Assert.AreEqual(result.CumulativeGpaGradePoints, academicProgressEval.Detail.CumulativeGpaGradePoints);
                Assert.AreEqual(result.EvaluationPeriodAttemptedCredits, academicProgressEval.Detail.EvaluationPeriodAttemptedCredits);
                Assert.AreEqual(result.EvaluationPeriodCompletedCredits, academicProgressEval.Detail.EvaluationPeriodCompletedCredits);
                Assert.AreEqual(result.EvaluationPeriodCompletedGradePoints, academicProgressEval.Detail.EvaluationPeriodCompletedGradePoints);
                Assert.AreEqual(result.EvaluationPeriodGpaCredits, academicProgressEval.Detail.EvaluationPeriodGpaCredits);
                Assert.AreEqual(result.EvaluationPeriodGpaGradePoints, academicProgressEval.Detail.EvaluationPeriodGpaGradePoints);                
            }

            [TestMethod]
            public void ResultAppeals_EqualExpectedTest()
            {
                CreateEvaluation();
                Assert.AreEqual(resultAppeals.Count, academicProgressEval.ResultAppeals.Count);
                for (int i = 0; i < resultAppeals.Count; i++)
                {
                    Assert.AreEqual(resultAppeals[i].AcademicProgressEvaluationId, academicProgressEval.ResultAppeals[i].AcademicProgressEvaluationId);
                    Assert.AreEqual(resultAppeals[i].Id, academicProgressEval.ResultAppeals[i].Id);
                    Assert.AreEqual(resultAppeals[i].AppealCounselorId, academicProgressEval.ResultAppeals[i].AppealCounselorId);
                    Assert.AreEqual(resultAppeals[i].AppealDate, academicProgressEval.ResultAppeals[i].AppealDate);
                    Assert.AreEqual(resultAppeals[i].AppealStatusCode, academicProgressEval.ResultAppeals[i].AppealStatusCode);
                }
            }

            [TestMethod]
            public void NoAppealIds_NoResultAppealsCreatedTest()
            {
                result.AppealIds = null;
                CreateEvaluation();
                Assert.IsFalse(academicProgressEval.ResultAppeals.Any());
            }

            [TestMethod]
            public void NoAppeals_NoResultAppealsCreatedTest()
            {
                resultAppeals = new List<AcademicProgressAppeal>();
                CreateEvaluation();
                Assert.IsFalse(academicProgressEval.ResultAppeals.Any());
            }

            [TestMethod]
            public void NoMatchingAppealId_NoResultAppealsCreatedTest()
            {
                result.AppealIds = new List<string>() { "foo" };
                CreateEvaluation();
                Assert.IsFalse(academicProgressEval.ResultAppeals.Any());
            }

            [TestMethod]
            public void NoMatchingEvaluationId_NoResultAppealsCreatedTest()
            {
                resultAppeals.First().AcademicProgressEvaluationId = 100;
                CreateEvaluation();
                Assert.IsFalse(academicProgressEval.ResultAppeals.Any());
            }
        }

        [TestClass]
        public class AcademicProgressEvaluationEqualsTests : AcademicProgressEvaluation2Tests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationInitialize();
            }

            [TestMethod]
            public void NullPassed_ReturnsFalseTest()
            {
                CreateEvaluation();
                Assert.IsFalse(academicProgressEval.Equals(null));
            }

            [TestMethod]
            public void WrongTypeObject_ReturnsFalseTest()
            {
                CreateEvaluation();
                Assert.IsFalse(academicProgressEval.Equals(programDetail));
            }

            [TestMethod]
            public void AcademicProgressEvaluationIdsMatch_ReturnsTrueTest()
            {
                var anotherEvaluation = new AcademicProgressEvaluation2(result, programDetail, new List<AcademicProgressAppeal>());
                CreateEvaluation();
                Assert.IsTrue(academicProgressEval.Equals(anotherEvaluation));
            }

            [TestMethod]
            public void AcademicProgressEvaluationIdsDoNotMatch_ReturnsFalseTest()
            {
                CreateEvaluation();
                result = new AcademicProgressEvaluationResult("bar", studentId, statusCode, evalDateTime, programDetail.ProgramCode) { EvaluationPeriodEndTerm = "fall" };
                var anotherEvaluation = new AcademicProgressEvaluation2(result, programDetail, new List<AcademicProgressAppeal>());
                Assert.IsFalse(academicProgressEval.Equals(anotherEvaluation));
            }
        }

        [TestClass]
        public class AcademicProgressEvaluationGetHashCodeToStringTests : AcademicProgressEvaluation2Tests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationInitialize();
            }

            [TestMethod]
            public void GetHashCode_ReturnsExpectedResultTest()
            {
                CreateEvaluation();
                int expected = academicProgressEval.Id.GetHashCode();
                Assert.AreEqual(expected, academicProgressEval.GetHashCode());
            }

            [TestMethod]
            public void ToStringMethod_ReturnsNonEmptyStringTest()
            {
                CreateEvaluation();
                Assert.IsFalse(string.IsNullOrEmpty(academicProgressEval.ToString()));
            }

            [TestMethod]
            public void ToStringMethod_ReturnsExpectedStringTest()
            {
                CreateEvaluation();
                string expected = string.Format("{0}*{1}*{2}", id, studentId, programDetail.ProgramCode);
                Assert.AreEqual(expected, academicProgressEval.ToString());
            }
        }

        [TestClass]
        public class AcademicProgressEvaluationSetEvaluationPeriodTests : AcademicProgressEvaluation2Tests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationInitialize();
                CreateEvaluation();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullEndTerm_ArgumentNullExceptionThrownTest()
            {
                academicProgressEval.SetEvaluationPeriod("spring", null);
            }

            [TestMethod]
            public void NullStartTerm_NoExceptionThrownTest()
            {
                bool exceptionThrown = false;
                try
                {
                    academicProgressEval.SetEvaluationPeriod(null, "fall");
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public void StartEndTermProvided_ExpectedValuesAreSetTest()
            {
                academicProgressEval.SetEvaluationPeriod(evalPeriodStartTerm, evalPeriodEndTerm);
                Assert.AreEqual(evalPeriodStartTerm, academicProgressEval.EvaluationPeriodStartTerm);
                Assert.AreEqual(evalPeriodEndTerm, academicProgressEval.EvaluationPeriodEndTerm);
                Assert.IsNull(academicProgressEval.EvaluationPeriodStartDate);
                Assert.IsNull(academicProgressEval.EvaluationPeriodEndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StartDateGreaterThanEndDate_ArgumentExceptionThrownTest()
            {
                academicProgressEval.SetEvaluationPeriod(DateTime.Today, new DateTime(2016, 08, 03));
            }

            [TestMethod]
            public void NoStartDate_NoExceptionThrownTest()
            {
                bool exceptionThrown = false;
                try
                {
                    academicProgressEval.SetEvaluationPeriod(null, new DateTime(2016, 08, 03));
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public void StartEndDateProvided_ExpectedValuesAreSetTest()
            {
                academicProgressEval.SetEvaluationPeriod(evalPeriodStartDate, evalPeriodEndDate);
                Assert.AreEqual(evalPeriodStartDate, academicProgressEval.EvaluationPeriodStartDate);
                Assert.AreEqual(evalPeriodEndDate, academicProgressEval.EvaluationPeriodEndDate);
                Assert.IsNull(academicProgressEval.EvaluationPeriodStartTerm);
                Assert.IsNull(academicProgressEval.EvaluationPeriodEndTerm);
            }
        }


    }
}
