// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Describes the details of a Financial Aid Satisfactory Academic Progress evaluation
    /// </summary>
    public class AcademicProgressEvaluationDetail
    {
        #region Evaluation Period Results
        /// <summary>
        /// The number of credits, or units, the student attempted during the evaluation period.
        /// </summary>
        public decimal EvaluationPeriodAttemptedCredits { get; set; }

        /// <summary>
        /// The number of credits, or units, the student completed during the evaluation period.
        /// </summary>
        public decimal EvaluationPeriodCompletedCredits { get; set; }

        /// <summary>
        /// The number of credits the student completed for courses flagged as GPA-contributing courses during the evaluation period
        /// </summary>
        public decimal EvaluationPeriodGpaCredits { get; set; }

        /// <summary>
        /// The number of grade points completed completed during the evaluation period.
        /// </summary>
        public decimal EvaluationPeriodCompletedGradePoints { get; set; }

        /// <summary>
        /// The number of grade points the student earned for courses flagged as GPA-contributing courses during the evaluation period.
        /// </summary>
        public decimal EvaluationPeriodGpaGradePoints { get; set; }

        /// <summary>
        /// The student's completed GPA for the evaluation period calculated by dividing the completed grade points by the completed credits
        /// </summary>
        public decimal EvaluationPeriodCompletedGpa { get; set; }

        /// <summary>
        /// The student's overall GPA based on courses flagged as GPA-contributing courses during the evaluation period.
        /// Calculated by dividing credits by grade points from those GPA-contributing courses.
        /// </summary>
        public decimal EvaluationPeriodOverallGpa { get; set; }

        /// <summary>
        /// The pace (rate) of completion within the evaluation period. 
        /// Calculated by dividing completed credits by attempted credits.
        /// Represented as a decimal between 0 and 1.
        /// </summary>
        public decimal EvaluationPeriodRateOfCompletion { get; set; }

        #endregion

        #region Cumulative With Remedial Results
        /// <summary>
        /// The number of cumulative credits, or units, including remedial credits the student has attempted.
        /// </summary>
        public decimal CumulativeAttemptedCredits { get; set; }

        /// <summary>
        /// The number of cumulative credits, or units, including remedial credits the student has completed
        /// </summary>
        public decimal CumulativeCompletedCredits { get; set; }

        /// <summary>
        /// The number of cumulative credits the student completed for courses flagged as GPA-contributing courses
        /// </summary>
        public decimal CumulativeGpaCredits { get; set; }

        /// <summary>
        /// The number of cumulative grade points including remedial credits the student has completed.
        /// </summary>
        public decimal CumulativeCompletedGradePoints { get; set; }

        /// <summary>
        /// The number of cumulative grade points the student earned for courses flagged as GPA-contributing courses
        /// </summary>
        public decimal CumulativeGpaGradePoints { get; set; }

        /// <summary>
        /// The student's cumulative completed GPA calculated by dividing the completed grade points by the completed credits including remedial credits
        /// </summary>
        public decimal CumulativeCompletedGpa { get; set; }

        /// <summary>
        /// The student's cumulative overall GPA based on courses flagged as GPA-contributing courses.
        /// Calculated by dividing credits by grade points from those GPA-contributing courses.
        /// </summary>
        public decimal CumulativeOverallGpa { get; set; }

        /// <summary>
        /// The student's cumulative pace (rate) of completion calculated by dividing cumulative completed credits by cumulative attempted credits including remedial credits.
        /// Represented as a decimal between 0 and 1
        /// </summary>
        public decimal CumulativeRateOfCompletion { get; set; }

        #endregion

        #region Cumulative Excluding Remedial Results
        /// <summary>
        /// The number of cumulative credits, or units, excluding remedial credits the student has attempted.
        /// </summary>
        public decimal CumulativeAttemptedCreditsExcludingRemedial { get; set; }

        /// <summary>
        /// The number of cumulative credits, or units, excluding remedial credits the student has completed.
        /// </summary>
        public decimal CumulativeCompletedCreditsExcludingRemedial { get; set; }

        /// <summary>
        /// The student's cumulative pace (rate) of completion calculated by dividing cumulative completed credits by cumulative attempted credits excluding remedial credits.
        /// </summary>
        public decimal CumulativeRateOfCompletionExcludingRemedial { get; set; }

        #endregion
    }
}
