// Copyright 2018 Ellucian Company L.P. and its affiliates.
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Permissions for a faculty 
    /// </summary>
    [Serializable]
    public class FacultyPermissions
    {
        private readonly bool canWaivePrerequisiteRequirement;
        private readonly bool canGrantFacultyConsent;
        private readonly bool canGrantStudentPetition;
        private readonly bool canUpdateGrades;
        private readonly bool canSearchStudents;

        /// <summary>
        /// Faculty can waive prerequisite requirements for their sections
        /// </summary>
        public bool CanWaivePrerequisiteRequirement { get { return canWaivePrerequisiteRequirement; } }

        /// <summary>
        /// Faculty can grant faculty consent to take their sections
        /// </summary>
        public bool CanGrantFacultyConsent { get { return canGrantFacultyConsent; } }

        /// <summary>
        /// FAculty can grant student petitions to take their sections
        /// </summary>
        public bool CanGrantStudentPetition { get { return canGrantStudentPetition; } }

        /// <summary>
        /// Faculty can enter and update grade information for their sections
        /// </summary>
        public bool CanUpdateGrades { get { return canUpdateGrades; } }

        /// <summary>
        /// Faculty can search for students in the sections
        /// </summary>
        public bool CanSearchStudents { get { return canSearchStudents; } }

        /// <summary>
        /// Creates a new <see cref="FacultyPermissions"/> object.
        /// </summary>
        public FacultyPermissions()
        {
            canWaivePrerequisiteRequirement = false;
            canGrantFacultyConsent = false;
            canGrantStudentPetition = false;
            canUpdateGrades = false;
            canSearchStudents = false;
        }

        /// <summary>
        /// Creates a new <see cref="FacultyPermissions"/> object and sets permissions based on the supplied permission codes
        /// </summary>
        /// <param name="permissionCodes">Permission codes from which faculty permissions will be built</param>
        /// <param name="logger">Logging interface</param>
        public FacultyPermissions(IEnumerable<string> permissionCodes)
        {
            if (permissionCodes == null)
            {
                throw new ArgumentNullException("Collection of permission codes cannot be null when building faculty permissions.");
            }

            if (permissionCodes.Contains(StudentPermissionCodes.CreatePrerequisiteWaiver))
            {
                canWaivePrerequisiteRequirement = true;
            }

            if (permissionCodes.Contains(StudentPermissionCodes.CreateFacultyConsent))
            {
                canGrantFacultyConsent = true;
            }

            if (permissionCodes.Contains(StudentPermissionCodes.CreateStudentPetition))
            {
                canGrantStudentPetition = true;
            }

            if (permissionCodes.Contains(StudentPermissionCodes.UpdateGrades))
            {
                canUpdateGrades = true;
            }

            if (permissionCodes.Contains(StudentPermissionCodes.ViewStudentInformation))
            {
                canSearchStudents = true;
            }

        }
    }
}
