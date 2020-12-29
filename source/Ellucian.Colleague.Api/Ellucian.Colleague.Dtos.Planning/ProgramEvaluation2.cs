using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Results of a program evaluation
    /// </summary>
    public class ProgramEvaluation2
    {
        /// <summary>
        /// The unique code of the program evaluated
        /// </summary>
        public string ProgramCode { get; set; }
        /// <summary>
        /// The catalog, used with the program to get the list of requirements to evaluate
        /// </summary>
        public string CatalogCode { get; set; }
        /// <summary>
        /// Total number of credits completed
        /// </summary>
        public decimal Credits { get; set; }
        /// <summary>
        /// Total number of institutional credits completed
        /// </summary>
        public decimal InstitutionalCredits { get; set; }
        /// <summary>
        /// Of the total credits, number of credits in progress
        /// </summary>
        public decimal InProgressCredits { get; set; }
        /// <summary>
        /// Of the total credits, number of credits planned
        /// </summary>
        public decimal PlannedCredits { get; set; }
        /// <summary>
        /// GPA based on institutional credits completed
        /// </summary>
        public decimal? InstGpa { get; set; }
        /// <summary>
        /// GPA based on overall credits completed
        /// </summary>
        public decimal? CumGpa { get; set; }

        /// <summary>
        /// Overall credits modification message
        /// </summary>
        public string OverallCreditsModificationMessage { get; set; }
        /// <summary>
        /// Institutional credits modification message
        /// </summary>
        public string InstitutionalCreditsModificationMessage { get; set; }
        /// <summary>
        /// Overall GPA modification message
        /// </summary>
        public string OverallGpaModificationMessage { get; set; }
        /// <summary>
        /// Institutional GPA modification message
        /// </summary>
        public string InstitutionalGpaModificationMessage  { get; set; }

        /// <summary>
        /// List of evaluation <see cref="RequirementResult2">RequirementResult</see> items
        /// </summary>
        public List<RequirementResult2> RequirementResults { get; set; }
        /// <summary>
        /// <see cref="ProgramRequirements">ProgramRequirements</see> that were evaluated
        /// </summary>
        public ProgramRequirements ProgramRequirements { get; set; }

        /// <summary>
        /// List of planned courses with no associated academic credit that were included in the evaluation 
        /// but not applied to any requirement.
        /// </summary>
        public List<PlannedCredit> OtherPlannedCredits { get; set; }

        /// <summary>
        /// List of academic credits that were included in the evaluation but not applied to any requirement.
        /// </summary>
        public List<string> OtherAcademicCredits { get; set; }
    }
}
