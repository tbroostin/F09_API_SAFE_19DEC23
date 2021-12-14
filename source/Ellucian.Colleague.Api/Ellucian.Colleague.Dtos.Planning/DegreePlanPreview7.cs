// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Contains information related to previewing a sample degree plan against a student degree plan.
    /// Both the limited preview of the plan and a merged student degree plan are included.
    /// </summary>
    public class DegreePlanPreview7
    {
        /// <summary>
        /// Limited degree plan containing only the courses from the sample plan
        /// </summary>
        public DegreePlan4 Preview { get; set; }

        /// <summary>
        /// Contains the student's degree plan merged with the sample degree plan - in a state that is ready to update.
        /// </summary>
        public DegreePlan4 MergedDegreePlan { get; set; }

        /// <summary>
        /// This contains the list of all the courses listed in preview degree plan with its evaluation status for the program for which sample plan is loaded.
        /// For example: A program has sample plan to load certain course in a term. After this program is evaluated the course  could have been applied or not applied or not applied due to min grade. 
        /// This list have one to one correspondance with course in preview in order to identify its evaluation status.
        /// </summary>
        public List<DegreePlanPreviewCourseEvaluationResult> DegreePlanPreviewCoursesEvaluation { get; set; }

    }
}
