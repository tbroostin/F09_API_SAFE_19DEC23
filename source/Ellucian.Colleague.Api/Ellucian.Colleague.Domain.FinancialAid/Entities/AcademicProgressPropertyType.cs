using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public enum AcademicProgressPropertyType
    {
        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Evaluation Period Attempted Credits
        /// </summary>
        EvaluationPeriodAttemptedCredits,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Evaluation Period Completed Credits
        /// </summary>
        EvaluationPeriodCompletedCredits,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Evaluation Period Overall GPA
        /// </summary>
        EvaluationPeriodOverallGpa,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Evaluation Period Rate of Completion
        /// </summary>
        EvaluationPeriodRateOfCompletion,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Cumulative Attempted Credits
        /// </summary>
        CumulativeAttemptedCredits,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Cumulative Completed Credits
        /// </summary>
        CumulativeCompletedCredits,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Cumulative Overall GPA
        /// </summary>
        CumulativeOverallGpa,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Cumulative Rate of Completion
        /// </summary>
        CumulativeRateOfCompletion,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Cumulative Attempted Credits excluding any remedial credits
        /// </summary>
        CumulativeAttemptedCreditsExcludingRemedial,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Cumulative Completed Credits excluding any remedial credits
        /// </summary>
        CumulativeCompletedCreditsExcludingRemedial,

        /// <summary>
        /// Indicates the AcademicProgressPropertyConfiguration describing the Cumulative Rate of Completion excluding any remedial credits
        /// </summary>
        CumulativeRateOfCompletionExcludingRemedial,

        /// <summary>
        /// Cumulative GPA including remedial credits
        /// </summary>
        CumulativeGpaIncludingRemedial,
        
        /// <summary>
        /// Identifies the AcademicProgressPropertyConfiguration describing the Academic Program's Maximum Credits
        /// </summary>
        MaximumProgramCredits
    }
}
