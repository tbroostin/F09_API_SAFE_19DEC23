// Copyright 2013-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class SubrequirementResult : BaseResult
    {
        public Subrequirement SubRequirement { get; set; }
        public List<GroupResult> GroupResults { get; set; }
        public HashSet<SubrequirementExplanation> Explanations { get; set; }

        public SubrequirementResult(Subrequirement sr)
        {
            this.SubRequirement = sr;
            this.GroupResults = new List<GroupResult>();
            this.Explanations = new HashSet<SubrequirementExplanation>();

        }

        public override bool IsSatisfied()
        {
            return Explanations.Count == 1 && Explanations.Contains(SubrequirementExplanation.Satisfied);
        }
        public override bool IsPlannedSatisfied()
        {
            return Explanations.Contains(SubrequirementExplanation.PlannedSatisfied);
        }
        /// <summary>
        /// Returns IEnumerable of GroupResults for the satisfied groups in this subrequirement
        /// </summary>
        public IEnumerable<GroupResult> GetSatisfied()
        {
            return GroupResults.Where(gr => gr.IsSatisfied());
        }
        /// <summary>
        /// Returns IEnumerable of GroupResults for the planned satisfied groups in this subrequirement
        /// </summary>
        public IEnumerable<GroupResult> GetPlannedSatisfied()
        {
            return GroupResults.Where(gr => gr.IsPlannedSatisfied());
        }
        /// <summary>
        /// Returns the sum of completed credits from academic credit applied to this subrequirement
        /// </summary>
        public override decimal GetCompletedCredits()
        {
            return GroupResults.Sum(gr => gr.GetCompletedCredits());
        }
        /// <summary>
        /// Returns the sum of completed credits from academic credit applied to this requirement 
        /// </summary>
        public override decimal GetAppliedCredits()
        {
            return GroupResults.Sum(gr => gr.GetAppliedCredits());
        }

        public override decimal GetPlannedAppliedCredits()
        {
            return GroupResults.Sum(gr => gr.GetPlannedAppliedCredits());
        }
        /// <summary>
        /// Returns the sum of completed institutional credits from academic credit applied to this subrequirement
        /// </summary>
        public decimal GetInstCredits()
        {
            return GroupResults.Sum(gr => gr.GetAppliedInstCredits());
        }

        /// <summary>
        /// Gets the academic credits applied to all groups within this subrequirement and removes duplicates
        /// </summary>
        /// <returns>List of <see cref="AcadResult"/> objects</returns>
        public override IEnumerable<AcadResult> GetCreditsToIncludeInGpa()
        {
            return GroupResults.SelectMany(gr => gr.GetCreditsToIncludeInGpa()).Distinct();
        }
        /// <summary>
        /// Returns the sum of institutional credits from credits applied to this subrequirement
        /// </summary>
        /// <returns></returns>
        public decimal GetAppliedInstitutionalCredits()
        {
            return GroupResults.Sum(gr => gr.GetAppliedInstCredits());
        }

        public override string ToString()
        {
            return "SubRequirementResult: " + SubRequirement.ToString();
        }


    }



}
