// Copyright 2018 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Permissions that an advisor has with respect to advisees
    /// </summary>
    public class AdvisingPermissions
    {
        /// <summary>
        /// Advisor can view their assigned advisees' degree plans
        /// </summary>
        public bool CanViewAssignedAdvisees { get; set; }

        /// <summary>
        /// Advisor can review their assigned advisees degree plans
        /// </summary>
        public bool CanReviewAssignedAdvisees { get; set; }

        /// <summary>
        /// Advisor can update their assigned advisees' data
        /// </summary>
        public bool CanUpdateAssignedAdvisees { get; set; }

        /// <summary>
        /// Advisor can view, review, and update their assigned advisees' data, and can register assigned advisees for classes
        /// </summary>
        public bool HasFullAccessForAssignedAdvisees { get; set; }

        /// <summary>
        /// Advisor can view any advisee's degree plan
        /// </summary>
        public bool CanViewAnyAdvisee { get; set; }

        /// <summary>
        /// Advisor can review any advisee's degree plan
        /// </summary>
        public bool CanReviewAnyAdvisee { get; set; }

        /// <summary>
        /// Advisor can update any advisee's degree plan
        /// </summary>
        public bool CanUpdateAnyAdvisee { get; set; }

        /// <summary>
        /// Advisor can view, review, and update any advisee's degree plan, and can register any advisee for classes
        /// </summary>
        public bool HasFullAccessForAnyAdvisee { get; set; }

        /// <summary>
        /// Advisor can create or update student academic programs
        /// </summary>
        public bool CanCreateStudentAcademicProgram { get; set; }
    }
}
