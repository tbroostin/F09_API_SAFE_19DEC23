﻿// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class Group : BlockBase
    {
        public Subrequirement SubRequirement { get; set; }
        public GroupType GroupType { get; set; }
        public bool SkipGroup { get; set; }  // When "skipping" a group, evaluate it to see what is related, but don't "apply" any credits.

        #region Ceilings (Maximums)

        /// <summary>
        /// "MAXIMUM n COURSES PER DEPARTMENT"
        /// </summary>
        public int? MaxCoursesPerDepartment { get; set; }

        /// <summary>
        /// "MAXIMUM n COURSES PER RULE rule.name"
        /// </summary>
        public int? MaxCoursesPerRule { get; set; }
        public RequirementRule MaxCoursesRule { get; set; }

        /// <summary>
        /// "MAXIMUM n COURSES PER SUBJECT"
        /// </summary>
        public int? MaxCoursesPerSubject { get; set; }

        /// <summary>
        /// "MAXIMUM n CREDITS"
        /// </summary>
        public decimal? MaxCredits { get; set; }

        /// <summary>
        /// "MAXIMUM n HOURS PER COURSE"
        /// </summary>
        public decimal? MaxCreditsPerCourse { get; set; }

        /// <summary>
        /// "MAXIMUM n HOURS PER DEPARTMENT"
        /// </summary>
        public decimal? MaxCreditsPerDepartment { get; set; }

        /// <summary>
        /// "MAXIMUM n HOURS PER SUBJECT"
        /// </summary>
        public decimal? MaxCreditsPerSubject { get; set; }

        /// <summary>
        /// "MAXIMUM n HOURS PER RULE rule.name"
        /// </summary>
        public decimal? MaxCreditsPerRule { get; set; }
        public RequirementRule MaxCreditsRule { get; set; }

        /// <summary>
        /// "MAXIMUM n COURSES"
        /// </summary>
        public int? MaxCourses { get; set; }

        /// <summary>
        /// "MAXIMUM n DEPARTMENTS"
        /// </summary>
        public int? MaxDepartments { get; set; }

        /// <summary>
        /// "MAXIMUM n SUBJECTS"
        /// </summary>
        public int? MaxSubjects { get; set; }

        #endregion Ceilings (Maximums)

        #region Minimums

        /// <summary>
        /// "MINIMUM n COURSES PER DEPARTMENT"
        /// </summary>
        public int? MinCoursesPerDepartment { get; set; }

        /// <summary>
        /// "MINIMUM n COURSES PER SUBJECT"
        /// </summary>
        public int? MinCoursesPerSubject { get; set; }

        /// <summary>
        /// "TAKE n CREDITS ..."
        /// </summary>
        public decimal? MinCredits { get; set; }

        /// <summary>
        /// "MINIMUM n CREDITS PER COURSE"
        /// </summary>
        public decimal? MinCreditsPerCourse { get; set; }

        /// <summary>
        /// "MINIMUM n CREDITS PER DEPARTMENT"
        /// </summary>
        public decimal? MinCreditsPerDepartment { get; set; }

        /// <summary>
        /// "MINIMUM n CREDITS PER SUBJECT"
        /// </summary>
        public decimal? MinCreditsPerSubject { get; set; }

        /// <summary>
        /// "TAKE 3 of the following..."
        /// </summary>
        public int? MinCourses { get; set; }

        /// <summary>
        /// "MINIMUM n DEPARTMENTS"
        /// </summary>
        public int? MinDepartments { get; set; }

        /// <summary>
        /// "MINIMUM n SUBJECTS"
        /// </summary>
        public int? MinSubjects { get; set; }


        #endregion


        #region Maximums

        /// <summary>
        /// "MAXIMUM n COURSES at LEVELS "
        /// </summary>
        public MaxCoursesAtLevels MaxCoursesAtLevels { get; set; }


        /// <summary>
        /// "MAXIMUM n CREDITS at LEVELS "
        /// </summary>
        /// 
        public MaxCreditAtLevels MaxCreditsAtLevels { get; set; }

        #endregion

        // but-nots

        #region Excepts

        // ACRB.BUT.NOT.COURSES
        /// <summary>
        /// "EXCEPT COURSES ..."
        /// </summary>
        public List<string> ButNotCourses { get; set; }

        /// <summary>
        /// "EXCEPT LEVELS ..."
        /// </summary>
        public List<string> ButNotCourseLevels { get; set; }

        /// <summary>
        /// "EXCEPT DEPARTMENTS ..."
        /// </summary>
        public List<string> ButNotDepartments { get; set; }

        /// <summary>
        /// "EXCEPT SUBJECTS ..."
        /// </summary>
        public List<string> ButNotSubjects { get; set; }

        /// <summary>
        /// "EXCLUDE MAJ ..."
        /// Used to exclude items applied to other requirements of a particular academic requirement type.
        /// Says whether a credit satisfying one requirement type can be
        /// used to satisfy this group.
        /// </summary>
        public List<string> Exclusions { get; set; }

        #endregion

        #region Takes

        /// <summary>
        /// ".. FROM COURSES ..."
        /// </summary>
        public List<string> FromCourses { get; set; }

        /// <summary>
        /// The list of FromCourses added via an exception
        /// </summary>
        public List<string> FromCoursesException { get; set; }

        /// <summary>
        /// ".. FROM LEVELS ..."  
        /// </summary>
        public List<string> FromLevels { get; set; }  //course level

        /// <summary>
        /// ".. FROM DEPARTMENTS .."
        /// </summary>
        public List<string> FromDepartments { get; set; }

        /// <summary>
        /// ".. FROM SUBJECTS .."
        /// </summary>
        public List<string> FromSubjects { get; set; }

        #endregion

        /// <summary>
        /// Courses they must take.
        /// </summary>
        public List<string> Courses { get; set; }

        // ACRB.IN.LIST.ORDER
        /// <summary>
        /// Forces taking courses in FromCourses in the order listed.
        /// </summary>
        public bool InListOrder { get; set; }

        // Determine sequence in which groups are evaluated based on presence of rules and group type
        public int GroupTypeEvalSequence
        {
            get
            {
                // Groups with "take all" are processed first
                if (this.GroupType == GroupType.TakeAll)
                {
                    return 1;
                }
                // Groups with "take selected" are processed second
                else if (this.GroupType == GroupType.TakeSelected)
                {
                    return 2;
                }
                // All other groups, are processed last
                else
                {
                    return 3;
                }

            }
        }

        public Group(string id, string code, Subrequirement subrequirement)
            : base(id, code)
        {
            SubRequirement = subrequirement;

            ButNotDepartments = new List<string>();
            ButNotCourseLevels = new List<string>();
            ButNotSubjects = new List<string>();
            ButNotCourses = new List<string>();
            Courses = new List<string>();
            FromCourses = new List<string>();
            FromDepartments = new List<string>();
           FromSubjects = new List<string>();
            FromLevels = new List<string>();
            FromCoursesException = new List<string>();
            Exclusions = new List<string>();
        }

        public GroupResult Evaluate(IEnumerable<AcadResult> acadresults, List<Override> overrides, IEnumerable<Course> courses, List<AcademicCredit> creditsExcludedFromTranscriptGrouping = null, bool showRelatedCourses=false,bool disableLookaheadOptimization=false,  bool skipgroup = false)
        {
            SkipGroup = skipgroup;
            GroupResult groupResult = new GroupResult(this, showRelatedCourses);
            // If waived or replaced
            if (this.IsWaived || SubRequirement.IsWaived || SubRequirement.Requirement.IsWaived)
            {
                groupResult.Explanations.Add(GroupExplanation.Satisfied);
                groupResult.EvalDebug.Add("Group satisfied due to Exception");
                return groupResult;
            }


            //validate if group has overrides and contains the  credits that were excluded from transcript grouping. We want to add those acad credits that were defined in override for a group but
            //were excluded from transcript grouping
            if (overrides != null && overrides.Any() && creditsExcludedFromTranscriptGrouping != null && creditsExcludedFromTranscriptGrouping.Any())
            {
                foreach (var credits in overrides.Where(o=> o != null).SelectMany(o => o.CreditsAllowed))
                {
                    var creditFound = creditsExcludedFromTranscriptGrouping.Where(c =>!string.IsNullOrEmpty(c.Id) &&  c.Id == credits).FirstOrDefault();
                    if (creditFound != null)
                    {
                        CreditResult creditresult = new CreditResult(creditFound);
                        creditresult.Result = Result.Applied;
                        creditresult.GroupId = this.Id;
                        groupResult.ForceAppliedAcademicCreditIds.Add(creditresult.GetAcadCredId());
                        groupResult.EvalDebug.Add("Forced applied academic credit due to overrides but were excluded from transcript grouping "+ creditresult.GetAcadCredId());
                        groupResult.Results.Add(creditresult);
                    }

                }
            }

            foreach (var acadresult in acadresults)
            {

                // If we have overrides to check
                if (overrides != null && overrides.Any())
                {
                    // if this is an academic credit
                    string acadcredid = acadresult.GetAcadCredId();
                    if (acadcredid != null)
                    {
                        foreach (var over in overrides)
                        {
                            if (over.CreditsAllowed.Contains(acadcredid))
                            {
                                acadresult.Result = Result.Applied;
                                acadresult.GroupId = this.Id;
                                groupResult.ForceAppliedAcademicCreditIds.Add(acadcredid);
                                groupResult.EvalDebug.Add("Forced applied academic credit due to overrides " + acadcredid);
                            }
                        }
                    }
                }

                // if not already applied by override, check the credit/course 
                // to see if it applies. 
                // This is an important call--CheckCredit makes sure the academic credit complies
                // with the basic requirement as stated in the language. Anything that successfully
                // complies with the basic requirements will be tagged as "Related", and will 
                // continue on to the more refined checks further below.

                if (acadresult.Result != Result.Applied )
                {
                    // Get the Ids of the equates that are associated with the course in this acad.result
                    var equCourses = new List<Course>();
                    var acadResultCourse = acadresult.GetCourse();
                    if (acadResultCourse != null && acadResultCourse.EquatedCourseIds != null && acadResultCourse.EquatedCourseIds.Count() > 0)
                    {
                        equCourses = courses.Where(crs => acadResultCourse.EquatedCourseIds.Contains(crs.Id)).ToList();
                    }
                    acadresult.Result = CheckCredit(acadresult, equCourses); // froms and butnots done here 
                }

                // add the credit/course result to the group's results
                groupResult.Results.Add(acadresult);
            }
            
            // Go through the filtered list.
            var relatedResults = groupResult.Results.Where(cc => cc.Result == Result.Related).ToList();
            foreach (var acadResult in relatedResults)
            {

                if (MinCredits.HasValue || MinCreditsPerSubject.HasValue || MinCreditsPerDepartment.HasValue || MinCreditsPerCourse.HasValue || MaxCredits.HasValue || MaxCreditsPerSubject.HasValue || MaxCreditsPerDepartment.HasValue || MaxCreditsPerCourse.HasValue)
                {
                    if (acadResult.GetCredits() == 0)
                    {
                        continue;
                    }
                }
                //This is to read a flag on COURSE file to identify if the course is retaken then will its credits  be counted or not. 
                var isCourseRetakeAllowed = (acadResult.GetCourse() != null) ? acadResult.GetCourse().AllowToCountCourseRetakeCredits : false;
                // If we have overrides to check
                if (overrides != null && overrides.Any())
                {
                    // if this is an academic credit
                    string acadcredid = acadResult.GetAcadCredId();
                    if (acadcredid != null)
                    {
                        foreach (var over in overrides)
                        {
                            if (over.CreditsDenied.Contains(acadcredid))
                            {
                                groupResult.ForceDeniedAcademicCreditIds.Add(acadcredid);
                                acadResult.Result = Result.ExcludedByOverride;
                                break;
                            }
                        }
                    }
                    if (acadResult.Result == Result.ExcludedByOverride)
                    {
                        continue;
                    }

                }

                // If item is "related" but is flagged as replaced, check for GPA-impacting numbers AND
                // check against the minimum grade setting (check only if the IncludeLowGradesInGpa flag is set), 
                // save as "AppliedForGPA". If it passes those checks, it is to be included in the GPA calculation on My Progress
                // and gets the special designation "ReplacedWithGPAValues".
                // Even without GPA inclusion, skip to next result item if replaced. Replaced items are not to be applied.
                if (acadResult.GetAcadCred() != null && acadResult.GetAcadCred().ReplacedStatus == ReplacedStatus.Replaced)
                {
                    if (acadResult.GetGpaCredit() > 0 &&
                            (
                                (groupResult.Group.IncludeLowGradesInGpa == true) ||
                                (
                                    (groupResult.Group.MinGrade != null) && (acadResult.GetAcadCred().HasVerifiedGrade) &&
                                    (acadResult.GetAcadCred().VerifiedGrade.IsGreaterOrEqualUsingComparisonGrade(groupResult.Group.MinGrade))
                                )
                            )
                        )
                    {
                        acadResult.Result = Result.ReplacedWithGPAValues;
                    }
                    else
                    {
                        acadResult.Result = Result.Replaced;
                    }
                    continue;
                }
                //if this course is planned course and cannot be retaken for credits and is already applied or planned applied to group then ignore it.
                // COURSES
                if (Courses.Count > 0)
                {
                    var thisCourseCourses = new List<string>() { acadResult.GetCourse().Id };
                    var thisCourseEquates = acadResult.GetCourse().EquatedCourseIds;
                    thisCourseCourses.AddRange(thisCourseEquates);
                    // Make certain that this course (or an equate) hasn't already been applied or planned
                    // Get the courses already applied
                    var coursesApplied = groupResult.GetApplied().Where(a => a.GetCourse() != null).Select(a => a.GetCourse().Id);
                    //Get the courses already planned
                    var coursesPlanned = groupResult.GetPlannedApplied().Where(a => a.GetCourse() != null).Select(a => a.GetCourse().Id);
                    //count how many times current course or its equated courses have already been applied
                    int countOfAlreadyApplied = coursesApplied.Count(a => thisCourseCourses.Contains(a));
                    //count how many times current course or its equated courses have already been planned
                    int countOfAlreadyPlanned = coursesPlanned.Count(a => thisCourseCourses.Contains(a));
                    int countOfAlreadyAppliedOrPlanned = countOfAlreadyApplied + countOfAlreadyPlanned;
                    if (countOfAlreadyAppliedOrPlanned > 0)
                    {
                        //check if has reached limit of counts. This is going to find how many times course is allowed to be retaken.
                        //example - TAKE CRS1 CRS1 CRS2 - here CRS1 = 2  CRS2 = 1. 
                        int countOfRepititions = Courses.Count(a => a == acadResult.GetCourse().Id);
                        //if limit of counts reached - skip the course otherwise include it
                        if (countOfAlreadyAppliedOrPlanned >= countOfRepititions)
                        {
                            //if min credits is specified with TAKE statements and even if limit of counts have reached, do check
                            //for min credits are met or not. 
                            //basically if particular course appears twice or thrice then that many times course can be planned or registered or completed.
                            //If that course goes beyond the range, then it will not be picked.
                            //also note while the same course is being picked, it needs to calculate if it has already met MinCredits, if met then don't take.
                            //For example lets suppose statement was TAKE CRS1 CRS1 CRS1 and after taking CRS1 two times minCredits are met then don't pick 3rd time.

                            if (!(MinCredits.HasValue))
                            {

                                acadResult.Result = Result.InCoursesListButAlreadyApplied;
                                continue;
                            }
                            else
                            {
                                if (!MinCreditsNotMet(groupResult, true))
                                {
                                    acadResult.Result = Result.InCoursesListButAlreadyApplied;
                                    continue;
                                }
                            }
                        }
                    }
                }

                // MAX COURSES
                if (MaxCourses.HasValue && (groupResult.CountApplied() + groupResult.CountPlannedApplied()) >= MaxCourses.Value)
                {
                    acadResult.Result = Result.MaxCourses;
                    continue;
                }

                // MAX DEPARTMENTS
                if (MaxDepartments.HasValue)
                {
                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    var appliedDepartments = appliedPlusThis.SelectMany(ap => ap.GetDepartments()).Distinct();
                    if (appliedDepartments.Count() > MaxDepartments.Value)
                    {
                        acadResult.Result = Result.MaxDepartments;
                        continue;
                    }
                }

                // MAX SUBJECTS
                if (MaxSubjects.HasValue)
                {
                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    var appliedSubjects = appliedPlusThis.Select(ap => ap.GetSubject()).Distinct();
                    if (appliedSubjects.Count() > MaxSubjects.Value)
                    {
                        acadResult.Result = Result.MaxSubjects;
                        continue;
                    }

                }
                // MAX CREDITS
                if (MaxCredits.HasValue && MaxCredits.Value > 0)
                {

                    if ((groupResult.GetAppliedCredits() + groupResult.GetPlannedAppliedCredits() + acadResult.GetCredits()) > MaxCredits.Value)
                    {
                        acadResult.Result = Result.MaxCredits;
                        continue;
                    }

                }

                // MAX_CREDITS_PER_COURSE

                if (MaxCreditsPerCourse.HasValue && acadResult.GetCredits() > MaxCreditsPerCourse.Value)
                {
                    acadResult.Result = Result.MaxCreditsPerCourse;
                    continue;
                }

                // MAX_CREDITS_PER_DEPARTMENT

                if (MaxCreditsPerDepartment.HasValue)
                {
                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    IEnumerable<string> deptList = appliedPlusThis.SelectMany(dl => dl.GetDepartments()).Distinct();

                    foreach (var dept in deptList)
                    {
                        // count the number of courses per department
                        // we have a unique list of departments
                        // get the credits that apply to each

                        ISet<AcadResult> resultsthisdept = new HashSet<AcadResult>();
                        foreach (var acadresult in appliedPlusThis)
                        {
                            if (acadresult.GetDepartments().Contains(dept))
                            {
                                resultsthisdept.Add(acadresult);
                            }
                        }

                        if (resultsthisdept.Sum(rtd => rtd.GetCredits()) > MaxCreditsPerDepartment.Value)
                        {
                            acadResult.Result = Result.MaxCreditsPerDepartment;
                            break;
                        }
                    }
                    if (acadResult.Result == Result.MaxCreditsPerDepartment)
                    {
                        continue;
                    }
                }


                // MAX_CREDITS_PER_SUBJECT

                if (MaxCreditsPerSubject.HasValue)
                {
                    //if (acadResult.GetCredits() == 0)
                    //{
                    //    continue;
                    //}
                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    IEnumerable<IGrouping<string, AcadResult>> resultsthissubject = appliedPlusThis.GroupBy(cc => cc.GetSubject());
                    // no individual grouping may have more than "n" credits
                    foreach (var grouping in resultsthissubject)
                    {
                        if (grouping.Sum(grp => grp.GetCredits()) > MaxCreditsPerSubject.Value)
                        {
                            acadResult.Result = Result.MaxCreditsPerSubject;
                            break;
                        }
                    }

                    if (acadResult.Result == Result.MaxCreditsPerSubject)
                    {
                        continue;
                    }
                }


                // MAX_COURSES_PER_SUBJECT

                if (MaxCoursesPerSubject.HasValue)
                {
                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    IEnumerable<IGrouping<string, AcadResult>> coursesthissubject = appliedPlusThis.GroupBy(cc => cc.GetSubject());
                    // no individual grouping may have more than "n" courses
                    foreach (var grouping in coursesthissubject)
                    {
                        if (grouping.Count() > MaxCoursesPerSubject.Value)
                        {
                            acadResult.Result = Result.MaxCoursesPerSubject;
                            break;
                        }
                    }
                    if (acadResult.Result == Result.MaxCoursesPerSubject)
                    {
                        continue;
                    }
                }

                // Max courses per department
                //
                // It is notable that each credit can be associated with multiple departments, even though it can have only
                // one subject.  We are counting the credit against *all* departments with which it is associated for this
                // maximum.   That the original degree audit did this may or may not be a valid assumption.

                if (MaxCoursesPerDepartment.HasValue)
                {

                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    IEnumerable<string> deptList = appliedPlusThis.SelectMany(dl => dl.GetDepartments()).Distinct().ToList();

                    foreach (var dept in deptList)
                    {
                        // count the number of courses per department
                        // we have a unique list of departments
                        // get the credits that apply to each

                        ISet<AcadResult> resultsthisdept = new HashSet<AcadResult>();
                        foreach (var acadresult in appliedPlusThis)
                        {
                            if (acadresult.GetDepartments().Contains(dept))
                            {
                                resultsthisdept.Add(acadresult);
                            }
                        }

                        if (resultsthisdept.Count > MaxCoursesPerDepartment.Value)
                        {
                            acadResult.Result = Result.MaxCoursesPerDepartment;
                            break;
                        }


                    }
                    if (acadResult.Result == Result.MaxCoursesPerDepartment)
                    {
                        continue;
                    }
                }

                // max credits at levels

                if (MaxCreditsAtLevels != null && MaxCreditsAtLevels.MaxCredit > 0)
                {
                    //if(acadResult.GetCredits()==0)
                    //{
                    //    continue;
                    //}
                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    IEnumerable<AcadResult> resultsthislevel = appliedPlusThis.Where(apt => apt.GetCourseLevels().Intersect(MaxCreditsAtLevels.Levels).Any());
                    var totalcredits = resultsthislevel.Sum(res => res.GetCredits());
                    if (totalcredits > MaxCreditsAtLevels.MaxCredit)
                    {
                        acadResult.Result = Result.MaxCreditsAtLevel;
                        continue;
                    }
                }

                // Max courses at levels

                if (MaxCoursesAtLevels != null && MaxCoursesAtLevels.MaxCourses > 0)
                {
                    IEnumerable<AcadResult> appliedPlusThis = groupResult.GetAppliedAndPlannedApplied().Union(new List<AcadResult>() { acadResult });
                    IEnumerable<AcadResult> resultsthislevel = appliedPlusThis.Where(apt => apt.GetCourseLevels().Intersect(MaxCoursesAtLevels.Levels).Any());
                    if (resultsthislevel.Count() > MaxCoursesAtLevels.MaxCourses)
                    {
                        acadResult.Result = Result.MaxCoursesAtLevel;
                        continue;
                    }
                }

                // MIN_CREDITS_PER_COURSE

                if (MinCreditsPerCourse.HasValue && acadResult.GetCredits() < MinCreditsPerCourse.Value)
                {
                    acadResult.Result = Result.MinCreditsPerCourse;
                    continue;
                }

                // MIN_GRADE <of> x  (In group syntax, first grade is the min grade, any other grades following it are the allowed grades.)
                // If a minimum grade has been specified and this item has a grade that is not incomplete (i.e. still in progress), 
                // check to see if the academic credit's grade is one of the allowed grades or if not,
                // check to see if it meets the minimum grade value. 
                // An item that is ungraded is not subject to these grade limits
                var acadResultGrade = acadResult.GetGrade();
                if (MinGrade != null && acadResultGrade != null && string.IsNullOrEmpty(acadResultGrade.IncompleteGrade))
                {
                    bool hitAllowedGrade = false;
                    if (AllowedGrades != null)
                    {
                        IEnumerable<Grade> ggg = AllowedGrades.Where(ag => ag.IsEquivalentUsingComparisonGrade(acadResultGrade));
                        if (ggg != null && ggg.Count() > 0)
                        {
                            hitAllowedGrade = true;
                        }
                    }
                    // If we haven't hit an otherwise allowed grade, check the value against the MinGrade value.
                    // Flag if it fails the minimum grade check
                    if (!hitAllowedGrade && !acadResultGrade.IsGreaterOrEqualUsingComparisonGrade(MinGrade))
                    {
                        acadResult.Result = Result.MinGrade;
                        continue;
                    }
                }


                // Min GPA lookahead optimization only when lookahead optimization is not disabled(which is default value)
                //
                // if there is a Min GPA requirement on this group       AND
                // we have not applied any planned courses to this group AND
                // this is a credit                                      AND
                // it has a grade                                        AND
                // we are about to go over the min courses/credits       AND
                // the current credit is not enough to take us over it   OR
                // would drag us back under it                           THEN
                // don't apply it.

                if (MinGpa != null && disableLookaheadOptimization==false)
                {
                    if (groupResult.GetApplied().Where(ap => ap.GetAcadCred() == null).Count() == 0)
                    {
                        AcademicCredit ar = acadResult.GetAcadCred();
                        if (ar != null)
                        {
                            if (ar.VerifiedGrade != null)
                            {
                                int coursecountwiththis = groupResult.GetApplied().Count() + 1;
                                decimal creditcountwiththis = groupResult.GetAppliedCredits() + ar.Credit;

                                if (((MinCourses.HasValue) && (groupResult.GetApplied().Count() + 1) >= MinCourses) ||
                                    ((MinCredits.HasValue) && (groupResult.GetAppliedCredits() + ar.Credit) >= MinCredits))
                                {
                                    var gpaResults = groupResult.GetCreditsToIncludeInGpa();
                                    decimal gpa = 0m;
                                    if ((groupResult.GetGpaCredits(gpaResults) + ar.GpaCredit ?? 0m) != 0)
                                    {
                                        gpa = (groupResult.GetGradePoints(gpaResults) + ar.GradePoints) / (groupResult.GetGpaCredits(gpaResults) + ar.GpaCredit ?? 0m);
                                    }
                                    if (gpa < MinGpa)
                                    {
                                        acadResult.Result = Result.MinGPA;
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }

                // Min Institutional Credit lookahead optimization
                //
                // if there is a Min inst cred requirement on this group AND
                // we have not applied any planned courses to this group AND
                // this is a credit                                      AND
                // it is not institutional                               AND
                // we haven't met the minimum inst for this group        AND
                // we are about to go over the min courses/credits       AND
                // don't apply it.

                if (MinInstitutionalCredits != null)
                {
                    if (groupResult.GetApplied().Where(ap => ap.GetAcadCred() == null).Count() == 0)
                    {
                        AcademicCredit ar = acadResult.GetAcadCred();
                        if (ar != null)
                        {
                            if (!ar.IsInstitutional() && MinInstCreditsNotMet(groupResult, true))
                            {
                                int coursecountwiththis = groupResult.GetApplied().Count() + 1;
                                decimal creditcountwiththis = groupResult.GetAppliedCredits() + ar.Credit;

                                if (((MinCourses.HasValue) && (groupResult.GetApplied().Count() + 1) >= MinCourses) ||
                                    ((MinCredits.HasValue) && (groupResult.GetAppliedCredits() + ar.Credit) >= MinCredits))
                                {
                                    acadResult.Result = Result.MinInstitutionalCredit;
                                    continue;
                                }
                            }
                        }
                    }
                }

                // MAX_COURSES_PER_RULE

                if (MaxCoursesPerRule != null)
                {
                    // if the credit fails the rule
                    if (MaxCoursesRule.Passes(acadResult, false))
                    {
                        // count how many others also failed
                        var appliedAlsoFailedCount = groupResult.GetAppliedAndPlannedApplied().Where(app => MaxCoursesRule.Passes(app, false)).Count();
                        if (appliedAlsoFailedCount + 1 > MaxCoursesPerRule)  //plus this
                        {
                            acadResult.Result = Result.MaxCoursesPerRule;
                            continue;
                        }
                    }
                }

                // MAX_CREDITS_PER_RULE

                if (MaxCreditsPerRule != null)
                {
                    // if the course or credit credit passes the rule, then the check is triggered
                    if (MaxCreditsRule.Passes(acadResult, false))
                    {
                        groupResult.EvalDebug.Add(" MaxCredits Rule " + MaxCreditsRule + " passes " + acadResult.ToString());

                        // count how many credits have been applied from others that also passed the rule
                        List<AcadResult> appliedAlsoPassed = groupResult.GetAppliedAndPlannedApplied().Where(ap => MaxCreditsRule.Passes(ap, false)).ToList();

                        // Add this credit's credits to it and check the total
                        var appliedAlsoPassedPlusThis = appliedAlsoPassed.Union(new List<AcadResult>() { acadResult });
                        var sumPassingPlusThis = appliedAlsoPassedPlusThis.Select(aaf => aaf.GetCredits()).Sum();
                        if (sumPassingPlusThis > MaxCreditsPerRule)
                        {
                            acadResult.Result = Result.MaxCreditsPerRule;
                            continue;
                        }
                    }
                }

                // We need to apply the current course/credit if:
                //  It's not a specific course that is already applied
                //  It's a required course or an equivalent of a required course.
                //  There is an unmet minimum, and the course/credit can count towards it.
                if (((acadResult.GetCourse() != null) && ((Courses.Contains(acadResult.GetCourse().Id)) || (Courses.Intersect(acadResult.GetCourse().EquatedCourseIds).Count() != 0)) ||
                    MinCoursesNotMet(groupResult, true) ||
                    (MinDepartmentsNotMet(groupResult, true) && IncreasesDeptCountBy(groupResult, acadResult) > 0) ||
                    (MinSubjectsNotMet(groupResult, true) && IncreasesSubjectCount(groupResult, acadResult)) ||
                    AppliesToNeededDepartment(groupResult, acadResult) ||
                    AppliesToNeededSubject(groupResult, acadResult) ||
                   MinCreditsNotMet(groupResult, true)))
                {

                    acadResult.GroupId = this.Id;
                    if (!SkipGroup)
                    {
                        if (acadResult.GetAcadCred() == null)
                        {
                            acadResult.Result = Result.PlannedApplied;
                        }
                        else
                        {
                            acadResult.Result = Result.Applied;
                        }
                    }
                    else
                    {
                        acadResult.Result = Result.Related;
                    }
                }
                //This code is to mark explanation of acadResult as 'Extra'. 
                //This point of code is only reached when no max rule is met but a min rule is met. It means all the courses
                //picked after min rules are met, will still be applied but marked as 'extra'

                if (acadResult.Result != Result.PlannedApplied && acadResult.Result != Result.Applied && ((ExtraCourseDirective == ExtraCourses.Apply) || (ExtraCourseDirective == ExtraCourses.SemiApply)))
                {
                    acadResult.GroupId = this.Id;
                    if (!SkipGroup)
                    {
                        if (acadResult.GetAcadCred() == null)
                        {
                            acadResult.Result = Result.PlannedApplied;
                        }
                        else
                        {
                            acadResult.Result = Result.Applied;
                        }
                        acadResult.Explanation = AcadResultExplanation.Extra;
                    }
                    else
                    {
                        acadResult.Result = Result.Related;
                    }
                }
            }

            // All credits processed.  Set group result.
            bool fullyPlanned = true;
            if (Courses.Count > 0 )
            {
                int countOfCoursesApplied = 0;
                if (groupResult.ForceAppliedAcademicCreditIds.Count > 0)
                {
                    //find all the applied course that are not override applied. Basically we don't want to count courses that were applied because of override.
                    countOfCoursesApplied = groupResult.GetNonOverrideApplied().Count();
                }
                else
                {
                    countOfCoursesApplied = groupResult.CountApplied();
                }

                if (countOfCoursesApplied < Courses.Count)
                {
                    groupResult.Explanations.Add(GroupExplanation.Courses);
                    if (countOfCoursesApplied + groupResult.CountPlannedApplied() < Courses.Count)
                    {
                        fullyPlanned = false;
                    }
                }
            }

            if (MinCoursesNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinCourses);
                if (MinCoursesNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinCreditsNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinCredits);
                if (MinCreditsNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinDepartmentsNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinDepartments);
                if (MinDepartmentsNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinSubjectsNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinSubjects);
                if (MinSubjectsNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinInstCreditsNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinInstCredits);
                if (MinInstCreditsNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinCreditsPerSubjectNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinCreditsPerSubject);
                if (MinCreditsPerSubjectNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinCreditsPerDepartmentNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinCreditsPerDepartment);
                if (MinCreditsPerDepartmentNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinCoursesPerSubjectNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinCoursesPerSubject);
                if (MinCoursesPerSubjectNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinCoursesPerDepartmentNotMet(groupResult, false))
            {
                groupResult.Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                if (MinCoursesPerDepartmentNotMet(groupResult, true))
                {
                    fullyPlanned = false;
                }
            }

            if (MinGpaNotMet(groupResult))
            {
                groupResult.Explanations.Add(GroupExplanation.MinGpa);
            }

            if (fullyPlanned == true && groupResult.Explanations.Count() != 0)
            {
                groupResult.Explanations.Add(GroupExplanation.PlannedSatisfied);
            }

            if (groupResult.Explanations.Count() == 0)
            {
                groupResult.Explanations.Add(GroupExplanation.Satisfied);
            }
            // Free up extra credits for 'ignore' option and semi-apply option only when course reuse is allowed.
            //for apply otion, 'free up extra credits' will not be executed.
            if (!(ExtraCourseDirective == ExtraCourses.Apply || (ExtraCourseDirective == ExtraCourses.SemiApply && groupResult.Group.SubRequirement.Requirement.AllowsCourseReuse)))
            {
                FreeUpExtraAppliedCredits(groupResult);
            }

            // Record the interaction of each course or credit with this group
            // For debugging only.  Used in ProgramResult.Dump().

            foreach (var cr in groupResult.Results)
            {
                groupResult.EvalDebug.Add(cr.ToString() + " " + cr.Result.ToString());
            }

            return groupResult;
        }
        //**********************************************************

        /// <summary>
        /// Used to free up academic credits if too many credits were used to satisfy a requirement
        /// </summary>
        /// <remarks>
        /// For example, if the requirement was "take 4 credits", and we applied (in the order given)...
        ///   1) a 1 credit course
        ///   2) a 2 credit course
        ///   3) a 3 credit course
        /// then we've applied (and locked) 3 courses.
        /// Since the second course is the closest match to the amount of extra credits it is not needed.
        /// The second course should be release it to be used elsewhere.
        /// </remarks>
        /// <param name="groupResult"></param>
        private void FreeUpExtraAppliedCredits(GroupResult groupResult)
        {
            //exit if the plan or group are not satisified and exit if there were no extra credits or extra courses applied 
            if (groupResult.Explanations.Contains(GroupExplanation.PlannedSatisfied) || groupResult.Explanations.Contains(GroupExplanation.Satisfied))
            {
                if (!CoursesAppliedExceedMinCourses(groupResult) && !CreditsAppliedExceedMinCredits(groupResult))
                    return;
            }
            else
            {
                return;
            }

            /* Check to remove extra planned courses first, that way, we will release planned courses if possible before we release actual work.
             * For each planned course that was applied to this block go backwards, trying to honor the user's sort order */
            var plannedResults = groupResult.Results.Where(cc => cc.Result == Result.PlannedApplied && cc.Explanation != AcadResultExplanation.Extra).Reverse().ToList();
            foreach (var acadResult in plannedResults)
            {
                var result = DetermineIfCanReleaseCourse(groupResult, acadResult);
                if (result)
                {
                    acadResult.Result = Result.Related;
                }
            }

            /* When planned courses were used to satisfy the requirements, then we do not want to release actual work (credits). */
            if (!plannedResults.Where(cc => cc.Result == Result.PlannedApplied).Any())
            {
                /* check to remove extra applied actual courses 
                 * For each course that was applied to this block go backwards, trying to honor the user's sort order */
                var appliedResults = groupResult.Results.Where(cc => cc.Result == Result.Applied && cc.Explanation != AcadResultExplanation.Extra).Reverse().ToList();

                /* When courses used to satisfy the requirements are in progress, then we do not want to release actual work (credits). */
                if (appliedResults.Where(cc => cc.GetAcadCred().IsCompletedCredit == false).Any())
                    return;

                foreach (var acadResult in appliedResults)
                {
                    var result = DetermineIfCanReleaseCourse(groupResult, acadResult);
                    if (result)
                    {
                        acadResult.Result = Result.Related;
                    }
                }
            }
        }

        /// <summary>
        /// Used to determine if it's safe to release a given course 
        /// </summary>
        /// <remarks>
        /// we need to ensure that the act of releasing a course doesn't 
        /// inadvertently leave a previously satisfied requirement now unsatisfied.
        /// </remarks>
        /// <param name="groupResult"></param>
        /// <param name="acadResult"></param>
        /// <returns></returns>
        private bool DetermineIfCanReleaseCourse(GroupResult groupResult, AcadResult acadResult)
        {
            // check override credit
            /* Exit when the course is used in a requirement or evaluation override */
            string acadCredId = acadResult.GetAcadCredId();
            if (groupResult.ForceAppliedAcademicCreditIds.Contains(acadCredId))
                return false;

            decimal nonExtraApplied = groupResult.GetNonExtraAppliedCredits();
            decimal nonExtraPlannedApplied = groupResult.GetNonExtraPlannedAppliedCredits();

            // check 'take x credits'
            /* When there is a 'TAKE X CREDITS' phrase in use
            /* Exit when freeing up these credits would not leave enough credits to satisfy the block's "TAKE X CREDITS" phrase */
            if (MinCredits.HasValue && (nonExtraApplied + nonExtraPlannedApplied) >= MinCredits.Value)
            {
                // In-progress/completed courses should use adjusted credits
                // but planned courses should use credits
                var creditsToConsider = acadResult.GetAdjustedCredits();
                if (acadResult is CourseResult)
                {
                    creditsToConsider = acadResult.GetCredits();
                }
                if ((nonExtraApplied + nonExtraPlannedApplied) - creditsToConsider < MinCredits.Value)
                    return false;
            }

            //check 'min x courses'
            /* When there is a 'MIN X COURSES' phrase in use and this student.acad.cred record is a course
             * Exit when freeing up the record would not leave us with enough courses to satisfy the "MIN X COURSES" phrase */
            if (MinCourses.HasValue && (groupResult.CountNonExtraApplied() + groupResult.CountNonExtraPlannedApplied() - 1) < MinCourses.Value)
                return false;

            /* get all of the planned applied and applied academic credits excluding the current academic credit */
            var acadCredits = groupResult.GetNonExtraApplied().Where(r => r.GetAcadCredId() != acadResult.GetAcadCredId());
            var plannedCredits = groupResult.GetNonExtraPlannedApplied().Where(r => r.GetCourse() != acadResult.GetCourse());

            //check 'min x subjects'
            /* When there is a "MIN X SUBJECTS" phrase and this record has a subject
             * Check to see if there are enough subjects when this course is excluded
             * Exit when freeing up the record would not leave enough subjects to satisfy the "MIN X SUBJECTS" phrase. */
            if (MinSubjects.HasValue)
            {
                var subjects = acadCredits.Select(ac => ac.GetSubject()).Concat(plannedCredits.Select(pc => pc.GetSubject())).Distinct();
                if (subjects.Count() < MinSubjects.Value)
                    return false;
            }

            //check 'min x departments'
            /* If there was a "MIN X DEPARTMENTS" phrase and this student.acad.cred record has a deptartment
             * Check to see if there are enough departments when this course is excluded
             * Exit if freeing up the record would not leave us with enough departments to satisfy the "MIN X DEPARTMENTS" phrase. */
            if (MinDepartments.HasValue)
            {
                var departments = acadCredits.SelectMany(ac => ac.GetDepartments()).Concat(plannedCredits.SelectMany(pc => pc.GetDepartments())).Distinct();
                if (departments.Count() < MinDepartments.Value)
                    return false;
            }

            //check 'Min x credits per subject' and 'Min x courses per subject' - not supported 
            /* As per Colleague EVAL - we don't currently handle freeing up of credits when there's a "PER SUBJECT" type of phrase involved */
            if (MinCreditsPerSubject.HasValue)
            {
                return false;
            }

            //check 'Min x credits per dept' and 'Min x courses per dept' - not supported
            /*  As per Colleague EVAL - we don't currently handle freeing up of credits when there's a "PER DEPARTMENT" type of phrase involved */
            if (MinCreditsPerDepartment.HasValue)
            {
                return false;
            }

            return true;
        }

        private static bool IncreasesSubjectCount(GroupResult groupResult, AcadResult academicResult)
        {
            return !groupResult.GetAppliedSubjects().Contains(academicResult.GetSubject()) && !groupResult.GetPlannedAppliedSubjects().Contains(academicResult.GetSubject());
        }

        private static int IncreasesDeptCountBy(GroupResult gr, AcadResult ar)
        {
            int count = 0;

            foreach (var dept in ar.GetDepartments())
            {
                if (!gr.GetAppliedDepartments().Contains(dept) && !gr.GetPlannedAppliedDepartments().Contains(dept))
                {
                    count++;
                }
            }
            return count;
        }

        private bool AppliesToNeededSubject(GroupResult groupResult, AcadResult acadResult)
        {
            bool returnval = false;
            if (MinCreditsPerSubject.HasValue || MinCoursesPerSubject.HasValue)
            {
                foreach (var subj in groupResult.Group.FromSubjects)
                {
                    IEnumerable<AcadResult> credsThisSubj = groupResult.GetApplied().Where(ap => ap.GetSubject().Equals(subj)).AsEnumerable();
                    IEnumerable<AcadResult> plannedCredsThisSubj = groupResult.GetPlannedApplied().Where(ap => ap.GetSubject().Equals(subj)).AsEnumerable();
                    if (MinCoursesPerSubject.HasValue && (credsThisSubj.Count() + plannedCredsThisSubj.Count() < MinCoursesPerSubject.Value) ||
                        MinCreditsPerSubject.HasValue && (credsThisSubj.Select(cd => cd.GetCredits()).Sum() + plannedCredsThisSubj.Select(cd => cd.GetCredits()).Sum() < MinCreditsPerSubject.Value))
                    {
                        if (acadResult.GetSubject() == subj)
                        {
                            returnval = true;
                        }
                    }
                }
            }
            return returnval;
        }

        private bool AppliesToNeededDepartment(GroupResult groupResult, AcadResult acadResult)
        {
            bool returnval = false;
            if (MinCoursesPerDepartment.HasValue || MinCreditsPerDepartment.HasValue)
            {
                foreach (var dept in groupResult.Group.FromDepartments)
                {
                    IEnumerable<AcadResult> credsThisDept = groupResult.GetApplied().Where(ap => ap.GetDepartments().Contains(dept)).AsEnumerable();
                    IEnumerable<AcadResult> plannedCredsThisDept = groupResult.GetPlannedApplied().Where(ap => ap.GetDepartments().Contains(dept)).AsEnumerable();
                    if (MinCoursesPerDepartment.HasValue && (credsThisDept.Count() + plannedCredsThisDept.Count() < MinCoursesPerDepartment.Value) ||
                        MinCreditsPerDepartment.HasValue && (credsThisDept.Select(cd => cd.GetCredits()).Sum() + plannedCredsThisDept.Select(cd => cd.GetCredits()).Sum() < MinCreditsPerDepartment.Value))
                    {
                        if (acadResult.GetDepartments().Contains(dept))
                        {
                            returnval = true;
                        }
                    }
                }
            }
            return returnval;
        }

        private bool MinGpaNotMet(GroupResult groupResult)
        {
            if (MinGpa == null || groupResult.Gpa == null)
                return false;
            if (MinGpa > groupResult.Gpa)
                return true;
            return false;
        }

        private bool MinCreditsPerSubjectNotMet(GroupResult groupResult, bool includePlanned)
        {
            bool returnval = false;
            if (MinCreditsPerSubject.HasValue)
            {
                foreach (var subj in groupResult.Group.FromSubjects)
                {
                    IEnumerable<AcadResult> credsThisSubj = groupResult.GetApplied().Where(ap => ap.GetSubject().Equals(subj));
                    IEnumerable<AcadResult> plannedCredsThisSubj = groupResult.GetPlannedApplied().Where(ap => ap.GetSubject().Equals(subj));
                    var sumPlanned = includePlanned ? plannedCredsThisSubj.Sum(xx => xx.GetCredits()) : 0;
                    if (credsThisSubj.Sum(xx => xx.GetCredits()) + sumPlanned < MinCreditsPerSubject.Value)
                    {
                        returnval = true;
                    }
                }
            }
            return returnval;
        }

        private bool MinCreditsPerDepartmentNotMet(GroupResult groupResult, bool includePlanned)
        {
            bool returnval = false;
            if (MinCreditsPerDepartment.HasValue)
            {
                foreach (var dept in groupResult.Group.FromDepartments)
                {
                    IEnumerable<AcadResult> credsThisDept = groupResult.GetApplied().Where(ap => ap.GetDepartments().Contains(dept));
                    IEnumerable<AcadResult> plannedCredsThisDept = groupResult.GetPlannedApplied().Where(ap => ap.GetDepartments().Equals(dept));
                    var sumPlanned = includePlanned ? plannedCredsThisDept.Sum(xx => xx.GetCredits()) : 0;
                    if (credsThisDept.Sum(ctd => ctd.GetCredits()) + sumPlanned < MinCreditsPerDepartment.Value)
                    {
                        returnval = true;
                    }
                }
            }
            return returnval;
        }

        private bool MinCoursesPerSubjectNotMet(GroupResult groupResult, bool includePlanned)
        {
            bool returnval = false;
            if (MinCoursesPerSubject.HasValue)
            {
                foreach (var subj in groupResult.Group.FromSubjects)
                {
                    IEnumerable<AcadResult> credsThisSubj = groupResult.GetApplied().Where(ap => ap.GetSubject().Equals(subj));
                    IEnumerable<AcadResult> plannedCredsThisSubj = groupResult.GetPlannedApplied().Where(ap => ap.GetSubject().Equals(subj));
                    var sumPlanned = includePlanned ? plannedCredsThisSubj.Sum(xx => xx.GetCredits()) : 0;
                    if (credsThisSubj.Count() + sumPlanned < MinCoursesPerSubject.Value)
                    {
                        returnval = true;
                    }
                }
            }
            return returnval;
        }

        private bool MinCoursesPerDepartmentNotMet(GroupResult groupResult, bool includePlanned)
        {
            bool returnval = false;
            if (MinCoursesPerDepartment.HasValue)
            {
                foreach (var dept in groupResult.Group.FromDepartments)
                {
                    IEnumerable<AcadResult> credsThisDept = groupResult.GetApplied().Where(ap => ap.GetDepartments().Contains(dept));
                    IEnumerable<AcadResult> plannedCredsThisDept = groupResult.GetPlannedApplied().Where(ap => ap.GetDepartments().Equals(dept));
                    var sumPlanned = includePlanned ? plannedCredsThisDept.Sum(xx => xx.GetCredits()) : 0;
                    if (credsThisDept.Count() + sumPlanned < MinCoursesPerDepartment.Value)
                    {
                        returnval = true;
                    }
                }
            }
            return returnval;
        }

        private bool MinSubjectsNotMet(GroupResult groupResult, bool includePlanned)
        {
            if (includePlanned)
                // Get the unique list of planned and applied subjects and see if that count is less than the MinSubjects.Value
                return MinSubjects.HasValue && groupResult.GetAppliedSubjects().Union(groupResult.GetPlannedAppliedSubjects()).Distinct().Count() < MinSubjects.Value;
            else
                return MinSubjects.HasValue && groupResult.GetAppliedSubjects().Count() < MinSubjects.Value;
        }

        private bool MinDepartmentsNotMet(GroupResult groupResult, bool includePlanned)
        {
            if (includePlanned)
                // Get the unique list of planned and applied departments and see if that count is less than the MinDepartments.Value
                return MinDepartments.HasValue && groupResult.GetAppliedDepartments().Union(groupResult.GetPlannedAppliedDepartments()).Distinct().Count() < MinDepartments.Value;
            else
                return MinDepartments.HasValue && groupResult.GetAppliedDepartments().Count() < MinDepartments.Value;
        }

        private bool MinCreditsNotMet(GroupResult groupResult, bool includePlanned)
        {

            if (includePlanned)
                return MinCredits.HasValue && groupResult.GetAppliedCredits() + groupResult.GetPlannedAppliedCredits() < MinCredits.Value;
            else
                return MinCredits.HasValue && groupResult.GetAppliedCredits() < MinCredits.Value;

        }

        private bool MinCoursesNotMet(GroupResult groupResult, bool includePlanned)
        {
            if (includePlanned)
                return MinCourses.HasValue && groupResult.CountApplied() + groupResult.CountPlannedApplied() < MinCourses.Value;
            else
                return MinCourses.HasValue && groupResult.CountApplied() < MinCourses.Value;
        }

        private bool MinInstCreditsNotMet(GroupResult groupResult, bool includePlanned)
        {
            if (includePlanned)
                return MinInstitutionalCredits.HasValue && groupResult.GetAppliedInstCredits() + groupResult.GetPlannedAppliedInstCredits() < MinInstitutionalCredits.Value;
            else
                return MinInstitutionalCredits.HasValue && groupResult.GetAppliedInstCredits() < MinInstitutionalCredits.Value;
        }

        private static int CountApplied(ICollection<CreditResult> results)
        {
            return results.Where(cr => cr.Result == Result.Applied).Count();
        }

        public override List<RequirementRule> GetRules()
        {
            List<RequirementRule> grpRules = base.GetRules();
            if (MaxCoursesRule != null)
            {
                grpRules.Add(MaxCoursesRule);
            }
            if (MaxCreditsRule != null)
            {
                grpRules.Add(MaxCreditsRule);
            }
            return grpRules;
        }

        private bool CoursesAppliedExceedMinCourses(GroupResult groupResult)
        {
            return MinCourses.HasValue && groupResult.CountApplied() + groupResult.CountPlannedApplied() > MinCourses.Value;
        }

        private bool CreditsAppliedExceedMinCredits(GroupResult groupResult)
        {
            return MinCredits.HasValue && groupResult.GetAppliedCredits() + groupResult.GetPlannedAppliedCredits() > MinCredits.Value;
        }

        //**********************************************************************************************

        /// <summary>
        /// Performs the first check of a course or credit against a group.  Checks to see
        /// if the course is in the list of courses that the group requires, or is from the 
        /// right subject, department, or level.
        /// </summary>
        private Result CheckCredit(AcadResult acadResult, List<Course> equatedCourses)
        {
            
            Result result = Result.Untested;
            result = CheckCreditsSteps(acadResult, equatedCourses);
            //if resut is related then it means have passed through all the clauses in syntax then perform last step to verify if it is repeated course and if it will be Replaced in progress because
            //there is another repeated course which is considered as possible replacement. 
            if(result==Result.Related)
            {
                result = CheckCreditForReplaceInProgressStep5(acadResult);

            }
            return result;
        }
        /// <summary>
        /// Going to pass academic credit and its equated courses for checks on cluases like Courses, From Courses, From Subjects, From Departments, ButNots , rules and exceptions
        /// </summary>
        /// <param name="acadResult"></param>
        /// <param name="equatedCourses"></param>
        /// <returns></returns>
        private Result CheckCreditsSteps(AcadResult acadResult, List<Course> equatedCourses)
        {
            Result result = Result.Untested;
            //check for FromCourseException, EligibilityRules, Courses list, FromCourses list, rules. If the course or an equated course is in FromCoursesEXception then it is considered as Related and returns.
            //there is no need to further look into other clauses in syntax
            //If academic credit fails rules or is not in courses or from courses list OR acadmeic credit's equated courses re not in Courses or FromCourses list then no need to go further to look
            //other clauses in syntax
            result = CheckCreditStep1(acadResult);
            if(result==Result.RelatedFromCourseException)
            {
                return Result.Related;
            }
            if (result != Result.Related)
                return result;

            //you will reach here when a  course on academic credit or any of the equated course have passed through step 1(as described above) such as result is marked Related. 
            //Even though result is marked as Related, it is not truely related because there could be other clauses in syntax

            //Go to step 2 of checks- which is FromDeparments, FromSubjects, FromLevels
            if (FromDepartments.Any() || FromSubjects.Any() || FromLevels.Any())
            {
                //reset the result back to untested. 
                result = Result.Untested;
                //Check the course on  current academic credit belongs to from department or from subject or from level clause. 
                result = CheckCreditFromClauseOnAcadResultStep2(acadResult);
                //If result is related then it means course on acad credit satisfy or passes from clause. In case it has not then take each equated course and validates if each individual equated course
                //belongs to From subjects, From deaprtments, From levels clause
                if (result != Result.Related)
                {

                    foreach (Course course in equatedCourses)
                    {
                        result = Result.Untested;
                        result = CheckCreditFromClauseOnEquatedCourseStep3(course);
                        //result is related means one of the equated course belongs to from subject/from dept/from level list then no need to further look into other equated courses
                        if (result == Result.Related)
                        {
                            break;
                        }
                    }
                }
            }
            //if acad credit or equated courses belongs to Courses, FromCourses but have failed to be in FromSubjects, FromDepatrments, FromLevels then jst return the result
            if (result != Result.Related)
                return result;

            //We will reach here if we have passed through step 1 or step 2 or step 3
            if (ButNotCourseLevels.Any() || ButNotCourses.Any() || ButNotDepartments.Any() || ButNotSubjects.Any())
            {
                //reset the result so that we can check on ButNot for course levels, courses, departments, subjects. courses, depts, subjects are also checked on equated courses
                result = Result.Untested;
                result = CheckCreditButNotStep4(acadResult, equatedCourses);
            }
            return result;
        }

        /// <summary>
        /// This checks course on academic credit for FromCoursesException (Exception to allow additional courses), Eligibility Rules, 
        /// Coursesm From Courses, Rules
        /// Equated course Ids are also looked for FromCoursesException, Courses and Fro courses clauses
        /// </summary>
        /// <param name="acadResult"></param>
        /// <returns></returns>
        private Result CheckCreditStep1(AcadResult acadResult)
        {
            // Check up the tree for rules to evaluate
            List<RequirementRule> EligibilityRules = new List<RequirementRule>();
            if (this.SubRequirement.AcademicCreditRules.Count > 0) { EligibilityRules.AddRange(this.SubRequirement.AcademicCreditRules); }
            if (this.SubRequirement.Requirement.AcademicCreditRules.Count > 0) { EligibilityRules.AddRange(this.SubRequirement.Requirement.AcademicCreditRules); }
            if (this.SubRequirement.Requirement.ProgramRequirements != null)
            {
                if (this.SubRequirement.Requirement.ProgramRequirements.ActivityEligibilityRules.Count > 0)
                {
                    EligibilityRules.AddRange(this.SubRequirement.Requirement.ProgramRequirements.ActivityEligibilityRules);
                }
            }

            // If an exception has been made to allow a certain course, then allow that course
            // and don't worry about the rest of the group criteria - including the eligibility rules.
            if (FromCoursesException.Count() > 0 && acadResult.GetCourse() != null)
            {
                if ((FromCoursesException.Contains(acadResult.GetCourse().Id)) ||
                    (FromCoursesException.Intersect(acadResult.GetCourse().EquatedCourseIds).Count() > 0))
                {
                    return Result.RelatedFromCourseException;
                }
            }

            // Now that the exceptions adding specific courses have been evaluated,  check any high level eligibility rules to see
            // if the academic credit qualifies for the rest of the syntax. Groups that are block replacements should not
            // evaluate the eligibility rules.
            if (EligibilityRules.Any() && !IsBlockReplacement)
            {

                foreach (var rule in EligibilityRules)
                {
                    if (!rule.Passes(acadResult, true))
                    {
                        return Result.RuleFailed;
                    }
                }
            }

            // If this is a course, evaluate only against courses 
            // Course equivalencies apply to Courses and FromCourses lists only.  EVAL has worked both ways
            // over the years, and half of the clients hate it one way, half the other.  In the opinion
            // of this coder, equivalencies are specific, targeted to allow one course to substitute forC:\Ellucian\VS2015\ColleagueWebAPI\Dev_Team_Student\Source\Ellucian.Colleague.Api\Ellucian.Colleague.Api.Client\Exceptions\
            // another, not statements of equivalence on a general level.

            // Clients have asked that My Progress replicate the equates as honored by EVAL, which means
            // that the subject, departments and course level of equated courses are also considered here
            // (as crazy as that seems).
            if (Courses.Count > 0)
            {
                if (acadResult.GetCourse() != null)
                {
                    if ((!Courses.Contains(acadResult.GetCourse().Id)) &&
                        (Courses.Intersect(acadResult.GetCourse().EquatedCourseIds).Count() == 0))
                    {
                        return Result.NotInCoursesList;
                    }
                }
                else
                {
                    return Result.NotInCoursesList;
                }
            }


            if (FromCourses.Count > 0)
            {
                if (acadResult.GetCourse() != null)
                {
                    if ((!FromCourses.Contains(acadResult.GetCourse().Id)) &&
                        (FromCourses.Intersect(acadResult.GetCourse().EquatedCourseIds).Count() == 0))
                    {
                        return Result.NotInFromCoursesList;
                    }
                }
                else
                {
                    return Result.NotInFromCoursesList;
                }
            }

            // Evaluate group rules AFTER the above logic, which may relate an item otherwise
            // excluded by the rules based on an exception.
            foreach (var rule in this.AcademicCreditRules)
            {
                if (!rule.Passes(acadResult, false))
                {
                    return Result.RuleFailed;
                }
            }


            return Result.Related;
        }
        /// <summary>
        /// This checks if course on academic credit belongs to From Daprtments list or From Levels list or From Subjects list
        /// If a course passes through all the from clauses then it is marked as Related course
        /// </summary>
        /// <param name="acadResult"></param>
        /// <returns></returns>
        private Result CheckCreditFromClauseOnAcadResultStep2(AcadResult acadResult)
        {

            // Check the departments listed for the academic result's course but also accept if one of the equated courses has one of these departments.
            if (FromDepartments.Count > 0)
            {
                if (FromDepartments.Intersect(acadResult.GetDepartments()).Count() == 0 )
                {
                    return Result.FromWrongDepartment;
                }
            }

            // Check the course levels listed for the academic result's course but also accept if any equated course has one of these levels
            if (FromLevels.Count > 0)
            {
                if (FromLevels.Intersect(acadResult.GetCourseLevels()).Count() == 0)
                {
                    return Result.FromWrongLevel;
                }
            }

            // Check FromSubjects against the subject in the academic result's course but also accept if any equated course has one of these subjects
            if (FromSubjects.Count > 0)
            {
                if (!FromSubjects.Contains(acadResult.GetSubject()) )
                {
                    return Result.FromWrongSubject;
                }
            }

           

            return Result.Related;
        }
        /// <summary>
        /// This checks if an equated belongs to From Daprtments list or From Levels list or From Subjects list
        /// If a course passes through all the from clauses then it is marked as Related course
        /// </summary>
        /// <param name="acadResult"></param>
        /// <returns></returns>
        private Result CheckCreditFromClauseOnEquatedCourseStep3(Course equatedCourse)
        {
                // Check the departments listed for the academic result's course but also accept if one of the equated courses has one of these departments.
                if (FromDepartments.Count > 0)
                {
                    if (FromDepartments.Intersect(equatedCourse.DepartmentCodes).Count() == 0)
                    {
                        return Result.FromWrongDepartment;
                    }
                }

                // Check the course levels listed for the academic result's course but also accept if any equated course has one of these levels
                if (FromLevels.Count > 0)
                {
                    if ( FromLevels.Intersect(equatedCourse.CourseLevelCodes).Count() == 0)
                    {
                        return Result.FromWrongLevel;
                    }
                }

                // Check FromSubjects against the subject in the academic result's course but also accept if any equated course has one of these subjects
                if (FromSubjects.Count > 0)
                {
                    if (!FromSubjects.Contains(equatedCourse.SubjectCode))
                    {
                        return Result.FromWrongSubject;
                    }
                }


            return Result.Related;
        }
        /// <summary>
        /// This step checks for ButNot clause on academic credits course and all the equated courses
        /// </summary>
        /// <param name="acadResult"></param>
        /// <param name="equatedCourses"></param>
        /// <returns></returns>
        private Result CheckCreditButNotStep4(AcadResult acadResult, List<Course> equatedCourses)
        {


            // If a NOT course level is found in the course of the academic result, exclude this item
            // Note that a NOT course level found in any *equated* courses for the course of the academic result are not considered
            if (ButNotCourseLevels.Intersect(acadResult.GetCourseLevels()).Count() > 0)
            {
                return Result.LevelExcluded;
            }

            if (acadResult.GetCourse() != null)
            {
                if ((ButNotCourses.Contains(acadResult.GetCourse().Id)) ||
                    (ButNotCourses.Intersect(acadResult.GetCourse().EquatedCourseIds).Count() != 0))
                {
                    return Result.CourseExcluded;
                }
            }

            // if a NOT department is found in the course of the academic result, exclude this item. We are not going to look into equated courses
            if (ButNotDepartments.Intersect(acadResult.GetDepartments()).Count() > 0)
            {
                return Result.DepartmentExcluded;
            }

            // If a NOT subject is found in the course of the academic result, exclude this item. We are not going to look into equated courses.
            if (ButNotSubjects.Contains(acadResult.GetSubject()))
            {
                return Result.SubjectExcluded;
            }

            return Result.Related;
        }
        /// <summary>
        /// Validate if academic credit is repeat such as this credit or planned course in particular was possible replace in progress
        /// </summary>
        /// <param name="acadResult"></param>
        /// <returns></returns>
        private Result CheckCreditForReplaceInProgressStep5(AcadResult acadResult)
        {
            // Check up the tree for rules to evaluate


            // Repeat - if credit has replaceInprogress flag then don't count that acad cred record for group evaluation
            if (acadResult != null && acadResult.GetAcadCred() != null && acadResult.GetAcadCred().ReplacedStatus == ReplacedStatus.ReplaceInProgress)
            {
                return Result.ReplaceInProgress;
            }
            //If planned course is replaced in progress then do not pass it for group evaluation
            if (acadResult != null && acadResult.GetAcadCred() == null && acadResult.GetCourse() != null && ((CourseResult)acadResult).PlannedCourse.ReplacedStatus == ReplacedStatus.ReplaceInProgress)
            {
                return Result.ReplaceInProgress;
            }


            return Result.Related;
        }

      
        public override string ToString()
        {
            return Id + " " + Code;
        }

        /// <summary>
        /// Flag indicating whether or not this block is used exclusively to convey print text
        /// Example: TAKE 0 CREDITS
        /// </summary>
        public bool OnlyConveysPrintText
        {
            get
            {
                return (GroupType == GroupType.TakeCredits && MinCredits == 0m)
                    && (AcademicCreditRules == null || !AcademicCreditRules.Any())
                    && (CourseBasedRules == null || !AcademicCreditRules.Any())
                    && !MaxCourses.HasValue
                    && (MaxCoursesAtLevels == null || MaxCoursesAtLevels.MaxCourses == 0m)
                    && !MaxCoursesPerDepartment.HasValue
                    && !MaxCoursesPerRule.HasValue
                    && !MaxCoursesPerSubject.HasValue
                    && MaxCoursesRule == null
                    && !MaxCredits.HasValue
                    && (MaxCreditsAtLevels == null || MaxCreditsAtLevels.MaxCredit == 0m)
                    && !MaxCreditsPerCourse.HasValue
                    && !MaxCreditsPerDepartment.HasValue
                    && !MaxCreditsPerRule.HasValue
                    && !MaxCreditsPerSubject.HasValue
                    && MaxCreditsRule == null
                    && !MaxDepartments.HasValue
                    && !MaxSubjects.HasValue
                    && !MinCourses.HasValue
                    && !MinCoursesPerDepartment.HasValue
                    && !MinCoursesPerSubject.HasValue
                    && !MinCreditsPerCourse.HasValue
                    && !MinCreditsPerDepartment.HasValue
                    && !MinCreditsPerSubject.HasValue
                    && !MinDepartments.HasValue
                    && !MinGpa.HasValue
                    && !MinInstitutionalCredits.HasValue
                    && !MinSubjects.HasValue
                    && (ButNotCourseLevels == null || !ButNotCourseLevels.Any())
                    && (ButNotCourses == null || !ButNotCourses.Any())
                    && (ButNotDepartments == null || !ButNotDepartments.Any())
                    && (ButNotSubjects == null || !ButNotSubjects.Any())
                    && (Exclusions == null || !Exclusions.Any())
                    && (FromCourses == null || !FromCourses.Any())
                    && (FromCoursesException == null || !FromCoursesException.Any())
                    && (FromDepartments == null || !FromDepartments.Any())
                    && (FromLevels == null || !FromLevels.Any())
                    && (FromSubjects == null || !FromSubjects.Any())
                    && (Courses == null || !Courses.Any());
            }
        }
    }
}

