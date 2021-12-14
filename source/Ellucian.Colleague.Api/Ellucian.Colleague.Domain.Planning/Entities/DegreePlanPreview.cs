// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Planning.Services;
using slf4net;

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
        /// This contains the list of all the courses listed in preview degree plan with its evaluation status for the program for which sample plan is loaded.
        /// For example: A program has sample plan to load certain course in a term. After this program is evaluated the course  could have been applied or not applied or not applied due to min grade. 
        /// This list have one to one correspondance with course in preview in order to identify its evaluation status.
        /// </summary>
        public List<DegreePlanPreviewCourseEvaluationResult> DegreePlanPreviewCoursesEvaluation { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DegreePlanPreview()
        {
            DegreePlanPreviewCoursesEvaluation = new List<DegreePlanPreviewCourseEvaluationResult>();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="degreePlan"></param>
        /// <param name="sampleDegreePlan"></param>
        /// <param name="studentAcademicCredits"></param>
        /// <param name="availablePlanningTerms"></param>
        /// <param name="firstTermCode">Code for the term at which to start the sample plan</param>
        /// <param name="addedBy">The system ID for the user applying this sample plan</param>
        public DegreePlanPreview(DegreePlan degreePlan, SampleDegreePlan sampleDegreePlan, IEnumerable<AcademicCredit> studentAcademicCredits, IEnumerable<Term> availablePlanningTerms, string firstTermCode, string addedBy = null)
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
                        preview.AddCourse(new PlannedCourse(
                            course: block.CourseIds.ElementAt(j), section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted,
                            addedBy: null, addedOn: DateTimeOffset.Now, coursePlaceholder: null), term.Code);
                    }
                }
            }
            Preview = preview;
            MergedDegreePlan = degreePlan;
        }

        /// <summary>
        /// Update current degreeplan with sample degree plan courses to make it as merged degree plan
        ///also create a new degree plan which will only be preview degree plan starting from the start term from which sample plan is loaded.
        /// </summary>
        /// <param name="degreePlan"></param>
        /// <param name="sampleDegreePlan"></param>
        /// <param name="studentAcademicCredits"></param>
        /// <param name="allTerms"></param>
        /// <param name="firstTermCode"></param>
        /// <param name="logger"></param>
        /// <param name="addedBy"></param>
        /// <param name="considerEquatedCourses">Flag indicating whether or not to consider in-progress or completed equated courses when determining if a course in a course block should be added to the sample course plan; default is false (do not consider equated courses)</param>
        public void LoadDegreePlanPreviewWithAllTerms(DegreePlan degreePlan, SampleDegreePlan sampleDegreePlan, IEnumerable<AcademicCredit> studentAcademicCredits, IEnumerable<Term> allTerms, string firstTermCode, ILogger logger, string addedBy = null, bool considerEquatedCourses = false)
        {
            if (sampleDegreePlan == null)
            {
                throw new ArgumentNullException("sampleDegreePlan", "A sample degree Plan must be provided.");
            }
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            if (allTerms == null || !allTerms.Any())
            {
                throw new ArgumentNullException("allTerms", "Terms are required in order to apply a sample plan.");
            }
            if (studentAcademicCredits == null)
            {
                throw new ArgumentNullException("studentAcademicCredits", "List of student academic credits cannot be null.");
            }

            // Find out all the planning terms in future that have default on plan set to Yes.  DefaultOnPlan is true on when Planning flag is true. so no need to check on planning flag.
            //we are only checking on default on DP flag because these terms will automatically be added to student's DP when added through load sample plan
            IEnumerable<Term> availablePlanningTermsFuture = allTerms.Where(t => t.EndDate > DateTime.Today).Where(t => t.DefaultOnPlan == true);
            // create the list of terms that are ON THE PLAN that we can use to apply the sample. We will take all the terms that are on DP.
            IEnumerable<Term> availablePlannedTerms = allTerms.Where(t => degreePlan.TermIds.Contains(t.Code));
            //find out if there are any future terms already in degree plan
            IEnumerable<Term> availablePlannedTermFuture = allTerms.Where(t => degreePlan.TermIds.Contains(t.Code) && t.EndDate > DateTime.Now);
            //We want all the terms that are already in DP and all the future terms that are available for planning, to be available for previewing the sample plan
            //This is because a student can load sample plan starting from any term in student's DP.
            // Get the list of merged degree plan terms in reporting term/sequence number order - this will be used to add terms to the preview.
            IEnumerable<Term> prospectivePreviewPlanTerms = availablePlanningTermsFuture.Union(availablePlannedTerms).OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();
            //also finding current term and all the future plan terms that are available for preview. This is added to find first planning term to load sample plan from in case user hasn't provided 
            //the first term to load from
            IEnumerable<Term> previewPlanTermsFuture = availablePlanningTermsFuture.Union(availablePlannedTermFuture).OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();
            Term currentOrFirstFutureTerm = previewPlanTermsFuture != null && previewPlanTermsFuture.Any() ? previewPlanTermsFuture.First() : null;


            int firstTermIndex = 0;
            //check if term code is provided. If so it means sample plan should be loaded starting from that term. If not then we will assume that sample plan needs to be loaded starting from 
            //current term or first future term 
            if (!string.IsNullOrEmpty(firstTermCode))
            {
                //find the term passed  in all the terms available for preview
                var firstTerm = prospectivePreviewPlanTerms.FirstOrDefault(t => t.Code == firstTermCode);
                if (firstTerm == null && currentOrFirstFutureTerm == null)
                {
                    string message = string.Format("The starting term {0} provided is not either in future terms or one of the student's planned term as well as student's current term is not in future and there are no future terms properly setup", firstTermCode);
                    logger.Error(message);
                    throw new Exception(message);
                }
                //if the term passed is in preview plan terms, then find the position of it otherwise take the position of current term or first future term
                firstTermIndex = firstTerm == null ? prospectivePreviewPlanTerms.ToList().IndexOf(currentOrFirstFutureTerm) : prospectivePreviewPlanTerms.ToList().IndexOf(firstTerm);
            }
            else
            {
                if (currentOrFirstFutureTerm == null)
                {
                    string message = "There was no starting term provided and there are no future terms available or student's degree plan current term is not in future in order to load sample plan from; either setup future terms or select the past term from student's degree plan";
                    logger.Error(message);
                    throw new Exception(message);
                }
                //if no term code is passed to load sample plan from then we are going to load from the start of current term or first future term.
                var firstTermFuture = prospectivePreviewPlanTerms.FirstOrDefault(t => t.Code == currentOrFirstFutureTerm.Code);
                firstTermIndex = firstTermFuture == null ? 0 : prospectivePreviewPlanTerms.ToList().IndexOf(firstTermFuture);
            }

            //now we have to find which preview plan term to select from
            //suppose there are more course block then number of preview terms then we only have to load only those course blocks as many as are in preview terms.
            //suppose there are more preview terms than course blocks then we are going to load all the course blocks to the preview terms
            //find how many terms to load sample plan to based upon conditions above and the index of starting first term to load the sample plan from
            int countOfTermsToLoadPlan = sampleDegreePlan.CourseBlocks.Count() <= prospectivePreviewPlanTerms.Count() - firstTermIndex ? sampleDegreePlan.CourseBlocks.Count() : prospectivePreviewPlanTerms.Count() - firstTermIndex;
            List<Term> previewPlanTerms = prospectivePreviewPlanTerms.ToList().GetRange(firstTermIndex, countOfTermsToLoadPlan);
            // Attempt to add the terms needed to add all the blocks from the sample to this degree plan
            foreach (var term in previewPlanTerms)
            {
                try
                {
                    // This method will throw an error if the term already exists on the plan. Ignore it.
                    degreePlan.AddTerm(term.Code);
                }
                catch
                {
                    // Ignore any exception thrown when trying to add a term to the degree plan
                }
            }

            // If there are still no terms in the degree plan, throw an error that the sample could not be applied.
            if (degreePlan.TermIds.Count() == 0)
            {
                throw new ArgumentException("Default planning terms have not been set up; DegreePlan cannot be updated");
            }
            // Create the limited preview using the merged degree plan as a basis 
            DegreePlan preview = new DegreePlan(degreePlan.PersonId);
            // Get a list of the student's active credits
            var activeAcademicCreditCourses = studentAcademicCredits.Where(a => a.Status == CreditStatus.Add || a.Status == CreditStatus.New || a.Status == CreditStatus.TransferOrNonCourse).Where(x => x.Course != null).Select(c => c.Course).ToList();

            // Add courses and placeholders in each block to each successive term in the degree plan starting with the selected first term, based on the relative sequence of each term.
            // This is merging sample plan course blocks to existing degree plan as well as creating preview (limited sample plan) starting from the first term onwards.
            for (int i = 0; i < countOfTermsToLoadPlan; i++)
            {
                try
                {
                    Term degreePlanTermToLoadCourseBlock = previewPlanTerms[i];
                    CourseBlocks courseBlock = sampleDegreePlan.CourseBlocks[i];
                    logger.Info(string.Format("Loading course block {0} to term {1}", courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));
                    //add a term to new preview degree plan
                    preview.AddTerm(degreePlanTermToLoadCourseBlock.Code);

                    //add courses and sections to new preview degree plan term
                    foreach (var courseId in courseBlock.CourseIds)
                    {
                        logger.Info(string.Format("Loading course Id {0} from course block {1} to term {2} in sample plan preview", courseId, courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));

                        //add the course from course block to preview plan
                        preview.AddCourse(new PlannedCourse(course: courseId, section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted,
                            addedBy: null, addedOn: DateTimeOffset.Now, coursePlaceholder: null), degreePlanTermToLoadCourseBlock.Code);

                        // Courses that are either "in progress" or "previously taken" (i.e. there is an active student academic credit) will not be added to the plan again.
                        var matchingCredits = activeAcademicCreditCourses.Where(c => c.Id == courseId).ToList();

                        if (considerEquatedCourses)
                        {
                            matchingCredits.AddRange(activeAcademicCreditCourses.Where(c => c.EquatedCourseIds.Contains(courseId)));
                        }

                        if (!matchingCredits.Any()) // course (or one of its equated courses, if those are to be considered) is not in the academic history 
                        {
                            //get all courses on the degree plan for the term
                            var courses = degreePlan.GetPlannedCourses(degreePlanTermToLoadCourseBlock.Code).Where(p => p.CourseId != null).Select(p => p.CourseId);
                            if (courses != null && !courses.Contains(courseId))
                            {
                                logger.Info(string.Format("Loading course Id {0} from course block {1} to term {2} in merged degree plan", courseId, courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));
                                //add the course to degree plan
                                degreePlan.AddCourse(new PlannedCourse(course: courseId, section: null, gradingType: GradingType.Graded,
                                    status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: addedBy, addedOn: DateTime.Now,
                                    coursePlaceholder: null), degreePlanTermToLoadCourseBlock.Code);
                            }
                        }
                        else
                        {
                            string courseNotAddedFormatString = "Degree Plan for student {0}: Course {1} from course block {2} not added to term {3} in merged degree plan; course {2} is already in-progress or previously taken.";
                            if (considerEquatedCourses)
                            {
                                courseNotAddedFormatString = "Degree Plan for student {0}: Course {1} from course block {2} not added to term {3} in merged degree plan; course {2} or one of its equated courses is already in-progress or previously taken.";
                            }
                            logger.Debug(String.Format(courseNotAddedFormatString,
                                degreePlan.PersonId,
                                courseId,
                                courseBlock.Description,
                                degreePlanTermToLoadCourseBlock.Code));
                        }
                    }

                    //add course placeholders to new preview degree plan term
                    foreach (var coursePlaceholderId in courseBlock.CoursePlaceholderIds)
                    {
                        logger.Info(string.Format("Loading course placeholder Id {0} from course block {1} to term {2} in sample plan preview", coursePlaceholderId, courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));

                        //add the course placeholder from course block to the preview plan term
                        preview.AddCourse(new PlannedCourse(course: null, section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted,
                            addedBy: null, addedOn: DateTimeOffset.Now, coursePlaceholder: coursePlaceholderId), degreePlanTermToLoadCourseBlock.Code);

                        // add the course placeholder to the term when the course placeholder does not already exist in the term
                        var coursePlacholders = degreePlan.GetPlannedCourses(degreePlanTermToLoadCourseBlock.Code).Where(p => p.CoursePlaceholderId != null).Select(p => p.CoursePlaceholderId);
                        if (coursePlacholders != null && !coursePlacholders.Contains(coursePlaceholderId))
                        {
                            //add the course placeholder from course block to student's existing (merged) degree plan
                            degreePlan.AddCourse(new PlannedCourse(course: null, section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted,
                                addedBy: null, addedOn: DateTimeOffset.Now, coursePlaceholder: coursePlaceholderId), degreePlanTermToLoadCourseBlock.Code);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format("Course block could not be loaded to degree plan, validate that there are enough preview terms available to load course blocks to");
                    logger.Error(ex, message);
                }
            }
            Preview = preview;
            MergedDegreePlan = degreePlan;
        }

        /// <summary>
        /// Updates a degree plan with sample course plan courses to build a merged degree plan. Also builds a new, preview-only degree plan beginning with the first academic term specified.
        /// </summary>
        /// <param name="degreePlan"></param>
        /// <param name="sampleDegreePlan"></param>
        /// <param name="studentAcademicCredits"></param>
        /// <param name="programEvaluationEntity"></param>
        /// <param name="allTerms"></param>
        /// <param name="firstTermCode"></param>
        /// <param name="logger"></param>
        /// <param name="addedBy"></param>
        public void LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(DegreePlan degreePlan, SampleDegreePlan sampleDegreePlan,
            IEnumerable<AcademicCredit> studentAcademicCredits, ProgramEvaluation programEvaluationEntity, IEnumerable<Course> allCourses,
            IEnumerable<Term> allTerms, string firstTermCode, ILogger logger, string addedBy = null)
        {
            if (sampleDegreePlan == null)
            {
                throw new ArgumentNullException("sampleDegreePlan", "A sample degree Plan must be provided.");
            }
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            if (allTerms == null || !allTerms.Any())
            {
                throw new ArgumentNullException("allTerms", "Terms are required in order to apply a sample plan.");
            }
            if (programEvaluationEntity == null)
            {
                throw new ArgumentNullException("programEvaluationEntity", "Academic program evaluation cannot be null.");
            }

            // Find out all the planning terms in future that have default on plan set to Yes.  DefaultOnPlan is true on when Planning flag is true. so no need to check on planning flag.
            //we are only checking on default on DP flag because these terms will automatically be added to student's DP when added through load sample plan
            IEnumerable<Term> availablePlanningTermsFuture = allTerms.Where(t => t.EndDate > DateTime.Today).Where(t => t.DefaultOnPlan == true);
            // create the list of terms that are ON THE PLAN that we can use to apply the sample. We will take all the terms that are on DP.
            IEnumerable<Term> availablePlannedTerms = allTerms.Where(t => degreePlan.TermIds.Contains(t.Code));
            //find out if there are any future terms already in degree plan
            IEnumerable<Term> availablePlannedTermFuture = allTerms.Where(t => degreePlan.TermIds.Contains(t.Code) && t.EndDate > DateTime.Now);
            //We want all the terms that are already in DP and all the future terms that are available for planning, to be available for previewing the sample plan
            //This is because a student can load sample plan starting from any term in student's DP.
            // Get the list of merged degree plan terms in reporting term/sequence number order - this will be used to add terms to the preview.
            IEnumerable<Term> prospectivePreviewPlanTerms = availablePlanningTermsFuture.Union(availablePlannedTerms).OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();
            //also finding current term and all the future plan terms that are available for preview. This is added to find first planning term to load sample plan from in case user hasn't provided 
            //the first term to load from
            IEnumerable<Term> previewPlanTermsFuture = availablePlanningTermsFuture.Union(availablePlannedTermFuture).OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();
            Term currentOrFirstFutureTerm = previewPlanTermsFuture != null && previewPlanTermsFuture.Any() ? previewPlanTermsFuture.First() : null;

            int firstTermIndex = 0;

            //check if term code is provided. If so it means sample plan should be loaded starting from that term. If not then we will assume that sample plan needs to be loaded starting from 
            //current term or first future term 
            if (!string.IsNullOrEmpty(firstTermCode))
            {
                //find the term passed  in all the terms available for preview
                var firstTerm = prospectivePreviewPlanTerms.FirstOrDefault(t => t.Code == firstTermCode);
                if (firstTerm == null && currentOrFirstFutureTerm == null)
                {
                    string message = string.Format("Student {0}: The starting term {0} provided is not either in future terms or one of the student's planned term as well as student's current term is not in future and there are no future terms properly setup",
                        degreePlan.PersonId,
                        firstTermCode);
                    logger.Error(message);
                    throw new Exception(message);
                }
                //if the term passed is in preview plan terms, then find the position of it otherwise take the position of current term or first future term
                firstTermIndex = firstTerm == null ? prospectivePreviewPlanTerms.ToList().IndexOf(currentOrFirstFutureTerm) : prospectivePreviewPlanTerms.ToList().IndexOf(firstTerm);
            }
            else
            {
                if (currentOrFirstFutureTerm == null)
                {
                    string message = string.Format("Student {0}: There was no starting term provided and there are no future terms available or student's degree plan current term is not in future in order to load sample plan from; either setup future terms or select the past term from student's degree plan",
                        degreePlan.PersonId);
                    logger.Error(message);
                    throw new Exception(message);
                }
                //if no term code is passed to load sample plan from then we are going to load from the start of current term or first future term.
                var firstTermFuture = prospectivePreviewPlanTerms.FirstOrDefault(t => t.Code == currentOrFirstFutureTerm.Code);
                firstTermIndex = firstTermFuture == null ? 0 : prospectivePreviewPlanTerms.ToList().IndexOf(firstTermFuture);
            }

            //now we have to find which preview plan term to select from
            //suppose there are more course block then number of preview terms then we only have to load only those course blocks as many as are in preview terms.
            //suppose there are more preview terms than course blocks then we are going to load all the course blocks to the preview terms
            //find how many terms to load sample plan to based upon conditions above and the index of starting first term to load the sample plan from
            int countOfTermsToLoadPlan = sampleDegreePlan.CourseBlocks.Count() <= prospectivePreviewPlanTerms.Count() - firstTermIndex ? sampleDegreePlan.CourseBlocks.Count() : prospectivePreviewPlanTerms.Count() - firstTermIndex;
            List<Term> previewPlanTerms = prospectivePreviewPlanTerms.ToList().GetRange(firstTermIndex, countOfTermsToLoadPlan);
            // Attempt to add the terms needed to add all the blocks from the sample to this degree plan
            foreach (var term in previewPlanTerms)
            {
                try
                {
                    // This method will throw an error if the term already exists on the plan. Ignore it.
                    degreePlan.AddTerm(term.Code);
                }
                catch
                {
                    // Ignore any exception thrown when trying to add a term to the degree plan
                }
            }

            // If there are still no terms in the degree plan, throw an error that the sample could not be applied.
            if (degreePlan.TermIds.Count() == 0)
            {
                throw new ArgumentException(string.Format("Student {0}: Default planning terms have not been set up; sample course plan cannot be loaded.",
                    degreePlan.PersonId));
            }
            // Create the limited preview using the merged degree plan as a basis 
            DegreePlan preview = new DegreePlan(degreePlan.PersonId);

            // Get a list of the student's active credits
            var activeAcademicCreditCourses = studentAcademicCredits.Where(a => a.Status == CreditStatus.Add || a.Status == CreditStatus.New || a.Status == CreditStatus.TransferOrNonCourse).Where(x => x.Course != null).Select(c => c.Course).ToList();

            // extract the previewed program evaluation results for the academic credits only
            var allEvaluationCredits = programEvaluationEntity.RequirementResults.SelectMany(rr => rr.SubRequirementResults).SelectMany(sr => sr.GroupResults).SelectMany(gr => gr.Results).Where(c=>c.GetAcadCred()!=null).ToList();
            var appliedEvaluationCredits = programEvaluationEntity.RequirementResults.SelectMany(rr => rr.SubRequirementResults).SelectMany(sr => sr.GroupResults).SelectMany(gr => gr.GetApplied()).ToList();
            var notAppliedEvaluationCredits = allEvaluationCredits.Where(x => !appliedEvaluationCredits.Contains(x)).ToList();

            //Result types to be excluded
            var acadResultTypesThatExcludeCreditsFromApplying = new List<Domain.Student.Entities.Requirements.Result>() { Student.Entities.Requirements.Result.MinGrade };

            // Add courses and placeholders in each block to each successive term in the degree plan starting with the selected first term, based on the relative sequence of each term.
            // This is merging sample plan course blocks to existing degree plan as well as creating preview (limited sample plan) starting from the first term onwards.
            for (int i = 0; i < countOfTermsToLoadPlan; i++)
            {
                try
                {
                    Term degreePlanTermToLoadCourseBlock = previewPlanTerms[i];
                    CourseBlocks courseBlock = sampleDegreePlan.CourseBlocks[i];
                    logger.Debug(string.Format("Student {0}: Loading course block {1} to term {2}", degreePlan.PersonId, courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));
                    //add a term to new preview degree plan
                    preview.AddTerm(degreePlanTermToLoadCourseBlock.Code);

                    bool creditApplied = false;
                    //add courses and sections to new preview degree plan term
                    foreach (var courseId in courseBlock.CourseIds)
                    {
                        creditApplied = false;
                        logger.Debug(string.Format("Student {0}: Loading course Id {1} from course block {2} to term {3} in sample plan preview...", degreePlan.PersonId, courseId, courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));

                        //add the course from course block to preview plan
                        preview.AddCourse(new PlannedCourse(course: courseId, section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted,
                            addedBy: null, addedOn: DateTimeOffset.Now, coursePlaceholder: null), degreePlanTermToLoadCourseBlock.Code);
                        logger.Debug(string.Format("Student {0}: Course {1} added to term {2} in sample plan preview with Grading Type = Graded, Waitlist Status = Not Waitlisted...", degreePlan.PersonId, courseId, degreePlanTermToLoadCourseBlock.Code));

                        //Also add preview course result for each corresponding course added in preview degree plan.  Initially we will start as if the course added to preview degree plan is not taken by the student. Therefore the status is NotApplied
                        //This status can later change in game as we find student's academic record shows the course is taken or the course was taken by the student but did not satisfy MinGrade requirement.  
                        DegreePlanPreviewCourseEvaluationResult courseEvalResult = new DegreePlanPreviewCourseEvaluationResult();
                        courseEvalResult.CourseId = courseId;
                        courseEvalResult.TermCode = degreePlanTermToLoadCourseBlock.Code;
                        courseEvalResult.IsEnrolled = false;
                        courseEvalResult.EvaluationStatus = DegreePlanPreviewCourseEvaluationStatusType.NonApplied;

                        // Courses (or their equated courses) that are either "in progress" or "previously taken" (i.e. there is an active student academic credit) will not be added to the plan again.
                        var acadCredsForCourseOrEquatedCourse = activeAcademicCreditCourses.Where(c => c.Id == courseId || c.EquatedCourseIds.Contains(courseId)).ToList();

                        // checked if the matching course in the student's academic credits has been applied to the program requirements
                        // - if not we need to add the previously taken course to the student's degree plan
                        // - this would happen when a completed credit failed a requirement (i.e. does not meet minimum grade)
                        if (acadCredsForCourseOrEquatedCourse.Count > 0)
                        {
                            logger.Debug(string.Format("Student {0}: One or more Add/New/Transfer/Noncourse academic credits found for course {1}: {2}", degreePlan.PersonId, courseId, string.Join(",", acadCredsForCourseOrEquatedCourse.Select(c => c.Id))));

                            // A course on the student's record will generally not be added to the plan, with one exception. The exception is that it  failed to apply to a group due to a
                            // minimum grade failure, and did not successfully apply to a different group.
                            creditApplied = true;
                            //Since we are in logic where student has taken the course; we will change the flag as IsEnrolled but status is NonApplied until we later check if it is in eval results
                            courseEvalResult.IsEnrolled = true;

                            // Check for the exception case
                            if (programEvaluationEntity.RequirementResults.Any())
                            {
                                // There must be evaluation results
                                bool courseOrEquateAppliedSuccessfully = false;
                                bool courseOrEquateFailedMinGrade = false;
                                var appliedCreditsForCourse = appliedEvaluationCredits.Where(apa => apa.GetCourse()!=null && (apa.GetCourse().Id == courseId || apa.GetCourse().EquatedCourseIds.Contains(courseId))).ToList();

                                // At least one applied or planned-applied academic credit, with course ID or equated course ID matching, was found for this course 
                                if (appliedCreditsForCourse.Any())
                                {
                                    logger.Debug(string.Format("Student {0}: course {1} satisfies requirements (via academic credit(s) {2}) for program {3} for catalog {4}.", degreePlan.PersonId,
                                        courseId,
                                        string.Join(",", appliedCreditsForCourse.Select(ac => ac.GetAcadCredId())),
                                        programEvaluationEntity.ProgramCode,
                                        programEvaluationEntity.CatalogCode));
                                    creditApplied = true;
                                    courseOrEquateAppliedSuccessfully = true;
                                    //since now we found that course was applied from eval result; make the status as Applied
                                    courseEvalResult.EvaluationStatus = DegreePlanPreviewCourseEvaluationStatusType.Applied;

                                }

                                var notAppliedDueToExclusionCreditsForCourse = notAppliedEvaluationCredits.Where(apa => apa.GetCourse() != null && (apa.GetCourse().Id == courseId || apa.GetCourse().EquatedCourseIds.Contains(courseId)) && acadResultTypesThatExcludeCreditsFromApplying.Contains(apa.Result)).ToList();
                                if (notAppliedDueToExclusionCreditsForCourse.Any())
                                {
                                    logger.Debug(string.Format("Student {0}: course {1} does not satisfy requirements for program {2} for catalog {3}, but is part of the student's academic history (academic credit(s) {4}) and is excluded based on program requirement exclusions, and will be added in the merged degree plan.", degreePlan.PersonId,
                                        courseId,
                                        programEvaluationEntity.ProgramCode,
                                        programEvaluationEntity.CatalogCode,
                                        string.Join(",", notAppliedDueToExclusionCreditsForCourse.Select(ac => ac.GetAcadCredId()))));

                                    courseOrEquateFailedMinGrade = true;
                                }

                                if (!courseOrEquateAppliedSuccessfully && courseOrEquateFailedMinGrade)
                                {
                                    logger.Debug(string.Format("Student {0}: course {1} either does not satisfy requirements for program {2} for catalog {3}, and is part of the student's academic history (academic credit(s) {4}) or is excluded based on program requirement exclusions, and will be added in the merged degree plan.", degreePlan.PersonId,
                                        courseId,
                                        programEvaluationEntity.ProgramCode,
                                        programEvaluationEntity.CatalogCode,
                                        string.Join(",", notAppliedDueToExclusionCreditsForCourse.Select(ac => ac.GetAcadCredId()))));
                                    creditApplied = false;
                                    //We will only change the status to Mingrade when student took the course but did not met min grade
                                    courseEvalResult.EvaluationStatus = DegreePlanPreviewCourseEvaluationStatusType.MinGrade;
                                }
                            }
                            else
                            {
                                // No program requirements - do not add this course again
                                logger.Debug(string.Format("Student {0}: no requirements for program {1} for catalog {2} - course {3} will not be added in the merged degree plan.", degreePlan.PersonId,
                                    programEvaluationEntity.ProgramCode,
                                    programEvaluationEntity.CatalogCode,
                                    courseId));
                            }
                        }

                        // Course not found in student's academic history or has been excluded
                        if (!acadCredsForCourseOrEquatedCourse.Any() || creditApplied == false)
                        {
                            logger.Debug(string.Format("Student {0}: No Add/New/Transfer/Noncourse academic credits found/applied for course {1}.", degreePlan.PersonId, courseId));

                            //get the course from the cached course catalog to check for any equated courses
                            var course = allCourses.Where(c => c.Id == courseId).FirstOrDefault();
                            var equatedCourses = course != null ? course.EquatedCourseIds.ToList() : new List<string>(); 

                            //get all courses on the degree plan for the term
                            var courses = degreePlan.GetPlannedCourses(degreePlanTermToLoadCourseBlock.Code).Where(p => p.CourseId != null).Select(p => p.CourseId);

                            // check if course or equated course been planned for term - add any course that is not planned for this term even when course is planned for a different term
                            if (courses != null && !courses.Contains(courseId) && !equatedCourses.Intersect(courses).Any())
                            {
                                logger.Debug(string.Format("Student {0}: Loading course Id {1} from course block {2} to term {3} in merged degree plan", degreePlan.PersonId, courseId, courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));

                                //add the course to degree plan
                                degreePlan.AddCourse(new PlannedCourse(course: courseId, section: null, gradingType: GradingType.Graded,
                                    status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: addedBy, addedOn: DateTime.Now,
                                    coursePlaceholder: null), degreePlanTermToLoadCourseBlock.Code);
                            }
                        }
                        //Now add the preview course result
                        DegreePlanPreviewCoursesEvaluation.Add(courseEvalResult);
                        logger.Debug(string.Format("Student {0}: Course {1} from preview degree plan is identified with status of {2} and added to the Degree plan preview evaluation collection", degreePlan.PersonId, courseId, courseEvalResult.EvaluationStatus.ToString()));
                    }

                    //add course placeholders to new preview degree plan term
                    foreach (var coursePlaceholderId in courseBlock.CoursePlaceholderIds)
                    {
                        logger.Debug(string.Format("Student {0}: Loading course placeholder Id {1} from course block {2} to term {3} in sample plan preview", degreePlan.PersonId, coursePlaceholderId, courseBlock.Description, degreePlanTermToLoadCourseBlock.Code));

                        //add the course placeholder from course block to the preview plan term
                        preview.AddCourse(new PlannedCourse(course: null, section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted,
                            addedBy: null, addedOn: DateTimeOffset.Now, coursePlaceholder: coursePlaceholderId), degreePlanTermToLoadCourseBlock.Code);

                        // add the course placeholder to the term when the course placeholder does not already exist in the term
                        var coursePlacholders = degreePlan.GetPlannedCourses(degreePlanTermToLoadCourseBlock.Code).Where(p => p.CoursePlaceholderId != null).Select(p => p.CoursePlaceholderId);
                        if (coursePlacholders != null && !coursePlacholders.Contains(coursePlaceholderId))
                        {
                            //add the course placeholder from course block to student's existing (merged) degree plan
                            degreePlan.AddCourse(new PlannedCourse(course: null, section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted,
                                addedBy: null, addedOn: DateTimeOffset.Now, coursePlaceholder: coursePlaceholderId), degreePlanTermToLoadCourseBlock.Code);
                            logger.Debug(string.Format("Student {0}: Course placeholder {1} added to term {2} in sample plan preview", degreePlan.PersonId, coursePlaceholderId, degreePlanTermToLoadCourseBlock.Code));
                        }
                        else
                        {
                            logger.Debug(string.Format("Student {0}: Course placeholder Id {1} already exists on term {2} in sample plan preview and will not be added again.", degreePlan.PersonId, coursePlaceholderId, degreePlanTermToLoadCourseBlock.Code));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format("Student {0}: Course block could not be loaded to degree plan, validate that there are enough preview terms available to load course blocks to.", degreePlan.PersonId);
                    logger.Error(ex, message);
                }
            }
            Preview = preview;
            MergedDegreePlan = degreePlan;
        }
    }
}
