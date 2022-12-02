// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Services
{
    public class ProgramEvaluator
    {
        private List<Requirement> Requirements;
        private List<Requirement> AdditionalRequirements;
        private List<GroupResult> GroupResults;
        private List<AcadResult> MasterResults;
        private Dictionary<string, List<AcadResult>> SortedMasterResultsBySortSpecification;
        private IEnumerable<PlannedCredit> PlannedCourses;
        private List<Override> Overrides;
        private ProgramEvaluation ProgramResult;
        private List<AcademicCredit> Credits;
        private List<SubrequirementResult> AllSubrequirementResults;
        private ProgramRequirements ProgramRequirements;
        private StudentProgram StudentProgram;
        private IDictionary<AcadResult, Dictionary<string, List<string>>> UseTracker;
        private List<string> SubRequirementsToSkip;
        private List<string> GroupsToSkip;
        private List<string> debuglist;
        private List<string> constructorDebug;
        private ILogger logger;
        private IEnumerable<Course> Courses;
        private Dictionary<string, RequirementExclusionTable> RequirementExclusionTable;
        private bool ShowRelatedCourses { get; set; }
        /// <summary>
        /// The purpose is to disable lookahead optimization feature in Self-Service MyProgress. 
          // In MyProgress if a group statement with MinCourses and MinCredits also have MinGPA defined then while applying the course that satisfy MinCourses or MinCredits but lowers the MinGPA or does not reach to MinGPA threshold
          //then that course is skipped but instead  the next course is picked from the list and applies it only if it would cross or reach to MinGPA.
          // If the value is true then it will work as EVAL does where it will apply the current course and will not see if applying current course will lower the
          //group level GPA to put it under MinGPA or not reach threshold of MinGPA instead it will just apply the course in a list as long as it doesn't go beyond MinCredits or MinCourses. 
        /// </summary>
        private bool DisableLookAheadOptimization { get; set; }
        private List<AcademicCredit> CreditsExcludedFromTranscriptGrouping { get;  set; }

        // Not sure this belongs here, but it has to get set in the constructor
        // then passed to the evaluation because it doesn't really belong in the 
        // ProgramRequirements tree - it's specific to a particular student.

        private string OverallCreditsModificationMessage;
        private string InstitutionalCreditsModificationMessage;
        private string OverallGpaModificationMessage;
        private string InstitutionalGpaModificationMessage;

        /// <summary>
        /// Constructor used for full program evaluation.
        /// </summary>
        /// <param name="studentprogram"></param>
        /// <param name="programrequirements"></param>
        /// <param name="additionalrequirements"></param>
        /// <param name="credits"></param>
        /// <param name="overrides"></param>
        public ProgramEvaluator(StudentProgram studentprogram, ProgramRequirements programrequirements, List<Requirement> additionalrequirements,
                                IEnumerable<AcademicCredit> credits, IEnumerable<PlannedCredit> plannedCourses, IEnumerable<RuleResult> ruleResults, List<Override> overrides,  IEnumerable<Course> courses, ILogger logger)
        {
            if (studentprogram == null) { throw new ArgumentNullException("studentprogram"); }
            if (programrequirements == null) { throw new ArgumentNullException("programrequirements"); }
            if (additionalrequirements == null) { throw new ArgumentNullException("additionalrequirements"); }
            if (credits == null) { throw new ArgumentNullException("credits"); }
            if (plannedCourses == null) { throw new ArgumentNullException("plannedCourses"); }
            if (ruleResults == null) { throw new ArgumentNullException("ruleResults"); }
            if (overrides == null) { throw new ArgumentNullException("overrides"); }
            if (courses == null) { throw new ArgumentNullException("courses"); }

            Overrides = overrides;
            Credits = credits.ToList();
            PlannedCourses = plannedCourses;
            Courses = courses;
            RequirementExclusionTable = new Dictionary<string, RequirementExclusionTable>();
            AdditionalRequirements = CopyRequirements(additionalrequirements);

            ProgramRequirements = CopyProgramRequirements(programrequirements);

            StudentProgram = studentprogram;

            Requirements = new List<Requirement>();

            SubRequirementsToSkip = new List<string>();
            GroupsToSkip = new List<string>();
            constructorDebug = new List<string>();

            this.logger = logger;

            // Apply any changes to the Requirements (Colleague "Exceptions" - Domain "Modifications")
            if (StudentProgram != null)
            {
                if (StudentProgram.RequirementModifications.Count() > 0)
                {
                    foreach (var reqmod in StudentProgram.RequirementModifications)
                    {
                        constructorDebug.Add("excp " + reqmod.GetType().ToString() + "for block " + reqmod.blockId);
                        try
                        {
                            // Modify the tree
                            reqmod.Modify(ProgramRequirements, AdditionalRequirements);

                            // Check for program-level messages
                            if ((reqmod.GetType() == typeof(CreditModification)) && (string.IsNullOrEmpty(reqmod.blockId)))
                            {
                                OverallCreditsModificationMessage = reqmod.modificationMessage;
                            }
                            if ((reqmod.GetType() == typeof(GpaModification)) && (string.IsNullOrEmpty(reqmod.blockId)))
                            {
                                OverallGpaModificationMessage = reqmod.modificationMessage;
                            }
                            if ((reqmod.GetType() == typeof(InstitutionalCreditModification)) && (string.IsNullOrEmpty(reqmod.blockId)))
                            {
                                InstitutionalCreditsModificationMessage = reqmod.modificationMessage;
                            }

                            if (reqmod.GetType() == typeof(InstitutionalGpaModification))
                            {
                                InstitutionalGpaModificationMessage = reqmod.modificationMessage;
                            }

                        }
                        catch (Exception)
                        {
                            constructorDebug.Add("Program Modification returned an exception.  Most likely '" + reqmod.blockId +
                                                 "' is not a valid block id within this set of program requirements.");
                        }
                    }
                }
            }

            // Get the Requirements list from the ProgramRequirements object and additionals passed in
            // (Program requirements and additional requirements will not be present if only a single 
            // requirement is being evaluated instead of the whole program.)
            if (ProgramRequirements != null)
            {
                //ProgramResult.ProgramRequirements = ProgramRequirements;

                Requirements.AddRange(ProgramRequirements.Requirements);
            }
            if (AdditionalRequirements != null)
            {
                Requirements.AddRange(AdditionalRequirements);
            }

            StoreRuleResultsInCopiedTree(ruleResults);
            //This will build exclusion table to identify which requirment is excludes or excluded by other requirements
            BuildRequirementExclusionTable();
        }

           
        public ProgramEvaluator(StudentProgram studentprogram, ProgramRequirements programrequirements, List<Requirement> additionalrequirements,
                               IEnumerable<AcademicCredit> credits, IEnumerable<PlannedCredit> plannedCourses, IEnumerable<RuleResult> ruleResults, List<Override> overrides, List<AcademicCredit> creditsExcludedFromTranscriptGrouping, IEnumerable<Course> courses, DegreeAuditParameters degreeAuditParameters, ILogger logger):
            this( studentprogram,  programrequirements,  additionalrequirements,credits, plannedCourses,  ruleResults,  overrides,courses, logger)
        {
            CreditsExcludedFromTranscriptGrouping = creditsExcludedFromTranscriptGrouping;
            if(degreeAuditParameters!=null) 
            {
                if (degreeAuditParameters.ShowRelatedCourses == true)
                {
                    ShowRelatedCourses = true;
                }
                if (degreeAuditParameters.DisableLookAheadOptimization == true)
                {
                    DisableLookAheadOptimization = true;
                }
            }
            
        }

        /// <summary>
        /// Constructor used for evaluation of a single requirement, such as a prerequisite.
        /// </summary>
        /// <param name="requirements">List containing requirement "copied" from prerequisite</param>
        /// <param name="credits">May not be null, but an empty list allowed</param>
        /// <param name="plannedTermCourses">May not be null, but an empty list allowed</param>
        public ProgramEvaluator(List<Requirement> requirements, List<AcademicCredit> credits, IEnumerable<PlannedCredit> plannedCourses, IEnumerable<RuleResult> ruleResults, IEnumerable<Course> courses)
        {
            if (requirements == null || requirements.Count() == 0) { throw new ArgumentNullException("requirements"); }
            if (credits == null) { throw new ArgumentNullException("credits"); }
            if (plannedCourses == null) { throw new ArgumentNullException("plannedCourses"); }
            if (courses == null) { throw new ArgumentNullException("courses"); }
            RequirementExclusionTable = new Dictionary<string, RequirementExclusionTable>();

            Requirements = CopyRequirements(requirements, null);

            // The following items are not used but are required by Evaluate
            StudentProgram = null;
            ProgramRequirements = null;
            AdditionalRequirements = null;

            // Initialize in order to avoid null reference exceptions
            constructorDebug = new List<string>();

            Overrides = new List<Override>();
            Credits = credits;
            PlannedCourses = plannedCourses;
            Courses = courses;

            SubRequirementsToSkip = new List<string>();
            GroupsToSkip = new List<string>();

            StoreRuleResultsInCopiedTree(ruleResults);
            //This will build exclusion table to identify which requirment is excludes or excluded by other requirements
            BuildRequirementExclusionTable();

        }

        private void StoreRuleResultsInCopiedTree(IEnumerable<RuleResult> results)
        {
            if (ProgramRequirements != null)
            {
                var allRules = ProgramRequirements.GetAllRules();
                foreach (var rule in allRules)
                {
                    var matchingResults = results.Where(result => result.RuleId == rule.Id).ToList();
                    foreach (var matchingResult in matchingResults)
                    {
                        rule.SetAnswer(matchingResult.Passed, matchingResult.Context);
                    }
                }
            }

            foreach (var requirement in Requirements)
            {
                var allRules = requirement.GetAllRules();
                foreach (var rule in allRules)
                {
                    var matchingResults = results.Where(result => result.RuleId == rule.Id).ToList();
                    foreach (var matchingResult in matchingResults)
                    {
                        rule.SetAnswer(matchingResult.Passed, matchingResult.Context);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates the requirements against the credits and planned courses.
        /// </summary>
        /// <param name="sortedCreditsDict">Dictionary of sorted academic credits, keyed by their sort specification ID</param>
        public ProgramEvaluation Evaluate(Dictionary<string, List<AcademicCredit>> sortedCreditsDict = null)
        {
            GroupResults = new List<GroupResult>();
            AllSubrequirementResults = new List<SubrequirementResult>();

            MasterResults = ConstructResultSet(Credits, PlannedCourses);

            UseTracker = ConstructUseTracker(MasterResults);

            debuglist = new List<string>();

            if (SubRequirementsToSkip.Count + GroupsToSkip.Count > 0)
            {
                debuglist.Add("Beginning optimized eval run");
            }

            // Student program is not always passed, such as when only a single requirement is being evaluated
            if (StudentProgram != null)
            {
                ProgramResult = new ProgramEvaluation(Credits, StudentProgram.ProgramCode, StudentProgram.CatalogCode);
            }
            else
            {
                ProgramResult = new ProgramEvaluation(Credits);
            }

            ProgramResult.OverallCreditsModificationMessage = OverallCreditsModificationMessage;
            ProgramResult.InstitutionalCreditsModificationMessage = InstitutionalCreditsModificationMessage;
            ProgramResult.OverallGpaModificationMessage = OverallGpaModificationMessage;
            ProgramResult.InstitutionalGpaModificationMessage = InstitutionalGpaModificationMessage;

            ProgramResult.AllPlannedCredits = PlannedCourses.ToList();

            if (ProgramRequirements != null)
            {
                ProgramResult.ProgramRequirements = ProgramRequirements;
            }
            if(AdditionalRequirements!=null)
            {
                ProgramResult.AdditionalRequirements = AdditionalRequirements;
            }

            ICollection<Group> grps = new List<Group>();
            IEnumerable<Group> sortgrps = new List<Group>();

            // unroll these into the matrix
            foreach (var req in Requirements)
            {
                foreach (var sub in req.SubRequirements)
                {
                    foreach (var grp in sub.Groups)
                    {
                        grps.Add(grp);
                    }
                }
            }
            // Sort the matrix - Sort groups by requirement type WITHIN group type. 
            sortgrps = grps.OrderBy(xx => xx.GroupTypeEvalSequence)                                  // Primarily by group block type
                           .ThenBy(xx => xx.SubRequirement.Requirement.RequirementType.Priority)     // Then by requirement type priority within group block
                           .ToList();

            // Evaluate the Groups
            if (sortedCreditsDict != null)
            {
                SortedMasterResultsBySortSpecification = new Dictionary<string,List<AcadResult>>();

            }
            foreach (Group g in sortgrps)
            {
                List<AcadResult> masterResultForInListOrder = new List<AcadResult>();
                if (!string.IsNullOrEmpty(g.SortSpecificationId) && sortedCreditsDict != null && sortedCreditsDict.ContainsKey(g.SortSpecificationId) && sortedCreditsDict[g.SortSpecificationId] != null && !SortedMasterResultsBySortSpecification.ContainsKey(g.SortSpecificationId))
                {
                    SortedMasterResultsBySortSpecification.Add(g.SortSpecificationId, ConstructResultSet(sortedCreditsDict[g.SortSpecificationId], PlannedCourses, false));
                }
                //if group have IN.LIST.ORDER and syntax have FROM COURSES -- This will cover Type 31 and type 32 syntax like Take x credits from <courses> or Take x courses from <courses>
                if(g.InListOrder && g.FromCourses.Any())
                {
                    //if academic credits were sorted based upon sortspecification or DEFAULT was provided
                    if(SortedMasterResultsBySortSpecification!=null && SortedMasterResultsBySortSpecification.ContainsKey(g.SortSpecificationId))
                    {
                        //call 2nd level of sorting which will re-sort on basis of courses listed in from courses
                        masterResultForInListOrder = SortOnBasisOfInListOrder(SortedMasterResultsBySortSpecification[g.SortSpecificationId], g.FromCourses);
                    }
                    else
                    {
                        //if academic credits were sorted as default
                        masterResultForInListOrder = SortOnBasisOfInListOrder(MasterResults, g.FromCourses);
                    }
                }

                // If the Requirement or Subrequirement to which this group belongs is already satisfied, 
                // we will not want to evaluate this group.  Evaluate the Requirement and Subrequirement with
                // the group results we have so far to make sure they are not yet satisfied.

                bool skipgroup = false;

                // if this is an optimized pass, check for the group or subrequirement to be in the skip list
                if (GroupsToSkip.Contains(g.Id))
                {
                    debuglist.Add("Optimizer says to skip group '" + g.Id + "'");
                    skipgroup = true;
                }
                if (SubRequirementsToSkip.Contains(g.SubRequirement.Id))
                {
                    debuglist.Add("Optimizer says to skip subrequirement '" + g.SubRequirement.Id + "' so skip group " + g.Id);
                    skipgroup = true;
                }

                // Make sure this Subrequirement is not already satisfied
                List<GroupResult> currentGroupResultsThisSubr = GroupResults.Where(grr => grr.Group.SubRequirement.Id == g.SubRequirement.Id).ToList();

                SubrequirementResult thisSubResult = g.SubRequirement.Evaluate(currentGroupResultsThisSubr);

                // Make sure this Requirement is not already satisfied
                List<SubrequirementResult> subResults = new List<SubrequirementResult>();
                foreach (var sub in g.SubRequirement.Requirement.SubRequirements)
                {
                    List<GroupResult> currentGroupResultsThisReqSubr = GroupResults.Where(grr => grr.Group.SubRequirement.Id == sub.Id).ToList();

                    SubrequirementResult thisReqSubResult = sub.Evaluate(currentGroupResultsThisReqSubr);

                    subResults.Add(thisReqSubResult);
                }
                RequirementResult thisReqResult = g.SubRequirement.Requirement.Evaluate(subResults);

                if (thisReqResult.IsSatisfied()) { skipgroup = true; }


                // Debug to check the order in which groups are evaluated
                debuglist.Add("Evaling Group: " + g.Id + " GroupType: " + g.GroupType.ToString() +
                    " (GroupTypeSeq: " + g.GroupTypeEvalSequence.ToString() + ")" +
                    " Req: " + g.SubRequirement.Requirement.Code +
                    " ReqType: " + g.SubRequirement.Requirement.RequirementType.Code +
                    " (ReqTypeSeq: " + g.SubRequirement.Requirement.RequirementType.Priority.ToString() + ")" +
                    " Subreq: " + g.SubRequirement.Code);

                // Check for overrides to evaluate with the group
                List<Override> OverridesThisGroup = new List<Override>();
                if (Overrides.Count() > 0)
                {
                    if (Overrides.Where(ovr => ovr.GroupId == g.Id).Count() > 0)
                    {
                        OverridesThisGroup.AddRange(Overrides.Where(ovr => ovr.GroupId == g.Id).ToList());
                    }
                }

                // Select Academic Credits and Planned Courses to evaluate against the group
                List<AcadResult> AGCsToEvalThisGroup = new List<AcadResult>();

                //send list of credits and planned courses for evaluation after those were re-sorted due to IN.LIST.ORDER
                if(masterResultForInListOrder!=null && masterResultForInListOrder.Any() && g.InListOrder)
                {
                    AGCsToEvalThisGroup = ConstructResultSet(masterResultForInListOrder, g, UseTracker, OverridesThisGroup);

                }
                //if not IN.LIST.ORDER then maybe sortspecification
                else if (!string.IsNullOrEmpty(g.SortSpecificationId) && SortedMasterResultsBySortSpecification != null && SortedMasterResultsBySortSpecification.ContainsKey(g.SortSpecificationId))
                {
                    var sortedMasterResultsForSpec = SortedMasterResultsBySortSpecification[g.SortSpecificationId];
                    AGCsToEvalThisGroup = ConstructResultSet(sortedMasterResultsForSpec, g, UseTracker, OverridesThisGroup );
                }
                //if not IN.LIST.ORDER or sortspecification then utilize default order of master list
                else
                {
                    AGCsToEvalThisGroup = ConstructResultSet(MasterResults, g, UseTracker, OverridesThisGroup);
                }

                GroupResult GroupResult;
                GroupResult = g.Evaluate(AGCsToEvalThisGroup, OverridesThisGroup, Courses, CreditsExcludedFromTranscriptGrouping, ShowRelatedCourses,DisableLookAheadOptimization, skipgroup);

                // Summarize the group's completion in CompletionStatus and PlanningStatus fields

                if (GroupResult.IsSatisfied())
                {
                    //This is to identify all the courses that are applied which are not due to override and then identify if the group is completed or not. 
                    if (g.Courses.Count > 0 && GroupResult.ForceAppliedAcademicCreditIds.Count > 0)
                    {
                        // If no acad results in the group, then it was waived
                        if (GroupResult.GetApplied().Count() == 0)
                        {
                            GroupResult.CompletionStatus = CompletionStatus.Waived;
                        }
                        else if (GroupResult.GetNonOverrideApplied().Count() == 0)
                        {
                            // Every AcadResult.GetAcadCred() in the bunch returned null, so these are all
                            // planned courses and the requirement is not actually started.
                            GroupResult.CompletionStatus = CompletionStatus.NotStarted;
                        }
                        else if (GroupResult.GetNonOverrideApplied().Where(aap => (!aap.GetAcadCred().IsCompletedCredit)).Count() > 0)
                        {
                            // At least one applied result is from a planned course, or from a credit that
                            // is not complete so the group is not actually factually complete
                            GroupResult.CompletionStatus = CompletionStatus.PartiallyCompleted;
                        }

                        else
                        {
                            GroupResult.CompletionStatus = CompletionStatus.Completed;
                        }

                    }
                    else
                    {
                        // If no acad results in the group, then it was waived
                        if (GroupResult.GetApplied().Count() == 0)
                        {
                            GroupResult.CompletionStatus = CompletionStatus.Waived;
                        }
                        else if (GroupResult.GetApplied().Where(ap => ap.GetAcadCred() == null).Count() == GroupResult.GetApplied().Count())
                        {
                            // Every AcadResult.GetAcadCred() in the bunch returned null, so these are all
                            // planned courses and the requirement is not actually started.
                            GroupResult.CompletionStatus = CompletionStatus.NotStarted;
                        }
                        else if ((GroupResult.GetNonExtraApplied().Where(ap => ap.GetAcadCred() == null).Count() > 0) ||
                                  (GroupResult.GetNonExtraApplied().Where(ap => ap.GetAcadCred() != null).Where(aap => (!aap.GetAcadCred().IsCompletedCredit)).Count() > 0))
                        {
                            // At least one applied result is from a planned course, or from a credit that
                            // is not complete so the group is not actually factually complete
                            GroupResult.CompletionStatus = CompletionStatus.PartiallyCompleted;
                        }

                        else
                        {
                            GroupResult.CompletionStatus = CompletionStatus.Completed;
                        }
                    }
                }
                else // (!GroupResult.IsSatisfied())
                {
                    if (GroupResult.GetCompletedCredits() == 0m)
                    {
                        GroupResult.CompletionStatus = CompletionStatus.NotStarted;
                    }
                    else
                    {
                        GroupResult.CompletionStatus = CompletionStatus.PartiallyCompleted;
                    }
                }

                // Planning status
                if (GroupResult.IsSatisfied())
                {
                    // For now, treating groups satisfied by partial planning, partial credit as "completely planned."
                    GroupResult.PlanningStatus = PlanningStatus.CompletelyPlanned;
                }
                else // (!GroupResult.IsSatisfied())
                {
                    if (GroupResult.Explanations.Contains(GroupExplanation.PlannedSatisfied))
                    {
                        GroupResult.PlanningStatus = PlanningStatus.CompletelyPlanned;
                    }
                    else if (GroupResult.CountApplied() + GroupResult.CountPlannedApplied() > 0)
                    {
                        GroupResult.PlanningStatus = PlanningStatus.PartiallyPlanned;
                    }
                    else
                    {
                        GroupResult.PlanningStatus = PlanningStatus.NotPlanned;
                    }
                }

                GroupResults.Add(GroupResult);




                // Update the Master list of acad results so we know if a course/credit has been used and for what type of requirement.
                foreach (var res in GroupResult.Results)
                {
                    //only when acad credit is applied or planned applied. UseTracker keeps track of requirements that acad credits 
                    //are applied to. This UseTracker is used for exclusion types later.
                    if (res.Result == Result.Applied || res.Result == Result.PlannedApplied)
                    {
                        AcadResult orig;
                            // Acad Cred
                            if (res.GetType() == typeof(CreditResult))
                            {
                                orig = MasterResults.Find(mstr => mstr.GetAcadCredId() == res.GetAcadCredId());
                            }
                            else
                            {
                                // Planned Courses

                                orig = MasterResults.Find(mstr => mstr.GetCourse() != null &&  // noncourses in master results will return null for GetCourse()
                                                           mstr.GetType() != typeof(CreditResult) &&      // exclude all ocurses that are of type CreditResult, we only want to look at Planned Courses
                                                           mstr.GetCourse().Id == res.GetCourse().Id);

                            }

                        try
                        {
                            if (orig != null && UseTracker[orig] != null)
                            {


                                // Add the Id of the requirement to which this academic credit was applied
                                //only when requirment extra course directive is not semi-apply and acadresult explanation is not extra
                                //basically when extraCourse directive is 'semi-apply' and course is marked as extra then that course should be available for 
                                //other requirements even if the other requirement have exclusion types defined.
                                if (!(GroupResult.Group.ExtraCourseDirective == ExtraCourses.SemiApply && res.Explanation == AcadResultExplanation.Extra))
                                {
                                    if (!UseTracker[orig].ContainsKey(g.SubRequirement.Requirement.Id))
                                    {

                                        UseTracker[orig].Add(g.SubRequirement.Requirement.Id, new List<string>() { g.Id });
                                    }
                                    else
                                    {
                                        UseTracker[orig][g.SubRequirement.Requirement.Id].Add(g.Id);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore.  If they typecode was missing it was a prerequisite, which has no need of sharing/exclusions
                        }


                    }
                }

                    //we are again evaluating subgroup because last group of subrequirment won't be evaluated which is needed to find if the last group is extra or not
                    //we will pass tracker in this call to remove extra credits from tracker.
                    List<GroupResult> groupResultsForCurrentSubReq = GroupResults.Where(grr => grr.Group.SubRequirement.Id == g.SubRequirement.Id).ToList();
                     SubrequirementResult subrequirmentResult = g.SubRequirement.Evaluate(groupResultsForCurrentSubReq, MasterResults,UseTracker);

            }


            // Evaluate Subrequirements

            foreach (var sub in Requirements.SelectMany(r => r.SubRequirements))
            {
                List<GroupResult> groupResultsThisSubr = GroupResults.Where(grr => grr.Group.SubRequirement.Id == sub.Id).ToList();
                SubrequirementResult thisSubResult = sub.Evaluate(groupResultsThisSubr);

                // Set Planning/Completion Status.  Constructor creates these as "Unplanned/Unstarted" so defaulting them here
                // is not necessary.

                int groupscomplete = 0;
                int groupsnotcomplete = 0;
                int groupscompleteplan = 0;
                int groupspartialplan = 0;
                int mingroups = sub.MinGroups ?? sub.Groups.Count;

                foreach (var gr in thisSubResult.GroupResults)
                {
                    if (gr.CompletionStatus == CompletionStatus.Completed ||
                        gr.CompletionStatus == CompletionStatus.Waived) { groupscomplete++; }
                    if (gr.CompletionStatus == CompletionStatus.PartiallyCompleted) { groupsnotcomplete++; }
                    if (gr.PlanningStatus == PlanningStatus.CompletelyPlanned) { groupscompleteplan++; }
                    if (gr.PlanningStatus == PlanningStatus.PartiallyPlanned) { groupspartialplan++; }
                }
                if (sub.IsWaived)
                {
                    thisSubResult.CompletionStatus = CompletionStatus.Waived;
                    thisSubResult.PlanningStatus = PlanningStatus.CompletelyPlanned;
                }
                else
                {
                    if (thisSubResult.IsSatisfied())
                    {
                        // this also means min gpa, etc. is satisfied
                        if (groupscomplete >= mingroups) { thisSubResult.CompletionStatus = CompletionStatus.Completed; }
                        //otherwise, look for any progress
                        else if (groupscomplete + groupsnotcomplete > 0) { thisSubResult.CompletionStatus = CompletionStatus.PartiallyCompleted; }
                    }
                    else
                    {
                        if (groupsnotcomplete + groupscomplete > 0) { thisSubResult.CompletionStatus = CompletionStatus.PartiallyCompleted; }
                    }
                    // Mark the subrequirement result as completely planned ONLY if all needed groups are complete AND the planned credits+applied credits is over the minimum institutional credits defined for the subrequirement
                    if (groupscompleteplan >= mingroups && (!sub.MinInstitutionalCredits.HasValue || (thisSubResult.GetPlannedAppliedCredits() + thisSubResult.GetAppliedInstitutionalCredits()) >= sub.MinInstitutionalCredits.Value)) { thisSubResult.PlanningStatus = PlanningStatus.CompletelyPlanned; }
                    else if (groupspartialplan + groupscompleteplan > 0) { thisSubResult.PlanningStatus = PlanningStatus.PartiallyPlanned; }

                }
                AllSubrequirementResults.Add(thisSubResult);

            }


            // Evaluate Requirements

            foreach (var req in Requirements)
            {
                List<SubrequirementResult> subrResultsThisReq = AllSubrequirementResults.Where(asrr => req.SubRequirements.Contains(asrr.SubRequirement)).ToList();

                RequirementResult thisRequirementResult = req.Evaluate(subrResultsThisReq);

                // Set Planning/Completion Status.  Constructor creates these as "Unplanned/Unstarted" so defaulting them here
                // is not necessary.

                int subscomplete = 0;
                int subsnotcomplete = 0;
                int subscompleteplan = 0;
                int subspartialplan = 0;
                int minsubs = req.MinSubRequirements ?? req.SubRequirements.Count;

                foreach (var sr in thisRequirementResult.SubRequirementResults)
                {
                    if (sr.CompletionStatus == CompletionStatus.Completed ||
                        sr.CompletionStatus == CompletionStatus.Waived) { subscomplete++; }
                    if (sr.CompletionStatus == CompletionStatus.PartiallyCompleted) { subsnotcomplete++; }
                    if (sr.PlanningStatus == PlanningStatus.CompletelyPlanned) { subscompleteplan++; }
                    if (sr.PlanningStatus == PlanningStatus.PartiallyPlanned) { subspartialplan++; }
                }
                if (req.IsWaived)
                {
                    thisRequirementResult.CompletionStatus = CompletionStatus.Waived;
                    thisRequirementResult.PlanningStatus = PlanningStatus.CompletelyPlanned;
                }
                else
                {
                    if (thisRequirementResult.IsSatisfied())
                    {
                        // this also means min gpa, etc. is satisfied
                        if (subscomplete >= minsubs) { thisRequirementResult.CompletionStatus = CompletionStatus.Completed; }
                        //otherwise, look for any progress
                        else if (subscomplete + subsnotcomplete > 0) { thisRequirementResult.CompletionStatus = CompletionStatus.PartiallyCompleted; }
                    }
                    else
                    {
                        if (subscomplete + subsnotcomplete > 0) { thisRequirementResult.CompletionStatus = CompletionStatus.PartiallyCompleted; }
                    }
                    // Mark the requirement as complete only if the needed subrequirements are planned and the number of credits applied+planned is at least the minimum inst credits defined.
                    if (subscompleteplan >= minsubs && (!req.MinInstitutionalCredits.HasValue || (thisRequirementResult.GetPlannedAppliedCredits() + thisRequirementResult.GetAppliedInstitutionalCredits()) >= req.MinInstitutionalCredits.Value)) { thisRequirementResult.PlanningStatus = PlanningStatus.CompletelyPlanned; }
                    else if (subspartialplan + subscompleteplan > 0) { thisRequirementResult.PlanningStatus = PlanningStatus.PartiallyPlanned; }

                }
                ProgramResult.RequirementResults.Add(thisRequirementResult);
            }

            if (ProgramResult.RequirementResults.Where(rr => rr.IsSatisfied()).Count() != Requirements.Count())
            {
                ProgramResult.Explanations.Add(ProgramRequirementsExplanation.MinRequirements);
                if (ProgramResult.RequirementResults.Where(rr => rr.IsSatisfied()).Count() + ProgramResult.RequirementResults.Where(rr => rr.IsPlannedSatisfied()).Count() >= Requirements.Count())
                {
                    ProgramResult.Explanations.Add(ProgramRequirementsExplanation.PlannedSatisfied);
                }
            }

            // Sum up the credits

            ProgramResult.InstitutionalCredits = ProgramResult.GetInstCredits();

            ProgramResult.Credits = ProgramResult.GetCredits();

            ProgramResult.InProgressCredits = ProgramResult.GetInProgressCredits();
            //Planned credits count should not include planned courses  that were replaced in progress
            ProgramResult.PlannedCredits = PlannedCourses.Where(p => p.ReplacedStatus != ReplacedStatus.ReplaceInProgress).Select(pc => pc.Credits ?? pc.Course.MinimumCredits ?? 0m).Sum();


            // Evaluate program-level requrements
            if (ProgramRequirements != null)
            {
                if (ProgramRequirements.MinimumCredits.HasValue)
                {
                    if (ProgramRequirements.MinimumCredits > ProgramResult.Credits)
                    {
                        ProgramResult.Explanations.Add(ProgramRequirementsExplanation.MinOverallCredits);
                    }
                }

                if (ProgramRequirements.MinimumInstitutionalCredits.HasValue)
                {
                    if (ProgramRequirements.MinimumInstitutionalCredits > ProgramResult.InstitutionalCredits)
                    {
                        ProgramResult.Explanations.Add(ProgramRequirementsExplanation.MinInstCredits);
                    }
                }

                if (ProgramRequirements.MinOverallGpa.HasValue)
                {
                    if (ProgramResult.CumGpa == null || ProgramRequirements.MinOverallGpa > ProgramResult.CumGpa)
                    {
                        ProgramResult.Explanations.Add(ProgramRequirementsExplanation.MinOverallGpa);
                    }
                }

                if (ProgramRequirements.MinInstGpa.HasValue)
                {
                    if (ProgramResult.InstGpa == null || ProgramRequirements.MinInstGpa > ProgramResult.InstGpa)
                    {
                        ProgramResult.Explanations.Add(ProgramRequirementsExplanation.MinInstGpa);
                    }
                }
            }
            //-----------

            if (ProgramResult.Explanations.Count() == 0)
            {
                ProgramResult.Explanations.Add(ProgramRequirementsExplanation.Satisfied);
                ProgramResult.IsSatisfied = true;
            }

            if (ProgramResult.Explanations.Contains(ProgramRequirementsExplanation.PlannedSatisfied))
            {
                ProgramResult.IsPlannedSatisfied = true;
            }



            // Log results

            if (logger != null && (logger.IsDebugEnabled || logger.IsInfoEnabled))
            {
                logger.Debug("Program Evaluation Results for " + StudentProgram.StudentId + " in program " + StudentProgram.ProgramCode);
                logger.Debug("Credits: " + ProgramResult.Credits);
                logger.Debug("InstitutionalCredits: " + ProgramResult.InstitutionalCredits);
                logger.Debug("PlannedCredits: " + ProgramResult.PlannedCredits);
                logger.Debug("InProgress: " + ProgramResult.InProgressCredits);
                logger.Debug((ProgramResult.CumGpa == null) ? "CumGPA: null" : "cumGpa: " + ProgramResult.CumGpa);

                Dump(debuglist, ProgramResult, "verbose");

                foreach (var line in constructorDebug)
                {
                    logger.Debug(line);
                }

                foreach (var line in debuglist)
                {
                    //logger.Debug(line);
                    logger.Debug(line);
                }
            }

            return ProgramResult;
        }

        /// <summary>
        /// After an optimizing pass over the results, the service layer may decide to rerun the evaluation
        /// without applying any credits/courses to groups or subrequirements that are not needed by adding
        /// ids to skip here and calling Evaluate() again.
        /// </summary>
        /// <param name="groupid"></param>
        public void AddGroupsToSkip(List<string> groupids)
        {
            if (groupids == null) { throw new ArgumentNullException("groupids"); }
            GroupsToSkip.AddRange(groupids);
        }
        /// <summary>
        /// After an optimizing pass over the results, the service layer may decide to rerun the evaluation
        /// without applying any credits/courses to groups or subrequirements that are not needed by adding
        /// ids to skip here and calling Evaluate() again.
        /// </summary>
        /// <param name="subid"></param>
        public void AddSubRequirementsToSkip(List<string> subids)
        {
            if (subids == null) { throw new ArgumentNullException("subids"); }
            SubRequirementsToSkip.AddRange(subids);
        }

        /// <summary>
        /// Creates a list of Academic Results for evaluation against a group
        /// </summary>
        /// <param name="masterlist">Master list of Academic Results from which to dole out credit and planned courses already in result form</param>
        /// <param name="exclusions">List of requirement type codes with which this requirement does not allow sharing of credit</param>
        /// <param name="usetracker">Dictionary showing which Academic Results have been used for what requirement types</param>
        /// <param name="grouptype">The group type of the current result set.  Take ALL type blocks ignore exclusions.</param>
        /// <returns>List of <see cref="AcadResult"/>Academic Result objects</returns>
        public List<AcadResult> ConstructResultSet(List<AcadResult> masterlist, Group group, IDictionary<AcadResult, Dictionary<string, List<string>>> usetracker, List<Override> groupOverrides)

        {
            // The returned list of academic credit results to evaluate against this group
            List<AcadResult> resultSet = new List<AcadResult>();

            // FIRST EXCLUDE: Build list of results to use for this group using the master list of academic credits.
            //    The master list is reduced based on requirement type reuse specifications at program level.


            foreach (var acr in masterlist)
            {
                List<string> appliedToReqs = new List<string>();
                // if this group is a "takeall" type (must take all the courses listed), all academic credits must be included in the group evaluation
                if (group.GroupType != GroupType.TakeAll)
                {
                    // Get the list of requirement codes this academic credit has already been applied to. (When the group evaluate logic determines
                    // a credit will be applied to a group, the requirement Id of the group is added to this dictionary, keyed by academic credit.)
                    if(usetracker.ContainsKey(acr))
                    {
                        appliedToReqs = usetracker[acr].Select(a => a.Key).ToList();
                    }
                    else
                    {
                        // try to retrieve a planning course from Master list and build a key to search in usetracker
                        var orig = MasterResults.Find(mstr => mstr.GetCourse() != null &&  // noncourses in master results will return null for GetCourse()
                                                          mstr.GetType() != typeof(CreditResult) &&      // exclude all ocurses that are of type CreditResult, we only want to look at Planned Courses
                                                          mstr.GetCourse().Id == acr.GetCourse().Id);
                        if(usetracker.ContainsKey(orig))
                        {
                            appliedToReqs = usetracker[orig].Select(a => a.Key).ToList();
                        }

                    }
                    //get the current group's requirement Id
                    var currentGroupRequirementId = group.SubRequirement.Requirement.Id;

                    //Check if requirement exclusion table contains requirement id as a key. Usually Exclusion table can never be null because it is created as empty dictionary in constructor.
                    //but it can be empty if none of the requirements have exclusion types defined. 
                    //If any of the requirement have exclusion type defined then there should always be an entry for each requirement id even if that Id does not have exclusion type defined.
                    if (RequirementExclusionTable.ContainsKey(currentGroupRequirementId))
                    {
                        //check if current academic credit is applied to the requirement which this group's requirement excludes. if so don't take it for current group evaluation
                        // In other words, Exclude this academic credit if it has already been applied to a group whose requirement is excluded by this group's requirement type
                        if (appliedToReqs.Intersect(RequirementExclusionTable[currentGroupRequirementId].ExcludesRequirementIds).Count() > 0)
                        {
                            continue;
                        }
                        //check if current academic credit is already applied to the requirment that this particular group's requirment is excluded by.  
                        //Or in other words check if current academic credit is already applied to the requirement that excludes this particular group's requirement.
                        // Exclude this academic credit if it has already been applied to a group whose requirement excludes this group's requirement type
                        //NOTE: This is being done so that the reverse of the exclude is also done(because that is what EVAL does).In other words, if
                        //  a requirement of type MAJ excludes requirements of type GEN, then the reverse will happen even if the GEN requirement does not have an
                        //    exclusion for MAJ.At the requirement level it is reciprocal.
                        if (appliedToReqs.Intersect(RequirementExclusionTable[currentGroupRequirementId].ExcludedByRequirementIds).Count() > 0)
                        {
                            continue;
                        }
                    }
                    //we should also validate for group's exclusions- do not take the acad cred if the requirements it is applied to is excluded in the group's excludes.
                    if (group.Exclusions.Any())
                    {
                        var appliedToReqTypes = Requirements.Where(r => appliedToReqs.Contains(r.Id) && r.Id != group.SubRequirement.Requirement.Id).Select(r => r.RequirementType.Code);
                        if (appliedToReqTypes.Intersect(group.Exclusions).Count() > 0)
                        {
                            continue;
                        }
                    }
                }
            
                // Add any academic credit that remains at this point (and has a valid status) to the result set, excluding withdrawn credits
                if (acr.GetType() == typeof(CreditResult))
                {
                    // The repository theoretically limits these to these statuses, plus withdrawn. We want to exclude withdrawn from evaluation for any group.
                    CreditStatus status = acr.GetAcadCred().Status;
                    if (status == CreditStatus.Add || status == CreditStatus.New || status == CreditStatus.TransferOrNonCourse || status == CreditStatus.Preliminary)
                    {
                        resultSet.Add((AcadResult)new CreditResult(acr.GetAcadCred()));
                    }
                }
                else
                {
                    var pcc = new PlannedCredit(acr.GetCourse(), acr.GetTermCode(), acr.GetSectionId());
                    pcc.ReplacedStatus = (acr as CourseResult).PlannedCourse.ReplacedStatus;
                    pcc.ReplacementStatus = (acr as CourseResult).PlannedCourse.ReplacementStatus;
                    pcc.Credits = (acr as CourseResult).PlannedCourse.Credits;
                    resultSet.Add((AcadResult)new CourseResult(pcc));
                }
            }

            // SECOND EXCLUDE: Current group's Requirement does not allow reuse between subrequirements

            // Check whether course reuse is allowed between the subrequirements within this current group's requirement
            // If reuse is not allowed and the group type is not a "take all"--which ignores reuse--filter out the credits that have already been used
            // by the other subrequirements in this current group's requirement.
            if (!group.SubRequirement.Requirement.AllowsCourseReuse && group.GroupType != GroupType.TakeAll)
            {
                // Reduce this list by the credits/planned courses already applied
                // Get all the applied academic credits and applied planned courses for all this requirement's
                // groups except those in the current subrequirement
                var usedItems = new List<AcadResult>();
                var allSubreqGroupResults = GroupResults
                                                .Where(gr => gr.Group.SubRequirement.Requirement.Id == group.SubRequirement.Requirement.Id)
                                                .Where(gr => gr.Group.SubRequirement.Id != group.SubRequirement.Id)
                                                .ToList();
                usedItems.AddRange(allSubreqGroupResults.SelectMany(gr => gr.GetApplied().ToList()));
                usedItems.AddRange(allSubreqGroupResults.SelectMany(gr => gr.GetPlannedApplied().ToList()));
                // Remove all items for the given course or (noncourse academic credit) from the list to include in the evaluation
                foreach (var acadRslt in usedItems)
                {
                    // Remove this academic credit only when extra course handling is not semiapply. It means for all other options acad credits will be removed if they have already been applied previously. Irrespective of the credit applied is extra or not.
                    if (group.SubRequirement.Requirement.ExtraCourseDirective != ExtraCourses.SemiApply)
                    {
                        resultSet.RemoveAll(agc => agc.GetAcadCredId() != null && agc.GetAcadCredId() == acadRslt.GetAcadCredId());
                        // For course-based academic credits, remove all instances of this course in planned courses
                        if (acadRslt.GetCourse() != null)
                        {
                            resultSet.RemoveAll(agc => agc.GetAcadCredId() == null && agc.GetCourse() != null && agc.GetCourse().Id == acadRslt.GetCourse().Id);
                        }
                    }
                    //specific condition only for semiApply option. This is to clean the applied credits  that are not marked as extra. This is because we want to to carry 'extra' courses to be picked for evaluation.

                    if (group.SubRequirement.Requirement.ExtraCourseDirective == ExtraCourses.SemiApply)
                    {
                        resultSet.RemoveAll(agc => agc.GetAcadCredId() != null && agc.GetAcadCredId() == acadRslt.GetAcadCredId() && acadRslt.Explanation!=AcadResultExplanation.Extra && acadRslt.Explanation!=AcadResultExplanation.ExtraInGroup);
                        // For course-based academic credits, remove all instances of this course in planned courses
                        if (acadRslt.GetCourse() != null)
                        {
                            resultSet.RemoveAll(agc => agc.GetAcadCredId() == null && agc.GetCourse() != null && agc.GetCourse().Id == acadRslt.GetCourse().Id && acadRslt.Explanation != AcadResultExplanation.Extra && acadRslt.Explanation != AcadResultExplanation.ExtraInGroup);
                        }
                    }

                }
            }

            // THIRD EXCLUDE: Current group's Subrequirement does not allow reuse between groups

            // Check whether course reuse is allowed between the groups within this current group's subrequirement
            // If reuse is not allowed and the group type is not a "take all"--which ignores reuse--filter out the credits that have already been used
            // by the other groups in this current group's subrequirement.
            if (!group.SubRequirement.AllowsCourseReuse && group.GroupType != GroupType.TakeAll)
            {
                // Get all acad results applied to all groups in this subrequirement. 
                // Remove all items with the same course from the list to include in the evaluation
                var usedItems = new List<AcadResult>();
                var allSubreqGroupResults = GroupResults
                                                .Where(gr => gr.Group.SubRequirement.Requirement.Id == group.SubRequirement.Requirement.Id)
                                                .Where(gr => gr.Group.SubRequirement.Id == group.SubRequirement.Id)
                                                .ToList();
                usedItems.AddRange(allSubreqGroupResults.SelectMany(gr => gr.GetApplied()).ToList());
                usedItems.AddRange(allSubreqGroupResults.SelectMany(gr => gr.GetPlannedApplied()).ToList());
                // Remove all items for the given course or (noncourse academic credit) from the list to include in the evaluation
                foreach (var acadRslt in usedItems)
                {

                    if (group.SubRequirement.ExtraCourseDirective != ExtraCourses.SemiApply)
                    {
                        resultSet.RemoveAll(agc => agc.GetAcadCredId() != null && agc.GetAcadCredId() == acadRslt.GetAcadCredId());
                        // For course-based academic credits, remove all instances of this course in planned courses
                        if (acadRslt.GetCourse() != null)
                        {
                            resultSet.RemoveAll(agc => agc.GetAcadCredId() == null && agc.GetCourse() != null && agc.GetCourse().Id == acadRslt.GetCourse().Id);
                        }
                    }
                    if (group.SubRequirement.ExtraCourseDirective == ExtraCourses.SemiApply)
                    {
                        resultSet.RemoveAll(agc => agc.GetAcadCredId() != null && agc.GetAcadCredId() == acadRslt.GetAcadCredId() && acadRslt.Explanation != AcadResultExplanation.Extra && acadRslt.Explanation != AcadResultExplanation.ExtraInGroup);
                        // For course-based academic credits, remove all instances of this course in planned courses
                        if (acadRslt.GetCourse() != null)
                        {
                            resultSet.RemoveAll(agc => agc.GetAcadCredId() == null && agc.GetCourse() != null && agc.GetCourse().Id == acadRslt.GetCourse().Id && acadRslt.Explanation != AcadResultExplanation.Extra && acadRslt.Explanation != AcadResultExplanation.ExtraInGroup);
                        }
                    }
                }
            }

            // INCLUDE SPECIFICALLY ALLOWED OVERRIDES FOR THIS GROUP THAT MIGHT HAVE BEEN OMITTED OR REMOVED BY REUSE CHECKS ABOVE.

            // Any academic credit result that is specifically allowed in the group overrides but that was filtered out by any of the non-reuse exclusions should be added back into the result set
            if (groupOverrides != null && groupOverrides.Any())
            {
                foreach (var groupOverride in groupOverrides)
                {
                    if (groupOverride.CreditsAllowed != null)
                    {
                        foreach (var creditAllowed in groupOverride.CreditsAllowed)
                        {
                            if (!resultSet.Any(acr => acr.GetAcadCredId() != null && acr.GetAcadCredId() == creditAllowed))
                            {
                                var overrideCreditResult = masterlist.Where(cr => cr.GetAcadCredId() == creditAllowed).FirstOrDefault();
                                if (overrideCreditResult != null && overrideCreditResult.GetAcadCred() != null)
                                {
                                    CreditStatus status = overrideCreditResult.GetAcadCred().Status;
                                    if (status == CreditStatus.Add || status == CreditStatus.New || status == CreditStatus.TransferOrNonCourse || status == CreditStatus.Preliminary)
                                    {
                                        resultSet.Add((AcadResult)new CreditResult(overrideCreditResult.GetAcadCred()));
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return resultSet;
        }

        private ProgramRequirements CopyProgramRequirements(ProgramRequirements pr1)
        {
            ProgramRequirements pr2 = new ProgramRequirements(pr1.ProgramCode, pr1.CatalogCode);

            if (pr1.ActivityEligibilityRules != null)
            {
                pr2.ActivityEligibilityRules = new List<RequirementRule>();
                foreach (var rule in pr1.ActivityEligibilityRules)
                {
                    pr2.ActivityEligibilityRules.Add(rule.Copy());
                }
            }

            if (pr1.AllowedGrades != null)
            {
                List<Grade> gradelist = new List<Grade>();
                foreach (var grade in pr1.AllowedGrades)
                {
                    gradelist.Add(grade.DeepCopy());
                }
                pr2.AllowedGrades = gradelist;
            }
            pr2.MaximumCredits = pr1.MaximumCredits;
            if (pr1.MinGrade != null)
            {
                pr2.MinGrade = pr1.MinGrade.DeepCopy();
            }
            pr2.MinimumCredits = pr1.MinimumCredits;
            pr2.MinimumInstitutionalCredits = pr1.MinimumInstitutionalCredits;
            pr2.MinInstGpa = pr1.MinInstGpa;
            pr2.MinOverallGpa = pr1.MinOverallGpa;

            //Requirements
            if (pr1.Requirements != null)
            {
                pr2.Requirements = CopyRequirements(pr1.Requirements, pr2);
            } // if requirements not null

            // Copy additional requirement insertion points
            pr2.RequirementToPrintCcdsAfter = pr1.RequirementToPrintCcdsAfter;
            pr2.RequirementToPrintMajorsAfter = pr1.RequirementToPrintMajorsAfter;
            pr2.RequirementToPrintMinorsAfter = pr1.RequirementToPrintMinorsAfter;
            pr2.RequirementToPrintSpecializationsAfter = pr1.RequirementToPrintSpecializationsAfter;

            return pr2;

        }



        private List<Requirement> CopyRequirements(List<Requirement> list1, ProgramRequirements newProgramRequirements = null)
        {
            List<Requirement> list2 = new List<Requirement>();
            foreach (var r1 in list1)
            {
                Requirement r2 = new Requirement(r1.Id, r1.Code, r1.Description, r1.GradeSchemeCode, r1.RequirementType, newProgramRequirements);

                if (r1.AcademicCreditRules != null)
                {
                    r2.AcademicCreditRules = new List<RequirementRule>();
                    foreach (var rule in r1.AcademicCreditRules)
                    {
                        r2.AcademicCreditRules.Add(rule.Copy());
                    }
                }

                if (r1.AllowedGrades != null)
                {
                    List<Grade> gradelist = new List<Grade>();
                    foreach (var grade in r1.AllowedGrades)
                    {
                        gradelist.Add(grade.DeepCopy());
                    }
                    r2.AllowedGrades = gradelist;
                }

                r2.AllowsCourseReuse = r1.AllowsCourseReuse;
                r2.CustomUse = r1.CustomUse;
                r2.SortSpecificationId = r1.SortSpecificationId;
                if (r1.RequirementExclusions != null)
                {
                    r2.RequirementExclusions = new List<RequirementBlockExclusion>();
                    foreach (var excl in r1.RequirementExclusions)
                    {
                        r2.RequirementExclusions.Add(new RequirementBlockExclusion(excl.ExclusionType, excl.FirstOnlyFlag));
                    }
                }

                r2.ExtraCourseDirective = r1.ExtraCourseDirective;
                r2.InternalType = r1.InternalType;
                r2.IsWaived = r1.IsWaived;  // This should always be false
                r2.MinGpa = r1.MinGpa;
                if (r1.MinGrade != null)
                {
                    r2.MinGrade = r1.MinGrade.DeepCopy();
                }
                r2.MinInstitutionalCredits = r1.MinInstitutionalCredits;
                r2.MinSubRequirements = r1.MinSubRequirements;
                r2.RequirementType = r1.RequirementType;
                r2.IncludeLowGradesInGpa = r1.IncludeLowGradesInGpa;
                r2.WaitToMerge = r1.WaitToMerge;

                if (r1.SubRequirements != null)
                {
                    r2.SubRequirements = new List<Subrequirement>();
                    foreach (var s1 in r1.SubRequirements)
                    {
                        Subrequirement s2 = new Subrequirement(s1.Id, s1.Code);

                        s2.DisplayText = s1.DisplayText;

                        if (s1.AcademicCreditRules != null)
                        {
                            s2.AcademicCreditRules = new List<RequirementRule>();
                            foreach (var rule in s1.AcademicCreditRules)
                            {
                                s2.AcademicCreditRules.Add(rule.Copy());
                            }
                        }

                        if (s1.AllowedGrades != null)
                        {
                            List<Grade> gradelist = new List<Grade>();
                            foreach (var grade in s1.AllowedGrades)
                            {
                                gradelist.Add(grade.DeepCopy());
                            }
                            s2.AllowedGrades = gradelist;
                        }

                        s2.AllowsCourseReuse = s1.AllowsCourseReuse;
                        s2.ExtraCourseDirective = s1.ExtraCourseDirective;
                        s2.InternalType = s1.InternalType;
                        s2.IsWaived = s1.IsWaived;
                        s2.MinGpa = s1.MinGpa;
                        if (s1.MinGrade != null)
                        {
                            s2.MinGrade = s1.MinGrade.DeepCopy();
                        }
                        s2.MinGroups = s1.MinGroups;
                        s2.MinInstitutionalCredits = s1.MinInstitutionalCredits;
                        s2.Requirement = r2;
                        s2.IncludeLowGradesInGpa = s1.IncludeLowGradesInGpa;
                        s2.WaitToMerge = s1.WaitToMerge;
                        s2.SortSpecificationId = s1.SortSpecificationId;

                        if (s1.Groups != null)
                        {
                            s2.Groups = new List<Group>();
                            foreach (var g1 in s1.Groups)
                            {
                                Group g2 = new Group(g1.Id, g1.Code, s2);
                                g2.DisplayText = g1.DisplayText;
                                if (g1.AcademicCreditRules != null)
                                {
                                    g2.AcademicCreditRules = new List<RequirementRule>();
                                    foreach (var rule in g1.AcademicCreditRules)
                                    {
                                        g2.AcademicCreditRules.Add(rule.Copy());
                                    }
                                }

                                if (g1.AllowedGrades != null)
                                {
                                    List<Grade> gradelist = new List<Grade>();
                                    foreach (var grade in g1.AllowedGrades)
                                    {
                                        gradelist.Add(grade.DeepCopy());
                                    }
                                    g2.AllowedGrades = gradelist;
                                }

                                foreach (var bn in g1.ButNotCourseLevels)
                                {
                                    g2.ButNotCourseLevels.Add(bn);
                                }

                                foreach (var bn in g1.ButNotCourses)
                                {
                                    g2.ButNotCourses.Add(bn);
                                }
                                foreach (var bn in g1.ButNotDepartments)
                                {
                                    g2.ButNotDepartments.Add(bn);
                                }
                                foreach (var bn in g1.ButNotSubjects)
                                {
                                    g2.ButNotSubjects.Add(bn);
                                }

                                foreach (var cr in g1.Courses)
                                {
                                    g2.Courses.Add(cr);
                                }

                                g2.ExtraCourseDirective = g1.ExtraCourseDirective;

                                foreach (var cr in g1.FromCourses)
                                {
                                    g2.FromCourses.Add(cr);
                                }
                                foreach (var cr in g1.FromCoursesException)
                                {
                                    g2.FromCoursesException.Add(cr);
                                }
                                foreach (var cr in g1.FromDepartments)
                                {
                                    g2.FromDepartments.Add(cr);
                                }
                                foreach (var cr in g1.FromLevels)
                                {
                                    g2.FromLevels.Add(cr);
                                }
                                foreach (var cr in g1.FromSubjects)
                                {
                                    g2.FromSubjects.Add(cr);
                                }
                                g2.GroupType = g1.GroupType;
                                g2.InListOrder = g1.InListOrder;
                                g2.InternalType = g1.InternalType;
                                g2.IsWaived = g1.IsWaived;
                                g2.MaxCourses = g1.MaxCourses;
                                g2.MaxCoursesAtLevels = g1.MaxCoursesAtLevels;
                                g2.MaxCoursesPerDepartment = g1.MaxCoursesPerDepartment;
                                g2.MaxCoursesPerRule = g1.MaxCoursesPerRule;
                                g2.MaxCoursesPerSubject = g1.MaxCoursesPerSubject;
                                g2.MaxCoursesRule = g1.MaxCoursesRule != null ? g1.MaxCoursesRule.Copy() : null;
                                g2.MaxCredits = g1.MaxCredits;
                                g2.MaxCreditsAtLevels = g1.MaxCreditsAtLevels;
                                g2.MaxCreditsPerCourse = g1.MaxCreditsPerCourse;
                                g2.MaxCreditsPerDepartment = g1.MaxCreditsPerDepartment;
                                g2.MaxCreditsPerRule = g1.MaxCreditsPerRule;
                                g2.MaxCreditsPerSubject = g1.MaxCreditsPerSubject;
                                g2.MaxCreditsRule = g1.MaxCreditsRule != null ? g1.MaxCreditsRule.Copy() : null;
                                g2.MaxDepartments = g1.MaxDepartments;
                                g2.MaxSubjects = g1.MaxSubjects;
                                g2.MinCourses = g1.MinCourses;
                                g2.MinCoursesPerDepartment = g1.MinCoursesPerDepartment;
                                g2.MinCoursesPerSubject = g1.MinCoursesPerSubject;
                                g2.MinCredits = g1.MinCredits;
                                g2.MinCreditsPerCourse = g1.MinCreditsPerCourse;
                                g2.MinCreditsPerDepartment = g1.MinCreditsPerDepartment;
                                g2.MinCreditsPerSubject = g1.MinCreditsPerSubject;
                                g2.MinDepartments = g1.MinDepartments;
                                g2.MinGpa = g1.MinGpa;
                                if (g1.MinGrade != null)
                                {
                                    g2.MinGrade = g1.MinGrade.DeepCopy();
                                }
                                g2.MinInstitutionalCredits = g1.MinInstitutionalCredits;
                                g2.MinSubjects = g1.MinSubjects;
                                g2.IncludeLowGradesInGpa = g1.IncludeLowGradesInGpa;
                                g2.SortSpecificationId = g1.SortSpecificationId;
                                g2.Exclusions = g1.Exclusions;
                                s2.Groups.Add(g2);
                            }
                        }

                        r2.SubRequirements.Add(s2);
                    }
                }

                list2.Add(r2);

            } // for each requirement

            return list2;
        }
        /// <summary>
        /// Creates the "master" list of Academic Results
        /// </summary>
        private List<AcadResult> ConstructResultSet(IEnumerable<AcademicCredit> credits, IEnumerable<PlannedCredit> plannedCredits, bool useDefault = true)
        {
            List<AcadResult> MasterResults;
            List<AcadResult> resultlist = new List<AcadResult>();
            Dictionary<AcadResult, int> sortorder = new Dictionary<AcadResult, int>();

            if (useDefault)
            {
                //Dictionary<AcadResult, DateTime> startdates = new Dictionary<AcadResult,DateTime>();

                // The Datatel-supplied DEFAULT sort type sorts in the}following way:
                // Category 1: In-house, graded courses
                // Category 2: In-house, ungraded courses which are nonetheless considered complete
                //             (they have no grade scheme, so they are not expected to ever get a grade)
                // Category 3: Equivalencies (transfer and non-course)
                // Category 4: In-progress courses
                // Category 5: Pre-registered courses

                // Within each category, items are sorted by start date,earliest date first.
                // Note that the term "courses" as used above actually refers to ANY STUDENT.ACAD.CRED 
                // record, even those which record only credits, and not a specific course, such as 
                // when a lump sum of credits is transferred in from another institution.


                foreach (var cred in credits)
                {
                    CreditResult creditresult = new CreditResult(cred);
                    //startdates.Add(creditresult, cred.DateOfSomeKind);
                    int sortvalue;

                    if (cred.VerifiedGrade != null && cred.IsInstitutional())
                    {
                        sortvalue = 1;
                    }
                    else if (cred.IsInstitutional() && cred.IsCompletedCredit)
                    {
                        sortvalue = 2;
                    }
                    else if (cred.Status == CreditStatus.TransferOrNonCourse)
                    {
                        sortvalue = 3;
                    }
                    else if (cred.Status == CreditStatus.Preliminary)
                    {
                        sortvalue = 5;
                    }
                    else // In progress
                    {
                        sortvalue = 4;
                    }

                    sortorder.Add(creditresult, sortvalue);
                    resultlist.Add(creditresult);

                }

                // Get the list of courses 
                foreach (var plannedCredit in plannedCredits)
                {
                    const int sortvalue = 6;
                    var courseresult  = new CourseResult(new PlannedCredit(plannedCredit.Course, plannedCredit.TermCode, plannedCredit.SectionId));
                    courseresult.PlannedCourse.ReplacedStatus = plannedCredit.ReplacedStatus;
                    courseresult.PlannedCourse.ReplacementStatus = plannedCredit.ReplacementStatus;
                    if (plannedCredit.Credits.HasValue) { courseresult.PlannedCourse.Credits = plannedCredit.Credits; }
                    sortorder.Add(courseresult, sortvalue);
                    resultlist.Add(courseresult);
                }

                // order by the default sort order to match host system results (Colleague EVAL):
                //    First by Category
                //    Second by Date
                //    Last by Descending Academic Credit ID
                MasterResults = resultlist.OrderBy(ar => sortorder[ar])
                                          .ThenBy(ar => (ar.GetAcadCred() == null) ? DateTime.MaxValue : ar.GetAcadCred().StartDate)
                                          .ThenByDescending(ar => (ar.GetAcadCredId() == null ? 0 : Int64.Parse(ar.GetAcadCredId())))
                                          .ToList();

                return MasterResults;
            }
            else
            {
                var creditsList = credits.ToList();
                for (int i = 0; i < creditsList.Count; i++)
                {
                    CreditResult creditresult = new CreditResult(creditsList[i]);
                    sortorder.Add(creditresult, i);
                    resultlist.Add(creditresult);

                }

                int plannedCreditSortvalue = creditsList.Count + 1;

                // Get the list of courses 
                foreach (var plannedCredit in plannedCredits)
                {
                    var courseresult = new CourseResult(new PlannedCredit(plannedCredit.Course, plannedCredit.TermCode, plannedCredit.SectionId));
                    courseresult.PlannedCourse.ReplacedStatus = plannedCredit.ReplacedStatus;
                    courseresult.PlannedCourse.ReplacementStatus = plannedCredit.ReplacementStatus;
                    if (plannedCredit.Credits.HasValue) { courseresult.PlannedCourse.Credits = plannedCredit.Credits; }
                    sortorder.Add(courseresult, plannedCreditSortvalue);
                    resultlist.Add(courseresult);
                }

                MasterResults = resultlist.OrderBy(ar => sortorder[ar]).ToList();
                return MasterResults;
            }
        }

        private IDictionary<AcadResult, Dictionary<string, List<string>>> ConstructUseTracker(List<AcadResult> MasterResults)
        {
            //dictionary to keep a track of which acadResult is applied to which requirment and groups within that requirment.
            Dictionary<AcadResult, Dictionary<string, List<string>>> UseTracker = new Dictionary<AcadResult, Dictionary<string, List<string>>>();
            foreach (var acr in MasterResults)
            {
                UseTracker[acr] = new Dictionary<string, List<string>>();
            }
            return UseTracker;
        }

        private void Dump(List<string> output, ProgramEvaluation pr, string option = null)
        {
            //Student Program 

            output.Add("Program Result: " /*+ StudentProgram.ProgramCode + "\t " */+ string.Join(",", pr.Explanations.Select(ex => ex.ToString())));


            foreach (var rr in pr.RequirementResults)
            {
                // Requirement
                output.Add("\tRequirement: " + rr.Requirement.Code + "\t " + " Status: " + rr.CompletionStatus.ToString()
                                                                                  + ",  " + rr.PlanningStatus.ToString());

                foreach (var sr in rr.SubRequirementResults)
                {
                    //Subrequirement
                    output.Add("\t\tSubrequirement: " + sr.SubRequirement.Code + "\t " + " Status: " + sr.CompletionStatus.ToString()
                                                                                              + ", " + sr.PlanningStatus.ToString());

                    foreach (var gr in sr.GroupResults)
                    {
                        //Group
                        output.Add("\t\t\tGroup: " + gr.Group.Id + " " + gr.Group.Code + "\t " + string.Join(",", gr.Explanations.Select(ex => ex.ToString())));


                        // result
                        if (!string.IsNullOrEmpty(option))
                        {
                            foreach (string res in gr.EvalDebug)
                            {
                                if (option.ToLower() != "brief" || res.ToLower().Contains("applied"))
                                {
                                    output.Add("\t\t\t\t\t" + res);
                                }
                            }
                        }
                    }

                }

            }
        }
        /// <summary>
        /// This is going to sort the acadmic credits on basis of the order of course ids in from courses.
        /// IN.LIST.ORDER is 2nd level of sort after the first sort on academic credits and planned courses is already done on basis of sort definition on group or the default sorting.
        /// </summary>
        /// <param name="masterResults">this master result is already sorted on sort definition on group or default sorting mechanism</param>
        /// <param name="fromCourses"></param>
        /// <returns></returns>
        private List<AcadResult> SortOnBasisOfInListOrder(List<AcadResult> masterResults, List<string> fromCourses)
        {
            Dictionary<AcadResult, int> indexedMasterResults = new Dictionary<AcadResult, int>();
            List<AcadResult> sortedMasterResults = new List<AcadResult>();
            if (fromCourses == null || !fromCourses.Any())
            {
                return masterResults;
            }
            if (masterResults == null || !masterResults.Any())
            {
                return masterResults;
            }
            
            try
            {
                //assigning a index value to each entry in masterresult
                foreach (AcadResult result in masterResults)
                {
                        //initial assignment are all with MaxValue. 
                        indexedMasterResults.Add(result , int.MaxValue);
                }
                int indexValue = 1;
                //assign index to academic credits and planned courses based upon the sequence in from courses list
                foreach (string course in fromCourses)
                {
                    //find all the academic credits that have same course as is in the list
                    List<AcadResult> academicCrditResults = masterResults.Where(m => m.GetCourse()!=null && m.GetCourse().Id == course).ToList();
                    if (academicCrditResults != null && academicCrditResults.Any())
                    {
                        //all those academic credits are given same index value
                        foreach (AcadResult result in academicCrditResults)
                        {
                            try
                            {
                                //we don't want to re-enter new index value if it is already there. An example of this is when a From Courses have same course in a list repeated eg: FROM HIST-100 POLI-100 FREN-100 POLI-100. Take first index value of POLI-100
                                if (indexedMasterResults[result] == int.MaxValue)
                                {
                                    indexedMasterResults[result] = indexValue;
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    indexValue += 1;
                }
                //create a masterlist that will have completed, inprogress , planned in order

                //push all completed first, sorted on the order of index provided that were in from list. Remember TE and NE are always considered as completed.
                IEnumerable<KeyValuePair<AcadResult, int>> completedCredits = indexedMasterResults.Where(i => i.Key.GetAcadCred()!=null && i.Key.GetAcadCred().IsCompletedCredit == true && i.Value< int.MaxValue).OrderBy(i => i.Value);
                sortedMasterResults.AddRange(completedCredits.Select(i => i.Key));
                //push all inprogress courses
                IEnumerable<KeyValuePair<AcadResult, int>> inprogressCredits = indexedMasterResults.Where(i => i.Key.GetAcadCred()!=null && i.Key.GetAcadCred().IsCompletedCredit == false && i.Value<int.MaxValue).OrderBy(i => i.Value);
                sortedMasterResults.AddRange(inprogressCredits.Select(i => i.Key));

                //push all planned courses
                IEnumerable<KeyValuePair<AcadResult, int>> orderedPlannedCourses = indexedMasterResults.Where(i => i.Key.GetAcadCred() == null && i.Value <int.MaxValue).OrderBy(i => i.Value);
                sortedMasterResults.AddRange(orderedPlannedCourses.Select(i => i.Key));

                //push all the remaining completed , inprogress credits and planned courses that were not in from list. Basically push all the remaining courses from masterlist that were not in from courses list.
                //this list is not to be in order of the status but should be in same sequence as were initially available.
                IEnumerable<KeyValuePair<AcadResult, int>> notInFromListCredits = indexedMasterResults.Where(i => i.Value == int.MaxValue);
                sortedMasterResults.AddRange(notInFromListCredits.Select(i => i.Key));

                return sortedMasterResults;
            }
            catch (Exception e)
            {
                logger.Error(e, "exception occured while doing IN.LIST.ORDER");
                return masterResults;
            }
        }

        /// <summary>
        /// This will build Requirement exclusion table that identifies which requirement excludes or is excluded by other requirements
        /// Having this table build in the beginning helps to not check everytime for each acad cred for each group evaluation.
        /// </summary>
        private void BuildRequirementExclusionTable()
        {
            try
            {
                if (RequirementExclusionTable == null)
                {
                    logger.Info("Requirement exclusion table is null, it should not be that case. An empty exclusion table should always be existing.");
                    return;
                }
                //build only when any of the requirement have exclusions defined
                if (!this.Requirements.Select(r => r.RequirementExclusions).Any())
                {
                    logger.Info("Requirement Exclusion table is empty because there are no exclusions defined.");
                    return;
                }

                //build empty exclusion table with all the requirement ids first. It means we have to have one to one corresponding enty with each requirement.
                foreach (Requirement req in Requirements)
                {
                    if (!RequirementExclusionTable.ContainsKey(req.Id))
                    {
                        RequirementExclusionTable.Add(req.Id, new RequirementExclusionTable(req.Id, req.Code, req.RequirementType));
                    }
                }

                //now fill the exclusion properties
                //In this loop taking each requirement id one by one  
                foreach (Requirement req in Requirements)
                {
                    if (req.RequirementExclusions != null)
                    {
                        //this loop will build excludes and excluded by properties for each requirement
                        //For example - MIN is declared such as Exclusion type is MAJ then the table is set in both the directions such as MIN excludes MAJ and MAJ is excluded by MIN.
                        //In this case all the requirements of type MIN will have all the requirement Ids of MAJ type in its Excludes collection 
                        //whereas all the requirements of MAJ type will have this particular requirment id of MIN type in its Excluded By collection. 

                        //Now for the requirment Id, loop for all the exclusions defined one by one
                        foreach (RequirementBlockExclusion requirmentExclusion in req.RequirementExclusions)
                        {
                            List<string> requirementIds = new List<string>();
                            //if the requirement excluion of first only flag then grab the requirement id of only first requirement of that type
                            //For example MIN has MAJ exclusion type with flag=Y and there are suppose 2 major requirements then it will take only the first one from the list
                            //otherwsie will take all the requirements of MAJ type.
                            if (requirmentExclusion.FirstOnlyFlag == true)
                            {
                                //take only the firt requirment id of that exclusion type
                                Requirement firstRequirment = Requirements.Where(r => r.Id != req.Id && r.RequirementType.Code == requirmentExclusion.ExclusionType).FirstOrDefault();
                                if (firstRequirment != null)
                                {
                                    requirementIds.Add(firstRequirment.Id);
                                }
                            }
                            else
                            {
                                List<string> alltheRequirmentIds = Requirements.Where(r => r.Id != req.Id && r.RequirementType.Code == requirmentExclusion.ExclusionType).Select(r => r.Id).ToList();
                                requirementIds.AddRange(alltheRequirmentIds);
                            }

                            //populate excludes- Above requirment Ids will go as Excludes for the requirment being processed.
                            RequirementExclusionTable[req.Id].ExcludesRequirementIds.AddRange(requirementIds);
                            //populate excluded by. Since it is 2 way table, for all recquirements that current requirement was excluding, we need to update for those requirments "Excluded By" collection
                            foreach (string reqId in requirementIds)
                            {
                                RequirementExclusionTable[reqId].ExcludedByRequirementIds.Add(req.Id);

                            }
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Exception occured while building Requirment Exclusion Table.");
                throw;
            }
        }
    }

}
