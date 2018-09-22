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
    public class AcademicProgressEvaluationDetailTests
    {
        public AcademicProgressEvaluationDetail detail;

        public void AcademicProgressEvaluationDetailTestsInitialize()
        {
            detail = new AcademicProgressEvaluationDetail();
        }

        [TestClass]
        public class AcademicProgressEvaluationDetailConstructorTests : AcademicProgressEvaluationDetailTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationDetailTestsInitialize();
            }

            [TestMethod]
            public void EvaluationPeriodAttemptedCreditsInitZeroTest()
            {
                Assert.AreEqual(0, detail.EvaluationPeriodAttemptedCredits);
            }
        }

        [TestClass]
        public class AcademicProgressEvaluationDetailPropertiesTests : AcademicProgressEvaluationDetailTests
        {
            public decimal num1;
            public decimal num2;            

            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationDetailTestsInitialize();
                num1 = 55.543m;
                num2 = 70m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EvaluationPeriodAttemptedCreditsNotLessThanZeroTest()
            {
                detail.EvaluationPeriodAttemptedCredits = -1;
            }

            [TestMethod]
            public void SetEvaluationPeriodAttemptedCreditsTest()
            {
                detail.EvaluationPeriodAttemptedCredits = num1;
                Assert.AreEqual(num1, detail.EvaluationPeriodAttemptedCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EvaluationPeriodCompleteCreditsNotLessThanZeroTest()
            {
                detail.EvaluationPeriodCompletedCredits = -1;
            }

            [TestMethod]
            public void SetEvaluationPeriodCompleteCreditsTest()
            {
                detail.EvaluationPeriodCompletedCredits = num1;
                Assert.AreEqual(num1, detail.EvaluationPeriodCompletedCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EvaluationPeriodCompleteGradePointsNotLessThanZeroTest()
            {
                detail.EvaluationPeriodCompletedGradePoints = -1;
            }

            [TestMethod]
            public void SetEvaluationPeriodCompleteGradePointsTest()
            {
                detail.EvaluationPeriodCompletedGradePoints = num1;
                Assert.AreEqual(num1, detail.EvaluationPeriodCompletedGradePoints);
            }

            [TestMethod]
            public void EvaluationPeriodGpa_EqualsGradePointsDividedByCreditsTest()
            {
                detail.EvaluationPeriodCompletedCredits = num1;
                detail.EvaluationPeriodCompletedGradePoints = num2;

                Assert.AreEqual(num2 / num1, detail.EvaluationPeriodCompletedGpa);
            }

            [TestMethod]
            public void EvaluationPeriodGpa_ZeroGradePointsReturnsZeroGpaTest()
            {
                detail.EvaluationPeriodCompletedCredits = num1;
                detail.EvaluationPeriodCompletedGradePoints = 0;

                Assert.AreEqual(0, detail.EvaluationPeriodCompletedGpa);
            }

            [TestMethod]
            public void EvaluationPeriodGpa_ZeroCreditsReturnsZeroGpaTest()
            {
                detail.EvaluationPeriodCompletedCredits = 0;
                detail.EvaluationPeriodCompletedGradePoints = num2;

                Assert.AreEqual(0, detail.EvaluationPeriodCompletedGpa);
            }

            [TestMethod]
            public void EvaluationPeriodPace_EqualsCompletedCreditsDividedByAttemptedCreditsTest()
            {
                detail.EvaluationPeriodCompletedCredits = num1;
                detail.EvaluationPeriodAttemptedCredits = num2;

                Assert.AreEqual(num1 / num2, detail.EvaluationPeriodRateOfCompletion);
            }

            [TestMethod]
            public void EvaluationPeriodPace_ZeroGradePointsReturnsZeroGpaTest()
            {
                detail.EvaluationPeriodCompletedCredits = num1;
                detail.EvaluationPeriodAttemptedCredits = 0;

                Assert.AreEqual(0, detail.EvaluationPeriodRateOfCompletion);
            }

            [TestMethod]
            public void EvaluationPeriodPace_ZeroCreditsReturnsZeroGpaTest()
            {
                detail.EvaluationPeriodCompletedCredits = 0;
                detail.EvaluationPeriodAttemptedCredits = num2;

                Assert.AreEqual(0, detail.EvaluationPeriodRateOfCompletion);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void CumulativeAttemptedCreditsNotLessThanZeroTest()
            {
                detail.CumulativeAttemptedCredits = -1;
            }

            [TestMethod]
            public void SetCumulativeAttemptedCreditsTest()
            {
                detail.CumulativeAttemptedCredits = num1;
                Assert.AreEqual(num1, detail.CumulativeAttemptedCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void CumulativeCompletedCreditsNotLessThanZeroTest()
            {
                detail.CumulativeCompletedCredits = -1;
            }

            [TestMethod]
            public void SetCumulativeCompletedCreditsTest()
            {
                detail.CumulativeCompletedCredits = num1;
                Assert.AreEqual(num1, detail.CumulativeCompletedCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void CumulativeCompletedGradePointsNotLessThanZeroTest()
            {
                detail.CumulativeCompletedGradePoints = -1;
            }

            [TestMethod]
            public void SetCumulativeCompletedGradePointsTest()
            {
                detail.CumulativeCompletedGradePoints = num1;
                Assert.AreEqual(num1, detail.CumulativeCompletedGradePoints);
            }

            [TestMethod]
            public void CumulativeGpa_EqualsGradePointsDividedByCreditsTest()
            {
                detail.CumulativeCompletedCredits = num1;
                detail.CumulativeCompletedGradePoints = num2;

                Assert.AreEqual(num2 / num1, detail.CumulativeCompletedGpa);
            }

            [TestMethod]
            public void CumulativeGpa_ZeroGradePointsReturnsZeroGpaTest()
            {
                detail.CumulativeCompletedCredits = num1;
                detail.CumulativeCompletedGradePoints = 0;

                Assert.AreEqual(0, detail.CumulativeCompletedGpa);
            }

            [TestMethod]
            public void CumulativeGpa_ZeroCreditsReturnsZeroGpaTest()
            {
                detail.CumulativeCompletedCredits = 0;
                detail.CumulativeCompletedGradePoints = num2;

                Assert.AreEqual(0, detail.CumulativeCompletedGpa);
            }

            [TestMethod]
            public void CumulativePace_EqualsCompletedCreditsDividedByAttemptedCreditsTest()
            {
                detail.CumulativeCompletedCredits = num1;
                detail.CumulativeAttemptedCredits = num2;

                Assert.AreEqual(num1 / num2, detail.CumulativeRateOfCompletion);
            }

            [TestMethod]
            public void CumulativePace_ZeroGradePointsReturnsZeroGpaTest()
            {
                detail.CumulativeCompletedCredits = num1;
                detail.CumulativeAttemptedCredits = 0;

                Assert.AreEqual(0, detail.CumulativeRateOfCompletion);
            }

            [TestMethod]
            public void CumulativePace_ZeroCreditsReturnsZeroGpaTest()
            {
                detail.CumulativeCompletedCredits = 0;
                detail.CumulativeAttemptedCredits = num2;

                Assert.AreEqual(0, detail.CumulativeRateOfCompletion);
            }

            [TestMethod]
            public void SetCumulativeAttemptedCreditsNoRemedialTest()
            {
                detail.CumulativeAttemptedCreditsExcludingRemedial = num1;
                Assert.AreEqual(num1, detail.CumulativeAttemptedCreditsExcludingRemedial);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void CumulativeAttemptedCreditsNoRemedialNotLessThanZeroTest()
            {
                detail.CumulativeAttemptedCreditsExcludingRemedial = -1;
            }

            [TestMethod]
            public void SetCumulativeCompletedCreditsNoRemedialTest()
            {
                detail.CumulativeCompletedCreditsExcludingRemedial = num1;
                Assert.AreEqual(num1, detail.CumulativeCompletedCreditsExcludingRemedial);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void CumulativeCompletedCreditsNoRemedialNotLessThanZeroTest()
            {
                detail.CumulativeCompletedCreditsExcludingRemedial = -1;
            }

            [TestMethod]
            public void CumulativePaceNoRemedial_EqualsCompletedCreditsDividedByAttemptedCreditsTest()
            {
                detail.CumulativeCompletedCreditsExcludingRemedial = num1;
                detail.CumulativeAttemptedCreditsExcludingRemedial = num2;

                Assert.AreEqual(num1 / num2, detail.CumulativeRateOfCompletionExcludingRemedial);
            }

            [TestMethod]
            public void CumulativePaceNoRemedial_ZeroGradePointsReturnsZeroGpaTest()
            {
                detail.CumulativeCompletedCreditsExcludingRemedial = num1;
                detail.CumulativeAttemptedCreditsExcludingRemedial = 0;

                Assert.AreEqual(0, detail.CumulativeRateOfCompletionExcludingRemedial);
            }

            [TestMethod]
            public void CumulativePaceNoRemedial_ZeroCreditsReturnsZeroGpaTest()
            {
                detail.CumulativeCompletedCreditsExcludingRemedial = 0;
                detail.CumulativeAttemptedCreditsExcludingRemedial = num2;

                Assert.AreEqual(0, detail.CumulativeRateOfCompletionExcludingRemedial);
            }
        }
    }
}
