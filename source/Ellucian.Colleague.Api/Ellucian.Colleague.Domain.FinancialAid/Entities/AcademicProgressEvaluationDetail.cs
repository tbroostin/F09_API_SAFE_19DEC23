// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Describes the details of a Financial Aid Satisfactory Academic Progress evaluation
    /// </summary>
    [Serializable]
    public class AcademicProgressEvaluationDetail
    {

        #region Evaluation Period Details

        /// <summary>
        /// The number of credits, or units, the student attempted during the evaluation period.
        /// Property cannot be set less than zero.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal EvaluationPeriodAttemptedCredits
        {
            get { return evaluationPeriodAttemptedCredits; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("EvaluationPeriodAttemptedCredits cannot be less than zero");
                evaluationPeriodAttemptedCredits = value;
            }
        }
        private decimal evaluationPeriodAttemptedCredits;

        /// <summary>
        /// The number of credits, or units, the student completed during the evaluation period.
        /// Property cannot be set less than zero.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal EvaluationPeriodCompletedCredits
        {
            get { return evaluationPeriodCompletedCredits; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("EvaluationPeriodCompletedCredits cannot be less than zero");
                evaluationPeriodCompletedCredits = value;
            }
        }
        private decimal evaluationPeriodCompletedCredits;

        /// <summary>
        /// The number of credits the student completed for courses flagged as GPA-contributing courses during the evaluation period
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal EvaluationPeriodGpaCredits
        {
            get { return evaluationPeriodGpaCredits; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("EvaluationPeriodGpaCredits cannot be less than zero");
                evaluationPeriodGpaCredits = value;
            }
        }
        private decimal evaluationPeriodGpaCredits;

        /// <summary>
        /// The number of grade points completed completed during the evaluation period.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal EvaluationPeriodCompletedGradePoints
        {
            get { return evaluationPeriodCompletedGradePoints; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("EvaluationPeriodCompletedGradePoints cannot be less than zero");
                evaluationPeriodCompletedGradePoints = value;
            }
        }
        private decimal evaluationPeriodCompletedGradePoints;

        /// <summary>
        /// The number of grade points the student earned for courses flagged as GPA-contributing courses during the evaluation period.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal EvaluationPeriodGpaGradePoints
        {
            get { return evaluationPeriodGpaGradePoints; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("EvaluationPeriodGpaGradePoints cannot be less than zero");
                evaluationPeriodGpaGradePoints = value;
            }
        }
        private decimal evaluationPeriodGpaGradePoints;


        /// <summary>
        /// The student's completed GPA for the evaluation period calculated by dividing the completed grade points by the completed credits
        /// </summary>
        public decimal EvaluationPeriodCompletedGpa
        {
            get
            {
                if (EvaluationPeriodCompletedCredits == 0)
                {
                    return 0;
                }
                return EvaluationPeriodCompletedGradePoints / EvaluationPeriodCompletedCredits;
            }
        }

        /// <summary>
        /// The student's overall GPA based on courses flagged as GPA-contributing courses during the evaluation period.
        /// Calculated by dividing credits by grade points from those GPA-contributing courses.
        /// </summary>
        public decimal EvaluationPeriodOverallGpa
        {
            get
            {
                if (EvaluationPeriodGpaCredits == 0)
                {
                    return 0;
                }
                return EvaluationPeriodGpaGradePoints / EvaluationPeriodGpaCredits;
            }
        }

        /// <summary>
        /// The pace (rate) of completion within the evaluation period. 
        /// Calculated by dividing completed credits by attempted credits.
        /// Represented as a decimal between 0 and 1.
        /// </summary>
        public decimal EvaluationPeriodRateOfCompletion
        {
            get
            {
                if (EvaluationPeriodAttemptedCredits == 0)
                {
                    return 0;
                }
                return EvaluationPeriodCompletedCredits / EvaluationPeriodAttemptedCredits;
            }
        }

        #endregion

        #region Cumulative Including Remedial Details
        /// <summary>
        /// The number of cumulative credits, or units, the student has attempted.
        /// Property cannot be set less than zero.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal CumulativeAttemptedCredits
        {
            get { return cumulativeAttemptedCredits; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("CumulativeAttemptedCredits cannot be less than zero");
                cumulativeAttemptedCredits = value;
            }
        }
        private decimal cumulativeAttemptedCredits;

        /// <summary>
        /// The number of cumulative credits, or units, the student has completed
        /// Property cannot be set less than zero.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal CumulativeCompletedCredits
        {
            get { return cumulativeCompletedCredits; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("CumulativeCompletedCredits cannot be less than zero");
                cumulativeCompletedCredits = value;
            }
        }
        private decimal cumulativeCompletedCredits;

        /// <summary>
        /// The number of cumulative credits the student completed for courses flagged as GPA-contributing courses
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal CumulativeGpaCredits
        {
            get { return cumulativeGpaCredits; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("CumulativeGpaCredits cannot be less than zero");
                cumulativeGpaCredits = value;
            }
        }
        private decimal cumulativeGpaCredits;

        /// <summary>
        /// The number of cumulative grade points the student has completed.
        /// Property cannot be set less than zero.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal CumulativeCompletedGradePoints
        {
            get { return cumulativeCompletedGradePoints; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("CumulativeCompletedGradePoints cannot be less than zero");
                cumulativeCompletedGradePoints = value;
            }
        }
        private decimal cumulativeCompletedGradePoints;

        /// <summary>
        /// The number of cumulative grade points the student earned for courses flagged as GPA-contributing courses
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if set to a value less than zero</exception>
        public decimal CumulativeGpaGradePoints
        {
            get { return cumulativeGpaGradePoints; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("CumulativeGpaGradePoints cannot be less than zero");
                cumulativeGpaGradePoints = value;
            }
        }
        private decimal cumulativeGpaGradePoints;

        /// <summary>
        /// The student's cumulative GPA based on completed credits calculated by dividing the completed grade points by the completed credits
        /// </summary>
        public decimal CumulativeCompletedGpa
        {
            get
            {
                if (CumulativeCompletedCredits == 0)
                {
                    return 0;
                }

                return CumulativeCompletedGradePoints / CumulativeCompletedCredits;
            }
        }

        /// <summary>
        /// The student's cumulative overall GPA based on courses flagged as GPA-contributing courses.
        /// Calculated by dividing credits by grade points from those GPA-contributing courses.
        /// </summary>
        public decimal CumulativeOverallGpa
        {
            get
            {
                if (CumulativeGpaCredits == 0)
                {
                    return 0;
                }
                return CumulativeGpaGradePoints / CumulativeGpaCredits;
            }
        }

        /// <summary>
        /// The student's cumulative pace (rate) of completion calculated by dividing cumulative completed credits by cumulative attempted credits including remedial credits.
        /// Represented as a decimal between 0 and 1
        /// </summary>
        public decimal CumulativeRateOfCompletion
        {
            get
            {
                if (CumulativeAttemptedCredits == 0)
                {
                    return 0;
                }
                return CumulativeCompletedCredits / CumulativeAttemptedCredits;
            }
        }
        #endregion

        #region Cumulative Excluding Remedial Details
        /// <summary>
        /// The number of cumulative credits, or units, excluding remedial credits the student has attempted.
        /// </summary>
        public decimal CumulativeAttemptedCreditsExcludingRemedial
        {
            get { return cumulativeAttemptedCreditsExcludingRemedial; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("CumulativeAttemptedCreditsExcludingRemedial cannot be less than zero");
                cumulativeAttemptedCreditsExcludingRemedial = value;
            }
        }
        private decimal cumulativeAttemptedCreditsExcludingRemedial;

        /// <summary>
        /// The number of cumulative credits, or units, excluding remedial credits the student has completed.
        /// </summary>
        public decimal CumulativeCompletedCreditsExcludingRemedial
        {
            get { return cumulativeCompletedCreditsExcludingRemedial; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("CumulativeCompletedCreditsExcludingRemedial cannot be less than zero");
                cumulativeCompletedCreditsExcludingRemedial = value;
            }

        }
        private decimal cumulativeCompletedCreditsExcludingRemedial;

        /// <summary>
        /// The student's cumulative pace (rate) of completion calculated by dividing cumulative completed credits by cumulative attempted credits excluding remedial credits.
        /// Represented as a decimal between 0 and 1
        /// </summary>
        public decimal CumulativeRateOfCompletionExcludingRemedial
        {
            get
            {
                if (CumulativeAttemptedCreditsExcludingRemedial == 0)
                {
                    return 0;
                }
                return CumulativeCompletedCreditsExcludingRemedial / CumulativeAttemptedCreditsExcludingRemedial;
            }
        }

        #endregion

    }
}
