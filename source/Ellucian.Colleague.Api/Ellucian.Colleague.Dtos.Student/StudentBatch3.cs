// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Base;
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Demographic and academic information about a student
    /// </summary>
    public class StudentBatch3
    {
        /// <summary>
        ///  Unique system ID of this person
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Person's last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Person's first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Person's middle name
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Prefixes
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Suffixes
        /// </summary>
        public string Suffix { get; set; }
        /// <summary>
        /// Gender (Male or Female)
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Date of Birth
        /// </summary>
        public DateTime? BirthDate { get; set; }
        /// <summary>
        /// Social Security or similar Government Id
        /// </summary>
        public string GovernmentId { get; set; }
        /// <summary>
        /// List of Races
        /// </summary>
        public List<string> RaceCodes { get; set; }
        /// <summary>
        /// List of Ethnicities
        /// </summary>
        public List<string> EthnicCodes { get; set; }
        /// <summary>
        /// Specific Ethnicities (Unknown, HispanicOrLatino, Asian, BlackOrAfricanAmerican, 
        /// NativeHawaiianOrOtherPacificIslander or White)
        /// </summary>
        public List<EthnicOrigin> Ethnicities { get; set; }
        /// <summary>
        /// Marital Status of the person (Single, Married, Divorced, Widowed)
        /// </summary>
        public MaritalState? MaritalStatus { get; set; }
        /// <summary>
        /// The Id of this student's degree plan
        /// </summary>
        //public int? DegreePlanId { get; set; }
        /// <summary>
        /// The programs in which this student is enrolled
        /// </summary>
        public List<string> ProgramIds { get; set; }
        /// <summary>
        /// Ids of restrictions for this student
        /// </summary>
        public List<string> StudentRestrictionIds { get; set; }
        /// <summary>
        /// Indicates whether student has an assigned advisor
        /// </summary>
        public bool HasAdvisor { get; set; }
        /// <summary>
        /// Preferred email address of student
        /// </summary>
        public string PreferredEmailAddress { get; set; }
        /// <summary>
        /// All email address of student
        /// </summary>
        public List<EmailAddress> EmailAddresses { get; set; }
        /// <summary>
        /// Phonetypes hierarchy for student profile
        /// </summary>
        public List<string> PhoneTypesHierarchy { get; set; }
        /// Added Fields for ESS project (SRM - 11/01/2013)
        /// <summary>
        /// If Parent attended this university, then True else False
        /// </summary>
        public bool IsLegacyStudent { get; set; }
        /// <summary>
        /// Flag to say if this student is the first generation to attend college
        /// </summary>
        public bool? IsFirstGenerationStudent { get; set; }
        /// <summary>
        /// Flag to show if this is an International Student
        /// </summary>
        public bool IsInternationalStudent { get; set; }
        /// <summary>
        /// List of start terms for each Academic Level(s) of a student
        /// </summary>
        public List<string> AdmitTerms { get; set; }
        /// <summary>
        /// List of Academic Levels for the student
        /// </summary>
        public List<StudentAcademicLevel> StudentAcademicLevels { get; set; }
        /// <summary>
        /// The Residency or Student Type code
        /// </summary>
        public string ResidencyStatus { get; set; }
        /// <summary>
        /// High School attended Person including High School ID and GPA
        /// </summary>
        public List<HighSchoolGpa> HighSchoolGpas { get; set; }
        /// <summary>
        /// List of student home locations and start/end date
        /// </summary>
        public List<StudentHomeLocation> StudentHomeLocations { get; set; }
        /// <summary>
        /// List of active advisements for the student (advisors, advisor types, start dates, end dates).
        /// For Advise 1.4+, lists advisements active at some point during the current term in Advise.
        /// </summary>
        public IEnumerable<Advisement> Advisements { get; set; }
        /// <summary>
        /// Student Type Code
        /// </summary>
        public string StudentTypeCode { get; set; }
        /// <summary>
        /// Class Level Codes for each academic level
        /// </summary>
        public List<string> ClassLevelCodes { get; set; }
        /// <summary>
        /// Flag to indicate if the student record is confidential
        /// </summary>
        public bool IsConfidential { get; set; }
        /// <summary>
        /// List of active advisor Ids who have this student assigned as an advisee.
        /// For Advise 1.4+, lists advisors active at some point during the current term in Advise.
        /// </summary>
        public List<string> AdvisorIds { get; set; }
        /// <summary>
        /// Flag to show if this is an transfer student
        /// </summary>
        public bool IsTransfer { get; set; }
        /// <summary>
        /// Privacy status code
        /// </summary>
        public string PrivacyStatusCode { get; set; }
        /// <summary>
        /// Information that should be used when displaying a student's name. The hierarchy that is used in calculating this 
        /// name is defined in the Student Display Name Hierarchy on the SPWP form in Colleague.  
        /// If no hierarchy is provide on SPWP, PersonDisplayName will be null.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }
        /// <summary>
        /// Pronoun to use to refer to student 
        /// </summary>
        public string PersonalPronounCode { get; set; }

    }
}
