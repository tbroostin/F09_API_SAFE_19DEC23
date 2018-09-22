// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Demographic and academic information about a student
    /// </summary>
    public class Student : Person
    {
        /// <summary>
        /// The Id of this student's degree plan
        /// </summary>
        public int? DegreePlanId { get; set; }
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
        public List<string> AcademicLevelCodes { get; set; }
        /// <summary>
        /// The Residency or Student Type code
        /// </summary>
        public string ResidencyStatus { get; set; }
        /// <summary>
        /// High School attended Person including High School ID and GPA
        /// </summary>
        public List<HighSchoolGpa> HighSchoolGpas { get; set; }
        /// <summary>
        /// List of advisements for the student (advisors, advisor types, start dates, end dates)
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
        /// List of advisor Ids who have this student assigned as an advisee
        /// </summary>
        public List<string> AdvisorIds { get; set; }
        /// <summary>
        /// Flag to show if this is an transfer student
        /// </summary>
        public bool IsTransfer { get; set; }

        /// <summary>
        /// Id of the student's Financial Aid counselor. If empty or null, the applicant has not been
        /// assigned a counselor yet.
        /// </summary>
        public string FinancialAidCounselorId { get; set; }

          /// <summary>
          /// List of all of the email addresses associated with a student
          /// </summary>
          public List<EmailAddress> EmailAddresses { get; set; }

          /// <summary>
          /// List of student home locations and start/end date
          /// </summary>
          public List<StudentHomeLocation> StudentHomeLocations { get; set; }

          /// <summary>
          /// Information that should be used when displaying a student's name. The hierarchy that is used in calculating this 
          /// name is defined in the Student Display Name Hierarchy on the SPWP form in Colleague.  
          /// If no hierarchy is provide on SPWP, PersonDisplayName will be null.
          /// </summary>
          public PersonHierarchyName PersonDisplayName { get; set; }
    }
}
