// Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    }
}
