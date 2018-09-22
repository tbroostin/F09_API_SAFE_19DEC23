// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class GroupResult : BaseResult
    {
        public Group Group { get; set; }  
        public List<AcadResult> Results { get; set; }
        public HashSet<GroupExplanation> Explanations { get; set; }
        public List<string> EvalDebug { get; set; }
        public List<string> ForceAppliedAcademicCreditIds;
        public List<string> ForceDeniedAcademicCreditIds;
        public GroupResultMinGroupStatus MinGroupStatus { get; set; }


        public GroupResult(Group group)
        {
            this.Group = group;
            this.Results = new List<AcadResult>();
            this.Explanations = new HashSet<GroupExplanation>();
            this.EvalDebug = new List<string>();
            ForceAppliedAcademicCreditIds = new List<string>();
            ForceDeniedAcademicCreditIds = new List<string>();
            MinGroupStatus = GroupResultMinGroupStatus.None;
        }

        /// <summary>
        /// Returns true if the group represented by this GroupResult was satisfied
        /// </summary>
        public override bool IsSatisfied()
        {
            return Explanations.Count == 1 && Explanations.Contains(GroupExplanation.Satisfied);
        }
        /// <summary>
        /// Returns true if the group represented by this GroupResult was planned satisfied
        /// </summary>
        public override bool IsPlannedSatisfied()
        {
            return Explanations.Contains(GroupExplanation.PlannedSatisfied);
        }
        /// <summary>
        /// Returns an the number of academic credits applied to this group.
        /// </summary>
        /// <returns></returns>
        public int CountApplied()
        {
            return GetApplied().Count();
        }
        /// <summary>
        /// Returns an the number of planned courses applied to this group.
        /// </summary>
        /// <returns></returns>
        public int CountPlannedApplied()
        {
            return GetPlannedApplied().Count();
        }
        /// <summary>
        /// Returns an the number of academic credits applied to this group.
        /// this is only count for academic credits with explanation not marked as 'Extra'
        /// </summary>
        /// <returns></returns>
        public int CountNonExtraApplied()
        {
            return GetApplied().Where(a=>a.Explanation!=AcadResultExplanation.Extra).Count();
        }
        /// <summary>
        /// Returns an the number of planned courses applied to this group.
        /// this  only consider planned courses with explanation not marked as 'Extra'

        /// </summary>
        /// <returns></returns>
        public int CountNonExtraPlannedApplied()
        {
            return GetPlannedApplied().Where(a => a.Explanation != AcadResultExplanation.Extra).Count();
        }

        /// <summary>
        /// Returns IEnumerable of CreditResults for the credits applied to this group
        /// </summary>
        public IEnumerable<AcadResult> GetApplied()
        {
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.Applied);
        }
        /// <summary>
        /// Returns IEnumerable of AcadResults for the planned courses applied to this group
        /// </summary>
        public IEnumerable<AcadResult> GetPlannedApplied()
        {
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.PlannedApplied);
        }
        /// <summary>
        /// Returns IEnumerable of AcadResults for the credits applied to this group
        /// this  only consider academic credits with explanation not marked as 'Extra'
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AcadResult> GetNonExtraApplied()
        {
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.Applied && cr.Explanation!=AcadResultExplanation.Extra);
        }
        /// <summary>
        /// Returns IEnumerable of AcadResults for the planned courses applied to this group
        /// this  only consider planned courses with explanation not marked as 'Extra'
        /// </summary>

        public IEnumerable<AcadResult> GetNonExtraPlannedApplied()
        {
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.PlannedApplied && cr.Explanation != AcadResultExplanation.Extra);
        }
        /// <summary>
        /// Returns IEnumerable of AcadResults for both planned courses applied to this group
        /// </summary>
        public IEnumerable<AcadResult> GetAppliedAndPlannedApplied()
        {
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.Applied || cr.Result == Result.PlannedApplied );
        }
        /// <summary>
        /// Returns IEnumerable of Subjects from credits applied to this group
        /// </summary>
        public IEnumerable<string> GetAppliedSubjects()
        {
            return GetApplied().Select(ap => ap.GetSubject()).Distinct();
        }
        /// <summary>
        /// Returns IEnumerable of Subjects from planned credits applied to this group
        /// </summary>
        public IEnumerable<string> GetPlannedAppliedSubjects()
        {
            return GetPlannedApplied().Select(ap => ap.GetSubject()).Distinct();
        }
        /// <summary>
        /// Returns IEnumerable of Departments from credits applied to this group
        /// </summary>
        public IEnumerable<string> GetAppliedDepartments()
        {
            return GetApplied().SelectMany(cr => cr.GetDepartments()).Distinct();
        }

        public IEnumerable<string> GetPlannedAppliedDepartments()
        {
            return GetPlannedApplied().SelectMany(cr => cr.GetDepartments()).Distinct();
        }
        /// <summary>
        /// Returns the sum of completed credits from academic credit applied to this group 
        /// </summary>
        public override decimal GetCompletedCredits()
        {
            return GetApplied().Sum(cr => cr.GetCompletedCredits());
        }

        /// <summary>
        /// Returns the sum of completed credits from academic credit applied to this group 
        /// </summary>
        public override decimal GetAppliedCredits()
        {
            return GetApplied().Sum(cr => cr.GetAdjustedCredits());
        }

        /// <summary>
        /// Returns the sum of planned credits from academic credit applied to this group
        /// </summary>
        /// <returns></returns>
        public override decimal GetPlannedAppliedCredits()
        {
            return GetPlannedApplied().Sum(cr => cr.GetCredits());
        }
        /// <summary>
        /// Returns the sum of completed credits from academic credit applied to this group 
        /// this  only consider academic credits with explanation not marked as 'Extra'
        /// </summary>
        public decimal GetNonExtraAppliedCredits()
        {
            var nonExtraApplied = GetApplied().Where(a => a.Explanation != AcadResultExplanation.Extra).ToList();
            decimal total = 0m;
            foreach(var cred in nonExtraApplied)
            {
                var acadCred = cred.GetAcadCred();
                if (acadCred.IsCompletedCredit)
                {
                    total += acadCred.AdjustedCredit ?? 0m;
                }
                else
                {
                    total += acadCred.Credit;
                }
            }

            return total;
        }
        /// <summary>
        /// Returns the sum of completed credits from planned courses applied to this group 
        /// this  only consider planned courses with explanation not marked as 'Extra'
        /// </summary>
        public decimal GetNonExtraPlannedAppliedCredits()
        {
            return GetPlannedApplied().Where(p=>p.Explanation!=AcadResultExplanation.Extra).Sum(cr => cr.GetCredits());
        }

        /// <summary>
        /// Returns the sum of completed institution credits from academic credit applied to this group
        /// </summary>
        public decimal GetAppliedInstCredits()
        {
            var applied = GetApplied().Where(cr => cr.IsInstitutional());
            return applied.Sum(cr => cr.GetCompletedCredits());
        }

        public decimal GetPlannedAppliedInstCredits()
        {
            return GetPlannedApplied().Where(cr => cr.IsInstitutional()).Sum(cr => cr.GetCompletedCredits());
        }

        /// <summary>
        /// Return all applied academic credits and all items flagged as replaced (not applied) but included in GPA.
        /// Also include academic credits rejected due to not meeting the minimum grade requirement if Group specifies to include low grades.
        /// </summary>
        /// <returns>List of <see cref="AcadResult">Academic Results</see></returns>
        public override IEnumerable<AcadResult> GetCreditsToIncludeInGpa()
        {
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.Applied || 
            cr.Result == Result.ReplacedWithGPAValues || 
                (Group.IncludeLowGradesInGpa && cr.Result == Result.MinGrade && cr.GetGpaCredit() > 0));
        }

        public override string ToString()
        {
            return "GroupResult: " + Group.ToString();
        }

    }

    [Serializable]
    public enum GroupExplanation
    {
        /* Group was satisfied by applied courses or credits */
        Satisfied,
        PlannedSatisfied,
        /* You must take all of the following courses: "course1", "course2", type requirement not satisfied */
        Courses,
        /* Not enough courses were applied to satisfy this group */
        MinCourses,
        /* Not enough credits were applied to satisfy this group */
        MinCredits,
        /* Credits or courses applied did not cover enough departments to satisfy this group */
        MinDepartments,
        /* Credits or courses applied did not cover enough subjects to satisfy this group */
        MinSubjects,
        /* Not enough institutional credits were applied to satisfy this group */
        MinInstCredits,

        MinCoursesPerSubject,
        MinCreditsPerSubject,
        MinCoursesPerDepartment,
        MinCreditsPerDepartment,
        /* gpa for courses applied to this block overall too low */
        MinGpa

    }

    /// <summary>
    /// This Enum idenitifies Status on Group 
    /// This status is used only in case when extra course handling is defined and Mingroup rule is setup -like Take X group of Y
    /// It identifies how to handle the groups that are left after min groups are taken to fulfill subrequirment
    /// It marks those non-required groups as extra or to Ignore depending upon how extra course handling is defined.
    /// The advantage of marking those groups with the below status is how do we want this group to appear in UI . In selfService we display 
    /// (extra) in front of group description and for Ignored groups, it is not displayed at all.
    /// </summary>
    [Serializable]
    public enum GroupResultMinGroupStatus
    {
        None,//display and normal with no min groups defined
        Ignore,// similar to AEDF Ignore is setup as
        Extra// semi-apply and apply

    }
}
