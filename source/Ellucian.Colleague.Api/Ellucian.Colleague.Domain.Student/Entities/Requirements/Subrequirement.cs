// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class Subrequirement : RequirementBlock
    {
        public Requirement Requirement { get; set; }
        public List<Group> Groups { get; set; }
        public int? MinGroups { get; set; }

        public Subrequirement(string id, string code)
            : base(id, code)
        {
            Groups = new List<Group>();
        }

        /// <summary>
        /// Flag indicating whether or not this block is used exclusively to convey print text
        /// Example: TAKE 0 CREDITS
        /// </summary>
        public bool OnlyConveysPrintText
        {
            get
            {
                return Groups != null && Groups.Where(g => g != null).All(g => g.OnlyConveysPrintText);
            }
        }

        public SubrequirementResult Evaluate(List<GroupResult> groupresults, List<AcadResult> MasterResult = null, IDictionary<AcadResult, Dictionary<string, List<string>>> UseTracker = null)
        { 

            SubrequirementResult srr = new SubrequirementResult(this);
            if (groupresults == null || groupresults.Count == 0)
            {
                return srr;
            }
            srr.GroupResults.AddRange(groupresults);
            List<GroupResult> satisfiedGroupResults = srr.GetSatisfied().ToList();
            //to find how many satisfied groups were actually completed
            int completedSatisfiedGroups = satisfiedGroupResults.Where(a => a.CompletionStatus == CompletionStatus.Completed).Count();
            //total number of satisfied groups that included completed and in-progress
            int satisfiedgroups = satisfiedGroupResults.Count();
            //total number of groups that were planned and satified
            int plannedSatisfiedGroups = srr.GetPlannedSatisfied().Count();
            int numberOfGroupsInSubrequirment = srr.SubRequirement.Groups.Count();
            int mingroups = (MinGroups.HasValue && MinGroups.Value != 0) ? MinGroups.Value : numberOfGroupsInSubrequirment;

          
            //logic about determining if the subrequirement is satisfied or not for min group condition
            if (MinGroups != null && MinGroups.Value>0)
            {
                //if there were not all completed satisfied groups but may have inprogress & completed groups  and have reached to last group being processed
                if (groupresults.Count() == numberOfGroupsInSubrequirment)
                {
                    
                    if (satisfiedgroups + plannedSatisfiedGroups >= MinGroups.Value || completedSatisfiedGroups >= MinGroups.Value)
                    {
                        // take completed , inprogress, planned in order until min groups and clear rest of the groups
                        //order the groups
                        List<string> groupsToSkip = FindGroupsToSkip( groupresults, srr.SubRequirement.Groups, MinGroups).ToList();
                        ClearGroupResults(srr.GroupResults.Where(g => groupsToSkip.Contains(g.Group.Id)), MasterResult, UseTracker);
                    }

                    else
                    {
                        srr.Explanations.Add(SubrequirementExplanation.MinGroups);
                    }
                }
                else
                {
                    srr.Explanations.Add(SubrequirementExplanation.MinGroups);

                }

            }

            else
            {

                if (satisfiedgroups < mingroups)
                {
                    srr.Explanations.Add(SubrequirementExplanation.MinGroups);
                }
            }

            if (MinGpa.HasValue)
            {
                if (srr.Gpa < MinGpa.Value)
                {
                    srr.Explanations.Add(SubrequirementExplanation.MinGpa);
                }
            }

            if (MinInstitutionalCredits.HasValue)
            {
                if (srr.GetAppliedInstitutionalCredits() < MinInstitutionalCredits.Value)
                {
                    srr.Explanations.Add(SubrequirementExplanation.MinInstitutionalCredits);
                }
            }

            // If all groups are not complete or the min institutional credits is not met, determine if we can call it "fully planned"
            if (srr.Explanations.Contains(SubrequirementExplanation.MinGroups) || srr.Explanations.Contains(SubrequirementExplanation.MinInstitutionalCredits))
            {
                // Consider planned satisfied if the minimum number of groups has been planned AND the applied institutional credits + the planned credits exceeds or equals the minimum institutional credits.
                if (satisfiedgroups + plannedSatisfiedGroups >= mingroups && (!MinInstitutionalCredits.HasValue || (srr.GetPlannedAppliedCredits() + srr.GetAppliedInstitutionalCredits()) >= MinInstitutionalCredits.Value))
                {
                    srr.Explanations.Add(SubrequirementExplanation.PlannedSatisfied);
                }
            }


            if (srr.Explanations.Count() == 0)
            {
                srr.Explanations.Add(SubrequirementExplanation.Satisfied);
            }


            return srr;

           
        }

        /// <summary>
        /// This is going to clean acadCredits within extra groups.
        /// </summary>
        /// <param name="groupResult">Extra Groups Results</param>
        private void ClearGroupResults(IEnumerable<GroupResult> groupResult, List<AcadResult> MasterResults, IDictionary<AcadResult, Dictionary<string, List<string>>> useTracker)
        {
            if (groupResult == null || !groupResult.Any())
            {
                return;
            }
            foreach (GroupResult gr in groupResult.ToList())
            {
                foreach (AcadResult ar in gr.Results)
                {
                    //if ExtraCourseDirective is apply or semi-apply then acad credits will still be applied but will be marked as ExtraInGroup
                    if ((gr.Group.ExtraCourseDirective == ExtraCourses.Apply) || (gr.Group.ExtraCourseDirective == ExtraCourses.SemiApply))
                    {
                        //Mark ExtraInGroup if not already identified extra course for group
                        if (ar.Explanation != AcadResultExplanation.Extra)
                        {
                            ar.Explanation = AcadResultExplanation.ExtraInGroup;
                        }
                    }
                    //if ExtraCourseDirective is Display, None or Ignore or semi-apply then the extra courses in this extra group
                    //should be removed from tracker so that those are available for other requirments that have exclusion type defined
                    if (gr.Group.ExtraCourseDirective != ExtraCourses.Apply)
                    {

                        //if acad credits are applied or planned applied should be made as related for availabilty to other groups
                        if (ar.Result == Result.PlannedApplied || ar.Result == Result.Applied)
                        {
                            AcadResult orig;
                            if (MasterResults != null && useTracker != null)
                            {
                                //those extra courses needs to be cleaned from tracker too so that those are available for other requirments and are not considered during exclusion logic within requirments.

                                if (ar.GetType() == typeof(CreditResult))
                                {
                                    orig = MasterResults.Find(mstr => mstr.GetAcadCredId() == ar.GetAcadCredId());


                                }
                                // Planned Courses
                                else
                                {

                                    orig = MasterResults.Find(mstr => mstr.GetCourse() != null &&  // noncourses in master results will return null for GetCourse()
                                                               mstr.GetType() != typeof(CreditResult) &&      // exclude all ocurses that are of type CreditResult, we only want to look at Planned Courses
                                                               mstr.GetCourse().Id == ar.GetCourse().Id);
                                }
                                //clean this acad credit from tracker too. 
                                if (useTracker.ContainsKey(orig))
                                {
                                    string requirmentId = gr.Group.SubRequirement.Requirement.Id;
                                    //if acad credit is applied to requirment for current group
                                    if (useTracker[orig].ContainsKey(requirmentId))
                                    {
                                        //if applied to current group
                                        if (useTracker[orig][requirmentId].Contains(gr.Group.Id))
                                        {
                                            //remove the group entry from list
                                            useTracker[orig][requirmentId].Remove(gr.Group.Id);
                                            //verify if this is the only group that this course was applied to, if so then delete whole requirment 
                                            if (!useTracker[orig][requirmentId].Any())
                                            {
                                                useTracker[orig].Remove(requirmentId);
                                            }
                                        }
                                    }
                                }
                            }
                            //if acad credits are applied or planned applied should be made as related for availabilty to other groups
                            if (gr.Group.ExtraCourseDirective != ExtraCourses.SemiApply)
                            {
                                ar.Result = Result.Related;
                            }
                        }
                    }

                    //now identify group as extra
                    if ((gr.Group.ExtraCourseDirective == ExtraCourses.Apply) || (gr.Group.ExtraCourseDirective == ExtraCourses.SemiApply))
                    {
                        gr.MinGroupStatus = GroupResultMinGroupStatus.Extra;
                    }
                    //when ignore or group completion and planning status needs to be cleaned up 
                    else if (gr.Group.ExtraCourseDirective == ExtraCourses.Ignore)
                    {
                        gr.MinGroupStatus = GroupResultMinGroupStatus.Ignore;
                        gr.CompletionStatus = CompletionStatus.NotStarted;
                        gr.PlanningStatus = PlanningStatus.NotPlanned;
                    }
                    else
                    {
                        gr.MinGroupStatus = GroupResultMinGroupStatus.None;
                        gr.CompletionStatus = CompletionStatus.NotStarted;
                        gr.PlanningStatus = PlanningStatus.NotPlanned;
                    }
                }
            }
        }

        /// <summary>
        /// This method is going to order the results within group according to CompletionStatus and PlanningStatus
        /// Suppose two or more groups have same status, they should be ordered according to sequence of  groups defined in subrequirement
        /// After all the groups are ordered, only minimum number of groups are kept from the top and rest of them are marked to skip
        /// </summary>
        /// <param name="groupResults"></param>
        /// <param name="groupSequenceInSubrequirment"></param>
        /// <param name="minGroupsToTake"></param>
        /// <returns></returns>
        private IEnumerable<string> FindGroupsToSkip(IEnumerable<GroupResult> groupResults, List<Group> groupSequenceInSubrequirment, int? minGroupsToTake)
        {
            if (groupResults == null || minGroupsToTake == null || groupSequenceInSubrequirment==null)
            {
                return new List<string>();
            }
            //create an empty bucket that will contain group results based upon its status. status will act as key to this bucket. These keys represents the order 
            //in which groups will be displayed.
            Dictionary<string, List<GroupResult>> sortedGroupResults = new Dictionary<string, List<GroupResult>>();
            //groups that are waived
            sortedGroupResults.Add("waived", new List<GroupResult>());
            //Those that are completed, meaning all necessary applied courses are completed.
            sortedGroupResults.Add("completed", new List<GroupResult>());
            //Those for which all courses needed to satisfy the sub-block are at least registered for. (Status Pending) - All are InProgress or registered
            //Those for which at least one course needed to satisfy the sub - block is registered for, and the sub - block is fully planned - All are planned but few are inprogress or registered
            sortedGroupResults.Add("pending", new List<GroupResult>());
            sortedGroupResults.Add("allRegistered", new List<GroupResult>());
            sortedGroupResults.Add("partiallyRegistered", new List<GroupResult>());

            //Those with no registered courses that apply to the sub-block, but the sub-block is fully planned. (Status Not Started, but fully planned)
            sortedGroupResults.Add("planned", new List<GroupResult>());
            //Those for which at least one course needed to satisfy the sub-block is registered for, and the sub-block is not fully planned. (Status In Progress, and not fully planned)
            sortedGroupResults.Add("inprogress", new List<GroupResult>());
            sortedGroupResults.Add("partiallyInprogress", new List<GroupResult>());
            //Those with no registered courses that apply to the sub-block, and the sub - block is not fully planned. (Status Not Started, and not fully planned)
            sortedGroupResults.Add("partiallyPlanned", new List<GroupResult>());
            //Those with no registered courses that apply to the sub-block, and the sub - block is not fully planned. (Status Not Started, and not planned)
            sortedGroupResults.Add("notStarted", new List<GroupResult>());
            List<string> groupIds = new List<string>();
            List<string> groupsToSkip = new List<string>();
            List<GroupResult> sortedGroups = new List<GroupResult>();

            //push the group results into appropriate bucket.
            foreach (Group g in groupSequenceInSubrequirment)
            {
                var groupResult = groupResults.FirstOrDefault(gr => gr.Group.Id == g.Id);
                if (groupResult != null)
                {
                    if (groupResult.CompletionStatus == CompletionStatus.Waived)
                    {
                        sortedGroupResults["waived"].Add(groupResult);
                    }
                    else if (groupResult.CompletionStatus == CompletionStatus.Completed)
                    {
                        sortedGroupResults["completed"].Add(groupResult);
                    }
                    else if (groupResult.CompletionStatus == CompletionStatus.PartiallyCompleted && groupResult.PlanningStatus == PlanningStatus.CompletelyPlanned)
                    {
                        sortedGroupResults["pending"].Add(groupResult);

                    }
                    
                    else if (groupResult.CompletionStatus == CompletionStatus.NotStarted && groupResult.PlanningStatus == PlanningStatus.CompletelyPlanned)
                    {
                        if (groupResult.CountApplied() > 0)
                        {
                            if (groupResult.CountPlannedApplied() == 0)
                            {  
                                sortedGroupResults["allRegistered"].Add(groupResult);
                            }
                            else
                            { 
                                sortedGroupResults["partiallyRegistered"].Add(groupResult);
                            }
                        }
                        else
                        {
                            sortedGroupResults["planned"].Add(groupResult);
                        }

                    }
                    else if (groupResult.CompletionStatus == CompletionStatus.PartiallyCompleted && groupResult.PlanningStatus == PlanningStatus.PartiallyPlanned)
                    {
                        sortedGroupResults["inprogress"].Add(groupResult);

                    }

                    else if (groupResult.CompletionStatus == CompletionStatus.NotStarted && groupResult.PlanningStatus == PlanningStatus.PartiallyPlanned)
                    {
                        if (groupResult.CountApplied() > 0)
                        {
                            sortedGroupResults["partiallyInprogress"].Add(groupResult);
                        }
                        else
                        {
                            sortedGroupResults["partiallyPlanned"].Add(groupResult);
                        }

                    }

                    else if (groupResult.CompletionStatus == CompletionStatus.NotStarted && groupResult.PlanningStatus == PlanningStatus.NotPlanned)
                    {
                            sortedGroupResults["notStarted"].Add(groupResult);

                    }
                }
            }


            sortedGroups = sortedGroupResults.SelectMany(s => s.Value).ToList();
            if (sortedGroups != null)
            {
                groupsToSkip = sortedGroups.Skip(minGroupsToTake.Value).Select(s => s.Group.Id).ToList();
            }
            return groupsToSkip;

        }
    }
}
