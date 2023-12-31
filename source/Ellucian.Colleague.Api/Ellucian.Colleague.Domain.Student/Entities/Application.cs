﻿// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Created to contain all information required for a Recruiter
    /// application status change and application/prospect import.
    /// Can be re-used for application related changes to Colleague.
    /// </summary>
    [Serializable]
    public class Application
    {
        public string ErpProspectId { get; set; }
        public string CrmApplicationId { get; set; }
        public string ApplicationStatus { get; set; }
        public string RecruiterOrganizationName { get; set; }
        public string RecruiterOrganizationId { get; set; }
        public string CrmProspectId { get; set; }
        public string Ssn { get; set; }
        public string Sin { get; set; }
        public string AcademicProgram { get; set; }
        public string StartTerm { get; set; }
        public string Prefix { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Nickname { get; set; }
        public string EmailAddress { get; set; }
        public string ImAddress { get; set; }
        public string ImProvider { get; set; }
        public string AddressLines1 { get; set; }
        public string AddressLines2 { get; set; }
        public string AddressLines3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Attention { get; set; }
        public string HomePhone { get; set; }
        public string CellPhone { get; set; }
        public string OtherFirstName { get; set; }
        public string OtherLastName { get; set; }
        public Nullable<DateTime> BirthDate { get; set; }
        public string Gender { get; set; }
        public string Ethnicity { get; set; }
        public string Race1 { get; set; }
        public string Race2 { get; set; }
        public string Race3 { get; set; }
        public string Race4 { get; set; }
        public string Race5 { get; set; }
        public string Disability { get; set; }
        public string MaritalStatus { get; set; }
        public string Denomination { get; set; }
        public string Veteran { get; set; }
        public string CitizenshipStatus { get; set; }
        public string BirthCity { get; set; }
        public string BirthState { get; set; }
        public string BirthCountry { get; set; }
        public string PrimaryLanguage { get; set; }
        public string Citizenship { get; set; }
        public string ForeignRegistrationId { get; set; }
        public string VisaType { get; set; }
        public Nullable<DateTime> CountryEntryDate { get; set; }
        public string TempAddressLines1 { get; set; }
        public string TempAddressLines2 { get; set; }
        public string TempAddressLines3 { get; set; }
        public string TempCity { get; set; }
        public string TempState { get; set; }
        public string TempZip { get; set; }
        public string TempCountry { get; set; }
        public string TempAttention { get; set; }
        public string TempPhone { get; set; }
        public Nullable<DateTime> TempStartDate { get; set; }
        public Nullable<DateTime> TempEndDate { get; set; }
        public string CareerGoal { get; set; }
        public string EducationalGoal { get; set; }
        public string DecisionPlan { get; set; }
        public string CourseLoad { get; set; }
        public string FaPlan { get; set; }
        public string HousingPlan { get; set; }
        public string DecisionFactor1 { get; set; }
        public string DecisionFactor2 { get; set; }
        public string ParentMaritalStatus { get; set; }
        public string Parent1RelationType { get; set; }
        public string Parent1Prefix { get; set; }
        public string Parent1FirstName { get; set; }
        public string Parent1MiddleName { get; set; }
        public string Parent1LastName { get; set; }
        public string Parent1Suffix { get; set; }
        public string Parent1Address1 { get; set; }
        public string Parent1Address2 { get; set; }
        public string Parent1Address3 { get; set; }
        public string Parent1City { get; set; }
        public string Parent1State { get; set; }
        public string Parent1Zip { get; set; }
        public string Parent1Country { get; set; }
        public string Parent1Phone { get; set; }
        public string Parent1EmailAddress { get; set; }
        public Nullable<DateTime> Parent1BirthDate { get; set; }
        public string Parent1BirthCountry { get; set; }
        public string Parent1Living { get; set; }
        public string Parent1SameAddress { get; set; }
        public string Parent2RelationType { get; set; }
        public string Parent2Prefix { get; set; }
        public string Parent2FirstName { get; set; }
        public string Parent2MiddleName { get; set; }
        public string Parent2LastName { get; set; }
        public string Parent2Suffix { get; set; }
        public string Parent2Address1 { get; set; }
        public string Parent2Address2 { get; set; }
        public string Parent2Address3 { get; set; }
        public string Parent2City { get; set; }
        public string Parent2State { get; set; }
        public string Parent2Zip { get; set; }
        public string Parent2Country { get; set; }
        public string Parent2Phone { get; set; }
        public string Parent2EmailAddress { get; set; }
        public Nullable<DateTime> Parent2BirthDate { get; set; }
        public string Parent2BirthCountry { get; set; }
        public string Parent2Living { get; set; }
        public string Parent2SameAddress { get; set; }
        public string GuardianRelationType { get; set; }
        public string GuardianPrefix { get; set; }
        public string GuardianFirstName { get; set; }
        public string GuardianMiddleName { get; set; }
        public string GuardianLastName { get; set; }
        public string GuardianSuffix { get; set; }
        public string GuardianAddress1 { get; set; }
        public string GuardianAddress2 { get; set; }
        public string GuardianCity { get; set; }
        public string GuardianState { get; set; }
        public string GuardianZip { get; set; }
        public string GuardianCountry { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianEmailAddress { get; set; }
        public Nullable<DateTime> GuardianBirthDate { get; set; }
        public string GuardianBirthCountry { get; set; }
        public string GuardianSameAddress { get; set; }
        public string RelationTypeSibling1 { get; set; }
        public string PrefixSibling1 { get; set; }
        public string FirstNameSibling1 { get; set; }
        public string MiddleNameSibling1 { get; set; }
        public string LastNameSibling1 { get; set; }
        public string SuffixSibling1 { get; set; }
        public Nullable<DateTime> BirthDateSibling1 { get; set; }
        public string RelationTypeSibling2 { get; set; }
        public string PrefixSibling2 { get; set; }
        public string FirstNameSibling2 { get; set; }
        public string MiddleNameSibling2 { get; set; }
        public string LastNameSibling2 { get; set; }
        public string SuffixSibling2 { get; set; }
        public Nullable<DateTime> BirthDateSibling2 { get; set; }
        public string RelationTypeSibling3 { get; set; }
        public string PrefixSibling3 { get; set; }
        public string FirstNameSibling3 { get; set; }
        public string MiddleNameSibling3 { get; set; }
        public string LastNameSibling3 { get; set; }
        public string SuffixSibling3 { get; set; }
        public Nullable<DateTime> BirthDateSibling3 { get; set; }
        public string EmergencyPrefix { get; set; }
        public string EmergencyFirstName { get; set; }
        public string EmergencyMiddleName { get; set; }
        public string EmergencyLastName { get; set; }
        public string EmergencySuffix { get; set; }
        public string EmergencyPhone { get; set; }
        public string Activity { get; set; }
        public string Part9Activity { get; set; }
        public string Part10Activity { get; set; }
        public string Part11Activity { get; set; }
        public string Part12Activity { get; set; }
        public string PartPgActivity { get; set; }
        public string FutureActivity { get; set; }
        public string HoursWeekActivity { get; set; }
        public string WeeksYearActivity { get; set; }
        public string HighSchoolCeebs { get; set; }
        public string HighSchoolNonCeebInfo { get; set; }
        public string HighSchoolAttendFromYears { get; set; }
        public string HighSchoolAttendFromMonths { get; set; }
        public string HighSchoolAttendToYears { get; set; }
        public string HighSchoolAttendToMonths { get; set; }
        public string CollegeCeebs { get; set; }
        public string CollegeNonCeebInfo { get; set; }
        public string CollegeAttendFromYears { get; set; }
        public string CollegeAttendFromMonths { get; set; }
        public string CollegeAttendToYears { get; set; }
        public string CollegeAttendToMonths { get; set; }
        public string CollegeDegrees { get; set; }
        public string CollegeDegreeDates { get; set; }
        public string CollegeHoursEarned { get; set; }
        public string GuardianAddress3 { get; set; }
        public string AdmitType { get; set; }
        public string ProspectSource { get; set; }
        public string HighSchoolNames { get; set; }
        public string CollegeNames { get; set; }
        public string Comments { get; set; }
        public string Misc1 { get; set; }
        public string Misc2 { get; set; }
        public string Misc3 { get; set; }
        public string Misc4 { get; set; }
        public string Misc5 { get; set; }
        public string ApplicationUser1 { get; set; }
        public string ApplicationUser2 { get; set; }
        public string ApplicationUser3 { get; set; }
        public string ApplicationUser4 { get; set; }
        public string ApplicationUser5 { get; set; }
        public string ApplicationUser6 { get; set; }
        public string ApplicationUser7 { get; set; }
        public string ApplicationUser8 { get; set; }
        public string ApplicationUser9 { get; set; }
        public string ApplicationUser10 { get; set; }
        public string ApplicantUser1 { get; set; }
        public string ApplicantUser2 { get; set; }
        public string ApplicantUser3 { get; set; }
        public string ApplicantUser4 { get; set; }
        public string ApplicantUser5 { get; set; }
        public string ApplicantUser6 { get; set; }
        public string ApplicantUser7 { get; set; }
        public string ApplicantUser8 { get; set; }
        public string ApplicantUser9 { get; set; }
        public string ApplicantUser10 { get; set; }
        public string CustomFieldsXML { get; set; }
        public string Location { get; set; }
        public Nullable<DateTime> SubmittedDate { get; set; }
        public string HighSchoolGraduated { get; set; }
        public string HighSchoolTranscriptStored { get; set; }
        public string HighSchoolTranscriptLocation { get; set; }
        public string HighSchoolTranscriptGpa { get; set; }
        public string HighSchoolTranscriptClassPercentage { get; set; }
        public string HighSchoolTranscriptClassRank { get; set; }
        public string HighSchoolTranscriptClassSize { get; set; }
        public string CollegeGraduated { get; set; }
        public string CollegeTranscriptStored { get; set; }
        public string CollegeTranscriptLocation { get; set; }
        public string CollegeTranscriptGpa { get; set; }
        public string CollegeTranscriptClassPercentage { get; set; }
        public string CollegeTranscriptClassRank { get; set; }
        public string CollegeTranscriptClassSize { get; set; }
        public string ApplicantCounty { get; set; }
        public string ResidencyStatus { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public Nullable<DateTime> ApplicationStatusDate { get; set; }
    }
}
