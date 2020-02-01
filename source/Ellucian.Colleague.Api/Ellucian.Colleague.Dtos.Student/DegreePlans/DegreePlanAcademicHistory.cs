// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// Combined result provides a student's Degree Plan (v3) and Academic History (v2)
    /// </summary>
    public class DegreePlanAcademicHistory
    {
        /// <summary>
        /// Student's Degree Plan
        /// </summary>
        public DegreePlan4 DegreePlan { get; set; }

        /// <summary>
        /// Student's Academic History
        /// </summary>
        public AcademicHistory2 AcademicHistory { get; set; }
    }
}
