﻿// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Describes the evaluation of Financial Aid Satisfactory Academic Progress for a student.
    /// </summary>
    [Serializable]
    public class AcademicProgressEvaluation
    {
        /// <summary>
        /// The unique identifier of this evaluation object
        /// </summary>
        public string Id { get { return id; } }
        private string id;

        /// <summary>
        /// Colleague PERSON id of the student to whom this evaluation belongs
        /// </summary>
        public string StudentId { get { return studentId; } }
        private string studentId;

        /// <summary>
        /// The Status Code of the status of this evaluation. See <see cref="AcademicProgressStatus">AcademicProgressStatus</see>
        /// </summary>
        public string StatusCode { get { return statusCode; } }
        private string statusCode;

        /// <summary>
        /// The id of the start term of the academic progress evaluation. If a start term exists, an end term also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both. <see cref="Term">Term</see>
        /// </summary>
        public string EvaluationPeriodStartTerm { get { return evaluationPeriodStartTerm; } }
        private string evaluationPeriodStartTerm;

        /// <summary>
        /// The id of the end term of the academic progress evaluation. If a start term exists, an end term also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both. <see cref="Term">Term</see>
        /// </summary>
        public string EvaluationPeriodEndTerm { get { return evaluationPeriodEndTerm; } }
        private string evaluationPeriodEndTerm;

        /// <summary>
        /// The start date of the academic progress evaluation. If a start date exists, an end date also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both.
        /// </summary>
        public DateTimeOffset? EvaluationPeriodStartDate { get { return evaluationPeriodStartDate; } }
        private DateTimeOffset? evaluationPeriodStartDate;

        /// <summary>
        /// The end date of the academic progress evaluation. If a start date exists, an end date also exists.
        /// Either Terms or Dates are used to determine the evaluation period, but not both.
        /// </summary>
        public DateTimeOffset? EvaluationPeriodEndDate { get { return evaluationPeriodEndDate; } }
        private DateTimeOffset? evaluationPeriodEndDate;

        /// <summary>
        /// Set the term-based evaluation period. The evaluation period is either term-based or date-based so
        /// this method removes any existing evaluation period dates.
        /// </summary>
        /// <param name="startTerm">Required: The start term of the evaluation period</param>
        /// <param name="endTerm">Required: The end term of the evaluation period</param>
        /// <exception cref="ArgumentNullException">Thrown if startTerm or endTerm are null or empty</exception>
        public void SetEvaluationPeriod(string startTerm, string endTerm)
        {
            if (string.IsNullOrEmpty(endTerm))
            {
                throw new ArgumentNullException("endTerm");
            }
            evaluationPeriodStartTerm = startTerm;
            evaluationPeriodEndTerm = endTerm;
            evaluationPeriodStartDate = null;
            evaluationPeriodEndDate = null;
        }

        /// <summary>
        /// Set the date-based evaluation period. The evaluation period is either term-based or date-based so 
        /// this method removes any existing evaluation period terms.
        /// </summary>
        /// <param name="startDate">The start date of the evaluation period</param>
        /// <param name="endDate">The end date of the evaluation period</param>
        /// <exception cref="ArgumentException">Thrown if the startDate is not before the endDate</exception>
        public void SetEvaluationPeriod(DateTime? startDate, DateTime endDate)
        {
            if (startDate.HasValue  && startDate > endDate)
            {
                throw new ArgumentException("startDate must be before or the same as the endDate");
            }
            evaluationPeriodStartDate = startDate;
            evaluationPeriodEndDate = endDate;
            evaluationPeriodStartTerm = null;
            evaluationPeriodEndTerm = null;
        }

        /// <summary>
        /// The date that the student's Academic Progress was Evaluated. 
        /// </summary>
        public DateTimeOffset EvaluationDateTime { get { return evaluationDateTime; } }
        private readonly DateTimeOffset evaluationDateTime;

        /// <summary>
        /// The requirements of the academic program used by this evaluation.
        /// </summary>
        public ProgramRequirements ProgramRequirements { get { return programRequirements; } }
        private readonly ProgramRequirements programRequirements;

        /// <summary>
        /// Details about the AcademicProgressEvaluation. Contains credit calculations, GPA calculations, etc.
        /// </summary>
        public AcademicProgressEvaluationDetail Detail { get { return detail; } }
        private AcademicProgressEvaluationDetail detail;

        /// <summary>
        /// Academic Progress Appeal information
        /// </summary>
        public List<AcademicProgressAppeal> ResultAppeals { get; set; }

        /// <summary>
        /// Defines the type of the evaluation result
        /// </summary>
        public string AcademicProgressTypeCode { get; set; }

        /// <summary>
        /// Use this constructor to create an AcademicProgressEvaluation that has already been calculated. The AcademicPrograms of the
        /// result and the requirements must match.
        /// </summary>
        /// <param name="result">Required: The result of the academic progress evaluation.</param>
        /// <param name="requirements">Required: The requirements of the student's academic program</param>
        /// <exception cref="ArgumentException">Thrown if the program codes of the result and requirements arguments do not match</exception>
        public AcademicProgressEvaluation(AcademicProgressEvaluationResult result, ProgramRequirements requirements, IEnumerable<AcademicProgressAppeal> appeals)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }
            if (requirements == null)
            {
                throw new ArgumentNullException("requirements");
            }
            if (result.AcademicProgramCode != requirements.ProgramCode)
            {
                throw new ArgumentException("AcademicProgramCode of the AcademicProgressEvaluationResult must match the ProgramCode of the ProgramRequirements");
            }

            id = result.Id;
            studentId = result.StudentId;
            statusCode = result.StatusCode;
            AcademicProgressTypeCode = result.AcademicProgressTypeCode;

            if (!string.IsNullOrEmpty(result.EvaluationPeriodEndTerm))
            {
                SetEvaluationPeriod(result.EvaluationPeriodStartTerm, result.EvaluationPeriodEndTerm);
            }
            else if (result.EvaluationPeriodEndDate.HasValue)
            {
                SetEvaluationPeriod(result.EvaluationPeriodStartDate, result.EvaluationPeriodEndDate.Value);
            }
            else
            {
                throw new ApplicationException("Not enough information to set evaluation period timeframe");
            }

            evaluationDateTime = result.EvaluationDateTime;
            programRequirements = requirements;

            detail = new AcademicProgressEvaluationDetail()
            {
                CumulativeAttemptedCredits = result.CumulativeAttemptedCredits,
                CumulativeCompletedCredits = result.CumulativeCompletedCredits,
                CumulativeGpaCredits = result.CumulativeGpaCredits,
                CumulativeGpaGradePoints = result.CumulativeGpaGradePoints,
                CumulativeCompletedGradePoints = result.CumulativeCompletedGradePoints,
                CumulativeAttemptedCreditsExcludingRemedial = result.CumulativeAttemptedCreditsExcludingRemedial,
                CumulativeCompletedCreditsExcludingRemedial = result.CumulativeCompletedCreditsExcludingRemedial,
                EvaluationPeriodAttemptedCredits = result.EvaluationPeriodAttemptedCredits,
                EvaluationPeriodCompletedCredits = result.EvaluationPeriodCompletedCredits,
                EvaluationPeriodGpaCredits = result.EvaluationPeriodGpaCredits,
                EvaluationPeriodCompletedGradePoints = result.EvaluationPeriodCompletedGradePoints,
                EvaluationPeriodGpaGradePoints = result.EvaluationPeriodGpaGradePoints
            };

            ResultAppeals = new List<AcademicProgressAppeal>();
            if (appeals != null && appeals.Any() && result.AppealIds != null)
            {
                foreach (var appealId in result.AppealIds)
                {
                    var resultAppeal = appeals.FirstOrDefault(a => a.Id == appealId && a.AcademicProgressEvaluationId.ToString() == id);
                    if (resultAppeal != null)
                    {
                        var singleAppeal = new AcademicProgressAppeal(resultAppeal.StudentId, resultAppeal.Id)
                        {
                            AppealStatusCode = resultAppeal.AppealStatusCode,
                            AppealDate = resultAppeal.AppealDate,
                            AppealCounselorId = resultAppeal.AppealCounselorId,
                            AcademicProgressEvaluationId = resultAppeal.AcademicProgressEvaluationId
                        };
                        ResultAppeals.Add(singleAppeal);
                    }
                }               
            }

        }

        /// <summary>
        /// Two Evaluations are equal when their Ids are equal, or if both Ids are null/empty, 
        /// evaluations are equal when their studentId, evaluation period and academicProgram are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var evaluation = obj as AcademicProgressEvaluation;

            return evaluation.Id == this.Id;
            //Getting ahead of myself here. Currently, there's no way for this object to have a null/empty id. 
            //If that ever happens, this is the logic.
            //------------------------------------------------------------------------------------------------
            //if (!string.IsNullOrEmpty(evaluation.Id) && !string.IsNullOrEmpty(this.Id))
            //{
            //    return evaluation.Id == this.Id;
            //}
            //else
            //{
            //    return evaluation.StudentId == this.StudentId &&
            //        evaluation.EvaluationPeriodStartDate == this.EvaluationPeriodStartDate &&
            //        evaluation.EvaluationPeriodEndDate == this.EvaluationPeriodEndDate &&
            //        evaluation.EvaluationPeriodStartTerm == this.EvaluationPeriodStartTerm &&
            //        evaluation.EvaluationPeriodEndTerm == this.EvaluationPeriodEndTerm &&
            //        evaluation.ProgramRequirements.ProgramCode == this.ProgramRequirements.ProgramCode;
            //}
        }

        /// <summary>
        /// Computes the HashCode of this object based on the id. Or, if the Id is null/empty,
        /// computes the HashCode based on the studentId, the evaluationPeriod and the academicProgram
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
            //Getting ahead of myself here. Currently, there's no way for this object to have a null/empty id. 
            //If that ever happens, this is the logic.
            //------------------------------------------------------------------------------------------------
            //if (!string.IsNullOrEmpty(Id))
            //{
            //    return Id.GetHashCode();
            //}
            //else
            //{
            //    var startHashCode = EvaluationPeriodStartDate.HasValue ? EvaluationPeriodStartDate.Value.GetHashCode() : EvaluationPeriodStartTerm.GetHashCode();
            //    var endHashCode = EvaluationPeriodEndDate.HasValue ? EvaluationPeriodEndDate.Value.GetHashCode() : EvaluationPeriodEndTerm.GetHashCode();

            //    return StudentId.GetHashCode() ^ startHashCode.GetHashCode() ^ endHashCode.GetHashCode() ^ ProgramRequirements.ProgramCode.GetHashCode();
            //}
        }

        /// <summary>
        /// Returns a string representation of the object based on the Id, StudentId, and AcademicProgram
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}*{2}", Id, StudentId, ProgramRequirements.ProgramCode);
        }

    }
}
