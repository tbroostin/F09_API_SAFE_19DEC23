/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Describing a single evaluation of a student's academic progress for the specified time period
    /// </summary>
    [Serializable]
    public class AcademicProgressEvaluationResult
    {
        /// <summary>
        /// The unique identifier of this evaluation object
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Colleague PERSON id of the student to whom this evaluation belongs
        /// </summary>
        public string StudentId { get { return studentId; } }
        private readonly string studentId;

        /// <summary>
        /// The Status Code of the status of this evaluation. See <see cref="AcademicProgressStatus"/>
        /// </summary>
        public string StatusCode { get { return statusCode; } }
        private readonly string statusCode;

        /// <summary>
        /// The Code of the <see cref="AcademicProgram">AcademicProgram</see> used to evaluate the student's academic progress.
        /// </summary>
        public string AcademicProgramCode { get { return academicProgramCode; } }
        private readonly string academicProgramCode;

        /// <summary>
        /// The date and time that the student's Academic Progress was Evaluated. 
        /// </summary>
        public DateTimeOffset EvaluationDateTime { get { return evaluationDateTime; } }
        private readonly DateTimeOffset evaluationDateTime;

        /// <summary>
        /// The id of the start term of the academic progress evaluation.
        /// </summary>
        public string EvaluationPeriodStartTerm { get; set; }

        /// <summary>
        /// The id of the end term of the academic progress evaluation.
        /// </summary>
        public string EvaluationPeriodEndTerm { get; set; }

        /// <summary>
        /// The start date of the academic progress evaluation.
        /// </summary>
        public DateTime? EvaluationPeriodStartDate { get; set; }

        /// <summary>
        /// The end date of the academic progress evaluation.
        /// </summary>
        public DateTime? EvaluationPeriodEndDate { get; set; }

        /// <summary>
        /// The type of Evaluation result
        /// </summary>
        public string AcademicProgressTypeCode { get; set; }

        /// <summary>
        /// List of Appeal ids associated with this result - need for 
        /// correct order of appeal statuses (most recent to oldest)
        /// </summary>
        public IEnumerable<string> AppealIds { get; set; }

        #region Evaluation Period Results

        /// <summary>
        /// The number of credits, or units, the student attempted during the evaluation period.
        /// </summary>
        public decimal EvaluationPeriodAttemptedCredits {get;set;}

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

        #endregion

        #region Cumulative Including Remedial Results
        /// <summary>
        /// The number of cumulative credits, or units, the student has attempted.
        /// </summary>
        public decimal CumulativeAttemptedCredits { get; set; }

        /// <summary>
        /// The number of cumulative credits, or units, the student has completed
        /// </summary>
        public decimal CumulativeCompletedCredits { get; set; }

        /// <summary>
        /// The number of cumulative credits the student completed for courses flagged as GPA-contributing courses
        /// </summary>
        public decimal CumulativeGpaCredits { get; set; }

        /// <summary>
        /// The number of cumulative grade points the student has completed.
        /// </summary>
        public decimal CumulativeCompletedGradePoints { get; set; }

        /// <summary>
        /// The number of cumulative grade points the student earned for courses flagged as GPA-contributing courses
        /// </summary>
        public decimal CumulativeGpaGradePoints { get; set; }



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

        #endregion

        /// <summary>
        /// Create an existing academic progress evaluation object
        /// </summary>
        /// <param name="id">The unique identifier</param>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="statusCode">The status code of the evaluation. This should be an <see cref="AcademicProgressStatus"/> code</param>
        /// <param name="evaluationDateTime">The date and time this evaluation was performed</param>
        /// <param name="academicProgramCode">The code of the AcademicProgram for which these evaluation results were calculated</param>
        public AcademicProgressEvaluationResult(string id, string studentId, string statusCode, DateTimeOffset evaluationDateTime, string academicProgramCode)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(statusCode))
            {
                throw new ArgumentNullException("statusCode");
            }
            if (string.IsNullOrEmpty(academicProgramCode))
            {
                throw new ArgumentNullException("programCode");
            }

            this.id = id;
            this.studentId = studentId;
            this.statusCode = statusCode;
            this.evaluationDateTime = evaluationDateTime;
            this.academicProgramCode = academicProgramCode;
        }

        /// <summary>
        /// Two AcademicProgressEvaluationResult objects are equal when their Ids are equal.
        /// </summary>
        /// <param name="obj">The AcademicProgressEvaluationResult to compare to this one</param>
        /// <returns>True if the Ids of the AcademicProgressEvaluationResult objects are equal. False otherwise.</returns>
        public override bool Equals(object obj)
        {
            
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var evaluation = obj as AcademicProgressEvaluationResult;

            if (this.Id == evaluation.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Computes the HashCode of this object based on the Id.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns the Id of this object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Id;
        }
    }
}
