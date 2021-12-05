// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.DegreePlans;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// This identifies the course displayed in preview degree plan along with its program evaluation status. 
    /// </summary>
    public class DegreePlanPreviewCourseEvaluationResult
    {
        /// <summary>
        /// Limited degree plan containing only the courses from the sample plan
        /// </summary>
        public string CourseId { get; set; }
        /// <summary>
        /// Term in which the course is previewed.
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// This identifies if student is enrolled in course.
        /// </summary>
        public bool IsEnrolled { get; set; }

        /// <summary>
        /// Contains the status of the course in preview degree plan. For example: if the course was already applied or was not applied or was not applied due to Min grade failure when sample plan was loaded for particular program.
        /// </summary>
        public DegreePlanPreviewCourseEvaluationStatusType EvaluationStatus { get; set; }

    }
}
