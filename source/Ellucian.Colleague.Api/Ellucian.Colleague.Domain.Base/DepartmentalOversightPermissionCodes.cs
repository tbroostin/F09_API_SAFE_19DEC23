// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base
{
    /// <summary>
    /// Departmental oversight permission codes
    /// </summary>
    [Serializable]
    public static class DepartmentalOversightPermissionCodes
    {
        // View faculty Section Information 
        public const string ViewStudentInformation = "VIEW.STUDENT.INFORMATION";
        public const string ViewSectionRoster = "VIEW.SECTION.ROSTER";
        public const string ViewSectionAttendance = "VIEW.SECTION.ATTENDANCE";
        public const string ViewSectionDropRoster = "VIEW.SECTION.DROP.ROSTER";
        public const string ViewSectionCensus = "VIEW.SECTION.CENSUS";
        public const string ViewSectionGrading = "VIEW.SECTION.GRADING";
        public const string ViewSectionBooks = "VIEW.SECTION.BOOKS";
        public const string ViewSectionPrerequisiteWaiver = "VIEW.SECTION.PREREQUISITE.WAIVER";
        public const string ViewSectionStudentPetitions = "VIEW.SECTION.STUDENT.PETITIONS";
        public const string ViewSectionFacultyConsents = "VIEW.SECTION.FACULTY.CONSENTS";
        public const string ViewSectionAddAuthorizations = "VIEW.SECTION.ADD.AUTHORIZATIONS";
        public const string ViewSectionWaitlists = "VIEW.SECTION.WAITLISTS";

        // CRUD permissions for section 
        public const string CreateSectionBooks = "CREATE.SECTION.BOOKS";
        public const string CreateSectionAddAuthorization = "CREATE.SECTION.ADD.AUTHORIZATION";
        public const string CreateSectionGrading = "CREATE.SECTION.GRADING";
        public const string CreateSectionPrerequisiteWaiver = "CREATE.SECTION.REQUISITE.WAIVER";
        public const string CreateSectionStudentPetition = "CREATE.SECTION.STUDENT.PETITION";
        public const string CreateSectionFacultyConsent = "CREATE.SECTION.FACULTY.CONSENT";
        public const string CreateSectionAttendance = "CREATE.SECTION.ATTENDANCE";
        public const string CreateSectionCensus = "CREATE.SECTION.CENSUS";
        public const string CreateSectionDropRoster = "CREATE.SECTION.DROP.STUDENT";
    }
}
