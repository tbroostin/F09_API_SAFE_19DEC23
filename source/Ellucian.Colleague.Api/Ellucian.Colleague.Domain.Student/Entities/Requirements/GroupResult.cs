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
        //Do we want to evalauate related courses for this group result. 
        //This is controlled by settings on AEDF.
        public bool ShowRelatedCourses { get; private set; }


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
        public GroupResult(Group group, bool showRelatedCourses):this(group)
        {
            this.ShowRelatedCourses = showRelatedCourses;
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
            return GetApplied().Where(a => a.Explanation != AcadResultExplanation.Extra).Count();
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
        /// Related Academic Credits.
        /// Related Academic Credits can only be retrieved when AEDF related policy is Together and a flag to honor related policy is Yes. 
        /// Related Academic Credits will be Null if above conditions are not met.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AcadResult> GetRelated()
        {
            if (this.ShowRelatedCourses)
            {
                return Results == null ? new List<AcadResult>() : Results.Where(cr =>cr!=null && cr.Result != Result.Applied &&
                (
                cr.Result == Result.InCoursesListButAlreadyApplied ||
                cr.Result == Result.Replaced ||
                cr.Result == Result.ReplaceInProgress ||
                cr.Result == Result.Related ||
                cr.Result == Result.ExcludedByOverride ||
                cr.Result == Result.MaxDepartments ||
                cr.Result == Result.MaxCourses ||
                cr.Result == Result.MaxSubjects ||
                cr.Result == Result.MaxCoursesPerSubject ||
                cr.Result == Result.MaxCoursesPerDepartment ||
                cr.Result == Result.MaxCoursesAtLevel ||
                cr.Result == Result.MaxCoursesPerRule ||
                cr.Result == Result.MaxCredits ||
                cr.Result == Result.MaxCreditsPerCourse ||
                cr.Result == Result.MaxCreditsPerSubject ||
                cr.Result == Result.MaxCreditsPerDepartment ||
                cr.Result == Result.MaxCreditsAtLevel ||
                cr.Result == Result.MaxCreditsPerRule ||
                cr.Result == Result.MinCreditsPerCourse ||
                cr.Result == Result.MinGrade ||
                cr.Result == Result.MinInstitutionalCredit ||
                cr.Result == Result.MinGPA
                )
                );
            }
            else
            {
                return null;
            }
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
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.Applied && cr.Explanation != AcadResultExplanation.Extra);
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
        /// Retrieves all the academic credits that are not override applied
        /// This will skip all the courses that are override applied 
        /// override applied courses will only be included if that overidden course is in the the list of Courses to take (either through exception in EXOV or in take statement)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AcadResult> GetNonOverrideApplied()
        {
          //do it only if there are override courses applied. If not then return all the applied courses. 
            if (ForceAppliedAcademicCreditIds!=null && ForceAppliedAcademicCreditIds.Count > 0)
            {
                List<AcadResult> nonOverrideAppliedCourses = new List<AcadResult>();
                
                //pick all the courses that are applied but are not applied due to override
                nonOverrideAppliedCourses.AddRange(GetApplied().Where(a => a.GetAcadCredId() != null && !ForceAppliedAcademicCreditIds.Contains(a.GetAcadCredId())).ToList<AcadResult>());
                //convert forced applied academic credit ids course wise
                //for example- HIST-200-01 2017/SP and HIST-200-02 2019/SP are both in override applied course list. We will group them with courseId 
                ILookup<string, AcadResult> coursewiseForcedAppliedAcadCred = GetApplied().Where(a => a.GetAcadCredId() != null && (a.Result != Result.ReplaceInProgress || a.Result != Result.Replaced || a.Result != Result.ReplacedWithGPAValues) && a.GetCourse()!=null && ForceAppliedAcademicCreditIds.Contains(a.GetAcadCredId()) ).ToLookup(c => c.GetCourse().Id, c => c);
                //loop through all the oveeride applied courses that are grouped to count how many should be counted for completion
                //check how many times these forced courses exist in Courses list in group (for example Take HIST-100 HIST-200)
                // if 1 in applied and 1 in Courses list - consider only one override as applied
                //if 2 in applied and 1 in Course list -- consider only one overrirde as applied
                //if 1 in applied and 2 in Course list -- consider only one override as applied
                //if 2 in applied and 2 in Course list -- consider 2 overrides applied
                foreach (IGrouping<string, AcadResult> overrideCourse in coursewiseForcedAppliedAcadCred)
                {
                    int countOfOverrideCredits = overrideCourse.Count();
                    int countOfCourses = Group.Courses.Where(c => c == overrideCourse.Key).Count();
                    //if the override applied course is in the Courses list then we should check for how many are in list and how many times the same course is override applied. 
                    int limit = 0;
                    if (countOfCourses > 0)
                    {
                        if (countOfCourses == countOfOverrideCredits || countOfOverrideCredits > countOfCourses)
                        {
                            limit = countOfCourses;
                        }
                        else
                        {
                            limit = countOfOverrideCredits;
                        }

                        List<AcadResult> academicResults = overrideCourse.ToList();
                        for (int c = 0; c < limit; c++)
                        {
                            try
                            {
                                nonOverrideAppliedCourses.Add(academicResults[c]);
                            }
                            catch { }
                        }
                    }
                }

                return nonOverrideAppliedCourses;
            }
            else
            {
                return this.GetApplied();
            }
        }
        /// <summary>
        /// Returns IEnumerable of AcadResults for both planned courses applied to this group
        /// </summary>
        public IEnumerable<AcadResult> GetAppliedAndPlannedApplied()
        {
            return Results == null ? new List<AcadResult>() : Results.Where(cr => cr.Result == Result.Applied || cr.Result == Result.PlannedApplied);
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
            foreach (var cred in nonExtraApplied)
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
            return GetPlannedApplied().Where(p => p.Explanation != AcadResultExplanation.Extra).Sum(cr => cr.GetCredits());
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
