// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Created to contain all information required for a Recruiter
    /// application status change and application/prospect import.
    /// Can be re-used for application related changes to Colleague.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// ERP prospect ID (ID in PERSON)
        /// </summary>
        public string ErpProspectId { get; set; }

        /// <summary>
        /// CRM application GUID
        /// </summary>
        public string CrmApplicationId { get; set; }
        
        /// <summary>
        /// Application status
        /// </summary>
        public string ApplicationStatus { get; set; }
        
        /// <summary>
        /// CRM organization name
        /// </summary>
        public string RecruiterOrganizationName { get; set; }
        
        /// <summary>
        /// CRM organization GUID
        /// </summary>
        public string RecruiterOrganizationId { get; set; }
               
        /// <summary>
        /// CRM prospect GUID
        /// </summary>
		public string CrmProspectId { get; set; }
        
        /// <summary>
        /// Social security number
        /// </summary>
		public string Ssn { get; set; }
        
        /// <summary>
        /// Social insurance number
        /// </summary>
		public string Sin { get; set; }
        
        /// <summary>
        /// Academic program
        /// </summary>
		public string AcademicProgram { get; set; }
        
        /// <summary>
        /// Start Term
        /// </summary>
		public string StartTerm { get; set; }
        
        /// <summary>
        /// Prefix
        /// </summary>
		public string Prefix { get; set; }
        
        /// <summary>
        /// 
        /// First name
        /// </summary>
		public string FirstName { get; set; }

        /// <summary>
        /// Middle name
        /// </summary>
		public string MiddleName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
		public string LastName { get; set; }

        /// <summary>
        /// Suffix
        /// </summary>
		public string Suffix { get; set; }

        /// <summary>
        /// Nickname
        /// </summary>
		public string Nickname { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
		public string EmailAddress { get; set; }

        /// <summary>
        /// Instant messaging address
        /// </summary>
		public string ImAddress { get; set; }

        /// <summary>
        /// Instant messaging provider
        /// </summary>
		public string ImProvider { get; set; }

        /// <summary>
        /// Address line 1
        /// </summary>
		public string AddressLines1 { get; set; }

        /// <summary>
        /// Address line 2
        /// </summary>
		public string AddressLines2 { get; set; }

        /// <summary>
        /// Address line 3
        /// </summary>
		public string AddressLines3 { get; set; }

        /// <summary>
        /// City
        /// </summary>
		public string City { get; set; }

        /// <summary>
        /// State
        /// </summary>
		public string State { get; set; }

        /// <summary>
        /// Zip code
        /// </summary>
		public string Zip { get; set; }

        /// <summary>
        /// Country
        /// </summary>
		public string Country { get; set; }

        /// <summary>
        /// Address modifier line
        /// </summary>
		public string Attention { get; set; }

        /// <summary>
        /// Home phone number
        /// </summary>
		public string HomePhone { get; set; }

        /// <summary>
        /// Cell phone number
        /// </summary>
		public string CellPhone { get; set; }

        /// <summary>
        /// Alternate first name
        /// </summary>
		public string OtherFirstName { get; set; }

        /// <summary>
        /// Alternate last name
        /// </summary>
		public string OtherLastName { get; set; }

        /// <summary>
        /// Birth date
        /// </summary>
		public Nullable<DateTime> BirthDate { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
		public string Gender { get; set; }

        /// <summary>
        /// Ethnicity
        /// </summary>
		public string Ethnicity { get; set; }

        /// <summary>
        /// American Indian or Alaska Native
        /// </summary>
		public string Race1 { get; set; }

        /// <summary>
        /// Asian
        /// </summary>
		public string Race2 { get; set; }

        /// <summary>
        /// Black or African American
        /// </summary>
		public string Race3 { get; set; }

        /// <summary>
        /// Native Hawaiian or Other Pacific Islander
        /// </summary>
		public string Race4 { get; set; }

        /// <summary>
        /// White
        /// </summary>
		public string Race5 { get; set; }

        /// <summary>
        /// Disability code
        /// </summary>
		public string Disability { get; set; }

        /// <summary>
        /// Marital Status
        /// </summary>
		public string MaritalStatus { get; set; }

        /// <summary>
        /// Religious affiliation
        /// </summary>
		public string Denomination { get; set; }

        /// <summary>
        /// Veteran type
        /// </summary>
		public string Veteran { get; set; }

        /// <summary>
        /// Citizenship status
        /// </summary>
		public string CitizenshipStatus { get; set; }

        /// <summary>
        /// Birth city
        /// </summary>
		public string BirthCity { get; set; }

        /// <summary>
        /// Birth state
        /// </summary>
		public string BirthState { get; set; }

        /// <summary>
        /// Birth country
        /// </summary>
		public string BirthCountry { get; set; }

        /// <summary>
        /// Primary language
        /// </summary>
		public string PrimaryLanguage { get; set; }

        /// <summary>
        /// Citizenship country
        /// </summary>
		public string Citizenship { get; set; }

        /// <summary>
        /// Foreign registration ID
        /// </summary>
		public string ForeignRegistrationId { get; set; }

        /// <summary>
        /// Visa type
        /// </summary>
		public string VisaType { get; set; }

        /// <summary>
        /// U.S. entry date
        /// </summary>
		public Nullable<DateTime> CountryEntryDate { get; set; }

        /// <summary>
        /// Temporary address line 1
        /// </summary>
		public string TempAddressLines1 { get; set; }

        /// <summary>
        /// Temporary address line 2
        /// </summary>
		public string TempAddressLines2 { get; set; }

        /// <summary>
        /// Temporary address line 3
        /// </summary>
		public string TempAddressLines3 { get; set; }

        /// <summary>
        /// Temporary city
        /// </summary>
		public string TempCity { get; set; }

        /// <summary>
        /// Temporary State
        /// </summary>
		public string TempState { get; set; }

        /// <summary>
        /// Temporary zip code
        /// </summary>
		public string TempZip { get; set; }

        /// <summary>
        /// Temporary country
        /// </summary>
		public string TempCountry { get; set; }

        /// <summary>
        /// Temporary address modifier line
        /// </summary>
		public string TempAttention { get; set; }

        /// <summary>
        /// Temporary phone number
        /// </summary>
		public string TempPhone { get; set; }

        /// <summary>
        /// Temporary effective start date
        /// </summary>
		public Nullable<DateTime> TempStartDate { get; set; }

        /// <summary>
        /// Temporary effective end date
        /// </summary>
		public Nullable<DateTime> TempEndDate { get; set; }

        /// <summary>
        /// Career goal
        /// </summary>
		public string CareerGoal { get; set; }

        /// <summary>
        /// Educational goal
        /// </summary>
		public string EducationalGoal { get; set; }

        /// <summary>
        /// Decision plan
        /// </summary>
		public string DecisionPlan { get; set; }

        /// <summary>
        /// Course Load
        /// </summary>
		public string CourseLoad { get; set; }

        /// <summary>
        /// Financial Aid plan
        /// </summary>
		public string FaPlan { get; set; }

        /// <summary>
        /// Housing plan
        /// </summary>
		public string HousingPlan { get; set; }

        /// <summary>
        /// Decision factor 1
        /// </summary>
		public string DecisionFactor1 { get; set; }

        /// <summary>
        /// Decision factor 2
        /// </summary>
		public string DecisionFactor2 { get; set; }

        /// <summary>
        /// Parents' marital status
        /// </summary>
		public string ParentMaritalStatus { get; set; }

        /// <summary>
        /// Parent 1 relationship 
        /// </summary>
		public string Parent1RelationType { get; set; }

        /// <summary>
        /// Parent 1 prefix
        /// </summary>
		public string Parent1Prefix { get; set; }

        /// <summary>
        /// Parent 1 first name
        /// </summary>
		public string Parent1FirstName { get; set; }

        /// <summary>
        /// Parent 1 middle name
        /// </summary>
		public string Parent1MiddleName { get; set; }

        /// <summary>
        /// Parent 1 last name
        /// </summary>
		public string Parent1LastName { get; set; }

        /// <summary>
        /// Parent 1 suffix
        /// </summary>
		public string Parent1Suffix { get; set; }

        /// <summary>
        /// Parent 1 address line 1
        /// </summary>
		public string Parent1Address1 { get; set; }

        /// <summary>
        /// Parent 1 address line 2
        /// </summary>
		public string Parent1Address2 { get; set; }

        /// <summary>
        /// Parent 1 address line 3
        /// </summary>
		public string Parent1Address3 { get; set; }

        /// <summary>
        /// Parent 1 city
        /// </summary>
		public string Parent1City { get; set; }

        /// <summary>
        /// Parent 1 state
        /// </summary>
		public string Parent1State { get; set; }

        /// <summary>
        /// Parent 1 zip code
        /// </summary>
		public string Parent1Zip { get; set; }

        /// <summary>
        /// Parent 1 country
        /// </summary>
		public string Parent1Country { get; set; }

        /// <summary>
        /// Parent 1 phone number
        /// </summary>
		public string Parent1Phone { get; set; }

        /// <summary>
        /// Parent 1 email address
        /// </summary>
		public string Parent1EmailAddress { get; set; }

        /// <summary>
        /// Parent 1 birth date
        /// </summary>
		public Nullable<DateTime> Parent1BirthDate { get; set; }

        /// <summary>
        /// Parent 1 birth country
        /// </summary>
		public string Parent1BirthCountry { get; set; }

        /// <summary>
        /// Is parent 1 living?
        /// </summary>
		public string Parent1Living { get; set; }

        /// <summary>
        /// Does parent 1 live at the same address?
        /// </summary>
		public string Parent1SameAddress { get; set; }

        /// <summary>
        /// Parent 2 relationship
        /// </summary>
		public string Parent2RelationType { get; set; }

        /// <summary>
        /// Parent 2 prefix
        /// </summary>
		public string Parent2Prefix { get; set; }

        /// <summary>
        /// Parent 2 first name
        /// </summary>
		public string Parent2FirstName { get; set; }

        /// <summary>
        /// Parent 2 middle name
        /// </summary>
		public string Parent2MiddleName { get; set; }

        /// <summary>
        /// Parent 2 last name
        /// </summary>
		public string Parent2LastName { get; set; }

        /// <summary>
        /// Parent 2 suffix
        /// </summary>
		public string Parent2Suffix { get; set; }

        /// <summary>
        /// Parent 2 address line 1
        /// </summary>
		public string Parent2Address1 { get; set; }

        /// <summary>
        /// Parent 2 address line 2
        /// </summary>
		public string Parent2Address2 { get; set; }

        /// <summary>
        /// Parent 2 address line 3
        /// </summary>
		public string Parent2Address3 { get; set; }

        /// <summary>
        /// Parent 2 city
        /// </summary>
		public string Parent2City { get; set; }

        /// <summary>
        /// Parent 2 state
        /// </summary>
		public string Parent2State { get; set; }

        /// <summary>
        /// Parent 2 zip code
        /// </summary>
		public string Parent2Zip { get; set; }

        /// <summary>
        /// Parent 2 country
        /// </summary>
		public string Parent2Country { get; set; }

        /// <summary>
        /// Parent 2 phone number
        /// </summary>
		public string Parent2Phone { get; set; }

        /// <summary>
        /// Parent 2 email address
        /// </summary>
		public string Parent2EmailAddress { get; set; }

        /// <summary>
        /// Parent 2 birth date
        /// </summary>
		public Nullable<DateTime> Parent2BirthDate { get; set; }

        /// <summary>
        /// Parent 2 birth country
        /// </summary>
		public string Parent2BirthCountry { get; set; }

        /// <summary>
        /// Is parent 2 living?
        /// </summary>
		public string Parent2Living { get; set; }

        /// <summary>
        /// Does parent 2 live at the same address?
        /// </summary>
		public string Parent2SameAddress { get; set; }

        /// <summary>
        /// Legal guardian relationship
        /// </summary>
		public string GuardianRelationType { get; set; }

        /// <summary>
        /// Legal guardian prefix
        /// </summary>
		public string GuardianPrefix { get; set; }

        /// <summary>
        /// Legal guardian first name
        /// </summary>
		public string GuardianFirstName { get; set; }

        /// <summary>
        /// Legal guardian middle name
        /// </summary>
		public string GuardianMiddleName { get; set; }

        /// <summary>
        /// Legal guardian last name
        /// </summary>
		public string GuardianLastName { get; set; }

        /// <summary>
        /// Legal guardian suffix
        /// </summary>
		public string GuardianSuffix { get; set; }

        /// <summary>
        /// Legal guardian address line 1
        /// </summary>
		public string GuardianAddress1 { get; set; }

        /// <summary>
        /// Legal guardian address line 2
        /// </summary>
		public string GuardianAddress2 { get; set; }

        /// <summary>
        /// Legal guardian city
        /// </summary>
		public string GuardianCity { get; set; }

        /// <summary>
        /// Legal guardian state
        /// </summary>
		public string GuardianState { get; set; }

        /// <summary>
        /// Legal guardian zip code
        /// </summary>
		public string GuardianZip { get; set; }

        /// <summary>
        /// Legal guardian country
        /// </summary>
		public string GuardianCountry { get; set; }

        /// <summary>
        /// Legal guardian phone number
        /// </summary>
		public string GuardianPhone { get; set; }

        /// <summary>
        /// Legal guardian email address
        /// </summary>
		public string GuardianEmailAddress { get; set; }

        /// <summary>
        /// Legal guardian birth date
        /// </summary>
		public Nullable<DateTime> GuardianBirthDate { get; set; }

        /// <summary>
        /// Legal guardian birth country
        /// </summary>
		public string GuardianBirthCountry { get; set; }

        /// <summary>
        /// Does legal guardian live at the same address?
        /// </summary>
		public string GuardianSameAddress { get; set; }

        /// <summary>
        /// Sibling 1 relationship
        /// </summary>
		public string RelationTypeSibling1 { get; set; }

        /// <summary>
        /// Sibling 1 prefix
        /// </summary>
		public string PrefixSibling1 { get; set; }

        /// <summary>
        /// Sibling 1 first name
        /// </summary>
		public string FirstNameSibling1 { get; set; }

        /// <summary>
        /// Sibling 1 middle name
        /// </summary>
		public string MiddleNameSibling1 { get; set; }

        /// <summary>
        /// Sibling 1 last name
        /// </summary>
		public string LastNameSibling1 { get; set; }

        /// <summary>
        /// Sibling 1 suffix
        /// </summary>
		public string SuffixSibling1 { get; set; }

        /// <summary>
        /// Sibling 1 birth date
        /// </summary>
		public Nullable<DateTime> BirthDateSibling1 { get; set; }

        /// <summary>
        /// Sibling 2 relationship
        /// </summary>
		public string RelationTypeSibling2 { get; set; }

        /// <summary>
        /// Sibling 2 prefix
        /// </summary>
		public string PrefixSibling2 { get; set; }

        /// <summary>
        /// Sibling 2 first name
        /// </summary>
		public string FirstNameSibling2 { get; set; }

        /// <summary>
        /// Sibling 2 middle name
        /// </summary>
		public string MiddleNameSibling2 { get; set; }

        /// <summary>
        /// Sibling 2 last name
        /// </summary>
		public string LastNameSibling2 { get; set; }

        /// <summary>
        /// Sibling 2 suffix
        /// </summary>
		public string SuffixSibling2 { get; set; }

        /// <summary>
        /// Sibling 2 birth date
        /// </summary>
		public Nullable<DateTime> BirthDateSibling2 { get; set; }

        /// <summary>
        /// Sibling 3 relationship
        /// </summary>
		public string RelationTypeSibling3 { get; set; }

        /// <summary>
        /// Sibling 3 prefix
        /// </summary>
		public string PrefixSibling3 { get; set; }

        /// <summary>
        /// Sibling 3 first name
        /// </summary>
		public string FirstNameSibling3 { get; set; }

        /// <summary>
        /// Sibling 3 middle name
        /// </summary>
		public string MiddleNameSibling3 { get; set; }

        /// <summary>
        /// Sibling 3 last name
        /// </summary>
		public string LastNameSibling3 { get; set; }

        /// <summary>
        /// Sibling 3 suffix
        /// </summary>
		public string SuffixSibling3 { get; set; }

        /// <summary>
        /// Sibling 3 birthdate
        /// </summary>
		public Nullable<DateTime> BirthDateSibling3 { get; set; }

        /// <summary>
        /// Emergency contact prefix
        /// </summary>
		public string EmergencyPrefix { get; set; }

        /// <summary>
        /// Emergency contact first name
        /// </summary>
		public string EmergencyFirstName { get; set; }

        /// <summary>
        /// Emergency contact middle name
        /// </summary>
		public string EmergencyMiddleName { get; set; }

        /// <summary>
        /// Emergency contact last name
        /// </summary>
		public string EmergencyLastName { get; set; }

        /// <summary>
        /// Emergency contact suffix
        /// </summary>
		public string EmergencySuffix { get; set; }

        /// <summary>
        /// Emergency contact phone number
        /// </summary>
		public string EmergencyPhone { get; set; }

        /// <summary>
        /// Extracurricular activity type
        /// </summary>
		public string Activity { get; set; }

        /// <summary>
        /// Did you participate in the activity in  grade 9?
        /// </summary>
		public string Part9Activity { get; set; }

        /// <summary>
        /// Did you participate in the activity in grade 10?
        /// </summary>
		public string Part10Activity { get; set; }

        /// <summary>
        /// Did you participate in the activity in grade 11?
        /// </summary>
		public string Part11Activity { get; set; }

        /// <summary>
        /// Did you participate in the activity in grade 12?
        /// </summary>
		public string Part12Activity { get; set; }

        /// <summary>
        /// Did you participate in the activity in a post-secondary grade?
        /// </summary>
		public string PartPgActivity { get; set; }

        /// <summary>
        /// Do you plan to participate in the activity in college?
        /// </summary>
		public string FutureActivity { get; set; }

        /// <summary>
        /// Activity hours/week?
        /// </summary>
		public string HoursWeekActivity { get; set; }

        /// <summary>
        /// Activity weeks/year?
        /// </summary>
		public string WeeksYearActivity { get; set; }

        /// <summary>
        /// High School CEEB
        /// </summary>
		public string HighSchoolCeebs { get; set; }

        /// <summary>
        /// Unlisted high school info
        /// </summary>
		public string HighSchoolNonCeebInfo { get; set; }

        /// <summary>
        /// High school attended from year
        /// </summary>
		public string HighSchoolAttendFromYears { get; set; }

        /// <summary>
        /// High school attended from month
        /// </summary>
		public string HighSchoolAttendFromMonths { get; set; }

        /// <summary>
        /// High school attended to year
        /// </summary>
		public string HighSchoolAttendToYears { get; set; }

        /// <summary>
        /// High school attended to month
        /// </summary>
        public string HighSchoolAttendToMonths { get; set; }

        /// <summary>
        /// College CEEB
        /// </summary>
		public string CollegeCeebs { get; set; }

        /// <summary>
        /// Unlisted college info
        /// </summary>
		public string CollegeNonCeebInfo { get; set; }

        /// <summary>
        /// College attended from Year
        /// </summary>
		public string CollegeAttendFromYears { get; set; }

        /// <summary>
        /// College attended from month
        /// </summary>
		public string CollegeAttendFromMonths { get; set; }

        /// <summary>
        /// College attended to year
        /// </summary>
		public string CollegeAttendToYears { get; set; }

        /// <summary>
        /// College attended to month
        /// </summary>
		public string CollegeAttendToMonths { get; set; }

        /// <summary>
        /// College degree type
        /// </summary>
		public string CollegeDegrees { get; set; }

        /// <summary>
        /// College degree date
        /// </summary>
		public string CollegeDegreeDates { get; set; }

        /// <summary>
        /// College hours earned
        /// </summary>
		public string CollegeHoursEarned { get; set; }

        /// <summary>
        /// Guardian address line 3
        /// </summary>
		public string GuardAddress3 { get; set; }

        /// <summary>
        /// Admit type
        /// </summary>
		public string AdmitType { get; set; }

        /// <summary>
        /// Prospect source
        /// </summary>
		public string ProspectSource { get; set; }

        /// <summary>
        /// High school name
        /// </summary>
		public string HighSchoolNames { get; set; }

        /// <summary>
        /// College name
        /// </summary>
		public string CollegeNames { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
		public string Comments { get; set; }

        /// <summary>
        /// Miscellaneous 1
        /// </summary>
		public string Misc1 { get; set; }

        /// <summary>
        /// Miscellaneous 1
        /// </summary>
		public string Misc2 { get; set; }

        /// <summary>
        /// Miscellaneous 3
        /// </summary>
		public string Misc3 { get; set; }

        /// <summary>
        /// Miscellaneous 4
        /// </summary>
		public string Misc4 { get; set; }

        /// <summary>
        /// Miscellaneous 5
        /// </summary>
		public string Misc5 { get; set; }

        /// <summary>
        /// Application user 1
        /// </summary>
		public string ApplicationUser1 { get; set; }

        /// <summary>
        /// Application user 2
        /// </summary>
		public string ApplicationUser2 { get; set; }

        /// <summary>
        /// Application user 3
        /// </summary>
		public string ApplicationUser3 { get; set; }

        /// <summary>
        /// Application user 4
        /// </summary>
		public string ApplicationUser4 { get; set; }

        /// <summary>
        /// Application user 5
        /// </summary>
		public string ApplicationUser5 { get; set; }

        /// <summary>
        /// Application user 6
        /// </summary>
		public string ApplicationUser6 { get; set; }

        /// <summary>
        /// Application user 7
        /// </summary>
		public string ApplicationUser7 { get; set; }

        /// <summary>
        /// Application user 8
        /// </summary>
		public string ApplicationUser8 { get; set; }

        /// <summary>
        /// Application user 9
        /// </summary>
		public string ApplicationUser9 { get; set; }

        /// <summary>
        /// Application user 10
        /// </summary>
		public string ApplicationUser10 { get; set; }

        /// <summary>
        /// Applicant user 1
        /// </summary>
		public string ApplicantUser1 { get; set; }

        /// <summary>
        /// Applicant user 2
        /// </summary>
		public string ApplicantUser2 { get; set; }

        /// <summary>
        /// Applicant user 3
        /// </summary>
		public string ApplicantUser3 { get; set; }

        /// <summary>
        /// Applicant user 4
        /// </summary>
		public string ApplicantUser4 { get; set; }

        /// <summary>
        /// Applicant user 5
        /// </summary>
		public string ApplicantUser5 { get; set; }

        /// <summary>
        /// Applicant user 6
        /// </summary>
		public string ApplicantUser6 { get; set; }

        /// <summary>
        /// Applicant user 7
        /// </summary>
		public string ApplicantUser7 { get; set; }
        
        /// <summary>
        /// Applicant user 8
        /// </summary>
		public string ApplicantUser8 { get; set; }

        /// <summary>
        /// Applicant user 9
        /// </summary>		
        public string ApplicantUser9 { get; set; }

        /// <summary>
        /// Applicant user 10
        /// </summary>
		public string ApplicantUser10 { get; set; }

        /// <summary>
        /// not used
        /// </summary>
		public string CustomFieldsXML { get; set; }

        /// <summary>
        /// Location
        /// </summary>
		public string Location { get; set; }

        /// <summary>
        /// Marked applied on
        /// </summary>
		public Nullable<DateTime> SubmittedDate { get; set; }

        /// <summary>
        /// Graduated from high school?
        /// </summary>
		public string HighSchoolGraduated { get; set; }

        /// <summary>
        /// High school transcript stored?
        /// </summary>
		public string HighSchoolTranscriptStored { get; set; }

        /// <summary>
        /// High school transcript location
        /// </summary>
		public string HighSchoolTrancriptLocation { get; set; }

        /// <summary>
        /// High school transcript GPA
        /// </summary>
		public string HighSchoolTranscriptGpa { get; set; }

        /// <summary>
        /// High school transcript class percentile
        /// </summary>
		public string HighSchoolTranscriptClassPercentage { get; set; }

        /// <summary>
        /// High school transcript class rank
        /// </summary>
		public string HighSchoolTranscriptClassRank { get; set; }

        /// <summary>
        /// High school transcript class size
        /// </summary>
		public string HighSchoolTranscriptClassSize { get; set; }

        /// <summary>
        /// Graduated from college?
        /// </summary>
		public string CollegeGraduated { get; set; }

        /// <summary>
        /// College transcript stored?
        /// </summary>
		public string CollegeTranscriptStored { get; set; }

        /// <summary>
        /// College transcript location
        /// </summary>
		public string CollegeTranscriptLocation { get; set; }

        /// <summary>
        /// College transcript GPA
        /// </summary>
		public string CollegeTranscriptGpa { get; set; }

        /// <summary>
        /// College transcript class percentile
        /// </summary>
		public string CollegeTranscriptClassPercentage { get; set; }

        /// <summary>
        /// College transcript class rank
        /// </summary>
		public string CollegeTranscriptClassRank { get; set; }

        /// <summary>
        /// College transcript class size
        /// </summary>
		public string CollegeTranscriptClassSize { get; set; }

        /// <summary>
        /// Applicant county
        /// </summary>
        public string ApplicantCounty { get; set; }

        /// <summary>
        /// Application residency status (In State, etc)
        /// </summary>
        public string ResidencyStatus { get; set; }

        /// <summary>
        /// Custom fields
        /// </summary>
        public IEnumerable<CustomField> CustomFields { get; set; }

        /// <summary>
        /// Application status date; if not provided, the application status will be associated with the current date
        /// </summary>
        public Nullable<DateTime> ApplicationStatusDate { get; set; }
    }
}
