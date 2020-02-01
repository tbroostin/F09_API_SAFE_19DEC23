// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.DegreePlans;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Contains information related to previewing a sample degree plan against a student degree plan.
    /// Both the limited preview of the plan and a merged student degree plan are included.
    /// </summary>
    public class DegreePlanPreview2
    {
        /// <summary>
        /// Limited degree plan containing only the courses from the sample plan
        /// </summary>
        public DegreePlan2 Preview{ get; set; }

        /// <summary>
        /// Contains the student's degree plan merged with the sample degree plan - in a state that is ready to update.
        /// </summary>
        public DegreePlan2 MergedDegreePlan { get; set; }
    }
}
