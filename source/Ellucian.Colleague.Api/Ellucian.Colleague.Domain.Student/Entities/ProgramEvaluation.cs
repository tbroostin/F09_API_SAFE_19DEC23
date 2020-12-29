// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ProgramEvaluation
    {
        /// <summary>
        /// Complete list of academic credit for passing to the DTO
        /// </summary>
        public List<AcademicCredit> AllCredit { get; set; }
        /// <summary>
        /// Sum of Institutional credits completed
        /// </summary>
        public decimal InstitutionalCredits { get; set; }
        /// <summary>
        /// Sum of credits completed
        /// </summary>
        public decimal Credits { get; set; }
        /// <summary>
        /// Sum of credits in progress
        /// </summary>
        public decimal InProgressCredits { get; set; }
        /// <summary>
        /// Sum of credits Planned
        /// </summary>
        public decimal PlannedCredits { get; set; }

        /// <summary>
        /// Detailed tree of requirements
        /// </summary>
        public ProgramRequirements ProgramRequirements { get; set; }
        /// <summary>
        /// Detailed tree of results
        /// </summary>
        public List<RequirementResult> RequirementResults { get; set; }
        /// <summary>
        /// Is this set of requirements satisfied, and if not, why?
        /// </summary>
        public ISet<ProgramRequirementsExplanation> Explanations { get; set; }
        /// <summary>
        /// Additional Requirements
        /// </summary>
        public List<Requirement> AdditionalRequirements { get; set; }

        public bool IsSatisfied { get; set; }
        public bool IsPlannedSatisfied { get; set; }

        public string ProgramCode { get; set; }
        public string CatalogCode { get; set; }

        // Messages set when top level of requirement tree is modified
        public string OverallCreditsModificationMessage { get; set; }
        public string InstitutionalCreditsModificationMessage { get; set; }
        public string OverallGpaModificationMessage { get; set; }
        public string InstitutionalGpaModificationMessage { get; set; }

        public List<PlannedCredit> AllPlannedCredits { get; set; }

        public List<PlannedCredit> OtherPlannedCredits
        {
            get
            {
                var notAppliedPlannedCredits = new List<PlannedCredit>();
                if (AllCredit == null)
                {
                    return notAppliedPlannedCredits;
                }
                if (RequirementResults != null && RequirementResults.Count() > 0)
                {
                    // Create a list of planned courses that were not applied to any group
                    var allAppliedPlannedCredits = RequirementResults.SelectMany(r => r.SubRequirementResults.SelectMany(s => s.GroupResults.SelectMany(g => g.GetPlannedApplied()))).ToList();
                    foreach (var plnCrs in AllPlannedCredits)
                    {
                        var appliedPlnCrs = allAppliedPlannedCredits.Where(aapc => aapc.GetCourse().Id == plnCrs.Course.Id && aapc.GetTermCode() == plnCrs.TermCode && aapc.GetSectionId() == plnCrs.SectionId).FirstOrDefault();
                        if (appliedPlnCrs == null)
                        {
                            notAppliedPlannedCredits.Add(plnCrs);
                        }
                    }
                }
                return notAppliedPlannedCredits;
            }
        }

        public List<string> OtherAcademicCredits
        {
            get
            {
                var notAppliedAcademicCreditIds = new List<string>();
                if (AllCredit == null)
                {
                    return notAppliedAcademicCreditIds;
                }
                if (RequirementResults != null && RequirementResults.Count() > 0)
                {
                    // Create a list of academic credits that were not applied to any group
                    List<string> allAppliedAcademicCreditIds = RequirementResults.SelectMany(r => r.SubRequirementResults.SelectMany(s => s.GroupResults.SelectMany(g => g.GetApplied()).Select(acadResult => acadResult.GetAcadCredId()))).ToList();
                    foreach (var acadCredit in AllCredit)
                    {
                        var appliedAcadCredId = allAppliedAcademicCreditIds.Where(aaci => aaci == acadCredit.Id).FirstOrDefault();
                        if (appliedAcadCredId == null)
                        {
                            notAppliedAcademicCreditIds.Add(acadCredit.Id);
                        }
                    }
                }
                return notAppliedAcademicCreditIds;
            }
        }

        // Constructor
        public ProgramEvaluation(List<AcademicCredit> academiccredit, string programcode = null, string catalogcode = null)
        {
            RequirementResults = new List<RequirementResult>();
            AdditionalRequirements = new List<Requirement>();
            this.Explanations = new HashSet<ProgramRequirementsExplanation>();
            if (academiccredit == null)
            {
                throw new ArgumentNullException("academiccredit");
            }
            this.AllCredit = academiccredit;

            ProgramCode = programcode;
            CatalogCode = catalogcode;
            IsSatisfied = false;
            IsPlannedSatisfied = false;

        }

        /// <summary>
        /// Returns the sum of in-progress credits from academic credit available to this program
        /// </summary>
        public decimal GetInProgressCredits()
        {
            if (AllCredit == null)
            {
                return 0m;
            }
            return AllCredit.Where(ac => !ac.IsCompletedCredit && ac.IsInstitutional() && ac.ReplacedStatus!=ReplacedStatus.ReplaceInProgress)
                .Select(ac => ac.Credit).Sum();
        }

        /// <summary>
        /// Returns the sum of adjusted completed credits from academic credit available to this program
        /// </summary>
        public decimal GetCredits()
        {
            if (AllCredit == null)
            {
                return 0m;
            }
            return AllCredit.Sum(rr => rr.AdjustedCredit ?? 0m);
        }

        /// <summary>
        /// Returns the sum of adjusted completed credits from institutional academic credit available to this program
        /// </summary>
        public decimal GetInstCredits()
        {
            if (AllCredit == null)
            {
                return 0m;
            }
            var InstCredit = AllCredit.Where(ac => ac.IsInstitutional());
            if (InstCredit.Count() > 0)
            {
                return InstCredit.Sum(ic => ic.AdjustedCredit ?? 0m);
            }
            else
            {
                return 0m;
            }
        }

        /// <summary>
        /// Calculates the GPA using the eligible academic credits. 
        /// </summary>
        public decimal? CumGpa
        {
            get
            {
                 if (AllCredit == null || AllCredit.Count == 0 || AllCredit.Sum(x => x.AdjustedGpaCredit) == 0){ return null;}
                 if(AllCredit.Sum(x => x.AdjustedGpaCredit) > 0){ return AllCredit.Sum(x => x.AdjustedGradePoints) / AllCredit.Sum(x => x.AdjustedGpaCredit);}
                 return 0;
            }
        }


        /// <summary>
        /// Calculates the Institution GPA using the eligible institution-based academic credits. 
        /// </summary>
        public decimal? InstGpa
        {
            get
            {
                if (AllCredit == null)
                {
                    return null;
                }
                var InstCredit = AllCredit.Where(ac => ac.IsInstitutional());
                if (InstCredit.Count() == 0 || InstCredit.Sum(x => x.AdjustedGpaCredit) == 0) return null;

                if (InstCredit.Sum(x => x.AdjustedGpaCredit) > 0)
                {
                    return InstCredit.Sum(x => x.AdjustedGradePoints) / InstCredit.Sum(x => x.AdjustedGpaCredit);
                }
                return 0;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Program Result: " + string.Join(",", Explanations.Select(ex => ex.ToString())));
            foreach (var rr in RequirementResults)
            {
                sb.AppendLine("\tRequirement: " + rr.Requirement.Code + "\t " + " Status: " + rr.CompletionStatus.ToString()
                                                                                  + ",  " + rr.PlanningStatus.ToString());
                foreach (var sr in rr.SubRequirementResults)
                {
                    sb.AppendLine("\t\tSubrequirement: " + sr.SubRequirement.Code + "\t " + " Status: " + sr.CompletionStatus.ToString()
                                                                                              + ", " + sr.PlanningStatus.ToString());
                    foreach (var gr in sr.GroupResults)
                    {
                        sb.AppendLine("\t\t\tGroup: " + gr.Group.Id + " " + gr.Group.Code + "\t " + string.Join(",", gr.Explanations.Select(ex => ex.ToString())));
                        foreach (string res in gr.EvalDebug)
                        {
                            sb.AppendLine("\t\t\t\t\t" + res);
                        }
                    }

                }

            }

            return sb.ToString();
        }


    }

    [Serializable]
    public enum ProgramRequirementsExplanation
    {
        Satisfied,
        PlannedSatisfied,
        MinRequirements,
        MinOverallGpa,
        MinOverallCredits,
        MinInstGpa,
        MinInstCredits

    }
}
