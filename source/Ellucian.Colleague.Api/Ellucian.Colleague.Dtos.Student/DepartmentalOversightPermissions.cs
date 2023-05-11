// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Departmental oversight permissions dto 
    /// </summary>
    public class DepartmentalOversightPermissions
    {
        /// <summary>
        /// Indicates whether the user can view section roster
        /// </summary>
        public bool CanViewSectionRoster { get; set; }

        /// <summary>
        /// Indicates whether the user can view section attendance
        /// </summary>
        public bool CanViewSectionAttendance { get; set; }
        
        /// <summary>
        /// Indicates whether the user can view section drop roster
        /// </summary>
        public bool CanViewSectionDropRoster { get; set; }

        /// <summary>
        /// Indicates whether the user can view section census
        /// </summary>
        public bool CanViewSectionCensus { get; set; }

        /// <summary>
        /// Indicates whether the user can view section grading
        /// </summary>
        public bool CanViewSectionGrading { get; set; }

        /// <summary>
        /// Indicates whether the user can view section books
        /// </summary>
        public bool CanViewSectionBooks { get; set; }

        /// <summary>
        /// Indicates whether the user can view section prerequisite waiver
        /// </summary>
        public bool CanViewSectionPrerequisiteWaiver { get; set; }

        /// <summary>
        /// Indicates whether the user can view section student petitions
        /// </summary>
        public bool CanViewSectionStudentPetitions { get; set; }

        /// <summary>
        /// Indicates whether the user can view section faculty consents
        /// </summary>
        public bool CanViewSectionFacultyConsents { get; set; }

        /// <summary>
        /// Indicates whether the user can view section add authorizations
        /// </summary>
        public bool CanViewSectionAddAuthorizations { get; set; }

        /// <summary>
        /// Indicates whether the user can view section waitlists
        /// </summary>
        public bool CanViewSectionWaitlists { get; set; }

        /// <summary>
        /// Indicates whether the user can create/modify section books
        /// </summary>
        public bool CanCreateSectionBooks { get; set; }

        /// <summary>
        /// Indicates whether the user can create/modify section add authorization
        /// </summary>
        public bool CanCreateSectionAddAuthorization { get; set; }

        /// <summary>
        /// Indicates whether the user can create/modify section grading
        /// </summary>
        public bool CanCreateSectionGrading { get; set; }
        /// <summary>
        /// Indicates whether the user can create section prerequiste waiver
        /// </summary>
        public bool CanCreateSectionPrerequisiteWaiver { get; set; }
        /// <summary>
        /// Indicates whether the user can create/modify section student petition
        /// </summary>
        public bool CanCreateSectionStudentPetition { get; set; }
        /// <summary>
        /// Indicates whether the user can create/modify section faculty consent
        /// </summary>
        public bool CanCreateSectionFacultyConsent { get; set; }
        /// <summary>
        /// Indicates whether the user can search for students in the sections
        /// </summary>
        public bool CanSearchStudents { get; set; }
        /// <summary>
        /// Indicates whether the user can create/modify section attendance
        /// </summary>
        public bool CanCreateSectionAttendance { get; set; }

        /// <summary>
        /// Indicates whether the user can create/modify section census
        /// </summary>
        public bool CanCreateSectionCensus { get; set; }

        /// <summary>
        /// Indicates whether the user is eligible to drop/modify a student from their sections
        /// </summary>
        public bool CanCreateSectionDropRoster { get; set; }





    }
}
