// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Domain.Planning.Services
{
    public static class SampleDegreePlanService
    {
        public static void ApplySample(ref DegreePlan degreePlan, SampleDegreePlan curriculumTrack, 
            IEnumerable<Term> availablePlanningTerms, IEnumerable<AcademicCredit> studentAcademicCredits, string firstTermCode, string addedBy)
        {
            if (degreePlan == null)
            {
                throw new ArgumentException("Degree Plan must be provided to apply a sample degree plan");
            }

            if (availablePlanningTerms == null || availablePlanningTerms.Count() == 0)
            {
                throw new ArgumentException("Terms must be provided to apply a sample degree plan");
            }

            // Throw out any planning terms that have ended, or aren't default terms, or fall before the firstTermCode specified
            var availablePlanningTermsFuture = availablePlanningTerms.Where(t => t.EndDate > DateTime.Today).Where(t => t.DefaultOnPlan == true);

            // create the list of terms that are ON THE PLAN that we can use to apply the sample
            // same first term applies, same end date check, but don't care if it's a default (because it's already on the plan)
            var degreePlanTermIds = degreePlan.TermIds; //cannot pass ref parameter in anonymous method
            var availablePlannedTermsFuture = availablePlanningTerms.Where(t => degreePlanTermIds.Contains(t.Code) && t.EndDate > DateTime.Now);

            var previewPlanTerms = availablePlanningTermsFuture.Union(availablePlannedTermsFuture).OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();

            var firstTerm = previewPlanTerms.FirstOrDefault(t => t.Code == firstTermCode);
            var firstTermIndex = firstTerm == null ? 0 : previewPlanTerms.IndexOf(firstTerm);
            previewPlanTerms = previewPlanTerms.GetRange(firstTermIndex, curriculumTrack.CourseBlocks.Count() <= previewPlanTerms.Count() - firstTermIndex ? curriculumTrack.CourseBlocks.Count() : previewPlanTerms.Count() - firstTermIndex);

            // Attempt to add the terms needed to add all the blocks from the sample to this degree plan
            foreach (var term in previewPlanTerms)
            {
                try
                {
                    // This method will throw an error if the term already
                    // exists on the plan. Ignore it.
                    degreePlan.AddTerm(term.Code);
                }
                catch
                {
                    // Ignore any exception thrown when trying to add a term to the degree plan
                }
            }

            // If there are still no terms in the degree plan, throw an error that the sample
            // could not be applied.
            if (degreePlan.TermIds.Count() == 0)
            {
                throw new ArgumentException("Default planning terms have not been set up; DegreePlan cannot be updated");
            }

            // Get a list of the student's active credits
            var activeAcademicCreditCourses = studentAcademicCredits.Where(a => a.Status == CreditStatus.Add || a.Status == CreditStatus.New || a.Status == CreditStatus.TransferOrNonCourse).Where(x => x.Course != null).Select(c => c.Course).ToList();

            // Add courses in each block to each successive term in the degree plan, starting with the selected first term, 
            // based on the relative sequence of each term.
            // Loop through each block adding courses to next term
            var i = 0;
            foreach (var courseBlock in curriculumTrack.CourseBlocks)
            {
                var term = previewPlanTerms.ElementAt(i);
                foreach (var courseId in courseBlock.CourseIds)
                {
                    // Courses that are either "in progress" or "previously taken" (i.e. there is an active student academic credit) will not be added to the plan again.
                    var matchingCredits = activeAcademicCreditCourses.Where(c => c.Id == courseId).ToList();
                    if (matchingCredits.Count() == 0)
                    {
                        var courses = degreePlan.GetPlannedCourses(term.Code).Select(p => p.CourseId);
                        if (!courses.Contains(courseId))
                        {
                            degreePlan.AddCourse(new PlannedCourse(courseId, null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy, DateTime.Now), term.Code);
                        }
                    }
                }
                i++;
                if (i >= availablePlanningTerms.Count())
                {
                    break;
                }
            }
        }

    }
}
