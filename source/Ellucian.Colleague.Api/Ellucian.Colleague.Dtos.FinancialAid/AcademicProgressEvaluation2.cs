/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Describes the evaluation of Financial Aid Satisfactory Academic Progress for a student.
    /// </summary>
    public class AcademicProgressEvaluation2
    {
        /// <summary>
        /// The unique identifier of this evaluation object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Colleague PERSON id of the student to whom this evaluation belongs
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The Status Code of the status of this evaluation. See <see cref="AcademicProgressStatus"/>
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// The id of the start term of the academic progress evaluation. If a start term exists, an end term also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both. 
        /// </summary>
        public string EvaluationPeriodStartTerm { get; set; }

        /// <summary>
        /// The id of the end term of the academic progress evaluation. If a start term exists, an end term also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both.
        /// </summary>
        public string EvaluationPeriodEndTerm { get; set; }

        /// <summary>
        /// The start date of the academic progress evaluation. If a start date exists, an end date also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both.
        /// </summary>
        public DateTimeOffset? EvaluationPeriodStartDate { get; set; }

        /// <summary>
        /// The end date of the academic progress evaluation. If a start date exists, an end date also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both.
        /// </summary>
        public DateTimeOffset? EvaluationPeriodEndDate { get; set; }

        /// <summary>
        /// The date that the student's Academic Progress was Evaluated. 
        /// </summary>
        public DateTimeOffset EvaluationDateTime { get; set; }

        /// <summary>
        /// The detail of the academic program used by this evaluation:
        /// program code, max and min credits
        /// </summary>
        public AcademicProgressProgramDetail ProgramDetail { get; set; }

        /// <summary>
        /// Details about the AcademicProgressEvaluation. Contains credit calculations, GPA calculations, etc.
        /// </summary>
        public AcademicProgressEvaluationDetail Detail { get; set; }

        /// <summary>
        /// Academic Progress Appeal information
        /// </summary>
        public List<AcademicProgressAppeal> ResultAppeals { get; set; }

        /// <summary>
        /// Defines the type of the evaluation result
        /// </summary>
        public string AcademicProgressTypeCode { get; set; }
    }
}
