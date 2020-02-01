// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// Combined result provides a student's Degree Plan (v4) and Academic History (v3)
    /// </summary>
    public class DegreePlanAcademicHistory3
    {
        /// <summary>
        /// Student's Degree Plan
        /// </summary>
        public DegreePlan4 DegreePlan { get; set; }

        /// <summary>
        /// Student's Academic History
        /// </summary>
        public AcademicHistory4 AcademicHistory { get; set; }
    }
}
