// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using System;

namespace Ellucian.Colleague.Domain.Student
{
    [Serializable]
    public static class StudentPermissionCodes
    {
        //Access to view applications
        public const string ViewApplications = "VIEW.APPLICATIONS";

        //UPDATE.APPLICATIONS
        // Permissions code that allows an external system to create/update admission applications.      
        public const string UpdateApplications = "UPDATE.APPLICATIONS";

        //Access to view instructor
        public const string ViewInstructors = "VIEW.INSTRUCTORS";

        // Access to view student academic period profile
        public const string ViewStudentAcademicPeriodProfile = "VIEW.STUDENT.ACADEMIC.PERIOD.PROFILE";

        // Access to view student information
        public const string ViewStudentInformation = "VIEW.STUDENT.INFORMATION";
        
        // Access to view person information
        public const string ViewPersonInformation = "VIEW.PERSON.INFORMATION";

        // Permission to view financial aid information
        public const string ViewFinancialAidInformation = "VIEW.FINANCIAL.AID.INFORMATION";

        // Permission to create and update courses
        public const string CreateAndUpdateCourse = "CREATE.UPDATE.COURSE";

        // Permission to create and update sections
        public const string CreateAndUpdateSection = "CREATE.UPDATE.SECTION";

        // Permission to create/update room bookings
        public const string CreateAndUpdateRoomBooking = "UPDATE.ROOM.BOOKING";

        // Permission to create/update faculty bookings
        public const string CreateAndUpdateFacultyBooking = "UPDATE.FACULTY.BOOKING";

        // Permissiong to create a prerequiste waiver
        public const string CreatePrerequisiteWaiver = "CREATE.PREREQUISITE.WAIVER";

        // Permission to create a new student petition
        public const string CreateStudentPetition = "CREATE.STUDENT.PETITION";

        //Permission to create a new faculty consent
        public const string CreateFacultyConsent = "CREATE.FACULTY.CONSENT";

        //Permission to create/update student academic program
        public const string CreateStudentAcademicProgramConsent = "CREATE.UPDATE.STUDENT.ACADEMIC.PROGRAM";

        //Permission to replace student academic program
        public const string ReplaceStudentAcademicProgram = "REPLACE.STUDENT.ACADEMIC.PROGRAM";

        //Permission to create/update an academic program enrollment
        public const string ViewStudentAcademicProgramConsent = "VIEW.STUDENT.ACADEMIC.PROGRAM";

        //Permission to Delete an academic program enrollment
        public const string DeleteStudentAcademicProgramConsent = "DELETE.STUDENT.ACADEMIC.PROGRAM";

        // Permission to update student grades by faculty
        public const string UpdateGrades = "UPDATE.GRADES";

        // Permission to view student charges
        public const string ViewStudentCharges = "VIEW.STUDENT.CHARGES";

        // Permission to view student payments
        public const string ViewStudentPayments = "VIEW.STUDENT.PAYMENTS";

        // Permission to create student charges
        public const string CreateStudentCharges = "CREATE.STUDENT.CHARGES";

        // Permission to create student payments
        public const string CreateStudentPayments = "CREATE.STUDENT.PAYMENTS";

        // Permission to view student academic standings
        public const string ViewStudentAcadStandings = "VIEW.STUDENT.ACAD.STANDINGS";

        // Permission to view student aptitude Assessments
        public const string ViewStudentAptitudeAssessmentsConsent = "VIEW.STUDENT.TEST.SCORES";

        // Permission to create/update student aptitude Assessments
        public const string CreateStudentAptitudeAssessmentsConsent = "UPDATE.STUDENT.TEST.SCORES";

        // Permission to delete student aptitude Assessment
        public const string DeleteStudentAptitudeAssessmentsConsent = "DELETE.STUDENT.TEST.SCORES";

        // Permission to view student advisor relationships
        public const string ViewStudentAdivsorRelationships = "VIEW.STU.ADV.RELATIONSHIPS";

        // Permission to view any student-meal-plans information
        public const string ViewMealPlanAssignment = "VIEW.MEAL.PLAN.ASSIGNMENT";

        // Permission to update any student-meal-plans information
        public const string CreateMealPlanAssignment = "CREATE.MEAL.PLAN.ASSIGNMENT";

        // Permission to view any student-meal-plans information
        public const string ViewMealPlanRequest = "VIEW.MEAL.PLAN.REQUEST";

        // Permission to update any student-meal-plans information
        public const string CreateMealPlanRequest = "CREATE.MEAL.PLAN.REQUEST";

        //Permission to view housing requests
        public const string ViewHousingRequest = "VIEW.HOUSING.REQS";

        // Permission to create/update housing requests
        public const string CreateHousingRequest = "UPDATE.HOUSING.REQS";

        // Permission to update any student-registration-eligibilities information
        public const string ViewStuRegistrationEligibility = "VIEW.STU.REGISTRATION.ELIGIBILITY";

        //Permission to view any housing-assignments information.
        public const string ViewHousingAssignment = "VIEW.ROOM.ASSIGNMENT";

        //Permission to create/update any housing-assignments information.
        public const string CreateUpdateHousingAssignment = "UPDATE.ROOM.ASSIGNMENT";

        //Permission to view any section-instructors information.
        public const string ViewSectionInstructors = "VIEW.SECTION.INSTRUCTORS";

        //Permission to create/update any section-instructors information.
        public const string CreateSectionInstructors = "UPDATE.SECTION.INSTRUCTORS";

        //Permission to delete any section-instructors information.
        public const string DeleteSectionInstructors = "DELETE.SECTION.INSTRUCTORS";

        //Permission to view any student-section-waitlists information
        public const string ViewStudentSectionWaitlist = "VIEW.STUDENT.SECTION.WAITLIST";

        //Permission to view any student-section-waitlists information
        public const string ViewStudentCourseTransfers = "VIEW.STUDENT.COURSE.TRANSFERS";

        //VIEW.ADMISSION.DECISIONS
        //Permissions code that allows an external system to do a READ operation.
        public const string ViewAdmissionDecisions = "VIEW.ADMISSION.DECISIONS";

        //UPDATE.ADMISSION.DECISIONS
        // Permissions code that allows an external system to create admission applications. Update is not allowed for admission decision.        
        public const string UpdateAdmissionDecisions = "UPDATE.ADMISSION.DECISIONS";

        // Enables access to view External Education
        public const string ViewExternalEducation = "VIEW.EXTERNAL.EDUCATION";

        // Enables access to create External Education
        public const string CreateExternalEducation = "UPDATE.EXTERNAL.EDUCATION";

        // Enables access to view External Education Credentials
        public const string ViewExternalEducationCredentials = "VIEW.EXTERNAL.EDUC.CREDENTIALS";

        // Enables access to create External Education Credentials
        public const string CreateExternalEducationCredentials = "UPDATE.EXTERNAL.EDUC.CREDENTIALS";

        // Enables access to create External Education Credentials
        public const string ViewApplicationSupportingItems = "VIEW.APPL.SUPPORTING.ITEMS";

        // Enables access to create External Education Credentials
        public const string UpdateApplicationSupportingItems = "UPDATE.APPL.SUPPORTING.ITEMS";

        // Enables access to view Person External Education
        public const string ViewPerExternalEducation = "VIEW.PER.EXTERNAL.EDUCATION";

        // Enables access to create Person External Education
        public const string CreatePerExternalEducation = "UPDATE.PER.EXTERNAL.EDUCATION";

        // Enables access to view Person External Education Credentials
        public const string ViewPerExternalEducationCredentials = "VIEW.PER.EXT.EDUC.CREDENTIAL";

        // Enables access to create Person External Education Credentials
        public const string CreatePerExternalEducationCredentials = "UPDATE.PER.EXT.EDUC.CREDENTIAL";

        // Enables access to view Campus Involvements
        public const string ViewCampusInvolvements = "VIEW.CAMPUS.ORG.MEMBERS";

        /// <summary>
        /// Enables access to the user's own 1098 tax form data
        /// </summary>
        public const string View1098 = BasePermissionCodes.View1098;

        /// <summary>
        /// Enables access to other users' 1098 data (ie: Tax Information Admin)
        /// </summary>
        public const string ViewStudent1098 = BasePermissionCodes.ViewStudent1098;

        /// <summary>
        /// Enables access to the user's own T2202A tax form data
        /// </summary>
        public const string ViewT2202A = BasePermissionCodes.ViewT2202A;

        /// <summary>
        /// Enables access to other users' T2202A data (ie: Tax Information Admin)
        /// </summary>
        public const string ViewStudentT2202A = BasePermissionCodes.ViewStudentT2202A;

        // Access to view student financial aid progress statuses
        public const string ViewStudentFinancialAidAcadProgress = "VIEW.STU.FA.ACAD.PROGRESS";

        // Access to view financial aid applications
        public const string ViewFinancialAidApplications = "VIEW.FA.APPLICATIONS";

        // Access to view financial aid application outcomes
        public const string ViewFinancialAidApplicationOutcomes = "VIEW.FA.APPLICATION.OUTCOMES";

        // Access to view student unverified grades
        public const string ViewStudentUnverifiedGrades = "VIEW.STUDENT.UNVERIFIED.GRADES";

        // Enables access to view student transcript grades
        public const string ViewStudentTranscriptGrades = "VIEW.STUDENT.TRANSCRIPT.GRADES";

        // Permissions code that allows an external system to perform the READ operation.
        public const string ViewStudentCohortAssignments = "VIEW.STUDENT.COHORT.ASSIGNMENTS";

        // Enables access to update student transcript grades
        public const string UpdateStudentTranscriptGradesAdjustments = "UPDATE.STUDENT.TRANSCRIPT.GRADES.ADJUSTMENTS";

        // Access to view student grade point averages
        public const string ViewStudentGradePointAverages = "VIEW.STUDENT.GRADE.POINT.AVERAGES";

        // Access to view student financial aid need summaries
        public const string ViewStudentFinancialAidNeedSummaries = "VIEW.STU.FA.NEED.SUMMARIES";

        // Access to view (unrestricted) student financial aid awards
        public const string ViewStudentFinancialAidAwards = "VIEW.STUDENT.FA.AWARDS";

        // Access to view retricted student financial aid awards
        public const string ViewRestrictedStudentFinancialAidAwards = "VIEW.RES.STU.FA.AWARDS";

        //Enables access to update student unverified grades
        public const string ViewStudentUnverifiedGradesSubmissions = "UPDATE.STUDENT.UNVERIFIED.GRADES.SUBMISSIONS";

        // Access to view student-academic-periods.
        public const string ViewStudentAcademicPeriods = "VIEW.STUDENT.ACADEMIC.PERIODS";

        //Permissions code that allows an external system to perform the READ operation.
        public const string ViewProspectOpportunity = "VIEW.PROSPECT.OPPORTUNITY";

        //Permissions code that allows an external system to perform the CREATE and UPDATE operations using prospect-opportunities-submissions and therefore 
        //also provides the same permissions as VIEW.PROSPECT.OPPORTUNITY.
        public const string UpdateProspectOpportunity = "UPDATE.PROSPECT.OPPORTUNITY";

        //Permissions code that allows an external system to perform the READ operation.
        public const string ViewStudentAcademicCredentials = "VIEW.STUDENT.ACADEMIC.CREDENTIALS";

        //Permissions code that allows users to perform instant enrollment operations.
        public const string InstantEnrollmentAllowAll = "IE.ALLOW.ALL";

        //Permission code that allows users to delete, update, sync courses and sections for Portal functionality.
        public const string PortalCatalogAdmin = "PORTAL.CATALOG.ADMIN";

        // Permission to allow faculty to drop students from sections.
        public const string CanDropStudent = "DROP.STUDENT";

        // Permission to create/update instructional events
        public const string UpdateInstructionalEvent = "UPDATE.INSTRUCTIONAL.EVENT";

        // Permission to delete instructional events
        public const string DeleteInstructionalEvent = "DELETE.INSTRUCTIONAL.EVENT";

        // Permission to check the success of failure of a proposed registration for any student
        public const string RegisterValidationOnly = "REGISTER.VALIDATION.ONLY";

        // Permission to register any student, bypassing validations. For cross-registration
        public const string RegisterSkipValidation = "REGISTER.SKIP.VALIDATION";

        // Enables access to update student information
        public const string UpdateStudentInformation = "UPDATE.STUDENT.INFORMATION";

        // Permission to view financial aid application demographics
        public const string ViewAidApplicationDemographics = "VIEW.AID.APPL.DEMO";

        // Permission to create/update financial aid application demographics
        public const string UpdateAidApplicationDemographics = "UPDATE.AID.APPL.DEMO";

        // Permission to view financial aid application additional information
        public const string ViewAidApplicationAdditionalInfo = "VIEW.AID.APPL.ADDITIONAL";

        // Permission to view financial aid applications
        public const string ViewAidApplications = "VIEW.AID.APPLICATIONS";

        // Permission to view financial aid application additional information
        public const string UpdateAidApplicationAdditionalInfo = "UPDATE.AID.APPL.ADDITIONAL";

        // Permission to create/update financial aid applications
        public const string UpdateAidApplications = "UPDATE.AID.APPLICATIONS";

        // Permission to view financial aid application results
        public const string ViewAidApplicationResults = "VIEW.AID.APPLICATION.RESULTS";

        // Permission to view financial aid application results
        public const string UpdateAidApplicationResults = "UPDATE.AID.APPLICATION.RESULTS";

    }
}