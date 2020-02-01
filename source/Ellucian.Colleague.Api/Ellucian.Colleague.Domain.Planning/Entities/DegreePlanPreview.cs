// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Planning.Services;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// Contains information related to previewing a sample degree plan against a student degree plan.
    /// Both the limited preview of the plan and a merged student degree plan are included.
    /// </summary>
    [Serializable]
    public class DegreePlanPreview
    {
        /// <summary>
        /// Limited degree plan containing only the courses from the sample plan
        /// </summary>
        public DegreePlan Preview { get; set; }

        /// <summary>
        /// Contains the student's degree plan merged with the sample degree plan - in a state that is ready to update.
        /// </summary>
        public DegreePlan MergedDegreePlan { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="degreePlan"></param>
        /// <param name="sampleDegreePlan"></param>
        /// <param name="studentAcademicCredits"></param>
        /// <param name="availablePlanningTerms"></param>
        /// <param name="firstTermCode">Code for the term at which to start the sample plan</param>
        /// <param name="addedBy">The system ID for the user applying this sample plan</param>
        public DegreePlanPreview(DegreePlan degreePlan, SampleDegreePlan sampleDegreePlan, IEnumerable<AcademicCredit> studentAcademicCredits, IEnumerable<Term> availablePlanningTerms, string firstTermCode, string addedBy=null)
        {
            if (sampleDegreePlan == null)
            {
                throw new ArgumentNullException("sampleDegreePlan", "A sample degree Plan must be provided.");
            }
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            if (availablePlanningTerms == null || availablePlanningTerms.Count() == 0)
            {
                throw new ArgumentNullException("planningTerms", "Planning Terms are required in order to apply a sample plan.");
            }
            if (studentAcademicCredits == null)
            {
                throw new ArgumentNullException("studentAcademicCredits", "List of student academic credits cannot be null.");
            }

            // APPLY THE SAMPLE PLAN TO THE DEGREE PLAN to form the merged degree plan.
            try
            {
                //// Apply the courses defined in the curriculum track course blocks to terms in the degree plan
                //degreePlan.ApplySample(sampleDegreePlan, availablePlanningTerms, studentAcademicCredits, firstTermCode, addedBy);
                SampleDegreePlanService.ApplySample(ref degreePlan, sampleDegreePlan, availablePlanningTerms, studentAcademicCredits, firstTermCode, addedBy);

                if (degreePlan.TermIds.Count() == 0)
                {
                    throw new ArgumentException("Unable to perform a preview of this degree plan against with the sample plan.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // Create the limited preview using the merged degree plan as a basis 
            var preview = new DegreePlan(degreePlan.PersonId);

            // Get the list of merged degree plan terms in reporting term/sequence number order - this will be used to add terms to the preview.

            // Throw out any planning terms that have ended, or aren't default terms, or fall before the firstTermCode specified
            var availablePlanningTermsFuture = availablePlanningTerms.Where(t => t.EndDate > DateTime.Today).Where(t => t.DefaultOnPlan == true);
            
            // create the list of terms that are ON THE PLAN that we can use to apply the sample
            // same first term applies, same end date check, but don't care if it's a default (because it's already on the plan)
            var availablePlannedTermsFuture = availablePlanningTerms.Where(t => degreePlan.TermIds.Contains(t.Code) && t.EndDate > DateTime.Now);

            var previewPlanTerms = availablePlanningTermsFuture.Union(availablePlannedTermsFuture).OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();

            var firstTerm = previewPlanTerms.FirstOrDefault(t => t.Code == firstTermCode);
            var firstTermIndex = firstTerm == null ? 0 : previewPlanTerms.ToList().IndexOf(firstTerm);
            previewPlanTerms = previewPlanTerms.ToList().GetRange(firstTermIndex, sampleDegreePlan.CourseBlocks.Count() <= previewPlanTerms.Count() - firstTermIndex ? sampleDegreePlan.CourseBlocks.Count() : previewPlanTerms.Count() - firstTermIndex);

            // Add courses in each block to each successive term in the degree plan, based on the relative sequence of each term.
            // First add the term, then loop through each block adding courses to the newly added term.
            for (int i = 0; i < sampleDegreePlan.CourseBlocks.Count(); i++)
            {
                // Add the next degree plan term to the preview for this course block.
                preview.AddTerm(previewPlanTerms.ElementAt(i).Code);
                // If there are not enough terms on the merged degree plan for some reason, stop adding courses.
                if (degreePlan.TermIds.Count() >= i + 1)
                {
                    var term = previewPlanTerms.ElementAt(i);
                    var block = sampleDegreePlan.CourseBlocks.ElementAt(i);
                    for (int j = 0; j < block.CourseIds.Count(); j++)
                    {
                        preview.AddCourse(new PlannedCourse(block.CourseIds.ElementAt(j), null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, DateTimeOffset.Now), term.Code);
                    }
                }
            }
            Preview = preview;
            MergedDegreePlan = degreePlan;
        }
    }
}
