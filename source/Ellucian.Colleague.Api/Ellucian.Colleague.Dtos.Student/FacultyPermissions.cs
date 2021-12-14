// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Permissions that an faculty has 
    /// </summary>
    public class FacultyPermissions
    {
        /// <summary>
        /// Faculty can waive prerequisite requirements for their sections
        /// </summary>
        public bool CanWaivePrerequisiteRequirement { get; set; }

        /// <summary>
        /// Faculty can grant faculty consent to take their sections
        /// </summary>
        public bool CanGrantFacultyConsent { get; set; }

        /// <summary>
        /// Faculty can grant student petitions to take their sections
        /// </summary>
        public bool CanGrantStudentPetition { get; set; }

        /// <summary>
        /// Faculty can enter and update grade information for their sections
        /// </summary>
        public bool CanUpdateGrades { get; set; }

        /// <summary>
        /// Faculty can search for students in the sections
        /// </summary>
        public bool CanSearchStudents { get; set; }

        /// <summary>
        /// Faculty can drop a student from their sectionss
        /// </summary>
        public bool CanDropStudent { get; set; }

        /// <summary>
        /// Faculty is elibigle to drop a student from their sections
        /// </summary>
        public bool IsEligibleToDrop { get; set; }

        /// <summary>
        /// Faculty has registration overrides in the sections
        /// </summary>
        public bool HasEligibilityOverrides { get; set; }
    }
}
