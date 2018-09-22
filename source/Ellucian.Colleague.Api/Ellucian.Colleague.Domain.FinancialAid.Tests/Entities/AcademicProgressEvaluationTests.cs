/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AcademicProgressEvaluationTests
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
        public ProgramRequirements programRequirements;
        public AcademicProgressEvaluationResult result;
        
        public AcademicProgressEvaluation evaluation;
        public AcademicProgressEvaluationDetail detail;
        public List<AcademicProgressAppeal> appeals;

        public void AcademicProgressEvaluationTestsInitialize()
        {
            id = "1";
            studentId = "0003914";
            statusCode = "U";
            evaluationDateTime = DateTime.Now;
            startTerm = "2015/FALL";
            endTerm = "2016/SPRING";
            startDate = new DateTime(2015, 9, 1, 0, 0, 0);
            endDate = new DateTime(2016, 5, 15, 0, 0, 0);

            programCode = "PROG1";
            programRequirements = new ProgramRequirements(programCode, "CAT1")
            {
                MinimumCredits = 100m,
                MaximumCredits = 120m
            };

            result = new AcademicProgressEvaluationResult(id, studentId, statusCode, evaluationDateTime, programCode)
            {
                EvaluationPeriodStartTerm = startTerm,
                EvaluationPeriodStartDate = startDate,
                EvaluationPeriodEndTerm = endTerm,
                EvaluationPeriodEndDate = endDate
            };

            appeals = new List<AcademicProgressAppeal>();
            evaluation = new AcademicProgressEvaluation(result, programRequirements, appeals);
            detail = evaluation.Detail;
        }

        [TestClass]
        public class AcademicProgressEvaluationConstructorTests : AcademicProgressEvaluationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationTestsInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResultsRequiredTest()
            {
                result = null;
                new AcademicProgressEvaluation(result, programRequirements, appeals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RequirementsRequiredTest()
            {
                programRequirements = null;
                new AcademicProgressEvaluation(result, programRequirements, appeals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AcademicProgramsRequiredToMatchTest()
            {
                programRequirements = new ProgramRequirements("foo", "bar");
                new AcademicProgressEvaluation(result, programRequirements, appeals);
            }

            [TestMethod]           
            public void IdFromResultsTest()
            {
                Assert.AreEqual(result.Id, evaluation.Id);
            }

            [TestMethod]
            public void StudentIdFromResultsTest()
            {
                Assert.AreEqual(result.StudentId, evaluation.StudentId);
            }

            [TestMethod]
            public void StatusCodeFromResultsTest()
            {
                Assert.AreEqual(result.StatusCode, evaluation.StatusCode);
            }

            [TestMethod]
            public void SetTermBasedEvalPeriodTest()
            {
                Assert.AreEqual(result.EvaluationPeriodStartTerm, evaluation.EvaluationPeriodStartTerm);
                Assert.AreEqual(result.EvaluationPeriodEndTerm, evaluation.EvaluationPeriodEndTerm);
            }

            [TestMethod]
            public void StartTermRequiredForTermBasedEvalPeriodTest()
            {
                result.EvaluationPeriodStartTerm = "";
                evaluation = new AcademicProgressEvaluation(result, programRequirements, appeals);
            }

            [TestMethod]
            public void EndTermRequiredForTermBasedEvalPeriodTest()
            {
                result.EvaluationPeriodEndTerm = "";
                evaluation = new AcademicProgressEvaluation(result, programRequirements, appeals);
            }

            [TestMethod]
            public void SetDateBasedEvalPeriodTest()
            {
                result.EvaluationPeriodEndTerm = "";
                result.EvaluationPeriodStartTerm = "";
                evaluation = new AcademicProgressEvaluation(result, programRequirements, appeals);
                Assert.AreEqual(result.EvaluationPeriodStartDate, evaluation.EvaluationPeriodStartDate);
                Assert.AreEqual(result.EvaluationPeriodEndDate, evaluation.EvaluationPeriodEndDate);
            }

            
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void EndDateRequiredForDateBasedEvalPeriodTest()
            {
                result.EvaluationPeriodEndTerm = "";
                result.EvaluationPeriodStartTerm = "";
                result.EvaluationPeriodEndDate = null;
                evaluation = new AcademicProgressEvaluation(result, programRequirements, appeals);
            }

            [TestMethod]
            public void EvaluationDateFromResultsTest()
            {
                Assert.AreEqual(result.EvaluationDateTime, evaluation.EvaluationDateTime);
            }

            [TestMethod]
            public void ProgramRequirementsTest()
            {
                Assert.AreEqual(programRequirements.ProgramCode, evaluation.ProgramRequirements.ProgramCode);
                Assert.AreEqual(programRequirements.CatalogCode, evaluation.ProgramRequirements.CatalogCode);
            }

            [TestMethod]
            public void AcademicProgressAppeals_InitializeTest()
            {
                Assert.IsNotNull(evaluation.ResultAppeals);
                Assert.AreEqual(appeals.Count(),evaluation.ResultAppeals.Count());
            }
            
            [TestMethod]
            public void DetailsTest()
            {
                Assert.AreEqual(result.CumulativeAttemptedCredits, detail.CumulativeAttemptedCredits);
                Assert.AreEqual(result.CumulativeAttemptedCreditsExcludingRemedial, detail.CumulativeAttemptedCreditsExcludingRemedial);
                Assert.AreEqual(result.CumulativeCompletedCredits, detail.CumulativeCompletedCredits);
                Assert.AreEqual(result.CumulativeCompletedCreditsExcludingRemedial, detail.CumulativeCompletedCreditsExcludingRemedial);
                Assert.AreEqual(result.CumulativeCompletedGradePoints, detail.CumulativeCompletedGradePoints);
                Assert.AreEqual(result.EvaluationPeriodAttemptedCredits, detail.EvaluationPeriodAttemptedCredits);
                Assert.AreEqual(result.EvaluationPeriodCompletedCredits, detail.EvaluationPeriodCompletedCredits);
                Assert.AreEqual(result.EvaluationPeriodCompletedGradePoints, detail.EvaluationPeriodCompletedGradePoints);
            }
        }
      

        [TestClass]
        public class SetEvaluationPeriodTests : AcademicProgressEvaluationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationTestsInitialize();
            }

            [TestMethod]
            public void SetTermBasedEvaluationPeriodTests()
            {
                evaluation.SetEvaluationPeriod(startTerm, endTerm);
                Assert.AreEqual(startTerm, evaluation.EvaluationPeriodStartTerm);
                Assert.AreEqual(endTerm, evaluation.EvaluationPeriodEndTerm);
                Assert.IsNull(evaluation.EvaluationPeriodStartDate);
                Assert.IsNull(evaluation.EvaluationPeriodEndDate);
            }
            
            [TestMethod]
            public void StartTermSetWhenNullTest()
            {
                evaluation.SetEvaluationPeriod(null, endTerm);
                Assert.AreEqual(evaluation.EvaluationPeriodStartTerm, null);
                Assert.AreEqual(evaluation.EvaluationPeriodEndTerm, endTerm);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EndTermRequiredTest()
            {
                evaluation.SetEvaluationPeriod(startTerm, string.Empty);
            }

            [TestMethod]
            public void SetDateBasedEvaluationPeriodTest()
            {
                evaluation.SetEvaluationPeriod(startDate, endDate);
                Assert.AreEqual(startDate, evaluation.EvaluationPeriodStartDate);
                Assert.AreEqual(endDate, evaluation.EvaluationPeriodEndDate);
                Assert.IsNull(evaluation.EvaluationPeriodStartTerm);
                Assert.IsNull(evaluation.EvaluationPeriodEndTerm);
            }

            [TestMethod]
            public void StartAndEndDatesCanBeEqualTest()
            {
                endDate = startDate;
                evaluation.SetEvaluationPeriod(startDate, endDate);
                Assert.AreEqual(startDate, evaluation.EvaluationPeriodStartDate);
                Assert.AreEqual(endDate, evaluation.EvaluationPeriodEndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StartDateRequiredBeforeEndDateTest()
            {
                endDate = startDate.AddDays(-1);
                evaluation.SetEvaluationPeriod(startDate, endDate);
            }

            [TestMethod]
            public void StartDateSetWhenNullTest()
            {
                evaluation.SetEvaluationPeriod(null, endDate);
                Assert.AreEqual(evaluation.EvaluationPeriodStartDate, null);
                Assert.AreEqual(evaluation.EvaluationPeriodEndDate, endDate);
            }
        }

        [TestClass]
        public class AcademicProgressEvaluationEqualsAndGetHashCodeTests : AcademicProgressEvaluationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationTestsInitialize();
            }

            [TestMethod]
            public void EqualIdsTest()
            {
                var test = new AcademicProgressEvaluation(result, programRequirements, appeals);
                Assert.AreEqual(test.Id, evaluation.Id);
                Assert.AreEqual(test, evaluation);
            }

            [TestMethod]
            public void NotEqualIdsTest()
            {
                result = new AcademicProgressEvaluationResult("foobar", studentId, statusCode, evaluationDateTime, programCode)
                {
                    EvaluationPeriodStartTerm = startTerm,
                    EvaluationPeriodStartDate = startDate,
                    EvaluationPeriodEndTerm = endTerm,
                    EvaluationPeriodEndDate = endDate
                };

                var test = new AcademicProgressEvaluation(result, programRequirements, appeals);
                Assert.AreNotEqual(test.Id, evaluation.Id);
                Assert.AreNotEqual(test, evaluation);
            }

            [TestMethod]
            public void NullObjectNotEqualTest()
            {
                Assert.IsFalse(evaluation.Equals(null));
            }

            [TestMethod]
            public void DiffTypeNotEqualTest()
            {
                Assert.IsFalse(evaluation.Equals(result));
            }
        }
    }
}