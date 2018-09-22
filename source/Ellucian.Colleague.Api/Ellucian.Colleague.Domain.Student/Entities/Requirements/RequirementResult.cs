// Copyright 2013-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class RequirementResult : BaseResult
    {
        public Requirement Requirement { get; set; }
        public List<SubrequirementResult> SubRequirementResults { get; set; }
        public HashSet<RequirementExplanation> Explanations { get; set; }

        public RequirementResult(Requirement requirement)
        {
            SubRequirementResults = new List<SubrequirementResult>();
            this.Requirement = requirement;
            this.Explanations = new HashSet<RequirementExplanation>();
        }

        public override bool IsSatisfied()
        {
            return Explanations.Count() == 1 && Explanations.Contains(RequirementExplanation.Satisfied);
        }
        public override bool IsPlannedSatisfied()
        {
            return Explanations.Contains(RequirementExplanation.PlannedSatisfied);
        }
        /// <summary>
        /// Returns IEnumerable of SubrequirementResults for the satisfied groups in this requirement
        /// </summary>
        public IEnumerable<SubrequirementResult> GetSatisfied()
        {
            return SubRequirementResults.Where(sr => sr.IsSatisfied());
        }
        /// <summary>
        /// Returns the sum of completed credits from academic credit applied to this requirement
        /// </summary>
        public override decimal GetCompletedCredits()
        {
            return SubRequirementResults.Sum(sr => sr.GetCompletedCredits());
        }
        /// <summary>
        /// Returns the sum of completed credits from academic credit applied to this requirement 
        /// </summary>
        public override decimal GetAppliedCredits()
        {
            return SubRequirementResults.Sum(sr => sr.GetAppliedCredits());
        }
        public override decimal GetPlannedAppliedCredits()
        {
            return SubRequirementResults.Sum(sr => sr.GetPlannedAppliedCredits());
        }
        /// <summary>
        /// Returns the sum of completed institutional credits from academic credit applied to this requirement
        /// </summary>
        public decimal GetInstCredits()
        {
            return SubRequirementResults.Sum(sr => sr.GetInstCredits());
        }

        /// <summary>
        /// Gets the academic credits applied to all subrequirements within this requirement and removes duplicates
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AcadResult> GetCreditsToIncludeInGpa()
        {
            return SubRequirementResults.SelectMany(sr => sr.GetCreditsToIncludeInGpa()).Distinct();
        }

        /// <summary>
        /// Returns the sum of institutional credits applied to this requirement.
        /// </summary>
        public decimal GetAppliedInstitutionalCredits()
        {
            return SubRequirementResults.Sum(sr => sr.GetAppliedInstitutionalCredits());
        }

        public override string ToString()
        {
            return "RequirementResult: " + Requirement.ToString();
        }
    }
}
