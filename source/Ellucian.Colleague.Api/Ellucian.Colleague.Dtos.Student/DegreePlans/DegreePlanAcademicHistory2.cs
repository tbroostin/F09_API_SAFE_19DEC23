// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// Combined result provides a student's Degree Plan (v4) and Academic History (v3)
    /// </summary>
    public class DegreePlanAcademicHistory2
    {
        /// <summary>
        /// Student's Degree Plan
        /// </summary>
        public DegreePlan4 DegreePlan { get; set; }

        /// <summary>
        /// Student's Academic History
        /// </summary>
        public AcademicHistory3 AcademicHistory { get; set; }
    }
}
